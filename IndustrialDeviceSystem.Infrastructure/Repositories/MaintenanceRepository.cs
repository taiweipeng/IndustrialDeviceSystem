using System.Globalization;
using IndustrialDeviceSystem.Core.Enums;
using IndustrialDeviceSystem.Core.Interfaces;
using IndustrialDeviceSystem.Core.Models;
using IndustrialDeviceSystem.Infrastructure.Data;
using Microsoft.Data.Sqlite;

namespace IndustrialDeviceSystem.Infrastructure.Repositories;

public sealed class MaintenanceRepository : IMaintenanceRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public MaintenanceRepository(
        SqliteConnectionFactory connectionFactory)
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);
        _connectionFactory = connectionFactory;
    }

    public IReadOnlyList<MaintenanceOrder> GetActiveOrders(
        int deviceId)
    {
        using SqliteConnection connection =
            _connectionFactory.CreateOpenConnection();

        using SqliteCommand command = connection.CreateCommand();

        command.CommandText = """
            SELECT OrderId, DeviceId, SourceAlarmId, AssigneeId,
                   Description, Result, Status, CreatedAt,
                   PlannedStartTime, PlannedEndTime,
                   StartedAt, CompletedAt
            FROM MaintenanceOrder
            WHERE DeviceId = $deviceId
              AND Status IN
              (
                  $created, $assigned,
                  $inProgress, $completed
              )
            ORDER BY CreatedAt, OrderId;
            """;

        command.Parameters.AddWithValue("$deviceId", deviceId);
        command.Parameters.AddWithValue(
            "$created",
            (int)MaintenanceStatus.Created);
        command.Parameters.AddWithValue(
            "$assigned",
            (int)MaintenanceStatus.Assigned);
        command.Parameters.AddWithValue(
            "$inProgress",
            (int)MaintenanceStatus.InProgress);
        command.Parameters.AddWithValue(
            "$completed",
            (int)MaintenanceStatus.Completed);

        var orders = new List<MaintenanceOrder>();

        using SqliteDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
            orders.Add(ReadOrder(reader));
        }

        return orders;
    }

    public void Add(MaintenanceOrder order)
    {
        ArgumentNullException.ThrowIfNull(order);

        using SqliteConnection connection =
            _connectionFactory.CreateOpenConnection();

        using SqliteCommand command = connection.CreateCommand();

        command.CommandText = """
            INSERT INTO MaintenanceOrder
            (
                DeviceId, SourceAlarmId, AssigneeId,
                Description, Result, Status, CreatedAt,
                PlannedStartTime, PlannedEndTime,
                StartedAt, CompletedAt
            )
            VALUES
            (
                $deviceId, $sourceAlarmId, $assigneeId,
                $description, $result, $status, $createdAt,
                $plannedStartTime, $plannedEndTime,
                $startedAt, $completedAt
            );

            SELECT last_insert_rowid();
            """;

        AddParameters(command, order);

        order.OrderId = Convert.ToInt32(
            command.ExecuteScalar(),
            CultureInfo.InvariantCulture);
    }

    public void Update(MaintenanceOrder order)
    {
        ArgumentNullException.ThrowIfNull(order);

        using SqliteConnection connection =
            _connectionFactory.CreateOpenConnection();

        using SqliteCommand command = connection.CreateCommand();

        command.CommandText = """
            UPDATE MaintenanceOrder
            SET DeviceId = $deviceId,
                SourceAlarmId = $sourceAlarmId,
                AssigneeId = $assigneeId,
                Description = $description,
                Result = $result,
                Status = $status,
                CreatedAt = $createdAt,
                PlannedStartTime = $plannedStartTime,
                PlannedEndTime = $plannedEndTime,
                StartedAt = $startedAt,
                CompletedAt = $completedAt
            WHERE OrderId = $orderId;
            """;

        AddParameters(command, order);
        command.Parameters.AddWithValue(
            "$orderId",
            order.OrderId);

        if (command.ExecuteNonQuery() == 0)
        {
            throw new InvalidOperationException(
                $"未找到维护工单：{order.OrderId}");
        }
    }

    private static void AddParameters(
        SqliteCommand command,
        MaintenanceOrder order)
    {
        command.Parameters.AddWithValue(
            "$deviceId",
            order.DeviceId);
        command.Parameters.AddWithValue(
            "$sourceAlarmId",
            SqliteValueConverter.ToValue(order.SourceAlarmId));
        command.Parameters.AddWithValue(
            "$assigneeId",
            SqliteValueConverter.ToValue(order.AssigneeId));
        command.Parameters.AddWithValue(
            "$description",
            order.Description);
        command.Parameters.AddWithValue(
            "$result",
            order.Result);
        command.Parameters.AddWithValue(
            "$status",
            (int)order.Status);
        command.Parameters.AddWithValue(
            "$createdAt",
            SqliteValueConverter.ToText(order.CreatedAt));
        command.Parameters.AddWithValue(
            "$plannedStartTime",
            SqliteValueConverter.ToValue(order.PlannedStartTime));
        command.Parameters.AddWithValue(
            "$plannedEndTime",
            SqliteValueConverter.ToValue(order.PlannedEndTime));
        command.Parameters.AddWithValue(
            "$startedAt",
            SqliteValueConverter.ToValue(order.StartedAt));
        command.Parameters.AddWithValue(
            "$completedAt",
            SqliteValueConverter.ToValue(order.CompletedAt));
    }

    private static MaintenanceOrder ReadOrder(
        SqliteDataReader reader)
    {
        int resultIndex = reader.GetOrdinal("Result");

        return new MaintenanceOrder
        {
            OrderId = reader.GetInt32(
                reader.GetOrdinal("OrderId")),
            DeviceId = reader.GetInt32(
                reader.GetOrdinal("DeviceId")),
            SourceAlarmId =
                SqliteValueConverter.ReadNullableInt32(
                    reader,
                    "SourceAlarmId"),
            AssigneeId =
                SqliteValueConverter.ReadNullableInt32(
                    reader,
                    "AssigneeId"),
            Description = reader.GetString(
                reader.GetOrdinal("Description")),
            Result = reader.IsDBNull(resultIndex)
                ? string.Empty
                : reader.GetString(resultIndex),
            Status = (MaintenanceStatus)reader.GetInt32(
                reader.GetOrdinal("Status")),
            CreatedAt = SqliteValueConverter.ToDateTime(
                reader.GetString(reader.GetOrdinal("CreatedAt"))),
            PlannedStartTime =
                SqliteValueConverter.ReadNullableDateTime(
                    reader,
                    "PlannedStartTime"),
            PlannedEndTime =
                SqliteValueConverter.ReadNullableDateTime(
                    reader,
                    "PlannedEndTime"),
            StartedAt =
                SqliteValueConverter.ReadNullableDateTime(
                    reader,
                    "StartedAt"),
            CompletedAt =
                SqliteValueConverter.ReadNullableDateTime(
                    reader,
                    "CompletedAt")
        };
    }
}
