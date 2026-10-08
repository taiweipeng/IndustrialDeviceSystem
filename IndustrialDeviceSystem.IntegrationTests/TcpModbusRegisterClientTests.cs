using System.Net;
using System.Net.Sockets;
using IndustrialDeviceSystem.Infrastructure.DeviceCommunication;

namespace IndustrialDeviceSystem.IntegrationTests;

public sealed class TcpModbusRegisterClientTests
{
    [Fact]
    public void Constructor_BlankHost_ThrowsArgumentException()
    {
        var options = new TcpModbusClientOptions
        {
            Host = " "
        };

        Assert.Throws<ArgumentException>(
            () => new TcpModbusRegisterClient(options));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(65536)]
    public void Constructor_InvalidPort_ThrowsArgumentOutOfRangeException(
        int port)
    {
        var options = new TcpModbusClientOptions
        {
            Host = "127.0.0.1",
            Port = port
        };

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new TcpModbusRegisterClient(options));
    }

    [Fact]
    public void ReadHoldingRegisters_ServerDoesNotRespond_ThrowsCommunicationException()
    {
        var listener = new TcpListener(
            IPAddress.Loopback,
            0);

        listener.Start();

        try
        {
            int port =
                ((IPEndPoint)listener.LocalEndpoint).Port;

            using var client =
                new TcpModbusRegisterClient(
                    new TcpModbusClientOptions
                    {
                        Host = "127.0.0.1",
                        Port = port,
                        ConnectTimeoutMilliseconds = 1000,
                        ReadTimeoutMilliseconds = 100,
                        WriteTimeoutMilliseconds = 100,
                        Retries = 0
                    });

            var exception =
                Assert.Throws<ModbusCommunicationException>(
                    () => client.ReadHoldingRegisters(
                        unitId: 1,
                        startAddress: 0,
                        count: 1));

            Assert.NotNull(exception.InnerException);
        }
        finally
        {
            listener.Stop();
        }
    }

    [Fact]
    public void ReadAfterDispose_ThrowsObjectDisposedException()
    {
        var client =
            new TcpModbusRegisterClient(
                new TcpModbusClientOptions
                {
                    Host = "127.0.0.1",
                    Port = 502
                });

        client.Dispose();

        Assert.Throws<ObjectDisposedException>(
            () => client.ReadCoils(
                unitId: 1,
                startAddress: 0,
                count: 1));
    }
}
