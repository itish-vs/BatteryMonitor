namespace BatteryMonitor
{
    partial class Monitor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private Label lblStatus;
        private Panel panel1;
        private Button btnMinimizeToTray;
        private Button btnMinimize;
        private Button btnClose;
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Monitor));
            lblStatus = new Label();
            panel1 = new Panel();
            lblConfig = new Label();
            pictureBoxCharging = new PictureBox();
            lblAutoClose = new Label();
            panel2 = new Panel();
            btnClose = new Button();
            label1 = new Label();
            btnMinimizeToTray = new Button();
            btnMinimize = new Button();
            notifyIcon1 = new NotifyIcon(components);
            timer1 = new System.Windows.Forms.Timer(components);
            toolTip1 = new ToolTip(components);
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxCharging).BeginInit();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblStatus.ForeColor = Color.DarkGreen;
            lblStatus.Location = new Point(2, 61);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(132, 21);
            lblStatus.TabIndex = 0;
            lblStatus.Text = "Battery Monitor";
            // 
            // panel1
            // 
            panel1.BackColor = Color.LightSteelBlue;
            panel1.BorderStyle = BorderStyle.FixedSingle;
            panel1.Controls.Add(lblStatus);
            panel1.Controls.Add(lblConfig);
            panel1.Controls.Add(pictureBoxCharging);
            panel1.Controls.Add(lblAutoClose);
            panel1.Controls.Add(panel2);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(577, 125);
            panel1.TabIndex = 1;
            // 
            // lblConfig
            // 
            lblConfig.AutoSize = true;
            lblConfig.Location = new Point(2, 94);
            lblConfig.Name = "lblConfig";
            lblConfig.Size = new Size(43, 15);
            lblConfig.TabIndex = 1;
            lblConfig.Text = "Config";
            // 
            // pictureBoxCharging
            // 
            pictureBoxCharging.Image = (Image)resources.GetObject("pictureBoxCharging.Image");
            pictureBoxCharging.Location = new Point(516, 61);
            pictureBoxCharging.Name = "pictureBoxCharging";
            pictureBoxCharging.Size = new Size(48, 48);
            pictureBoxCharging.SizeMode = PictureBoxSizeMode.AutoSize;
            pictureBoxCharging.TabIndex = 2;
            pictureBoxCharging.TabStop = false;
            // 
            // lblAutoClose
            // 
            lblAutoClose.AutoSize = true;
            lblAutoClose.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblAutoClose.ForeColor = Color.Firebrick;
            lblAutoClose.Location = new Point(24, 33);
            lblAutoClose.Name = "lblAutoClose";
            lblAutoClose.Size = new Size(0, 15);
            lblAutoClose.TabIndex = 2;
            // 
            // panel2
            // 
            panel2.BackColor = Color.Gray;
            panel2.BorderStyle = BorderStyle.FixedSingle;
            panel2.Controls.Add(btnClose);
            panel2.Controls.Add(label1);
            panel2.Controls.Add(btnMinimizeToTray);
            panel2.Controls.Add(btnMinimize);
            panel2.Location = new Point(0, 0);
            panel2.Name = "panel2";
            panel2.Size = new Size(576, 45);
            panel2.TabIndex = 4;
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.BackColor = Color.Transparent;
            btnClose.Cursor = Cursors.Hand;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Font = new Font("Segoe UI", 14F);
            btnClose.ForeColor = Color.Transparent;
            btnClose.Location = new Point(518, 1);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(48, 41);
            btnClose.TabIndex = 2;
            btnClose.Text = "✖";
            toolTip1.SetToolTip(btnClose, "Close");
            btnClose.UseVisualStyleBackColor = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            label1.ForeColor = Color.White;
            label1.Location = new Point(2, 7);
            label1.Name = "label1";
            label1.Size = new Size(156, 25);
            label1.TabIndex = 3;
            label1.Text = "Battery Monitor";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // btnMinimizeToTray
            // 
            btnMinimizeToTray.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnMinimizeToTray.BackColor = Color.Transparent;
            btnMinimizeToTray.Cursor = Cursors.Hand;
            btnMinimizeToTray.FlatAppearance.BorderSize = 0;
            btnMinimizeToTray.FlatStyle = FlatStyle.Flat;
            btnMinimizeToTray.Font = new Font("Segoe UI Emoji", 14F, FontStyle.Bold);
            btnMinimizeToTray.ForeColor = Color.Transparent;
            btnMinimizeToTray.Location = new Point(413, 1);
            btnMinimizeToTray.Name = "btnMinimizeToTray";
            btnMinimizeToTray.Size = new Size(48, 40);
            btnMinimizeToTray.TabIndex = 0;
            btnMinimizeToTray.Text = "⏬";
            toolTip1.SetToolTip(btnMinimizeToTray, "Minimize to Tray");
            btnMinimizeToTray.UseVisualStyleBackColor = false;
            btnMinimizeToTray.Click += BtnMinimizeToTray_Click;
            // 
            // btnMinimize
            // 
            btnMinimize.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnMinimize.BackColor = Color.Transparent;
            btnMinimize.Cursor = Cursors.Hand;
            btnMinimize.FlatAppearance.BorderSize = 0;
            btnMinimize.FlatStyle = FlatStyle.Flat;
            btnMinimize.Font = new Font("Segoe UI", 14F);
            btnMinimize.ForeColor = Color.Transparent;
            btnMinimize.Location = new Point(467, 1);
            btnMinimize.Name = "btnMinimize";
            btnMinimize.Size = new Size(45, 40);
            btnMinimize.TabIndex = 1;
            btnMinimize.Text = "➖";
            toolTip1.SetToolTip(btnMinimize, "Minimize to Taskbar");
            btnMinimize.UseVisualStyleBackColor = false;
            // 
            // notifyIcon1
            // 
            notifyIcon1.Text = "notifyIcon1";
            notifyIcon1.Visible = true;
            // 
            // Monitor
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.GradientActiveCaption;
            ClientSize = new Size(577, 125);
            Controls.Add(panel1);
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            Name = "Monitor";
            Text = "Battery Monitor";
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxCharging).EndInit();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Timer timer1;
        private NotifyIcon notifyIcon1;
        private Label lblConfig;
        private ToolTip toolTip1;
        private Label lblAutoClose;
        private PictureBox pictureBoxCharging;
        private Label label1;
        private Panel panel2;
    }
}
