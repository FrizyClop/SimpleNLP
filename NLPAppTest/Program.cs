using SimpleNLP;
using SimpleNLP.Classification;
using SimpleNLP.Transformation;
using SimpleNLP.Preprocessing;

List<string> train_texts = new List<string>();
List<string> labels = new List<string>() { "SSD", "SSD", "SSD","SSD", "SSD", "HDD" , "HDD" , "HDD","HDD","HDD" };

List<string> GetTexts(string path_to_folder)
{
    List<string> texts = new List<string>();
    string[] paths = Directory.GetFiles(path_to_folder);
    StreamReader stream_reader;
    foreach (string path in paths)
    {
        stream_reader = new StreamReader(path);
        texts.Add(stream_reader.ReadToEnd());
        stream_reader.Close();
    }
    return texts;
}

train_texts = GetTexts("c:\\C#_programms\\SimpleNLP\\NLPAppTest\\bin\\Debug\\net9.0\\Examples\\Train\\SSD\\");
train_texts.AddRange(GetTexts("c:\\C#_programms\\SimpleNLP\\NLPAppTest\\bin\\Debug\\net9.0\\Examples\\Train\\HDD\\"));

Console.WriteLine("Тексты загружены.");
// Создаём экземпляр класса NLPProcessor
var nlpProcessor = new NLPProcessor(train_texts);
Console.WriteLine("Тексты подготовлены.");
// Создаём экземпляр класса Vectorizer
TfIdfVectorizer vectorizer = new TfIdfVectorizer();
Console.WriteLine("Обучим векторизатор на основе трёх SSD и трёх HDD");
vectorizer.Fit(nlpProcessor.texts);
Console.WriteLine("Векторизатор обучен. Построим векторы 8-ми документов.");

List<double[]> train_vectors = new List<double[]>();
for(int i = 0; i < 4; i++)
{
    train_vectors.Add(vectorizer.Vectorize(nlpProcessor.texts[i]));
}

for (int i = 5; i < 9; i++)
{
    train_vectors.Add(vectorizer.Vectorize(nlpProcessor.texts[i]));
}
List<double[]> test_vectors = new List<double[]>() { vectorizer.Vectorize(nlpProcessor.texts[9]), vectorizer.Vectorize(nlpProcessor.texts[4]) };
Console.WriteLine("Векторы текстов построены.\n");

Console.WriteLine("Начинаем обучение модели Наивного Байеса.");
NaiveBayesClassifier nbc_model = new NaiveBayesClassifier();
nbc_model.Fit(train_vectors, labels);
Console.WriteLine("Обучение модели завершено.");
Console.WriteLine("Проверка предположений модели: сначала модель должна выдать HDD, а потом SSD");
List<string> result = nbc_model.Predict(test_vectors);
Console.WriteLine("Вывод предположения: ");
foreach (var result_item in result)
    Console.Write(result_item + " ");
Console.WriteLine();
StreamReader streamreader = new StreamReader($"Examples\\HDD№05.txt");
string text = streamreader.ReadToEnd();
streamreader.Close();
Console.WriteLine("Проверим модель на ещё одном файле HDD:");
List<string> tokens = Preprocessor.Preprocess(text);
double[] last_vector = vectorizer.Vectorize(tokens);
string res = nbc_model.Predict(last_vector);
Console.WriteLine("Вывод предположения: " + res);


Console.WriteLine("\n\nНачинаем обучение модели Логистической регрессии.");
LogisticRegression lr_model = new LogisticRegression();
lr_model.Train(train_vectors, labels);
Console.WriteLine("Обучение модели завершено.");
Console.WriteLine("Проверка предположений модели: сначала модель должна выдать HDD, а потом SSD");
result = lr_model.Predict(test_vectors);
Console.WriteLine("Вывод предположения: ");
foreach (var result_item in result)
    Console.Write(result_item + " ");
Console.WriteLine();

Console.WriteLine("Проверим модель на ещё одном файле HDD:");

res = lr_model.Predict(last_vector);
Console.WriteLine("Вывод предположения: " + res);

Console.WriteLine("\n");
TestNB();
TestLR();

void TestLR()
{
    Console.WriteLine("\nТест логистической регресии:");
    // предсказание вероятностей
    List<Dictionary<string, double>> probabilities = lr_model.PredictProbabilities(test_vectors);
    Console.WriteLine("\nВероятности для каждого класса:");
    for (int i = 0; i < test_vectors.Count; i++)
    {
        Console.WriteLine($"Вектор {i + 1}:");
        foreach (var p in probabilities[i])
            Console.WriteLine($"  {p.Key}: {p.Value:P2}");
    }
}

void TestNB()
{
    Console.WriteLine("\nТест Наивного Байеса:");
    // предсказание вероятностей
    List<Dictionary<string, double>> probabilities = nbc_model.PredictProbabilities(test_vectors);
    Console.WriteLine("\nВероятности для каждого класса:");
    for (int i = 0; i < test_vectors.Count; i++)
    {
        Console.WriteLine($"Вектор {i + 1}:");
        foreach (var p in probabilities[i])
            Console.WriteLine($"  {p.Key}: {p.Value:P2}");
    }
}