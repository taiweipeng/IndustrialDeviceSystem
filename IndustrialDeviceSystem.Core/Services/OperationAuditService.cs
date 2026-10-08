using IndustrialDeviceSystem.Core.Interfaces;
using IndustrialDeviceSystem.Core.Models;

namespace IndustrialDeviceSystem.Core.Services;

public sealed class OperationAuditService
{
    private readonly IOperationLogRepository
        _operationLogRepository;

    private readonly IClock _clock;

    public OperationAuditService(
        IOperationLogRepository operationLogRepository,
        IClock clock)
    {
        ArgumentNullException.ThrowIfNull(
            operationLogRepository);

        ArgumentNullException.ThrowIfNull(clock);

        _operationLogRepository =
            operationLogRepository;

        _clock = clock;
    }

    public OperationLog Record(
        UserSession userSession,
        string actionType,
        string targetType,
        int? targetId,
        string detail)
    {
        ArgumentNullException.ThrowIfNull(
            userSession);

        if (string.IsNullOrWhiteSpace(actionType))
        {
            throw new ArgumentException(
                "操作类型不能为空。",
                nameof(actionType));
        }

        if (string.IsNullOrWhiteSpace(targetType))
        {
            throw new ArgumentException(
                "目标类型不能为空。",
                nameof(targetType));
        }

        if (string.IsNullOrWhiteSpace(detail))
        {
            throw new ArgumentException(
                "操作详情不能为空。",
                nameof(detail));
        }

        var operationLog = new OperationLog
        {
            UserId = userSession.UserId,
            ActionType = actionType.Trim(),
            TargetType = targetType.Trim(),
            TargetId = targetId,
            Detail = detail.Trim(),
            OccurredAt = _clock.Now
        };

        _operationLogRepository.Add(operationLog);

        return operationLog;
    }
}

public static class AuditActionTypes
{
    public const string Login =
        "Login";

    public const string Logout =
        "Logout";

    public const string CreateBooking =
        "Booking.Create";

    public const string CancelBooking =
        "Booking.Cancel";

    public const string ConfirmBooking =
        "Booking.Confirm";

    public const string CompleteBooking =
        "Booking.Complete";

    public const string AcknowledgeAlarm =
        "Alarm.Acknowledge";

    public const string ClearAlarm =
        "Alarm.Clear";

    public const string CreateMaintenanceOrder =
        "Maintenance.Create";

    public const string StartMaintenanceOrder =
        "Maintenance.Start";

    public const string CompleteMaintenanceOrder =
        "Maintenance.Complete";

    public const string ExportHistory =
        "History.Export";

    public const string StartSimulation =
        "Simulation.Start";

    public const string StopSimulation =
        "Simulation.Stop";

    public const string AssignMaintenanceOrder =
        "Maintenance.Assign";

    public const string CloseMaintenanceOrder =
        "Maintenance.Close";

    public const string StartMonitoring =
        "Monitoring.Start";

    public const string StopMonitoring =
        "Monitoring.Stop";

    public const string ChangeSimulationScenario =
        "Simulation.ChangeScenario";

    public const string ChangeOwnPassword =
        "User.ChangeOwnPassword";

    public const string CreateUser =
        "User.Create";

    public const string ResetUserPassword =
        "User.ResetPassword";

    public const string EnableUser =
        "User.Enable";

    public const string DisableUser =
        "User.Disable";

    public const string ChangeUserRole =
        "User.ChangeRole";
}

public static class AuditTargetTypes
{
    public const string UserAccount =
        "UserAccount";

    public const string Booking =
        "Booking";

    public const string Alarm =
        "Alarm";

    public const string MaintenanceOrder =
        "MaintenanceOrder";

    public const string Device =
        "Device";

    public const string Telemetry =
        "Telemetry";
}
