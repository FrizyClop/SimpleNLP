
namespace SimpleNLPApp
{
    public static class MetricsVisualizer
    {
        public static Dictionary<(string trueClass, string predClass), int> GetConfusionMatrix(IEnumerable<TextEvaluationViewModel> texts)
        {
            var result = new Dictionary<(string, string), int>();

            foreach (var text in texts)
            {
                if (text.TrueClass != null && text.PredictedClass != null)
                {
                    var key = (text.TrueClass, text.PredictedClass);
                    if (result.ContainsKey(key))
                        result[key]++;
                    else
                        result[key] = 1;
                }
            }

            return result;
        }

        public static Dictionary<string, (double Precision, double Recall)> GetPrecisionRecall(IEnumerable<TextEvaluationViewModel> texts, List<string> allClasses)
        {
            var metrics = new Dictionary<string, (double Precision, double Recall)>();

            foreach (var cls in allClasses)
            {
                int tp = texts.Count(t => t.TrueClass == cls && t.PredictedClass == cls);
                int fp = texts.Count(t => t.TrueClass != cls && t.PredictedClass == cls);
                int fn = texts.Count(t => t.TrueClass == cls && t.PredictedClass != cls);

                double precision = tp + fp == 0 ? 0 : (double)tp / (tp + fp);
                double recall = tp + fn == 0 ? 0 : (double)tp / (tp + fn);

                metrics[cls] = (precision, recall);
            }

            return metrics;
        }
    }
}
