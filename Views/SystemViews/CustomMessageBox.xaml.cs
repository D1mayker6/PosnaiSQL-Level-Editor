// CustomMessageBox.xaml.cs
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media; // ← ВАЖНО

namespace PosnaiSQLauncher
{
    public partial class CustomMessageBox : Window
    {
        public MessageBoxResult Result { get; set; } = MessageBoxResult.None;

        public CustomMessageBox(string message, string title = "Уведомление", CustomMessageBoxType type = CustomMessageBoxType.Ok)
        {
            InitializeComponent();
            
            MessageBlock.Text = message;
            TitleBlock.Text = title;
            
            SetupByType(type);
        }

        private void SetupByType(CustomMessageBoxType type)
        {
            switch (type)
            {
                case CustomMessageBoxType.Ok:
                    SetupOkButton();
                    break;

                case CustomMessageBoxType.YesNo:
                    SetupYesNoButtons();
                    break;

                case CustomMessageBoxType.OkCancel:
                    SetupOkCancelButtons();
                    break;

                case CustomMessageBoxType.Success:
                    SetupSuccess();
                    break;

                case CustomMessageBoxType.Error:
                    SetupError();
                    break;

                case CustomMessageBoxType.Warning:
                    SetupWarning();
                    break;

                case CustomMessageBoxType.DeleteConfirm:
                    SetupDeleteConfirm();
                    break;
            }
        }

        // ← ИСПРАВЛЕННЫЕ МЕТОДЫ
        private void SetupOkButton()
        {
            IconBlock.Text = "ℹ️";
            IconBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 33, 150, 243)); // #2196F3
            Button1.Content = "ОК";
            Button1.Style = (Style)FindResource("PrimaryButtonStyle");
            Button2.Visibility = Visibility.Collapsed;

            GridColumn2Definition();
        }

        private void SetupYesNoButtons()
        {
            IconBlock.Text = "❓";
            IconBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 152, 0)); // #FF9800
            Button1.Content = "Нет";
            Button1.Style = (Style)FindResource("SecondaryButtonStyle");
            Button2.Content = "Да";
            Button2.Style = (Style)FindResource("PrimaryButtonStyle");
            Button2.Visibility = Visibility.Visible;

            GridTwoColumns();
        }

        private void SetupOkCancelButtons()
        {
            IconBlock.Text = "⚠️";
            IconBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 152, 0)); // #FF9800
            Button1.Content = "Отмена";
            Button1.Style = (Style)FindResource("SecondaryButtonStyle");
            Button2.Content = "ОК";
            Button2.Style = (Style)FindResource("PrimaryButtonStyle");
            Button2.Visibility = Visibility.Visible;

            GridTwoColumns();
        }

        private void SetupSuccess()
        {
            IconBlock.Text = "✓";
            IconBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 56, 142, 60)); // #388E3C
            TitleBlock.Text = "Успешно!";
            Button1.Content = "ОК";
            Button1.Style = (Style)FindResource("PrimaryButtonStyle");
            Button2.Visibility = Visibility.Collapsed;

            GridColumn2Definition();
        }

        private void SetupError()
        {
            IconBlock.Text = "✕";
            IconBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 229, 57, 53)); // #E53935
            TitleBlock.Text = "Ошибка";
            Button1.Content = "ОК";
            Button1.Style = (Style)FindResource("PrimaryButtonStyle");
            Button2.Visibility = Visibility.Collapsed;

            GridColumn2Definition();
        }

        private void SetupWarning()
        {
            IconBlock.Text = "⚠️";
            IconBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 152, 0)); // #FF9800
            TitleBlock.Text = "Внимание";
            Button1.Content = "ОК";
            Button1.Style = (Style)FindResource("PrimaryButtonStyle");
            Button2.Visibility = Visibility.Collapsed;

            GridColumn2Definition();
        }

        private void SetupDeleteConfirm()
        {
            IconBlock.Text = "🗑";
            IconBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 229, 57, 53)); // #E53935
            TitleBlock.Text = "Удаление";
            Button1.Content = "Отмена";
            Button1.Style = (Style)FindResource("SecondaryButtonStyle");
            Button2.Content = "Удалить";
            Button2.Foreground = new SolidColorBrush(Color.FromArgb(255, 229, 57, 53));
            Button2.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 229, 57, 53));
            Button2.Style = (Style)FindResource("SecondaryButtonStyle");
            Button2.Visibility = Visibility.Visible;

            GridTwoColumns();
        }

        private void GridColumn2Definition()
        {
            var gridDef = (Grid)LogicalTreeHelper.FindLogicalNode(this, "ButtonPanel");
            if (gridDef != null && gridDef.ColumnDefinitions.Count == 3)
            {
                gridDef.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                gridDef.ColumnDefinitions[1].Width = new GridLength(0);
                gridDef.ColumnDefinitions[2].Width = new GridLength(0);
            }
        }

        private void GridTwoColumns()
        {
            // Оставляем оба столбца видимыми
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.No;
            Close();
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Yes;
            Close();
        }
    }

    public enum CustomMessageBoxType
    {
        Ok,
        YesNo,
        OkCancel,
        Success,
        Error,
        Warning,
        DeleteConfirm
    }
}