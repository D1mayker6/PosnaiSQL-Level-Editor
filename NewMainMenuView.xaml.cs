using System.Windows;
using System.Windows.Controls;

namespace PosnaiSQLauncher
{
    public partial class NewMainMenuView : UserControl
    {
        private NewMainWindow _parent;

        public NewMainMenuView(NewMainWindow parent)
        {
            InitializeComponent();
            _parent = parent;
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            _parent.ShowCreateOption();
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