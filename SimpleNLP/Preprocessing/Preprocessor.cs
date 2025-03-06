using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleNLP.Preprocessing
{
    /// <summary>
    /// Внутренний класс, используемый для предподготовки данных к анализу
    /// </summary>
    internal static class Preprocessor
    {
        /// <summary>
        /// Метод предподготовки данных перед анализом
        /// </summary>
        /// <param name="text">Данные для предподготовки</param>
        /// <returns>(предварительно) Список слов из текста в виде токенов</returns>
        public static List<string> Preprocess(string text)
        {
            var tokens = Tokenizer.Tokenize(text);  //Токенизируем текст, и приводим все слова к нижнему регистру
            tokens = Filtrator.FilterTokens(tokens);//Фильтруем токены, удаляя русские стоп-слова и числа
            return tokens;                                      //Возвращаем отфильтрованный список токенов
        }
    }
}
