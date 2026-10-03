using IndustrialDeviceSystem.Core.Enums;

namespace IndustrialDeviceSystem.Core.Models;

public sealed class DeviceSnapshot
{
    public string DeviceCode { get; set; } = string.Empty;

    public DateTime CollectedAt { get; set; }

    public double Temperature { get; set; }

    public double Pressure { get; set; }

    public double Vibration { get; set; }

    public bool IsRunning { get; set; }

    public bool FaultSignal { get; set; }

    public bool EmergencyStop { get; set; }

    public DataQuality Quality { get; set; }
}
