using SimpleNLP.Preprocessing;
using SimpleNLP.Transformation;

namespace SimpleNLP
{
    public class NLPProcessor
    {
        public List<Text> texts;
        private MethodOfOneForm method_of_one_form = MethodOfOneForm.STEMMER;

        public NLPProcessor()
        {
            texts = new List<Text>();
        }

        // Строит словарь уникальных слов из всех документов
        //private static HashSet<string> BuildVocabulary(List<List<string>> docs)
        //{
        //    return new HashSet<string>(docs.SelectMany(doc => doc));
        //}
    }
}
