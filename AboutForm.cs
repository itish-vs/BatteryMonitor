using System.Reflection;

namespace BatteryMonitor
{
    /// <summary>
    /// Represents the About window for the application, displaying product information, version, and a description.
    /// </summary>
    partial class AboutForm : Form
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="AboutForm"/> class.
        /// Sets up the form's properties and populates controls with assembly information.
        /// </summary>
        public AboutForm()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "About";
            this.labelProductName.Text = AssemblyProduct;
            this.labelVersion.Text = $"Version {AssemblyVersion}";
            this.txtDescription.Multiline = true;
            this.txtDescription.ReadOnly = true;
            this.txtDescription.BorderStyle = BorderStyle.None;
            this.txtDescription.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            this.txtDescription.Dock = DockStyle.Fill;
            this.txtDescription.BackColor = this.BackColor;
            this.txtDescription.TabStop = false;
            this.txtDescription.Text = AssemblyDescription;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the version of the executing assembly.
        /// </summary>
        public string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
            }
        }

        /// <summary>
        /// Gets the description of the application.
        /// </summary>
        public string AssemblyDescription
        {
            get
            {
                return @"Battery Monitor helps you keep track of your laptop’s battery.
• Shows current battery percentage and charging status.
• Alerts you when battery goes above or below your chosen limits.
• Plays sound alerts which you can customize or mute.
• Runs in the system tray for quick access.

Upper and lower limits:
You can set your preferred maximum and minimum battery levels 
(e.g., warn me when above 80% or below 20%). The app checks these 
and notifies you.

Sound alerts:
When the limit is reached, a sound will play. 
You can mute or change the sound in settings.";
            }
        }

        /// <summary>
        /// Gets the product name from the assembly attributes.
        /// </summary>
        public string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        #endregion
    }
}