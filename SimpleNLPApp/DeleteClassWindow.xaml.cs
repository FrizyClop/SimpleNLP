using System.Windows;
using System.Windows.Controls;
using SimpleNLP;

namespace SimpleNLPApp
{
    /// <summary>
    /// Логика взаимодействия для DeleteClassWindow.xaml
    /// </summary>
    public partial class DeleteClassWindow : Window
    {
        List<Text> _list;
        ComboBox _comboBox;

        public DeleteClassWindow(ComboBox cb,List<Text> list)
        {
            InitializeComponent();
            CopyInfoToList(cb);
            _list = list;
            _comboBox = cb;
        }

        private void CopyInfoToList(ComboBox cb)
        {
            for (int i = 2; i < cb.Items.Count; i++)
            {
                ListBoxClasses.Items.Add(cb.Items[i]);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if(ListBoxClasses.SelectedItems.Count == 0)
            {
                MessageBox.Show("Выберите хотя бы один класс для удаления!");
                return;
            }
            foreach(var item in ListBoxClasses.SelectedItems)
            {
                _comboBox.Items.Remove(item.ToString());
                DeleteClassInTexts(item.ToString());
            }
            MessageBox.Show("Классы удалены!");
            Close();
        }

        private void DeleteClassInTexts(string _class)
        {
            foreach (Text text in _list) 
            {
                if (text.ClassOfText == _class) text.ClassOfText = null;
            }
        }

        private void CancleButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
