using System.Windows;
using System.Windows.Controls;
using SimpleNLP;

namespace SimpleNLPApp
{
    /// <summary>
    /// Логика взаимодействия для SetClassWindow.xaml
    /// </summary>
    public partial class SetClassWindow : Window
    {
        ListBox _listBox;

        public SetClassWindow(ComboBox cb, ListBox list)
        {
            InitializeComponent();
            string[] temp = new string[cb.Items.Count];
            cb.Items.CopyTo(temp,1); //проблемы здесь, надо сделать свой CopyTo
            ComboBoxOnWindow.ItemsSource = temp;
            ComboBoxOnWindow.SelectedIndex = 0;
            _listBox = list;
        }

        private void ButtonAccept_Click(object sender, RoutedEventArgs e)
        {
            if (ComboBoxOnWindow.SelectedItem == null || ComboBoxOnWindow.Text.Trim() == "")
            {
                MessageBox.Show("Выберите один из добавленных классов!");
                return;
            }
            foreach(var item in _listBox.SelectedItems.Cast<Text>())
            {
                item.ClassOfText = ComboBoxOnWindow.Text;
            }
            MessageBox.Show("Классы текстов изменены");
        }
    }
}
