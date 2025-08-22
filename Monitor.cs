using System;
using System.Drawing;
using System.Windows.Forms;


namespace BatteryMonitor
{
    public partial class Monitor : Form
    {
        private System.Windows.Forms.Timer timer;
        private int upperThreshold = 90; // default
        private int lowerThreshold = 15; // default

        public Monitor()
        {
            InitializeComponent();

            // Start timer
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 2000; // 2 seconds
            timer.Tick += Timer_Tick;
            timer.Start();

            UpdateBatteryStatus();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateBatteryStatus();
        }

        private void UpdateBatteryStatus()
        {
            PowerStatus powerStatus = SystemInformation.PowerStatus;
            float batteryLevel = powerStatus.BatteryLifePercent * 100;
            BatteryChargeStatus chargeStatus = powerStatus.BatteryChargeStatus;

            lblBatteryPercent.Text = $"{batteryLevel:F0}%";
            progressBarBattery.Value = Math.Min((int)batteryLevel, 100);

            if (batteryLevel >= upperThreshold)
            {
                lblStatus.Text = "Status: High";
                lblStatus.ForeColor = Color.Green;
                lblBatteryPercent.ForeColor = Color.Green;
               // pictureBoxBattery.Image = Properties.Resources.battery_full;
            }
            else if (batteryLevel <= lowerThreshold)
            {
                lblStatus.Text = "Status: Low";
                lblStatus.ForeColor = Color.Red;
                lblBatteryPercent.ForeColor = Color.Red;
               // pictureBoxBattery.Image = Properties.Resources.battery_low;
            }
            else
            {
                lblStatus.Text = "Status: Medium";
                lblStatus.ForeColor = Color.Orange;
                lblBatteryPercent.ForeColor = Color.Orange;
               // pictureBoxBattery.Image = Properties.Resources.battery_half;
            }

            // Charging overrides
            if (chargeStatus.HasFlag(BatteryChargeStatus.Charging))
            {
                lblStatus.Text = "Status: Charging";
                lblStatus.ForeColor = Color.Blue;
            //    pictureBoxBattery.Image = Properties.Resources.battery_charging;
            }

            // Update thresholds
            lblConfigUpper.Text = $"Upper limit: {upperThreshold}%";
            lblConfigLower.Text = $"Lower limit: {lowerThreshold}%";
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void BtnMinimize_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }
    }
}
