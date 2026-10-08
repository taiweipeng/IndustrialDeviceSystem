using IndustrialDeviceSystem.Core.Enums;
using IndustrialDeviceSystem.Core.Models;
using IndustrialDeviceSystem.Infrastructure.Data;
using IndustrialDeviceSystem.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;

namespace IndustrialDeviceSystem.IntegrationTests;

public sealed class UserRepositoryTests
{
    [Fact]
    public void Add_ShouldAssignIdAndPersistAllFields()
    {
        using var database =
            new UserRepositoryTestDatabase();

        var repository =
            new UserRepository(
                database.ConnectionFactory);

        var createdAt = new DateTime(
            2026,
            10,
            6,
            10,
            30,
            0,
            DateTimeKind.Utc);

        var user = new UserAccount
        {
            UserName = "operator02",
            PasswordHash = "test-password-hash-v1",
            Role = UserRole.Operator,
            IsEnabled = true,
            CreatedAt = createdAt
        };

        repository.Add(user);

        Assert.True(user.UserId > 0);

        var storedUser =
            repository.GetById(user.UserId);

        Assert.NotNull(storedUser);
        Assert.Equal(
            user.UserId,
            storedUser!.UserId);
        Assert.Equal(
            "operator02",
            storedUser.UserName);
        Assert.Equal(
            "test-password-hash-v1",
            storedUser.PasswordHash);
        Assert.Equal(
            UserRole.Operator,
            storedUser.Role);
        Assert.True(storedUser.IsEnabled);
        Assert.Equal(
            createdAt,
            storedUser.CreatedAt);
    }

    [Fact]
    public void GetByUserName_ShouldIgnoreCase()
    {
        using var database =
            new UserRepositoryTestDatabase();

        var repository =
            new UserRepository(
                database.ConnectionFactory);

        var user = CreateUser(
            "Maintainer02",
            UserRole.Maintainer);

        repository.Add(user);

        var storedUser =
            repository.GetByUserName(
                "maintainer02");

        Assert.NotNull(storedUser);
        Assert.Equal(
            user.UserId,
            storedUser!.UserId);
        Assert.Equal(
            "Maintainer02",
            storedUser.UserName);
    }

    [Fact]
    public void Update_ShouldPersistChanges()
    {
        using var database =
            new UserRepositoryTestDatabase();

        var repository =
            new UserRepository(
                database.ConnectionFactory);

        var user = CreateUser(
            "temporary-user",
            UserRole.Operator);

        repository.Add(user);

        user.PasswordHash =
            "test-password-hash-v2";

        user.Role =
            UserRole.Maintainer;

        user.IsEnabled = false;

        repository.Update(user);

        var storedUser =
            repository.GetById(user.UserId);

        Assert.NotNull(storedUser);
        Assert.Equal(
            "test-password-hash-v2",
            storedUser!.PasswordHash);
        Assert.Equal(
            UserRole.Maintainer,
            storedUser.Role);
        Assert.False(storedUser.IsEnabled);
    }

    [Fact]
    public void GetAll_ShouldOrderByUserName()
    {
        using var database =
            new UserRepositoryTestDatabase();

        var repository =
            new UserRepository(
                database.ConnectionFactory);

        repository.Add(
            CreateUser(
                "charlie",
                UserRole.Auditor));

        repository.Add(
            CreateUser(
                "Alpha",
                UserRole.Administrator));

        repository.Add(
            CreateUser(
                "bravo",
                UserRole.Operator));

        var users = repository.GetAll();

        Assert.Equal(
            new[]
            {
                "Alpha",
                "bravo",
                "charlie"
            },
            users
                .Select(user => user.UserName)
                .ToArray());
    }

    [Fact]
    public void GetById_UnknownUser_ShouldReturnNull()
    {
        using var database =
            new UserRepositoryTestDatabase();

        var repository =
            new UserRepository(
                database.ConnectionFactory);

        var user =
            repository.GetById(99999);

        Assert.Null(user);
    }

    private static UserAccount CreateUser(
        string userName,
        UserRole role)
    {
        return new UserAccount
        {
            UserName = userName,
            PasswordHash =
                $"test-hash-{userName}",
            Role = role,
            IsEnabled = true,
            CreatedAt = new DateTime(
                2026,
                10,
                6,
                8,
                0,
                0,
                DateTimeKind.Utc)
        };
    }

    private sealed class UserRepositoryTestDatabase :
        IDisposable
    {
        private readonly string
            _temporaryDirectory;

        public SqliteConnectionFactory
            ConnectionFactory
        { get; }

        public UserRepositoryTestDatabase()
        {
            _temporaryDirectory = Path.Combine(
                Path.GetTempPath(),
                "IndustrialDeviceSystemTests",
                Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(
                _temporaryDirectory);

            var databasePath = Path.Combine(
                _temporaryDirectory,
                "user-repository-tests.db");

            ConnectionFactory =
                new SqliteConnectionFactory(
                    $"Data Source={databasePath}");

            CreateSchema();
        }

        private void CreateSchema()
        {
            using var connection =
                ConnectionFactory
                    .CreateOpenConnection();

            using var command =
                connection.CreateCommand();

            command.CommandText =
                """
                CREATE TABLE UserAccount
                (
                    UserId INTEGER
                        PRIMARY KEY AUTOINCREMENT,

                    UserName TEXT
                        NOT NULL
                        COLLATE NOCASE
                        UNIQUE,

                    PasswordHash TEXT
                        NOT NULL,

                    Role INTEGER
                        NOT NULL
                        CHECK (Role BETWEEN 0 AND 3),

                    IsEnabled INTEGER
                        NOT NULL
                        DEFAULT 1
                        CHECK (IsEnabled IN (0, 1)),

                    CreatedAt TEXT
                        NOT NULL
                );
                """;

            command.ExecuteNonQuery();
        }

        public void Dispose()
        {
            SqliteConnection.ClearAllPools();

            if (Directory.Exists(
                    _temporaryDirectory))
            {
                Directory.Delete(
                    _temporaryDirectory,
                    recursive: true);
            }
        }
    }
}
