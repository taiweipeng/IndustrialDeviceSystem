using Microsoft.Data.Sqlite;
using Xunit;

namespace IndustrialDeviceSystem.IntegrationTests;

public sealed class DatabaseInitializationTests
{
    [Fact]
    public void Initialize_ShouldCreateRequiredTables()
    {
        using var database =
            new TemporaryDatabase(
                includeSeed: false);

        using var connection =
            new SqliteConnection(
                database.ConnectionString);

        connection.Open();

        using var command =
            connection.CreateCommand();

        command.CommandText =
            """
            SELECT COUNT(*)
            FROM sqlite_master
            WHERE type = 'table'
              AND name IN
              (
                  'UserAccount',
                  'Device',
                  'Booking',
                  'TelemetryData',
                  'Alarm',
                  'MaintenanceOrder',
                  'OperationLog'
              );
            """;

        int tableCount =
            Convert.ToInt32(
                command.ExecuteScalar());

        Assert.Equal(7, tableCount);
    }
}
