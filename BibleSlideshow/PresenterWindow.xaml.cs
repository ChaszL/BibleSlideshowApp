using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

using Brush = System.Windows.Media.Brush;
using FlowDirection = System.Windows.FlowDirection;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace BibleSlideshow
{
    public partial class PresenterWindow : Window
    {
        // Fraction of the screen kept empty around the text (each side / top & bottom).
        private const double SideMarginFraction = 0.05;
        private const double VerticalMarginFraction = 0.07;

        // Reference size relative to the verse size, and the gap between them.
        private const double RefSizeRatio = 0.45;
        private const double RefGapRatio = 0.35;

        // Font size limits. Max is a fraction of screen height so one-word titles don't get absurd.
        private const double MinFontSize = 24;
        private const double MaxFontHeightFraction = 0.16;

        // How dark the background image gets behind the text (0 = untouched, 1 = black).
        private const double BackgroundDimOpacity = 0.45;

        public PresenterWindow()
        {
            InitializeComponent();
        }

        public void UpdateSlide(string text, string reference)
        {
            lblText.Text = text;
            lblRef.Text = reference;
            FitText();
        }

        public void SetBackground(Brush? brush)
        {
            this.Background = brush ?? System.Windows.Media.Brushes.Black;
            dimOverlay.Opacity = brush == null ? 0 : BackgroundDimOpacity;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e) => FitText();

        /// <summary>
        /// Picks the largest font size at which the verse and its reference fit on screen.
        /// It measures the real wrapped text rather than guessing from the character count.
        /// </summary>
        private void FitText()
        {
            double screenW = ActualWidth;
            double screenH = ActualHeight;
            if (screenW <= 0 || screenH <= 0) return;

            double availW = screenW * (1 - 2 * SideMarginFraction);
            double availH = screenH * (1 - 2 * VerticalMarginFraction);

            string text = lblText.Text ?? string.Empty;
            string reference = lblRef.Text ?? string.Empty;

            double maxSize = Math.Max(MinFontSize, screenH * MaxFontHeightFraction);

            // Binary search for the largest size that fits.
            double lo = MinFontSize, hi = maxSize;
            if (Fits(text, reference, hi, availW, availH))
            {
                lo = hi;
            }
            else
            {
                for (int i = 0; i < 20; i++)
                {
                    double mid = (lo + hi) / 2;
                    if (Fits(text, reference, mid, availW, availH)) lo = mid; else hi = mid;
                }
            }

            double size = Math.Floor(lo);
            lblText.MaxWidth = availW;
            lblText.FontSize = size;
            lblRef.FontSize = Math.Max(MinFontSize, size * RefSizeRatio);
            lblRef.Margin = new Thickness(0, string.IsNullOrEmpty(reference) ? 0 : size * RefGapRatio, 0, 0);
        }

        private bool Fits(string text, string reference, double size, double availW, double availH)
        {
            // No single word may be wider than the line, or it would be split mid-word.
            foreach (string word in text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            {
                if (Measure(word, lblText, size, double.PositiveInfinity).Width > availW) return false;
            }

            double totalH = Measure(text, lblText, size, availW).Height;

            if (!string.IsNullOrEmpty(reference))
            {
                double refSize = Math.Max(MinFontSize, size * RefSizeRatio);
                FormattedText refFt = Measure(reference, lblRef, refSize, double.PositiveInfinity);
                if (refFt.Width > availW) return false;
                totalH += size * RefGapRatio + refFt.Height;
            }

            return totalH <= availH;
        }

        private FormattedText Measure(string s, System.Windows.Controls.TextBlock style, double size, double maxWidth)
        {
            var typeface = new Typeface(style.FontFamily, style.FontStyle, style.FontWeight, style.FontStretch);
            var ft = new FormattedText(
                s.Length == 0 ? " " : s,
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                typeface,
                size,
                System.Windows.Media.Brushes.White,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);

            if (!double.IsInfinity(maxWidth))
            {
                ft.MaxTextWidth = maxWidth;
                ft.TextAlignment = TextAlignment.Center;
            }
            return ft;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) this.Close();
        }
    }
}
