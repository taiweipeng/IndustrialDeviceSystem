using IndustrialDeviceSystem.Core.Enums;
using IndustrialDeviceSystem.Core.Models;
using IndustrialDeviceSystem.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Xunit;

namespace IndustrialDeviceSystem.IntegrationTests;

public sealed class RepositoryIntegrationTests
{
    [Fact]
    public void BookingRepository_ShouldReadSeedBooking()
    {
        using var database = new TemporaryDatabase(includeSeed: true);
        var repository = new BookingRepository(
            database.ConnectionFactory);

        IReadOnlyList<Booking> bookings =
            repository.GetActiveBookings(1);

        Assert.NotEmpty(bookings);
        Assert.All(
            bookings,
            booking => Assert.True(
                booking.Status is BookingStatus.Pending
                    or BookingStatus.Confirmed));
    }

    [Fact]
    public void BookingRepository_Add_ShouldAssignIdAndPersist()
    {
        using var database = new TemporaryDatabase(includeSeed: true);
        var repository = new BookingRepository(
            database.ConnectionFactory);

        Booking booking = CreateBooking();

        repository.Add(booking);

        Assert.True(booking.BookingId > 0);
        Assert.Contains(
            repository.GetActiveBookings(1),
            item => item.BookingId == booking.BookingId);
    }

    [Fact]
    public void BookingRepository_CancelledBooking_ShouldNotBeActive()
    {
        using var database = new TemporaryDatabase(includeSeed: true);
        var repository = new BookingRepository(
            database.ConnectionFactory);

        Booking booking =
            repository.GetActiveBookings(1).First();

        booking.Status = BookingStatus.Cancelled;
        repository.Update(booking);

        Assert.DoesNotContain(
            repository.GetActiveBookings(1),
            item => item.BookingId == booking.BookingId);
    }

    [Fact]
    public void BookingRepository_UnknownDevice_ShouldFail()
    {
        using var database = new TemporaryDatabase(includeSeed: true);
        var repository = new BookingRepository(
            database.ConnectionFactory);

        Booking booking = CreateBooking();
        booking.DeviceId = 999;

        Assert.Throws<SqliteException>(
            () => repository.Add(booking));
    }

    [Fact]
    public void BookingRepository_InvalidTimeRange_ShouldFail()
    {
        using var database = new TemporaryDatabase(includeSeed: true);
        var repository = new BookingRepository(
            database.ConnectionFactory);

        Booking booking = CreateBooking();
        booking.EndTime = booking.StartTime;

        Assert.Throws<SqliteException>(
            () => repository.Add(booking));
    }

    [Fact]
    public void AlarmRepository_ShouldReadSeedAlarm()
    {
        using var database = new TemporaryDatabase(includeSeed: true);
        var repository = new AlarmRepository(
            database.ConnectionFactory);

        Alarm? alarm =
            repository.GetActiveAlarm(3, "Temperature");

        Assert.NotNull(alarm);
        Assert.Equal(3, alarm!.DeviceId);
    }

    [Fact]
    public void AlarmRepository_DuplicateActiveAlarm_ShouldFail()
    {
        using var database = new TemporaryDatabase(includeSeed: true);
        var repository = new AlarmRepository(
            database.ConnectionFactory);

        Alarm alarm = CreateAlarm();

        Assert.Throws<SqliteException>(
            () => repository.Add(alarm));
    }

    [Fact]
    public void AlarmRepository_ClearedAlarm_ShouldAllowNewAlarm()
    {
        using var database = new TemporaryDatabase(includeSeed: true);
        var repository = new AlarmRepository(
            database.ConnectionFactory);

        Alarm existing =
            repository.GetActiveAlarm(3, "Temperature")!;

        int oldAlarmId = existing.AlarmId;
        existing.Status = AlarmStatus.Cleared;
        existing.ClearedAt = Utc(3, 10);

        repository.Update(existing);

        Alarm newAlarm = CreateAlarm();
        repository.Add(newAlarm);

        Alarm? saved =
            repository.GetActiveAlarm(3, "Temperature");

        Assert.NotNull(saved);
        Assert.NotEqual(oldAlarmId, saved!.AlarmId);
    }

