﻿using System.Collections.Generic;
using System.Linq;

namespace FFA_Clustering
{
    public class ClusterPoint
    {
        /// <summary>
        /// Coordinates
        /// </summary>
        public List<double> X { get; set; } = new List<double>();

        public bool IsCentroid { get; set; }

        /// <summary>
        /// Number of centroid point belongs to
        /// </summary>
        public int BelongsToCentroid { get; set; } = -1;

        public double DistTo(ClusterPoint cp)
        {
            return X.Select((t, i) => (t - cp.X[i])*(t - cp.X[i])).Sum();
        }
    }
}
