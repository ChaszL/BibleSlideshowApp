using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

using Brush = System.Windows.Media.Brush;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace BibleSlideshow
{
    public partial class PresenterWindow : Window
    {
        public PresenterWindow()
        {
            InitializeComponent();
        }

        public void UpdateSlide(string text, string reference)
        {
            lblText.Text = text;
            lblRef.Text = reference;
            UpdateTextSize(text);
        }

        private void UpdateTextSize(string text)
        {
            int length = text?.Length ?? 0;

            // Formula: Start at 125pt, and smoothly subtract 0.35pt per character
            double calculatedSize = 125 - (length * 0.35);

            // Clamp the size between a safe TV minimum (46pt) and an impactful maximum (115pt)
            // Math.Clamp(value, min, max) ensures it never goes outside these boundaries
            lblText.FontSize = Math.Clamp(calculatedSize, 46, 115);
        }
        public void SetBackground(Brush? brush)
        {
            this.Background = brush ?? System.Windows.Media.Brushes.Black;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) this.Close();
        }
    }
}