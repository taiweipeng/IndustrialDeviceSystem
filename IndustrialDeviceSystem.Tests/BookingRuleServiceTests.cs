using System.Globalization;
using IndustrialDeviceSystem.Core.Enums;
using IndustrialDeviceSystem.Core.Models;
using IndustrialDeviceSystem.Core.Services;
using Xunit;

namespace IndustrialDeviceSystem.Tests;

public sealed class BookingRuleServiceTests
{
    private readonly BookingRuleService _service = new();

    [Theory]
    [InlineData(
        "2026-10-10T08:00:00",
        "2026-10-10T09:00:00",
        "2026-10-10T09:00:00",
        "2026-10-10T10:00:00",
        false)]
    [InlineData(
        "2026-10-10T10:00:00",
        "2026-10-10T11:00:00",
        "2026-10-10T09:00:00",
        "2026-10-10T10:00:00",
        false)]
    [InlineData(
        "2026-10-10T08:30:00",
        "2026-10-10T09:30:00",
        "2026-10-10T09:00:00",
        "2026-10-10T10:00:00",
        true)]
    [InlineData(
        "2026-10-10T09:15:00",
        "2026-10-10T09:45:00",
        "2026-10-10T09:00:00",
        "2026-10-10T10:00:00",
        true)]
    [InlineData(
        "2026-10-10T08:00:00",
        "2026-10-10T11:00:00",
        "2026-10-10T09:00:00",
        "2026-10-10T10:00:00",
        true)]
    public void HasConflict_ShouldReturnExpectedResult(
        string newStartText,
        string newEndText,
        string existingStartText,
        string existingEndText,
        bool expected)
    {
        DateTime newStart = Parse(newStartText);
        DateTime newEnd = Parse(newEndText);
        DateTime existingStart = Parse(existingStartText);
        DateTime existingEnd = Parse(existingEndText);

        bool actual = _service.HasConflict(
            newStart,
            newEnd,
            existingStart,
            existingEnd);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Validate_ShouldAcceptValidBooking()
    {
        Device device = CreateIdleDevice();
        Booking candidate = CreateBooking(
            "2026-10-10T10:00:00",
            "2026-10-10T11:00:00");

        BookingValidationResult result =
            _service.Validate(
                device,
                candidate,
                Array.Empty<Booking>(),
                Array.Empty<MaintenanceOrder>(),
                UserRole.Operator);

        Assert.True(result.IsValid);
        Assert.Equal(
            BookingErrorCode.None,
            result.ErrorCode);
    }

    [Fact]
    public void Validate_ShouldRejectOfflineDevice()
    {
        Device device = CreateIdleDevice();
        device.Status = DeviceStatus.Offline;

        Booking candidate = CreateBooking(
            "2026-10-10T10:00:00",
            "2026-10-10T11:00:00");

        BookingValidationResult result =
            _service.Validate(
                device,
                candidate,
                Array.Empty<Booking>(),
                Array.Empty<MaintenanceOrder>(),
                UserRole.Operator);

        Assert.False(result.IsValid);
        Assert.Equal(
            BookingErrorCode.DeviceUnavailable,
            result.ErrorCode);
    }

    [Fact]
    public void Validate_ShouldRejectMaintenanceConflict()
    {
        Device device = CreateIdleDevice();

        Booking candidate = CreateBooking(
            "2026-10-10T09:30:00",
            "2026-10-10T10:30:00");

        var maintenanceOrder =
            new MaintenanceOrder
            {
                DeviceId = 1,
                Status = MaintenanceStatus.Assigned,
                PlannedStartTime =
                    Parse("2026-10-10T09:00:00"),
                PlannedEndTime =
                    Parse("2026-10-10T10:00:00")
            };

        BookingValidationResult result =
            _service.Validate(
                device,
                candidate,
                Array.Empty<Booking>(),
                new[] { maintenanceOrder },
                UserRole.Operator);

        Assert.False(result.IsValid);
        Assert.Equal(
            BookingErrorCode.MaintenanceConflict,
            result.ErrorCode);
    }

    private static Device CreateIdleDevice()
    {
        return new Device
        {
            DeviceId = 1,
            DeviceCode = "DEV-001",
            DeviceName = "教学设备",
            Status = DeviceStatus.Idle,
            Enabled = true
        };
    }

    private static Booking CreateBooking(
        string start,
        string end)
    {
        return new Booking
        {
            DeviceId = 1,
            UserId = 1,
            StartTime = Parse(start),
            EndTime = Parse(end),
            Status = BookingStatus.Pending
        };
    }

    private static DateTime Parse(string value)
    {
        return DateTime.Parse(
            value,
            CultureInfo.InvariantCulture);
    }
}
