// QueryModeView.xaml.cs
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace PosnaiSQLauncher
{
    public partial class QueryModeView : UserControl
    {
        private string _selectedMode = null;
        private MainWindow _parent;
        private int _databaseId;

        public QueryModeView(MainWindow parent, int databaseId)
        {
            InitializeComponent();
            _parent = parent;
            _databaseId = databaseId;
        }

        private void CardNew_Click(object sender, MouseButtonEventArgs e)
        {
            SelectCard(CardNew, CardExisting, "new");
        }

        private void CardExisting_Click(object sender, MouseButtonEventArgs e)
        {
            SelectCard(CardExisting, CardNew, "existing");
        }

        private void SelectCard(Border selectedCard, Border otherCard, string mode)
        {
            var activeColor = (Color)ColorConverter.ConvertFromString("#2196F3");
            var inactiveColor = (Color)ColorConverter.ConvertFromString("#E0E0E0");

            AnimateBorder(selectedCard, activeColor, 3);
            AnimateBorder(otherCard, inactiveColor, 2);

            _selectedMode = mode;
            NextButton.IsEnabled = true;
        }

        private void AnimateBorder(Border border, Color targetColor, double targetThickness)
        {
            var colorAnimation = new ColorAnimation
            {
                To = targetColor,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            var thicknessAnimation = new ThicknessAnimation
            {
                To = new Thickness(targetThickness),
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            var animatedBrush = new SolidColorBrush(((SolidColorBrush)border.BorderBrush).Color);
            border.BorderBrush = animatedBrush;

            animatedBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
            border.BeginAnimation(Border.BorderThicknessProperty, thicknessAnimation);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            FadeOutAndSwitch(() => _parent.ShowDatabaseConfig("existing"));
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedMode))
            {
                MessageBox.Show("Выберите режим работы с запросом", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            FadeOutAndSwitch(() => _parent.ShowQueryView(_databaseId, _selectedMode));
        }

        private void FadeOutAndSwitch(Action switchAction)
        {
            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300)
            };

            var storyboard = new Storyboard();
            Storyboard.SetTargetProperty(fadeOut, new PropertyPath("Opacity"));
            storyboard.Children.Add(fadeOut);

            storyboard.Completed += (s, e) => { switchAction(); };

            storyboard.Begin(this);
        }
    }
}