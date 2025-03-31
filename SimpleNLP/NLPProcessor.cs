using SimpleNLP.Preprocessing;
using SimpleNLP.Transformation;

namespace SimpleNLP
{
    public class NLPProcessor
    {
        public List<List<string>> texts;
        public HashSet<string> vocabulary;
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
            vocabulary = BuildVocabulary(texts);
            return true;
        }

        public bool Vectorize()
        {
            vectors = TF_IDF.VectorizeAll(texts,vocabulary);
            return true;
        }

        // Строит словарь уникальных слов из всех документов
        private static HashSet<string> BuildVocabulary(List<List<string>> docs)
        {
            return new HashSet<string>(docs.SelectMany(doc => doc));
        }
    }
}
