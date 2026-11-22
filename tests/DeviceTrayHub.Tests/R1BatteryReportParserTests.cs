using German.DeviceTrayHub;
using Xunit;

namespace DeviceTrayHub.Tests;

public sealed class R1BatteryReportParserTests
{
    [Fact]
    public void ParsesTenthsScaleAndDischargingFlag()
    {
        byte[] report = [0x03, 0x10, 0x40, 0x01, 6, 0, 0, 0];

        var valid = R1BatteryReportParser.TryParse(report, out var reading);

        Assert.True(valid);
        Assert.Equal(60, reading.Percentage);
        Assert.False(reading.IsCharging);
    }

    [Fact]
    public void ParsesPercentageScaleAndWirelessChargingFlag()
    {
        byte[] report = [0x03, 0x10, 0x40, 0x03, 87, 0, 0, 0];

        var valid = R1BatteryReportParser.TryParse(report, out var reading);

        Assert.True(valid);
        Assert.Equal(87, reading.Percentage);
        Assert.True(reading.IsCharging);
    }

    [Fact]
    public void LeavesUnknownChargingFlagUnknown()
    {
        byte[] report = [0x03, 0x10, 0x40, 0x00, 4, 0, 0, 0];

        var valid = R1BatteryReportParser.TryParse(report, out var reading);

        Assert.True(valid);
        Assert.Equal(40, reading.Percentage);
        Assert.Null(reading.IsCharging);
    }

    [Fact]
    public void RejectsMalformedOrOutOfRangeReports()
    {
        byte[][] reports =
        [
            [0x02, 0x10, 0x40, 0x01, 6],
            [0x03, 0x11, 0x40, 0x01, 6],
            [0x03, 0x10, 0x00, 0x01, 6],
            [0x03, 0x10, 0x40, 0x01, 101],
            [0x03, 0x10, 0x40]
        ];

        foreach (var report in reports)
            Assert.False(R1BatteryReportParser.TryParse(report, out _));
    }
}