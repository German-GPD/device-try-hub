namespace German.DeviceTrayHub;

internal static class KeychronBridgeStateParser
{
    private const byte GetStateCommand = 0xB2;
    private const int KeychronVendorId = 0x3434;

    public static bool TryParse(ReadOnlySpan<byte> report, out bool keyboardConnected)
    {
        keyboardConnected = false;

        // HidSharp includes the HID report ID at index 0; bridge payload starts at index 1.
        if (report.Length < 18 || report[0] != 0x00 || report[1] != GetStateCommand)
            return false;

        // Three keyboard-bridge slots, each encoded as VID (BE), PID (BE), state.
        for (var offset = 3; offset <= 13; offset += 5)
        {
            var vendorId = (report[offset] << 8) | report[offset + 1];
            var status = report[offset + 4];

            if (vendorId == KeychronVendorId && status == 1)
            {
                keyboardConnected = true;
                return true;
            }
        }

        return true;
    }
}