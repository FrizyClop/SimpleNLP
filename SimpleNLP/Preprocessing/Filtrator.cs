using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleNLP.Preprocessing
{
    /// <summary>
    /// Внутренний класс, отвечающий за фильтрацию токенов
    /// </summary>
    internal static class Filtrator
    {
        public static List<List<string>> FilterSentences(List<List<string>> tokenized_sentences)
        {
            for(int i = 0; i < tokenized_sentences.Count;i++)
                tokenized_sentences[i] = FilterTokens(tokenized_sentences[i]);
            return tokenized_sentences;
        }
        /// <summary>
        /// Метод фильтрации токенов, удаляющий русские стоп-слова и числа
        /// </summary>
        /// <param name="tokens">Список токенов</param>
        /// <returns>Список отфильтрованных токенов</returns>
        public static List<string> FilterTokens(List<string> tokens)
        {
            var filtred_tokens = RemoveStopWords(tokens);
            filtred_tokens = RemoveNumbers(filtred_tokens);
            filtred_tokens = RemoveOneLetter(filtred_tokens);
            return filtred_tokens;
        }

        /// <summary>
        /// Метод удаления русских-стоп слов из списка токенов
        /// </summary>
        /// <param name="tokens">Список токенов</param>
        /// <returns>Список токенов без русских стоп-слов</returns>
        private static List<string> RemoveStopWords(List<string> tokens)
        {
            StreamReader stream_reader = new StreamReader("russian_stop_words.txt");
            List<string> stop_words = stream_reader.ReadToEnd().Split("\n").ToList();
            stream_reader.Close();
            return tokens.Where(token => !stop_words.Contains(token)).ToList();
        }

        /// <summary>
        /// Метод удаления чисел из списка токенов
        /// </summary>
        /// <param name="tokens">Список токенов</param>
        /// <returns>Список токенов без чисел</returns>
        private static List<string> RemoveNumbers(List<string> tokens)
        {
            int num;
            return tokens.Where(token => !int.TryParse(token, out num)).ToList();
        }

        /// <summary>
        /// Метод удаления токенов состоящих из одной буквы
        /// </summary>
        /// <param name="tokens">Список токенов</param>
        /// <returns>Список токенов без токенов состоящих из одной буквы</returns>
        private static List<string> RemoveOneLetter(List<string> tokens)
        {
            return tokens.Where(token => token.Length > 1).ToList();
        }
    }
}
