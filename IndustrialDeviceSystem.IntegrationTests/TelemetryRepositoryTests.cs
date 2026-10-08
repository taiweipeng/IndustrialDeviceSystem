using IndustrialDeviceSystem.Core.Enums;
using IndustrialDeviceSystem.Core.Models;
using IndustrialDeviceSystem.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Xunit;

namespace IndustrialDeviceSystem.IntegrationTests;

public sealed class TelemetryRepositoryTests
{
    [Fact]
    public void Add_ShouldAssignIdAndPersistAllFields()
    {
        using var database =
            new TemporaryDatabase(includeSeed: true);

        var repository =
            new TelemetryRepository(
                database.ConnectionFactory);

        TelemetryRecord telemetry = CreateTelemetry(
            collectedAt: Utc(1, 10),
            temperature: 42.5);

        telemetry.IsRunning = false;
        telemetry.FaultSignal = true;
        telemetry.EmergencyStop = false;
        telemetry.Quality = DataQuality.Uncertain;

        repository.Add(telemetry);

        Assert.True(telemetry.TelemetryId > 0);

        TelemetryRecord saved = repository
            .GetRecent(telemetry.DeviceId, 20)
            .Single(item =>
                item.TelemetryId ==
                telemetry.TelemetryId);

        Assert.Equal(telemetry.DeviceId, saved.DeviceId);
        Assert.Equal(telemetry.CollectedAt, saved.CollectedAt);
        Assert.Equal(42.5, saved.Temperature);
        Assert.Equal(1.2, saved.Pressure);
        Assert.Equal(0.25, saved.Vibration);
        Assert.False(saved.IsRunning);
        Assert.True(saved.FaultSignal);
        Assert.False(saved.EmergencyStop);
        Assert.Equal(DataQuality.Uncertain, saved.Quality);
    }

    [Fact]
    public void GetRecent_ShouldReturnNewestRecordsFirst()
    {
        using var database =
            new TemporaryDatabase(includeSeed: true);

        var repository =
            new TelemetryRepository(
                database.ConnectionFactory);

        TelemetryRecord first =
            CreateTelemetry(Utc(1, 8), 35.0);

        TelemetryRecord second =
            CreateTelemetry(Utc(1, 9), 36.0);

        TelemetryRecord third =
            CreateTelemetry(Utc(1, 10), 37.0);

        repository.Add(first);
        repository.Add(second);
        repository.Add(third);

        IReadOnlyList<TelemetryRecord> records =
            repository.GetRecent(
                deviceId: 1,
                count: 2);

        Assert.Equal(2, records.Count);
        Assert.Equal(third.TelemetryId, records[0].TelemetryId);
        Assert.Equal(second.TelemetryId, records[1].TelemetryId);
    }

    [Fact]
    public void Add_UnknownDevice_ShouldFailForeignKey()
    {
        using var database =
            new TemporaryDatabase(includeSeed: true);

        var repository =
            new TelemetryRepository(
                database.ConnectionFactory);

        TelemetryRecord telemetry =
            CreateTelemetry(Utc(1, 10), 35.0);

        telemetry.DeviceId = 999;

        Assert.Throws<SqliteException>(
            () => repository.Add(telemetry));

        Assert.Equal(0, telemetry.TelemetryId);
    }

    [Fact]
    public void GetRecent_NonPositiveCount_ShouldThrow()
    {
        using var database =
            new TemporaryDatabase(includeSeed: true);

        var repository =
            new TelemetryRepository(
                database.ConnectionFactory);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => repository.GetRecent(
                deviceId: 1,
                count: 0));
    }

    private static TelemetryRecord CreateTelemetry(
        DateTime collectedAt,
        double temperature)
    {
        return new TelemetryRecord
        {
            DeviceId = 1,
            CollectedAt = collectedAt,
            Temperature = temperature,
            Pressure = 1.2,
            Vibration = 0.25,
            IsRunning = true,
            FaultSignal = false,
            EmergencyStop = false,
            Quality = DataQuality.Good
        };
    }

    private static DateTime Utc(
        int day,
        int hour)
    {
        // 使用2099年，确保测试数据晚于seed.sql中的教学数据。
        return new DateTime(
            2099,
            1,
            day,
            hour,
            0,
            0,
            DateTimeKind.Utc);
    }
}
