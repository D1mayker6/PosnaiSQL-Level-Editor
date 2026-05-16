using System.Windows.Controls;

namespace PosnaiSQLauncher
{
    public partial class LoadingOverlay : UserControl
    {
        public LoadingOverlay(string loadingText = "Загрузка...")
        {
            InitializeComponent();
            LoadingText.Text = loadingText;
        }
    }
}