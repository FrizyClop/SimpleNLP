
namespace SimpleNLP.Classification
{
    public class ClassificationMetrics
    {
        public double Accuracy { get; set; }
        public double Precision { get; set; }
        public double Recall { get; set; }
        public double F1Score { get; set; }

        public static ClassificationMetrics Evaluate(List<string> yTrue, List<string> yPred, string positiveClass)
        {
            int tp = 0, tn = 0, fp = 0, fn = 0;
            for (int i = 0; i < yTrue.Count; i++)
            {
                if (yTrue[i] == positiveClass && yPred[i] == positiveClass) tp++;
                else if (yTrue[i] == positiveClass && yPred[i] != positiveClass) fn++;
                else if (yTrue[i] != positiveClass && yPred[i] == positiveClass) fp++;
                else if (yTrue[i] != positiveClass && yPred[i] != positiveClass) tn++;
            }

            double accuracy = (double)(tp + tn) / (tp + tn + fp + fn);
            double precision = tp + fp == 0 ? 0 : (double)tp / (tp + fp);
            double recall = tp + fn == 0 ? 0 : (double)tp / (tp + fn);
            double f1 = precision + recall == 0 ? 0 : 2 * precision * recall / (precision + recall);

            return new ClassificationMetrics
            {
                Accuracy = accuracy,
                Precision = precision,
                Recall = recall,
                F1Score = f1
            };
        }

        public string ToJson() => System.Text.Json.JsonSerializer.Serialize(this, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
    }
}
