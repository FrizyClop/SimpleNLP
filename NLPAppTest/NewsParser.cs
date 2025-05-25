using HtmlAgilityPack;
using System.Text;

public class NewsParser : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly int _delayBetweenRequests;
    private readonly int _maxNewsPerSection;

    public NewsParser(string baseUrl = "https://www.interfax.ru/",
                    int delayBetweenRequests = 1000,
                    int maxNewsPerSection = 5)
    {
        _baseUrl = baseUrl;
        _delayBetweenRequests = delayBetweenRequests;
        _maxNewsPerSection = maxNewsPerSection;

        _httpClient = new HttpClient(new HttpClientHandler
        {
            AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
        });

        _httpClient.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public async Task<Dictionary<string, List<NewsItem>>> ParseNewsWithFullTextAsync()
    {
        var allNews = new Dictionary<string, List<NewsItem>>();

        try
        {
            string html = await GetHtmlContentAsync(_baseUrl);
            var sections = ParseMainSections(html);

            foreach (var section in sections)
            {
                var news = await GetNewsFromSectionAsync(section.Value);

                foreach (var newsItem in news)
                {
                    if (!string.IsNullOrEmpty(newsItem.Url))
                    {
                        newsItem.FullText = await GetNewsFullTextAsync(newsItem.Url);
                        await Task.Delay(_delayBetweenRequests);
                    }
                }

                allNews.Add(section.Key, news);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при парсинге новостей: {ex.Message}");
            throw;
        }

        return allNews;
    }

    private async Task<string> GetNewsFullTextAsync(string newsUrl)
    {
        try
        {
            string html = await GetHtmlContentAsync(newsUrl);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Основной текст новости - ищем article с itemprop="articleBody"
            var articleNode = doc.DocumentNode.SelectSingleNode("//article[@itemprop='articleBody']");

            if (articleNode != null)
            {
                // Удаляем ненужные элементы (реклама, скрипты, ссылки и т.д.)
                var nodesToRemove = articleNode.SelectNodes(".//div[contains(@class, 'adv')]|.//script|.//style|.//iframe|.//aside|.//div[contains(@class, 'textMTags')]|.//div[contains(@class, 'group-btns')]|.//div[contains(@class, 'textMMat')]");
                if (nodesToRemove != null)
                {
                    foreach (var node in nodesToRemove)
                    {
                        node.Remove();
                    }
                }

                // Извлекаем все параграфы текста (упрощенный селектор)
                var paragraphs = articleNode.SelectNodes(".//p[not(contains(@class, 'hidden'))]");

                // Если не нашли с классом, пробуем просто все параграфы
                if (paragraphs == null || paragraphs.Count == 0)
                {
                    paragraphs = articleNode.SelectNodes(".//p");
                }

                if (paragraphs != null && paragraphs.Count > 0)
                {
                    var textBuilder = new StringBuilder();

                    foreach (var p in paragraphs)
                    {
                        // Очищаем текст от лишних пробелов и переносов
                        string paragraphText = p.InnerText.Trim();
                        paragraphText = System.Text.RegularExpressions.Regex.Replace(paragraphText, @"\s+", " ");

                        if (!string.IsNullOrWhiteSpace(paragraphText))
                        {
                            textBuilder.AppendLine(paragraphText);
                        }
                    }

                    return textBuilder.ToString().Trim();
                }
            }

            return "Не удалось извлечь текст новости";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при получении полного текста новости: {ex.Message}");
            return "Ошибка при получении текста новости";
        }
    }

    private async Task<string> GetHtmlContentAsync(string url)
    {
        try
        {
            // Получаем ответ как массив байтов
            var responseBytes = await _httpClient.GetByteArrayAsync(url);

            // Конвертируем в строку с указанием правильной кодировки
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var encoding = Encoding.GetEncoding("windows-1251");
            return encoding.GetString(responseBytes);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при получении HTML с {url}: {ex.Message}");
            throw;
        }
    }

    private Dictionary<string, string> ParseMainSections(string html)
    {
        var sections = new Dictionary<string, string>();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var menuItems = doc.DocumentNode.SelectNodes("//div[@class='toplinks']//nav//ul//li/a");

        if (menuItems != null)
        {
            foreach (var item in menuItems)
            {
                string href = item.GetAttributeValue("href", "");
                string text = item.InnerText.Trim();

                if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(href))
                {
                    if (IsSectionForbidden(href))
                        continue;

                    href = MakeAbsoluteUrl(href);
                    text = CleanSectionName(text);

                    if (!sections.ContainsKey(text))
                    {
                        sections.Add(text, href);
                    }
                }
            }
        }

        return sections;
    }

    private async Task<List<NewsItem>> GetNewsFromSectionAsync(string sectionUrl)
    {
        var newsItems = new List<NewsItem>();

        try
        {
            string html = await GetHtmlContentAsync(sectionUrl);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var newsNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'timeline')]//div[contains(@class, 'timeline__')]");

            if (newsNodes == null)
            {
                newsNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'newsmain')]//a/h3");
            }

            if (newsNodes != null)
            {
                foreach (var node in newsNodes)
                {
                    if (newsItems.Count >= _maxNewsPerSection)
                        break;

                    var newsItem = ParseNewsItem(node);
                    if (newsItem != null)
                    {
                        newsItems.Add(newsItem);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при парсинге раздела {sectionUrl}: {ex.Message}");
        }

        return newsItems;
    }

    private NewsItem ParseNewsItem(HtmlNode node)
    {
        try
        {
            var timeNode = node.SelectSingleNode(".//time");
            var titleNode = node.SelectSingleNode(".//h3/a") ?? node.SelectSingleNode(".//h3");

            if (titleNode == null)
            {
                titleNode = node;
            }

            if (titleNode == null)
                return null;

            string time = timeNode?.GetAttributeValue("datetime", "") ?? "";
            string title = titleNode.InnerText.Trim();
            string url = titleNode.ParentNode?.GetAttributeValue("href", "") ?? "";

            if (!string.IsNullOrEmpty(url) && url.StartsWith("/"))
            {
                url = MakeAbsoluteUrl(url);
            }

            return new NewsItem
            {
                Title = title,
                Time = time,
                Url = url
            };
        }
        catch
        {
            return null;
        }
    }

    private bool IsSectionForbidden(string url)
    {
        return url.Contains("/tourism/") ||
               url.Contains("/realty/") ||
               url.Contains("/digit.asp");
    }

    private string MakeAbsoluteUrl(string relativeUrl)
    {
        return relativeUrl.StartsWith("http") ?
               relativeUrl :
               $"{_baseUrl.TrimEnd('/')}/{relativeUrl.TrimStart('/')}";
    }

    private string CleanSectionName(string name)
    {
        return name.Replace("Все", "").Trim();
    }

    public class NewsItem
    {
        public string Title { get; set; }
        public string Time { get; set; }
        public string Url { get; set; }
        public string FullText { get; set; }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}