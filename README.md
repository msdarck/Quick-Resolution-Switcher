# Quick Resolution Switcher

A tiny Windows-native app for quickly switching between saved resolution and refresh-rate presets.

It's designed to be fast. The app is built with WinForms and talks directly to the Win32 display APIs.

<img width="500" height="auto" alt="QuickResolutionSwitcher_muInMfTHH7" src="https://github.com/user-attachments/assets/fae247f3-a0ed-4591-9343-3d453d51be5e" />

## Preset Storage

Custom presets are saved at:

```text
%AppData%\QuickResolutionSwitcher\presets.json
```

## Requirements

- .NET 10 SDK to build from source
- .NET 10 Desktop Runtime to run the framework-dependent release build

## Build

```powershell
dotnet build -c Release
```

## Publish

```powershell
dotnet publish -c Release -p:SelfContained=false -p:PublishSelfContained=false -p:PublishSingleFile=true -p:RuntimeIdentifier=win-x64 -o .\publish\framework-dependent-single-file
```

The executable is written to:

```text
publish\framework-dependent-single-file\QuickResolutionSwitcher.exe
```

## Notes

Windows only accepts display modes exposed by the selected monitor, GPU, driver, and cable path. If a preset is unsupported, the app shows Windows' rejection reason.
