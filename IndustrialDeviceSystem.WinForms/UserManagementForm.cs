using IndustrialDeviceSystem.Core.Enums;
using IndustrialDeviceSystem.Core.Interfaces;
using IndustrialDeviceSystem.Core.Models;
using IndustrialDeviceSystem.Core.Services;

namespace IndustrialDeviceSystem.WinForms;

public sealed class UserManagementForm : Form
{
    private readonly UserAccountService
        _userAccountService;

    private readonly IUserRepository
        _userRepository;

    private readonly OperationAuditService
        _operationAuditService;

    private readonly UserSession
        _actor;

    private readonly DataGridView
        _dgvUsers = new();

    private readonly TextBox
        _txtUserName = new();

    private readonly TextBox
        _txtInitialPassword = new();

    private readonly ComboBox
        _cmbRole = new();

    private readonly CheckBox
        _chkShowPassword = new();

    private readonly Button
        _btnCreate = new();

    private readonly Label
        _lblUserCount = new();

    private readonly Label
    _lblSelectedUser = new();

    private readonly TextBox
        _txtResetPassword = new();

    private readonly ComboBox
        _cmbNewRole = new();

    private readonly Button
        _btnResetPassword = new();

    private readonly Button
        _btnChangeRole = new();

    private readonly Button
        _btnToggleEnabled = new();

    public UserManagementForm(
        UserAccountService userAccountService,
        IUserRepository userRepository,
        OperationAuditService operationAuditService,
        UserSession actor)
    {
        _userAccountService =
            userAccountService ??
            throw new ArgumentNullException(
                nameof(userAccountService));

        _userRepository =
            userRepository ??
            throw new ArgumentNullException(
                nameof(userRepository));

        _operationAuditService =
            operationAuditService ??
            throw new ArgumentNullException(
                nameof(operationAuditService));

        _actor =
            actor ??
            throw new ArgumentNullException(
                nameof(actor));

        InitializeView();
        LoadUsers();
    }

    private void InitializeView()
    {
        Text = "用户管理";

        StartPosition =
            FormStartPosition.CenterParent;

        FormBorderStyle =
            FormBorderStyle.Sizable;

        MinimumSize = new Size(900, 560);
        ClientSize = new Size(1080, 680);

        Font = new Font(
            "Microsoft YaHei UI",
            10F);

        BackColor = Color.FromArgb(
            248,
            250,
            252);

        var createGroup = new GroupBox
        {
            Text = "创建用户",
            Dock = DockStyle.Top,
            Height = 175,
            Padding = new Padding(20),
            BackColor = Color.White
        };

        var userNameLabel =
            CreateLabel(
                "用户名：",
                new Point(28, 45));

        _txtUserName.Location =
            new Point(105, 41);
        _txtUserName.Size =
            new Size(190, 30);
        _txtUserName.MaxLength = 50;

        var passwordLabel =
            CreateLabel(
                "初始密码：",
                new Point(325, 45));

        _txtInitialPassword.Location =
            new Point(420, 41);
        _txtInitialPassword.Size =
            new Size(210, 30);
        _txtInitialPassword.MaxLength = 128;
        _txtInitialPassword
            .UseSystemPasswordChar = true;

        _chkShowPassword.Text = "显示";
        _chkShowPassword.AutoSize = true;
        _chkShowPassword.Location =
            new Point(640, 44);

        _chkShowPassword.CheckedChanged +=
            (_, _) =>
            {
                _txtInitialPassword
                    .UseSystemPasswordChar =
                    !_chkShowPassword.Checked;
            };

        var roleLabel =
            CreateLabel(
                "用户角色：",
                new Point(735, 45));

        _cmbRole.Location =
            new Point(830, 41);
        _cmbRole.Size =
            new Size(180, 32);
        _cmbRole.DropDownStyle =
            ComboBoxStyle.DropDownList;

        _cmbRole.DataSource =
            Enum.GetValues<UserRole>();

        _cmbRole.Format += (_, args) =>
        {
            if (args.ListItem is UserRole role)
            {
                args.Value =
                    GetRoleText(role);
            }
        };

        _cmbRole.SelectedItem =
            UserRole.Operator;

        var requirementLabel = new Label
        {
            AutoSize = true,
            Location = new Point(28, 99),
            ForeColor = Color.FromArgb(
                100,
                116,
                139),
            Text =
                "密码至少8位，必须包含大写字母、" +
                "小写字母、数字和特殊字符。"
        };

        _btnCreate.Text = "创建用户";
        _btnCreate.Size = new Size(120, 38);
        _btnCreate.Location =
            new Point(890, 104);
        _btnCreate.BackColor =
            Color.FromArgb(37, 99, 235);
        _btnCreate.ForeColor = Color.White;
        _btnCreate.FlatStyle =
            FlatStyle.Flat;
        _btnCreate.UseVisualStyleBackColor =
            false;
        _btnCreate.FlatAppearance.BorderSize =
            0;
        _btnCreate.Click += BtnCreate_Click;

        createGroup.Controls.Add(userNameLabel);
        createGroup.Controls.Add(_txtUserName);
        createGroup.Controls.Add(passwordLabel);
        createGroup.Controls.Add(
            _txtInitialPassword);
        createGroup.Controls.Add(
            _chkShowPassword);
        createGroup.Controls.Add(roleLabel);
        createGroup.Controls.Add(_cmbRole);
        createGroup.Controls.Add(
            requirementLabel);
        createGroup.Controls.Add(_btnCreate);

        var listHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 55,
            BackColor = Color.FromArgb(
                248,
                250,
                252)
        };

