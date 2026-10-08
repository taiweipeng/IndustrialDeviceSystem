using IndustrialDeviceSystem.Core.Interfaces;

namespace IndustrialDeviceSystem.Core.Services;

public sealed class SystemClock : IClock
{
    public DateTime Now => DateTime.UtcNow;
}
