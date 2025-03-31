using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimpleNLP.Preprocessing
{
    internal static class TextSplitter
    {
        public static List<string> SplitIntoSentences(string text)
        {
            List<string> sentences = new List<string>();

            // Нормализуем переносы строк и лишние пробелы
            text = Regex.Replace(text, @"(\s*\r\n\s*)+", "\n").Trim();

            // Разделяем по:
            // 1. Обычным правилам (точка, !, ? + пробел + заглавная буква/цифра)
            // 2. Перенос строки (\n), если после него идет заглавная буква/цифра
            string pattern = @"(?<=[.!?])\s+(?=[А-ЯA-Z0-9])|(?<=\S)\n(?=[А-ЯA-Z0-9])";

            string[] splitSentences = Regex.Split(text, pattern);

            foreach (var sentence in splitSentences)
            {
                if (!string.IsNullOrWhiteSpace(sentence))
                {
                    sentences.Add(sentence.Trim());
                }
            }

            return sentences;
        }
    }
}
