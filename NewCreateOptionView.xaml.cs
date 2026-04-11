using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace PosnaiSQLauncher
{
    public partial class NewCreateOptionView : UserControl
    {
        private string _selectedOption = null;
        private NewMainWindow _parent;

        public NewCreateOptionView(NewMainWindow parent)
        {
            InitializeComponent();
            _parent = parent;
        }

        private void Card1_Click(object sender, MouseButtonEventArgs e)
        {
            SelectCard(Card1, Card2, "create");
        }

        private void Card2_Click(object sender, MouseButtonEventArgs e)
        {
            SelectCard(Card2, Card1, "existing");
        }

        private void SelectCard(Border selectedCard, Border otherCard, string option)
        {
            // Анимация снятия выделения с другой карточки
            AnimateBorder(otherCard, 
                new SolidColorBrush(Color.FromArgb(255, 224, 224, 224)), 
                new Thickness(2));

            // Анимация выделения выбранной карточки
            AnimateBorder(selectedCard, 
                new SolidColorBrush(Color.FromArgb(255, 33, 150, 243)), 
                new Thickness(3));

            _selectedOption = option;
            NextButton.IsEnabled = true;
        }

        private void AnimateBorder(Border border, SolidColorBrush targetBrush, Thickness targetThickness)
        {
            // Анимация цвета обводки
            var colorAnimation = new ColorAnimation
            {
                To = targetBrush.Color,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            // Анимация толщины обводки
            var thicknessAnimation = new ThicknessAnimation
            {
                To = targetThickness,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            // Создаём новую кисть для анимации
            var animatedBrush = new SolidColorBrush(((SolidColorBrush)border.BorderBrush).Color);
            border.BorderBrush = animatedBrush;

            // Запускаем анимации
            animatedBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
            border.BeginAnimation(Border.BorderThicknessProperty, thicknessAnimation);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            FadeOutAndSwitch(() => _parent.ShowMainMenu());
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            // Переход на следующий шаг
            // TODO: Логика перехода на следующий шаг в зависимости от _selectedOption
        }

        private void FadeOutAndSwitch(System.Action switchAction)
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