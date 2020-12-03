namespace Mirle.Agv.AseMiddler.View
{
    partial class AgvcConnectorForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AgvcConnectorForm));
            this.btnDisConnect = new System.Windows.Forms.Button();
            this.btnConnect = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.txtRemotePort = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtRemoteIp = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.boxAgvCmdNums = new System.Windows.Forms.ComboBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel3 = new System.Windows.Forms.ToolStripStatusLabel();
            this.btnIsClientAgentNull = new System.Windows.Forms.Button();
            this.btnHide = new System.Windows.Forms.Button();
            this.tbxCommLogMsg = new System.Windows.Forms.TextBox();
            this.timerUI = new System.Windows.Forms.Timer(this.components);
            this.txtAgvCommandInfo = new System.Windows.Forms.TextBox();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnDisConnect
            // 
            this.btnDisConnect.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnDisConnect.Location = new System.Drawing.Point(374, 6);
            this.btnDisConnect.Name = "btnDisConnect";
            this.btnDisConnect.Size = new System.Drawing.Size(105, 23);
            this.btnDisConnect.TabIndex = 20;
            this.btnDisConnect.Text = "DisConnect";
            this.btnDisConnect.UseVisualStyleBackColor = true;
            this.btnDisConnect.Click += new System.EventHandler(this.btnDisConnect_Click);
            // 
            // btnConnect
            // 
            this.btnConnect.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnConnect.Location = new System.Drawing.Point(293, 6);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(75, 23);
            this.btnConnect.TabIndex = 19;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label3.Location = new System.Drawing.Point(186, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(33, 16);
            this.label3.TabIndex = 18;
            this.label3.Text = "Port";
            // 
            // txtRemotePort
            // 
            this.txtRemotePort.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtRemotePort.Location = new System.Drawing.Point(225, 6);
            this.txtRemotePort.Name = "txtRemotePort";
            this.txtRemotePort.Size = new System.Drawing.Size(62, 27);
            this.txtRemotePort.TabIndex = 17;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(21, 16);
            this.label2.TabIndex = 16;
            this.label2.Text = "IP";
            // 
            // txtRemoteIp
            // 
            this.txtRemoteIp.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtRemoteIp.Location = new System.Drawing.Point(39, 6);
            this.txtRemoteIp.Name = "txtRemoteIp";
            this.txtRemoteIp.Size = new System.Drawing.Size(141, 27);
            this.txtRemoteIp.TabIndex = 15;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label1.Location = new System.Drawing.Point(8, 42);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 16);
            this.label1.TabIndex = 23;
            this.label1.Text = "Cmd";
            // 
            // boxAgvCmdNums
            // 
            this.boxAgvCmdNums.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.boxAgvCmdNums.FormattingEnabled = true;
            this.boxAgvCmdNums.Items.AddRange(new object[] {
            "Cmd31_TransferRequest",
            "Cmd32_TransferCompleteResponse",
            "Cmd33_ControlZoneCancelRequest",
            "Cmd35_CarrierIdRenameRequest",
            "Cmd36_TransferEventResponse",
            "Cmd37_TransferCancelRequest",
            "Cmd39_PauseRequest",
            "Cmd41_ModeChange",
            "Cmd43_StatusRequest",
            "Cmd44_StatusRequest",
            "Cmd45_PowerOnoffRequest",
            "Cmd51_AvoidRequest",
            "Cmd52_AvoidCompleteResponse",
            "Cmd71_RangeTeachRequest",
            "Cmd72_RangeTeachCompleteResponse",
            "Cmd74_AddressTeachResponse",
            "Cmd91_AlarmResetRequest",
            "Cmd94_AlarmResponse",
            "Cmd131_TransferResponse",
            "Cmd132_TransferCompleteReport",
            "Cmd133_ControlZoneCancelResponse",
            "Cmd134_TransferEventReport",
            "Cmd135_CarrierIdRenameResponse",
            "Cmd136_TransferEventReport",
            "Cmd137_TransferCancelResponse",
            "Cmd139_PauseResponse",
            "Cmd141_ModeChangeResponse",
            "Cmd143_StatusResponse",
            "Cmd144_StatusReport",
            "Cmd145_PowerOnoffResponse",
            "Cmd151_AvoidResponse",
            "Cmd152_AvoidCompleteReport",
            "Cmd171_RangeTeachResponse",
            "Cmd172_RangeTeachCompleteReport",
            "Cmd174_AddressTeachReport",
            "Cmd191_AlarmResetResponse",
            "Cmd194_AlarmReport"});
            this.boxAgvCmdNums.Location = new System.Drawing.Point(52, 39);
            this.boxAgvCmdNums.Name = "boxAgvCmdNums";
            this.boxAgvCmdNums.Size = new System.Drawing.Size(960, 24);
            this.boxAgvCmdNums.TabIndex = 22;
            this.boxAgvCmdNums.SelectedValueChanged += new System.EventHandler(this.cbSend_SelectedValueChanged);
            // 
            // btnSend
            // 
            this.btnSend.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnSend.Location = new System.Drawing.Point(1018, 42);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(265, 23);
            this.btnSend.TabIndex = 21;
            this.btnSend.Text = "Send";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel2,
            this.toolStripStatusLabel3});
            this.statusStrip1.Location = new System.Drawing.Point(0, 849);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1295, 22);
            this.statusStrip1.TabIndex = 26;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(71, 17);
            this.toolStripStatusLabel1.Text = "DisConnect";
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)));
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(1209, 17);
            this.toolStripStatusLabel2.Spring = true;
            // 
            // toolStripStatusLabel3
            // 
            this.toolStripStatusLabel3.Name = "toolStripStatusLabel3";
            this.toolStripStatusLabel3.Size = new System.Drawing.Size(0, 17);
            // 
            // btnIsClientAgentNull
            // 
            this.btnIsClientAgentNull.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnIsClientAgentNull.Location = new System.Drawing.Point(485, 6);
            this.btnIsClientAgentNull.Name = "btnIsClientAgentNull";
            this.btnIsClientAgentNull.Size = new System.Drawing.Size(69, 23);
            this.btnIsClientAgentNull.TabIndex = 20;
            this.btnIsClientAgentNull.Text = "IsNull";
            this.btnIsClientAgentNull.UseVisualStyleBackColor = true;
            this.btnIsClientAgentNull.Click += new System.EventHandler(this.btnIsClientAgentNull_Click);
            // 
            // btnHide
            // 
            this.btnHide.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnHide.Location = new System.Drawing.Point(1018, 6);
            this.btnHide.Name = "btnHide";
            this.btnHide.Size = new System.Drawing.Size(265, 27);
            this.btnHide.TabIndex = 27;
            this.btnHide.Text = "X";
            this.btnHide.UseVisualStyleBackColor = true;
            this.btnHide.Click += new System.EventHandler(this.btnHide_Click);
            // 
            // tbxCommLogMsg
            // 
            this.tbxCommLogMsg.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.tbxCommLogMsg.Location = new System.Drawing.Point(0, 402);
            this.tbxCommLogMsg.Multiline = true;
            this.tbxCommLogMsg.Name = "tbxCommLogMsg";
            this.tbxCommLogMsg.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbxCommLogMsg.Size = new System.Drawing.Size(1295, 444);
            this.tbxCommLogMsg.TabIndex = 60;
            // 
            // timerUI
            // 
            this.timerUI.Enabled = true;
            this.timerUI.Interval = 2000;
            this.timerUI.Tick += new System.EventHandler(this.timerUI_Tick);
            // 
            // txtAgvCommandInfo
            // 
            this.txtAgvCommandInfo.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtAgvCommandInfo.Location = new System.Drawing.Point(0, 69);
            this.txtAgvCommandInfo.Multiline = true;
            this.txtAgvCommandInfo.Name = "txtAgvCommandInfo";
            this.txtAgvCommandInfo.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtAgvCommandInfo.Size = new System.Drawing.Size(1295, 327);
            this.txtAgvCommandInfo.TabIndex = 61;
            this.txtAgvCommandInfo.WordWrap = false;
            // 
            // AgvcConnectorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.ClientSize = new System.Drawing.Size(1295, 871);
            this.ControlBox = false;
            this.Controls.Add(this.txtAgvCommandInfo);
            this.Controls.Add(this.tbxCommLogMsg);
            this.Controls.Add(this.btnHide);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.boxAgvCmdNums);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.btnIsClientAgentNull);
            this.Controls.Add(this.btnDisConnect);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtRemotePort);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtRemoteIp);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AgvcConnectorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AgvcConnectorForm";
            this.Load += new System.EventHandler(this.CommunicationForm_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnDisConnect;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtRemotePort;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtRemoteIp;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox boxAgvCmdNums;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel3;
        private System.Windows.Forms.Button btnIsClientAgentNull;
        private System.Windows.Forms.Button btnHide;
        private System.Windows.Forms.TextBox tbxCommLogMsg;
        private System.Windows.Forms.Timer timerUI;
        private System.Windows.Forms.TextBox txtAgvCommandInfo;
    }
}