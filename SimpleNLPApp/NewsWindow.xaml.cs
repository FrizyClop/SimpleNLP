using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace SimpleNLPApp
{
    public partial class NewsWindow : Window
    {
        private NewsParser parser;
        public ObservableCollection<NewsSection> NewsSections { get; set; } = new ObservableCollection<NewsSection>();

        public NewsWindow()
        {
            InitializeComponent();
            DataContext = this;
            LoadSectionsAsync();
        }

        private async void LoadSectionsAsync()
        {
            parser = new NewsParser();

            try
            {
                await parser.InitializeAsync();

                int id = 0;
                foreach (var section in parser.Sections)
                {
                    NewsSections.Add(new NewsSection(id, section.Key));
                    id++;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке секций: {ex.Message}");
            }
        }

        private async void ButtonDownloadNews_Click(object sender, RoutedEventArgs e)
        {
            var selectedSections = NewsSections.Where(s => s.DownloadSection).ToList();
            if (!selectedSections.Any())
            {
                MessageBox.Show("Не выбрано ни одной секции для скачивания!");
                return;
            }

            CanvasProgress.Visibility = Visibility.Visible;
            ProgressBarFit.Value = 0;
            LabelProgress.Content = "Подготовка к загрузке...";

            try
            {
                int total = selectedSections.Sum(s => s.CountOfNews);
                int downloaded = 0;

                var progress = new Progress<(int current, int total)>(value =>
                {
                    ProgressBarFit.Maximum = value.total;
                    ProgressBarFit.Value = value.current;
                    LabelProgress.Content = $"Загружено: {value.current} из {value.total}";
                });

                var allNews = new Dictionary<string, List<NewsItem>>();

                foreach (var section in selectedSections)
                {
                    var newsItems = await parser.GetNewsFromSectionAsync(section.SectionName, section.CountOfNews);
                    allNews[section.SectionName] = newsItems;

                    downloaded += newsItems.Count;
                    (progress as IProgress<(int, int)>)?.Report((downloaded, total));
                }

                foreach (var kvp in allNews)
                {
                    string sectionPath = Path.Combine(Directory.GetCurrentDirectory(), "News", kvp.Key);
                    Directory.CreateDirectory(sectionPath);
                    SaveNewsFromOneSection(sectionPath, kvp.Value);
                }

                MessageBox.Show($"Успешно сохранено {downloaded} новостей!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке новостей: {ex.Message}");
            }
            finally
            {
                CanvasProgress.Visibility = Visibility.Hidden;
            }
        }

        private void SaveNewsFromOneSection(string sectionPath, List<NewsItem> news)
        {
            foreach (NewsItem newsItem in news)
            {
                string sanitizedTitle = string.Join("_", newsItem.Title.Split(Path.GetInvalidFileNameChars()));
                string sanitizedTime = string.Join("_", newsItem.Date.Split(Path.GetInvalidFileNameChars()));
                string fileName = $"{sanitizedTime}_{sanitizedTitle}.txt".Trim();

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    fileName = $"news_{DateTime.Now.Ticks}.txt";
                }

                string filePath = Path.Combine(sectionPath, fileName);
                File.WriteAllText(filePath, newsItem.Body ?? "");
            }
        }

        public class NewsSection
        {
            public NewsSection(int ID, string SectionName)
            {
                Id = ID;
                this.SectionName = SectionName;
                DownloadSection = true;
                CountOfNews = 5;
            }

            public int Id { get; set; }
            public string SectionName { get; set; }
            public bool DownloadSection { get; set; }
            public int CountOfNews { get; set; }
        }

        private void CheckBoxSelectAll_Checked(object sender, RoutedEventArgs e)
        {
            if (NewsSections.Count == 0) return;
            foreach (NewsSection s in NewsSections)
            {
                s.DownloadSection = true;
            }
            ItemsControlSections.Items.Refresh();
        }

        private void CheckBoxSelectAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (NewsSections.Count == 0) return;
            foreach (NewsSection s in NewsSections)
            {
                s.DownloadSection = false;
            }
            ItemsControlSections.Items.Refresh();
        }
    }
}
