// Helpers/MessageBoxHelper.cs
using System.Windows;

namespace PosnaiSQLauncher.Helpers
{
    public static class MessageBoxHelper
    {
        /// <summary>
        /// Показать простое сообщение (ОК)
        /// </summary>
        public static void ShowInfo(string message, string title = "Информация")
        {
            var msgBox = new CustomMessageBox(message, title, CustomMessageBoxType.Ok);
            msgBox.ShowDialog();
        }

        /// <summary>
        /// Показать сообщение об ошибке
        /// </summary>
        public static void ShowError(string message, string title = "Ошибка")
        {
            var msgBox = new CustomMessageBox(message, title, CustomMessageBoxType.Error);
            msgBox.ShowDialog();
        }

        /// <summary>
        /// Показать сообщение об успехе
        /// </summary>
        public static void ShowSuccess(string message, string title = "Успешно!")
        {
            var msgBox = new CustomMessageBox(message, title, CustomMessageBoxType.Success);
            msgBox.ShowDialog();
        }

        /// <summary>
        /// Показать предупреждение
        /// </summary>
        public static void ShowWarning(string message, string title = "Внимание")
        {
            var msgBox = new CustomMessageBox(message, title, CustomMessageBoxType.Warning);
            msgBox.ShowDialog();
        }

        /// <summary>
        /// Показать вопрос Да/Нет
        /// </summary>
        public static MessageBoxResult ShowQuestion(string message, string title = "Вопрос")
        {
            var msgBox = new CustomMessageBox(message, title, CustomMessageBoxType.YesNo);
            msgBox.ShowDialog();
            return msgBox.Result;
        }

        /// <summary>
        /// Показать подтверждение удаления
        /// </summary>
        public static MessageBoxResult ShowDeleteConfirmation(string itemName)
        {
            var msgBox = new CustomMessageBox(
                $"Вы уверены, что хотите безвозвратно удалить:\n\n«{itemName}» ?",
                "Удаление",
                CustomMessageBoxType.DeleteConfirm
            );
            msgBox.ShowDialog();
            return msgBox.Result;
        }
    }
}