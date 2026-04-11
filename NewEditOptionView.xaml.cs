using System.Windows.Controls;

namespace PosnaiSQLauncher;

public partial class NewEditOptionView : UserControl
{
    private NewMainWindow _parent;
    public NewEditOptionView(NewMainWindow parent)
    {
        InitializeComponent();
        _parent = parent;
    }
}