// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JSONObject.cs" company="NTUU 'KPI'">
//   Created by Max Bondarchuk
// </copyright>
// <summary>
//   Defines the JsonObject type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace FFA_Clustering
{
    using System.Collections.Generic;

    /// <summary>
    /// Class for load to/upload from JSON-file data set
    /// </summary>
    public class JsonObject
    {
        /// <summary>
        /// Gets or sets the points.
        /// </summary>
        public List<ClusterPoint> Points { get; set; }

        /// <summary>
        /// Gets or sets the fireflies.
        /// </summary>
        public List<Firefly> Fireflies { get; set; }

        /// <summary>
        /// Gets or sets the sum of squared error.
        /// </summary>
        public double SumOfSquaredError { get; set; }

        /// <summary>
        /// Gets or sets the silhouette method.
        /// </summary>
        public double SilhouetteMethod { get; set; }

        /// <summary>
        /// Gets or sets the xie beni index.
        /// </summary>
        // ReSharper disable once StyleCop.SA1650
        public double XieBeniIndex { get; set; }

        /// <summary>
        /// Gets or sets the clusters number.
        /// </summary>
        public int ClustersNumber { get; set; }

        /// <summary>
        /// Gets or sets the test runs number.
        /// </summary>
        public int TestRunsNumber { get; set; }
    }
}