        var listTitle = new Label
        {
            Text = "系统用户",
            AutoSize = true,
            Location = new Point(20, 17),
            Font = new Font(
                "Microsoft YaHei UI",
                11F,
                FontStyle.Bold),
            ForeColor = Color.FromArgb(
                30,
                41,
                59)
        };

        _lblUserCount.AutoSize = true;
        _lblUserCount.Location =
            new Point(145, 19);
        _lblUserCount.ForeColor =
            Color.FromArgb(100, 116, 139);

        var refreshButton = new Button
        {
            Text = "刷新",
            Size = new Size(90, 34),
            Location = new Point(950, 10),
            Anchor =
                AnchorStyles.Top |
                AnchorStyles.Right
        };

        refreshButton.Click +=
            (_, _) => LoadUsers();

        listHeader.Controls.Add(listTitle);
        listHeader.Controls.Add(_lblUserCount);
        listHeader.Controls.Add(refreshButton);

        ConfigureUserGrid();

        var actionsGroup =
            CreateUserActionsGroup();

        _dgvUsers.SelectionChanged +=
            (_, _) =>
                UpdateSelectedUserActions();

        Controls.Add(_dgvUsers);
        Controls.Add(actionsGroup);
        Controls.Add(listHeader);
        Controls.Add(createGroup);        

