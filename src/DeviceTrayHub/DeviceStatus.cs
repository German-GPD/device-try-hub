namespace German.DeviceTrayHub;

public sealed record DeviceStatus(
    string Name,
    string Connection,
    string Battery,
    string Charging,
    string Detail);

internal static class DeviceMonitor
{
    public static IReadOnlyList<DeviceStatus> ReadCurrent() =>
    [
        KeychronBridgeAdapter.ReadStatus(),
        R1BatteryAdapter.ReadStatus(),
        Ds4WindowsAdapter.ReadStatus()
    ];
}