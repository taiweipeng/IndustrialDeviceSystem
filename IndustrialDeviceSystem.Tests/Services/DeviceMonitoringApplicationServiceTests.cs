using IndustrialDeviceSystem.Core.Enums;
using IndustrialDeviceSystem.Core.Interfaces;
using IndustrialDeviceSystem.Core.Models;
using IndustrialDeviceSystem.Core.Services;
using Xunit;

namespace IndustrialDeviceSystem.Tests.Services;

public sealed class DeviceMonitoringApplicationServiceTests
{
    private static readonly DateTime CurrentTime =
        new(2026, 10, 4, 13, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void CollectOnce_GoodRunningData_ShouldPersistAndAlarm()
    {
        TestContext context = CreateContext(
            DeviceStatus.Idle,
            CreateSnapshot(
                temperature: 85.0,
                isRunning: true));

        DeviceMonitoringResult result =
            context.Service.CollectOnce(
                "DEV-001",
                CreateThresholds());

        Assert.True(result.Telemetry.TelemetryId > 0);
        Assert.Single(context.Telemetry.Items);

        Assert.Equal(
            DeviceStatus.Running,
            result.DeviceStatus);

        Assert.Equal(
            CurrentTime,
            context.Devices.Device!.LastCommunicationTime);

        Assert.NotNull(result.Alarm);
        Assert.Equal(
            AlarmLevel.High,
            result.Alarm!.Level);

        Assert.Equal(0, result.ConsecutiveCommunicationFailures);
    }

    [Fact]
    public void CollectOnce_CommunicationLostBelowLimit_ShouldKeepStatus()
    {
        TestContext context = CreateContext(
            DeviceStatus.Running,
            CreateSnapshot(
                quality: DataQuality.CommunicationLost));

        DeviceMonitoringResult first =
            context.Service.CollectOnce(
                "DEV-001",
                CreateThresholds(),
                maximumCommunicationFailures: 3);

        DeviceMonitoringResult second =
            context.Service.CollectOnce(
                "DEV-001",
                CreateThresholds(),
                maximumCommunicationFailures: 3);

        Assert.Equal(DeviceStatus.Running, first.DeviceStatus);
        Assert.Equal(DeviceStatus.Running, second.DeviceStatus);
        Assert.Equal(1, first.ConsecutiveCommunicationFailures);
        Assert.Equal(2, second.ConsecutiveCommunicationFailures);

        Assert.Equal(2, context.Telemetry.Items.Count);
        Assert.Equal(0, context.Alarms.AddCallCount);
        Assert.Null(
            context.Devices.Device!.LastCommunicationTime);
    }

    [Fact]
    public void CollectOnce_CommunicationLostAtLimit_ShouldSetOffline()
    {
        TestContext context = CreateContext(
            DeviceStatus.Running,
            CreateSnapshot(
                quality: DataQuality.CommunicationLost));

        context.Service.CollectOnce(
            "DEV-001",
            CreateThresholds(),
            maximumCommunicationFailures: 3);

        context.Service.CollectOnce(
            "DEV-001",
            CreateThresholds(),
            maximumCommunicationFailures: 3);

        DeviceMonitoringResult third =
            context.Service.CollectOnce(
                "DEV-001",
                CreateThresholds(),
                maximumCommunicationFailures: 3);

        Assert.Equal(
            DeviceStatus.Offline,
            third.DeviceStatus);

        Assert.Equal(
            3,
            third.ConsecutiveCommunicationFailures);

        Assert.Equal(
            DeviceStatus.Offline,
            context.Devices.Device!.Status);
    }

    [Fact]
    public void CollectOnce_GoodDataAfterOffline_ShouldRecoverThroughIdle()
    {
        TestContext context = CreateContext(
            DeviceStatus.Offline,
            CreateSnapshot(
                temperature: 35.0,
                isRunning: true));

        DeviceMonitoringResult first =
            context.Service.CollectOnce(
                "DEV-001",
                CreateThresholds());

        Assert.Equal(
            DeviceStatus.Idle,
            first.DeviceStatus);

        DeviceMonitoringResult second =
            context.Service.CollectOnce(
                "DEV-001",
                CreateThresholds());

        Assert.Equal(
            DeviceStatus.Running,
            second.DeviceStatus);
    }

    [Fact]
    public void CollectOnce_FaultStatus_ShouldRemainLatched()
    {
        TestContext context = CreateContext(
            DeviceStatus.Idle,
            CreateSnapshot(
                temperature: 35.0,
                isRunning: false,
                faultSignal: true));

        DeviceMonitoringResult faultResult =
            context.Service.CollectOnce(
                "DEV-001",
                CreateThresholds());

        Assert.Equal(
            DeviceStatus.Fault,
            faultResult.DeviceStatus);

        context.DataSource.Snapshot =
            CreateSnapshot(
                temperature: 35.0,
                isRunning: false,
                faultSignal: false);

        DeviceMonitoringResult normalResult =
            context.Service.CollectOnce(
                "DEV-001",
                CreateThresholds());

        Assert.Equal(
            DeviceStatus.Fault,
            normalResult.DeviceStatus);
    }

    [Fact]
    public void CollectOnce_UncertainHighTemperature_ShouldNotAlarm()
    {
        TestContext context = CreateContext(
            DeviceStatus.Idle,
            CreateSnapshot(
                temperature: 95.0,
                isRunning: true,
                quality: DataQuality.Uncertain));

        DeviceMonitoringResult result =
            context.Service.CollectOnce(
                "DEV-001",
                CreateThresholds());

        Assert.Single(context.Telemetry.Items);
        Assert.Null(result.Alarm);
        Assert.Equal(0, context.Alarms.AddCallCount);

        // Uncertain数据不能把设备自动改成Running。
        Assert.Equal(
            DeviceStatus.Idle,
            result.DeviceStatus);

        Assert.Equal(
            CurrentTime,
            context.Devices.Device!.LastCommunicationTime);
    }

    private static TestContext CreateContext(
        DeviceStatus initialStatus,
        DeviceSnapshot snapshot)
    {
        var dataSource = new FakeDeviceDataSource
        {
            Snapshot = snapshot
        };

        var deviceRepository =
            new FakeDeviceRepository(new Device
            {
                DeviceId = 1,
                DeviceCode = "DEV-001",
                DeviceName = "教学设备1",
                DeviceType = "Simulator",
                Status = initialStatus,
                Enabled = true
            });

        var telemetryRepository =
            new FakeTelemetryRepository();

        var alarmRepository =
            new FakeAlarmRepository();

        var alarmService =
            new AlarmApplicationService(
                alarmRepository,
                new AlarmRuleService(),
                new FixedClock(CurrentTime));

        var service =
            new DeviceMonitoringApplicationService(
                dataSource,
                deviceRepository,
                telemetryRepository,
                alarmService,
                new DeviceStateService());

        return new TestContext(
            service,
            dataSource,
            deviceRepository,
            telemetryRepository,
            alarmRepository);
    }

    private static DeviceSnapshot CreateSnapshot(
        double temperature = 35.0,
        bool isRunning = false,
        bool faultSignal = false,
        bool emergencyStop = false,
        DataQuality quality = DataQuality.Good)
    {
        return new DeviceSnapshot
        {
            DeviceCode = "DEV-001",
            CollectedAt = CurrentTime,
            Temperature = temperature,
            Pressure = 1.2,
            Vibration = 0.25,
            IsRunning = isRunning,
            FaultSignal = faultSignal,
            EmergencyStop = emergencyStop,
            Quality = quality
        };
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
        DeviceMonitoringApplicationService Service,
        FakeDeviceDataSource DataSource,
        FakeDeviceRepository Devices,
        FakeTelemetryRepository Telemetry,
        FakeAlarmRepository Alarms);

    private sealed class FixedClock : IClock
    {
        public FixedClock(DateTime now)
        {
            Now = now;
        }

        public DateTime Now { get; }
    }

    private sealed class FakeDeviceDataSource
        : IDeviceDataSource
    {
        public DeviceSnapshot Snapshot { get; set; } = new();

        public DeviceSnapshot ReadSnapshot(string deviceCode)
        {
            return Snapshot;
        }
    }

    private sealed class FakeDeviceRepository
        : IDeviceRepository
    {
        public FakeDeviceRepository(Device device)
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
            return Device is null
                ? Array.Empty<Device>()
                : new[] { Device };
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

    private sealed class FakeTelemetryRepository
        : ITelemetryRepository
    {
        public List<TelemetryRecord> Items { get; } = new();

        public void Add(TelemetryRecord telemetry)
        {
            telemetry.TelemetryId = Items.Count + 1;
            Items.Add(telemetry);
        }

        public IReadOnlyList<TelemetryRecord> GetRecent(
            int deviceId,
            int count)
        {
            return Items
                .Where(item => item.DeviceId == deviceId)
                .OrderByDescending(item => item.CollectedAt)
                .ThenByDescending(item => item.TelemetryId)
                .Take(count)
                .ToList();
        }

        public IReadOnlyList<TelemetryRecord> GetByTimeRange(
    int deviceId,
    DateTime startTime,
    DateTime endTime,
    int maximumCount)
        {
            if (deviceId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(deviceId));
            }

            if (startTime >= endTime)
            {
                throw new ArgumentException(
                    "开始时间必须早于结束时间。",
                    nameof(startTime));
            }

            if (maximumCount <= 0 ||
                maximumCount > 5000)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumCount));
            }

