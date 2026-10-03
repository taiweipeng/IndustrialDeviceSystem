using IndustrialDeviceSystem.Core.Enums;

namespace IndustrialDeviceSystem.Core.Models;

public sealed class Booking
{
    public int BookingId { get; set; }

    public int DeviceId { get; set; }

    public int UserId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public string Purpose { get; set; } = string.Empty;

    public BookingStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }
}
