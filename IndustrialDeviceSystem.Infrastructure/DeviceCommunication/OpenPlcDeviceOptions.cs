namespace IndustrialDeviceSystem.Infrastructure.DeviceCommunication;

public sealed class OpenPlcDeviceOptions
{
    public string DeviceCode { get; init; } = string.Empty;

    public byte UnitId { get; init; } = 1;
}