    [Fact]
    public void AlarmRepository_Update_ShouldPersistOccurrenceCount()
    {
        using var database = new TemporaryDatabase(includeSeed: true);
        var repository = new AlarmRepository(
            database.ConnectionFactory);

        Alarm alarm =
            repository.GetActiveAlarm(3, "Temperature")!;

        alarm.OccurrenceCount++;
        alarm.LastOccurredAt = Utc(3, 11);

        repository.Update(alarm);

        Alarm? saved =
            repository.GetActiveAlarm(3, "Temperature");

        Assert.NotNull(saved);
        Assert.Equal(alarm.OccurrenceCount, saved!.OccurrenceCount);
        Assert.Equal(alarm.LastOccurredAt, saved.LastOccurredAt);
    }

    [Fact]
    public void MaintenanceRepository_ShouldReadSeedOrder()
    {
        using var database = new TemporaryDatabase(includeSeed: true);
        var repository = new MaintenanceRepository(
            database.ConnectionFactory);

        IReadOnlyList<MaintenanceOrder> orders =
            repository.GetActiveOrders(2);

        Assert.NotEmpty(orders);
    }

    [Fact]
    public void MaintenanceRepository_Add_ShouldAssignIdAndPersist()
    {
        using var database = new TemporaryDatabase(includeSeed: true);
        var repository = new MaintenanceRepository(
            database.ConnectionFactory);

        MaintenanceOrder order = CreateOrder();

        repository.Add(order);

        Assert.True(order.OrderId > 0);
        Assert.Contains(
            repository.GetActiveOrders(1),
            item => item.OrderId == order.OrderId);
    }

    [Fact]
    public void MaintenanceRepository_ClosedOrder_ShouldNotBeActive()
    {
        using var database = new TemporaryDatabase(includeSeed: true);
        var repository = new MaintenanceRepository(
            database.ConnectionFactory);

        MaintenanceOrder order =
            repository.GetActiveOrders(2).First();

        order.Status = MaintenanceStatus.Closed;
        order.Result = "维护完成";
        order.CompletedAt = Utc(3, 15);

        repository.Update(order);

        Assert.DoesNotContain(
            repository.GetActiveOrders(2),
            item => item.OrderId == order.OrderId);
    }

    [Fact]
    public void MaintenanceRepository_UnknownAssignee_ShouldFail()
    {
        using var database = new TemporaryDatabase(includeSeed: true);
        var repository = new MaintenanceRepository(
            database.ConnectionFactory);

        MaintenanceOrder order = CreateOrder();
        order.AssigneeId = 999;

        Assert.Throws<SqliteException>(
            () => repository.Add(order));
    }

    private static Booking CreateBooking()
    {
        return new Booking
        {
            DeviceId = 1,
            UserId = 2,
            StartTime = Utc(10, 9),
            EndTime = Utc(10, 11),
            Purpose = "仓储集成测试",
            Status = BookingStatus.Pending,
            CreatedAt = Utc(3, 8)
        };
    }

    private static Alarm CreateAlarm()
    {
        return new Alarm
        {
            DeviceId = 3,
            AlarmType = "Temperature",
            Level = AlarmLevel.High,
            Message = "温度超过高限",
            Status = AlarmStatus.Active,
            OccurredAt = Utc(3, 12),
            LastOccurredAt = Utc(3, 12),
            OccurrenceCount = 1
        };
    }

    private static MaintenanceOrder CreateOrder()
    {
        return new MaintenanceOrder
        {
            DeviceId = 1,
            AssigneeId = 3,
            Description = "检查设备通信线路",
            Result = string.Empty,
            Status = MaintenanceStatus.Assigned,
            CreatedAt = Utc(3, 8),
            PlannedStartTime = Utc(11, 8),
            PlannedEndTime = Utc(11, 10)
        };
    }

    private static DateTime Utc(int day, int hour)
    {
        return new DateTime(
            2026,
            11,
            day,
            hour,
            0,
            0,
            DateTimeKind.Utc);
    }
}
