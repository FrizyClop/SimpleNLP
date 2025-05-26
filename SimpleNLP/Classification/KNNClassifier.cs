using System.Text.Json;

namespace SimpleNLP.Classification
{
    public class KNNClassifier : PredictModel
    {
        private List<double[]> _trainVectors;
        private List<string> _trainLabels;
        private int _k;

        public int K { get { return _k; } }

        public KNNClassifier(int k = 3)
        {
            _trainVectors = new List<double[]>();
            _trainLabels = new List<string>();
            _k = k;
        }

        public KNNClassifier(JsonElement json)
        {
            _k = json.GetProperty("K").GetInt32();
            _trainVectors = json.GetProperty("TrainVectors").Deserialize<List<double[]>>();
            _trainLabels = json.GetProperty("TrainLabels").Deserialize<List<string>>();
            _classes = json.GetProperty("Classes").Deserialize<List<string>>();
            _isTrained = json.GetProperty("IsTrained").GetBoolean();
        }

        public override void Fit(List<double[]> X, List<string> y)
        {
            _trainVectors = X;
            _trainLabels = y;
            _classes = y.Distinct().ToList();
            _isTrained = true;
        }

        public override string Predict(double[] x)
        {
            if (!_isTrained) throw new InvalidOperationException("Модель не обучена.");

            var neighbors = _trainVectors
                .Select((vec, i) => new
                {
                    Label = _trainLabels[i],
                    Dist = CosineDistance(vec, x)
                })
                .OrderBy(p => p.Dist)
                .Take(_k)
                .GroupBy(p => p.Label)
                .OrderByDescending(g => g.Count())
                .First()
                .Key;

            return neighbors;
        }

        public override List<string> Predict(List<double[]> X)
        {
            return X.Select(Predict).ToList();
        }

        public override Dictionary<string, double> PredictProbabilities(double[] x)
        {
            if (!_isTrained) throw new InvalidOperationException("Модель не обучена.");

            var neighbors = _trainVectors
                .Select((vec, i) => new
                {
                    Label = _trainLabels[i],
                    Dist = CosineDistance(vec, x)
                })
                .OrderBy(p => p.Dist)
                .Take(_k);

            var grouped = neighbors
                .GroupBy(n => n.Label)
                .ToDictionary(g => g.Key, g => (double)g.Count() / _k);

            return _classes.ToDictionary(c => c, c => grouped.ContainsKey(c) ? grouped[c] : 0.0);
        }

        public override List<Dictionary<string, double>> PredictProbabilities(List<double[]> batchX)
        {
            return batchX.Select(PredictProbabilities).ToList();
        }

        public override string GetJsonRepresentation()
        {
            var data = new
            {
                Model = "KNN",
                K = _k,
                Classes = _classes,
                TrainVectors = _trainVectors,
                TrainLabels = _trainLabels,
                IsTrained = _isTrained
            };

            return JsonSerializer.Serialize(data);
        }

    private double CosineDistance(double[] a, double[] b)
        {
            double dot = 0, magA = 0, magB = 0;
            for (int i = 0; i < a.Length; i++)
            {
                dot += a[i] * b[i];
                magA += a[i] * a[i];
                magB += b[i] * b[i];
            }

            return 1.0 - dot / (Math.Sqrt(magA) * Math.Sqrt(magB) + 1e-10); // косинусная метрика
        }
    }

    public struct KNNParameters
    {
        private int _k;

        public int K => _k;

        public KNNParameters(int k)
        {
            _k = k;
        }
    }

}

