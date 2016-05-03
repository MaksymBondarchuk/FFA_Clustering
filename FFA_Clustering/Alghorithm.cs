using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FFA_Clustering
{
    public class Alghorithm
    {
        //public double SilhouetteMethod { get; set; }
        public const int MaximumGenerations = 100;

        private double Delta { get; set; }

        private Random Rand { get; } = new Random();

        public Point RangeX { get; set; }
        public Point RangeY { get; set; }

        public int Dimension { get; set; } = 2;

        public List<ClusterPoint> Points { get; set; } = new List<ClusterPoint>();

        public List<Firefly> Fireflies { get; set; } = new List<Firefly>();


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
                firefly.SumOfSquaredError = SumOfSquaredError(firefly);
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

        public void UpdatePoints(Firefly firefly)
        {
            for (var i = 0; i < firefly.Centroids.Count; i++)
                foreach (var pointIdx in firefly.CentroidPoints[i])
                    Points[pointIdx].BelongsToCentroid = i;
        }

        public double SilhouetteMethod(Firefly firefly)
        {
            if (firefly == null)
                return double.MinValue;

            //UpdatePoints(firefly);

            var s = 0.0;
            for (var i = 0; i < firefly.Centroids.Count; i++)
            {
                foreach (var cpI in firefly.CentroidPoints[i])
                {
                    var a = firefly.CentroidPoints[i].Where(cpJ => cpI != cpJ).Sum(cpJ => Points[cpI].DistTo(Points[cpJ]));

                    var b = 0.0;
                    for (var j = 0; j < firefly.Centroids.Count; j++)
                    {
                        if (i == j) continue;
                        b += firefly.CentroidPoints[j].Sum(cpJ => Points[cpI].DistTo(Points[cpJ]));
                    }

                    s += (b - a) / Math.Max(a, b);
                }
            }
            return s / Points.Count;
        }

        public double SumOfSquaredError(Firefly firefly)
        {
            var sse = 0.0;
            for (var pIndex = 0; pIndex < Points.Count; pIndex++)
            {
                var point = Points[pIndex];
                var distMin = double.MaxValue;
                var indexOfMinCentroid = -1;
                for (var i = 0; i < firefly.Centroids.Count; i++)
                {
                    var r2 = point.X.Select((t, j) => (t - firefly.Centroids[i].X[j]) * (t - firefly.Centroids[i].X[j])).Sum();
                    if (distMin < r2) continue;
                    distMin = r2;
                    indexOfMinCentroid = i;
                }
                sse += distMin;
                firefly.CentroidPoints[indexOfMinCentroid].Add(pIndex);
            }

            return sse;
        }

        public void Itialization(int firefliesNumber, int clustersNumber)
        {
            Fireflies.Clear();
            Delta = Math.Pow(1e-4 / 1.09, 1.0 / MaximumGenerations);

            AddRandomFireflies(firefliesNumber, clustersNumber);
            RankSwarm();
        }

        public async Task Iteration(int number)
        {
            var alphaT = 1e-3 * Math.Pow(Delta, number);

            for (var i = 0; i < Fireflies.Count; i++)
            {
                for (var j = 0; j < Fireflies.Count; j++)
                {
                    if (i == j || Fireflies[i].SumOfSquaredError < Fireflies[j].SumOfSquaredError)
                        continue;

                    var lambdaI = .5 - i * (.5 - 1.9) / (Fireflies.Count - 1);
                    MoveTowards(Fireflies[i], Fireflies[j], alphaT, lambdaI);
                    Fireflies[i].SumOfSquaredError = SumOfSquaredError(Fireflies[i]);
                }
            }

            RankSwarm();
            await Task.Delay(0);
        }

        public Firefly Run(int firefliesNumber, int clustersNumber)
        {
            //AddRandomFireflies(firefliesNumber, clustersNumber);
            ////const int maximumGenerations = 1;
            //var delta = Math.Pow(1e-4 / 1.09, 1.0 / MaximumGenerations);

            //RankSwarm();

            //for (long iter = 0; iter < MaximumGenerations; iter++)
            //{
            //    var alphaT = 1e-3 * Math.Pow(delta, iter);

            //    for (var i = 0; i < Fireflies.Count; i++)
            //    {
            //        for (var j = 0; j < Fireflies.Count; j++)
            //        {
            //            if (i == j || Fireflies[i].SumOfSquaredError < Fireflies[j].SumOfSquaredError)
            //                continue;

            //            var lambdaI = .5 - i * (.5 - 1.9) / (Fireflies.Count - 1);
            //            Fireflies[i].MoveTowards(Fireflies[j], alphaT, lambdaI);
            //            Fireflies[i].SumOfSquaredError = SumOfSquaredError(Fireflies[i]);
            //        }
            //    }

            //    RankSwarm();
            //}

            return Fireflies.FirstOrDefault();
        }

        private void RankSwarm()
        {
            Fireflies.Sort((f1, f2) => f1.SumOfSquaredError.CompareTo(f2.SumOfSquaredError));
        }

        public void MoveTowards(Firefly firefly, Firefly fireflyTo, double alpha, double lambda)
        {
            for (var i = 0; i < firefly.Centroids.Count; i++)
            {
                var r2 = firefly.Centroids[i].X.Select((t, h) =>
                (t - fireflyTo.Centroids[i].X[h]) * (t - fireflyTo.Centroids[i].X[h])).Sum();

                var brightness = .5 / (1 + 1e-4 * r2);
                for (var h = 0; h < firefly.Centroids[i].X.Count; h++)
                {
                    var randomPart = alpha * (Rand.NextDouble() - .5) * MantegnaRandom(lambda);
                    firefly.Centroids[i].X[h] += brightness * (fireflyTo.Centroids[i].X[h] - firefly.Centroids[i].X[h]) + randomPart;
                }

                if (firefly.Centroids[i].X[0] < RangeX.X)
                    firefly.Centroids[i].X[0] = RangeX.X;
                else
                if (firefly.Centroids[i].X[0] > RangeX.Y)
                    firefly.Centroids[i].X[0] = RangeX.Y;
                if (firefly.Centroids[i].X[1] < RangeY.X)
                    firefly.Centroids[i].X[1] = RangeY.X;
                else
                if (firefly.Centroids[i].X[1] > RangeY.Y)
                    firefly.Centroids[i].X[1] = RangeY.Y;

                firefly.SumOfSquaredError = SumOfSquaredError(firefly);
            }
        }


        private double GaussianRandom(double mue, double sigma)
        {
            double x1;
            double w;
            const int randMax = 0x7fff;
            do
            {
                x1 = 2.0 * Rand.Next(randMax) / (randMax + 1) - 1.0;
                var x2 = 2.0 * Rand.Next(randMax) / (randMax + 1) - 1.0;
                w = x1 * x1 + x2 * x2;
            } while (w >= 1.0);
            // ReSharper disable once IdentifierTypo
            var llog = Math.Log(w);
            w = Math.Sqrt((-2.0 * llog) / w);
            var y = x1 * w;
            return mue + sigma * y;
        }

        private double MantegnaRandom(double lambda)
        {
            var sigmaX = SpecialFunction.lgamma(lambda + 1) * Math.Sin(Math.PI * lambda * .5);
            var divider = SpecialFunction.lgamma(lambda * .5) * lambda * Math.Pow(2.0, (lambda - 1) * .5);
            sigmaX /= divider;
            var lambda1 = 1.0 / lambda;
            sigmaX = Math.Pow(Math.Abs(sigmaX), lambda1);
            var x = GaussianRandom(0, sigmaX);
            var y = Math.Abs(GaussianRandom(0, 1.0));
            return x / Math.Pow(y, lambda1);
        }
    }
}
