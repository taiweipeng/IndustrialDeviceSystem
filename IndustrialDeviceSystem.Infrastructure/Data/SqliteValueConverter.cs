using System.Globalization;
using Microsoft.Data.Sqlite;

namespace IndustrialDeviceSystem.Infrastructure.Data;

internal static class SqliteValueConverter
{
    public static string ToText(DateTime value)
    {
        return value.ToString("O", CultureInfo.InvariantCulture);
    }

    public static object ToValue(DateTime? value)
    {
        return value.HasValue
            ? ToText(value.Value)
            : DBNull.Value;
    }

    public static object ToValue(int? value)
    {
        return value.HasValue
            ? value.Value
            : DBNull.Value;
    }

    public static DateTime ToDateTime(string value)
    {
        return DateTime.Parse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind);
    }

    public static DateTime? ReadNullableDateTime(
        SqliteDataReader reader,
        string columnName)
    {
        int index = reader.GetOrdinal(columnName);

        return reader.IsDBNull(index)
            ? null
            : ToDateTime(reader.GetString(index));
    }

    public static int? ReadNullableInt32(
        SqliteDataReader reader,
        string columnName)
    {
        int index = reader.GetOrdinal(columnName);

        return reader.IsDBNull(index)
            ? null
            : reader.GetInt32(index);
    }
}
