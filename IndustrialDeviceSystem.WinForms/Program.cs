using IndustrialDeviceSystem.Core.Services;
using IndustrialDeviceSystem.Infrastructure.Data;
using IndustrialDeviceSystem.Infrastructure.DeviceCommunication;
using IndustrialDeviceSystem.Infrastructure.Repositories;
using IndustrialDeviceSystem.Simulator;
using IndustrialDeviceSystem.Core.Models;
using IndustrialDeviceSystem.Infrastructure.Security;

namespace IndustrialDeviceSystem.WinForms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        try
        {
            var applicationDirectory = AppContext.BaseDirectory;

            // 1. 加载学生个性化配置
            var profilePath = Path.Combine(
                applicationDirectory,
                "config",
                "student-profile.json");

            EnsureRequiredFile(
                profilePath,
                "学生个性化配置文件");

            var studentProfile = new StudentProfileLoader()
                .Load(profilePath);

            // 2. 为每名学生建立独立的本地数据库
            var localDataDirectory = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "IndustrialDeviceSystem",
                studentProfile.StudentId);

            Directory.CreateDirectory(localDataDirectory);

            var databasePath = Path.Combine(
                localDataDirectory,
                "industrial-device-system.db");

            var initScriptPath = Path.Combine(
                applicationDirectory,
                "database",
                "init.sql");

            var seedScriptPath = Path.Combine(
                applicationDirectory,
                "database",
                "seed.sql");

            EnsureRequiredFile(
                initScriptPath,
                "数据库初始化脚本");

            EnsureRequiredFile(
                seedScriptPath,
                "数据库测试数据脚本");

            // 整个程序只使用同一个数据库连接工厂
            var connectionFactory = new SqliteConnectionFactory(
                $"Data Source={databasePath}");

            // 3. 必须先建表并写入基础数据，再创建和使用仓储
            var initializer = new DatabaseInitializer(
                connectionFactory);

            initializer.Initialize(
                initScriptPath,
                seedScriptPath);

            var deviceRepository = new DeviceRepository(
                connectionFactory);

            var bookingRepository = new BookingRepository(
                connectionFactory);

            var alarmRepository = new AlarmRepository(
                connectionFactory);

            var maintenanceRepository = new MaintenanceRepository(
                connectionFactory);

            var telemetryRepository = new TelemetryRepository(
                connectionFactory);
            var userRepository = new UserRepository(
                connectionFactory);
            var operationLogRepository = new OperationLogRepository(
                    connectionFactory);

            var passwordHasher =
                new Pbkdf2PasswordHasher();

            InitializeSeedUserPasswords(
                userRepository,
                passwordHasher);

            var authenticationService =
                new AuthenticationService(
                    userRepository,
                    passwordHasher);

            var clock =
                new SystemClock();

            var operationAuditService =
                new OperationAuditService(
                    operationLogRepository,
                    clock);

            // 4. 先把学生配置中的设备和预约写入数据库
            var profileSeeder =
                new StudentProfileDatabaseSeeder();

            profileSeeder.Apply(
                studentProfile,
                deviceRepository,
                bookingRepository);

            // 5. 个性化数据写入完成后，再查询本学生的设备
            var assignedDeviceCodes =
                studentProfile.Devices
                .Select(device => device.DeviceCode)
                .ToHashSet(
                    StringComparer.OrdinalIgnoreCase);

            var devices = deviceRepository
                .GetAll()
                .Where(device =>
                    assignedDeviceCodes.Contains(
                        device.DeviceCode))
                .OrderBy(device => device.DeviceCode)
                .ToList();

            if (devices.Count == 0)
            {
                throw new InvalidOperationException(
                    $"个性化设备没有成功写入数据库。" +
                    Environment.NewLine +
                    $"数据库：{databasePath}" +
                    Environment.NewLine +
                    $"配置设备：{string.Join(", ", assignedDeviceCodes)}");
            }

            // 6. 创建应用服务
            var bookingApplicationService =
                new BookingApplicationService(
                    deviceRepository,
                    bookingRepository,
                    maintenanceRepository,
                    new BookingRuleService(),
                    clock);

            var alarmApplicationService =
                new AlarmApplicationService(
                    alarmRepository,
                    new AlarmRuleService(),
                    clock);

            var deviceStateService =
                new DeviceStateService();

            var maintenanceApplicationService =
                new MaintenanceApplicationService(
                    maintenanceRepository,
                    new MaintenanceRuleService(),
                    clock);

            var maintenanceWorkflowService =
                new MaintenanceWorkflowService(
                    maintenanceApplicationService,
                    deviceRepository,
                    deviceStateService);

            // 7. 创建C#设备模拟器
            var simulatorProfiles = devices
                .Select((device, index) =>
                    new SimulatorDeviceProfile
                    {
                        DeviceCode = device.DeviceCode,
                        BaseTemperature = 35.0 + index,
                        BasePressure = 1.0 + index * 0.05,
                        BaseVibration = 0.20 + index * 0.02,
                        NormallyRunning = true
                    })
                .ToList();

            var simulatorDataSource =
                new SimulatorDataSource(
                    simulatorProfiles,
                    clock);

            var simulatorMonitoringService =
                new DeviceMonitoringApplicationService(
                    simulatorDataSource,
                    deviceRepository,
                    telemetryRepository,
                    alarmApplicationService,
                    deviceStateService);

            // 8. 将个性化配置中的第一台启用设备映射到OpenPLC。
            // 其余设备继续由C#模拟器提供数据。
            string preferredOpenPlcDeviceCode =
                studentProfile.Devices[0].DeviceCode;

            var openPlcDevice = devices
                .FirstOrDefault(device =>
                    device.Enabled &&
                    string.Equals(
                        device.DeviceCode,
                        preferredOpenPlcDeviceCode,
                        StringComparison.OrdinalIgnoreCase))
                ?? devices.FirstOrDefault(device =>
                    device.Enabled)
                ?? devices[0];

            var openPlcClientOptions =
                new TcpModbusClientOptions
                {
                    Host = "127.0.0.1",
                    Port = 5020,
                    ConnectTimeoutMilliseconds = 2000,
                    ReadTimeoutMilliseconds = 2000,
                    WriteTimeoutMilliseconds = 2000,
                    Retries = 0
                };

            using var openPlcClient =
                new TcpModbusRegisterClient(
                    openPlcClientOptions);

            var openPlcDataSource =
                new OpenPlcDataSource(
                    openPlcClient,
                    new OpenPlcDeviceOptions
                    {
                        DeviceCode = openPlcDevice.DeviceCode,
                        UnitId = 1
                    },
                    clock);

            var openPlcMonitoringService =
                new DeviceMonitoringApplicationService(
                    openPlcDataSource,
                    deviceRepository,
                    telemetryRepository,
                    alarmApplicationService,
                    deviceStateService);

            var authorizationService =
                new AuthorizationService();

            var userAccountService =
                new UserAccountService(
                    userRepository,
                    passwordHasher,
                    authorizationService,
                    clock);

            // 9. 登录—运行—退出登录循环。
            // 公共服务只创建一次；每次登录都重新创建主窗体，
            // 确保角色权限、用户信息和可见页面完全刷新。
            while (true)
            {
                UserSession userSession;

                using (var loginForm =
                       new LoginForm(
                           authenticationService))
                {
                    DialogResult loginResult =
                        loginForm.ShowDialog();

                    if (loginResult != DialogResult.OK)
                    {
                        break;
                    }

                    userSession =
                        loginForm.AuthenticatedSession
                        ?? throw new InvalidOperationException(
                            "登录成功但没有生成用户会话。");
                }

                operationAuditService.Record(
                    userSession,
                    AuditActionTypes.Login,
                    AuditTargetTypes.UserAccount,
                    userSession.UserId,
                    $"用户 {userSession.UserName} 登录系统。");

                using var mainForm = new MainForm(
                    deviceRepository,
                    simulatorMonitoringService,
                    simulatorDataSource,
                    openPlcMonitoringService,
                    openPlcDevice.DeviceCode,
                    bookingApplicationService,
                    bookingRepository,
                    alarmApplicationService,
                    alarmRepository,
                    maintenanceWorkflowService,
                    maintenanceRepository,
                    telemetryRepository,
                    studentProfile,
                    userSession,
                    authorizationService,
                    deviceStateService,
                    operationAuditService,
                    operationLogRepository,
                    userRepository,
                    userAccountService);

                Application.Run(mainForm);

                operationAuditService.Record(
                    userSession,
                    AuditActionTypes.Logout,
                    AuditTargetTypes.UserAccount,
                    userSession.UserId,
                    mainForm.LogoutRequested
                        ? $"用户 {userSession.UserName} 退出登录。"
                        : $"用户 {userSession.UserName} 关闭系统。");

                if (!mainForm.LogoutRequested)
                {
                    break;
                }
            }
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                $"系统启动失败：{exception.Message}",
                "启动错误",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// 首次创建数据库时，将seed.sql中的密码占位值替换为
    /// PBKDF2密码哈希。已经修改过的密码不会被覆盖。
    /// </summary>
    private static void InitializeSeedUserPasswords(
        UserRepository userRepository,
        Pbkdf2PasswordHasher passwordHasher)
    {
        const string unconfiguredPasswordHash =
            "NOT_CONFIGURED_FOR_LOGIN";

        var defaultAccounts = new[]
        {
            (UserName: "admin",
                Password: "Admin@2026"),
            (UserName: "operator01",
                Password: "Operator@2026"),
            (UserName: "maintainer01",
                Password: "Maintainer@2026"),
            (UserName: "auditor01",
                Password: "Auditor@2026")
        };

        foreach (var account in defaultAccounts)
        {
            var user = userRepository
                .GetByUserName(account.UserName);

            if (user is null ||
                !string.Equals(
                    user.PasswordHash,
                    unconfiguredPasswordHash,
                    StringComparison.Ordinal))
            {
                continue;
            }

            user.PasswordHash =
                passwordHasher.HashPassword(
                    account.Password);

            userRepository.Update(user);
        }
    }

    private static void EnsureRequiredFile(
        string filePath,
        string displayName)
    {
        if (File.Exists(filePath))
        {
            return;
        }

        throw new FileNotFoundException(
            $"{displayName}不存在：{filePath}",
            filePath);
    }
}
