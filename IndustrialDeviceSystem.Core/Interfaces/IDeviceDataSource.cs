using IndustrialDeviceSystem.Core.Models;

namespace IndustrialDeviceSystem.Core.Interfaces;

public interface IDeviceDataSource
{
    DeviceSnapshot ReadSnapshot(string deviceCode);
}
