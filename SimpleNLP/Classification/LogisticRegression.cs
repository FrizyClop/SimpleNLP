using System.Reflection;
using System.Text.Json;

namespace SimpleNLP.Classification
{
    public class LogisticRegression : PredictModel
    {
        private double[][] weights; // weights[k][j] — вес для класса k и признака j
        private double[] biases;    // biases[k] — смещение для класса k
        private readonly double learningRate;
        private readonly int epochs;
        private Dictionary<string, int> _classToIndex; // Словарь для преобразования строк в индексы

        public double LearningRate { get { return learningRate; } }
        public int Epochs { get { return epochs; } }

        public LogisticRegression(double learningRate = 0.01, int epochs = 1000)
        {
            this.learningRate = learningRate;
            this.epochs = epochs;
        }

        public LogisticRegression(JsonElement json)
        {
            weights = json.GetProperty("Weights").Deserialize<double[][]>();
            biases = json.GetProperty("Biases").Deserialize<double[]>();
            learningRate = json.GetProperty("LearningRate").GetDouble();
            epochs = json.GetProperty("Epochs").GetInt32();
            _classToIndex = json.GetProperty("ClassToIndex").Deserialize<Dictionary<string, int>>();
            _classes = json.GetProperty("Classes").Deserialize<List<string>>();
            _isTrained = json.GetProperty("IsTrained").GetBoolean();
        }

        // Softmax: преобразует scores в вероятности [0, 1]
        private double[] Softmax(double[] scores)
        {
            double[] expScores = scores.Select(s => Math.Exp(s - scores.Max())).ToArray(); // Для численной стабильности
            double sumExp = expScores.Sum();
            return expScores.Select(e => e / sumExp).ToArray();
        }

        // Обучение модели (принимает List<double[]> и List<string>)
        public override void Fit(List<double[]> X, List<string> y)
        {
            if (X.Count != y.Count)
                throw new ArgumentException("Количество образцов и меток должно совпадать.");

            // Создаём словарь для преобразования строк в индексы
            _classes = y.Distinct().ToList();
            _classToIndex = _classes.ToDictionary(c => c, c => _classes.IndexOf(c));

            int nSamples = X.Count;
            int nFeatures = X[0].Length;
            int nClasses = _classes.Count;

            // Инициализация весов и смещений
            weights = new double[nClasses][];
            biases = new double[nClasses];
            for (int k = 0; k < nClasses; k++)
            {
                weights[k] = new double[nFeatures];
                biases[k] = 0;
            }

            for (int epoch = 0; epoch < epochs; epoch++)
            {
                double[][] gradientsWeights = new double[nClasses][];
                double[] gradientsBiases = new double[nClasses];
                for (int k = 0; k < nClasses; k++)
                    gradientsWeights[k] = new double[nFeatures];

                for (int i = 0; i < nSamples; i++)
                {
                    // Вычисляем scores для каждого класса
                    double[] scores = new double[nClasses];
                    for (int k = 0; k < nClasses; k++)
                        scores[k] = weights[k].Zip(X[i], (w, x) => w * x).Sum() + biases[k];

                    // Преобразуем в вероятности через Softmax
                    double[] probabilities = Softmax(scores);

                    // Получаем индекс истинного класса
                    int trueClassIndex = _classToIndex[y[i]];

                    // Вычисляем градиенты
                    for (int k = 0; k < nClasses; k++)
                    {
                        double gradient = probabilities[k] - (trueClassIndex == k ? 1 : 0);
                        for (int j = 0; j < nFeatures; j++)
                            gradientsWeights[k][j] += gradient * X[i][j];
                        gradientsBiases[k] += gradient;
                    }
                }

                // Обновляем веса и смещения
                for (int k = 0; k < nClasses; k++)
                {
                    for (int j = 0; j < nFeatures; j++)
                        weights[k][j] -= learningRate * gradientsWeights[k][j] / nSamples;
                    biases[k] -= learningRate * gradientsBiases[k] / nSamples;
                }
            }
            _isTrained = true;
        }

        // Предсказание вероятностей для каждого класса
        public override Dictionary<string, double> PredictProbabilities(double[] x)
        {
            double[] scores = new double[_classes.Count];
            for (int k = 0; k < _classes.Count; k++)
                scores[k] = weights[k].Zip(x, (w, xi) => w * xi).Sum() + biases[k];

            double[] proba = Softmax(scores);
            var result = new Dictionary<string, double>();
            for (int k = 0; k < _classes.Count; k++)
                result[_classes[k]] = proba[k];
            return result;
        }

        public override List<Dictionary<string, double>> PredictProbabilities(List<double[]> batchX)
        {
            return batchX.Select(x => PredictProbabilities(x)).ToList();
        }

        // Предсказание класса (возвращает строку с именем класса)
        public override string Predict(double[] x)
        {
            var proba = PredictProbabilities(x);
            return proba.OrderByDescending(p => p.Value).First().Key;
        }

        public override List<string> Predict(List<double[]> X)
        {
            List<string> predictions = new List<string>();
            foreach (double[] x in X)
                predictions.Add(Predict(x));
            return predictions;
        }

        public override string GetJsonRepresentation()
        {
            var data = new
            {
                Model = "LogisticRegression",
                Weights = weights,
                Biases = biases,
                LearningRate = learningRate,
                Epochs = epochs,
                ClassToIndex = _classToIndex,
                Classes = _classes,
                IsTrained = _isTrained
            };
            return JsonSerializer.Serialize(data);
        }
    }

    public struct LogisticRegressionParameters
    {
        private double _learning_rate;
        private int _epochs;

        public double LearningRate { get { return _learning_rate; } }
        public int Epochs { get { return _epochs; } }

        public LogisticRegressionParameters(double LearningRate, int Epochs)
        {
            _learning_rate = LearningRate;
            _epochs = Epochs;
        }
    }
}
