namespace IndustrialDeviceSystem.Infrastructure.DeviceCommunication;

public interface IModbusRegisterClient
{
    ushort[] ReadHoldingRegisters(
        byte unitId,
        ushort startAddress,
        ushort count);

    bool[] ReadCoils(
        byte unitId,
        ushort startAddress,
        ushort count);
}
