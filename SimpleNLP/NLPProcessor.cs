using SimpleNLP.Preprocessing;

namespace SimpleNLP
{
    public class NLPProcessor
    {
        public NLPProcessor()
        {

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
            var preprocessed_text = Preprocessor.Preprocess(text, MethodOfOneForm.LEMMATIZATOR);
            return CountWordFrequency(preprocessed_text);
        }
    }
}
