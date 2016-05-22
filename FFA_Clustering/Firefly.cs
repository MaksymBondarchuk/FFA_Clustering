using System.Collections.Generic;

namespace FFA_Clustering
{
    public class Firefly
    {
        public List<ClusterPoint> Centroids { get; } = new List<ClusterPoint>();
        public List<List<int>> CentroidPoints { get; } = new List<List<int>>();

        public double SumOfSquaredError { get; set; }
    }
}
