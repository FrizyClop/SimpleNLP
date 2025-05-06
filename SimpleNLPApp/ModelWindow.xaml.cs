using Microsoft.Win32;
using System.IO;
using System.Windows;
using SimpleNLP;
using System.Windows.Controls;
using SimpleNLP.Classification;
using SimpleNLP.Preprocessing;
using SimpleNLP.Transformation;
using System.Text.Json;

namespace SimpleNLPApp
{
    /// <summary>
    /// Логика взаимодействия для ModelWindow.xaml
    /// </summary>
    public partial class ModelWindow : Window
    {
        List<Text> texts;
        List<string> classes;
        PredictModel model;
        string name_of_model;
        PredictModels model_type;
        TfIdfVectorizer tfIdfVectorizer;

        public ModelWindow(string name, double alpha)
        {
            InitializeComponent();
            name_of_model = name;
            model = new NaiveBayesClassifier(alpha);
            model_type = PredictModels.NaiveBayes;
            FirstOpen();
        }

        public ModelWindow(string name, double learning_rate, int epochs)
        {
            InitializeComponent();
            name_of_model = name;
            model = new LogisticRegression(learning_rate, epochs);
            model_type = PredictModels.LogisticRegression;
            FirstOpen();
        }

        private void FirstOpen()
        {
            this.Title = name_of_model;
            texts = new List<Text>();
            classes = new List<string>();
            ListBoxTrainTexts.ItemsSource = texts;
            ComboBoxClasses.Items.Add("Все");
            ComboBoxClasses.SelectedIndex = 0;
            switch (model_type) 
            {
                case PredictModels.NaiveBayes:
                    LabelModel.Content += " Наивный Байес";
                    break;
                case PredictModels.LogisticRegression:
                    LabelModel.Content += " Логистическая регрессия";
                    break;
            }
            tfIdfVectorizer = new TfIdfVectorizer();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if(Owner.OwnedWindows.Count == 0)
            Owner.Visibility = Visibility.Visible;
        }

        private void MenuItemNewModel_Click(object sender, RoutedEventArgs e)
        {
            //добавить реализацию
        }

        private void MenuItemAddText_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog file_dialog = new OpenFileDialog();
            file_dialog.Filter = "Text|*.txt";
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
            AddClassWindow acw = new AddClassWindow(ComboBoxClasses,classes);
            acw.ShowDialog();
        }

        private void MenuItemDeleteClass_Click(object sender, RoutedEventArgs e)
        {
            if (!ChekingForClass()) return;
            DeleteClassWindow dcw = new DeleteClassWindow(ComboBoxClasses, texts);
            dcw.ShowDialog();
            ListBoxTrainTexts.Items.Refresh();
        }

