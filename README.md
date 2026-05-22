# LuminaDesktop

LuminaDesktop is a powerful all-in-one desktop utility application for Windows. It integrates three standalone productivity tools into a single, unified system tray application with a modern, dark-themed configuration dashboard.

## Features

LuminaDesktop consists of three core modules that can be individually toggled:

### 1. Mousewarp
Automatically moves (warps) your mouse cursor to the center of the newly activated window whenever you use `Alt+Tab`. This saves time and reduces mouse movement across large or multiple monitors.

### 2. Better Window Switcher (bws)
A high-performance, minimalist replacement for the default Windows `Alt+Tab` menu. It provides a faster, distraction-free window switching experience.

### 3. LumaEdges
Screen edge hotkeys. Assign custom keyboard shortcuts to specific zones on the edge of your screen (Top, Bottom, Left, Right, and all four corners). Trigger actions simply by bumping your mouse against the edge of the screen and clicking.
- Customizable edge thickness
- Customizable trigger mouse button (Left, Right, Middle)

### Core Capabilities
- **Start with Windows**: Automatically launch seamlessly in the background on system startup.
- **Start as Administrator**: Automatically elevate the application privileges. This ensures that modules like Mousewarp and BWS work flawlessly even when interacting with other elevated applications (like Task Manager or admin terminals).
- **Modern UI**: A fully custom, fluent-inspired dark theme Settings Dashboard built with WPF.

---

## Installation

You can install LuminaDesktop easily using the provided setup executable.

1. Go to the [Releases](https://github.com/quartz1216/LuminaDesktop/releases) page or locate the `LuminaDesktopInstaller_v1.0.0.exe` in the `Output/` folder.
2. Run the installer and follow the instructions.
3. Once installed, LuminaDesktop will run in your system tray. Right-click the icon and select **Settings...** to configure the modules.

---

## Building from Source

LuminaDesktop is built with C# and WPF on **.NET 10.0**.

### Prerequisites
- [.NET 10.0 SDK](https://dotnet.microsoft.com/)
- [Inno Setup 6](https://jrsoftware.org/isinfo.php) (Required only if you want to build the installer executable)

### Automated Build & Packaging
We provide a PowerShell script to automatically build the project and compile the installer.

1. Clone the repository:
   ```bash
   git clone https://github.com/quartz1216/LuminaDesktop.git
   cd LuminaDesktop
   ```
2. Run the build script:
   ```powershell
   .\build.ps1
   ```
3. The compiled installer will be available in the `Output/` directory.

### Manual Build
If you only want to compile the executable without building an installer:
```bash
dotnet build -c Release
```
The executable will be located in `bin/Release/net10.0-windows10.0.19041.0/`.

---

## License
MIT License
