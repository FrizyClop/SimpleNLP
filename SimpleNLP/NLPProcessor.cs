using SimpleNLP.Preprocessing;

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

        public IEnumerable<string> Stemming(IEnumerable<string> tokenized_words)
        {
            Stemmer stemmer = new Stemmer();
            return stemmer.Stemming(tokenized_words);
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
            var preprocessed_text = Preprocessor.Preprocess(text);
            return CountWordFrequency(preprocessed_text);
        }
    }
}
