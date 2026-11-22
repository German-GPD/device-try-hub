namespace German.DeviceTrayHub;

internal readonly record struct R1BatteryReading(int Percentage, bool? IsCharging);

internal static class R1BatteryReportParser
{
    private const byte BatteryReportId = 0x03;
    private const byte R1DeviceId = 0x10;
    private const byte BatteryReady = 0x40;

    public static bool TryParse(ReadOnlySpan<byte> report, out R1BatteryReading reading)
    {
        reading = default;

        if (report.Length < 5 ||
            report[0] != BatteryReportId ||
            report[1] != R1DeviceId ||
            report[2] != BatteryReady)
        {
            return false;
        }

        var rawCharge = report[4];
        if (rawCharge > 100) return false;

        var percentage = rawCharge <= 10 ? rawCharge * 10 : rawCharge;
        bool? isCharging = report[3] switch
        {
            0x01 => false,
            0x02 or 0x03 or 0x80 => true,
            _ => null
        };

        reading = new R1BatteryReading(percentage, isCharging);
        return true;
    }
}