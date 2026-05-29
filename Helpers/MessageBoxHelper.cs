using System.Windows;

namespace PosnaiSQLauncher.Helpers
{
    public static class MessageBoxHelper
    {
        public static void ShowInfo(string message, string title = "Информация")
        {
            var msgBox = new CustomMessageBox(message, title, CustomMessageBoxType.Ok);
            msgBox.ShowDialog();
        }


        public static void ShowError(string message, string title = "Ошибка")
        {
            var msgBox = new CustomMessageBox(message, title, CustomMessageBoxType.Error);
            msgBox.ShowDialog();
        }
        
        public static void ShowSuccess(string message, string title = "Успешно!")
        {
            var msgBox = new CustomMessageBox(message, title, CustomMessageBoxType.Success);
            msgBox.ShowDialog();
        }
        
        public static void ShowWarning(string message, string title = "Внимание")
        {
            var msgBox = new CustomMessageBox(message, title, CustomMessageBoxType.Warning);
            msgBox.ShowDialog();
        }
        
        public static MessageBoxResult ShowQuestion(string message, string title = "Вопрос")
        {
            var msgBox = new CustomMessageBox(message, title, CustomMessageBoxType.YesNo);
            msgBox.ShowDialog();
            return msgBox.Result;
        }
        
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