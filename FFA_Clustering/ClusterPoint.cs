using System.Windows;

namespace FFA_Clustering
{
    public class ClusterPoint
    {
        /// <summary>
        /// Coordinates
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global (without this "set" JSON-file cannot be loaded)
        public double X { get; set; }
        public double Y { get; set; }

        /// <summary>
        /// Number of centroid point belongs to
        /// </summary>
        public int BelongsToCentroid { get; set; } = -1;

        /// <summary>
        /// Calculates distance to other point
        /// </summary>
        /// <param name="cp">Point calculate distance to</param>
        /// <returns></returns>
        public double Dist2To(ClusterPoint cp)
        {
            var x = X - cp.X;
            var y = Y - cp.Y;
            return x * x + y * y;
        }
    }
}
