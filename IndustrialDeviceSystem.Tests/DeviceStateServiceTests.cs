using IndustrialDeviceSystem.Core.Enums;
using IndustrialDeviceSystem.Core.Services;
using Xunit;

namespace IndustrialDeviceSystem.Tests;

public sealed class DeviceStateServiceTests
{
    private readonly DeviceStateService _service = new();

    [Theory]
    [InlineData(DeviceStatus.Offline, DeviceStatus.Idle)]
    [InlineData(DeviceStatus.Idle, DeviceStatus.Running)]
    [InlineData(DeviceStatus.Running, DeviceStatus.Idle)]
    [InlineData(DeviceStatus.Idle, DeviceStatus.Fault)]
    [InlineData(DeviceStatus.Running, DeviceStatus.Fault)]
    [InlineData(DeviceStatus.Fault, DeviceStatus.Maintenance)]
    [InlineData(DeviceStatus.Maintenance, DeviceStatus.Idle)]
    public void CanTransition_ShouldAcceptAllowedTransitions(
        DeviceStatus from,
        DeviceStatus to)
    {
        Assert.True(
            _service.CanTransition(from, to));
    }

    [Theory]
    [InlineData(DeviceStatus.Offline, DeviceStatus.Running)]
    [InlineData(DeviceStatus.Maintenance, DeviceStatus.Running)]
    [InlineData(DeviceStatus.Fault, DeviceStatus.Running)]
    [InlineData(DeviceStatus.Running, DeviceStatus.Maintenance)]
    public void CanTransition_ShouldRejectForbiddenTransitions(
        DeviceStatus from,
        DeviceStatus to)
    {
        Assert.False(
            _service.CanTransition(from, to));
    }

    [Theory]
    [InlineData(0, DeviceStatus.Running)]
    [InlineData(2, DeviceStatus.Running)]
    [InlineData(3, DeviceStatus.Offline)]
    [InlineData(4, DeviceStatus.Offline)]
    public void CommunicationFailures_ShouldUseMaximumOfThree(
        int consecutiveFailures,
        DeviceStatus expected)
    {
        DeviceStatus actual =
            _service.GetStatusAfterCommunicationFailures(
                DeviceStatus.Running,
                consecutiveFailures,
                3);

        Assert.Equal(expected, actual);
    }
}
