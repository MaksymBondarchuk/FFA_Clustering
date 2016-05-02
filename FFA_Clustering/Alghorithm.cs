using System;
using System.Collections.Generic;
using System.Windows;

namespace FFA_Clustering
{
    public class Alghorithm
    {
        public Point RangeX { get; set; }
        public Point RangeY { get; set; }

        public int Dimension { get; set; }

        public List<ClusterPoint> Points { get; set; } = new List<ClusterPoint>();

        public List<Firefly> Fireflies { get; set; } = new List<Firefly>();

        private Random Rand { get; } = new Random();

        private void AddRandomFireflies(int firefliesNumber, int clustersNumber)
        {
            for (var iter = 0; iter < firefliesNumber; iter++)
            {
                var firefly = new Firefly();
                for (var i = 0; i < clustersNumber; i++)
                {
                    var point = new ClusterPoint {IsCentroid = true};
                    point.X.Add(Rand.Next((int) RangeX.X, (int) RangeX.Y));
                    point.X.Add(Rand.Next((int) RangeY.X, (int) RangeY.Y));
                    firefly.Centroids.Add(point);
                }
                Fireflies.Add(firefly);
            }
        }

        public void Test(int clustersNumber)
        {
            Fireflies.Clear();

            AddRandomFireflies(1, clustersNumber);

            foreach (var point in Points)
            {
                var distMin = double.MaxValue;
                var indexOfMinCentroid = -1;
                for (var i = 0; i < clustersNumber; i++)
                {
                    var r2 = 0.0;
                    for (var j = 0; j < Dimension; j++)
                        r2 += (point.X[j] - Fireflies[0].Centroids[i].X[j]) *
                              (point.X[j] - Fireflies[0].Centroids[i].X[j]);
                    if (distMin < r2) continue;
                    distMin = r2;
                    indexOfMinCentroid = i;
                }
                point.BelongsToCentroid = indexOfMinCentroid;
            }
        }
    }
}
