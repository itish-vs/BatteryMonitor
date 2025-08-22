using Microsoft.Extensions.Configuration;
using System.Drawing.Drawing2D;
using System.Media;
using System.Runtime.InteropServices;

namespace BatteryMonitor
{
    public partial class Monitor : Form
    {
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO
        {
            public UInt32 cbSize;
            public IntPtr hwnd;
            public UInt32 dwFlags;
            public UInt32 uCount;
            public UInt32 dwTimeout;
        }

        private const UInt32 FLASHW_ALL = 3;
        private const UInt32 FLASHW_TIMERNOFG = 12;

        private int upperAlertCounter = 0;
        private int lowerAlertCounter = 0;

        // Overlay controls
        private Panel startupOverlay;
        private Label startupLabel;
        private System.Windows.Forms.Timer overlayTimer;
        private int overlayAlpha = 180;

        // Alert sound controls
        private SoundPlayer? alertPlayer;
        private System.Windows.Forms.Timer? alertSoundTimer; // handles play/delay loop
        private DateTime? alertSoundStartTime;
        private int alertPlayDelayMs = 2000; // 1 second delay between plays
        private int alertTotalDurationMs = 30 * 1000; // 15 seconds total
        private string? currentAlertSoundFile;               // currently playing sound path
        private bool isAlertCompleted = false;

        public Monitor()
        {
            InitializeComponent();

            // === CREATE STARTUP OVERLAY ===
            CreateStartupOverlay();

            try
            {
                this.StartPosition = FormStartPosition.CenterScreen;
                LoadSettings();
                ValidateSettings();

                // Setup form icon safely
                string appPath = AppDomain.CurrentDomain.BaseDirectory;
                string iconPath = Path.Combine(appPath, "BatteryIcon.ico");
                if (File.Exists(iconPath))
                {
                    this.Icon = new Icon(iconPath);
                }

                // Setup NotifyIcon
                trayIcon = new NotifyIcon();
                trayIcon.Icon = this.Icon;
                trayIcon.Visible = true;
                trayIcon.Text = "Battery Monitor";
                trayIcon.DoubleClick += (s, e) =>
                {
                    this.Show();
                    this.WindowState = FormWindowState.Normal;
                    this.BringToFront();
                    this.Activate();
                };
                btnMinimize.Click += BtnMinimize_Click;
                btnClose.Click += BtnClose_Click;

                panel1.MouseDown += panel1_MouseDown;
                panel1.MouseUp += panel1_MouseUp;
                btnClose.MouseEnter += (s, e) => btnClose.BackColor = Color.Red;
                btnClose.MouseLeave += (s, e) => btnClose.BackColor = Color.Transparent;
                btnMinimizeToTray.MouseEnter += (s, e) => btnMinimizeToTray.ForeColor = Color.DarkGreen;
                btnMinimizeToTray.MouseEnter += (s, e) => btnMinimizeToTray.BackColor = Color.Transparent;
                btnMinimizeToTray.MouseLeave += (s, e) => btnMinimizeToTray.ForeColor = Color.Transparent;
                btnMinimizeToTray.MouseLeave += (s, e) => btnMinimizeToTray.BackColor = Color.Transparent;
                btnMinimize.MouseEnter += (s, e) => btnMinimize.ForeColor = Color.DarkBlue;
                btnMinimize.MouseEnter += (s, e) => btnMinimize.BackColor = Color.Transparent;
                btnMinimize.MouseLeave += (s, e) => btnMinimize.ForeColor = Color.Transparent;
                btnMinimize.MouseLeave += (s, e) => btnMinimize.BackColor = Color.Transparent;

                ContextMenuStrip menu = new ContextMenuStrip();
                menu.Items.Add("Exit", null, Exit);
                trayIcon.ContextMenuStrip = menu;

                // Setup Timer
                timer = new System.Windows.Forms.Timer();
                timer.Interval = 5 * 1000; // 5 seconds
                timer.Tick += Timer_Tick;
                timer.Start();

                foreach (Control ctrl in panel1.Controls)
                {
                    if (ctrl is Label || ctrl is PictureBox)
                    {
                        ctrl.MouseDown += panel1_MouseDown;
                        ctrl.MouseUp += panel1_MouseUp;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorOverlay(ex.Message);
            }
        }

        // ================= OVERLAY METHODS =================
        private async void CreateStartupOverlay()
        {
            startupOverlay = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(overlayAlpha, 30, 30, 30),
                Visible = true,
                BorderStyle = BorderStyle.None
            };

            startupLabel = new Label
            {
                Text = "Initializing...",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            startupOverlay.Controls.Add(startupLabel);
            this.Controls.Add(startupOverlay);
            startupOverlay.BringToFront();
            await Task.Delay(500);
        }

        private async void StartOverlayFadeOut()
        {
            if (overlayTimer != null)
            {
                overlayTimer.Stop();
                overlayTimer.Dispose();
                overlayTimer = null;
            }

            overlayTimer = new System.Windows.Forms.Timer { Interval = 50 }; // fade speed
            overlayTimer.Tick += (s, e) =>
            {
                overlayAlpha -= 15;
                if (overlayAlpha <= 0)
                {
                    overlayTimer.Stop();
                    startupOverlay.Visible = false;
                }
                else
                {
                    startupOverlay.BackColor = Color.FromArgb(overlayAlpha, 30, 30, 30);
                }
            };
            overlayTimer.Start();
            await Task.Delay(500);
        }

        private async void ShowErrorOverlay(string message)
        {
            startupLabel.Text = $"Error: {message}";
            startupOverlay.BackColor = Color.FromArgb(200, 120, 0, 0);
            startupOverlay.Visible = true;
            startupOverlay.BringToFront();

            await Task.Delay(5000);
            Exit(null, EventArgs.Empty);
        }
        // ====================================================

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Cursor = Cursors.SizeAll;
                ReleaseCapture();
                SendMessage(this.Handle, 0xA1, 0x2, 0);
                this.Cursor = Cursors.Default;
            }
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Default;
        }

