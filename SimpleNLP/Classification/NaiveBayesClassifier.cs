using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleNLP.Classification
{
    public class NaiveBayesClassifier
    {
        private double alpha; // Лапласовский параметр
        private Dictionary<string, double> classProbs; // P(c)
        private Dictionary<string, Dictionary<int, double>> featureProbs; // P(w_i|c)
        private List<string> classes;
        private int vocabSize;

        public NaiveBayesClassifier(double alpha = 1.0)
        {
            this.alpha = alpha;
            this.classProbs = new Dictionary<string, double>();
            this.featureProbs = new Dictionary<string, Dictionary<int, double>>();
            this.classes = new List<string>();
        }

        public void Fit(List<double[]> X, List<string> y)
        {
            // Определяем классы
            this.classes = y.Distinct().ToList();
            int totalDocs = y.Count;

            // Вычисляем P(c) для каждого класса
            foreach (var c in classes)
            {
                int count = y.Count(label => label == c);
                classProbs[c] = (double)count / totalDocs;
            }

            // Размер словаря (количество признаков TF-IDF)
            vocabSize = X[0].Length;

            // Вычисляем P(w_i|c) с лапласовским сглаживанием
            foreach (var c in classes)
            {
                // Выбираем документы текущего класса
                var classDocs = X.Where((_, idx) => y[idx] == c).ToArray();

                // Сумма TF-IDF по всем документам класса (для нормировки)
                double[] featureSums = new double[vocabSize];
                for (int i = 0; i < vocabSize; i++)
                {
                    featureSums[i] = classDocs.Sum(doc => doc[i]) + alpha; // + alpha (сглаживание)
                }
                double totalSum = featureSums.Sum();

                // Сохраняем P(w_i|c)
                featureProbs[c] = new Dictionary<int, double>();
                for (int i = 0; i < vocabSize; i++)
                {
                    featureProbs[c][i] = featureSums[i] / totalSum;
                }
            }
        }

        public List<string> Predict(List<double[]> X)
        {
            var predictions = new List<string>();
            for (int i = 0; i < X.Count; i++)
            {
                double maxLogProb = double.NegativeInfinity;
                string bestClass = null;

                foreach (var c in classes)
                {
                    double logProb = Math.Log(classProbs[c]);

                    for (int j = 0; j < vocabSize; j++)
                    {
                        if (X[i][j] > 0) // Если признак (слово) присутствует
                        {
                            logProb += Math.Log(featureProbs[c][j]) * X[i][j]; // Учитываем TF-IDF вес
                        }
                    }

                    if (logProb > maxLogProb)
                    {
                        maxLogProb = logProb;
                        bestClass = c;
                    }
                }

                predictions.Add(bestClass);
            }

            return predictions;
        }
    }
}
