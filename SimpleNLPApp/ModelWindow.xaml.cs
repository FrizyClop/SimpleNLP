using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Input;
using SimpleNLP;
using System.Windows.Controls;

namespace SimpleNLPApp
{
    /// <summary>
    /// Логика взаимодействия для ModelWindow.xaml
    /// </summary>
    public partial class ModelWindow : Window
    {
        List<Text> texts;

        public ModelWindow()
        {
            InitializeComponent();
            texts = new List<Text>();
            ListBoxTrainTexts.ItemsSource = texts;
            ComboBoxClasses.Items.Add("Все");
            ComboBoxClasses.SelectedIndex = 0;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if(Owner.OwnedWindows.Count == 0)
            Owner.Visibility = Visibility.Visible;
        }

        private void MenuItemNewModel_Click(object sender, RoutedEventArgs e)
        {
            ModelWindow mw = new ModelWindow();
            mw.Owner = Owner;
            mw.Show();
        }

        private void MenuItemAddText_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog file_dialog = new OpenFileDialog();
            file_dialog.Multiselect = true;
            file_dialog.ShowDialog();
            if (file_dialog.FileNames == null) return;

            string[] file_paths = file_dialog.FileNames;
            foreach(string file_path in file_paths)
            {
                texts.Add(GetText(file_path));
            }

            ListBoxTrainTexts.Items.Refresh();
        }

        private Text GetText(string file_path)
        {
            StreamReader sr = new StreamReader(file_path);
            string text = sr.ReadToEnd();
            sr.Close();
            return new Text(file_path, text);
        }

        private void MenuItemAddClass_Click(object sender, RoutedEventArgs e)
        {
            AddClassWindow acw = new AddClassWindow(ComboBoxClasses);
            acw.ShowDialog();
        }

        private void ComboBoxClasses_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ContextMenuSetClass_Click(object sender, RoutedEventArgs e)
        {
            if(ComboBoxClasses.Items.Count == 1)
            {
                MessageBox.Show("Добавьте хотя бы один класс!");
                return;
            }
            SetClassWindow scw = new SetClassWindow(ComboBoxClasses,ListBoxTrainTexts);
            scw.ShowDialog();
            ListBoxTrainTexts.Items.Refresh();
        }
    }
}
