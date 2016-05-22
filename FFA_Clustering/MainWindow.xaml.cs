﻿using System;
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
        #region Private constants
        private const int GroupBoxDrawPointsHeight = 190;
        private const int HalfPointSize = 2;
        private const int IterationDelay = 125;
        #endregion

        #region Private properties
        private bool IsRunClicked { get; set; }

        private Algorithm Algorithm { get; }
        private Random Rand { get; } = new Random();

        private List<Color> Clrs { get; } = new List<Color>();
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            //GroupBoxDrawPoints.Height = 0;

            this.Algorithm = new Algorithm
            {
                RangeX = (int)CanvasMain.ActualWidth,
                RangeY = (int)CanvasMain.ActualHeight,
                Dimension = 2
            };

            Application.Current.Resources["IsMenuItemSaveEnabled"] = false;

            OpenFile("C:\\Users\\Max\\Downloads\\InitialSet.json");

            for (var i = 0; i < 200; i++)
                Clrs.Add(Color.FromRgb((byte)Rand.Next(255), (byte)Rand.Next(255), (byte)Rand.Next(255)));
        }

        private void CheckTextForInt(object sender, TextCompositionEventArgs e)
        {
            int result;
            e.Handled = !int.TryParse(e.Text, out result);
        }

        private void CanvasMain_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (TabControlMain.SelectedIndex == 0 ||    // Test mode
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
                this.Algorithm.Points.Add(p);
                Application.Current.Resources["IsMenuItemSaveEnabled"] = true;
            }
        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            this.Algorithm.Points.Clear();
            CanvasMain.Children.Clear();

            Application.Current.Resources["IsMenuItemSaveEnabled"] = false;
            TextBoxSilhouetteMethod.Text = string.Empty;
        }

        private void Draw()
        {
            foreach (var point in this.Algorithm.Points)
            {
                var pointColor = !IsRunClicked && point.BelongsToCentroid != -1 ? Clrs[point.BelongsToCentroid] : Colors.Red;
                CanvasMain.Children.Add(new Rectangle
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
                if (!IsRunClicked && ffI != 0)
                    return;

                var firefly = this.Algorithm.Fireflies[ffI];
                for (var j = 0; j < firefly.Centroids.Count; j++)
                //foreach (var fireflyPoint in firefly.Centroids)
                {
                    var fireflyPoint = firefly.Centroids[j];
                    //if (IsRunClicked)
                    var sz = IsRunClicked && ffI == 0 ? 8 * HalfPointSize : 4 * HalfPointSize;
                    var clr = IsRunClicked ? Clrs[ffI] : Clrs[j];
                    //var sz = 4 * HalfPointSize;
                    CanvasMain.Children.Add(new Rectangle
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

        private void MenuItemSave_Click(object sender, RoutedEventArgs e)
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
                Points = this.Algorithm.Points,
                Fireflies = this.Algorithm.Fireflies
            };
            var json = new JavaScriptSerializer().Serialize(obj);
            var file = new StreamWriter(fileName);
            file.WriteLine(json);
            file.Close();
        }

        private void MenuItemOpen_Click(object sender, RoutedEventArgs e)
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
            this.Algorithm.Points = new List<ClusterPoint>(results.Points);
            this.Algorithm.Fireflies = new List<Firefly>(results.Fireflies);

            var firstFirefly = results.Fireflies.FirstOrDefault();
            this.Algorithm.Dimension = firstFirefly?.Centroids.Count ?? 5;

            TextBoxClustersNumber.Text = this.Algorithm.Dimension.ToString();
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
            IsRunClicked = true;

            this.Algorithm.RangeX = (int)CanvasMain.ActualWidth;
            this.Algorithm.RangeY = (int)CanvasMain.ActualHeight;
            this.Algorithm.Dimension = 2;

            var clustersNumber = Convert.ToInt32(TextBoxClustersNumber.Text);
            this.Algorithm.Initialization(Convert.ToInt32(TextBoxFirefliesNumber.Text), clustersNumber);

            for (var iter = 0; iter < Algorithm.MaximumGenerations; iter++)
            {
                await this.Algorithm.Iteration(iter);

                var ff = this.Algorithm.Fireflies.First();
                TextBoxSumOfSquaredError.Text = $"{ff.SumOfSquaredError,-18:0.}";
                TextBoxSilhouetteMethod.Text = $"{this.Algorithm.SilhouetteMethod(ff),-18:0.0000000000}";
                TextBoxXieBeniIndex.Text = $"{this.Algorithm.XieBeniIndex(ff),-18:0.0000000000}";

                this.Algorithm.UpdatePoints(this.Algorithm.Fireflies.First());
                CanvasMain.Children.Clear();
                if (iter == Algorithm.MaximumGenerations - 1)
                {
                    IsRunClicked = false;
                    await CanvasFlash();
                }
                Draw();
                ProgressBarInfo.Value = iter * 100 / (double)Algorithm.MaximumGenerations;
                LabelInfo.Content = $"Iteration #{iter}";
                await Task.Delay(IterationDelay);
            }
        }

        private async void ButtonKMeans_Click(object sender, RoutedEventArgs e)
        {
            IsRunClicked = false;

            this.Algorithm.RangeX = (int)CanvasMain.ActualWidth;
            this.Algorithm.RangeY = (int)CanvasMain.ActualHeight;
            this.Algorithm.Dimension = 2;

            var clustersNumber = Convert.ToInt32(TextBoxClustersNumber.Text);
            this.Algorithm.InitializationKMeans(clustersNumber);

            for (var iter = 0; iter < Algorithm.MaximumGenerations; iter++)
            {
                await this.Algorithm.IterationKMeans();

                var ff = this.Algorithm.Fireflies.First();
                TextBoxSumOfSquaredError.Text = $"{ff.SumOfSquaredError,-18:0.}";
                TextBoxSilhouetteMethod.Text = $"{this.Algorithm.SilhouetteMethod(ff),-18:0.0000000000}";
                TextBoxXieBeniIndex.Text = $"{this.Algorithm.XieBeniIndex(ff),-18:0.0000000000}";
                this.Algorithm.UpdatePoints(ff);
                CanvasMain.Children.Clear();
                Draw();
                LabelInfo.Content = $"Iteration #{iter}";
                await Task.Delay(IterationDelay);

                if (!this.Algorithm.KMeansCanStop) continue;
                LabelInfo.Content = "K-means finished";
                await CanvasFlash();

                return;
            }
        }

        private async Task CanvasFlash()
        {
            const int animationWait = 150;
            var prevColor = new SolidColorBrush(((SolidColorBrush)CanvasMain.Background).Color).Color;
            var cb = CanvasMain.Background;
            var convertFromString = ColorConverter.ConvertFromString("#FF007ACC");
            if (convertFromString != null)
            {
                var da = new ColorAnimation
                {
                    From = prevColor,
                    To = (Color)convertFromString,
                    Duration = new Duration(TimeSpan.FromMilliseconds(animationWait))
                };
                cb.BeginAnimation(SolidColorBrush.ColorProperty, da);
            }
            await Task.Delay(animationWait);
            if (convertFromString != null)
            {
                var da1 = new ColorAnimation
                {
                    From = (Color)convertFromString,
                    To = prevColor,
                    Duration = new Duration(TimeSpan.FromMilliseconds(animationWait))
                };
                cb.BeginAnimation(SolidColorBrush.ColorProperty, da1);
            }
        }
    }
}
