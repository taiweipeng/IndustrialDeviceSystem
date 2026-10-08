using IndustrialDeviceSystem.Core.Enums;
using IndustrialDeviceSystem.Core.Models;

namespace IndustrialDeviceSystem.Core.Services;

public sealed class DeviceStateService
{
    public bool CanMonitor(Device device)
    {
        ArgumentNullException.ThrowIfNull(device);

        return device.Enabled &&
               device.Status is
                   DeviceStatus.Idle or
                   DeviceStatus.Running;
    }

    public bool CanTransition(
        DeviceStatus from,
        DeviceStatus to)
    {
        if (!IsKnownStatus(from) ||
            !IsKnownStatus(to))
        {
            return false;
        }

        if (from == to)
        {
            return true;
        }

        if (to == DeviceStatus.Offline)
        {
            return true;
        }

        return (from, to) switch
        {
            (DeviceStatus.Offline, DeviceStatus.Idle) => true,
            (DeviceStatus.Idle, DeviceStatus.Running) => true,
            (DeviceStatus.Running, DeviceStatus.Idle) => true,
            (DeviceStatus.Idle, DeviceStatus.Fault) => true,
            (DeviceStatus.Running, DeviceStatus.Fault) => true,
            (DeviceStatus.Idle, DeviceStatus.Maintenance) => true,
            (DeviceStatus.Fault, DeviceStatus.Maintenance) => true,
            (DeviceStatus.Maintenance, DeviceStatus.Idle) => true,
            _ => false
        };
    }

    public DeviceStatus GetStatusAfterCommunicationFailures(
        DeviceStatus currentStatus,
        int consecutiveFailures,
        int maximumFailures = 3)
    {
        if (consecutiveFailures < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(consecutiveFailures));
        }

        if (maximumFailures < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumFailures));
        }

        DeviceStatus safeCurrentStatus =
            IsKnownStatus(currentStatus)
                ? currentStatus
                : DeviceStatus.Fault;

        return consecutiveFailures >= maximumFailures
            ? DeviceStatus.Offline
            : safeCurrentStatus;
    }

    private static bool IsKnownStatus(
        DeviceStatus status)
    {
        return Enum.IsDefined(
            typeof(DeviceStatus),
            status);
    }
}