        AcceptButton = _btnCreate;
    }
    /// <summary>
    /// 用户操作区域
    /// </summary>
    /// <returns></returns>
    private GroupBox CreateUserActionsGroup()
    {
        var group = new GroupBox
        {
            Text = "选中用户操作",
            Dock = DockStyle.Bottom,
            Height = 165,
            Padding = new Padding(20),
            BackColor = Color.White
        };

        _lblSelectedUser.Text =
            "当前未选择用户";

        _lblSelectedUser.AutoSize = true;
        _lblSelectedUser.Location =
            new Point(28, 35);

        _lblSelectedUser.Font =
            new Font(
                "Microsoft YaHei UI",
                10F,
                FontStyle.Bold);

        _lblSelectedUser.ForeColor =
            Color.FromArgb(30, 64, 175);

        var passwordLabel =
            CreateLabel(
                "新密码：",
                new Point(28, 88));

        _txtResetPassword.Location =
            new Point(105, 84);

        _txtResetPassword.Size =
            new Size(230, 30);

        _txtResetPassword.MaxLength = 128;

        _txtResetPassword
            .UseSystemPasswordChar = true;

        _btnResetPassword.Text =
            "重置密码";

        _btnResetPassword.Size =
            new Size(112, 36);

        _btnResetPassword.Location =
            new Point(350, 81);

        _btnResetPassword.Click +=
            BtnResetPassword_Click;

        var roleLabel =
            CreateLabel(
                "新角色：",
                new Point(510, 88));

        _cmbNewRole.Location =
            new Point(590, 84);

        _cmbNewRole.Size =
            new Size(150, 32);

        _cmbNewRole.DropDownStyle =
            ComboBoxStyle.DropDownList;

        _cmbNewRole.DataSource =
            Enum.GetValues<UserRole>();

        _cmbNewRole.Format += (_, args) =>
        {
            if (args.ListItem is UserRole role)
            {
                args.Value =
                    GetRoleText(role);
            }
        };

        _btnChangeRole.Text =
            "修改角色";

        _btnChangeRole.Size =
            new Size(112, 36);

        _btnChangeRole.Location =
            new Point(755, 81);

        _btnChangeRole.Click +=
            BtnChangeRole_Click;

        _btnToggleEnabled.Text =
            "禁用账号";

        _btnToggleEnabled.Size =
            new Size(120, 36);

        _btnToggleEnabled.Location =
            new Point(885, 81);

        _btnToggleEnabled.Click +=
            BtnToggleEnabled_Click;

        group.Controls.Add(_lblSelectedUser);
        group.Controls.Add(passwordLabel);
        group.Controls.Add(_txtResetPassword);
        group.Controls.Add(_btnResetPassword);
        group.Controls.Add(roleLabel);
        group.Controls.Add(_cmbNewRole);
        group.Controls.Add(_btnChangeRole);
        group.Controls.Add(_btnToggleEnabled);

        return group;
    }

    private void ConfigureUserGrid()
    {
        _dgvUsers.Dock = DockStyle.Fill;
        _dgvUsers.ReadOnly = true;
        _dgvUsers.AllowUserToAddRows = false;
        _dgvUsers.AllowUserToDeleteRows = false;
        _dgvUsers.AllowUserToResizeRows = false;
        _dgvUsers.RowHeadersVisible = false;
        _dgvUsers.MultiSelect = false;

        _dgvUsers.SelectionMode =
            DataGridViewSelectionMode.FullRowSelect;

        _dgvUsers.AutoGenerateColumns = false;

        _dgvUsers.AutoSizeColumnsMode =
            DataGridViewAutoSizeColumnsMode.Fill;

        _dgvUsers.BackgroundColor = Color.White;
        _dgvUsers.BorderStyle = BorderStyle.None;

        _dgvUsers.EnableHeadersVisualStyles =
            false;

        _dgvUsers.ColumnHeadersDefaultCellStyle =
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
                    FontStyle.Bold)
            };

        _dgvUsers.DefaultCellStyle =
            new DataGridViewCellStyle
            {
                SelectionBackColor =
                    Color.FromArgb(
                        219,
                        234,
                        254),
                SelectionForeColor =
                    Color.FromArgb(
                        15,
                        23,
                        42)
            };

        _dgvUsers.Columns.AddRange(
            new DataGridViewTextBoxColumn
            {
                HeaderText = "用户编号",
                DataPropertyName =
                    nameof(UserRowViewModel.UserId),
                FillWeight = 12
            },
            new DataGridViewTextBoxColumn
            {
                HeaderText = "用户名",
                DataPropertyName =
                    nameof(
                        UserRowViewModel.UserName),
                FillWeight = 22
            },
            new DataGridViewTextBoxColumn
            {
                HeaderText = "角色",
                DataPropertyName =
                    nameof(
                        UserRowViewModel.RoleText),
                FillWeight = 18
            },
            new DataGridViewTextBoxColumn
            {
                HeaderText = "账号状态",
                DataPropertyName =
                    nameof(
                        UserRowViewModel.StatusText),
                FillWeight = 16
            },
            new DataGridViewTextBoxColumn
            {
                HeaderText = "创建时间",
                DataPropertyName =
                    nameof(
                        UserRowViewModel.CreatedAtText),
                FillWeight = 26
            });
    }

    private void LoadUsers()
    {
        try
        {
            var rows = _userRepository
                .GetAll()
                .OrderBy(user => user.UserId)
                .Select(user =>
                    new UserRowViewModel(
                        user.UserId,
                        user.UserName,
                        user.Role,
                        GetRoleText(user.Role),
                        user.IsEnabled,
                        user.IsEnabled
                        ? "启用"
                        : "禁用",
                        user.CreatedAt
                        .ToLocalTime()
                        .ToString(
                            "yyyy-MM-dd HH:mm:ss")))
                .ToList();

            _dgvUsers.DataSource = null;
            _dgvUsers.DataSource = rows;

            _lblUserCount.Text =
                $"共 {rows.Count} 个账号";
            UpdateSelectedUserActions();
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                $"加载用户失败：{exception.Message}",
                "用户管理",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private UserRowViewModel?
    GetSelectedUser()
    {
        return _dgvUsers.CurrentRow?
            .DataBoundItem
            as UserRowViewModel;
    }

    private void UpdateSelectedUserActions()
    {
        var selectedUser =
            GetSelectedUser();

        var hasSelection =
            selectedUser is not null;

        _txtResetPassword.Enabled =
            hasSelection;

        _btnResetPassword.Enabled =
            hasSelection;

        _cmbNewRole.Enabled =
            hasSelection &&
            selectedUser!.UserId !=
                _actor.UserId;

        _btnChangeRole.Enabled =
            hasSelection &&
            selectedUser!.UserId !=
                _actor.UserId;

        _btnToggleEnabled.Enabled =
            hasSelection &&
            selectedUser!.UserId !=
                _actor.UserId;

        if (selectedUser is null)
        {
            _lblSelectedUser.Text =
                "当前未选择用户";

            _btnToggleEnabled.Text =
                "启用/禁用";

            return;
        }

        _lblSelectedUser.Text =
            $"当前用户：{selectedUser.UserName}　" +
            $"角色：{selectedUser.RoleText}　" +
            $"状态：{selectedUser.StatusText}";

        _cmbNewRole.SelectedItem =
            selectedUser.Role;

        _btnToggleEnabled.Text =
            selectedUser.IsEnabled
                ? "禁用账号"
                : "启用账号";
    }

    private void BtnResetPassword_Click(
        object? sender,
        EventArgs e)
    {
        var selectedUser =
            GetSelectedUser();

        if (selectedUser is null)
        {
            return;
        }

        if (string.IsNullOrEmpty(
                _txtResetPassword.Text))
        {
            ShowValidationError(
                "请输入新的登录密码。");

            return;
        }

        var confirmation = MessageBox.Show(
            $"确定重置用户" +
            $"“{selectedUser.UserName}”的密码吗？" +
            Environment.NewLine +
            "重置后，原密码将立即失效。",
            "重置密码",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);

        if (confirmation != DialogResult.Yes)
        {
            return;
        }

        if (!TryExecuteUserOperation(
                () =>
                    _userAccountService
                        .ResetPassword(
                            _actor,
                            selectedUser.UserId,
                            _txtResetPassword.Text)))
        {
            return;
        }

        RecordAuditSafely(
            AuditActionTypes.ResetUserPassword,
            selectedUser.UserId,
            $"重置用户密码；" +
            $"用户名={selectedUser.UserName}。");

        _txtResetPassword.Clear();

        MessageBox.Show(
            $"用户“{selectedUser.UserName}”" +
            "的密码已重置。",
            "用户管理",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void BtnChangeRole_Click(
        object? sender,
        EventArgs e)
    {
        var selectedUser =
            GetSelectedUser();

        if (selectedUser is null ||
            _cmbNewRole.SelectedItem
                is not UserRole newRole)
        {
            return;
        }

        if (selectedUser.Role == newRole)
        {
            MessageBox.Show(
                "新角色与当前角色相同。",
                "用户管理",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            return;
        }

        var confirmation = MessageBox.Show(
            $"确定将用户" +
            $"“{selectedUser.UserName}”" +
            $"的角色从“{selectedUser.RoleText}”" +
            $"修改为“{GetRoleText(newRole)}”吗？",
            "修改用户角色",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);

        if (confirmation != DialogResult.Yes)
        {
            return;
        }

        if (!TryExecuteUserOperation(
                () =>
                    _userAccountService.ChangeRole(
                        _actor,
                        selectedUser.UserId,
                        newRole)))
        {
            return;
        }

        RecordAuditSafely(
            AuditActionTypes.ChangeUserRole,
            selectedUser.UserId,
            $"修改用户角色；" +
            $"用户名={selectedUser.UserName}；" +
            $"原角色={selectedUser.Role}；" +
            $"新角色={newRole}。");

        LoadUsers();

        MessageBox.Show(
            "用户角色修改成功。",
            "用户管理",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void BtnToggleEnabled_Click(
        object? sender,
        EventArgs e)
    {
        var selectedUser =
            GetSelectedUser();

        if (selectedUser is null)
        {
            return;
        }

        var newEnabledState =
            !selectedUser.IsEnabled;

        var actionText =
            newEnabledState
                ? "启用"
                : "禁用";

        var confirmation = MessageBox.Show(
            $"确定{actionText}用户" +
            $"“{selectedUser.UserName}”吗？" +
            Environment.NewLine +
            (newEnabledState
                ? "启用后，该用户可以按其角色权限登录系统。"
                : "禁用后，该用户将无法登录系统。"),
            $"{actionText}用户",
            MessageBoxButtons.YesNo,
            newEnabledState
                ? MessageBoxIcon.Question
                : MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);

        if (confirmation != DialogResult.Yes)
        {
            return;
        }

        if (!TryExecuteUserOperation(
                () =>
                    _userAccountService.SetEnabled(
                        _actor,
                        selectedUser.UserId,
                        newEnabledState)))
        {
            return;
        }

        RecordAuditSafely(
            newEnabledState
                ? AuditActionTypes.EnableUser
                : AuditActionTypes.DisableUser,
            selectedUser.UserId,
            $"{actionText}用户；" +
            $"用户名={selectedUser.UserName}。");

        LoadUsers();

        MessageBox.Show(
            $"用户“{selectedUser.UserName}”" +
            $"已{actionText}。",
            "用户管理",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void BtnCreate_Click(
        object? sender,
        EventArgs e)
    {
        if (_cmbRole.SelectedItem
            is not UserRole role)
        {
            MessageBox.Show(
                "请选择用户角色。",
                "用户管理",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        var userName =
            _txtUserName.Text.Trim();

        if (string.IsNullOrWhiteSpace(userName))
        {
            ShowValidationError(
                "请输入用户名。");

            return;
        }

        if (string.IsNullOrEmpty(
                _txtInitialPassword.Text))
        {
            ShowValidationError(
                "请输入初始密码。");

            return;
        }

        var confirmation = MessageBox.Show(
            $"确定创建用户“{userName}”吗？" +
            Environment.NewLine +
            $"用户角色：{GetRoleText(role)}",
            "确认创建用户",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2);

        if (confirmation != DialogResult.Yes)
        {
            return;
        }

        UserAccount createdUser;

        try
        {
            createdUser =
                _userAccountService.CreateUser(
                    _actor,
                    userName,
                    _txtInitialPassword.Text,
                    role);
        }
        catch (ArgumentException exception)
        {
            ShowValidationError(exception.Message);
            return;
        }
        catch (InvalidOperationException exception)
        {
            ShowValidationError(exception.Message);
            return;
        }
        catch (UnauthorizedAccessException exception)
        {
            ShowValidationError(exception.Message);
            return;
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                $"创建用户失败：{exception.Message}",
                "用户管理",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            return;
        }

        RecordAuditSafely(
            AuditActionTypes.CreateUser,
            createdUser.UserId,
            $"创建用户；" +
            $"用户名={createdUser.UserName}；" +
            $"角色={createdUser.Role}。");

        _txtUserName.Clear();
        _txtInitialPassword.Clear();
        _cmbRole.SelectedItem =
            UserRole.Operator;
        _txtUserName.Focus();

        LoadUsers();     

        MessageBox.Show(
            $"用户“{createdUser.UserName}”" +
            "创建成功。",
            "用户管理",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void RecordAuditSafely(
        string actionType,
        int userId,
        string detail)
    {
        try
        {
            _operationAuditService.Record(
                _actor,
                actionType,
                AuditTargetTypes.UserAccount,
                userId,
                detail);
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                "用户操作已经完成，但审计日志" +
                "写入失败。" +
                Environment.NewLine +
                exception.Message,
                "审计日志警告",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }

    private static Label CreateLabel(
        string text,
        Point location)
    {
        return new Label
        {
            Text = text,
            AutoSize = true,
            Location = location,
            ForeColor = Color.FromArgb(
                51,
                65,
                85)
        };
    }

    private static string GetRoleText(
        UserRole role)
    {
        return role switch
        {
            UserRole.Administrator =>
                "管理员",

            UserRole.Operator =>
                "操作员",

            UserRole.Maintainer =>
                "维护员",

            UserRole.Auditor =>
                "审计员",

            _ => role.ToString()
        };
    }

    private void ShowValidationError(
        string message)
    {
        MessageBox.Show(
            message,
            "用户管理",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
    }

    private bool TryExecuteUserOperation(
    Action operation)
    {
        try
        {
            operation();
            return true;
        }
        catch (ArgumentException exception)
        {
            ShowValidationError(
                exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            ShowValidationError(
                exception.Message);
        }
        catch (UnauthorizedAccessException exception)
        {
            ShowValidationError(
                exception.Message);
        }
        catch (KeyNotFoundException exception)
        {
            ShowValidationError(
                exception.Message);
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                $"用户操作失败：{exception.Message}",
                "用户管理",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        return false;
    }

    private sealed record UserRowViewModel(
    int UserId,
    string UserName,
    UserRole Role,
    string RoleText,
    bool IsEnabled,
    string StatusText,
    string CreatedAtText);
}
