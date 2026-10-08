using IndustrialDeviceSystem.Core.Enums;
using IndustrialDeviceSystem.Core.Interfaces;
using IndustrialDeviceSystem.Core.Models;

namespace IndustrialDeviceSystem.Core.Services;

public sealed class MaintenanceWorkflowService
{
    private readonly MaintenanceApplicationService
        _maintenanceService;

    private readonly IDeviceRepository
        _deviceRepository;

    private readonly DeviceStateService
        _deviceStateService;

    public MaintenanceWorkflowService(
        MaintenanceApplicationService maintenanceService,
        IDeviceRepository deviceRepository,
        DeviceStateService deviceStateService)
    {
        _maintenanceService = maintenanceService
            ?? throw new ArgumentNullException(
                nameof(maintenanceService));

        _deviceRepository = deviceRepository
            ?? throw new ArgumentNullException(
                nameof(deviceRepository));

        _deviceStateService = deviceStateService
            ?? throw new ArgumentNullException(
                nameof(deviceStateService));
    }

    public MaintenanceOrder CreateFromAlarm(
        Alarm alarm,
        int? assigneeId,
        string description,
        DateTime? plannedStartTime = null,
        DateTime? plannedEndTime = null)
    {
        return _maintenanceService.CreateFromAlarm(
            alarm,
            assigneeId,
            description,
            plannedStartTime,
            plannedEndTime);
    }

    public bool Assign(
        MaintenanceOrder order,
        int assigneeId)
    {
        return _maintenanceService.Assign(
            order,
            assigneeId);
    }

    public bool Start(MaintenanceOrder order)
    {
        ArgumentNullException.ThrowIfNull(order);

        var device = _deviceRepository.GetById(
            order.DeviceId);

        if (device is null ||
            !CanEnterMaintenance(device.Status))
        {
            return false;
        }

        if (!_maintenanceService.Start(order))
        {
            return false;
        }

        // 表示现场已经确认停机。
        // Running设备逻辑上经过Running→Idle→Maintenance。
        device.Status = DeviceStatus.Maintenance;
        _deviceRepository.Update(device);

        return true;
    }

    public bool Complete(
        MaintenanceOrder order,
        string result)
    {
        return _maintenanceService.Complete(
            order,
            result);
    }

    public bool Close(
        MaintenanceOrder order,
        bool verificationPassed)
    {
        ArgumentNullException.ThrowIfNull(order);

        var device = _deviceRepository.GetById(
            order.DeviceId);

        if (device is null ||
            device.Status != DeviceStatus.Maintenance ||
            !_deviceStateService.CanTransition(
                DeviceStatus.Maintenance,
                DeviceStatus.Idle))
        {
            return false;
        }

        if (!_maintenanceService.Close(
                order,
                verificationPassed))
        {
            return false;
        }

        device.Status = DeviceStatus.Idle;
        _deviceRepository.Update(device);

        return true;
    }

    private bool CanEnterMaintenance(
        DeviceStatus status)
    {
        return status switch
        {
            DeviceStatus.Idle =>
                _deviceStateService.CanTransition(
                    DeviceStatus.Idle,
                    DeviceStatus.Maintenance),

            DeviceStatus.Fault =>
                _deviceStateService.CanTransition(
                    DeviceStatus.Fault,
                    DeviceStatus.Maintenance),

            DeviceStatus.Running =>
                _deviceStateService.CanTransition(
                    DeviceStatus.Running,
                    DeviceStatus.Idle)
                &&
                _deviceStateService.CanTransition(
                    DeviceStatus.Idle,
                    DeviceStatus.Maintenance),

            DeviceStatus.Maintenance => true,

            _ => false
        };
    }
}
