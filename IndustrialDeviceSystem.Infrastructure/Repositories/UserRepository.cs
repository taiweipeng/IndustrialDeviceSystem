using System.Globalization;
using IndustrialDeviceSystem.Core.Enums;
using IndustrialDeviceSystem.Core.Interfaces;
using IndustrialDeviceSystem.Core.Models;
using IndustrialDeviceSystem.Infrastructure.Data;
using Microsoft.Data.Sqlite;

namespace IndustrialDeviceSystem.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly SqliteConnectionFactory
        _connectionFactory;

    public UserRepository(
        SqliteConnectionFactory connectionFactory)
    {
        ArgumentNullException.ThrowIfNull(
            connectionFactory);

        _connectionFactory = connectionFactory;
    }

    public UserAccount? GetById(int userId)
    {
        if (userId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(userId),
                "用户编号必须大于0。");
        }

        using SqliteConnection connection =
            _connectionFactory.CreateOpenConnection();

        using SqliteCommand command =
            connection.CreateCommand();

        command.CommandText = """
            SELECT UserId,
                   UserName,
                   PasswordHash,
                   Role,
                   IsEnabled,
                   CreatedAt
            FROM UserAccount
            WHERE UserId = $userId
            LIMIT 1;
            """;

        command.Parameters.AddWithValue(
            "$userId",
            userId);

        using SqliteDataReader reader =
            command.ExecuteReader();

        return reader.Read()
            ? ReadUser(reader)
            : null;
    }

    public UserAccount? GetByUserName(
        string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new ArgumentException(
                "用户名不能为空。",
                nameof(userName));
        }

        using SqliteConnection connection =
            _connectionFactory.CreateOpenConnection();

        using SqliteCommand command =
            connection.CreateCommand();

        command.CommandText = """
            SELECT UserId,
                   UserName,
                   PasswordHash,
                   Role,
                   IsEnabled,
                   CreatedAt
            FROM UserAccount
            WHERE UserName = $userName
                  COLLATE NOCASE
            LIMIT 1;
            """;

        command.Parameters.AddWithValue(
            "$userName",
            userName.Trim());

        using SqliteDataReader reader =
            command.ExecuteReader();

        return reader.Read()
            ? ReadUser(reader)
            : null;
    }

    public IReadOnlyList<UserAccount> GetAll()
    {
        using SqliteConnection connection =
            _connectionFactory.CreateOpenConnection();

        using SqliteCommand command =
            connection.CreateCommand();

        command.CommandText = """
            SELECT UserId,
                   UserName,
                   PasswordHash,
                   Role,
                   IsEnabled,
                   CreatedAt
            FROM UserAccount
            ORDER BY UserName;
            """;

        var users = new List<UserAccount>();

        using SqliteDataReader reader =
            command.ExecuteReader();

        while (reader.Read())
        {
            users.Add(ReadUser(reader));
        }

        return users;
    }

    public void Add(UserAccount user)
    {
        ArgumentNullException.ThrowIfNull(user);
        ValidateUser(user);

        using SqliteConnection connection =
            _connectionFactory.CreateOpenConnection();

        using SqliteCommand command =
            connection.CreateCommand();

        command.CommandText = """
            INSERT INTO UserAccount
            (
                UserName,
                PasswordHash,
                Role,
                IsEnabled,
                CreatedAt
            )
            VALUES
            (
                $userName,
                $passwordHash,
                $role,
                $isEnabled,
                $createdAt
            );

            SELECT last_insert_rowid();
            """;

        AddUserParameters(command, user);

        user.UserId = Convert.ToInt32(
            command.ExecuteScalar(),
            CultureInfo.InvariantCulture);
    }

    public void Update(UserAccount user)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (user.UserId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(user),
                "用户编号必须大于0。");
        }

        ValidateUser(user);

        using SqliteConnection connection =
            _connectionFactory.CreateOpenConnection();

        using SqliteCommand command =
            connection.CreateCommand();

        command.CommandText = """
            UPDATE UserAccount
            SET UserName = $userName,
                PasswordHash = $passwordHash,
                Role = $role,
                IsEnabled = $isEnabled,
                CreatedAt = $createdAt
            WHERE UserId = $userId;
            """;

        AddUserParameters(command, user);

        command.Parameters.AddWithValue(
            "$userId",
            user.UserId);

        int affectedRows =
            command.ExecuteNonQuery();

        if (affectedRows == 0)
        {
            throw new KeyNotFoundException(
                $"找不到用户：{user.UserId}。");
        }
    }

    private static void AddUserParameters(
        SqliteCommand command,
        UserAccount user)
    {
        command.Parameters.AddWithValue(
            "$userName",
            user.UserName.Trim());

        command.Parameters.AddWithValue(
            "$passwordHash",
            user.PasswordHash);

        command.Parameters.AddWithValue(
            "$role",
            (int)user.Role);

        command.Parameters.AddWithValue(
            "$isEnabled",
            user.IsEnabled ? 1 : 0);

        command.Parameters.AddWithValue(
            "$createdAt",
            SqliteValueConverter.ToText(
                user.CreatedAt));
    }

    private static UserAccount ReadUser(
        SqliteDataReader reader)
    {
        return new UserAccount
        {
            UserId = reader.GetInt32(
                reader.GetOrdinal("UserId")),

            UserName = reader.GetString(
                reader.GetOrdinal("UserName")),

            PasswordHash = reader.GetString(
                reader.GetOrdinal("PasswordHash")),

            Role = (UserRole)reader.GetInt32(
                reader.GetOrdinal("Role")),

            IsEnabled = reader.GetInt32(
                reader.GetOrdinal("IsEnabled")) != 0,

            CreatedAt =
                SqliteValueConverter.ToDateTime(
                    reader.GetString(
                        reader.GetOrdinal("CreatedAt")))
        };
    }

    private static void ValidateUser(
        UserAccount user)
    {
        if (string.IsNullOrWhiteSpace(
                user.UserName))
        {
            throw new ArgumentException(
                "用户名不能为空。",
                nameof(user));
        }

        if (string.IsNullOrWhiteSpace(
                user.PasswordHash))
        {
            throw new ArgumentException(
                "密码哈希不能为空。",
                nameof(user));
        }

        if (!Enum.IsDefined(
                typeof(UserRole),
                user.Role))
        {
            throw new ArgumentOutOfRangeException(
                nameof(user),
                "用户角色无效。");
        }

        if (user.CreatedAt == default)
        {
            throw new ArgumentException(
                "用户创建时间不能为空。",
                nameof(user));
        }
    }
}
