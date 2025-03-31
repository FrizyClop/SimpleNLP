using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleNLP.Transformation
{
    internal static class TF_IDF
    {
        // Вычисляет IDF для каждого слова в словаре
        private static Dictionary<string, double> CalculateIdfValues(List<List<string>> documents, HashSet<string> vocabulary)
        {
            var idfValues = new Dictionary<string, double>();
            int totalDocs = documents.Count;

            foreach (var term in vocabulary)
            {
                int docsContainingTerm = documents.Count(doc => doc.Contains(term));
                idfValues[term] = Math.Log((double)totalDocs / (1 + docsContainingTerm));
            }

            return idfValues;
        }

        // Вычисляет TF для термина в документе
        private static double ComputeTf(string term, List<string> document)
        {
            return (double)document.Count(t => t == term) / document.Count;
        }

        // Векторизует один документ
        private static double[] Vectorize(List<string> document, Dictionary<string, double> idfCache, HashSet<string> vocabulary)
        {
            return vocabulary
                .Select(term => ComputeTf(term, document) * idfCache[term])
                .ToArray();
        }

        // Векторизует все документы
        public static List<double[]> VectorizeAll(List<List<string>> documents, HashSet<string> vocabulary)
        {
            Dictionary<string,double> idfCache = CalculateIdfValues(documents, vocabulary);
            List<double[]> vectors = new List<double[]>();
            foreach (List<string> document in documents)
                vectors.Add(Vectorize(document,idfCache,vocabulary));
            return vectors;
        }
    }
}