        private void LoadSettings()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string resourcePath = Path.Combine(baseDir, "resources", "appsettings.json");
            string rootPath = Path.Combine(baseDir, "appsettings.json");
            string configPath = File.Exists(resourcePath) ? resourcePath : rootPath;

            if (!File.Exists(configPath))
                throw new FileNotFoundException($"appsettings.json not found in resources or root folder. Checked:\n{resourcePath}\n{rootPath}");

            var config = new ConfigurationBuilder()
                .SetBasePath(baseDir)
                .AddJsonFile(configPath, optional: false, reloadOnChange: true)
                .Build();

            settings = config.GetSection("BatteryMonitor").Get<BatterySettings>()!;
            if (settings == null)
            {
                throw new InvalidOperationException("BatteryMonitor section is missing or invalid in appsettings.json.");
            }
            else
            {
                lblConfig.Text = $"Configuration - Upper limit: {settings.UpperThreshold}%, Lower limit: {settings.LowerThreshold}%";
            }
        }

        private void ValidateSettings()
        {
            if (settings.UpperThreshold <= 0 || settings.UpperThreshold > 100)
                throw new InvalidOperationException($"UpperThreshold {settings.UpperThreshold} is out of valid range (1–100).");

            if (settings.LowerThreshold < 0 || settings.LowerThreshold >= 100)
                throw new InvalidOperationException($"LowerThreshold {settings.LowerThreshold} is out of valid range (0–99).");

            if (settings.LowerThreshold >= settings.UpperThreshold)
                throw new InvalidOperationException($"LowerThreshold {settings.LowerThreshold} must be less than UpperThreshold {settings.UpperThreshold}.");

            if (string.IsNullOrWhiteSpace(settings.FullBatterySound))
                throw new InvalidOperationException("FullBatterySound file name is missing or empty.");

            if (string.IsNullOrWhiteSpace(settings.LowBatterySound))
                throw new InvalidOperationException("LowBatterySound file name is missing or empty.");

            string basePath = AppDomain.CurrentDomain.BaseDirectory;

            string fullSoundPath = Path.Combine(basePath, settings.FullBatterySound);
            string lowSoundPath = Path.Combine(basePath, settings.LowBatterySound);

            if (!File.Exists(fullSoundPath))
                throw new FileNotFoundException($"FullBatterySound file not found: {fullSoundPath}");

            if (!File.Exists(lowSoundPath))
                throw new FileNotFoundException($"LowBatterySound file not found: {lowSoundPath}");
        }

