using IndustrialDeviceSystem.Core.Enums;
using IndustrialDeviceSystem.Core.Models;
using IndustrialDeviceSystem.Core.Services;
using Xunit;

namespace IndustrialDeviceSystem.Tests;

public sealed class MaintenanceRuleServiceTests
{
    private readonly MaintenanceRuleService _service = new();

    [Theory]
    [InlineData(
        MaintenanceStatus.InProgress,
        "已更换损坏轴承",
        true)]
    [InlineData(
        MaintenanceStatus.InProgress,
        "   ",
        false)]
    [InlineData(
        MaintenanceStatus.Created,
        "已更换损坏轴承",
        false)]
    public void CanComplete_ShouldCheckStatusAndResult(
        MaintenanceStatus status,
        string resultText,
        bool expected)
    {
        var order = new MaintenanceOrder
        {
            Status = status,
            Result = resultText
        };

        Assert.Equal(
            expected,
            _service.CanComplete(order));
    }

    [Theory]
    [InlineData(
        MaintenanceStatus.Completed,
        true,
        true)]
    [InlineData(
        MaintenanceStatus.Completed,
        false,
        false)]
    [InlineData(
        MaintenanceStatus.InProgress,
        true,
        false)]
    public void CanClose_ShouldRequireCompletedAndVerified(
        MaintenanceStatus status,
        bool verificationPassed,
        bool expected)
    {
        var order = new MaintenanceOrder
        {
            Status = status
        };

        Assert.Equal(
            expected,
            _service.CanClose(
                order,
                verificationPassed));
    }
}
