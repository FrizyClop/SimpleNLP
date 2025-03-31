using SimpleNLP.Preprocessing;
using SimpleNLP.Transformation;

namespace SimpleNLP
{
    public class NLPProcessor
    {
        public List<List<string>> texts;
        public MethodOfOneForm method_of_one_form = MethodOfOneForm.STEMMER;
        public List<double[]> vectors;

        public NLPProcessor(string text)
        {
            texts = new List<List<string>>();
            AddText(text);
        }

        public NLPProcessor(List<string> in_texts)
        {
            texts = new List<List<string>>();
            foreach (string text in in_texts)
            {
                AddText(text);
            }
        }

        public bool AddText(string text)
        {
            texts.Add(Preprocessor.Preprocess(text, method_of_one_form));
            return true;
        }

        public bool Vectorize()
        {
            vectors = TF_IDF.VectorizeAll(texts);
            return true;
        }
    }
}
