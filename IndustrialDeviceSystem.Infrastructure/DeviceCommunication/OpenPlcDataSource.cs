using IndustrialDeviceSystem.Core.Enums;
using IndustrialDeviceSystem.Core.Interfaces;
using IndustrialDeviceSystem.Core.Models;

namespace IndustrialDeviceSystem.Infrastructure.DeviceCommunication;

public sealed class OpenPlcDataSource : IDeviceDataSource
{
    private readonly IModbusRegisterClient _client;
    private readonly OpenPlcDeviceOptions _options;
    private readonly IClock _clock;

    public OpenPlcDataSource(
        IModbusRegisterClient client,
        OpenPlcDeviceOptions options,
        IClock clock)
    {
        _client = client
            ?? throw new ArgumentNullException(nameof(client));

        _options = options
            ?? throw new ArgumentNullException(nameof(options));

        _clock = clock
            ?? throw new ArgumentNullException(nameof(clock));

        if (string.IsNullOrWhiteSpace(_options.DeviceCode))
        {
            throw new ArgumentException(
                "必须配置OpenPLC设备编码。",
                nameof(options));
        }
    }

    public DeviceSnapshot ReadSnapshot(string deviceCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceCode);

        if (!string.Equals(
                deviceCode,
                _options.DeviceCode,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new KeyNotFoundException(
                $"OpenPLC数据源未配置设备：{deviceCode}");
        }

        try
        {
            ushort[] registers =
                _client.ReadHoldingRegisters(
                    _options.UnitId,
                    OpenPlcRegisterMap.HoldingRegisterStartAddress,
                    OpenPlcRegisterMap.HoldingRegisterCount);

            bool[] coils =
                _client.ReadCoils(
                    _options.UnitId,
                    OpenPlcRegisterMap.CoilStartAddress,
                    OpenPlcRegisterMap.CoilCount);

            if (registers is null ||
                registers.Length <
                    OpenPlcRegisterMap.HoldingRegisterCount ||
                coils is null ||
                coils.Length <
                    OpenPlcRegisterMap.CoilCount)
            {
                return CreateUnavailableSnapshot(DataQuality.Bad);
            }

            return new DeviceSnapshot
            {
                DeviceCode = _options.DeviceCode,
                CollectedAt = _clock.Now,

                Temperature = registers[0] * 0.1,
                Pressure = registers[1] * 0.01,
                Vibration = registers[2] * 0.01,

                IsRunning = coils[0],
                FaultSignal = coils[1],
                EmergencyStop = coils[2],

                Quality = DataQuality.Good
            };
        }
        catch (ModbusCommunicationException)
        {
            return CreateUnavailableSnapshot(
                DataQuality.CommunicationLost);
        }
    }

    private DeviceSnapshot CreateUnavailableSnapshot(
        DataQuality quality)
    {
        return new DeviceSnapshot
        {
            DeviceCode = _options.DeviceCode,
            CollectedAt = _clock.Now,

            Temperature = 0,
            Pressure = 0,
            Vibration = 0,

            IsRunning = false,
            FaultSignal = false,
            EmergencyStop = false,

            Quality = quality
        };
    }
}
