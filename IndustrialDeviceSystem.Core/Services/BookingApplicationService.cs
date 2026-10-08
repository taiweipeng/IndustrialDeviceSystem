using IndustrialDeviceSystem.Core.Enums;
using IndustrialDeviceSystem.Core.Interfaces;
using IndustrialDeviceSystem.Core.Models;

namespace IndustrialDeviceSystem.Core.Services;

public sealed class BookingApplicationService
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IMaintenanceRepository _maintenanceRepository;
    private readonly BookingRuleService _ruleService;
    private readonly IClock _clock;

    public BookingApplicationService(
        IDeviceRepository deviceRepository,
        IBookingRepository bookingRepository,
        IMaintenanceRepository maintenanceRepository,
        BookingRuleService ruleService,
        IClock clock)
    {
        ArgumentNullException.ThrowIfNull(deviceRepository);
        ArgumentNullException.ThrowIfNull(bookingRepository);
        ArgumentNullException.ThrowIfNull(maintenanceRepository);
        ArgumentNullException.ThrowIfNull(ruleService);
        ArgumentNullException.ThrowIfNull(clock);

        _deviceRepository = deviceRepository;
        _bookingRepository = bookingRepository;
        _maintenanceRepository = maintenanceRepository;
        _ruleService = ruleService;
        _clock = clock;
    }

    public BookingValidationResult CreateBooking(
        Booking candidate,
        UserRole role)
    {
        ArgumentNullException.ThrowIfNull(candidate);

        DateTime now = _clock.Now;

        Device? device =
            _deviceRepository.GetById(candidate.DeviceId);

        if (device is null)
        {
            return Failure(
                BookingErrorCode.DeviceUnavailable,
                "指定设备不存在。");
        }

        if (candidate.StartTime <= now)
        {
            return Failure(
                BookingErrorCode.InvalidTimeRange,
                "预约开始时间必须晚于当前时间。");
        }

        IReadOnlyList<Booking> existingBookings =
            _bookingRepository.GetActiveBookings(
                candidate.DeviceId);

        IReadOnlyList<MaintenanceOrder> maintenanceOrders =
            _maintenanceRepository.GetActiveOrders(
                candidate.DeviceId);

        BookingValidationResult validationResult =
            _ruleService.Validate(
                device,
                candidate,
                existingBookings,
                maintenanceOrders,
                role);

        if (!validationResult.IsValid)
        {
            return validationResult;
        }

        // 这些字段由系统统一生成，不信任调用方传入的值。
        candidate.BookingId = 0;
        candidate.Status = BookingStatus.Pending;
        candidate.CreatedAt = now;

        bool created =
            _bookingRepository.TryAddIfNoConflict(candidate);

        if (!created)
        {
            candidate.BookingId = 0;

            return Failure(
                BookingErrorCode.TimeConflict,
                "其他用户刚刚预约了该设备的重叠时段。");
        }

        return validationResult;
    }

    public bool CancelBooking(Booking booking)
    {
        ArgumentNullException.ThrowIfNull(booking);

        if (booking.BookingId <= 0)
        {
            return false;
        }

        if (booking.Status != BookingStatus.Pending &&
            booking.Status != BookingStatus.Confirmed)
        {
            throw new InvalidOperationException(
                "只有待确认或已确认的预约才能取消。");
        }

        booking.Status = BookingStatus.Cancelled;
        _bookingRepository.Update(booking);

        return true;
    }

    /// <summary>
    /// 确认待处理的预约。
    /// </summary>
    public void ConfirmBooking(Booking booking)
    {
        ArgumentNullException.ThrowIfNull(booking);

        if (booking.Status != BookingStatus.Pending)
        {
            throw new InvalidOperationException(
                "只有待确认的预约才能执行确认操作。");
        }

        if (booking.EndTime <= _clock.Now)
        {
            throw new InvalidOperationException(
                "预约已经过期，不能再进行确认。");
        }

        booking.Status = BookingStatus.Confirmed;

        _bookingRepository.Update(booking);
    }

    /// <summary>
    /// 将已经结束的预约标记为完成。
    /// </summary>
    public void CompleteBooking(Booking booking)
    {
        ArgumentNullException.ThrowIfNull(booking);

        if (booking.Status != BookingStatus.Confirmed)
        {
            throw new InvalidOperationException(
                "只有已确认的预约才能标记为完成。");
        }

        if (_clock.Now < booking.EndTime)
        {
            throw new InvalidOperationException(
                "预约尚未到达结束时间，不能提前完成。");
        }

        booking.Status = BookingStatus.Completed;

        _bookingRepository.Update(booking);
    }

    private static BookingValidationResult Failure(
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
