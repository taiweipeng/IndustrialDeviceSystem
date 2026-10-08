using System.Drawing;
using IndustrialDeviceSystem.Core.Enums;
using IndustrialDeviceSystem.Core.Interfaces;
using IndustrialDeviceSystem.Core.Models;
using IndustrialDeviceSystem.Core.Services;
using IndustrialDeviceSystem.Infrastructure.Repositories;
using IndustrialDeviceSystem.Simulator;
using System.Globalization;
using System.Text;

namespace IndustrialDeviceSystem.WinForms;

public partial class MainForm : Form
{
    private IDeviceRepository? _deviceRepository;

    private DeviceMonitoringApplicationService?
    _simulatorMonitoringService;

    private DeviceMonitoringApplicationService?
        _openPlcMonitoringService;

    private string _openPlcDeviceCode =
        string.Empty;

    private ITelemetryRepository? _telemetryRepository;

    private IReadOnlyList<TelemetryRecord> _historyRecords =
        Array.Empty<TelemetryRecord>();

    private StudentProfile?
        _studentProfile;

    private UserSession? _userSession;

    private AuthorizationService?
        _authorizationService;

    private DeviceStateService?
        _deviceStateService;

    private TemperatureThresholds
        _temperatureThresholds = new();

    private int
        _maximumCommunicationFailures = 3;

    private bool _isCollecting;

    private bool _isClosing;

    private readonly System.Windows.Forms.Timer _clockTimer =
        new()
        {
            Interval = 1000
        };

    private SimulatorDataSource? _simulatorDataSource;

    private readonly System.Windows.Forms.Timer _monitorTimer =
        new();

    private readonly Dictionary<string, SimulationScenario>
        _appliedScenarios =
            new(StringComparer.Ordinal);

    private bool _isMonitoring;

    private BookingApplicationService?
        _bookingApplicationService;

    private IBookingRepository?
        _bookingRepository;

    private AlarmApplicationService?
        _alarmApplicationService;

    private IAlarmRepository?
        _alarmRepository;

    private MaintenanceWorkflowService?
        _maintenanceWorkflowService;

    private IMaintenanceRepository?
        _maintenanceRepository;

    private readonly Dictionary<int, Booking> _loadedBookings = new();

    private bool _isLoadingDeviceList;
    private bool _isRefreshingBusinessViews;

    private Label? _dashboardOverallStatusLabel;
    private Label? _dashboardSummaryLabel;

    private ToolStripStatusLabel?
        _tslChangePassword;

    private ToolStripStatusLabel?
        _tslLogout;

    private OperationAuditService? _operationAuditService;

    private static readonly Font EmptyStateFont =
        new(
            "Microsoft YaHei UI",
            11F,
            FontStyle.Regular);

    private IOperationLogRepository?
    _operationLogRepository;

    private IUserRepository?
        _userRepository;

    private UserAccountService?
    _userAccountService;

    private TabPage? _tabAudit;

    private ToolStripStatusLabel?
        _tslUserManagement;

    private DataGridView? _dgvAuditLogs;

    private Label? _lblAuditScope;

    private TabPage? _tabSystemStatus;

    private Label? _lblDatabaseSystemStatus;
    private Label? _lblSimulatorSystemStatus;
    private Label? _lblOpenPlcSystemStatus;
    private Label? _lblCurrentUserSystemStatus;
    private Label? _lblVersionSystemStatus;
    private Label? _lblRefreshTimeSystemStatus;

    private DateTime? _lastBusinessRefreshTime;

    private DateTime? _lastOpenPlcCheckTime;
    private bool? _lastOpenPlcCommunicationSucceeded;

    public bool LogoutRequested
    {
        get;
        private set;
    }

    private static string EscapeCsvField(
    string? value)
    {
        var text = value ?? string.Empty;

        var requiresQuotes =
            text.Contains(',') ||
            text.Contains('"') ||
            text.Contains('\r') ||
            text.Contains('\n');

        if (!requiresQuotes)
        {
            return text;
        }

        return "\"" +
               text.Replace(
                   "\"",
                   "\"\"") +
               "\"";
    }

    // 供WinForms设计器使用
    public MainForm()
    {
        InitializeComponent();

        ConfigureDashboardLayout();
        ConfigurePageCaptions();
        ConfigureGridEmptyStates();

        _clockTimer.Tick += (_, _) =>
            UpdateCurrentTime();

        _monitorTimer.Tick +=
            MonitorTimer_Tick;
    }

    // 程序运行时使用
    public MainForm(
        IDeviceRepository deviceRepository,
        DeviceMonitoringApplicationService simulatorMonitoringService,
        SimulatorDataSource simulatorDataSource,
        DeviceMonitoringApplicationService openPlcMonitoringService,
        string openPlcDeviceCode,
        BookingApplicationService bookingApplicationService,
        IBookingRepository bookingRepository,
        AlarmApplicationService alarmApplicationService,
        IAlarmRepository alarmRepository,
        MaintenanceWorkflowService maintenanceWorkflowService,
        IMaintenanceRepository maintenanceRepository,
        ITelemetryRepository telemetryRepository,
        StudentProfile studentProfile,
        UserSession userSession,
        AuthorizationService authorizationService,
        DeviceStateService deviceStateService,
        OperationAuditService operationAuditService,
        IOperationLogRepository operationLogRepository,
        IUserRepository userRepository,
        UserAccountService userAccountService)
        : this()
    {
        _deviceRepository = deviceRepository
            ?? throw new ArgumentNullException(
                nameof(deviceRepository));

        _simulatorMonitoringService =
            simulatorMonitoringService
            ?? throw new ArgumentNullException(
                nameof(simulatorMonitoringService));

        _simulatorDataSource = simulatorDataSource
            ?? throw new ArgumentNullException(
                nameof(simulatorDataSource));

        _openPlcMonitoringService =
            openPlcMonitoringService
            ?? throw new ArgumentNullException(
                nameof(openPlcMonitoringService));

        _openPlcDeviceCode =
            string.IsNullOrWhiteSpace(openPlcDeviceCode)
                ? throw new ArgumentException(
                    "OpenPLC设备编码不能为空。",
                    nameof(openPlcDeviceCode))
                : openPlcDeviceCode;

        _bookingApplicationService =
            bookingApplicationService
            ?? throw new ArgumentNullException(
                nameof(bookingApplicationService));

        _bookingRepository = bookingRepository
            ?? throw new ArgumentNullException(
                nameof(bookingRepository));
        _alarmApplicationService = alarmApplicationService
            ?? throw new ArgumentNullException(
                nameof(alarmApplicationService));

        _alarmRepository = alarmRepository
            ?? throw new ArgumentNullException(
                nameof(alarmRepository));

        _maintenanceWorkflowService = maintenanceWorkflowService
            ?? throw new ArgumentNullException(
                nameof(maintenanceWorkflowService));

        _maintenanceRepository = maintenanceRepository
            ?? throw new ArgumentNullException(
                nameof(maintenanceRepository));

        _telemetryRepository =
            telemetryRepository ??
            throw new ArgumentNullException(
                nameof(telemetryRepository));

        _studentProfile = studentProfile
            ?? throw new ArgumentNullException(
                nameof(studentProfile));

        _userSession = userSession
            ?? throw new ArgumentNullException(
                nameof(userSession));

        _authorizationService = authorizationService
            ?? throw new ArgumentNullException(
                nameof(authorizationService));

        _deviceStateService = deviceStateService
            ?? throw new ArgumentNullException(
                nameof(deviceStateService));

        _authorizationService.EnsurePermission(
            _userSession,
            SystemPermission.ViewDashboard);

        _operationAuditService =
            operationAuditService ??
            throw new ArgumentNullException(
                nameof(operationAuditService));

        _temperatureThresholds =
            new TemperatureThresholds
            {
                Warning =
                    studentProfile
                        .TemperatureThresholds.Warning,

                High =
                    studentProfile
                        .TemperatureThresholds.High,

                Emergency =
                    studentProfile
                        .TemperatureThresholds.Emergency
            };

        _operationLogRepository =
            operationLogRepository ??
            throw new ArgumentNullException(
                nameof(operationLogRepository));

        _userRepository =
            userRepository ??
            throw new ArgumentNullException(
                nameof(userRepository));

        _userAccountService =
            userAccountService ??
            throw new ArgumentNullException(
                nameof(userAccountService));

        _maximumCommunicationFailures =
            studentProfile.MaximumCommunicationFailures;

        _userAccountService =
            userAccountService ??
            throw new ArgumentNullException(
                nameof(userAccountService));

        ConfigureAccountCommands();
        ConfigureUserManagementCommand();
        UiTheme.Apply(this);
    }
    /// <summary>
    /// 管理员入口初始化
    /// </summary>
    private void ConfigureUserManagementCommand()
    {
        if (_tslUserManagement is not null)
        {
            return;
        }

        if (!HasPermission(
                SystemPermission.ManageUsers))
        {
            return;
        }

        _tslUserManagement =
            new ToolStripStatusLabel
            {
                Name = "tslUserManagement",
                Text = "用户管理",
                IsLink = true,

                LinkColor = Color.FromArgb(
                    37,
                    99,
                    235),

                ActiveLinkColor = Color.FromArgb(
                    30,
                    64,
                    175),

                Font = new Font(
                    "Microsoft YaHei UI",
                    9F,
                    FontStyle.Bold),

                BorderSides =
                    ToolStripStatusLabelBorderSides.Left,

                Padding =
                    new Padding(8, 0, 8, 0),

                Overflow =
                    ToolStripItemOverflow.Never,

                ToolTipText =
                    "创建和管理系统用户"
            };

        _tslUserManagement.Click +=
            TslUserManagement_Click;

        var changePasswordIndex =
            _tslChangePassword is null
                ? -1
                : statusMain.Items.IndexOf(
                    _tslChangePassword);

        var insertIndex =
            changePasswordIndex >= 0
                ? changePasswordIndex + 1
                : statusMain.Items.Count;

        statusMain.Items.Insert(
            insertIndex,
            _tslUserManagement);

        AppendRuntimeLog(
            "用户管理入口已加载。");
    }

    private void TslUserManagement_Click(
        object? sender,
        EventArgs e)
    {
        if (_userAccountService is null ||
            _userRepository is null ||
            _operationAuditService is null ||
            _userSession is null)
        {
            return;
        }

        if (!EnsurePermission(
                SystemPermission.ManageUsers,
                "管理系统用户"))
        {
            return;
        }

        using var form =
            new UserManagementForm(
                _userAccountService,
                _userRepository,
                _operationAuditService,
                _userSession);

        form.ShowDialog(this);

        LoadOperationLogs();
    }

