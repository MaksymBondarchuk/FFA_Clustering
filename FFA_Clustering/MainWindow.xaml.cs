using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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
using System.Windows.Shell;
using Microsoft.Win32;

namespace FFA_Clustering
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Constants: private
        private const int HalfPointSize = 2;
        private const int IterationDelay = 125;
        private const int ClickDispersion = 15;
        #endregion

        #region Properties: private
        private string ClipboardMessage { get; set; }
        private bool IsRunClicked { get; set; }

        private string LabelInfoRequiredPart { get; set; }

        private Algorithm Algorithm { get; }
        private Random Rand { get; } = new Random();

        private List<Color> Clrs { get; } = new List<Color>();

        // ReSharper disable once PossibleNullReferenceException
        private SolidColorBrush ClickBrush { get; set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF007ACC"));

        private int CanvasClicks { get; set; }

        private int CanvasClicksHandled { get; set; }

        private bool AlreadyViolet { get; set; } = true;

        private TestResultsWindow TestResultsWindow { get; set; }

        private int TabControlMainHeight { get; }
        #endregion

        #region Constructors
        public MainWindow()
        {
            InitializeComponent();

            //GroupBoxDrawPoints.Height = 0;

            Algorithm = new Algorithm
            {
                RangeX = (int)CanvasMain.ActualWidth,
                RangeY = (int)CanvasMain.ActualHeight
            };

            Application.Current.Resources["IsMenuItemSaveEnabled"] = false;
            ProgressBarInfo.Visibility = Visibility.Hidden;

            //OpenFile("C:\\Users\\Max\\Downloads\\InitialSet.json");

            for (var i = 0; i < 200; i++)
            {
                var color = Color.FromRgb((byte)Rand.Next(255), (byte)Rand.Next(255), (byte)Rand.Next(255));

                var alreadyHaveThisColor = false;
                foreach (var clr in Clrs)
                    if (Color.AreClose(color, clr))
                        alreadyHaveThisColor = true;
                if (!alreadyHaveThisColor)
                    Clrs.Add(Color.FromRgb((byte)Rand.Next(255), (byte)Rand.Next(255), (byte)Rand.Next(255)));
            }

            TabControlMainHeight = (int)TabControlMain.Height;
        }

        private async void WindowLoaded(object sender, RoutedEventArgs e)
        {
            //            await Task.Delay(1000);
            //            await Dispatcher.BeginInvoke((Action)(() => TabControlMain.SelectedIndex = 1));
            //            await Task.Delay(1000);

            //#pragma warning disable 4014
            //            MouseClick(new Point(200, 200));
            //            await Task.Delay(1000);

            //            MouseClick(new Point(350, 350));
            //            await Task.Delay(1000);

            //            MouseClick(new Point(500, 200));
            //            await Task.Delay(1000);

            //            MouseClick(new Point(650, 350));
            //            await Task.Delay(1000);

            //            await MouseClick(new Point(800, 200));
            //            await Task.Delay(1000);
            //#pragma warning restore 4014

            //            await Dispatcher.BeginInvoke((Action)(() => TabControlMain.SelectedIndex = 0));
            //            await Task.Delay(1000);

            //            await Dispatcher.BeginInvoke((Action)(() => TabControlTest.SelectedIndex = 1));
            //            await Task.Delay(1000);

            //            ButtonRunTestsClick(null, null);

            await Task.Delay(0);
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
            await MouseClick(Mouse.GetPosition(CanvasMain));
        }

        private async Task MouseClick(Point mouseLocation)
        {
            if (TabControlMain.SelectedIndex == 0 ||
                TextBoxDispersion.Text.Equals(string.Empty) ||
                TextBoxPointsPerClick.Text.Equals(string.Empty))
                return;

            var dispersion = Convert.ToInt32(TextBoxDispersion.Text);
            var pointsPerClick = Convert.ToInt32(TextBoxPointsPerClick.Text);

            #region Click animation

            var newBrush = new SolidColorBrush(ClickBrush.Color);

            var circle = new Ellipse
            {
                Height = ClickDispersion * 2,
                Width = ClickDispersion * 2,
                StrokeThickness = 2,
                Stroke = new SolidColorBrush(Colors.Transparent),
                Fill = ClickBrush,
                Margin = new Thickness(mouseLocation.X - ClickDispersion, mouseLocation.Y - ClickDispersion, 0, 0)
            };

            CanvasMain.Children.Add(circle);
            await ClickFlash(circle);

            CanvasMain.Children.RemoveAt(CanvasMain.Children.Count - 1);
            ClickBrush = newBrush;
            #endregion

            if (mouseLocation.X < dispersion * .5 || CanvasMain.Width - dispersion * .5 < mouseLocation.X ||
                mouseLocation.Y < dispersion * .5 || CanvasMain.Height - dispersion * .5 < mouseLocation.Y)
                return;

            #region Draw points
            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Indeterminate;
            CanvasClicks++;
            if (AlreadyViolet)
                LabelInfo.Content = "Drawing points";
            else
                await ProgressBarAnimation(false, Properties.Resources.DrawingMessage);
            AlreadyViolet = true;
            for (var i = 0; i < pointsPerClick; i++)
            {
                int x, y;
                if (RadioButtonDispersionAsSquare.IsChecked != null && RadioButtonDispersionAsSquare.IsChecked.Value)
                {
                    x = Rand.Next((int)(mouseLocation.X - dispersion * .5), (int)(mouseLocation.X + dispersion * .5));
                    y = Rand.Next((int)(mouseLocation.Y - dispersion * .5), (int)(mouseLocation.Y + dispersion * .5));
                }
                else
                {
                    var radius = Rand.NextDouble() * dispersion * .5;
                    var angle = Rand.NextDouble() * 2 * Math.PI;

                    x = (int)(mouseLocation.X + radius * Math.Cos(angle));
                    y = (int)(mouseLocation.Y + radius * Math.Sin(angle));
                }

                CanvasMain.Children.Add(new Rectangle
                {
                    Stroke = new SolidColorBrush(Colors.Red),
                    Fill = new SolidColorBrush(Colors.Red),
                    Width = 2 * HalfPointSize,
                    Height = 2 * HalfPointSize,
                    Margin = new Thickness(x - HalfPointSize, y - HalfPointSize, 0, 0)
                });

                Algorithm.Points.Add(new ClusterPoint { X = x, Y = y });
                await Task.Delay(1);
            }
            CanvasClicksHandled++;
            if (CanvasClicks == CanvasClicksHandled)
            {
                TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
                await ProgressBarAnimation(true, Properties.Resources.ReadyMessage);
                AlreadyViolet = false;
            }

            #endregion
        }
        #endregion

        #region Button events
        private async void ButtonClearClick(object sender, RoutedEventArgs e)
        {
            Algorithm.Points.Clear();
            CanvasMain.Children.Clear();

            TextBoxSumOfSquaredError.Text = string.Empty;
            TextBoxSilhouetteMethod.Text = string.Empty;
            TextBoxXieBeniIndex.Text = string.Empty;

            await ProgressBarAnimation(false, Properties.Resources.InitialMessage);
            AlreadyViolet = true;
        }

        private async void ButtonRunClick(object sender, RoutedEventArgs e)
        {
            await ButtonRunClickTask();
        }

        private async Task ButtonRunClickTask()
        {
            IsRunClicked = true;

            Algorithm.RangeX = (int)CanvasMain.ActualWidth;
            Algorithm.RangeY = (int)CanvasMain.ActualHeight;

            var clustersNumber = Convert.ToInt32(TextBoxClustersNumber.Text);

            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Indeterminate;
            Algorithm.Initialization(Convert.ToInt32(TextBoxFirefliesNumber.Text), clustersNumber);
            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;

            for (var iter = 1; iter <= Algorithm.MaximumGenerations; iter++)
            {
                await Algorithm.Iteration(iter);

                Algorithm.UpdatePoints(Algorithm.Fireflies.First());

                if (CheckBoxSimpleDrawMode.IsChecked == false)
                    Draw();

                if (Algorithm.MfaCanStop)
                    break;

                var pbValue = iter * 100 / (double)Algorithm.MaximumGenerations;
                ProgressBarInfo.Value = pbValue;
                TaskbarItemInfo.ProgressValue = pbValue * .01;
                LabelInfo.Content = $"{LabelInfoRequiredPart}Iteration #{iter}";
                await Task.Delay(IterationDelay);
            }
            IsRunClicked = false;
            LabelInfo.Content = "MFA finished";
            ProgressBarInfo.Value = 100;
            TaskbarItemInfo.ProgressValue = 1;
            Draw();
            await CanvasFlash();
        }

        private async void ButtonKMeansClick(object sender, RoutedEventArgs e)
        {
            await ButtonKMeansClickTask(sender);
        }

        private async Task ButtonKMeansClickTask(object sender)
        {
            IsRunClicked = false;

            Algorithm.RangeX = (int)CanvasMain.ActualWidth;
            Algorithm.RangeY = (int)CanvasMain.ActualHeight;

            var clustersNumber = Convert.ToInt32(TextBoxClustersNumber.Text);
            var button = sender as Button;
            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Indeterminate;
            if (Equals(button, ButtonKmeans))
                Algorithm.InitializationKMeans(clustersNumber);
            else
                Algorithm.InitializationKMeansPlusPlus(clustersNumber);
            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;

            var iter = 0;
            while (!Algorithm.KMeansCanStop)
            {
                await Algorithm.IterationKMeans();

                if (CheckBoxSimpleDrawMode.IsChecked == false)
                    Draw();
                LabelInfo.Content = $"{LabelInfoRequiredPart}Iteration #{iter}";
                iter++;
                ProgressBarInfo.Value = iter * 5;
                TaskbarItemInfo.ProgressValue = iter * .05;
                await Task.Delay(IterationDelay);
            }

            LabelInfo.Content = Equals(button, ButtonKmeans)
                    ? $"{LabelInfoRequiredPart}K-means finished"
                    : $"{LabelInfoRequiredPart}K-means++ finished";
            ProgressBarInfo.Value = 100;
            TaskbarItemInfo.ProgressValue = 1;
            Draw();
            await CanvasFlash();
        }

        private void WindowPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) &&
                Keyboard.IsKeyDown(Key.S))
                MenuItemSave.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

            if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) &&
                Keyboard.IsKeyDown(Key.O))
                MenuItemOpen.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        }
        #endregion

        #region Additional methods
        private void Draw()
        {
            if (Algorithm.Fireflies.Count != 0)
            {
                var ff = Algorithm.Fireflies.First();
                TextBoxSumOfSquaredError.Text = $"{Math.Truncate(ff.SumOfSquaredError)}";
                TextBoxSilhouetteMethod.Text = $"{Algorithm.SilhouetteMethod(ff),-10:0.00000000}";
                TextBoxXieBeniIndex.Text = $"{Algorithm.XieBeniIndex(ff),-10:0.00000000}";
                TextBoxMovements.Text = Algorithm.MovesOnLastIteration == -1
                    ? string.Empty
                    : Algorithm.MovesOnLastIteration.ToString();
                Algorithm.UpdatePoints(ff);
                CanvasMain.Children.Clear();
            }

            foreach (var point in Algorithm.Points)
            {
                var pointColor = !IsRunClicked && point.BelongsToCentroid != -1 ? Clrs[point.BelongsToCentroid] : Colors.Red;
                CanvasMain.Children.Add(new Rectangle
                {
                    Stroke = new SolidColorBrush(pointColor),
                    Fill = new SolidColorBrush(pointColor),
                    Width = 2 * HalfPointSize,
                    Height = 2 * HalfPointSize,
                    Margin = new Thickness(point.X - HalfPointSize, point.Y - HalfPointSize, 0, 0)
                });
            }

            for (var ffI = 0; ffI < Algorithm.Fireflies.Count; ffI++)
            {
                var offset = (ffI == 0 ? 8 * HalfPointSize : 4 * HalfPointSize) * .5;
                var sz = 2 * offset;

                var firefly = Algorithm.Fireflies[ffI];
                for (var j = 0; j < firefly.Centroids.Count; j++)
                {
                    var fireflyPoint = firefly.Centroids[j];
                    var clr = IsRunClicked ? Clrs[ffI] : Clrs[j];
                    CanvasMain.Children.Add(new Rectangle
                    {
                        Stroke = new SolidColorBrush(clr),
                        Fill = new SolidColorBrush(clr),//new SolidColorBrush(Colors.Black),
                        Width = sz,
                        Height = sz,
                        Margin = new Thickness(fireflyPoint.X - offset, fireflyPoint.Y - offset, 0, 0)
                    });
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            TestResultsWindow.Close();
        }

        private void CheckBoxFastMfaMode_Checked(object sender, RoutedEventArgs e)
        {
            Algorithm.IsInFastMfaMode = CheckBoxFastMfaMode.IsChecked != null && (bool)CheckBoxFastMfaMode.IsChecked;
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
                SaveToFile(dlg.FileName);
            }
        }

        private void SaveToFile(string fileName)
        {
            if (fileName.Equals(string.Empty))
                return;

            var obj = new JsonObject
            {
                Points = Algorithm.Points,
                Fireflies = Algorithm.Fireflies,
                SumOfSquaredError = TextBoxSumOfSquaredError.Text.Equals(string.Empty) ?
                    -1 :
                    Convert.ToDouble(TextBoxSumOfSquaredError.Text),
                SilhouetteMethod = TextBoxSilhouetteMethod.Text.Equals(string.Empty) ?
                    -1 :
                    Convert.ToDouble(TextBoxSilhouetteMethod.Text),
                XieBeniIndex = TextBoxXieBeniIndex.Text.Equals(string.Empty) ?
                    -1 :
                    Convert.ToDouble(TextBoxXieBeniIndex.Text),
                ClustersNumber = Convert.ToInt32(TextBoxClustersNumber.Text),
                TestRunsNumber = Convert.ToInt32(TextBoxRunsNumber.Text)
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
                OpenFile(dlg.FileName);
            }
        }

        private async void OpenFile(string fileName)
        {
            if (fileName.Equals(string.Empty))
                return;

            var file = new StreamReader(fileName);
            var json = file.ReadToEnd();
            var deserializer = new JavaScriptSerializer();
            var results = deserializer.Deserialize<JsonObject>(json);
            file.Close();

            ButtonClear.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

            Algorithm.Points = new List<ClusterPoint>(results.Points);
            Algorithm.Fireflies = new List<Firefly>(results.Fireflies);

            const double precision = .0001;
            if (Math.Abs(results.SumOfSquaredError + 1) > precision)
                TextBoxSumOfSquaredError.Text = results.SumOfSquaredError.ToString(CultureInfo.InvariantCulture);

            if (Math.Abs(results.SilhouetteMethod + 1) > precision)
                TextBoxSilhouetteMethod.Text = results.SilhouetteMethod.ToString(CultureInfo.InvariantCulture);

            if (Math.Abs(results.XieBeniIndex + 1) > precision)
                TextBoxXieBeniIndex.Text = results.XieBeniIndex.ToString(CultureInfo.InvariantCulture);

            TextBoxClustersNumber.Text = results.ClustersNumber.ToString();
            TextBoxRunsNumber.Text = results.TestRunsNumber.ToString();

            Draw();
            if (Algorithm.Points.Count != 0)
                await ProgressBarAnimation(true, Properties.Resources.ReadyMessage);
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
                action = async () => { await ButtonKMeansClickTask(ButtonKmeans); };
            else if (algorithm.Equals(Properties.Resources.KmeansPlusPlus))
                action = async () => { await ButtonKMeansClickTask(ButtonKmeansPlusPlus); };
            else action = async () => { await ButtonRunClickTask(); };

            var sse = 0.0;
            var sm = 0.0;
            var xb = 0.0;
            var runsNumber = Convert.ToInt32(TextBoxRunsNumber.Text);
            for (var i = 1; i <= runsNumber; i++)
            {
                LabelInfoRequiredPart = $"Testing {algorithm}\t\tTest #{i}\t\t";
                await action();
                sse += Convert.ToDouble(TextBoxSumOfSquaredError.Text);
                sm += Convert.ToDouble(TextBoxSilhouetteMethod.Text);
                xb += Convert.ToDouble(TextBoxXieBeniIndex.Text);
                await Task.Delay(500);
            }

            var sseText = $"{Math.Truncate(sse / runsNumber),15}";
            var smText = $"{sm / runsNumber,-15:0.0000000000}";
            var xbText = $"{xb / runsNumber,-15:0.0000000000}";
            ClipboardMessage += $"{algorithm};{sseText};{smText};{xbText}";
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
            ClipboardMessage = string.Empty;
            TestResultsWindow = new TestResultsWindow();
            TestResultsWindow.ListViewInfoTestResults.Items.Clear();
            TestResultsWindow.ListViewInfoTestResults.Items.Add(await GetResult(Properties.Resources.Kmeans));
            Clrs.Clear();
            for (var i = 0; i < 200; i++)
                Clrs.Add(Color.FromRgb((byte)Rand.Next(255), (byte)Rand.Next(255), (byte)Rand.Next(255)));
            TestResultsWindow.ListViewInfoTestResults.Items.Add(await GetResult(Properties.Resources.KmeansPlusPlus));
            TestResultsWindow.ListViewInfoTestResults.Items.Add(await GetResult(Properties.Resources.ModifiedFireflyAlgorithm));
            TestResultsWindow.ShowDialog();
            LabelInfoRequiredPart = string.Empty;
            LabelInfo.Content = "Test are finished";
            Clipboard.SetText(ClipboardMessage);

            //await Task.Delay(2000);
            //var testWindow = new TestWindow();
            //testWindow.Show();
        }
        #endregion

        #region Animation
        private void TabControlMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var da = new DoubleAnimation
            {
                To = TabControlMain.SelectedIndex == 0 ? TabControlMainHeight : 260,
                Duration = new Duration(TimeSpan.FromMilliseconds(500))
            };
            TabControlMain.BeginAnimation(HeightProperty, da);
        }

        private async Task ClickFlash(Shape line)
        {
            const int animationWait = 150;
            var cb = line.Fill;

            var da = new ColorAnimation
            {
                From = Colors.Transparent,
                To = ClickBrush.Color,
                Duration = new Duration(TimeSpan.FromMilliseconds(animationWait * 2))
            };
            cb.BeginAnimation(SolidColorBrush.ColorProperty, da);
            await Task.Delay(animationWait);
            var da1 = new ColorAnimation
            {
                From = ClickBrush.Color,
                To = Colors.Transparent,
                Duration = new Duration(TimeSpan.FromMilliseconds(animationWait))
            };
            cb.BeginAnimation(SolidColorBrush.ColorProperty, da1);
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private async Task CanvasFlash()
        {
            const int animationWait = 150;
            //var previousColor = new SolidColorBrush(((SolidColorBrush)CanvasMain.Background).Color).Color;
            var cb = CanvasMain.Background;
            var convertFromString = (Color)ColorConverter.ConvertFromString("#FF007ACC");
            var da = new ColorAnimation
            {
                To = convertFromString,
                Duration = new Duration(TimeSpan.FromMilliseconds(animationWait))
            };
            cb.BeginAnimation(SolidColorBrush.ColorProperty, da);
            await Task.Delay(animationWait);
            var da1 = new ColorAnimation
            {
                To = Colors.White,
                Duration = new Duration(TimeSpan.FromMilliseconds(animationWait))
            };
            cb.BeginAnimation(SolidColorBrush.ColorProperty, da1);
        }

        /// <summary>
        /// Changes color of status bar
        /// </summary>
        /// <param name="fillOrClear">True to make blue</param>
        /// <param name="message">Text to put on LabelInfo</param>
        /// <returns></returns>
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private async Task ProgressBarAnimation(bool fillOrClear, string message)
        {
            Color newColorBrush;
            Color oldColorBrush;
            var blue = (Color)ColorConverter.ConvertFromString("#FF007ACC");
            var violet = (Color)ColorConverter.ConvertFromString("#FF68217A");

            if (fillOrClear)
            {
                ProgressBarInfo.Visibility = Visibility.Visible;
                Application.Current.Resources["IsMenuItemSaveEnabled"] = CanvasClicks == CanvasClicksHandled;
                LabelInfo.Content = "Ready";

                newColorBrush = blue;
                oldColorBrush = violet;
            }
            else
            {
                Application.Current.Resources["IsMenuItemSaveEnabled"] = false;
                LabelInfo.Content = Properties.Resources.InitialMessage;
                ProgressBarInfo.Visibility = Visibility.Hidden;

                newColorBrush = violet;
                oldColorBrush = blue;
            }

            const int animationWait = 300;
            var ap = StatusBarMain.Background;
            var da = new ColorAnimation
            {
                From = oldColorBrush,
                To = newColorBrush,
                Duration = new Duration(TimeSpan.FromMilliseconds(animationWait))
            };
            ap.BeginAnimation(SolidColorBrush.ColorProperty, da);
            LabelInfo.Content = message;
            await Task.Delay(0);
        }
        #endregion
    }
}
