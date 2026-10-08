namespace IndustrialDeviceSystem.Infrastructure.DeviceCommunication;

public sealed class TcpModbusClientOptions
{
    public string Host { get; init; } = "127.0.0.1";

    public int Port { get; init; } = 502;

    public int ConnectTimeoutMilliseconds { get; init; } = 2000;

    public int ReadTimeoutMilliseconds { get; init; } = 2000;

    public int WriteTimeoutMilliseconds { get; init; } = 2000;

    public int Retries { get; init; }
}
