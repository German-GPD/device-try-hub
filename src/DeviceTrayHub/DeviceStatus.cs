namespace German.DeviceTrayHub;

public sealed record DeviceStatus(
    string Name,
    string Connection,
    string Battery,
    string Charging,
    string Detail);

internal static class DeviceMonitor
{
    // Deliberately conservative: no percentage or connection state is fabricated.
    // Model-specific HID/DS4Windows adapters must replace these placeholders after
    // the exact hardware protocol has been verified on physical devices.
    public static IReadOnlyList<DeviceStatus> ReadCurrent() =>
    [
        new("Keychron K3 Max", "Ожидает проверки HID", "Недоступно", "Неизвестно",
            "USB-приёмник обнаруживается отдельно; протокол батареи ещё не подтверждён."),
        new("Attack Shark R1", "Ожидает проверки HID", "Недоступно", "Неизвестно",
            "Нужна проверка идентификаторов и отчётов конкретного приёмника."),
        new("Sony DualShock 4", "Ожидает интеграции", "Недоступно", "Неизвестно",
            "Нужно подтвердить доступный интерфейс DS4Windows или Bluetooth API.")
    ];
}