using Microsoft.Win32;
using SimpleNLP.Classification;
using SimpleNLP.Preprocessing;
using SimpleNLP.Transformation;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.Json;
using SimpleNLP;

namespace SimpleNLPApp
{
    public partial class ClassificationTextsWindow : Window
    {
        public ICommand RemoveTextCommand { get; }
        public ObservableCollection<TextEvaluationViewModel> Texts { get; set; } = new();
        private List<TextEvaluationViewModel> AllTexts;
        private PredictModel model;
        private TfIdfVectorizer tfIdfVectorizer;
        public List<string> Classes { get; }
        private List<ClassFilterOption> FilterOptions;
        private ClassificationMetrics lastMetrics;

        public ClassificationTextsWindow(List<TextEvaluationViewModel> texts,
                                         PredictModel model,
                                         TfIdfVectorizer vectorizer,
                                         List<string> classes)
        {
            InitializeComponent();
            AllTexts = new List<TextEvaluationViewModel>(texts);
            foreach (var t in AllTexts)
                Texts.Add(t);

            this.model = model;
            this.tfIdfVectorizer = vectorizer;
            this.Classes = classes;
            this.RemoveTextCommand = new RelayCommand(RemoveText);

            DataContext = this;

            FilterOptions = new List<ClassFilterOption>
            {
                new ClassFilterOption("Все", "ALL"),
                new ClassFilterOption("Без класса", "NOC")
            };
            FilterOptions.AddRange(classes.Select(c => new ClassFilterOption($"Класс: {c}", c)));

            FilterComboBox.ItemsSource = FilterOptions;
            FilterComboBox.SelectedValue = "ALL";
        }

        private void OnFilterChanged(object sender, SelectionChangedEventArgs e) => RefreshView();

        private void RefreshView()
        {
            if (Texts == null || FilterComboBox?.SelectedValue is not string selectedValue)
                return;

            IEnumerable<TextEvaluationViewModel> filtered = AllTexts;

            if (selectedValue == "NOC")
                filtered = AllTexts.Where(t => string.IsNullOrWhiteSpace(t.TrueClass));
            else if (selectedValue != "ALL")
                filtered = AllTexts.Where(t => t.TrueClass == selectedValue);

            Texts.Clear();
            foreach (var item in filtered)
                Texts.Add(item);
        }

        private void OnClassifyClick(object sender, RoutedEventArgs e)
        {
            foreach (var item in AllTexts)
            {
                var tokens = Preprocessor.Preprocess(item.Content);
                var vector = tfIdfVectorizer.Vectorize(tokens);
                item.PredictedClass = model.Predict(vector);
                item.ClassProbabilities = model.PredictProbabilities(vector);
            }

            var yTrue = AllTexts.Where(t => t.TrueClass != null).Select(t => t.TrueClass).ToList();
            var yPred = AllTexts.Where(t => t.TrueClass != null).Select(t => t.PredictedClass).ToList();

            if (yTrue.Count == yPred.Count && yTrue.Count > 0)
            {
                lastMetrics = ClassificationMetrics.Evaluate(yTrue, yPred);
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

            RefreshView();
        }

        private void OnAddTextsClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Text files|*.txt",
                Multiselect = true
            };

            if (dialog.ShowDialog() == true)
            {
                foreach (var file in dialog.FileNames)
                {
                    var t = GetText(file);
                    var vm = new TextEvaluationViewModel
                    {
                        Title = t.Title,
                        Content = t.Content,
                        TrueClass = null,
                        PredictedClass = null
                    };
                    AllTexts.Add(vm);
                }

                RefreshView();
            }
        }

        private void OnExportMetricsClick(object sender, RoutedEventArgs e)
        {
            if (lastMetrics == null)
            {
                MessageBox.Show("Сначала классифицируйте тексты.");
                return;
            }

            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "S.NLP Метрики|*.snlpmetrics",
                DefaultExt = "snlpmetrics"
            };

            if (dialog.ShowDialog() == true)
            {
                File.WriteAllText(dialog.FileName,
                    JsonSerializer.Serialize(lastMetrics, new JsonSerializerOptions { WriteIndented = true }));
                MessageBox.Show("Метрики сохранены.");
            }
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

        private void OnVisualClick(object sender, RoutedEventArgs e)
        {
            new VisualizationWindow(AllTexts.ToList(), Classes).ShowDialog();
        }

        private void OnSelectAllClick(object sender, RoutedEventArgs e)
        {
            ListBoxTexts.SelectAll();
        }

        private void RemoveText(object obj)
        {
            if (obj is TextEvaluationViewModel t)
            {
                AllTexts.Remove(t);
                RefreshView();
            }
        }

        private Text GetText(string file_path)
        {
            using var sr = new StreamReader(file_path);
            return new Text(file_path, sr.ReadToEnd(), true);
        }
    }

    public class TextEvaluationViewModel : INotifyPropertyChanged
    {
        public string Title { get; set; }
        public string Content { get; set; }

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

        private Dictionary<string, double> _classProbabilities;
        public Dictionary<string, double> ClassProbabilities
        {
            get => _classProbabilities;
            set { _classProbabilities = value; OnPropertyChanged(nameof(ClassProbabilities)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string prop) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public class ClassFilterOption
    {
        public string Label { get; set; }
        public string Value { get; set; }

        public ClassFilterOption(string label, string value)
        {
            Label = label;
            Value = value;
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> execute;
        private readonly Predicate<object> canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            this.execute = execute;
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
