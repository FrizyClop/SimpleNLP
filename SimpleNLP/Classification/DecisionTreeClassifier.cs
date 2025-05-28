using System.Text.Json;

namespace SimpleNLP.Classification
{
    public class DecisionTreeClassifier : PredictModel
    {
        private Node _root;
        private int _maxDepth;

        public int MaxDepth { get { return _maxDepth; } }

        public DecisionTreeClassifier(DecisionTreeParameters parameters)
        {
            _maxDepth = parameters.MaxDepth;
            _classes = new List<string>();
        }

        public DecisionTreeClassifier(JsonElement json)
        {
            _maxDepth = json.GetProperty("MaxDepth").GetInt32();
            _classes = JsonSerializer.Deserialize<List<string>>(json.GetProperty("Classes"));
            _root = JsonSerializer.Deserialize<Node>(json.GetProperty("Tree"));
            _isTrained = json.GetProperty("IsTrained").GetBoolean();
        }

        public override void Fit(List<double[]> X, List<string> y)
        {
            _classes = y.Distinct().ToList();
            _root = BuildTree(X, y, depth: 0);
            _isTrained = true;
        }

        public override string Predict(double[] x)
        {
            return Traverse(_root, x);
        }

        public override List<string> Predict(List<double[]> X)
        {
            return X.Select(Predict).ToList();
        }

        public override Dictionary<string, double> PredictProbabilities(double[] x)
        {
            return TraverseProbabilities(_root, x);
        }

        public override List<Dictionary<string, double>> PredictProbabilities(List<double[]> batchX)
        {
            return batchX.Select(PredictProbabilities).ToList();
        }

        public override string GetJsonRepresentation()
        {
            var data = new
            {
                Model = "DecisionTree",
                MaxDepth = _maxDepth,
                Classes = _classes,
                Tree = _root,
                IsTrained = _isTrained
            };
            return JsonSerializer.Serialize(data);
        }

        private Node BuildTree(List<double[]> X, List<string> y, int depth)
        {
            if (depth >= _maxDepth || y.Distinct().Count() == 1 || y.Count < 2)
            {
                string label = y.GroupBy(l => l).OrderByDescending(g => g.Count()).First().Key;
                return new Node
                {
                    IsLeaf = true,
                    Label = label,
                    ClassCounts = y.GroupBy(c => c).ToDictionary(g => g.Key, g => g.Count())
                };
            }

            int bestFeature = -1;
            double bestThreshold = 0;
            double bestGini = double.MaxValue;

            for (int feature = 0; feature < X[0].Length; feature++)
            {
                var thresholds = X.Select(v => v[feature]).Distinct().OrderBy(t => t);

                foreach (var threshold in thresholds)
                {
                    var leftYa = new List<string>();
                    var rightYa = new List<string>();

                    for (int i = 0; i < X.Count; i++)
                    {
                        if (X[i][feature] <= threshold) leftYa.Add(y[i]);
                        else rightYa.Add(y[i]);
                    }

                    if (leftYa.Count == 0 || rightYa.Count == 0) continue;

                    double gini = Gini(leftYa, rightYa);

                    if (gini < bestGini)
                    {
                        bestGini = gini;
                        bestFeature = feature;
                        bestThreshold = threshold;
                    }
                }
            }

            if (bestFeature == -1)
            {
                string label = y.GroupBy(l => l).OrderByDescending(g => g.Count()).First().Key;
                return new Node
                {
                    IsLeaf = true,
                    Label = label,
                    ClassCounts = y.GroupBy(c => c).ToDictionary(g => g.Key, g => g.Count())
                };
            }

            var leftIndices = X.Select((_, i) => i).Where(i => X[i][bestFeature] <= bestThreshold).ToList();
            var rightIndices = X.Select((_, i) => i).Where(i => X[i][bestFeature] > bestThreshold).ToList();

            var leftX = leftIndices.Select(i => X[i]).ToList();
            var leftY = leftIndices.Select(i => y[i]).ToList();
            var rightX = rightIndices.Select(i => X[i]).ToList();
            var rightY = rightIndices.Select(i => y[i]).ToList();

            return new Node
            {
                FeatureIndex = bestFeature,
                Threshold = bestThreshold,
                Left = BuildTree(leftX, leftY, depth + 1),
                Right = BuildTree(rightX, rightY, depth + 1),
                IsLeaf = false
            };
        }

        private string Traverse(Node node, double[] x)
        {
            if (node.IsLeaf)
                return node.Label;

            if (x[node.FeatureIndex] <= node.Threshold)
                return Traverse(node.Left, x);
            else
                return Traverse(node.Right, x);
        }

        private Dictionary<string, double> TraverseProbabilities(Node node, double[] x)
        {
            if (node.IsLeaf)
            {
                var total = node.ClassCounts?.Values.Sum() ?? 1;
                return _classes.ToDictionary(
                    c => c,
                    c => node.ClassCounts != null && node.ClassCounts.ContainsKey(c)
                        ? (double)node.ClassCounts[c] / total
                        : 0.0);
            }

            if (x[node.FeatureIndex] <= node.Threshold)
                return TraverseProbabilities(node.Left, x);
            else
                return TraverseProbabilities(node.Right, x);
        }

        private double Gini(List<string> left, List<string> right)
        {
            double Total(int count, List<string> subset)
            {
                if (count == 0) return 0;
                return 1.0 - subset.GroupBy(c => c)
                                    .Select(g => Math.Pow((double)g.Count() / count, 2))
                                    .Sum();
            }

            int total = left.Count + right.Count;
            return (left.Count * Total(left.Count, left) + right.Count * Total(right.Count, right)) / total;
        }

        private double Entropy(List<string> left, List<string> right)
        {
            double H(List<string> list)
            {
                int total = list.Count;
                if (total == 0) return 0;
                return -list.GroupBy(c => c)
                            .Select(g => (double)g.Count() / total)
                            .Sum(p => p * Math.Log2(p));
            }

            int totalCount = left.Count + right.Count;
            return (left.Count * H(left) + right.Count * H(right)) / totalCount;
        }


        public class Node
        {
            public bool IsLeaf { get; set; }
            public string Label { get; set; }
            public int FeatureIndex { get; set; }
            public double Threshold { get; set; }
            public Node Left { get; set; }
            public Node Right { get; set; }

            public Dictionary<string, int> ClassCounts { get; set; }
        }
    }

    public struct DecisionTreeParameters
    {
        public int MaxDepth { get; }

        public DecisionTreeParameters(int maxDepth)
        {
            MaxDepth = maxDepth;
        }
    }

    public enum TreeFunction
    {
        Gini,
        Entropy
    }
}
