using System.Drawing;
using System.Windows.Forms;

namespace BatteryMonitor
{
    partial class Monitor
    {
        private Label lblBatteryPercent;
        private Label lblStatus;
        private ProgressBar progressBarBattery;
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
            lblBatteryPercent = new Label();
            lblStatus = new Label();
            progressBarBattery = new ProgressBar();
            lblConfigUpper = new Label();
            lblConfigLower = new Label();
            pictureBoxBattery = new PictureBox();
            panelTop = new Panel();
            btnClose = new Button();
            btnMinimize = new Button();
            labelTitle = new Label();
            toolTip1 = new ToolTip();

            SuspendLayout();

            // === Form ===
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(500, 250);
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            Name = "Monitor";
            Text = "Battery Monitor";

            // === Top Panel ===
            panelTop.Dock = DockStyle.Top;
            panelTop.Height = 40;
            panelTop.BackColor = Color.FromArgb(40, 120, 210);
            panelTop.Controls.Add(btnClose);
            panelTop.Controls.Add(btnMinimize);
            panelTop.Controls.Add(labelTitle);
            Controls.Add(panelTop);

            // === Title ===
            labelTitle.Text = "Battery Monitor";
            labelTitle.ForeColor = Color.White;
            labelTitle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            labelTitle.Location = new Point(10, 8);
            labelTitle.AutoSize = true;

            // === Close Button ===
            btnClose.Text = "✖";
            btnClose.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btnClose.ForeColor = Color.White;
            btnClose.BackColor = Color.Transparent;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Location = new Point(460, 5);
            btnClose.Size = new Size(30, 30);
            btnClose.Click += BtnClose_Click;

            // === Minimize Button ===
            btnMinimize.Text = "➖";
            btnMinimize.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btnMinimize.ForeColor = Color.White;
            btnMinimize.BackColor = Color.Transparent;
            btnMinimize.FlatStyle = FlatStyle.Flat;
            btnMinimize.FlatAppearance.BorderSize = 0;
            btnMinimize.Location = new Point(420, 5);
            btnMinimize.Size = new Size(30, 30);
            btnMinimize.Click += BtnMinimize_Click;

            // === Battery Icon ===
            pictureBoxBattery.Size = new Size(80, 120);
            pictureBoxBattery.Location = new Point(30, 70);
            pictureBoxBattery.SizeMode = PictureBoxSizeMode.StretchImage;
            //pictureBoxBattery.Image = Properties.Resources.battery_full;
            Controls.Add(pictureBoxBattery);

            // === Battery Percentage Label ===
            lblBatteryPercent.Font = new Font("Segoe UI", 28, FontStyle.Bold);
            lblBatteryPercent.ForeColor = Color.Green;
            lblBatteryPercent.Text = "100%";
            lblBatteryPercent.AutoSize = true;
            lblBatteryPercent.Location = new Point(130, 70);
            Controls.Add(lblBatteryPercent);

            // === Status Label ===
            lblStatus.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            lblStatus.ForeColor = Color.Green;
            lblStatus.Text = "Status: High";
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(130, 120);
            Controls.Add(lblStatus);

            // === Progress Bar ===
            progressBarBattery.Size = new Size(320, 20);
            progressBarBattery.Location = new Point(130, 160);
            progressBarBattery.Value = 100;
            Controls.Add(progressBarBattery);

            // === Config Labels ===
            lblConfigUpper.Text = "Upper limit: 90%";
            lblConfigUpper.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            lblConfigUpper.AutoSize = true;
            lblConfigUpper.Location = new Point(130, 200);
            Controls.Add(lblConfigUpper);

            lblConfigLower.Text = "Lower limit: 15%";
            lblConfigLower.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            lblConfigLower.AutoSize = true;
            lblConfigLower.Location = new Point(300, 200);
            Controls.Add(lblConfigLower);

            ResumeLayout(false);
        }
    }
}
