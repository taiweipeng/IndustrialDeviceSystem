using IndustrialDeviceSystem.Infrastructure.Data;
using Microsoft.Data.Sqlite;

namespace IndustrialDeviceSystem.IntegrationTests;

internal sealed class TemporaryDatabase :
    IDisposable
{
    public TemporaryDatabase(
        bool includeSeed)
    {
        DatabasePath = Path.Combine(
            Path.GetTempPath(),
            $"IndustrialDeviceSystem-{Guid.NewGuid():N}.db");

        ConnectionString =
            $"Data Source={DatabasePath};Pooling=False";

        ConnectionFactory =
            new SqliteConnectionFactory(
                ConnectionString);

        string initScriptPath =
            Path.Combine(
                AppContext.BaseDirectory,
                "database",
                "init.sql");

        string seedScriptPath =
            Path.Combine(
                AppContext.BaseDirectory,
                "database",
                "seed.sql");

        var initializer =
            new DatabaseInitializer(
                ConnectionFactory);

        initializer.Initialize(
            initScriptPath,
            includeSeed
                ? seedScriptPath
                : null);
    }

    public string DatabasePath { get; }

    public string ConnectionString { get; }

    public SqliteConnectionFactory
        ConnectionFactory
    { get; }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();

        if (File.Exists(DatabasePath))
        {
            File.Delete(DatabasePath);
        }
    }
}
