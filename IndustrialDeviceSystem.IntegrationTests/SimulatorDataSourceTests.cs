using IndustrialDeviceSystem.Core.Enums;
using IndustrialDeviceSystem.Core.Interfaces;
using IndustrialDeviceSystem.Core.Models;
using IndustrialDeviceSystem.Simulator;
using Xunit;

namespace IndustrialDeviceSystem.IntegrationTests;

public sealed class SimulatorDataSourceTests
{
    private static readonly DateTime CurrentTime =
        new(2026, 10, 4, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void ReadSnapshot_Normal_ShouldReturnGoodData()
    {
        SimulatorDataSource source = CreateSource();

        DeviceSnapshot snapshot =
            source.ReadSnapshot("DEV-001");

        Assert.Equal("DEV-001", snapshot.DeviceCode);
        Assert.Equal(CurrentTime, snapshot.CollectedAt);
        Assert.Equal(DataQuality.Good, snapshot.Quality);
        Assert.True(snapshot.IsRunning);
        Assert.False(snapshot.FaultSignal);
        Assert.False(snapshot.EmergencyStop);

        Assert.InRange(
            snapshot.Temperature,
            34.0,
            36.0);
    }

    [Fact]
    public void ReadSnapshot_TemperatureRise_ShouldIncrease()
    {
        SimulatorDataSource source = CreateSource();

        source.SetScenario(
            "DEV-001",
            SimulationScenario.TemperatureRise);

        DeviceSnapshot first =
            source.ReadSnapshot("DEV-001");

        DeviceSnapshot second =
            source.ReadSnapshot("DEV-001");

        DeviceSnapshot third =
            source.ReadSnapshot("DEV-001");

        Assert.True(
            first.Temperature <
            second.Temperature);

        Assert.True(
            second.Temperature <
            third.Temperature);

        Assert.Equal(DataQuality.Good, third.Quality);
    }

    [Fact]
    public void SetScenario_ShouldResetSampleSequence()
    {
        SimulatorDataSource source = CreateSource();

        source.SetScenario(
            "DEV-001",
            SimulationScenario.TemperatureRise);

        double firstTemperature =
            source.ReadSnapshot("DEV-001").Temperature;

        source.ReadSnapshot("DEV-001");
        source.ReadSnapshot("DEV-001");

        source.SetScenario(
            "DEV-001",
            SimulationScenario.TemperatureRise);

        double restartedTemperature =
            source.ReadSnapshot("DEV-001").Temperature;

        Assert.Equal(
            firstTemperature,
            restartedTemperature);
    }

    [Fact]
    public void ReadSnapshot_Fault_ShouldSetFaultSignal()
    {
        SimulatorDataSource source = CreateSource();

        source.SetScenario(
            "DEV-001",
            SimulationScenario.Fault);

        DeviceSnapshot snapshot =
            source.ReadSnapshot("DEV-001");

        Assert.Equal(DataQuality.Good, snapshot.Quality);
        Assert.True(snapshot.FaultSignal);
        Assert.False(snapshot.IsRunning);
        Assert.False(snapshot.EmergencyStop);
    }

    [Fact]
    public void ReadSnapshot_EmergencyStop_ShouldSetEmergencySignal()
    {
        SimulatorDataSource source = CreateSource();

        source.SetScenario(
            "DEV-001",
            SimulationScenario.EmergencyStop);

        DeviceSnapshot snapshot =
            source.ReadSnapshot("DEV-001");

        Assert.Equal(DataQuality.Good, snapshot.Quality);
        Assert.True(snapshot.EmergencyStop);
        Assert.False(snapshot.IsRunning);
        Assert.False(snapshot.FaultSignal);
    }

    [Fact]
    public void ReadSnapshot_CommunicationLost_ShouldMarkBadQuality()
    {
        SimulatorDataSource source = CreateSource();

        source.SetScenario(
            "DEV-001",
            SimulationScenario.CommunicationLost);

        DeviceSnapshot snapshot =
            source.ReadSnapshot("DEV-001");

        Assert.Equal(
            DataQuality.CommunicationLost,
            snapshot.Quality);

        Assert.False(snapshot.IsRunning);
    }

    [Fact]
    public void ReadSnapshot_UnknownDevice_ShouldThrow()
    {
        SimulatorDataSource source = CreateSource();

        Assert.Throws<KeyNotFoundException>(() =>
            source.ReadSnapshot("UNKNOWN"));
    }

    [Fact]
    public void Constructor_DuplicateDeviceCodes_ShouldThrow()
    {
        var profiles = new[]
        {
            CreateProfile("DEV-001"),
            CreateProfile("dev-001")
        };

        Assert.Throws<ArgumentException>(() =>
            new SimulatorDataSource(
                profiles,
                new FixedClock(CurrentTime)));
    }

    private static SimulatorDataSource CreateSource()
    {
        return new SimulatorDataSource(
            new[]
            {
                CreateProfile("DEV-001")
            },
            new FixedClock(CurrentTime));
    }

    private static SimulatorDeviceProfile CreateProfile(
        string deviceCode)
    {
        return new SimulatorDeviceProfile
        {
            DeviceCode = deviceCode,
            BaseTemperature = 35.0,
            BasePressure = 1.0,
            BaseVibration = 0.2,
            NormallyRunning = true
        };
    }

    private sealed class FixedClock : IClock
    {
        public FixedClock(DateTime now)
        {
            Now = now;
        }

        public DateTime Now { get; }
    }
}
