using IndustrialDeviceSystem.Core.Enums;
using IndustrialDeviceSystem.Core.Interfaces;
using IndustrialDeviceSystem.Core.Models;

namespace IndustrialDeviceSystem.Core.Services;

public sealed class UserAccountService
{
    private readonly IUserRepository
        _userRepository;

    private readonly IPasswordHasher
        _passwordHasher;

    private readonly AuthorizationService
        _authorizationService;

    private readonly IClock _clock;

    public UserAccountService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        AuthorizationService authorizationService,
        IClock clock)
    {
        ArgumentNullException.ThrowIfNull(
            userRepository);

        ArgumentNullException.ThrowIfNull(
            passwordHasher);

        ArgumentNullException.ThrowIfNull(
            authorizationService);

        ArgumentNullException.ThrowIfNull(clock);

        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _authorizationService =
            authorizationService;
        _clock = clock;
    }

    public void ChangeOwnPassword(
        UserSession session,
        string currentPassword,
        string newPassword)
    {
        ArgumentNullException.ThrowIfNull(session);

        var user =
            _userRepository.GetById(
                session.UserId)
            ?? throw new InvalidOperationException(
                "当前登录账号不存在。");

        if (!user.IsEnabled)
        {
            throw new UnauthorizedAccessException(
                "当前账号已经被禁用。");
        }

        if (!_passwordHasher.VerifyPassword(
                currentPassword,
                user.PasswordHash))
        {
            throw new InvalidOperationException(
                "当前密码不正确。");
        }

        ValidatePassword(newPassword);

        if (string.Equals(
                currentPassword,
                newPassword,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "新密码不能与当前密码相同。");
        }

        user.PasswordHash =
            _passwordHasher.HashPassword(
                newPassword);

        _userRepository.Update(user);
    }

    public UserAccount CreateUser(
        UserSession actor,
        string userName,
        string initialPassword,
        UserRole role)
    {
        EnsureManageUsers(actor);
        ValidateUserName(userName);
        ValidatePassword(initialPassword);
        ValidateRole(role);

        var normalizedUserName =
            userName.Trim();

        if (_userRepository.GetByUserName(
                normalizedUserName) is not null)
        {
            throw new InvalidOperationException(
                $"用户名“{normalizedUserName}”" +
                "已经存在。");
        }

        var user = new UserAccount
        {
            UserName = normalizedUserName,

            PasswordHash =
                _passwordHasher.HashPassword(
                    initialPassword),

            Role = role,
            IsEnabled = true,
            CreatedAt = _clock.Now
        };

        _userRepository.Add(user);

        return user;
    }

    public void ResetPassword(
        UserSession actor,
        int userId,
        string newPassword)
    {
        EnsureManageUsers(actor);
        ValidatePassword(newPassword);

        var user = GetRequiredUser(userId);

        user.PasswordHash =
            _passwordHasher.HashPassword(
                newPassword);

        _userRepository.Update(user);
    }

    public void SetEnabled(
        UserSession actor,
        int userId,
        bool isEnabled)
    {
        EnsureManageUsers(actor);

        var user = GetRequiredUser(userId);

        if (!isEnabled &&
            user.UserId == actor.UserId)
        {
            throw new InvalidOperationException(
                "不能禁用当前登录账号。");
        }

        if (!isEnabled)
        {
            EnsureNotLastEnabledAdministrator(
                user);
        }

        user.IsEnabled = isEnabled;

        _userRepository.Update(user);
    }

    public void ChangeRole(
        UserSession actor,
        int userId,
        UserRole newRole)
    {
        EnsureManageUsers(actor);
        ValidateRole(newRole);

        var user = GetRequiredUser(userId);

        if (user.UserId == actor.UserId &&
            user.Role != newRole)
        {
            throw new InvalidOperationException(
                "不能修改当前登录账号的角色。");
        }

        if (user.Role ==
                UserRole.Administrator &&
            newRole !=
                UserRole.Administrator)
        {
            EnsureNotLastEnabledAdministrator(
                user);
        }

        user.Role = newRole;

        _userRepository.Update(user);
    }

    private UserAccount GetRequiredUser(
        int userId)
    {
        if (userId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(userId),
                "用户编号必须大于0。");
        }

        return _userRepository.GetById(userId)
            ?? throw new KeyNotFoundException(
                $"找不到用户：{userId}。");
    }

    private void EnsureManageUsers(
        UserSession actor)
    {
        ArgumentNullException.ThrowIfNull(actor);

        _authorizationService.EnsurePermission(
            actor,
            SystemPermission.ManageUsers);
    }

    private void
        EnsureNotLastEnabledAdministrator(
            UserAccount targetUser)
    {
        if (targetUser.Role !=
                UserRole.Administrator ||
            !targetUser.IsEnabled)
        {
            return;
        }

        var otherEnabledAdministratorExists =
            _userRepository
                .GetAll()
                .Any(user =>
                    user.UserId !=
                        targetUser.UserId &&
                    user.Role ==
                        UserRole.Administrator &&
                    user.IsEnabled);

        if (!otherEnabledAdministratorExists)
        {
            throw new InvalidOperationException(
                "系统必须至少保留一个启用的管理员账号。");
        }
    }

    private static void ValidateUserName(
        string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new ArgumentException(
                "用户名不能为空。",
                nameof(userName));
        }

        var normalized =
            userName.Trim();

        if (normalized.Length < 3 ||
            normalized.Length > 50)
        {
            throw new ArgumentException(
                "用户名长度必须在3到50个字符之间。",
                nameof(userName));
        }

        if (normalized.Any(
                char.IsWhiteSpace))
        {
            throw new ArgumentException(
                "用户名中不能包含空白字符。",
                nameof(userName));
        }
    }

    private static void ValidatePassword(
        string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            throw new ArgumentException(
                "密码不能为空。",
                nameof(password));
        }

        if (password.Length < 8 ||
            password.Length > 128)
        {
            throw new ArgumentException(
                "密码长度必须在8到128个字符之间。",
                nameof(password));
        }

        if (!password.Any(char.IsUpper) ||
            !password.Any(char.IsLower) ||
            !password.Any(char.IsDigit) ||
            !password.Any(
                character =>
                    !char.IsLetterOrDigit(
                        character)))
        {
            throw new ArgumentException(
                "密码必须同时包含大写字母、" +
                "小写字母、数字和特殊字符。",
                nameof(password));
        }
    }

    private static void ValidateRole(
        UserRole role)
    {
        if (!Enum.IsDefined(
                typeof(UserRole),
                role))
        {
            throw new ArgumentOutOfRangeException(
                nameof(role),
                "用户角色无效。");
        }
    }
}