            return Items
                .Where(item =>
                    item.DeviceId == deviceId &&
                    item.CollectedAt >= startTime &&
                    item.CollectedAt <= endTime)
                .OrderByDescending(item =>
                    item.CollectedAt)
                .ThenByDescending(item =>
                    item.TelemetryId)
                .Take(maximumCount)
                .OrderBy(item =>
                    item.CollectedAt)
                .ThenBy(item =>
                    item.TelemetryId)
                .ToList();
        }
    }

    private sealed class FakeAlarmRepository
        : IAlarmRepository
    {
        private Alarm? _alarm;

        public int AddCallCount { get; private set; }

        public Alarm? GetActiveAlarm(
            int deviceId,
            string alarmType)
        {
            if (_alarm is null)
            {
                return null;
            }

            bool active =
                _alarm.Status is AlarmStatus.Active
                    or AlarmStatus.Acknowledged;

            return active &&
                   _alarm.DeviceId == deviceId &&
                   _alarm.AlarmType == alarmType
                ? _alarm
                : null;
        }

        public void Add(Alarm alarm)
        {
            AddCallCount++;
            alarm.AlarmId = AddCallCount;
            _alarm = alarm;
        }

        public void Update(Alarm alarm)
        {
            _alarm = alarm;
        }
    }
}
