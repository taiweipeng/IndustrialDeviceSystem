using System.Diagnostics;
using IndustrialDeviceSystem.Core.Models;
using IndustrialDeviceSystem.Core.Services;

namespace IndustrialDeviceSystem.WinForms;

public partial class LoginForm : Form
{
    private readonly AuthenticationService
        _authenticationService;

    public LoginForm(
        AuthenticationService authenticationService)
    {
        ArgumentNullException.ThrowIfNull(
            authenticationService);

        _authenticationService =
            authenticationService;

        InitializeComponent();
    }

    public UserSession? AuthenticatedSession
    {
        get;
        private set;
    }

    protected override void OnShown(
        EventArgs e)
    {
        base.OnShown(e);

        txtUserName.Focus();
    }

    private void btnLogin_Click(
        object? sender,
        EventArgs e)
    {
        lblError.Text = string.Empty;

        string userName =
            txtUserName.Text.Trim();

        string password =
            txtPassword.Text;

        if (string.IsNullOrWhiteSpace(userName))
        {
            ShowInputError(
                "请输入用户名。",
                txtUserName);

            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            ShowInputError(
                "请输入密码。",
                txtPassword);

            return;
        }

        UserSession? session;

        try
        {
            SetBusy(true);

            session =
                _authenticationService.Authenticate(
                    userName,
                    password);
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);

            lblError.Text =
                "登录服务出现异常，请检查数据库配置。";

            return;
        }
        finally
        {
            SetBusy(false);
        }

        if (session is null)
        {
            lblError.Text =
                "用户名或密码错误，或者账户已被禁用。";

            txtPassword.SelectAll();
            txtPassword.Focus();

            return;
        }

        AuthenticatedSession = session;
        DialogResult = DialogResult.OK;
        Close();
    }

    private void btnExit_Click(
        object? sender,
        EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private void chkShowPassword_CheckedChanged(
        object? sender,
        EventArgs e)
    {
        txtPassword.UseSystemPasswordChar =
            !chkShowPassword.Checked;
    }

    private void CredentialTextChanged(
        object? sender,
        EventArgs e)
    {
        lblError.Text = string.Empty;
    }

    private void ShowInputError(
        string message,
        Control control)
    {
        lblError.Text = message;
        control.Focus();
    }

    private void SetBusy(bool busy)
    {
        UseWaitCursor = busy;

        txtUserName.Enabled = !busy;
        txtPassword.Enabled = !busy;
        chkShowPassword.Enabled = !busy;
        btnLogin.Enabled = !busy;
        btnExit.Enabled = !busy;

        btnLogin.Text =
            busy ? "正在验证..." : "登 录";
    }
}
