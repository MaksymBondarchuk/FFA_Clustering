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

        public int Dimension { get; set; } = 2;

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
                    var point = new ClusterPoint();
                    point.X.Add(this.Rand.Next(this.RangeX));
                    point.X.Add(this.Rand.Next(this.RangeY));
                    firefly.Centroids.Add(point);
                    firefly.CentroidPoints.Add(new List<int>());
                }
                firefly.SumOfSquaredError = this.SumOfSquaredError(firefly);
                this.Fireflies.Add(firefly);
            }
        }

        public void FillCentroidPoints(Firefly firefly)
        {
            foreach (var cp in firefly.CentroidPoints)
                cp.Clear();

            for (var pIndex = 0; pIndex < this.Points.Count; pIndex++)
            {
                var point = this.Points[pIndex];
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
                foreach (var pointIdx in firefly.CentroidPoints[i]) this.Points[pointIdx].BelongsToCentroid = i;
        }
        #endregion

        #region Clustering validation criteria
        public double SumOfSquaredError(Firefly firefly)
        {
            foreach (var t in firefly.CentroidPoints)
                t.Clear();

            var sse = 0.0;
            for (var pIndex = 0; pIndex < this.Points.Count; pIndex++)
            {
                var point = this.Points[pIndex];
                var distMin = double.MaxValue;
                var indexOfMinCentroid = -1;
                for (var i = 0; i < firefly.Centroids.Count; i++)
                {
                    var r2 = point.X.Select((t, j) => (t - firefly.Centroids[i].X[j]) * (t - firefly.Centroids[i].X[j])).Sum();
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
                        Math.Sqrt(this.Points[cpI].Dist2To(this.Points[cpJ])));

                    var b = 0.0;
                    for (var j = 0; j < firefly.Centroids.Count; j++)
                    {
                        if (i == j) continue;
                        b += firefly.CentroidPoints[j].Sum(cpJ =>
                            Math.Sqrt(this.Points[cpI].Dist2To(this.Points[cpJ])));
                    }

                    s += (b - a) / Math.Max(a, b);
                }
            }
            return s / this.Points.Count;
        }

        public double XieBeniIndex(Firefly firefly)
        {
            var sum = firefly.Centroids.Select((t, i) => firefly.CentroidPoints[i].Sum(pIdx =>
                this.Points[pIdx].Dist2To(t)) / firefly.CentroidPoints[i].Count).Sum();

            var minDist = double.MaxValue;
            for (var i = 0; i < firefly.Centroids.Count; i++)
                minDist = firefly.Centroids.Where((t, j) => i != j).Select(t =>
                    firefly.Centroids[i].Dist2To(t)).Concat(new[] { minDist }).Min();
            var res = sum / this.Points.Count / minDist;
            return double.IsNaN(res) ? -1 : res;
        }
        #endregion

        #region MFA
        public void Initialization(int firefliesNumber, int clustersNumber)
        {
            this.Fireflies.Clear();
            this.Delta = Math.Pow(1e-4 / 1.09, 1.0 / MaximumGenerations);

            this.AddRandomFireflies(firefliesNumber, clustersNumber);
            this.RankSwarm();
        }

        public async Task Iteration(int number)
        {
            this.MfaCanStop = true;
            var bestSse = this.Fireflies.First().SumOfSquaredError;
            var alphaT = 1e-3 * Math.Pow(this.Delta, number);

            for (var i = 0; i < this.Fireflies.Count; i++)
            {
                var lambdaI = .5 - i * (.5 - 1.9) / (this.Fireflies.Count - 1);
                for (var j = 0; j < this.Fireflies.Count; j++)
                {
                    if (i == j || this.Fireflies[i].SumOfSquaredError < this.Fireflies[j].SumOfSquaredError)
                        continue;

                    this.MoveTowards(this.Fireflies[i], this.Fireflies[j], alphaT, lambdaI);
                    this.MfaCanStop = false;
                }
            }

            this.RankSwarm();

            //if (Math.Abs(this.Fireflies.First().SumOfSquaredError - bestSse) <= 1)
            //    this.MfaCanStop = true;

            await Task.Delay(0);
        }

        private void RankSwarm()
        {
            this.Fireflies.Sort((f1, f2) => f1.SumOfSquaredError.CompareTo(f2.SumOfSquaredError));
        }

        private void MoveTowards(Firefly firefly, Firefly fireflyTo, double alpha, double lambda)
        {
            for (var i = 0; i < firefly.Centroids.Count; i++)
            {
                var r2 = firefly.Centroids[i].X.Select((t, h) =>
                (t - fireflyTo.Centroids[i].X[h]) * (t - fireflyTo.Centroids[i].X[h])).Sum();

                var brightness = .5 / (1 + 1e-4 * r2);
                for (var h = 0; h < firefly.Centroids[i].X.Count; h++)
                {
                    var randomPart = alpha * (this.Rand.NextDouble() - .5) * this.MantegnaRandom(lambda);
                    firefly.Centroids[i].X[h] += brightness * (fireflyTo.Centroids[i].X[h] - firefly.Centroids[i].X[h]) + randomPart;

                    if (firefly.Centroids[i].X[h] < 0 || this.RangeX < firefly.Centroids[i].X[h] ||
                        double.IsNaN(firefly.Centroids[i].X[h]))
                        firefly.Centroids[i].X[h] = this.Rand.Next(this.RangeX);
                }

                firefly.SumOfSquaredError = this.SumOfSquaredError(firefly);
            }
        }


        private double GaussianRandom(double mue, double sigma)
        {
            double x1;
            double w;
            const int RandMax = 0x7fff;
            do
            {
                x1 = 2.0 * this.Rand.Next(RandMax) / (RandMax + 1) - 1.0;
                var x2 = 2.0 * this.Rand.Next(RandMax) / (RandMax + 1) - 1.0;
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
            var x = this.GaussianRandom(0, sigmaX);
            var y = Math.Abs(this.GaussianRandom(0, 1.0));
            return x / Math.Pow(y, lambda1);
        }
        #endregion

        #region K-means
        public void InitializationKMeans(int clustersNumber)
        {
            this.KMeansCanStop = false;
            this.Fireflies.Clear();
            this.AddRandomFireflies(1, clustersNumber);
            this.FillCentroidPoints(this.Fireflies.First());
        }

        public void InitializationKMeansPlusPlus(int clustersNumber)
        {
            this.KMeansCanStop = false;
            this.Fireflies.Clear();

            var firefly = new Firefly();
            var randomPoint = this.Points[this.Rand.Next(this.Points.Count)];
            var firstcentroid = new ClusterPoint();
            firstcentroid.X.Add(randomPoint.X[0]);
            firstcentroid.X.Add(randomPoint.X[1]);
            firefly.Centroids.Add(firstcentroid);
            firefly.CentroidPoints.Add(new List<int>());

            for (var i = 1; i < clustersNumber; i++)
            {
                var sse = 0.0;
                for (var pIndex = 0; pIndex < this.Points.Count; pIndex++)
                {
                    var point = this.Points[pIndex];
                    var distMin = double.MaxValue;
                    //var indexOfMinCentroid = -1;
                    foreach (var c in firefly.Centroids)
                    {
                        var r2 = point.X.Select((t, j) => (t - c.X[j]) * (t - c.X[j])).Sum();
                        if (distMin <= r2) continue;
                        distMin = r2;
                        //indexOfMinCentroid = i;
                    }
                    sse += distMin;
                    //firefly.CentroidPoints[indexOfMinCentroid].Add(pIndex);
                }

                //var point = new ClusterPoint();
                //point.X.Add(this.Rand.Next(this.RangeX));
                //point.X.Add(this.Rand.Next(this.RangeY));
                //firefly.Centroids.Add(point);
                //firefly.CentroidPoints.Add(new List<int>());
            }
            firefly.SumOfSquaredError = this.SumOfSquaredError(firefly);
            this.Fireflies.Add(firefly);

            this.FillCentroidPoints(this.Fireflies.First());
        }

        public async Task IterationKMeans()
        {
            var firefly = this.Fireflies.First();

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
                    for (var h = 0; h < this.Points[cpIdx].X.Count; h++)
                        newPoint.X[h] += this.Points[cpIdx].X[h];

                for (var h = 0; h < newPoint.X.Count; h++)
                    newPoint.X[h] /= cp.Count;

                if (firefly.Centroids[i].Dist2To(newPoint) < 1e-4)
                    finalPointsNumber++;
                firefly.Centroids[i] = newPoint;
            }

            this.FillCentroidPoints(firefly);
            this.UpdatePoints(firefly);

            if (finalPointsNumber == firefly.Centroids.Count) this.KMeansCanStop = true;

            await Task.Delay(0);
        }
        #endregion
    }
}
