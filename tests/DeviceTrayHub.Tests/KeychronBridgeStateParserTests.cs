using German.DeviceTrayHub;
using Xunit;

namespace DeviceTrayHub.Tests;

public sealed class KeychronBridgeStateParserTests
{
    [Fact]
    public void DetectsConnectedKeychronDeviceInReceiverSlot()
    {
        byte[] report = new byte[18];
        report[0] = 0x00;
        report[1] = 0xB2;
        report[3] = 0x34;
        report[4] = 0x34;
        report[5] = 0x0A;
        report[6] = 0x33;
        report[7] = 0x01;

        var valid = KeychronBridgeStateParser.TryParse(report, out var connected);

        Assert.True(valid);
        Assert.True(connected);
    }

    [Fact]
    public void ReportsDisconnectedWhenNoKeychronSlotIsActive()
    {
        byte[] report = new byte[18];
        report[0] = 0x00;
        report[1] = 0xB2;
        report[3] = 0x34;
        report[4] = 0x34;
        report[7] = 0x00;

        var valid = KeychronBridgeStateParser.TryParse(report, out var connected);

        Assert.True(valid);
        Assert.False(connected);
    }

    [Fact]
    public void DetectsKeychronInSecondOrThirdSlot()
    {
        foreach (var offset in new[] { 8, 13 })
        {
            byte[] report = new byte[18];
            report[0] = 0x00;
            report[1] = 0xB2;
            report[offset] = 0x34;
            report[offset + 1] = 0x34;
            report[offset + 4] = 0x01;

            Assert.True(KeychronBridgeStateParser.TryParse(report, out var connected));
            Assert.True(connected);
        }
    }

    [Fact]
    public void RejectsWrongReportIdCommandAndShortPackets()
    {
        byte[] wrongId = new byte[18];
        wrongId[0] = 0x01;
        wrongId[1] = 0xB2;

        byte[] wrongCommand = new byte[18];
        wrongCommand[0] = 0x00;
        wrongCommand[1] = 0xB1;

        byte[] shortReport = [0x00, 0xB2, 0x00];

        Assert.False(KeychronBridgeStateParser.TryParse(wrongId, out _));
        Assert.False(KeychronBridgeStateParser.TryParse(wrongCommand, out _));
        Assert.False(KeychronBridgeStateParser.TryParse(shortReport, out _));
    }
}
