﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JSONObject.cs" >
//   Copyright (c) 2016 Maxym Bondarchuk, Yuri Zorin
//   Created as diploma project. Resarch advisor - Yuri Zorin.
//   National Technical University of Ukraine "Kyiv Polytechnic Institute" Kyiv, Ukraine, 2016
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace FFA_Clustering
{
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

        /// <summary>
        /// Gets or sets fast mode for MFA
        /// </summary>
        public bool IsInFastMfaMode { get; set; }

        /// <summary>
        /// Gets or sets simple draw mode
        /// </summary>
        public bool IsInSimpleDrawMode { get; set; }
    }
}
