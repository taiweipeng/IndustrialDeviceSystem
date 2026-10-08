using System.Globalization;
using IndustrialDeviceSystem.Core.Enums;
using IndustrialDeviceSystem.Core.Interfaces;
using IndustrialDeviceSystem.Core.Models;
using IndustrialDeviceSystem.Infrastructure.Data;
using Microsoft.Data.Sqlite;

namespace IndustrialDeviceSystem.Infrastructure.Repositories;

public sealed class AlarmRepository : IAlarmRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public AlarmRepository(SqliteConnectionFactory connectionFactory)
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);
        _connectionFactory = connectionFactory;
    }

    public Alarm? GetActiveAlarm(int deviceId, string alarmType)
    {
        using SqliteConnection connection =
            _connectionFactory.CreateOpenConnection();

        using SqliteCommand command = connection.CreateCommand();

        command.CommandText = """
            SELECT AlarmId, DeviceId, AlarmType, Level,
                   Message, Status, OccurredAt, LastOccurredAt,
                   OccurrenceCount, AcknowledgedBy,
                   AcknowledgedAt, ClearedAt
            FROM Alarm
            WHERE DeviceId = $deviceId
              AND AlarmType = $alarmType
              AND Status IN ($active, $acknowledged)
            ORDER BY OccurredAt DESC
            LIMIT 1;
            """;

        command.Parameters.AddWithValue("$deviceId", deviceId);
        command.Parameters.AddWithValue("$alarmType", alarmType);
        command.Parameters.AddWithValue(
            "$active",
            (int)AlarmStatus.Active);
        command.Parameters.AddWithValue(
            "$acknowledged",
            (int)AlarmStatus.Acknowledged);

        using SqliteDataReader reader = command.ExecuteReader();

        return reader.Read() ? ReadAlarm(reader) : null;
    }

    public void Add(Alarm alarm)
    {
        ArgumentNullException.ThrowIfNull(alarm);

        using SqliteConnection connection =
            _connectionFactory.CreateOpenConnection();

        using SqliteCommand command = connection.CreateCommand();

        command.CommandText = """
            INSERT INTO Alarm
            (
                DeviceId, AlarmType, Level, Message, Status,
                OccurredAt, LastOccurredAt, OccurrenceCount,
                AcknowledgedBy, AcknowledgedAt, ClearedAt
            )
            VALUES
            (
                $deviceId, $alarmType, $level, $message, $status,
                $occurredAt, $lastOccurredAt, $occurrenceCount,
                $acknowledgedBy, $acknowledgedAt, $clearedAt
            );

            SELECT last_insert_rowid();
            """;

        AddParameters(command, alarm);

        alarm.AlarmId = Convert.ToInt32(
            command.ExecuteScalar(),
            CultureInfo.InvariantCulture);
    }

    public void Update(Alarm alarm)
    {
        ArgumentNullException.ThrowIfNull(alarm);

        using SqliteConnection connection =
            _connectionFactory.CreateOpenConnection();

        using SqliteCommand command = connection.CreateCommand();

        command.CommandText = """
            UPDATE Alarm
            SET DeviceId = $deviceId,
                AlarmType = $alarmType,
                Level = $level,
                Message = $message,
                Status = $status,
                OccurredAt = $occurredAt,
                LastOccurredAt = $lastOccurredAt,
                OccurrenceCount = $occurrenceCount,
                AcknowledgedBy = $acknowledgedBy,
                AcknowledgedAt = $acknowledgedAt,
                ClearedAt = $clearedAt
            WHERE AlarmId = $alarmId;
            """;

        AddParameters(command, alarm);
        command.Parameters.AddWithValue(
            "$alarmId",
            alarm.AlarmId);

        if (command.ExecuteNonQuery() == 0)
        {
            throw new InvalidOperationException(
                $"未找到报警记录：{alarm.AlarmId}");
        }
    }

    private static void AddParameters(
        SqliteCommand command,
        Alarm alarm)
    {
        command.Parameters.AddWithValue(
            "$deviceId",
            alarm.DeviceId);
        command.Parameters.AddWithValue(
            "$alarmType",
            alarm.AlarmType);
        command.Parameters.AddWithValue(
            "$level",
            (int)alarm.Level);
        command.Parameters.AddWithValue(
            "$message",
            alarm.Message);
        command.Parameters.AddWithValue(
            "$status",
            (int)alarm.Status);
        command.Parameters.AddWithValue(
            "$occurredAt",
            SqliteValueConverter.ToText(alarm.OccurredAt));
        command.Parameters.AddWithValue(
            "$lastOccurredAt",
            SqliteValueConverter.ToText(alarm.LastOccurredAt));
        command.Parameters.AddWithValue(
            "$occurrenceCount",
            alarm.OccurrenceCount);
        command.Parameters.AddWithValue(
            "$acknowledgedBy",
            SqliteValueConverter.ToValue(alarm.AcknowledgedBy));
        command.Parameters.AddWithValue(
            "$acknowledgedAt",
            SqliteValueConverter.ToValue(alarm.AcknowledgedAt));
        command.Parameters.AddWithValue(
            "$clearedAt",
            SqliteValueConverter.ToValue(alarm.ClearedAt));
    }

    private static Alarm ReadAlarm(SqliteDataReader reader)
    {
        return new Alarm
        {
            AlarmId = reader.GetInt32(
                reader.GetOrdinal("AlarmId")),
            DeviceId = reader.GetInt32(
                reader.GetOrdinal("DeviceId")),
            AlarmType = reader.GetString(
                reader.GetOrdinal("AlarmType")),
            Level = (AlarmLevel)reader.GetInt32(
                reader.GetOrdinal("Level")),
            Message = reader.GetString(
                reader.GetOrdinal("Message")),
            Status = (AlarmStatus)reader.GetInt32(
                reader.GetOrdinal("Status")),
            OccurredAt = SqliteValueConverter.ToDateTime(
                reader.GetString(reader.GetOrdinal("OccurredAt"))),
            LastOccurredAt = SqliteValueConverter.ToDateTime(
                reader.GetString(reader.GetOrdinal("LastOccurredAt"))),
            OccurrenceCount = reader.GetInt32(
                reader.GetOrdinal("OccurrenceCount")),
            AcknowledgedBy =
                SqliteValueConverter.ReadNullableInt32(
                    reader,
                    "AcknowledgedBy"),
            AcknowledgedAt =
                SqliteValueConverter.ReadNullableDateTime(
                    reader,
                    "AcknowledgedAt"),
            ClearedAt =
                SqliteValueConverter.ReadNullableDateTime(
                    reader,
                    "ClearedAt")
        };
    }
}
