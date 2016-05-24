using System.Collections.Generic;
using System.Linq;

namespace FFA_Clustering
{
    public class ClusterPoint
    {
        /// <summary>
        /// Coordinates
        /// </summary>
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global (without this "set" JSON-file cannot be loaded)
        public List<double> X { get; set; } = new List<double>();

        /// <summary>
        /// Number of centroid point belongs to
        /// </summary>
        public int BelongsToCentroid { get; set; } = -1;

        public double Dist2To(ClusterPoint cp)
        {
            return this.X.Select((t, i) => (t - cp.X[i])*(t - cp.X[i])).Sum();
        }
    }
}
