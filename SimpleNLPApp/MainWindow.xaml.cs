using Microsoft.Win32;
using SimpleNLP.Classification;
using System.IO;
using System.Windows;

namespace SimpleNLPApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<ModelWindow> child_links;

        public List<ModelWindow> ChildLinks { get {  return child_links; } }

        public MainWindow()
        {
            InitializeComponent();
            child_links = new();
            ChangeInterface(0);
        }

        private void buttonNewModel_Click(object sender, RoutedEventArgs e)
        {
            ChangeInterface(1);
        }

        private void ButtonCreate_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckName()) return;
            ModelWindow modelWindow = null;
            switch (ComboBoxType.SelectedIndex)
            {
                case -1:
                    MessageBox.Show("Выберите одну из доступных моделей!");
                    return;
                case 0:
                    modelWindow = new ModelWindow(TextBoxName.Text, new NaiveBayesParameters(SliderAlpha.Value));
                    break;
                case 1:
                    modelWindow = new ModelWindow(TextBoxName.Text, new LogisticRegressionParameters(Math.Round(SliderLRLearningRate.Value,4),Convert.ToInt32(SliderEpochs.Value)));
                    break;
                case 2:
                    modelWindow = new ModelWindow(TextBoxName.Text, new SVMParameters(Convert.ToInt32(SliderSVMMaxIterations.Value), Math.Round(SliderSVMLearningRate.Value,4), Math.Round(SliderSVMLambda.Value,4)));
                    break;
            }

            child_links.Add(modelWindow);
            modelWindow.MainWindow = this;
            ButtonPrev_Click(sender, e);
            this.Visibility = Visibility.Collapsed;
            modelWindow.Show();
        }

        private void ButtonPrev_Click(object sender, RoutedEventArgs e)
        {
            ChangeInterface(0);
        }

        #region Проверки

        private bool CheckName()
        {
            if (TextBoxName.Text.Trim() == "")
            {
                MessageBox.Show("Строка имени не может быть пустой!");
                return false;
            }
            if (ContainsInvalidFileNameChars(TextBoxName.Text))
            {
                MessageBox.Show("Строка имени cодержит недопустмые символы!");
                return false;
            }
            return true;
        }

        static bool ContainsInvalidFileNameChars(string input)
        {
            char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();
            if (input.Contains('.') || input.Contains('/') || input.Contains('\\')) return true;
            foreach(char c in invalidChars)
            {
                if(input.Contains(c)) return true;
            }
            return false;
        }

        #endregion

        private void SliderAlpha_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (LabelAlpha == null) return;
            LabelAlpha.Content = Math.Round(SliderAlpha.Value,2).ToString();
        }

        public void ChangeInterface(int n)
        {
            switch (n)
            {
                case 0:
                    TextBoxName.Text = "Untitled";
                    MainMenu.Visibility = Visibility.Visible;
                    MenuNewModel.Visibility = Visibility.Hidden;
                    break;
                case 1:
                    MenuNewModel.Visibility = Visibility.Visible;
                    MainMenu.Visibility = Visibility.Hidden;
                    ShowParameters();
                    break;
            }
        }

        private void ShowParameters()
        {
            if (ComboBoxType.SelectedIndex == -1)
            {
                Parameters.Visibility = Visibility.Hidden;
                LogisticRegressionParameters.Visibility = Visibility.Hidden;
                BayesParameters.Visibility = Visibility.Hidden;
                SVMParameters.Visibility = Visibility.Hidden;
            }
            else if (ComboBoxType.SelectedIndex == 0)
            {
                Parameters.Visibility = Visibility.Visible;
                LogisticRegressionParameters.Visibility = Visibility.Hidden;
                BayesParameters.Visibility = Visibility.Visible;
                SVMParameters.Visibility = Visibility.Hidden;
            }
            else if (ComboBoxType.SelectedIndex == 1)
            {
                Parameters.Visibility = Visibility.Visible;
                LogisticRegressionParameters.Visibility = Visibility.Visible;
                BayesParameters.Visibility = Visibility.Hidden;
                SVMParameters.Visibility = Visibility.Hidden;
            }
            else if (ComboBoxType.SelectedIndex == 2)
            {
                Parameters.Visibility = Visibility.Visible;
                LogisticRegressionParameters.Visibility = Visibility.Hidden;
                BayesParameters.Visibility = Visibility.Hidden;
                SVMParameters.Visibility = Visibility.Visible;
            }
        }

        private void ComboBoxType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ShowParameters();
        }

        private void SliderEpochs_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (LabelEpochs == null) return;
            LabelEpochs.Content = SliderEpochs.Value.ToString();
        }

        public void ButtonOpenModel_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog{
                Filter = "SNLP files (*.snlp)|*.snlp",
                DefaultExt = ".snlp",
                AddExtension = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                // Считываем файл
                string json = File.ReadAllText(openFileDialog.FileName);

                ModelWindow modelWindow = new ModelWindow(openFileDialog.FileName, json);
                child_links.Add(modelWindow);
                modelWindow.MainWindow = this;
                this.Visibility = Visibility.Collapsed;
                modelWindow.Show();
            }
        }

        private void SliderLRLearningRate_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (LabelLRLearningRate == null) return;
            LabelLRLearningRate.Content = Math.Round(SliderLRLearningRate.Value,4).ToString();
        }

        private void SliderSVMMaxIterations_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (LabelSVMMaxIterations == null) return;
            LabelSVMMaxIterations.Content = SliderSVMMaxIterations.Value.ToString();
        }

        private void SliderSVMLearningRate_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (LabelSVMLearningRate == null) return;
            LabelSVMLearningRate.Content = Math.Round(SliderSVMLearningRate.Value,4).ToString();
        }

        private void SliderSVMLambda_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (LabelSVMLambda == null) return;
            LabelSVMLambda.Content = Math.Round(SliderSVMLambda.Value,4).ToString();
        }

        private void MainWindow1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(child_links.Count > 0)
            {
                e.Cancel = true;
                this.Hide();
                ChangeInterface(0);
            }
        }
    }
}