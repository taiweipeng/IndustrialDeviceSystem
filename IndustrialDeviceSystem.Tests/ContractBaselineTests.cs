using IndustrialDeviceSystem.Core.Enums;
using IndustrialDeviceSystem.Core.Models;
using Xunit;

namespace IndustrialDeviceSystem.Tests;

public sealed class ContractBaselineTests
{
    [Fact]
    public void DeviceStatus_ShouldUseFixedNumericValues()
    {
        Assert.Equal(0, (int)DeviceStatus.Offline);
        Assert.Equal(1, (int)DeviceStatus.Idle);
        Assert.Equal(2, (int)DeviceStatus.Running);
        Assert.Equal(3, (int)DeviceStatus.Maintenance);
        Assert.Equal(4, (int)DeviceStatus.Fault);
    }

    [Fact]
    public void BookingStatus_ShouldUseFixedNumericValues()
    {
        Assert.Equal(0, (int)BookingStatus.Pending);
        Assert.Equal(1, (int)BookingStatus.Confirmed);
        Assert.Equal(2, (int)BookingStatus.Cancelled);
        Assert.Equal(3, (int)BookingStatus.Completed);
        Assert.Equal(4, (int)BookingStatus.Rejected);
    }

    [Fact]
    public void AlarmLevel_ShouldUseFixedNumericValues()
    {
        Assert.Equal(0, (int)AlarmLevel.Normal);
        Assert.Equal(1, (int)AlarmLevel.Warning);
        Assert.Equal(2, (int)AlarmLevel.High);
        Assert.Equal(3, (int)AlarmLevel.Emergency);
    }

    [Fact]
    public void TemperatureThresholds_ShouldUseTeachingDefaults()
    {
        var thresholds = new TemperatureThresholds();

        Assert.Equal(60.0, thresholds.Warning);
        Assert.Equal(80.0, thresholds.High);
        Assert.Equal(90.0, thresholds.Emergency);
    }

    [Fact]
    public void NewDevice_ShouldUseSafeDefaults()
    {
        var device = new Device();

        Assert.True(device.Enabled);
        Assert.Equal(DeviceStatus.Offline, device.Status);
        Assert.Equal(string.Empty, device.DeviceCode);
    }

    [Fact]
    public void NewAlarm_ShouldStartWithOneOccurrence()
    {
        var alarm = new Alarm();

        Assert.Equal(1, alarm.OccurrenceCount);
        Assert.Equal(AlarmStatus.Active, alarm.Status);
    }
}
