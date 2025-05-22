using System.Text.Json;

namespace SimpleNLP.Classification
{
    public class SVMClassifier : PredictModel
    {
        private Dictionary<string, double[]> weights; // Веса для каждого класса (one-vs-rest)
        private Dictionary<string, double> biases;    // Смещения для каждого класса
        private int maxIterations;                   // Максимальное число итераций обучения
        private double learningRate;                 // Скорость обучения
        private double lambda;                      // Параметр регуляризации

        public int MaxIterations { get { return maxIterations; } }
        public double LearningRate {  get { return learningRate; } }
        public double Lambda {  get { return lambda; } }

        public SVMClassifier(int maxIterations = 1000, double learningRate = 0.01, double lambda = 0.01)
        {
            this.maxIterations = maxIterations;
            this.learningRate = learningRate;
            this.lambda = lambda;
            this.weights = new Dictionary<string, double[]>();
            this.biases = new Dictionary<string, double>();
            this._classes = new List<string>();
        }

        public SVMClassifier(JsonElement json)
        {
            weights = json.GetProperty("Weights").Deserialize<Dictionary<string, double[]>>();
            biases = json.GetProperty("Biases").Deserialize<Dictionary<string, double>>();
            _classes = json.GetProperty("Classes").Deserialize<List<string>>();
            maxIterations = json.GetProperty("MaxIterations").GetInt32();
            learningRate = json.GetProperty("LearningRate").GetDouble();
            lambda = json.GetProperty("Lambda").GetDouble();
            _isTrained = json.GetProperty("IsTrained").GetBoolean();
        }

        public override void Fit(List<double[]> X, List<string> y)
        {
            _classes = y.Distinct().ToList();
            int featureCount = X[0].Length;

            // Инициализация весов и смещений для каждого класса (one-vs-rest)
            foreach (var c in _classes)
            {
                weights[c] = new double[featureCount];
                biases[c] = 0;
            }

            // Обучение SVM (стохастический градиентный спуск)
            for (int iter = 0; iter < maxIterations; iter++)
            {
                for (int i = 0; i < X.Count; i++)
                {
                    double[] x = X[i];
                    string trueClass = y[i];

                    foreach (var c in _classes)
                    {
                        // Вычисляем отступ для текущего класса
                        double margin = biases[c];
                        for (int j = 0; j < featureCount; j++)
                        {
                            margin += weights[c][j] * x[j];
                        }

                        // Если это trueClass, желаем margin >= 1, иначе margin <= -1
                        double target = (c == trueClass) ? 1 : -1;

                        // Вычисляем градиент и обновляем веса
                        if (target * margin < 1)
                        {
                            for (int j = 0; j < featureCount; j++)
                            {
                                weights[c][j] += learningRate * (target * x[j] - lambda * weights[c][j]);
                            }
                            biases[c] += learningRate * target;
                        }
                        else
                        {
                            // L2-регуляризация
                            for (int j = 0; j < featureCount; j++)
                            {
                                weights[c][j] -= learningRate * lambda * weights[c][j];
                            }
                        }
                    }
                }
            }
            _isTrained = true;
        }

        public override string Predict(double[] x)
        {
            double maxScore = double.NegativeInfinity;
            string bestClass = null;

            foreach (var c in _classes)
            {
                double score = biases[c];
                for (int j = 0; j < x.Length; j++)
                {
                    score += weights[c][j] * x[j];
                }

                if (score > maxScore)
                {
                    maxScore = score;
                    bestClass = c;
                }
            }

            return bestClass;
        }

        public override List<string> Predict(List<double[]> X)
        {
            return X.Select(x => Predict(x)).ToList();
        }

        public override Dictionary<string, double> PredictProbabilities(double[] x)
        {
            // Для SVM нет естественных вероятностей, используем softmax по scores
            var scores = new Dictionary<string, double>();
            double total = 0.0;

            foreach (var c in _classes)
            {
                double score = biases[c];
                for (int j = 0; j < x.Length; j++)
                {
                    score += weights[c][j] * x[j];
                }
                double expScore = Math.Exp(score);
                scores[c] = expScore;
                total += expScore;
            }

            // Нормализация (softmax)
            return scores.ToDictionary(kvp => kvp.Key, kvp => kvp.Value / total);
        }

        public override List<Dictionary<string, double>> PredictProbabilities(List<double[]> X)
        {
            return X.Select(x => PredictProbabilities(x)).ToList();
        }

        public override string GetJsonRepresentation()
        {
            var data = new
            {
                Model = "SVM",
                Weights = weights,
                Biases = biases,
                Classes = _classes,
                MaxIterations = maxIterations,
                LearningRate = learningRate,
                Lambda = lambda,
                IsTrained = _isTrained
            };
            return JsonSerializer.Serialize(data);
        }
    }

    public struct SVMParameters
    {
        private int _max_iterations;
        private double _learningRate;
        private double _lambda;

        public int MaxIterations { get { return _max_iterations; } }
        public double LearningRate { get { return _learningRate; } }
        public double Lambda { get { return _lambda; } }

        public SVMParameters(int MaxIterations, double LearningRate, double Lambda)
        {
            _max_iterations = MaxIterations;
            _learningRate = LearningRate;
            _lambda = Lambda;
        }
    }
}