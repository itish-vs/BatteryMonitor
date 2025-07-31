
# 🔋 Battery Monitor
**Battery Monitor** is a lightweight Windows tray application that tracks your laptop’s battery status in real-time. It alerts you with custom sounds, pop-ups, and clear status displays when your battery is low or fully charged — helping you extend battery health and avoid accidental shutdowns.

---

## 📌 Features

- ✅ **Real-time Monitoring**: Tracks charge percentage and charging status continuously.
- ✅ **Custom Thresholds**: Configure upper and lower limits via `appsettings.json` — no code change needed.
- ✅ **Sound Alerts**: Plays user-defined `.wav` files when battery is low or full.
- ✅ **Balloon Tips & Popups**: Clear Windows tray notifications when thresholds are reached.
- ✅ **Minimize to Tray**: Runs silently in your system tray without cluttering your taskbar.
- ✅ **Single Instance Enforcement**: Ensures only one instance runs at a time — prevents duplicates.
- ✅ **Dynamic Versioning**: Displays version information automatically.
- ✅ **Custom UI Styling**: Optional border styles, colors, rounded corners, and a draggable window.
- ✅ **Battery Charging Animation**: Shows an animated GIF when charging, hidden otherwise.

---
📣 Screenshots
📸 Img1
📸 Img2
📸 Img3
📸 Img4

---
## ⚙️ Configuration

The app uses an `appsettings.json` for easy customization.  
Example:
```json
{
  "BatteryMonitor": {
    "UpperThreshold": 90,
    "LowerThreshold": 15,
    "FullBatterySound": "Resources/Full Battery.wav",
    "LowBatterySound": "Resources/Low Battery.wav"
  }
}
```
---

## ✅ Tips:
1) UpperThreshold: The % at which to alert when charging.
2) LowerThreshold: The % at which to alert when discharging.
3) Sound files must exist in the Resources folder.

---

## 📦 Installation
1) Download the latest release from <a href="https://github.com/itish-vs/BatteryMonitor/releases">Releases</a>
2) Extract the zip to any folder.
3) Run BatteryMonitor.exe (no installer required).

---

## 🚫 Requirements
✔️ No installation needed if using self-contained build (recommended).
⚠️ If using the framework-dependent version, ensure the .NET Desktop Runtime 8.0 is installed. Download from <a href="https://dotnet.microsoft.com/en-us/download/dotnet/8.0">Microsoft</a>

---

## 🙌 Credits
Developed and maintained by Itish Nigam.
If you find this useful, give the repository a ⭐️ and share it!

---

## 📣 Support
If you encounter issues, please open an Issue describing the problem, steps to reproduce, and relevant logs or screenshots.

---

## 📜 License
This project is licensed under the MIT License — see <a href="https://github.com/itish-vs/BatteryMonitor/blob/main/LICENSE">LICENSE</a>.


#BatteryMonitor #WindowsTrayApp #BatteryAlert #BatterySaver #WindowsUtility #PowerManagement #LaptopTools #OpenSource #DotNet #CSharp #WinForms