        private async void Timer_Tick(object sender, EventArgs e)
        {
            int iErrorCount = 0;
            try
            {
                PowerStatus pwr = SystemInformation.PowerStatus;
                float batteryLevel = pwr.BatteryLifePercent * 100;
                BatteryChargeStatus chargeStatus = pwr.BatteryChargeStatus;
                // clear the old auto-close label usage
                lblAutoClose.Text = string.Empty;

                if (chargeStatus == BatteryChargeStatus.NoSystemBattery)
                {
                    trayIcon.ShowBalloonTip(3000, "Battery Monitor", "No Battery Detected!", ToolTipIcon.Error);
                    lblStatus.Text = "No Battery Detected!";
                    throw new Exception("No Battery Detected!");
                }


                lblStatus.ForeColor = batteryLevel switch
                {
                    var level when level > settings.UpperThreshold || level > 61f => Color.DarkGreen,
                    var level when level > 25f && level < 60f => Color.DarkOrange,
                    _ => Color.Red
                };

                pictureBoxCharging.Visible = chargeStatus.HasFlag(BatteryChargeStatus.Charging);

                string status = chargeStatus.ToString() == "0" ? "Discharging" : chargeStatus.ToString();
                string strDispText = $"Battery: {batteryLevel:F0}% | Status: {status}";
                trayIcon.Text = strDispText;
                this.Text = $"{batteryLevel:F0}% | {status}";
                lblStatus.Text = strDispText;

                // Upper alert: show UI and play full battery sound (stop after 15s)
                if (batteryLevel >= settings.UpperThreshold && chargeStatus.HasFlag(BatteryChargeStatus.Charging))
                {
                    upperAlertCounter++;
                    lowerAlertCounter = 0;

                    trayIcon.ShowBalloonTip(3000, "Battery Monitor", $"Battery is fully charged! {batteryLevel:F0}%", ToolTipIcon.Info);
                    lblStatus.Text = $"Battery is fully charged! {batteryLevel:F0}%";
                    this.Text = $"Full Charged! {batteryLevel:F0}%";
                    this.Show();
                    this.WindowState = FormWindowState.Normal;
                    this.BringToFront();
                    this.Activate();
                    this.Focus();
                    ForceAttention();

                    // start or keep a limited-duration alert sound (does not close the app)
                    StartAlertSound(settings.FullBatterySound);
                    isAlertCompleted = true;
                    // do not auto close the app anymore — removed Application.Exit()
                }
                // Lower alert: show UI and play low battery sound (stop after 15s)
                else if (batteryLevel <= settings.LowerThreshold && !chargeStatus.HasFlag(BatteryChargeStatus.Charging))
                {
                    lowerAlertCounter++;
                    upperAlertCounter = 0;

                    trayIcon.ShowBalloonTip(3000, "Battery Monitor", $"Low Battery! {batteryLevel:F0}%", ToolTipIcon.Warning);
                    lblStatus.Text = $"Low Battery! {batteryLevel:F0}%";
                    this.Text = $"Low Battery! {batteryLevel:F0}%";
                    this.Show();
                    this.WindowState = FormWindowState.Normal;
                    this.BringToFront();
                    this.Activate();
                    this.Focus();
                    ForceAttention();

                    // start or keep a limited-duration alert sound (does not close the app)
                    StartAlertSound(settings.LowBatterySound);
                    isAlertCompleted = true;
                }
                
                else
                {
                    // reset counters and stop sound if no alert
                    upperAlertCounter = 0;
                    lowerAlertCounter = 0;
                    isAlertCompleted = false;
                    StopAlertSound();
                }
                await Task.Delay(500);
            }
            catch (Exception ex)
            {
                iErrorCount++;
                ShowErrorOverlay(ex.Message);
            }
            finally
            {
                // Ensure overlay is hidden after first tick
                if (startupOverlay.Visible && iErrorCount == 0)
                {
                    StartOverlayFadeOut();
                }
            }
        }

