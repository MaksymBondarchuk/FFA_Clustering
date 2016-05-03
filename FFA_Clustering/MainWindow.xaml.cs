using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Web.Script.Serialization;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Microsoft.Win32;

// For button click functions call
namespace System.Windows.Controls
{
    /// <summary>
    /// For allow perform button click 
    /// </summary>
    public static class MyExt
    {
        public static void PerformClick(this Button btn)
        {
            btn.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        }
    }
}

namespace FFA_Clustering
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private const int GroupBoxDrawPointsHeight = 190;
        private const int HalfPointSize = 2;

        public bool IsInDrawPointsMode { get; set; }
        public Alghorithm Alghorithm { get; set; }
        public Random Rand { get; } = new Random();

        private int Shit { get; set; }
        //private bool IsMenuItemSaveEnabled = false;

        private List<Color> Clrs { get; } = new List<Color>();

        public MainWindow()
        {
            InitializeComponent();

            GroupBoxDrawPoints.Height = 0;

            Alghorithm = new Alghorithm
            {
                RangeX = new Point(0, CanvasMain.ActualWidth),
                RangeY = new Point(0, CanvasMain.ActualHeight),
                Dimension = 2
            };

            Application.Current.Resources["IsMenuItemSaveEnabled"] = false;

            OpenFile("C:\\Users\\Max\\Downloads\\InitialSet.json");

            for (var i = 0; i < 20; i++)
                Clrs.Add(Color.FromRgb((byte)Rand.Next(255), (byte)Rand.Next(255), (byte)Rand.Next(255)));
        }

        private void CheckTextForInt(object sender, TextCompositionEventArgs e)
        {
            int result;
            e.Handled = !int.TryParse(e.Text, out result);
        }

        private void ButtonDrawPoints_Click(object sender, RoutedEventArgs e)
        {
            var da = new DoubleAnimation
            {
                From = IsInDrawPointsMode ? GroupBoxDrawPointsHeight : 0,
                To = IsInDrawPointsMode ? 0 : GroupBoxDrawPointsHeight,
                Duration = new Duration(TimeSpan.FromMilliseconds(250))
            };
            GroupBoxDrawPoints.BeginAnimation(HeightProperty, da);

            IsInDrawPointsMode = !IsInDrawPointsMode;

            BottomGrid.IsEnabled = !IsInDrawPointsMode;

            ButtonDrawPoints.Content = IsInDrawPointsMode ?
                Properties.Resources.ButtonDrawPointsDrawModeCaption :
                Properties.Resources.ButtonDrawPointsDefaultModeCaption;
        }

        private void CanvasMain_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!IsInDrawPointsMode)
            {
                var p = Mouse.GetPosition(CanvasMain);
                Shit++;
                if (Shit > 5)
                    return;

                if (Shit == 1)
                {
                    var firefly = new Firefly();
                    var point = new ClusterPoint {IsCentroid = true};
                    point.X.Add(p.X);
                    point.X.Add(p.Y);
                    firefly.Centroids.Add(point);
                    firefly.CentroidPoints.Add(new List<int>());
                    Alghorithm.Fireflies.Add(firefly);
                    return;
                }

                var ff = Alghorithm.Fireflies.First();

                var pnt = new ClusterPoint {IsCentroid = true};
                pnt.X.Add(p.X);
                pnt.X.Add(p.Y);
                ff.Centroids.Add(pnt);
                ff.CentroidPoints.Add(new List<int>());

                if (Shit == 5)
                {
                    ff.SumOfSquaredError = Alghorithm.SumOfSquaredError(ff);
                    CanvasMain.Children.Clear();
                    Draw();

                    TextBoxSilhouetteMethod.Text =
                        Alghorithm.SilhouetteMethod(Alghorithm.Fireflies.FirstOrDefault()).
                            ToString(CultureInfo.InvariantCulture);
                    TextBoxSumOfSquaredError.Text =
                        Alghorithm.Fireflies.First().SumOfSquaredError.ToString(CultureInfo.InvariantCulture);
                }
            }

            if (!IsInDrawPointsMode ||
                TextBoxDispersion.Text.Equals(string.Empty) ||
                TextBoxPointsPerClick.Text.Equals(string.Empty))
                return;

            var mousePos = Mouse.GetPosition(CanvasMain);
            var dispersion = Convert.ToInt32(TextBoxDispersion.Text);
            var pointsPerClick = Convert.ToInt32(TextBoxPointsPerClick.Text);

            if (mousePos.X < dispersion * .5 || CanvasMain.Width - dispersion * .5 < mousePos.X ||
                mousePos.Y < dispersion * .5 || CanvasMain.Height - dispersion * .5 < mousePos.Y)
                return;

            for (var i = 0; i < pointsPerClick; i++)
            {
                int x, y;
                if (RadioButtonDispersionAsSquare.IsChecked != null &&
                    RadioButtonDispersionAsSquare.IsChecked.Value)
                {
                    x = Rand.Next((int)(mousePos.X - dispersion * .5), (int)(mousePos.X + dispersion * .5));
                    y = Rand.Next((int)(mousePos.Y - dispersion * .5), (int)(mousePos.Y + dispersion * .5));
                }
                else
                {
                    var radius = Rand.NextDouble() * dispersion * .5;
                    var angle = Rand.NextDouble() * 2 * Math.PI;

                    x = (int)(mousePos.X + radius * Math.Cos(angle));
                    y = (int)(mousePos.Y + radius * Math.Sin(angle));
                }

                CanvasMain.Children.Add(new Rectangle
                {
                    Stroke = new SolidColorBrush(Colors.Red),
                    Fill = new SolidColorBrush(Colors.Red),
                    Width = 2 * HalfPointSize,
                    Height = 2 * HalfPointSize,
                    Margin = new Thickness(x - HalfPointSize, y - HalfPointSize, 0, 0)
                });

                var p = new ClusterPoint();
                p.X.Add(x);
                p.X.Add(y);
                Alghorithm.Points.Add(p);
                Application.Current.Resources["IsMenuItemSaveEnabled"] = true;
            }
        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            Alghorithm.Points.Clear();
            CanvasMain.Children.Clear();

            Application.Current.Resources["IsMenuItemSaveEnabled"] = false;
            TextBoxSilhouetteMethod.Text = string.Empty;
        }

        private void GroupBoxDrawPoints_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            BottomGrid.Margin = new Thickness(BottomGrid.Margin.Left,
                GroupBoxDrawPoints.Margin.Top + GroupBoxDrawPoints.Height + 10,
                BottomGrid.Margin.Right, BottomGrid.Margin.Bottom);
        }

        private void ButtonMakeClusters_Click(object sender, RoutedEventArgs e)
        {
            Alghorithm.RangeX = new Point(0, CanvasMain.ActualWidth);
            Alghorithm.RangeY = new Point(0, CanvasMain.ActualHeight);
            Alghorithm.Dimension = 2;

            var clustersNumber = Convert.ToInt32(TextBoxClustersNumber.Text);
            Alghorithm.Test(clustersNumber);

            CanvasMain.Children.Clear();
            Draw();

            TextBoxSilhouetteMethod.Text =
                Alghorithm.SilhouetteMethod(Alghorithm.Fireflies.FirstOrDefault()).
                ToString(CultureInfo.InvariantCulture);
            TextBoxSumOfSquaredError.Text =
                    Alghorithm.Fireflies.First().SumOfSquaredError.ToString(CultureInfo.InvariantCulture);
        }

        private void Draw()
        {
            foreach (var point in Alghorithm.Points)
            {
                var pointColor = point.BelongsToCentroid != -1 ? Clrs[point.BelongsToCentroid] : Colors.Red;
                CanvasMain.Children.Add(new Rectangle
                {
                    Stroke = new SolidColorBrush(pointColor),
                    Fill = new SolidColorBrush(pointColor),
                    Width = 2 * HalfPointSize,
                    Height = 2 * HalfPointSize,
                    Margin = new Thickness(point.X[0] - HalfPointSize, point.X[1] - HalfPointSize, 0, 0)
                });
            }

            //colors.Clear();
            //for (var i = 0; i < Alghorithm.Fireflies.Count; i++)
            //    colors.Add(Color.FromRgb((byte)Rand.Next(255), (byte)Rand.Next(255), (byte)Rand.Next(255)));
            for (var ffI = 0; ffI < Alghorithm.Fireflies.Count; ffI++)
            {
                var firefly = Alghorithm.Fireflies[ffI];
                for (var j = 0; j < firefly.Centroids.Count; j++)
                //foreach (var fireflyPoint in firefly.Centroids)
                {
                    var fireflyPoint = firefly.Centroids[j];
                    //var sz = ffI == 0 ? 8*HalfPointSize : 4*HalfPointSize;
                    var sz = 4 * HalfPointSize;
                    CanvasMain.Children.Add(new Rectangle
                    {
                        Stroke = new SolidColorBrush(Clrs[j]),
                        Fill = new SolidColorBrush(Clrs[j]),
                        Width = sz,
                        Height = sz,
                        Margin = new Thickness(fireflyPoint.X[0] - HalfPointSize, fireflyPoint.X[1] - HalfPointSize, 0, 0)
                    });
                }
            }
        }

        private void MenuItemSave_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog()
            {
                DefaultExt = ".json",
                Filter = "JavaScript Object Notation File (.json)|*.json"
            };

            if (dlg.ShowDialog() == true)
            {
                SaveToFile(dlg.FileName);
            }
        }

        private void SaveToFile(string fileName)
        {
            if (fileName.Equals(string.Empty))
                return;

            var obj = new JsonObject
            {
                IsClustered = Alghorithm.Fireflies.Count != 0,
                Points = Alghorithm.Points,
                Fireflies = Alghorithm.Fireflies
            };
            var json = new JavaScriptSerializer().Serialize(obj);
            var file = new StreamWriter(fileName);
            file.WriteLine(json);
            file.Close();
        }

        private void MenuItemOpen_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog()
            {
                DefaultExt = ".json",
                Filter = "JavaScript Object Notation File (.json)|*.json"
            };

            if (dlg.ShowDialog() == true)
            {
                OpenFile(dlg.FileName);
            }
        }

        private void OpenFile(string fileName)
        {
            if (fileName.Equals(string.Empty))
                return;

            var file = new StreamReader(fileName);
            var json = file.ReadToEnd();
            var deserializer = new JavaScriptSerializer();
            var results = deserializer.Deserialize<JsonObject>(json);
            file.Close();

            ButtonClear.PerformClick();
            Alghorithm.Points = new List<ClusterPoint>(results.Points);
            Alghorithm.Fireflies = new List<Firefly>(results.Fireflies);

            var firstFirefly = results.Fireflies.FirstOrDefault();
            Alghorithm.Dimension = firstFirefly?.Centroids.Count ?? 5;

            TextBoxClustersNumber.Text = Alghorithm.Dimension.ToString();
            Draw();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) &&
                Keyboard.IsKeyDown(Key.S))
                MenuItemSave.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

            if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) &&
                Keyboard.IsKeyDown(Key.O))
                MenuItemOpen.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        }

        private async void ButtonRun_Click(object sender, RoutedEventArgs e)
        {
            Alghorithm.RangeX = new Point(0, CanvasMain.ActualWidth);
            Alghorithm.RangeY = new Point(0, CanvasMain.ActualHeight);
            Alghorithm.Dimension = 2;

            var clustersNumber = Convert.ToInt32(TextBoxClustersNumber.Text);
            Alghorithm.Itialization(10, clustersNumber);

            //var firefly = Alghorithm.Run(10, clustersNumber);

            for (var iter = 0; iter < Alghorithm.MaximumGenerations; iter++)
            {
                //await Task.Run(Alghorithm.Iteration(iter));
                await Alghorithm.Iteration(iter);
                TextBoxSilhouetteMethod.Text =
                    Alghorithm.SilhouetteMethod(Alghorithm.Fireflies.First()).ToString(CultureInfo.InvariantCulture);
                TextBoxSumOfSquaredError.Text =
                    Alghorithm.Fireflies.First().SumOfSquaredError.ToString(CultureInfo.InvariantCulture);
                Alghorithm.UpdatePoints(Alghorithm.Fireflies.First());
                CanvasMain.Children.Clear();
                Draw();
                ProgressBarInfo.Value = (int)(iter * 100 / Alghorithm.MaximumGenerations);
                await Task.Delay(500);
                LabelInfo.Content = $"Iteration #{iter}";

                //if (iter == 30)
                //{
                //    var x = 0;
                //}
            }
        }

        private async void ButtonKMeans_Click(object sender, RoutedEventArgs e)
        {
            Alghorithm.RangeX = new Point(0, CanvasMain.ActualWidth);
            Alghorithm.RangeY = new Point(0, CanvasMain.ActualHeight);
            Alghorithm.Dimension = 2;

            var clustersNumber = Convert.ToInt32(TextBoxClustersNumber.Text);
            Alghorithm.ItializationKMeans(clustersNumber);

            for (var iter = 0; iter < Alghorithm.MaximumGenerations; iter++)
            {
                await Alghorithm.IterationKMeans();

                var ff = Alghorithm.Fireflies.First();
                TextBoxSilhouetteMethod.Text =
                    Alghorithm.SilhouetteMethod(ff).ToString(CultureInfo.InvariantCulture);
                TextBoxSumOfSquaredError.Text =
                    ff.SumOfSquaredError.ToString(CultureInfo.InvariantCulture);
                Alghorithm.UpdatePoints(ff);
                CanvasMain.Children.Clear();
                Draw();
                await Task.Delay(500);
                LabelInfo.Content = $"Iteration #{iter}";

                if (!Alghorithm.KMeansCanStop) continue;
                LabelInfo.Content = "K-means finished";
                return;
            }
        }
    }
}
