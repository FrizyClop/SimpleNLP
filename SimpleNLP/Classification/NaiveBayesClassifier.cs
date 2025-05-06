using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SimpleNLP.Classification
{
    public class NaiveBayesClassifier : PredictModel
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

        public override void Fit(List<double[]> X, List<string> y)
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

        public override List<string> Predict(List<double[]> X)
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

        public override string Predict(double[] X)
        {
            double maxLogProb = double.NegativeInfinity;
            string bestClass = null;

            foreach (var c in classes)
            {
                double logProb = Math.Log(classProbs[c]);

                for (int j = 0; j < vocabSize; j++)
                {
                    if (X[j] > 0) // Если признак (слово) присутствует
                    {
                        logProb += Math.Log(featureProbs[c][j]) * X[j]; // Учитываем TF-IDF вес
                    }
                }

                if (logProb > maxLogProb)
                {
                    maxLogProb = logProb;
                    bestClass = c;
                }
            }

            return bestClass;
        }

        public override Dictionary<string, double> PredictProbabilities(double[] x)
        {
            var probabilities = new Dictionary<string, double>();
            double total = 0.0;

            foreach (var c in classes)
            {
                // Начинаем с логарифма вероятности класса
                double logProb = Math.Log(classProbs[c]);

                // Добавляем вклад каждого признака
                for (int j = 0; j < vocabSize; j++)
                {
                    if (x[j] > 0) // Если признак присутствует
                    {
                        logProb += Math.Log(featureProbs[c][j]) * x[j];
                    }
                }

                // Преобразуем из логарифмической шкалы обратно в вероятности
                double prob = Math.Exp(logProb);
                probabilities[c] = prob;
                total += prob;
            }

            // Нормализуем вероятности
            foreach (var c in classes)
            {
                probabilities[c] /= total;
            }

            return probabilities;
        }

        public override List<Dictionary<string, double>> PredictProbabilities(List<double[]> X)
        {
            return X.Select(x => PredictProbabilities(x)).ToList();
        }

        public override string GetJsonRepresentation()
        {
            var data = new
            {
                Model = "NaiveBayes",
                Alpha = alpha,
                ClassProbs = classProbs,
                FeatureProbs = featureProbs,
                Classes = classes,
                VocabSize = vocabSize
            };
            return JsonSerializer.Serialize(data);
        }
    }
}
