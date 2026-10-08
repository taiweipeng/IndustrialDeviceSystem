using IndustrialDeviceSystem.Core.Enums;
using IndustrialDeviceSystem.Core.Interfaces;
using IndustrialDeviceSystem.Core.Models;
using IndustrialDeviceSystem.Core.Services;
using Xunit;

namespace IndustrialDeviceSystem.Tests.Services;

public sealed class AlarmApplicationServiceTests
{
    private static readonly DateTime CurrentTime =
        new(2026, 10, 4, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void ProcessTemperature_NormalWithoutAlarm_ShouldDoNothing()
    {
        TestContext context = CreateContext();

        Alarm? result = context.Service.ProcessTemperature(
            deviceId: 1,
            temperature: 59.9,
            CreateThresholds());

        Assert.Null(result);
        Assert.Equal(0, context.Repository.AddCallCount);
        Assert.Equal(0, context.Repository.UpdateCallCount);
    }

    [Fact]
    public void ProcessTemperature_FirstWarning_ShouldCreateAlarm()
    {
        TestContext context = CreateContext();

        Alarm? result = context.Service.ProcessTemperature(
            deviceId: 1,
            temperature: 60.0,
            CreateThresholds());

        Assert.NotNull(result);

        Assert.True(result!.AlarmId > 0);
        Assert.Equal(1, result.DeviceId);
        Assert.Equal("Temperature", result.AlarmType);
        Assert.Equal(AlarmLevel.Warning, result.Level);
        Assert.Equal(AlarmStatus.Active, result.Status);
        Assert.Equal(CurrentTime, result.OccurredAt);
        Assert.Equal(CurrentTime, result.LastOccurredAt);
        Assert.Equal(1, result.OccurrenceCount);
        Assert.Null(result.AcknowledgedBy);
        Assert.Null(result.AcknowledgedAt);
        Assert.Null(result.ClearedAt);

        Assert.Equal(1, context.Repository.AddCallCount);
        Assert.Equal(0, context.Repository.UpdateCallCount);
    }

    [Fact]
    public void ProcessTemperature_RepeatedAlarm_ShouldUpdateExistingAlarm()
    {
        TestContext context = CreateContext();

        DateTime occurredAt =
            CurrentTime.AddMinutes(-10);

        DateTime acknowledgedAt =
            CurrentTime.AddMinutes(-8);

        context.Repository.Seed(new Alarm
        {
            AlarmId = 20,
            DeviceId = 1,
            AlarmType = "Temperature",
            Level = AlarmLevel.Warning,
            Message = "原报警",
            Status = AlarmStatus.Acknowledged,
            OccurredAt = occurredAt,
            LastOccurredAt = CurrentTime.AddMinutes(-5),
            OccurrenceCount = 2,
            AcknowledgedBy = 3,
            AcknowledgedAt = acknowledgedAt
        });

        Alarm? result = context.Service.ProcessTemperature(
            deviceId: 1,
            temperature: 85.0,
            CreateThresholds());

        Assert.NotNull(result);

        Assert.Equal(20, result!.AlarmId);
        Assert.Equal(AlarmLevel.High, result.Level);
        Assert.Equal(AlarmStatus.Acknowledged, result.Status);
        Assert.Equal(occurredAt, result.OccurredAt);
        Assert.Equal(CurrentTime, result.LastOccurredAt);
        Assert.Equal(3, result.OccurrenceCount);
        Assert.Equal(3, result.AcknowledgedBy);
        Assert.Equal(acknowledgedAt, result.AcknowledgedAt);

        Assert.Equal(0, context.Repository.AddCallCount);
        Assert.Equal(1, context.Repository.UpdateCallCount);
    }

    [Fact]
    public void ProcessTemperature_ReturnsToNormal_ShouldClearAlarm()
    {
        TestContext context = CreateContext();

        context.Repository.Seed(new Alarm
        {
            AlarmId = 30,
            DeviceId = 1,
            AlarmType = "Temperature",
            Level = AlarmLevel.High,
            Message = "温度高报警",
            Status = AlarmStatus.Active,
            OccurredAt = CurrentTime.AddMinutes(-10),
            LastOccurredAt = CurrentTime.AddMinutes(-2),
            OccurrenceCount = 3
        });

        Alarm? result = context.Service.ProcessTemperature(
            deviceId: 1,
            temperature: 50.0,
            CreateThresholds());

        Assert.NotNull(result);
        Assert.Equal(AlarmStatus.Cleared, result!.Status);
        Assert.Equal(CurrentTime, result.ClearedAt);

        Assert.Equal(0, context.Repository.AddCallCount);
        Assert.Equal(1, context.Repository.UpdateCallCount);

        Assert.Null(
            context.Repository.GetActiveAlarm(
                1,
                "Temperature"));
    }

    [Fact]
    public void ProcessTemperature_InvalidThresholds_ShouldNotWrite()
    {
        TestContext context = CreateContext();

        var invalidThresholds = new TemperatureThresholds
        {
            Warning = 80.0,
            High = 70.0,
            Emergency = 90.0
        };

        Assert.Throws<ArgumentException>(() =>
            context.Service.ProcessTemperature(
                deviceId: 1,
                temperature: 85.0,
                invalidThresholds));

        Assert.Equal(0, context.Repository.AddCallCount);
        Assert.Equal(0, context.Repository.UpdateCallCount);
    }

    private static TestContext CreateContext()
    {
        var repository = new FakeAlarmRepository();
        var clock = new MutableClock
        {
            Now = CurrentTime
        };

        var service = new AlarmApplicationService(
            repository,
            new AlarmRuleService(),
            clock);

        return new TestContext(
            service,
            repository,
            clock);
    }

    private static TemperatureThresholds CreateThresholds()
    {
        return new TemperatureThresholds
        {
            Warning = 60.0,
            High = 80.0,
            Emergency = 90.0
        };
    }

    private sealed record TestContext(
        AlarmApplicationService Service,
        FakeAlarmRepository Repository,
        MutableClock Clock);

    private sealed class MutableClock : IClock
    {
        public DateTime Now { get; set; }
    }

    private sealed class FakeAlarmRepository
        : IAlarmRepository
    {
        private Alarm? _storedAlarm;

        public int AddCallCount { get; private set; }

        public int UpdateCallCount { get; private set; }

        public void Seed(Alarm alarm)
        {
            _storedAlarm = alarm;
        }

        public Alarm? GetActiveAlarm(
            int deviceId,
            string alarmType)
        {
            if (_storedAlarm is null)
            {
                return null;
            }

            bool isActive =
                _storedAlarm.Status is AlarmStatus.Active
                    or AlarmStatus.Acknowledged;

            bool matches =
                _storedAlarm.DeviceId == deviceId &&
                _storedAlarm.AlarmType == alarmType;

            return isActive && matches
                ? _storedAlarm
                : null;
        }

        public void Add(Alarm alarm)
        {
            if (GetActiveAlarm(
                    alarm.DeviceId,
                    alarm.AlarmType) is not null)
            {
                throw new InvalidOperationException(
                    "已经存在未解除的同类报警。");
            }

            AddCallCount++;

            alarm.AlarmId = 100 + AddCallCount;
            _storedAlarm = alarm;
        }

        public void Update(Alarm alarm)
        {
            if (_storedAlarm is null ||
                _storedAlarm.AlarmId != alarm.AlarmId)
            {
                throw new InvalidOperationException(
                    "报警记录不存在。");
            }

            UpdateCallCount++;
            _storedAlarm = alarm;
        }
    }
}
