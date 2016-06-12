// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" >
//   Copyright (c) 2016 Maxym Bondarchuk, Yuri Zorin
//   Created as diploma project. Resarch advisor - Yuri Zorin.
//   National Technical University of Ukraine "Kyiv Polytechnic Institute" Kyiv, Ukraine, 2016
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

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
        /// <summary>
        /// Half size of a point to draw on canvas
        /// </summary>
        private const int HalfPointSize = 2;

        /// <summary>
        /// Milliseconds to delay iteration
        /// </summary>
        private const int IterationDelay = 125;

        /// <summary>
        /// Radius of circle to animate click
        /// </summary>
        private const int ClickDispersion = 15;
        #endregion

        #region Properties: private
        /// <summary>
        /// Message copied to clipboard after tests
        /// </summary>
        private string ClipboardMessage { get; set; }

        /// <summary>
        /// Determines is button to run MFA clicked
        /// </summary>
        private bool IsRunClicked { get; set; }

        /// <summary>
        /// PArt to label from tests
        /// </summary>
        private string LabelInfoRequiredPart { get; set; }

        /// <summary>
        /// Class to run algorithms
        /// </summary>
        private Algorithm Algorithm { get; }

        /// <summary>
        /// For random numbers
        /// </summary>
        private Random Rand { get; } = new Random();

        /// <summary>
        /// List of colors to draw clusters
        /// </summary>
        private List<Color> Clrs { get; } = new List<Color>();

        /// <summary>
        /// Brush to draw click animation
        /// </summary>
        // ReSharper disable once PossibleNullReferenceException
        private SolidColorBrush ClickBrush { get; set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF007ACC"));

        /// <summary>
        /// How much times were clicked on canvas to draw points
        /// </summary>
        private int CanvasClicks { get; set; }

        /// <summary>
        /// How much clicks were handled (all points from that clicks are drawn)
        /// </summary>
        private int CanvasClicksHandled { get; set; }

        /// <summary>
        /// Determines whether status bar is violet
        /// </summary>
        private bool IsStatusBarAlreadyViolet { get; set; } = true;

        /// <summary>
        /// Window for tests results
        /// </summary>
        private TestResultsWindow TestResultsWindow { get; set; }

        /// <summary>
        /// Height for 0 tab of left tab control
        /// </summary>
        private int TabControlMainHeight { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
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

            // Fill colors
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

        /// <summary>
        /// Performs initial steps when window is opened
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

            //            OnButtonRunTestsClick(null, null);

            await Task.Delay(0);
        }
        #endregion

        #region Controller
        /// <summary>
        /// Checks inputed value for integer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckTextForInt(object sender, TextCompositionEventArgs e)
        {
            int result;
            e.Handled = !int.TryParse(e.Text, out result);
        }
        #endregion

        #region Mouse events
        /// <summary>
        /// Canvas mouse up event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnCanvasMainMouseUp(object sender, MouseButtonEventArgs e)
        {
            await MouseClick(Mouse.GetPosition(CanvasMain));
        }

        /// <summary>
        /// Task for canvas mouse up event
        /// </summary>
        /// <param name="mouseLocation"></param>
        /// <returns></returns>
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
            if (IsStatusBarAlreadyViolet)
                LabelInfo.Content = "Drawing points";
            else
                await ProgressBarAnimation(false, Properties.Resources.DrawingMessage);
            IsStatusBarAlreadyViolet = true;
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
                IsStatusBarAlreadyViolet = false;
            }

            #endregion
        }
        #endregion

        #region Buttons events
        /// <summary>
        /// Clears configuration
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnButtonClearClick(object sender, RoutedEventArgs e)
        {
            Algorithm.Points.Clear();
            CanvasMain.Children.Clear();

            TextBoxSumOfSquaredError.Text = string.Empty;
            TextBoxSilhouetteMethod.Text = string.Empty;
            TextBoxXieBeniIndex.Text = string.Empty;

            await ProgressBarAnimation(false, Properties.Resources.InitialMessage);
            IsStatusBarAlreadyViolet = true;

            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
            TaskbarItemInfo.ProgressValue = 0;
        }

        /// <summary>
        /// Runs MFA
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnButtonRunClick(object sender, RoutedEventArgs e)
        {
            await ButtonRunClickTask();
        }

        /// <summary>
        /// Task for ButtonRun click event
        /// </summary>
        /// <returns></returns>
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

                if (Algorithm.CanMfaStop)
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

        /// <summary>
        /// Runs k-means/k-means++
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnButtonKMeansClick(object sender, RoutedEventArgs e)
        {
            await ButtonKMeansClickTask(sender);
        }

        /// <summary>
        /// Task for ButtonKMeans and ButtonKmeansPlusPlus click events
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
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
            while (!Algorithm.CanKmeansStop)
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

        /// <summary>
        /// Handles keyboard keys press events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowPreviewKeyDown(object sender, KeyEventArgs e)
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
        /// <summary>
        /// Draws clusters and fireflies (for MFA)
        /// </summary>
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

        /// <summary>
        /// Handles windows close event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowClosed(object sender, EventArgs e)
        {
            TestResultsWindow.Close();
        }

        /// <summary>
        /// Changes MFA fast mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCheckBoxFastMfaModeChecked(object sender, RoutedEventArgs e)
        {
            Algorithm.IsInFastMfaMode = CheckBoxFastMfaMode.IsChecked != null && (bool)CheckBoxFastMfaMode.IsChecked;
        }
        #endregion

        #region JSON
        /// <summary>
        /// "Save" action in menu click event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMenuItemSaveClick(object sender, RoutedEventArgs e)
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

        /// <summary>
        /// Saves configuration to JSON
        /// </summary>
        /// <param name="fileName"></param>
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
                TestRunsNumber = Convert.ToInt32(TextBoxRunsNumber.Text),
                IsInFastMfaMode = CheckBoxFastMfaMode.IsChecked == true,
                IsInSimpleDrawMode = CheckBoxSimpleDrawMode.IsChecked == true
            };
            var json = new JavaScriptSerializer().Serialize(obj);
            var file = new StreamWriter(fileName);
            file.WriteLine(json);
            file.Close();
        }

        /// <summary>
        /// "Open" action in menu click event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMenuItemOpenClick(object sender, RoutedEventArgs e)
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

        /// <summary>
        /// Opens JSON and loads configuration
        /// </summary>
        /// <param name="fileName"></param>
        private async void OpenFile(string fileName)
        {
            try
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

                CheckBoxFastMfaMode.IsChecked = results.IsInFastMfaMode;
                CheckBoxSimpleDrawMode.IsChecked = results.IsInSimpleDrawMode;

                Draw();
                if (Algorithm.Points.Count != 0)
                    await ProgressBarAnimation(true, Properties.Resources.ReadyMessage);
            }
            catch (Exception)
            {
                MessageBox.Show("Cannot load this file");
            }
        }
        #endregion

        #region Tests
        /// <summary>
        /// Class to print data to ListView on TestResultsWindow
        /// </summary>
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
        private class TestListViewItem
        {
            /// <summary>
            /// Name of algorithm
            /// </summary>
            public string Algorithm { get; set; }

            /// <summary>
            /// SSE value
            /// </summary>
            public string SumOfSquaredError { get; set; }

            /// <summary>
            /// Deviation for SSE (deviation from average SSE value in %)
            /// </summary>
            public string Deviation { get; set; }

            /// <summary>
            /// SM value
            /// </summary>
            public string SilhouetteMethod { get; set; }

            /// <summary>
            /// XB value
            /// </summary>
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
            var sses = new List<double>(runsNumber);
            for (var i = 1; i <= runsNumber; i++)
            {
                LabelInfoRequiredPart = $"Testing {algorithm}\t\tTest #{i}\t\t";
                await action();
                sses.Add(Convert.ToDouble(TextBoxSumOfSquaredError.Text));
                sse += Convert.ToDouble(TextBoxSumOfSquaredError.Text);
                sm += Convert.ToDouble(TextBoxSilhouetteMethod.Text);
                xb += Convert.ToDouble(TextBoxXieBeniIndex.Text);
                await Task.Delay(500);
            }
            sse /= runsNumber;

            double sum = 0;
            foreach (var t in sses)
                if (t > sse)
                    sum += 100 * t / sse % 100;
                else
                    sum += 100 - 100 * t / sse % 100;
            var deviation = sum / sses.Count;

            var sseText = $"{Math.Truncate(sse / runsNumber),15}";
            var smText = $"{sm / runsNumber,-15:0.0000000000}";
            var xbText = $"{xb / runsNumber,-15:0.0000000000}";
            ClipboardMessage += $"{algorithm};{sseText};{smText};{xbText}";
            return new TestListViewItem
            {
                Algorithm = algorithm,
                Deviation = $"{Math.Truncate(deviation)}%",
                SumOfSquaredError = sseText,
                SilhouetteMethod = smText,
                XieBeniIndex = xbText
            };
        }

        /// <summary>
        /// Handles click event for button to run tests
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnButtonRunTestsClick(object sender, RoutedEventArgs e)
        {
            ClipboardMessage = string.Empty;
            TestResultsWindow = new TestResultsWindow();
            TestResultsWindow.ListViewInfoTestResults.Items.Clear();
            TestResultsWindow.ListViewInfoTestResults.Items.Add(await GetResult(Properties.Resources.Kmeans));
            Clrs.Clear();
            for (var i = 0; i < 200; i++)
                Clrs.Add(Color.FromRgb((byte)Rand.Next(255), (byte)Rand.Next(255), (byte)Rand.Next(255)));
            TestResultsWindow.ListViewInfoTestResults.Items.Add(await GetResult(Properties.Resources.KmeansPlusPlus));
            //TestResultsWindow.ListViewInfoTestResults.Items.Add(await GetResult(Properties.Resources.ModifiedFireflyAlgorithm));
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
        /// <summary>
        /// Animation for switch between left Tabcontrol tabs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTabControlMainSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var da = new DoubleAnimation
            {
                To = TabControlMain.SelectedIndex == 0 ? TabControlMainHeight : 260,
                Duration = new Duration(TimeSpan.FromMilliseconds(500))
            };
            TabControlMain.BeginAnimation(HeightProperty, da);
        }

        /// <summary>
        /// Animates click on canvas
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Animates algorithm execution end
        /// </summary>
        /// <returns></returns>
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
