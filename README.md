# ProxyStat for Windows

A Windows system tray application that shows whether the system proxy is enabled.

![Windows system tray screenshot showing ProxyStat status tooltip above the taskbar overflow panel. Tooltip text reads ProxyStat - Proxy Enabled and http=localhost:8000;https=localhost:80. In the overflow panel, the orange ProxyStat icon is highlighted among other app icons, and the taskbar at the bottom shows 1:55 PM and 1/10/2026. The scene has a clean, practical, informative tone.](assets/screenshot.png)

This is a Windows port of the macOS ProxyStat application.

## Features

- **System Tray Icon**: Shows proxy status at a glance
  - 🟠 Orange icon: Proxy is enabled
  - ⚫ Gray icon: Proxy is disabled
- **Instant Updates**: Responds immediately to proxy setting changes (no polling)
- **Quick Access**: Double-click or right-click to access proxy settings
- **Context Menu**:
  - Open System Proxy Settings
  - Quit

## Requirements

- Windows 10/11
- No runtime required (self-contained executable)

## Building

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (for building only)

### Build

```bash
cd ProxyStat
dotnet build
```

### Run

```bash
dotnet run
```

### Publish (Self-contained executable)

```bash
dotnet publish -c Release
```

The executable will be in `bin/Release/net10.0-windows/win-x64/publish/ProxyStat.exe`

## How It Works

The app reads the Windows proxy settings from the registry key:
`HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings`

It uses `RegNotifyChangeKeyValue` to watch for changes — no polling, instant response when proxy settings change.

## License

MIT
