using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using HidLibrary;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace SC15DS
{
    // --- 1. DATA CONFIGURATION ---
    public class SC15Config
    {
        public Dictionary<string, string> Bindings = new Dictionary<string, string>()
        {
            {"LT", "Xbox Left Trigger"}, {"RT", "Xbox Right Trigger"},
            {"LB", "Xbox Left Bumper"},  {"RB", "Xbox Right Bumper"},
            {"LPad", "Windows Mouse"},   {"RPad", "Windows Mouse"},
            {"Joy", "W"},                {"Face", "Standard A/B/X/Y"},
            {"LGrip", "Space"},          {"RGrip", "Escape"}
        };
        
        public Dictionary<string, int> AdvSettings = new Dictionary<string, int>()
        {
            {"Trackpad_Deadzone", 2500}, {"Mouse_Sensitivity", 1500}, {"Haptic_Intensity", 50},
            {"Trigger_Deadzone", 10}, {"Trigger_Max", 100}, {"Joy_Deadzone", 4000},
            {"Joy_AntiDeadzone", 0}, {"Invert_Mouse_X", 0}, {"Invert_Mouse_Y", 0},
            {"Mouse_Acceleration", 0}, {"Gyro_Enabled", 0}, {"Gyro_Sensitivity", 100},
            {"Double_Tap_Delay_ms", 250}, {"Polling_Rate_Limit_ms", 2}, {"Boot_On_Startup", 0}
        };
    }

    // --- 2. DARK MODE CONTEXT MENU RENDERER ---
    public class DarkMenuRenderer : ToolStripProfessionalRenderer { public DarkMenuRenderer() : base(new DarkColors()) {} }
    public class DarkColors : ProfessionalColorTable {
        public override Color MenuItemSelected => Color.FromArgb(0, 255, 150);
        public override Color MenuItemBorder => Color.Transparent;
        public override Color ToolStripDropDownBackground => Color.FromArgb(20, 20, 22);
        public override Color MenuItemSelectedGradientBegin => Color.FromArgb(0, 255, 150);
        public override Color MenuItemSelectedGradientEnd => Color.FromArgb(0, 255, 150);
    }

    // --- 3. THE INTERACTIVE BIND NODE ---
    public class BindNode : Panel
    {
        public Button BtnBind;
        public string MapKey;
        private MainForm ParentForm;
        private SC15Config Config;

        public BindNode(string title, int x, int y, SC15Config config, string mapKey, MainForm parent)
        {
            Location = new Point(x, y); Size = new Size(140, 60);
            BackColor = Color.FromArgb(18, 18, 20);
            MapKey = mapKey; Config = config; ParentForm = parent;
            
            Label lbl = new Label() { Text = title, ForeColor = Color.FromArgb(0, 255, 150), Font = new Font("Segoe UI", 8, FontStyle.Bold), Location = new Point(5, 5), AutoSize = true };
            BtnBind = new Button() { Text = config.Bindings[mapKey], Location = new Point(5, 25), Width = 130, Height = 25, BackColor = Color.FromArgb(30, 30, 35), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8) };
            BtnBind.FlatAppearance.BorderSize = 0; BtnBind.Cursor = Cursors.Hand;

            ContextMenuStrip cm = new ContextMenuStrip() { Renderer = new DarkMenuRenderer(), ForeColor = Color.White };
            string[] specialBinds = { "Windows Mouse", "Xbox 'A'", "Xbox 'B'", "Xbox Left Trigger", "Xbox Right Trigger", "Mouse Left Click", "Mouse Right Click", "Standard A/B/X/Y" };
            foreach(var b in specialBinds) cm.Items.Add(b, (Image?)null, (s, e) => UpdateBind(b));

            BtnBind.MouseDown += (s, e) => {
                if (e.Button == MouseButtons.Left) ParentForm.StartListening(this);
                else if (e.Button == MouseButtons.Right) cm.Show(BtnBind, e.Location);
            };

            Controls.Add(lbl); Controls.Add(BtnBind);
        }

        public void UpdateBind(string newBind) {
            Config.Bindings[MapKey] = newBind;
            BtnBind.Text = newBind;
            BtnBind.BackColor = Color.FromArgb(30, 30, 35);
        }

        protected override void OnPaint(PaintEventArgs e) {
            e.Graphics.DrawRectangle(new Pen(Color.FromArgb(40, 40, 50), 1), 0, 0, Width - 1, Height - 1);
        }
    }

    // --- 4. ADVANCED SETTINGS MENU ---
    public class AdvancedMenu : Panel
    {
        public AdvancedMenu(SC15Config config)
        {
            Size = new Size(250, 400); Location = new Point(620, 40);
            BackColor = Color.FromArgb(15, 15, 18);
            AutoScroll = true;

            Label title = new Label() { Text = "ADVANCED PARAMETERS", ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(10, 10), AutoSize = true };
            Controls.Add(title);

            int yOffset = 40;
            foreach (var key in config.AdvSettings.Keys.ToList())
            {
                Label lbl = new Label() { Text = key.Replace("_", " "), ForeColor = Color.Gray, Location = new Point(10, yOffset), AutoSize = true };
                NumericUpDown nud = new NumericUpDown() { Location = new Point(150, yOffset - 2), Width = 70, Minimum = 0, Maximum = 10000, Value = config.AdvSettings[key], BackColor = Color.FromArgb(30, 30, 35), ForeColor = Color.White };
                
                nud.ValueChanged += (s, e) => { 
                    config.AdvSettings[key] = (int)nud.Value; 
                    if (key == "Boot_On_Startup") ToggleRegistryStartup((int)nud.Value == 1);
                };

                Controls.Add(lbl); Controls.Add(nud);
                yOffset += 30;
            }
        }

        private void ToggleRegistryStartup(bool enable)
        {
            RegistryKey? rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (rk != null) {
                if (enable) rk.SetValue("SC15DS", Application.ExecutablePath);
                else rk.DeleteValue("SC15DS", false);
            }
        }
    }

    // --- 5. CORE ENGINE & UI ---
    public class MainForm : Form
    {
        [DllImport("user32.dll")] static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);
        [DllImport("user32.dll")] static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
        [DllImport("user32.dll")] public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")] public static extern bool ReleaseCapture();
        
        const uint MOUSEEVENTF_MOVE = 0x0001; const uint MOUSEEVENTF_LEFTDOWN = 0x0002; const uint MOUSEEVENTF_LEFTUP = 0x0004; const uint KEYEVENTF_KEYUP = 0x0002;

        private SC15Config config = new SC15Config();
        private bool isRunning = false;
        private Thread? driverThread;
        private Label lblStatus;
        private BindNode? activeListener = null;
        private bool advMenuOpen = false;
        private NotifyIcon trayIcon;

        public MainForm()
        {
            Text = "SC15DS Ultimate"; Width = 600; Height = 480;
            BackColor = Color.FromArgb(10, 10, 12); FormBorderStyle = FormBorderStyle.None;
            Opacity = 0; DoubleBuffered = true; KeyPreview = true; 

            // --- TRAY ICON ENGINE ---
            trayIcon = new NotifyIcon() { Icon = SystemIcons.Shield, Text = "SC15DS Engine", Visible = true };
            ContextMenuStrip trayMenu = new ContextMenuStrip() { Renderer = new DarkMenuRenderer(), ForeColor = Color.White };
            trayMenu.Items.Add("Open Dashboard", (Image?)null, (s, e) => { this.Show(); this.WindowState = FormWindowState.Normal; });
            trayMenu.Items.Add("Exit System", (Image?)null, (s, e) => { isRunning = false; trayIcon.Visible = false; Application.Exit(); });
            trayIcon.ContextMenuStrip = trayMenu;
            trayIcon.DoubleClick += (s, e) => { this.Show(); this.WindowState = FormWindowState.Normal; };

            // --- BORDERLESS TITLE BAR ---
            Panel titleBar = new Panel() { Width = 900, Height = 30, BackColor = Color.FromArgb(5, 5, 6), Cursor = Cursors.SizeAll };
            titleBar.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) { ReleaseCapture(); SendMessage(Handle, 0xA1, 0x2, 0); } };
            
            Button btnMin = new Button() { Text = "—", Left = 520, Top = 0, Width = 40, Height = 30, FlatStyle = FlatStyle.Flat, ForeColor = Color.Gray, BackColor = titleBar.BackColor }; btnMin.FlatAppearance.BorderSize = 0;
            btnMin.Click += (s, e) => { this.Hide(); trayIcon.ShowBalloonTip(2000, "SC15DS Active", "Engine is running in the background.", ToolTipIcon.Info); };
            
            Button btnClose = new Button() { Text = "X", Left = 560, Top = 0, Width = 40, Height = 30, FlatStyle = FlatStyle.Flat, ForeColor = Color.Gray, BackColor = titleBar.BackColor }; btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => { isRunning = false; trayIcon.Visible = false; Application.Exit(); };
            
            titleBar.Controls.Add(btnMin); titleBar.Controls.Add(btnClose); Controls.Add(titleBar);

            // --- VISUAL CONTROLLER NODES ---
            Controls.Add(new BindNode("LEFT TRIGGER", 15, 60, config, "LT", this));
            Controls.Add(new BindNode("LEFT BUMPER", 15, 130, config, "LB", this));
            Controls.Add(new BindNode("LEFT TRACKPAD", 15, 200, config, "LPad", this));
            Controls.Add(new BindNode("JOYSTICK", 90, 270, config, "Joy", this));
            Controls.Add(new BindNode("LEFT GRIP", 15, 340, config, "LGrip", this));

            Controls.Add(new BindNode("RIGHT TRIGGER", 445, 60, config, "RT", this));
            Controls.Add(new BindNode("RIGHT BUMPER", 445, 130, config, "RB", this));
            Controls.Add(new BindNode("FACE BUTTONS", 370, 200, config, "Face", this));
            Controls.Add(new BindNode("RIGHT TRACKPAD", 445, 270, config, "RPad", this));
            Controls.Add(new BindNode("RIGHT GRIP", 445, 340, config, "RGrip", this));

            // --- MAIN CONTROLS ---
            Button btnStart = new Button() { Text = "ENGAGE ENGINE", Left = 220, Top = 200, Width = 160, Height = 40, BackColor = Color.FromArgb(0, 255, 150), ForeColor = Color.Black, Font = new Font("Segoe UI", 9, FontStyle.Bold), FlatStyle = FlatStyle.Flat }; btnStart.FlatAppearance.BorderSize = 0;
            Button btnStop = new Button() { Text = "STANDBY", Left = 220, Top = 250, Width = 160, Height = 30, BackColor = Color.FromArgb(30, 30, 35), ForeColor = Color.White, Font = new Font("Segoe UI", 8, FontStyle.Bold), FlatStyle = FlatStyle.Flat }; btnStop.FlatAppearance.BorderSize = 0;
            Button btnAdv = new Button() { Text = "ADVANCED >>", Left = 480, Top = 430, Width = 100, Height = 30, BackColor = Color.FromArgb(20, 20, 25), ForeColor = Color.Gray, FlatStyle = FlatStyle.Flat }; btnAdv.FlatAppearance.BorderSize = 0;
            
            lblStatus = new Label() { Text = "SYSTEM IDLE", Left = 220, Top = 165, Width = 160, ForeColor = Color.Gray, Font = new Font("Consolas", 10), TextAlign = ContentAlignment.MiddleCenter };

            // Start Thread as Background so it doesn't block closing
            btnStart.Click += (s, e) => { if (!isRunning) { isRunning = true; driverThread = new Thread(DriverLoop) { IsBackground = true }; driverThread.Start(); } };
            btnStop.Click += (s, e) => { isRunning = false; lblStatus.Text = "STOPPING..."; };
            
            AdvancedMenu advPanel = new AdvancedMenu(config); Controls.Add(advPanel);
            btnAdv.Click += (s, e) => { 
                advMenuOpen = !advMenuOpen; this.Width = advMenuOpen ? 900 : 600; titleBar.Width = this.Width; 
                btnClose.Left = this.Width - 40; btnMin.Left = this.Width - 80; 
            };

            Controls.Add(btnStart); Controls.Add(btnStop); Controls.Add(btnAdv); Controls.Add(lblStatus);

            System.Windows.Forms.Timer fadeTimer = new System.Windows.Forms.Timer { Interval = 15 };
            fadeTimer.Tick += (s, e) => { if (Opacity < 1) Opacity += 0.05; else fadeTimer.Stop(); };
            this.Load += (s, e) => fadeTimer.Start();
        }

        // --- BACKGROUND PAINTING (VISUAL MAP) ---
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            
            using (Font fBold = new Font("Segoe UI Black", 24, FontStyle.Italic))
            using (Font fLight = new Font("Segoe UI Light", 14))
            {
                e.Graphics.DrawString("SC15DS", fBold, new SolidBrush(Color.FromArgb(0, 255, 150)), 220, 40);
                e.Graphics.DrawString("DRIVER ENGINE", fLight, Brushes.Gray, 225, 75);
            }

            using (Pen glow = new Pen(Color.FromArgb(15, 45, 35), 4))
            {
                e.Graphics.DrawArc(glow, 150, 120, 150, 150, 90, 180);
                e.Graphics.DrawArc(glow, 300, 120, 150, 150, 270, 180); 
                e.Graphics.DrawLine(glow, 225, 120, 375, 120); 
                e.Graphics.DrawLine(glow, 225, 270, 375, 270); 
                e.Graphics.DrawEllipse(glow, 160, 140, 80, 80); 
                e.Graphics.DrawEllipse(glow, 360, 140, 80, 80); 
            }
        }

        // --- KEY GRABBER ---
        public void StartListening(BindNode node) {
            if (activeListener != null) activeListener.UpdateBind(config.Bindings[activeListener.MapKey]); 
            activeListener = node;
            node.BtnBind.Text = "[ PRESS ANY KEY ]";
            node.BtnBind.BackColor = Color.FromArgb(0, 150, 200);
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            if (activeListener != null) {
                activeListener.UpdateBind(e.KeyCode.ToString()); 
                activeListener = null;
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }

        private void PressKey(string keyName, bool isPressed, ref bool wasPressed) {
            if (isPressed && !wasPressed && Enum.TryParse(keyName, out Keys key)) keybd_event((byte)key, 0, 0, UIntPtr.Zero);
            else if (!isPressed && wasPressed && Enum.TryParse(keyName, out Keys keyUp)) keybd_event((byte)keyUp, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            wasPressed = isPressed;
        }

        // --- HARDWARE LOOP ---
        private void DriverLoop()
        {
            Invoke(new Action(() => { lblStatus.Text = "CONNECTING"; lblStatus.ForeColor = Color.Yellow; }));
            try
            {
                var client = new ViGEmClient(); var pad = client.CreateXbox360Controller(); pad.Connect();
                var sc = HidDevices.Enumerate(0x28DE, 0x1142).FirstOrDefault() ?? HidDevices.Enumerate(0x28DE, 0x1102).FirstOrDefault();
                if (sc == null) { Invoke(new Action(() => { lblStatus.Text = "NOT FOUND"; lblStatus.ForeColor = Color.Red; })); return; }

                sc.OpenDevice();
                Invoke(new Action(() => { lblStatus.Text = "ACTIVE"; lblStatus.ForeColor = Color.FromArgb(0, 255, 150); }));
                byte[] magic = new byte[65]; magic[1] = 0x81; sc.Write(magic); 

                bool lgWasPressed = false;

                while (isRunning)
                {
                    var data = sc.Read(10);
                    if (data.Status == HidDeviceData.ReadStatus.Success && data.Data.Length >= 24)
                    {
                        byte[] bytes = data.Data;

                        bool lGripDown = (bytes[2] & 0x01) != 0;
                        string gripBind = config.Bindings["LGrip"];

                        if (gripBind.StartsWith("Xbox")) pad.SetButtonState(Xbox360Button.A, lGripDown); 
                        else if (gripBind == "Mouse Left Click" && lGripDown) mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                        else PressKey(gripBind, lGripDown, ref lgWasPressed); 

                        short rightX = (short)(bytes[20] | (bytes[21] << 8)); short rightY = (short)(bytes[22] | (bytes[23] << 8));
                        int dz = config.AdvSettings["Trackpad_Deadzone"];
                        int sens = config.AdvSettings["Mouse_Sensitivity"];
                        
                        if (config.Bindings["RPad"] == "Windows Mouse" && (Math.Abs(rightX) > dz)) {
                            if (config.AdvSettings["Invert_Mouse_Y"] == 1) rightY = (short)-rightY; 
                            mouse_event(MOUSEEVENTF_MOVE, rightX / sens, -rightY / sens, 0, 0);
                        }
                    }
                }
                sc.CloseDevice(); pad.Disconnect();
            }
            catch (Exception) { Invoke(new Action(() => { lblStatus.Text = "ERROR"; lblStatus.ForeColor = Color.Red; })); }
        }

        [STAThread] static void Main() { Application.EnableVisualStyles(); Application.Run(new MainForm()); }
    }
}