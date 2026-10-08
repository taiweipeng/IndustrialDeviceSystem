using IndustrialDeviceSystem.Core.Enums;
using IndustrialDeviceSystem.Core.Models;
using IndustrialDeviceSystem.Core.Services;
using IndustrialDeviceSystem.Infrastructure.Data;
using IndustrialDeviceSystem.Infrastructure.Repositories;
using IndustrialDeviceSystem.Infrastructure.Security;
using Microsoft.Data.Sqlite;

namespace IndustrialDeviceSystem.IntegrationTests;

public sealed class SeededAuthenticationIntegrationTests
{
    [Theory]
    [InlineData(
        "admin",
        "Admin@2026",
        UserRole.Administrator)]
    [InlineData(
        "operator01",
        "Operator@2026",
        UserRole.Operator)]
    [InlineData(
        "maintainer01",
        "Maintain@2026",
        UserRole.Maintainer)]
    [InlineData(
        "auditor01",
        "Auditor@2026",
        UserRole.Auditor)]
    public void Authenticate_SeededAccount_ShouldReturnExpectedSession(
        string userName,
        string password,
        UserRole expectedRole)
    {
        using var environment =
            new SeededAuthenticationTestEnvironment();

        var session =
            environment.AuthenticationService
                .Authenticate(
                    userName,
                    password);

        Assert.NotNull(session);
        Assert.True(session!.UserId > 0);
        Assert.Equal(
            userName,
            session.UserName);
        Assert.Equal(
            expectedRole,
            session.Role);
    }

    [Fact]
    public void Authenticate_SeededAccountWithWrongPassword_ShouldReturnNull()
    {
        using var environment =
            new SeededAuthenticationTestEnvironment();

        var session =
            environment.AuthenticationService
                .Authenticate(
                    "admin",
                    "WrongPassword@2026");

        Assert.Null(session);
    }

    private sealed class
        SeededAuthenticationTestEnvironment :
        IDisposable
    {
        private readonly string
            _temporaryDirectory;

        public AuthenticationService
            AuthenticationService
        { get; }

        public SeededAuthenticationTestEnvironment()
        {
            var initScriptPath =
                FindDatabaseScript("init.sql");

            var seedScriptPath =
                FindDatabaseScript("seed.sql");

            _temporaryDirectory =
                Path.Combine(
                    Path.GetTempPath(),
                    "IndustrialDeviceSystemTests",
                    Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(
                _temporaryDirectory);

            var databasePath =
                Path.Combine(
                    _temporaryDirectory,
                    "authentication-tests.db");

            var connectionFactory =
                new SqliteConnectionFactory(
                    $"Data Source={databasePath}");

            var initializer =
                new DatabaseInitializer(
                    connectionFactory);

            initializer.Initialize(
                initScriptPath,
                seedScriptPath);

            var userRepository =
                new UserRepository(
                    connectionFactory);

            var passwordHasher =
                new Pbkdf2PasswordHasher();

            AuthenticationService =
                new AuthenticationService(
                    userRepository,
                    passwordHasher);
        }

        public void Dispose()
        {
            SqliteConnection.ClearAllPools();

            if (Directory.Exists(
                    _temporaryDirectory))
            {
                Directory.Delete(
                    _temporaryDirectory,
                    recursive: true);
            }
        }
    }

    private static string FindDatabaseScript(
        string fileName)
    {
        var currentDirectory =
            new DirectoryInfo(
                AppContext.BaseDirectory);

        while (currentDirectory is not null)
        {
            var candidates = new[]
            {
                Path.Combine(
                    currentDirectory.FullName,
                    "database",
                    fileName),

                Path.Combine(
                    currentDirectory.FullName,
                    "IndustrialDeviceSystem.WinForms",
                    "database",
                    fileName)
            };

            foreach (var candidate in candidates)
            {
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            currentDirectory =
                currentDirectory.Parent;
        }

        throw new FileNotFoundException(
            $"找不到数据库脚本：{fileName}");
    }
}
