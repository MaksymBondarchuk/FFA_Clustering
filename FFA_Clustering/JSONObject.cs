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
    }
}
