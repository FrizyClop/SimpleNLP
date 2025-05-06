using System.Windows;
using System.Windows.Controls;

namespace SimpleNLPApp
{
    /// <summary>
    /// Логика взаимодействия для AddClassWindow.xaml
    /// </summary>
    public partial class AddClassWindow : Window
    {
        ComboBox _comboBox;
        List<string> link_to_list;

        public AddClassWindow(ComboBox cb, List<string> classes)
        {
            InitializeComponent();
            _comboBox = cb;
            link_to_list = classes;
        }

        private void AddClassButton_Click(object sender, RoutedEventArgs e)
        {
            string name_of_class = TextBoxClass.Text.Trim();
            if (name_of_class == null || name_of_class == "")
            {
                MessageBox.Show("Поле 'Имя класса' не должно быть пустым!");
                return;
            }

            int id = _comboBox.Items.IndexOf(name_of_class);
            if (id != -1)
            {
                MessageBox.Show("Такой класс уже присутствует!");
                return;
            }
            _comboBox.Items.Add(name_of_class);
            link_to_list.Add(name_of_class);
            MessageBox.Show("Класс '" + name_of_class + "' успешно добавлен.");
            this.Close();
        }
    }
}
