using IndustrialDeviceSystem.Core.Enums;
using IndustrialDeviceSystem.Core.Interfaces;
using IndustrialDeviceSystem.Core.Models;

namespace IndustrialDeviceSystem.Core.Services;

public sealed class MaintenanceApplicationService
{
    private readonly IMaintenanceRepository _maintenanceRepository;
    private readonly MaintenanceRuleService _ruleService;
    private readonly IClock _clock;

    public MaintenanceApplicationService(
        IMaintenanceRepository maintenanceRepository,
        MaintenanceRuleService ruleService,
        IClock clock)
    {
        ArgumentNullException.ThrowIfNull(maintenanceRepository);
        ArgumentNullException.ThrowIfNull(ruleService);
        ArgumentNullException.ThrowIfNull(clock);

        _maintenanceRepository = maintenanceRepository;
        _ruleService = ruleService;
        _clock = clock;
    }

    public MaintenanceOrder CreateFromAlarm(
        Alarm sourceAlarm,
        int? assigneeId,
        string description,
        DateTime? plannedStartTime = null,
        DateTime? plannedEndTime = null)
    {
        ArgumentNullException.ThrowIfNull(sourceAlarm);

        if (sourceAlarm.AlarmId <= 0)
        {
            throw new ArgumentException(
                "必须使用已经持久化的报警记录创建工单。",
                nameof(sourceAlarm));
        }

        MaintenanceOrder? existingOrder =
            _maintenanceRepository
                .GetActiveOrders(sourceAlarm.DeviceId)
                .FirstOrDefault(order =>
                    order.SourceAlarmId == sourceAlarm.AlarmId);

        // 幂等处理：同一报警不能重复创建活动工单。
        if (existingOrder is not null)
        {
            return existingOrder;
        }

        bool alarmIsOpen =
            sourceAlarm.Status is AlarmStatus.Active
                or AlarmStatus.Acknowledged;

        bool levelRequiresMaintenance =
            sourceAlarm.Level is AlarmLevel.High
                or AlarmLevel.Emergency;

        if (!alarmIsOpen || !levelRequiresMaintenance)
        {
            throw new InvalidOperationException(
                "只有未解除的高等级或紧急报警才能创建维护工单。");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException(
                "维护说明不能为空。",
                nameof(description));
        }

        if (assigneeId.HasValue && assigneeId.Value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(assigneeId),
                "维护人员编号必须大于0。");
        }

        ValidatePlannedTime(
            plannedStartTime,
            plannedEndTime);

        var order = new MaintenanceOrder
        {
            DeviceId = sourceAlarm.DeviceId,
            SourceAlarmId = sourceAlarm.AlarmId,
            AssigneeId = assigneeId,
            Description = description.Trim(),
            Result = string.Empty,
            Status = assigneeId.HasValue
                ? MaintenanceStatus.Assigned
                : MaintenanceStatus.Created,
            CreatedAt = _clock.Now,
            PlannedStartTime = plannedStartTime,
            PlannedEndTime = plannedEndTime,
            StartedAt = null,
            CompletedAt = null
        };

        _maintenanceRepository.Add(order);

        return order;
    }

    public bool Assign(
        MaintenanceOrder order,
        int assigneeId)
    {
        ArgumentNullException.ThrowIfNull(order);

        if (assigneeId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(assigneeId),
                "维护人员编号必须大于0。");
        }

        if (order.Status != MaintenanceStatus.Created)
        {
            return false;
        }

        order.AssigneeId = assigneeId;
        order.Status = MaintenanceStatus.Assigned;

        _maintenanceRepository.Update(order);

        return true;
    }

    public bool Start(MaintenanceOrder order)
    {
        ArgumentNullException.ThrowIfNull(order);

        if (order.Status != MaintenanceStatus.Assigned ||
            !order.AssigneeId.HasValue)
        {
            return false;
        }

        order.Status = MaintenanceStatus.InProgress;
        order.StartedAt = _clock.Now;

        _maintenanceRepository.Update(order);

        return true;
    }

    public bool Complete(
        MaintenanceOrder order,
        string result)
    {
        ArgumentNullException.ThrowIfNull(order);

        if (string.IsNullOrWhiteSpace(result))
        {
            return false;
        }

        string originalResult = order.Result;
        order.Result = result.Trim();

        if (!_ruleService.CanComplete(order))
        {
            order.Result = originalResult;
            return false;
        }

        order.Status = MaintenanceStatus.Completed;
        order.CompletedAt = _clock.Now;

        _maintenanceRepository.Update(order);

        return true;
    }

    public bool Close(
        MaintenanceOrder order,
        bool verificationPassed)
    {
        ArgumentNullException.ThrowIfNull(order);

        if (!_ruleService.CanClose(
                order,
                verificationPassed))
        {
            return false;
        }

        order.Status = MaintenanceStatus.Closed;

        _maintenanceRepository.Update(order);

        return true;
    }

    private static void ValidatePlannedTime(
        DateTime? plannedStartTime,
        DateTime? plannedEndTime)
    {
        bool onlyOneProvided =
            plannedStartTime.HasValue !=
            plannedEndTime.HasValue;

        if (onlyOneProvided)
        {
            throw new ArgumentException(
                "计划开始时间和结束时间必须同时填写或同时为空。");
        }

        if (plannedStartTime.HasValue &&
            plannedStartTime.Value >= plannedEndTime!.Value)
        {
            throw new ArgumentException(
                "计划开始时间必须早于计划结束时间。");
        }
    }
}
