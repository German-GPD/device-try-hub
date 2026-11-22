using German.DeviceTrayHub;
using Xunit;

namespace DeviceTrayHub.Tests;

public sealed class KeychronBridgeStateParserTests
{
    [Fact]
    public void DetectsActiveLinkReceiverStatusReport()
    {
        byte[] report = [0x54, 0x02, 0x01, 0x00, 0x00];

        var valid = KeychronBridgeStateParser.TryParse(report, out var connected);

        Assert.True(valid);
        Assert.True(connected);
    }

    [Fact]
    public void ReportsNoActiveLinkWhenReceiverFlagIsZero()
    {
        byte[] report = [0x54, 0x02, 0x00, 0x00, 0x00];

        var valid = KeychronBridgeStateParser.TryParse(report, out var connected);

        Assert.True(valid);
        Assert.False(connected);
    }

    [Fact]
    public void RejectsWrongReportIdAndShortPackets()
    {
        byte[] wrongReport = [0x00, 0xB2, 0x01, 0x00];
        byte[] shortReport = [0x54, 0x02];

        Assert.False(KeychronBridgeStateParser.TryParse(wrongReport, out _));
        Assert.False(KeychronBridgeStateParser.TryParse(shortReport, out _));
    }
}
