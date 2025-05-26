using System.Windows;
using System.Windows.Controls;

namespace SimpleNLPApp
{
    /// <summary>
    /// Логика взаимодействия для HelpWindow.xaml
    /// </summary>
    public partial class HelpWindow : Window
    {
        private static HelpWindow _instance;

        public HelpWindow()
        {
            InitializeComponent();
            TreeViewItemGeneral.IsSelected = true;
        }

        public static void ShowSingleton()
        {
            if (_instance == null)
            {
                _instance = new HelpWindow();
                _instance.Closed += (s, e) => _instance = null;
                _instance.Show();
            }
            else
            {
                _instance.Activate(); // просто выводим уже открытое
            }
        }

        private void TreeHelp_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is TreeViewItem item && item.Tag is string key)
            {
                string text = Properties.Resources.ResourceManager.GetString(key);
                TextBlockHelp.Text = text ?? "(Раздел справки не найден)";
            }
        }
    }
}
