using IndustrialDeviceSystem.Core.Enums;
using IndustrialDeviceSystem.Core.Interfaces;
using IndustrialDeviceSystem.Core.Models;
using IndustrialDeviceSystem.Core.Services;

namespace IndustrialDeviceSystem.IntegrationTests;

public sealed class UserAccountServiceTests
{
    [Fact]
    public void ChangeOwnPassword_ValidPassword_ShouldUpdateHash()
    {
        var repository = CreateRepository(
            CreateUser(
                2,
                "operator01",
                UserRole.Operator,
                "Current@2026"));

        var service =
            CreateService(repository);

        var session =
            CreateSession(
                2,
                "operator01",
                UserRole.Operator);

        service.ChangeOwnPassword(
            session,
            "Current@2026",
            "Changed@2027");

        var updated =
            repository.GetById(2);

        Assert.NotNull(updated);
        Assert.Equal(
            "HASH::Changed@2027",
            updated!.PasswordHash);
    }

    [Fact]
    public void ChangeOwnPassword_WrongCurrentPassword_ShouldThrow()
    {
        var repository = CreateRepository(
            CreateUser(
                2,
                "operator01",
                UserRole.Operator,
                "Current@2026"));

        var service =
            CreateService(repository);

        var session =
            CreateSession(
                2,
                "operator01",
                UserRole.Operator);

        Assert.Throws<InvalidOperationException>(
            () =>
                service.ChangeOwnPassword(
                    session,
                    "Wrong@2026",
                    "Changed@2027"));
    }

    [Fact]
    public void ChangeOwnPassword_WeakNewPassword_ShouldThrow()
    {
        var repository = CreateRepository(
            CreateUser(
                2,
                "operator01",
                UserRole.Operator,
                "Current@2026"));

        var service =
            CreateService(repository);

        var session =
            CreateSession(
                2,
                "operator01",
                UserRole.Operator);

        Assert.Throws<ArgumentException>(
            () =>
                service.ChangeOwnPassword(
                    session,
                    "Current@2026",
                    "12345678"));
    }

    [Fact]
    public void CreateUser_Administrator_ShouldAddEnabledUser()
    {
        var repository = CreateRepository(
            CreateUser(
                1,
                "admin",
                UserRole.Administrator,
                "Admin@2026"));

        var service =
            CreateService(repository);

        var actor =
            CreateSession(
                1,
                "admin",
                UserRole.Administrator);

        var createdUser =
            service.CreateUser(
                actor,
                "operator02",
                "Operator@2027",
                UserRole.Operator);

        Assert.True(createdUser.UserId > 0);
        Assert.True(createdUser.IsEnabled);
        Assert.Equal(
            UserRole.Operator,
            createdUser.Role);
        Assert.Equal(
            "HASH::Operator@2027",
            createdUser.PasswordHash);

        Assert.NotNull(
            repository.GetByUserName(
                "OPERATOR02"));
    }

    [Fact]
    public void CreateUser_NonAdministrator_ShouldThrow()
    {
        var repository = CreateRepository(
            CreateUser(
                2,
                "operator01",
                UserRole.Operator,
                "Operator@2026"));

        var service =
            CreateService(repository);

        var actor =
            CreateSession(
                2,
                "operator01",
                UserRole.Operator);

        Assert.Throws<UnauthorizedAccessException>(
            () =>
                service.CreateUser(
                    actor,
                    "operator02",
                    "Operator@2027",
                    UserRole.Operator));
    }

    [Fact]
    public void ResetPassword_Administrator_ShouldUpdateHash()
    {
        var repository = CreateRepository(
            CreateUser(
                1,
                "admin",
                UserRole.Administrator,
                "Admin@2026"),
            CreateUser(
                2,
                "operator01",
                UserRole.Operator,
                "Operator@2026"));

        var service =
            CreateService(repository);

        var actor =
            CreateSession(
                1,
                "admin",
                UserRole.Administrator);

        service.ResetPassword(
            actor,
            2,
            "Reset@2027");

        Assert.Equal(
            "HASH::Reset@2027",
            repository.GetById(2)!
                .PasswordHash);
    }

    [Fact]
    public void SetEnabled_DisableCurrentAccount_ShouldThrow()
    {
        var repository = CreateRepository(
            CreateUser(
                1,
                "admin",
                UserRole.Administrator,
                "Admin@2026"));

        var service =
            CreateService(repository);

        var actor =
            CreateSession(
                1,
                "admin",
                UserRole.Administrator);

        Assert.Throws<InvalidOperationException>(
            () =>
                service.SetEnabled(
                    actor,
                    1,
                    false));
    }

    [Fact]
    public void SetEnabled_DisableLastAdministrator_ShouldThrow()
    {
        var repository = CreateRepository(
            CreateUser(
                1,
                "admin",
                UserRole.Administrator,
                "Admin@2026"));

        var service =
            CreateService(repository);

        // 模拟另一个管理员会话执行管理操作，
        // 但数据库中只有一个启用管理员。
        var actor =
            CreateSession(
                99,
                "security-admin",
                UserRole.Administrator);

        Assert.Throws<InvalidOperationException>(
            () =>
                service.SetEnabled(
                    actor,
                    1,
                    false));

        Assert.True(
            repository.GetById(1)!
                .IsEnabled);
    }

