using System.Text.Json;

namespace SimpleNLP.Transformation
{
    public class TfIdfVectorizer
    {
        private List<string> _vocabularyList; // Упорядоченный словарь
        private Dictionary<string, int> _termToIndex; // Термин -> индекс
        private Dictionary<string, double> _idfCache;

        public void Fit(List<List<string>> trainDocuments)
        {
            // Сохраняем словарь как упорядоченный список
            _vocabularyList = trainDocuments.SelectMany(doc => doc).Distinct().ToList();
            _termToIndex = _vocabularyList.Select((term, index) => (term, index))
                                         .ToDictionary(x => x.term, x => x.index);

            // Добавляем сглаживание в IDF (избегаем деления на 0)
            _idfCache = CalculateIdfValues(trainDocuments, _vocabularyList);
        }

        public double[] Vectorize(List<string> document)
        {
            if (_vocabularyList == null || _idfCache == null)
                throw new InvalidOperationException("Векторизатор не обучен. Сначала вызовите Fit().");

            double[] vector = new double[_vocabularyList.Count];
            var filteredDoc = document.Where(term => _termToIndex.ContainsKey(term)).ToList();
            int totalTermsInDoc = filteredDoc.Count;

            foreach (var term in filteredDoc.Distinct())
            {
                double tf = (double)filteredDoc.Count(t => t == term) / totalTermsInDoc;
                double idf = _idfCache[term];
                vector[_termToIndex[term]] = tf * idf;
            }

            // Нормализация вектора (L2)
            double norm = Math.Sqrt(vector.Sum(x => x * x));
            if (norm > 0)
                vector = vector.Select(x => x / norm).ToArray();

            return vector;
        }

        private Dictionary<string, double> CalculateIdfValues(List<List<string>> documents, List<string> vocabulary)
        {
            int totalDocs = documents.Count;
            var idfValues = new Dictionary<string, double>();

            foreach (var term in vocabulary)
            {
                int docsContainingTerm = documents.Count(doc => doc.Contains(term));
                // Сглаживание +1 в знаменателе (избегаем log(inf))
                idfValues[term] = Math.Log((1 + totalDocs) / (1 + docsContainingTerm)) + 1;
            }

            return idfValues;
        }

        // Новый метод для получения термина по индексу
        public string GetFeatureName(int index) => _vocabularyList[index];

        public string GetJsonRepresentation()
        {
            var data = new
            {
                VocabularyList = _vocabularyList,
                TermToIndex = _termToIndex,
                IdfCache = _idfCache
            };
            return JsonSerializer.Serialize(data);
        }

        public void LoadFromJson(JsonElement json)
        {
            _vocabularyList = json.GetProperty("VocabularyList").Deserialize<List<string>>();
            _termToIndex = json.GetProperty("TermToIndex").Deserialize<Dictionary<string,int>>();
            _idfCache = json.GetProperty("IdfCache").Deserialize<Dictionary<string,double>>();
        }
    }
}