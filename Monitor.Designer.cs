using System.Drawing;
using System.Windows.Forms;

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
            btnClose = new Button();
            btnMinimize = new Button();
            labelTitle = new Label();
            toolTip1 = new ToolTip(components);
            chkAlert = new CheckBox();
            gradientProgressBar1 = new BatteryMonitor.Controls.GradientProgressBar();
            panelConfig = new Panel();
            chkViewConfig = new CheckBox();
            configSlideTimer = new System.Windows.Forms.Timer(components);
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
            pictureBoxBattery.Image = Properties.Resources.FullCharge;
            pictureBoxBattery.InitialImage = null;
            pictureBoxBattery.Location = new Point(42, 95);
            pictureBoxBattery.Name = "pictureBoxBattery";
            pictureBoxBattery.Size = new Size(82, 83);
            pictureBoxBattery.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBoxBattery.TabIndex = 1;
            pictureBoxBattery.TabStop = false;
            // 
            // panelTop
            // 
            panelTop.BackColor = Color.FromArgb(40, 120, 210);
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
            btnMinimize.Location = new Point(420, 5);
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
            // chkAlert
            // 
            chkAlert.AutoSize = true;
            chkAlert.Location = new Point(359, 46);
            chkAlert.Name = "chkAlert";
            chkAlert.Size = new Size(92, 19);
            chkAlert.TabIndex = 7;
            chkAlert.Text = "Mute Alerts?";
            toolTip1.SetToolTip(chkAlert, "Mute alert sound");
            chkAlert.UseVisualStyleBackColor = true;
            chkAlert.CheckedChanged += chkAlert_CheckedChanged;
            // 
            // gradientProgressBar1
            // 
            gradientProgressBar1.GradientEnd = Color.MediumPurple;
            gradientProgressBar1.GradientStart = Color.LightSeaGreen;
            gradientProgressBar1.Location = new Point(130, 148);
            gradientProgressBar1.Maximum = 100;
            gradientProgressBar1.Name = "gradientProgressBar1";
            gradientProgressBar1.Size = new Size(321, 23);
            gradientProgressBar1.TabIndex = 3;
            gradientProgressBar1.Text = "gradientProgressBar1";
            gradientProgressBar1.Value = 0;
            // 
            // panelConfig
            // 
            panelConfig.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panelConfig.BackColor = Color.Beige;
            panelConfig.BorderStyle = BorderStyle.FixedSingle;
            panelConfig.Controls.Add(lblConfigUpper);
            panelConfig.Controls.Add(lblConfigLower);
            panelConfig.Location = new Point(130, 195);
            panelConfig.Name = "panelConfig";
            panelConfig.Size = new Size(321, 0);
            panelConfig.TabIndex = 8;
            // 
            // chkViewConfig
            // 
            chkViewConfig.AutoSize = true;
            chkViewConfig.Location = new Point(359, 65);
            chkViewConfig.Name = "chkViewConfig";
            chkViewConfig.Size = new Size(128, 19);
            chkViewConfig.TabIndex = 0;
            chkViewConfig.Text = "View Configuration";
            chkViewConfig.UseVisualStyleBackColor = true;
            // 
            // Monitor
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Honeydew;
            ClientSize = new Size(500, 250);
            Controls.Add(chkViewConfig);
            Controls.Add(panelConfig);
            Controls.Add(chkAlert);
            Controls.Add(gradientProgressBar1);
            Controls.Add(panelTop);
            Controls.Add(pictureBoxBattery);
            Controls.Add(lblBatteryPercent);
            Controls.Add(lblStatus);
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
        private System.ComponentModel.IContainer components;
        private Controls.GradientProgressBar gradientProgressBar1;
        private CheckBox chkAlert;
        private Panel panelConfig;
        private CheckBox chkViewConfig;
        private System.Windows.Forms.Timer configSlideTimer;
    }
}
