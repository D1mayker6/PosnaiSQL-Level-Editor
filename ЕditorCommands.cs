using System.Windows.Input;

public static class EditorCommands
{
    public static readonly RoutedUICommand SaveVariant =
        new RoutedUICommand(
            "Сохранить вариант",          
            nameof(SaveVariant),          
            typeof(EditorCommands),      
            new InputGestureCollection    
            {
                new KeyGesture(Key.S, ModifierKeys.Control)
            });
}