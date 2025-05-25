using System.Net;
using System.Net.Http;
using System.Text;
using HtmlAgilityPack;

public class NewsItem
{
    public string Title { get; set; }
    public string Url { get; set; }
    public string Date { get; set; }
    public string Body { get; set; }
}

public class NewsParser
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://www.interfax.ru";
    public Dictionary<string, string> Sections { get; private set; }

    public NewsParser()
    {
        _httpClient = new HttpClient();
        Sections = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // windows-1251
    }

    public async Task InitializeAsync()
    {
        await LoadSectionsAsync();
    }

    private async Task LoadSectionsAsync()
    {
        var response = await _httpClient.GetAsync(BaseUrl);
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();
        var doc = new HtmlDocument();
        doc.Load(stream, Encoding.GetEncoding("windows-1251"));

        var menuNodes = doc.DocumentNode.SelectNodes("//div[@class='toplinks']//ul/li/a[starts-with(@href, '/') and not(contains(@href, 'http'))]");

        if (menuNodes != null)
        {
            foreach (var node in menuNodes)
            {
                var name = WebUtility.HtmlDecode(node.InnerText.Trim());
                var href = node.GetAttributeValue("href", string.Empty).Trim();

                if (!string.IsNullOrEmpty(name) && href.StartsWith("/") && !Sections.ContainsKey(name))
                {
                    Sections[name] = href;
                }
            }
        }
    }

    public async Task<List<NewsItem>> GetNewsFromSectionAsync(string sectionName, int count)
    {
        if (!Sections.ContainsKey(sectionName))
            throw new ArgumentException($"Раздел '{sectionName}' не найден.");

        var baseSectionUrl = new Uri(new Uri(BaseUrl), Sections[sectionName]).ToString();
        var newsList = new List<NewsItem>();
        int currentPage = 0;

        while (newsList.Count < count)
        {
            string pageUrl = currentPage == 0 ? baseSectionUrl : $"{baseSectionUrl}?page={currentPage + 1}";
            var response = await _httpClient.GetAsync(pageUrl);
            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync();
            var doc = new HtmlDocument();
            doc.Load(stream, Encoding.GetEncoding("windows-1251"));

            var newsNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'timeline__group') or contains(@class, 'timeline__text')]");
            if (newsNodes == null || newsNodes.Count == 0)
                break;

            foreach (var node in newsNodes)
            {
                if (newsList.Count >= count)
                    break;

                var titleNode = node.SelectSingleNode(".//h3");
                var timeNode = node.SelectSingleNode(".//time");
                var linkNode = node.SelectSingleNode(".//a[h3]");

                if (titleNode != null && timeNode != null && linkNode != null)
                {
                    string datetimeAttr = timeNode.GetAttributeValue("datetime", "").Trim();
                    string formattedDate = datetimeAttr;
                    if (DateTime.TryParse(datetimeAttr, out var dt))
                        formattedDate = dt.ToString("dd.MM.yyyy HH:mm");

                    var href = linkNode.GetAttributeValue("href", string.Empty).Trim();
                    var newsUrl = new Uri(new Uri(BaseUrl), href).ToString();

                    var full = await GetNewsDetailsAsync(newsUrl);

                    if (string.IsNullOrWhiteSpace(full.Body))
                        continue;

                    newsList.Add(new NewsItem
                    {
                        Title = HtmlEntity.DeEntitize(titleNode.InnerText.Trim()),
                        Url = newsUrl,
                        Date = formattedDate,
                        Body = full.Body
                    });
                }
            }

            currentPage++;
        }

        return newsList;
    }

    private async Task<NewsItem> GetNewsDetailsAsync(string newsUrl)
    {
        var response = await _httpClient.GetAsync(newsUrl);
        response.EnsureSuccessStatusCode();

        var bytes = await response.Content.ReadAsByteArrayAsync();
        var html = Encoding.GetEncoding("windows-1251").GetString(bytes);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var articleNode = doc.DocumentNode.SelectSingleNode("//article");
        if (articleNode == null)
            return new NewsItem { Body = string.Empty };

        var paragraphs = articleNode.SelectNodes(".//p");
        if (paragraphs == null || paragraphs.Count == 0)
            return new NewsItem { Body = string.Empty };

        var fullText = string.Join("\n", paragraphs.Select(p => HtmlEntity.DeEntitize(p.InnerText.Trim())));
        return new NewsItem { Body = fullText };
    }
}
