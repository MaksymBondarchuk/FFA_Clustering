using System.Collections.Generic;
using System.Windows.Documents;

namespace FFA_Clustering
{
    public class Firefly
    {
        public List<ClusterPoint> Centroids { get; set; } = new List<ClusterPoint>();
        public List<List<int>> CentroidPoints { get; set; } = new List<List<int>>();
    }
}
