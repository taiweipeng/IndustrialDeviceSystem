using IndustrialDeviceSystem.Core.Enums;
using IndustrialDeviceSystem.Core.Models;

namespace IndustrialDeviceSystem.Core.Services;

public sealed class MaintenanceRuleService
{
    public bool CanComplete(
        MaintenanceOrder order)
    {
        ArgumentNullException.ThrowIfNull(order);

        return order.Status ==
                   MaintenanceStatus.InProgress &&
               !string.IsNullOrWhiteSpace(
                   order.Result);
    }

    public bool CanClose(
        MaintenanceOrder order,
        bool verificationPassed)
    {
        ArgumentNullException.ThrowIfNull(order);

        return order.Status ==
                   MaintenanceStatus.Completed &&
               verificationPassed;
    }
}
