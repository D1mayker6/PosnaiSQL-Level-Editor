using System.Windows;
using System.Windows.Controls;

namespace PosnaiSQLauncher
{
    public partial class MainMenuView : UserControl
    {
        private MainWindow _parent;

        public MainMenuView(MainWindow parent)
        {
            InitializeComponent();
            _parent = parent;
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            _parent.ShowDatabaseModeView();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            _parent.ShowEditOption();
        }
        
        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            _parent.ShowExportView();
        }
    }
}