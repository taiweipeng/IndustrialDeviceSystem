using IndustrialDeviceSystem.Core.Enums;

namespace IndustrialDeviceSystem.Core.Models;

public sealed class BookingValidationResult
{
    public bool IsValid { get; init; }

    public BookingErrorCode ErrorCode { get; init; }

    public string Message { get; init; } = string.Empty;
}
