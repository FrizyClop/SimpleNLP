using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimpleNLP.Preprocessing
{
    /// <summary>
    /// Внутренний класс, отвечающий за токенизацию входного текста
    /// </summary>
    internal static class Tokenizer
    {
        /// <summary>
        /// Метод токенизации текста, удаление знаков препинания и escape-последовательностей
        /// </summary>
        /// <param name="sentences">Входные данные в виде списка предложений исходного текста</param>
        /// <returns>Список токенов без знаков препинания</returns>
        public static List<List<string>> TokenizeSentences(List<string> sentences)
        {
            List<List<string>> tokenized_sentences = new List<List<string>>();
            foreach (string sentence in sentences) {
                tokenized_sentences.Add(TokenizeSentence(sentence));
            }
            return tokenized_sentences;
        }

        private static List<string> TokenizeSentence(string sentence)
        {
            List<string> tokens = new List<string>();

            // Удаляем все знаки препинания (оставляем только буквы, цифры, дефисы)
            string cleaned = Regex.Replace(sentence, @"[^\w\s-]", "");

            // Разбиваем на слова по пробелам и неразрывным дефисам
            string[] words = Regex.Split(cleaned, @"[\s\u2011]+");

            foreach (string word in words)
            {
                if (!string.IsNullOrWhiteSpace(word))
                {
                    // Приводим к нижнему регистру и добавляем
                    tokens.Add(word.ToLowerInvariant());
                }
            }

            return tokens;
        }

        /// <summary>
        /// Метод токенизации текста, удаление знаков препинания и escape-последовательностей
        /// </summary>
        /// <param name="text">Входные данные в виде текста</param>
        /// <returns>Список токенов без знаков препинания</returns>
        public static List<string> TokenizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<string>();

            // Разделяем текст на слова, убирая знаки препинания
            var tokens = text.Split(new[] { ' ', '.', ',', '!', '?', ';', ':', '-', '—', '(', ')', '/', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            return tokens.Select(token => token.ToLowerInvariant()).ToList();
        }
    }
}
