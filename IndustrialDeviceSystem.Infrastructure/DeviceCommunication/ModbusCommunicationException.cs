namespace IndustrialDeviceSystem.Infrastructure.DeviceCommunication;

public sealed class ModbusCommunicationException : Exception
{
    public ModbusCommunicationException(string message)
        : base(message)
    {
    }

    public ModbusCommunicationException(
        string message,
        Exception innerException)
        : base(message, innerException)
    {
    }
}
