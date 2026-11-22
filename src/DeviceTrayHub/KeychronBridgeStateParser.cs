namespace German.DeviceTrayHub;

internal static class KeychronBridgeStateParser
{
    private const byte ReceiverStatusReportId = 0x54;

    public static bool TryParse(ReadOnlySpan<byte> report, out bool keyboardConnected)
    {
        keyboardConnected = false;

        // Link receiver status input report: 54 <channel> <carrier/link flag> ...
        if (report.Length < 3 || report[0] != ReceiverStatusReportId)
            return false;

        keyboardConnected = report[2] != 0;
        return true;
    }
}
