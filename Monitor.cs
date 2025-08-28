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

        // Counters/State
        private int upperAlertCounter = 0;
        private int lowerAlertCounter = 0;
        private static int irndradius = 20;

        // Tray
        private NotifyIcon? trayIcon;

        // Overlay controls
        private Panel? startupOverlay;
        private Label? startupLabel;
        private System.Windows.Forms.Timer? overlayTimer;
        private int overlayAlpha = 180;

        // Alert sound controls
        private SoundPlayer? alertPlayer;
        private System.Windows.Forms.Timer? alertSoundTimer;
        private DateTime? alertSoundStartTime;
        private int alertPlayDelayMs = 2000;
        private int alertTotalDurationMs = 30 * 1000;
        private string? currentAlertSoundFile;
        private bool isAlertCompleted = false;

        // Config panel sliding
        private int configPanelTargetHeight = 30;
        private bool configPanelExpanding = false;
        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Monitor"/> class.
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
                this.Icon = Properties.Resources.battery;

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

                SetRoundedCorners(irndradius);

                timer = new System.Windows.Forms.Timer();
                timer.Interval = 5000;
                timer.Tick += Timer_Tick;
                timer.Start();

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

                configSlideTimer = new System.Windows.Forms.Timer();
                configSlideTimer.Interval = 30;
                configSlideTimer.Tick += ConfigSlideTimer_Tick;
            }
            catch (Exception ex)
            {
                Exception? inner = ex.InnerException;
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
        /// Creates a semi-transparent overlay panel with a label for startup messages.
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
        }

        /// <summary>
        /// Starts the fade out animation for the startup overlay.
        /// </summary>
        private void StartOverlayFadeOut()
        {
            if (overlayTimer != null)
            {
                overlayTimer.Stop();
                overlayTimer.Dispose();
                overlayTimer = null;
            }

            overlayTimer = new System.Windows.Forms.Timer { Interval = 50 };
            overlayTimer.Tick += (s, e) =>
            {
                overlayAlpha -= 15;
                if (overlayAlpha <= 0)
                {
                    overlayTimer.Stop();
                    if (startupOverlay != null)
                        startupOverlay.Visible = false;
                }
                else
                {
                    if (startupOverlay != null)
                        startupOverlay.BackColor = Color.FromArgb(overlayAlpha, 30, 30, 30);
                }
            };
            overlayTimer.Start();
        }

        /// <summary>
        /// Shows an error message on the overlay and exits after a delay.
        /// </summary>
        /// <param name="message">Error message to display.</param>
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
        /// Loads settings from appsettings.json.
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
        /// Validates settings and sound files.
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
        /// Handles the timer tick event to update battery status.
        /// </summary>
        private async void Timer_Tick(object? sender, EventArgs e)
        {
            try
            {
                pictureBoxBattery.Invalidate();
                UpdateBatteryStatus();
                if (startupOverlay != null && startupOverlay.Visible)
                {
                    StartOverlayFadeOut();
                }
            }
            catch (Exception ex)
            {
                Exception? inner = ex.InnerException;
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
                await Task.Delay(500);
            }
        }

        #endregion

        #region Battery Status and UI

        /// <summary>
        /// Updates battery status and UI elements.
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
            trayIcon!.Text = $"Battery: {batteryLevel:F0}% | {status}";
            float midThreshold = (settings.UpperThreshold + settings.LowerThreshold) / 2f;

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

                if (chargeStatus.HasFlag(BatteryChargeStatus.Charging))
                {
                    ShowAndFocus();
                    upperAlertCounter++;
                    lowerAlertCounter = 0;
                    StartAlertSound(settings.FullBatterySound);
                    isAlertCompleted = true;
                }
                else
                {
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

                if (chargeStatus.HasFlag(BatteryChargeStatus.Charging))
                {
                    ShowAndFocus();
                    upperAlertCounter++;
                    lowerAlertCounter = 0;
                    StartAlertSound(settings.FullBatterySound);
                    isAlertCompleted = true;
                }
                else
                {
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
            }
            else if (batteryLevel > settings.LowerThreshold)
            {
                string newStatus = string.Format("Status: {0}", status);
                if (lblStatus.Text != newStatus)
                    lblStatus.Text = newStatus;

                lblStatus.ForeColor = Color.Orange;
                lblBatteryPercent.ForeColor = Color.Orange;
                gradientProgressBar1.ForeColor = Color.Orange;
            }
            else if (batteryLevel <= settings.LowerThreshold)
            {
                string newStatus = string.Format("Status: {0}", status);
                if (lblStatus.Text != newStatus)
                    lblStatus.Text = newStatus;

                lblStatus.ForeColor = Color.Red;
                lblBatteryPercent.ForeColor = Color.Red;
                gradientProgressBar1.ForeColor = Color.Red;

                if (!chargeStatus.HasFlag(BatteryChargeStatus.Charging))
                {
                    ShowAndFocus();
                    lowerAlertCounter++;
                    upperAlertCounter = 0;
                    StartAlertSound(settings.LowBatterySound);
                    isAlertCompleted = true;
                }
                else
                {
                    upperAlertCounter = 0;
                    lowerAlertCounter = 0;
                    isAlertCompleted = false;
                    StopAlertSound();
                }
            }
            else
            {
                upperAlertCounter = 0;
                lowerAlertCounter = 0;
                isAlertCompleted = false;
                StopAlertSound();
            }

            if (chargeStatus.HasFlag(BatteryChargeStatus.Charging))
            {
                string newStatus = "Status: Charging";
                if (lblStatus.Text != newStatus)
                    lblStatus.Text = newStatus;

                lblStatus.ForeColor = Color.Blue;
                pictureBoxBattery.Image = Properties.Resources.BatteryCharging;
            }
            else
            {
                pictureBoxBattery.Image = null;
                pictureBoxBattery.Invalidate();
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
        /// Starts playing alert sound in a loop with delay until stopped or duration exceeded.
        /// </summary>
        /// <param name="fileName">Sound file name to play.</param>
        private void StartAlertSound(string? fileName)
        {
            try
            {
                if (tglMute != null && tglMute.Checked) { return; }
                if (string.IsNullOrWhiteSpace(fileName))
                    return;
                if (isAlertCompleted)
                    return;

                string soundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
                if (!File.Exists(soundPath))
                    return;

                if (alertSoundTimer != null && alertSoundTimer.Enabled
                    && string.Equals(currentAlertSoundFile, soundPath, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                StopAlertSound();

                alertPlayer = new SoundPlayer(soundPath);
                currentAlertSoundFile = soundPath;
                alertSoundStartTime = DateTime.Now;

                if (alertSoundTimer == null)
                {
                    alertSoundTimer = new System.Windows.Forms.Timer();
                    alertSoundTimer.Tick += AlertSoundTimer_Tick;
                }

                alertSoundTimer.Interval = 10;
                alertSoundTimer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sound error: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the timer tick to play alert sound and manage delay and total duration.
        /// </summary>
        private void AlertSoundTimer_Tick(object? sender, EventArgs e)
        {
            if (alertPlayer == null || alertSoundStartTime == null)
            {
                StopAlertSound();
                return;
            }

            if ((DateTime.Now - alertSoundStartTime.Value).TotalMilliseconds >= alertTotalDurationMs)
            {
                StopAlertSound();
                return;
            }

            try
            {
                alertPlayer.Stop();
                alertPlayer.Play();
            }
            catch
            {
            }

            alertSoundTimer!.Interval = alertPlayDelayMs;
        }

        /// <summary>
        /// Stops playing alert sound and cleans up.
        /// </summary>
        private void StopAlertSound()
        {
            try
            {
                if (alertPlayer != null)
                {
                    try { alertPlayer.Stop(); } catch { }
                    try { alertPlayer.Dispose(); } catch { }
                    alertPlayer = null;
                }

                if (alertSoundTimer != null)
                {
                    try { alertSoundTimer.Stop(); } catch { }
                }

                alertSoundStartTime = null;
                currentAlertSoundFile = null;
            }
            catch
            {
            }
        }

        #endregion

        #region Window and UI Events

        /// <summary>
        /// Brings window to front and flashes taskbar icon to get user attention.
        /// </summary>
        private void ForceAttention()
        {
            IntPtr hWnd = this.Handle;
            SetForegroundWindow(hWnd);

            FLASHWINFO fw = new FLASHWINFO
            {
                cbSize = Convert.ToUInt32(Marshal.SizeOf(typeof(FLASHWINFO))),
                hwnd = hWnd,
                dwFlags = FLASHW_ALL | FLASHW_TIMERNOFG,
                uCount = UInt32.MaxValue,
                dwTimeout = 0
            };

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

            using (GraphicsPath cardPath = GetRoundedRectPath(rect, radius))
            using (SolidBrush cardBrush = new SolidBrush(this.BackColor))
            {
                e.Graphics.FillPath(cardBrush, cardPath);
            }

            using (GraphicsPath borderPath = GetRoundedRectPath(rect, radius))
            using (Pen borderPen = new Pen(Color.FromArgb(60, 60, 60), 2))
            {
                e.Graphics.DrawPath(borderPen, borderPath);
            }
        }

        /// <summary>
        /// Returns a rounded rectangle path.
        /// </summary>
        /// <param name="rect">Rectangle bounds.</param>
        /// <param name="radius">Corner radius.</param>
        /// <returns>GraphicsPath for rounded rectangle.</returns>
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
        private void panelTop_MouseDown(object? sender, MouseEventArgs e)
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
        private void panelTop_MouseUp(object? sender, MouseEventArgs e)
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

        /// <summary>
        /// Overrides OnResize to keep corners rounded.
        /// </summary>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            SetRoundedCorners(irndradius);
        }

        /// <summary>
        /// Handles the timer tick for sliding animation of the configuration panel.
        /// </summary>
        private void ConfigSlideTimer_Tick(object? sender, EventArgs e)
        {
            if (configPanelExpanding)
            {
                if (panelConfig.Height < configPanelTargetHeight)
                {
                    panelConfig.Height += 2;
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

        /// <summary>
        /// Handles form closing to clean up resources like tray icon and sounds.
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            StopAlertSound();
            if (trayIcon != null) trayIcon.Visible = false;
            base.OnFormClosing(e);
        }

        /// <summary>
        /// Handles minimize to tray button click.
        /// </summary>
        private void btnTray_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        /// <summary>
        /// Handles toggle mute changed event. Stops alert sound immediately when enabled.
        /// </summary>
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
                Timer_Tick(null, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Handles toggle configuration panel visibility with slide animation.
        /// </summary>
        private void tglConfig_CheckedChanged(object sender, EventArgs e)
        {
            configPanelExpanding = tglConfig.Checked;
            configSlideTimer.Start();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets the current battery percentage.
        /// </summary>
        /// <returns>Battery percentage as integer.</returns>
        private int GetBatteryPercentage()
        {
            return (int)(SystemInformation.PowerStatus.BatteryLifePercent * 100);
        }

        /// <summary>
        /// Creates a rounded rectangle path.
        /// </summary>
        /// <param name="rect">Rectangle bounds.</param>
        /// <param name="radius">Corner radius.</param>
        /// <returns>GraphicsPath for rounded rectangle.</returns>
        private GraphicsPath CreateRoundedRectangle(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        /// <summary>
        /// Handles the Paint event for the battery PictureBox to draw the battery icon.
        /// </summary>
        private void pictureBoxBattery_Paint(object sender, PaintEventArgs e)
        {
            if (pictureBoxBattery.Image != null) return;

            int percent = GetBatteryPercentage();
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int bodyWidth = pictureBoxBattery.Width / 3;
            int bodyHeight = pictureBoxBattery.Height - 20;
            int bodyX = (pictureBoxBattery.Width - bodyWidth) / 2;
            int bodyY = 10;

            Rectangle body = new Rectangle(bodyX, bodyY, bodyWidth, bodyHeight);

            int capWidth = bodyWidth / 2;
            int capHeight = 8;
            int capX = bodyX + (bodyWidth - capWidth) / 2;
            int capY = bodyY - capHeight;
            Rectangle cap = new Rectangle(capX, capY, capWidth, capHeight);

            using (GraphicsPath bodyPath = CreateRoundedRectangle(body, 8))
            using (GraphicsPath capPath = CreateRoundedRectangle(cap, 4))
            using (Pen pen = new Pen(Color.Black, 4))
            {
                g.DrawPath(pen, bodyPath);
                g.DrawPath(pen, capPath);
                g.FillPath(Brushes.Transparent, capPath);
            }

            Color fillColor;
            if (percent <= 25) fillColor = Color.Red;
            else if (percent <= 50) fillColor = Color.Orange;
            else if (percent <= 75) fillColor = Color.Green;
            else fillColor = Color.DarkGreen;

            int fillHeight = (body.Height - 4) * percent / 100;
            Rectangle fillRect = new Rectangle(body.X + 2,
                                               body.Y + body.Height - fillHeight - 2,
                                               body.Width - 4,
                                               fillHeight);

            using (SolidBrush brush = new SolidBrush(fillColor))
            {
                g.FillRectangle(brush, fillRect);
            }

            int barCount = 4;
            int barSpacing = body.Height / barCount;
            using (Pen barPen = new Pen(Color.White, 1))
            {
                for (int i = 1; i < barCount; i++)
                {
                    int y = body.Y + i * barSpacing;
                    g.DrawLine(barPen, body.Left + 4, y, body.Right - 4, y);
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// Settings class to hold configuration values.
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