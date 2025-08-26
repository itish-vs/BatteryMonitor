using Microsoft.Extensions.Configuration;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Media;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static JCS.ToggleSwitch;

namespace BatteryMonitor
{
    /// <summary>
    /// Main monitoring form
    /// </summary>
    public partial class Monitor : Form
    {
        #region Fields

        // Timers and settings
        private System.Windows.Forms.Timer? timer;
        private BatterySettings settings = new();

        // Win32 API imports
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
        private static int irndradius = 20;
        private NotifyIcon? trayIcon;
        // Overlay controls
        private Panel? startupOverlay;
        private Label? startupLabel;
        private System.Windows.Forms.Timer? overlayTimer;
        private int overlayAlpha = 180;

        // Alert sound controls
        private SoundPlayer? alertPlayer;
        private System.Windows.Forms.Timer? alertSoundTimer; // handles play/delay loop
        private DateTime? alertSoundStartTime;
        private int alertPlayDelayMs = 2000; // 2 seconds delay between plays
        private int alertTotalDurationMs = 30 * 1000; // 30 seconds total
        private string? currentAlertSoundFile; // currently playing sound path
        private bool isAlertCompleted = false;

        // Config panel sliding
        private int configPanelTargetHeight = 30; // Adjust as needed
        private bool configPanelExpanding = false;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public Monitor()
        {
            try
            {
                InitializeComponent();
                this.StartPosition = FormStartPosition.CenterScreen;
                CreateStartupOverlay();
                LoadSettings();
                ValidateSettings();
                this.Icon = Properties.Resources.BatteryIcon;
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
                // Set rounded corners (e.g., 30px radius)
                SetRoundedCorners(irndradius);

                // Start timer
                timer = new System.Windows.Forms.Timer();
                timer.Interval = 4000; // 4 seconds
                timer.Tick += Timer_Tick;
                timer.Start();

                // Register drag events for controls
                foreach (Control ctrl in this.Controls)
                {
                    if (ctrl is Label || ctrl is PictureBox)
                    {
                        ctrl.MouseDown += panelTop_MouseDown;
                        ctrl.MouseUp += panelTop_MouseUp;
                    }
                }
                btnClose.MouseEnter += (s, e) => btnClose.BackColor = Color.Red;
                btnClose.MouseLeave += (s, e) => btnClose.BackColor = Color.Transparent;
                btnTray.MouseEnter += (s, e) => btnTray.ForeColor = Color.DarkGreen;
                btnTray.MouseEnter += (s, e) => btnTray.BackColor = Color.Transparent;
                btnTray.MouseLeave += (s, e) => btnTray.ForeColor = Color.Transparent;
                btnTray.MouseLeave += (s, e) => btnTray.BackColor = Color.Transparent;
                btnMinimize.MouseEnter += (s, e) => btnMinimize.ForeColor = Color.DarkGreen;
                btnMinimize.MouseEnter += (s, e) => btnMinimize.BackColor = Color.Transparent;
                btnMinimize.MouseLeave += (s, e) => btnMinimize.ForeColor = Color.Transparent;
                btnMinimize.MouseLeave += (s, e) => btnMinimize.BackColor = Color.Transparent;
                SetRobotoFont(this);

                // Wire up events
                configSlideTimer = new System.Windows.Forms.Timer();
                configSlideTimer.Interval = 30; // ms, adjust for speed
                configSlideTimer.Tick += ConfigSlideTimer_Tick;




            }
            catch (Exception ex)
            {
                Exception inner = ex.InnerException;
                System.Text.StringBuilder sb = new();
                while (inner != null)
                {
                    if (!string.IsNullOrWhiteSpace(inner.Message))
                        sb.AppendLine(inner.Message);
                    inner = inner.InnerException;
                }
                ShowErrorOverlay(sb.Length > 0 ? sb.ToString().Trim() : ex.Message);
            }
        }

        #endregion

        #region Overlay Methods

        /// <summary>
        /// Create a semi-transparent overlay panel with a label for startup messages
        /// </summary>
        private void CreateStartupOverlay()
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
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            startupOverlay.Controls.Add(startupLabel);
            this.Controls.Add(startupOverlay);
            startupOverlay.BringToFront();
            // No need for async/await here
        }

