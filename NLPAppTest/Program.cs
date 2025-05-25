class Program
{
    static async Task Main(string[] args)
    {
        using (var parser = new NewsParser())
        {
            try
            {
                Console.WriteLine("Начинаем парсинг Interfax.ru...");
                var news = await parser.ParseNewsWithFullTextAsync();

                Console.WriteLine("\nРезультаты парсинга:");
                foreach (var section in news)
                {
                    Console.WriteLine($"\n=== {section.Key} ===");
                    foreach (var item in section.Value)
                    {
                        Console.WriteLine($"\n[{item.Time}] {item.Title}");
                        Console.WriteLine($"URL: {item.Url}");
                        Console.WriteLine($"Текст новости (сокращенный):\n{item.FullText}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка: {ex.Message}");
            }
        }
    }
}