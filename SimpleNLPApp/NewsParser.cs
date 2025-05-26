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

        var baseSectionUrl = BaseUrl + Sections[sectionName];
        var newsList = new List<NewsItem>();
        var visitedUrls = new HashSet<string>();
        int currentPage = 0;

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        while (newsList.Count < count)
        {
            string pageUrl = currentPage == 0 ? baseSectionUrl : $"{baseSectionUrl}?page={currentPage + 1}";
            var response = await _httpClient.GetAsync(pageUrl);
            if (!response.IsSuccessStatusCode) break;

            var stream = await response.Content.ReadAsStreamAsync();
            var doc = new HtmlDocument();
            doc.Load(stream, Encoding.GetEncoding("windows-1251"));

            // timeline__group содержит вложенные div с новостями, timeline__text — одиночные
            var groupNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'timeline__group')]");
            var textNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'timeline__text')]");
            var allNewsDivs = new List<HtmlNode>();

            if (groupNodes != null)
            {
                foreach (var group in groupNodes)
                {
                    var items = group.SelectNodes("./div");
                    if (items != null) allNewsDivs.AddRange(items);
                }
            }

            if (textNodes != null)
            {
                allNewsDivs.AddRange(textNodes);
            }

            if (allNewsDivs.Count == 0) break;

            foreach (var block in allNewsDivs)
            {
                if (newsList.Count >= count)
                    break;

                var linkNode = block.SelectSingleNode(".//a");
                var timeNode = block.SelectSingleNode(".//time");

                if (linkNode == null || timeNode == null)
                    continue;

                string href = linkNode.GetAttributeValue("href", "").Trim();
                if (!href.StartsWith("/")) continue;

                string fullUrl = new Uri(new Uri(BaseUrl), href).ToString();
                if (visitedUrls.Contains(fullUrl)) continue;
                visitedUrls.Add(fullUrl);

                string title = linkNode.GetAttributeValue("title", "")?.Trim();
                if (string.IsNullOrWhiteSpace(title))
                    title = HtmlEntity.DeEntitize(linkNode.InnerText.Trim());

                string datetime = timeNode.GetAttributeValue("datetime", "");
                string formattedDate = DateTime.TryParse(datetime, out var dt)
                    ? dt.ToString("dd.MM.yyyy HH:mm")
                    : datetime;

                var full = await GetNewsDetailsAsync(fullUrl);
                if (string.IsNullOrWhiteSpace(full.Body)) continue;

                newsList.Add(new NewsItem
                {
                    Title = title,
                    Url = fullUrl,
                    Date = formattedDate,
                    Body = full.Body
                });
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