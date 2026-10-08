using System.Globalization;
using IndustrialDeviceSystem.Core.Enums;
using IndustrialDeviceSystem.Core.Interfaces;
using IndustrialDeviceSystem.Core.Models;
using IndustrialDeviceSystem.Infrastructure.Data;
using Microsoft.Data.Sqlite;

namespace IndustrialDeviceSystem.Infrastructure.Repositories;

public sealed class DeviceRepository :
    IDeviceRepository
{
    private const string SelectColumns =
        """
        SELECT
            DeviceId,
            DeviceCode,
            DeviceName,
            DeviceType,
            Status,
            Enabled,
            LastCommunicationTime
        FROM Device
        """;

    private readonly SqliteConnectionFactory
        _connectionFactory;

    public DeviceRepository(
        SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory =
            connectionFactory ??
            throw new ArgumentNullException(
                nameof(connectionFactory));
    }

    public Device? GetById(int deviceId)
    {
        return GetSingle(
            $"{SelectColumns} WHERE DeviceId = $value;",
            deviceId);
    }

    public Device? GetByCode(
        string deviceCode)
    {
        if (string.IsNullOrWhiteSpace(
                deviceCode))
        {
            throw new ArgumentException(
                "设备编号不能为空。",
                nameof(deviceCode));
        }

        return GetSingle(
            $"{SelectColumns} WHERE DeviceCode = $value;",
            deviceCode);
    }

    public IReadOnlyList<Device> GetAll()
    {
        var devices = new List<Device>();

        using var connection =
            _connectionFactory
                .CreateOpenConnection();

        using var command =
            connection.CreateCommand();

        command.CommandText =
            $"{SelectColumns} ORDER BY DeviceCode;";

        using SqliteDataReader reader =
            command.ExecuteReader();

        while (reader.Read())
        {
            devices.Add(MapDevice(reader));
        }

        return devices;
    }

    public void Add(Device device)
    {
        ArgumentNullException.ThrowIfNull(device);

        using var connection =
            _connectionFactory
                .CreateOpenConnection();

        using var command =
            connection.CreateCommand();

        command.CommandText =
            """
            INSERT INTO Device
            (
                DeviceCode,
                DeviceName,
                DeviceType,
                Status,
                Enabled,
                LastCommunicationTime
            )
            VALUES
            (
                $deviceCode,
                $deviceName,
                $deviceType,
                $status,
                $enabled,
                $lastCommunicationTime
            );

            SELECT last_insert_rowid();
            """;

        AddDeviceParameters(command, device);

        object? result =
            command.ExecuteScalar();

        device.DeviceId =
            Convert.ToInt32(
                result,
                CultureInfo.InvariantCulture);
    }

    public void Update(Device device)
    {
        ArgumentNullException.ThrowIfNull(device);

        using var connection =
            _connectionFactory
                .CreateOpenConnection();

        using var command =
            connection.CreateCommand();

        command.CommandText =
            """
            UPDATE Device
            SET
                DeviceCode = $deviceCode,
                DeviceName = $deviceName,
                DeviceType = $deviceType,
                Status = $status,
                Enabled = $enabled,
                LastCommunicationTime =
                    $lastCommunicationTime
            WHERE DeviceId = $deviceId;
            """;

        AddDeviceParameters(command, device);

        command.Parameters.AddWithValue(
            "$deviceId",
            device.DeviceId);

        int affectedRows =
            command.ExecuteNonQuery();

        if (affectedRows == 0)
        {
            throw new InvalidOperationException(
                $"没有找到设备ID：{device.DeviceId}。");
        }
    }

    private Device? GetSingle(
        string sql,
        object value)
    {
        using var connection =
            _connectionFactory
                .CreateOpenConnection();

        using var command =
            connection.CreateCommand();

        command.CommandText = sql;

        command.Parameters.AddWithValue(
            "$value",
            value);

        using SqliteDataReader reader =
            command.ExecuteReader();

        return reader.Read()
            ? MapDevice(reader)
            : null;
    }

    private static void AddDeviceParameters(
        SqliteCommand command,
        Device device)
    {
        command.Parameters.AddWithValue(
            "$deviceCode",
            device.DeviceCode);

        command.Parameters.AddWithValue(
            "$deviceName",
            device.DeviceName);

        command.Parameters.AddWithValue(
            "$deviceType",
            device.DeviceType);

        command.Parameters.AddWithValue(
            "$status",
            (int)device.Status);

        command.Parameters.AddWithValue(
            "$enabled",
            device.Enabled ? 1 : 0);

        object lastCommunicationTime =
            device.LastCommunicationTime
                is DateTime value
                ? value.ToString(
                    "O",
                    CultureInfo.InvariantCulture)
                : DBNull.Value;

        command.Parameters.AddWithValue(
            "$lastCommunicationTime",
            lastCommunicationTime);
    }

    private static Device MapDevice(
        SqliteDataReader reader)
    {
        int lastCommunicationOrdinal =
            reader.GetOrdinal(
                "LastCommunicationTime");

        DateTime? lastCommunicationTime =
            reader.IsDBNull(
                lastCommunicationOrdinal)
                ? null
                : DateTime.Parse(
                    reader.GetString(
                        lastCommunicationOrdinal),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind);

        return new Device
        {
            DeviceId =
                reader.GetInt32(
                    reader.GetOrdinal(
                        "DeviceId")),

            DeviceCode =
                reader.GetString(
                    reader.GetOrdinal(
                        "DeviceCode")),

            DeviceName =
                reader.GetString(
                    reader.GetOrdinal(
                        "DeviceName")),

            DeviceType =
                reader.GetString(
                    reader.GetOrdinal(
                        "DeviceType")),

            Status =
                (DeviceStatus)reader.GetInt32(
                    reader.GetOrdinal(
                        "Status")),

            Enabled =
                reader.GetInt32(
                    reader.GetOrdinal(
                        "Enabled")) == 1,

            LastCommunicationTime =
                lastCommunicationTime
        };
    }
}
