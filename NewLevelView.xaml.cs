using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace PosnaiSQLauncher
{
    public partial class NewLevelView : UserControl
    {
        private NewMainWindow _parent;
        private string _selectedLevel = null;
        private int _totalSeconds = 60; 

        public NewLevelView(NewMainWindow parent)
        {
            InitializeComponent();
            _parent = parent;
            UpdateTimeDisplay();
        }

        private void CardDesert_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SelectCard(CardDesert, CardForest, "desert");
            ChangeBackground("#FFF8E1", "pack://application:,,,/Assets/desert_bg.png");
        }

        private void CardForest_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SelectCard(CardForest, CardDesert, "forest");
            ChangeBackground("#E8F5E9", "pack://application:,,,/Assets/forest_bg.png");
        }

        private void SelectCard(Border selectedCard, Border otherCard, string level)
        {
            _selectedLevel = level;
            NextButton.IsEnabled = true;
            AnimateBorder(selectedCard, new SolidColorBrush(Color.FromRgb(33, 150, 243)), 3);
            AnimateBorder(otherCard, new SolidColorBrush(Color.FromRgb(224, 224, 224)), 2);
        }

        // Логика кнопок
        private void TimePlus5_Click(object sender, RoutedEventArgs e) => AddTime(5);
        private void TimePlus30_Click(object sender, RoutedEventArgs e) => AddTime(30);
        private void TimeMinus5_Click(object sender, RoutedEventArgs e) => AddTime(-5);
        private void TimeMinus30_Click(object sender, RoutedEventArgs e) => AddTime(-30);

        private void AddTime(int seconds)
        {
            _totalSeconds += seconds;
            if (_totalSeconds < 0) _totalSeconds = 0;
            if (_totalSeconds > 3595) _totalSeconds = 3595; // Лимит ~60 мин
            UpdateTimeDisplay();
        }

        private void UpdateTimeDisplay()
        {
            int minutes = _totalSeconds / 60;
            int seconds = _totalSeconds % 60;
            MinDisplay.Text = minutes.ToString("D2");
            SecDisplay.Text = seconds.ToString("D2");
        }

        private void ChangeBackground(string colorHex, string imagePath)
        {
            var color = (Color)ColorConverter.ConvertFromString(colorHex);
            var colorAnim = new ColorAnimation { To = color, Duration = TimeSpan.FromMilliseconds(500) };
            if (BgOverlay.Background is SolidColorBrush brush)
            {
                var animatedBrush = brush.IsFrozen ? brush.Clone() : brush;
                BgOverlay.Background = animatedBrush;
                animatedBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);
            }
            try
            {
                BgTexture.Source = new BitmapImage(new Uri(imagePath));
                BgTexture.BeginAnimation(OpacityProperty, new DoubleAnimation(0.18, TimeSpan.FromMilliseconds(500)));
            }
            catch { }
        }

        private void AnimateBorder(Border border, SolidColorBrush targetColor, double thickness)
        {
            var colorAnim = new ColorAnimation { To = targetColor.Color, Duration = TimeSpan.FromMilliseconds(250) };
            var thickAnim = new ThicknessAnimation { To = new Thickness(thickness), Duration = TimeSpan.FromMilliseconds(250) };
            var brush = border.BorderBrush is SolidColorBrush b ? (b.IsFrozen ? b.Clone() : b) : targetColor;
            border.BorderBrush = brush;
            brush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);
            border.BeginAnimation(Border.BorderThicknessProperty, thickAnim);
        }

        private void Back_Click(object sender, RoutedEventArgs e) => FadeOutAndSwitch(() => _parent?.ShowQueryView());

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            // Здесь будет метод ShowOverviewView
            FadeOutAndSwitch(() => _parent?.ShowSaveView());
        }

        private void FadeOutAndSwitch(Action switchAction)
        {
            var fadeOut = new DoubleAnimation { From = 1, To = 0, Duration = TimeSpan.FromMilliseconds(300) };
            var storyboard = new Storyboard();
            Storyboard.SetTargetProperty(fadeOut, new PropertyPath("Opacity"));
            storyboard.Children.Add(fadeOut);
            storyboard.Completed += (s, e) => switchAction();
            storyboard.Begin(this);
        }
    }
}