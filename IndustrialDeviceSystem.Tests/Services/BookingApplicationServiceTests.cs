using IndustrialDeviceSystem.Core.Enums;
using IndustrialDeviceSystem.Core.Interfaces;
using IndustrialDeviceSystem.Core.Models;
using IndustrialDeviceSystem.Core.Services;
using Xunit;

namespace IndustrialDeviceSystem.Tests.Services;

public sealed class BookingApplicationServiceTests
{
    private static readonly DateTime CurrentTime =
        new(2026, 10, 3, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void CreateBooking_ValidRequest_ShouldPersistPendingBooking()
    {
        var context = CreateContext();
        Booking candidate = CreateCandidate();

        BookingValidationResult result =
            context.Service.CreateBooking(
                candidate,
                UserRole.Operator);

        Assert.True(result.IsValid);
        Assert.Equal(BookingErrorCode.None, result.ErrorCode);

        Booking saved = Assert.Single(context.Bookings.Items);

        Assert.Same(candidate, saved);
        Assert.True(saved.BookingId > 0);
        Assert.Equal(BookingStatus.Pending, saved.Status);
        Assert.Equal(CurrentTime, saved.CreatedAt);
    }

    [Fact]
    public void CreateBooking_DeviceNotFound_ShouldNotPersist()
    {
        var context = CreateContext();
        context.Devices.Device = null;

        BookingValidationResult result =
            context.Service.CreateBooking(
                CreateCandidate(),
                UserRole.Operator);

        Assert.False(result.IsValid);
        Assert.Equal(
            BookingErrorCode.DeviceUnavailable,
            result.ErrorCode);
        Assert.Empty(context.Bookings.Items);
    }

    [Fact]
    public void CreateBooking_StartTimeNotInFuture_ShouldNotPersist()
    {
        var context = CreateContext();
        Booking candidate = CreateCandidate();

        candidate.StartTime = CurrentTime;
        candidate.EndTime = CurrentTime.AddHours(1);

        BookingValidationResult result =
            context.Service.CreateBooking(
                candidate,
                UserRole.Operator);

        Assert.False(result.IsValid);
        Assert.Equal(
            BookingErrorCode.InvalidTimeRange,
            result.ErrorCode);
        Assert.Empty(context.Bookings.Items);
    }

    [Fact]
    public void CreateBooking_ConflictsWithBooking_ShouldNotPersist()
    {
        var context = CreateContext();

        context.Bookings.Items.Add(new Booking
        {
            BookingId = 1,
            DeviceId = 1,
            UserId = 3,
            StartTime = CurrentTime.AddHours(3),
            EndTime = CurrentTime.AddHours(5),
            Purpose = "已有预约",
            Status = BookingStatus.Confirmed,
            CreatedAt = CurrentTime.AddDays(-1)
        });

        BookingValidationResult result =
            context.Service.CreateBooking(
                CreateCandidate(),
                UserRole.Operator);

        Assert.False(result.IsValid);
        Assert.Equal(
            BookingErrorCode.TimeConflict,
            result.ErrorCode);

        // 原有一条，没有新增第二条。
        Assert.Single(context.Bookings.Items);
    }

    [Fact]
    public void CreateBooking_ConflictsWithMaintenance_ShouldNotPersist()
    {
        var context = CreateContext();

        context.Maintenance.Orders.Add(new MaintenanceOrder
        {
            OrderId = 1,
            DeviceId = 1,
            Description = "计划检修",
            Status = MaintenanceStatus.Assigned,
            CreatedAt = CurrentTime,
            PlannedStartTime = CurrentTime.AddHours(3),
            PlannedEndTime = CurrentTime.AddHours(5)
        });

        BookingValidationResult result =
            context.Service.CreateBooking(
                CreateCandidate(),
                UserRole.Operator);

        Assert.False(result.IsValid);
        Assert.Equal(
            BookingErrorCode.MaintenanceConflict,
            result.ErrorCode);
        Assert.Empty(context.Bookings.Items);
    }

    [Fact]
    public void CreateBooking_MaintainerRole_ShouldNotPersist()
    {
        var context = CreateContext();

        BookingValidationResult result =
            context.Service.CreateBooking(
                CreateCandidate(),
                UserRole.Maintainer);

        Assert.False(result.IsValid);
        Assert.Equal(
            BookingErrorCode.UserNotAllowed,
            result.ErrorCode);
        Assert.Empty(context.Bookings.Items);
    }

    [Fact]
    public void CreateBooking_ConcurrentConflict_ShouldReturnTimeConflict()
    {
        var context = CreateContext();

        context.Bookings.SimulateConcurrentConflict = true;

        Booking candidate = CreateCandidate();

        BookingValidationResult result =
            context.Service.CreateBooking(
                candidate,
                UserRole.Operator);

        Assert.False(result.IsValid);
        Assert.Equal(
            BookingErrorCode.TimeConflict,
            result.ErrorCode);

        Assert.Equal(0, candidate.BookingId);
        Assert.Empty(context.Bookings.Items);
    }

    private static TestContext CreateContext()
    {
        var deviceRepository =
            new FakeDeviceRepository(CreateIdleDevice());

        var bookingRepository =
            new FakeBookingRepository();

        var maintenanceRepository =
            new FakeMaintenanceRepository();

        var service = new BookingApplicationService(
            deviceRepository,
            bookingRepository,
            maintenanceRepository,
            new BookingRuleService(),
            new FixedClock(CurrentTime));

        return new TestContext(
            service,
            deviceRepository,
            bookingRepository,
            maintenanceRepository);
    }

    private static Device CreateIdleDevice()
    {
        return new Device
        {
            DeviceId = 1,
            DeviceCode = "DEV-001",
            DeviceName = "实验设备1",
            DeviceType = "Teaching",
            Status = DeviceStatus.Idle,
            Enabled = true
        };
    }

    private static Booking CreateCandidate()
    {
        return new Booking
        {
            DeviceId = 1,
            UserId = 2,
            StartTime = CurrentTime.AddHours(2),
            EndTime = CurrentTime.AddHours(4),
            Purpose = "设备性能实验",

            // 故意设置错误值，用于验证应用服务会重新赋值。
            BookingId = 999,
            Status = BookingStatus.Confirmed,
            CreatedAt = CurrentTime.AddYears(-1)
        };
    }

    private sealed record TestContext(
        BookingApplicationService Service,
        FakeDeviceRepository Devices,
        FakeBookingRepository Bookings,
        FakeMaintenanceRepository Maintenance);

    private sealed class FixedClock : IClock
    {
        public FixedClock(DateTime now)
        {
            Now = now;
        }

        public DateTime Now { get; }
    }

    private sealed class FakeDeviceRepository : IDeviceRepository
    {
        public FakeDeviceRepository(Device? device)
        {
            Device = device;
        }

        public Device? Device { get; set; }

        public Device? GetById(int deviceId)
        {
            return Device?.DeviceId == deviceId
                ? Device
                : null;
        }

        public Device? GetByCode(string deviceCode)
        {
            return Device?.DeviceCode == deviceCode
                ? Device
                : null;
        }

        public IReadOnlyList<Device> GetAll()
        {
            if (Device is null)
            {
                return Array.Empty<Device>();
            }

            return new[] { Device };
        }

        public void Add(Device device)
        {
            Device = device;
        }

        public void Update(Device device)
        {
            Device = device;
        }
    }

    private sealed class FakeBookingRepository
        : IBookingRepository
    {
        public List<Booking> Items { get; } = new();

        public IReadOnlyList<Booking> GetActiveBookings(
            int deviceId)
        {
            return Items
                .Where(item =>
                    item.DeviceId == deviceId &&
                    item.Status is BookingStatus.Pending
                        or BookingStatus.Confirmed)
                .ToList();
        }

        public void Add(Booking booking)
        {
            booking.BookingId = Items.Count + 100;
            Items.Add(booking);
        }

        public void Update(Booking booking)
        {
            int index = Items.FindIndex(
                item => item.BookingId == booking.BookingId);

            if (index < 0)
            {
                throw new InvalidOperationException();
            }

            Items[index] = booking;
        }

        public bool SimulateConcurrentConflict { get; set; }

        public bool TryAddIfNoConflict(Booking booking)
        {
            if (SimulateConcurrentConflict)
            {
                return false;
            }

            bool hasConflict = Items.Any(item =>
                item.DeviceId == booking.DeviceId &&
                (item.Status is BookingStatus.Pending
                    or BookingStatus.Confirmed) &&
                booking.StartTime < item.EndTime &&
                item.StartTime < booking.EndTime);

            if (hasConflict)
            {
                return false;
            }

            Add(booking);
            return true;
        }
    }



    private sealed class FakeMaintenanceRepository
        : IMaintenanceRepository
    {
        public List<MaintenanceOrder> Orders { get; } = new();

        public IReadOnlyList<MaintenanceOrder> GetActiveOrders(
            int deviceId)
        {
            return Orders
                .Where(item =>
                    item.DeviceId == deviceId &&
                    item.Status is MaintenanceStatus.Created
                        or MaintenanceStatus.Assigned
                        or MaintenanceStatus.InProgress
                        or MaintenanceStatus.Completed)
                .ToList();
        }

        public void Add(MaintenanceOrder order)
        {
            Orders.Add(order);
        }

        public void Update(MaintenanceOrder order)
        {
            int index = Orders.FindIndex(
                item => item.OrderId == order.OrderId);

            if (index < 0)
            {
                throw new InvalidOperationException();
            }

            Orders[index] = order;
        }
    }
}