        /// <summary>
        /// Start playing the alert sound with a delay between each play, for up to 15 seconds.
        /// </summary>
        private void StartAlertSound(string? fileName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileName))
                    return;
                if (isAlertCompleted)
                    return;
                string soundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);

                if (!File.Exists(soundPath))
                    return;

                // If same sound is playing and timer active, do not restart
                if (alertSoundTimer != null && alertSoundTimer.Enabled
                    && string.Equals(currentAlertSoundFile, soundPath, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                // Stop any previous
                StopAlertSound();

                alertPlayer = new SoundPlayer(soundPath);
                currentAlertSoundFile = soundPath;
                alertSoundStartTime = DateTime.Now;

                // Timer for play/delay loop
                if (alertSoundTimer == null)
                {
                    alertSoundTimer = new System.Windows.Forms.Timer();
                    alertSoundTimer.Tick += AlertSoundTimer_Tick;
                }
                alertSoundTimer.Interval = 10; // Start immediately
                alertSoundTimer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sound error: {ex.Message}");
            }
        }

        private void AlertSoundTimer_Tick(object? sender, EventArgs e)
        {
            if (alertPlayer == null || alertSoundStartTime == null)
            {
                StopAlertSound();
                return;
            }

            // Check if total duration exceeded
            if ((DateTime.Now - alertSoundStartTime.Value).TotalMilliseconds >= alertTotalDurationMs)
            {
                StopAlertSound();
                return;
            }

            // Play sound once
            try
            {
                alertPlayer.Stop(); // Ensure not overlapping
                alertPlayer.Play();
            }
            catch { /* ignore */ }

            // Set timer for next play (sound duration + delay)
            alertSoundTimer!.Interval = alertPlayDelayMs;
        }

        private void StopAlertSound()
        {
            try
            {
                if (alertPlayer != null)
                {
                    try { alertPlayer.Stop(); }
                    catch { /* ignore */ }
                    try { alertPlayer.Dispose(); }
                    catch { /* ignore */ }
                    alertPlayer = null;
                }

                if (alertSoundTimer != null)
                {
                    try { alertSoundTimer.Stop(); }
                    catch { /* ignore */ }
                }

                alertSoundStartTime = null;
                currentAlertSoundFile = null;
            }
            catch { /* swallow any cleanup exceptions */ }
        }


        private void Exit(object? sender, EventArgs e)
        {
            StopAlertSound();
            if (trayIcon != null) trayIcon.Visible = false;
            this.Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            StopAlertSound();
            if (trayIcon != null) trayIcon.Visible = false;
            base.OnFormClosing(e);
        }

        private void BtnMinimizeToTray_Click(object sender, EventArgs e)
        {
            this.Hide();
            trayIcon.ShowBalloonTip(1000, "Battery Monitor", "Application minimized to tray.", ToolTipIcon.Info);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            int borderWidth = 4;
            Color borderColor = Color.FromArgb(200, 20, 20, 20);

            using (Pen pen = new Pen(borderColor, borderWidth))
            {
                pen.Alignment = PenAlignment.Inset;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle rect = this.ClientRectangle;
                rect.Width -= 1;
                rect.Height -= 1;
                e.Graphics.DrawRectangle(pen, rect);
            }
        }

        private void ForceAttention()
        {
            IntPtr hWnd = this.Handle;
            SetForegroundWindow(hWnd);

            FLASHWINFO fw = new FLASHWINFO();
            fw.cbSize = Convert.ToUInt32(Marshal.SizeOf(fw));
            fw.hwnd = hWnd;
            fw.dwFlags = FLASHW_ALL | FLASHW_TIMERNOFG;
            fw.uCount = UInt32.MaxValue;
            fw.dwTimeout = 0;

            FlashWindowEx(ref fw);
        }

        private BatterySettings settings = new();
        private NotifyIcon? trayIcon;
        private System.Windows.Forms.Timer? timer;

        private void BtnMinimize_Click(object? sender, EventArgs e) => this.WindowState = FormWindowState.Minimized;
        private void BtnClose_Click(object? sender, EventArgs e) => Application.Exit();

        

    }

    public class BatterySettings
    {
        public float UpperThreshold { get; set; } = 0;
        public float LowerThreshold { get; set; } = 0;
        public string FullBatterySound { get; set; } = string.Empty;
        public string LowBatterySound { get; set; } = string.Empty;
    }
}