        private void ComboBoxClasses_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxClasses.SelectedIndex == 0)
            {
                if (ListBoxTrainTexts.Items.Count > 0)
                    ListBoxTrainTexts.Items.Clear();
                ListBoxTrainTexts.ItemsSource = texts;
                ListBoxTrainTexts.Items.Refresh();
            }
            else
            {
                ListBoxTrainTexts.ItemsSource = null;
                ListBoxTrainTexts.Items.Clear();
                foreach(Text text in texts)
                {
                    if (text.ClassOfText == ComboBoxClasses.SelectedItem.ToString())
                        ListBoxTrainTexts.Items.Add(text);
                }
            }
        }

        private void ContextMenuSetClass_Click(object sender, RoutedEventArgs e)
        {
            if (!ChekingForClass()) return;
            if (!CheckingForText() || !ChekingForSelectedTexts()) return;
            SetClassWindow scw = new SetClassWindow(ComboBoxClasses,ListBoxTrainTexts);
            scw.ShowDialog();
            ListBoxTrainTexts.Items.Refresh();
        }

        private void ContextMenuDeleteText_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckingForText() || !ChekingForSelectedTexts()) return;
            MessageBoxResult res = MessageBox.Show("Вы действительно хотите удалить выбранные тексты?", "Удаление", MessageBoxButton.YesNo);
            if (res == MessageBoxResult.Yes)
            {
                List<Text> deleted_texts = new List<Text>();
                foreach (Text item in ListBoxTrainTexts.SelectedItems)
                    if (texts.Contains(item))
                    {
                        deleted_texts.Add(item);
                    }
                foreach(Text item in deleted_texts)
                    if (texts.Contains(item))
                    {
                        texts.Remove(item);
                    }
                ListBoxTrainTexts.Items.Refresh();
            }
        }

        #region Проверки

        /// <summary>
        /// Проверка на наличие хотя бы одного добавленного текста
        /// </summary>
        /// <returns>Правда если есть хотя бы один тренировочный текст, ложь в обратном случае<returns>
        private bool CheckingForText()
        {
            if(ListBoxTrainTexts.Items.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы один текст.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Проверка на хотя бы один выбранный текст в ListBox
        /// </summary>
        /// <returns>Правда если хотя бы один текст выбран, ложь в обратном случае</returns>
        private bool ChekingForSelectedTexts()
        {
            if (ListBoxTrainTexts.SelectedItem == null)
            {
                MessageBox.Show("Выберите хотя бы один текст.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Проверка на наличие хотя бы одного класса (кроме класса Все)
        /// </summary>
        /// <returns>Правда если есть хотя бы один класс (кроме Все), ложь в обратном случае</returns>
        private bool ChekingForClass()
        {
            if (ComboBoxClasses.Items.Count == 1)
            {
                MessageBox.Show("Добавьте хотя бы один класс!");
                return false;
            }
            return true;
        }

        private bool ChekingAllTextsWithClasses()
        {
            foreach(Text text in texts)
            {
                if(text.ClassOfText == null)
                {
                    MessageBox.Show("Всем текстам должен быть назначен класс!");
                    return false;
                }
            }
            return true;
        }

        #endregion

        private async void MenuItemFit_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckingForText()) return;
            if (!ChekingAllTextsWithClasses()) return;

            CanvasProgress.Visibility = Visibility.Visible;

            await Task.Run(() =>
            {
                List<List<string>> preproccessed_texts = new List<List<string>>();
                int i = 1;
                double val_of_one_text = 50 / texts.Count;
                foreach (Text text in texts)
                {
                    Dispatcher.Invoke(() => LabelProgress.Content = "Подготовка текста №" + i);
                    preproccessed_texts.Add(Preprocessor.Preprocess(text.Content, MethodOfOneForm.LEMMATIZATOR));
                    Dispatcher.Invoke(() => ProgressBarFit.Value += val_of_one_text);
                    i++;
                }

                Dispatcher.Invoke(() => LabelProgress.Content = "Обучение векторизатора.");
                tfIdfVectorizer.Fit(preproccessed_texts);
                Dispatcher.Invoke(() => ProgressBarFit.Value += 28);

                Dispatcher.Invoke(() => LabelProgress.Content = "Векторизация текстов.");
                List<double[]> vectors = new List<double[]>();
                foreach (List<string> text in preproccessed_texts)
                {
                    vectors.Add(tfIdfVectorizer.Vectorize(text));
                }
                Dispatcher.Invoke(() => ProgressBarFit.Value += 22);

                Dispatcher.Invoke(() => LabelProgress.Content = "Обучение модели.");
                model.Fit(vectors, CreateYforTrain());
                Dispatcher.Invoke(() => ProgressBarFit.Value += 30);
            });

            CanvasProgress.Visibility= Visibility.Hidden;
            ProgressBarFit.Value = 0;
            MessageBox.Show("Модель обучена!");
        }

        private List<string> CreateYforTrain()
        {
            List<string> y = new List<string>();
            foreach(Text text in texts)
            {
                y.Add(text.ClassOfText);
            }
            return y;
        }

        private void MenuItemClassificationOfText_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text|*.txt";
            openFileDialog.Multiselect = false;
            openFileDialog.ShowDialog();
            Text classification_text = GetText(openFileDialog.FileName);

            List<string> tokens = Preprocessor.Preprocess(classification_text.Content);

            double[] vector = tfIdfVectorizer.Vectorize(tokens);

            string class_of_text = model.Predict(vector);

            Dictionary<string,double> probabilities = model.PredictProbabilities(vector);

            string result = "Класс загруженного текста: " + class_of_text;
            foreach (string _class in classes)
                result += $"\n Вероятность {_class}: {probabilities[_class]}";

            MessageBox.Show(result);
        }

        private void MenuItemUnloadModel_Click(object sender, RoutedEventArgs e)
        {
            SaveModel();
        }

        public void SaveModel()
        {
            // Создаем диалог сохранения файла
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                FileName = name_of_model + ".json",
                DefaultExt = ".json",
                AddExtension = true
            };

            // Показываем диалог и ждем выбора файла
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    string json = model.GetJsonRepresentation();

                    // Сохраняем в файл
                    File.WriteAllText(saveFileDialog.FileName, json);

                    MessageBox.Show("Модель успешно сохранена!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении модели:\n{ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
