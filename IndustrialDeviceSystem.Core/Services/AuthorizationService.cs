using IndustrialDeviceSystem.Core.Enums;
using IndustrialDeviceSystem.Core.Models;

namespace IndustrialDeviceSystem.Core.Services;

public sealed class AuthorizationService
{
    public bool HasPermission(
        UserSession session,
        SystemPermission permission)
    {
        ArgumentNullException.ThrowIfNull(session);

        if (!Enum.IsDefined(
                typeof(SystemPermission),
                permission))
        {
            throw new ArgumentOutOfRangeException(
                nameof(permission),
                "系统权限无效。");
        }

        return session.Role switch
        {
            UserRole.Administrator => true,

            UserRole.Operator =>
                HasOperatorPermission(permission),

            UserRole.Maintainer =>
                HasMaintainerPermission(permission),

            UserRole.Auditor =>
                HasAuditorPermission(permission),

            _ => false
        };
    }

    public void EnsurePermission(
        UserSession session,
        SystemPermission permission)
    {
        if (!HasPermission(session, permission))
        {
            throw new UnauthorizedAccessException(
                $"用户“{session.UserName}”" +
                $"没有执行“{permission}”的权限。");
        }
    }

    public bool CanManageBooking(
        UserSession session,
        Booking booking)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(booking);

        if (HasPermission(
                session,
                SystemPermission.ManageAllBookings))
        {
            return true;
        }

        return HasPermission(
                   session,
                   SystemPermission.ManageOwnBooking) &&
               booking.UserId == session.UserId;
    }

    public void EnsureCanManageBooking(
        UserSession session,
        Booking booking)
    {
        if (!CanManageBooking(session, booking))
        {
            throw new UnauthorizedAccessException(
                $"用户“{session.UserName}”" +
                "无权操作该预约。");
        }
    }

    private static bool HasOperatorPermission(
        SystemPermission permission)
    {
        return permission is
            SystemPermission.ViewDashboard or
            SystemPermission.MonitorDevice or
            SystemPermission.ControlSimulation or
            SystemPermission.CreateBooking or
            SystemPermission.ManageOwnBooking or
            SystemPermission.AcknowledgeAlarm or
            SystemPermission.CreateMaintenanceOrder or
            SystemPermission.ViewHistory or
            SystemPermission.ExportHistory or
            SystemPermission.ViewOwnOperationLog;
    }

    private static bool HasMaintainerPermission(
        SystemPermission permission)
    {
        return permission is
            SystemPermission.ViewDashboard or
            SystemPermission.MonitorDevice or
            SystemPermission.AcknowledgeAlarm or
            SystemPermission.ClearAlarm or
            SystemPermission.CreateMaintenanceOrder or
            SystemPermission.ProcessMaintenanceOrder or
            SystemPermission.ViewHistory or
            SystemPermission.ExportHistory or
            SystemPermission.ViewOwnOperationLog;
    }

    private static bool HasAuditorPermission(
        SystemPermission permission)
    {
        return permission is
            SystemPermission.ViewDashboard or
            SystemPermission.MonitorDevice or
            SystemPermission.ViewHistory or
            SystemPermission.ExportHistory or
            SystemPermission.ViewAllOperationLogs;
    }
}
