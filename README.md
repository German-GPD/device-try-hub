# German'$ Device Tray Hub

A lightweight Windows system-tray utility for monitoring the battery and connection state of selected wireless peripherals.

The goal is to give each supported device its own tray icon, making it easy to see whether it is connected, how much battery remains, and whether it is charging — without opening several manufacturer utilities.

## Initial Device Support

The initial development scope is intentionally limited to the hardware currently available for testing:

| Device | Connection | Planned integration |
| --- | --- | --- |
| Keychron K3 Max | 2.4 GHz USB receiver | Device-specific HID investigation |
| Attack Shark R1 | 2.4 GHz USB receiver | Device-specific HID investigation |
| Sony DualShock 4 (DS4) | Bluetooth USB adapter | DS4Windows integration / Windows device APIs |

Support for other devices is not currently planned or guaranteed. Additional models may be considered after the initial devices are implemented and tested.

## Planned Features

- A separate system-tray icon for each supported device.
- Battery percentage, when the device exposes reliable battery information.
- Connection state: connected or disconnected.
- Charging state: charging, not charging, or unknown when the hardware does not report it.
- Automatic device detection and periodic status updates.
- Clear indication when a device is unavailable or its battery level cannot be read.
- Optional launch at Windows sign-in.
- A small settings window with minimal background resource usage.

> **Hardware limitation:** A USB receiver does not automatically expose battery information to Windows. Battery reporting and charging detection depend on each device's firmware and communication protocol. The application will not invent or estimate battery percentages; unavailable values will be shown as unknown.

## Technology

The planned implementation is based on:

- **C# / .NET** — application logic.
- **WPF / XAML** — desktop interface and settings.
- **Windows APIs (Win32)** — device discovery and system integration.
- **HID** — communication with compatible USB receivers and peripherals.
- **Windows Bluetooth APIs** — Bluetooth device information where available.
- **DS4Windows** — integration point for the DualShock 4 workflow, subject to the available interface and reported data.

The exact framework target and device-specific libraries will be finalized during implementation and hardware testing.

## Platform

- Windows 10 / Windows 11
- x64

## Development Status

**Early planning / initial repository stage.**

The repository currently documents the project scope. Device communication, battery reporting, tray icons, and the user interface still need to be implemented and verified on the actual hardware.

## Scope and Compatibility

This is a device-specific utility, not a universal peripheral manager. A device will only be marked as supported after its detection and reported status have been tested.

If a device does not provide a reliable battery or charging value, the application will show that the information is unavailable instead of displaying a guessed value.

## License

License: not decided yet.
