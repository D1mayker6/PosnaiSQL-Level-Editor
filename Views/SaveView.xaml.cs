using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using PosnaiSQLauncher.Context;
using PosnaiSQLauncher.Services;
using PosnaiSQLauncher.Helpers;

namespace PosnaiSQLauncher
{
    public partial class SaveView : UserControl
    {
        private MainWindow _parent;
        private readonly OptionService _optionService;

        public SaveView(MainWindow parent)
        {
            InitializeComponent();
            _parent = parent;

            var context = new AppDbContext();
            _optionService = new OptionService(context);

            this.Loaded += (s, e) => DisplaySummary();
        }

        private void DisplaySummary()
        {
            if (_parent?.CurrentOption == null) return;

            var variant = _parent.CurrentOption;
            
            if (FindName("DbNameTextBlock") is TextBlock dbText) 
                dbText.Text = variant.DatabaseName ?? "Не указано";

            if (FindName("QueryNameTextBlock") is TextBlock queryText) 
                queryText.Text = variant.QueryName ?? "Не указано";

            if (FindName("LocationTextBlock") is TextBlock locText) 
                locText.Text = variant.LocationName ?? "Не выбрана";

            if (FindName("TimeTextBlock") is TextBlock timeText)
            {
                int min = variant.TimeLimit / 60;
                int sec = variant.TimeLimit % 60;
                timeText.Text = $"{min:D2}:{sec:D2}";
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            int dbId = _parent.CurrentOption.DatabaseId;
            FadeOutAndSwitch(() => _parent?.ShowLevelView(dbId)); 
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadingOverlayContainer.Visibility = Visibility.Visible;
                LoadingOverlayContainer.Children.Clear();
                LoadingOverlayContainer.Children.Add(new LoadingOverlay("Записываем в базу данных..."));

                var variant = _parent.CurrentOption;

                if (variant.QueryId == 0) throw new Exception("Данные запроса потеряны. Вернитесь назад.");
                if (variant.LocationId == 0) throw new Exception("Локация не выбрана.");

                await _optionService.CreateAsync(
                    variant.QueryId,
                    variant.LocationId,
                    variant.TimeLimit
                );

                LoadingOverlayContainer.Visibility = Visibility.Collapsed;

                MessageBoxHelper.ShowSuccess("Вариант успешно добавлен в систему!");
                
                _parent.CurrentOption = new Models.OptionData(); 

                FadeOutAndSwitch(() => _parent?.ShowMainMenu());
            }
            catch (Exception ex)
            {
                LoadingOverlayContainer.Visibility = Visibility.Collapsed;
                MessageBoxHelper.ShowError($"Ошибка при сохранении: {ex.Message}");
            }
        }

        private void FadeOutAndSwitch(Action switchAction)
        {
            var fadeOut = new DoubleAnimation { From = 1, To = 0, Duration = TimeSpan.FromMilliseconds(300) };
            var storyboard = new Storyboard();
            Storyboard.SetTargetProperty(fadeOut, new PropertyPath("Opacity"));
            storyboard.Children.Add(fadeOut);
            storyboard.Completed += (s, e) => { switchAction?.Invoke(); };
            storyboard.Begin(this);
        }
    }
}