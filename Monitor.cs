using Microsoft.Extensions.Configuration;
using System.Drawing.Drawing2D;
using System.Management;
using System.Media;
using System.Runtime.InteropServices;

namespace BatteryMonitor
{
    /// <summary>
    /// Main form for monitoring battery status, displaying alerts, and providing configuration options.
    /// </summary>
    public partial class Monitor : Form
    {
        #region Win32 API Imports

        /// <summary>
        /// Releases the mouse capture from a window.
        /// </summary>
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        /// <summary>
        /// Sends the specified message to a window or windows.
        /// </summary>
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        /// <summary>
        /// Brings the thread that created the specified window into the foreground and activates the window.
        /// </summary>
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>
        /// Flashes the specified window. It does not change the active state of the window.
        /// </summary>
        [DllImport("user32.dll")]
        private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        /// <summary>
        /// Contains the flash status for a window and the number of times the system should flash the window.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO
        {
            public UInt32 cbSize;
            public IntPtr hwnd;
            public UInt32 dwFlags;
            public UInt32 uCount;
            public UInt32 dwTimeout;
        }

        // Flash window until it comes to the foreground.
        private const UInt32 FLASHW_ALL = 3;
        // Flash the window continuously until the user clicks it.
        private const UInt32 FLASHW_TIMERNOFG = 12;

        #endregion

        #region Fields

        // Timers and settings
        private System.Windows.Forms.Timer? timer;
        private BatterySettings settings = new();

        // State management
        private int upperAlertCounter = 0;
        private int lowerAlertCounter = 0;
        private static int irndradius = 20;
        private bool isAlertCompleted = false;

        // Tray icon
        private NotifyIcon? trayIcon;

        // Overlay controls for startup and error messages
        private Panel? startupOverlay;
        private Label? startupLabel;
        private System.Windows.Forms.Timer? overlayTimer;
        private int overlayAlpha = 180;

        // Alert sound management
        private SoundPlayer? alertPlayer;
        private System.Windows.Forms.Timer? alertSoundTimer;
        private DateTime? alertSoundStartTime;
        private int alertPlayDelayMs = 2000;
        private int alertTotalDurationMs = 30 * 1000;
        private string? currentAlertSoundFile;

        // Configuration panel animation
        private int configPanelTargetHeight = 30;
        private bool configPanelExpanding = false;

        // Charge estimation
        private readonly Queue<(DateTime time, float percent)> chargeHistory = new();
        private const int MaxHistoryCount = 5; // Keep last 5 samples (≈25 sec if 5s interval)

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Monitor"/> class.
        /// Sets up components, loads settings, and initializes timers and UI elements.
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

                // Initialize and configure the tray icon
                trayIcon = new NotifyIcon
                {
                    Icon = this.Icon,
                    Visible = true,
                    Text = "Battery Monitor"
                };
                trayIcon.DoubleClick += (s, e) =>
                {
                    this.Show();
                    this.WindowState = FormWindowState.Normal;
                    this.BringToFront();
                    this.Activate();
                };

                SetRoundedCorners(irndradius);

                // Main timer for checking battery status
                timer = new System.Windows.Forms.Timer
                {
                    Interval = 5000
                };
                timer.Tick += Timer_Tick;
                timer.Start();

                // Enable window dragging for labels and picture boxes
                foreach (Control ctrl in this.Controls)
                {
                    if (ctrl is Label || ctrl is PictureBox)
                    {
                        ctrl.MouseDown += panelTop_MouseDown;
                        ctrl.MouseUp += panelTop_MouseUp;
                    }
                }

                // Button hover effects
                btnClose.MouseEnter += (s, e) => btnClose.BackColor = Color.Red;
                btnClose.MouseLeave += (s, e) => btnClose.BackColor = Color.Transparent;
                btnTray.MouseEnter += (s, e) => btnTray.ForeColor = Color.DarkGreen;
                btnTray.MouseLeave += (s, e) => btnTray.ForeColor = Color.Transparent;
                btnMinimize.MouseEnter += (s, e) => btnMinimize.ForeColor = Color.DarkGreen;
                btnMinimize.MouseLeave += (s, e) => btnMinimize.ForeColor = Color.Transparent;

