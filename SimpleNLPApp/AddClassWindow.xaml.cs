using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SimpleNLPApp
{
    /// <summary>
    /// Логика взаимодействия для AddClassWindow.xaml
    /// </summary>
    public partial class AddClassWindow : Window
    {
        ComboBox _comboBox;

        public AddClassWindow(ComboBox cb)
        {
            InitializeComponent();
            _comboBox = cb;
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
            MessageBox.Show("Класс '" + name_of_class + "' успешно добавлен.");
            this.Close();
        }
    }
}
