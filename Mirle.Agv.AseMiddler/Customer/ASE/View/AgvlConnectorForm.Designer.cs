namespace Mirle.Agv.AseMiddler.View
{
    partial class AgvlConnectorForm
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
            this.btnHide = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.InfoPage = new System.Windows.Forms.TabPage();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.ucLinkTestIntervalMs = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucT6Timeout = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucT3Timeout = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucIsServer = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucPort = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucIp = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.btnLoadLocalConfig = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.txtConnection = new System.Windows.Forms.Label();
            this.radioOnline = new System.Windows.Forms.RadioButton();
            this.radioOffline = new System.Windows.Forms.RadioButton();
            this.SingleCommandPage = new System.Windows.Forms.TabPage();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.btnSingleMessageSend = new System.Windows.Forms.Button();
            this.btnSaveAutoReplyMessage = new System.Windows.Forms.Button();
            this.txtPsMessageText = new System.Windows.Forms.TextBox();
            this.numPsMessageNumber = new System.Windows.Forms.NumericUpDown();
            this.boxPsMessageType = new System.Windows.Forms.ComboBox();
            this.boxPspMessageMap = new System.Windows.Forms.ComboBox();
            this.MovePage = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.ucMoveLastSection = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucMoveMoveState = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucMovePositionX = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.btnRefreshPosition = new System.Windows.Forms.Button();
            this.ucMoveLastAddress = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucMovePositionY = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucAskPositionInterval = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucMoveIsMoveEnd = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.boxKeepOrGo = new System.Windows.Forms.ComboBox();
            this.btnSearchMapAddress = new System.Windows.Forms.Button();
            this.txtMapAddress = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.boxIsEnd = new System.Windows.Forms.ComboBox();
            this.boxSlotOpen = new System.Windows.Forms.ComboBox();
            this.btnSendMove = new System.Windows.Forms.Button();
            this.numMoveSpeed = new System.Windows.Forms.NumericUpDown();
            this.numHeadAngle = new System.Windows.Forms.NumericUpDown();
            this.numMovePositionY = new System.Windows.Forms.NumericUpDown();
            this.numMovePositionX = new System.Windows.Forms.NumericUpDown();
            this.RobotPage = new System.Windows.Forms.TabPage();
            this.groupBox8 = new System.Windows.Forms.GroupBox();
            this.ucRobotRobotState = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.btnRefreshRobotState = new System.Windows.Forms.Button();
            this.ucRobotSlotLId = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucRobotIsHome = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucRobotSlotLState = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucRobotSlotRId = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucRobotSlotRState = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.numPortNumber2 = new System.Windows.Forms.NumericUpDown();
            this.numPortNumber1 = new System.Windows.Forms.NumericUpDown();
            this.txtAddressId2 = new System.Windows.Forms.TextBox();
            this.numGateType = new System.Windows.Forms.NumericUpDown();
            this.btnSendRobot = new System.Windows.Forms.Button();
            this.boxLDUD2 = new System.Windows.Forms.ComboBox();
            this.boxSlotSelect2 = new System.Windows.Forms.ComboBox();
            this.boxSlotSelect1 = new System.Windows.Forms.ComboBox();
            this.boxLDUD1 = new System.Windows.Forms.ComboBox();
            this.boxPioDirection = new System.Windows.Forms.ComboBox();
            this.txtAddressId1 = new System.Windows.Forms.TextBox();
            this.ChargePage = new System.Windows.Forms.TabPage();
            this.groupBox9 = new System.Windows.Forms.GroupBox();
            this.ucBatteryCharging = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucWatchSocIntervalMs = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucBatteryTemperature = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.btnRefreshBatterySate = new System.Windows.Forms.Button();
            this.ucBatteryVoltage = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.ucBatteryPercentage = new Mirle.Agv.AseMiddler.UcVerticalLabelText();
            this.groupBox10 = new System.Windows.Forms.GroupBox();
            this.btnSearchChargeAddress = new System.Windows.Forms.Button();
            this.btnStopCharge = new System.Windows.Forms.Button();
            this.btnStartCharge = new System.Windows.Forms.Button();
            this.boxChargeDirection = new System.Windows.Forms.ComboBox();
            this.txtChargeAddress = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.tabControl1.SuspendLayout();
            this.InfoPage.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SingleCommandPage.SuspendLayout();
            this.groupBox6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPsMessageNumber)).BeginInit();
            this.MovePage.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMoveSpeed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numHeadAngle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMovePositionY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMovePositionX)).BeginInit();
            this.RobotPage.SuspendLayout();
            this.groupBox8.SuspendLayout();
            this.groupBox7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPortNumber2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPortNumber1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numGateType)).BeginInit();
            this.ChargePage.SuspendLayout();
            this.groupBox9.SuspendLayout();
            this.groupBox10.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnHide
            // 
            this.btnHide.Location = new System.Drawing.Point(1050, 12);
            this.btnHide.Name = "btnHide";
            this.btnHide.Size = new System.Drawing.Size(117, 46);
            this.btnHide.TabIndex = 0;
            this.btnHide.Text = "X";
            this.btnHide.UseVisualStyleBackColor = true;
            this.btnHide.Click += new System.EventHandler(this.btnHide_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.InfoPage);
            this.tabControl1.Controls.Add(this.SingleCommandPage);
            this.tabControl1.Controls.Add(this.MovePage);
            this.tabControl1.Controls.Add(this.RobotPage);
            this.tabControl1.Controls.Add(this.ChargePage);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1032, 292);
            this.tabControl1.TabIndex = 1;
            // 
            // InfoPage
            // 
            this.InfoPage.Controls.Add(this.groupBox5);
            this.InfoPage.Controls.Add(this.groupBox4);
            this.InfoPage.Location = new System.Drawing.Point(4, 22);
            this.InfoPage.Name = "InfoPage";
            this.InfoPage.Padding = new System.Windows.Forms.Padding(3);
            this.InfoPage.Size = new System.Drawing.Size(1024, 266);
            this.InfoPage.TabIndex = 0;
            this.InfoPage.Text = "Info";
            this.InfoPage.UseVisualStyleBackColor = true;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.ucLinkTestIntervalMs);
            this.groupBox5.Controls.Add(this.ucT6Timeout);
            this.groupBox5.Controls.Add(this.ucT3Timeout);
            this.groupBox5.Controls.Add(this.ucIsServer);
            this.groupBox5.Controls.Add(this.ucPort);
            this.groupBox5.Controls.Add(this.ucIp);
            this.groupBox5.Controls.Add(this.btnLoadLocalConfig);
            this.groupBox5.Location = new System.Drawing.Point(174, 6);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(443, 253);
            this.groupBox5.TabIndex = 10;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Local Config";
            // 
            // ucLinkTestIntervalMs
            // 
            this.ucLinkTestIntervalMs.Location = new System.Drawing.Point(292, 92);
            this.ucLinkTestIntervalMs.Name = "ucLinkTestIntervalMs";
            this.ucLinkTestIntervalMs.Size = new System.Drawing.Size(140, 70);
            this.ucLinkTestIntervalMs.TabIndex = 13;
            this.ucLinkTestIntervalMs.TagColor = System.Drawing.Color.Black;
            this.ucLinkTestIntervalMs.TagName = "Heart Beat";
            this.ucLinkTestIntervalMs.TagValue = "TagVelue";
            // 
            // ucT6Timeout
            // 
            this.ucT6Timeout.Location = new System.Drawing.Point(149, 92);
            this.ucT6Timeout.Name = "ucT6Timeout";
            this.ucT6Timeout.Size = new System.Drawing.Size(140, 70);
            this.ucT6Timeout.TabIndex = 13;
            this.ucT6Timeout.TagColor = System.Drawing.Color.Black;
            this.ucT6Timeout.TagName = "T6";
            this.ucT6Timeout.TagValue = "TagVelue";
            // 
            // ucT3Timeout
            // 
            this.ucT3Timeout.Location = new System.Drawing.Point(6, 92);
            this.ucT3Timeout.Name = "ucT3Timeout";
            this.ucT3Timeout.Size = new System.Drawing.Size(140, 70);
            this.ucT3Timeout.TabIndex = 13;
            this.ucT3Timeout.TagColor = System.Drawing.Color.Black;
            this.ucT3Timeout.TagName = "T3";
            this.ucT3Timeout.TagValue = "TagVelue";
            // 
            // ucIsServer
            // 
            this.ucIsServer.Location = new System.Drawing.Point(292, 21);
            this.ucIsServer.Name = "ucIsServer";
            this.ucIsServer.Size = new System.Drawing.Size(140, 70);
            this.ucIsServer.TabIndex = 13;
            this.ucIsServer.TagColor = System.Drawing.Color.Black;
            this.ucIsServer.TagName = "IsServer";
            this.ucIsServer.TagValue = "TagVelue";
            // 
            // ucPort
            // 
            this.ucPort.Location = new System.Drawing.Point(149, 21);
            this.ucPort.Name = "ucPort";
            this.ucPort.Size = new System.Drawing.Size(140, 70);
            this.ucPort.TabIndex = 13;
            this.ucPort.TagColor = System.Drawing.Color.Black;
            this.ucPort.TagName = "Port";
            this.ucPort.TagValue = "TagVelue";
            // 
            // ucIp
            // 
            this.ucIp.Location = new System.Drawing.Point(6, 21);
            this.ucIp.Name = "ucIp";
            this.ucIp.Size = new System.Drawing.Size(140, 70);
            this.ucIp.TabIndex = 13;
            this.ucIp.TagColor = System.Drawing.Color.Black;
            this.ucIp.TagName = "IP";
            this.ucIp.TagValue = "TagVelue";
            // 
            // btnLoadLocalConfig
            // 
            this.btnLoadLocalConfig.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnLoadLocalConfig.Location = new System.Drawing.Point(149, 168);
            this.btnLoadLocalConfig.Name = "btnLoadLocalConfig";
            this.btnLoadLocalConfig.Size = new System.Drawing.Size(140, 70);
            this.btnLoadLocalConfig.TabIndex = 12;
            this.btnLoadLocalConfig.Text = "Load";
            this.btnLoadLocalConfig.UseVisualStyleBackColor = true;
            this.btnLoadLocalConfig.Click += new System.EventHandler(this.btnLoadLocalConfig_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.BackColor = System.Drawing.Color.Transparent;
            this.groupBox4.Controls.Add(this.txtConnection);
            this.groupBox4.Controls.Add(this.radioOnline);
            this.groupBox4.Controls.Add(this.radioOffline);
            this.groupBox4.Location = new System.Drawing.Point(6, 6);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(162, 123);
            this.groupBox4.TabIndex = 9;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Local Connection";
            // 
            // txtConnection
            // 
            this.txtConnection.BackColor = System.Drawing.Color.LightGray;
            this.txtConnection.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtConnection.Location = new System.Drawing.Point(6, 40);
            this.txtConnection.Name = "txtConnection";
            this.txtConnection.Size = new System.Drawing.Size(140, 70);
            this.txtConnection.TabIndex = 2;
            this.txtConnection.Text = "Connection";
            this.txtConnection.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // radioOnline
            // 
            this.radioOnline.AutoSize = true;
            this.radioOnline.Location = new System.Drawing.Point(97, 21);
            this.radioOnline.Name = "radioOnline";
            this.radioOnline.Size = new System.Drawing.Size(54, 16);
            this.radioOnline.TabIndex = 1;
            this.radioOnline.TabStop = true;
            this.radioOnline.Text = "Online";
            this.radioOnline.UseVisualStyleBackColor = true;
            // 
            // radioOffline
            // 
            this.radioOffline.AutoSize = true;
            this.radioOffline.Location = new System.Drawing.Point(6, 21);
            this.radioOffline.Name = "radioOffline";
            this.radioOffline.Size = new System.Drawing.Size(56, 16);
            this.radioOffline.TabIndex = 0;
            this.radioOffline.TabStop = true;
            this.radioOffline.Text = "Offline";
            this.radioOffline.UseVisualStyleBackColor = true;
            // 
            // SingleCommandPage
            // 
            this.SingleCommandPage.Controls.Add(this.groupBox6);
            this.SingleCommandPage.Location = new System.Drawing.Point(4, 22);
            this.SingleCommandPage.Name = "SingleCommandPage";
            this.SingleCommandPage.Padding = new System.Windows.Forms.Padding(3);
            this.SingleCommandPage.Size = new System.Drawing.Size(1024, 266);
            this.SingleCommandPage.TabIndex = 1;
            this.SingleCommandPage.Text = "SingleCommand";
            this.SingleCommandPage.UseVisualStyleBackColor = true;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.btnSingleMessageSend);
            this.groupBox6.Controls.Add(this.btnSaveAutoReplyMessage);
            this.groupBox6.Controls.Add(this.txtPsMessageText);
            this.groupBox6.Controls.Add(this.numPsMessageNumber);
            this.groupBox6.Controls.Add(this.boxPsMessageType);
            this.groupBox6.Controls.Add(this.boxPspMessageMap);
            this.groupBox6.Location = new System.Drawing.Point(6, 6);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(303, 194);
            this.groupBox6.TabIndex = 9;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Single Command";
            // 
            // btnSingleMessageSend
            // 
            this.btnSingleMessageSend.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnSingleMessageSend.Location = new System.Drawing.Point(152, 144);
            this.btnSingleMessageSend.Name = "btnSingleMessageSend";
            this.btnSingleMessageSend.Size = new System.Drawing.Size(140, 35);
            this.btnSingleMessageSend.TabIndex = 17;
            this.btnSingleMessageSend.Text = "Send";
            this.btnSingleMessageSend.UseVisualStyleBackColor = true;
            this.btnSingleMessageSend.Click += new System.EventHandler(this.btnSingleMessageSend_Click);
            // 
            // btnSaveAutoReplyMessage
            // 
            this.btnSaveAutoReplyMessage.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnSaveAutoReplyMessage.Location = new System.Drawing.Point(6, 144);
            this.btnSaveAutoReplyMessage.Name = "btnSaveAutoReplyMessage";
            this.btnSaveAutoReplyMessage.Size = new System.Drawing.Size(140, 35);
            this.btnSaveAutoReplyMessage.TabIndex = 16;
            this.btnSaveAutoReplyMessage.Text = "Save";
            this.btnSaveAutoReplyMessage.UseVisualStyleBackColor = true;
            this.btnSaveAutoReplyMessage.Click += new System.EventHandler(this.btnSaveAutoReplyMessage_Click);
            // 
            // txtPsMessageText
            // 
            this.txtPsMessageText.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtPsMessageText.Location = new System.Drawing.Point(6, 103);
            this.txtPsMessageText.Name = "txtPsMessageText";
            this.txtPsMessageText.Size = new System.Drawing.Size(286, 35);
            this.txtPsMessageText.TabIndex = 15;
            this.txtPsMessageText.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // numPsMessageNumber
            // 
            this.numPsMessageNumber.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.numPsMessageNumber.Location = new System.Drawing.Point(152, 62);
            this.numPsMessageNumber.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.numPsMessageNumber.Name = "numPsMessageNumber";
            this.numPsMessageNumber.Size = new System.Drawing.Size(140, 35);
            this.numPsMessageNumber.TabIndex = 14;
            this.numPsMessageNumber.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // boxPsMessageType
            // 
            this.boxPsMessageType.Cursor = System.Windows.Forms.Cursors.Default;
            this.boxPsMessageType.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.boxPsMessageType.FormattingEnabled = true;
            this.boxPsMessageType.Location = new System.Drawing.Point(6, 62);
            this.boxPsMessageType.Name = "boxPsMessageType";
            this.boxPsMessageType.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.boxPsMessageType.Size = new System.Drawing.Size(140, 35);
            this.boxPsMessageType.Sorted = true;
            this.boxPsMessageType.TabIndex = 13;
            // 
            // boxPspMessageMap
            // 
            this.boxPspMessageMap.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.boxPspMessageMap.FormattingEnabled = true;
            this.boxPspMessageMap.Location = new System.Drawing.Point(6, 21);
            this.boxPspMessageMap.Name = "boxPspMessageMap";
            this.boxPspMessageMap.Size = new System.Drawing.Size(286, 35);
            this.boxPspMessageMap.TabIndex = 12;
            this.boxPspMessageMap.SelectedIndexChanged += new System.EventHandler(this.boxPspMessageMap_SelectedIndexChanged);
            // 
            // MovePage
            // 
            this.MovePage.Controls.Add(this.groupBox2);
            this.MovePage.Controls.Add(this.groupBox1);
            this.MovePage.Controls.Add(this.groupBox3);
            this.MovePage.Location = new System.Drawing.Point(4, 22);
            this.MovePage.Name = "MovePage";
            this.MovePage.Size = new System.Drawing.Size(1024, 266);
            this.MovePage.TabIndex = 2;
            this.MovePage.Text = "Move";
            this.MovePage.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.ucMoveLastSection);
            this.groupBox2.Controls.Add(this.ucMoveMoveState);
            this.groupBox2.Controls.Add(this.ucMovePositionX);
            this.groupBox2.Controls.Add(this.btnRefreshPosition);
            this.groupBox2.Controls.Add(this.ucMoveLastAddress);
            this.groupBox2.Controls.Add(this.ucMovePositionY);
            this.groupBox2.Controls.Add(this.ucAskPositionInterval);
            this.groupBox2.Controls.Add(this.ucMoveIsMoveEnd);
            this.groupBox2.Location = new System.Drawing.Point(531, 11);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(490, 241);
            this.groupBox2.TabIndex = 20;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Move Status";
            // 
            // ucMoveLastSection
            // 
            this.ucMoveLastSection.Location = new System.Drawing.Point(15, 22);
            this.ucMoveLastSection.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucMoveLastSection.Name = "ucMoveLastSection";
            this.ucMoveLastSection.Size = new System.Drawing.Size(135, 65);
            this.ucMoveLastSection.TabIndex = 17;
            this.ucMoveLastSection.TagColor = System.Drawing.Color.Black;
            this.ucMoveLastSection.TagName = "Last Section";
            this.ucMoveLastSection.TagValue = "00101";
            // 
            // ucMoveMoveState
            // 
            this.ucMoveMoveState.Location = new System.Drawing.Point(156, 22);
            this.ucMoveMoveState.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucMoveMoveState.Name = "ucMoveMoveState";
            this.ucMoveMoveState.Size = new System.Drawing.Size(135, 65);
            this.ucMoveMoveState.TabIndex = 14;
            this.ucMoveMoveState.TagColor = System.Drawing.Color.Black;
            this.ucMoveMoveState.TagName = "MoveState";
            this.ucMoveMoveState.TagValue = "Idle";
            // 
            // ucMovePositionX
            // 
            this.ucMovePositionX.Location = new System.Drawing.Point(15, 165);
            this.ucMovePositionX.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucMovePositionX.Name = "ucMovePositionX";
            this.ucMovePositionX.Size = new System.Drawing.Size(135, 65);
            this.ucMovePositionX.TabIndex = 19;
            this.ucMovePositionX.TagColor = System.Drawing.Color.Black;
            this.ucMovePositionX.TagName = "X";
            this.ucMovePositionX.TagValue = "123456";
            // 
            // btnRefreshPosition
            // 
            this.btnRefreshPosition.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnRefreshPosition.Location = new System.Drawing.Point(297, 20);
            this.btnRefreshPosition.Name = "btnRefreshPosition";
            this.btnRefreshPosition.Size = new System.Drawing.Size(135, 65);
            this.btnRefreshPosition.TabIndex = 12;
            this.btnRefreshPosition.Text = "Refresh  Status";
            this.btnRefreshPosition.UseVisualStyleBackColor = true;
            this.btnRefreshPosition.Click += new System.EventHandler(this.btnRefreshPosition_Click);
            // 
            // ucMoveLastAddress
            // 
            this.ucMoveLastAddress.Location = new System.Drawing.Point(15, 94);
            this.ucMoveLastAddress.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucMoveLastAddress.Name = "ucMoveLastAddress";
            this.ucMoveLastAddress.Size = new System.Drawing.Size(135, 65);
            this.ucMoveLastAddress.TabIndex = 15;
            this.ucMoveLastAddress.TagColor = System.Drawing.Color.Black;
            this.ucMoveLastAddress.TagName = "Last Address";
            this.ucMoveLastAddress.TagValue = "10001";
            // 
            // ucMovePositionY
            // 
            this.ucMovePositionY.Location = new System.Drawing.Point(156, 165);
            this.ucMovePositionY.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucMovePositionY.Name = "ucMovePositionY";
            this.ucMovePositionY.Size = new System.Drawing.Size(135, 65);
            this.ucMovePositionY.TabIndex = 18;
            this.ucMovePositionY.TagColor = System.Drawing.Color.Black;
            this.ucMovePositionY.TagName = "Y";
            this.ucMovePositionY.TagValue = "-13579";
            // 
            // ucAskPositionInterval
            // 
            this.ucAskPositionInterval.Location = new System.Drawing.Point(297, 94);
            this.ucAskPositionInterval.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucAskPositionInterval.Name = "ucAskPositionInterval";
            this.ucAskPositionInterval.Size = new System.Drawing.Size(135, 65);
            this.ucAskPositionInterval.TabIndex = 16;
            this.ucAskPositionInterval.TagColor = System.Drawing.Color.Black;
            this.ucAskPositionInterval.TagName = "Pos Interval";
            this.ucAskPositionInterval.TagValue = "TagValue";
            // 
            // ucMoveIsMoveEnd
            // 
            this.ucMoveIsMoveEnd.Location = new System.Drawing.Point(156, 94);
            this.ucMoveIsMoveEnd.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucMoveIsMoveEnd.Name = "ucMoveIsMoveEnd";
            this.ucMoveIsMoveEnd.Size = new System.Drawing.Size(135, 65);
            this.ucMoveIsMoveEnd.TabIndex = 16;
            this.ucMoveIsMoveEnd.TagColor = System.Drawing.Color.Black;
            this.ucMoveIsMoveEnd.TagName = "Is Move End";
            this.ucMoveIsMoveEnd.TagValue = "True";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.boxKeepOrGo);
            this.groupBox1.Controls.Add(this.btnSearchMapAddress);
            this.groupBox1.Controls.Add(this.txtMapAddress);
            this.groupBox1.Location = new System.Drawing.Point(266, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(259, 249);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "MapAddress";
            // 
            // boxKeepOrGo
            // 
            this.boxKeepOrGo.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.boxKeepOrGo.FormattingEnabled = true;
            this.boxKeepOrGo.Location = new System.Drawing.Point(6, 50);
            this.boxKeepOrGo.Name = "boxKeepOrGo";
            this.boxKeepOrGo.Size = new System.Drawing.Size(247, 24);
            this.boxKeepOrGo.TabIndex = 7;
            // 
            // btnSearchMapAddress
            // 
            this.btnSearchMapAddress.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnSearchMapAddress.Location = new System.Drawing.Point(6, 205);
            this.btnSearchMapAddress.Name = "btnSearchMapAddress";
            this.btnSearchMapAddress.Size = new System.Drawing.Size(247, 33);
            this.btnSearchMapAddress.TabIndex = 3;
            this.btnSearchMapAddress.Text = "Search MapAddress";
            this.btnSearchMapAddress.UseVisualStyleBackColor = true;
            this.btnSearchMapAddress.Click += new System.EventHandler(this.btnSearchMapAddress_Click);
            // 
            // txtMapAddress
            // 
            this.txtMapAddress.ImeMode = System.Windows.Forms.ImeMode.Alpha;
            this.txtMapAddress.Location = new System.Drawing.Point(6, 15);
            this.txtMapAddress.Name = "txtMapAddress";
            this.txtMapAddress.Size = new System.Drawing.Size(247, 22);
            this.txtMapAddress.TabIndex = 2;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.boxIsEnd);
            this.groupBox3.Controls.Add(this.boxSlotOpen);
            this.groupBox3.Controls.Add(this.btnSendMove);
            this.groupBox3.Controls.Add(this.numMoveSpeed);
            this.groupBox3.Controls.Add(this.numHeadAngle);
            this.groupBox3.Controls.Add(this.numMovePositionY);
            this.groupBox3.Controls.Add(this.numMovePositionX);
            this.groupBox3.Location = new System.Drawing.Point(3, 3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(257, 249);
            this.groupBox3.TabIndex = 4;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Move(P41)";
            // 
            // boxIsEnd
            // 
            this.boxIsEnd.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.boxIsEnd.FormattingEnabled = true;
            this.boxIsEnd.Location = new System.Drawing.Point(6, 21);
            this.boxIsEnd.Name = "boxIsEnd";
            this.boxIsEnd.Size = new System.Drawing.Size(245, 28);
            this.boxIsEnd.TabIndex = 6;
            // 
            // boxSlotOpen
            // 
            this.boxSlotOpen.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.boxSlotOpen.FormattingEnabled = true;
            this.boxSlotOpen.Location = new System.Drawing.Point(6, 160);
            this.boxSlotOpen.Name = "boxSlotOpen";
            this.boxSlotOpen.Size = new System.Drawing.Size(245, 24);
            this.boxSlotOpen.TabIndex = 6;
            // 
            // btnSendMove
            // 
            this.btnSendMove.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnSendMove.Location = new System.Drawing.Point(6, 205);
            this.btnSendMove.Name = "btnSendMove";
            this.btnSendMove.Size = new System.Drawing.Size(245, 33);
            this.btnSendMove.TabIndex = 5;
            this.btnSendMove.Text = "Send";
            this.btnSendMove.UseVisualStyleBackColor = true;
            this.btnSendMove.Click += new System.EventHandler(this.btnSendMove_Click);
            // 
            // numMoveSpeed
            // 
            this.numMoveSpeed.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.numMoveSpeed.Location = new System.Drawing.Point(129, 125);
            this.numMoveSpeed.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.numMoveSpeed.Name = "numMoveSpeed";
            this.numMoveSpeed.Size = new System.Drawing.Size(122, 29);
            this.numMoveSpeed.TabIndex = 4;
            this.numMoveSpeed.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numMoveSpeed.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // numHeadAngle
            // 
            this.numHeadAngle.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.numHeadAngle.Location = new System.Drawing.Point(6, 125);
            this.numHeadAngle.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.numHeadAngle.Name = "numHeadAngle";
            this.numHeadAngle.Size = new System.Drawing.Size(117, 29);
            this.numHeadAngle.TabIndex = 3;
            this.numHeadAngle.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numHeadAngle.Value = new decimal(new int[] {
            360,
            0,
            0,
            0});
            // 
            // numMovePositionY
            // 
            this.numMovePositionY.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.numMovePositionY.Location = new System.Drawing.Point(6, 90);
            this.numMovePositionY.Maximum = new decimal(new int[] {
            99999999,
            0,
            0,
            0});
            this.numMovePositionY.Minimum = new decimal(new int[] {
            99999999,
            0,
            0,
            -2147483648});
            this.numMovePositionY.Name = "numMovePositionY";
            this.numMovePositionY.Size = new System.Drawing.Size(245, 29);
            this.numMovePositionY.TabIndex = 2;
            this.numMovePositionY.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numMovePositionY.Value = new decimal(new int[] {
            87654321,
            0,
            0,
            -2147483648});
            // 
            // numMovePositionX
            // 
            this.numMovePositionX.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.numMovePositionX.Location = new System.Drawing.Point(6, 55);
            this.numMovePositionX.Maximum = new decimal(new int[] {
            99999999,
            0,
            0,
            0});
            this.numMovePositionX.Minimum = new decimal(new int[] {
            99999999,
            0,
            0,
            -2147483648});
            this.numMovePositionX.Name = "numMovePositionX";
            this.numMovePositionX.Size = new System.Drawing.Size(245, 29);
            this.numMovePositionX.TabIndex = 1;
            this.numMovePositionX.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numMovePositionX.Value = new decimal(new int[] {
            12345678,
            0,
            0,
            0});
            // 
            // RobotPage
            // 
            this.RobotPage.Controls.Add(this.groupBox8);
            this.RobotPage.Controls.Add(this.groupBox7);
            this.RobotPage.Location = new System.Drawing.Point(4, 22);
            this.RobotPage.Name = "RobotPage";
            this.RobotPage.Size = new System.Drawing.Size(1024, 266);
            this.RobotPage.TabIndex = 3;
            this.RobotPage.Text = "Robot";
            this.RobotPage.UseVisualStyleBackColor = true;
            // 
            // groupBox8
            // 
            this.groupBox8.Controls.Add(this.ucRobotRobotState);
            this.groupBox8.Controls.Add(this.btnRefreshRobotState);
            this.groupBox8.Controls.Add(this.ucRobotSlotLId);
            this.groupBox8.Controls.Add(this.ucRobotIsHome);
            this.groupBox8.Controls.Add(this.ucRobotSlotLState);
            this.groupBox8.Controls.Add(this.ucRobotSlotRId);
            this.groupBox8.Controls.Add(this.ucRobotSlotRState);
            this.groupBox8.Location = new System.Drawing.Point(333, 4);
            this.groupBox8.Name = "groupBox8";
            this.groupBox8.Size = new System.Drawing.Size(581, 177);
            this.groupBox8.TabIndex = 60;
            this.groupBox8.TabStop = false;
            this.groupBox8.Text = "Robot Status";
            // 
            // ucRobotRobotState
            // 
            this.ucRobotRobotState.Location = new System.Drawing.Point(6, 22);
            this.ucRobotRobotState.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucRobotRobotState.Name = "ucRobotRobotState";
            this.ucRobotRobotState.Size = new System.Drawing.Size(135, 65);
            this.ucRobotRobotState.TabIndex = 58;
            this.ucRobotRobotState.TagColor = System.Drawing.Color.Black;
            this.ucRobotRobotState.TagName = "Robot State";
            this.ucRobotRobotState.TagValue = "Idle";
            // 
            // btnRefreshRobotState
            // 
            this.btnRefreshRobotState.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnRefreshRobotState.Location = new System.Drawing.Point(429, 22);
            this.btnRefreshRobotState.Name = "btnRefreshRobotState";
            this.btnRefreshRobotState.Size = new System.Drawing.Size(140, 70);
            this.btnRefreshRobotState.TabIndex = 59;
            this.btnRefreshRobotState.Text = "Refresh Status";
            this.btnRefreshRobotState.UseVisualStyleBackColor = true;
            this.btnRefreshRobotState.Click += new System.EventHandler(this.btnRefreshRobotState_Click);
            // 
            // ucRobotSlotLId
            // 
            this.ucRobotSlotLId.Location = new System.Drawing.Point(147, 95);
            this.ucRobotSlotLId.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucRobotSlotLId.Name = "ucRobotSlotLId";
            this.ucRobotSlotLId.Size = new System.Drawing.Size(135, 65);
            this.ucRobotSlotLId.TabIndex = 57;
            this.ucRobotSlotLId.TagColor = System.Drawing.Color.Black;
            this.ucRobotSlotLId.TagName = "Slot L Id";
            this.ucRobotSlotLId.TagValue = "ABC";
            // 
            // ucRobotIsHome
            // 
            this.ucRobotIsHome.Location = new System.Drawing.Point(6, 95);
            this.ucRobotIsHome.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucRobotIsHome.Name = "ucRobotIsHome";
            this.ucRobotIsHome.Size = new System.Drawing.Size(135, 65);
            this.ucRobotIsHome.TabIndex = 53;
            this.ucRobotIsHome.TagColor = System.Drawing.Color.Black;
            this.ucRobotIsHome.TagName = "Is Home";
            this.ucRobotIsHome.TagValue = "false";
            // 
            // ucRobotSlotLState
            // 
            this.ucRobotSlotLState.Location = new System.Drawing.Point(147, 22);
            this.ucRobotSlotLState.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucRobotSlotLState.Name = "ucRobotSlotLState";
            this.ucRobotSlotLState.Size = new System.Drawing.Size(135, 65);
            this.ucRobotSlotLState.TabIndex = 56;
            this.ucRobotSlotLState.TagColor = System.Drawing.Color.Black;
            this.ucRobotSlotLState.TagName = "Slot L State";
            this.ucRobotSlotLState.TagValue = "Empty";
            // 
            // ucRobotSlotRId
            // 
            this.ucRobotSlotRId.Location = new System.Drawing.Point(288, 95);
            this.ucRobotSlotRId.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucRobotSlotRId.Name = "ucRobotSlotRId";
            this.ucRobotSlotRId.Size = new System.Drawing.Size(135, 65);
            this.ucRobotSlotRId.TabIndex = 54;
            this.ucRobotSlotRId.TagColor = System.Drawing.Color.Black;
            this.ucRobotSlotRId.TagName = "Slot R Id";
            this.ucRobotSlotRId.TagValue = "PQR";
            // 
            // ucRobotSlotRState
            // 
            this.ucRobotSlotRState.Location = new System.Drawing.Point(288, 22);
            this.ucRobotSlotRState.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucRobotSlotRState.Name = "ucRobotSlotRState";
            this.ucRobotSlotRState.Size = new System.Drawing.Size(135, 65);
            this.ucRobotSlotRState.TabIndex = 55;
            this.ucRobotSlotRState.TagColor = System.Drawing.Color.Black;
            this.ucRobotSlotRState.TagName = "Slot R State";
            this.ucRobotSlotRState.TagValue = "Empty";
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.numPortNumber2);
            this.groupBox7.Controls.Add(this.numPortNumber1);
            this.groupBox7.Controls.Add(this.txtAddressId2);
            this.groupBox7.Controls.Add(this.numGateType);
            this.groupBox7.Controls.Add(this.btnSendRobot);
            this.groupBox7.Controls.Add(this.boxLDUD2);
            this.groupBox7.Controls.Add(this.boxSlotSelect2);
            this.groupBox7.Controls.Add(this.boxSlotSelect1);
            this.groupBox7.Controls.Add(this.boxLDUD1);
            this.groupBox7.Controls.Add(this.boxPioDirection);
            this.groupBox7.Controls.Add(this.txtAddressId1);
            this.groupBox7.Location = new System.Drawing.Point(3, 3);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(324, 260);
            this.groupBox7.TabIndex = 51;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Robot(P45)";
            // 
            // numPortNumber2
            // 
            this.numPortNumber2.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.numPortNumber2.Location = new System.Drawing.Point(162, 184);
            this.numPortNumber2.Maximum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numPortNumber2.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numPortNumber2.Name = "numPortNumber2";
            this.numPortNumber2.Size = new System.Drawing.Size(150, 29);
            this.numPortNumber2.TabIndex = 61;
            this.numPortNumber2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numPortNumber2.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // numPortNumber1
            // 
            this.numPortNumber1.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.numPortNumber1.Location = new System.Drawing.Point(8, 184);
            this.numPortNumber1.Maximum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numPortNumber1.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numPortNumber1.Name = "numPortNumber1";
            this.numPortNumber1.Size = new System.Drawing.Size(150, 29);
            this.numPortNumber1.TabIndex = 61;
            this.numPortNumber1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numPortNumber1.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // txtAddressId2
            // 
            this.txtAddressId2.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtAddressId2.ImeMode = System.Windows.Forms.ImeMode.Alpha;
            this.txtAddressId2.Location = new System.Drawing.Point(161, 149);
            this.txtAddressId2.Name = "txtAddressId2";
            this.txtAddressId2.Size = new System.Drawing.Size(151, 29);
            this.txtAddressId2.TabIndex = 8;
            this.txtAddressId2.Text = "AddressId2";
            // 
            // numGateType
            // 
            this.numGateType.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.numGateType.Location = new System.Drawing.Point(8, 45);
            this.numGateType.Maximum = new decimal(new int[] {
            9,
            0,
            0,
            0});
            this.numGateType.Name = "numGateType";
            this.numGateType.Size = new System.Drawing.Size(303, 29);
            this.numGateType.TabIndex = 61;
            this.numGateType.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // btnSendRobot
            // 
            this.btnSendRobot.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnSendRobot.Location = new System.Drawing.Point(8, 220);
            this.btnSendRobot.Name = "btnSendRobot";
            this.btnSendRobot.Size = new System.Drawing.Size(304, 33);
            this.btnSendRobot.TabIndex = 52;
            this.btnSendRobot.Text = "Send";
            this.btnSendRobot.UseVisualStyleBackColor = true;
            this.btnSendRobot.Click += new System.EventHandler(this.btnSendRobot_Click);
            // 
            // boxLDUD2
            // 
            this.boxLDUD2.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.boxLDUD2.FormattingEnabled = true;
            this.boxLDUD2.Location = new System.Drawing.Point(161, 81);
            this.boxLDUD2.Name = "boxLDUD2";
            this.boxLDUD2.Size = new System.Drawing.Size(150, 28);
            this.boxLDUD2.TabIndex = 10;
            // 
            // boxSlotSelect2
            // 
            this.boxSlotSelect2.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.boxSlotSelect2.FormattingEnabled = true;
            this.boxSlotSelect2.Location = new System.Drawing.Point(161, 115);
            this.boxSlotSelect2.Name = "boxSlotSelect2";
            this.boxSlotSelect2.Size = new System.Drawing.Size(151, 28);
            this.boxSlotSelect2.TabIndex = 10;
            // 
            // boxSlotSelect1
            // 
            this.boxSlotSelect1.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.boxSlotSelect1.FormattingEnabled = true;
            this.boxSlotSelect1.Location = new System.Drawing.Point(8, 115);
            this.boxSlotSelect1.Name = "boxSlotSelect1";
            this.boxSlotSelect1.Size = new System.Drawing.Size(147, 28);
            this.boxSlotSelect1.TabIndex = 10;
            // 
            // boxLDUD1
            // 
            this.boxLDUD1.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.boxLDUD1.FormattingEnabled = true;
            this.boxLDUD1.Location = new System.Drawing.Point(8, 80);
            this.boxLDUD1.Name = "boxLDUD1";
            this.boxLDUD1.Size = new System.Drawing.Size(147, 28);
            this.boxLDUD1.TabIndex = 10;
            // 
            // boxPioDirection
            // 
            this.boxPioDirection.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.boxPioDirection.FormattingEnabled = true;
            this.boxPioDirection.Location = new System.Drawing.Point(7, 14);
            this.boxPioDirection.Name = "boxPioDirection";
            this.boxPioDirection.Size = new System.Drawing.Size(304, 28);
            this.boxPioDirection.TabIndex = 10;
            // 
            // txtAddressId1
            // 
            this.txtAddressId1.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtAddressId1.ImeMode = System.Windows.Forms.ImeMode.Alpha;
            this.txtAddressId1.Location = new System.Drawing.Point(8, 149);
            this.txtAddressId1.Name = "txtAddressId1";
            this.txtAddressId1.Size = new System.Drawing.Size(147, 29);
            this.txtAddressId1.TabIndex = 8;
            this.txtAddressId1.Text = "AddressId1";
            // 
            // ChargePage
            // 
            this.ChargePage.Controls.Add(this.groupBox9);
            this.ChargePage.Controls.Add(this.groupBox10);
            this.ChargePage.Location = new System.Drawing.Point(4, 22);
            this.ChargePage.Name = "ChargePage";
            this.ChargePage.Size = new System.Drawing.Size(1024, 266);
            this.ChargePage.TabIndex = 4;
            this.ChargePage.Text = "Charge";
            this.ChargePage.UseVisualStyleBackColor = true;
            // 
            // groupBox9
            // 
            this.groupBox9.Controls.Add(this.ucBatteryCharging);
            this.groupBox9.Controls.Add(this.ucWatchSocIntervalMs);
            this.groupBox9.Controls.Add(this.ucBatteryTemperature);
            this.groupBox9.Controls.Add(this.btnRefreshBatterySate);
            this.groupBox9.Controls.Add(this.ucBatteryVoltage);
            this.groupBox9.Controls.Add(this.ucBatteryPercentage);
            this.groupBox9.Location = new System.Drawing.Point(294, 3);
            this.groupBox9.Name = "groupBox9";
            this.groupBox9.Size = new System.Drawing.Size(628, 179);
            this.groupBox9.TabIndex = 3;
            this.groupBox9.TabStop = false;
            this.groupBox9.Text = "Battery Status";
            // 
            // ucBatteryCharging
            // 
            this.ucBatteryCharging.Location = new System.Drawing.Point(146, 20);
            this.ucBatteryCharging.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucBatteryCharging.Name = "ucBatteryCharging";
            this.ucBatteryCharging.Size = new System.Drawing.Size(135, 65);
            this.ucBatteryCharging.TabIndex = 2;
            this.ucBatteryCharging.TagColor = System.Drawing.Color.Black;
            this.ucBatteryCharging.TagName = "Charging";
            this.ucBatteryCharging.TagValue = "false";
            // 
            // ucWatchSocIntervalMs
            // 
            this.ucWatchSocIntervalMs.Location = new System.Drawing.Point(287, 92);
            this.ucWatchSocIntervalMs.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucWatchSocIntervalMs.Name = "ucWatchSocIntervalMs";
            this.ucWatchSocIntervalMs.Size = new System.Drawing.Size(135, 65);
            this.ucWatchSocIntervalMs.TabIndex = 2;
            this.ucWatchSocIntervalMs.TagColor = System.Drawing.Color.Black;
            this.ucWatchSocIntervalMs.TagName = "Soc Interval";
            this.ucWatchSocIntervalMs.TagValue = "40.5";
            // 
            // ucBatteryTemperature
            // 
            this.ucBatteryTemperature.Location = new System.Drawing.Point(146, 93);
            this.ucBatteryTemperature.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucBatteryTemperature.Name = "ucBatteryTemperature";
            this.ucBatteryTemperature.Size = new System.Drawing.Size(135, 65);
            this.ucBatteryTemperature.TabIndex = 2;
            this.ucBatteryTemperature.TagColor = System.Drawing.Color.Black;
            this.ucBatteryTemperature.TagName = "Temperature";
            this.ucBatteryTemperature.TagValue = "40.5";
            // 
            // btnRefreshBatterySate
            // 
            this.btnRefreshBatterySate.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnRefreshBatterySate.Location = new System.Drawing.Point(285, 20);
            this.btnRefreshBatterySate.Name = "btnRefreshBatterySate";
            this.btnRefreshBatterySate.Size = new System.Drawing.Size(135, 65);
            this.btnRefreshBatterySate.TabIndex = 3;
            this.btnRefreshBatterySate.Text = "Refresh State";
            this.btnRefreshBatterySate.UseVisualStyleBackColor = true;
            this.btnRefreshBatterySate.Click += new System.EventHandler(this.btnRefreshBatterySate_Click);
            // 
            // ucBatteryVoltage
            // 
            this.ucBatteryVoltage.Location = new System.Drawing.Point(6, 93);
            this.ucBatteryVoltage.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucBatteryVoltage.Name = "ucBatteryVoltage";
            this.ucBatteryVoltage.Size = new System.Drawing.Size(135, 65);
            this.ucBatteryVoltage.TabIndex = 2;
            this.ucBatteryVoltage.TagColor = System.Drawing.Color.Black;
            this.ucBatteryVoltage.TagName = "Voltage";
            this.ucBatteryVoltage.TagValue = "55.66";
            // 
            // ucBatteryPercentage
            // 
            this.ucBatteryPercentage.Location = new System.Drawing.Point(6, 21);
            this.ucBatteryPercentage.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ucBatteryPercentage.Name = "ucBatteryPercentage";
            this.ucBatteryPercentage.Size = new System.Drawing.Size(135, 65);
            this.ucBatteryPercentage.TabIndex = 0;
            this.ucBatteryPercentage.TagColor = System.Drawing.Color.Black;
            this.ucBatteryPercentage.TagName = "Percentage";
            this.ucBatteryPercentage.TagValue = " 70.0";
            // 
            // groupBox10
            // 
            this.groupBox10.Controls.Add(this.btnSearchChargeAddress);
            this.groupBox10.Controls.Add(this.btnStopCharge);
            this.groupBox10.Controls.Add(this.btnStartCharge);
            this.groupBox10.Controls.Add(this.boxChargeDirection);
            this.groupBox10.Controls.Add(this.txtChargeAddress);
            this.groupBox10.Location = new System.Drawing.Point(3, 3);
            this.groupBox10.Name = "groupBox10";
            this.groupBox10.Size = new System.Drawing.Size(285, 179);
            this.groupBox10.TabIndex = 2;
            this.groupBox10.TabStop = false;
            this.groupBox10.Text = "Charge(P47)";
            // 
            // btnSearchChargeAddress
            // 
            this.btnSearchChargeAddress.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnSearchChargeAddress.Location = new System.Drawing.Point(6, 53);
            this.btnSearchChargeAddress.Name = "btnSearchChargeAddress";
            this.btnSearchChargeAddress.Size = new System.Drawing.Size(273, 34);
            this.btnSearchChargeAddress.TabIndex = 4;
            this.btnSearchChargeAddress.Text = "Search Address";
            this.btnSearchChargeAddress.UseVisualStyleBackColor = true;
            // 
            // btnStopCharge
            // 
            this.btnStopCharge.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnStopCharge.Location = new System.Drawing.Point(140, 131);
            this.btnStopCharge.Name = "btnStopCharge";
            this.btnStopCharge.Size = new System.Drawing.Size(139, 42);
            this.btnStopCharge.TabIndex = 3;
            this.btnStopCharge.Text = "DisCharge";
            this.btnStopCharge.UseVisualStyleBackColor = true;
            this.btnStopCharge.Click += new System.EventHandler(this.btnStopCharge_Click);
            // 
            // btnStartCharge
            // 
            this.btnStartCharge.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnStartCharge.Location = new System.Drawing.Point(6, 131);
            this.btnStartCharge.Name = "btnStartCharge";
            this.btnStartCharge.Size = new System.Drawing.Size(128, 42);
            this.btnStartCharge.TabIndex = 3;
            this.btnStartCharge.Text = "Charge";
            this.btnStartCharge.UseVisualStyleBackColor = true;
            this.btnStartCharge.Click += new System.EventHandler(this.btnStartCharge_Click);
            // 
            // boxChargeDirection
            // 
            this.boxChargeDirection.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.boxChargeDirection.FormattingEnabled = true;
            this.boxChargeDirection.Location = new System.Drawing.Point(6, 93);
            this.boxChargeDirection.Name = "boxChargeDirection";
            this.boxChargeDirection.Size = new System.Drawing.Size(273, 32);
            this.boxChargeDirection.TabIndex = 1;
            this.boxChargeDirection.Text = "Right";
            // 
            // txtChargeAddress
            // 
            this.txtChargeAddress.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtChargeAddress.Location = new System.Drawing.Point(6, 21);
            this.txtChargeAddress.Name = "txtChargeAddress";
            this.txtChargeAddress.Size = new System.Drawing.Size(273, 26);
            this.txtChargeAddress.TabIndex = 0;
            // 
            // textBox1
            // 
            this.textBox1.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.textBox1.Location = new System.Drawing.Point(16, 310);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox1.Size = new System.Drawing.Size(1032, 445);
            this.textBox1.TabIndex = 2;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 2000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // AgvlConnectorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1179, 767);
            this.ControlBox = false;
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.btnHide);
            this.Name = "AgvlConnectorForm";
            this.Text = "AGVL";
            this.tabControl1.ResumeLayout(false);
            this.InfoPage.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.SingleCommandPage.ResumeLayout(false);
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPsMessageNumber)).EndInit();
            this.MovePage.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numMoveSpeed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numHeadAngle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMovePositionY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMovePositionX)).EndInit();
            this.RobotPage.ResumeLayout(false);
            this.groupBox8.ResumeLayout(false);
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPortNumber2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPortNumber1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numGateType)).EndInit();
            this.ChargePage.ResumeLayout(false);
            this.groupBox9.ResumeLayout(false);
            this.groupBox10.ResumeLayout(false);
            this.groupBox10.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnHide;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage InfoPage;
        private System.Windows.Forms.TabPage SingleCommandPage;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.GroupBox groupBox5;
        private UcVerticalLabelText ucLinkTestIntervalMs;
        private UcVerticalLabelText ucT6Timeout;
        private UcVerticalLabelText ucT3Timeout;
        private UcVerticalLabelText ucIsServer;
        private UcVerticalLabelText ucPort;
        private UcVerticalLabelText ucIp;
        private System.Windows.Forms.Button btnLoadLocalConfig;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label txtConnection;
        private System.Windows.Forms.RadioButton radioOnline;
        private System.Windows.Forms.RadioButton radioOffline;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.Button btnSingleMessageSend;
        private System.Windows.Forms.Button btnSaveAutoReplyMessage;
        private System.Windows.Forms.TextBox txtPsMessageText;
        private System.Windows.Forms.NumericUpDown numPsMessageNumber;
        private System.Windows.Forms.ComboBox boxPsMessageType;
        private System.Windows.Forms.ComboBox boxPspMessageMap;
        private System.Windows.Forms.TabPage MovePage;
        private System.Windows.Forms.TabPage RobotPage;
        private System.Windows.Forms.TabPage ChargePage;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox boxKeepOrGo;
        private System.Windows.Forms.Button btnSearchMapAddress;
        private System.Windows.Forms.TextBox txtMapAddress;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ComboBox boxIsEnd;
        private System.Windows.Forms.ComboBox boxSlotOpen;
        private System.Windows.Forms.Button btnSendMove;
        private System.Windows.Forms.NumericUpDown numMoveSpeed;
        private System.Windows.Forms.NumericUpDown numHeadAngle;
        private System.Windows.Forms.NumericUpDown numMovePositionY;
        private System.Windows.Forms.NumericUpDown numMovePositionX;
        private System.Windows.Forms.GroupBox groupBox2;
        private UcVerticalLabelText ucMoveLastSection;
        private UcVerticalLabelText ucMoveMoveState;
        private UcVerticalLabelText ucMovePositionX;
        private UcVerticalLabelText ucMoveLastAddress;
        private UcVerticalLabelText ucMovePositionY;
        private UcVerticalLabelText ucMoveIsMoveEnd;
        private System.Windows.Forms.Button btnRefreshPosition;
        private System.Windows.Forms.GroupBox groupBox8;
        private UcVerticalLabelText ucRobotRobotState;
        private System.Windows.Forms.Button btnRefreshRobotState;
        private UcVerticalLabelText ucRobotSlotLId;
        private UcVerticalLabelText ucRobotIsHome;
        private UcVerticalLabelText ucRobotSlotLState;
        private UcVerticalLabelText ucRobotSlotRId;
        private UcVerticalLabelText ucRobotSlotRState;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.ComboBox boxPioDirection;
        private System.Windows.Forms.TextBox txtAddressId1;
        private System.Windows.Forms.Button btnSendRobot;
        private System.Windows.Forms.GroupBox groupBox9;
        private UcVerticalLabelText ucBatteryCharging;
        private UcVerticalLabelText ucBatteryTemperature;
        private System.Windows.Forms.Button btnRefreshBatterySate;
        private UcVerticalLabelText ucBatteryVoltage;
        private UcVerticalLabelText ucBatteryPercentage;
        private System.Windows.Forms.GroupBox groupBox10;
        private System.Windows.Forms.Button btnSearchChargeAddress;
        private System.Windows.Forms.Button btnStopCharge;
        private System.Windows.Forms.Button btnStartCharge;
        private System.Windows.Forms.ComboBox boxChargeDirection;
        private System.Windows.Forms.TextBox txtChargeAddress;
        private UcVerticalLabelText ucAskPositionInterval;
        private System.Windows.Forms.ComboBox boxLDUD1;
        private System.Windows.Forms.NumericUpDown numGateType;
        private System.Windows.Forms.NumericUpDown numPortNumber2;
        private System.Windows.Forms.NumericUpDown numPortNumber1;
        private System.Windows.Forms.ComboBox boxLDUD2;
        private System.Windows.Forms.TextBox txtAddressId2;
        private System.Windows.Forms.ComboBox boxSlotSelect2;
        private System.Windows.Forms.ComboBox boxSlotSelect1;
        private UcVerticalLabelText ucWatchSocIntervalMs;
    }
}