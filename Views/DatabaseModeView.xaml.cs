using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace PosnaiSQLauncher
{
    public partial class DatabaseModeView : UserControl
    {
        private string _selectedOption = null;
        private MainWindow _parent;

        public DatabaseModeView(MainWindow parent)
        {
            InitializeComponent();
            _parent = parent;

            this.Loaded += DatabaseModeView_Loaded;
        }
        
        private void DatabaseModeView_Loaded(object sender, RoutedEventArgs e)
        {
            if (_parent?.CurrentOption != null && !string.IsNullOrEmpty(_parent.CurrentOption.DatabaseMode))
            {
                string savedMode = _parent.CurrentOption.DatabaseMode;
                if (savedMode == "create")
                {
                    SelectCard(Card1, Card2, "create");
                }
                else if (savedMode == "existing")
                {
                    SelectCard(Card2, Card1, "existing");
                }
            }
        }

        private void Card1_Click(object sender, MouseButtonEventArgs e)
        {
            SelectCard(Card1, Card2, "create");
            ResetDatabaseState();
        }

        private void Card2_Click(object sender, MouseButtonEventArgs e)
        {
            SelectCard(Card2, Card1, "existing");
            ResetDatabaseState();
        }
        
        private void ResetDatabaseState()
        {
            if (_parent?.CurrentOption != null)
            {
                _parent.CurrentOption.DatabaseId = 0;
                _parent.CurrentOption.DatabaseName = null;
                _parent.CurrentOption.DatabaseSchemaImage = null;
        
                _parent.CurrentOption.QueryId = 0;
                _parent.CurrentOption.QueryName = null;
                _parent.CurrentOption.QueryCondition = null;
                _parent.CurrentOption.QueryString = null;
            }
        }

        private void SelectCard(Border selectedCard, Border otherCard, string option)
        {
            AnimateBorder(otherCard, 
                new SolidColorBrush(Color.FromArgb(255, 224, 224, 224)), 
                new Thickness(2));

            AnimateBorder(selectedCard, 
                new SolidColorBrush(Color.FromArgb(255, 33, 150, 243)), 
                new Thickness(3));

            _selectedOption = option;
            NextButton.IsEnabled = true;
        }

        private void AnimateBorder(Border border, SolidColorBrush targetBrush, Thickness targetThickness)
        {
            var colorAnimation = new ColorAnimation
            {
                To = targetBrush.Color,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            var thicknessAnimation = new ThicknessAnimation
            {
                To = targetThickness,
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
            FadeOutAndSwitch(() => _parent.ShowMainMenu());
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedOption))
            {
                MessageBox.Show("Выберите способ настройки БД", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _parent.CurrentOption.DatabaseMode = _selectedOption;
            FadeOutAndSwitch(() => _parent.ShowDatabaseConfig(_selectedOption));
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

            storyboard.Completed += (s, e) =>
            {
                switchAction();
            };

            storyboard.Begin(this);
        }
    }
}