    private void ConfigureDashboardLayout()
    {
        tabDashboard.SuspendLayout();
        tableLayoutPanel1.SuspendLayout();

        try
        {
            tabDashboard.Controls.Clear();
            tabDashboard.Padding = new Padding(18);
            tabDashboard.BackColor =
                Color.FromArgb(244, 247, 251);

            var rootLayout = new TableLayoutPanel
            {
                Name = "tlpDashboardRoot",
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = Color.FromArgb(244, 247, 251),
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            rootLayout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100F));

            rootLayout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 88F));

            rootLayout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 360F));

            rootLayout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 180F));

            rootLayout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100F));

            var headerLayout = new TableLayoutPanel
            {
                Name = "tlpDashboardHeader",
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent,
                Margin = new Padding(10, 4, 10, 4)
            };

            headerLayout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 62F));

            headerLayout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 38F));

            var titlePanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Margin = Padding.Empty
            };

            var titleLabel = new Label
            {
                AutoSize = true,
                Location = new Point(0, 7),
                Text = "系统运行概览",
                Font = new Font(
                    "Microsoft YaHei UI",
                    18F,
                    FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42)
            };

            var subtitleLabel = new Label
            {
                AutoSize = true,
                Location = new Point(2, 49),
                Text = "设备、预约、报警与维护业务的实时汇总",
                Font = new Font(
                    "Microsoft YaHei UI",
                    9F,
                    FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 116, 139)
            };

            titlePanel.Controls.Add(titleLabel);
            titlePanel.Controls.Add(subtitleLabel);

            var headerActions = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 18, 0, 0),
                Margin = Padding.Empty
            };

            btnRefreshDashboard.Tag = "secondary";
            btnRefreshDashboard.Size = new Size(112, 36);
            btnRefreshDashboard.Margin = new Padding(12, 0, 0, 0);

            lblDashboardRefreshTime.AutoSize = true;
            lblDashboardRefreshTime.Text = "最后刷新：尚未刷新";
            lblDashboardRefreshTime.ForeColor =
                Color.FromArgb(100, 116, 139);
            lblDashboardRefreshTime.Margin =
                new Padding(0, 8, 8, 0);

            headerActions.Controls.Add(btnRefreshDashboard);
            headerActions.Controls.Add(lblDashboardRefreshTime);

            headerLayout.Controls.Add(titlePanel, 0, 0);
            headerLayout.Controls.Add(headerActions, 1, 0);

            tableLayoutPanel1.Controls.Clear();
            tableLayoutPanel1.ColumnStyles.Clear();
            tableLayoutPanel1.RowStyles.Clear();
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.BackColor =
                Color.FromArgb(244, 247, 251);
            tableLayoutPanel1.Margin = Padding.Empty;
            tableLayoutPanel1.Padding = Padding.Empty;

            for (var column = 0; column < 3; column++)
            {
                tableLayoutPanel1.ColumnStyles.Add(
                    new ColumnStyle(
                        SizeType.Percent,
                        33.333333F));
            }

            for (var row = 0; row < 2; row++)
            {
                tableLayoutPanel1.RowStyles.Add(
                    new RowStyle(
                        SizeType.Percent,
                        50F));
            }

            tableLayoutPanel1.Controls.Add(
                CreateDashboardCard(
                    "pnlDashboardDevices",
                    "可见设备",
                    "当前学生配置范围内的设备",
                    lblDashboardDeviceCount,
                    Color.FromArgb(37, 99, 235)),
                0,
                0);

            tableLayoutPanel1.Controls.Add(
                CreateDashboardCard(
                    "pnlDashboardRunning",
                    "运行设备",
                    "正在采集或执行任务的设备",
                    lblDashboardRunningCount,
                    Color.FromArgb(22, 163, 74)),
                1,
                0);

            tableLayoutPanel1.Controls.Add(
                CreateDashboardCard(
                    "pnlDashboardFaults",
                    "故障设备",
                    "当前处于故障状态的设备",
                    lblDashboardFaultCount,
                    Color.FromArgb(220, 38, 38)),
                2,
                0);

            tableLayoutPanel1.Controls.Add(
                CreateDashboardCard(
                    "pnlDashboardBookings",
                    "有效预约",
                    "待执行、已确认或执行中的预约",
                    lblDashboardBookingCount,
                    Color.FromArgb(124, 58, 237)),
                0,
                1);

            tableLayoutPanel1.Controls.Add(
                CreateDashboardCard(
                    "pnlDashboardAlarms",
                    "未解除报警",
                    "仍需要确认或处理的设备报警",
                    lblDashboardAlarmCount,
                    Color.FromArgb(217, 119, 6)),
                1,
                1);

            tableLayoutPanel1.Controls.Add(
                CreateDashboardCard(
                    "pnlDashboardMaintenance",
                    "未完成工单",
                    "尚未关闭的设备维护工单",
                    lblDashboardMaintenanceCount,
                    Color.FromArgb(8, 145, 178)),
                2,
                1);

            var summaryPanel = new Panel
            {
                Name = "pnlDashboardSummary",
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(10, 10, 10, 10),
                Padding = new Padding(24, 16, 20, 16),
                Tag = Color.FromArgb(99, 102, 241)
            };

            var summaryLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Color.White,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            summaryLayout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100F));

            summaryLayout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 28F));

            summaryLayout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 46F));

            summaryLayout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100F));

            var summaryTitleLabel = new Label
            {
                Dock = DockStyle.Fill,
                Text = "运行状态摘要",
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font(
                    "Microsoft YaHei UI",
                    10F,
                    FontStyle.Bold),
                ForeColor = Color.FromArgb(71, 85, 105),
                Margin = Padding.Empty
            };

            _dashboardOverallStatusLabel = new Label
            {
                Dock = DockStyle.Fill,
                Text = "正在读取系统状态…",
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font(
                    "Microsoft YaHei UI",
                    15F,
                    FontStyle.Bold),
                ForeColor = Color.FromArgb(37, 99, 235),
                Margin = Padding.Empty
            };

            _dashboardSummaryLabel = new Label
            {
                Dock = DockStyle.Fill,
                Text = "统计范围：当前学生可见设备及其预约、报警和维护工单。",
                TextAlign = ContentAlignment.TopLeft,
                Font = new Font(
                    "Microsoft YaHei UI",
                    9F,
                    FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 116, 139),
                Margin = Padding.Empty
            };

            summaryLayout.Controls.Add(
                summaryTitleLabel,
                0,
                0);

            summaryLayout.Controls.Add(
                _dashboardOverallStatusLabel,
                0,
                1);

            summaryLayout.Controls.Add(
                _dashboardSummaryLabel,
                0,
                2);

            summaryPanel.Controls.Add(summaryLayout);

            rootLayout.Controls.Add(headerLayout, 0, 0);
            rootLayout.Controls.Add(tableLayoutPanel1, 0, 1);
            rootLayout.Controls.Add(summaryPanel, 0, 2);

            tabDashboard.Controls.Add(rootLayout);
        }
        finally
        {
            tableLayoutPanel1.ResumeLayout(true);
            tabDashboard.ResumeLayout(true);
        }
    }

    private void ConfigureAuditPage()
    {
        if (_tabAudit is not null)
        {
            return;
        }

        var canViewOwn =
            HasPermission(
                SystemPermission.ViewOwnOperationLog);

        var canViewAll =
            HasPermission(
                SystemPermission.ViewAllOperationLogs);

        if (!canViewOwn && !canViewAll)
        {
            return;
        }

        _tabAudit = new TabPage
        {
            Name = "tabAudit",
            Text = "操作审计",
            Padding = new Padding(0),
            BackColor = Color.White,
            UseVisualStyleBackColor = false
        };

        var queryPanel = new Panel
        {
            Name = "pnlAuditQuery",
            Dock = DockStyle.Top,
            Height = 68,
            BackColor = Color.FromArgb(
                248,
                250,
                252)
        };

        _lblAuditScope = new Label
        {
            AutoSize = true,
            Location = new Point(24, 22),
            Font = new Font(
                "Microsoft YaHei UI",
                10F,
                FontStyle.Regular),
            ForeColor = Color.FromArgb(
                51,
                65,
                85),
            Text = canViewAll
                ? "查看范围：全部用户"
                : "查看范围：仅当前用户"
        };

        var refreshButton = new Button
        {
            Name = "btnRefreshAudit",
            Text = "刷新日志",
            Size = new Size(112, 36),
            Location = new Point(310, 16),
            BackColor = Color.FromArgb(
                37,
                99,
                235),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            UseVisualStyleBackColor = false
        };

        refreshButton.FlatAppearance.BorderSize = 0;

        refreshButton.Click += (_, _) =>
            LoadOperationLogs();

        queryPanel.Controls.Add(
            _lblAuditScope);

        queryPanel.Controls.Add(
            refreshButton);

        _dgvAuditLogs = new DataGridView
        {
            Name = "dgvAuditLogs",
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            AutoGenerateColumns = false,
            AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            MultiSelect = false,
            RowHeadersVisible = false,
            SelectionMode =
                DataGridViewSelectionMode.FullRowSelect
        };

        _dgvAuditLogs.ColumnHeadersDefaultCellStyle =
            new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(
                    30,
                    64,
                    175),
                ForeColor = Color.White,
                Font = new Font(
                    "Microsoft YaHei UI",
                    9F,
                    FontStyle.Bold),
                Alignment =
                    DataGridViewContentAlignment.MiddleLeft
            };

        _dgvAuditLogs.EnableHeadersVisualStyles =
            false;

        _dgvAuditLogs.DefaultCellStyle =
            new DataGridViewCellStyle
            {
                Font = new Font(
                    "Microsoft YaHei UI",
                    9F),
                SelectionBackColor = Color.FromArgb(
                    219,
                    234,
                    254),
                SelectionForeColor = Color.FromArgb(
                    15,
                    23,
                    42)
            };

        _dgvAuditLogs.Columns.AddRange(
            new DataGridViewTextBoxColumn
            {
                HeaderText = "日志编号",
                DataPropertyName =
                    nameof(AuditLogRowViewModel.LogId),
                FillWeight = 8
            },
            new DataGridViewTextBoxColumn
            {
                HeaderText = "发生时间",
                DataPropertyName =
                    nameof(
                        AuditLogRowViewModel.OccurredAtText),
                FillWeight = 16
            },
            new DataGridViewTextBoxColumn
            {
                HeaderText = "操作用户",
                DataPropertyName =
                    nameof(
                        AuditLogRowViewModel.UserText),
                FillWeight = 13
            },
            new DataGridViewTextBoxColumn
            {
                HeaderText = "操作类型",
                DataPropertyName =
                    nameof(
                        AuditLogRowViewModel.ActionType),
                FillWeight = 15
            },
            new DataGridViewTextBoxColumn
            {
                HeaderText = "目标类型",
                DataPropertyName =
                    nameof(
                        AuditLogRowViewModel.TargetType),
                FillWeight = 12
            },
            new DataGridViewTextBoxColumn
            {
                HeaderText = "目标编号",
                DataPropertyName =
                    nameof(
                        AuditLogRowViewModel.TargetIdText),
                FillWeight = 9
            },
            new DataGridViewTextBoxColumn
            {
                HeaderText = "操作详情",
                DataPropertyName =
                    nameof(
                        AuditLogRowViewModel.Detail),
                FillWeight = 35
            });

        _tabAudit.Controls.Add(
            _dgvAuditLogs);

        _tabAudit.Controls.Add(
            queryPanel);

        tabMain.TabPages.Add(
            _tabAudit);
    }

    private void LoadOperationLogs()
    {
        if (_operationLogRepository is null ||
            _userRepository is null ||
            _userSession is null ||
            _dgvAuditLogs is null)
        {
            return;
        }

        try
        {
            var canViewAll =
                HasPermission(
                    SystemPermission
                        .ViewAllOperationLogs);

            IReadOnlyList<OperationLog> logs =
                canViewAll
                    ? _operationLogRepository
                        .GetRecent(500)
                    : _operationLogRepository
                        .GetByUser(
                            _userSession.UserId,
                            500);

            var users = _userRepository
                .GetAll()
                .ToDictionary(
                    user => user.UserId,
                    user => user.UserName);

            var rows = logs
                .Select(log =>
                    new AuditLogRowViewModel(
                        log.LogId,
                        log.OccurredAt
                            .ToLocalTime()
                            .ToString(
                                "yyyy-MM-dd HH:mm:ss"),
                        GetAuditUserText(
                            log.UserId,
                            users),
                        log.ActionType,
                        log.TargetType,
                        log.TargetId?.ToString() ?? "-",
                        log.Detail))
                .ToList();

            _dgvAuditLogs.DataSource = null;
            _dgvAuditLogs.DataSource = rows;

            if (_lblAuditScope is not null)
            {
                var scopeText = canViewAll
                    ? "全部用户"
                    : $"当前用户 {_userSession.UserName}";

                _lblAuditScope.Text =
                    $"查看范围：{scopeText}　" +
                    $"共 {rows.Count} 条";
            }
        }
        catch (Exception exception)
        {
            AppendRuntimeLog(
                $"加载审计日志失败：" +
                $"{exception.Message}");

            MessageBox.Show(
                $"无法加载审计日志：" +
                $"{exception.Message}",
                "审计日志错误",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private static string GetAuditUserText(
        int? userId,
        IReadOnlyDictionary<int, string> users)
    {
        if (!userId.HasValue)
        {
            return "系统";
        }

        return users.TryGetValue(
            userId.Value,
            out var userName)
                ? $"{userName}（{userId.Value}）"
                : $"未知用户（{userId.Value}）";
    }
    private static Panel CreateDashboardCard(
        string name,
        string title,
        string description,
        Label valueLabel,
        Color accentColor)
    {
        var cardPanel = new Panel
        {
            Name = name,
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Margin = new Padding(10),
            Padding = new Padding(24, 14, 18, 14),
            Tag = accentColor
        };

        var cardLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Color.White,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };

        cardLayout.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 100F));

        cardLayout.RowStyles.Add(
            new RowStyle(SizeType.Absolute, 30F));

        cardLayout.RowStyles.Add(
            new RowStyle(SizeType.Absolute, 70F));

        cardLayout.RowStyles.Add(
            new RowStyle(SizeType.Percent, 100F));

        var titleLabel = new Label
        {
            Dock = DockStyle.Fill,
            Text = title,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font(
                "Microsoft YaHei UI",
                10F,
                FontStyle.Bold),
            ForeColor = Color.FromArgb(71, 85, 105),
            Margin = Padding.Empty
        };

        valueLabel.AutoSize = false;
        valueLabel.Dock = DockStyle.Fill;
        valueLabel.Text = "0";
        valueLabel.TextAlign = ContentAlignment.MiddleLeft;
        valueLabel.Font = new Font(
            "Microsoft YaHei UI",
            28F,
            FontStyle.Bold);
        valueLabel.ForeColor = accentColor;
        valueLabel.Margin = Padding.Empty;

        var descriptionLabel = new Label
        {
            Dock = DockStyle.Fill,
            Text = description,
            TextAlign = ContentAlignment.TopLeft,
            AutoEllipsis = true,
            Font = new Font(
                "Microsoft YaHei UI",
                8.5F,
                FontStyle.Regular),
            ForeColor = Color.FromArgb(100, 116, 139),
            Margin = Padding.Empty
        };

        cardLayout.Controls.Add(titleLabel, 0, 0);
        cardLayout.Controls.Add(valueLabel, 0, 1);
        cardLayout.Controls.Add(descriptionLabel, 0, 2);

        cardPanel.Controls.Add(cardLayout);

        return cardPanel;
    }

    private void ConfigurePageCaptions()
    {
        ConfigureMonitoringControlLayout();
        ConfigureBookingEditorLayout();
        ConfigureMaintenanceLayout();
        ConfigureAlarmLayout();
        ConfigureHistoryQueryLayout();

        AddFieldCaption(
            grpRealtimeData,
            "capTemperature",
            "温度",
            lblTemperatureValue);

        AddFieldCaption(
            grpRealtimeData,
            "capPressure",
            "压力",
            lblPressureValue);

        AddFieldCaption(
            grpRealtimeData,
            "capVibration",
            "振动",
            lblVibrationValue);

        AddFieldCaption(
            grpRealtimeData,
            "capRunning",
            "运行状态",
            lblRunningValue);

        AddFieldCaption(
            grpRealtimeData,
            "capFault",
            "故障信号",
            lblFaultValue);

        AddFieldCaption(
            grpRealtimeData,
            "capEmergency",
            "急停信号",
            lblEmergencyValue);

        AddFieldCaption(
            grpRealtimeData,
            "capQuality",
            "数据质量",
            lblQualityValue);

        AddFieldCaption(
            grpRealtimeData,
            "capDeviceStatus",
            "设备状态",
            lblDeviceStatusValue);

        AddFieldCaption(
            grpRealtimeData,
            "capCollectedAt",
            "采集时间",
            lblCollectedAtValue);

        AddFieldCaption(
            grpRealtimeData,
            "capAlarm",
            "报警状态",
            lblAlarmValue);

        AddFieldCaption(
            grpBookingEditor,
            "capBookingDevice",
            "设备",
            cmbBookingDevice);

        AddFieldCaption(
            grpBookingEditor,
            "capBookingUser",
            "预约人",
            cmbBookingUser);

        AddFieldCaption(
            grpBookingEditor,
            "capBookingStart",
            "开始时间",
            dtpBookingStart);

        AddFieldCaption(
            grpBookingEditor,
            "capBookingEnd",
            "结束时间",
            dtpBookingEnd);

        AddFieldCaption(
            grpBookingEditor,
            "capBookingPurpose",
            "用途",
            txtBookingPurpose);

        AddFieldCaptionAt(
            grpAlarmActions,
            "capAlarmUser",
            "确认人：",
            new Point(35, 39));

        AddFieldCaption(
            grpMaintenanceCreate,
            "capMaintenanceAlarm",
            "来源报警",
            cmbMaintenanceAlarm);

        AddFieldCaption(
            grpMaintenanceCreate,
            "capMaintenanceAssignee",
            "负责人",
            cmbMaintenanceAssignee);

        AddFieldCaption(
            grpMaintenanceCreate,
            "capMaintenanceDescription",
            "工单说明",
            txtMaintenanceDescription);

        AddFieldCaption(
            grpMaintenanceCreate,
            "capMaintenancePlanStart",
            "计划开始",
            dtpMaintenancePlanStart);

        AddFieldCaption(
            grpMaintenanceCreate,
            "capMaintenancePlanEnd",
            "计划结束",
            dtpMaintenancePlanEnd);

        AddFieldCaption(
            grpMaintenanceActions,
            "capMaintenanceResult",
            "维护结果",
            txtMaintenanceResult);
    }

    private void ConfigureAlarmLayout()
    {
        cmbAlarmUser.Location = new Point(105, 33);
        cmbAlarmUser.Size = new Size(230, 32);

        btnAcknowledgeAlarm.Location = new Point(365, 31);
        btnAcknowledgeAlarm.Size = new Size(137, 34);

        btnRefreshAlarms.Location = new Point(530, 33);
        btnRefreshAlarms.Size = new Size(112, 34);

        lblActiveAlarmCount.Location = new Point(680, 38);
    }

    private void ConfigureHistoryQueryLayout()
    {
        lblHistoryDevice.Location = new Point(15, 21);
        cmbHistoryDevice.Location = new Point(72, 18);
        cmbHistoryDevice.Size = new Size(250, 32);

        lblHistoryStart.Location = new Point(345, 21);
        dtpHistoryStart.Location = new Point(438, 18);
        dtpHistoryStart.Size = new Size(210, 30);

        lblHistoryEnd.Location = new Point(672, 21);
        dtpHistoryEnd.Location = new Point(765, 18);
        dtpHistoryEnd.Size = new Size(210, 30);

        lblHistoryLimit.Location = new Point(995, 21);
        numHistoryLimit.Location = new Point(1085, 18);
        numHistoryLimit.Size = new Size(90, 30);

        btnQueryHistory.Location = new Point(1200, 16);
        btnQueryHistory.Size = new Size(90, 34);

        btnRecent100.Location = new Point(1302, 16);
        btnRecent100.Size = new Size(112, 34);

        btnExportHistory.Location = new Point(1426, 14);
        btnExportHistory.Size = new Size(112, 34);

        lblHistoryCount.Location = new Point(1560, 18);
    }

    private void ConfigureGridEmptyStates()
    {
        ConfigureGridEmptyState(
            dgvDevices,
            "暂无可见设备");

        ConfigureGridEmptyState(
            dgvBookings,
            "暂无有效预约");

        ConfigureGridEmptyState(
            dgvAlarms,
            "暂无未解除报警");

        ConfigureGridEmptyState(
            dgvMaintenance,
            "暂无维护工单");

        ConfigureGridEmptyState(
            dgvTelemetryHistory,
            "当前查询范围内暂无历史数据");
    }

    private static void ConfigureGridEmptyState(
        DataGridView grid,
        string message)
    {
        grid.Tag = message;

        grid.Paint -= DataGridViewEmptyState_Paint;
        grid.Paint += DataGridViewEmptyState_Paint;
    }

    private static void DataGridViewEmptyState_Paint(
        object? sender,
        PaintEventArgs e)
    {
        if (sender is not DataGridView grid ||
            grid.Rows.Count > 0 ||
            grid.Tag is not string message ||
            string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        var contentTop =
            grid.ColumnHeadersVisible
                ? grid.ColumnHeadersHeight
                : 0;

        var contentBounds = new Rectangle(
            0,
            contentTop,
            grid.ClientSize.Width,
            Math.Max(
                0,
                grid.ClientSize.Height - contentTop));

        TextRenderer.DrawText(
            e.Graphics,
            message,
            EmptyStateFont,
            contentBounds,
            Color.FromArgb(148, 163, 184),
            TextFormatFlags.HorizontalCenter |
            TextFormatFlags.VerticalCenter |
            TextFormatFlags.EndEllipsis);
    }

    private void ConfigureBookingEditorLayout()
    {
        cmbBookingDevice.Location = new Point(40, 51);
        cmbBookingDevice.Size = new Size(230, 32);

        cmbBookingUser.Location = new Point(310, 51);
        cmbBookingUser.Size = new Size(215, 32);

        dtpBookingStart.Location = new Point(565, 49);
        dtpBookingStart.Size = new Size(265, 30);

        dtpBookingEnd.Location = new Point(865, 49);
        dtpBookingEnd.Size = new Size(265, 30);

        txtBookingPurpose.Location = new Point(1165, 47);
        txtBookingPurpose.Size = new Size(175, 30);

        btnCreateBooking.Location = new Point(1370, 43);
        btnCreateBooking.Size = new Size(112, 34);

        btnCancelBooking.Location = new Point(1490, 43);
        btnCancelBooking.Size = new Size(122, 34);

        btnRefreshBookings.Location = new Point(1622, 43);
        btnRefreshBookings.Size = new Size(112, 34);

        btnConfirmBooking.Location = new Point(1370, 97);
        btnConfirmBooking.Size = new Size(112, 34);

        btnCompleteBooking.Location = new Point(1490, 97);
        btnCompleteBooking.Size = new Size(112, 34);
    }

    private void ConfigureMaintenanceLayout()
    {
        cmbMaintenanceAlarm.Location = new Point(35, 59);
        cmbMaintenanceAlarm.Size = new Size(210, 32);

        cmbMaintenanceAssignee.Location = new Point(280, 59);
        cmbMaintenanceAssignee.Size = new Size(210, 32);

        txtMaintenanceDescription.Location = new Point(525, 59);
        txtMaintenanceDescription.Size = new Size(200, 30);

        chkMaintenancePlan.Location = new Point(755, 61);

        dtpMaintenancePlanStart.Location = new Point(925, 57);
        dtpMaintenancePlanStart.Size = new Size(240, 30);

        dtpMaintenancePlanEnd.Location = new Point(1200, 57);
        dtpMaintenancePlanEnd.Size = new Size(240, 30);

        btnCreateMaintenance.Location = new Point(1490, 53);
        btnCreateMaintenance.Size = new Size(130, 34);

        txtMaintenanceResult.Location = new Point(35, 55);
        txtMaintenanceResult.Size = new Size(230, 30);

        chkVerificationPassed.Location = new Point(300, 57);

        btnAssignMaintenance.Location = new Point(490, 51);
        btnStartMaintenance.Location = new Point(640, 51);
        btnCompleteMaintenance.Location = new Point(790, 51);
        btnCloseMaintenance.Location = new Point(940, 51);
        btnRefreshMaintenance.Location = new Point(1090, 53);
    }

    private void ConfigureMonitoringControlLayout()
    {
        cmbDataSource.Location = new Point(90, 38);
        cmbDataSource.Size = new Size(132, 32);

        cmbDevice.Location = new Point(294, 38);
        cmbDevice.Size = new Size(160, 32);

        lblScenario.Location = new Point(469, 43);
        cmbScenario.Location = new Point(551, 38);
        cmbScenario.Size = new Size(150, 32);

        numSampleInterval.Location = new Point(810, 38);
        numSampleInterval.Size = new Size(80, 30);

        AddFieldCaptionAt(
            grpMonitoringControl,
            "capDataSource",
            "数据源：",
            new Point(20, 43));

        AddFieldCaptionAt(
            grpMonitoringControl,
            "capMonitoringDevice",
            "设备：",
            new Point(235, 43));

        AddFieldCaptionAt(
            grpMonitoringControl,
            "capSampleInterval",
            "周期(秒)：",
            new Point(713, 43));
    }

    private static void AddFieldCaption(
        Control parent,
        string name,
        string text,
        Control fieldControl)
    {
        AddFieldCaptionAt(
            parent,
            name,
            text,
            new Point(
                fieldControl.Left,
                Math.Max(27, fieldControl.Top - 24)));
    }

    private static void AddFieldCaptionAt(
        Control parent,
        string name,
        string text,
        Point location)
    {
        if (parent.Controls.ContainsKey(name))
        {
            return;
        }

        var caption = new Label
        {
            Name = name,
            AutoSize = true,
            Location = location,
            Text = text,
            Font = new Font(
                "Microsoft YaHei UI",
                8.25F,
                FontStyle.Regular),
            ForeColor = Color.FromArgb(100, 116, 139)
        };

        parent.Controls.Add(caption);
        caption.BringToFront();
    }

    private bool IsSimulatorSelected =>
    string.Equals(
        cmbDataSource.SelectedItem?.ToString(),
        "C#模拟器",
        StringComparison.Ordinal);

    private bool IsOpenPlcSelected =>
        string.Equals(
            cmbDataSource.SelectedItem?.ToString(),
            "OpenPLC",
            StringComparison.Ordinal);

    private DeviceMonitoringApplicationService?
        SelectedMonitoringService =>
            IsSimulatorSelected
                ? _simulatorMonitoringService
                : IsOpenPlcSelected
                    ? _openPlcMonitoringService
                    : null;

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        //ConfigureAccountCommands();

        ConfigureDeviceGrid();
        ConfigureDataSourceSelector();
        ConfigureScenarioSelector();

        dgvDevices.SelectionChanged +=
            DgvDevices_SelectionChanged;

        btnCollectOnce.Click +=
            BtnCollectOnce_Click;

        btnStartMonitoring.Click +=
            BtnStartMonitoring_Click;

        btnStopMonitoring.Click +=
            BtnStopMonitoring_Click;

        UpdateCurrentTime();
        _clockTimer.Start();

        if (_deviceRepository is null)
        {
            AppendRuntimeLog(
                "未配置设备仓储，无法加载设备。");

            tslConnection.Text =
                "数据库状态：未配置";

            UpdateControlStates();
            return;
        }
        ConfigureScenarioSelector();

        //LoadDeviceList();
        ConfigureBookingPage();
        RefreshBusinessViews();

        ConfigureAlarmPage();
        //LoadAlarmList();

        ApplyStudentProfileToView();

        ConfigureHistoryPage();

        ConfigureMaintenancePage();
        LoadMaintenanceAlarmOptions();
        //LoadMaintenanceOrderList();
        RefreshBusinessViews();
        UpdateControlStates();
        ApplyAuthorizationToView();
        
        ConfigureAuditPage();
        LoadOperationLogs();

        ConfigureSystemStatusPage();
        RefreshSystemStatusView();

        RefreshDashboard();
    }
    /// <summary>
    /// 状态栏入口初始化方法
    /// </summary>
    private void ConfigureAccountCommands()
    {
        if (_tslChangePassword is not null)
        {
            return;
        }

        _tslChangePassword =
            new ToolStripStatusLabel
            {
                Name = "tslChangePassword",
                Text = "修改密码",
                IsLink = true,

                LinkColor = Color.FromArgb(
                    37,
                    99,
                    235),

                ActiveLinkColor = Color.FromArgb(
                    30,
                    64,
                    175),

                VisitedLinkColor = Color.FromArgb(
                    37,
                    99,
                    235),

                Font = new Font(
                    "Microsoft YaHei UI",
                    9F,
                    FontStyle.Bold),

                BorderSides =
                    ToolStripStatusLabelBorderSides.Left,

                BorderStyle =
                    Border3DStyle.Etched,

                Padding =
                    new Padding(8, 0, 8, 0),

                Overflow = ToolStripItemOverflow.Never,

                ToolTipText =
                    "修改当前登录账号的密码"
            };

        _tslChangePassword.Click +=
            TslChangePassword_Click;

        var currentTimeIndex =
            statusMain.Items.IndexOf(
                tslCurrentTime);

        var insertIndex =
            currentTimeIndex >= 0
                ? currentTimeIndex + 1
                : statusMain.Items.Count;

        statusMain.Items.Insert(
            insertIndex,
            _tslChangePassword);

        ConfigureLogoutCommand();

        AppendRuntimeLog(
            "修改密码入口已加载。");
    }

    private void ConfigureLogoutCommand()
    {
        if (_tslLogout is not null)
        {
            return;
        }

        _tslLogout =
            new ToolStripStatusLabel
            {
                Name = "tslLogout",
                Text = "退出登录",
                IsLink = true,

                LinkColor = Color.FromArgb(
                    220,
                    38,
                    38),

                ActiveLinkColor = Color.FromArgb(
                    153,
                    27,
                    27),

                VisitedLinkColor = Color.FromArgb(
                    220,
                    38,
                    38),

                Font = new Font(
                    "Microsoft YaHei UI",
                    9F,
                    FontStyle.Bold),

                BorderSides =
                    ToolStripStatusLabelBorderSides.Left,

                BorderStyle =
                    Border3DStyle.Etched,

                Padding =
                    new Padding(8, 0, 8, 0),

                Overflow =
                    ToolStripItemOverflow.Never,

                ToolTipText =
                    "退出当前账户并返回登录窗口"
            };

        _tslLogout.Click +=
            TslLogout_Click;

        var currentTimeIndex =
            statusMain.Items.IndexOf(
                tslCurrentTime);

        var insertIndex =
            currentTimeIndex >= 0
                ? currentTimeIndex + 1
                : 0;

        statusMain.Items.Insert(
            insertIndex,
            _tslLogout);
    }

    private void TslLogout_Click(
        object? sender,
        EventArgs e)
    {
        if (_userSession is null)
        {
            return;
        }

        if (_isCollecting)
        {
            MessageBox.Show(
                "当前正在采集设备数据，请等待本次采集完成后再退出登录。",
                "暂时无法退出",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            return;
        }

        DialogResult result = MessageBox.Show(
            $"确定退出当前账户“{_userSession.UserName}”吗？" +
            Environment.NewLine +
            "退出后将返回登录窗口。",
            "退出登录",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2);

        if (result != DialogResult.Yes)
        {
            return;
        }

        if (_isMonitoring)
        {
            StopContinuousMonitoring(
                "退出登录，连续监控已停止。");
        }
        else
        {
            _monitorTimer.Stop();
        }

        LogoutRequested = true;
        Close();
    }

    private void TslChangePassword_Click(
    object? sender,
    EventArgs e)
    {
        if (_userAccountService is null ||
            _userSession is null)
        {
            return;
        }

        using var form =
            new ChangePasswordForm(
                _userAccountService,
                _userSession);

        if (form.ShowDialog(this) !=
            DialogResult.OK)
        {
            return;
        }

        RecordAudit(
            AuditActionTypes.ChangeOwnPassword,
            AuditTargetTypes.UserAccount,
            _userSession.UserId,
            "用户修改本人登录密码。");

        MessageBox.Show(
            "密码修改成功。" +
            Environment.NewLine +
            "下次登录请使用新密码。",
            "账户安全",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }
    private void RefreshBusinessViews()
    {
        if (_isRefreshingBusinessViews)
        {
            return;
        }

        _isRefreshingBusinessViews = true;

        try
        {
            if (_deviceRepository is not null)
            {
                LoadDeviceList();
            }

            if (_bookingRepository is not null)
            {
                LoadBookingList();
            }

            if (_alarmRepository is not null)
            {
                LoadAlarmList();
            }

            if (_maintenanceRepository is not null)
            {
                LoadMaintenanceAlarmOptions();
                LoadMaintenanceOrderList();
            }

            RefreshDashboard();

            _lastBusinessRefreshTime =
                DateTime.Now;

            RefreshSystemStatusView();
        }
        finally
        {
            _isRefreshingBusinessViews = false;
        }
    }

    private void ConfigureSystemStatusPage()
    {
        if (_tabSystemStatus is not null)
        {
            return;
        }

        _tabSystemStatus = new TabPage
        {
            Name = "tabSystemStatus",
            Text = "系统状态",
            Padding = new Padding(18),
            BackColor = Color.FromArgb(
                244,
                247,
                251)
        };

        var headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 76,
            BackColor = Color.White,
            Padding = new Padding(18, 12, 18, 12)
        };

        var titleLabel = new Label
        {
            AutoSize = true,
            Location = new Point(18, 12),
            Text = "系统运行状态",
            Font = new Font(
                "Microsoft YaHei UI",
                16F,
                FontStyle.Bold),
            ForeColor = Color.FromArgb(
                30,
                41,
                59)
        };

        var descriptionLabel = new Label
        {
            AutoSize = true,
            Location = new Point(20, 46),
            Text = "查看核心服务、当前会话和数据刷新状态",
            Font = new Font(
                "Microsoft YaHei UI",
                9F),
            ForeColor = Color.FromArgb(
                100,
                116,
                139)
        };

        var refreshButton = new Button
        {
            Name = "btnRefreshSystemStatus",
            Text = "刷新状态",
            Size = new Size(110, 36),
            Location = new Point(
                650,
                20),
            Anchor =
                AnchorStyles.Top |
                AnchorStyles.Right,
            BackColor = Color.FromArgb(
                37,
                99,
                235),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };

        refreshButton.FlatAppearance.BorderSize = 0;
        refreshButton.Click += (_, _) =>
            RefreshSystemStatusView();

        headerPanel.Resize += (_, _) =>
        {
            refreshButton.Left =
                headerPanel.ClientSize.Width -
                refreshButton.Width - 18;
        };

        headerPanel.Controls.Add(titleLabel);
        headerPanel.Controls.Add(descriptionLabel);
        headerPanel.Controls.Add(refreshButton);

        var statusFlowPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            WrapContents = true,
            FlowDirection =
                FlowDirection.LeftToRight,
            Padding = new Padding(0, 18, 0, 0),
            BackColor = Color.FromArgb(
                244,
                247,
                251)
        };

        statusFlowPanel.Controls.Add(
            CreateSystemStatusCard(
                "数据库",
                out _lblDatabaseSystemStatus));

        statusFlowPanel.Controls.Add(
            CreateSystemStatusCard(
                "C#设备模拟器",
                out _lblSimulatorSystemStatus));

        statusFlowPanel.Controls.Add(
            CreateSystemStatusCard(
                "OpenPLC",
                out _lblOpenPlcSystemStatus));

        statusFlowPanel.Controls.Add(
            CreateSystemStatusCard(
                "当前用户",
                out _lblCurrentUserSystemStatus));

        statusFlowPanel.Controls.Add(
            CreateSystemStatusCard(
                "程序版本",
                out _lblVersionSystemStatus));

        statusFlowPanel.Controls.Add(
            CreateSystemStatusCard(
                "最近数据刷新",
                out _lblRefreshTimeSystemStatus));

        _tabSystemStatus.Controls.Add(
            statusFlowPanel);
        _tabSystemStatus.Controls.Add(
            headerPanel);

        tabMain.TabPages.Add(
            _tabSystemStatus);
    }

    private static Panel CreateSystemStatusCard(
        string title,
        out Label valueLabel)
    {
        var card = new Panel
        {
            Size = new Size(355, 125),
            Margin = new Padding(0, 0, 18, 18),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(18)
        };

        var titleLabel = new Label
        {
            AutoSize = true,
            Location = new Point(18, 18),
            Text = title,
            Font = new Font(
                "Microsoft YaHei UI",
                10F,
                FontStyle.Bold),
            ForeColor = Color.FromArgb(
                71,
                85,
                105)
        };

        valueLabel = new Label
        {
            Location = new Point(18, 52),
            Size = new Size(315, 54),
            Text = "正在检查...",
            Font = new Font(
                "Microsoft YaHei UI",
                11F,
                FontStyle.Bold),
            ForeColor = Color.FromArgb(
                100,
                116,
                139),
            AutoEllipsis = true
        };

        card.Controls.Add(titleLabel);
        card.Controls.Add(valueLabel);

        return card;
    }

    private void RefreshSystemStatusView()
    {
        RefreshDatabaseSystemStatus();

        SetSystemStatus(
            _lblSimulatorSystemStatus,
            _simulatorDataSource is not null &&
            _simulatorMonitoringService is not null
                ? "已就绪"
                : "未配置",
            _simulatorDataSource is not null &&
            _simulatorMonitoringService is not null
                ? Color.FromArgb(22, 163, 74)
                : Color.FromArgb(220, 38, 38));

        RefreshOpenPlcSystemStatus();

        SetSystemStatus(
            _lblCurrentUserSystemStatus,
            _userSession is null
                ? "未登录"
                : $"{_userSession.UserName}（{_userSession.Role}）",
            _userSession is null
                ? Color.FromArgb(220, 38, 38)
                : Color.FromArgb(37, 99, 235));

        SetSystemStatus(
            _lblVersionSystemStatus,
            $"v{Application.ProductVersion} · " +
            $".NET {Environment.Version}",
            Color.FromArgb(51, 65, 85));

        SetSystemStatus(
            _lblRefreshTimeSystemStatus,
            _lastBusinessRefreshTime.HasValue
                ? _lastBusinessRefreshTime.Value
                    .ToString("yyyy-MM-dd HH:mm:ss")
                : "尚未刷新",
            _lastBusinessRefreshTime.HasValue
                ? Color.FromArgb(22, 163, 74)
                : Color.FromArgb(217, 119, 6));
    }

    private void RefreshDatabaseSystemStatus()
    {
        if (_deviceRepository is null)
        {
            SetSystemStatus(
                _lblDatabaseSystemStatus,
                "未配置",
                Color.FromArgb(220, 38, 38));

            return;
        }

        try
        {
            int deviceCount =
                _deviceRepository.GetAll().Count;

            SetSystemStatus(
                _lblDatabaseSystemStatus,
                $"连接正常 · {deviceCount}台设备",
                Color.FromArgb(22, 163, 74));
        }
        catch (Exception exception)
        {
            SetSystemStatus(
                _lblDatabaseSystemStatus,
                $"连接失败：{exception.Message}",
                Color.FromArgb(220, 38, 38));
        }
    }

    private void RefreshOpenPlcSystemStatus()
    {
        if (_deviceRepository is null ||
            _openPlcMonitoringService is null ||
            string.IsNullOrWhiteSpace(
                _openPlcDeviceCode))
        {
            SetSystemStatus(
                _lblOpenPlcSystemStatus,
                "未配置",
                Color.FromArgb(220, 38, 38));

            return;
        }

        try
        {
            var device =
                _deviceRepository.GetByCode(
                    _openPlcDeviceCode);

            if (device is null)
            {
                SetSystemStatus(
                    _lblOpenPlcSystemStatus,
                    "映射设备不存在",
                    Color.FromArgb(220, 38, 38));

                return;
            }

            if (!device.Enabled)
            {
                SetSystemStatus(
                    _lblOpenPlcSystemStatus,
                    $"{device.DeviceCode} · 设备已停用",
                    Color.FromArgb(100, 116, 139));

                return;
            }

            if (!_lastOpenPlcCheckTime.HasValue ||
                !_lastOpenPlcCommunicationSucceeded.HasValue)
            {
                SetSystemStatus(
                    _lblOpenPlcSystemStatus,
                    $"{device.DeviceCode} · 已配置，尚未通信",
                    Color.FromArgb(217, 119, 6));

                return;
            }

            if (!_lastOpenPlcCommunicationSucceeded.Value)
            {
                SetSystemStatus(
                    _lblOpenPlcSystemStatus,
                    $"{device.DeviceCode} · 通信失败" +
                    Environment.NewLine +
                    $"最近检测 {_lastOpenPlcCheckTime.Value:HH:mm:ss}",
                    Color.FromArgb(220, 38, 38));

                return;
            }

            SetSystemStatus(
                _lblOpenPlcSystemStatus,
                $"{device.DeviceCode} · {device.Status}" +
                Environment.NewLine +
                $"最近通信 {_lastOpenPlcCheckTime.Value:HH:mm:ss}",
                Color.FromArgb(22, 163, 74));
        }
        catch (Exception exception)
        {
            SetSystemStatus(
                _lblOpenPlcSystemStatus,
                $"状态读取失败：{exception.Message}",
                Color.FromArgb(220, 38, 38));
        }
    }

    private static void SetSystemStatus(
        Label? label,
        string text,
        Color color)
    {
        if (label is null)
        {
            return;
        }

        label.Text = text;
        label.ForeColor = color;
    }

    private static DateTime ConvertPickerValueToUtc(
    DateTime value)
    {
        return DateTime.SpecifyKind(
                value,
                DateTimeKind.Local)
            .ToUniversalTime();
    }

    private static DateTime ConvertTelemetryTimeToLocal(
        DateTime value)
    {
        var utcValue = value.Kind switch
        {
            DateTimeKind.Utc => value,

            DateTimeKind.Local =>
                value.ToUniversalTime(),

            _ => DateTime.SpecifyKind(
                value,
                DateTimeKind.Utc)
        };

        return utcValue.ToLocalTime();
    }

    private void BindHistoryRecords(
    IEnumerable<TelemetryRecord> records)
    {
        _historyRecords = records
            .OrderBy(record => record.CollectedAt)
            .ThenBy(record => record.TelemetryId)
            .ToList();

        var rows = _historyRecords
            .OrderByDescending(record =>
                record.CollectedAt)
            .ThenByDescending(record =>
                record.TelemetryId)
            .Select(record =>
                new TelemetryHistoryRowViewModel(
                    record.TelemetryId,
                    ConvertTelemetryTimeToLocal(
                            record.CollectedAt)
                        .ToString(
                            "yyyy-MM-dd HH:mm:ss"),
                    Math.Round(
                        record.Temperature,
                        2),
                    Math.Round(
                        record.Pressure,
                        3),
                    Math.Round(
                        record.Vibration,
                        3),
                    record.IsRunning ? "是" : "否",
                    record.FaultSignal ? "是" : "否",
                    record.EmergencyStop ? "是" : "否",
                    record.Quality.ToString()))
            .ToList();

        dgvTelemetryHistory.DataSource = null;
        dgvTelemetryHistory.DataSource = rows;

        ConfigureHistoryGridColumns();

        lblHistoryCount.Text =
            $"共 {_historyRecords.Count} 条";

        btnExportHistory.Enabled =
            _historyRecords.Count > 0;
    }

    private void ConfigureHistoryGridColumns()
    {
        SetHistoryColumnHeader(
            nameof(
                TelemetryHistoryRowViewModel.TelemetryId),
            "编号");

        SetHistoryColumnHeader(
            nameof(
                TelemetryHistoryRowViewModel.CollectedAt),
            "采集时间");

        SetHistoryColumnHeader(
            nameof(
                TelemetryHistoryRowViewModel.Temperature),
            "温度（℃）");

        SetHistoryColumnHeader(
            nameof(
                TelemetryHistoryRowViewModel.Pressure),
            "压力");

        SetHistoryColumnHeader(
            nameof(
                TelemetryHistoryRowViewModel.Vibration),
            "振动");

        SetHistoryColumnHeader(
            nameof(
                TelemetryHistoryRowViewModel.IsRunning),
            "运行");

        SetHistoryColumnHeader(
            nameof(
                TelemetryHistoryRowViewModel.FaultSignal),
            "故障");

        SetHistoryColumnHeader(
            nameof(
                TelemetryHistoryRowViewModel.EmergencyStop),
            "急停");

        SetHistoryColumnHeader(
            nameof(
                TelemetryHistoryRowViewModel.Quality),
            "数据质量");
    }

    private void SetHistoryColumnHeader(
        string columnName,
        string headerText)
    {
        if (dgvTelemetryHistory.Columns[
                columnName] is DataGridViewColumn column)
        {
            column.HeaderText = headerText;
        }
    }

    private void ApplyStudentProfileToView()
    {
        if (_studentProfile is null)
        {
            return;
        }

        string loginUserText =
            _userSession is null
        ? "未登录"
        : $"{_userSession.UserName}" +
          $"（{GetUserRoleText(_userSession.Role)}）";

        Text =
            $"工业设备运行、预约与维护管理系统 - " +
            $"{_studentProfile.StudentId} - " +
            loginUserText;

        tslStudent.Text =
            $"学生：{_studentProfile.StudentId} " +
            $"| GitHub：{_studentProfile.GithubUser} " +
            $"| 登录用户：{loginUserText}";

        var interval = Math.Clamp(
            _studentProfile.SampleIntervalSeconds,
            decimal.ToInt32(
                numSampleInterval.Minimum),
            decimal.ToInt32(
                numSampleInterval.Maximum));

        numSampleInterval.Value = interval;

        AppendRuntimeLog(
            $"个性化配置加载成功：" +
            $"学号={_studentProfile.StudentId}，" +
            $"GitHub={_studentProfile.GithubUser}，" +
            $"采样周期={interval}秒，" +
            $"通信失败上限=" +
            $"{_maximumCommunicationFailures}，" +
            $"温度阈值=" +
            $"{_temperatureThresholds.Warning}/" +
            $"{_temperatureThresholds.High}/" +
            $"{_temperatureThresholds.Emergency}℃。");
    }
    private static string GetUserRoleText(
    UserRole role)
    {
        return role switch
        {
            UserRole.Administrator => "管理员",
            UserRole.Operator => "操作员",
            UserRole.Maintainer => "维护人员",
            UserRole.Auditor => "审计员",
            _ => "未知角色"
        };
    }

    /// <summary>
    /// 权限判断方法
    /// </summary>
    /// <param name="permission"></param>
    /// <returns></returns>
    private bool HasPermission(
        SystemPermission permission)
    {
        return _userSession is not null &&
               _authorizationService is not null &&
               _authorizationService.HasPermission(
                   _userSession,
                   permission);
    }
    /// <summary>
    /// 统一应用权限的方法
    /// </summary>
    private void ApplyAuthorizationToView()
    {
        if (_userSession is null)
        {
            return;
        }

        // 预约用户默认定位到当前登录用户
        for (int index = 0;
             index < cmbBookingUser.Items.Count;
             index++)
        {
            if (cmbBookingUser.Items[index]
                    is BookingUserOption option &&
                option.UserId == _userSession.UserId)
            {
                cmbBookingUser.SelectedIndex = index;
                break;
            }
        }

        // 报警确认人必须是当前登录用户
        for (int index = 0;
             index < cmbAlarmUser.Items.Count;
             index++)
        {
            if (cmbAlarmUser.Items[index]
                    is AlarmUserOption option &&
                option.UserId == _userSession.UserId)
            {
                cmbAlarmUser.SelectedIndex = index;
                break;
            }
        }

        // 已登录后不允许冒用其他身份确认报警
        cmbAlarmUser.Enabled = false;

        UpdateControlStates();
        UpdateBookingActionButtons();
        UpdateAlarmButtonStates();
        UpdateMaintenanceButtonStates();

        AppendRuntimeLog(
            $"已加载用户权限：" +
            $"{_userSession.UserName}，" +
            $"角色={GetUserRoleText(_userSession.Role)}。");
    }

    private void ConfigureScenarioSelector()
    {
        cmbScenario.DataSource = null;
        cmbScenario.Items.Clear();

        cmbScenario.Items.AddRange(
            new object[]
            {
            new ScenarioOption(
                SimulationScenario.Normal,
                "正常运行"),

            new ScenarioOption(
                SimulationScenario.TemperatureRise,
                "温度持续上升"),

            new ScenarioOption(
                SimulationScenario.Fault,
                "设备故障"),

            new ScenarioOption(
                SimulationScenario.EmergencyStop,
                "急停触发"),

            new ScenarioOption(
                SimulationScenario.CommunicationLost,
                "通信中断")
            });

        cmbScenario.SelectedIndex = 0;
    }

    private void ConfigureMaintenancePage()
    {
        ConfigureMaintenanceGrid();

        cmbMaintenanceAssignee.DisplayMember =
            nameof(MaintenanceAssigneeOption.DisplayName);

        cmbMaintenanceAssignee.DataSource =
            new List<MaintenanceAssigneeOption>
            {
            new(null, "暂不指派"),
            new(3, "maintainer01（维护员）")
            };

        cmbMaintenanceAssignee.SelectedIndex = 0;

        dtpMaintenancePlanStart.Format =
            DateTimePickerFormat.Custom;

        dtpMaintenancePlanStart.CustomFormat =
            "yyyy-MM-dd HH:mm";

        dtpMaintenancePlanEnd.Format =
            DateTimePickerFormat.Custom;

        dtpMaintenancePlanEnd.CustomFormat =
            "yyyy-MM-dd HH:mm";

        dtpMaintenancePlanStart.Value =
            DateTime.Now.AddHours(1);

        dtpMaintenancePlanEnd.Value =
            DateTime.Now.AddHours(2);

        chkMaintenancePlan.Checked = false;

        chkMaintenancePlan.CheckedChanged += (_, _) =>
            UpdateMaintenanceButtonStates();

        btnCreateMaintenance.Click +=
            BtnCreateMaintenance_Click;

        btnAssignMaintenance.Click +=
            BtnAssignMaintenance_Click;

        btnStartMaintenance.Click +=
            BtnStartMaintenance_Click;

        btnCompleteMaintenance.Click +=
            BtnCompleteMaintenance_Click;

        btnCloseMaintenance.Click +=
            BtnCloseMaintenance_Click;

        btnRefreshMaintenance.Click += (_, _) =>
        {
            LoadMaintenanceAlarmOptions();
            LoadMaintenanceOrderList();
        };

        dgvMaintenance.SelectionChanged += (_, _) =>
            UpdateMaintenanceButtonStates();

        UpdateMaintenanceButtonStates();
    }

    private void ConfigureMaintenanceGrid()
    {
        dgvMaintenance.AutoGenerateColumns = false;
        dgvMaintenance.Columns.Clear();

        dgvMaintenance.Columns.AddRange(
            new DataGridViewTextBoxColumn
            {
                HeaderText = "工单号",
                DataPropertyName =
                    nameof(MaintenanceRowViewModel.OrderId),
                FillWeight = 9
            },
            new DataGridViewTextBoxColumn
            {
                HeaderText = "设备",
                DataPropertyName =
                    nameof(MaintenanceRowViewModel.DeviceText),
                FillWeight = 20
            },
            new DataGridViewTextBoxColumn
            {
                HeaderText = "来源报警",
                DataPropertyName =
                    nameof(MaintenanceRowViewModel.SourceAlarmText),
                FillWeight = 11
            },
            new DataGridViewTextBoxColumn
            {
                HeaderText = "负责人",
                DataPropertyName =
                    nameof(MaintenanceRowViewModel.AssigneeText),
                FillWeight = 16
            },
            new DataGridViewTextBoxColumn
            {
                HeaderText = "说明",
                DataPropertyName =
                    nameof(MaintenanceRowViewModel.Description),
                FillWeight = 24
            },
            new DataGridViewTextBoxColumn
            {
                HeaderText = "状态",
                DataPropertyName =
                    nameof(MaintenanceRowViewModel.Status),
                FillWeight = 12
            },
            new DataGridViewTextBoxColumn
            {
                HeaderText = "计划时间",
                DataPropertyName =
                    nameof(MaintenanceRowViewModel.PlannedTimeText),
                FillWeight = 22
            },
            new DataGridViewTextBoxColumn
            {
                HeaderText = "开始时间",
                DataPropertyName =
                    nameof(MaintenanceRowViewModel.StartedAtText),
                FillWeight = 18
            },
            new DataGridViewTextBoxColumn
            {
                HeaderText = "完成时间",
                DataPropertyName =
                    nameof(MaintenanceRowViewModel.CompletedAtText),
                FillWeight = 18
            },
            new DataGridViewTextBoxColumn
            {
                HeaderText = "维护结果",
                DataPropertyName =
                    nameof(MaintenanceRowViewModel.Result),
                FillWeight = 24
            });
    }

    private void LoadMaintenanceAlarmOptions()
    {
        if (_deviceRepository is null ||
            _alarmRepository is null ||
            _maintenanceRepository is null)
        {
            return;
        }

        var devices = GetVisibleDevices();

        var activeSourceAlarmIds = devices
            .SelectMany(device =>
                _maintenanceRepository
                    .GetActiveOrders(device.DeviceId))
            .Where(order =>
                order.SourceAlarmId.HasValue)
            .Select(order =>
                order.SourceAlarmId!.Value)
            .ToHashSet();

        var options =
            new List<MaintenanceAlarmOption>();

        foreach (var device in devices)
        {
            var alarm =
                _alarmRepository.GetActiveAlarm(
                    device.DeviceId,
                    "Temperature");

            if (alarm is null ||
                alarm.Level < AlarmLevel.High ||
                alarm.Status == AlarmStatus.Cleared ||
                activeSourceAlarmIds.Contains(
                    alarm.AlarmId))
            {
                continue;
            }

            options.Add(
                new MaintenanceAlarmOption(
                    alarm,
                    $"{device.DeviceCode} - " +
                    $"{alarm.Level} - " +
                    alarm.Message));
        }

        cmbMaintenanceAlarm.DisplayMember =
            nameof(MaintenanceAlarmOption.DisplayName);

        cmbMaintenanceAlarm.DataSource = null;
        cmbMaintenanceAlarm.DataSource = options;

        UpdateMaintenanceButtonStates();
    }

    private void LoadMaintenanceOrderList()
    {
        if (_deviceRepository is null ||
            _maintenanceRepository is null)
        {
            return;
        }

        var rows =
            new List<MaintenanceRowViewModel>();

        foreach (var device in GetVisibleDevices())
        {
            foreach (var order in
                     _maintenanceRepository
                         .GetActiveOrders(
                             device.DeviceId))
            {
                rows.Add(
                    new MaintenanceRowViewModel(
                        order,
                        $"{device.DeviceCode} - " +
                        device.DeviceName));
            }
        }

        rows = rows
            .OrderByDescending(row =>
                row.Order.CreatedAt)
            .ToList();

        dgvMaintenance.DataSource = null;
        dgvMaintenance.DataSource = rows;

        foreach (DataGridViewRow gridRow
                 in dgvMaintenance.Rows)
        {
            if (gridRow.DataBoundItem
                is not MaintenanceRowViewModel row)
            {
                continue;
            }

            gridRow.DefaultCellStyle.BackColor =
                row.Order.Status switch
                {
                    MaintenanceStatus.Created =>
                        Color.LightYellow,

                    MaintenanceStatus.Assigned =>
                        Color.LightBlue,

                    MaintenanceStatus.InProgress =>
                        Color.PeachPuff,

                    MaintenanceStatus.Completed =>
                        Color.LightGreen,

                    _ => Color.White
                };
        }

        UpdateMaintenanceButtonStates();
    }

    private void BtnCreateMaintenance_Click(
        object? sender,
        EventArgs e)
    {
        if (!EnsurePermission(
        SystemPermission.CreateMaintenanceOrder,
        "创建维护工单"))
        {
            return;
        }
        if (_maintenanceWorkflowService is null ||
            cmbMaintenanceAlarm.SelectedItem
                is not MaintenanceAlarmOption option ||
            cmbMaintenanceAssignee.SelectedItem
                is not MaintenanceAssigneeOption assignee)
        {
            return;
        }

        var description =
            txtMaintenanceDescription.Text.Trim();

        if (string.IsNullOrWhiteSpace(description))
        {
            MessageBox.Show(
                "请输入维护工单说明。",
                "提示",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        DateTime? plannedStart = null;
        DateTime? plannedEnd = null;

        if (chkMaintenancePlan.Checked)
        {
            plannedStart = ToUtc(
                dtpMaintenancePlanStart.Value);

            plannedEnd = ToUtc(
                dtpMaintenancePlanEnd.Value);
        }

        try
        {
            var order =
                _maintenanceWorkflowService
                    .CreateFromAlarm(
                        option.Alarm,
                        assignee.UserId,
                        description,
                        plannedStart,
                        plannedEnd);

            AppendRuntimeLog(
                $"维护工单{order.OrderId}已创建，" +
                $"来源报警={option.Alarm.AlarmId}。");

            RecordAudit(
                AuditActionTypes.CreateMaintenanceOrder,
                AuditTargetTypes.MaintenanceOrder,
                order.OrderId,
                $"创建维修工单；" +
                $"工单ID={order.OrderId}；" +
                $"设备ID={option.Alarm.DeviceId}；" +
                $"来源告警ID={option.Alarm.AlarmId}；" +
                $"负责人ID={assignee.UserId}；" +
                $"说明={description}。");

            txtMaintenanceDescription.Clear();

            LoadMaintenanceAlarmOptions();
            LoadMaintenanceOrderList();
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                $"创建维护工单失败：{exception.Message}",
                "维护管理",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void BtnAssignMaintenance_Click(
        object? sender,
        EventArgs e)
    {
        if (!EnsurePermission(
        SystemPermission.ProcessMaintenanceOrder,
        "处理维护工单"))
        {
            return;
        }
        var order = GetSelectedMaintenanceOrder();

        if (order is null ||
            _maintenanceWorkflowService is null ||
            cmbMaintenanceAssignee.SelectedItem
                is not MaintenanceAssigneeOption assignee ||
            assignee.UserId is not int assigneeId)
        {
            MessageBox.Show(
                "请选择工单和维护负责人。",
                "提示",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        var confirmation = MessageBox.Show(
            $"确定将维护工单{order.OrderId}指派给" +
            $"{assignee.DisplayName}吗？",
            "确认指派",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2);

        if (confirmation != DialogResult.Yes)
        {
            return;
        }

        if (!_maintenanceWorkflowService.Assign(
                order,
                assigneeId))
        {
            MessageBox.Show(
                "当前工单状态不能执行指派。",
                "提示",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        AppendRuntimeLog(
            $"维护工单{order.OrderId}已指派。");
        RecordAudit(
            AuditActionTypes.AssignMaintenanceOrder,
            AuditTargetTypes.MaintenanceOrder,
            order.OrderId,
            $"指派维修工单；" +
            $"工单ID={order.OrderId}；" +
            $"设备ID={order.DeviceId}；" +
            $"负责人ID={assigneeId}。");

        LoadMaintenanceOrderList();
    }

    private void BtnStartMaintenance_Click(
        object? sender,
        EventArgs e)
    {
        if (!EnsurePermission(
        SystemPermission.ProcessMaintenanceOrder,
        "处理维护工单"))
        {
            return;
        }
        var order = GetSelectedMaintenanceOrder();

        if (order is null ||
            _maintenanceWorkflowService is null)
        {
            return;
        }

        var confirmation = MessageBox.Show(
            "请确认设备已经停止运行，是否开始维护？",
            "开始维护",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2);

        if (confirmation != DialogResult.Yes)
        {
            return;
        }

        if (!_maintenanceWorkflowService.Start(order))
        {
            MessageBox.Show(
                "无法开始维护，请检查工单状态和设备状态。",
                "维护管理",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        if (_isMonitoring &&
            TryGetSelectedDeviceCode(
                out var monitoredDeviceCode,
                showMessage: false) &&
            GetDeviceIdByCode(monitoredDeviceCode) ==
                order.DeviceId)
        {
            StopContinuousMonitoring(
                "设备进入维护状态，连续监控已自动停止。");

            RecordAudit(
                AuditActionTypes.StopMonitoring,
                AuditTargetTypes.Device,
                order.DeviceId,
                "设备进入维护状态，系统自动停止连续监控。");
        }

        AppendRuntimeLog(
            $"维护工单{order.OrderId}已开始，" +
            "设备进入Maintenance状态。");

        RecordAudit(
            AuditActionTypes.StartMaintenanceOrder,
            AuditTargetTypes.MaintenanceOrder,
            order.OrderId,
            $"开始维修；" +
            $"工单ID={order.OrderId}；" +
            $"设备ID={order.DeviceId}；" +
            $"负责人ID={order.AssigneeId?.ToString() ?? "未设置"}。");

        LoadDeviceList();
        LoadMaintenanceOrderList();
    }

    private void BtnCompleteMaintenance_Click(
        object? sender,
        EventArgs e)
    {
        if (!EnsurePermission(
        SystemPermission.ProcessMaintenanceOrder,
        "处理维护工单"))
        {
            return;
        }
        var order = GetSelectedMaintenanceOrder();

        if (order is null ||
            _maintenanceWorkflowService is null)
        {
            return;
        }

        var result =
            txtMaintenanceResult.Text.Trim();

        if (string.IsNullOrWhiteSpace(result))
        {
            MessageBox.Show(
                "请输入维护结果。",
                "提示",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        var confirmation = MessageBox.Show(
            $"确定将维护工单{order.OrderId}标记为完成吗？\n" +
            $"维护结果：{result}",
            "确认完成维修",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2);

        if (confirmation != DialogResult.Yes)
        {
            return;
        }

        if (!_maintenanceWorkflowService.Complete(
                order,
                result))
        {
            MessageBox.Show(
                "当前工单状态不能完成。",
                "维护管理",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        AppendRuntimeLog(
            $"维护工单{order.OrderId}已完成。");

        RecordAudit(
            AuditActionTypes.CompleteMaintenanceOrder,
            AuditTargetTypes.MaintenanceOrder,
            order.OrderId,
            $"完成维修；" +
            $"工单ID={order.OrderId}；" +
            $"设备ID={order.DeviceId}；" +
            $"维修结果={result}。");

        txtMaintenanceResult.Clear();
        LoadMaintenanceOrderList();
    }

    private void BtnCloseMaintenance_Click(
        object? sender,
        EventArgs e)
    {
        if (!EnsurePermission(
        SystemPermission.ProcessMaintenanceOrder,
        "处理维护工单"))
        {
            return;
        }
        var order = GetSelectedMaintenanceOrder();

        if (order is null ||
            _maintenanceWorkflowService is null)
        {
            return;
        }

        if (!chkVerificationPassed.Checked)
        {
            MessageBox.Show(
                "关闭工单前必须确认维修后验证通过。",
                "提示",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        var confirmation = MessageBox.Show(
            $"确定关闭维护工单{order.OrderId}并将设备恢复为" +
            "Idle状态吗？\n关闭后不能继续处理该工单。",
            "确认关闭工单",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);

        if (confirmation != DialogResult.Yes)
        {
            return;
        }

        if (!_maintenanceWorkflowService.Close(
                order,
                verificationPassed: true))
        {
            MessageBox.Show(
                "无法关闭工单，请检查工单和设备状态。",
                "维护管理",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        AppendRuntimeLog(
            $"维护工单{order.OrderId}已关闭，" +
            "设备恢复为Idle状态。");

        RecordAudit(
            AuditActionTypes.CloseMaintenanceOrder,
            AuditTargetTypes.MaintenanceOrder,
            order.OrderId,
            $"关闭维修工单；" +
            $"工单ID={order.OrderId}；" +
            $"设备ID={order.DeviceId}；" +
            "维修后验证结果=通过。");

        chkVerificationPassed.Checked = false;

        LoadDeviceList();
        LoadMaintenanceAlarmOptions();
        LoadMaintenanceOrderList();
    }

    private MaintenanceOrder?
    GetSelectedMaintenanceOrder()
    {
        return dgvMaintenance.CurrentRow?
            .DataBoundItem
            is MaintenanceRowViewModel row
                ? row.Order
                : null;
    }

    private void UpdateMaintenanceButtonStates()
    {
        MaintenanceOrder? order =
            GetSelectedMaintenanceOrder();

        bool canCreate =
            HasPermission(
                SystemPermission.CreateMaintenanceOrder);

        bool canProcess =
            HasPermission(
                SystemPermission.ProcessMaintenanceOrder);

        btnCreateMaintenance.Enabled =
            canCreate &&
            cmbMaintenanceAlarm.SelectedItem
                is MaintenanceAlarmOption;

        btnAssignMaintenance.Enabled =
            canProcess &&
            order?.Status ==
                MaintenanceStatus.Created;

        btnStartMaintenance.Enabled =
            canProcess &&
            order?.Status ==
                MaintenanceStatus.Assigned;

        btnCompleteMaintenance.Enabled =
            canProcess &&
            order?.Status ==
                MaintenanceStatus.InProgress;

        btnCloseMaintenance.Enabled =
            canProcess &&
            order?.Status ==
                MaintenanceStatus.Completed;

        cmbMaintenanceAlarm.Enabled = canCreate;

        cmbMaintenanceAssignee.Enabled =
            canCreate || canProcess;

        txtMaintenanceDescription.Enabled =
            canCreate;

        chkMaintenancePlan.Enabled =
            canCreate;

        dtpMaintenancePlanStart.Enabled =
            canCreate &&
            chkMaintenancePlan.Checked;

        dtpMaintenancePlanEnd.Enabled =
            canCreate &&
            chkMaintenancePlan.Checked;

        txtMaintenanceResult.Enabled =
            canProcess &&
            order?.Status ==
                MaintenanceStatus.InProgress;

        chkVerificationPassed.Enabled =
            canProcess &&
            order?.Status ==
                MaintenanceStatus.Completed;
    }

    /// <summary>
    /// 统一权限守卫
    /// </summary>
    /// <param name="permission"></param>
    /// <param name="operationName"></param>
    /// <returns></returns>
    private bool EnsurePermission(
        SystemPermission permission,
        string operationName)
    {
        if (HasPermission(permission))
        {
            return true;
        }

        ShowAccessDenied(operationName);
        return false;
    }
    private void RecordAudit(
        string actionType,
        string targetType,
        int? targetId,
        string detail)
    {
        if (_operationAuditService is null ||
            _userSession is null)
        {
            return;
        }

        try
        {
            _operationAuditService.Record(
                _userSession,
                actionType,
                targetType,
                targetId,
                detail);
            LoadOperationLogs();
        }
        catch (Exception exception)
        {
            AppendRuntimeLog(
                $"审计日志写入失败：" +
                $"{exception.Message}");

            MessageBox.Show(
                "业务操作已经完成，但审计日志写入失败。" +
                Environment.NewLine +
                exception.Message,
                "审计日志警告",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }
    private bool EnsureCanManageBooking(
        Booking booking,
        string operationName)
    {
        if (_userSession is not null &&
            _authorizationService is not null &&
            _authorizationService.CanManageBooking(
                _userSession,
                booking))
        {
            return true;
        }

        ShowAccessDenied(operationName);
        return false;
    }

    private void ShowAccessDenied(
        string operationName)
    {
        string userName =
            _userSession?.UserName ?? "未登录用户";

        AppendRuntimeLog(
            $"权限拒绝：用户={userName}，" +
            $"操作={operationName}。");

        MessageBox.Show(
            $"当前用户无权执行“{operationName}”。",
            "权限不足",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
    }

    /// <summary>
    /// 报警页面初始化
    /// </summary>
    private void ConfigureAlarmPage()
    {
        ConfigureAlarmGrid();

        cmbAlarmUser.DisplayMember =
            nameof(AlarmUserOption.DisplayName);

        cmbAlarmUser.DataSource =
            new List<AlarmUserOption>
            {
            new(
                1,
                "admin（管理员）"),

            new(
                2,
                "operator01（操作员）"),

            new(
                3,
                "maintainer01（维护员）")
            };

        // 默认由操作员确认
        cmbAlarmUser.SelectedIndex = 1;

        btnAcknowledgeAlarm.Click +=
            BtnAcknowledgeAlarm_Click;

        btnRefreshAlarms.Click +=
            (_, _) => LoadAlarmList();

        dgvAlarms.SelectionChanged +=
            (_, _) => UpdateAlarmButtonStates();

        UpdateAlarmButtonStates();
    }
    /// <summary>
    /// 设备编号辅助方法
    /// </summary>
    /// <param name="deviceCode"></param>
    /// <returns></returns>
    private int? GetDeviceIdByCode(
    string deviceCode)
    {
        if (_deviceRepository is null ||
            string.IsNullOrWhiteSpace(deviceCode))
        {
            return null;
        }

        return _deviceRepository
            .GetByCode(deviceCode)
            ?.DeviceId;
    }

    /// <summary>
    /// 配置报警列表列
    /// </summary>
    private void ConfigureAlarmGrid()
    {
        dgvAlarms.AutoGenerateColumns = false;
        dgvAlarms.Columns.Clear();

        dgvAlarms.Columns.AddRange(
            new DataGridViewTextBoxColumn
            {
                Name = "colAlarmId",
                HeaderText = "报警编号",
                DataPropertyName =
                    nameof(AlarmRowViewModel.AlarmId),
                FillWeight = 10
            },
            new DataGridViewTextBoxColumn
            {
                Name = "colAlarmDevice",
                HeaderText = "设备",
                DataPropertyName =
                    nameof(AlarmRowViewModel.DeviceText),
                FillWeight = 20
            },
            new DataGridViewTextBoxColumn
            {
                Name = "colAlarmType",
                HeaderText = "类型",
                DataPropertyName =
                    nameof(AlarmRowViewModel.AlarmType),
                FillWeight = 12
            },
            new DataGridViewTextBoxColumn
            {
                Name = "colAlarmLevel",
                HeaderText = "等级",
                DataPropertyName =
                    nameof(AlarmRowViewModel.Level),
                FillWeight = 10
            },
            new DataGridViewTextBoxColumn
            {
                Name = "colAlarmMessage",
                HeaderText = "报警内容",
                DataPropertyName =
                    nameof(AlarmRowViewModel.Message),
                FillWeight = 28
            },
            new DataGridViewTextBoxColumn
            {
                Name = "colAlarmStatus",
                HeaderText = "状态",
                DataPropertyName =
                    nameof(AlarmRowViewModel.Status),
                FillWeight = 12
            },
            new DataGridViewTextBoxColumn
            {
                Name = "colAlarmOccurredAt",
                HeaderText = "首次发生",
                DataPropertyName =
                    nameof(AlarmRowViewModel.OccurredAtText),
                FillWeight = 18
            },
            new DataGridViewTextBoxColumn
            {
                Name = "colAlarmLastOccurredAt",
                HeaderText = "最近发生",
                DataPropertyName =
                    nameof(AlarmRowViewModel.LastOccurredAtText),
                FillWeight = 18
            },
            new DataGridViewTextBoxColumn
            {
                Name = "colAlarmOccurrenceCount",
                HeaderText = "次数",
                DataPropertyName =
                    nameof(AlarmRowViewModel.OccurrenceCount),
                FillWeight = 8
            },
            new DataGridViewTextBoxColumn
            {
                Name = "colAlarmAcknowledgedBy",
                HeaderText = "确认人",
                DataPropertyName =
                    nameof(AlarmRowViewModel.AcknowledgedByText),
                FillWeight = 16
            },
            new DataGridViewTextBoxColumn
            {
                Name = "colAlarmAcknowledgedAt",
                HeaderText = "确认时间",
                DataPropertyName =
                    nameof(AlarmRowViewModel.AcknowledgedAtText),
                FillWeight = 18
            });
    }

    /// <summary>
    /// 报警列表加载方法
    /// </summary>
    private void LoadAlarmList()
    {
        if (_deviceRepository is null ||
            _alarmRepository is null)
        {
            return;
        }

        try
        {
            var rows =
                new List<AlarmRowViewModel>();

            foreach (var device in GetVisibleDevices())
            {
                var alarm =
                    _alarmRepository.GetActiveAlarm(
                        device.DeviceId,
                        "Temperature");

                if (alarm is null)
                {
                    continue;
                }

                rows.Add(
                    new AlarmRowViewModel(
                        alarm,
                        $"{device.DeviceCode} - " +
                        device.DeviceName));
            }

            rows = rows
                .OrderByDescending(row =>
                    row.Alarm.Level)
                .ThenByDescending(row =>
                    row.Alarm.LastOccurredAt)
                .ToList();

            dgvAlarms.DataSource = null;
            dgvAlarms.DataSource = rows;

            lblActiveAlarmCount.Text =
                $"未解除报警：{rows.Count}";

            ApplyAlarmRowStyles();
            UpdateAlarmButtonStates();
        }
        catch (Exception exception)
        {
            AppendRuntimeLog(
                $"加载报警列表失败：{exception.Message}");

            MessageBox.Show(
                $"无法加载报警列表：{exception.Message}",
                "报警数据错误",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
    /// <summary>
    /// 按报警等级设置行颜色
    /// </summary>
    private void ApplyAlarmRowStyles()
    {
        foreach (DataGridViewRow gridRow
                 in dgvAlarms.Rows)
        {
            if (gridRow.DataBoundItem
                is not AlarmRowViewModel row)
            {
                continue;
            }

            gridRow.DefaultCellStyle.BackColor =
                row.Alarm.Level switch
                {
                    AlarmLevel.Warning =>
                        Color.LemonChiffon,

                    AlarmLevel.High =>
                        Color.MistyRose,

                    AlarmLevel.Emergency =>
                        Color.LightCoral,

                    _ => Color.White
                };

            if (row.Alarm.Status ==
                AlarmStatus.Acknowledged)
            {
                gridRow.DefaultCellStyle.ForeColor =
                    Color.DimGray;
            }
        }
    }

    /// <summary>
    /// 确认报警事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BtnAcknowledgeAlarm_Click(
        object? sender,
        EventArgs e)
    {
        if (!EnsurePermission(
        SystemPermission.AcknowledgeAlarm,
        "确认报警"))
        {
            return;
        }
        if (_alarmApplicationService is null ||
            _userSession is null ||
            dgvAlarms.CurrentRow?.DataBoundItem
            is not AlarmRowViewModel selected)
        {
            return;
        }

        var confirmation = MessageBox.Show(
            $"确定由{_userSession.UserName}确认报警" +
            $"{selected.AlarmId}吗？",
            "确认报警",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2);

        if (confirmation != DialogResult.Yes)
        {
            return;
        }

        try
        {
            var succeeded =
                _alarmApplicationService
                    .AcknowledgeAlarm(
                        selected.Alarm,
                        _userSession.UserId);

            if (!succeeded)
            {
                MessageBox.Show(
                    "该报警已经确认或已经解除。",
                    "确认失败",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                LoadAlarmList();
                return;
            }

            AppendRuntimeLog(
                $"报警{selected.AlarmId}已由" +
                $"{_userSession.UserName}确认。");

            RecordAudit(
                AuditActionTypes.AcknowledgeAlarm,
                AuditTargetTypes.Alarm,
                selected.AlarmId,
                $"确认告警；" +
                $"告警ID={selected.AlarmId}；" +
                $"设备ID={selected.Alarm.DeviceId}；" +
                $"告警类型={selected.Alarm.AlarmType}；" +
                $"告警等级={selected.Alarm.Level}。");

            LoadAlarmList();
        }
        catch (Exception exception)
        {
            AppendRuntimeLog(
                $"确认报警失败：{exception.Message}");

            MessageBox.Show(
                $"确认报警失败：{exception.Message}",
                "报警处理错误",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void UpdateAlarmButtonStates()
    {
        bool canAcknowledge =
            HasPermission(
                SystemPermission.AcknowledgeAlarm);

        btnAcknowledgeAlarm.Enabled =
            canAcknowledge &&
            dgvAlarms.CurrentRow?.DataBoundItem
                is AlarmRowViewModel row &&
            row.Alarm.Status ==
                AlarmStatus.Active;

        // 报警确认人始终采用当前登录用户
        cmbAlarmUser.Enabled = false;
    }

    //配置预约列表列
    private void ConfigureBookingGrid()
    {
        dgvBookings.AutoGenerateColumns = false;
        dgvBookings.Columns.Clear();

        dgvBookings.Columns.AddRange(
            new DataGridViewTextBoxColumn
            {
                Name = "colBookingId",
                HeaderText = "预约编号",
                DataPropertyName =
                    nameof(BookingRowViewModel.BookingId),
                FillWeight = 12
            },
            new DataGridViewTextBoxColumn
            {
                Name = "colBookingDevice",
                HeaderText = "设备",
                DataPropertyName =
                    nameof(BookingRowViewModel.DeviceText),
                FillWeight = 24
            },
            new DataGridViewTextBoxColumn
            {
                Name = "colBookingUser",
                HeaderText = "预约人",
                DataPropertyName =
                    nameof(BookingRowViewModel.UserText),
                FillWeight = 18
            },
            new DataGridViewTextBoxColumn
            {
                Name = "colBookingStart",
                HeaderText = "开始时间",
                DataPropertyName =
                    nameof(BookingRowViewModel.StartTimeText),
                FillWeight = 20
            },
            new DataGridViewTextBoxColumn
            {
                Name = "colBookingEnd",
                HeaderText = "结束时间",
                DataPropertyName =
                    nameof(BookingRowViewModel.EndTimeText),
                FillWeight = 20
            },
            new DataGridViewTextBoxColumn
            {
                Name = "colBookingPurpose",
                HeaderText = "用途",
                DataPropertyName =
                    nameof(BookingRowViewModel.Purpose),
                FillWeight = 28
            },
            new DataGridViewTextBoxColumn
            {
                Name = "colBookingStatus",
                HeaderText = "状态",
                DataPropertyName =
                    nameof(BookingRowViewModel.Status),
                FillWeight = 12
            });
    }

    //预约页面初始化
    private void ConfigureBookingPage()
    {
        ConfigureBookingGrid();

        var deviceOptions = GetVisibleDevices()
            .OrderBy(device => device.DeviceCode)
            .Select(device =>
                new BookingDeviceOption(
                    device.DeviceId,
                    $"{device.DeviceCode} - " +
                    device.DeviceName))
            .ToList();

        cmbBookingDevice.DisplayMember =
            nameof(BookingDeviceOption.DisplayName);

        cmbBookingDevice.ValueMember =
            nameof(BookingDeviceOption.DeviceId);

        cmbBookingDevice.DataSource =
            deviceOptions;

        var users = new List<BookingUserOption>
    {
        new(
            1,
            "admin",
            UserRole.Administrator,
            "admin（管理员）"),

        new(
            2,
            "operator01",
            UserRole.Operator,
            "operator01（操作员）"),

        new(
            3,
            "maintainer01",
            UserRole.Maintainer,
            "maintainer01（维护员）")
    };

        cmbBookingUser.DisplayMember =
            nameof(BookingUserOption.DisplayName);

        cmbBookingUser.DataSource = users;

        // 默认使用operator01
        cmbBookingUser.SelectedIndex = 1;

        dtpBookingStart.Format =
            DateTimePickerFormat.Custom;

        dtpBookingStart.CustomFormat =
            "yyyy-MM-dd HH:mm";

        dtpBookingEnd.Format =
            DateTimePickerFormat.Custom;

        dtpBookingEnd.CustomFormat =
            "yyyy-MM-dd HH:mm";

        var defaultStart =
            DateTime.Now.AddHours(1);

        defaultStart = new DateTime(
            defaultStart.Year,
            defaultStart.Month,
            defaultStart.Day,
            defaultStart.Hour,
            defaultStart.Minute,
            0);

        dtpBookingStart.Value =
            defaultStart;

        dtpBookingEnd.Value =
            defaultStart.AddHours(1);

        dtpBookingStart.ValueChanged += (_, _) =>
        {
            if (dtpBookingEnd.Value <=
                dtpBookingStart.Value)
            {
                dtpBookingEnd.Value =
                    dtpBookingStart.Value.AddHours(1);
            }
        };

        btnCreateBooking.Click +=
            BtnCreateBooking_Click;

        btnCancelBooking.Click +=
            BtnCancelBooking_Click;

        btnRefreshBookings.Click +=
            (_, _) => RefreshBusinessViews();

        dgvBookings.SelectionChanged +=
            (_, _) => UpdateBookingButtonStates();

        UpdateBookingButtonStates();
    }

    private void BtnCreateBooking_Click(
        object? sender,
        EventArgs e)
    {
        if (!EnsurePermission(
        SystemPermission.CreateBooking,
        "创建预约"))
        {
            return;
        }
        if (_bookingApplicationService is null)
        {
            return;
        }

        if (cmbBookingDevice.SelectedValue
            is not int deviceId)
        {
            MessageBox.Show(
                "请选择预约设备。",
                "提示",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        if (cmbBookingUser.SelectedItem
            is not BookingUserOption user)
        {
            MessageBox.Show(
                "请选择预约用户。",
                "提示",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }
        if (_userSession is null ||
            (!HasPermission(
                SystemPermission.ManageAllBookings) &&
                user.UserId != _userSession.UserId))
        {
            ShowAccessDenied(
                "为其他用户创建预约");
            return;
        }

        var purpose =
            txtBookingPurpose.Text.Trim();

        if (string.IsNullOrWhiteSpace(purpose))
        {
            MessageBox.Show(
                "请输入预约用途。",
                "提示",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            txtBookingPurpose.Focus();
            return;
        }

        var candidate = new Booking
        {
            DeviceId = deviceId,
            UserId = user.UserId,
            StartTime = ToUtc(
                dtpBookingStart.Value),
            EndTime = ToUtc(
                dtpBookingEnd.Value),
            Purpose = purpose
        };

        try
        {
            var result =
                _bookingApplicationService
                    .CreateBooking(
                        candidate,
                        user.Role);

            if (!result.IsValid)
            {
                AppendRuntimeLog(
                    $"预约创建失败：" +
                    $"{result.ErrorCode}，" +
                    result.Message);

                MessageBox.Show(
                    $"{result.Message}" +
                    Environment.NewLine +
                    $"错误代码：{result.ErrorCode}",
                    "预约失败",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            AppendRuntimeLog(
                $"预约创建成功：设备ID={deviceId}，" +
                $"预约编号={candidate.BookingId}。");

            RecordAudit(
                AuditActionTypes.CreateBooking,
                AuditTargetTypes.Booking,
                candidate.BookingId,
                $"创建预约；" +
                $"设备ID={candidate.DeviceId}；" +
                $"预约用户ID={candidate.UserId}；" +
                $"开始时间={candidate.StartTime:O}；" +
                $"结束时间={candidate.EndTime:O}；" +
                $"用途={candidate.Purpose}。");

            txtBookingPurpose.Clear();
            RefreshBusinessViews();

            MessageBox.Show(
                "预约创建成功。",
                "预约管理",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception exception)
        {
            AppendRuntimeLog(
                $"预约创建异常：{exception.Message}");

            MessageBox.Show(
                $"预约创建失败：{exception.Message}",
                "预约错误",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    //预约列表加载
    private void LoadBookingList()
    {
        if (_deviceRepository is null ||
            _bookingRepository is null)
        {
            return;
        }

        try
        {
            var devices = GetVisibleDevices();
            var rows = devices
                .SelectMany(device =>
                    _bookingRepository
                        .GetActiveBookings(device.DeviceId)
                        .Select(booking =>
                            new BookingRowViewModel(
                                booking,
                                $"{device.DeviceCode} - " +
                                device.DeviceName,
                                GetBookingUserText(
                                    booking.UserId))))
                .OrderBy(row =>
                    row.Booking.StartTime)
                .ToList();

            _loadedBookings.Clear();

            foreach (var row in rows)
            {
                var booking = row.Booking;

                _loadedBookings[booking.BookingId] =
                    booking;
            }

            dgvBookings.DataSource = null;
            dgvBookings.DataSource = rows;

            UpdateBookingButtonStates();
        }
        catch (Exception exception)
        {
            AppendRuntimeLog(
                $"加载预约列表失败：{exception.Message}");

            MessageBox.Show(
                $"无法加载预约列表：{exception.Message}",
                "预约数据错误",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    // 预约取消
    private void BtnCancelBooking_Click(
        object? sender,
        EventArgs e)
    {
        if (_bookingApplicationService is null ||
            dgvBookings.CurrentRow?.DataBoundItem
                is not BookingRowViewModel selected)
        {
            return;
        }

        if (!EnsureCanManageBooking(
                selected.Booking,
                "取消预约"))
        {
            return;
        }

        var confirmation = MessageBox.Show(
            $"确定取消预约{selected.BookingId}吗？",
            "取消预约",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2);

        if (confirmation != DialogResult.Yes)
        {
            return;
        }

        try
        {
            var succeeded =
                _bookingApplicationService
                    .CancelBooking(
                        selected.Booking);

            if (!succeeded)
            {
                MessageBox.Show(
                    "当前预约状态不允许取消。",
                    "取消失败",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            AppendRuntimeLog(
                $"预约{selected.BookingId}已取消。");
            RecordAudit(
                AuditActionTypes.CancelBooking,
                AuditTargetTypes.Booking,
                selected.BookingId,
                $"取消预约；" +
                $"预约ID={selected.BookingId}；" +
                $"设备ID={selected.Booking.DeviceId}；" +
                $"预约用户ID={selected.Booking.UserId}。");

            RefreshBusinessViews();
            UpdateBookingActionButtons();
        }
        catch (Exception exception)
        {
            AppendRuntimeLog(
                $"取消预约失败：{exception.Message}");

            MessageBox.Show(
                $"取消预约失败：{exception.Message}",
                "预约错误",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void UpdateBookingButtonStates()
    {
        UpdateBookingActionButtons();
    }

    protected override void OnFormClosing(
        FormClosingEventArgs e)
    {
        if (_isClosing)
        {
            base.OnFormClosing(e);
            return;
        }

        if (_isCollecting)
        {
            e.Cancel = true;

            MessageBox.Show(
                "当前正在采集设备数据，请等待本次采集完成后再关闭窗口。",
                "暂时无法关闭",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            return;
        }

        if (!LogoutRequested &&
            e.CloseReason == CloseReason.UserClosing)
        {
            var confirmation = MessageBox.Show(
                "确定退出工业设备运行、预约与维护管理系统吗？",
                "退出系统",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (confirmation != DialogResult.Yes)
            {
                e.Cancel = true;
                return;
            }
        }

        _isClosing = true;

        _monitorTimer.Stop();
        _clockTimer.Stop();
        _isMonitoring = false;

        base.OnFormClosing(e);
    }

    protected override void OnFormClosed(
        FormClosedEventArgs e)
    {
        _monitorTimer.Stop();
        _monitorTimer.Tick -=
            MonitorTimer_Tick;
        _monitorTimer.Dispose();

        _clockTimer.Stop();
        _clockTimer.Dispose();

        base.OnFormClosed(e);
    }

    private void ConfigureDataSourceSelector()
    {
        cmbDataSource.Items.Clear();
        cmbDataSource.Items.Add("C#模拟器");
        cmbDataSource.Items.Add("OpenPLC");
        cmbDataSource.SelectedIndex = 0;

        UpdateDataSourceStatus();

        cmbDataSource.SelectedIndexChanged += (_, _) =>
        {
            UpdateDataSourceStatus();
            UpdateControlStates();
        };
    }

    private void UpdateDataSourceStatus()
    {
        tslDataSource.Text =
            $"数据源：{cmbDataSource.SelectedItem}";
    }

    private void ConfigureDeviceGrid()
    {
        dgvDevices.AutoGenerateColumns = false;
        dgvDevices.Columns.Clear();

        dgvDevices.Columns.AddRange(
            new DataGridViewTextBoxColumn
            {
                Name = "colDeviceCode",
                HeaderText = "设备编码",
                DataPropertyName =
                    nameof(Device.DeviceCode),
                FillWeight = 18
            },
            new DataGridViewTextBoxColumn
            {
                Name = "colDeviceName",
                HeaderText = "设备名称",
                DataPropertyName =
                    nameof(Device.DeviceName),
                FillWeight = 24
            },
            new DataGridViewTextBoxColumn
            {
                Name = "colDeviceType",
                HeaderText = "设备类型",
                DataPropertyName =
                    nameof(Device.DeviceType),
                FillWeight = 18
            },
            new DataGridViewTextBoxColumn
            {
                Name = "colDeviceStatus",
                HeaderText = "状态",
                DataPropertyName =
                    nameof(Device.Status),
                FillWeight = 14
            },
            new DataGridViewCheckBoxColumn
            {
                Name = "colEnabled",
                HeaderText = "启用",
                DataPropertyName =
                    nameof(Device.Enabled),
                FillWeight = 10
            },
            new DataGridViewTextBoxColumn
            {
                Name = "colLastCommunicationTime",
                HeaderText = "最后通信时间",
                DataPropertyName =
                    nameof(Device.LastCommunicationTime),
                FillWeight = 25,
                DefaultCellStyle =
                    new DataGridViewCellStyle
                    {
                        Format = "yyyy-MM-dd HH:mm:ss"
                    }
            });
    }

    private void LoadDeviceList(
        string? selectedDeviceCode = null)
    {
        if (_isLoadingDeviceList)
        {
            return;
        }

        _isLoadingDeviceList = true;
        try
        {
            selectedDeviceCode ??=
                cmbDevice.SelectedValue as string;

            var devices = GetVisibleDevices()
                .OrderBy(device => device.DeviceCode)
                .ToList();

            dgvDevices.DataSource = null;
            dgvDevices.DataSource = devices;

            cmbDevice.DataSource = null;
            cmbDevice.DisplayMember =
                nameof(Device.DeviceName);
            cmbDevice.ValueMember =
                nameof(Device.DeviceCode);
            cmbDevice.DataSource = devices.ToList();

            if (!string.IsNullOrWhiteSpace(
                    selectedDeviceCode))
            {
                cmbDevice.SelectedValue =
                    selectedDeviceCode;
            }

            tslConnection.Text =
                "数据库状态：已连接";

            if (devices.Count == 0)
            {
                AppendRuntimeLog(
                    "数据库中没有设备，请检查seed.sql。");
            }
        }
        catch (Exception exception)
        {
            tslConnection.Text =
                "数据库状态：加载失败";

            AppendRuntimeLog(
                $"加载设备失败：{exception.Message}");

            MessageBox.Show(
                $"无法加载设备列表：{exception.Message}",
                "数据读取错误",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            _isLoadingDeviceList = false;
        }
        UpdateControlStates();
    }

    private async void BtnCollectOnce_Click(
        object? sender,
        EventArgs e)
    {
        if (!EnsurePermission(
            SystemPermission.ControlSimulation,
            "单次采集设备数据"))
        {
            return;
        }
        if (!EnsureSelectedDataSourceAvailable() ||
            !TryGetSelectedDeviceCode(
                out var deviceCode) ||
            !EnsureDeviceCanBeMonitored(
                deviceCode))
        {
            return;
        }

        await CollectSelectedDeviceAsync(
            showErrorDialog: true);
    }

    private void DisplayMonitoringResult(
        DeviceMonitoringResult result)
    {
        var snapshot = result.Snapshot;

        lblTemperatureValue.Text =
            $"{snapshot.Temperature:F1} ℃";

        lblPressureValue.Text =
            $"{snapshot.Pressure:F2} MPa";

        lblVibrationValue.Text =
            $"{snapshot.Vibration:F2}";

        lblRunningValue.Text =
            snapshot.IsRunning ? "运行" : "停止";

        lblFaultValue.Text =
            snapshot.FaultSignal ? "有故障" : "正常";

        lblEmergencyValue.Text =
            snapshot.EmergencyStop ? "已触发" : "未触发";

        lblQualityValue.Text =
            snapshot.Quality.ToString();

        lblDeviceStatusValue.Text =
            result.DeviceStatus.ToString();

        lblCollectedAtValue.Text =
            snapshot.CollectedAt
                .ToLocalTime()
                .ToString("yyyy-MM-dd HH:mm:ss");

        lblFaultValue.ForeColor =
            snapshot.FaultSignal
                ? Color.Red
                : Color.DarkGreen;

        lblEmergencyValue.ForeColor =
            snapshot.EmergencyStop
                ? Color.Red
                : Color.DarkGreen;

        lblQualityValue.ForeColor =
            snapshot.Quality switch
            {
                DataQuality.Good => Color.DarkGreen,
                DataQuality.Uncertain => Color.DarkOrange,
                DataQuality.Bad => Color.Red,
                DataQuality.CommunicationLost => Color.Red,
                _ => SystemColors.ControlText
            };

        lblDeviceStatusValue.ForeColor =
            result.DeviceStatus switch
            {
                DeviceStatus.Running => Color.DarkGreen,
                DeviceStatus.Idle => Color.RoyalBlue,
                DeviceStatus.Maintenance => Color.DarkOrange,
                DeviceStatus.Fault => Color.Red,
                DeviceStatus.Offline => Color.Gray,
                _ => SystemColors.ControlText
            };

        if (result.Alarm is null)
        {
            lblAlarmValue.Text = "无";
            lblAlarmValue.ForeColor = Color.DarkGreen;
            lblTemperatureValue.ForeColor =
                SystemColors.ControlText;

            return;
        }

        if (result.Alarm.Status == AlarmStatus.Cleared)
        {
            lblAlarmValue.Text = "温度报警已解除";
            lblAlarmValue.ForeColor = Color.DarkGreen;
            lblTemperatureValue.ForeColor =
                SystemColors.ControlText;

            return;
        }

        lblAlarmValue.Text =
            $"{result.Alarm.Level}：" +
            result.Alarm.Message;

        lblAlarmValue.ForeColor =
            result.Alarm.Level switch
            {
                AlarmLevel.Warning => Color.DarkOrange,
                AlarmLevel.High => Color.OrangeRed,
                AlarmLevel.Emergency => Color.Red,
                _ => SystemColors.ControlText
            };

        lblTemperatureValue.ForeColor =
            lblAlarmValue.ForeColor;
    }

    private void UpdateControlStates()
    {
        bool canMonitor =
            HasPermission(
                SystemPermission.MonitorDevice);

        bool canControl =
            HasPermission(
                SystemPermission.ControlSimulation);

        bool hasDevice =
            cmbDevice.SelectedValue is string;

        bool dataSourceReady =
            SelectedMonitoringService is not null;

        btnCollectOnce.Enabled =
            canControl &&
            !_isCollecting &&
            !_isMonitoring &&
            dataSourceReady &&
            hasDevice;

        btnStartMonitoring.Enabled =
            canControl &&
            !_isCollecting &&
            !_isMonitoring &&
            dataSourceReady &&
            hasDevice;

        btnStopMonitoring.Enabled =
            canControl &&
            _isMonitoring;

        cmbDataSource.Enabled =
            canControl &&
            !_isCollecting &&
            !_isMonitoring;

        cmbDevice.Enabled =
            canMonitor &&
            !_isCollecting &&
            !_isMonitoring &&
            !IsOpenPlcSelected;

        cmbScenario.Enabled =
            canControl &&
            !_isCollecting &&
            !_isMonitoring &&
            IsSimulatorSelected;

        numSampleInterval.Enabled =
            canControl &&
            !_isCollecting &&
            !_isMonitoring;

        Cursor = _isCollecting
            ? Cursors.WaitCursor
            : Cursors.Default;
    }


    private void DgvDevices_SelectionChanged(
        object? sender,
        EventArgs e)
    {
        if (IsOpenPlcSelected)
        {
            return;
        }

        if (dgvDevices.CurrentRow?.DataBoundItem
            is Device device)
        {
            cmbDevice.SelectedValue =
                device.DeviceCode;
        }
    }

    private void UpdateCurrentTime()
    {
        tslCurrentTime.Text =
            $"当前时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
    }

    private void AppendRuntimeLog(string message)
    {
        txtRuntimeLog.AppendText(
            $"[{DateTime.Now:HH:mm:ss}] {message}" +
            Environment.NewLine);
    }

    private void HandleDataSourceChanged()
    {
        if (IsOpenPlcSelected &&
            !string.IsNullOrWhiteSpace(
                _openPlcDeviceCode))
        {
            cmbDevice.SelectedValue =
                _openPlcDeviceCode;

            tslDataSource.Text =
                $"数据源：OpenPLC " +
                $"(127.0.0.1:5020，" +
                $"设备：{_openPlcDeviceCode})";

            AppendRuntimeLog(
                $"已切换到OpenPLC数据源，" +
                $"映射设备为{_openPlcDeviceCode}。");
        }
        else
        {
            tslDataSource.Text =
                "数据源：C#模拟器";

            AppendRuntimeLog(
                "已切换到C#模拟器数据源。");
        }

        UpdateControlStates();
    }

    private async void BtnStartMonitoring_Click(
        object? sender,
        EventArgs e)
    {
        if (!EnsurePermission(
                SystemPermission.ControlSimulation,
                "启动连续监控"))
        {
            return;
        }

        if (!EnsureSelectedDataSourceAvailable() ||
            !TryGetSelectedDeviceCode(
                out var deviceCode) ||
            !EnsureDeviceCanBeMonitored(
                deviceCode))
        {
            return;
        }

        _monitorTimer.Interval =
            decimal.ToInt32(
                numSampleInterval.Value) * 1000;

        _isMonitoring = true;
        UpdateControlStates();

        AppendRuntimeLog(
            $"{cmbDataSource.SelectedItem}" +
            "连续监控已启动，" +
            $"采样周期为" +
            $"{numSampleInterval.Value}秒。");

        var succeeded =
            await CollectSelectedDeviceAsync(
                showErrorDialog: true);

        if (_isMonitoring && succeeded)
        {
            _monitorTimer.Start();

            RecordAudit(
                AuditActionTypes.StartMonitoring,
                AuditTargetTypes.Device,
                GetDeviceIdByCode(deviceCode),
                $"启动连续监控；" +
                $"设备代码={deviceCode}；" +
                $"数据源={cmbDataSource.SelectedItem}；" +
                $"采样周期=" +
                $"{numSampleInterval.Value}秒。");
        }
    }

    private void BtnStopMonitoring_Click(
        object? sender,
        EventArgs e)
    {
        if (!EnsurePermission(
                SystemPermission.ControlSimulation,
                "停止连续监控"))
        {
            return;
        }

        if (!_isMonitoring)
        {
            return;
        }

        TryGetSelectedDeviceCode(
            out var deviceCode,
            showMessage: false);

        var dataSourceText =
            cmbDataSource.SelectedItem?.ToString()
            ?? "未知数据源";

        StopContinuousMonitoring(
            "连续监控已由用户停止。");

        RecordAudit(
            AuditActionTypes.StopMonitoring,
            AuditTargetTypes.Device,
            GetDeviceIdByCode(deviceCode),
            $"停止连续监控；" +
            $"设备代码={deviceCode}；" +
            $"数据源={dataSourceText}。");
    }

    private async void MonitorTimer_Tick(
        object? sender,
        EventArgs e)
    {
        if (!_isMonitoring || _isCollecting)
        {
            return;
        }

        await CollectSelectedDeviceAsync(
            showErrorDialog: false);
    }

    private sealed record ScenarioOption(
    SimulationScenario Scenario,
    string DisplayName)
    {
        public override string ToString()
        {
            return DisplayName;
        }
    }

    private sealed record HistoryDeviceOption(
    int DeviceId,
    string DisplayText);

    private sealed record TelemetryHistoryRowViewModel(
        int TelemetryId,
        string CollectedAt,
        double Temperature,
        double Pressure,
        double Vibration,
        string IsRunning,
        string FaultSignal,
        string EmergencyStop,
        string Quality);

    private sealed record AuditLogRowViewModel(
    int LogId,
    string OccurredAtText,
    string UserText,
    string ActionType,
    string TargetType,
    string TargetIdText,
    string Detail);

    private void ConfigureHistoryPage()
    {
        if (_deviceRepository is null ||
            _telemetryRepository is null)
        {
            return;
        }

        var deviceOptions = GetVisibleDevices()
            .OrderBy(device => device.DeviceCode)
            .Select(device =>
                new HistoryDeviceOption(
                    device.DeviceId,
                    $"{device.DeviceCode} - " +
                    device.DeviceName))
            .ToList();

        cmbHistoryDevice.DataSource = null;
        cmbHistoryDevice.DisplayMember =
            nameof(HistoryDeviceOption.DisplayText);
        cmbHistoryDevice.ValueMember =
            nameof(HistoryDeviceOption.DeviceId);
        cmbHistoryDevice.DataSource = deviceOptions;

        var now = DateTime.Now;

        dtpHistoryStart.Value =
            now.AddHours(-1);

        dtpHistoryEnd.Value = now;

        numHistoryLimit.Minimum = 1;
        numHistoryLimit.Maximum = 5000;
        numHistoryLimit.Value = 500;

        btnQueryHistory.Enabled =
            deviceOptions.Count > 0;

        btnRecent100.Enabled =
            deviceOptions.Count > 0;

        btnExportHistory.Enabled = false;

        lblHistoryCount.Text = "共 0 条";
    }

    private async Task<bool> CollectSelectedDeviceAsync(
        bool showErrorDialog)
    {
        var monitoringService =
            SelectedMonitoringService;

        if (_isCollecting ||
            monitoringService is null)
        {
            return false;
        }

        if (!TryGetSelectedDeviceCode(
                out var deviceCode,
                showErrorDialog))
        {
            return false;
        }

        _isCollecting = true;
        UpdateControlStates();

        try
        {
            if (IsSimulatorSelected)
            {
                ApplyScenarioIfChanged(deviceCode);
            }

            var result = await Task.Run(() =>
                monitoringService.CollectOnce(
                    deviceCode,
                    _temperatureThresholds,
                    _maximumCommunicationFailures));

            DisplayMonitoringResult(result);
            LoadDeviceList(deviceCode);
            LoadAlarmList();
            LoadMaintenanceAlarmOptions();

            var mode = _isMonitoring
                ? "连续监控"
                : "单次采集";

            tslConnection.Text =
                result.Snapshot.Quality ==
                DataQuality.CommunicationLost
                    ? $"{mode}：通信中断"
                    : $"{mode}：采集成功";

            AppendRuntimeLog(
                $"[{cmbDataSource.SelectedItem}] " +
                $"设备{deviceCode}采集完成；" +
                $"温度={result.Snapshot.Temperature:F1}℃；" +
                $"压力={result.Snapshot.Pressure:F2}MPa；" +
                $"振动={result.Snapshot.Vibration:F2}；" +
                $"质量={result.Snapshot.Quality}；" +
                $"状态={result.DeviceStatus}；" +
                $"连续通信失败=" +
                $"{result.ConsecutiveCommunicationFailures}。");

            if (IsOpenPlcSelected)
            {
                _lastOpenPlcCheckTime =
                    DateTime.Now;

                _lastOpenPlcCommunicationSucceeded =
                    result.Snapshot.Quality !=
                    DataQuality.CommunicationLost;
            }

            _lastBusinessRefreshTime =
                DateTime.Now;

            RefreshSystemStatusView();

            return true;
        }
        catch (Exception exception)
        {
            if (IsOpenPlcSelected)
            {
                _lastOpenPlcCheckTime =
                    DateTime.Now;

                _lastOpenPlcCommunicationSucceeded =
                    false;
            }

            tslConnection.Text =
                "采集状态：失败";

            AppendRuntimeLog(
                $"设备{deviceCode}采集失败：" +
                exception.Message);

            if (_isMonitoring)
            {
                StopContinuousMonitoring(
                    "连续监控因采集异常而停止。");
            }

            if (showErrorDialog)
            {
                MessageBox.Show(
                    $"采集失败：{exception.Message}",
                    "设备采集错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            RefreshSystemStatusView();

            return false;
        }
        finally
        {
            _isCollecting = false;
            UpdateControlStates();
        }
    }
    private void ApplyScenarioIfChanged(
        string deviceCode)
    {
        var scenario =
            cmbScenario.SelectedItem
            is ScenarioOption option
            ? option.Scenario
            : SimulationScenario.Normal;

        if (_appliedScenarios.TryGetValue(
                deviceCode,
                out var currentScenario) &&
            currentScenario == scenario)
        {
            return;
        }

        _simulatorDataSource!.SetScenario(
            deviceCode,
            scenario);

        _appliedScenarios[deviceCode] =
            scenario;

        AppendRuntimeLog(
            $"设备{deviceCode}切换模拟场景：" +
            $"{cmbScenario.Text}。");

        RecordAudit(
            AuditActionTypes.ChangeSimulationScenario,
            AuditTargetTypes.Device,
            GetDeviceIdByCode(deviceCode),
            $"切换模拟场景；" +
            $"设备代码={deviceCode}；" +
            $"场景={scenario}；" +
            $"场景名称={cmbScenario.Text}。");
    }

    private bool EnsureSelectedDataSourceAvailable()
    {
        if (SelectedMonitoringService is not null)
        {
            return true;
        }

        MessageBox.Show(
            "当前数据源尚未正确配置。",
            "提示",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);

        return false;
    }

    private bool EnsureDeviceCanBeMonitored(
        string deviceCode)
    {
        if (_deviceRepository is null ||
            _deviceStateService is null)
        {
            MessageBox.Show(
                "设备状态服务尚未正确配置。",
                "监控不可用",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return false;
        }

        var device =
            _deviceRepository.GetByCode(
                deviceCode);

        if (device is null)
        {
            MessageBox.Show(
                $"未找到设备“{deviceCode}”。",
                "监控不可用",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return false;
        }

        if (_deviceStateService.CanMonitor(device))
        {
            return true;
        }

        string reason = !device.Enabled
            ? "设备已经停用。"
            : device.Status switch
            {
                DeviceStatus.Maintenance =>
                    "设备正在维护，不能采集监控数据。",

                DeviceStatus.Fault =>
                    "设备处于故障状态，不能启动监控。",

                DeviceStatus.Offline =>
                    "设备处于离线状态，不能启动监控。",

                _ =>
                    $"设备当前状态为{device.Status}，" +
                    "不允许启动监控。"
            };

        AppendRuntimeLog(
            $"设备{device.DeviceCode}监控启动被拒绝：{reason}");

        MessageBox.Show(
            reason,
            "监控不可用",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);

        return false;
    }

    private bool TryGetSelectedDeviceCode(
    out string deviceCode,
    bool showMessage = true)
    {
        deviceCode =
            cmbDevice.SelectedValue as string
            ?? string.Empty;

        if (IsOpenPlcSelected &&
            !string.Equals(
                deviceCode,
                _openPlcDeviceCode,
                StringComparison.Ordinal))
        {
            deviceCode =
                _openPlcDeviceCode;
        }

        if (!string.IsNullOrWhiteSpace(deviceCode))
        {
            return true;
        }

        if (showMessage)
        {
            MessageBox.Show(
                "请先选择需要监控的设备。",
                "提示",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        return false;
    }

    private void StopContinuousMonitoring(
        string message)
    {
        _monitorTimer.Stop();
        _isMonitoring = false;

        AppendRuntimeLog(message);
        UpdateControlStates();
    }

    private static DateTime ToUtc(
    DateTime localTime)
    {
        return DateTime.SpecifyKind(
                localTime,
                DateTimeKind.Local)
            .ToUniversalTime();
    }

    private static DateTime ToDisplayTime(
        DateTime value)
    {
        return value.Kind == DateTimeKind.Utc
            ? value.ToLocalTime()
            : value;
    }

    private static string GetBookingUserText(
        int userId)
    {
        return userId switch
        {
            1 => "admin（管理员）",
            2 => "operator01（操作员）",
            3 => "maintainer01（维护员）",
            _ => $"用户{userId}"
        };
    }

    private sealed record BookingDeviceOption(
        int DeviceId,
        string DisplayName);

    private sealed record BookingUserOption(
        int UserId,
        string UserName,
        UserRole Role,
        string DisplayName);

    private sealed class BookingRowViewModel
    {
        public BookingRowViewModel(
            Booking booking,
            string deviceText,
            string userText)
        {
            Booking = booking;
            DeviceText = deviceText;
            UserText = userText;
        }

        public Booking Booking { get; }

        public int BookingId =>
            Booking.BookingId;

        public string DeviceText { get; }

        public string UserText { get; }

        public string StartTimeText =>
            ToDisplayTime(
                Booking.StartTime)
            .ToString("yyyy-MM-dd HH:mm");

        public string EndTimeText =>
            ToDisplayTime(
                Booking.EndTime)
            .ToString("yyyy-MM-dd HH:mm");

        public string Purpose =>
            Booking.Purpose;

        public BookingStatus Status =>
            Booking.Status;
    }

    private sealed record AlarmUserOption(
    int UserId,
    string DisplayName);

    private sealed record MaintenanceAlarmOption(
    Alarm Alarm,
    string DisplayName);

    private sealed record MaintenanceAssigneeOption(
        int? UserId,
        string DisplayName);

    private sealed class MaintenanceRowViewModel
    {
        public MaintenanceRowViewModel(
            MaintenanceOrder order,
            string deviceText)
        {
            Order = order;
            DeviceText = deviceText;
        }

        public MaintenanceOrder Order { get; }

        public int OrderId =>
            Order.OrderId;

        public string DeviceText { get; }

        public string SourceAlarmText =>
            Order.SourceAlarmId?.ToString() ?? "--";

        public string AssigneeText =>
            Order.AssigneeId.HasValue
                ? GetBookingUserText(
                    Order.AssigneeId.Value)
                : "未指派";

        public string Description =>
            Order.Description;

        public MaintenanceStatus Status =>
            Order.Status;

        public string PlannedTimeText =>
            Order.PlannedStartTime.HasValue &&
            Order.PlannedEndTime.HasValue
                ? $"{ToDisplayTime(
                        Order.PlannedStartTime.Value):yyyy-MM-dd HH:mm}" +
                  $" 至 {ToDisplayTime(
                        Order.PlannedEndTime.Value):yyyy-MM-dd HH:mm}"
                : "--";

        public string StartedAtText =>
            Order.StartedAt.HasValue
                ? ToDisplayTime(
                        Order.StartedAt.Value)
                    .ToString("yyyy-MM-dd HH:mm")
                : "--";

        public string CompletedAtText =>
            Order.CompletedAt.HasValue
                ? ToDisplayTime(
                        Order.CompletedAt.Value)
                    .ToString("yyyy-MM-dd HH:mm")
                : "--";

        public string Result =>
            string.IsNullOrWhiteSpace(Order.Result)
                ? "--"
                : Order.Result;
    }

    private sealed class AlarmRowViewModel
    {
        public AlarmRowViewModel(
            Alarm alarm,
            string deviceText)
        {
            Alarm = alarm;
            DeviceText = deviceText;
        }

        public Alarm Alarm { get; }

        public int AlarmId =>
            Alarm.AlarmId;

        public string DeviceText { get; }

        public string AlarmType =>
            Alarm.AlarmType;

        public AlarmLevel Level =>
            Alarm.Level;

        public string Message =>
            Alarm.Message;

        public AlarmStatus Status =>
            Alarm.Status;

        public string OccurredAtText =>
            ToDisplayTime(
                Alarm.OccurredAt)
            .ToString("yyyy-MM-dd HH:mm:ss");

        public string LastOccurredAtText =>
            ToDisplayTime(
                Alarm.LastOccurredAt)
            .ToString("yyyy-MM-dd HH:mm:ss");

        public int OccurrenceCount =>
            Alarm.OccurrenceCount;

        public string AcknowledgedByText =>
            Alarm.AcknowledgedBy.HasValue
                ? GetBookingUserText(
                    Alarm.AcknowledgedBy.Value)
                : "--";

        public string AcknowledgedAtText =>
            Alarm.AcknowledgedAt.HasValue
                ? ToDisplayTime(
                        Alarm.AcknowledgedAt.Value)
                    .ToString(
                        "yyyy-MM-dd HH:mm:ss")
                : "--";
    }
    /// <summary>
    /// 统一的可见设备方法
    /// </summary>
    /// <returns></returns>
    /// <summary>
    /// 获取当前学生可以看到的设备。
    /// </summary>
    private IReadOnlyList<Device> GetVisibleDevices()
    {
        var allDevices = _deviceRepository.GetAll();

        if (_studentProfile is null ||
            _studentProfile.Devices.Count == 0)
        {
            return allDevices;
        }

        var allowedDeviceCodes = _studentProfile.Devices
            .Select(device => device.DeviceCode)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return allDevices
            .Where(device =>
                allowedDeviceCodes.Contains(device.DeviceCode))
            .OrderBy(device => device.DeviceCode)
            .ToList();
    }

    /// <summary>
    /// 获取预约表格中当前选中的预约。
    /// </summary>
    /// <summary>
    /// 获取预约表格中当前选中的预约。
    /// </summary>
    private Booking? GetSelectedBooking()
    {
        var currentRow = dgvBookings.CurrentRow;

        if (currentRow is null)
        {
            return null;
        }

        if (currentRow.DataBoundItem is Booking booking)
        {
            return booking;
        }

        var bookingIdColumn = dgvBookings.Columns
            .Cast<DataGridViewColumn>()
            .FirstOrDefault(column =>
                string.Equals(
                    column.DataPropertyName,
                    nameof(Booking.BookingId),
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    column.Name,
                    nameof(Booking.BookingId),
                    StringComparison.OrdinalIgnoreCase));

        if (bookingIdColumn is null)
        {
            return null;
        }

        var value = currentRow
            .Cells[bookingIdColumn.Index]
            .Value;

        if (value is null ||
            !int.TryParse(value.ToString(), out var bookingId))
        {
            return null;
        }

        return _loadedBookings.TryGetValue(
            bookingId,
            out var selectedBooking)
            ? selectedBooking
            : null;
    }

    private void btnConfirmBooking_Click(
        object sender,
        EventArgs e)
    {
        var booking = GetSelectedBooking();

        if (booking is null)
        {
            MessageBox.Show(
                "请先选择一条预约记录。",
                "预约管理",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            return;
        }

        if (!EnsureCanManageBooking(
                booking,
                "确认预约"))
        {
            return;
        }

        var result = MessageBox.Show(
            "确定要确认当前预约吗？",
            "确认预约",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2);

        if (result != DialogResult.Yes)
        {
            return;
        }

        try
        {
            _bookingApplicationService.ConfirmBooking(booking);
            RecordAudit(
                AuditActionTypes.ConfirmBooking,
                AuditTargetTypes.Booking,
                booking.BookingId,
                $"确认预约；" +
                $"预约ID={booking.BookingId}；" +
                $"设备ID={booking.DeviceId}；" +
                $"预约用户ID={booking.UserId}。");
            RefreshBusinessViews();

            MessageBox.Show(
                "预约确认成功。",
                "预约管理",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                exception.Message,
                "确认预约失败",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }

    private void btnCompleteBooking_Click(
        object sender,
        EventArgs e)
    {
        var booking = GetSelectedBooking();

        if (booking is null)
        {
            MessageBox.Show(
                "请先选择一条预约记录。",
                "预约管理",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            return;
        }

        if (!EnsureCanManageBooking(
                booking,
                "完成预约"))
        {
            return;
        }


        var result = MessageBox.Show(
            "确定要将当前预约标记为完成吗？",
            "完成预约",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2);

        if (result != DialogResult.Yes)
        {
            return;
        }

        try
        {
            _bookingApplicationService.CompleteBooking(booking);

            RecordAudit(
                AuditActionTypes.CompleteBooking,
                AuditTargetTypes.Booking,
                booking.BookingId,
                $"完成预约；" +
                $"预约ID={booking.BookingId}；" +
                $"设备ID={booking.DeviceId}；" +
                $"预约用户ID={booking.UserId}。");

            RefreshBusinessViews();
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                exception.Message,
                "完成预约失败",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }

    private void UpdateBookingActionButtons()
    {
        Booking? booking =
            GetSelectedBooking();

        bool canCreate =
            HasPermission(
                SystemPermission.CreateBooking);

        bool canManageBooking =
            booking is not null &&
            _userSession is not null &&
            _authorizationService is not null &&
            _authorizationService.CanManageBooking(
                _userSession,
                booking);

        btnCreateBooking.Enabled = canCreate;

        cmbBookingDevice.Enabled = canCreate;

        cmbBookingUser.Enabled =
            canCreate &&
            HasPermission(
                SystemPermission.ManageAllBookings);

        dtpBookingStart.Enabled = canCreate;
        dtpBookingEnd.Enabled = canCreate;
        txtBookingPurpose.Enabled = canCreate;

        btnConfirmBooking.Enabled =
            canManageBooking &&
            booking?.Status ==
                BookingStatus.Pending;

        btnCancelBooking.Enabled =
            canManageBooking &&
            (booking?.Status ==
                 BookingStatus.Pending ||
             booking?.Status ==
                 BookingStatus.Confirmed);

        btnCompleteBooking.Enabled =
            canManageBooking &&
            booking?.Status ==
                BookingStatus.Confirmed &&
            booking.EndTime <= DateTime.UtcNow;
    }

    private void dgvBookings_SelectionChanged(
    object sender,
    EventArgs e)
    {
        UpdateBookingActionButtons();
    }

    private void dgvBookings_SelectionChanged_1(object sender, EventArgs e)
    {
        UpdateBookingActionButtons();
    }

    private void btnQueryHistory_Click(object sender, EventArgs e)
    {
        if (!EnsurePermission(
        SystemPermission.ViewHistory,
        "查询历史数据"))
        {
            return;
        }
        if (_telemetryRepository is null)
        {
            return;
        }

        if (cmbHistoryDevice.SelectedItem
            is not HistoryDeviceOption device)
        {
            MessageBox.Show(
                "请选择要查询的设备。",
                "历史数据",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            return;
        }

        if (dtpHistoryStart.Value >=
            dtpHistoryEnd.Value)
        {
            MessageBox.Show(
                "开始时间必须早于结束时间。",
                "历史数据",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        try
        {
            var startTimeUtc =
                ConvertPickerValueToUtc(
                    dtpHistoryStart.Value);

            var endTimeUtc =
                ConvertPickerValueToUtc(
                    dtpHistoryEnd.Value);

            var maximumCount =
                Decimal.ToInt32(
                    numHistoryLimit.Value);

            var records =
                _telemetryRepository.GetByTimeRange(
                    device.DeviceId,
                    startTimeUtc,
                    endTimeUtc,
                    maximumCount);

            BindHistoryRecords(records);

            AppendRuntimeLog(
                $"历史数据查询完成：" +
                $"{device.DisplayText}，" +
                $"{records.Count}条。");
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                $"查询历史数据失败：{exception.Message}",
                "历史数据错误",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
    private void btnRecent100_Click(
        object sender,
        EventArgs e)
    {
        if (!EnsurePermission(
            SystemPermission.ViewHistory,
            "查询历史数据"))
        {
            return;
        }
        if (_telemetryRepository is null)
        {
            return;
        }

        if (cmbHistoryDevice.SelectedItem
            is not HistoryDeviceOption device)
        {
            MessageBox.Show(
                "请选择要查询的设备。",
                "历史数据",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            return;
        }

        try
        {
            var records =
                _telemetryRepository.GetRecent(
                    device.DeviceId,
                    100);

            BindHistoryRecords(records);

            AppendRuntimeLog(
                $"已加载{device.DisplayText}" +
                $"最近{records.Count}条遥测数据。");
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                $"加载最近数据失败：{exception.Message}",
                "历史数据错误",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
    private static void WriteTelemetryCsvRow(
    StreamWriter writer,
    TelemetryRecord record)
    {
        var localTime =
            ConvertTelemetryTimeToLocal(
                record.CollectedAt);

        var fields = new[]
        {
        record.TelemetryId.ToString(
            CultureInfo.InvariantCulture),

        record.DeviceId.ToString(
            CultureInfo.InvariantCulture),

        localTime.ToString(
            "yyyy-MM-dd HH:mm:ss",
            CultureInfo.InvariantCulture),

        record.Temperature.ToString(
            "F2",
            CultureInfo.InvariantCulture),

        record.Pressure.ToString(
            "F3",
            CultureInfo.InvariantCulture),

        record.Vibration.ToString(
            "F3",
            CultureInfo.InvariantCulture),

        record.IsRunning ? "是" : "否",
        record.FaultSignal ? "是" : "否",
        record.EmergencyStop ? "是" : "否",
        record.Quality.ToString()
    };

        writer.WriteLine(
            string.Join(
                ",",
                fields.Select(EscapeCsvField)));
    }
    private void btnExportHistory_Click(
        object sender,
        EventArgs e)
    {
        if (!EnsurePermission(
        SystemPermission.ExportHistory,
        "导出历史数据"))
        {
            return;
        }
        if (_historyRecords.Count == 0)
        {
            MessageBox.Show(
                "当前没有可以导出的历史数据。",
                "导出历史数据",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            return;
        }

        var deviceId =
            _historyRecords[0].DeviceId;

        using var dialog =
            new SaveFileDialog
            {
                Title = "导出历史遥测数据",

                Filter =
                    "CSV文件（*.csv）|*.csv|" +
                    "所有文件（*.*）|*.*",

                DefaultExt = "csv",
                AddExtension = true,
                OverwritePrompt = true,
                RestoreDirectory = true,

                FileName =
                    $"telemetry-device-{deviceId}-" +
                    $"{DateTime.Now:yyyyMMdd-HHmmss}.csv"
            };

        if (dialog.ShowDialog(this) !=
            DialogResult.OK)
        {
            return;
        }

        try
        {
            // 输出UTF-8 BOM，保证Excel识别中文
            var encoding =
                new UTF8Encoding(
                    encoderShouldEmitUTF8Identifier: true);

            using var writer =
                new StreamWriter(
                    dialog.FileName,
                    append: false,
                    encoding);

            writer.WriteLine(
                "遥测编号,设备编号,采集时间," +
                "温度（℃）,压力,振动," +
                "运行,故障,急停,数据质量");

            foreach (var record in _historyRecords)
            {
                WriteTelemetryCsvRow(
                    writer,
                    record);
            }

            AppendRuntimeLog(
                $"历史数据导出完成：" +
                $"{_historyRecords.Count}条，" +
                $"{dialog.FileName}");

            RecordAudit(
                AuditActionTypes.ExportHistory,
                AuditTargetTypes.Telemetry,
                deviceId,
                $"导出历史遥测数据；" +
                $"设备ID={deviceId}；" +
                $"记录数量={_historyRecords.Count}；" +
                $"文件名={Path.GetFileName(dialog.FileName)}。");

            MessageBox.Show(
                $"成功导出 {_historyRecords.Count} 条数据。" +
                Environment.NewLine +
                Environment.NewLine +
                dialog.FileName,
                "导出成功",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception exception)
        {
            AppendRuntimeLog(
                $"历史数据导出失败：" +
                exception.Message);

            MessageBox.Show(
                $"导出失败：{exception.Message}",
                "导出历史数据",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
    private void RefreshDashboard()
    {
        if (_deviceRepository is null ||
            _bookingRepository is null ||
            _alarmRepository is null ||
            _maintenanceRepository is null)
        {
            return;
        }

        try
        {
            var devices = GetVisibleDevices();

            var deviceCount =
                devices.Count;

            var runningCount =
                devices.Count(device =>
                    device.Status ==
                    DeviceStatus.Running);

            var faultCount =
                devices.Count(device =>
                    device.Status ==
                    DeviceStatus.Fault);

            var bookingCount =
                devices.Sum(device =>
                    _bookingRepository
                        .GetActiveBookings(
                            device.DeviceId)
                        .Count);

            // 当前系统只生成温度报警
            var alarmCount =
                devices.Count(device =>
                    _alarmRepository
                        .GetActiveAlarm(
                            device.DeviceId,
                            "Temperature") is not null);

            var maintenanceCount =
                devices.Sum(device =>
                    _maintenanceRepository
                        .GetActiveOrders(
                            device.DeviceId)
                        .Count);

            lblDashboardDeviceCount.Text =
                deviceCount.ToString();

            lblDashboardRunningCount.Text =
                runningCount.ToString();

            lblDashboardFaultCount.Text =
                faultCount.ToString();

            lblDashboardBookingCount.Text =
                bookingCount.ToString();

            lblDashboardAlarmCount.Text =
                alarmCount.ToString();

            lblDashboardMaintenanceCount.Text =
                maintenanceCount.ToString();

            lblDashboardRefreshTime.Text =
                $"最后刷新：" +
                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}";

            UpdateDashboardSummary(
                deviceCount,
                runningCount,
                faultCount,
                bookingCount,
                alarmCount,
                maintenanceCount);
        }
        catch (Exception exception)
        {
            if (_dashboardOverallStatusLabel is not null)
            {
                _dashboardOverallStatusLabel.Text =
                    "系统概览刷新失败";

                _dashboardOverallStatusLabel.ForeColor =
                    Color.FromArgb(220, 38, 38);
            }

            if (_dashboardSummaryLabel is not null)
            {
                _dashboardSummaryLabel.Text =
                    exception.Message;
            }

            AppendRuntimeLog(
                $"刷新系统概览失败：" +
                exception.Message);
        }
    }

    private void UpdateDashboardSummary(
        int deviceCount,
        int runningCount,
        int faultCount,
        int bookingCount,
        int alarmCount,
        int maintenanceCount)
    {
        if (_dashboardOverallStatusLabel is null ||
            _dashboardSummaryLabel is null)
        {
            return;
        }

        if (deviceCount == 0)
        {
            _dashboardOverallStatusLabel.Text =
                "暂无可见设备";

            _dashboardOverallStatusLabel.ForeColor =
                Color.FromArgb(100, 116, 139);
        }
        else if (faultCount > 0 || alarmCount > 0)
        {
            _dashboardOverallStatusLabel.Text =
                "当前存在需要处理的异常";

            _dashboardOverallStatusLabel.ForeColor =
                Color.FromArgb(220, 38, 38);
        }
        else if (maintenanceCount > 0)
        {
            _dashboardOverallStatusLabel.Text =
                "设备维护业务正在处理中";

            _dashboardOverallStatusLabel.ForeColor =
                Color.FromArgb(217, 119, 6);
        }
        else if (runningCount > 0)
        {
            _dashboardOverallStatusLabel.Text =
                "系统运行正常";

            _dashboardOverallStatusLabel.ForeColor =
                Color.FromArgb(22, 163, 74);
        }
        else
        {
            _dashboardOverallStatusLabel.Text =
                "系统当前处于待命状态";

            _dashboardOverallStatusLabel.ForeColor =
                Color.FromArgb(37, 99, 235);
        }

        _dashboardSummaryLabel.Text =
            $"设备 {runningCount}/{deviceCount} 台运行，" +
            $"有效预约 {bookingCount} 条，" +
            $"未解除报警 {alarmCount} 条，" +
            $"未完成工单 {maintenanceCount} 条。";
    }

    private void btnRefreshDashboard_Click(
    object sender,
    EventArgs e)
    {
        RefreshDashboard();
    }
}
