# Quick Resolution Switcher

A tiny Windows-native app for quickly switching a selected monitor between saved resolution and refresh-rate presets.

It is built with WinForms and calls the Win32 display APIs directly. No Electron, no webview, no background service.

## Features

- Select an active monitor
- Distinguish primary and secondary displays
- Apply saved resolution presets
- Add or remove custom presets
- Dark native UI
- Stores presets locally as JSON
- Publishes as a single Windows executable

## Preset Storage

Custom presets are saved at:

```text
%AppData%\QuickResolutionSwitcher\presets.json
```

For a default Windows user path, that expands to something like:

```text
C:\Users\<you>\AppData\Roaming\QuickResolutionSwitcher\presets.json
```

## Requirements

- Windows
- .NET 10 SDK to build from source

The published `win-x64` executable is self-contained.

## Build

```powershell
dotnet build -c Release
```

## Publish

```powershell
dotnet publish -c Release -r win-x64
```

The executable is written to:

```text
bin\Release\net10.0-windows\win-x64\publish\QuickResolutionSwitcher.exe
```

## Notes

Windows only accepts display modes exposed by the selected monitor, GPU, driver, and cable path. If a preset is unsupported, the app shows Windows' rejection reason.

## License

No license has been selected yet.
