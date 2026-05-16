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
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            FadeOutAndSwitch(() => _parent?.ShowLevelView(_parent.CurrentOption.QueryId));
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Показываем загрузку
                LoadingOverlayContainer.Visibility = Visibility.Visible;
                LoadingOverlayContainer.Children.Clear();
                LoadingOverlayContainer.Children.Add(new LoadingOverlay("Записываем в базу данных..."));

                var variant = _parent.CurrentOption;

                // Валидация перед записью
                if (variant.QueryId == 0) throw new Exception("Данные запроса потеряны. Вернитесь назад.");
                if (variant.LocationId == 0) throw new Exception("Локация не выбрана.");

                // Сохранение (Option в твоей БД — это и есть вариант уровня)
                await _optionService.CreateAsync(
                    variant.QueryId,
                    variant.LocationId,
                    variant.TimeLimit
                );

                LoadingOverlayContainer.Visibility = Visibility.Collapsed;

                MessageBoxHelper.ShowSuccess("Вариант успешно добавлен в систему!");
                
                // Очищаем временные данные варианта после сохранения
                _parent.CurrentOption = new Models.OptionData(); 

                FadeOutAndSwitch(() => _parent?.ShowMainMenu());
            }
            catch (Exception ex)
            {
                LoadingOverlayContainer.Visibility = Visibility.Collapsed;
                MessageBoxHelper.ShowError($"Ошибка: {ex.Message}");
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