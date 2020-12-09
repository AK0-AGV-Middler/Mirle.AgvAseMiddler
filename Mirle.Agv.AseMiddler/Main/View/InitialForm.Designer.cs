namespace Mirle.Agv.AseMiddler.View
{
    partial class InitialForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InitialForm));
            this.Label3 = new System.Windows.Forms.Label();
            this.Label2 = new System.Windows.Forms.Label();
            this.Label1 = new System.Windows.Forms.Label();
            this.Label4 = new System.Windows.Forms.Label();
            this.lst_StartUpMsg = new System.Windows.Forms.ListBox();
            this.cmd_Close = new System.Windows.Forms.Button();
            this.PictureBox2 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // Label3
            // 
            this.Label3.AutoSize = true;
            this.Label3.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Label3.Location = new System.Drawing.Point(12, 286);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(146, 16);
            this.Label3.TabIndex = 19;
            this.Label3.Text = "盟立自動化(股)公司";
            // 
            // Label2
            // 
            this.Label2.AutoSize = true;
            this.Label2.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Label2.Location = new System.Drawing.Point(12, 306);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(288, 16);
            this.Label2.TabIndex = 18;
            this.Label2.Text = "台灣 30076 新竹市科學園區研發二路3號";
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Label1.Location = new System.Drawing.Point(12, 326);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(259, 16);
            this.Label1.TabIndex = 17;
            this.Label1.Text = "Tel:886-3-5783280 Fax:886-3-5780408";
            // 
            // Label4
            // 
            this.Label4.BackColor = System.Drawing.Color.LightGreen;
            this.Label4.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Label4.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.Label4.Location = new System.Drawing.Point(404, 1);
            this.Label4.Name = "Label4";
            this.Label4.Size = new System.Drawing.Size(541, 80);
            this.Label4.TabIndex = 21;
            this.Label4.Text = "盟立 AGV";
            this.Label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lst_StartUpMsg
            // 
            this.lst_StartUpMsg.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lst_StartUpMsg.FormattingEnabled = true;
            this.lst_StartUpMsg.ItemHeight = 16;
            this.lst_StartUpMsg.Location = new System.Drawing.Point(408, 85);
            this.lst_StartUpMsg.Name = "lst_StartUpMsg";
            this.lst_StartUpMsg.Size = new System.Drawing.Size(538, 276);
            this.lst_StartUpMsg.TabIndex = 22;
            // 
            // cmd_Close
            // 
            this.cmd_Close.Image = ((System.Drawing.Image)(resources.GetObject("cmd_Close.Image")));
            this.cmd_Close.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.cmd_Close.Location = new System.Drawing.Point(319, 284);
            this.cmd_Close.Name = "cmd_Close";
            this.cmd_Close.Size = new System.Drawing.Size(83, 58);
            this.cmd_Close.TabIndex = 20;
            this.cmd_Close.Text = "關閉";
            this.cmd_Close.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.cmd_Close.UseVisualStyleBackColor = true;
            this.cmd_Close.Click += new System.EventHandler(this.cmd_Close_Click);
            // 
            // PictureBox2
            // 
            this.PictureBox2.Image = global::Mirle.Agv.AseMiddler.Properties.Resources.Mirle_Logo22;
            this.PictureBox2.Location = new System.Drawing.Point(-4, 1);
            this.PictureBox2.Name = "PictureBox2";
            this.PictureBox2.Size = new System.Drawing.Size(412, 268);
            this.PictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.PictureBox2.TabIndex = 23;
            this.PictureBox2.TabStop = false;
            // 
            // InitialForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(947, 358);
            this.Controls.Add(this.PictureBox2);
            this.Controls.Add(this.lst_StartUpMsg);
            this.Controls.Add(this.Label4);
            this.Controls.Add(this.cmd_Close);
            this.Controls.Add(this.Label3);
            this.Controls.Add(this.Label2);
            this.Controls.Add(this.Label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "InitialForm";
            this.Text = "InitialForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.InitialForm_FormClosing);
            this.Shown += new System.EventHandler(this.InitialForm_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        internal System.Windows.Forms.Label Label3;
        internal System.Windows.Forms.Label Label2;
        internal System.Windows.Forms.Label Label1;
        internal System.Windows.Forms.Button cmd_Close;
        internal System.Windows.Forms.Label Label4;
        internal System.Windows.Forms.ListBox lst_StartUpMsg;
        internal System.Windows.Forms.PictureBox PictureBox2;
    }
}