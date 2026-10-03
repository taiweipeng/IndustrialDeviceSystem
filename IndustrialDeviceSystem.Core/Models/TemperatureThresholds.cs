namespace IndustrialDeviceSystem.Core.Models;

public sealed class TemperatureThresholds
{
    public double Warning { get; set; } = 60.0;

    public double High { get; set; } = 80.0;

    public double Emergency { get; set; } = 90.0;
}
