// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Firefly.cs" >
//   Copyright (c) 2016 Maxym Bondarchuk, Yuri Zorin
//   Created as diploma project. Resarch advisor - Yuri Zorin.
//   National Technical University of Ukraine "Kyiv Polytechnic Institute" Kyiv, Ukraine, 2016
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace FFA_Clustering
{
    /// <summary>
    /// Represents firefly for MFA
    /// </summary>
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
