namespace German.DeviceTrayHub;

internal readonly record struct R1BatteryReading(int Percentage, bool? IsCharging, byte RawState);

internal static class R1BatteryReportParser
{
    private const byte BatteryReportId = 0x03;
    private const byte R1DeviceId = 0x10;
    private const byte BatteryReady = 0x40;

    public static bool TryParse(ReadOnlySpan<byte> report, out R1BatteryReading reading)
    {
        reading = default;

        // The FA60 receiver sends a five-byte battery frame:
        // 03 10 40 <state> <percentage>. The last byte is already 0..100;
        // interpreting values 0..10 as tenths produces incorrect readings.
        if (report.Length < 5 ||
            report[0] != BatteryReportId ||
            report[1] != R1DeviceId ||
            report[2] != BatteryReady)
        {
            return false;
        }

        var percentage = report[4];
        if (percentage > 100) return false;

        var state = report[3];
        bool? isCharging = state switch
        {
            0x01 or 0x02 => false,
            0x03 or 0x80 => true,
            _ => null
        };

        reading = new R1BatteryReading(percentage, isCharging, state);
        return true;
    }
}
