# German'$ Device Tray Hub

A lightweight Windows tray application for viewing the connection and battery state of selected wireless peripherals from one compact popup.

## Current project status

**Early implementation — UI and installer pipeline are being built. Hardware telemetry is not implemented yet.**

The current branch contains the first WPF application shell, tray integration, Windows theme/accent detection, a conservative device-status model, an Inno Setup installer definition, and a Windows CI build workflow. The three device rows intentionally show that hardware reporting is pending; no battery values or connection states are fabricated.

## Target devices

| Device | Connection | Integration status |
| --- | --- | --- |
| Keychron K3 Max | 2.4 GHz USB receiver | HID protocol investigation required |
| Attack Shark R1 | 2.4 GHz USB receiver | HID protocol investigation required |
| Sony DualShock 4 | Bluetooth USB adapter / DS4Windows | Integration interface still needs verification |

A USB receiver does not automatically expose battery information to Windows. The device firmware and receiver protocol must provide a reliable report. Battery percentage and charging state will remain unavailable until confirmed against the actual hardware.

## Current implementation

- One notification-area icon; left-click opens the compact device popup.
- Context menu with Open and Exit.
- Single-instance process guard.
- Popup closes when it loses focus.
- Light/dark mode follows the Windows app theme.
- Accent color is read from the current Windows user theme and refreshed after Windows preference changes.
- Optional per-user startup registration; no administrator permission is required.
- Self-contained x64 publish and Inno Setup installer pipeline.
- No background polling loop is enabled until real device adapters are ready.

## Planned work

1. Identify the exact USB VID/PID and HID interfaces for the K3 Max and R1 receivers.
2. Inspect device reports safely and implement model-specific battery/charging adapters only where data is documented or verified.
3. Verify the available DS4Windows integration surface and compare it with Windows Bluetooth device information.
4. Add settings for refresh behavior, startup, and diagnostics.
5. Add device reconnect handling, bounded polling, structured logs, and failure recovery.
6. Test on the actual devices and Windows 11 before marking any model as supported.
7. Add signed releases and an update mechanism after the installer is stable.

## Technology

- C# / WPF on **.NET 10 LTS**
- Windows notification-area integration
- Windows Registry for per-user theme, accent, and startup preferences
- Inno Setup for the Windows installer
- GitHub Actions for Windows x64 build and installer packaging

.NET 10 is used because it is the current LTS line. See the [official .NET support policy](https://learn.microsoft.com/dotnet/core/releases-and-support).

## Build

Requirements: Windows 11 x64, .NET 10 SDK, and Inno Setup 6.

\`\`\`powershell
dotnet restore src/DeviceTrayHub/DeviceTrayHub.csproj
dotnet build src/DeviceTrayHub/DeviceTrayHub.csproj -c Release
dotnet publish src/DeviceTrayHub/DeviceTrayHub.csproj -c Release -r win-x64 --self-contained true -o artifacts/publish -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
\`\`\`

Then compile the installer:

\`\`\`powershell
iscc installer/DeviceTrayHub.iss
\`\`\`

GitHub Actions runs the Windows build and publishes the setup executable as a workflow artifact.

## Platform

- Primary target: Windows 11 x64.
- Windows 10 compatibility is not promised by the current .NET 10 target; it requires a separate compatibility decision and test matrix.

## Reliability policy

- Unknown means unknown — never estimate a battery percentage.
- A device is not considered supported until detection and telemetry have been tested on the physical model and receiver.
- Missing charging information is displayed as unavailable, not inferred from battery movement.
- Device communication must use bounded polling and must not block the UI thread.

## License

Not decided yet.
