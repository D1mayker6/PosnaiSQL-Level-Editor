using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace PosnaiSQLauncher
{
    public partial class NewMainMenuView : UserControl
    {
        private NewMainWindow _parent;

        public NewMainMenuView(NewMainWindow parent)
        {
            InitializeComponent();
            _parent = parent;
            
            this.Loaded += (s, e) =>
            {
                var window = Window.GetWindow(this) as NewMainWindow;
                if (window != null)
                {
                    window.StateChanged += Window_StateChanged;
                    window.SizeChanged += Window_SizeChanged;
                    UpdateScale(window);
                }
            };
        }

        private void Window_StateChanged(object sender, System.EventArgs e)
        {
            if (sender is NewMainWindow window)
            {
                UpdateScale(window);
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is NewMainWindow window)
            {
                UpdateScale(window);
            }
        }

        private void UpdateScale(NewMainWindow window)
        {
            double scale = 1.0;

            if (window.WindowState == WindowState.Maximized)
            {
                scale = 1.8;
            }
            else if (window.ActualWidth > 950 && window.ActualHeight > 750)
            {
                scale = 1.5;
            }
            else
            {
                scale = 1.0;
            }

            var scaleAnimation = new DoubleAnimation
            {
                From = (this.RenderTransform as ScaleTransform)?.ScaleX ?? 1.0,
                To = scale,
                Duration = new System.TimeSpan(0, 0, 0, 0, 300)
            };

            var easingFunction = new ExponentialEase();
            easingFunction.EasingMode = EasingMode.EaseOut;
            scaleAnimation.EasingFunction = easingFunction;

            if (this.RenderTransform is ScaleTransform scaleTransform)
            {
                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
            }
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            _parent.ShowCreateOption();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            _parent.ShowEditOption();
        }
    }
}