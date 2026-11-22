# German'$ Device Tray Hub

A lightweight Windows tray application for viewing the connection and battery state of selected wireless peripherals from one compact popup.

## Current project status

**Early implementation. The application shell and first two telemetry integrations are in progress; hardware validation is still required.**

The current branch contains the WPF tray application, system theme/accent integration, per-user startup, asynchronous polling, an experimental Attack Shark R1 HID adapter, a DS4Windows IPC adapter, an Inno Setup installer definition, and a Windows x64 build workflow.

## Target devices

| Device | Connection | Current integration |
| --- | --- | --- |
| Keychron K3 Max | 2.4 GHz USB receiver | Experimental receiver/keyboard connection detection; battery still unavailable |
| Attack Shark R1 | 2.4 GHz USB receiver | Experimental HID reader; physical validation pending |
| Sony DualShock 4 | Bluetooth USB adapter / DS4Windows | DS4Windows IPC query adapter; requires DS4Windows to be running |

## Implemented

- One custom-drawn notification-area icon; left-click opens the compact device popup.
- Context menu with Open and Exit.
- Single-instance process guard.
- Popup closes when it loses focus.
- Light/dark mode follows the Windows app theme.
- Accent color is read from the current Windows user theme and refreshed after Windows preference changes.
- Windows high-contrast colors take priority for accessibility.
- Optional per-user startup registration; no administrator permission is required.
- Device polling runs off the UI thread and avoids overlapping refresh cycles.
- **Keychron K3 Max (experimental):** probes Keychron Link receivers (VID/PID `3434:D030` / `3434:D031`) with the read-only `FR_GET_STATE` command and reports whether a Keychron device is connected in a receiver slot. It does not claim a battery percentage because the exact battery response over this receiver path is not yet verified.
- **Attack Shark R1 (experimental):** reads the wireless receiver's HID input report for VID/PID `1D57:FA60`, interface 2, validates report ID/device/status bytes, and decodes battery and known charging flags. If the mouse sleeps, the last confirmed value is retained; disconnecting the receiver clears it.
- **DualShock 4:** reads battery, charging, and connection type from a running `DS4Windows.exe` process using its documented read-only IPC query command. The first controller slot is used.
- Self-contained x64 publish and Inno Setup installer pipeline.
- PowerShell build script checks the SDK, builds, runs tests, publishes, and packages the installer.
- Unit tests cover Keychron receiver-state parsing and R1 report signature validation, charge scaling, and charging-flag decoding.

## Hardware notes

### Attack Shark R1

The adapter is based on a public Linux driver that documents the wireless receiver as USB `1D57:FA60`, HID interface 2, battery input report ID `0x03`, and charging/status fields. Its report format is ported to a Windows HID reader; it has **not yet been verified on the user's physical mouse and receiver**.

The adapter only displays a percentage after the report ID, R1 device identifier, ready status, and charge range pass validation. A timeout does not create a new percentage. It keeps the last confirmed reading while the receiver remains present.

Reference: [jlucaso1/attack-shark-r1](https://github.com/jlucaso1/attack-shark-r1).

### Sony DualShock 4

The app looks for a running `DS4Windows.exe` process and sends read-only queries such as `Query.1.Battery`, `Query.1.Charging`, and `Query.1.ConnType` through DS4Windows' documented IPC interface. It does not change controller settings or install drivers.

Reference: [DS4Windows command-line query documentation](https://ds4window.org/command-line/).

### Keychron K3 Max

The receiver adapter uses the documented Keychron Link bridge protocol to query connected slots. The receiver reports the connected peripheral state, but the current public protocol notes do not establish a trustworthy battery percentage for this exact keyboard/receiver path. The app therefore reports connection state only and leaves battery/charging unavailable. It will not substitute a guessed value or silently switch the keyboard to Bluetooth.

Reference: [Keychron Vial protocol notes — bridge/dongle](https://github.com/Tymon3310/keychron-vial/blob/main/docs/launcher/bridge-dongle-protocol.md) and [wireless communication](https://github.com/Tymon3310/keychron-vial/blob/main/docs/launcher/wireless-communication.md).

## Planned work

1. Validate the R1 adapter against the actual receiver and mouse, including sleep, wake, charging, and disconnect behavior.
2. Confirm the DS4Windows query output on the installed DS4Windows version and test disconnect/reconnect and charging states.
3. Continue investigating a reliable K3 Max battery source while preserving the user's 2.4 GHz connection; receiver connection detection is already implemented experimentally.
4. Add settings for polling interval, DS4 controller slot, startup, and diagnostics.
5. Add device reconnect handling, bounded polling, structured logs, and failure recovery.
6. Improve tray icon, popup animation, accessibility, and high-contrast support.
7. Add signed releases and an update mechanism after the installer is stable.

## Technology

- C# / WPF on **.NET 10 LTS**
- [HidSharp 2.6.4](https://www.nuget.org/packages/HidSharp) for Windows HID access
- Windows notification-area integration
- Windows Registry for per-user theme, accent, and startup preferences
- Inno Setup for the Windows installer
- GitHub Actions for Windows x64 build and installer packaging

.NET 10 is the current LTS line. See the [official .NET support policy](https://learn.microsoft.com/dotnet/core/releases-and-support).

## Build

Requirements: Windows 11 x64, .NET 10 SDK, and Inno Setup 6.

Run PowerShell from the repository root:

```powershell
Set-ExecutionPolicy -Scope Process Bypass
.\scripts\build-installer.ps1
```

The script restores dependencies, builds the app, runs unit tests, publishes a self-contained x64 executable, and compiles the installer. Output: `artifacts/installer/German-Device-Tray-Hub-Setup.exe`.

GitHub Actions runs the same build/test/publish/installer pipeline and uploads the setup executable as a workflow artifact.

## Platform

- Primary target: Windows 11 x64.
- Windows 10 compatibility is not promised by the current .NET 10 target; it requires a separate compatibility decision and test matrix.

## Privacy and permissions

- No analytics, advertising, or telemetry is sent by the app.
- No administrator rights or third-party device drivers are required.
- The R1 reader only opens the matching HID interface and reads reports.
- DS4Windows integration uses read-only local IPC queries.
- Startup registration is per-user in HKCU; uninstall removes the installer-created startup entry.

## Reliability policy

- Unknown means unknown — never estimate a battery percentage.
- A device is not considered fully supported until detection and telemetry have been tested on the physical model and receiver.
- Missing charging information is displayed as unavailable, not inferred from battery movement.
- Device communication must use bounded timeouts and must not block the UI thread.

## License

Project license: not decided yet. Third-party dependencies and protocol references are documented in [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).
