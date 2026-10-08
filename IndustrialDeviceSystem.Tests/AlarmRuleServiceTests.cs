using IndustrialDeviceSystem.Core.Enums;
using IndustrialDeviceSystem.Core.Models;
using IndustrialDeviceSystem.Core.Services;
using Xunit;

namespace IndustrialDeviceSystem.Tests;

public sealed class AlarmRuleServiceTests
{
    private readonly AlarmRuleService _service = new();

    private readonly TemperatureThresholds _thresholds =
        new()
        {
            Warning = 60.0,
            High = 80.0,
            Emergency = 90.0
        };

    [Theory]
    [InlineData(59.9, AlarmLevel.Normal)]
    [InlineData(60.0, AlarmLevel.Warning)]
    [InlineData(79.9, AlarmLevel.Warning)]
    [InlineData(80.0, AlarmLevel.High)]
    [InlineData(89.9, AlarmLevel.High)]
    [InlineData(90.0, AlarmLevel.Emergency)]
    public void GetTemperatureLevel_ShouldCoverBoundaries(
        double temperature,
        AlarmLevel expected)
    {
        AlarmLevel actual =
            _service.GetTemperatureLevel(
                temperature,
                _thresholds);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetTemperatureLevel_ShouldRejectInvalidThresholds()
    {
        var invalidThresholds =
            new TemperatureThresholds
            {
                Warning = 80.0,
                High = 60.0,
                Emergency = 90.0
            };

        Assert.Throws<ArgumentException>(() =>
            _service.GetTemperatureLevel(
                70.0,
                invalidThresholds));
    }
}
