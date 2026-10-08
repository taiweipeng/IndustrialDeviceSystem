using IndustrialDeviceSystem.Core.Enums;

namespace IndustrialDeviceSystem.Core.Models;

public sealed class DeviceMonitoringResult
{
    public DeviceSnapshot Snapshot { get; init; } = new();

    public TelemetryRecord Telemetry { get; init; } = new();

    public Alarm? Alarm { get; init; }

    public DeviceStatus DeviceStatus { get; init; }

    public int ConsecutiveCommunicationFailures { get; init; }
}
