using Microsoft.Data.Sqlite;

namespace IndustrialDeviceSystem.Infrastructure.Data;

public sealed class SqliteConnectionFactory
{
    private readonly string _connectionString;

    public SqliteConnectionFactory(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException(
                "数据库连接字符串不能为空。",
                nameof(connectionString));
        }

        _connectionString = connectionString;
    }

    public SqliteConnection CreateOpenConnection()
    {
        var connection =
            new SqliteConnection(_connectionString);

        connection.Open();

        using SqliteCommand command =
            connection.CreateCommand();

        command.CommandText = """
            PRAGMA foreign_keys = ON;
            PRAGMA busy_timeout = 5000;
            """;

        command.ExecuteNonQuery();

        return connection;
    }
}
