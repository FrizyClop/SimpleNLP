using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleNLP.Preprocessing
{
    internal static class Lemmatizator
    {
        public static List<string> Lemmatization(List<string> words)
        {
            if (words.Count == 0) return words;

            for (int i = 0; i < words.Count; i++)
                words[i] = FindLemm(words[i]);

            return words;
        }

        private static string FindLemm(string word)
        {
            using (StreamReader reader = new StreamReader("lemms.csv"))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (!line.StartsWith(word[0]))
                        continue;

                    string[] parts = line.Split(';');
                    if (parts[0] == word)
                        return parts[1];
                }
                reader.Close();
                reader.Dispose();
            }
            return word;
        }
    }
}
