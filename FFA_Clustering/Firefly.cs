using System.Collections.Generic;

namespace FFA_Clustering
{
    public class Firefly
    {
        /// <summary>
        /// Centroids of clusters
        /// </summary>
        public List<ClusterPoint> Centroids { get; } = new List<ClusterPoint>();

        /// <summary>
        /// Indexes of configuration points that belong to cluster from Centroids
        /// </summary>
        public List<List<int>> CentroidPoints { get; } = new List<List<int>>();

        /// <summary>
        /// Sum of squared error value for firefly
        /// </summary>
        public double SumOfSquaredError { get; set; }
    }
}
