using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace PosnaiSQLauncher
{
    public partial class NewEditOptionView : UserControl
    {
        private NewMainWindow _parent;

        public NewEditOptionView(NewMainWindow parent)
        {
            InitializeComponent();
            _parent = parent;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            FadeOutAndSwitch(() => _parent.ShowMainMenu());
        }

        private void FadeOutAndSwitch(System.Action switchAction)
        {
            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = new System.TimeSpan(0, 0, 0, 0, 300)
            };

            var storyboard = new Storyboard();
            Storyboard.SetTargetProperty(fadeOut, new System.Windows.PropertyPath("Opacity"));
            storyboard.Children.Add(fadeOut);

            storyboard.Completed += (s, e) =>
            {
                switchAction();
            };

            storyboard.Begin(this);
        }
    }
}