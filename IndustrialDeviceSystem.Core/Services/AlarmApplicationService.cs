using IndustrialDeviceSystem.Core.Enums;
using IndustrialDeviceSystem.Core.Interfaces;
using IndustrialDeviceSystem.Core.Models;

namespace IndustrialDeviceSystem.Core.Services;

public sealed class AlarmApplicationService
{
    private const string TemperatureAlarmType = "Temperature";

    private readonly IAlarmRepository _alarmRepository;
    private readonly AlarmRuleService _ruleService;
    private readonly IClock _clock;

    public AlarmApplicationService(
        IAlarmRepository alarmRepository,
        AlarmRuleService ruleService,
        IClock clock)
    {
        ArgumentNullException.ThrowIfNull(alarmRepository);
        ArgumentNullException.ThrowIfNull(ruleService);
        ArgumentNullException.ThrowIfNull(clock);

        _alarmRepository = alarmRepository;
        _ruleService = ruleService;
        _clock = clock;
    }

    public Alarm? ProcessTemperature(
        int deviceId,
        double temperature,
        TemperatureThresholds thresholds)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(deviceId),
                "设备编号必须大于0。");
        }

        ArgumentNullException.ThrowIfNull(thresholds);

        AlarmLevel level =
            _ruleService.GetTemperatureLevel(
                temperature,
                thresholds);

        Alarm? existingAlarm =
            _alarmRepository.GetActiveAlarm(
                deviceId,
                TemperatureAlarmType);

        DateTime now = _clock.Now;

        if (level == AlarmLevel.Normal)
        {
            return ClearExistingAlarm(
                existingAlarm,
                now);
        }

        if (existingAlarm is null)
        {
            return CreateNewAlarm(
                deviceId,
                temperature,
                level,
                now);
        }

        return UpdateExistingAlarm(
            existingAlarm,
            temperature,
            level,
            now);
    }

    private Alarm? ClearExistingAlarm(
        Alarm? existingAlarm,
        DateTime now)
    {
        if (existingAlarm is null)
        {
            return null;
        }

        existingAlarm.Status = AlarmStatus.Cleared;
        existingAlarm.ClearedAt = now;

        _alarmRepository.Update(existingAlarm);

        return existingAlarm;
    }

    private Alarm CreateNewAlarm(
        int deviceId,
        double temperature,
        AlarmLevel level,
        DateTime now)
    {
        var alarm = new Alarm
        {
            DeviceId = deviceId,
            AlarmType = TemperatureAlarmType,
            Level = level,
            Message = CreateMessage(temperature, level),
            Status = AlarmStatus.Active,
            OccurredAt = now,
            LastOccurredAt = now,
            OccurrenceCount = 1,
            AcknowledgedBy = null,
            AcknowledgedAt = null,
            ClearedAt = null
        };

        _alarmRepository.Add(alarm);

        return alarm;
    }

    private Alarm UpdateExistingAlarm(
        Alarm existingAlarm,
        double temperature,
        AlarmLevel level,
        DateTime now)
    {
        existingAlarm.Level = level;
        existingAlarm.Message =
            CreateMessage(temperature, level);
        existingAlarm.LastOccurredAt = now;
        existingAlarm.OccurrenceCount++;

        // 不修改OccurredAt、Status和报警确认信息。
        _alarmRepository.Update(existingAlarm);

        return existingAlarm;
    }

    private static string CreateMessage(
        double temperature,
        AlarmLevel level)
    {
        return $"设备温度为{temperature:F1}℃，报警等级为{level}。";
    }

    public bool AcknowledgeAlarm(
    Alarm alarm,
    int acknowledgedBy)
    {
        ArgumentNullException.ThrowIfNull(alarm);

        if (acknowledgedBy <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(acknowledgedBy));
        }

        if (alarm.AlarmId <= 0 ||
            alarm.Status != AlarmStatus.Active)
        {
            return false;
        }

        alarm.Status =
            AlarmStatus.Acknowledged;

        alarm.AcknowledgedBy =
            acknowledgedBy;

        alarm.AcknowledgedAt =
            _clock.Now;

        _alarmRepository.Update(alarm);

        return true;
    }
}
