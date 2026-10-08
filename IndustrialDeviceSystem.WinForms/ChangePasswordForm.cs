using IndustrialDeviceSystem.Core.Models;
using IndustrialDeviceSystem.Core.Services;

namespace IndustrialDeviceSystem.WinForms;

public sealed class ChangePasswordForm : Form
{
    private readonly UserAccountService
        _userAccountService;

    private readonly UserSession
        _userSession;

    private readonly TextBox
        _txtCurrentPassword = new();

    private readonly TextBox
        _txtNewPassword = new();

    private readonly TextBox
        _txtConfirmPassword = new();

    private readonly CheckBox
        _chkShowPassword = new();

    private readonly Button
        _btnSave = new();

    private readonly Button
        _btnCancel = new();

    public ChangePasswordForm(
        UserAccountService userAccountService,
        UserSession userSession)
    {
        _userAccountService =
            userAccountService ??
            throw new ArgumentNullException(
                nameof(userAccountService));

        _userSession =
            userSession ??
            throw new ArgumentNullException(
                nameof(userSession));

        InitializeView();
    }

    private void InitializeView()
    {
        Text =
            $"修改密码 - {_userSession.UserName}";

        StartPosition =
            FormStartPosition.CenterParent;

        FormBorderStyle =
            FormBorderStyle.FixedDialog;

        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;

        ClientSize = new Size(500, 390);

        Font = new Font(
            "Microsoft YaHei UI",
            10F);

        BackColor = Color.FromArgb(
            248,
            250,
            252);

        var titleLabel = new Label
        {
            Text = "修改登录密码",
            AutoSize = true,
            Location = new Point(35, 28),
            Font = new Font(
                "Microsoft YaHei UI",
                16F,
                FontStyle.Bold),
            ForeColor = Color.FromArgb(
                30,
                64,
                175)
        };

        var accountLabel = new Label
        {
            Text =
                $"当前账号：{_userSession.UserName}",
            AutoSize = true,
            Location = new Point(38, 72),
            ForeColor = Color.FromArgb(
                100,
                116,
                139)
        };

        ConfigurePasswordTextBox(
            _txtCurrentPassword,
            new Point(170, 119));

        ConfigurePasswordTextBox(
            _txtNewPassword,
            new Point(170, 174));

        ConfigurePasswordTextBox(
            _txtConfirmPassword,
            new Point(170, 229));

        Controls.Add(
            CreateFieldLabel(
                "当前密码：",
                new Point(45, 123)));

        Controls.Add(
            CreateFieldLabel(
                "新密码：",
                new Point(45, 178)));

        Controls.Add(
            CreateFieldLabel(
                "确认新密码：",
                new Point(45, 233)));

        _chkShowPassword.Text =
            "显示密码";

        _chkShowPassword.AutoSize = true;
        _chkShowPassword.Location =
            new Point(170, 272);

        _chkShowPassword.CheckedChanged +=
            (_, _) =>
            {
                var hidePassword =
                    !_chkShowPassword.Checked;

                _txtCurrentPassword
                    .UseSystemPasswordChar =
                    hidePassword;

                _txtNewPassword
                    .UseSystemPasswordChar =
                    hidePassword;

                _txtConfirmPassword
                    .UseSystemPasswordChar =
                    hidePassword;
            };

        var requirementLabel = new Label
        {
            Text =
                "密码至少8位，且必须包含大写字母、" +
                "小写字母、数字和特殊字符。",
            AutoSize = false,
            Size = new Size(420, 38),
            Location = new Point(45, 301),
            ForeColor = Color.FromArgb(
                100,
                116,
                139)
        };

        _btnSave.Text = "保存";
        _btnSave.Size = new Size(100, 36);
        _btnSave.Location =
            new Point(258, 342);
        _btnSave.BackColor =
            Color.FromArgb(37, 99, 235);
        _btnSave.ForeColor = Color.White;
        _btnSave.FlatStyle =
            FlatStyle.Flat;
        _btnSave.UseVisualStyleBackColor =
            false;
        _btnSave.FlatAppearance.BorderSize =
            0;
        _btnSave.Click += BtnSave_Click;

        _btnCancel.Text = "取消";
        _btnCancel.Size = new Size(100, 36);
        _btnCancel.Location =
            new Point(370, 342);
        _btnCancel.DialogResult =
            DialogResult.Cancel;

        AcceptButton = _btnSave;
        CancelButton = _btnCancel;

        Controls.Add(titleLabel);
        Controls.Add(accountLabel);
        Controls.Add(_txtCurrentPassword);
        Controls.Add(_txtNewPassword);
        Controls.Add(_txtConfirmPassword);
        Controls.Add(_chkShowPassword);
        Controls.Add(requirementLabel);
        Controls.Add(_btnSave);
        Controls.Add(_btnCancel);
    }

    private static void ConfigurePasswordTextBox(
        TextBox textBox,
        Point location)
    {
        textBox.Location = location;
        textBox.Size = new Size(290, 30);
        textBox.UseSystemPasswordChar = true;
        textBox.MaxLength = 128;
    }

    private static Label CreateFieldLabel(
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

    private void BtnSave_Click(
        object? sender,
        EventArgs e)
    {
        if (!string.Equals(
                _txtNewPassword.Text,
                _txtConfirmPassword.Text,
                StringComparison.Ordinal))
        {
            MessageBox.Show(
                "两次输入的新密码不一致。",
                "修改密码",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            _txtConfirmPassword.SelectAll();
            _txtConfirmPassword.Focus();
            return;
        }

        try
        {
            _userAccountService
                .ChangeOwnPassword(
                    _userSession,
                    _txtCurrentPassword.Text,
                    _txtNewPassword.Text);

            DialogResult = DialogResult.OK;
            Close();
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
        catch (Exception exception)
        {
            MessageBox.Show(
                $"修改密码失败：{exception.Message}",
                "账户安全错误",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void ShowValidationError(
        string message)
    {
        MessageBox.Show(
            message,
            "修改密码",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
    }
}
