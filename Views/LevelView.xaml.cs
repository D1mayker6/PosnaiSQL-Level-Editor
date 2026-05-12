// LevelView.xaml.cs
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using PosnaiSQLauncher.Context;
using PosnaiSQLauncher.Services;

namespace PosnaiSQLauncher
{
    public partial class LevelView : UserControl
    {
        private MainWindow _parent;
        private int _queryId;
        
        private readonly QueryService _queryService;
        private int _selectedLocationId;
        private int _totalSeconds = 300;

        public LevelView(MainWindow parent, int queryId = 0)
        {
            InitializeComponent();
            _parent = parent;
            _queryId = queryId;

            var context = new AppDbContext();
            _queryService = new QueryService(context);

            UpdateTimeDisplay();
            
            // По умолчанию выбираем пустыню
            AnimateBorder(CardDesert, (Color)ColorConverter.ConvertFromString("#2196F3"), 3);
            _selectedLocationId = 1;
        }

        // ===== ТАЙМЕР =====
        private void TimePlus5_Click(object sender, RoutedEventArgs e) => AddTime(5);
        private void TimePlus30_Click(object sender, RoutedEventArgs e) => AddTime(30);
        private void TimeMinus5_Click(object sender, RoutedEventArgs e) => AddTime(-5);
        private void TimeMinus30_Click(object sender, RoutedEventArgs e) => AddTime(-30);

        private void AddTime(int s)
        {
            _totalSeconds = Math.Max(0, Math.Min(3595, _totalSeconds + s));
            UpdateTimeDisplay();
        }

        private void UpdateTimeDisplay()
        {
            MinDisplay.Text = (_totalSeconds / 60).ToString("D2");
            SecDisplay.Text = (_totalSeconds % 60).ToString("D2");
        }

        // ===== ВЫБОР ЛОКАЦИИ =====
        private void CardDesert_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            AnimateBorder(CardDesert, (Color)ColorConverter.ConvertFromString("#2196F3"), 3);
            AnimateBorder(CardForest, (Color)ColorConverter.ConvertFromString("#E0E0E0"), 2);
            _selectedLocationId = 1;
        }

        private void CardForest_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            AnimateBorder(CardForest, (Color)ColorConverter.ConvertFromString("#2196F3"), 3);
            AnimateBorder(CardDesert, (Color)ColorConverter.ConvertFromString("#E0E0E0"), 2);
            _selectedLocationId = 2;
        }

        private void AnimateBorder(Border border, Color targetColor, double targetThickness)
        {
            ColorAnimation colorAnim = new ColorAnimation 
            { 
                To = targetColor, 
                Duration = TimeSpan.FromMilliseconds(300) 
            };
            
            ThicknessAnimation thickAnim = new ThicknessAnimation 
            { 
                To = new Thickness(targetThickness), 
                Duration = TimeSpan.FromMilliseconds(300) 
            };

            if (border.BorderBrush is SolidColorBrush currentBrush)
            {
                if (currentBrush.IsFrozen) border.BorderBrush = currentBrush.Clone();
                border.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);
            }
            else
            {
                border.BorderBrush = new SolidColorBrush(Colors.Transparent);
                border.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);
            }

            border.BeginAnimation(Border.BorderThicknessProperty, thickAnim);
        }

        // ===== НАВИГАЦИЯ =====
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            FadeOutAndSwitch(() => _parent?.ShowQueryView());
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            FadeOutAndSwitch(() => _parent?.ShowSaveView());
        }

        private void FadeOutAndSwitch(Action switchAction)
        {
            var fadeOut = new DoubleAnimation { From = 1, To = 0, Duration = TimeSpan.FromMilliseconds(300) };
            var storyboard = new Storyboard();
            Storyboard.SetTargetProperty(fadeOut, new PropertyPath("Opacity"));
            storyboard.Children.Add(fadeOut);
            storyboard.Completed += (s, e) => { switchAction(); };
            storyboard.Begin(this);
        }
    }
}