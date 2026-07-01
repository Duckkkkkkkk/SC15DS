# SC15DS (Steam Controller '15 Driver Software)
![Version](https://img.shields.io/badge/Version-1.0_Ultimate-blue.svg)
![Platform](https://img.shields.io/badge/Platform-Windows_10%2F11-lightgrey.svg)
![Language](https://img.shields.io/badge/Language-C%23_.NET_7.0-brightgreen.svg)
![License](https://img.shields.io/badge/License-MIT-orange.svg)

<img width="598" height="476" alt="image" src="https://github.com/user-attachments/assets/415d30a2-e7b9-4485-b44b-e32b45ce8740" />

**SC15DS** is a high-performance, standalone native Windows driver and configuration dashboard for the original 2015 Valve Steam Controller. It allows you to bypass the Steam client entirely, mapping your controller to act as a system-wide Xbox 360 gamepad, a Windows mouse, or any universal keyboard stroke with virtually zero latency.

---

## ❓ Why is this needed?
By default, the Steam Controller is heavily locked into the Steam Input ecosystem. If you try to use the controller without Steam running—or try to play games on Epic Games, Xbox Game Pass, or standalone emulators—the controller defaults to a restricted, uncustomizable "Lizard Mode." 

SC15DS solves this by directly intercepting the controller's raw USB/Bluetooth HID packets, automatically sending the `0x81` kill-command to disable Lizard Mode, and rerouting the inputs through a lightweight, custom-built translation engine. **You get full control over your hardware, completely independent of Steam.**

## ✨ Core Features
* **Zero Steam Dependency:** Operates completely independently of the Steam client. Play Game Pass, Epic, or retro emulators flawlessly.
* **Universal Key Grabber:** Click any node on the visual map and press a key on your keyboard to instantly bind it. 
* **Trackpad Mouse Translation:** Flawlessly translates the high-definition trackpads into Windows cursor movements.
* **Borderless GDI+ Dashboard:** A sleek, custom-rendered dark-mode interface that visually maps your controller.
* **Background Engine:** Minimizes directly to the Windows System Tray. It utilizes virtually zero CPU/RAM while translating inputs in a dedicated background thread.
* **Live Telemetry:** Automatically monitors packet reception and alerts you if the controller connection drops or the battery dies.

## ⚙️ Advanced Hardware Tuning
Click the **ADVANCED >>** panel in the dashboard to access 15 driver-level configuration parameters, including:
* **Trackpad & Joystick Deadzones:** Eliminate drift with granular threshold tuning.
* **Mouse Sensitivity & Inversion:** Custom DPI scaling for trackpad-to-mouse translation, plus X/Y axis inversion.
* **Trigger Actuation:** Set exact threshold limits for the dual-stage triggers.
* **Boot-on-Startup:** Writes directly to the Windows Registry to automatically silently launch the driver engine when you turn on your PC.

---

## 🚀 Installation & Quick Start
1. Go to the [Releases](../../releases) tab.
2. Download the latest **`SC15DS_Installer_v1.0.exe`**.
3. Run the installer. 
> **Note:** The installer will automatically download and silently install the prerequisite `ViGEmBus` virtual driver if your system does not already have it.
4. Launch SC15DS, connect your Steam Controller (wired or via wireless dongle), and click **ENGAGE ENGINE**.

## 🧠 Under the Hood (Architecture)
SC15DS was built with extreme optimization in mind:
* **Dictionary Hash Mapping:** Instead of bulky `if/else` UI chains, bindings are stored in a high-speed memory dictionary, allowing the hardware thread to route inputs instantly.
* **Win32 API Hooking:** Utilizes `user32.dll` for deep system-level mouse events (`MOUSEEVENTF`) and keyboard strokes (`keybd_event`), ensuring inputs are detected by anti-cheat engines and exclusive-fullscreen games.
* **AOT Trimming:** Compiled with `.NET 7.0` Ahead-of-Time trimming for a massively reduced, standalone binary footprint.

## 🙏 Credits & Dependencies
* Built utilizing the incredible [ViGEmBus](https://github.com/nefarius/ViGEmBus) framework by Nefarius.
* Powered by [HidLibrary](https://github.com/mikeobrien/HidLibrary) for raw HID packet interception.
