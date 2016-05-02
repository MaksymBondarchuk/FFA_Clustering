using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;

namespace FFA_Clustering
{
    public class Firefly
    {
        public List<ClusterPoint> Centroids { get; set; } = new List<ClusterPoint>();
        public List<List<int>> CentroidPoints { get; set; } = new List<List<int>>();

        private Random Rand { get; } = new Random();

        public double Sse { get; set; }

        public void MoveTowards(Firefly firefly, double alpha, double lambda)
        {
            for (var i = 0; i < Centroids.Count; i++)
            {
                var r2 = Centroids[i].X.Select((t, h) => 
                (t - firefly.Centroids[i].X[h])*(t - firefly.Centroids[i].X[h])).Sum();

                var brightness = .5 / (1 + 1e-4 * r2);
                for (var h = 0; h < Centroids[i].X.Count; h++)
                {
                    var randomPart = alpha * (Rand.NextDouble() - .5) * MantegnaRandom(lambda);
                    //var randomPart = 0;
                    Centroids[i].X[h] += brightness * (firefly.Centroids[i].X[h] - Centroids[i].X[h]) + randomPart;
                }
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
