using IndustrialDeviceSystem.Core.Enums;
using IndustrialDeviceSystem.Core.Interfaces;
using IndustrialDeviceSystem.Core.Models;

namespace IndustrialDeviceSystem.Core.Services;

public sealed class DeviceMonitoringApplicationService
{
    private readonly IDeviceDataSource _dataSource;
    private readonly IDeviceRepository _deviceRepository;
    private readonly ITelemetryRepository _telemetryRepository;
    private readonly AlarmApplicationService _alarmService;
    private readonly DeviceStateService _stateService;

    private readonly Dictionary<int, int>
        _communicationFailures = new();

    private readonly object _syncRoot = new();

    public DeviceMonitoringApplicationService(
        IDeviceDataSource dataSource,
        IDeviceRepository deviceRepository,
        ITelemetryRepository telemetryRepository,
        AlarmApplicationService alarmService,
        DeviceStateService stateService)
    {
        ArgumentNullException.ThrowIfNull(dataSource);
        ArgumentNullException.ThrowIfNull(deviceRepository);
        ArgumentNullException.ThrowIfNull(telemetryRepository);
        ArgumentNullException.ThrowIfNull(alarmService);
        ArgumentNullException.ThrowIfNull(stateService);

        _dataSource = dataSource;
        _deviceRepository = deviceRepository;
        _telemetryRepository = telemetryRepository;
        _alarmService = alarmService;
        _stateService = stateService;
    }

    public DeviceMonitoringResult CollectOnce(
        string deviceCode,
        TemperatureThresholds thresholds,
        int maximumCommunicationFailures = 3)
    {
        if (string.IsNullOrWhiteSpace(deviceCode))
        {
            throw new ArgumentException(
                "设备编码不能为空。",
                nameof(deviceCode));
        }

        ArgumentNullException.ThrowIfNull(thresholds);

        if (maximumCommunicationFailures < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumCommunicationFailures),
                "最大通信失败次数必须大于0。");
        }

        lock (_syncRoot)
        {
            return CollectOnceCore(
                deviceCode.Trim(),
                thresholds,
                maximumCommunicationFailures);
        }
    }

    private DeviceMonitoringResult CollectOnceCore(
        string deviceCode,
        TemperatureThresholds thresholds,
        int maximumCommunicationFailures)
    {
        Device? device =
            _deviceRepository.GetByCode(deviceCode);

        if (device is null)
        {
            throw new KeyNotFoundException(
                $"没有找到设备：{deviceCode}");
        }

        if (!device.Enabled)
        {
            throw new InvalidOperationException(
                $"设备已停用：{deviceCode}");
        }

        DeviceSnapshot snapshot =
            _dataSource.ReadSnapshot(deviceCode);

        if (!string.Equals(
                snapshot.DeviceCode,
                device.DeviceCode,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "数据源返回的设备编码与请求设备不一致。");
        }

        TelemetryRecord telemetry =
            CreateTelemetry(device.DeviceId, snapshot);

        // 无论质量是否正常，都保存采集事实。
        _telemetryRepository.Add(telemetry);

        if (snapshot.Quality ==
            DataQuality.CommunicationLost)
        {
            return ProcessCommunicationLost(
                device,
                snapshot,
                telemetry,
                maximumCommunicationFailures);
        }

        _communicationFailures.Remove(device.DeviceId);

        // Uncertain和Bad说明通信成功，但数据不能参与业务判断。
        device.LastCommunicationTime =
            snapshot.CollectedAt;

        Alarm? alarm = null;

        if (snapshot.Quality == DataQuality.Good)
        {
            UpdateDeviceStatusFromSnapshot(
                device,
                snapshot);

            alarm = _alarmService.ProcessTemperature(
                device.DeviceId,
                snapshot.Temperature,
                thresholds);
        }

        _deviceRepository.Update(device);

        return new DeviceMonitoringResult
        {
            Snapshot = snapshot,
            Telemetry = telemetry,
            Alarm = alarm,
            DeviceStatus = device.Status,
            ConsecutiveCommunicationFailures = 0
        };
    }

    private DeviceMonitoringResult ProcessCommunicationLost(
        Device device,
        DeviceSnapshot snapshot,
        TelemetryRecord telemetry,
        int maximumCommunicationFailures)
    {
        int failureCount =
            GetCommunicationFailureCount(device.DeviceId) + 1;

        _communicationFailures[device.DeviceId] =
            failureCount;

        DeviceStatus newStatus =
            _stateService
                .GetStatusAfterCommunicationFailures(
                    device.Status,
                    failureCount,
                    maximumCommunicationFailures);

        if (newStatus != device.Status)
        {
            device.Status = newStatus;
            _deviceRepository.Update(device);
        }

        // 通信中断时不更新LastCommunicationTime，
        // 也不调用温度报警服务。
        return new DeviceMonitoringResult
        {
            Snapshot = snapshot,
            Telemetry = telemetry,
            Alarm = null,
            DeviceStatus = device.Status,
            ConsecutiveCommunicationFailures =
                failureCount
        };
    }

    private void UpdateDeviceStatusFromSnapshot(
        Device device,
        DeviceSnapshot snapshot)
    {
        DeviceStatus targetStatus =
            ResolveTargetStatus(device, snapshot);

        if (targetStatus == device.Status)
        {
            return;
        }

        if (_stateService.CanTransition(
                device.Status,
                targetStatus))
        {
            device.Status = targetStatus;
        }
    }

    private static DeviceStatus ResolveTargetStatus(
        Device device,
        DeviceSnapshot snapshot)
    {
        // 维护状态只能通过维护闭环退出。
        if (device.Status == DeviceStatus.Maintenance)
        {
            return DeviceStatus.Maintenance;
        }

        // 故障状态不能因为下一帧故障信号消失而自动恢复。
        if (device.Status == DeviceStatus.Fault)
        {
            return DeviceStatus.Fault;
        }

        // Offline恢复时先进入Idle。
        if (device.Status == DeviceStatus.Offline)
        {
            return DeviceStatus.Idle;
        }

        if (snapshot.EmergencyStop ||
            snapshot.FaultSignal)
        {
            return DeviceStatus.Fault;
        }

        return snapshot.IsRunning
            ? DeviceStatus.Running
            : DeviceStatus.Idle;
    }

    private int GetCommunicationFailureCount(int deviceId)
    {
        return _communicationFailures.TryGetValue(
            deviceId,
            out int count)
            ? count
            : 0;
    }

    private static TelemetryRecord CreateTelemetry(
        int deviceId,
        DeviceSnapshot snapshot)
    {
        return new TelemetryRecord
        {
            DeviceId = deviceId,
            CollectedAt = snapshot.CollectedAt,
            Temperature = snapshot.Temperature,
            Pressure = snapshot.Pressure,
            Vibration = snapshot.Vibration,
            IsRunning = snapshot.IsRunning,
            FaultSignal = snapshot.FaultSignal,
            EmergencyStop = snapshot.EmergencyStop,
            Quality = snapshot.Quality
        };
    }
}
