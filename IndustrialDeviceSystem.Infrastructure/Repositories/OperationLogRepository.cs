using System.Globalization;
using IndustrialDeviceSystem.Core.Interfaces;
using IndustrialDeviceSystem.Core.Models;
using IndustrialDeviceSystem.Infrastructure.Data;
using Microsoft.Data.Sqlite;

namespace IndustrialDeviceSystem.Infrastructure.Repositories;

public sealed class OperationLogRepository :
    IOperationLogRepository
{
    private const int MaximumQueryCount = 1000;

    private readonly SqliteConnectionFactory
        _connectionFactory;

    public OperationLogRepository(
        SqliteConnectionFactory connectionFactory)
    {
        ArgumentNullException.ThrowIfNull(
            connectionFactory);

        _connectionFactory = connectionFactory;
    }

    public void Add(
        OperationLog operationLog)
    {
        ArgumentNullException.ThrowIfNull(
            operationLog);

        ValidateOperationLog(operationLog);

        using SqliteConnection connection =
            _connectionFactory.CreateOpenConnection();

        using SqliteCommand command =
            connection.CreateCommand();

        command.CommandText =
            """
            INSERT INTO OperationLog
            (
                UserId,
                ActionType,
                TargetType,
                TargetId,
                Detail,
                OccurredAt
            )
            VALUES
            (
                $userId,
                $actionType,
                $targetType,
                $targetId,
                $detail,
                $occurredAt
            );

            SELECT last_insert_rowid();
            """;

        command.Parameters.AddWithValue(
            "$userId",
            operationLog.UserId.HasValue
                ? operationLog.UserId.Value
                : DBNull.Value);

        command.Parameters.AddWithValue(
            "$actionType",
            operationLog.ActionType.Trim());

        command.Parameters.AddWithValue(
            "$targetType",
            operationLog.TargetType.Trim());

        command.Parameters.AddWithValue(
            "$targetId",
            operationLog.TargetId.HasValue
                ? operationLog.TargetId.Value
                : DBNull.Value);

        command.Parameters.AddWithValue(
            "$detail",
            operationLog.Detail.Trim());

        command.Parameters.AddWithValue(
            "$occurredAt",
            SqliteValueConverter.ToText(
                operationLog.OccurredAt));

        operationLog.LogId =
            Convert.ToInt32(
                command.ExecuteScalar(),
                CultureInfo.InvariantCulture);
    }

    public IReadOnlyList<OperationLog> GetRecent(
        int count)
    {
        ValidateQueryCount(count);

        using SqliteConnection connection =
            _connectionFactory.CreateOpenConnection();

        using SqliteCommand command =
            connection.CreateCommand();

        command.CommandText =
            """
            SELECT LogId,
                   UserId,
                   ActionType,
                   TargetType,
                   TargetId,
                   Detail,
                   OccurredAt
            FROM OperationLog
            ORDER BY OccurredAt DESC,
                     LogId DESC
            LIMIT $count;
            """;

        command.Parameters.AddWithValue(
            "$count",
            count);

        return ExecuteQuery(command);
    }

    public IReadOnlyList<OperationLog> GetByUser(
        int userId,
        int count)
    {
        if (userId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(userId),
                "用户编号必须大于0。");
        }

        ValidateQueryCount(count);

        using SqliteConnection connection =
            _connectionFactory.CreateOpenConnection();

        using SqliteCommand command =
            connection.CreateCommand();

        command.CommandText =
            """
            SELECT LogId,
                   UserId,
                   ActionType,
                   TargetType,
                   TargetId,
                   Detail,
                   OccurredAt
            FROM OperationLog
            WHERE UserId = $userId
            ORDER BY OccurredAt DESC,
                     LogId DESC
            LIMIT $count;
            """;

        command.Parameters.AddWithValue(
            "$userId",
            userId);

        command.Parameters.AddWithValue(
            "$count",
            count);

        return ExecuteQuery(command);
    }

    private static IReadOnlyList<OperationLog>
        ExecuteQuery(
            SqliteCommand command)
    {
        var operationLogs =
            new List<OperationLog>();

        using SqliteDataReader reader =
            command.ExecuteReader();

        while (reader.Read())
        {
            operationLogs.Add(
                ReadOperationLog(reader));
        }

        return operationLogs;
    }

    private static OperationLog ReadOperationLog(
        SqliteDataReader reader)
    {
        int userIdOrdinal =
            reader.GetOrdinal("UserId");

        int targetIdOrdinal =
            reader.GetOrdinal("TargetId");

        return new OperationLog
        {
            LogId = reader.GetInt32(
                reader.GetOrdinal("LogId")),

            UserId = reader.IsDBNull(userIdOrdinal)
                ? null
                : reader.GetInt32(userIdOrdinal),

            ActionType = reader.GetString(
                reader.GetOrdinal("ActionType")),

            TargetType = reader.GetString(
                reader.GetOrdinal("TargetType")),

            TargetId = reader.IsDBNull(
                    targetIdOrdinal)
                ? null
                : reader.GetInt32(targetIdOrdinal),

            Detail = reader.GetString(
                reader.GetOrdinal("Detail")),

            OccurredAt =
                SqliteValueConverter.ToDateTime(
                    reader.GetString(
                        reader.GetOrdinal(
                            "OccurredAt")))
        };
    }

    private static void ValidateOperationLog(
        OperationLog operationLog)
    {
        if (operationLog.UserId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(operationLog),
                "用户编号必须大于0或为空。");
        }

        if (string.IsNullOrWhiteSpace(
                operationLog.ActionType))
        {
            throw new ArgumentException(
                "操作类型不能为空。",
                nameof(operationLog));
        }

        if (string.IsNullOrWhiteSpace(
                operationLog.TargetType))
        {
            throw new ArgumentException(
                "目标类型不能为空。",
                nameof(operationLog));
        }

        if (operationLog.TargetId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(operationLog),
                "目标编号必须大于0或为空。");
        }

        if (string.IsNullOrWhiteSpace(
                operationLog.Detail))
        {
            throw new ArgumentException(
                "操作详情不能为空。",
                nameof(operationLog));
        }

        if (operationLog.OccurredAt == default)
        {
            throw new ArgumentException(
                "操作时间不能为空。",
                nameof(operationLog));
        }
    }

    private static void ValidateQueryCount(
        int count)
    {
        if (count <= 0 ||
            count > MaximumQueryCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(count),
                $"查询数量必须在1到" +
                $"{MaximumQueryCount}之间。");
        }
    }
}
