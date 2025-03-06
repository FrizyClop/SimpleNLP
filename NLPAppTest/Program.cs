using SimpleNLP;
using SimpleNLP.Preprocessing;

// Создаём экземпляр класса NLPProcessor
var nlpProcessor = new NLPProcessor();

// Чтение из файла
string file_name = "Examples\\SSD№01.txt";
StreamReader stream_reader = new StreamReader(file_name);
string text = stream_reader.ReadToEnd();
stream_reader.Close();

// Анализируем текст
var wordFrequency = nlpProcessor.AnalyzeText(text);

// Выводим результаты
foreach (var word_frequency in wordFrequency)
{
    Console.WriteLine($"{word_frequency.Key}: {word_frequency.Value}");
}