namespace SimpleNLP
{
    public class NLPProcessor
    {
        private readonly HashSet<string> stop_words;

        public NLPProcessor()
        {
            StreamReader stream_reader = new StreamReader("russian_stop_words.txt");
            stop_words = stream_reader.ReadToEnd().Split("\n").ToHashSet<string>();
            stream_reader.Close();
        }

        // Токенизация текста
        public IEnumerable<string> Tokenize(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Enumerable.Empty<string>();

            // Разделяем текст на слова, убирая знаки препинания
            var tokens = text.Split(new[] { ' ', '.', ',', '!', '?', ';', ':', '-', '—', '(', ')', '/', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            return tokens.Select(token => token.ToLowerInvariant());
        }

        public IEnumerable<string> FilterTokens(IEnumerable<string> tokens)
        {
            var filtred_tokens = RemoveStopWords(tokens);
            filtred_tokens = RemoveNumbers(filtred_tokens);
            return filtred_tokens;
        }

        public IEnumerable<string> Stemming(IEnumerable<string> tokenized_words)
        {
            Stemmer stemmer = new Stemmer();
            return stemmer.Stemming(tokenized_words);
        }

        // Удаление стоп-слов
        public IEnumerable<string> RemoveStopWords(IEnumerable<string> tokens)
        {
            return tokens.Where(token => !stop_words.Contains(token));
        }

        // Удаление чисел
        public IEnumerable<string> RemoveNumbers(IEnumerable<string> tokens)
        {
            int num;
            return tokens.Where(token => !int.TryParse(token,out num));
        }

        // Подсчет частоты слов
        public Dictionary<string, int> CountWordFrequency(IEnumerable<string> tokens)
        {
            var frequency = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var token in tokens)
            {
                if (frequency.ContainsKey(token))
                {
                    frequency[token]++;
                }
                else
                {
                    frequency[token] = 1;
                }
            }
            return frequency;
        }

        // Полный анализ текста
        public Dictionary<string, int> AnalyzeText(string text)
        {
            var tokens = Tokenize(text);
            tokens = Stemming(tokens);
            var filteredTokens = FilterTokens(tokens);
            return CountWordFrequency(filteredTokens);
        }
    }
}
