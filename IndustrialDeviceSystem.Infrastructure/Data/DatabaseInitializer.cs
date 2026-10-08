namespace IndustrialDeviceSystem.Infrastructure.Data;

public sealed class DatabaseInitializer
{
    private readonly SqliteConnectionFactory
        _connectionFactory;

    public DatabaseInitializer(
        SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory =
            connectionFactory ??
            throw new ArgumentNullException(
                nameof(connectionFactory));
    }

    public void Initialize(
        string initScriptPath,
        string? seedScriptPath = null)
    {
        ExecuteScript(initScriptPath);

        if (!string.IsNullOrWhiteSpace(
                seedScriptPath))
        {
            ExecuteScript(seedScriptPath);
        }
    }

    private void ExecuteScript(
        string scriptPath)
    {
        if (string.IsNullOrWhiteSpace(
                scriptPath))
        {
            throw new ArgumentException(
                "脚本路径不能为空。",
                nameof(scriptPath));
        }

        if (!File.Exists(scriptPath))
        {
            throw new FileNotFoundException(
                "找不到数据库脚本。",
                scriptPath);
        }

        string script =
            File.ReadAllText(scriptPath);

        using var connection =
            _connectionFactory
                .CreateOpenConnection();

        using var command =
            connection.CreateCommand();

        command.CommandText = script;
        command.ExecuteNonQuery();
    }
}
