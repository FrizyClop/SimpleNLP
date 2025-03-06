using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleNLP
{
    internal class Stemmer
    {
        public Stemmer()
        {

        }

        // Метод для стемминга слова
        internal protected IEnumerable<string> Stemming(IEnumerable<string> words)
        {
            if (words.Count() == 0)
                return words;
            var result_stemming = new List<string>();

            foreach (string word in words)
                 result_stemming.Add(Stem(word));

            return result_stemming;
        }

        // Метод для стемминга слова
        protected string Stem(string word)
        {
            if (string.IsNullOrEmpty(word))
                return word;

            // Удаляем окончания и суффиксы
            word = RemoveEndings(word);
            //word = RemoveSuffixes(word);

            return word;
        }

        // Метод для удаления окончаний
        private static string RemoveEndings(string word)
        {
            //string[] endings = new string[] { "ый", "ий", "ая", "яя", "ое", "ее", "ой", "ей", "ом", "ем", "ому", "ему", "ых", "их", "ую", "юю", "ами", "ями", "ах", "ях", "ия", "ии", "ие", "ью", "ью", "ию", "ием", "ией", "иям", "иях", "иями" };

            StreamReader stream_reader = new StreamReader("endings.txt");
            var endings = stream_reader.ReadToEnd().Split(new[] { '\r','\n' },StringSplitOptions.RemoveEmptyEntries).ToHashSet<string>();
            stream_reader.Close();

            foreach (var ending in endings)
            {
                if (word.EndsWith(ending))
                {
                    word = word.Substring(0, word.Length - ending.Length);
                    break;
                }
            }

            return word;
        }
    }
}
