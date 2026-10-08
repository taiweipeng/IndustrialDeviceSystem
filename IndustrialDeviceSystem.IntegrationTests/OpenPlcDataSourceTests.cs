using IndustrialDeviceSystem.Core.Enums;
using IndustrialDeviceSystem.Core.Interfaces;
using IndustrialDeviceSystem.Infrastructure.DeviceCommunication;

namespace IndustrialDeviceSystem.IntegrationTests;

public sealed class OpenPlcDataSourceTests
{
    private static readonly DateTime CollectedAt =
        new(2026, 10, 4, 3, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void ReadSnapshot_ValidResponse_MapsRegistersAndCoils()
    {
        var client = new StubModbusRegisterClient
        {
            Registers = [723, 125, 37],
            Coils = [true, false, true]
        };

        OpenPlcDataSource source = CreateSource(client);

        var snapshot = source.ReadSnapshot("DEV-PLC-01");

        Assert.Equal("DEV-PLC-01", snapshot.DeviceCode);
        Assert.Equal(CollectedAt, snapshot.CollectedAt);
        Assert.Equal(72.3, snapshot.Temperature, 3);
        Assert.Equal(1.25, snapshot.Pressure, 3);
        Assert.Equal(0.37, snapshot.Vibration, 3);

        Assert.True(snapshot.IsRunning);
        Assert.False(snapshot.FaultSignal);
        Assert.True(snapshot.EmergencyStop);
        Assert.Equal(DataQuality.Good, snapshot.Quality);

        Assert.Equal((byte)7, client.LastHoldingUnitId);
        Assert.Equal(
            OpenPlcRegisterMap.HoldingRegisterStartAddress,
            client.LastHoldingStartAddress);
        Assert.Equal(
            OpenPlcRegisterMap.HoldingRegisterCount,
            client.LastHoldingCount);

        Assert.Equal((byte)7, client.LastCoilUnitId);
        Assert.Equal(
            OpenPlcRegisterMap.CoilStartAddress,
            client.LastCoilStartAddress);
        Assert.Equal(
            OpenPlcRegisterMap.CoilCount,
            client.LastCoilCount);
    }

    [Fact]
    public void ReadSnapshot_CommunicationFailure_ReturnsCommunicationLost()
    {
        var client = new StubModbusRegisterClient
        {
            ThrowCommunicationException = true
        };

        OpenPlcDataSource source = CreateSource(client);

        var snapshot = source.ReadSnapshot("DEV-PLC-01");

        Assert.Equal(
            DataQuality.CommunicationLost,
            snapshot.Quality);

        Assert.Equal(CollectedAt, snapshot.CollectedAt);
        Assert.False(snapshot.IsRunning);
        Assert.False(snapshot.FaultSignal);
        Assert.False(snapshot.EmergencyStop);

        Assert.Equal(1, client.HoldingReadCalls);
        Assert.Equal(0, client.CoilReadCalls);
    }

    [Fact]
    public void ReadSnapshot_IncompleteRegisters_ReturnsBadQuality()
    {
        var client = new StubModbusRegisterClient
        {
            Registers = [723, 125],
            Coils = [true, false, false]
        };

        OpenPlcDataSource source = CreateSource(client);

        var snapshot = source.ReadSnapshot("DEV-PLC-01");

        Assert.Equal(DataQuality.Bad, snapshot.Quality);
        Assert.Equal(0, snapshot.Temperature);
        Assert.Equal(0, snapshot.Pressure);
        Assert.Equal(0, snapshot.Vibration);
    }

    [Fact]
    public void ReadSnapshot_IncompleteCoils_ReturnsBadQuality()
    {
        var client = new StubModbusRegisterClient
        {
            Registers = [723, 125, 37],
            Coils = [true, false]
        };

        OpenPlcDataSource source = CreateSource(client);

        var snapshot = source.ReadSnapshot("DEV-PLC-01");

        Assert.Equal(DataQuality.Bad, snapshot.Quality);
        Assert.False(snapshot.IsRunning);
        Assert.False(snapshot.FaultSignal);
        Assert.False(snapshot.EmergencyStop);
    }

    [Fact]
    public void ReadSnapshot_UnknownDevice_ThrowsKeyNotFoundException()
    {
        var client = new StubModbusRegisterClient();

        OpenPlcDataSource source = CreateSource(client);

        Assert.Throws<KeyNotFoundException>(
            () => source.ReadSnapshot("DEV-OTHER"));

        Assert.Equal(0, client.HoldingReadCalls);
        Assert.Equal(0, client.CoilReadCalls);
    }

    private static OpenPlcDataSource CreateSource(
        StubModbusRegisterClient client)
    {
        return new OpenPlcDataSource(
            client,
            new OpenPlcDeviceOptions
            {
                DeviceCode = "DEV-PLC-01",
                UnitId = 7
            },
            new FixedClock(CollectedAt));
    }

    private sealed class FixedClock : IClock
    {
        public FixedClock(DateTime now)
        {
            Now = now;
        }

        public DateTime Now { get; }
    }

    private sealed class StubModbusRegisterClient
        : IModbusRegisterClient
    {
        public ushort[] Registers { get; set; } = [0, 0, 0];

        public bool[] Coils { get; set; } =
            [false, false, false];

        public bool ThrowCommunicationException { get; set; }

        public int HoldingReadCalls { get; private set; }

        public int CoilReadCalls { get; private set; }

        public byte LastHoldingUnitId { get; private set; }

        public ushort LastHoldingStartAddress { get; private set; }

        public ushort LastHoldingCount { get; private set; }

        public byte LastCoilUnitId { get; private set; }

        public ushort LastCoilStartAddress { get; private set; }

        public ushort LastCoilCount { get; private set; }

        public ushort[] ReadHoldingRegisters(
            byte unitId,
            ushort startAddress,
            ushort count)
        {
            HoldingReadCalls++;
            LastHoldingUnitId = unitId;
            LastHoldingStartAddress = startAddress;
            LastHoldingCount = count;

            if (ThrowCommunicationException)
            {
                throw new ModbusCommunicationException(
                    "模拟Modbus通信失败。");
            }

            return Registers;
        }

        public bool[] ReadCoils(
            byte unitId,
            ushort startAddress,
            ushort count)
        {
            CoilReadCalls++;
            LastCoilUnitId = unitId;
            LastCoilStartAddress = startAddress;
            LastCoilCount = count;

            if (ThrowCommunicationException)
            {
                throw new ModbusCommunicationException(
                    "模拟Modbus通信失败。");
            }

            return Coils;
        }
    }
}
