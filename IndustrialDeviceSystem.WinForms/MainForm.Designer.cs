namespace IndustrialDeviceSystem.WinForms
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tabMain = new TabControl();
            tabDashboard = new TabPage();
            lblDashboardRefreshTime = new Label();
            btnRefreshDashboard = new Button();
            tableLayoutPanel1 = new TableLayoutPanel();
            lblDashboardMaintenanceCount = new Label();
            lblDashboardAlarmCount = new Label();
            lblDashboardBookingCount = new Label();
            lblDashboardFaultCount = new Label();
            lblDashboardRunningCount = new Label();
            lblDashboardDeviceCount = new Label();
            tabDeviceMonitoring = new TabPage();
            splitMonitoring = new SplitContainer();
            grpDevices = new GroupBox();
            statusMain = new StatusStrip();
            tslDataSource = new ToolStripStatusLabel();
            tslConnection = new ToolStripStatusLabel();
            tslCurrentTime = new ToolStripStatusLabel();
            tslStudent = new ToolStripStatusLabel();
            dgvDevices = new DataGridView();
            GroupBox = new GroupBox();
            txtRuntimeLog = new TextBox();
            grpRealtimeData = new GroupBox();
            lblAlarmValue = new Label();
            lblFaultValue = new Label();
            lblCollectedAtValue = new Label();
            lblDeviceStatusValue = new Label();
            lblRunningValue = new Label();
            lblQualityValue = new Label();
            lblVibrationValue = new Label();
            lblEmergencyValue = new Label();
            lblPressureValue = new Label();
            lblTemperatureValue = new Label();
            grpMonitoringControl = new GroupBox();
            cmbScenario = new ComboBox();
            lblScenario = new Label();
            btnStopMonitoring = new Button();
            btnStartMonitoring = new Button();
            btnCollectOnce = new Button();
            numSampleInterval = new NumericUpDown();
            cmbDevice = new ComboBox();
            cmbDataSource = new ComboBox();
            tabBookingManagement = new TabPage();
            dgvBookings = new DataGridView();
            grpBookingEditor = new GroupBox();
            btnRefreshBookings = new Button();
            btnCompleteBooking = new Button();
            btnConfirmBooking = new Button();
            btnCancelBooking = new Button();
            btnCreateBooking = new Button();
            txtBookingPurpose = new TextBox();
            dtpBookingEnd = new DateTimePicker();
            dtpBookingStart = new DateTimePicker();
            cmbBookingUser = new ComboBox();
            cmbBookingDevice = new ComboBox();
            tabAlarmManagement = new TabPage();
            dgvAlarms = new DataGridView();
            grpAlarmActions = new GroupBox();
            lblActiveAlarmCount = new Label();
            btnRefreshAlarms = new Button();
            btnAcknowledgeAlarm = new Button();
            cmbAlarmUser = new ComboBox();
            tabMaintenanceManagement = new TabPage();
            dgvMaintenance = new DataGridView();
            grpMaintenanceActions = new GroupBox();
            chkVerificationPassed = new CheckBox();
            btnRefreshMaintenance = new Button();
            btnCloseMaintenance = new Button();
            btnCompleteMaintenance = new Button();
            btnStartMaintenance = new Button();
            btnAssignMaintenance = new Button();
            txtMaintenanceResult = new TextBox();
            grpMaintenanceCreate = new GroupBox();
            chkMaintenancePlan = new CheckBox();
            btnCreateMaintenance = new Button();
            txtMaintenanceDescription = new TextBox();
            dtpMaintenancePlanEnd = new DateTimePicker();
            dtpMaintenancePlanStart = new DateTimePicker();
            cmbMaintenanceAssignee = new ComboBox();
            cmbMaintenanceAlarm = new ComboBox();
            tabHistory = new TabPage();
            dgvTelemetryHistory = new DataGridView();
            pnlHistoryQuery = new Panel();
            lblHistoryCount = new Label();
            btnExportHistory = new Button();
            btnRecent100 = new Button();
            numHistoryLimit = new NumericUpDown();
            dtpHistoryEnd = new DateTimePicker();
            lblHistoryEnd = new Label();
            btnQueryHistory = new Button();
            dtpHistoryStart = new DateTimePicker();
            lblHistoryLimit = new Label();
            lblHistoryStart = new Label();
            lblHistoryDevice = new Label();
            cmbHistoryDevice = new ComboBox();
            tabMain.SuspendLayout();
            tabDashboard.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            tabDeviceMonitoring.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitMonitoring).BeginInit();
            splitMonitoring.Panel1.SuspendLayout();
            splitMonitoring.Panel2.SuspendLayout();
            splitMonitoring.SuspendLayout();
            grpDevices.SuspendLayout();
            statusMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvDevices).BeginInit();
            GroupBox.SuspendLayout();
            grpRealtimeData.SuspendLayout();
            grpMonitoringControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numSampleInterval).BeginInit();
            tabBookingManagement.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvBookings).BeginInit();
            grpBookingEditor.SuspendLayout();
            tabAlarmManagement.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvAlarms).BeginInit();
            grpAlarmActions.SuspendLayout();
            tabMaintenanceManagement.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvMaintenance).BeginInit();
            grpMaintenanceActions.SuspendLayout();
            grpMaintenanceCreate.SuspendLayout();
            tabHistory.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvTelemetryHistory).BeginInit();
            pnlHistoryQuery.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numHistoryLimit).BeginInit();
            SuspendLayout();
            // 
            // tabMain
            // 
            tabMain.Controls.Add(tabDashboard);
            tabMain.Controls.Add(tabDeviceMonitoring);
            tabMain.Controls.Add(tabBookingManagement);
            tabMain.Controls.Add(tabAlarmManagement);
            tabMain.Controls.Add(tabMaintenanceManagement);
            tabMain.Controls.Add(tabHistory);
            tabMain.Dock = DockStyle.Fill;
            tabMain.Location = new Point(0, 0);
            tabMain.Name = "tabMain";
            tabMain.SelectedIndex = 0;
            tabMain.Size = new Size(1774, 796);
            tabMain.TabIndex = 0;
            // 
            // tabDashboard
            // 
            tabDashboard.Controls.Add(lblDashboardRefreshTime);
            tabDashboard.Controls.Add(btnRefreshDashboard);
            tabDashboard.Controls.Add(tableLayoutPanel1);
            tabDashboard.Location = new Point(4, 33);
            tabDashboard.Name = "tabDashboard";
            tabDashboard.Size = new Size(1766, 759);
            tabDashboard.TabIndex = 5;
            tabDashboard.Text = " 系统概览";
            tabDashboard.UseVisualStyleBackColor = true;
            // 
            // lblDashboardRefreshTime
            // 
            lblDashboardRefreshTime.AutoSize = true;
            lblDashboardRefreshTime.Location = new Point(836, 575);
            lblDashboardRefreshTime.Name = "lblDashboardRefreshTime";
            lblDashboardRefreshTime.Size = new Size(82, 24);
            lblDashboardRefreshTime.TabIndex = 3;
            lblDashboardRefreshTime.Text = "尚未刷新";
            // 
            // btnRefreshDashboard
            // 
            btnRefreshDashboard.Location = new Point(612, 571);
            btnRefreshDashboard.Name = "btnRefreshDashboard";
            btnRefreshDashboard.Size = new Size(112, 34);
            btnRefreshDashboard.TabIndex = 2;
            btnRefreshDashboard.Text = "刷新统计";
            btnRefreshDashboard.UseVisualStyleBackColor = true;
            btnRefreshDashboard.Click += btnRefreshDashboard_Click;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
            tableLayoutPanel1.Controls.Add(lblDashboardMaintenanceCount, 2, 1);
            tableLayoutPanel1.Controls.Add(lblDashboardAlarmCount, 1, 1);
            tableLayoutPanel1.Controls.Add(lblDashboardBookingCount, 0, 1);
            tableLayoutPanel1.Controls.Add(lblDashboardFaultCount, 2, 0);
            tableLayoutPanel1.Controls.Add(lblDashboardRunningCount, 1, 0);
            tableLayoutPanel1.Controls.Add(lblDashboardDeviceCount, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Top;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.Size = new Size(1766, 526);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // lblDashboardMaintenanceCount
            // 
            lblDashboardMaintenanceCount.AutoSize = true;
            lblDashboardMaintenanceCount.Location = new Point(1179, 253);
            lblDashboardMaintenanceCount.Name = "lblDashboardMaintenanceCount";
            lblDashboardMaintenanceCount.Size = new Size(63, 24);
            lblDashboardMaintenanceCount.TabIndex = 5;
            lblDashboardMaintenanceCount.Text = "label2";
            // 
            // lblDashboardAlarmCount
            // 
            lblDashboardAlarmCount.AutoSize = true;
            lblDashboardAlarmCount.Location = new Point(591, 253);
            lblDashboardAlarmCount.Name = "lblDashboardAlarmCount";
            lblDashboardAlarmCount.Size = new Size(63, 24);
            lblDashboardAlarmCount.TabIndex = 4;
            lblDashboardAlarmCount.Text = "label2";
            // 
            // lblDashboardBookingCount
            // 
            lblDashboardBookingCount.AutoSize = true;
            lblDashboardBookingCount.Location = new Point(3, 253);
            lblDashboardBookingCount.Name = "lblDashboardBookingCount";
            lblDashboardBookingCount.Size = new Size(63, 24);
            lblDashboardBookingCount.TabIndex = 3;
            lblDashboardBookingCount.Text = "label2";
            // 
            // lblDashboardFaultCount
            // 
            lblDashboardFaultCount.AutoSize = true;
            lblDashboardFaultCount.Location = new Point(1179, 0);
            lblDashboardFaultCount.Name = "lblDashboardFaultCount";
            lblDashboardFaultCount.Size = new Size(63, 24);
            lblDashboardFaultCount.TabIndex = 2;
            lblDashboardFaultCount.Text = "label2";
            // 
            // lblDashboardRunningCount
            // 
            lblDashboardRunningCount.AutoSize = true;
            lblDashboardRunningCount.Location = new Point(591, 0);
            lblDashboardRunningCount.Name = "lblDashboardRunningCount";
            lblDashboardRunningCount.Size = new Size(63, 24);
            lblDashboardRunningCount.TabIndex = 1;
            lblDashboardRunningCount.Text = "label2";
            // 
            // lblDashboardDeviceCount
            // 
            lblDashboardDeviceCount.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblDashboardDeviceCount.AutoSize = true;
            lblDashboardDeviceCount.Location = new Point(3, 0);
            lblDashboardDeviceCount.Name = "lblDashboardDeviceCount";
            lblDashboardDeviceCount.Size = new Size(582, 253);
            lblDashboardDeviceCount.TabIndex = 0;
            lblDashboardDeviceCount.Text = "可见设备";
            // 
            // tabDeviceMonitoring
            // 
            tabDeviceMonitoring.Controls.Add(splitMonitoring);
            tabDeviceMonitoring.Location = new Point(4, 33);
            tabDeviceMonitoring.Name = "tabDeviceMonitoring";
            tabDeviceMonitoring.Padding = new Padding(3);
            tabDeviceMonitoring.Size = new Size(1766, 759);
            tabDeviceMonitoring.TabIndex = 0;
            tabDeviceMonitoring.Text = "设备监控";
            tabDeviceMonitoring.UseVisualStyleBackColor = true;
            // 
            // splitMonitoring
            // 
            splitMonitoring.Dock = DockStyle.Fill;
            splitMonitoring.Location = new Point(3, 3);
            splitMonitoring.Name = "splitMonitoring";
            // 
            // splitMonitoring.Panel1
            // 
            splitMonitoring.Panel1.Controls.Add(grpDevices);
            // 
            // splitMonitoring.Panel2
            // 
            splitMonitoring.Panel2.Controls.Add(GroupBox);
            splitMonitoring.Panel2.Controls.Add(grpRealtimeData);
            splitMonitoring.Panel2.Controls.Add(grpMonitoringControl);
            splitMonitoring.Size = new Size(1760, 753);
            splitMonitoring.SplitterDistance = 858;
            splitMonitoring.TabIndex = 0;
            // 
            // grpDevices
            // 
            grpDevices.Controls.Add(statusMain);
            grpDevices.Controls.Add(dgvDevices);
            grpDevices.Dock = DockStyle.Fill;
            grpDevices.Location = new Point(0, 0);
            grpDevices.Name = "grpDevices";
            grpDevices.Size = new Size(858, 753);
            grpDevices.TabIndex = 0;
            grpDevices.TabStop = false;
            grpDevices.Text = "设备列表";
            // 
            // statusMain
            // 
            statusMain.ImageScalingSize = new Size(24, 24);
            statusMain.Items.AddRange(new ToolStripItem[] { tslDataSource, tslConnection, tslCurrentTime, tslStudent });
            statusMain.Location = new Point(3, 719);
            statusMain.Name = "statusMain";
            statusMain.Size = new Size(852, 31);
            statusMain.TabIndex = 1;
            statusMain.Text = "statusStrip1";
            // 
            // tslDataSource
            // 
            tslDataSource.Name = "tslDataSource";
            tslDataSource.Size = new Size(136, 24);
            tslDataSource.Text = "数据源：未选择";
            // 
            // tslConnection
            // 
            tslConnection.Name = "tslConnection";
            tslConnection.Size = new Size(154, 24);
            tslConnection.Text = "连接状态：未启动";
            // 
            // tslCurrentTime
            // 
            tslCurrentTime.Name = "tslCurrentTime";
            tslCurrentTime.Size = new Size(116, 24);
            tslCurrentTime.Text = "当前时间：--";
            // 
            // tslStudent
            // 
            tslStudent.Name = "tslStudent";
            tslStudent.Size = new Size(118, 24);
            tslStudent.Text = "学生：未加载";
            // 
            // dgvDevices
            // 
            dgvDevices.AllowUserToAddRows = false;
            dgvDevices.AllowUserToDeleteRows = false;
            dgvDevices.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvDevices.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvDevices.Dock = DockStyle.Fill;
            dgvDevices.Location = new Point(3, 26);
            dgvDevices.MultiSelect = false;
            dgvDevices.Name = "dgvDevices";
            dgvDevices.ReadOnly = true;
            dgvDevices.RowHeadersVisible = false;
            dgvDevices.RowHeadersWidth = 62;
            dgvDevices.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDevices.Size = new Size(852, 724);
            dgvDevices.TabIndex = 0;
            // 
            // GroupBox
            // 
            GroupBox.Controls.Add(txtRuntimeLog);
            GroupBox.Dock = DockStyle.Fill;
            GroupBox.Location = new Point(0, 350);
            GroupBox.Name = "GroupBox";
            GroupBox.Size = new Size(898, 403);
            GroupBox.TabIndex = 2;
            GroupBox.TabStop = false;
            GroupBox.Text = "运行日志";
            // 
            // txtRuntimeLog
            // 
            txtRuntimeLog.Dock = DockStyle.Fill;
            txtRuntimeLog.Location = new Point(3, 26);
            txtRuntimeLog.Multiline = true;
            txtRuntimeLog.Name = "txtRuntimeLog";
            txtRuntimeLog.ReadOnly = true;
            txtRuntimeLog.ScrollBars = ScrollBars.Vertical;
            txtRuntimeLog.Size = new Size(892, 374);
            txtRuntimeLog.TabIndex = 0;
            // 
            // grpRealtimeData
            // 
            grpRealtimeData.Controls.Add(lblAlarmValue);
            grpRealtimeData.Controls.Add(lblFaultValue);
            grpRealtimeData.Controls.Add(lblCollectedAtValue);
            grpRealtimeData.Controls.Add(lblDeviceStatusValue);
            grpRealtimeData.Controls.Add(lblRunningValue);
            grpRealtimeData.Controls.Add(lblQualityValue);
            grpRealtimeData.Controls.Add(lblVibrationValue);
            grpRealtimeData.Controls.Add(lblEmergencyValue);
            grpRealtimeData.Controls.Add(lblPressureValue);
            grpRealtimeData.Controls.Add(lblTemperatureValue);
            grpRealtimeData.Dock = DockStyle.Top;
            grpRealtimeData.Location = new Point(0, 120);
            grpRealtimeData.Name = "grpRealtimeData";
            grpRealtimeData.Size = new Size(898, 230);
            grpRealtimeData.TabIndex = 1;
            grpRealtimeData.TabStop = false;
            grpRealtimeData.Text = "实时运行数据";
            // 
            // lblAlarmValue
            // 
            lblAlarmValue.AutoSize = true;
            lblAlarmValue.Location = new Point(735, 142);
            lblAlarmValue.Name = "lblAlarmValue";
            lblAlarmValue.Size = new Size(28, 24);
            lblAlarmValue.TabIndex = 0;
            lblAlarmValue.Text = "无";
            // 
            // lblFaultValue
            // 
            lblFaultValue.AutoSize = true;
            lblFaultValue.Location = new Point(735, 52);
            lblFaultValue.Name = "lblFaultValue";
            lblFaultValue.Size = new Size(26, 24);
            lblFaultValue.TabIndex = 0;
            lblFaultValue.Text = "--";
            // 
            // lblCollectedAtValue
            // 
            lblCollectedAtValue.AutoSize = true;
            lblCollectedAtValue.Location = new Point(557, 142);
            lblCollectedAtValue.Name = "lblCollectedAtValue";
            lblCollectedAtValue.Size = new Size(26, 24);
            lblCollectedAtValue.TabIndex = 0;
            lblCollectedAtValue.Text = "--";
            // 
            // lblDeviceStatusValue
            // 
            lblDeviceStatusValue.AutoSize = true;
            lblDeviceStatusValue.Location = new Point(379, 142);
            lblDeviceStatusValue.Name = "lblDeviceStatusValue";
            lblDeviceStatusValue.Size = new Size(26, 24);
            lblDeviceStatusValue.TabIndex = 0;
            lblDeviceStatusValue.Text = "--";
            // 
            // lblRunningValue
            // 
            lblRunningValue.AutoSize = true;
            lblRunningValue.Location = new Point(557, 52);
            lblRunningValue.Name = "lblRunningValue";
            lblRunningValue.Size = new Size(26, 24);
            lblRunningValue.TabIndex = 0;
            lblRunningValue.Text = "--";
            // 
            // lblQualityValue
            // 
            lblQualityValue.AutoSize = true;
            lblQualityValue.Location = new Point(201, 142);
            lblQualityValue.Name = "lblQualityValue";
            lblQualityValue.Size = new Size(26, 24);
            lblQualityValue.TabIndex = 0;
            lblQualityValue.Text = "--";
            // 
            // lblVibrationValue
            // 
            lblVibrationValue.AutoSize = true;
            lblVibrationValue.Location = new Point(379, 52);
            lblVibrationValue.Name = "lblVibrationValue";
            lblVibrationValue.Size = new Size(26, 24);
            lblVibrationValue.TabIndex = 0;
            lblVibrationValue.Text = "--";
            // 
            // lblEmergencyValue
            // 
            lblEmergencyValue.AutoSize = true;
            lblEmergencyValue.Location = new Point(23, 142);
            lblEmergencyValue.Name = "lblEmergencyValue";
            lblEmergencyValue.Size = new Size(26, 24);
            lblEmergencyValue.TabIndex = 0;
            lblEmergencyValue.Text = "--";
            // 
            // lblPressureValue
            // 
            lblPressureValue.AutoSize = true;
            lblPressureValue.Location = new Point(201, 52);
            lblPressureValue.Name = "lblPressureValue";
            lblPressureValue.Size = new Size(70, 24);
            lblPressureValue.TabIndex = 0;
            lblPressureValue.Text = "-- MPa";
            // 
            // lblTemperatureValue
            // 
            lblTemperatureValue.AutoSize = true;
            lblTemperatureValue.Location = new Point(23, 52);
            lblTemperatureValue.Name = "lblTemperatureValue";
            lblTemperatureValue.Size = new Size(49, 24);
            lblTemperatureValue.TabIndex = 0;
            lblTemperatureValue.Text = "-- ℃";
            // 
            // grpMonitoringControl
            // 
            grpMonitoringControl.Controls.Add(cmbScenario);
            grpMonitoringControl.Controls.Add(lblScenario);
            grpMonitoringControl.Controls.Add(btnStopMonitoring);
            grpMonitoringControl.Controls.Add(btnStartMonitoring);
            grpMonitoringControl.Controls.Add(btnCollectOnce);
            grpMonitoringControl.Controls.Add(numSampleInterval);
            grpMonitoringControl.Controls.Add(cmbDevice);
            grpMonitoringControl.Controls.Add(cmbDataSource);
            grpMonitoringControl.Dock = DockStyle.Top;
            grpMonitoringControl.Location = new Point(0, 0);
            grpMonitoringControl.Name = "grpMonitoringControl";
            grpMonitoringControl.Size = new Size(898, 120);
            grpMonitoringControl.TabIndex = 0;
            grpMonitoringControl.TabStop = false;
            grpMonitoringControl.Text = "监控控制";
            // 
            // cmbScenario
            // 
            cmbScenario.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbScenario.FormattingEnabled = true;
            cmbScenario.Location = new Point(533, 37);
            cmbScenario.Name = "cmbScenario";
            cmbScenario.Size = new Size(169, 32);
            cmbScenario.TabIndex = 5;
            // 
            // lblScenario
            // 
            lblScenario.AutoSize = true;
            lblScenario.Location = new Point(445, 44);
            lblScenario.Name = "lblScenario";
            lblScenario.Size = new Size(82, 24);
            lblScenario.TabIndex = 4;
            lblScenario.Text = "模拟场景";
            // 
            // btnStopMonitoring
            // 
            btnStopMonitoring.Location = new Point(512, 74);
            btnStopMonitoring.Name = "btnStopMonitoring";
            btnStopMonitoring.Size = new Size(112, 34);
            btnStopMonitoring.TabIndex = 3;
            btnStopMonitoring.Text = "停止监控";
            btnStopMonitoring.UseVisualStyleBackColor = true;
            // 
            // btnStartMonitoring
            // 
            btnStartMonitoring.Location = new Point(279, 76);
            btnStartMonitoring.Name = "btnStartMonitoring";
            btnStartMonitoring.Size = new Size(112, 34);
            btnStartMonitoring.TabIndex = 3;
            btnStartMonitoring.Text = "开始监控";
            btnStartMonitoring.UseVisualStyleBackColor = true;
            // 
            // btnCollectOnce
            // 
            btnCollectOnce.Location = new Point(62, 76);
            btnCollectOnce.Name = "btnCollectOnce";
            btnCollectOnce.Size = new Size(112, 34);
            btnCollectOnce.TabIndex = 3;
            btnCollectOnce.Text = "采集一次";
            btnCollectOnce.UseVisualStyleBackColor = true;
            // 
            // numSampleInterval
            // 
            numSampleInterval.Location = new Point(748, 38);
            numSampleInterval.Maximum = new decimal(new int[] { 60, 0, 0, 0 });
            numSampleInterval.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numSampleInterval.Name = "numSampleInterval";
            numSampleInterval.Size = new Size(143, 30);
            numSampleInterval.TabIndex = 2;
            numSampleInterval.Value = new decimal(new int[] { 3, 0, 0, 0 });
            // 
            // cmbDevice
            // 
            cmbDevice.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDevice.FormattingEnabled = true;
            cmbDevice.Location = new Point(242, 38);
            cmbDevice.Name = "cmbDevice";
            cmbDevice.Size = new Size(182, 32);
            cmbDevice.TabIndex = 1;
            // 
            // cmbDataSource
            // 
            cmbDataSource.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDataSource.FormattingEnabled = true;
            cmbDataSource.Location = new Point(20, 38);
            cmbDataSource.Name = "cmbDataSource";
            cmbDataSource.Size = new Size(182, 32);
            cmbDataSource.TabIndex = 0;
            // 
            // tabBookingManagement
            // 
            tabBookingManagement.Controls.Add(dgvBookings);
            tabBookingManagement.Controls.Add(grpBookingEditor);
            tabBookingManagement.Location = new Point(4, 33);
            tabBookingManagement.Name = "tabBookingManagement";
            tabBookingManagement.Padding = new Padding(3);
            tabBookingManagement.Size = new Size(1766, 759);
            tabBookingManagement.TabIndex = 1;
            tabBookingManagement.Text = "预约管理";
            tabBookingManagement.UseVisualStyleBackColor = true;
            // 
            // dgvBookings
            // 
            dgvBookings.AllowUserToAddRows = false;
            dgvBookings.AllowUserToDeleteRows = false;
            dgvBookings.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvBookings.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvBookings.Dock = DockStyle.Fill;
            dgvBookings.Location = new Point(3, 183);
            dgvBookings.MultiSelect = false;
            dgvBookings.Name = "dgvBookings";
            dgvBookings.ReadOnly = true;
            dgvBookings.RowHeadersVisible = false;
            dgvBookings.RowHeadersWidth = 62;
            dgvBookings.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvBookings.Size = new Size(1760, 573);
            dgvBookings.TabIndex = 1;
            dgvBookings.SelectionChanged += dgvBookings_SelectionChanged_1;
            // 
            // grpBookingEditor
            // 
            grpBookingEditor.Controls.Add(btnRefreshBookings);
            grpBookingEditor.Controls.Add(btnCompleteBooking);
            grpBookingEditor.Controls.Add(btnConfirmBooking);
            grpBookingEditor.Controls.Add(btnCancelBooking);
            grpBookingEditor.Controls.Add(btnCreateBooking);
            grpBookingEditor.Controls.Add(txtBookingPurpose);
            grpBookingEditor.Controls.Add(dtpBookingEnd);
            grpBookingEditor.Controls.Add(dtpBookingStart);
            grpBookingEditor.Controls.Add(cmbBookingUser);
            grpBookingEditor.Controls.Add(cmbBookingDevice);
            grpBookingEditor.Dock = DockStyle.Top;
            grpBookingEditor.Location = new Point(3, 3);
            grpBookingEditor.Name = "grpBookingEditor";
            grpBookingEditor.Size = new Size(1760, 180);
            grpBookingEditor.TabIndex = 0;
            grpBookingEditor.TabStop = false;
            grpBookingEditor.Text = "新建设备预约";
            // 
            // btnRefreshBookings
            // 
            btnRefreshBookings.Location = new Point(1622, 43);
            btnRefreshBookings.Name = "btnRefreshBookings";
            btnRefreshBookings.Size = new Size(112, 34);
            btnRefreshBookings.TabIndex = 6;
            btnRefreshBookings.Text = "刷新列表";
            btnRefreshBookings.UseVisualStyleBackColor = true;
            // 
            // btnCompleteBooking
            // 
            btnCompleteBooking.Enabled = false;
            btnCompleteBooking.Location = new Point(1494, 97);
            btnCompleteBooking.Name = "btnCompleteBooking";
            btnCompleteBooking.Size = new Size(112, 34);
            btnCompleteBooking.TabIndex = 6;
            btnCompleteBooking.Text = "完成预约";
            btnCompleteBooking.UseVisualStyleBackColor = true;
            btnCompleteBooking.Click += btnCompleteBooking_Click;
            // 
            // btnConfirmBooking
            // 
            btnConfirmBooking.Enabled = false;
            btnConfirmBooking.Location = new Point(1376, 97);
            btnConfirmBooking.Name = "btnConfirmBooking";
            btnConfirmBooking.Size = new Size(112, 34);
            btnConfirmBooking.TabIndex = 6;
            btnConfirmBooking.Text = "确认预约";
            btnConfirmBooking.UseVisualStyleBackColor = true;
            btnConfirmBooking.Click += btnConfirmBooking_Click;
            // 
            // btnCancelBooking
            // 
            btnCancelBooking.Location = new Point(1494, 43);
            btnCancelBooking.Name = "btnCancelBooking";
            btnCancelBooking.Size = new Size(112, 34);
            btnCancelBooking.TabIndex = 6;
            btnCancelBooking.Text = "取消选中预约";
            btnCancelBooking.UseVisualStyleBackColor = true;
            // 
            // btnCreateBooking
            // 
            btnCreateBooking.Location = new Point(1376, 43);
            btnCreateBooking.Name = "btnCreateBooking";
            btnCreateBooking.Size = new Size(112, 34);
            btnCreateBooking.TabIndex = 6;
            btnCreateBooking.Text = "创建预约";
            btnCreateBooking.UseVisualStyleBackColor = true;
            // 
            // txtBookingPurpose
            // 
            txtBookingPurpose.Location = new Point(1207, 47);
            txtBookingPurpose.Name = "txtBookingPurpose";
            txtBookingPurpose.Size = new Size(150, 30);
            txtBookingPurpose.TabIndex = 5;
            // 
            // dtpBookingEnd
            // 
            dtpBookingEnd.Location = new Point(877, 49);
            dtpBookingEnd.Name = "dtpBookingEnd";
            dtpBookingEnd.Size = new Size(300, 30);
            dtpBookingEnd.TabIndex = 4;
            // 
            // dtpBookingStart
            // 
            dtpBookingStart.Location = new Point(535, 49);
            dtpBookingStart.Name = "dtpBookingStart";
            dtpBookingStart.Size = new Size(300, 30);
            dtpBookingStart.TabIndex = 3;
            // 
            // cmbBookingUser
            // 
            cmbBookingUser.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbBookingUser.FormattingEnabled = true;
            cmbBookingUser.Location = new Point(275, 51);
            cmbBookingUser.Name = "cmbBookingUser";
            cmbBookingUser.Size = new Size(182, 32);
            cmbBookingUser.TabIndex = 2;
            // 
            // cmbBookingDevice
            // 
            cmbBookingDevice.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbBookingDevice.FormattingEnabled = true;
            cmbBookingDevice.Location = new Point(40, 51);
            cmbBookingDevice.Name = "cmbBookingDevice";
            cmbBookingDevice.Size = new Size(182, 32);
            cmbBookingDevice.TabIndex = 1;
            // 
            // tabAlarmManagement
            // 
            tabAlarmManagement.Controls.Add(dgvAlarms);
            tabAlarmManagement.Controls.Add(grpAlarmActions);
            tabAlarmManagement.Location = new Point(4, 33);
            tabAlarmManagement.Name = "tabAlarmManagement";
            tabAlarmManagement.Size = new Size(1766, 759);
            tabAlarmManagement.TabIndex = 2;
            tabAlarmManagement.Text = "报警管理";
            tabAlarmManagement.UseVisualStyleBackColor = true;
            // 
            // dgvAlarms
            // 
            dgvAlarms.AllowUserToAddRows = false;
            dgvAlarms.AllowUserToDeleteRows = false;
            dgvAlarms.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvAlarms.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvAlarms.Dock = DockStyle.Fill;
            dgvAlarms.Location = new Point(0, 90);
            dgvAlarms.MultiSelect = false;
            dgvAlarms.Name = "dgvAlarms";
            dgvAlarms.ReadOnly = true;
            dgvAlarms.RowHeadersVisible = false;
            dgvAlarms.RowHeadersWidth = 62;
            dgvAlarms.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvAlarms.Size = new Size(1766, 669);
            dgvAlarms.TabIndex = 2;
            // 
            // grpAlarmActions
            // 
            grpAlarmActions.Controls.Add(lblActiveAlarmCount);
            grpAlarmActions.Controls.Add(btnRefreshAlarms);
            grpAlarmActions.Controls.Add(btnAcknowledgeAlarm);
            grpAlarmActions.Controls.Add(cmbAlarmUser);
            grpAlarmActions.Dock = DockStyle.Top;
            grpAlarmActions.Location = new Point(0, 0);
            grpAlarmActions.Name = "grpAlarmActions";
            grpAlarmActions.Size = new Size(1766, 90);
            grpAlarmActions.TabIndex = 0;
            grpAlarmActions.TabStop = false;
            grpAlarmActions.Text = "报警处理";
            // 
            // lblActiveAlarmCount
            // 
            lblActiveAlarmCount.AutoSize = true;
            lblActiveAlarmCount.Location = new Point(643, 38);
            lblActiveAlarmCount.Name = "lblActiveAlarmCount";
            lblActiveAlarmCount.Size = new Size(129, 24);
            lblActiveAlarmCount.TabIndex = 10;
            lblActiveAlarmCount.Text = "未解除报警：0";
            // 
            // btnRefreshAlarms
            // 
            btnRefreshAlarms.Location = new Point(486, 33);
            btnRefreshAlarms.Name = "btnRefreshAlarms";
            btnRefreshAlarms.Size = new Size(112, 34);
            btnRefreshAlarms.TabIndex = 9;
            btnRefreshAlarms.Text = "刷新报警";
            btnRefreshAlarms.UseVisualStyleBackColor = true;
            // 
            // btnAcknowledgeAlarm
            // 
            btnAcknowledgeAlarm.Location = new Point(315, 31);
            btnAcknowledgeAlarm.Name = "btnAcknowledgeAlarm";
            btnAcknowledgeAlarm.Size = new Size(137, 34);
            btnAcknowledgeAlarm.TabIndex = 8;
            btnAcknowledgeAlarm.Text = "确认选中报警";
            btnAcknowledgeAlarm.UseVisualStyleBackColor = true;
            // 
            // cmbAlarmUser
            // 
            cmbAlarmUser.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbAlarmUser.FormattingEnabled = true;
            cmbAlarmUser.Location = new Point(47, 33);
            cmbAlarmUser.Name = "cmbAlarmUser";
            cmbAlarmUser.Size = new Size(224, 32);
            cmbAlarmUser.TabIndex = 7;
            // 
            // tabMaintenanceManagement
            // 
            tabMaintenanceManagement.Controls.Add(dgvMaintenance);
            tabMaintenanceManagement.Controls.Add(grpMaintenanceActions);
            tabMaintenanceManagement.Controls.Add(grpMaintenanceCreate);
            tabMaintenanceManagement.Location = new Point(4, 33);
            tabMaintenanceManagement.Name = "tabMaintenanceManagement";
            tabMaintenanceManagement.Size = new Size(1766, 759);
            tabMaintenanceManagement.TabIndex = 3;
            tabMaintenanceManagement.Text = "维护管理";
            tabMaintenanceManagement.UseVisualStyleBackColor = true;
            // 
            // dgvMaintenance
            // 
            dgvMaintenance.AllowUserToAddRows = false;
            dgvMaintenance.AllowUserToDeleteRows = false;
            dgvMaintenance.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvMaintenance.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvMaintenance.Dock = DockStyle.Fill;
            dgvMaintenance.Location = new Point(0, 250);
            dgvMaintenance.MultiSelect = false;
            dgvMaintenance.Name = "dgvMaintenance";
            dgvMaintenance.ReadOnly = true;
            dgvMaintenance.RowHeadersVisible = false;
            dgvMaintenance.RowHeadersWidth = 62;
            dgvMaintenance.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvMaintenance.Size = new Size(1766, 509);
            dgvMaintenance.TabIndex = 3;
            // 
            // grpMaintenanceActions
            // 
            grpMaintenanceActions.Controls.Add(chkVerificationPassed);
            grpMaintenanceActions.Controls.Add(btnRefreshMaintenance);
            grpMaintenanceActions.Controls.Add(btnCloseMaintenance);
            grpMaintenanceActions.Controls.Add(btnCompleteMaintenance);
            grpMaintenanceActions.Controls.Add(btnStartMaintenance);
            grpMaintenanceActions.Controls.Add(btnAssignMaintenance);
            grpMaintenanceActions.Controls.Add(txtMaintenanceResult);
            grpMaintenanceActions.Dock = DockStyle.Top;
            grpMaintenanceActions.Location = new Point(0, 140);
            grpMaintenanceActions.Name = "grpMaintenanceActions";
            grpMaintenanceActions.Size = new Size(1766, 110);
            grpMaintenanceActions.TabIndex = 1;
            grpMaintenanceActions.TabStop = false;
            grpMaintenanceActions.Text = "工单处理";
            // 
            // chkVerificationPassed
            // 
            chkVerificationPassed.AutoSize = true;
            chkVerificationPassed.Location = new Point(222, 57);
            chkVerificationPassed.Name = "chkVerificationPassed";
            chkVerificationPassed.Size = new Size(162, 28);
            chkVerificationPassed.TabIndex = 13;
            chkVerificationPassed.Text = "维修后验证通过";
            chkVerificationPassed.UseVisualStyleBackColor = true;
            // 
            // btnRefreshMaintenance
            // 
            btnRefreshMaintenance.Location = new Point(1124, 53);
            btnRefreshMaintenance.Name = "btnRefreshMaintenance";
            btnRefreshMaintenance.Size = new Size(112, 34);
            btnRefreshMaintenance.TabIndex = 12;
            btnRefreshMaintenance.Text = "刷新";
            btnRefreshMaintenance.UseVisualStyleBackColor = true;
            // 
            // btnCloseMaintenance
            // 
            btnCloseMaintenance.Location = new Point(958, 51);
            btnCloseMaintenance.Name = "btnCloseMaintenance";
            btnCloseMaintenance.Size = new Size(112, 34);
            btnCloseMaintenance.TabIndex = 12;
            btnCloseMaintenance.Text = "关闭工单";
            btnCloseMaintenance.UseVisualStyleBackColor = true;
            // 
            // btnCompleteMaintenance
            // 
            btnCompleteMaintenance.Location = new Point(759, 51);
            btnCompleteMaintenance.Name = "btnCompleteMaintenance";
            btnCompleteMaintenance.Size = new Size(112, 34);
            btnCompleteMaintenance.TabIndex = 12;
            btnCompleteMaintenance.Text = "完成维护";
            btnCompleteMaintenance.UseVisualStyleBackColor = true;
            // 
            // btnStartMaintenance
            // 
            btnStartMaintenance.Location = new Point(580, 51);
            btnStartMaintenance.Name = "btnStartMaintenance";
            btnStartMaintenance.Size = new Size(112, 34);
            btnStartMaintenance.TabIndex = 12;
            btnStartMaintenance.Text = "开始维护";
            btnStartMaintenance.UseVisualStyleBackColor = true;
            // 
            // btnAssignMaintenance
            // 
            btnAssignMaintenance.Location = new Point(420, 51);
            btnAssignMaintenance.Name = "btnAssignMaintenance";
            btnAssignMaintenance.Size = new Size(112, 34);
            btnAssignMaintenance.TabIndex = 12;
            btnAssignMaintenance.Text = "指派";
            btnAssignMaintenance.UseVisualStyleBackColor = true;
            // 
            // txtMaintenanceResult
            // 
            txtMaintenanceResult.Location = new Point(35, 55);
            txtMaintenanceResult.Name = "txtMaintenanceResult";
            txtMaintenanceResult.Size = new Size(150, 30);
            txtMaintenanceResult.TabIndex = 11;
            // 
            // grpMaintenanceCreate
            // 
            grpMaintenanceCreate.Controls.Add(chkMaintenancePlan);
            grpMaintenanceCreate.Controls.Add(btnCreateMaintenance);
            grpMaintenanceCreate.Controls.Add(txtMaintenanceDescription);
            grpMaintenanceCreate.Controls.Add(dtpMaintenancePlanEnd);
            grpMaintenanceCreate.Controls.Add(dtpMaintenancePlanStart);
            grpMaintenanceCreate.Controls.Add(cmbMaintenanceAssignee);
            grpMaintenanceCreate.Controls.Add(cmbMaintenanceAlarm);
            grpMaintenanceCreate.Dock = DockStyle.Top;
            grpMaintenanceCreate.Location = new Point(0, 0);
            grpMaintenanceCreate.Name = "grpMaintenanceCreate";
            grpMaintenanceCreate.Size = new Size(1766, 140);
            grpMaintenanceCreate.TabIndex = 0;
            grpMaintenanceCreate.TabStop = false;
            grpMaintenanceCreate.Text = "从报警创建维护工单";
            // 
            // chkMaintenancePlan
            // 
            chkMaintenancePlan.AutoSize = true;
            chkMaintenancePlan.Location = new Point(673, 61);
            chkMaintenancePlan.Name = "chkMaintenancePlan";
            chkMaintenancePlan.Size = new Size(144, 28);
            chkMaintenancePlan.TabIndex = 13;
            chkMaintenancePlan.Text = "设置计划时间";
            chkMaintenancePlan.UseVisualStyleBackColor = true;
            // 
            // btnCreateMaintenance
            // 
            btnCreateMaintenance.Location = new Point(1588, 53);
            btnCreateMaintenance.Name = "btnCreateMaintenance";
            btnCreateMaintenance.Size = new Size(112, 34);
            btnCreateMaintenance.TabIndex = 12;
            btnCreateMaintenance.Text = "创建工单";
            btnCreateMaintenance.UseVisualStyleBackColor = true;
            // 
            // txtMaintenanceDescription
            // 
            txtMaintenanceDescription.Location = new Point(488, 59);
            txtMaintenanceDescription.Name = "txtMaintenanceDescription";
            txtMaintenanceDescription.Size = new Size(150, 30);
            txtMaintenanceDescription.TabIndex = 11;
            // 
            // dtpMaintenancePlanEnd
            // 
            dtpMaintenancePlanEnd.Location = new Point(1178, 57);
            dtpMaintenancePlanEnd.Name = "dtpMaintenancePlanEnd";
            dtpMaintenancePlanEnd.Size = new Size(300, 30);
            dtpMaintenancePlanEnd.TabIndex = 10;
            // 
            // dtpMaintenancePlanStart
            // 
            dtpMaintenancePlanStart.Location = new Point(836, 57);
            dtpMaintenancePlanStart.Name = "dtpMaintenancePlanStart";
            dtpMaintenancePlanStart.Size = new Size(300, 30);
            dtpMaintenancePlanStart.TabIndex = 9;
            // 
            // cmbMaintenanceAssignee
            // 
            cmbMaintenanceAssignee.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMaintenanceAssignee.FormattingEnabled = true;
            cmbMaintenanceAssignee.Location = new Point(270, 59);
            cmbMaintenanceAssignee.Name = "cmbMaintenanceAssignee";
            cmbMaintenanceAssignee.Size = new Size(182, 32);
            cmbMaintenanceAssignee.TabIndex = 8;
            // 
            // cmbMaintenanceAlarm
            // 
            cmbMaintenanceAlarm.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMaintenanceAlarm.FormattingEnabled = true;
            cmbMaintenanceAlarm.Location = new Point(35, 59);
            cmbMaintenanceAlarm.Name = "cmbMaintenanceAlarm";
            cmbMaintenanceAlarm.Size = new Size(182, 32);
            cmbMaintenanceAlarm.TabIndex = 7;
            // 
            // tabHistory
            // 
            tabHistory.Controls.Add(dgvTelemetryHistory);
            tabHistory.Controls.Add(pnlHistoryQuery);
            tabHistory.Location = new Point(4, 33);
            tabHistory.Name = "tabHistory";
            tabHistory.Size = new Size(1766, 759);
            tabHistory.TabIndex = 4;
            tabHistory.Text = "历史数据";
            tabHistory.UseVisualStyleBackColor = true;
            // 
            // dgvTelemetryHistory
            // 
            dgvTelemetryHistory.AllowUserToAddRows = false;
            dgvTelemetryHistory.AllowUserToDeleteRows = false;
            dgvTelemetryHistory.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvTelemetryHistory.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvTelemetryHistory.Dock = DockStyle.Fill;
            dgvTelemetryHistory.Location = new Point(0, 75);
            dgvTelemetryHistory.MultiSelect = false;
            dgvTelemetryHistory.Name = "dgvTelemetryHistory";
            dgvTelemetryHistory.ReadOnly = true;
            dgvTelemetryHistory.RowHeadersVisible = false;
            dgvTelemetryHistory.RowHeadersWidth = 62;
            dgvTelemetryHistory.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvTelemetryHistory.Size = new Size(1766, 684);
            dgvTelemetryHistory.TabIndex = 4;
            // 
            // pnlHistoryQuery
            // 
            pnlHistoryQuery.Controls.Add(lblHistoryCount);
            pnlHistoryQuery.Controls.Add(btnExportHistory);
            pnlHistoryQuery.Controls.Add(btnRecent100);
            pnlHistoryQuery.Controls.Add(numHistoryLimit);
            pnlHistoryQuery.Controls.Add(dtpHistoryEnd);
            pnlHistoryQuery.Controls.Add(lblHistoryEnd);
            pnlHistoryQuery.Controls.Add(btnQueryHistory);
            pnlHistoryQuery.Controls.Add(dtpHistoryStart);
            pnlHistoryQuery.Controls.Add(lblHistoryLimit);
            pnlHistoryQuery.Controls.Add(lblHistoryStart);
            pnlHistoryQuery.Controls.Add(lblHistoryDevice);
            pnlHistoryQuery.Controls.Add(cmbHistoryDevice);
            pnlHistoryQuery.Dock = DockStyle.Top;
            pnlHistoryQuery.Location = new Point(0, 0);
            pnlHistoryQuery.Name = "pnlHistoryQuery";
            pnlHistoryQuery.Size = new Size(1766, 75);
            pnlHistoryQuery.TabIndex = 2;
            // 
            // lblHistoryCount
            // 
            lblHistoryCount.AutoSize = true;
            lblHistoryCount.Location = new Point(1633, 18);
            lblHistoryCount.Name = "lblHistoryCount";
            lblHistoryCount.Size = new Size(67, 24);
            lblHistoryCount.TabIndex = 21;
            lblHistoryCount.Text = "共 0 条";
            // 
            // btnExportHistory
            // 
            btnExportHistory.Enabled = false;
            btnExportHistory.Location = new Point(1473, 14);
            btnExportHistory.Name = "btnExportHistory";
            btnExportHistory.Size = new Size(112, 34);
            btnExportHistory.TabIndex = 20;
            btnExportHistory.Text = "导出CSV";
            btnExportHistory.UseVisualStyleBackColor = true;
            btnExportHistory.Click += btnExportHistory_Click;
            // 
            // btnRecent100
            // 
            btnRecent100.Location = new Point(1342, 16);
            btnRecent100.Name = "btnRecent100";
            btnRecent100.Size = new Size(112, 34);
            btnRecent100.TabIndex = 19;
            btnRecent100.Text = "最近100条";
            btnRecent100.UseVisualStyleBackColor = true;
            btnRecent100.Click += btnRecent100_Click;
            // 
            // numHistoryLimit
            // 
            numHistoryLimit.Location = new Point(1083, 21);
            numHistoryLimit.Maximum = new decimal(new int[] { 5000, 0, 0, 0 });
            numHistoryLimit.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numHistoryLimit.Name = "numHistoryLimit";
            numHistoryLimit.Size = new Size(111, 30);
            numHistoryLimit.TabIndex = 18;
            numHistoryLimit.Value = new decimal(new int[] { 500, 0, 0, 0 });
            // 
            // dtpHistoryEnd
            // 
            dtpHistoryEnd.CustomFormat = "yyyy-MM-dd HH:mm:ss";
            dtpHistoryEnd.Format = DateTimePickerFormat.Custom;
            dtpHistoryEnd.Location = new Point(748, 18);
            dtpHistoryEnd.Name = "dtpHistoryEnd";
            dtpHistoryEnd.Size = new Size(213, 30);
            dtpHistoryEnd.TabIndex = 17;
            // 
            // lblHistoryEnd
            // 
            lblHistoryEnd.AutoSize = true;
            lblHistoryEnd.Location = new Point(651, 18);
            lblHistoryEnd.Name = "lblHistoryEnd";
            lblHistoryEnd.Size = new Size(100, 24);
            lblHistoryEnd.TabIndex = 16;
            lblHistoryEnd.Text = "结束时间：";
            // 
            // btnQueryHistory
            // 
            btnQueryHistory.Location = new Point(1224, 16);
            btnQueryHistory.Name = "btnQueryHistory";
            btnQueryHistory.Size = new Size(112, 34);
            btnQueryHistory.TabIndex = 15;
            btnQueryHistory.Text = "查询";
            btnQueryHistory.UseVisualStyleBackColor = true;
            btnQueryHistory.Click += btnQueryHistory_Click;
            // 
            // dtpHistoryStart
            // 
            dtpHistoryStart.CustomFormat = "yyyy-MM-dd HH:mm:ss";
            dtpHistoryStart.Format = DateTimePickerFormat.Custom;
            dtpHistoryStart.Location = new Point(419, 21);
            dtpHistoryStart.Name = "dtpHistoryStart";
            dtpHistoryStart.Size = new Size(213, 30);
            dtpHistoryStart.TabIndex = 13;
            // 
            // lblHistoryLimit
            // 
            lblHistoryLimit.AutoSize = true;
            lblHistoryLimit.Location = new Point(992, 21);
            lblHistoryLimit.Name = "lblHistoryLimit";
            lblHistoryLimit.Size = new Size(100, 24);
            lblHistoryLimit.TabIndex = 12;
            lblHistoryLimit.Text = "最大条数：";
            // 
            // lblHistoryStart
            // 
            lblHistoryStart.AutoSize = true;
            lblHistoryStart.Location = new Point(322, 21);
            lblHistoryStart.Name = "lblHistoryStart";
            lblHistoryStart.Size = new Size(100, 24);
            lblHistoryStart.TabIndex = 12;
            lblHistoryStart.Text = "开始时间：";
            // 
            // lblHistoryDevice
            // 
            lblHistoryDevice.AutoSize = true;
            lblHistoryDevice.Location = new Point(8, 21);
            lblHistoryDevice.Name = "lblHistoryDevice";
            lblHistoryDevice.Size = new Size(64, 24);
            lblHistoryDevice.TabIndex = 12;
            lblHistoryDevice.Text = "设备：";
            // 
            // cmbHistoryDevice
            // 
            cmbHistoryDevice.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbHistoryDevice.FormattingEnabled = true;
            cmbHistoryDevice.Location = new Point(64, 18);
            cmbHistoryDevice.Name = "cmbHistoryDevice";
            cmbHistoryDevice.Size = new Size(224, 32);
            cmbHistoryDevice.TabIndex = 11;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1774, 796);
            Controls.Add(tabMain);
            MinimumSize = new Size(1100, 650);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "工业设备运行、预约与维护管理系统";
            WindowState = FormWindowState.Maximized;
            tabMain.ResumeLayout(false);
            tabDashboard.ResumeLayout(false);
            tabDashboard.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            tabDeviceMonitoring.ResumeLayout(false);
            splitMonitoring.Panel1.ResumeLayout(false);
            splitMonitoring.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitMonitoring).EndInit();
            splitMonitoring.ResumeLayout(false);
            grpDevices.ResumeLayout(false);
            grpDevices.PerformLayout();
            statusMain.ResumeLayout(false);
            statusMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvDevices).EndInit();
            GroupBox.ResumeLayout(false);
            GroupBox.PerformLayout();
            grpRealtimeData.ResumeLayout(false);
            grpRealtimeData.PerformLayout();
            grpMonitoringControl.ResumeLayout(false);
            grpMonitoringControl.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numSampleInterval).EndInit();
            tabBookingManagement.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvBookings).EndInit();
            grpBookingEditor.ResumeLayout(false);
            grpBookingEditor.PerformLayout();
            tabAlarmManagement.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvAlarms).EndInit();
            grpAlarmActions.ResumeLayout(false);
            grpAlarmActions.PerformLayout();
            tabMaintenanceManagement.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvMaintenance).EndInit();
            grpMaintenanceActions.ResumeLayout(false);
            grpMaintenanceActions.PerformLayout();
            grpMaintenanceCreate.ResumeLayout(false);
            grpMaintenanceCreate.PerformLayout();
            tabHistory.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvTelemetryHistory).EndInit();
            pnlHistoryQuery.ResumeLayout(false);
            pnlHistoryQuery.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numHistoryLimit).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabMain;
        private TabPage tabDeviceMonitoring;
        private SplitContainer splitMonitoring;
        private GroupBox grpDevices;
        private TabPage tabBookingManagement;
        private TabPage tabAlarmManagement;
        private TabPage tabMaintenanceManagement;
        private DataGridView dgvDevices;
        private GroupBox grpMonitoringControl;
        private Button btnStopMonitoring;
        private Button btnStartMonitoring;
        private Button btnCollectOnce;
        private NumericUpDown numSampleInterval;
        private ComboBox cmbDevice;
        private ComboBox cmbDataSource;
        private GroupBox grpRealtimeData;
        private Label lblFaultValue;
        private Label lblRunningValue;
        private Label lblVibrationValue;
        private Label lblPressureValue;
        private Label lblTemperatureValue;
        private Label lblAlarmValue;
        private Label lblCollectedAtValue;
        private Label lblDeviceStatusValue;
        private Label lblQualityValue;
        private Label lblEmergencyValue;
        private StatusStrip statusMain;
        private GroupBox GroupBox;
        private TextBox txtRuntimeLog;
        private ToolStripStatusLabel tslDataSource;
        private ToolStripStatusLabel tslConnection;
        private ToolStripStatusLabel tslCurrentTime;
        private Label lblScenario;
        private ComboBox cmbScenario;
        private GroupBox grpBookingEditor;
        private DataGridView dgvBookings;
        private Button btnCancelBooking;
        private Button btnCreateBooking;
        private TextBox txtBookingPurpose;
        private DateTimePicker dtpBookingEnd;
        private DateTimePicker dtpBookingStart;
        private ComboBox cmbBookingUser;
        private ComboBox cmbBookingDevice;
        private Button btnRefreshBookings;
        private GroupBox grpAlarmActions;
        private Label lblActiveAlarmCount;
        private Button btnRefreshAlarms;
        private Button btnAcknowledgeAlarm;
        private ComboBox cmbAlarmUser;
        private DataGridView dgvAlarms;
        private GroupBox grpMaintenanceCreate;
        private Button btnCreateMaintenance;
        private TextBox txtMaintenanceDescription;
        private DateTimePicker dtpMaintenancePlanEnd;
        private DateTimePicker dtpMaintenancePlanStart;
        private ComboBox cmbMaintenanceAssignee;
        private ComboBox cmbMaintenanceAlarm;
        private CheckBox chkMaintenancePlan;
        private GroupBox grpMaintenanceActions;
        private CheckBox chkVerificationPassed;
        private Button btnRefreshMaintenance;
        private Button btnCloseMaintenance;
        private Button btnCompleteMaintenance;
        private Button btnStartMaintenance;
        private Button btnAssignMaintenance;
        private TextBox txtMaintenanceResult;
        private DataGridView dgvMaintenance;
        private ToolStripStatusLabel tslStudent;
        private Button btnCompleteBooking;
        private Button btnConfirmBooking;
        private TabPage tabHistory;
        private Panel pnlHistoryQuery;
        private Button btnQueryHistory;
        private DateTimePicker dtpHistoryStart;
        private Label lblHistoryStart;
        private Label lblHistoryDevice;
        private ComboBox cmbHistoryDevice;
        private NumericUpDown numHistoryLimit;
        private DateTimePicker dtpHistoryEnd;
        private Label lblHistoryEnd;
        private Label lblHistoryLimit;
        private Button btnExportHistory;
        private Button btnRecent100;
        private Label lblHistoryCount;
        private DataGridView dgvTelemetryHistory;
        private TabPage tabDashboard;
        private Label lblDashboardRefreshTime;
        private Button btnRefreshDashboard;
        private TableLayoutPanel tableLayoutPanel1;
        private Label lblDashboardMaintenanceCount;
        private Label lblDashboardAlarmCount;
        private Label lblDashboardBookingCount;
        private Label lblDashboardFaultCount;
        private Label lblDashboardRunningCount;
        private Label lblDashboardDeviceCount;
    }
}