        /// <summary>
        /// Start fading out the overlay
        /// </summary>
        private void StartOverlayFadeOut()
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
        }

        /// <summary>
        /// Show an error message on the overlay and exit after a delay
        /// </summary>
        /// <param name="message"></param>
        private async void ShowErrorOverlay(string message)
        {
            if (startupLabel != null && startupOverlay != null)
            {
                startupLabel.Text = $"Error: {message}";
                startupLabel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                startupOverlay.BackColor = Color.FromArgb(200, 120, 0, 0);
                startupOverlay.Visible = true;
                startupOverlay.BringToFront();
                await Task.Delay(5000);
            }
            Application.Exit();
        }

        #endregion

        #region Settings and Validation

        /// <summary>
        /// Load settings from appsettings.json
        /// </summary>
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
                lblConfigUpper.Text = $"Upper limit: {settings.UpperThreshold}%";
                lblConfigLower.Text = $"Lower limit: {settings.LowerThreshold}%";
                tglMute.Checked = settings.MuteAlerts;
            }
        }

        /// <summary>
        /// Validate settings and sound files
        /// </summary>
        private void ValidateSettings()
        {

            if (!typeof(bool).IsAssignableFrom(settings.MuteAlerts.GetType()))
                throw new InvalidOperationException("MuteAlerts value must be a boolean (true or false).");
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

        #endregion

        #region Timer Events

        /// <summary>
        /// Timer tick event to update battery status
        /// </summary>
        private async void Timer_Tick(object? sender, EventArgs e)
        {
            try
            {
                UpdateBatteryStatus();
                if (startupOverlay != null && startupOverlay.Visible)
                {
                    StartOverlayFadeOut();
                }
            }
            catch (Exception ex)
            {
                Exception inner = ex.InnerException;
                System.Text.StringBuilder sb = new();
                while (inner != null)
                {
                    if (!string.IsNullOrWhiteSpace(inner.Message))
                        sb.AppendLine(inner.Message);
                    inner = inner.InnerException;
                }
                ShowErrorOverlay(sb.Length > 0 ? sb.ToString().Trim() : ex.Message);
            }
            finally
            {
                await Task.Delay(500); // slight delay to ensure UI updates
            }
        }

        #endregion

        #region Battery Status and UI

        /// <summary>
        /// Update battery status and UI elements
        /// </summary>
        private void UpdateBatteryStatus()
        {
            PowerStatus powerStatus = SystemInformation.PowerStatus;
            float batteryLevel = powerStatus.BatteryLifePercent * 100;
            BatteryChargeStatus chargeStatus = powerStatus.BatteryChargeStatus;
            string status = chargeStatus.ToString() == "0" ? "Discharging" : chargeStatus.ToString();
            lblBatteryPercent.Text = $"{batteryLevel:F0}%";
            gradientProgressBar1.Value = Math.Min((int)batteryLevel, 100);
            this.Text = $"{batteryLevel:F0}% | {status}";
            float midThreshold = (settings.UpperThreshold + settings.LowerThreshold) / 2f;
            Image animatedImage = null;

            if (chargeStatus == BatteryChargeStatus.NoSystemBattery)
            {
                throw new Exception("No Battery Detected!!");
            }

            if (batteryLevel == 100)
            {
                string newStatus = string.Format("Status: {0}", status);
                if (lblStatus.Text != newStatus)
                    lblStatus.Text = newStatus;
                lblStatus.ForeColor = Color.DarkGreen;
                lblBatteryPercent.ForeColor = Color.DarkGreen;
                gradientProgressBar1.ForeColor = Color.DarkGreen;
                animatedImage = Properties.Resources.FullCharge;
                if (chargeStatus.HasFlag(BatteryChargeStatus.Charging))
                {
                    ShowAndFocus();
                    upperAlertCounter++;
                    lowerAlertCounter = 0;
                    StartAlertSound(settings.FullBatterySound);
                    isAlertCompleted = true;
                }
                else
                {                     // Reset if not charging
                    upperAlertCounter = 0;
                    lowerAlertCounter = 0;
                    isAlertCompleted = false;
                    StopAlertSound();
                }
            }
            else if (batteryLevel >= settings.UpperThreshold)
            {
                string newStatus = string.Format("Status: {0}", status);
                if (lblStatus.Text != newStatus)
                    lblStatus.Text = newStatus;
                lblStatus.ForeColor = Color.Green;
                lblBatteryPercent.ForeColor = Color.Green;
                gradientProgressBar1.ForeColor = Color.Green;
                animatedImage = Properties.Resources.AlmostFull;
                if (chargeStatus.HasFlag(BatteryChargeStatus.Charging))
                {

                    ShowAndFocus();
                    upperAlertCounter++;
                    lowerAlertCounter = 0;
                    StartAlertSound(settings.FullBatterySound);
                    isAlertCompleted = true;
                }
                else
                {                     // Reset if not charging
                    upperAlertCounter = 0;
                    lowerAlertCounter = 0;
                    isAlertCompleted = false;
                    StopAlertSound();
                }
            }
            else if (batteryLevel > midThreshold)
            {
                string newStatus = string.Format("Status: {0}", status);
                if (lblStatus.Text != newStatus)
                    lblStatus.Text = newStatus;
                lblStatus.ForeColor = Color.YellowGreen;
                lblBatteryPercent.ForeColor = Color.YellowGreen;
                gradientProgressBar1.ForeColor = Color.YellowGreen;
                animatedImage = Properties.Resources.LowMid;
            }
            else if (batteryLevel > settings.LowerThreshold)
            {
                string newStatus = string.Format("Status: {0}", status);
                if (lblStatus.Text != newStatus)
                    lblStatus.Text = newStatus;
                lblStatus.ForeColor = Color.Orange;
                lblBatteryPercent.ForeColor = Color.Orange;
                gradientProgressBar1.ForeColor = Color.Orange;
                animatedImage = Properties.Resources.LowMid;
            }
            else if (batteryLevel <= settings.LowerThreshold)
            {
                string newStatus = string.Format("Status: {0}", status);
                if (lblStatus.Text != newStatus)
                    lblStatus.Text = newStatus;
                lblStatus.ForeColor = Color.Red;
                lblBatteryPercent.ForeColor = Color.Red;
                gradientProgressBar1.ForeColor = Color.Red;
                animatedImage = Properties.Resources.LowCharging;
                if (!chargeStatus.HasFlag(BatteryChargeStatus.Charging))
                {
                    ShowAndFocus();
                    lowerAlertCounter++;
                    upperAlertCounter = 0;
                    StartAlertSound(settings.LowBatterySound);
                    isAlertCompleted = true;
                }
                else
                {   // Reset if charging
                    upperAlertCounter = 0;
                    lowerAlertCounter = 0;
                    isAlertCompleted = false;
                    StopAlertSound();
                }
            }
            else
            {
                // reset counters and stop sound if no alert
                upperAlertCounter = 0;
                lowerAlertCounter = 0;
                isAlertCompleted = false;
                StopAlertSound();
            }

            // Animate only if charging
            if (chargeStatus.HasFlag(BatteryChargeStatus.Charging))
            {
                string newStatus = "Status: Charging";
                if (lblStatus.Text != newStatus)
                    lblStatus.Text = newStatus;
                lblStatus.ForeColor = Color.Blue;
                pictureBoxBattery.Image = Properties.Resources.BatteryCharging;
                pictureBoxBattery.Width = 54;
                pictureBoxBattery.Height = 64;
                pictureBoxBattery.Location = new Point(62, 78);
            }
            else
            {
                pictureBoxBattery.Width = 94;
                pictureBoxBattery.Height = 64;
                pictureBoxBattery.Location = new Point(42, 80);
                if (animatedImage != null)
                {
                    pictureBoxBattery.Image = new Bitmap(animatedImage);
                }
            }

        }

        /// <summary>
        /// Brings the window to the front and focuses it.
        /// </summary>
        private void ShowAndFocus()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
            this.Activate();
            this.Focus();
            ForceAttention();
        }

        #endregion

        #region Alert Sound Methods

        /// <summary>
        /// Start playing alert sound in a loop with delay until stopped or duration exceeded
        /// </summary>
        /// <param name="fileName"></param>
        private void StartAlertSound(string? fileName)
        {
            try
            {

                if (tglMute != null && tglMute.Checked) { return; } // Alert muted
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

        /// <summary>
        /// Timer tick to play alert sound and manage delay and total duration
        /// </summary>
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

        /// <summary>
        /// Stop playing alert sound and clean up
        /// </summary>
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

        #endregion

        #region Window and UI Events

        /// <summary>
        /// Bring window to front and flash taskbar icon to get user attention
        /// </summary>
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

        /// <summary>
        /// Applies rounded corners to the form.
        /// </summary>
        /// <param name="radius">The radius of the corners in pixels.</param>
        private void SetRoundedCorners(int radius)
        {
            var bounds = new Rectangle(0, 0, this.Width, this.Height);
            var path = new GraphicsPath();
            path.StartFigure();
            path.AddArc(bounds.Left, bounds.Top, radius, radius, 180, 90);
            path.AddArc(bounds.Right - radius, bounds.Top, radius, radius, 270, 90);
            path.AddArc(bounds.Right - radius, bounds.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();
            this.Region = new Region(path);
        }

        /// <summary>
        /// Handles the Paint event to draw a custom border around the form.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            int radius = 10;
            Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw card background
            using (GraphicsPath cardPath = GetRoundedRectPath(rect, radius))
            using (SolidBrush cardBrush = new SolidBrush(this.BackColor))
            {
                e.Graphics.FillPath(cardBrush, cardPath);
            }

            // Draw border
            using (GraphicsPath borderPath = GetRoundedRectPath(rect, radius))
            using (Pen borderPen = new Pen(Color.FromArgb(60, 60, 60), 2))
            {
                e.Graphics.DrawPath(borderPen, borderPath);
            }
        }

        /// <summary>
        /// Returns a rounded rectangle path.
        /// </summary>
        private GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        /// <summary>
        /// Handles the click event for the close button, stops alert sound and exits the application.
        /// </summary>
        private void BtnClose_Click(object sender, EventArgs e)
        {
            StopAlertSound();
            Application.Exit();
        }

        /// <summary>
        /// Handles the click event for the minimize button, minimizing the window.
        /// </summary>
        private void BtnMinimize_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        /// <summary>
        /// Handles the MouseDown event for draggable controls to allow moving the window.
        /// </summary>
        private void panelTop_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Cursor = Cursors.SizeAll;
                ReleaseCapture();
                SendMessage(this.Handle, 0xA1, 0x2, 0);
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Handles the MouseUp event for draggable controls, resetting the cursor.
        /// </summary>
        private void panelTop_MouseUp(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Default;
        }

        /// <summary>
        /// Recursively sets the Roboto font for the specified control and its child controls.
        /// </summary>
        /// <param name="parent">The parent control.</param>
        private void SetRobotoFont(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                c.Font = new Font("Segoe UI", c.Font.Size, c.Font.Style);
                if (c.HasChildren)
                    SetRobotoFont(c);
            }
        }

        // Override OnResize to keep corners rounded
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            SetRoundedCorners(irndradius);
        }

        /// <summary>
        /// Timer tick for sliding animation
        /// </summary>
        private void ConfigSlideTimer_Tick(object? sender, EventArgs e)
        {
            if (configPanelExpanding)
            {
                // Slide up (show)
                if (panelConfig.Height < configPanelTargetHeight)
                {
                    panelConfig.Height += 2; // step size
                    if (panelConfig.Height > configPanelTargetHeight)
                        panelConfig.Height = configPanelTargetHeight;
                }
                else
                {
                    configSlideTimer.Stop();
                }
            }
            else
            {
                // Slide down (hide)
                if (panelConfig.Height > 0)
                {
                    panelConfig.Height -= 2;
                    if (panelConfig.Height < 0)
                        panelConfig.Height = 0;
                }
                else
                {
                    configSlideTimer.Stop();
                }
            }
        }

        #endregion


        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            StopAlertSound();
            if (trayIcon != null) trayIcon.Visible = false;
            base.OnFormClosing(e);
        }
        private void btnTray_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void tglMute_CheckedChanged(object sender, EventArgs e)
        {
            if (tglMute.Checked)
            {
                StopAlertSound();
            }
            else
            {
                upperAlertCounter = 0;
                lowerAlertCounter = 0;
                isAlertCompleted = false;
                // Restart timer to check battery status immediately
                Timer_Tick(null, EventArgs.Empty);
            }
        }

        private void tglConfig_CheckedChanged(object sender, EventArgs e)
        {
            configPanelExpanding = tglConfig.Checked;
            configSlideTimer.Start();
        }
    }

    /// <summary>
    /// Settings class to hold configuration values
    /// </summary>
    public class BatterySettings
    {
        public float UpperThreshold { get; set; } = 0;
        public float LowerThreshold { get; set; } = 0;
        public string FullBatterySound { get; set; } = string.Empty;
        public string LowBatterySound { get; set; } = string.Empty;
        public bool MuteAlerts { get; set; } = false;
    }
}
