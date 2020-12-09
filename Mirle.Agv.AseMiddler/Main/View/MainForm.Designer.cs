using Mirle.Agv.AseMiddler;

namespace Mirle.Agv.AseMiddler.View
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.ToolStripMenuItemSystem = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItemLogin = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItemLogout = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItemCloseApp = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItemLanguage = new System.Windows.Forms.ToolStripMenuItem();
            this.中文ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.englishToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItemMode = new System.Windows.Forms.ToolStripMenuItem();
            this.VehicleConfigPage = new System.Windows.Forms.ToolStripMenuItem();
            this.AlarmPage = new System.Windows.Forms.ToolStripMenuItem();
            this.AgvcConnectorPage = new System.Windows.Forms.ToolStripMenuItem();
            this.AgvlConnectorPage = new System.Windows.Forms.ToolStripMenuItem();
            this.timeUpdateUI = new System.Windows.Forms.Timer(this.components);
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tspbCommding = new System.Windows.Forms.ToolStripProgressBar();
            this.tstextClientName = new System.Windows.Forms.ToolStripStatusLabel();
            this.tstextRemoteIp = new System.Windows.Forms.ToolStripStatusLabel();
            this.tstextRemotePort = new System.Windows.Forms.ToolStripStatusLabel();
            this.tstextLastPosX = new System.Windows.Forms.ToolStripStatusLabel();
            this.tstextLastPosY = new System.Windows.Forms.ToolStripStatusLabel();
            this.timer_SetupInitialSoc = new System.Windows.Forms.Timer(this.components);
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.tbxDebugLogMsg = new System.Windows.Forms.TextBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.pageBasicState = new System.Windows.Forms.TabPage();
            this.cbIsIgnoreAppendDebug = new System.Windows.Forms.CheckBox();
            this.groupHighLevelManualControl = new System.Windows.Forms.GroupBox();
            this.txtDisableChargerAddressId = new System.Windows.Forms.TextBox();
            this.checkEnableToCharge = new System.Windows.Forms.CheckBox();
            this.checkBoxDisableLeftSlot = new System.Windows.Forms.CheckBox();
            this.checkBoxDisableRightSlot = new System.Windows.Forms.CheckBox();
            this.btnCharge = new System.Windows.Forms.Button();
            this.btnStopCharge = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.btnRefreshStatus = new System.Windows.Forms.Button();
            this.btnPrintScreen = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.ucCurrentTransferStepType = new Mirle.Agv.AseMiddler.UcLabelTextBox();
            this.ucOpPauseFlag = new Mirle.Agv.AseMiddler.UcLabelTextBox();
            this.ucPauseFlag = new Mirle.Agv.AseMiddler.UcLabelTextBox();
            this.ucReserveFlag = new Mirle.Agv.AseMiddler.UcLabelTextBox();
            this.ucErrorFlag = new Mirle.Agv.AseMiddler.UcLabelTextBox();
            this.ucCommanding = new Mirle.Agv.AseMiddler.UcLabelTextBox();
            this.gbPerformanceCounter = new System.Windows.Forms.GroupBox();
            this.ucHeadAngle = new Mirle.Agv.AseMiddler.UcLabelTextBox();
            this.ucRCstId = new Mirle.Agv.AseMiddler.UcLabelTextBox();
            this.ucLCstId = new Mirle.Agv.AseMiddler.UcLabelTextBox();
            this.ucMapAddressId = new Mirle.Agv.AseMiddler.UcLabelTextBox();
            this.ucRobotHome = new Mirle.Agv.AseMiddler.UcLabelTextBox();
            this.ucCharging = new Mirle.Agv.AseMiddler.UcLabelTextBox();
            this.ucWifiSignalStrength = new Mirle.Agv.AseMiddler.UcLabelTextBox();
            this.ucCommandCount = new Mirle.Agv.AseMiddler.UcLabelTextBox();
            this.ucSoc = new Mirle.Agv.AseMiddler.UcLabelTextBox();
            this.gbConnection = new System.Windows.Forms.GroupBox();
            this.txtAgvcConnection = new System.Windows.Forms.Label();
            this.radAgvcOnline = new System.Windows.Forms.RadioButton();
            this.radAgvcOffline = new System.Windows.Forms.RadioButton();
            this.txtLastAlarm = new System.Windows.Forms.Label();
            this.btnAlarmReset = new System.Windows.Forms.Button();
            this.txtCannotAutoReason = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtAgvlConnection = new System.Windows.Forms.Label();
            this.radAgvlOnline = new System.Windows.Forms.RadioButton();
            this.radAgvlOffline = new System.Windows.Forms.RadioButton();
            this.btnAutoManual = new System.Windows.Forms.Button();
            this.pageMoveState = new System.Windows.Forms.TabPage();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.txtVehiclePauseFlags = new System.Windows.Forms.TextBox();
            this.btnRefreshMoveState = new System.Windows.Forms.Button();
            this.ucMoveMovingIndex = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucMoveMoveState = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucMovePauseStop = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucMoveReserveStop = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucMoveLastAddress = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucMoveIsMoveEnd = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucMoveLastSection = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucMovePositionY = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucMovePositionX = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.pageRobotSate = new System.Windows.Forms.TabPage();
            this.btnRefreshRobotState = new System.Windows.Forms.Button();
            this.ucRobotIsHome = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucRobotSlotRId = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucRobotSlotRState = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucRobotSlotLState = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucRobotSlotLId = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucRobotRobotState = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.pageBatteryState = new System.Windows.Forms.TabPage();
            this.ucAddress = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucChargeCount = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucCurransferStepType = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucStepsCount = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucIsSimulation = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucIsCharger = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucIsArrivalCharge = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucIsLowPowerStartChargeTimeout = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucIsLowPower = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucIsOptimize = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucIsVehicleIdle = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucAutoState = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.btnRefreshBatteryState = new System.Windows.Forms.Button();
            this.ucBatteryCharging = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucBatteryTemperature = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucBatteryVoltage = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucBatteryPercentage = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.pageVehicleState = new System.Windows.Forms.TabPage();
            this.labException = new System.Windows.Forms.Label();
            this.txtVisitTransferCount = new System.Windows.Forms.Label();
            this.lab1 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.txtBatterysAbnormal = new System.Windows.Forms.Label();
            this.txtMainFlowAbnormal = new System.Windows.Forms.Label();
            this.txtAgvcConnectorAbnormal = new System.Windows.Forms.Label();
            this.txtRobotAbnormal = new System.Windows.Forms.Label();
            this.txtMoveControlAbnormal = new System.Windows.Forms.Label();
            this.ucIsCharging = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucIsHome = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucIsSleepByAskReserveFail = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucIsMoveEnd = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucIsMoveStep = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucIsAskReservePause = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucTransferCommandTransferStep = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucTransferCommandEnrouteState = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.pageReserveInfo = new System.Windows.Forms.TabPage();
            this.gbReserve = new System.Windows.Forms.GroupBox();
            this.lbxReserveOkSections = new System.Windows.Forms.ListBox();
            this.lbxNeedReserveSections = new System.Windows.Forms.ListBox();
            this.pageTransferCommand = new System.Windows.Forms.TabPage();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.txtTransferCommand03 = new System.Windows.Forms.TextBox();
            this.txtTransferCommand04 = new System.Windows.Forms.TextBox();
            this.txtTransferCommand02 = new System.Windows.Forms.TextBox();
            this.txtTransferCommand01 = new System.Windows.Forms.TextBox();
            this.pageSimulator = new System.Windows.Forms.TabPage();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.btnKeyInTestAlarm = new System.Windows.Forms.Button();
            this.numTestErrorCode = new System.Windows.Forms.NumericUpDown();
            this.gbVehicleLocation = new System.Windows.Forms.GroupBox();
            this.numPositionY = new System.Windows.Forms.NumericUpDown();
            this.numPositionX = new System.Windows.Forms.NumericUpDown();
            this.btnKeyInPosition = new System.Windows.Forms.Button();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.btnKeyInSoc = new System.Windows.Forms.Button();
            this.numSoc = new System.Windows.Forms.NumericUpDown();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.pageBasicState.SuspendLayout();
            this.groupHighLevelManualControl.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.gbPerformanceCounter.SuspendLayout();
            this.gbConnection.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.pageMoveState.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.pageRobotSate.SuspendLayout();
            this.pageBatteryState.SuspendLayout();
            this.pageVehicleState.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.pageReserveInfo.SuspendLayout();
            this.gbReserve.SuspendLayout();
            this.pageTransferCommand.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.pageSimulator.SuspendLayout();
            this.groupBox6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTestErrorCode)).BeginInit();
            this.gbVehicleLocation.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPositionY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPositionX)).BeginInit();
            this.groupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numSoc)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItemSystem,
            this.ToolStripMenuItemLanguage,
            this.ToolStripMenuItemMode});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1375, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // ToolStripMenuItemSystem
            // 
            this.ToolStripMenuItemSystem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItemLogin,
            this.ToolStripMenuItemLogout,
            this.ToolStripMenuItemCloseApp});
            this.ToolStripMenuItemSystem.Name = "ToolStripMenuItemSystem";
            this.ToolStripMenuItemSystem.Size = new System.Drawing.Size(43, 20);
            this.ToolStripMenuItemSystem.Text = "系統";
            // 
            // ToolStripMenuItemLogin
            // 
            this.ToolStripMenuItemLogin.Name = "ToolStripMenuItemLogin";
            this.ToolStripMenuItemLogin.Size = new System.Drawing.Size(98, 22);
            this.ToolStripMenuItemLogin.Text = "登入";
            this.ToolStripMenuItemLogin.Click += new System.EventHandler(this.ToolStripMenuItemLogin_Click);
            // 
            // ToolStripMenuItemLogout
            // 
            this.ToolStripMenuItemLogout.Name = "ToolStripMenuItemLogout";
            this.ToolStripMenuItemLogout.Size = new System.Drawing.Size(98, 22);
            this.ToolStripMenuItemLogout.Text = "登出";
            this.ToolStripMenuItemLogout.Click += new System.EventHandler(this.ToolStripMenuItemLogout_Click);
            // 
            // ToolStripMenuItemCloseApp
            // 
            this.ToolStripMenuItemCloseApp.Name = "ToolStripMenuItemCloseApp";
            this.ToolStripMenuItemCloseApp.Size = new System.Drawing.Size(98, 22);
            this.ToolStripMenuItemCloseApp.Text = "關閉";
            this.ToolStripMenuItemCloseApp.Click += new System.EventHandler(this.關閉ToolStripMenuItem_Click);
            // 
            // ToolStripMenuItemLanguage
            // 
            this.ToolStripMenuItemLanguage.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.中文ToolStripMenuItem,
            this.englishToolStripMenuItem});
            this.ToolStripMenuItemLanguage.Name = "ToolStripMenuItemLanguage";
            this.ToolStripMenuItemLanguage.Size = new System.Drawing.Size(43, 20);
            this.ToolStripMenuItemLanguage.Text = "語言";
            // 
            // 中文ToolStripMenuItem
            // 
            this.中文ToolStripMenuItem.Name = "中文ToolStripMenuItem";
            this.中文ToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.中文ToolStripMenuItem.Text = "中文";
            // 
            // englishToolStripMenuItem
            // 
            this.englishToolStripMenuItem.Name = "englishToolStripMenuItem";
            this.englishToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.englishToolStripMenuItem.Text = "English";
            // 
            // ToolStripMenuItemMode
            // 
            this.ToolStripMenuItemMode.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.VehicleConfigPage,
            this.AlarmPage,
            this.AgvcConnectorPage,
            this.AgvlConnectorPage});
            this.ToolStripMenuItemMode.Name = "ToolStripMenuItemMode";
            this.ToolStripMenuItemMode.Size = new System.Drawing.Size(43, 20);
            this.ToolStripMenuItemMode.Text = "模式";
            // 
            // VehicleConfigPage
            // 
            this.VehicleConfigPage.Name = "VehicleConfigPage";
            this.VehicleConfigPage.Size = new System.Drawing.Size(122, 22);
            this.VehicleConfigPage.Text = "即時參數";
            this.VehicleConfigPage.Click += new System.EventHandler(this.VehicleStatusPage_Click);
            // 
            // AlarmPage
            // 
            this.AlarmPage.Name = "AlarmPage";
            this.AlarmPage.Size = new System.Drawing.Size(122, 22);
            this.AlarmPage.Text = "Alarm";
            this.AlarmPage.Click += new System.EventHandler(this.AlarmPage_Click);
            // 
            // AgvcConnectorPage
            // 
            this.AgvcConnectorPage.Name = "AgvcConnectorPage";
            this.AgvcConnectorPage.Size = new System.Drawing.Size(122, 22);
            this.AgvcConnectorPage.Text = "AGVC";
            this.AgvcConnectorPage.Click += new System.EventHandler(this.AgvcConnectorPage_Click);
            // 
            // AgvlConnectorPage
            // 
            this.AgvlConnectorPage.Name = "AgvlConnectorPage";
            this.AgvlConnectorPage.Size = new System.Drawing.Size(122, 22);
            this.AgvlConnectorPage.Text = "AGVL";
            this.AgvlConnectorPage.Click += new System.EventHandler(this.AgvlConnectorPage_Click);
            // 
            // timeUpdateUI
            // 
            this.timeUpdateUI.Enabled = true;
            this.timeUpdateUI.Interval = 2000;
            this.timeUpdateUI.Tick += new System.EventHandler(this.timeUpdateUI_Tick);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tspbCommding,
            this.tstextClientName,
            this.tstextRemoteIp,
            this.tstextRemotePort,
            this.tstextLastPosX,
            this.tstextLastPosY});
            this.statusStrip1.Location = new System.Drawing.Point(0, 739);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1375, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tspbCommding
            // 
            this.tspbCommding.Name = "tspbCommding";
            this.tspbCommding.Size = new System.Drawing.Size(100, 16);
            // 
            // tstextClientName
            // 
            this.tstextClientName.Name = "tstextClientName";
            this.tstextClientName.Size = new System.Drawing.Size(104, 17);
            this.tstextClientName.Text = "tstextClientName";
            // 
            // tstextRemoteIp
            // 
            this.tstextRemoteIp.Name = "tstextRemoteIp";
            this.tstextRemoteIp.Size = new System.Drawing.Size(93, 17);
            this.tstextRemoteIp.Text = "tstextRemoteIp";
            // 
            // tstextRemotePort
            // 
            this.tstextRemotePort.Name = "tstextRemotePort";
            this.tstextRemotePort.Size = new System.Drawing.Size(105, 17);
            this.tstextRemotePort.Text = "tstextRemotePort";
            // 
            // tstextLastPosX
            // 
            this.tstextLastPosX.Name = "tstextLastPosX";
            this.tstextLastPosX.Size = new System.Drawing.Size(90, 17);
            this.tstextLastPosX.Text = "tstextRealPosX";
            // 
            // tstextLastPosY
            // 
            this.tstextLastPosY.Name = "tstextLastPosY";
            this.tstextLastPosY.Size = new System.Drawing.Size(89, 17);
            this.tstextLastPosY.Text = "tstextRealPosY";
            // 
            // timer_SetupInitialSoc
            // 
            this.timer_SetupInitialSoc.Interval = 50;
            this.timer_SetupInitialSoc.Tick += new System.EventHandler(this.timer_SetupInitialSoc_Tick);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.AutoScroll = true;
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer3);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabControl1);
            this.splitContainer1.Size = new System.Drawing.Size(1375, 737);
            this.splitContainer1.SplitterDistance = 749;
            this.splitContainer1.TabIndex = 1;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.AutoScroll = true;
            this.splitContainer3.Panel1.BackColor = System.Drawing.Color.Transparent;
            this.splitContainer3.Panel1.Controls.Add(this.pictureBox1);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.tbxDebugLogMsg);
            this.splitContainer3.Size = new System.Drawing.Size(749, 737);
            this.splitContainer3.SplitterDistance = 319;
            this.splitContainer3.SplitterIncrement = 10;
            this.splitContainer3.TabIndex = 0;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(100, 100);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseDown);
            // 
            // tbxDebugLogMsg
            // 
            this.tbxDebugLogMsg.Font = new System.Drawing.Font("Times New Roman", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbxDebugLogMsg.Location = new System.Drawing.Point(3, 6);
            this.tbxDebugLogMsg.Multiline = true;
            this.tbxDebugLogMsg.Name = "tbxDebugLogMsg";
            this.tbxDebugLogMsg.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbxDebugLogMsg.Size = new System.Drawing.Size(742, 383);
            this.tbxDebugLogMsg.TabIndex = 58;
            this.tbxDebugLogMsg.WordWrap = false;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.pageBasicState);
            this.tabControl1.Controls.Add(this.pageMoveState);
            this.tabControl1.Controls.Add(this.pageRobotSate);
            this.tabControl1.Controls.Add(this.pageBatteryState);
            this.tabControl1.Controls.Add(this.pageVehicleState);
            this.tabControl1.Controls.Add(this.pageReserveInfo);
            this.tabControl1.Controls.Add(this.pageTransferCommand);
            this.tabControl1.Controls.Add(this.pageSimulator);
            this.tabControl1.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.tabControl1.Location = new System.Drawing.Point(3, 3);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(615, 709);
            this.tabControl1.TabIndex = 63;
            // 
            // pageBasicState
            // 
            this.pageBasicState.Controls.Add(this.pictureBox2);
            this.pageBasicState.Controls.Add(this.cbIsIgnoreAppendDebug);
            this.pageBasicState.Controls.Add(this.groupHighLevelManualControl);
            this.pageBasicState.Controls.Add(this.label1);
            this.pageBasicState.Controls.Add(this.btnRefreshStatus);
            this.pageBasicState.Controls.Add(this.btnPrintScreen);
            this.pageBasicState.Controls.Add(this.groupBox2);
            this.pageBasicState.Controls.Add(this.gbPerformanceCounter);
            this.pageBasicState.Controls.Add(this.gbConnection);
            this.pageBasicState.Controls.Add(this.txtLastAlarm);
            this.pageBasicState.Controls.Add(this.btnAlarmReset);
            this.pageBasicState.Controls.Add(this.txtCannotAutoReason);
            this.pageBasicState.Controls.Add(this.groupBox1);
            this.pageBasicState.Controls.Add(this.btnAutoManual);
            this.pageBasicState.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.pageBasicState.Location = new System.Drawing.Point(4, 29);
            this.pageBasicState.Name = "pageBasicState";
            this.pageBasicState.Size = new System.Drawing.Size(607, 676);
            this.pageBasicState.TabIndex = 6;
            this.pageBasicState.Text = "Basic";
            this.pageBasicState.UseVisualStyleBackColor = true;
            // 
            // cbIsIgnoreAppendDebug
            // 
            this.cbIsIgnoreAppendDebug.AutoSize = true;
            this.cbIsIgnoreAppendDebug.Location = new System.Drawing.Point(446, 252);
            this.cbIsIgnoreAppendDebug.Name = "cbIsIgnoreAppendDebug";
            this.cbIsIgnoreAppendDebug.Size = new System.Drawing.Size(156, 24);
            this.cbIsIgnoreAppendDebug.TabIndex = 65;
            this.cbIsIgnoreAppendDebug.Text = "關閉左下訊息顯示";
            this.cbIsIgnoreAppendDebug.UseVisualStyleBackColor = true;
            this.cbIsIgnoreAppendDebug.CheckedChanged += new System.EventHandler(this.cbIsIgnoreAppendDebug_CheckedChanged);
            // 
            // groupHighLevelManualControl
            // 
            this.groupHighLevelManualControl.Controls.Add(this.txtDisableChargerAddressId);
            this.groupHighLevelManualControl.Controls.Add(this.checkEnableToCharge);
            this.groupHighLevelManualControl.Controls.Add(this.checkBoxDisableLeftSlot);
            this.groupHighLevelManualControl.Controls.Add(this.checkBoxDisableRightSlot);
            this.groupHighLevelManualControl.Controls.Add(this.btnCharge);
            this.groupHighLevelManualControl.Controls.Add(this.btnStopCharge);
            this.groupHighLevelManualControl.Location = new System.Drawing.Point(440, 282);
            this.groupHighLevelManualControl.Name = "groupHighLevelManualControl";
            this.groupHighLevelManualControl.Size = new System.Drawing.Size(163, 391);
            this.groupHighLevelManualControl.TabIndex = 64;
            this.groupHighLevelManualControl.TabStop = false;
            this.groupHighLevelManualControl.Text = "Manual";
            // 
            // txtDisableChargerAddressId
            // 
            this.txtDisableChargerAddressId.ImeMode = System.Windows.Forms.ImeMode.Alpha;
            this.txtDisableChargerAddressId.Location = new System.Drawing.Point(4, 228);
            this.txtDisableChargerAddressId.Name = "txtDisableChargerAddressId";
            this.txtDisableChargerAddressId.Size = new System.Drawing.Size(153, 29);
            this.txtDisableChargerAddressId.TabIndex = 64;
            this.txtDisableChargerAddressId.Text = "Address ID";
            this.txtDisableChargerAddressId.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtDisableChargerAddressId.TextChanged += new System.EventHandler(this.txtDisableChargerAddressId_TextChanged);
            // 
            // checkEnableToCharge
            // 
            this.checkEnableToCharge.AutoSize = true;
            this.checkEnableToCharge.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.checkEnableToCharge.Location = new System.Drawing.Point(4, 267);
            this.checkEnableToCharge.Name = "checkEnableToCharge";
            this.checkEnableToCharge.Size = new System.Drawing.Size(108, 24);
            this.checkEnableToCharge.TabIndex = 63;
            this.checkEnableToCharge.Text = "視為充電站";
            this.checkEnableToCharge.UseVisualStyleBackColor = true;
            this.checkEnableToCharge.CheckedChanged += new System.EventHandler(this.checkEnableToCharge_CheckedChanged);
            // 
            // checkBoxDisableLeftSlot
            // 
            this.checkBoxDisableLeftSlot.AutoSize = true;
            this.checkBoxDisableLeftSlot.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.checkBoxDisableLeftSlot.Location = new System.Drawing.Point(6, 22);
            this.checkBoxDisableLeftSlot.Name = "checkBoxDisableLeftSlot";
            this.checkBoxDisableLeftSlot.Size = new System.Drawing.Size(140, 24);
            this.checkBoxDisableLeftSlot.TabIndex = 63;
            this.checkBoxDisableLeftSlot.Text = "禁用車上左儲位";
            this.checkBoxDisableLeftSlot.UseVisualStyleBackColor = true;
            this.checkBoxDisableLeftSlot.CheckedChanged += new System.EventHandler(this.checkBoxDisableSlot_CheckedChanged);
            // 
            // checkBoxDisableRightSlot
            // 
            this.checkBoxDisableRightSlot.AutoSize = true;
            this.checkBoxDisableRightSlot.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.checkBoxDisableRightSlot.Location = new System.Drawing.Point(6, 56);
            this.checkBoxDisableRightSlot.Name = "checkBoxDisableRightSlot";
            this.checkBoxDisableRightSlot.Size = new System.Drawing.Size(140, 24);
            this.checkBoxDisableRightSlot.TabIndex = 63;
            this.checkBoxDisableRightSlot.Text = "禁用車上右儲位";
            this.checkBoxDisableRightSlot.UseVisualStyleBackColor = true;
            this.checkBoxDisableRightSlot.CheckedChanged += new System.EventHandler(this.checkBoxDisableSlot_CheckedChanged);
            // 
            // btnCharge
            // 
            this.btnCharge.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnCharge.ForeColor = System.Drawing.Color.Red;
            this.btnCharge.Location = new System.Drawing.Point(4, 86);
            this.btnCharge.Name = "btnCharge";
            this.btnCharge.Size = new System.Drawing.Size(153, 65);
            this.btnCharge.TabIndex = 2;
            this.btnCharge.Text = "充電";
            this.btnCharge.UseVisualStyleBackColor = true;
            this.btnCharge.Click += new System.EventHandler(this.btnCharge_Click);
            // 
            // btnStopCharge
            // 
            this.btnStopCharge.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnStopCharge.ForeColor = System.Drawing.Color.Red;
            this.btnStopCharge.Location = new System.Drawing.Point(4, 157);
            this.btnStopCharge.Name = "btnStopCharge";
            this.btnStopCharge.Size = new System.Drawing.Size(153, 65);
            this.btnStopCharge.TabIndex = 2;
            this.btnStopCharge.Text = "中斷充電";
            this.btnStopCharge.UseVisualStyleBackColor = true;
            this.btnStopCharge.Click += new System.EventHandler(this.btnStopCharge_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(436, 151);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 20);
            this.label1.TabIndex = 62;
            this.label1.Text = "label1";
            // 
            // btnRefreshStatus
            // 
            this.btnRefreshStatus.Location = new System.Drawing.Point(6, 523);
            this.btnRefreshStatus.Name = "btnRefreshStatus";
            this.btnRefreshStatus.Size = new System.Drawing.Size(209, 74);
            this.btnRefreshStatus.TabIndex = 60;
            this.btnRefreshStatus.Text = "Refresh Status";
            this.btnRefreshStatus.UseVisualStyleBackColor = true;
            this.btnRefreshStatus.Click += new System.EventHandler(this.btnRefreshStatus_Click);
            // 
            // btnPrintScreen
            // 
            this.btnPrintScreen.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnPrintScreen.ForeColor = System.Drawing.Color.OrangeRed;
            this.btnPrintScreen.Location = new System.Drawing.Point(6, 599);
            this.btnPrintScreen.Name = "btnPrintScreen";
            this.btnPrintScreen.Size = new System.Drawing.Size(210, 74);
            this.btnPrintScreen.TabIndex = 59;
            this.btnPrintScreen.Text = "拍照截圖";
            this.btnPrintScreen.UseVisualStyleBackColor = true;
            this.btnPrintScreen.Click += new System.EventHandler(this.btnPrintScreen_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.ucCurrentTransferStepType);
            this.groupBox2.Controls.Add(this.ucOpPauseFlag);
            this.groupBox2.Controls.Add(this.ucPauseFlag);
            this.groupBox2.Controls.Add(this.ucReserveFlag);
            this.groupBox2.Controls.Add(this.ucErrorFlag);
            this.groupBox2.Controls.Add(this.ucCommanding);
            this.groupBox2.Location = new System.Drawing.Point(6, 282);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(209, 235);
            this.groupBox2.TabIndex = 61;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Flow Status";
            // 
            // ucCurrentTransferStepType
            // 
            this.ucCurrentTransferStepType.Location = new System.Drawing.Point(3, 21);
            this.ucCurrentTransferStepType.Margin = new System.Windows.Forms.Padding(8);
            this.ucCurrentTransferStepType.Name = "ucCurrentTransferStepType";
            this.ucCurrentTransferStepType.Size = new System.Drawing.Size(200, 30);
            this.ucCurrentTransferStepType.TabIndex = 1;
            this.ucCurrentTransferStepType.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucCurrentTransferStepType.TagName = "階段";
            this.ucCurrentTransferStepType.TagValue = "TagValue";
            // 
            // ucOpPauseFlag
            // 
            this.ucOpPauseFlag.Location = new System.Drawing.Point(4, 201);
            this.ucOpPauseFlag.Margin = new System.Windows.Forms.Padding(37);
            this.ucOpPauseFlag.Name = "ucOpPauseFlag";
            this.ucOpPauseFlag.Size = new System.Drawing.Size(200, 30);
            this.ucOpPauseFlag.TabIndex = 0;
            this.ucOpPauseFlag.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucOpPauseFlag.TagName = "遙控停車";
            this.ucOpPauseFlag.TagValue = "TagValue";
            // 
            // ucPauseFlag
            // 
            this.ucPauseFlag.Location = new System.Drawing.Point(4, 163);
            this.ucPauseFlag.Margin = new System.Windows.Forms.Padding(22);
            this.ucPauseFlag.Name = "ucPauseFlag";
            this.ucPauseFlag.Size = new System.Drawing.Size(200, 30);
            this.ucPauseFlag.TabIndex = 0;
            this.ucPauseFlag.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucPauseFlag.TagName = "AGVC停車";
            this.ucPauseFlag.TagValue = "TagValue";
            // 
            // ucReserveFlag
            // 
            this.ucReserveFlag.Location = new System.Drawing.Point(4, 127);
            this.ucReserveFlag.Margin = new System.Windows.Forms.Padding(13);
            this.ucReserveFlag.Name = "ucReserveFlag";
            this.ucReserveFlag.Size = new System.Drawing.Size(200, 30);
            this.ucReserveFlag.TabIndex = 0;
            this.ucReserveFlag.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucReserveFlag.TagName = "路權停車";
            this.ucReserveFlag.TagValue = "TagValue";
            // 
            // ucErrorFlag
            // 
            this.ucErrorFlag.Location = new System.Drawing.Point(4, 92);
            this.ucErrorFlag.Margin = new System.Windows.Forms.Padding(8);
            this.ucErrorFlag.Name = "ucErrorFlag";
            this.ucErrorFlag.Size = new System.Drawing.Size(200, 30);
            this.ucErrorFlag.TabIndex = 0;
            this.ucErrorFlag.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucErrorFlag.TagName = "重大異常";
            this.ucErrorFlag.TagValue = "TagValue";
            // 
            // ucCommanding
            // 
            this.ucCommanding.Location = new System.Drawing.Point(3, 57);
            this.ucCommanding.Margin = new System.Windows.Forms.Padding(5);
            this.ucCommanding.Name = "ucCommanding";
            this.ucCommanding.Size = new System.Drawing.Size(200, 30);
            this.ucCommanding.TabIndex = 0;
            this.ucCommanding.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucCommanding.TagName = "命令中";
            this.ucCommanding.TagValue = "TagValue";
            // 
            // gbPerformanceCounter
            // 
            this.gbPerformanceCounter.Controls.Add(this.ucHeadAngle);
            this.gbPerformanceCounter.Controls.Add(this.ucRCstId);
            this.gbPerformanceCounter.Controls.Add(this.ucLCstId);
            this.gbPerformanceCounter.Controls.Add(this.ucMapAddressId);
            this.gbPerformanceCounter.Controls.Add(this.ucRobotHome);
            this.gbPerformanceCounter.Controls.Add(this.ucCharging);
            this.gbPerformanceCounter.Controls.Add(this.ucWifiSignalStrength);
            this.gbPerformanceCounter.Controls.Add(this.ucCommandCount);
            this.gbPerformanceCounter.Controls.Add(this.ucSoc);
            this.gbPerformanceCounter.Location = new System.Drawing.Point(222, 282);
            this.gbPerformanceCounter.Name = "gbPerformanceCounter";
            this.gbPerformanceCounter.Size = new System.Drawing.Size(208, 391);
            this.gbPerformanceCounter.TabIndex = 10;
            this.gbPerformanceCounter.TabStop = false;
            this.gbPerformanceCounter.Text = "Vehicle Property";
            // 
            // ucHeadAngle
            // 
            this.ucHeadAngle.Location = new System.Drawing.Point(13, 56);
            this.ucHeadAngle.Margin = new System.Windows.Forms.Padding(8, 12, 8, 12);
            this.ucHeadAngle.Name = "ucHeadAngle";
            this.ucHeadAngle.Size = new System.Drawing.Size(187, 30);
            this.ucHeadAngle.TabIndex = 43;
            this.ucHeadAngle.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucHeadAngle.TagName = "車頭角度";
            this.ucHeadAngle.TagValue = "";
            // 
            // ucRCstId
            // 
            this.ucRCstId.Location = new System.Drawing.Point(13, 130);
            this.ucRCstId.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucRCstId.Name = "ucRCstId";
            this.ucRCstId.Size = new System.Drawing.Size(187, 30);
            this.ucRCstId.TabIndex = 42;
            this.ucRCstId.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucRCstId.TagName = "RCstId";
            this.ucRCstId.TagValue = "";
            // 
            // ucLCstId
            // 
            this.ucLCstId.Location = new System.Drawing.Point(13, 92);
            this.ucLCstId.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucLCstId.Name = "ucLCstId";
            this.ucLCstId.Size = new System.Drawing.Size(187, 30);
            this.ucLCstId.TabIndex = 42;
            this.ucLCstId.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucLCstId.TagName = "LCstId";
            this.ucLCstId.TagValue = "";
            // 
            // ucMapAddressId
            // 
            this.ucMapAddressId.Location = new System.Drawing.Point(13, 22);
            this.ucMapAddressId.Margin = new System.Windows.Forms.Padding(5, 7, 5, 7);
            this.ucMapAddressId.Name = "ucMapAddressId";
            this.ucMapAddressId.Size = new System.Drawing.Size(187, 30);
            this.ucMapAddressId.TabIndex = 3;
            this.ucMapAddressId.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucMapAddressId.TagName = "Addr";
            this.ucMapAddressId.TagValue = "";
            // 
            // ucRobotHome
            // 
            this.ucRobotHome.Location = new System.Drawing.Point(13, 165);
            this.ucRobotHome.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucRobotHome.Name = "ucRobotHome";
            this.ucRobotHome.Size = new System.Drawing.Size(187, 30);
            this.ucRobotHome.TabIndex = 3;
            this.ucRobotHome.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucRobotHome.TagName = "手臂Home";
            this.ucRobotHome.TagValue = "";
            // 
            // ucCharging
            // 
            this.ucCharging.Location = new System.Drawing.Point(13, 239);
            this.ucCharging.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucCharging.Name = "ucCharging";
            this.ucCharging.Size = new System.Drawing.Size(187, 30);
            this.ucCharging.TabIndex = 3;
            this.ucCharging.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucCharging.TagName = "充電中";
            this.ucCharging.TagValue = "";
            // 
            // ucWifiSignalStrength
            // 
            this.ucWifiSignalStrength.Location = new System.Drawing.Point(13, 317);
            this.ucWifiSignalStrength.Margin = new System.Windows.Forms.Padding(8, 12, 8, 12);
            this.ucWifiSignalStrength.Name = "ucWifiSignalStrength";
            this.ucWifiSignalStrength.Size = new System.Drawing.Size(187, 30);
            this.ucWifiSignalStrength.TabIndex = 2;
            this.ucWifiSignalStrength.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucWifiSignalStrength.TagName = "WIFI";
            this.ucWifiSignalStrength.TagValue = "";
            // 
            // ucCommandCount
            // 
            this.ucCommandCount.Location = new System.Drawing.Point(13, 280);
            this.ucCommandCount.Margin = new System.Windows.Forms.Padding(5, 7, 5, 7);
            this.ucCommandCount.Name = "ucCommandCount";
            this.ucCommandCount.Size = new System.Drawing.Size(187, 30);
            this.ucCommandCount.TabIndex = 2;
            this.ucCommandCount.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucCommandCount.TagName = "命令數";
            this.ucCommandCount.TagValue = "";
            // 
            // ucSoc
            // 
            this.ucSoc.Location = new System.Drawing.Point(13, 201);
            this.ucSoc.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucSoc.Name = "ucSoc";
            this.ucSoc.Size = new System.Drawing.Size(187, 30);
            this.ucSoc.TabIndex = 2;
            this.ucSoc.TagColor = System.Drawing.SystemColors.ControlText;
            this.ucSoc.TagName = "電量/電壓";
            this.ucSoc.TagValue = "";
            // 
            // gbConnection
            // 
            this.gbConnection.Controls.Add(this.txtAgvcConnection);
            this.gbConnection.Controls.Add(this.radAgvcOnline);
            this.gbConnection.Controls.Add(this.radAgvcOffline);
            this.gbConnection.Location = new System.Drawing.Point(6, 12);
            this.gbConnection.Name = "gbConnection";
            this.gbConnection.Size = new System.Drawing.Size(209, 86);
            this.gbConnection.TabIndex = 0;
            this.gbConnection.TabStop = false;
            this.gbConnection.Text = "AGVC Connection";
            // 
            // txtAgvcConnection
            // 
            this.txtAgvcConnection.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtAgvcConnection.Location = new System.Drawing.Point(6, 49);
            this.txtAgvcConnection.Name = "txtAgvcConnection";
            this.txtAgvcConnection.Size = new System.Drawing.Size(195, 24);
            this.txtAgvcConnection.TabIndex = 2;
            this.txtAgvcConnection.Text = "Connection";
            this.txtAgvcConnection.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // radAgvcOnline
            // 
            this.radAgvcOnline.AutoSize = true;
            this.radAgvcOnline.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.radAgvcOnline.Location = new System.Drawing.Point(107, 24);
            this.radAgvcOnline.Name = "radAgvcOnline";
            this.radAgvcOnline.Size = new System.Drawing.Size(78, 21);
            this.radAgvcOnline.TabIndex = 1;
            this.radAgvcOnline.Text = "Connect";
            this.radAgvcOnline.UseVisualStyleBackColor = true;
            this.radAgvcOnline.CheckedChanged += new System.EventHandler(this.radAgvcOnline_CheckedChanged);
            // 
            // radAgvcOffline
            // 
            this.radAgvcOffline.AutoSize = true;
            this.radAgvcOffline.Checked = true;
            this.radAgvcOffline.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.radAgvcOffline.Location = new System.Drawing.Point(3, 24);
            this.radAgvcOffline.Name = "radAgvcOffline";
            this.radAgvcOffline.Size = new System.Drawing.Size(98, 21);
            this.radAgvcOffline.TabIndex = 0;
            this.radAgvcOffline.TabStop = true;
            this.radAgvcOffline.Text = "DisConnect";
            this.radAgvcOffline.UseVisualStyleBackColor = true;
            this.radAgvcOffline.CheckedChanged += new System.EventHandler(this.radAgvcOffline_CheckedChanged);
            // 
            // txtLastAlarm
            // 
            this.txtLastAlarm.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtLastAlarm.Location = new System.Drawing.Point(221, 181);
            this.txtLastAlarm.Name = "txtLastAlarm";
            this.txtLastAlarm.Size = new System.Drawing.Size(155, 92);
            this.txtLastAlarm.TabIndex = 50;
            this.txtLastAlarm.Text = "Last Alarm";
            // 
            // btnAlarmReset
            // 
            this.btnAlarmReset.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnAlarmReset.ForeColor = System.Drawing.Color.Red;
            this.btnAlarmReset.Location = new System.Drawing.Point(6, 184);
            this.btnAlarmReset.Name = "btnAlarmReset";
            this.btnAlarmReset.Size = new System.Drawing.Size(209, 92);
            this.btnAlarmReset.TabIndex = 2;
            this.btnAlarmReset.Text = "Alarm Reset";
            this.btnAlarmReset.UseVisualStyleBackColor = true;
            this.btnAlarmReset.Click += new System.EventHandler(this.btnAlarmReset_Click);
            // 
            // txtCannotAutoReason
            // 
            this.txtCannotAutoReason.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtCannotAutoReason.Location = new System.Drawing.Point(218, 104);
            this.txtCannotAutoReason.Name = "txtCannotAutoReason";
            this.txtCannotAutoReason.Size = new System.Drawing.Size(213, 77);
            this.txtCannotAutoReason.TabIndex = 58;
            this.txtCannotAutoReason.Text = "Not Auto Reason";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtAgvlConnection);
            this.groupBox1.Controls.Add(this.radAgvlOnline);
            this.groupBox1.Controls.Add(this.radAgvlOffline);
            this.groupBox1.Location = new System.Drawing.Point(221, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(209, 86);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "AGVL Connection";
            // 
            // txtAgvlConnection
            // 
            this.txtAgvlConnection.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtAgvlConnection.Location = new System.Drawing.Point(6, 49);
            this.txtAgvlConnection.Name = "txtAgvlConnection";
            this.txtAgvlConnection.Size = new System.Drawing.Size(195, 24);
            this.txtAgvlConnection.TabIndex = 2;
            this.txtAgvlConnection.Text = "Connection";
            this.txtAgvlConnection.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // radAgvlOnline
            // 
            this.radAgvlOnline.AutoSize = true;
            this.radAgvlOnline.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.radAgvlOnline.Location = new System.Drawing.Point(107, 24);
            this.radAgvlOnline.Name = "radAgvlOnline";
            this.radAgvlOnline.Size = new System.Drawing.Size(78, 21);
            this.radAgvlOnline.TabIndex = 1;
            this.radAgvlOnline.Text = "Connect";
            this.radAgvlOnline.UseVisualStyleBackColor = true;
            this.radAgvlOnline.CheckedChanged += new System.EventHandler(this.radAgvlOnline_CheckedChanged);
            // 
            // radAgvlOffline
            // 
            this.radAgvlOffline.AutoSize = true;
            this.radAgvlOffline.Checked = true;
            this.radAgvlOffline.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.radAgvlOffline.Location = new System.Drawing.Point(3, 24);
            this.radAgvlOffline.Name = "radAgvlOffline";
            this.radAgvlOffline.Size = new System.Drawing.Size(98, 21);
            this.radAgvlOffline.TabIndex = 0;
            this.radAgvlOffline.TabStop = true;
            this.radAgvlOffline.Text = "DisConnect";
            this.radAgvlOffline.UseVisualStyleBackColor = true;
            this.radAgvlOffline.CheckedChanged += new System.EventHandler(this.radAgvlOffline_CheckedChanged);
            // 
            // btnAutoManual
            // 
            this.btnAutoManual.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnAutoManual.Location = new System.Drawing.Point(6, 104);
            this.btnAutoManual.Name = "btnAutoManual";
            this.btnAutoManual.Size = new System.Drawing.Size(209, 77);
            this.btnAutoManual.TabIndex = 53;
            this.btnAutoManual.Text = "Auto/Manual";
            this.btnAutoManual.UseVisualStyleBackColor = true;
            this.btnAutoManual.Click += new System.EventHandler(this.btnAutoManual_Click);
            // 
            // pageMoveState
            // 
            this.pageMoveState.Controls.Add(this.groupBox7);
            this.pageMoveState.Controls.Add(this.btnRefreshMoveState);
            this.pageMoveState.Controls.Add(this.ucMoveMovingIndex);
            this.pageMoveState.Controls.Add(this.ucMoveMoveState);
            this.pageMoveState.Controls.Add(this.ucMovePauseStop);
            this.pageMoveState.Controls.Add(this.ucMoveReserveStop);
            this.pageMoveState.Controls.Add(this.ucMoveLastAddress);
            this.pageMoveState.Controls.Add(this.ucMoveIsMoveEnd);
            this.pageMoveState.Controls.Add(this.ucMoveLastSection);
            this.pageMoveState.Controls.Add(this.ucMovePositionY);
            this.pageMoveState.Controls.Add(this.ucMovePositionX);
            this.pageMoveState.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.pageMoveState.Location = new System.Drawing.Point(4, 29);
            this.pageMoveState.Name = "pageMoveState";
            this.pageMoveState.Padding = new System.Windows.Forms.Padding(3);
            this.pageMoveState.Size = new System.Drawing.Size(607, 676);
            this.pageMoveState.TabIndex = 0;
            this.pageMoveState.Text = "Move";
            this.pageMoveState.UseVisualStyleBackColor = true;
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.txtVehiclePauseFlags);
            this.groupBox7.Location = new System.Drawing.Point(6, 335);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(291, 241);
            this.groupBox7.TabIndex = 44;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Pause Flags";
            // 
            // txtVehiclePauseFlags
            // 
            this.txtVehiclePauseFlags.Location = new System.Drawing.Point(9, 28);
            this.txtVehiclePauseFlags.Multiline = true;
            this.txtVehiclePauseFlags.Name = "txtVehiclePauseFlags";
            this.txtVehiclePauseFlags.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtVehiclePauseFlags.Size = new System.Drawing.Size(270, 205);
            this.txtVehiclePauseFlags.TabIndex = 43;
            this.txtVehiclePauseFlags.WordWrap = false;
            // 
            // btnRefreshMoveState
            // 
            this.btnRefreshMoveState.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnRefreshMoveState.Location = new System.Drawing.Point(285, 234);
            this.btnRefreshMoveState.Name = "btnRefreshMoveState";
            this.btnRefreshMoveState.Size = new System.Drawing.Size(135, 92);
            this.btnRefreshMoveState.TabIndex = 42;
            this.btnRefreshMoveState.Text = "更新走行狀態";
            this.btnRefreshMoveState.UseVisualStyleBackColor = true;
            this.btnRefreshMoveState.Click += new System.EventHandler(this.btnRefreshMoveState_Click);
            // 
            // ucMoveMovingIndex
            // 
            this.ucMoveMovingIndex.Location = new System.Drawing.Point(285, 152);
            this.ucMoveMovingIndex.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucMoveMovingIndex.Name = "ucMoveMovingIndex";
            this.ucMoveMovingIndex.Size = new System.Drawing.Size(135, 65);
            this.ucMoveMovingIndex.TabIndex = 0;
            this.ucMoveMovingIndex.TagColor = System.Drawing.Color.Black;
            this.ucMoveMovingIndex.TagName = "MovingIndex";
            this.ucMoveMovingIndex.TagValue = "0";
            // 
            // ucMoveMoveState
            // 
            this.ucMoveMoveState.Location = new System.Drawing.Point(144, 10);
            this.ucMoveMoveState.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucMoveMoveState.Name = "ucMoveMoveState";
            this.ucMoveMoveState.Size = new System.Drawing.Size(135, 65);
            this.ucMoveMoveState.TabIndex = 0;
            this.ucMoveMoveState.TagColor = System.Drawing.Color.Black;
            this.ucMoveMoveState.TagName = "MoveState";
            this.ucMoveMoveState.TagValue = "Idle";
            // 
            // ucMovePauseStop
            // 
            this.ucMovePauseStop.Location = new System.Drawing.Point(285, 81);
            this.ucMovePauseStop.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucMovePauseStop.Name = "ucMovePauseStop";
            this.ucMovePauseStop.Size = new System.Drawing.Size(135, 65);
            this.ucMovePauseStop.TabIndex = 0;
            this.ucMovePauseStop.TagColor = System.Drawing.Color.Black;
            this.ucMovePauseStop.TagName = "Pause Stop";
            this.ucMovePauseStop.TagValue = "Off";
            // 
            // ucMoveReserveStop
            // 
            this.ucMoveReserveStop.Location = new System.Drawing.Point(285, 10);
            this.ucMoveReserveStop.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucMoveReserveStop.Name = "ucMoveReserveStop";
            this.ucMoveReserveStop.Size = new System.Drawing.Size(135, 65);
            this.ucMoveReserveStop.TabIndex = 0;
            this.ucMoveReserveStop.TagColor = System.Drawing.Color.Black;
            this.ucMoveReserveStop.TagName = "Reserve Stop";
            this.ucMoveReserveStop.TagValue = "Off";
            // 
            // ucMoveLastAddress
            // 
            this.ucMoveLastAddress.Location = new System.Drawing.Point(3, 81);
            this.ucMoveLastAddress.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucMoveLastAddress.Name = "ucMoveLastAddress";
            this.ucMoveLastAddress.Size = new System.Drawing.Size(135, 65);
            this.ucMoveLastAddress.TabIndex = 0;
            this.ucMoveLastAddress.TagColor = System.Drawing.Color.Black;
            this.ucMoveLastAddress.TagName = "Last Address";
            this.ucMoveLastAddress.TagValue = "10001";
            // 
            // ucMoveIsMoveEnd
            // 
            this.ucMoveIsMoveEnd.Location = new System.Drawing.Point(144, 81);
            this.ucMoveIsMoveEnd.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucMoveIsMoveEnd.Name = "ucMoveIsMoveEnd";
            this.ucMoveIsMoveEnd.Size = new System.Drawing.Size(135, 65);
            this.ucMoveIsMoveEnd.TabIndex = 0;
            this.ucMoveIsMoveEnd.TagColor = System.Drawing.Color.Black;
            this.ucMoveIsMoveEnd.TagName = "Is Move End";
            this.ucMoveIsMoveEnd.TagValue = "True";
            // 
            // ucMoveLastSection
            // 
            this.ucMoveLastSection.Location = new System.Drawing.Point(3, 10);
            this.ucMoveLastSection.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucMoveLastSection.Name = "ucMoveLastSection";
            this.ucMoveLastSection.Size = new System.Drawing.Size(135, 65);
            this.ucMoveLastSection.TabIndex = 0;
            this.ucMoveLastSection.TagColor = System.Drawing.Color.Black;
            this.ucMoveLastSection.TagName = "Last Section";
            this.ucMoveLastSection.TagValue = "00101";
            // 
            // ucMovePositionY
            // 
            this.ucMovePositionY.Location = new System.Drawing.Point(3, 234);
            this.ucMovePositionY.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucMovePositionY.Name = "ucMovePositionY";
            this.ucMovePositionY.Size = new System.Drawing.Size(135, 65);
            this.ucMovePositionY.TabIndex = 0;
            this.ucMovePositionY.TagColor = System.Drawing.Color.Black;
            this.ucMovePositionY.TagName = "Y";
            this.ucMovePositionY.TagValue = "-13579";
            // 
            // ucMovePositionX
            // 
            this.ucMovePositionX.Location = new System.Drawing.Point(3, 152);
            this.ucMovePositionX.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucMovePositionX.Name = "ucMovePositionX";
            this.ucMovePositionX.Size = new System.Drawing.Size(135, 65);
            this.ucMovePositionX.TabIndex = 0;
            this.ucMovePositionX.TagColor = System.Drawing.Color.Black;
            this.ucMovePositionX.TagName = "X";
            this.ucMovePositionX.TagValue = "123456";
            // 
            // pageRobotSate
            // 
            this.pageRobotSate.Controls.Add(this.btnRefreshRobotState);
            this.pageRobotSate.Controls.Add(this.ucRobotIsHome);
            this.pageRobotSate.Controls.Add(this.ucRobotSlotRId);
            this.pageRobotSate.Controls.Add(this.ucRobotSlotRState);
            this.pageRobotSate.Controls.Add(this.ucRobotSlotLState);
            this.pageRobotSate.Controls.Add(this.ucRobotSlotLId);
            this.pageRobotSate.Controls.Add(this.ucRobotRobotState);
            this.pageRobotSate.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.pageRobotSate.Location = new System.Drawing.Point(4, 29);
            this.pageRobotSate.Name = "pageRobotSate";
            this.pageRobotSate.Padding = new System.Windows.Forms.Padding(3);
            this.pageRobotSate.Size = new System.Drawing.Size(607, 676);
            this.pageRobotSate.TabIndex = 1;
            this.pageRobotSate.Text = "Robot";
            this.pageRobotSate.UseVisualStyleBackColor = true;
            // 
            // btnRefreshRobotState
            // 
            this.btnRefreshRobotState.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnRefreshRobotState.Location = new System.Drawing.Point(3, 180);
            this.btnRefreshRobotState.Name = "btnRefreshRobotState";
            this.btnRefreshRobotState.Size = new System.Drawing.Size(135, 65);
            this.btnRefreshRobotState.TabIndex = 43;
            this.btnRefreshRobotState.Text = "更新手臂狀態";
            this.btnRefreshRobotState.UseVisualStyleBackColor = true;
            this.btnRefreshRobotState.Click += new System.EventHandler(this.btnRefreshRobotState_Click);
            // 
            // ucRobotIsHome
            // 
            this.ucRobotIsHome.Location = new System.Drawing.Point(3, 88);
            this.ucRobotIsHome.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucRobotIsHome.Name = "ucRobotIsHome";
            this.ucRobotIsHome.Size = new System.Drawing.Size(135, 65);
            this.ucRobotIsHome.TabIndex = 1;
            this.ucRobotIsHome.TagColor = System.Drawing.Color.Black;
            this.ucRobotIsHome.TagName = "Is Home";
            this.ucRobotIsHome.TagValue = "false";
            // 
            // ucRobotSlotRId
            // 
            this.ucRobotSlotRId.Location = new System.Drawing.Point(298, 88);
            this.ucRobotSlotRId.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucRobotSlotRId.Name = "ucRobotSlotRId";
            this.ucRobotSlotRId.Size = new System.Drawing.Size(135, 65);
            this.ucRobotSlotRId.TabIndex = 1;
            this.ucRobotSlotRId.TagColor = System.Drawing.Color.Black;
            this.ucRobotSlotRId.TagName = "Slot R Id";
            this.ucRobotSlotRId.TagValue = "PQR";
            // 
            // ucRobotSlotRState
            // 
            this.ucRobotSlotRState.Location = new System.Drawing.Point(298, 6);
            this.ucRobotSlotRState.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucRobotSlotRState.Name = "ucRobotSlotRState";
            this.ucRobotSlotRState.Size = new System.Drawing.Size(135, 65);
            this.ucRobotSlotRState.TabIndex = 1;
            this.ucRobotSlotRState.TagColor = System.Drawing.Color.Black;
            this.ucRobotSlotRState.TagName = "Slot R State";
            this.ucRobotSlotRState.TagValue = "Empty";
            // 
            // ucRobotSlotLState
            // 
            this.ucRobotSlotLState.Location = new System.Drawing.Point(147, 6);
            this.ucRobotSlotLState.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucRobotSlotLState.Name = "ucRobotSlotLState";
            this.ucRobotSlotLState.Size = new System.Drawing.Size(135, 65);
            this.ucRobotSlotLState.TabIndex = 1;
            this.ucRobotSlotLState.TagColor = System.Drawing.Color.Black;
            this.ucRobotSlotLState.TagName = "Slot L State";
            this.ucRobotSlotLState.TagValue = "Empty";
            // 
            // ucRobotSlotLId
            // 
            this.ucRobotSlotLId.Location = new System.Drawing.Point(147, 88);
            this.ucRobotSlotLId.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucRobotSlotLId.Name = "ucRobotSlotLId";
            this.ucRobotSlotLId.Size = new System.Drawing.Size(135, 65);
            this.ucRobotSlotLId.TabIndex = 1;
            this.ucRobotSlotLId.TagColor = System.Drawing.Color.Black;
            this.ucRobotSlotLId.TagName = "Slot L Id";
            this.ucRobotSlotLId.TagValue = "ABC";
            // 
            // ucRobotRobotState
            // 
            this.ucRobotRobotState.Location = new System.Drawing.Point(6, 6);
            this.ucRobotRobotState.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucRobotRobotState.Name = "ucRobotRobotState";
            this.ucRobotRobotState.Size = new System.Drawing.Size(135, 65);
            this.ucRobotRobotState.TabIndex = 1;
            this.ucRobotRobotState.TagColor = System.Drawing.Color.Black;
            this.ucRobotRobotState.TagName = "Robot State";
            this.ucRobotRobotState.TagValue = "Idle";
            // 
            // pageBatteryState
            // 
            this.pageBatteryState.Controls.Add(this.ucAddress);
            this.pageBatteryState.Controls.Add(this.ucChargeCount);
            this.pageBatteryState.Controls.Add(this.ucCurransferStepType);
            this.pageBatteryState.Controls.Add(this.ucStepsCount);
            this.pageBatteryState.Controls.Add(this.ucIsSimulation);
            this.pageBatteryState.Controls.Add(this.ucIsCharger);
            this.pageBatteryState.Controls.Add(this.ucIsArrivalCharge);
            this.pageBatteryState.Controls.Add(this.ucIsLowPowerStartChargeTimeout);
            this.pageBatteryState.Controls.Add(this.ucIsLowPower);
            this.pageBatteryState.Controls.Add(this.ucIsOptimize);
            this.pageBatteryState.Controls.Add(this.ucIsVehicleIdle);
            this.pageBatteryState.Controls.Add(this.ucAutoState);
            this.pageBatteryState.Controls.Add(this.btnRefreshBatteryState);
            this.pageBatteryState.Controls.Add(this.ucBatteryCharging);
            this.pageBatteryState.Controls.Add(this.ucBatteryTemperature);
            this.pageBatteryState.Controls.Add(this.ucBatteryVoltage);
            this.pageBatteryState.Controls.Add(this.ucBatteryPercentage);
            this.pageBatteryState.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.pageBatteryState.Location = new System.Drawing.Point(4, 29);
            this.pageBatteryState.Name = "pageBatteryState";
            this.pageBatteryState.Size = new System.Drawing.Size(607, 676);
            this.pageBatteryState.TabIndex = 2;
            this.pageBatteryState.Text = "Battery";
            this.pageBatteryState.UseVisualStyleBackColor = true;
            // 
            // ucAddress
            // 
            this.ucAddress.Location = new System.Drawing.Point(347, 447);
            this.ucAddress.Margin = new System.Windows.Forms.Padding(8, 12, 8, 12);
            this.ucAddress.Name = "ucAddress";
            this.ucAddress.Size = new System.Drawing.Size(135, 65);
            this.ucAddress.TabIndex = 53;
            this.ucAddress.TagColor = System.Drawing.Color.Black;
            this.ucAddress.TagName = "Address";
            this.ucAddress.TagValue = " 70.0";
            // 
            // ucChargeCount
            // 
            this.ucChargeCount.Location = new System.Drawing.Point(347, 353);
            this.ucChargeCount.Margin = new System.Windows.Forms.Padding(8, 12, 8, 12);
            this.ucChargeCount.Name = "ucChargeCount";
            this.ucChargeCount.Size = new System.Drawing.Size(135, 65);
            this.ucChargeCount.TabIndex = 52;
            this.ucChargeCount.TagColor = System.Drawing.Color.Black;
            this.ucChargeCount.TagName = "ChargeCount";
            this.ucChargeCount.TagValue = " 70.0";
            // 
            // ucCurransferStepType
            // 
            this.ucCurransferStepType.Location = new System.Drawing.Point(347, 258);
            this.ucCurransferStepType.Margin = new System.Windows.Forms.Padding(5, 7, 5, 7);
            this.ucCurransferStepType.Name = "ucCurransferStepType";
            this.ucCurransferStepType.Size = new System.Drawing.Size(135, 65);
            this.ucCurransferStepType.TabIndex = 51;
            this.ucCurransferStepType.TagColor = System.Drawing.Color.Black;
            this.ucCurransferStepType.TagName = "TRStepType";
            this.ucCurransferStepType.TagValue = " 70.0";
            // 
            // ucStepsCount
            // 
            this.ucStepsCount.Location = new System.Drawing.Point(347, 171);
            this.ucStepsCount.Margin = new System.Windows.Forms.Padding(5, 7, 5, 7);
            this.ucStepsCount.Name = "ucStepsCount";
            this.ucStepsCount.Size = new System.Drawing.Size(135, 65);
            this.ucStepsCount.TabIndex = 50;
            this.ucStepsCount.TagColor = System.Drawing.Color.Black;
            this.ucStepsCount.TagName = "StepsCount";
            this.ucStepsCount.TagValue = " 70.0";
            // 
            // ucIsSimulation
            // 
            this.ucIsSimulation.Location = new System.Drawing.Point(347, 92);
            this.ucIsSimulation.Margin = new System.Windows.Forms.Padding(5, 7, 5, 7);
            this.ucIsSimulation.Name = "ucIsSimulation";
            this.ucIsSimulation.Size = new System.Drawing.Size(135, 65);
            this.ucIsSimulation.TabIndex = 49;
            this.ucIsSimulation.TagColor = System.Drawing.Color.Black;
            this.ucIsSimulation.TagName = "Simulation";
            this.ucIsSimulation.TagValue = " 70.0";
            // 
            // ucIsCharger
            // 
            this.ucIsCharger.Location = new System.Drawing.Point(347, 13);
            this.ucIsCharger.Margin = new System.Windows.Forms.Padding(5, 7, 5, 7);
            this.ucIsCharger.Name = "ucIsCharger";
            this.ucIsCharger.Size = new System.Drawing.Size(135, 65);
            this.ucIsCharger.TabIndex = 48;
            this.ucIsCharger.TagColor = System.Drawing.Color.Black;
            this.ucIsCharger.TagName = "IsCharger";
            this.ucIsCharger.TagValue = " 70.0";
            // 
            // ucIsArrivalCharge
            // 
            this.ucIsArrivalCharge.Location = new System.Drawing.Point(175, 447);
            this.ucIsArrivalCharge.Margin = new System.Windows.Forms.Padding(5, 7, 5, 7);
            this.ucIsArrivalCharge.Name = "ucIsArrivalCharge";
            this.ucIsArrivalCharge.Size = new System.Drawing.Size(135, 65);
            this.ucIsArrivalCharge.TabIndex = 47;
            this.ucIsArrivalCharge.TagColor = System.Drawing.Color.Black;
            this.ucIsArrivalCharge.TagName = "ArrivalCharge";
            this.ucIsArrivalCharge.TagValue = " 70.0";
            // 
            // ucIsLowPowerStartChargeTimeout
            // 
            this.ucIsLowPowerStartChargeTimeout.Location = new System.Drawing.Point(175, 353);
            this.ucIsLowPowerStartChargeTimeout.Margin = new System.Windows.Forms.Padding(5, 7, 5, 7);
            this.ucIsLowPowerStartChargeTimeout.Name = "ucIsLowPowerStartChargeTimeout";
            this.ucIsLowPowerStartChargeTimeout.Size = new System.Drawing.Size(135, 65);
            this.ucIsLowPowerStartChargeTimeout.TabIndex = 46;
            this.ucIsLowPowerStartChargeTimeout.TagColor = System.Drawing.Color.Black;
            this.ucIsLowPowerStartChargeTimeout.TagName = "ChargeTimeout";
            this.ucIsLowPowerStartChargeTimeout.TagValue = " 70.0";
            // 
            // ucIsLowPower
            // 
            this.ucIsLowPower.Location = new System.Drawing.Point(175, 258);
            this.ucIsLowPower.Margin = new System.Windows.Forms.Padding(5, 7, 5, 7);
            this.ucIsLowPower.Name = "ucIsLowPower";
            this.ucIsLowPower.Size = new System.Drawing.Size(135, 65);
            this.ucIsLowPower.TabIndex = 45;
            this.ucIsLowPower.TagColor = System.Drawing.Color.Black;
            this.ucIsLowPower.TagName = "IsLowPower";
            this.ucIsLowPower.TagValue = " 70.0";
            // 
            // ucIsOptimize
            // 
            this.ucIsOptimize.Location = new System.Drawing.Point(175, 171);
            this.ucIsOptimize.Margin = new System.Windows.Forms.Padding(5, 7, 5, 7);
            this.ucIsOptimize.Name = "ucIsOptimize";
            this.ucIsOptimize.Size = new System.Drawing.Size(135, 65);
            this.ucIsOptimize.TabIndex = 44;
            this.ucIsOptimize.TagColor = System.Drawing.Color.Black;
            this.ucIsOptimize.TagName = "IsOptimize";
            this.ucIsOptimize.TagValue = " 70.0";
            // 
            // ucIsVehicleIdle
            // 
            this.ucIsVehicleIdle.Location = new System.Drawing.Point(175, 92);
            this.ucIsVehicleIdle.Margin = new System.Windows.Forms.Padding(5, 7, 5, 7);
            this.ucIsVehicleIdle.Name = "ucIsVehicleIdle";
            this.ucIsVehicleIdle.Size = new System.Drawing.Size(135, 65);
            this.ucIsVehicleIdle.TabIndex = 43;
            this.ucIsVehicleIdle.TagColor = System.Drawing.Color.Black;
            this.ucIsVehicleIdle.TagName = "IsVehicleIdle";
            this.ucIsVehicleIdle.TagValue = " 70.0";
            // 
            // ucAutoState
            // 
            this.ucAutoState.Location = new System.Drawing.Point(175, 13);
            this.ucAutoState.Margin = new System.Windows.Forms.Padding(5, 7, 5, 7);
            this.ucAutoState.Name = "ucAutoState";
            this.ucAutoState.Size = new System.Drawing.Size(135, 65);
            this.ucAutoState.TabIndex = 42;
            this.ucAutoState.TagColor = System.Drawing.Color.Black;
            this.ucAutoState.TagName = "AutoState";
            this.ucAutoState.TagValue = " 70.0";
            // 
            // btnRefreshBatteryState
            // 
            this.btnRefreshBatteryState.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnRefreshBatteryState.Location = new System.Drawing.Point(3, 353);
            this.btnRefreshBatteryState.Name = "btnRefreshBatteryState";
            this.btnRefreshBatteryState.Size = new System.Drawing.Size(135, 65);
            this.btnRefreshBatteryState.TabIndex = 41;
            this.btnRefreshBatteryState.Text = "更新電池狀態";
            this.btnRefreshBatteryState.UseVisualStyleBackColor = true;
            this.btnRefreshBatteryState.Click += new System.EventHandler(this.AseRobotControlForm_RefreshBatteryState);
            // 
            // ucBatteryCharging
            // 
            this.ucBatteryCharging.Location = new System.Drawing.Point(3, 258);
            this.ucBatteryCharging.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucBatteryCharging.Name = "ucBatteryCharging";
            this.ucBatteryCharging.Size = new System.Drawing.Size(135, 65);
            this.ucBatteryCharging.TabIndex = 5;
            this.ucBatteryCharging.TagColor = System.Drawing.Color.Black;
            this.ucBatteryCharging.TagName = "Charging";
            this.ucBatteryCharging.TagValue = "false";
            // 
            // ucBatteryTemperature
            // 
            this.ucBatteryTemperature.Location = new System.Drawing.Point(3, 169);
            this.ucBatteryTemperature.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucBatteryTemperature.Name = "ucBatteryTemperature";
            this.ucBatteryTemperature.Size = new System.Drawing.Size(135, 65);
            this.ucBatteryTemperature.TabIndex = 6;
            this.ucBatteryTemperature.TagColor = System.Drawing.Color.Black;
            this.ucBatteryTemperature.TagName = "Temperature";
            this.ucBatteryTemperature.TagValue = "40.5";
            // 
            // ucBatteryVoltage
            // 
            this.ucBatteryVoltage.Location = new System.Drawing.Point(3, 84);
            this.ucBatteryVoltage.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucBatteryVoltage.Name = "ucBatteryVoltage";
            this.ucBatteryVoltage.Size = new System.Drawing.Size(135, 65);
            this.ucBatteryVoltage.TabIndex = 7;
            this.ucBatteryVoltage.TagColor = System.Drawing.Color.Black;
            this.ucBatteryVoltage.TagName = "Voltage";
            this.ucBatteryVoltage.TagValue = "55.66";
            // 
            // ucBatteryPercentage
            // 
            this.ucBatteryPercentage.Location = new System.Drawing.Point(3, 13);
            this.ucBatteryPercentage.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucBatteryPercentage.Name = "ucBatteryPercentage";
            this.ucBatteryPercentage.Size = new System.Drawing.Size(135, 65);
            this.ucBatteryPercentage.TabIndex = 3;
            this.ucBatteryPercentage.TagColor = System.Drawing.Color.Black;
            this.ucBatteryPercentage.TagName = "Percentage";
            this.ucBatteryPercentage.TagValue = " 70.0";
            // 
            // pageVehicleState
            // 
            this.pageVehicleState.Controls.Add(this.labException);
            this.pageVehicleState.Controls.Add(this.txtVisitTransferCount);
            this.pageVehicleState.Controls.Add(this.lab1);
            this.pageVehicleState.Controls.Add(this.groupBox3);
            this.pageVehicleState.Controls.Add(this.ucIsCharging);
            this.pageVehicleState.Controls.Add(this.ucIsHome);
            this.pageVehicleState.Controls.Add(this.ucIsSleepByAskReserveFail);
            this.pageVehicleState.Controls.Add(this.ucIsMoveEnd);
            this.pageVehicleState.Controls.Add(this.ucIsMoveStep);
            this.pageVehicleState.Controls.Add(this.ucIsAskReservePause);
            this.pageVehicleState.Controls.Add(this.ucTransferCommandTransferStep);
            this.pageVehicleState.Controls.Add(this.ucTransferCommandEnrouteState);
            this.pageVehicleState.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.pageVehicleState.Location = new System.Drawing.Point(4, 29);
            this.pageVehicleState.Name = "pageVehicleState";
            this.pageVehicleState.Size = new System.Drawing.Size(607, 676);
            this.pageVehicleState.TabIndex = 3;
            this.pageVehicleState.Text = "Vehicle";
            this.pageVehicleState.UseVisualStyleBackColor = true;
            // 
            // labException
            // 
            this.labException.AutoSize = true;
            this.labException.Location = new System.Drawing.Point(98, 587);
            this.labException.Name = "labException";
            this.labException.Size = new System.Drawing.Size(83, 20);
            this.labException.TabIndex = 73;
            this.labException.Text = "Exception";
            // 
            // txtVisitTransferCount
            // 
            this.txtVisitTransferCount.AutoSize = true;
            this.txtVisitTransferCount.Location = new System.Drawing.Point(506, 645);
            this.txtVisitTransferCount.Name = "txtVisitTransferCount";
            this.txtVisitTransferCount.Size = new System.Drawing.Size(88, 20);
            this.txtVisitTransferCount.TabIndex = 72;
            this.txtVisitTransferCount.Text = "VisitCount";
            // 
            // lab1
            // 
            this.lab1.AutoSize = true;
            this.lab1.Location = new System.Drawing.Point(9, 587);
            this.lab1.Name = "lab1";
            this.lab1.Size = new System.Drawing.Size(83, 20);
            this.lab1.TabIndex = 72;
            this.lab1.Text = "Exception";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.txtBatterysAbnormal);
            this.groupBox3.Controls.Add(this.txtMainFlowAbnormal);
            this.groupBox3.Controls.Add(this.txtAgvcConnectorAbnormal);
            this.groupBox3.Controls.Add(this.txtRobotAbnormal);
            this.groupBox3.Controls.Add(this.txtMoveControlAbnormal);
            this.groupBox3.Location = new System.Drawing.Point(3, 3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(140, 181);
            this.groupBox3.TabIndex = 62;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "IsAbnormal";
            // 
            // txtBatterysAbnormal
            // 
            this.txtBatterysAbnormal.BackColor = System.Drawing.Color.LightGreen;
            this.txtBatterysAbnormal.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtBatterysAbnormal.Location = new System.Drawing.Point(6, 142);
            this.txtBatterysAbnormal.Name = "txtBatterysAbnormal";
            this.txtBatterysAbnormal.Size = new System.Drawing.Size(126, 31);
            this.txtBatterysAbnormal.TabIndex = 4;
            this.txtBatterysAbnormal.Text = "電池";
            this.txtBatterysAbnormal.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtMainFlowAbnormal
            // 
            this.txtMainFlowAbnormal.BackColor = System.Drawing.Color.LightGreen;
            this.txtMainFlowAbnormal.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtMainFlowAbnormal.Location = new System.Drawing.Point(6, 18);
            this.txtMainFlowAbnormal.Name = "txtMainFlowAbnormal";
            this.txtMainFlowAbnormal.Size = new System.Drawing.Size(126, 31);
            this.txtMainFlowAbnormal.TabIndex = 1;
            this.txtMainFlowAbnormal.Text = "流程";
            this.txtMainFlowAbnormal.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtAgvcConnectorAbnormal
            // 
            this.txtAgvcConnectorAbnormal.BackColor = System.Drawing.Color.LightGreen;
            this.txtAgvcConnectorAbnormal.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtAgvcConnectorAbnormal.Location = new System.Drawing.Point(6, 49);
            this.txtAgvcConnectorAbnormal.Name = "txtAgvcConnectorAbnormal";
            this.txtAgvcConnectorAbnormal.Size = new System.Drawing.Size(126, 31);
            this.txtAgvcConnectorAbnormal.TabIndex = 3;
            this.txtAgvcConnectorAbnormal.Text = "通訊";
            this.txtAgvcConnectorAbnormal.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtRobotAbnormal
            // 
            this.txtRobotAbnormal.BackColor = System.Drawing.Color.LightGreen;
            this.txtRobotAbnormal.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtRobotAbnormal.Location = new System.Drawing.Point(6, 111);
            this.txtRobotAbnormal.Name = "txtRobotAbnormal";
            this.txtRobotAbnormal.Size = new System.Drawing.Size(126, 31);
            this.txtRobotAbnormal.TabIndex = 2;
            this.txtRobotAbnormal.Text = "手臂";
            this.txtRobotAbnormal.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtMoveControlAbnormal
            // 
            this.txtMoveControlAbnormal.BackColor = System.Drawing.Color.LightGreen;
            this.txtMoveControlAbnormal.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtMoveControlAbnormal.Location = new System.Drawing.Point(6, 80);
            this.txtMoveControlAbnormal.Name = "txtMoveControlAbnormal";
            this.txtMoveControlAbnormal.Size = new System.Drawing.Size(126, 31);
            this.txtMoveControlAbnormal.TabIndex = 0;
            this.txtMoveControlAbnormal.Text = "走行";
            this.txtMoveControlAbnormal.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ucIsCharging
            // 
            this.ucIsCharging.Location = new System.Drawing.Point(303, 413);
            this.ucIsCharging.Margin = new System.Windows.Forms.Padding(8, 12, 8, 12);
            this.ucIsCharging.Name = "ucIsCharging";
            this.ucIsCharging.Size = new System.Drawing.Size(135, 65);
            this.ucIsCharging.TabIndex = 69;
            this.ucIsCharging.TagColor = System.Drawing.Color.Black;
            this.ucIsCharging.TagName = "IsCharging";
            this.ucIsCharging.TagValue = " 70.0";
            // 
            // ucIsHome
            // 
            this.ucIsHome.Location = new System.Drawing.Point(303, 324);
            this.ucIsHome.Margin = new System.Windows.Forms.Padding(8, 12, 8, 12);
            this.ucIsHome.Name = "ucIsHome";
            this.ucIsHome.Size = new System.Drawing.Size(135, 65);
            this.ucIsHome.TabIndex = 68;
            this.ucIsHome.TagColor = System.Drawing.Color.Black;
            this.ucIsHome.TagName = "IsHome";
            this.ucIsHome.TagValue = " 70.0";
            // 
            // ucIsSleepByAskReserveFail
            // 
            this.ucIsSleepByAskReserveFail.Location = new System.Drawing.Point(303, 162);
            this.ucIsSleepByAskReserveFail.Margin = new System.Windows.Forms.Padding(8, 12, 8, 12);
            this.ucIsSleepByAskReserveFail.Name = "ucIsSleepByAskReserveFail";
            this.ucIsSleepByAskReserveFail.Size = new System.Drawing.Size(135, 65);
            this.ucIsSleepByAskReserveFail.TabIndex = 67;
            this.ucIsSleepByAskReserveFail.TagColor = System.Drawing.Color.Black;
            this.ucIsSleepByAskReserveFail.TagName = "IsSleepByAskReserveFail";
            this.ucIsSleepByAskReserveFail.TagValue = " 70.0";
            // 
            // ucIsMoveEnd
            // 
            this.ucIsMoveEnd.Location = new System.Drawing.Point(303, 235);
            this.ucIsMoveEnd.Margin = new System.Windows.Forms.Padding(8, 12, 8, 12);
            this.ucIsMoveEnd.Name = "ucIsMoveEnd";
            this.ucIsMoveEnd.Size = new System.Drawing.Size(135, 65);
            this.ucIsMoveEnd.TabIndex = 66;
            this.ucIsMoveEnd.TagColor = System.Drawing.Color.Black;
            this.ucIsMoveEnd.TagName = "IsMoveEnd";
            this.ucIsMoveEnd.TagValue = " 70.0";
            // 
            // ucIsMoveStep
            // 
            this.ucIsMoveStep.Location = new System.Drawing.Point(303, 83);
            this.ucIsMoveStep.Margin = new System.Windows.Forms.Padding(8, 12, 8, 12);
            this.ucIsMoveStep.Name = "ucIsMoveStep";
            this.ucIsMoveStep.Size = new System.Drawing.Size(135, 65);
            this.ucIsMoveStep.TabIndex = 64;
            this.ucIsMoveStep.TagColor = System.Drawing.Color.Black;
            this.ucIsMoveStep.TagName = "IsMoveStep";
            this.ucIsMoveStep.TagValue = " 70.0";
            // 
            // ucIsAskReservePause
            // 
            this.ucIsAskReservePause.Location = new System.Drawing.Point(303, 10);
            this.ucIsAskReservePause.Margin = new System.Windows.Forms.Padding(8, 12, 8, 12);
            this.ucIsAskReservePause.Name = "ucIsAskReservePause";
            this.ucIsAskReservePause.Size = new System.Drawing.Size(135, 65);
            this.ucIsAskReservePause.TabIndex = 63;
            this.ucIsAskReservePause.TagColor = System.Drawing.Color.Black;
            this.ucIsAskReservePause.TagName = "IsAskReservePause";
            this.ucIsAskReservePause.TagValue = " 70.0";
            // 
            // ucTransferCommandTransferStep
            // 
            this.ucTransferCommandTransferStep.Location = new System.Drawing.Point(148, 10);
            this.ucTransferCommandTransferStep.Name = "ucTransferCommandTransferStep";
            this.ucTransferCommandTransferStep.Size = new System.Drawing.Size(135, 65);
            this.ucTransferCommandTransferStep.TabIndex = 1;
            this.ucTransferCommandTransferStep.TagColor = System.Drawing.Color.Black;
            this.ucTransferCommandTransferStep.TagName = "TransferStep";
            this.ucTransferCommandTransferStep.TagValue = "None";
            // 
            // ucTransferCommandEnrouteState
            // 
            this.ucTransferCommandEnrouteState.Location = new System.Drawing.Point(148, 83);
            this.ucTransferCommandEnrouteState.Name = "ucTransferCommandEnrouteState";
            this.ucTransferCommandEnrouteState.Size = new System.Drawing.Size(135, 65);
            this.ucTransferCommandEnrouteState.TabIndex = 1;
            this.ucTransferCommandEnrouteState.TagColor = System.Drawing.Color.Black;
            this.ucTransferCommandEnrouteState.TagName = "EnrouteState";
            this.ucTransferCommandEnrouteState.TagValue = "-1";
            // 
            // pageReserveInfo
            // 
            this.pageReserveInfo.Controls.Add(this.gbReserve);
            this.pageReserveInfo.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.pageReserveInfo.Location = new System.Drawing.Point(4, 29);
            this.pageReserveInfo.Name = "pageReserveInfo";
            this.pageReserveInfo.Padding = new System.Windows.Forms.Padding(3);
            this.pageReserveInfo.Size = new System.Drawing.Size(607, 676);
            this.pageReserveInfo.TabIndex = 4;
            this.pageReserveInfo.Text = "Reserve";
            this.pageReserveInfo.UseVisualStyleBackColor = true;
            // 
            // gbReserve
            // 
            this.gbReserve.Controls.Add(this.lbxReserveOkSections);
            this.gbReserve.Controls.Add(this.lbxNeedReserveSections);
            this.gbReserve.Location = new System.Drawing.Point(6, 9);
            this.gbReserve.Name = "gbReserve";
            this.gbReserve.Size = new System.Drawing.Size(595, 661);
            this.gbReserve.TabIndex = 49;
            this.gbReserve.TabStop = false;
            this.gbReserve.Text = "Reserve";
            // 
            // lbxReserveOkSections
            // 
            this.lbxReserveOkSections.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbxReserveOkSections.FormattingEnabled = true;
            this.lbxReserveOkSections.ItemHeight = 19;
            this.lbxReserveOkSections.Location = new System.Drawing.Point(238, 21);
            this.lbxReserveOkSections.Name = "lbxReserveOkSections";
            this.lbxReserveOkSections.ScrollAlwaysVisible = true;
            this.lbxReserveOkSections.Size = new System.Drawing.Size(235, 631);
            this.lbxReserveOkSections.TabIndex = 42;
            // 
            // lbxNeedReserveSections
            // 
            this.lbxNeedReserveSections.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbxNeedReserveSections.FormattingEnabled = true;
            this.lbxNeedReserveSections.ItemHeight = 19;
            this.lbxNeedReserveSections.Location = new System.Drawing.Point(6, 21);
            this.lbxNeedReserveSections.Name = "lbxNeedReserveSections";
            this.lbxNeedReserveSections.ScrollAlwaysVisible = true;
            this.lbxNeedReserveSections.Size = new System.Drawing.Size(215, 631);
            this.lbxNeedReserveSections.TabIndex = 41;
            // 
            // pageTransferCommand
            // 
            this.pageTransferCommand.Controls.Add(this.groupBox4);
            this.pageTransferCommand.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.pageTransferCommand.Location = new System.Drawing.Point(4, 29);
            this.pageTransferCommand.Name = "pageTransferCommand";
            this.pageTransferCommand.Size = new System.Drawing.Size(607, 676);
            this.pageTransferCommand.TabIndex = 5;
            this.pageTransferCommand.Text = "TransferCmd";
            this.pageTransferCommand.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.txtTransferCommand03);
            this.groupBox4.Controls.Add(this.txtTransferCommand04);
            this.groupBox4.Controls.Add(this.txtTransferCommand02);
            this.groupBox4.Controls.Add(this.txtTransferCommand01);
            this.groupBox4.Location = new System.Drawing.Point(3, 3);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(603, 676);
            this.groupBox4.TabIndex = 60;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Transfer Command";
            // 
            // txtTransferCommand03
            // 
            this.txtTransferCommand03.Font = new System.Drawing.Font("Times New Roman", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTransferCommand03.Location = new System.Drawing.Point(6, 349);
            this.txtTransferCommand03.Multiline = true;
            this.txtTransferCommand03.Name = "txtTransferCommand03";
            this.txtTransferCommand03.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtTransferCommand03.Size = new System.Drawing.Size(290, 320);
            this.txtTransferCommand03.TabIndex = 60;
            // 
            // txtTransferCommand04
            // 
            this.txtTransferCommand04.Font = new System.Drawing.Font("Times New Roman", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTransferCommand04.Location = new System.Drawing.Point(302, 349);
            this.txtTransferCommand04.Multiline = true;
            this.txtTransferCommand04.Name = "txtTransferCommand04";
            this.txtTransferCommand04.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtTransferCommand04.Size = new System.Drawing.Size(290, 320);
            this.txtTransferCommand04.TabIndex = 59;
            // 
            // txtTransferCommand02
            // 
            this.txtTransferCommand02.Font = new System.Drawing.Font("Times New Roman", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTransferCommand02.Location = new System.Drawing.Point(302, 23);
            this.txtTransferCommand02.Multiline = true;
            this.txtTransferCommand02.Name = "txtTransferCommand02";
            this.txtTransferCommand02.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtTransferCommand02.Size = new System.Drawing.Size(290, 320);
            this.txtTransferCommand02.TabIndex = 59;
            // 
            // txtTransferCommand01
            // 
            this.txtTransferCommand01.Font = new System.Drawing.Font("Times New Roman", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTransferCommand01.Location = new System.Drawing.Point(6, 23);
            this.txtTransferCommand01.Multiline = true;
            this.txtTransferCommand01.Name = "txtTransferCommand01";
            this.txtTransferCommand01.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtTransferCommand01.Size = new System.Drawing.Size(290, 320);
            this.txtTransferCommand01.TabIndex = 59;
            // 
            // pageSimulator
            // 
            this.pageSimulator.Controls.Add(this.groupBox6);
            this.pageSimulator.Controls.Add(this.gbVehicleLocation);
            this.pageSimulator.Controls.Add(this.groupBox5);
            this.pageSimulator.Location = new System.Drawing.Point(4, 29);
            this.pageSimulator.Name = "pageSimulator";
            this.pageSimulator.Size = new System.Drawing.Size(607, 676);
            this.pageSimulator.TabIndex = 7;
            this.pageSimulator.Text = "Simulator";
            this.pageSimulator.UseVisualStyleBackColor = true;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.btnKeyInTestAlarm);
            this.groupBox6.Controls.Add(this.numTestErrorCode);
            this.groupBox6.Location = new System.Drawing.Point(3, 235);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(209, 110);
            this.groupBox6.TabIndex = 3;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Alarm";
            // 
            // btnKeyInTestAlarm
            // 
            this.btnKeyInTestAlarm.Location = new System.Drawing.Point(6, 63);
            this.btnKeyInTestAlarm.Name = "btnKeyInTestAlarm";
            this.btnKeyInTestAlarm.Size = new System.Drawing.Size(192, 27);
            this.btnKeyInTestAlarm.TabIndex = 12;
            this.btnKeyInTestAlarm.Text = "Alarm Test";
            this.btnKeyInTestAlarm.UseVisualStyleBackColor = true;
            this.btnKeyInTestAlarm.Click += new System.EventHandler(this.btnKeyInTestAlarm_Click);
            // 
            // numTestErrorCode
            // 
            this.numTestErrorCode.Location = new System.Drawing.Point(6, 28);
            this.numTestErrorCode.Maximum = new decimal(new int[] {
            300000,
            0,
            0,
            0});
            this.numTestErrorCode.Name = "numTestErrorCode";
            this.numTestErrorCode.Size = new System.Drawing.Size(191, 29);
            this.numTestErrorCode.TabIndex = 11;
            // 
            // gbVehicleLocation
            // 
            this.gbVehicleLocation.Controls.Add(this.numPositionY);
            this.gbVehicleLocation.Controls.Add(this.numPositionX);
            this.gbVehicleLocation.Controls.Add(this.btnKeyInPosition);
            this.gbVehicleLocation.Location = new System.Drawing.Point(3, 3);
            this.gbVehicleLocation.Name = "gbVehicleLocation";
            this.gbVehicleLocation.Size = new System.Drawing.Size(209, 93);
            this.gbVehicleLocation.TabIndex = 2;
            this.gbVehicleLocation.TabStop = false;
            this.gbVehicleLocation.Text = "Vehicle Location";
            // 
            // numPositionY
            // 
            this.numPositionY.Location = new System.Drawing.Point(104, 19);
            this.numPositionY.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.numPositionY.Minimum = new decimal(new int[] {
            99999,
            0,
            0,
            -2147483648});
            this.numPositionY.Name = "numPositionY";
            this.numPositionY.Size = new System.Drawing.Size(94, 29);
            this.numPositionY.TabIndex = 41;
            // 
            // numPositionX
            // 
            this.numPositionX.Location = new System.Drawing.Point(6, 19);
            this.numPositionX.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.numPositionX.Minimum = new decimal(new int[] {
            999999,
            0,
            0,
            -2147483648});
            this.numPositionX.Name = "numPositionX";
            this.numPositionX.Size = new System.Drawing.Size(92, 29);
            this.numPositionX.TabIndex = 41;
            // 
            // btnKeyInPosition
            // 
            this.btnKeyInPosition.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnKeyInPosition.Location = new System.Drawing.Point(6, 47);
            this.btnKeyInPosition.Name = "btnKeyInPosition";
            this.btnKeyInPosition.Size = new System.Drawing.Size(192, 27);
            this.btnKeyInPosition.TabIndex = 40;
            this.btnKeyInPosition.Text = "鍵入車輛位置";
            this.btnKeyInPosition.UseVisualStyleBackColor = true;
            this.btnKeyInPosition.Click += new System.EventHandler(this.btnKeyInPosition_Click);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.btnKeyInSoc);
            this.groupBox5.Controls.Add(this.numSoc);
            this.groupBox5.Location = new System.Drawing.Point(3, 102);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(209, 127);
            this.groupBox5.TabIndex = 0;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Battery Percentage";
            // 
            // btnKeyInSoc
            // 
            this.btnKeyInSoc.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnKeyInSoc.Location = new System.Drawing.Point(6, 61);
            this.btnKeyInSoc.Name = "btnKeyInSoc";
            this.btnKeyInSoc.Size = new System.Drawing.Size(191, 28);
            this.btnKeyInSoc.TabIndex = 42;
            this.btnKeyInSoc.Text = "校正電量";
            this.btnKeyInSoc.UseVisualStyleBackColor = true;
            this.btnKeyInSoc.Click += new System.EventHandler(this.btnKeyInSoc_Click);
            // 
            // numSoc
            // 
            this.numSoc.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.numSoc.Location = new System.Drawing.Point(6, 28);
            this.numSoc.Name = "numSoc";
            this.numSoc.Size = new System.Drawing.Size(191, 26);
            this.numSoc.TabIndex = 43;
            this.numSoc.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numSoc.Value = new decimal(new int[] {
            70,
            0,
            0,
            0});
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = global::Mirle.Agv.AseMiddler.Properties.Resources.Mirle_Logo2;
            this.pictureBox2.Location = new System.Drawing.Point(437, 3);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(165, 133);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox2.TabIndex = 66;
            this.pictureBox2.TabStop = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(1375, 761);
            this.ControlBox = false;
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MainForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            this.splitContainer3.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.pageBasicState.ResumeLayout(false);
            this.pageBasicState.PerformLayout();
            this.groupHighLevelManualControl.ResumeLayout(false);
            this.groupHighLevelManualControl.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.gbPerformanceCounter.ResumeLayout(false);
            this.gbConnection.ResumeLayout(false);
            this.gbConnection.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.pageMoveState.ResumeLayout(false);
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            this.pageRobotSate.ResumeLayout(false);
            this.pageBatteryState.ResumeLayout(false);
            this.pageVehicleState.ResumeLayout(false);
            this.pageVehicleState.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.pageReserveInfo.ResumeLayout(false);
            this.gbReserve.ResumeLayout(false);
            this.pageTransferCommand.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.pageSimulator.ResumeLayout(false);
            this.groupBox6.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numTestErrorCode)).EndInit();
            this.gbVehicleLocation.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numPositionY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPositionX)).EndInit();
            this.groupBox5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numSoc)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemSystem;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemLanguage;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemMode;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemLogin;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemLogout;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemCloseApp;
        private System.Windows.Forms.ToolStripMenuItem 中文ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem englishToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem AlarmPage;
        private System.Windows.Forms.ToolStripMenuItem AgvcConnectorPage;
        private System.Windows.Forms.ToolStripMenuItem VehicleConfigPage;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.GroupBox gbConnection;
        private System.Windows.Forms.RadioButton radAgvcOnline;
        private System.Windows.Forms.RadioButton radAgvcOffline;
        private System.Windows.Forms.Button btnAlarmReset;
        private System.Windows.Forms.ListBox lbxReserveOkSections;
        private System.Windows.Forms.ListBox lbxNeedReserveSections;
        private System.Windows.Forms.Timer timeUpdateUI;
        private System.Windows.Forms.GroupBox gbPerformanceCounter;
        private System.Windows.Forms.Label txtLastAlarm;
        private UcLabelTextBox ucSoc;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button btnAutoManual;
        private System.Windows.Forms.GroupBox gbReserve;
        private UcLabelTextBox ucCharging;
        private System.Windows.Forms.Label txtAgvcConnection;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar tspbCommding;
        private System.Windows.Forms.ToolStripStatusLabel tstextClientName;
        private System.Windows.Forms.ToolStripStatusLabel tstextRemoteIp;
        private System.Windows.Forms.ToolStripStatusLabel tstextRemotePort;
        private System.Windows.Forms.ToolStripStatusLabel tstextLastPosX;
        private System.Windows.Forms.ToolStripStatusLabel tstextLastPosY;
        private UcLabelTextBox ucLCstId;
        private System.Windows.Forms.Timer timer_SetupInitialSoc;
        private System.Windows.Forms.Label txtCannotAutoReason;
        private System.Windows.Forms.TextBox tbxDebugLogMsg;
        private System.Windows.Forms.TextBox txtTransferCommand01;
        private System.Windows.Forms.TextBox txtTransferCommand03;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label txtBatterysAbnormal;
        private System.Windows.Forms.Label txtMainFlowAbnormal;
        private System.Windows.Forms.Label txtAgvcConnectorAbnormal;
        private System.Windows.Forms.Label txtRobotAbnormal;
        private System.Windows.Forms.Label txtMoveControlAbnormal;
        private System.Windows.Forms.Button btnPrintScreen;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label txtAgvlConnection;
        private System.Windows.Forms.RadioButton radAgvlOnline;
        private System.Windows.Forms.RadioButton radAgvlOffline;
        private System.Windows.Forms.Button btnRefreshStatus;
        private System.Windows.Forms.ToolStripMenuItem AgvlConnectorPage;
        private UcLabelTextBox ucRCstId;
        private UcLabelTextBox ucRobotHome;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage pageMoveState;
        private System.Windows.Forms.TabPage pageRobotSate;
        private System.Windows.Forms.TabPage pageBatteryState;
        private System.Windows.Forms.TabPage pageVehicleState;
        private UcVerticalLabelText ucBatteryCharging;
        private UcVerticalLabelText ucBatteryTemperature;
        private UcVerticalLabelText ucBatteryVoltage;
        private UcVerticalLabelText ucBatteryPercentage;
        private System.Windows.Forms.Button btnRefreshBatteryState;
        private UcVerticalLabelText ucMoveLastAddress;
        private UcVerticalLabelText ucMoveLastSection;
        private UcVerticalLabelText ucMovePositionY;
        private UcVerticalLabelText ucMovePositionX;
        private System.Windows.Forms.Button btnRefreshMoveState;
        private UcVerticalLabelText ucMoveIsMoveEnd;
        private UcVerticalLabelText ucMoveReserveStop;
        private UcVerticalLabelText ucMoveMovingIndex;
        private UcVerticalLabelText ucMovePauseStop;
        private UcVerticalLabelText ucRobotIsHome;
        private UcVerticalLabelText ucRobotRobotState;
        private UcVerticalLabelText ucRobotSlotRId;
        private UcVerticalLabelText ucRobotSlotLId;
        private UcVerticalLabelText ucRobotSlotRState;
        private UcVerticalLabelText ucRobotSlotLState;
        private System.Windows.Forms.Button btnRefreshRobotState;
        private UcVerticalLabelText ucTransferCommandTransferStep;
        private UcVerticalLabelText ucTransferCommandEnrouteState;
        private UcVerticalLabelText ucMoveMoveState;
        private System.Windows.Forms.TabPage pageBasicState;
        private System.Windows.Forms.TabPage pageReserveInfo;
        private System.Windows.Forms.TabPage pageTransferCommand;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TextBox txtTransferCommand02;
        private UcLabelTextBox ucCurrentTransferStepType;
        private UcLabelTextBox ucErrorFlag;
        private UcLabelTextBox ucCommanding;
        private UcLabelTextBox ucReserveFlag;
        private System.Windows.Forms.Label label1;
        private UcLabelTextBox ucHeadAngle;
        private UcLabelTextBox ucMapAddressId;
        private System.Windows.Forms.CheckBox checkBoxDisableRightSlot;
        private System.Windows.Forms.CheckBox checkBoxDisableLeftSlot;
        private System.Windows.Forms.Button btnStopCharge;
        private System.Windows.Forms.Button btnCharge;
        private System.Windows.Forms.TabPage pageSimulator;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.GroupBox gbVehicleLocation;
        private System.Windows.Forms.NumericUpDown numPositionY;
        private System.Windows.Forms.NumericUpDown numPositionX;
        private System.Windows.Forms.Button btnKeyInPosition;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Button btnKeyInSoc;
        private System.Windows.Forms.NumericUpDown numSoc;
        private System.Windows.Forms.Button btnKeyInTestAlarm;
        private System.Windows.Forms.NumericUpDown numTestErrorCode;
        private System.Windows.Forms.GroupBox groupHighLevelManualControl;
        private System.Windows.Forms.TextBox txtDisableChargerAddressId;
        private System.Windows.Forms.CheckBox checkEnableToCharge;
        private UcLabelTextBox ucOpPauseFlag;
        private UcLabelTextBox ucPauseFlag;
        private UcLabelTextBox ucCommandCount;
        private UcLabelTextBox ucWifiSignalStrength;
        private System.Windows.Forms.TextBox txtVehiclePauseFlags;
        private System.Windows.Forms.GroupBox groupBox7;
        private UcVerticalLabelText ucCurransferStepType;
        private UcVerticalLabelText ucStepsCount;
        private UcVerticalLabelText ucIsSimulation;
        private UcVerticalLabelText ucIsCharger;
        private UcVerticalLabelText ucIsArrivalCharge;
        private UcVerticalLabelText ucIsLowPowerStartChargeTimeout;
        private UcVerticalLabelText ucIsLowPower;
        private UcVerticalLabelText ucIsOptimize;
        private UcVerticalLabelText ucIsVehicleIdle;
        private UcVerticalLabelText ucAutoState;
        private UcVerticalLabelText ucChargeCount;
        private UcVerticalLabelText ucIsCharging;
        private UcVerticalLabelText ucIsHome;
        private UcVerticalLabelText ucIsSleepByAskReserveFail;
        private UcVerticalLabelText ucIsMoveEnd;
        private UcVerticalLabelText ucIsMoveStep;
        private UcVerticalLabelText ucIsAskReservePause;
        private System.Windows.Forms.Label lab1;
        private System.Windows.Forms.Label labException;
        private System.Windows.Forms.Label txtVisitTransferCount;
        private UcVerticalLabelText ucAddress;
        private System.Windows.Forms.TextBox txtTransferCommand04;
        private System.Windows.Forms.CheckBox cbIsIgnoreAppendDebug;
        private System.Windows.Forms.PictureBox pictureBox2;
    }
}