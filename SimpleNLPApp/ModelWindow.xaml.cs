using Microsoft.Win32;
using System.IO;
using System.Windows;
using SimpleNLP;
using System.Windows.Controls;
using SimpleNLP.Classification;
using SimpleNLP.Preprocessing;
using SimpleNLP.Transformation;
using System.Text.Json;
using System.Net.NetworkInformation;

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
        MethodOfOneForm methodOfOneForm = MethodOfOneForm.LEMMATIZATOR;
        TfIdfVectorizer tfIdfVectorizer = new TfIdfVectorizer();
        MainWindow main_window_link;

        public MainWindow MainWindow { get { return main_window_link; } set {  main_window_link = value; } }

        #region Инициализация

        public ModelWindow(string name, NaiveBayesParameters param)
        {
            InitializeComponent();
            name_of_model = name;
            model = new NaiveBayesClassifier(param.Alpha);
            model_type = PredictModels.NaiveBayes;
            FirstOpen();
        }

        public ModelWindow(string name, LogisticRegressionParameters param)
        {
            InitializeComponent();
            name_of_model = name;
            model = new LogisticRegression(param.LearningRate, param.Epochs);
            model_type = PredictModels.LogisticRegression;
            FirstOpen();
        }

        public ModelWindow(string name, SVMParameters param)
        {
            InitializeComponent();
            name_of_model = name;
            model = new SVMClassifier(param.MaxIterations, param.LearningRate,param.Lambda);
            model_type = PredictModels.SVM;
            FirstOpen();
        }

        public ModelWindow(string name, KNNParameters param)
        {
            InitializeComponent();
            name_of_model = name;
            model = new KNNClassifier(param.K);
            model_type = PredictModels.KNN;
            FirstOpen();
        }

        public ModelWindow(string name, string json_representation)
        {
            InitializeComponent();
            name_of_model = name;
            model = LoadModelFromJson(json_representation);
            if (model == null) throw new Exception("Не удалось загрузить модель. Возможно json файл не подходит для данной программы.");
            LoadOpen();
        }

        private void FirstOpen()
        {
            this.Title = name_of_model;
            texts = new List<Text>();
            classes = new List<string>();
            ListBoxTrainTexts.ItemsSource = texts;
            LoadClassesIntoComboBox();
            ShowTypeOfModel();
            ShowParametersOfModel();
        }

        private void LoadOpen()
        {
            this.Title = name_of_model;
            texts = new List<Text>();
            classes = model.Classes;
            LoadClassesIntoComboBox();
            ShowTypeOfModel();
            ShowParametersOfModel();
            ModelWasTrained();
        }

        private void ShowTypeOfModel()
        {
            switch (model_type)
            {
                case PredictModels.NaiveBayes:
                    LabelModel.Content += " Наивный Байес";
                    break;
                case PredictModels.LogisticRegression:
                    LabelModel.Content += " Логистическая регрессия";
                    break;
                case PredictModels.SVM:
                    LabelModel.Content += " Метод опорных векторов (SVM)";
                    break;
                case PredictModels.KNN:
                    LabelModel.Content += " KNN (k-ближайших соседей)";
                    break;
            }
        }

        private void ShowParametersOfModel()
        {
            switch (model_type) 
            {
                case PredictModels.NaiveBayes:
                    NaiveBayesClassifier nb_model = (NaiveBayesClassifier)model;
                    LabelNaiveBayesAlpha.Content = LabelNaiveBayesAlpha.Content.ToString() + nb_model.Alpha;
                    NaiveBayesParameters.Visibility = Visibility.Visible;
                    break;
                case PredictModels.LogisticRegression:
                    LogisticRegression lr_model = (LogisticRegression)model;
                    LogisticRegressionLearningRate.Content = LogisticRegressionLearningRate.Content.ToString() + lr_model.LearningRate;
                    LogisticRegressionEpochs.Content = LogisticRegressionEpochs.Content.ToString() + lr_model.Epochs;
                    LogisticRegressionParameters.Visibility = Visibility.Visible;
                    break;
                case PredictModels.SVM:
                    SVMClassifier svm_model = (SVMClassifier)model;
                    SVMLearningRate.Content = SVMLearningRate.Content.ToString() + svm_model.LearningRate;
                    SVMMaxIterations.Content = SVMMaxIterations.Content.ToString() + svm_model.MaxIterations;
                    SVMLambda.Content = SVMLambda.Content.ToString() + svm_model.Lambda;
                    SVMParameters.Visibility = Visibility.Visible;
                    break;
                case PredictModels.KNN:
                    KNNClassifier knn_model = (KNNClassifier)model;
                    LabelKNNk.Content = LabelKNNk.Content.ToString() + knn_model.K;
                    KNNParameters.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void LoadClassesIntoComboBox()
        {
            ComboBoxClasses.Items.Add("Все");
            ComboBoxClasses.Items.Add("-");
            foreach (string cls in classes)
            {
                ComboBoxClasses.Items.Add(cls);
            }
            ComboBoxClasses.SelectedIndex = 0;
        }

        #endregion

        private void Window_Closed(object sender, EventArgs e)
        {
            main_window_link.ChildLinks.Remove(this);
            if(main_window_link.ChildLinks.Count == 0)
            main_window_link.Visibility = Visibility.Visible;
        }

        private void MenuItemNewModel_Click(object sender, RoutedEventArgs e)
        {
            main_window_link.Visibility = Visibility.Visible;
            main_window_link.Activate();
            main_window_link.ChangeInterface(1);
        }

        private void MenuItemAddText_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog file_dialog = new OpenFileDialog();
            file_dialog.Filter = "Text|*.txt";
            file_dialog.Multiselect = true;
            file_dialog.InitialDirectory = Properties.Settings.Default.StringLastPathToTexts;
            file_dialog.ShowDialog();
            if (file_dialog.FileNames == null) return;

            // Сохраняем путь к папке, в которой была выбрана модель
            Properties.Settings.Default.StringLastPathToTexts = Path.GetDirectoryName(file_dialog.FileName);
            Properties.Settings.Default.Save(); // Сохраняем настройки

            string[] file_paths = file_dialog.FileNames;
            foreach(string file_path in file_paths)
            {
                texts.Add(GetText(file_path));
            }
            ListBoxTrainTexts.Items.Refresh();
        }

        private void MenuItemAddTextWithout_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog file_dialog = new OpenFileDialog();
            file_dialog.Filter = "Text|*.txt";
            file_dialog.Multiselect = true;
            file_dialog.InitialDirectory = Properties.Settings.Default.StringLastPathToTextsWithout;
            file_dialog.ShowDialog();
            if (file_dialog.FileNames == null) return;

            // Сохраняем путь к папке, в которой была выбрана модель
            Properties.Settings.Default.StringLastPathToTextsWithout = Path.GetDirectoryName(file_dialog.FileName);
            Properties.Settings.Default.Save(); // Сохраняем настройки

            string[] file_paths = file_dialog.FileNames;
            foreach (string file_path in file_paths)
            {
                Text text = GetText(file_path);
                text.IsPreprocessed = true;
                texts.Add(text);
            }

            ListBoxTrainTexts.Items.Refresh();
        }

        public Text GetText(string file_path)
        {
            StreamReader sr = new StreamReader(file_path);
            string text = sr.ReadToEnd();
            sr.Close();
            return new Text(file_path, text, true);
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
                {
                    ListBoxTrainTexts.ItemsSource = null;
                    ListBoxTrainTexts.Items.Clear();
                }
                ListBoxTrainTexts.ItemsSource = texts;
                ListBoxTrainTexts.Items.Refresh();
            }
            else if (ComboBoxClasses.SelectedIndex == 1)
            {
                ListBoxTrainTexts.ItemsSource = null;
                ListBoxTrainTexts.Items.Clear();
                foreach (Text text in texts)
                {
                    if (text.ClassOfText == null)
                        ListBoxTrainTexts.Items.Add(text);
                }
            }
            else if (ComboBoxClasses.SelectedIndex == -1)
            {
                return;
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

        private void ContextMenuAddDeletePreprocessing_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckingForText() || !ChekingForSelectedTexts()) return;
            foreach (Text item in ListBoxTrainTexts.SelectedItems)
                if (item.IsPreprocessed)
                    item.IsPreprocessed = false;
                else
                    item.IsPreprocessed = true;
            ListBoxTrainTexts.Items.Refresh();
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
            if (ComboBoxClasses.Items.Count == 2)
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

        public static bool CheckInternetConnection()
        {
            try
            {
                // Проверяем доступность популярного стабильного ресурса (Google DNS)
                using (var ping = new Ping())
                {
                    var reply = ping.Send("8.8.8.8", 3000); // 3 секунды таймаут
                    return reply?.Status == IPStatus.Success;
                }
            }
            catch
            {
                return false;
            }
        }

        #endregion

        private async void MenuItemFit_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckingForText()) return;
            if (!ChekingAllTextsWithClasses()) return;

            CanvasProgress.Visibility = Visibility.Visible;
            UpperMenu.IsEnabled = false;
            ListBoxTrainTexts.IsEnabled = false;
            ComboBoxClasses.IsEnabled = false;

            await Task.Run(() =>
            {
                List<List<string>> preproccessed_texts = new List<List<string>>();
                int i = 1;
                double val_of_one_text = 50 / texts.Count;
                foreach (Text text in texts)
                {
                    Dispatcher.Invoke(() => LabelProgress.Content = "Подготовка текста №" + i);
                    if (text.IsPreprocessed)
                        preproccessed_texts.Add(Preprocessor.Preprocess(text.Content, MethodOfOneForm.NONE));
                    else
                        preproccessed_texts.Add(Preprocessor.Preprocess(text.Content, methodOfOneForm));
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
            UpperMenu.IsEnabled = true;
            ComboBoxClasses.IsEnabled = true;
            ModelWasTrained();
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
            if (!model.IsTrained)
            {
                MessageBox.Show("Невозможно провести классификацию. Сначала обучите модель.");
                return;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Text files|*.txt",
                Multiselect = true
            };

            if (!(bool)openFileDialog.ShowDialog()) return;

            List<TextEvaluationViewModel> items = new List<TextEvaluationViewModel>();

            foreach (var file in openFileDialog.FileNames)
            {
                var text = GetText(file);
                items.Add(new TextEvaluationViewModel
                {
                    Title = text.Title,
                    Content = text.Content,
                    TrueClass = null,
                    PredictedClass = null
                });
            }

            var window = new ClassificationTextsWindow(items, model, tfIdfVectorizer, classes);
            window.ShowDialog();
        }

        private void MenuItemUnloadModel_Click(object sender, RoutedEventArgs e)
        {
            if (!model.IsTrained)
            {
                MessageBox.Show("Модель не может быть выгружена. Для начала проведите тренировку.");
                return;
            }
            SaveModel();
        }

        public void SaveModel()
        {
            // Создаем диалог сохранения файла
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "SNLP files (*.snlp)|*.snlp",
                FileName = name_of_model + ".snlp",
                DefaultExt = ".snlp",
                AddExtension = true
            };

            // Показываем диалог и ждем выбора файла
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var combinedData = new
                    {
                        Model = JsonSerializer.Deserialize<JsonElement>(model.GetJsonRepresentation()),
                        Vectorizer = JsonSerializer.Deserialize<JsonElement>(tfIdfVectorizer.GetJsonRepresentation())
                    };

                    string json = JsonSerializer.Serialize(combinedData, new JsonSerializerOptions { WriteIndented = true });

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

        public PredictModel LoadModelFromJson(string json)
        {
            try
            {
                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;
                var model_json = root.GetProperty("Model");
                var vectorizer_json = root.GetProperty("Vectorizer");

                tfIdfVectorizer.LoadFromJson(vectorizer_json);

                if (model_json.GetProperty("Model").GetString() == "NaiveBayes")
                {
                    model_type = PredictModels.NaiveBayes;
                    return new NaiveBayesClassifier(model_json);
                }
                else if (model_json.GetProperty("Model").GetString() == "LogisticRegression")
                {
                    model_type = PredictModels.LogisticRegression;
                    return new LogisticRegression(model_json);
                }
                else if (model_json.GetProperty("Model").GetString() == "SVM")
                {
                    model_type = PredictModels.SVM;
                    return new SVMClassifier(model_json);
                }
                else if (model_json.GetProperty("Model").GetString() == "KNN")
                {
                    model_type = PredictModels.KNN;
                    return new KNNClassifier(model_json);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии модели:\n{ex.Message}", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
            return null;
        }

        protected void ModelWasTrained()
        {
            ListBoxTrainTexts.IsEnabled = false;
            MenuItemClass.IsEnabled = false;
            MenuItemAddText.IsEnabled = false;
            MenuItemAddTextWithout.IsEnabled = false;
            MenuItemFit.IsEnabled = false;
        }

        private void MenuItemPreprocessTexts_Click(object sender, RoutedEventArgs e)
        {
            PreprocessingTextWindow ptw = new PreprocessingTextWindow();
            ptw.ShowDialog();
        }

        private void ComboBoxOneForm_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

        private void MenuItemCloseModel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MenuItemOpenModel_Click(object sender, RoutedEventArgs e)
        {
            main_window_link.ButtonOpenModel_Click(sender,e);
        }

        private void MenuItemDownloadNews_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckInternetConnection())
            {
                MessageBox.Show("Для использования данной функции требуется интернет-соединение.");
                return;
            }
            NewsWindow nw = new NewsWindow(classes,texts,model);
            nw.ShowDialog();
            ComboBoxClasses.Items.Clear();
            LoadClassesIntoComboBox();
            ListBoxTrainTexts.Items.Refresh();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ListBoxTrainTexts.SelectAll();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ListBoxTrainTexts.UnselectAll();
        }

        private void MenuItemHelp_Click(object sender, RoutedEventArgs e)
        {
            HelpWindow.ShowSingleton();
        }
    }
}