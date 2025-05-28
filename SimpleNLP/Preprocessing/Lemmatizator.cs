namespace SimpleNLP.Preprocessing
{
    internal static class Lemmatizator
    {
        public static List<List<string>> LemmatizationAll(List<List<string>> tokenized_sentences)
        {
            for (int i = 0; i < tokenized_sentences.Count; i++)
                tokenized_sentences[i] = Lemmatization(tokenized_sentences[i]);
            return tokenized_sentences;
        }

        public static List<string> Lemmatization(List<string> words)
        {
            if (words.Count == 0) return words;

            for (int i = 0; i < words.Count; i++)
                words[i] = FindLemm(words[i]);

            return words;
        }

        private static string FindLemm(string word)
        {
            if(word.Length <= 0) return word;
            word = ReplaceE(word);
            char firstChar = word[0];
            if ((firstChar >= 'А' && firstChar <= 'Я') ||
            (firstChar >= 'а' && firstChar <= 'я') ||
            firstChar == 'Ё' || firstChar == 'ё')
            {
                if (firstChar == 'ь' || firstChar == 'ъ' || firstChar == 'ы')
                    return word;
                using (StreamReader reader = new StreamReader("Dictionary\\" + word[0] + ".txt"))
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
            else { return word; }
        }

        private static string ReplaceE(string word)
        {
            return word.Replace('ё', 'e');
        }
    }
}
