# SC15DS (Steam Controller '15 Driver System) # ALERT - RUN AS ADMINISTRATOR
![Version](https://img.shields.io/badge/Version-2.0.0_Stable-103C19.svg?color=67c219)
![Platform](https://img.shields.io/badge/Platform-Windows_10%2F11-lightgrey.svg)
![Language](https://img.shields.io/badge/Language-C%23_.NET_7.0-brightgreen.svg)
![License](https://img.shields.io/badge/License-MIT-orange.svg)

<img width="801" height="558" alt="image" src="https://github.com/user-attachments/assets/b2016ff4-af52-475d-9129-756ed8d7492e" />

**SC15DS** is a high-performance, standalone native Windows driver and configuration suite built for the original 2015 Valve Steam Controller. This utility bypasses the Steam client completely, transforming your controller into a system-wide Xbox 360 gamepad, a Windows mouse, or a universal keyboard translator with near-zero input latency.

v2.0.0 introduces a retro **Classic Steam Theme** design coupled with a proprietary, bare-metal configuration engine.

---

## ❓ Why is this needed?
Out of the box, the Steam Controller is locked tightly inside the Steam Input ecosystem. Without Steam running constantly in the background, the hardware defaults to a highly restrictive, unchangeable fallback state known as "Lizard Mode." 

SC15DS breaks this dependency. It intercepts raw USB/Bluetooth HID packets directly from the controller hardware, issues a custom `0x81` kill-command to completely disable Lizard Mode, and translates all movements through an asynchronous, background worker thread. **Your hardware is fully unlocked, system-wide, across Epic Games, Xbox Game Pass, and standalone retro emulators.**

## ✨ Features in v2.0.0
* **OG Steam Grid Aesthetics:** Fully reskinned with the classic dark charcoal palettes and high-contrast green typography of the original desktop client.
* **Symmetric Hardware Mapping:** UI button mapping nodes are perfectly spaced and aligned to follow the actual physical contours of the controller chassis.
* **Proprietary Flat-File Subsystem (`.sc15`):** Stripped of heavy external JSON libraries. Profiles are loaded, written, and read line-by-line as raw structural string-maps for blistering load speeds.
* **Live Telemetry Canvas:** Features an active visual crosshair diagnostic dock that tracks your thumbstick coordinates and trackpad touch states in real time.
* **Auto-Sensing Game Detection:** A lightweight 1.2-second interval process scanning loop checks active Windows memory via `user32.dll` to seamlessly auto-swap configurations when you switch games.
* **Action Layers & Mode Shifting:** Dynamically binds custom modifier keys (like holding Left Grip) to completely swap your button bindings on the fly.
* **Dual-Stage Mechanical Triggers:** Differentiates between a smooth analog soft-pull and the distinct physical mechanical click at the bottom of the trigger gate.
* **Integrated Gyroscope Aiming:** Translates pitch and yaw motion data directly into standard OS mouse cursor scaling.
* **Zero-Latency Background Execution:** Fully integrates into the Windows System Tray, enabling the interface to fade down using the minimize (`—`) option while keeping the driver completely active.

---

## 🚀 Installation & Quick Start
1. Head over to the [Releases](../../releases) section.
2. Download the unified installer package: **`SC15DS_ClassicSteam_v2.0.0.exe`**.
3. Execute the setup program with Administrator privileges.
> **Note:** The wizard will automatically evaluate your computer's OS environment and perform a silent background installation of the prerequisite virtual controller bus driver (`ViGEmBus`) if it is missing.
4. Open SC15DS, plug in or wirelessly sync your Steam Controller, and click **ENGAGE DRIVER SYSTEM**.

## 🛠️ Compiling from Source
If you wish to compile the raw monolithic codebase yourself:
1. Ensure the `.NET 7.0 SDK` environment is available on your machine.
2. Restore the essential NuGet packages (`HidLibrary` and `Nefarius.ViGEm.Client`).
3. Run this standard single-file publish string inside your project directory to avoid Windows Forms compilation conflicts:
   ```bash
   dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
