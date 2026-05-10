using System.Windows;
using System.Windows.Controls;

namespace PosnaiSQLauncher;

public partial class SettingsView : UserControl
{
    private NewMainWindow _parent;
    public SettingsView(NewMainWindow parent)
    {
        InitializeComponent();
        _parent = parent;
    }
}