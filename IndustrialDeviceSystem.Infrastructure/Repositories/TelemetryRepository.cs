using System.Globalization;
using IndustrialDeviceSystem.Core.Enums;
using IndustrialDeviceSystem.Core.Interfaces;
using IndustrialDeviceSystem.Core.Models;
using IndustrialDeviceSystem.Infrastructure.Data;
using Microsoft.Data.Sqlite;

namespace IndustrialDeviceSystem.Infrastructure.Repositories;

public sealed class TelemetryRepository : ITelemetryRepository
{
    private const int MaximumTimeRangeQueryCount = 5000;

    private readonly SqliteConnectionFactory _connectionFactory;

    public TelemetryRepository(
        SqliteConnectionFactory connectionFactory)
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);
        _connectionFactory = connectionFactory;
    }

    public void Add(TelemetryRecord telemetry)
    {
        ArgumentNullException.ThrowIfNull(telemetry);

        using SqliteConnection connection =
            _connectionFactory.CreateOpenConnection();

        using SqliteCommand command =
            connection.CreateCommand();

        command.CommandText = """
            INSERT INTO TelemetryData
            (
                DeviceId,
                CollectedAt,
                Temperature,
                Pressure,
                Vibration,
                IsRunning,
                FaultSignal,
                EmergencyStop,
                Quality
            )
            VALUES
            (
                $deviceId,
                $collectedAt,
                $temperature,
                $pressure,
                $vibration,
                $isRunning,
                $faultSignal,
                $emergencyStop,
                $quality
            );

            SELECT last_insert_rowid();
            """;

        command.Parameters.AddWithValue(
            "$deviceId",
            telemetry.DeviceId);

        command.Parameters.AddWithValue(
            "$collectedAt",
            SqliteValueConverter.ToText(
                telemetry.CollectedAt));

        command.Parameters.AddWithValue(
            "$temperature",
            telemetry.Temperature);

        command.Parameters.AddWithValue(
            "$pressure",
            telemetry.Pressure);

        command.Parameters.AddWithValue(
            "$vibration",
            telemetry.Vibration);

        command.Parameters.AddWithValue(
            "$isRunning",
            telemetry.IsRunning ? 1 : 0);

        command.Parameters.AddWithValue(
            "$faultSignal",
            telemetry.FaultSignal ? 1 : 0);

        command.Parameters.AddWithValue(
            "$emergencyStop",
            telemetry.EmergencyStop ? 1 : 0);

        command.Parameters.AddWithValue(
            "$quality",
            (int)telemetry.Quality);

        telemetry.TelemetryId = Convert.ToInt32(
            command.ExecuteScalar(),
            CultureInfo.InvariantCulture);
    }

    public IReadOnlyList<TelemetryRecord> GetRecent(
        int deviceId,
        int count)
    {
        if (count <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(count),
                "查询数量必须大于0。");
        }

        using SqliteConnection connection =
            _connectionFactory.CreateOpenConnection();

        using SqliteCommand command =
            connection.CreateCommand();

        command.CommandText = """
            SELECT TelemetryId,
                   DeviceId,
                   CollectedAt,
                   Temperature,
                   Pressure,
                   Vibration,
                   IsRunning,
                   FaultSignal,
                   EmergencyStop,
                   Quality
            FROM TelemetryData
            WHERE DeviceId = $deviceId
            ORDER BY CollectedAt DESC,
                     TelemetryId DESC
            LIMIT $count;
            """;

        command.Parameters.AddWithValue(
            "$deviceId",
            deviceId);

        command.Parameters.AddWithValue(
            "$count",
            count);

        var records = new List<TelemetryRecord>();

        using SqliteDataReader reader =
            command.ExecuteReader();

        while (reader.Read())
        {
            records.Add(ReadTelemetry(reader));
        }

        return records;
    }

    public IReadOnlyList<TelemetryRecord> GetByTimeRange(
        int deviceId,
        DateTime startTime,
        DateTime endTime,
        int maximumCount)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(deviceId),
                "设备编号必须大于0。");
        }

        if (startTime >= endTime)
        {
            throw new ArgumentException(
                "开始时间必须早于结束时间。",
                nameof(startTime));
        }

        if (maximumCount <= 0 ||
            maximumCount > MaximumTimeRangeQueryCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumCount),
                $"查询数量必须在1到" +
                $"{MaximumTimeRangeQueryCount}之间。");
        }

        using SqliteConnection connection =
            _connectionFactory.CreateOpenConnection();

        using SqliteCommand command =
            connection.CreateCommand();

        // 内层选出时间段内最新的一批数据，
        // 外层再按时间正序排列，便于表格和趋势图使用。
        command.CommandText = """
            SELECT TelemetryId,
                   DeviceId,
                   CollectedAt,
                   Temperature,
                   Pressure,
                   Vibration,
                   IsRunning,
                   FaultSignal,
                   EmergencyStop,
                   Quality
            FROM
            (
                SELECT TelemetryId,
                       DeviceId,
                       CollectedAt,
                       Temperature,
                       Pressure,
                       Vibration,
                       IsRunning,
                       FaultSignal,
                       EmergencyStop,
                       Quality
                FROM TelemetryData
                WHERE DeviceId = $deviceId
                  AND CollectedAt >= $startTime
                  AND CollectedAt <= $endTime
                ORDER BY CollectedAt DESC,
                         TelemetryId DESC
                LIMIT $maximumCount
            ) AS LimitedTelemetry
            ORDER BY CollectedAt ASC,
                     TelemetryId ASC;
            """;

        command.Parameters.AddWithValue(
            "$deviceId",
            deviceId);

        command.Parameters.AddWithValue(
            "$startTime",
            SqliteValueConverter.ToText(startTime));

        command.Parameters.AddWithValue(
            "$endTime",
            SqliteValueConverter.ToText(endTime));

        command.Parameters.AddWithValue(
            "$maximumCount",
            maximumCount);

        var records = new List<TelemetryRecord>();

        using SqliteDataReader reader =
            command.ExecuteReader();

        while (reader.Read())
        {
            records.Add(ReadTelemetry(reader));
        }

        return records;
    }

    private static TelemetryRecord ReadTelemetry(
        SqliteDataReader reader)
    {
        return new TelemetryRecord
        {
            TelemetryId = reader.GetInt32(
                reader.GetOrdinal("TelemetryId")),

            DeviceId = reader.GetInt32(
                reader.GetOrdinal("DeviceId")),

            CollectedAt =
                SqliteValueConverter.ToDateTime(
                    reader.GetString(
                        reader.GetOrdinal("CollectedAt"))),

            Temperature = reader.GetDouble(
                reader.GetOrdinal("Temperature")),

            Pressure = reader.GetDouble(
                reader.GetOrdinal("Pressure")),

            Vibration = reader.GetDouble(
                reader.GetOrdinal("Vibration")),

            IsRunning = reader.GetInt32(
                reader.GetOrdinal("IsRunning")) != 0,

            FaultSignal = reader.GetInt32(
                reader.GetOrdinal("FaultSignal")) != 0,

            EmergencyStop = reader.GetInt32(
                reader.GetOrdinal("EmergencyStop")) != 0,

            Quality = (DataQuality)reader.GetInt32(
                reader.GetOrdinal("Quality"))
        };
    }
}
