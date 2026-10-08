using IndustrialDeviceSystem.Core.Enums;
using IndustrialDeviceSystem.Core.Models;
using IndustrialDeviceSystem.Infrastructure.Repositories;
using Xunit;

namespace IndustrialDeviceSystem.IntegrationTests;

public sealed class ConcurrentBookingTests
{
    [Fact]
    public async Task ConcurrentOverlappingBookings_OnlyOneShouldSucceed()
    {
        using var database =
            new TemporaryDatabase(includeSeed: true);

        var firstRepository =
            new BookingRepository(database.ConnectionFactory);

        var secondRepository =
            new BookingRepository(database.ConnectionFactory);

        DateTime startTime = new(
            2030,
            1,
            15,
            9,
            0,
            0,
            DateTimeKind.Utc);

        DateTime endTime =
            startTime.AddHours(2);

        Booking firstBooking = CreateBooking(
            userId: 1,
            startTime,
            endTime,
            "并发预约A");

        Booking secondBooking = CreateBooking(
            userId: 2,
            startTime,
            endTime,
            "并发预约B");

        using var startGate =
            new ManualResetEventSlim(initialState: false);

        Task<bool> firstTask = Task.Run(() =>
        {
            startGate.Wait();

            return firstRepository.TryAddIfNoConflict(
                firstBooking);
        });

        Task<bool> secondTask = Task.Run(() =>
        {
            startGate.Wait();

            return secondRepository.TryAddIfNoConflict(
                secondBooking);
        });

        startGate.Set();

        bool[] results = await Task.WhenAll(
            firstTask,
            secondTask);

        Assert.Equal(
            1,
            results.Count(result => result));

        Assert.Equal(
            1,
            results.Count(result => !result));

        var verificationRepository =
            new BookingRepository(database.ConnectionFactory);

        IReadOnlyList<Booking> savedBookings =
            verificationRepository.GetActiveBookings(1);

        List<Booking> matchingBookings = savedBookings
            .Where(item =>
                item.StartTime == startTime &&
                item.EndTime == endTime)
            .ToList();

        Assert.Single(matchingBookings);

        Assert.True(
            firstBooking.BookingId > 0 ^
            secondBooking.BookingId > 0);
    }

    private static Booking CreateBooking(
        int userId,
        DateTime startTime,
        DateTime endTime,
        string purpose)
    {
        return new Booking
        {
            DeviceId = 1,
            UserId = userId,
            StartTime = startTime,
            EndTime = endTime,
            Purpose = purpose,
            Status = BookingStatus.Pending,
            CreatedAt = new DateTime(
                2026,
                10,
                4,
                8,
                0,
                0,
                DateTimeKind.Utc)
        };
    }
}
