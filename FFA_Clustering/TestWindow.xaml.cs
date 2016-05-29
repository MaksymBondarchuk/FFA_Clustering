using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FFA_Clustering
{
    /// <summary>
    /// Interaction logic for TestWindow.xaml
    /// </summary>
    public partial class TestWindow : Window
    {
        public TestWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await WindowFlash();
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private async Task WindowFlash()
        {
            const int animationWait = 500;
            var cb = Background;
            var blue = (Color)ColorConverter.ConvertFromString("#FF007ACC");
            var white = Colors.White;

            var da1 = new ColorAnimation
            {
                From = white,
                To = blue,
                Duration = new Duration(TimeSpan.FromMilliseconds(animationWait))
            };
            cb.BeginAnimation(SolidColorBrush.ColorProperty, da1);
        }
    }
}
