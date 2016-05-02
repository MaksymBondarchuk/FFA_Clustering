using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public bool IsInDrawPointsMode { get; set; }
        public Alghorithm Alghorithm { get; set; }
        public Random Rand { get; } = new Random();

        private const int GroupBoxDrawPointsHeight = 190;
        private const int HalfPointSize = 2;
        //private bool IsMenuItemSaveEnabled = false;

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
        }

        private void GroupBoxDrawPoints_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            BottomGrid.Margin = new Thickness(BottomGrid.Margin.Left,
                GroupBoxDrawPoints.Margin.Top + GroupBoxDrawPoints.Height + 10,
                BottomGrid.Margin.Right, BottomGrid.Margin.Bottom);
        }

        private void ButtonMakeClusters_Click(object sender, RoutedEventArgs e)
        {
            CanvasMain.Children.Clear();

            Alghorithm.RangeX = new Point(0, CanvasMain.ActualWidth);
            Alghorithm.RangeY = new Point(0, CanvasMain.ActualHeight);
            Alghorithm.Dimension = 2;

            var clustersNumber = Convert.ToInt32(TextBoxClustersNumber.Text);
            Alghorithm.Test(clustersNumber);

            CanvasMain.Children.Clear();
            Draw();

            //var colors = new List<Color>();
            //for (var i = 0; i < clustersNumber; i++)
            //    colors.Add(Color.FromRgb((byte)Rand.Next(255), (byte)Rand.Next(255), (byte)Rand.Next(255)));

            //foreach (var point in Alghorithm.Points)
            //    CanvasMain.Children.Add(new Rectangle
            //    {
            //        Stroke = new SolidColorBrush(colors[point.BelongsToCentroid]),
            //        Fill = new SolidColorBrush(colors[point.BelongsToCentroid]),
            //        Width = 2 * HalfPointSize,
            //        Height = 2 * HalfPointSize,
            //        Margin = new Thickness(point.X[0] - HalfPointSize, point.X[1] - HalfPointSize, 0, 0)
            //    });

            //for (var i = 0; i < Alghorithm.Fireflies[0].Centroids.Count; i++)
            ////foreach (var fireflyPoint in Alghorithm.Fireflies[0].Centroids)
            //{
            //    var fireflyPoint = Alghorithm.Fireflies[0].Centroids[i];
            //    CanvasMain.Children.Add(new Rectangle
            //    {
            //        Stroke = new SolidColorBrush(colors[i]),
            //        Fill = new SolidColorBrush(colors[i]),
            //        Width = 4 * HalfPointSize,
            //        Height = 4 * HalfPointSize,
            //        Margin = new Thickness(fireflyPoint.X[0] - HalfPointSize, fireflyPoint.X[1] - HalfPointSize, 0, 0)
            //    });
            //}
        }

        private void Draw()
        {
            var colors = new List<Color>();
            for (var i = 0; i < Convert.ToInt32(TextBoxClustersNumber.Text); i++)
                colors.Add(Color.FromRgb((byte)Rand.Next(255), (byte)Rand.Next(255), (byte)Rand.Next(255)));

            foreach (var point in Alghorithm.Points)
            {
                var pointColor = point.BelongsToCentroid != -1 ? colors[point.BelongsToCentroid] : Colors.Red;
                CanvasMain.Children.Add(new Rectangle
                {
                    Stroke = new SolidColorBrush(pointColor),
                    Fill = new SolidColorBrush(pointColor),
                    Width = 2*HalfPointSize,
                    Height = 2*HalfPointSize,
                    Margin = new Thickness(point.X[0] - HalfPointSize, point.X[1] - HalfPointSize, 0, 0)
                });
            }

            if (Alghorithm.Fireflies.Count <= 0) return;
            for (var i = 0; i < Alghorithm.Fireflies[0].Centroids.Count; i++)
            {
                var fireflyPoint = Alghorithm.Fireflies[0].Centroids[i];
                CanvasMain.Children.Add(new Rectangle
                {
                    Stroke = new SolidColorBrush(colors[i]),
                    Fill = new SolidColorBrush(colors[i]),
                    Width = 4 * HalfPointSize,
                    Height = 4 * HalfPointSize,
                    Margin = new Thickness(fireflyPoint.X[0] - HalfPointSize, fireflyPoint.X[1] - HalfPointSize, 0, 0)
                });
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
    }
}
