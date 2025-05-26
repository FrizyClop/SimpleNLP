using Microsoft.Win32;
using SimpleNLP.Classification;
using System.IO;
using System.Windows;
using System.Windows.Controls;

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
                case 3:
                    modelWindow = new ModelWindow(TextBoxName.Text, new KNNParameters(Convert.ToInt32(SliderKNNk.Value)));
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
            if (TextBoxBayesAlpha == null) return;
            TextBoxBayesAlpha.Text = Math.Round(SliderAlpha.Value,2).ToString();
        }

        public void ChangeInterface(int n)
        {
            switch (n)
            {
                case 0:
                    TextBoxName.Text = "Untitled";
                    MainMenu.Visibility = Visibility.Visible;
                    MainLogo.Visibility = Visibility.Visible;
                    MenuNewModel.Visibility = Visibility.Hidden;
                    HelpButton.Visibility = Visibility.Visible;
                    break;
                case 1:
                    MenuNewModel.Visibility = Visibility.Visible;
                    MainMenu.Visibility = Visibility.Hidden;
                    MainLogo.Visibility = Visibility.Hidden;
                    HelpButton.Visibility = Visibility.Hidden;
                    ShowParameters();
                    break;
            }
        }

        private void ShowParameters()
        {
            switch (ComboBoxType.SelectedIndex)
            {
                case -1:
                    ChangeParameters(-1);
                    break;
                case 0:
                    ChangeParameters(0);
                    break;
                case 1:
                    ChangeParameters(1);
                    break;
                case 2:
                    ChangeParameters(2);
                    break;
                case 3:
                    ChangeParameters(3);
                    break;
            }
        }

        private void ChangeParameters(int index)
        {
            if (index < 0)
            {
                Parameters.Visibility = Visibility.Hidden;
                return;
            }
            else
                Parameters.Visibility = Visibility.Visible;

            if(index == 0)
                BayesParameters.Visibility = Visibility.Visible;
            else
                BayesParameters.Visibility = Visibility.Hidden;

            if (index == 1)
                LogisticRegressionParameters.Visibility = Visibility.Visible;
            else
                LogisticRegressionParameters.Visibility = Visibility.Hidden;

            if (index == 2)
                SVMParameters.Visibility = Visibility.Visible;
            else
                SVMParameters.Visibility = Visibility.Hidden;

            if(index == 3)
                KNNParameters.Visibility = Visibility.Visible;
            else
                KNNParameters.Visibility = Visibility.Hidden;
        }

        private void ComboBoxType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ShowParameters();
        }

        private void SliderEpochs_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TextBoxLREpochs == null) return;
            TextBoxLREpochs.Text = SliderEpochs.Value.ToString();
        }

        public void ButtonOpenModel_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog {
                Filter = "SNLP files (*.snlp)|*.snlp",
                InitialDirectory = Properties.Settings.Default.StringLastPathToModels,
                DefaultExt = ".snlp",
                AddExtension = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                // Сохраняем путь к папке, в которой была выбрана модель
                Properties.Settings.Default.StringLastPathToModels = Path.GetDirectoryName(openFileDialog.FileName);
                Properties.Settings.Default.Save(); // Сохраняем настройки

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
            if (TextBoxLRLearningRate == null) return;
            TextBoxLRLearningRate.Text = Math.Round(SliderLRLearningRate.Value,4).ToString();
        }

        private void SliderSVMMaxIterations_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TextBoxSVMMaxIterations == null) return;
            TextBoxSVMMaxIterations.Text = SliderSVMMaxIterations.Value.ToString();
        }

        private void SliderSVMLearningRate_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TextBoxSVMLearningRate == null) return;
            TextBoxSVMLearningRate.Text = Math.Round(SliderSVMLearningRate.Value,4).ToString();
        }

        private void SliderSVMLambda_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TextBoxSVMLambda == null) return;
            TextBoxSVMLambda.Text = Math.Round(SliderSVMLambda.Value,4).ToString();
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

        private void ButtonPreprocessingTexts_Click(object sender, RoutedEventArgs e)
        {
            PreprocessingTextWindow ptw = new PreprocessingTextWindow();
            ptw.ShowDialog();
        }

        private bool IsTextNumeric(string text)
        {
            return double.TryParse(text, out _);
        }

        private void TextBoxCheckValidation(TextBox textBox, Slider slider, string default_text, bool integer)
        {
            if (!IsTextNumeric(textBox.Text))
            {
                textBox.Text = default_text;
                slider.Value = Convert.ToDouble(default_text);
                return;
            }

            if (integer)
            {
                textBox.Text = Math.Round(Convert.ToDouble(textBox.Text)).ToString();
            }

            double param = Convert.ToDouble(textBox.Text);

            if (param < slider.Minimum || param > slider.Maximum)
            {
                textBox.Text = default_text;
                slider.Value = Convert.ToDouble(default_text);
                return;
            }

            slider.Value = param;
        }

        private void TextBoxBayesAlpha_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBoxCheckValidation(TextBoxBayesAlpha,SliderAlpha,"0,1",false);
        }

        private void TextBoxLRLearningRate_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBoxCheckValidation(TextBoxLRLearningRate,SliderLRLearningRate,"0,01",false);
        }

        private void TextBoxLREpochs_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBoxCheckValidation(TextBoxLREpochs, SliderEpochs, "500", true);
        }

        private void TextBoxSVMMaxIterations_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBoxCheckValidation(TextBoxSVMMaxIterations, SliderSVMMaxIterations, "500",true);
        }

        private void TextBoxSVMLearningRate_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBoxCheckValidation(TextBoxSVMLearningRate, SliderSVMLearningRate, "0,01", false);
        }

        private void TextBoxSVMLambda_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBoxCheckValidation(TextBoxSVMLambda, SliderSVMLambda, "0,1",false);
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            HelpWindow.ShowSingleton();
        }

        private void SliderKNNk_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TextBoxKNNk == null) return;
            TextBoxKNNk.Text = SliderKNNk.Value.ToString();
        }

        private void TextBoxKNNk_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBoxCheckValidation(TextBoxKNNk, SliderKNNk, "3", true);
        }
    }
}