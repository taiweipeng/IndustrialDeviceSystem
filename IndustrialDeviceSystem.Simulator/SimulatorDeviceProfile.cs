namespace IndustrialDeviceSystem.Simulator;

public sealed class SimulatorDeviceProfile
{
    public string DeviceCode { get; set; } = string.Empty;

    public double BaseTemperature { get; set; } = 35.0;

    public double BasePressure { get; set; } = 1.0;

    public double BaseVibration { get; set; } = 0.2;

    public bool NormallyRunning { get; set; } = true;
}
