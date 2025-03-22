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
        public static List<string> Preprocess(string text, MethodOfOneForm stem_or_lemm)
        {
            var tokens = Tokenizer.Tokenize(text);  //Токенизируем текст, и приводим все слова к нижнему регистру
            tokens = Filtrator.FilterTokens(tokens);//Фильтруем токены, удаляя русские стоп-слова и числа
            if(stem_or_lemm == MethodOfOneForm.STEMMER)
                tokens = Stemmer.Stemming(tokens);  //Используем стемминг для упрощения форм слов
            else                                                            //иначе
                tokens = Lemmatizator.Lemmatization(tokens); //Используем лемматизатор для приведения к одной форме
            return tokens;                                      //Возвращаем отфильтрованный список токенов
        }
    }

    public enum MethodOfOneForm { STEMMER, LEMMATIZATOR }
}
