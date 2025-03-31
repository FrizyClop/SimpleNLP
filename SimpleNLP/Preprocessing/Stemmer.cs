using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Diagnostics.Tracing;

namespace SimpleNLP.Preprocessing
{
    internal static class Stemmer
    {
        //совершенный вид глаголов
        private static Regex PERFECTIVEGROUND = new Regex("((ив|ивши|ившись|ыв|ывши|ывшись)|((<;=[ая])(в|вши|вшись)))$");
        //возвратные частицы
        private static Regex REFLEXIVE = new Regex("(с[яь])$");
        //прилагательные
        private static Regex ADJECTIVE = new Regex("(ее|ие|ые|ое|ими|ыми|ей|ий|ый|ой|ем|им|ым|ом|его|ого|ему|ому|их|ых|ую|юю|ая|яя|ою|ею)$");
        //причастия
        private static Regex PARTICIPLE = new Regex("((ивш|ывш|ующ)|((?<=[ая])(ем|нн|вш|ющ|щ)))$");
        //глаголы
        private static Regex VERB = new Regex("((ила|ыла|ена|ейте|уйте|ите|или|ыли|ей|уй|ил|ыл|им|ым|ен|ило|ыло|ено|ят|ует|уют|ит|ыт|ены|ить|ыть|ишь|ую|ю)|((?<=[ая])(ла|на|ете|йте|ли|й|л|ем|н|ло|но|ет|ют|ны|ть|ешь|нно)))$");
        //существительные
        private static Regex NOUN = new Regex("(а|ев|ов|ие|ье|е|иями|ями|ами|еи|ии|и|ией|ей|ой|ий|й|иям|ям|ием|ем|ам|ом|о|у|ах|иях|ях|ы|ь|ию|ью|ю|ия|ья|я)$");
        //регулярное выражение для выделения основы слова
        private static Regex RVRE = new Regex("^(.*?[аеиоуыэюя])(.*)$");
        //производные формы
        private static Regex DERIVATIONAL = new Regex(".*[^аеиоуыэюя]+[аеиоуыэюя].*ость?$");
        //регулярное выражение для удаления суффиксов ость/ост
        private static Regex DER = new Regex("ость?$");
        //превосходная степень прилагательных
        private static Regex SUPERLATIVE = new Regex("(ейше|ейш)$");
        //удаление окончания "и"
        private static Regex I = new Regex("и$");
        //удаление окончания "ь"
        private static Regex P = new Regex("ь$");
        //удаление суффикса "нн"
        private static Regex NN = new Regex("нн$");

        public static List<List<string>> StemmingAll(List<List<string>> tokenized_sentences)
        {
            for (int i = 0; i < tokenized_sentences.Count; i++)
                tokenized_sentences[i] = Stemming(tokenized_sentences[i]);
            return tokenized_sentences;
        }

        public static List<string> Stemming(List<string> words)
        {
            if (words.Count == 0) return words;

            for(int i = 0; i < words.Count; i++)
                words[i] = TransformingWord(words[i]);

            return words;
        }

        private static string TransformingWord(string word)
        {
            word = word.Replace('ё', 'е');
            MatchCollection m = RVRE.Matches(word);
            if (m.Count > 0)
            {
                Match match = m[0];
                GroupCollection groupCollection = match.Groups;
                string pre = groupCollection[1].ToString();
                string rv = groupCollection[2].ToString();

                MatchCollection temp = PERFECTIVEGROUND.Matches(rv);
                string StringTemp = ReplaceFirst(temp, rv);

                if (StringTemp.Equals(rv))
                {
                    MatchCollection tempRV = REFLEXIVE.Matches(rv);
                    rv = ReplaceFirst(tempRV, rv);
                    temp = ADJECTIVE.Matches(rv);
                    StringTemp = ReplaceFirst(temp, rv);
                    if (!StringTemp.Equals(rv))
                    {
                        rv = StringTemp;
                        tempRV = PARTICIPLE.Matches(rv);
                        rv = ReplaceFirst(tempRV, rv);
                    }
                    else
                    {
                        temp = VERB.Matches(rv);
                        StringTemp = ReplaceFirst(temp, rv);
                        if (StringTemp.Equals(rv))
                        {
                            tempRV = NOUN.Matches(rv);
                            rv = ReplaceFirst(tempRV, rv);
                        }
                        else
                        {
                            rv = StringTemp;
                        }
                    }
                }
                else
                {
                    rv = StringTemp;
                }

                MatchCollection tempRv = I.Matches(rv);
                rv = ReplaceFirst(tempRv, rv);
                if (DERIVATIONAL.Matches(rv).Count > 0)
                {
                    tempRv = DER.Matches(rv);
                    rv = ReplaceFirst(tempRv, rv);
                }

                temp = P.Matches(rv);
                StringTemp = ReplaceFirst(temp, rv);
                if (StringTemp.Equals(rv))
                {
                    tempRv = SUPERLATIVE.Matches(rv);
                    rv = ReplaceFirst(tempRv, rv);
                    tempRv = NN.Matches(rv);
                    rv = ReplaceFirst(tempRv, rv);
                }
                else
                {
                    rv = StringTemp;
                }
                word = pre + rv;
            }

            return word;
        }

        private static string ReplaceFirst(MatchCollection collection, string part)
        {
            string StringTemp = "";
            if (collection.Count == 0)
            {
                return part;
            }
            else
            {
                StringTemp = part;
                for (int i = 0; i < collection.Count; i++)
                {
                    GroupCollection GroupCollection = collection[i].Groups;
                    if (StringTemp.Contains(GroupCollection[i].ToString()))
                    {
                        string deletePart = GroupCollection[i].ToString();
                        StringTemp = StringTemp.Replace(deletePart, "");
                    }
                }
            }
            return StringTemp;
        }
    }
}
