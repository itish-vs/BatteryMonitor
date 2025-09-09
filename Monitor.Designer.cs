using System.Drawing;
using System.Windows.Forms;
using static JCS.ToggleSwitch;
namespace BatteryMonitor
{
    partial class Monitor
    {
        private Label lblBatteryPercent;
        private Label lblStatus;
        private Label lblConfigUpper;
        private Label lblConfigLower;
        private PictureBox pictureBoxBattery;
        private Panel panelTop;
        private Button btnClose;
        private Button btnMinimize;
        private Label labelTitle;
        private ToolTip toolTip1;
        private System.ComponentModel.IContainer components;
        private Controls.GradientProgressBar gradientProgressBar1;
        private Panel panelConfig;
        private System.Windows.Forms.Timer configSlideTimer;
        private Button btnTray;
        private NotifyIcon notifyIcon1;
        private JCS.ToggleSwitch tglMute;
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Monitor));
            lblBatteryPercent = new Label();
            lblStatus = new Label();
            lblConfigUpper = new Label();
            lblConfigLower = new Label();
            pictureBoxBattery = new PictureBox();
            panelTop = new Panel();
            btnTray = new Button();
            btnClose = new Button();
            btnMinimize = new Button();
            labelTitle = new Label();
            toolTip1 = new ToolTip(components);
            button1 = new Button();
            panelConfig = new Panel();
            tglMute = new JCS.ToggleSwitch();
            configSlideTimer = new System.Windows.Forms.Timer(components);
            gradientProgressBar1 = new BatteryMonitor.Controls.GradientProgressBar();
            notifyIcon1 = new NotifyIcon(components);
            label1 = new Label();
            tglConfig = new JCS.ToggleSwitch();
            lblConfig = new Label();
            lblBy = new Label();
            lblTimeRemaining = new Label();
            lblBatteryHealth = new Label();
            ((System.ComponentModel.ISupportInitialize)pictureBoxBattery).BeginInit();
            panelTop.SuspendLayout();
            panelConfig.SuspendLayout();
            SuspendLayout();
            // 
            // lblBatteryPercent
            // 
            lblBatteryPercent.AutoSize = true;
            lblBatteryPercent.Font = new Font("Segoe UI", 28F, FontStyle.Bold);
            lblBatteryPercent.ForeColor = Color.Green;
            lblBatteryPercent.Location = new Point(129, 69);
            lblBatteryPercent.Name = "lblBatteryPercent";
            lblBatteryPercent.Size = new Size(121, 51);
            lblBatteryPercent.TabIndex = 2;
            lblBatteryPercent.Text = "100%";
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblStatus.ForeColor = Color.Green;
            lblStatus.Location = new Point(130, 120);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(120, 25);
            lblStatus.TabIndex = 3;
            lblStatus.Text = "Status: High";
            // 
            // lblConfigUpper
            // 
            lblConfigUpper.AutoSize = true;
            lblConfigUpper.Font = new Font("Segoe UI", 10F);
            lblConfigUpper.Location = new Point(3, 3);
            lblConfigUpper.Name = "lblConfigUpper";
            lblConfigUpper.Size = new Size(111, 19);
            lblConfigUpper.TabIndex = 5;
            lblConfigUpper.Text = "Upper limit: 90%";
            // 
            // lblConfigLower
            // 
            lblConfigLower.AutoSize = true;
            lblConfigLower.Font = new Font("Segoe UI", 10F);
            lblConfigLower.Location = new Point(206, 3);
            lblConfigLower.Name = "lblConfigLower";
            lblConfigLower.Size = new Size(110, 19);
            lblConfigLower.TabIndex = 6;
            lblConfigLower.Text = "Lower limit: 15%";
            // 
            // pictureBoxBattery
            // 
            pictureBoxBattery.BackgroundImageLayout = ImageLayout.Center;
            pictureBoxBattery.InitialImage = null;
            pictureBoxBattery.Location = new Point(5, 73);
            pictureBoxBattery.Name = "pictureBoxBattery";
            pictureBoxBattery.Size = new Size(124, 97);
            pictureBoxBattery.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxBattery.TabIndex = 1;
            pictureBoxBattery.TabStop = false;
            pictureBoxBattery.Paint += pictureBoxBattery_Paint;
            // 
            // panelTop
            // 
            panelTop.BackColor = Color.FromArgb(40, 120, 210);
            panelTop.Controls.Add(btnTray);
            panelTop.Controls.Add(btnClose);
            panelTop.Controls.Add(btnMinimize);
            panelTop.Controls.Add(labelTitle);
            panelTop.Dock = DockStyle.Top;
            panelTop.Location = new Point(0, 0);
            panelTop.Name = "panelTop";
            panelTop.Size = new Size(500, 40);
            panelTop.TabIndex = 0;
            panelTop.MouseDown += panelTop_MouseDown;
            panelTop.MouseUp += panelTop_MouseUp;
            // 
            // btnTray
            // 
            btnTray.BackColor = Color.Transparent;
            btnTray.FlatAppearance.BorderSize = 0;
            btnTray.FlatStyle = FlatStyle.Flat;
            btnTray.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnTray.ForeColor = Color.White;
            btnTray.Location = new Point(372, 5);
            btnTray.Name = "btnTray";
            btnTray.Size = new Size(30, 30);
            btnTray.TabIndex = 3;
            btnTray.Text = "🔽";
            toolTip1.SetToolTip(btnTray, "Minimize to Tray");
            btnTray.UseVisualStyleBackColor = false;
            btnTray.Click += btnTray_Click;
            // 
            // btnClose
            // 
            btnClose.BackColor = Color.Transparent;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnClose.ForeColor = Color.White;
            btnClose.Location = new Point(460, 5);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(30, 30);
            btnClose.TabIndex = 0;
            btnClose.Text = "✖";
            btnClose.UseVisualStyleBackColor = false;
            btnClose.Click += BtnClose_Click;
            // 
            // btnMinimize
            // 
            btnMinimize.BackColor = Color.Transparent;
            btnMinimize.FlatAppearance.BorderSize = 0;
            btnMinimize.FlatStyle = FlatStyle.Flat;
            btnMinimize.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnMinimize.ForeColor = Color.White;
            btnMinimize.Location = new Point(416, 5);
            btnMinimize.Name = "btnMinimize";
            btnMinimize.Size = new Size(30, 30);
            btnMinimize.TabIndex = 1;
            btnMinimize.Text = "➖";
            btnMinimize.UseVisualStyleBackColor = false;
            btnMinimize.Click += BtnMinimize_Click;
            // 
            // labelTitle
            // 
            labelTitle.AutoSize = true;
            labelTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            labelTitle.ForeColor = Color.White;
            labelTitle.Location = new Point(4, 10);
            labelTitle.Name = "labelTitle";
            labelTitle.Size = new Size(132, 21);
            labelTitle.TabIndex = 2;
            labelTitle.Text = "Battery Monitor";
            // 
            // button1
            // 
            button1.BackColor = Color.Transparent;
            button1.Cursor = Cursors.Hand;
            button1.FlatAppearance.BorderSize = 0;
            button1.FlatAppearance.MouseDownBackColor = Color.Transparent;
            button1.FlatAppearance.MouseOverBackColor = Color.Transparent;
            button1.FlatStyle = FlatStyle.Flat;
            button1.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            button1.ForeColor = Color.Blue;
            button1.Location = new Point(69, 202);
            button1.Name = "button1";
            button1.Size = new Size(19, 23);
            button1.TabIndex = 18;
            button1.Text = "?";
            button1.TextAlign = ContentAlignment.MiddleLeft;
            button1.TextImageRelation = TextImageRelation.TextAboveImage;
            toolTip1.SetToolTip(button1, "About");
            button1.UseVisualStyleBackColor = false;
            button1.Click += button1_Click;
            // 
            // panelConfig
            // 
            panelConfig.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panelConfig.BackColor = Color.Beige;
            panelConfig.BorderStyle = BorderStyle.FixedSingle;
            panelConfig.Controls.Add(lblConfigUpper);
            panelConfig.Controls.Add(lblConfigLower);
            panelConfig.Location = new Point(134, 190);
            panelConfig.Name = "panelConfig";
            panelConfig.Size = new Size(324, 0);
            panelConfig.TabIndex = 8;
            // 
            // tglMute
            // 
            tglMute.Location = new Point(450, 45);
            tglMute.Name = "tglMute";
            tglMute.OffFont = new Font("Segoe UI", 9F);
            tglMute.OnFont = new Font("Segoe UI", 9F);
            tglMute.Size = new Size(40, 20);
            tglMute.Style = ToggleSwitchStyle.IOS5;
            tglMute.TabIndex = 10;
            tglMute.CheckedChanged += tglMute_CheckedChanged;
            // 
            // gradientProgressBar1
            // 
            gradientProgressBar1.GradientEnd = Color.MediumPurple;
            gradientProgressBar1.GradientStart = Color.LightSeaGreen;
            gradientProgressBar1.Location = new Point(134, 148);
            gradientProgressBar1.Maximum = 100;
            gradientProgressBar1.Name = "gradientProgressBar1";
            gradientProgressBar1.Size = new Size(324, 23);
            gradientProgressBar1.TabIndex = 9;
            gradientProgressBar1.Text = "gradientProgressBar1";
            gradientProgressBar1.Value = 0;
            // 
            // notifyIcon1
            // 
            notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon1.Text = "notifyIcon1";
            notifyIcon1.Visible = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(368, 48);
            label1.Name = "label1";
            label1.Size = new Size(73, 15);
            label1.TabIndex = 11;
            label1.Text = "Mute Alerts?";
            // 
            // tglConfig
            // 
            tglConfig.Location = new Point(450, 69);
            tglConfig.Name = "tglConfig";
            tglConfig.OffFont = new Font("Segoe UI", 9F);
            tglConfig.OnFont = new Font("Segoe UI", 9F);
            tglConfig.Size = new Size(40, 20);
            tglConfig.Style = ToggleSwitchStyle.IOS5;
            tglConfig.TabIndex = 12;
            tglConfig.CheckedChanged += tglConfig_CheckedChanged;
            // 
            // lblConfig
            // 
            lblConfig.AutoSize = true;
            lblConfig.Location = new Point(368, 71);
            lblConfig.Name = "lblConfig";
            lblConfig.Size = new Size(71, 15);
            lblConfig.TabIndex = 13;
            lblConfig.Text = "View Config";
            // 
            // lblBy
            // 
            lblBy.AutoSize = true;
            lblBy.Font = new Font("Segoe UI", 7F, FontStyle.Bold);
            lblBy.Location = new Point(7, 210);
            lblBy.Name = "lblBy";
            lblBy.Size = new Size(68, 12);
            lblBy.TabIndex = 14;
            lblBy.Text = "©Itish Nigam";
            lblBy.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblTimeRemaining
            // 
            lblTimeRemaining.AutoSize = true;
            lblTimeRemaining.Location = new Point(134, 173);
            lblTimeRemaining.Name = "lblTimeRemaining";
            lblTimeRemaining.Size = new Size(121, 15);
            lblTimeRemaining.TabIndex = 15;
            lblTimeRemaining.Text = "Time Remaining: N/A";
            // 
            // lblBatteryHealth
            // 
            lblBatteryHealth.AutoSize = true;
            lblBatteryHealth.Location = new Point(345, 173);
            lblBatteryHealth.Name = "lblBatteryHealth";
            lblBatteryHealth.Size = new Size(110, 15);
            lblBatteryHealth.TabIndex = 16;
            lblBatteryHealth.Text = "Battery Health: N/A";
            // 
            // Monitor
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FloralWhite;
            ClientSize = new Size(500, 234);
            Controls.Add(lblBatteryHealth);
            Controls.Add(lblTimeRemaining);
            Controls.Add(lblBy);
            Controls.Add(lblConfig);
            Controls.Add(tglConfig);
            Controls.Add(label1);
            Controls.Add(gradientProgressBar1);
            Controls.Add(lblBatteryPercent);
            Controls.Add(lblStatus);
            Controls.Add(panelConfig);
            Controls.Add(panelTop);
            Controls.Add(pictureBoxBattery);
            Controls.Add(tglMute);
            Controls.Add(button1);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.None;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Monitor";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Battery Monitor";
            ((System.ComponentModel.ISupportInitialize)pictureBoxBattery).EndInit();
            panelTop.ResumeLayout(false);
            panelTop.PerformLayout();
            panelConfig.ResumeLayout(false);
            panelConfig.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
        private Label label1;
        private JCS.ToggleSwitch tglConfig;
        private Label lblConfig;
        private Label lblBy;
        private Label lblTimeRemaining;
        private Label lblBatteryHealth;
        private Button button1;
    }
}
