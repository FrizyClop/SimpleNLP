using SimpleNLP;
using SimpleNLP.Classification;
using System.Reflection.Metadata;

List<string> texts = new List<string>();
List<string> labels = new List<string>() { "SSD", "SSD", "SSD", "HDD" , "HDD" , "HDD" };

for(int i = 1; i < 5; i++)
{
    StreamReader stream_reader = new StreamReader($"Examples\\SSD№0{i}.txt");
    texts.Add(stream_reader.ReadToEnd());
    stream_reader.Close();
}

for (int i = 1; i < 5; i++)
{
    StreamReader stream_reader = new StreamReader($"Examples\\HDD№0{i}.txt");
    texts.Add(stream_reader.ReadToEnd());
    stream_reader.Close();
}
Console.WriteLine("Тексты загружены.");
// Создаём экземпляр класса NLPProcessor
var nlpProcessor = new NLPProcessor(texts);
Console.WriteLine("Тексты подготовлены.");
nlpProcessor.Vectorize();
Console.WriteLine("Векторы текстов построены.");

List<double[]> train_vectors = new List<double[]>();
for(int i = 0; i < 3; i++)
{
    train_vectors.Add(nlpProcessor.vectors[i]);
}

for (int i = 4; i < 7; i++)
{
    train_vectors.Add(nlpProcessor.vectors[i]);
}
List<double[]> test_vectors = new List<double[]>() { nlpProcessor.vectors[3], nlpProcessor.vectors[7] };

Console.WriteLine("Начинаем обучение модели.");
NaiveBayesClassifier nbc_model = new NaiveBayesClassifier();
nbc_model.Fit(train_vectors, labels);
Console.WriteLine("Обучение модели завершено.");
Console.WriteLine("Проверка предположений модели: сначала модель должна выдать HDD, а потом SSD");
List<string> result = nbc_model.Predict(test_vectors);
Console.WriteLine("Вывод предположения: ");
foreach (var result_item in result)
    Console.Write(result_item + " ");