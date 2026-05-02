# Quick Resolution Switcher

A tiny Windows-native app for quickly switching between saved resolution and refresh-rate presets.

It's designed to be fast. The app is built with WinForms and talks directly to the Win32 display APIs.

<img width="500" height="auto" alt="QuickResolutionSwitcher_muInMfTHH7" src="https://github.com/user-attachments/assets/fae247f3-a0ed-4591-9343-3d453d51be5e" />

## Requirements

- .NET 10 SDK to build from source
- .NET 10 Desktop Runtime to run the release build

## Build

```powershell
dotnet publish -c Release -p:SelfContained=false -p:PublishSelfContained=false -p:PublishSingleFile=true -p:RuntimeIdentifier=win-x64
```

The executable is written to:

```text
bin\Release\net10.0-windows\win-x64\publish\QuickResolutionSwitcher.exe
```

## Notes

- Windows only accepts display modes exposed by the selected monitor, GPU, driver, and cable path. If a preset is unsupported, the app shows Windows' rejection reason.

- Custom presets are saved at:
```text
%AppData%\QuickResolutionSwitcher\presets.json
```
