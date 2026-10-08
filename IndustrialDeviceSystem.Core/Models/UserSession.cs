using IndustrialDeviceSystem.Core.Enums;

namespace IndustrialDeviceSystem.Core.Models;

public sealed class UserSession
{
    public UserSession(
        int userId,
        string userName,
        UserRole role)
    {
        if (userId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(userId),
                "用户编号必须大于0。");
        }

        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new ArgumentException(
                "用户名不能为空。",
                nameof(userName));
        }

        if (!Enum.IsDefined(
                typeof(UserRole),
                role))
        {
            throw new ArgumentOutOfRangeException(
                nameof(role),
                "用户角色无效。");
        }

        UserId = userId;
        UserName = userName.Trim();
        Role = role;
    }

    public int UserId { get; }

    public string UserName { get; }

    public UserRole Role { get; }
}