    [Fact]
    public void SetEnabled_WithAnotherAdministrator_ShouldAllowDisable()
    {
        var repository = CreateRepository(
            CreateUser(
                1,
                "admin01",
                UserRole.Administrator,
                "Admin01@2026"),
            CreateUser(
                2,
                "admin02",
                UserRole.Administrator,
                "Admin02@2026"));

        var service =
            CreateService(repository);

        var actor =
            CreateSession(
                1,
                "admin01",
                UserRole.Administrator);

        service.SetEnabled(
            actor,
            2,
            false);

        Assert.False(
            repository.GetById(2)!
                .IsEnabled);
    }

    [Fact]
    public void ChangeRole_CurrentAdministrator_ShouldThrow()
    {
        var repository = CreateRepository(
            CreateUser(
                1,
                "admin",
                UserRole.Administrator,
                "Admin@2026"),
            CreateUser(
                2,
                "admin02",
                UserRole.Administrator,
                "Admin02@2026"));

        var service =
            CreateService(repository);

        var actor =
            CreateSession(
                1,
                "admin",
                UserRole.Administrator);

        Assert.Throws<InvalidOperationException>(
            () =>
                service.ChangeRole(
                    actor,
                    1,
                    UserRole.Operator));
    }

    private static UserAccountService CreateService(
        IUserRepository repository)
    {
        return new UserAccountService(
            repository,
            new FakePasswordHasher(),
            new AuthorizationService(),
            new FakeClock());
    }

    private sealed class FakeClock : IClock
    {
        public DateTime Now =>
            new(
                2026,
                10,
                7,
                8,
                30,
                0,
                DateTimeKind.Local);
    }

    private static FakeUserRepository
        CreateRepository(
            params UserAccount[] users)
    {
        return new FakeUserRepository(users);
    }

    private static UserAccount CreateUser(
        int userId,
        string userName,
        UserRole role,
        string password,
        bool isEnabled = true)
    {
        return new UserAccount
        {
            UserId = userId,
            UserName = userName,
            PasswordHash =
                $"HASH::{password}",
            Role = role,
            IsEnabled = isEnabled,
            CreatedAt = new DateTime(
                2026,
                10,
                7,
                8,
                0,
                0,
                DateTimeKind.Utc)
        };
    }

    private static UserSession CreateSession(
     int userId,
     string userName,
     UserRole role)
    {
        return new UserSession(
            userId,
            userName,
            role);
    }

    private sealed class FakePasswordHasher :
        IPasswordHasher
    {
        public string HashPassword(
            string password)
        {
            return $"HASH::{password}";
        }

        public bool VerifyPassword(
            string password,
            string storedPasswordHash)
        {
            return string.Equals(
                HashPassword(password),
                storedPasswordHash,
                StringComparison.Ordinal);
        }
    }

    private sealed class FakeUserRepository :
        IUserRepository
    {
        private readonly List<UserAccount>
            _users;

        public FakeUserRepository(
            IEnumerable<UserAccount> users)
        {
            _users = users
                .Select(Clone)
                .ToList();
        }

        public UserAccount? GetById(
            int userId)
        {
            var user = _users
                .SingleOrDefault(
                    item =>
                        item.UserId == userId);

            return user is null
                ? null
                : Clone(user);
        }

        public UserAccount? GetByUserName(
            string userName)
        {
            var user = _users
                .SingleOrDefault(
                    item =>
                        string.Equals(
                            item.UserName,
                            userName,
                            StringComparison
                                .OrdinalIgnoreCase));

            return user is null
                ? null
                : Clone(user);
        }

        public IReadOnlyList<UserAccount>
            GetAll()
        {
            return _users
                .OrderBy(user => user.UserName)
                .Select(Clone)
                .ToList();
        }

        public void Add(
            UserAccount user)
        {
            if (_users.Any(
                    item =>
                        string.Equals(
                            item.UserName,
                            user.UserName,
                            StringComparison
                                .OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException(
                    "用户名已经存在。");
            }

            user.UserId =
                _users.Count == 0
                    ? 1
                    : _users.Max(
                        item => item.UserId) + 1;

            _users.Add(Clone(user));
        }

        public void Update(
            UserAccount user)
        {
            var index = _users.FindIndex(
                item =>
                    item.UserId == user.UserId);

            if (index < 0)
            {
                throw new KeyNotFoundException(
                    "用户不存在。");
            }

            _users[index] = Clone(user);
        }

        private static UserAccount Clone(
            UserAccount user)
        {
            return new UserAccount
            {
                UserId = user.UserId,
                UserName = user.UserName,
                PasswordHash =
                    user.PasswordHash,
                Role = user.Role,
                IsEnabled = user.IsEnabled,
                CreatedAt = user.CreatedAt
            };
        }
    }
}
