using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FFA_Clustering
{
    public class Algorithm
    {
        #region Public constants
        public const int MaximumGenerations = 50;
        #endregion

        #region Private properties
        private double Delta { get; set; }

        private Random Rand { get; } = new Random();
        #endregion

        #region Public properties
        public bool MfaCanStop { get; private set; }
        public int RangeX { private get; set; }
        public int RangeY { private get; set; }

        public List<ClusterPoint> Points { get; set; } = new List<ClusterPoint>();

        public List<Firefly> Fireflies { get; set; } = new List<Firefly>();

        public bool KMeansCanStop { get; private set; }
        #endregion

        #region Additional 
        private void AddRandomFireflies(int firefliesNumber, int clustersNumber)
        {
            for (var iter = 0; iter < firefliesNumber; iter++)
            {
                var firefly = new Firefly();
                for (var i = 0; i < clustersNumber; i++)
                {
                    firefly.Centroids.Add(new ClusterPoint
                    {
                        X = Rand.Next(RangeX),
                        Y = Rand.Next(RangeY)
                    });
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

        public void UpdatePoints(Firefly firefly)
        {
            for (var i = 0; i < firefly.Centroids.Count; i++)
                foreach (var pointIdx in firefly.CentroidPoints[i]) Points[pointIdx].BelongsToCentroid = i;
        }
        #endregion

        #region Clustering validation criteria
        public double SumOfSquaredError(Firefly firefly)
        {
            foreach (var t in firefly.CentroidPoints)
                t.Clear();

            var sse = 0.0;
            for (var pIndex = 0; pIndex < Points.Count; pIndex++)
            {
                var point = Points[pIndex];
                var distMin = double.MaxValue;
                var indexOfMinCentroid = -1;
                for (var i = 0; i < firefly.Centroids.Count; i++)
                {
                    var r2 = point.Dist2To(firefly.Centroids[i]);
                    if (distMin <= r2) continue;
                    distMin = r2;
                    indexOfMinCentroid = i;
                }
                sse += distMin;
                firefly.CentroidPoints[indexOfMinCentroid].Add(pIndex);
            }

            return sse;
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
                    var a = firefly.CentroidPoints[i].Where(cpJ => cpI != cpJ).Sum(cpJ =>
                        Math.Sqrt(Points[cpI].Dist2To(Points[cpJ])));

                    var b = 0.0;
                    for (var j = 0; j < firefly.Centroids.Count; j++)
                    {
                        if (i == j) continue;
                        b += firefly.CentroidPoints[j].Sum(cpJ =>
                            Math.Sqrt(Points[cpI].Dist2To(Points[cpJ])));
                    }

                    s += (b - a) / Math.Max(a, b);
                }
            }
            return s / Points.Count;
        }

        public double XieBeniIndex(Firefly firefly)
        {
            var sum = firefly.Centroids.Select((t, i) => firefly.CentroidPoints[i].Sum(pIdx =>
                Points[pIdx].Dist2To(t)) / firefly.CentroidPoints[i].Count).Sum();

            var minDist = double.MaxValue;
            for (var i = 0; i < firefly.Centroids.Count; i++)
                minDist = firefly.Centroids.Where((t, j) => i != j).Select(t =>
                    firefly.Centroids[i].Dist2To(t)).Concat(new[] { minDist }).Min();
            var res = sum / Points.Count / minDist;
            return double.IsNaN(res) ? -1 : res;
        }
        #endregion

        #region MFA
        public void Initialization(int firefliesNumber, int clustersNumber)
        {
            Fireflies.Clear();
            Delta = Math.Pow(1e-4 / 1.09, 1.0 / MaximumGenerations);

            AddRandomFireflies(firefliesNumber, clustersNumber);
            RankSwarm();
        }

        public async Task Iteration(int number)
        {
            MfaCanStop = true;
            //var bestSse = this.Fireflies.First().SumOfSquaredError;
            var alphaT = 1e-3 * Math.Pow(Delta, number);

            for (var i = 0; i < Fireflies.Count; i++)
            {
                var lambdaI = .5 - i * (.5 - 1.9) / (Fireflies.Count - 1);
                for (var j = 0; j < Fireflies.Count; j++)
                {
                    if (i == j || Fireflies[i].SumOfSquaredError < Fireflies[j].SumOfSquaredError)
                        continue;

                    MoveTowards(Fireflies[i], Fireflies[j], alphaT, lambdaI);
                    MfaCanStop = false;
                }
            }

            RankSwarm();

            //if (Math.Abs(this.Fireflies.First().SumOfSquaredError - bestSse) <= 1)
            //    this.MfaCanStop = true;

            await Task.Delay(0);
        }

        private void RankSwarm()
        {
            Fireflies.Sort((f1, f2) => f1.SumOfSquaredError.CompareTo(f2.SumOfSquaredError));
        }

        private void MoveTowards(Firefly firefly, Firefly fireflyTo, double alpha, double lambda)
        {
            for (var i = 0; i < firefly.Centroids.Count; i++)
            {
                var r2 = firefly.Centroids[i].Dist2To(fireflyTo.Centroids[i]);
                //var r2 = firefly.Centroids[i].P.Select((t, h) =>
                //(t - fireflyTo.Centroids[i].P[h]) * (t - fireflyTo.Centroids[i].P[h])).Sum();

                var brightness = .5 / (1 + 1e-4 * r2);
                var randomPartX = alpha * (Rand.NextDouble() - .5) * MantegnaRandom(lambda);
                var randomPartY = alpha * (Rand.NextDouble() - .5) * MantegnaRandom(lambda);
                firefly.Centroids[i].X += brightness * (fireflyTo.Centroids[i].X - firefly.Centroids[i].X) + randomPartX;
                firefly.Centroids[i].Y += brightness * (fireflyTo.Centroids[i].Y - firefly.Centroids[i].Y) + randomPartY;

                if (firefly.Centroids[i].X < 0 || RangeX <= firefly.Centroids[i].X ||
                        double.IsNaN(firefly.Centroids[i].X))
                    firefly.Centroids[i].X = Rand.Next(RangeX);
                if (firefly.Centroids[i].Y < 0 || RangeY <= firefly.Centroids[i].Y ||
                        double.IsNaN(firefly.Centroids[i].Y))
                    firefly.Centroids[i].Y = Rand.Next(RangeY);

                //for (var h = 0; h < firefly.Centroids[i].P.Count; h++)
                //{
                //    var randomPart = alpha * (Rand.NextDouble() - .5) * MantegnaRandom(lambda);
                //    firefly.Centroids[i].P[h] += brightness * (fireflyTo.Centroids[i].P[h] - firefly.Centroids[i].P[h]) + randomPart;

                //    if (firefly.Centroids[i].P[h] < 0 || RangeX < firefly.Centroids[i].P[h] ||
                //        double.IsNaN(firefly.Centroids[i].P[h]))
                //        firefly.Centroids[i].P[h] = Rand.Next(RangeX);
                //}

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
        #endregion

        #region K-means
        public void InitializationKMeans(int clustersNumber)
        {
            KMeansCanStop = false;
            Fireflies.Clear();
            AddRandomFireflies(1, clustersNumber);
            FillCentroidPoints(Fireflies.First());
        }

        public void InitializationKMeansPlusPlus(int clustersNumber)
        {
            KMeansCanStop = false;
            Fireflies.Clear();

            var firefly = new Firefly();
            var randomPoint = Points[Rand.Next(Points.Count)];
            firefly.Centroids.Add(new ClusterPoint { X = randomPoint.X, Y = randomPoint.Y });
            firefly.CentroidPoints.Add(new List<int>());

            for (var i = 1; i < clustersNumber; i++)
            {
                var sse = 0.0;
                foreach (var point in Points)
                {
                    var distMin = double.MaxValue;
                    foreach (var c in firefly.Centroids)
                    {
                        var r2 = point.Dist2To(c);
                        if (distMin <= r2) continue;
                        distMin = r2;
                    }
                    sse += distMin;
                }

                var rNeeded = Rand.NextDouble() * sse;

                sse = 0.0;
                foreach (var point in Points)
                {
                    var distMin = double.MaxValue;
                    foreach (var c in firefly.Centroids)
                    {
                        var r2 = point.Dist2To(c);
                        if (distMin <= r2) continue;
                        distMin = r2;
                    }

                    if (sse <= rNeeded && rNeeded <= sse + distMin)
                    {
                        firefly.Centroids.Add(new ClusterPoint { X = point.X, Y = point.Y });
                        firefly.CentroidPoints.Add(new List<int>());
                        break;
                    }

                    sse += distMin;
                }
            }
            firefly.SumOfSquaredError = SumOfSquaredError(firefly);
            Fireflies.Add(firefly);

            FillCentroidPoints(Fireflies.First());
        }

        public async Task IterationKMeans()
        {
            var firefly = Fireflies.First();

            var finalPointsNumber = 0;
            for (var i = 0; i < firefly.Centroids.Count; i++)
            {
                var cp = firefly.CentroidPoints[i];

                if (cp.Count == 0)
                    cp.Add(0);

                var newPoint = new ClusterPoint { X = 0, Y = 0 };

                foreach (var cpIdx in cp)
                {
                    newPoint.X += Points[cpIdx].X;
                    newPoint.Y += Points[cpIdx].Y;
                }
                newPoint.X /= cp.Count;
                newPoint.Y /= cp.Count;

                if (firefly.Centroids[i].Dist2To(newPoint) < 1e-4)
                    finalPointsNumber++;
                firefly.Centroids[i] = newPoint;
            }

            FillCentroidPoints(firefly);
            UpdatePoints(firefly);

            if (finalPointsNumber == firefly.Centroids.Count) KMeansCanStop = true;

            await Task.Delay(0);
        }
        #endregion
    }
}
