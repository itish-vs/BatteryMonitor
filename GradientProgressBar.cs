using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BatteryMonitor.Controls
{
    /// <summary>
    /// A custom progress bar with animated gradient fill.
    /// </summary>
    public class GradientProgressBar : Control
    {
        #region Fields

        private int value = 0;
        private int max = 100;
        private int animationTarget = 0;
        private System.Windows.Forms.Timer animationTimer = new System.Windows.Forms.Timer();

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="GradientProgressBar"/> class.
        /// </summary>
        public GradientProgressBar()
        {
            this.DoubleBuffered = true;
            this.Height = 24;
            animationTimer.Interval = 15;
            animationTimer.Tick += AnimationTimer_Tick;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the current value of the progress bar.
        /// </summary>
        [Category("Behavior")]
        public int Value
        {
            get => value;
            set
            {
                animationTarget = Math.Max(0, Math.Min(Maximum, value));
                if (!animationTimer.Enabled)
                    animationTimer.Start();
            }
        }

        /// <summary>
        /// Gets or sets the maximum value of the progress bar.
        /// </summary>
        [Category("Behavior")]
        public int Maximum
        {
            get => max;
            set { max = Math.Max(1, value); Invalidate(); }
        }

        /// <summary>
        /// Gets or sets the start color of the gradient.
        /// </summary>
        [Category("Appearance")]
        public Color GradientStart { get; set; } = Color.LightSeaGreen;

        /// <summary>
        /// Gets or sets the end color of the gradient.
        /// </summary>
        [Category("Appearance")]
        public Color GradientEnd { get; set; } = Color.MediumPurple;

        #endregion

        #region Events

        /// <summary>
        /// Handles the animation timer tick event to animate the progress bar value.
        /// </summary>
        private void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            if (value < animationTarget)
                value++;
            else if (value > animationTarget)
                value--;
            else
                animationTimer.Stop();

            Invalidate();
        }

        #endregion

        #region Painting

        /// <summary>
        /// Paints the progress bar with gradient fill and percentage text.
        /// </summary>
        /// <param name="e">Paint event arguments.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(0, 0, Width, Height);

            using (SolidBrush backBrush = new SolidBrush(Color.Beige))
                e.Graphics.FillRectangle(backBrush, rect);

            int fillWidth = (int)((float)value / max * Width);
            if (fillWidth > 0)
            {
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    new Rectangle(0, 0, fillWidth, Height),
                    GradientStart, GradientEnd, LinearGradientMode.Horizontal))
                {
                    e.Graphics.FillRectangle(brush, 0, 0, fillWidth, Height);
                }
            }

            string percent = $"{(int)((float)value / max * 100)}%";
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            e.Graphics.DrawString(percent, Font, Brushes.Black, rect, sf);
        }

        #endregion
    }
}