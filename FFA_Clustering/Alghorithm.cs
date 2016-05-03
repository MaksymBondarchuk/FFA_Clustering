﻿using System;
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

        public bool KMeansCanStop { get; set; }

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

        public void FillCentroidPoints(Firefly firefly)
        {
            foreach (var cp in firefly.CentroidPoints)
                cp.Clear();

            for (var pIndex = 0; pIndex < Points.Count; pIndex++)
            {
                var point = Points[pIndex];
                var distMin = double.MaxValue;
                var indexOfMinCentroid = -1;
                for (var i = 0; i < firefly.Centroids.Count; i++)
                {
                    var r2 = point.Dist2To(firefly.Centroids[i]);
                    if (distMin < r2) continue;
                    distMin = r2;
                    indexOfMinCentroid = i;
                }
                firefly.CentroidPoints[indexOfMinCentroid].Add(pIndex);
            }
        }

        public void Test(int clustersNumber)
        {
            Fireflies.Clear();

            AddRandomFireflies(1, clustersNumber);

            FillCentroidPoints(Fireflies.First());
            UpdatePoints(Fireflies.First());
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

            var s = 0.0;
            for (var i = 0; i < firefly.Centroids.Count; i++)
            {
                foreach (var cpI in firefly.CentroidPoints[i])
                {
                    var a = firefly.CentroidPoints[i].Where(cpJ => cpI != cpJ).Sum(cpJ => Math.Sqrt(Points[cpI].Dist2To(Points[cpJ])));

                    var b = 0.0;
                    for (var j = 0; j < firefly.Centroids.Count; j++)
                    {
                        if (i == j) continue;
                        b += firefly.CentroidPoints[j].Sum(cpJ => Math.Sqrt(Points[cpI].Dist2To(Points[cpJ])));
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

        public void ItializationKMeans(int clustersNumber)
        {
            Fireflies.Clear();
            AddRandomFireflies(1, clustersNumber);
            FillCentroidPoints(Fireflies.First());
        }

        public async Task IterationKMeans()
        {
            var firefly = Fireflies.First();

            var finalPointsNumber = 0;
            for (var i = 0; i < firefly.Centroids.Count; i++)
            {
                var c = firefly.Centroids[i];
                var cp = firefly.CentroidPoints[i];

                if (cp.Count == 0)
                    cp.Add(0);

                var newPoint = new ClusterPoint();
                for (var j = 0; j < c.X.Count; j++)
                    newPoint.X.Add(0);

                foreach (var cpIdx in cp)
                    for (var h = 0; h < Points[cpIdx].X.Count; h++)
                        newPoint.X[h] += Points[cpIdx].X[h];

                for (var h = 0; h < newPoint.X.Count; h++)
                    newPoint.X[h] /= cp.Count;

                if (firefly.Centroids[i].Dist2To(newPoint) < 1e-4)
                    finalPointsNumber++;
                firefly.Centroids[i] = newPoint;
            }

            FillCentroidPoints(firefly);
            UpdatePoints(firefly);

            if (finalPointsNumber == firefly.Centroids.Count)
                KMeansCanStop = true;

            await Task.Delay(0);
        }

        public double XieBeniIndex(Firefly firefly)
        {
            var sum = firefly.Centroids.Select((t, i) => firefly.CentroidPoints[i].Sum(pIdx => Points[pIdx].Dist2To(t))/firefly.CentroidPoints[i].Count).Sum();

            var minDist = double.MaxValue;
            for (var i = 0; i < firefly.Centroids.Count; i++)
                minDist = firefly.Centroids.Where((t, j) => i != j).Select(t => firefly.Centroids[i].Dist2To(t)).Concat(new[] {minDist}).Min();
            return sum / Points.Count / minDist;
        }
    }
}
