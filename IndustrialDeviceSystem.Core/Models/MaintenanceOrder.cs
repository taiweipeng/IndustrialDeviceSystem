using IndustrialDeviceSystem.Core.Enums;

namespace IndustrialDeviceSystem.Core.Models;

public sealed class MaintenanceOrder
{
    public int OrderId { get; set; }

    public int DeviceId { get; set; }

    public int? SourceAlarmId { get; set; }

    public int? AssigneeId { get; set; }

    public string Description { get; set; } = string.Empty;

    public string Result { get; set; } = string.Empty;

    public MaintenanceStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? PlannedStartTime { get; set; }

    public DateTime? PlannedEndTime { get; set; }
}
