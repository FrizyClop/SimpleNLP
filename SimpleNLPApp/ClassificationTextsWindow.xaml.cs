using Microsoft.Win32;
using SimpleNLP.Classification;
using SimpleNLP.Preprocessing;
using SimpleNLP.Transformation;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using SimpleNLP;
using System.Windows.Input;
using System.Text.Json;
using System.Windows.Controls;

namespace SimpleNLPApp
{
    /// <summary>
    /// Логика взаимодействия для ClassificationTextsWindow.xaml
    /// </summary>
    public partial class ClassificationTextsWindow : Window
    {
        public ICommand RemoveTextCommand { get; }
        public ObservableCollection<TextEvaluationViewModel> Texts { get; set; }
        private PredictModel model;
        private TfIdfVectorizer tfIdfVectorizer;
        public List<string> Classes { get; }
        private ClassificationMetrics lastMetrics;

        public ClassificationTextsWindow(List<TextEvaluationViewModel> texts,
                                         PredictModel model,
                                         TfIdfVectorizer vectorizer,
                                         List<string> classes)
        {
            InitializeComponent();
            this.Texts = new ObservableCollection<TextEvaluationViewModel>(texts);
            this.model = model;
            this.tfIdfVectorizer = vectorizer;
            this.Classes = classes;

            this.RemoveTextCommand = new RelayCommand(RemoveText);
            DataContext = this;
        }

        private void OnClassifyClick(object sender, RoutedEventArgs e)
        {
            foreach (var item in Texts)
            {
                var tokens = Preprocessor.Preprocess(item.Content);
                var vector = tfIdfVectorizer.Vectorize(tokens);
                item.PredictedClass = model.Predict(vector);
                item.ClassProbabilities = model.PredictProbabilities(vector); // ← здесь
            }

            var yTrue = Texts.Where(t => t.TrueClass != null).Select(t => t.TrueClass).ToList();
            var yPred = Texts.Where(t => t.TrueClass != null).Select(t => t.PredictedClass).ToList();

            if (yTrue.Count == yPred.Count && yTrue.Count > 0)
            {
                var mainClass = yTrue.First();
                lastMetrics = ClassificationMetrics.Evaluate(yTrue, yPred, mainClass);
                LabelAccuracy.Content = "Accuracy: " + lastMetrics.Accuracy;
                LabelPrecision.Content = "Precision: " + lastMetrics.Precision;
                LabelRecall.Content = "Recall: " + lastMetrics.Recall;
                LabelF1.Content = "F1Score: " + lastMetrics.F1Score;
            }
            else
            {
                lastMetrics = null;
                LabelAccuracy.Content = "Accuracy: ";
                LabelPrecision.Content = "Precision: ";
                LabelRecall.Content = "Recall: ";
                LabelF1.Content = "F1Score: ";
                MessageBox.Show("Не для всех текстов указаны реальные классы.");
            }
        }

        private void OnAddTextsClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Text files|*.txt",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (var file in openFileDialog.FileNames)
                {
                    var text = GetText(file); // или свой метод загрузки
                    Texts.Add(new TextEvaluationViewModel
                    {
                        Title = text.Title,
                        Content = text.Content,
                        TrueClass = null,
                        PredictedClass = null
                    });
                }
            }
        }

        private void OnExportMetricsClick(object sender, RoutedEventArgs e)
        {
            if (lastMetrics == null)
            {
                MessageBox.Show("Сначала проведите классификацию с заполненными реальными метками.");
                return;
            }

            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "S.NLP Метрики|*.snlpmetrics",
                DefaultExt = "snlpmetrics",
                FileName = "metrics.snlpmetrics"
            };

            if (saveDialog.ShowDialog() == true)
            {
                var json = JsonSerializer.Serialize(lastMetrics, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(saveDialog.FileName, json);
                MessageBox.Show("Метрики успешно экспортированы.");
            }
        }

        private void RemoveText(object obj)
        {
            if (obj is TextEvaluationViewModel text)
            {
                Texts.Remove(text);
            }
        }

        public Text GetText(string file_path)
        {
            StreamReader sr = new StreamReader(file_path);
            string text = sr.ReadToEnd();
            sr.Close();
            return new Text(file_path, text, true);
        }

        private void OnAssignClassToSelected(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item && item.Header is string selectedClass)
            {
                foreach (var selected in ListBoxTexts.SelectedItems)
                {
                    if (selected is TextEvaluationViewModel vm)
                        vm.TrueClass = selectedClass;
                }
            }
        }

    }

    public class TextEvaluationViewModel : INotifyPropertyChanged
    {
        public string Title { get; set; }
        public string Content { get; set; }

        private Dictionary<string, double> _classProbabilities;
        public Dictionary<string, double> ClassProbabilities
        {
            get => _classProbabilities;
            set
            {
                _classProbabilities = value;
                OnPropertyChanged(nameof(ClassProbabilities));
            }
        }

        private string _trueClass;
        public string TrueClass
        {
            get => _trueClass;
            set { _trueClass = value; OnPropertyChanged(nameof(TrueClass)); }
        }

        private string _predictedClass;
        public string PredictedClass
        {
            get => _predictedClass;
            set { _predictedClass = value; OnPropertyChanged(nameof(PredictedClass)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> execute;
        private readonly Predicate<object> canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => canExecute == null || canExecute(parameter);
        public void Execute(object parameter) => execute(parameter);
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }

}
