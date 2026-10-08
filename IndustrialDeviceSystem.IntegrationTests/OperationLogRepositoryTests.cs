using IndustrialDeviceSystem.Core.Models;
using IndustrialDeviceSystem.Infrastructure.Data;
using IndustrialDeviceSystem.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;

namespace IndustrialDeviceSystem.IntegrationTests;

public sealed class OperationLogRepositoryTests
{
    [Fact]
    public void Add_ShouldAssignIdAndPersistAllFields()
    {
        using var database =
            new OperationLogRepositoryTestDatabase();

        var repository =
            new OperationLogRepository(
                database.ConnectionFactory);

        var occurredAt = new DateTime(
            2026,
            10,
            8,
            8,
            30,
            0,
            DateTimeKind.Utc);

        var operationLog = new OperationLog
        {
            UserId = 1,
            ActionType = "CreateBooking",
            TargetType = "Booking",
            TargetId = 101,
            Detail = "创建设备预约。",
            OccurredAt = occurredAt
        };

        repository.Add(operationLog);

        Assert.True(operationLog.LogId > 0);

        var storedLog =
            Assert.Single(
                repository.GetRecent(10));

        Assert.Equal(
            operationLog.LogId,
            storedLog.LogId);
        Assert.Equal(1, storedLog.UserId);
        Assert.Equal(
            "CreateBooking",
            storedLog.ActionType);
        Assert.Equal(
            "Booking",
            storedLog.TargetType);
        Assert.Equal(101, storedLog.TargetId);
        Assert.Equal(
            "创建设备预约。",
            storedLog.Detail);
        Assert.Equal(
            occurredAt,
            storedLog.OccurredAt);
    }

    [Fact]
    public void Add_ShouldTrimTextFields()
    {
        using var database =
            new OperationLogRepositoryTestDatabase();

        var repository =
            new OperationLogRepository(
                database.ConnectionFactory);

        repository.Add(
            CreateLog(
                userId: 1,
                actionType: "  Login  ",
                targetType: "  UserAccount  ",
                targetId: 1,
                detail: "  用户登录成功。  ",
                occurredAt: CreateTime(8, 0)));

        var storedLog =
            Assert.Single(
                repository.GetRecent(10));

        Assert.Equal(
            "Login",
            storedLog.ActionType);
        Assert.Equal(
            "UserAccount",
            storedLog.TargetType);
        Assert.Equal(
            "用户登录成功。",
            storedLog.Detail);
    }

    [Fact]
    public void GetRecent_ShouldReturnNewestFirstAndRespectCount()
    {
        using var database =
            new OperationLogRepositoryTestDatabase();

        var repository =
            new OperationLogRepository(
                database.ConnectionFactory);

        repository.Add(
            CreateLog(
                1,
                "First",
                "Device",
                1,
                "第一条日志。",
                CreateTime(8, 0)));

        repository.Add(
            CreateLog(
                1,
                "Second",
                "Device",
                1,
                "第二条日志。",
                CreateTime(9, 0)));

        repository.Add(
            CreateLog(
                1,
                "Third",
                "Device",
                1,
                "第三条日志。",
                CreateTime(10, 0)));

        var logs = repository.GetRecent(2);

        Assert.Equal(2, logs.Count);
        Assert.Equal(
            new[] { "Third", "Second" },
            logs
                .Select(log => log.ActionType)
                .ToArray());
    }

    [Fact]
    public void GetRecent_SameTime_ShouldReturnLargerIdFirst()
    {
        using var database =
            new OperationLogRepositoryTestDatabase();

        var repository =
            new OperationLogRepository(
                database.ConnectionFactory);

        var sameTime = CreateTime(11, 0);

        var firstLog = CreateLog(
            1,
            "First",
            "Device",
            1,
            "先写入。",
            sameTime);

        var secondLog = CreateLog(
            1,
            "Second",
            "Device",
            1,
            "后写入。",
            sameTime);

        repository.Add(firstLog);
        repository.Add(secondLog);

        var logs = repository.GetRecent(10);

        Assert.Equal(
            new[]
            {
                secondLog.LogId,
                firstLog.LogId
            },
            logs
                .Select(log => log.LogId)
                .ToArray());
    }

