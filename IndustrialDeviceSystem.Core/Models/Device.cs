using IndustrialDeviceSystem.Core.Enums;

namespace IndustrialDeviceSystem.Core.Models;

public sealed class Device
{
    public int DeviceId { get; set; }

    public string DeviceCode { get; set; } = string.Empty;

    public string DeviceName { get; set; } = string.Empty;

    public string DeviceType { get; set; } = string.Empty;

    public DeviceStatus Status { get; set; }

    public bool Enabled { get; set; } = true;

    public DateTime? LastCommunicationTime { get; set; }
}
