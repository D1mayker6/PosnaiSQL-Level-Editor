using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using PosnaiSQLauncher.Context;
using PosnaiSQLauncher.Services;
using PosnaiSQLauncher.Helpers;

namespace PosnaiSQLauncher
{
    public partial class LevelView : UserControl
    {
        private MainWindow _parent;
        private int _queryId;
        
        private readonly QueryService _queryService;
        private readonly LocationService _locationService;
        private int _selectedLocationId;
        private int _totalSeconds = 300;
        private bool _locationSelected = false;  

        public LevelView(MainWindow parent, int queryId)
        {
            InitializeComponent();
            _parent = parent;
            _queryId = queryId;

            var context = new AppDbContext();
            _queryService = new QueryService(context);
            _locationService = new LocationService(context);

            UpdateTimeDisplay();
    
            RestoreFromState();
        }
        
        private void RestoreFromState()
        {
            if (_parent?.CurrentOption != null && _parent.CurrentOption.LocationId > 0)
            {
                _selectedLocationId = _parent.CurrentOption.LocationId;
                _totalSeconds = _parent.CurrentOption.TimeLimit;
                _locationSelected = true;
                NextButton.IsEnabled = true;

                UpdateTimeDisplay();

                this.Loaded += (s, e) =>
                {
                    if (_selectedLocationId == 1)
                        AnimateBorder(CardDesert, (Color)ColorConverter.ConvertFromString("#2196F3"), 3);
                    else if (_selectedLocationId == 2)
                        AnimateBorder(CardForest, (Color)ColorConverter.ConvertFromString("#2196F3"), 3);
                };
            }
        }
        
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

        private void EnableNextButton()
        {
            NextButton.IsEnabled = _locationSelected;
        }

        private void CardDesert_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            AnimateBorder(CardDesert, (Color)ColorConverter.ConvertFromString("#2196F3"), 3);
            AnimateBorder(CardForest, (Color)ColorConverter.ConvertFromString("#E0E0E0"), 2);
            _selectedLocationId = 1;
            _locationSelected = true;  
            EnableNextButton();          
        }

        private void CardForest_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            AnimateBorder(CardForest, (Color)ColorConverter.ConvertFromString("#2196F3"), 3);
            AnimateBorder(CardDesert, (Color)ColorConverter.ConvertFromString("#E0E0E0"), 2);
            _selectedLocationId = 2;
            _locationSelected = true;  
            EnableNextButton();          
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

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            int dbId = _parent.CurrentOption.DatabaseId;
            string queryMode = _parent.CurrentOption.QueryMode ?? "new"; 

            if (dbId > 0)
            {
                FadeOutAndSwitch(() => _parent?.ShowQueryView(dbId, queryMode));
            }
            else
            {
                var errorBox = new CustomMessageBox(
                    "Ошибка: потерян идентификатор базы данных.", 
                    "Ошибка", 
                    CustomMessageBoxType.Error
                );
                errorBox.ShowDialog();
            }
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_locationSelected)
                {
                    MessageBoxHelper.ShowWarning("Выберите локацию");
                    return;
                }

                if (_parent == null)
                {
                    MessageBoxHelper.ShowError("Parent is null");
                    return;
                }

                if (_parent.CurrentOption == null)
                {
                    MessageBoxHelper.ShowError("CurrentVariant is null");
                    return;
                }

                _parent.CurrentOption.LocationId = _selectedLocationId;
                _parent.CurrentOption.LocationName = _selectedLocationId == 1 ? "Пустыня" : "Лес";
                _parent.CurrentOption.TimeLimit = _totalSeconds;

                System.Diagnostics.Debug.WriteLine($"QueryId: {_parent.CurrentOption.QueryId}");
                System.Diagnostics.Debug.WriteLine($"LocationId: {_parent.CurrentOption.LocationId}");
                System.Diagnostics.Debug.WriteLine($"TimeLimit: {_parent.CurrentOption.TimeLimit}");

                FadeOutAndSwitch(() => 
                {
                    if (_parent != null)
                    {
                        _parent.ShowSaveView();
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBoxHelper.ShowError($"Ошибка: {ex.Message}\n\n{ex.StackTrace}");
            }
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
                try
                {
                    switchAction?.Invoke();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error during navigation: {ex.Message}\n\n{ex.StackTrace}", "Navigation Error");
                }
            };
            storyboard.Begin(this);
        }
    }
}