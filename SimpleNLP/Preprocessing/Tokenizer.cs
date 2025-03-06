using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        /// <param name="text">Входные данные в виде текста</param>
        /// <returns>Список токенов без знаков препинания</returns>
        public static List<string> Tokenize(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<string>();

            // Разделяем текст на слова, убирая знаки препинания
            var tokens = text.Split(new[] { ' ', '.', ',', '!', '?', ';', ':', '-', '—', '(', ')', '/', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            return tokens.Select(token => token.ToLowerInvariant()).ToList();
        }
    }
}
