using IndustrialDeviceSystem.Core.Enums;

namespace IndustrialDeviceSystem.Core.Models;

public sealed class UserAccount
{
    public int UserId { get; set; }

    public string UserName { get; set; } =
        string.Empty;

    public string PasswordHash { get; set; } =
        string.Empty;

    public UserRole Role { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime CreatedAt { get; set; }
}
