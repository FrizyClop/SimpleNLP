using System.Text.Json;

namespace SimpleNLP.Classification
{
    public class NaiveBayesClassifier : PredictModel
    {
        private double alpha; // Лапласовский параметр
        private Dictionary<string, double> classProbs; // P(c)
        private Dictionary<string, Dictionary<int, double>> featureProbs; // P(w_i|c)
        private int vocabSize;

        public double Alpha { get { return alpha; } }
        public int VocabularySize { get { return vocabSize; } }
        public Dictionary<string, Dictionary<int, double>> FeatureProbs { get { return featureProbs; } }
        public Dictionary<string, double> ClassProbs {  get { return classProbs; } }

        public NaiveBayesClassifier(double alpha = 1.0)
        {
            this.alpha = alpha;
            this.classProbs = new Dictionary<string, double>();
            this.featureProbs = new Dictionary<string, Dictionary<int, double>>();
            this._classes = new List<string>();
        }

        public NaiveBayesClassifier(JsonElement json)
        {
            alpha = json.GetProperty("Alpha").GetDouble();
            classProbs = json.GetProperty("ClassProbs").Deserialize<Dictionary<string, double>>();
            featureProbs = json.GetProperty("FeatureProbs").Deserialize<Dictionary<string, Dictionary<int, double>>>();
            _classes = json.GetProperty("Classes").Deserialize<List<string>>();
            vocabSize = json.GetProperty("VocabSize").GetInt32();
            _isTrained = json.GetProperty("IsTrained").GetBoolean();
        }

        public override void Fit(List<double[]> X, List<string> y)
        {
            // Определяем классы
            this._classes = y.Distinct().ToList();
            int totalDocs = y.Count;

            // Вычисляем P(c) для каждого класса
            classProbs = _classes.ToDictionary(c => c, c => (double)y.Count(label => label == c) / totalDocs);

            // Размер словаря (количество признаков TF-IDF)
            vocabSize = X[0].Length;

            // Вычисляем P(w_i|c) с лапласовским сглаживанием
            foreach (var c in _classes)
            {
                // Выбираем документы текущего класса
                var classDocs = X.Where((_, idx) => y[idx] == c).ToList();

                // Сумма TF-IDF по всем документам класса (без alpha!)
                double[] featureSums = new double[vocabSize];
                for (int i = 0; i < vocabSize; i++)
                {
                    featureSums[i] = classDocs.Sum(doc => doc[i]);
                }

                // Общий TF-IDF класса + alpha * размер словаря (для нормировки)
                double totalSum = featureSums.Sum() + alpha * vocabSize;

                // Сохраняем P(w_i|c) = (TF-IDF слова в классе + alpha) / (totalSum)
                featureProbs[c] = new Dictionary<int, double>();
                for (int i = 0; i < vocabSize; i++)
                {
                    featureProbs[c][i] = (featureSums[i] + alpha) / totalSum;
                }
            }
            _isTrained = true;
        }

        public override List<string> Predict(List<double[]> X)
        {
            var predictions = new List<string>();
            for (int i = 0; i < X.Count; i++)
            {
                double maxLogProb = double.NegativeInfinity;
                string bestClass = null;

                foreach (var c in _classes)
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

            foreach (var c in _classes)
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

            foreach (var c in _classes)
            {
                double logProb = Math.Log(classProbs[c]);

                for (int j = 0; j < vocabSize; j++)
                {
                    if (x[j] > 0)
                    {
                        double prob = featureProbs[c][j];
                        // Защита от log(0)
                        logProb += Math.Log(Math.Max(prob, 1e-10)) * x[j];
                    }
                }

                double probExp = Math.Exp(logProb);
                probabilities[c] = probExp;
                total += probExp;
            }

            // Нормализация
            foreach (var c in _classes)
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
                Classes = _classes,
                VocabSize = vocabSize,
                IsTrained = _isTrained
            };
            return JsonSerializer.Serialize(data);
        }
    }

    public struct NaiveBayesParameters
    {
        private double _alpha;

        public double Alpha { get { return _alpha;  } }

        public NaiveBayesParameters(double Alpha)
        {
            _alpha = Alpha;
        }
    }
}
