namespace Mirle.Agv.AseMiddler.View
{
    partial class AlarmForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AlarmForm));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tbxHistoryAlarms = new System.Windows.Forms.TextBox();
            this.tbxHappendingAlarms = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnAlarmReset = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.timeUpdateUI = new System.Windows.Forms.Timer(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.tbxHistoryAlarms, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.tbxHappendingAlarms, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80.75117F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 19.24883F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1311, 728);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tbxHistoryAlarms
            // 
            this.tbxHistoryAlarms.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbxHistoryAlarms.Font = new System.Drawing.Font("新細明體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.tbxHistoryAlarms.Location = new System.Drawing.Point(658, 3);
            this.tbxHistoryAlarms.Multiline = true;
            this.tbxHistoryAlarms.Name = "tbxHistoryAlarms";
            this.tbxHistoryAlarms.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbxHistoryAlarms.Size = new System.Drawing.Size(650, 581);
            this.tbxHistoryAlarms.TabIndex = 61;
            // 
            // tbxHappendingAlarms
            // 
            this.tbxHappendingAlarms.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbxHappendingAlarms.Font = new System.Drawing.Font("新細明體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.tbxHappendingAlarms.Location = new System.Drawing.Point(3, 3);
            this.tbxHappendingAlarms.Multiline = true;
            this.tbxHappendingAlarms.Name = "tbxHappendingAlarms";
            this.tbxHappendingAlarms.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbxHappendingAlarms.Size = new System.Drawing.Size(649, 581);
            this.tbxHappendingAlarms.TabIndex = 60;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnAlarmReset);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 590);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(649, 135);
            this.panel1.TabIndex = 3;
            // 
            // btnAlarmReset
            // 
            this.btnAlarmReset.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnAlarmReset.ForeColor = System.Drawing.Color.Red;
            this.btnAlarmReset.Location = new System.Drawing.Point(3, 0);
            this.btnAlarmReset.Name = "btnAlarmReset";
            this.btnAlarmReset.Size = new System.Drawing.Size(158, 132);
            this.btnAlarmReset.TabIndex = 6;
            this.btnAlarmReset.Text = "Alarm Reset";
            this.btnAlarmReset.UseVisualStyleBackColor = true;
            this.btnAlarmReset.Click += new System.EventHandler(this.btnAlarmReset_Click);
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("微軟正黑體", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.button1.ForeColor = System.Drawing.Color.Black;
            this.button1.Location = new System.Drawing.Point(488, 0);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(158, 132);
            this.button1.TabIndex = 8;
            this.button1.Text = "X";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // timeUpdateUI
            // 
            this.timeUpdateUI.Enabled = true;
            this.timeUpdateUI.Interval = 1000;
            this.timeUpdateUI.Tick += new System.EventHandler(this.timeUpdateUI_Tick);
            // 
            // AlarmForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(1311, 728);
            this.ControlBox = false;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AlarmForm";
            this.Text = "Alarms";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnAlarmReset;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox tbxHappendingAlarms;
        private System.Windows.Forms.TextBox tbxHistoryAlarms;
        private System.Windows.Forms.Timer timeUpdateUI;
    }
}