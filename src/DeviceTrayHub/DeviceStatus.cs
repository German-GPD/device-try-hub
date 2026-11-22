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
        new(
            "Keychron K3 Max",
            "Ожидает адаптер",
            "Недоступно",
            "Неизвестно",
            "Протокол батареи через 2,4 ГГц-приёмник пока не подтверждён."),
        R1BatteryAdapter.ReadStatus(),
        new(
            "Sony DualShock 4",
            "Ожидает интеграции DS4Windows",
            "Недоступно",
            "Неизвестно",
            "Нужно включить и проверить внешний мониторинг OSC или IPC.")
    ];
}