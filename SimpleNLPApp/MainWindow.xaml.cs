using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SimpleNLPApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void buttonNewModel_Click(object sender, RoutedEventArgs e)
        {
            ModelWindow modelWindow = new ModelWindow();
            modelWindow.Show();
            modelWindow.Owner = this;
            this.Visibility = Visibility.Collapsed;
        }
    }
}