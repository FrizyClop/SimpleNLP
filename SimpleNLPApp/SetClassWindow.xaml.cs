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
            CopyInfoToComboBox(cb);
            ComboBoxOnWindow.SelectedIndex = 0;
            _listBox = list;
        }

        public void CopyInfoToComboBox(ComboBox cb)
        {
            for (int i = 2; i < cb.Items.Count; i++)
            {
                ComboBoxOnWindow.Items.Add(cb.Items[i]);
            }
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
            Close();
        }
    }
}
