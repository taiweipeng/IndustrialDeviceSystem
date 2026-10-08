#nullable disable

namespace IndustrialDeviceSystem.WinForms;

partial class LoginForm
{
    private System.ComponentModel.IContainer
        components = null;

    private Panel pnlBrand;
    private Label lblBrandCode;
    private Label lblBrandTitle;
    private Label lblBrandSubtitle;
    private Label lblVersion;

    private Label lblLoginTitle;
    private Label lblLoginSubtitle;
    private Label lblUserName;
    private TextBox txtUserName;
    private Label lblPassword;
    private TextBox txtPassword;
    private CheckBox chkShowPassword;
    private Label lblError;
    private Button btnLogin;
    private Button btnExit;

    protected override void Dispose(
        bool disposing)
    {
        if (disposing &&
            components is not null)
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        pnlBrand = new Panel();
        lblBrandCode = new Label();
        lblBrandTitle = new Label();
        lblBrandSubtitle = new Label();
        lblVersion = new Label();

        lblLoginTitle = new Label();
        lblLoginSubtitle = new Label();
        lblUserName = new Label();
        txtUserName = new TextBox();
        lblPassword = new Label();
        txtPassword = new TextBox();
        chkShowPassword = new CheckBox();
        lblError = new Label();
        btnLogin = new Button();
        btnExit = new Button();

        pnlBrand.SuspendLayout();
        SuspendLayout();

        // pnlBrand
        pnlBrand.BackColor =
            Color.FromArgb(30, 41, 59);

        pnlBrand.Controls.Add(lblBrandCode);
        pnlBrand.Controls.Add(lblBrandTitle);
        pnlBrand.Controls.Add(lblBrandSubtitle);
        pnlBrand.Controls.Add(lblVersion);

        pnlBrand.Dock = DockStyle.Left;
        pnlBrand.Location = new Point(0, 0);
        pnlBrand.Name = "pnlBrand";
        pnlBrand.Size = new Size(270, 420);
        pnlBrand.TabIndex = 0;

        // lblBrandCode
        lblBrandCode.AutoSize = true;
        lblBrandCode.Font = new Font(
            "Segoe UI",
            28F,
            FontStyle.Bold);

        lblBrandCode.ForeColor =
            Color.FromArgb(96, 165, 250);

        lblBrandCode.Location =
            new Point(34, 52);

        lblBrandCode.Name = "lblBrandCode";
        lblBrandCode.Size = new Size(75, 51);
        lblBrandCode.Text = "IDS";

        // lblBrandTitle
        lblBrandTitle.Font = new Font(
            "Microsoft YaHei UI",
            16F,
            FontStyle.Bold);

        lblBrandTitle.ForeColor = Color.White;
        lblBrandTitle.Location =
            new Point(34, 122);

        lblBrandTitle.Name = "lblBrandTitle";
        lblBrandTitle.Size =
            new Size(205, 70);

        lblBrandTitle.Text =
            "工业设备运行与\r\n预约管理系统";

        // lblBrandSubtitle
        lblBrandSubtitle.Font = new Font(
            "Microsoft YaHei UI",
            9.5F);

        lblBrandSubtitle.ForeColor =
            Color.FromArgb(203, 213, 225);

        lblBrandSubtitle.Location =
            new Point(36, 216);

        lblBrandSubtitle.Name =
            "lblBrandSubtitle";

        lblBrandSubtitle.Size =
            new Size(200, 68);

        lblBrandSubtitle.Text =
            "设备监控 · 预约管理\r\n" +
            "报警处置 · 维护管理";

        // lblVersion
        lblVersion.AutoSize = true;

        lblVersion.ForeColor =
            Color.FromArgb(148, 163, 184);

        lblVersion.Location =
            new Point(36, 370);

        lblVersion.Name = "lblVersion";
        lblVersion.Size = new Size(132, 17);
        lblVersion.Text = ".NET 8 · WinForms";

        // lblLoginTitle
        lblLoginTitle.AutoSize = true;

        lblLoginTitle.Font = new Font(
            "Microsoft YaHei UI",
            20F,
            FontStyle.Bold);

        lblLoginTitle.ForeColor =
            Color.FromArgb(30, 41, 59);

        lblLoginTitle.Location =
            new Point(320, 42);

        lblLoginTitle.Name = "lblLoginTitle";
        lblLoginTitle.Size = new Size(134, 36);
        lblLoginTitle.Text = "用户登录";

        // lblLoginSubtitle
        lblLoginSubtitle.AutoSize = true;

        lblLoginSubtitle.ForeColor =
            Color.FromArgb(100, 116, 139);

        lblLoginSubtitle.Location =
            new Point(322, 88);

        lblLoginSubtitle.Name =
            "lblLoginSubtitle";

        lblLoginSubtitle.Size =
            new Size(224, 17);

        lblLoginSubtitle.Text =
            "请输入系统分配的账户和密码";

        // lblUserName
        lblUserName.AutoSize = true;

        lblUserName.ForeColor =
            Color.FromArgb(51, 65, 85);

        lblUserName.Location =
            new Point(322, 132);

        lblUserName.Name = "lblUserName";
        lblUserName.Size = new Size(44, 17);
        lblUserName.Text = "用户名";

        // txtUserName
        txtUserName.BorderStyle =
            BorderStyle.FixedSingle;

        txtUserName.Font = new Font(
            "Microsoft YaHei UI",
            11F);

        txtUserName.Location =
            new Point(322, 155);

        txtUserName.MaxLength = 50;
        txtUserName.Name = "txtUserName";
        txtUserName.Size = new Size(340, 27);
        txtUserName.TabIndex = 1;

        txtUserName.TextChanged +=
            CredentialTextChanged;

        // lblPassword
        lblPassword.AutoSize = true;

        lblPassword.ForeColor =
            Color.FromArgb(51, 65, 85);

        lblPassword.Location =
            new Point(322, 204);

        lblPassword.Name = "lblPassword";
        lblPassword.Size = new Size(32, 17);
        lblPassword.Text = "密码";

        // txtPassword
        txtPassword.BorderStyle =
            BorderStyle.FixedSingle;

        txtPassword.Font = new Font(
            "Microsoft YaHei UI",
            11F);

        txtPassword.Location =
            new Point(322, 227);

        txtPassword.MaxLength = 128;
        txtPassword.Name = "txtPassword";
        txtPassword.Size = new Size(340, 27);
        txtPassword.TabIndex = 2;

        txtPassword.UseSystemPasswordChar =
            true;

        txtPassword.TextChanged +=
            CredentialTextChanged;

        // chkShowPassword
        chkShowPassword.AutoSize = true;

        chkShowPassword.ForeColor =
            Color.FromArgb(71, 85, 105);

        chkShowPassword.Location =
            new Point(322, 267);

        chkShowPassword.Name =
            "chkShowPassword";

        chkShowPassword.Size =
            new Size(75, 21);

        chkShowPassword.TabIndex = 3;
        chkShowPassword.Text = "显示密码";

        chkShowPassword.CheckedChanged +=
            chkShowPassword_CheckedChanged;

        // lblError
        lblError.AutoEllipsis = true;

        lblError.ForeColor =
            Color.FromArgb(220, 38, 38);

        lblError.Location =
            new Point(322, 296);

        lblError.Name = "lblError";
        lblError.Size = new Size(340, 24);
        lblError.TextAlign =
            ContentAlignment.MiddleLeft;

        // btnLogin
        btnLogin.BackColor =
            Color.FromArgb(37, 99, 235);

        btnLogin.Cursor = Cursors.Hand;

        btnLogin.FlatAppearance.BorderSize = 0;
        btnLogin.FlatStyle = FlatStyle.Flat;

        btnLogin.Font = new Font(
            "Microsoft YaHei UI",
            10F,
            FontStyle.Bold);

        btnLogin.ForeColor = Color.White;

        btnLogin.Location =
            new Point(322, 334);

        btnLogin.Name = "btnLogin";
        btnLogin.Size = new Size(190, 42);
        btnLogin.TabIndex = 4;
        btnLogin.Text = "登 录";
        btnLogin.UseVisualStyleBackColor = false;
        btnLogin.Click += btnLogin_Click;

        // btnExit
        btnExit.BackColor = Color.White;
        btnExit.Cursor = Cursors.Hand;
        btnExit.DialogResult =
            DialogResult.Cancel;

        btnExit.FlatAppearance.BorderColor =
            Color.FromArgb(203, 213, 225);

        btnExit.FlatStyle = FlatStyle.Flat;

        btnExit.Font = new Font(
            "Microsoft YaHei UI",
            10F);

        btnExit.ForeColor =
            Color.FromArgb(51, 65, 85);

        btnExit.Location =
            new Point(526, 334);

        btnExit.Name = "btnExit";
        btnExit.Size = new Size(136, 42);
        btnExit.TabIndex = 5;
        btnExit.Text = "退 出";
        btnExit.UseVisualStyleBackColor = false;
        btnExit.Click += btnExit_Click;

        // LoginForm
        AcceptButton = btnLogin;
        AutoScaleDimensions =
            new SizeF(7F, 17F);

        AutoScaleMode = AutoScaleMode.Font;

        BackColor =
            Color.FromArgb(248, 250, 252);

        CancelButton = btnExit;
        ClientSize = new Size(720, 420);

        Controls.Add(btnExit);
        Controls.Add(btnLogin);
        Controls.Add(lblError);
        Controls.Add(chkShowPassword);
        Controls.Add(txtPassword);
        Controls.Add(lblPassword);
        Controls.Add(txtUserName);
        Controls.Add(lblUserName);
        Controls.Add(lblLoginSubtitle);
        Controls.Add(lblLoginTitle);
        Controls.Add(pnlBrand);

        Font = new Font(
            "Microsoft YaHei UI",
            9F);

        FormBorderStyle =
            FormBorderStyle.FixedDialog;

        MaximizeBox = false;
        MinimizeBox = false;
        Name = "LoginForm";
        StartPosition =
            FormStartPosition.CenterScreen;

        Text = "工业设备管理系统 - 用户登录";

        pnlBrand.ResumeLayout(false);
        pnlBrand.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}

#nullable restore
