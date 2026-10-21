using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace BibleSlideshow
{
    public partial class MainWindow : Window
    {
        private readonly List<Verse> _structuredBible = new();
        private PresenterWindow? _presenterWindow;
        private ImageBrush? _currentBgImage;
        private int _monitorIndex = 1;
        private readonly string[] _books = { "Genesis", "Exodus", "Leviticus", "Numbers", "Deuteronomy", "Joshua", "Judges", "Ruth", "1 Samuel", "2 Samuel", "1 Kings", "2 Kings", "1 Chronicles", "2 Chronicles", "Ezra", "Nehemiah", "Esther", "Job", "Psalms", "Proverbs", "Ecclesiastes", "Song of Solomon", "Isaiah", "Jeremiah", "Lamentations", "Ezekiel", "Daniel", "Hosea", "Joel", "Amos", "Obadiah", "Jonah", "Micah", "Nahum", "Habakkuk", "Zephaniah", "Haggai", "Zechariah", "Malachi", "Matthew", "Mark", "Luke", "John", "Acts", "Romans", "1 Corinthians", "2 Corinthians", "Galatians", "Ephesians", "Philippians", "Colossians", "1 Thessalonians", "2 Thessalonians", "1 Timothy", "2 Timothy", "Titus", "Philemon", "Hebrews", "James", "1 Peter", "2 Peter", "1 John", "2 John", "3 John", "Jude", "Revelation" };

        public MainWindow()
        {
            InitializeComponent();
            cmbBooks.Items.Add("Select Book...");
            foreach (var book in _books) cmbBooks.Items.Add(book);
            cmbBooks.SelectedIndex = 0;
        }

        private void BtnLoadBible_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog { Filter = "Text Files|*.txt" };
            if (ofd.ShowDialog() == true)
            {
                lblStatusFile.Text = "Compiling scripture file index arrays...";
                _structuredBible.Clear();

                string[] lines = File.ReadAllLines(ofd.FileName);
                string pattern = @"^[^A-Za-z0-9]*([1-3]?\s*[A-Za-z\s]+?)\s+(\d+)[\s:]+(\d+)[^A-Za-z0-9]*(.+)$";

                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    string upper = line.ToUpper();
                    if (upper.StartsWith("KJV") || upper.StartsWith("KING JAMES") || upper.StartsWith("AKJV") || upper.StartsWith("BOOK OF")) continue;

                    Match match = Regex.Match(line, pattern);
                    if (match.Success)
                    
                    {
                        string rawBook = match.Groups[1].Value.Trim();
                        int chap = int.Parse(match.Groups[2].Value);
                        int verse = int.Parse(match.Groups[3].Value);
                        string text = match.Groups[4].Value.Trim();

                        string resolvedBook = ResolveBookName(rawBook);
                        if (!string.IsNullOrEmpty(resolvedBook))
                        {
                            _structuredBible.Add(new Verse { Book = resolvedBook, Chapter = chap, VerseNum = verse, FullRef = $"{resolvedBook} {chap}:{verse}", Text = text });
                        }
                    }
                }

                if (_structuredBible.Count == 0)
                {
                    System.Windows.MessageBox.Show("Could not parse any verses. Ensure standard formatting.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                lblStatusFile.Text = $"{_structuredBible.Count} verses compiled.";
                RunFilter();
            }
        }

        private string ResolveBookName(string input)
        {
            string cleaned = new string(input.ToLower().Where(char.IsLetterOrDigit).ToArray());
            foreach (var book in _books)
            {
                string bClean = new string(book.ToLower().Where(char.IsLetterOrDigit).ToArray());
                if (bClean == cleaned || bClean.StartsWith(cleaned)) return book;
            }
            return input.Trim();
        }

        private void CmbBooks_SelectionChanged(object sender, SelectionChangedEventArgs e) => RunFilter();
        private void TxtChap_TextChanged(object sender, TextChangedEventArgs e) => RunFilter();
        private void TxtVerse_TextChanged(object sender, TextChangedEventArgs e) => RunFilter();

        private void RunFilter()
        {
            if (_structuredBible.Count == 0 || cmbBooks == null) return;

            string targetBook = cmbBooks.SelectedIndex > 0 ? cmbBooks.SelectedItem?.ToString() ?? string.Empty : string.Empty;
            int.TryParse(txtChap.Text, out int targetChap);
            int.TryParse(txtVerse.Text, out int targetVerse);

            IEnumerable<Verse> filtered = _structuredBible;

            if (!string.IsNullOrEmpty(targetBook))
            {
                string b2 = new string(targetBook.ToLower().Where(char.IsLetterOrDigit).ToArray());
                filtered = filtered.Where(v =>
                {
                    string bookClean = new string(v.Book.ToLower().Where(char.IsLetterOrDigit).ToArray());
                    return bookClean == b2 || bookClean.StartsWith(b2);
                });
            }

            if (targetChap > 0) filtered = filtered.Where(v => v.Chapter == targetChap);
            if (targetVerse > 0) filtered = filtered.Where(v => v.VerseNum == targetVerse);

            listVerses.ItemsSource = filtered.Take(100).ToList();
        }

        private void ListVerses_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (listVerses.SelectedItem is Verse v) TransmitPayload(v.Text, v.FullRef);
        }

        private void TransmitPayload(string content, string reference)
        {
            lblPreviewRef.Text = reference;
            lblPreviewRef.Foreground = System.Windows.Media.Brushes.LightGreen;
            lblPreviewText.Text = $"\"{content}\"";

            if (_presenterWindow != null && _presenterWindow.IsLoaded)
                _presenterWindow.UpdateSlide(content, reference);
        }

        private void BtnShowSermonTitle_Click(object sender, RoutedEventArgs e)
        {
            string title = string.IsNullOrWhiteSpace(txtSubject.Text) ? "WELCOME" : txtSubject.Text.ToUpper();
            lblPreviewRef.Text = "DISPLAYING SERMON SLIDE";
            lblPreviewRef.Foreground = System.Windows.Media.Brushes.Orange;
            lblPreviewText.Text = title;

            if (_presenterWindow != null && _presenterWindow.IsLoaded)
                _presenterWindow.UpdateSlide(title, "");
        }

        private void BtnLoadBg_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog { Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp" };
            if (ofd.ShowDialog() == true)
            {
                var bitmap = new BitmapImage(new Uri(ofd.FileName));
                _currentBgImage = new ImageBrush(bitmap) { Stretch = Stretch.UniformToFill };
                
                pnlPreview.Background = _currentBgImage;
                if (_presenterWindow != null && _presenterWindow.IsLoaded)
                    _presenterWindow.SetBackground(_currentBgImage);
            }
        }

        private void BtnClearBg_Click(object sender, RoutedEventArgs e)
        {
            _currentBgImage = null;
            pnlPreview.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(34, 34, 34));
            if (_presenterWindow != null && _presenterWindow.IsLoaded)
                _presenterWindow.SetBackground(null);
        }

        private void BtnOpenPresenter_Click(object sender, RoutedEventArgs e)
        {
            if (_presenterWindow == null || !_presenterWindow.IsLoaded)
            {
                _presenterWindow = new PresenterWindow();
                _presenterWindow.Closed += (s, args) =>
                {
                    lblStatusWindow.Text = "🔴 Live Window Offline";
                    lblStatusWindow.Foreground = System.Windows.Media.Brushes.Red;
                    _presenterWindow = null;
                };

                // 1. Show the window first so Windows creates the handle
                _presenterWindow.Show(); 

                // 2. Calculate the monitor and move it
                _monitorIndex = GetPresenterMonitorIndex();
                MovePresenterToMonitor(_monitorIndex);
                
                if (_currentBgImage != null) _presenterWindow.SetBackground(_currentBgImage);

                lblStatusWindow.Text = "🟢 Live Window Connected";
                lblStatusWindow.Foreground = System.Windows.Media.Brushes.Green;
            }
            else
            {
                _presenterWindow.Focus();
            }
        }

        private void BtnChangeMonitor_Click(object sender, RoutedEventArgs e)
        {
            var screens = System.Windows.Forms.Screen.AllScreens;
            if (screens.Length == 0) return;

            _monitorIndex = (_monitorIndex + 1) % screens.Length;

            if (_presenterWindow == null || !_presenterWindow.IsLoaded)
            {
                BtnOpenPresenter_Click(sender, e);
                return;
            }

            MovePresenterToMonitor(_monitorIndex);
            _presenterWindow.Activate();
            _presenterWindow.Focus();
        }

        private void BtnClosePresenter_Click(object sender, RoutedEventArgs e)
        {
            if (_presenterWindow != null)
            {
                _presenterWindow.Close();
                _presenterWindow = null;
            }

            lblStatusWindow.Text = "🔴 Live Window Offline";
            lblStatusWindow.Foreground = System.Windows.Media.Brushes.Red;
        }

        private int GetCurrentMainWindowMonitorIndex()
        {
            var screens = System.Windows.Forms.Screen.AllScreens;
            if (screens.Length == 0) return 0;

            var centerPoint = new System.Windows.Point(
                this.Left + (this.ActualWidth > 0 ? this.ActualWidth : this.Width) / 2,
                this.Top + (this.ActualHeight > 0 ? this.ActualHeight : this.Height) / 2);

            for (int i = 0; i < screens.Length; i++)
            {
                var area = screens[i].WorkingArea;
                if (centerPoint.X >= area.Left && centerPoint.X < area.Right &&
                    centerPoint.Y >= area.Top && centerPoint.Y < area.Bottom)
                {
                    return i;
                }
            }

            return 0;
        }

        private int GetPresenterMonitorIndex()
        {
            var screens = System.Windows.Forms.Screen.AllScreens;
            if (screens.Length <= 1) return 0;

            return 1;
        }

        private void MovePresenterToMonitor(int index)
        {
            if (_presenterWindow == null) return;

            var screens = System.Windows.Forms.Screen.AllScreens;
            if (screens.Length == 0) return;

            var targetScreen = screens[Math.Min(index, screens.Length - 1)];
            _presenterWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            _presenterWindow.WindowState = WindowState.Normal;
            _presenterWindow.Left = targetScreen.WorkingArea.Left;
            _presenterWindow.Top = targetScreen.WorkingArea.Top;
            _presenterWindow.Width = targetScreen.WorkingArea.Width;
            _presenterWindow.Height = targetScreen.WorkingArea.Height;
            _presenterWindow.WindowState = WindowState.Maximized;
        }
    }

    public class Verse
    {
        public string Book { get; set; } = string.Empty;
        public int Chapter { get; set; }
        public int VerseNum { get; set; }
        public string FullRef { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }
}