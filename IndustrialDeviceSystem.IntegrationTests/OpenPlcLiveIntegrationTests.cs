using IndustrialDeviceSystem.Core.Enums;
using IndustrialDeviceSystem.Core.Services;
using IndustrialDeviceSystem.Infrastructure.DeviceCommunication;
using Xunit;
using Xunit.Abstractions;

namespace IndustrialDeviceSystem.IntegrationTests;

public sealed class OpenPlcLiveIntegrationTests
{
    private readonly ITestOutputHelper _output;

    public OpenPlcLiveIntegrationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [OpenPlcFact]
    [Trait("Category", "OpenPlcLive")]
    public void ReadSnapshot_NormalScenario_ReturnsExpectedValues()
    {
        using var client = new TcpModbusRegisterClient(
            new TcpModbusClientOptions
            {
                Host = "127.0.0.1",
                Port = 5020,
                ConnectTimeoutMilliseconds = 2000,
                ReadTimeoutMilliseconds = 2000,
                WriteTimeoutMilliseconds = 2000,
                Retries = 1
            });

        var dataSource = new OpenPlcDataSource(
            client,
            new OpenPlcDeviceOptions
            {
                DeviceCode = "DEV-001",
                UnitId = 1
            },
            new SystemClock());

        var snapshot = dataSource.ReadSnapshot("DEV-001");

        _output.WriteLine(
            $"Temperature={snapshot.Temperature:F1}, " +
            $"Pressure={snapshot.Pressure:F2}, " +
            $"Vibration={snapshot.Vibration:F2}, " +
            $"Running={snapshot.IsRunning}, " +
            $"Fault={snapshot.FaultSignal}, " +
            $"EmergencyStop={snapshot.EmergencyStop}, " +
            $"Quality={snapshot.Quality}");

        Assert.Equal("DEV-001", snapshot.DeviceCode);
        Assert.Equal(35.0, snapshot.Temperature, 3);
        Assert.Equal(1.00, snapshot.Pressure, 3);
        Assert.Equal(0.20, snapshot.Vibration, 3);
        Assert.True(snapshot.IsRunning);
        Assert.False(snapshot.FaultSignal);
        Assert.False(snapshot.EmergencyStop);
        Assert.Equal(DataQuality.Good, snapshot.Quality);
    }
}

public sealed class OpenPlcFactAttribute : FactAttribute
{
    public OpenPlcFactAttribute()
    {
        if (Environment.GetEnvironmentVariable(
                "RUN_OPENPLC_LIVE_TESTS") != "1")
        {
            Skip = "需要本机运行OpenPLC Runtime并设置RUN_OPENPLC_LIVE_TESTS=1。";
        }
    }
}
