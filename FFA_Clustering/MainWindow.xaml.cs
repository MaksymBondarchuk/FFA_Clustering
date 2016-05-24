using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace FFA_Clustering
{
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Private constants
        private const int HalfPointSize = 2;
        private const int IterationDelay = 125;
        private const int ClickDispersion = 15;
        private const int ClickLineStroke = 10;
        #endregion

        #region Private properties
        private Firefly ShitEater { get; } = new Firefly();
        private int ClickedTimes { get; set; }

        private string ClipboardMessage { get; set; }
        private bool IsRunClicked { get; set; }

        private string LabelInfoRequiredPart { get; set; }

        private Algorithm Algorithm { get; }
        private Random Rand { get; } = new Random();

        private List<Color> Clrs { get; } = new List<Color>();

        private SolidColorBrush ClickBrush { get; set; } = new SolidColorBrush(Colors.SlateGray);
        #endregion

        #region Constructor
        public MainWindow()
        {
            this.InitializeComponent();

            //GroupBoxDrawPoints.Height = 0;

            this.Algorithm = new Algorithm
            {
                RangeX = (int)this.CanvasMain.ActualWidth,
                RangeY = (int)this.CanvasMain.ActualHeight,
                Dimension = 2
            };

            Application.Current.Resources["IsMenuItemSaveEnabled"] = false;

            //OpenFile("C:\\Users\\Max\\Downloads\\InitialSet.json");

            for (var i = 0; i < 200; i++)
                this.Clrs.Add(Color.FromRgb((byte)this.Rand.Next(255), (byte)this.Rand.Next(255), (byte)this.Rand.Next(255)));
        }

        private async void WindowLoaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(1000);
            await this.Dispatcher.BeginInvoke((Action)(() => this.TabControlMain.SelectedIndex = 1));

            await this.MouseClick(new Point(100, 100));
            await Task.Delay(1000);

            await this.MouseClick(new Point(200, 200));
            await Task.Delay(1000);

            await this.MouseClick(new Point(300, 300));
            await Task.Delay(1000);
        }
        #endregion

        #region Controller
        private void CheckTextForInt(object sender, TextCompositionEventArgs e)
        {
            int result;
            e.Handled = !int.TryParse(e.Text, out result);
        }
        #endregion

        #region Mouse events
        private async void CanvasMainMouseUp(object sender, MouseButtonEventArgs e)
        {
            await this.MouseClick(Mouse.GetPosition(this.CanvasMain));
        }

        private async Task MouseClick(Point mouseLocation)
        {
            if (this.TabControlMain.SelectedIndex == 0 ||    // Test mode
                this.TextBoxDispersion.Text.Equals(string.Empty) ||
                this.TextBoxPointsPerClick.Text.Equals(string.Empty))
                return;

            var dispersion = Convert.ToInt32(this.TextBoxDispersion.Text);
            var pointsPerClick = Convert.ToInt32(this.TextBoxPointsPerClick.Text);

            #region Test
            var isChecked = this.CheckBoxShitMode.IsChecked;
            if (isChecked != null && (bool)isChecked)
            {
                var k = Convert.ToInt32(this.TextBoxClustersNumber.Text);

                if (k - 1 <= this.ClickedTimes)
                {
                    this.Algorithm.Fireflies.Add(this.ShitEater);
                    this.Algorithm.FillCentroidPoints(this.Algorithm.Fireflies.First());
                    this.Algorithm.Fireflies.First().SumOfSquaredError = this.Algorithm.SumOfSquaredError(this.Algorithm.Fireflies.First());
                    this.Draw();
                    return;
                }

                var cp = new ClusterPoint();
                cp.X.Add(mouseLocation.X);
                cp.X.Add(mouseLocation.Y);
                this.ShitEater.Centroids.Add(cp);
                this.ShitEater.CentroidPoints.Add(new List<int>());

                this.ClickedTimes++;
                return;
            }
            #endregion

            #region Click animation

            var newBrush = new SolidColorBrush(this.ClickBrush.Color);
            var line1 = new Line
            {
                X1 = mouseLocation.X - ClickDispersion,
                Y1 = mouseLocation.Y - ClickDispersion,
                X2 = mouseLocation.X + ClickDispersion,
                Y2 = mouseLocation.Y + ClickDispersion,
                StrokeThickness = ClickLineStroke,
                Stroke = this.ClickBrush
            };
            var line2 = new Line
            {
                X1 = mouseLocation.X - ClickDispersion,
                Y1 = mouseLocation.Y + ClickDispersion,
                X2 = mouseLocation.X + ClickDispersion,
                Y2 = mouseLocation.Y - ClickDispersion,
                StrokeThickness = ClickLineStroke,
                Stroke = this.ClickBrush
            };

            this.CanvasMain.Children.Add(line1);
            this.CanvasMain.Children.Add(line2);

            var t1 = this.ClickFlash(line1);
            var t2 = this.ClickFlash(line2);

            await Task.WhenAll(t1, t2);
            this.CanvasMain.Children.RemoveRange(this.CanvasMain.Children.Count - 2, 2);
            this.ClickBrush = newBrush;
            #endregion

            if (mouseLocation.X < dispersion * .5 || this.CanvasMain.Width - dispersion * .5 < mouseLocation.X ||
                mouseLocation.Y < dispersion * .5 || this.CanvasMain.Height - dispersion * .5 < mouseLocation.Y)
                return;

            #region Draw points
            for (var i = 0; i < pointsPerClick; i++)
            {
                int x, y;
                if (this.RadioButtonDispersionAsSquare.IsChecked != null && this.RadioButtonDispersionAsSquare.IsChecked.Value)
                {
                    x = this.Rand.Next((int)(mouseLocation.X - dispersion * .5), (int)(mouseLocation.X + dispersion * .5));
                    y = this.Rand.Next((int)(mouseLocation.Y - dispersion * .5), (int)(mouseLocation.Y + dispersion * .5));
                }
                else
                {
                    var radius = this.Rand.NextDouble() * dispersion * .5;
                    var angle = this.Rand.NextDouble() * 2 * Math.PI;

                    x = (int)(mouseLocation.X + radius * Math.Cos(angle));
                    y = (int)(mouseLocation.Y + radius * Math.Sin(angle));
                }

                this.CanvasMain.Children.Add(new Rectangle
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
                this.Algorithm.Points.Add(p);
                Application.Current.Resources["IsMenuItemSaveEnabled"] = true;
            }
            #endregion
        }
        #endregion

        #region Button events
        private void ButtonClearClick(object sender, RoutedEventArgs e)
        {
            this.Algorithm.Points.Clear();
            this.CanvasMain.Children.Clear();

            Application.Current.Resources["IsMenuItemSaveEnabled"] = false;
            this.TextBoxSilhouetteMethod.Text = string.Empty;
        }

        private async void ButtonRunClick(object sender, RoutedEventArgs e)
        {
            await this.ButtonRunClickTask();
        }

        private async Task ButtonRunClickTask()
        {
            this.IsRunClicked = true;

            this.Algorithm.RangeX = (int)this.CanvasMain.ActualWidth;
            this.Algorithm.RangeY = (int)this.CanvasMain.ActualHeight;
            this.Algorithm.Dimension = 2;

            var clustersNumber = Convert.ToInt32(this.TextBoxClustersNumber.Text);
            this.Algorithm.Initialization(Convert.ToInt32(this.TextBoxFirefliesNumber.Text), clustersNumber);

            for (var iter = 1; iter <= Algorithm.MaximumGenerations; iter++)
            {
                await this.Algorithm.Iteration(iter);

                //TextBoxSumOfSquaredError.Text = $"{ff.SumOfSquaredError,-18:0.}";
                //TextBoxSilhouetteMethod.Text = $"{this.Algorithm.SilhouetteMethod(ff),-18:0.0000000000}";
                //TextBoxXieBeniIndex.Text = $"{this.Algorithm.XieBeniIndex(ff),-18:0.0000000000}";

                this.Algorithm.UpdatePoints(this.Algorithm.Fireflies.First());
                //CanvasMain.Children.Clear();

                this.Draw();

                if (iter == Algorithm.MaximumGenerations - 1 ||
                    this.Algorithm.MfaCanStop)
                {
                    this.IsRunClicked = false;
                    this.LabelInfo.Content = "MFA finished";
                    await this.CanvasFlash();
                    return;
                }

                this.ProgressBarInfo.Value = iter * 100 / (double)Algorithm.MaximumGenerations;
                this.LabelInfo.Content = $"{this.LabelInfoRequiredPart}Iteration #{iter}";
                await Task.Delay(IterationDelay);
            }
        }

        private async void ButtonKMeansClick(object sender, RoutedEventArgs e)
        {
            await this.ButtonKMeansClickTask(sender);
        }

        private async Task ButtonKMeansClickTask(object sender)
        {
            this.IsRunClicked = false;

            this.Algorithm.RangeX = (int)this.CanvasMain.ActualWidth;
            this.Algorithm.RangeY = (int)this.CanvasMain.ActualHeight;
            this.Algorithm.Dimension = 2;

            var clustersNumber = Convert.ToInt32(this.TextBoxClustersNumber.Text);
            var button = sender as Button;
            if (button != null && ReferenceEquals(button.Content, "K-means"))
                this.Algorithm.InitializationKMeans(clustersNumber);
            else
                this.Algorithm.InitializationKMeansPlusPlus(clustersNumber);

            for (var iter = 0; iter < Algorithm.MaximumGenerations; iter++)
            {
                await this.Algorithm.IterationKMeans();

                //var ff = this.Algorithm.Fireflies.First();
                //this.TextBoxSumOfSquaredError.Text = $"{ff.SumOfSquaredError,-18:0.}";
                //this.TextBoxSilhouetteMethod.Text = $"{this.Algorithm.SilhouetteMethod(ff),-18:0.0000000000}";
                //this.TextBoxXieBeniIndex.Text = $"{this.Algorithm.XieBeniIndex(ff),-18:0.0000000000}";
                //this.Algorithm.UpdatePoints(ff);
                //this.CanvasMain.Children.Clear();
                this.Draw();
                this.LabelInfo.Content = $"{this.LabelInfoRequiredPart}Iteration #{iter}";
                await Task.Delay(IterationDelay);

                if (!this.Algorithm.KMeansCanStop) continue;
                this.LabelInfo.Content = $"{this.LabelInfoRequiredPart}K-means finished";
                await this.CanvasFlash();

                return;
            }
        }

        private void WindowPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) &&
                Keyboard.IsKeyDown(Key.S))
                this.MenuItemSave.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

            if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) &&
                Keyboard.IsKeyDown(Key.O))
                this.MenuItemOpen.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        }

        #endregion

        #region Additional methods
        private void Draw()
        {
            if (this.Algorithm.Fireflies.Count != 0)
            {
                var ff = this.Algorithm.Fireflies.First();
                this.TextBoxSumOfSquaredError.Text = $"{ff.SumOfSquaredError,-18:0.}";
                this.TextBoxSilhouetteMethod.Text = $"{this.Algorithm.SilhouetteMethod(ff),-18:0.0000000000}";
                this.TextBoxXieBeniIndex.Text = $"{this.Algorithm.XieBeniIndex(ff),-18:0.0000000000}";
                this.Algorithm.UpdatePoints(ff);
                this.CanvasMain.Children.Clear();
            }

            foreach (var point in this.Algorithm.Points)
            {
                var pointColor = !this.IsRunClicked && point.BelongsToCentroid != -1 ? this.Clrs[point.BelongsToCentroid] : Colors.Red;
                this.CanvasMain.Children.Add(new Rectangle
                {
                    Stroke = new SolidColorBrush(pointColor),
                    Fill = new SolidColorBrush(pointColor),
                    Width = 2 * HalfPointSize,
                    Height = 2 * HalfPointSize,
                    Margin = new Thickness(point.X[0] - HalfPointSize, point.X[1] - HalfPointSize, 0, 0)
                });
            }

            for (var ffI = 0; ffI < this.Algorithm.Fireflies.Count; ffI++)
            {
                if (!this.IsRunClicked && ffI != 0)
                    return;

                var firefly = this.Algorithm.Fireflies[ffI];
                for (var j = 0; j < firefly.Centroids.Count; j++)
                //foreach (var fireflyPoint in firefly.Centroids)
                {
                    var fireflyPoint = firefly.Centroids[j];
                    //if (IsRunClicked)
                    var sz = this.IsRunClicked && ffI == 0 ? 8 * HalfPointSize : 4 * HalfPointSize;
                    var clr = this.IsRunClicked ? this.Clrs[ffI] : this.Clrs[j];
                    //var sz = 4 * HalfPointSize;
                    this.CanvasMain.Children.Add(new Rectangle
                    {
                        Stroke = new SolidColorBrush(clr),
                        Fill = new SolidColorBrush(clr),
                        Width = sz,
                        Height = sz,
                        Margin = new Thickness(fireflyPoint.X[0] - HalfPointSize, fireflyPoint.X[1] - HalfPointSize, 0, 0)
                    });
                }
            }
        }

        private async Task ClickFlash(Shape line)
        {
            const int AnimationWait = 150;
            var cb = line.Stroke;

            var da = new ColorAnimation
            {
                From = Colors.Transparent,
                To = this.ClickBrush.Color,
                Duration = new Duration(TimeSpan.FromMilliseconds(AnimationWait*2))
            };
            cb.BeginAnimation(SolidColorBrush.ColorProperty, da);
            await Task.Delay(AnimationWait);
            var da1 = new ColorAnimation
            {
                From = this.ClickBrush.Color,
                To = Colors.Transparent,
                Duration = new Duration(TimeSpan.FromMilliseconds(AnimationWait))
            };
            cb.BeginAnimation(SolidColorBrush.ColorProperty, da1);
        }

        private async Task CanvasFlash()
        {
            const int AnimationWait = 150;
            var previousColor = new SolidColorBrush(((SolidColorBrush)this.CanvasMain.Background).Color).Color;
            var cb = this.CanvasMain.Background;
            var convertFromString = ColorConverter.ConvertFromString("#FF007ACC");
            if (convertFromString != null)
            {
                var da = new ColorAnimation
                {
                    From = previousColor,
                    To = (Color)convertFromString,
                    Duration = new Duration(TimeSpan.FromMilliseconds(AnimationWait))
                };
                cb.BeginAnimation(SolidColorBrush.ColorProperty, da);
            }
            await Task.Delay(AnimationWait);
            if (convertFromString != null)
            {
                var da1 = new ColorAnimation
                {
                    From = (Color)convertFromString,
                    To = previousColor,
                    Duration = new Duration(TimeSpan.FromMilliseconds(AnimationWait))
                };
                cb.BeginAnimation(SolidColorBrush.ColorProperty, da1);
            }
        }
        #endregion

        #region JSON
        private void MenuItemSaveClick(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog
            {
                DefaultExt = ".json",
                Filter = "JavaScript Object Notation File (.json)|*.json"
            };

            if (dlg.ShowDialog() == true)
            {
                this.SaveToFile(dlg.FileName);
            }
        }

        private void SaveToFile(string fileName)
        {
            if (fileName.Equals(string.Empty))
                return;

            var obj = new JsonObject
            {
                Points = this.Algorithm.Points,
                Fireflies = this.Algorithm.Fireflies
            };
            var json = new JavaScriptSerializer().Serialize(obj);
            var file = new StreamWriter(fileName);
            file.WriteLine(json);
            file.Close();
        }

        private void MenuItemOpenClick(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                DefaultExt = ".json",
                Filter = "JavaScript Object Notation File (.json)|*.json"
            };

            if (dlg.ShowDialog() == true)
            {
                this.OpenFile(dlg.FileName);
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
            this.ButtonClear.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            this.Algorithm.Points = new List<ClusterPoint>(results.Points);
            this.Algorithm.Fireflies = new List<Firefly>(results.Fireflies);

            var firstFirefly = results.Fireflies.FirstOrDefault();
            this.Algorithm.Dimension = firstFirefly?.Centroids.Count ?? 5;

            this.TextBoxClustersNumber.Text = this.Algorithm.Dimension.ToString();
            this.Draw();
        }
        #endregion

        #region Tests

        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
        private class TestListViewItem
        {
            public string Algorithm { get; set; }
            public string SumOfSquaredError { get; set; }
            public string SilhouetteMethod { get; set; }
            public string XieBeniIndex { get; set; }
        }

        /// <summary>
        /// Launches algorithm for several runs
        /// </summary>
        /// <param name="algorithm">Algorithm name</param>
        /// <returns>Row with results</returns>
        private async Task<TestListViewItem> GetResult(string algorithm)
        {
            Func<Task> action;
            if (algorithm.Equals(Properties.Resources.Kmeans))
                action = async () => { await this.ButtonKMeansClickTask(this.ButtonKmeans); };
            else if (algorithm.Equals(Properties.Resources.KmeansPlusPlus))
                action = async () => { await this.ButtonKMeansClickTask(this.ButtonKmeans); };
            else action = async () => { await this.ButtonRunClickTask(); };

            var sse = 0.0;
            var sm = 0.0;
            var xb = 0.0;
            var runsNumber = Convert.ToInt32(this.TextBoxRunsNumber.Text);
            for (var i = 0; i < runsNumber; i++)
            {
                this.LabelInfoRequiredPart = $"Testing {algorithm}. Test #{i}: ";
                await action();
                sse += Convert.ToDouble(this.TextBoxSumOfSquaredError.Text);
                sm += Convert.ToDouble(this.TextBoxSilhouetteMethod.Text);
                xb += Convert.ToDouble(this.TextBoxXieBeniIndex.Text);
            }

            var sseText = Convert.ToString(sse / runsNumber, CultureInfo.InvariantCulture);
            var smText = Convert.ToString(sm / runsNumber, CultureInfo.InvariantCulture);
            var xbText = Convert.ToString(xb / runsNumber, CultureInfo.InvariantCulture);
            this.ClipboardMessage += $"{algorithm};{sseText};{smText};{xbText}";
            return new TestListViewItem
            {
                Algorithm = algorithm,
                SumOfSquaredError = sseText,
                SilhouetteMethod = smText,
                XieBeniIndex = xbText
            };
        }

        private async void ButtonRunTestsClick(object sender, RoutedEventArgs e)
        {
            this.ClipboardMessage = string.Empty;
            var testResultsWindow = new TestResultsWindow();
            var a = await this.GetResult(Properties.Resources.Kmeans);
            testResultsWindow.ListViewInfoTestResults.Items.Add(a);
            testResultsWindow.ListViewInfoTestResults.Items.Add(await this.GetResult(Properties.Resources.KmeansPlusPlus));
            testResultsWindow.ListViewInfoTestResults.Items.Add(await this.GetResult(Properties.Resources.ModifiedFireflyAlgorithm));
            testResultsWindow.Show();
            this.LabelInfoRequiredPart = string.Empty;
            this.LabelInfo.Content = "Test are finished";
            Clipboard.SetText(this.ClipboardMessage);
        }

        #endregion
    }
}
