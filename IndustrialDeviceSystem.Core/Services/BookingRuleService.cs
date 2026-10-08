using IndustrialDeviceSystem.Core.Enums;
using IndustrialDeviceSystem.Core.Models;

namespace IndustrialDeviceSystem.Core.Services;

public sealed class BookingRuleService
{
    public bool IsValidTimeRange(DateTime start, DateTime end)
    {
        return start < end;
    }

    public bool HasConflict(
        DateTime newStart,
        DateTime newEnd,
        DateTime existingStart,
        DateTime existingEnd)
    {
        return newStart < existingEnd &&
               existingStart < newEnd;
    }

    public BookingValidationResult Validate(
        Device device,
        Booking candidate,
        IEnumerable<Booking> existingBookings,
        IEnumerable<MaintenanceOrder> maintenanceOrders,
        UserRole role)
    {
        ArgumentNullException.ThrowIfNull(device);
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(existingBookings);
        ArgumentNullException.ThrowIfNull(maintenanceOrders);

        if (role is not UserRole.Administrator and
            not UserRole.Operator)
        {
            return Invalid(
                BookingErrorCode.UserNotAllowed,
                "当前用户没有创建设备预约的权限。");
        }

        if (!device.Enabled)
        {
            return Invalid(
                BookingErrorCode.DeviceDisabled,
                "设备已经停用。");
        }

        if (candidate.DeviceId != device.DeviceId)
        {
            return Invalid(
                BookingErrorCode.DeviceUnavailable,
                "预约设备与当前设备不一致。");
        }

        if (device.Status is not DeviceStatus.Idle and
            not DeviceStatus.Running)
        {
            return Invalid(
                BookingErrorCode.DeviceUnavailable,
                "当前设备状态不允许预约。");
        }

        if (!IsValidTimeRange(
                candidate.StartTime,
                candidate.EndTime))
        {
            return Invalid(
                BookingErrorCode.InvalidTimeRange,
                "预约开始时间必须早于结束时间。");
        }

        bool bookingConflict = existingBookings.Any(existing =>
            existing.DeviceId == candidate.DeviceId &&
            existing.Status is BookingStatus.Pending or
                BookingStatus.Confirmed &&
            HasConflict(
                candidate.StartTime,
                candidate.EndTime,
                existing.StartTime,
                existing.EndTime));

        if (bookingConflict)
        {
            return Invalid(
                BookingErrorCode.TimeConflict,
                "预约时段与已有预约冲突。");
        }

        bool maintenanceConflict = maintenanceOrders.Any(order =>
            order.DeviceId == candidate.DeviceId &&
            order.Status is not MaintenanceStatus.Closed and
                not MaintenanceStatus.Cancelled &&
            order.PlannedStartTime is DateTime maintenanceStart &&
            order.PlannedEndTime is DateTime maintenanceEnd &&
            IsValidTimeRange(
                maintenanceStart,
                maintenanceEnd) &&
            HasConflict(
                candidate.StartTime,
                candidate.EndTime,
                maintenanceStart,
                maintenanceEnd));

        if (maintenanceConflict)
        {
            return Invalid(
                BookingErrorCode.MaintenanceConflict,
                "预约时段与设备维护计划冲突。");
        }

        return new BookingValidationResult
        {
            IsValid = true,
            ErrorCode = BookingErrorCode.None,
            Message = "预约条件检查通过。"
        };
    }

    private static BookingValidationResult Invalid(
        BookingErrorCode errorCode,
        string message)
    {
        return new BookingValidationResult
        {
            IsValid = false,
            ErrorCode = errorCode,
            Message = message
        };
    }
}
