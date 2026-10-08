using System.Globalization;
using IndustrialDeviceSystem.Core.Enums;
using IndustrialDeviceSystem.Core.Interfaces;
using IndustrialDeviceSystem.Core.Models;
using IndustrialDeviceSystem.Infrastructure.Data;
using Microsoft.Data.Sqlite;

namespace IndustrialDeviceSystem.Infrastructure.Repositories;

public sealed class BookingRepository : IBookingRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public BookingRepository(SqliteConnectionFactory connectionFactory)
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);
        _connectionFactory = connectionFactory;
    }

    public IReadOnlyList<Booking> GetActiveBookings(int deviceId)
    {
        using SqliteConnection connection =
            _connectionFactory.CreateOpenConnection();

        using SqliteCommand command = connection.CreateCommand();

        command.CommandText = """
            SELECT BookingId, DeviceId, UserId,
                   StartTime, EndTime, Purpose,
                   Status, CreatedAt
            FROM Booking
            WHERE DeviceId = $deviceId
              AND Status IN ($pending, $confirmed)
            ORDER BY StartTime, BookingId;
            """;

        command.Parameters.AddWithValue("$deviceId", deviceId);
        command.Parameters.AddWithValue(
            "$pending",
            (int)BookingStatus.Pending);
        command.Parameters.AddWithValue(
            "$confirmed",
            (int)BookingStatus.Confirmed);

        var bookings = new List<Booking>();

        using SqliteDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
            bookings.Add(ReadBooking(reader));
        }

        return bookings;
    }

    public void Add(Booking booking)
    {
        ArgumentNullException.ThrowIfNull(booking);

        if (!TryAddIfNoConflict(booking))
        {
            throw new InvalidOperationException(
                "该设备在指定时段内已经存在活动预约。");
        }
    }

    public bool TryAddIfNoConflict(Booking booking)
    {
        ArgumentNullException.ThrowIfNull(booking);

        using SqliteConnection connection =
            _connectionFactory.CreateOpenConnection();

        using SqliteTransaction transaction =
            connection.BeginTransaction(deferred: false);

        bool requiresConflictCheck =
            booking.Status is BookingStatus.Pending
                or BookingStatus.Confirmed;

        if (requiresConflictCheck &&
            HasActiveConflict(
                connection,
                transaction,
                booking))
        {
            transaction.Rollback();
            return false;
        }

        InsertBooking(
            connection,
            transaction,
            booking);

        transaction.Commit();
        return true;
    }

    private static bool HasActiveConflict(
    SqliteConnection connection,
    SqliteTransaction transaction,
    Booking booking)
    {
        using SqliteCommand command =
            connection.CreateCommand();

        command.Transaction = transaction;

        command.CommandText = """
        SELECT EXISTS
        (
            SELECT 1
            FROM Booking
            WHERE DeviceId = $deviceId
              AND Status IN ($pending, $confirmed)
              AND julianday($startTime) < julianday(EndTime)
              AND julianday(StartTime) < julianday($endTime)
        );
        """;

        command.Parameters.AddWithValue(
            "$deviceId",
            booking.DeviceId);
        command.Parameters.AddWithValue(
            "$pending",
            (int)BookingStatus.Pending);
        command.Parameters.AddWithValue(
            "$confirmed",
            (int)BookingStatus.Confirmed);
        command.Parameters.AddWithValue(
            "$startTime",
            SqliteValueConverter.ToText(booking.StartTime));
        command.Parameters.AddWithValue(
            "$endTime",
            SqliteValueConverter.ToText(booking.EndTime));

        object? result = command.ExecuteScalar();

        return Convert.ToInt32(
            result,
            CultureInfo.InvariantCulture) == 1;
    }

    private static void InsertBooking(
    SqliteConnection connection,
    SqliteTransaction transaction,
    Booking booking)
    {
        using SqliteCommand command =
            connection.CreateCommand();

        command.Transaction = transaction;

        command.CommandText = """
        INSERT INTO Booking
        (
            DeviceId,
            UserId,
            StartTime,
            EndTime,
            Purpose,
            Status,
            CreatedAt
        )
        VALUES
        (
            $deviceId,
            $userId,
            $startTime,
            $endTime,
            $purpose,
            $status,
            $createdAt
        );

        SELECT last_insert_rowid();
        """;

        AddParameters(command, booking);

        booking.BookingId = Convert.ToInt32(
            command.ExecuteScalar(),
            CultureInfo.InvariantCulture);
    }

    public void Update(Booking booking)
    {
        ArgumentNullException.ThrowIfNull(booking);

        using SqliteConnection connection =
            _connectionFactory.CreateOpenConnection();

        using SqliteCommand command = connection.CreateCommand();

        command.CommandText = """
            UPDATE Booking
            SET DeviceId = $deviceId,
                UserId = $userId,
                StartTime = $startTime,
                EndTime = $endTime,
                Purpose = $purpose,
                Status = $status,
                CreatedAt = $createdAt
            WHERE BookingId = $bookingId;
            """;

        AddParameters(command, booking);
        command.Parameters.AddWithValue(
            "$bookingId",
            booking.BookingId);

        if (command.ExecuteNonQuery() == 0)
        {
            throw new InvalidOperationException(
                $"未找到预约记录：{booking.BookingId}");
        }
    }

    private static void AddParameters(
        SqliteCommand command,
        Booking booking)
    {
        command.Parameters.AddWithValue(
            "$deviceId",
            booking.DeviceId);
        command.Parameters.AddWithValue(
            "$userId",
            booking.UserId);
        command.Parameters.AddWithValue(
            "$startTime",
            SqliteValueConverter.ToText(booking.StartTime));
        command.Parameters.AddWithValue(
            "$endTime",
            SqliteValueConverter.ToText(booking.EndTime));
        command.Parameters.AddWithValue(
            "$purpose",
            booking.Purpose);
        command.Parameters.AddWithValue(
            "$status",
            (int)booking.Status);
        command.Parameters.AddWithValue(
            "$createdAt",
            SqliteValueConverter.ToText(booking.CreatedAt));
    }

    private static Booking ReadBooking(SqliteDataReader reader)
    {
        return new Booking
        {
            BookingId = reader.GetInt32(
                reader.GetOrdinal("BookingId")),
            DeviceId = reader.GetInt32(
                reader.GetOrdinal("DeviceId")),
            UserId = reader.GetInt32(
                reader.GetOrdinal("UserId")),
            StartTime = SqliteValueConverter.ToDateTime(
                reader.GetString(reader.GetOrdinal("StartTime"))),
            EndTime = SqliteValueConverter.ToDateTime(
                reader.GetString(reader.GetOrdinal("EndTime"))),
            Purpose = reader.GetString(
                reader.GetOrdinal("Purpose")),
            Status = (BookingStatus)reader.GetInt32(
                reader.GetOrdinal("Status")),
            CreatedAt = SqliteValueConverter.ToDateTime(
                reader.GetString(reader.GetOrdinal("CreatedAt")))
        };
    }
}
