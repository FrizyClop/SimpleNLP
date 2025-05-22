using System.IO;
using System.Text;
using System.Windows;
using Microsoft.Win32;
using SimpleNLP;
using SimpleNLP.Preprocessing;

namespace SimpleNLPApp
{
    /// <summary>
    /// Логика взаимодействия для PreprocessingTextWindow.xaml
    /// </summary>
    public partial class PreprocessingTextWindow : Window
    {
        string save_path = Properties.Settings.Default.StringSavePreprocessedTexts;
        MethodOfOneForm methodOfOneForm = MethodOfOneForm.LEMMATIZATOR;

        public PreprocessingTextWindow()
        {
            InitializeComponent();
            TextBoxSavePath.Text = save_path;
        }

        private Text GetText(string file_path)
        {
            StreamReader sr = new StreamReader(file_path);
            string text = sr.ReadToEnd();
            sr.Close();
            return new Text(file_path, text, true);
        }

        private void ButtonAddText_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog file_dialog = new OpenFileDialog();
            file_dialog.Filter = "Text|*.txt";
            file_dialog.Multiselect = true;
            file_dialog.ShowDialog();
            if (file_dialog.FileNames == null) return;

            string[] file_paths = file_dialog.FileNames;
            foreach (string file_path in file_paths)
            {
                ListBoxTrainTexts.Items.Add(file_path);
            }
        }

        private void ComboBoxOneForm_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            switch (ComboBoxOneForm.SelectedIndex)
            {
                case 0:
                    methodOfOneForm = MethodOfOneForm.LEMMATIZATOR;
                    break;
                case 1:
                    methodOfOneForm = MethodOfOneForm.STEMMER;
                    break;
                default:
                    MessageBox.Show("Выберите один из предложенных методов приведения слов к одной форме.");
                    return;
            }
        }

        private void ButtonSavePath_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog openFolderDialog = new OpenFolderDialog();
            if (openFolderDialog.ShowDialog() == false) return;
            if (openFolderDialog.FolderName == null || openFolderDialog.FolderName == "") return;

            Properties.Settings.Default.StringSavePreprocessedTexts = openFolderDialog.FolderName;
            Properties.Settings.Default.Save();
            TextBoxSavePath.Text = openFolderDialog.FolderName;
            save_path = openFolderDialog.FolderName;
        }

        private async void ButtonPreprocessTexts_Click(object sender, RoutedEventArgs e)
        {
            if (ListBoxTrainTexts.Items.Count == 0)
            {
                MessageBox.Show("Для начала добавьте тексты для предобработки!");
                return;
            }
            if (!Directory.Exists(TextBoxSavePath.Text) || TextBoxSavePath.Text == "")
            {
                MessageBox.Show("Выберите существующую папку для сохранения файлов!");
                return;
            }

            List<Text> texts = new();
            foreach (var str in ListBoxTrainTexts.Items)
            {
                if (!File.Exists(str.ToString()))
                {
                    MessageBox.Show($"Ошибка: файл {str.ToString()} не существует.");
                    return;
                }
                texts.Add(GetText(str.ToString()));
            }

            CanvasProgress.Visibility = Visibility.Visible;
            ListBoxTrainTexts.IsEnabled = false;
            ButtonAddText.IsEnabled = false;
            ButtonPreprocessTexts.IsEnabled = false;
            ButtonSavePath.IsEnabled = false;
            TextBoxSavePath.IsEnabled = false;
            ComboBoxOneForm.IsEnabled = false;

            await Task.Run(() =>
            {
                List<List<string>> preproccessed_texts = new List<List<string>>();
                int i = 1;
                double val_of_one_text = 100 / texts.Count;
                foreach (Text text in texts)
                {
                    Dispatcher.Invoke(() => LabelProgress.Content = "Подготовка текста №" + i);
                    preproccessed_texts.Add(Preprocessor.Preprocess(text.Content, methodOfOneForm));
                    Dispatcher.Invoke(() => ProgressBarFit.Value += val_of_one_text / 2);
                    i++;
                }
                i = 0;
                foreach (List<string> text in preproccessed_texts)
                {
                    Dispatcher.Invoke(() => LabelProgress.Content = $"Сохранение текста №{i + 1}");
                    string fileNameWithoutExt = Path.GetFileNameWithoutExtension(texts[i].Title);
                    string newFileName = $"{fileNameWithoutExt}_preprocessed.txt";
                    string outputFilePath = "";
                    int for_name = -1;
                    do
                    {
                        for_name++;
                        outputFilePath = Path.Combine(save_path, newFileName);
                        if (for_name != 0)
                        {
                            outputFilePath = outputFilePath.Remove(outputFilePath.LastIndexOf(".txt"));
                            outputFilePath = outputFilePath + $"({for_name}).txt";
                        }
                    }
                    while (File.Exists(outputFilePath));
                    File.WriteAllText(outputFilePath, JoinWords(text));
                    Dispatcher.Invoke(() => ProgressBarFit.Value += val_of_one_text / 2);
                    i++;
                }
            });

            CanvasProgress.Visibility = Visibility.Hidden;
            ListBoxTrainTexts.IsEnabled = true;
            ButtonAddText.IsEnabled = true;
            ButtonPreprocessTexts.IsEnabled = true;
            ButtonSavePath.IsEnabled = true;
            TextBoxSavePath.IsEnabled = true;
            ComboBoxOneForm.IsEnabled = true;
            ProgressBarFit.Value = 0;
            MessageBox.Show("Тексты подготовлены!");
            this.Close();
        }

        /// <summary>
        /// Объединяет список слов в одну строку с указанным разделителем.
        /// </summary>
        /// <param name="words">Список слов</param>
        /// <param name="separator">Разделитель (по умолчанию — пробел)</param>
        /// <returns>Объединенная строка</returns>
        private static string JoinWords(List<string> words, string separator = " ")
        {
            // Используем StringBuilder для эффективной конкатенации
            var result = new StringBuilder();

            for (int i = 0; i < words.Count; i++)
            {
                result.Append(words[i]);

                // Добавляем разделитель, кроме последнего слова
                if (i < words.Count - 1)
                {
                    result.Append(separator);
                }
            }

            return result.ToString();
        }

        private void MenuItemDeleteText_Click(object sender, RoutedEventArgs e)
        {
            if (ListBoxTrainTexts.Items.Count == 0)
            {
                MessageBox.Show("Для начала добавьте хотя бы один текст!");
                return;
            }
            MessageBoxResult res = MessageBox.Show("Вы действительно хотите удалить выбранные тексты?", "Удаление", MessageBoxButton.YesNo);
            if (res == MessageBoxResult.Yes)
            {
                List<Text> deleted_texts = new List<Text>();
                for (int i = 0; i < ListBoxTrainTexts.SelectedItems.Count; i++)
                {
                    ListBoxTrainTexts.Items.Remove(ListBoxTrainTexts.SelectedItems[i]);
                }
            }
        }
    }
}
