
namespace SimpleNLP.Preprocessing
{
    /// <summary>
    /// Внутренний класс, используемый для предподготовки данных к анализу
    /// </summary>
    public static class Preprocessor
    {
        /// <summary>
        /// Метод предподготовки данных перед анализом
        /// </summary>
        /// <param name="text">Данные для предподготовки</param>
        /// <returns>(предварительно) Список слов из текста в виде токенов</returns>
        public static List<string> Preprocess(string text, MethodOfOneForm reduction_to_one_form = MethodOfOneForm.NONE)
        {
            List<string> tokens = Tokenizer.TokenizeText(text);
            tokens = Filtrator.FilterTokens(tokens);
            switch(reduction_to_one_form)
            {
                case MethodOfOneForm.NONE:
                {
                    break;
                }
                case MethodOfOneForm.STEMMER:
                {
                    tokens = Stemmer.Stemming(tokens);  //Используем стемминг для упрощения форм слов
                    break;
                }
                case MethodOfOneForm.LEMMATIZATOR:
                {
                    tokens = Lemmatizator.Lemmatization(tokens); //Используем лемматизатор для приведения к одной форме
                    break;
                }
            }

            return tokens;  //Возвращаем отфильтрованный список токенов
        }
    }

    public enum MethodOfOneForm { NONE ,STEMMER, LEMMATIZATOR }
}
