using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace FFA_Clustering
{
    public class Alghorithm
    {
        //public double SilhouetteMethod { get; set; }

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
                    var point = new ClusterPoint { IsCentroid = true };
                    point.X.Add(Rand.Next((int)RangeX.X, (int)RangeX.Y));
                    point.X.Add(Rand.Next((int)RangeY.X, (int)RangeY.Y));
                    firefly.Centroids.Add(point);
                    firefly.CentroidPoints.Add(new List<int>());
                }
                Fireflies.Add(firefly);
            }
        }

        public void Test(int clustersNumber)
        {
            Fireflies.Clear();

            AddRandomFireflies(1, clustersNumber);

            for (var pIndex = 0; pIndex < Points.Count; pIndex++)
            //foreach (var point in Points)
            {
                var point = Points[pIndex];
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
                Fireflies.First().CentroidPoints[indexOfMinCentroid].Add(pIndex);
            }
            //SilhouetteMethod = SilhouetteMethod(Fireflies.FirstOrDefault());
        }

        public double SilhouetteMethod(Firefly firefly)
        {
            if (firefly == null)
                return double.MinValue;

            for (var i = 0; i < firefly.Centroids.Count; i++)
                foreach (var pointIdx in firefly.CentroidPoints[i])
                    Points[pointIdx].BelongsToCentroid = i;

            var s = 0.0;
            foreach (var point in Points)
            {
                var a = 0.0;
                var brothers = 0;
                foreach (var brother in Points)
                {
                    if (brother.BelongsToCentroid != point.BelongsToCentroid) continue;
                    var r = point.X.Select((t, i) => (t * brother.X[i]) * (t * brother.X[i])).Sum();
                    a += Math.Sqrt(r);
                    brothers++;
                }
                a /= brothers;

                var b = double.MaxValue;
                for (var i = 0; i < firefly.Centroids.Count; i++)
                {
                    if (i == point.BelongsToCentroid) continue;
                    var bLocal = 0.0;
                    var neighbours = 0;
                    foreach (var neighbour in Points)
                    {
                        if (neighbour.BelongsToCentroid != i) continue;
                        var r = point.X.Select((t, j) => (t * neighbour.X[j]) * (t * neighbour.X[j])).Sum();
                        bLocal += Math.Sqrt(r);
                        neighbours++;
                    }
                    bLocal /= neighbours;
                    if (bLocal < b)
                        b = bLocal;
                }

                s += (b - a) / Math.Max(a, b);
            }
            return s;
        }

        public double Sse(Firefly firefly)
        {
            if (firefly == null)
                return double.MaxValue;

            var r = 0.0;
            for (var i = 0; i < firefly.Centroids.Count; i++)
            {
                var rLocal = 0.0;
                foreach (var pointIdx in firefly.CentroidPoints[i])
                {
                    var r2Local = Points[pointIdx].X.Select((t, j) =>
                    (t - firefly.Centroids[i].X[j]) * (t - firefly.Centroids[i].X[j])).Sum();
                    rLocal += Math.Sqrt(r2Local);
                }
                if (firefly.CentroidPoints[i].Count != 0)
                    rLocal /= firefly.CentroidPoints[i].Count;
                r += rLocal;
            }

            return r / firefly.Centroids.Count;
        }

        public Firefly Run(int firefliesNumber, int clustersNumber)
        {
            AddRandomFireflies(firefliesNumber, clustersNumber);
            const int maximumGenerations = 1;
            var delta = Math.Pow(1e-4 / 1.09, 1.0 / maximumGenerations);

            var bestEver = Sse(Fireflies.First());
            for (long iter = 0; iter < maximumGenerations; iter++)
            {
                var alphaT = 1e-3 * Math.Pow(delta, iter);

                for (var i = 0; i < Fireflies.Count; i++)
                {
                    for (var j = 0; j < Fireflies.Count; j++)
                    {
                        if (i == j || Fireflies[i].Sse < Fireflies[j].Sse)
                            continue;

                        var lambdaI = .5 - i * (.5 - 1.9) / (Fireflies.Count - 1);
                        Fireflies[i].MoveTowards(Fireflies[j], alphaT, lambdaI);
                        Fireflies[i].Sse = Sse(Fireflies[i]);
                    }
                }

                RankSwarm();
                //var bestIter = Fireflies.Min(ff => ff.Sse);
                //if (bestIter < bestEver)
                //    bestEver = bestIter;
            }

            return Fireflies.FirstOrDefault();
        }

        private void RankSwarm()
        {
            Fireflies.Sort((f1, f2) => f1.Sse.CompareTo(f2.Sse));
        }
    }
}