    [Fact]
    public void GetByUser_ShouldReturnOnlySpecifiedUsersLogs()
    {
        using var database =
            new OperationLogRepositoryTestDatabase();

        var repository =
            new OperationLogRepository(
                database.ConnectionFactory);

        repository.Add(
            CreateLog(
                1,
                "AdminAction",
                "UserAccount",
                2,
                "管理员操作。",
                CreateTime(8, 0)));

        repository.Add(
            CreateLog(
                2,
                "OperatorAction1",
                "Booking",
                100,
                "操作员第一次操作。",
                CreateTime(9, 0)));

        repository.Add(
            CreateLog(
                2,
                "OperatorAction2",
                "Booking",
                101,
                "操作员第二次操作。",
                CreateTime(10, 0)));

        repository.Add(
            CreateLog(
                null,
                "SystemAction",
                "System",
                null,
                "系统自动操作。",
                CreateTime(11, 0)));

        var logs = repository.GetByUser(2, 10);

        Assert.Equal(2, logs.Count);
        Assert.All(
            logs,
            log => Assert.Equal(2, log.UserId));
        Assert.Equal(
            new[]
            {
                "OperatorAction2",
                "OperatorAction1"
            },
            logs
                .Select(log => log.ActionType)
                .ToArray());
    }

    [Fact]
    public void Add_SystemLog_ShouldAllowNullUserAndTargetId()
    {
        using var database =
            new OperationLogRepositoryTestDatabase();

        var repository =
            new OperationLogRepository(
                database.ConnectionFactory);

        var operationLog = CreateLog(
            null,
            "SystemStart",
            "System",
            null,
            "系统启动。",
            CreateTime(8, 0));

        repository.Add(operationLog);

        var storedLog =
            Assert.Single(
                repository.GetRecent(10));

        Assert.Null(storedLog.UserId);
        Assert.Null(storedLog.TargetId);
        Assert.Equal(
            "SystemStart",
            storedLog.ActionType);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1001)]
    public void GetRecent_InvalidCount_ShouldThrow(
        int count)
    {
        using var database =
            new OperationLogRepositoryTestDatabase();

        var repository =
            new OperationLogRepository(
                database.ConnectionFactory);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => repository.GetRecent(count));
    }

    [Fact]
    public void GetByUser_InvalidUserId_ShouldThrow()
    {
        using var database =
            new OperationLogRepositoryTestDatabase();

        var repository =
            new OperationLogRepository(
                database.ConnectionFactory);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => repository.GetByUser(0, 10));
    }

    private static OperationLog CreateLog(
        int? userId,
        string actionType,
        string targetType,
        int? targetId,
        string detail,
        DateTime occurredAt)
    {
        return new OperationLog
        {
            UserId = userId,
            ActionType = actionType,
            TargetType = targetType,
            TargetId = targetId,
            Detail = detail,
            OccurredAt = occurredAt
        };
    }

    private static DateTime CreateTime(
        int hour,
        int minute)
    {
        return new DateTime(
            2026,
            10,
            8,
            hour,
            minute,
            0,
            DateTimeKind.Utc);
    }

    private sealed class
        OperationLogRepositoryTestDatabase :
        IDisposable
    {
        private readonly string
            _temporaryDirectory;

        public SqliteConnectionFactory
            ConnectionFactory
        { get; }

        public OperationLogRepositoryTestDatabase()
        {
            _temporaryDirectory = Path.Combine(
                Path.GetTempPath(),
                "IndustrialDeviceSystemTests",
                Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(
                _temporaryDirectory);

            var databasePath = Path.Combine(
                _temporaryDirectory,
                "operation-log-repository-tests.db");

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
                        NOT NULL,

                    IsEnabled INTEGER
                        NOT NULL,

                    CreatedAt TEXT
                        NOT NULL
                );

                INSERT INTO UserAccount
                (
                    UserId,
                    UserName,
                    PasswordHash,
                    Role,
                    IsEnabled,
                    CreatedAt
                )
                VALUES
                    (1, 'admin', 'test-hash-1', 0, 1,
                     '2026-10-08T08:00:00.0000000Z'),
                    (2, 'operator01', 'test-hash-2', 1, 1,
                     '2026-10-08T08:00:00.0000000Z');

                CREATE TABLE OperationLog
                (
                    LogId INTEGER
                        PRIMARY KEY AUTOINCREMENT,

                    UserId INTEGER NULL,

                    ActionType TEXT
                        NOT NULL,

                    TargetType TEXT
                        NOT NULL,

                    TargetId INTEGER NULL,

                    Detail TEXT
                        NOT NULL,

                    OccurredAt TEXT
                        NOT NULL,

                    FOREIGN KEY (UserId)
                        REFERENCES UserAccount(UserId)
                );

                CREATE INDEX
                    IX_OperationLog_OccurredAt
                ON OperationLog(OccurredAt DESC);

                CREATE INDEX
                    IX_OperationLog_UserId_OccurredAt
                ON OperationLog(UserId, OccurredAt DESC);
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
