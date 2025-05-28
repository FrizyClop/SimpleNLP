
namespace SimpleNLP.Classification
{
    public class ClassificationMetrics
    {
        public double Accuracy { get; set; }
        public double Precision { get; set; }
        public double Recall { get; set; }
        public double F1Score { get; set; }

        public static ClassificationMetrics Evaluate(List<string> yTrue, List<string> yPred)
        {
            var uniqueClasses = yTrue.Union(yPred).Distinct().ToList();

            double macroPrecision = 0;
            double macroRecall = 0;
            double macroF1 = 0;

            foreach (var cls in uniqueClasses)
            {
                int tp = 0, fp = 0, fn = 0;

                for (int i = 0; i < yTrue.Count; i++)
                {
                    if (yTrue[i] == cls && yPred[i] == cls) tp++;
                    else if (yTrue[i] != cls && yPred[i] == cls) fp++;
                    else if (yTrue[i] == cls && yPred[i] != cls) fn++;
                }

                double precision = tp + fp == 0 ? 0 : (double)tp / (tp + fp);
                double recall = tp + fn == 0 ? 0 : (double)tp / (tp + fn);
                double f1 = (precision + recall) == 0 ? 0 : 2 * precision * recall / (precision + recall);

                macroPrecision += precision;
                macroRecall += recall;
                macroF1 += f1;
            }

            int classCount = uniqueClasses.Count;

            double accuracy = yTrue.Zip(yPred, (t, p) => t == p ? 1 : 0).Average();

            return new ClassificationMetrics
            {
                Accuracy = accuracy,
                Precision = macroPrecision / classCount,
                Recall = macroRecall / classCount,
                F1Score = macroF1 / classCount
            };
        }

        public string ToJson() => System.Text.Json.JsonSerializer.Serialize(this, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
    }
}