                SetRobotoFont(this);

                // Timer for configuration panel slide animation
                configSlideTimer = new System.Windows.Forms.Timer
                {
                    Interval = 30
                };
                configSlideTimer.Tick += ConfigSlideTimer_Tick;
            }
            catch (Exception ex)
            {
                // Display any initialization errors on the overlay
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

        #region Form and Control Events

        /// <summary>
        /// Handles the main timer tick event to update battery status and UI.
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

        /// <summary>
        /// Handles the Paint event for the battery PictureBox to draw the custom battery icon.
        /// </summary>
        private void pictureBoxBattery_Paint(object sender, PaintEventArgs e)
        {
            if (pictureBoxBattery.Image != null) return;

            int percent = GetBatteryPercentage();
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Define battery body and cap dimensions
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

            // Draw battery outline
            using (GraphicsPath bodyPath = CreateRoundedRectangle(body, 8))
            using (GraphicsPath capPath = CreateRoundedRectangle(cap, 4))
            using (Pen pen = new Pen(Color.Black, 4))
            {
                g.DrawPath(pen, bodyPath);
                g.DrawPath(pen, capPath);
                g.FillPath(Brushes.Transparent, capPath);
            }

            // Determine fill color based on percentage
            Color fillColor;
            if (percent <= 25) fillColor = Color.Red;
            else if (percent <= 50) fillColor = Color.Orange;
            else if (percent <= 75) fillColor = Color.Green;
            else fillColor = Color.DarkGreen;

            // Draw battery fill level
            int fillHeight = (body.Height - 4) * percent / 100;
            Rectangle fillRect = new Rectangle(body.X + 2,
                                               body.Y + body.Height - fillHeight - 2,
                                               body.Width - 4,
                                               fillHeight);

            using (SolidBrush brush = new SolidBrush(fillColor))
            {
                g.FillRectangle(brush, fillRect);
            }

            // Draw internal separator bars
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

        /// <summary>
        /// Handles the click event for the close button.
        /// </summary>
        private void BtnClose_Click(object sender, EventArgs e)
        {
            StopAlertSound();
            Application.Exit();
        }

        /// <summary>
        /// Handles the click event for the minimize button.
        /// </summary>
        private void BtnMinimize_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        /// <summary>
        /// Handles the click event for the tray button to hide the form.
        /// </summary>
        private void btnTray_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        /// <summary>
        /// Handles the MouseDown event on draggable controls to enable window movement.
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
        /// Handles the MouseUp event on draggable controls to reset the cursor.
        /// </summary>
        private void panelTop_MouseUp(object? sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Default;
        }

        /// <summary>
        /// Handles the CheckedChanged event for the mute toggle switch.
        /// </summary>
        private void tglMute_CheckedChanged(object sender, EventArgs e)
        {
            if (tglMute.Checked)
            {
                StopAlertSound();
            }
            else
            {
                // Reset counters and re-evaluate state if unmuted
                upperAlertCounter = 0;
                lowerAlertCounter = 0;
                isAlertCompleted = false;
                Timer_Tick(null, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event for the config toggle to show/hide the panel.
        /// </summary>
        private void tglConfig_CheckedChanged(object sender, EventArgs e)
        {
            configPanelExpanding = tglConfig.Checked;
            configSlideTimer?.Start();
        }

        /// <summary>
        /// Handles the timer tick for the configuration panel's slide animation.
        /// </summary>
        private void ConfigSlideTimer_Tick(object? sender, EventArgs e)
        {
            if (configPanelExpanding)
            {
                if (panelConfig.Height < configPanelTargetHeight)
                {
                    panelConfig.Height = Math.Min(panelConfig.Height + 2, configPanelTargetHeight);
                }
                else
                {
                    configSlideTimer?.Stop();
                }
            }
            else
            {
                if (panelConfig.Height > 0)
                {
                    panelConfig.Height = Math.Max(panelConfig.Height - 2, 0);
                }
                else
                {
                    configSlideTimer?.Stop();
                }
            }
        }

        /// <summary>
        /// Handles the click event for the 'About' button.
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            using (var about = new AboutForm())
            {
                about.ShowDialog(this);
            }
        }

        /// <summary>
        /// Handles the form's closing event to clean up resources.
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            StopAlertSound();
            if (trayIcon != null) trayIcon.Visible = false;
            base.OnFormClosing(e);
        }

        /// <summary>
        /// Overrides the resize event to re-apply rounded corners.
        /// </summary>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            SetRoundedCorners(irndradius);
        }

        /// <summary>
        /// Overrides the paint event to draw a custom border.
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
        /// Starts the fade-out animation for the startup overlay.
        /// </summary>
        private void StartOverlayFadeOut()
        {
            if (overlayTimer != null)
            {
                overlayTimer.Stop();
                overlayTimer.Dispose();
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
        /// Shows an error message on the overlay and exits the application after a delay.
        /// </summary>
        /// <param name="message">The error message to display.</param>
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
        /// Loads settings from the appsettings.json file.
        /// </summary>
        private void LoadSettings()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string? exePath = Environment.ProcessPath ?? Environment.GetCommandLineArgs().FirstOrDefault() ?? baseDir;
            string exeDir = Path.GetDirectoryName(exePath) ?? baseDir;

            // Define potential paths for the configuration file
            string[] checkedPaths = {
                Path.Combine(baseDir, "Resources", "appsettings.json"),
                Path.Combine(baseDir, "appsettings.json"),
                Path.Combine(exeDir, "Resources", "appsettings.json"),
                Path.Combine(exeDir, "appsettings.json")
            };

            string? configPath = checkedPaths.FirstOrDefault(File.Exists);

            if (string.IsNullOrEmpty(configPath))
            {
                System.Text.StringBuilder sb = new();
                foreach (var p in checkedPaths) sb.AppendLine(p);
                throw new FileNotFoundException($"appsettings.json not found. Checked:\n{sb.ToString().Trim()}");
            }

            var config = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(configPath) ?? baseDir)
                .AddJsonFile(Path.GetFileName(configPath), optional: false, reloadOnChange: true)
                .Build();

            settings = config.GetSection("BatteryMonitor").Get<BatterySettings>()
                ?? throw new InvalidOperationException("BatteryMonitor section is missing or invalid in appsettings.json.");

            lblConfigUpper.Text = $"Upper limit: {settings.UpperThreshold}%";
            lblConfigLower.Text = $"Lower limit: {settings.LowerThreshold}%";
            tglMute.Checked = settings.MuteAlerts;
        }

        /// <summary>
        /// Validates the loaded settings to ensure they are within acceptable ranges.
        /// </summary>
        private void ValidateSettings()
        {
            if (settings.UpperThreshold <= 0 || settings.UpperThreshold > 100)
                throw new InvalidOperationException($"UpperThreshold {settings.UpperThreshold} is out of valid range (1–100).");

            if (settings.LowerThreshold < 0 || settings.LowerThreshold >= 100)
                throw new InvalidOperationException($"LowerThreshold {settings.LowerThreshold} is out of valid range (0–99).");

            if (settings.LowerThreshold >= settings.UpperThreshold)
                throw new InvalidOperationException($"LowerThreshold {settings.LowerThreshold} must be less than UpperThreshold {settings.UpperThreshold}.");

            if (string.IsNullOrWhiteSpace(settings.FullBatterySound))
                throw new InvalidOperationException("FullBatterySound file name is missing.");

            if (string.IsNullOrWhiteSpace(settings.LowBatterySound))
                throw new InvalidOperationException("LowBatterySound file name is missing.");

            if (ResolveExternalFile(settings.FullBatterySound) == null)
                throw new FileNotFoundException($"FullBatterySound file not found: {settings.FullBatterySound}");

            if (ResolveExternalFile(settings.LowBatterySound) == null)
                throw new FileNotFoundException($"LowBatterySound file not found: {settings.LowBatterySound}");
        }

        #endregion

        #region Battery Status and UI Updates

        /// <summary>
        /// Fetches the current power status and updates all relevant UI elements.
        /// </summary>
        private void UpdateBatteryStatus()
        {
            PowerStatus powerStatus = SystemInformation.PowerStatus;
            float batteryLevel = powerStatus.BatteryLifePercent * 100;
            BatteryChargeStatus chargeStatus = powerStatus.BatteryChargeStatus;
            string status = chargeStatus == 0 ? "Discharging" : chargeStatus.ToString();

            if (chargeStatus == BatteryChargeStatus.NoSystemBattery)
            {
                throw new Exception("No Battery Detected!");
            }

            // Update core UI elements
            lblBatteryPercent.Text = $"{batteryLevel:F0}%";
            gradientProgressBar1.Value = Math.Min((int)batteryLevel, 100);
            this.Text = $"{batteryLevel:F0}% | {status}";
            trayIcon!.Text = $"Battery: {batteryLevel:F0}% | {status}";

            // Update colors and trigger alerts based on battery level
            UpdateStatusColorsAndAlerts(batteryLevel, chargeStatus, status);

            // Update charging/discharging specific UI
            UpdateChargeTimeEstimate(powerStatus, batteryLevel, chargeStatus);

            var (design, full) = GetBatteryInfo();
            if (design > 0 && full > 0)
            {
                double healthPercent = (double)full / design * 100;
                lblBatteryHealth.Text = $"Battery Health: {healthPercent:F1}%";
            }
            else
            {
                lblBatteryHealth.Text = "Battery Health: N/A";
            }
        }

        /// <summary>
        /// Updates UI colors and triggers alerts based on the current battery level and charge status.
        /// </summary>
        private void UpdateStatusColorsAndAlerts(float batteryLevel, BatteryChargeStatus chargeStatus, string status)
        {
            float midThreshold = (settings.UpperThreshold + settings.LowerThreshold) / 2f;
            string newStatus = $"Status: {status}";
            if (lblStatus.Text != newStatus) lblStatus.Text = newStatus;

            if (batteryLevel >= settings.UpperThreshold)
            {
                SetUIColors(Color.Green);
                if (chargeStatus.HasFlag(BatteryChargeStatus.Charging))
                {
                    HandleAlertCondition(true, settings.FullBatterySound);
                }
                else
                {
                    ResetAlertState();
                }
            }
            else if (batteryLevel > midThreshold)
            {
                SetUIColors(Color.YellowGreen);
                ResetAlertState();
            }
            else if (batteryLevel > settings.LowerThreshold)
            {
                SetUIColors(Color.Orange);
                ResetAlertState();
            }
            else // batteryLevel <= settings.LowerThreshold
            {
                SetUIColors(Color.Red);
                if (!chargeStatus.HasFlag(BatteryChargeStatus.Charging))
                {
                    HandleAlertCondition(false, settings.LowBatterySound);
                }
                else
                {
                    ResetAlertState();
                }
            }
        }

        /// <summary>
        /// Sets the color for various UI elements.
        /// </summary>
        private void SetUIColors(Color color)
        {
            lblStatus.ForeColor = color;
            lblBatteryPercent.ForeColor = color;
            gradientProgressBar1.ForeColor = color;
        }

        /// <summary>
        /// Updates the time estimate for charging or discharging.
        /// </summary>
        private void UpdateChargeTimeEstimate(PowerStatus powerStatus, float batteryLevel, BatteryChargeStatus chargeStatus)
        {
            if (chargeStatus.HasFlag(BatteryChargeStatus.Charging))
            {
                lblStatus.Text = "Status: Charging";
                lblStatus.ForeColor = Color.Blue;
                pictureBoxBattery.Image = Properties.Resources.BatteryCharging;

                // Record history for charge rate calculation
                chargeHistory.Enqueue((DateTime.Now, batteryLevel));
                if (chargeHistory.Count > MaxHistoryCount) chargeHistory.Dequeue();

                if (chargeHistory.Count >= 2)
                {
                    var first = chargeHistory.Peek();
                    var last = chargeHistory.Last();
                    double elapsedSeconds = (last.time - first.time).TotalSeconds;
                    float deltaPercent = last.percent - first.percent;

                    if (deltaPercent > 0.01)
                    {
                        double secondsPerPercent = elapsedSeconds / deltaPercent;
                        double remainingPercent = Math.Max(0, 100 - batteryLevel);
                        double remainingSeconds = secondsPerPercent * remainingPercent;
                        lblTimeRemaining.Text = $"Time to Full: {FormatTime(remainingSeconds)}";
                    }
                }
            }
            else
            {
                pictureBoxBattery.Image = null;
                pictureBoxBattery.Invalidate();
                chargeHistory.Clear();
                int lifeRemaining = powerStatus.BatteryLifeRemaining; // in seconds
                if (lifeRemaining > 0)
                {
                    lblTimeRemaining.Text = $"Time Remaining: {FormatTime(lifeRemaining)}";
                }
            }
        }

        #endregion

        #region Alert Methods

        /// <summary>
        /// Handles the logic for triggering an alert.
        /// </summary>
        private void HandleAlertCondition(bool isUpper, string soundFile)
        {
            ShowAndFocus();
            if (isUpper) upperAlertCounter++; else lowerAlertCounter++;
            if (isUpper) lowerAlertCounter = 0; else upperAlertCounter = 0;
            StartAlertSound(soundFile);
            isAlertCompleted = true;
        }

        /// <summary>
        /// Resets alert counters and stops any active alert sound.
        /// </summary>
        private void ResetAlertState()
        {
            upperAlertCounter = 0;
            lowerAlertCounter = 0;
            isAlertCompleted = false;
            StopAlertSound();
        }

        /// <summary>
        /// Starts playing an alert sound file in a loop.
        /// </summary>
        /// <param name="fileName">The sound file to play.</param>
        private void StartAlertSound(string? fileName)
        {
            try
            {
                if (tglMute.Checked || string.IsNullOrWhiteSpace(fileName) || isAlertCompleted)
                    return;

                string? soundPath = ResolveExternalFile(fileName);
                if (string.IsNullOrWhiteSpace(soundPath))
                    return;

                // Do not restart if the same sound is already playing
                if (alertSoundTimer?.Enabled == true && string.Equals(currentAlertSoundFile, soundPath, StringComparison.OrdinalIgnoreCase))
                    return;

                StopAlertSound();

                alertPlayer = new SoundPlayer(soundPath);
                currentAlertSoundFile = soundPath;
                alertSoundStartTime = DateTime.Now;

                if (alertSoundTimer == null)
                {
                    alertSoundTimer = new System.Windows.Forms.Timer();
                    alertSoundTimer.Tick += AlertSoundTimer_Tick;
                }

                alertSoundTimer.Interval = 10; // Start almost immediately
                alertSoundTimer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sound error: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the alert sound timer tick to replay the sound after a delay.
        /// </summary>
        private void AlertSoundTimer_Tick(object? sender, EventArgs e)
        {
            if (alertPlayer == null || alertSoundStartTime == null)
            {
                StopAlertSound();
                return;
            }

            // Stop after total duration has been exceeded
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
            catch { /* Ignore errors during playback */ }

            // Set interval for the next play
            if (alertSoundTimer != null)
                alertSoundTimer.Interval = alertPlayDelayMs;
        }

        /// <summary>
        /// Stops the alert sound and releases associated resources.
        /// </summary>
        private void StopAlertSound()
        {
            try
            {
                alertPlayer?.Stop();
                alertPlayer?.Dispose();
                alertPlayer = null;

                alertSoundTimer?.Stop();

                alertSoundStartTime = null;
                currentAlertSoundFile = null;
            }
            catch { /* Ignore cleanup errors */ }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Retrieves battery design and full charge capacity from WMI.
        /// </summary>
        /// <returns>A tuple containing the design capacity and full charge capacity. Returns (-1, -1) on failure.</returns>
        private (int designCapacity, int fullChargeCapacity) GetBatteryInfo()
        {
            int designCapacity = -1;
            int fullChargeCapacity = -1;

            try
            {
                // Battery Static Data = Design Capacity
                using (var searcher = new ManagementObjectSearcher("root\\wmi", "SELECT * FROM BatteryStaticData"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        designCapacity = Convert.ToInt32(obj["DesignedCapacity"]);
                    }
                }

                // Battery Full Charged Capacity = Current Max Capacity
                using (var searcher = new ManagementObjectSearcher("root\\wmi", "SELECT * FROM BatteryFullChargedCapacity"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        fullChargeCapacity = Convert.ToInt32(obj["FullChargedCapacity"]);
                    }
                }
            }
            catch
            {
                // Some machines may not report values
            }

            return (designCapacity, fullChargeCapacity);
        }

        /// <summary>
        /// Brings the window to the foreground and flashes the taskbar icon to get user attention.
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
        /// Applies rounded corners to the form's region.
        /// </summary>
        /// <param name="radius">The radius of the corners.</param>
        private void SetRoundedCorners(int radius)
        {
            var path = GetRoundedRectPath(new Rectangle(0, 0, this.Width, this.Height), radius);
            this.Region = new Region(path);
        }

        /// <summary>
        /// Creates a GraphicsPath for a rectangle with rounded corners.
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
        /// Recursively sets the font for a control and all its children.
        /// </summary>
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
        /// Gets the current battery percentage as an integer.
        /// </summary>
        private int GetBatteryPercentage()
        {
            return (int)(SystemInformation.PowerStatus.BatteryLifePercent * 100);
        }

        /// <summary>
        /// Creates a GraphicsPath for a rounded rectangle.
        /// </summary>
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
        /// Formats a duration in seconds into a human-readable string (e.g., "1h 30m" or "15m 10s").
        /// </summary>
        private string FormatTime(double seconds)
        {
            TimeSpan t = TimeSpan.FromSeconds(seconds);
            if (t.TotalHours >= 1)
                return $"{(int)t.TotalHours}h {t.Minutes}m";
            else
                return $"{t.Minutes}m {t.Seconds}s";
        }

        /// <summary>
        /// Resolves an external file path by checking multiple likely locations.
        /// </summary>
        /// <returns>The full path if found; otherwise, null.</returns>
        private string? ResolveExternalFile(string relativePath)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string? exePath = Environment.ProcessPath ?? Environment.GetCommandLineArgs().FirstOrDefault() ?? baseDir;
            string exeDir = Path.GetDirectoryName(exePath) ?? baseDir;
            string fileName = Path.GetFileName(relativePath);

            var candidates = new[]
            {
                Path.Combine(baseDir, relativePath),
                Path.Combine(baseDir, fileName),
                Path.Combine(baseDir, "Resources", fileName),
                Path.Combine(exeDir, relativePath),
                Path.Combine(exeDir, fileName),
                Path.Combine(exeDir, "Resources", fileName)
            };

            foreach (var c in candidates)
            {
                try
                {
                    if (!string.IsNullOrEmpty(c) && File.Exists(c))
                        return c;
                }
                catch
                {
                    // Ignore path access exceptions
                }
            }
            return null;
        }

        /// <summary>
        /// Brings the main window to the front and activates it.
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
    }

    #region Configuration Class

    /// <summary>
    /// Holds configuration settings for the battery monitor application.
    /// </summary>
    public class BatterySettings
    {
        /// <summary>
        /// The battery percentage above which an alert is triggered when charging.
        /// </summary>
        public float UpperThreshold { get; set; } = 0;
        /// <summary>
        /// The battery percentage below which an alert is triggered when discharging.
        /// </summary>
        public float LowerThreshold { get; set; } = 0;
        /// <summary>
        /// The path to the sound file for the full battery alert.
        /// </summary>
        public string FullBatterySound { get; set; } = string.Empty;
        /// <summary>
        /// The path to the sound file for the low battery alert.
        /// </summary>
        public string LowBatterySound { get; set; } = string.Empty;
        /// <summary>
        /// A value indicating whether to mute all sound alerts.
        /// </summary>
        public bool MuteAlerts { get; set; } = false;
    }

    #endregion
}