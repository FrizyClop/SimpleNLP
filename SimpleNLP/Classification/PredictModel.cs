using System.Text.Json;

namespace SimpleNLP.Classification
{
    public abstract class PredictModel
    {
        public abstract void Fit(List<double[]> X, List<string> y);
        public abstract string Predict(double[] x);
        public abstract List<string> Predict(List<double[]> X);
        public abstract Dictionary<string, double> PredictProbabilities(double[] x);
        public abstract List<Dictionary<string, double>> PredictProbabilities(List<double[]> batchX);
        public abstract string GetJsonRepresentation();
    }
}
