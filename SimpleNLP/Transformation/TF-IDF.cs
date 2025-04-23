using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleNLP.Transformation
{
    public class TfIdfVectorizer
    {
        private HashSet<string> _vocabulary;
        private Dictionary<string, double> _idfCache;

        // Обучение векторизатора (фиксируем словарь и IDF)
        public void Fit(List<List<string>> trainDocuments)
        {
            _vocabulary = new HashSet<string>(trainDocuments.SelectMany(doc => doc).Distinct());
            _idfCache = CalculateIdfValues(trainDocuments, _vocabulary);
        }

        // Векторизация новых текстов (на основе сохранённых vocabulary и idfCache)
        public double[] Vectorize(List<string> document)
        {
            if (_vocabulary == null || _idfCache == null)
                throw new InvalidOperationException("Векторизатор не обучен. Сначала вызовите Fit().");

            var filteredDoc = document.Where(term => _vocabulary.Contains(term)).ToList();
            return _vocabulary
                .Select(term => ComputeTf(term, filteredDoc) * _idfCache.GetValueOrDefault(term, 0))
                .ToArray();
        }

        private static Dictionary<string, double> CalculateIdfValues(List<List<string>> documents, HashSet<string> vocabulary)
        {
            var idfValues = new Dictionary<string, double>();
            int totalDocs = documents.Count;

            foreach (var term in vocabulary)
            {
                int docsContainingTerm = documents.Count(doc => doc.Contains(term));
                idfValues[term] = Math.Log((double)totalDocs / (docsContainingTerm + 1));
            }

            return idfValues;
        }

        private static double ComputeTf(string term, List<string> document)
        {
            int termCount = document.Count(t => t == term);
            int totalTermsInDoc = document.Count;
            return totalTermsInDoc == 0 ? 0 : (double)termCount / totalTermsInDoc;
        }
    }
}