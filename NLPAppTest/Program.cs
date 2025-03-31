using SimpleNLP;
using System.Reflection.Metadata;

// Чтение из файла
string file_name = "Examples\\SSD№01.txt";
StreamReader stream_reader = new StreamReader(file_name);
string text = stream_reader.ReadToEnd();
stream_reader.Close();

// Создаём экземпляр класса NLPProcessor
var nlpProcessor = new NLPProcessor(text);

file_name = "Examples\\SSD№02.txt";
stream_reader = new StreamReader(file_name);
text = stream_reader.ReadToEnd();
stream_reader.Close();

nlpProcessor.AddText(text);

file_name = "Examples\\SSD№03.txt";
stream_reader = new StreamReader(file_name);
text = stream_reader.ReadToEnd();
stream_reader.Close();

nlpProcessor.AddText(text);
//foreach (var text_out in nlpProcessor.texts)
//{
//    int j = 1;
//    foreach(var token in text_out)
//    {
//        Console.WriteLine(j + ". " + token);
//        j++;
//    }
//    j = 0;
//    Console.WriteLine();
//    Console.WriteLine();
//}

Console.WriteLine("Словарь: ");
int i = 1;
foreach (var word in nlpProcessor.vocabulary)
{
    Console.WriteLine(i + ". " + word);
    i++;
}

nlpProcessor.Vectorize();
Console.WriteLine("Вектора: ");
int j = 1;
foreach (var vector in nlpProcessor.vectors)
{
    Console.Write(j + ". ");
    foreach (var val in vector)
    {
        Console.Write("[ " + val + " ], ");
    }
    j++;
    Console.WriteLine();
}