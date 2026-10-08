namespace IndustrialDeviceSystem.Core.Models;

public sealed class OperationLog
{
    public int LogId { get; set; }

    public int? UserId { get; set; }

    public string ActionType { get; set; } =
        string.Empty;

    public string TargetType { get; set; } =
        string.Empty;

    public int? TargetId { get; set; }

    public string Detail { get; set; } =
        string.Empty;

    public DateTime OccurredAt { get; set; }
}
