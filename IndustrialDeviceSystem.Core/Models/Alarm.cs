using IndustrialDeviceSystem.Core.Enums;

namespace IndustrialDeviceSystem.Core.Models;

public sealed class Alarm
{
    public int AlarmId { get; set; }

    public int DeviceId { get; set; }

    public string AlarmType { get; set; } = string.Empty;

    public AlarmLevel Level { get; set; }

    public string Message { get; set; } = string.Empty;

    public AlarmStatus Status { get; set; }

    public DateTime OccurredAt { get; set; }

    public DateTime LastOccurredAt { get; set; }

    public int OccurrenceCount { get; set; } = 1;

    public int? AcknowledgedBy { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public DateTime? ClearedAt { get; set; }
}
