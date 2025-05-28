using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;

namespace SimpleNLPApp
{
    public partial class VisualizationWindow : Window
    {
        private readonly List<TextEvaluationViewModel> texts;
        private readonly List<string> classes;

        public VisualizationWindow(IEnumerable<TextEvaluationViewModel> allTexts, List<string> classLabels)
        {
            InitializeComponent();
            texts = allTexts.Where(t => t.TrueClass != null && t.PredictedClass != null).ToList();
            classes = classLabels;

            DrawConfusionMatrix();
            DrawPrecisionRecall();
        }

        private void DrawConfusionMatrix()
        {
            int n = classes.Count;
            double cellSize = 40;
            double labelOffset = 130;

            var matrix = MetricsVisualizer.GetConfusionMatrix(texts);

            ConfusionCanvas.Children.Clear();
            ConfusionCanvas.Width = labelOffset + cellSize * n + 10;
            ConfusionCanvas.Height = labelOffset + cellSize * n + 50;

            // Подписи классов
            for (int i = 0; i < n; i++)
            {
                var trueLabel = new TextBlock { Text = classes[i], FontSize = 12 };
                Canvas.SetLeft(trueLabel, 5);
                Canvas.SetTop(trueLabel, labelOffset + i * cellSize + 10);
                ConfusionCanvas.Children.Add(trueLabel);

                var predLabel = new TextBlock { Text = classes[i], FontSize = 12 };
                Canvas.SetLeft(predLabel, labelOffset + i * cellSize + 10);
                Canvas.SetTop(predLabel, 30);
                ConfusionCanvas.Children.Add(predLabel);
            }

            // Ячейки
            for (int row = 0; row < n; row++)
            {
                for (int col = 0; col < n; col++)
                {
                    var key = (classes[row], classes[col]);
                    int count = matrix.ContainsKey(key) ? matrix[key] : 0;

                    var rect = new Rectangle
                    {
                        Width = cellSize - 2,
                        Height = cellSize - 2,
                        Fill = count > 0 ? Brushes.LightBlue : Brushes.White,
                        Stroke = Brushes.Black
                    };
                    Canvas.SetLeft(rect, labelOffset + col * cellSize);
                    Canvas.SetTop(rect, labelOffset + row * cellSize);
                    ConfusionCanvas.Children.Add(rect);

                    if (count > 0)
                    {
                        var label = new TextBlock
                        {
                            Text = count.ToString(),
                            FontSize = 14
                        };
                        Canvas.SetLeft(label, labelOffset + col * cellSize + 10);
                        Canvas.SetTop(label, labelOffset + row * cellSize + 10);
                        ConfusionCanvas.Children.Add(label);
                    }
                }
            }

            // Accuracy
            int total = texts.Count;
            int correct = texts.Count(t => t.TrueClass == t.PredictedClass);
            double accuracy = (double)correct / total;

            var accText = new TextBlock
            {
                Text = $"Accuracy: {accuracy:0.00}",
                FontSize = 16,
                FontWeight = FontWeights.SemiBold
            };
            Canvas.SetLeft(accText, ConfusionCanvas.Width - 140);
            Canvas.SetTop(accText, 10);
            ConfusionCanvas.Children.Add(accText);
        }

        private void DrawPrecisionRecall()
        {
            var metrics = MetricsVisualizer.GetPrecisionRecall(texts, classes);

            double barWidth = 30;
            double spacing = 20;
            double maxBarHeight = 150;
            double x = 60;

            PrecisionRecallCanvas.Children.Clear();
            PrecisionRecallCanvas.Height = maxBarHeight + 80;
            PrecisionRecallCanvas.Width = (barWidth * 2 + spacing * 2) * classes.Count + 100;

            // Ось Y
            var yAxis = new Line
            {
                X1 = 40,
                Y1 = 30,
                X2 = 40,
                Y2 = 30 + maxBarHeight,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            PrecisionRecallCanvas.Children.Add(yAxis);

            var topLabel = new TextBlock { Text = "1.0", FontSize = 10 };
            Canvas.SetLeft(topLabel, 5);
            Canvas.SetTop(topLabel, 30);
            PrecisionRecallCanvas.Children.Add(topLabel);

            var bottomLabel = new TextBlock { Text = "0.0", FontSize = 10 };
            Canvas.SetLeft(bottomLabel, 5);
            Canvas.SetTop(bottomLabel, 30 + maxBarHeight - 10);
            PrecisionRecallCanvas.Children.Add(bottomLabel);

            foreach (var cls in classes)
            {
                var (precision, recall) = metrics[cls];

                // Precision
                var precRect = new Rectangle
                {
                    Width = barWidth,
                    Height = precision * maxBarHeight,
                    Fill = Brushes.Green
                };
                Canvas.SetLeft(precRect, x);
                Canvas.SetTop(precRect, 30 + (maxBarHeight - precRect.Height));
                PrecisionRecallCanvas.Children.Add(precRect);

                // Recall
                var recallRect = new Rectangle
                {
                    Width = barWidth,
                    Height = recall * maxBarHeight,
                    Fill = Brushes.Orange
                };
                Canvas.SetLeft(recallRect, x + barWidth + 5);
                Canvas.SetTop(recallRect, 30 + (maxBarHeight - recallRect.Height));
                PrecisionRecallCanvas.Children.Add(recallRect);

                // Label
                var label = new TextBlock
                {
                    Text = cls,
                    FontSize = 12
                };
                Canvas.SetLeft(label, x);
                Canvas.SetTop(label, 30 + maxBarHeight + 5);
                PrecisionRecallCanvas.Children.Add(label);

                x += barWidth * 2 + spacing;
            }

            // Легенда
            AddLegend(PrecisionRecallCanvas, 20, PrecisionRecallCanvas.Height - 40);
        }

        private void AddLegend(Canvas canvas, double startX, double startY)
        {
            AddLegendItem(canvas, startX, startY, Brushes.Green, "Precision");
            AddLegendItem(canvas, startX, startY + 20, Brushes.Orange, "Recall");
        }

        private void AddLegendItem(Canvas canvas, double x, double y, Brush color, string label)
        {
            var rect = new Rectangle
            {
                Width = 16,
                Height = 16,
                Fill = color
            };
            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);
            canvas.Children.Add(rect);

            var text = new TextBlock
            {
                Text = label,
                FontSize = 12
            };
            Canvas.SetLeft(text, x + 20);
            Canvas.SetTop(text, y - 2);
            canvas.Children.Add(text);
        }
    }
}
