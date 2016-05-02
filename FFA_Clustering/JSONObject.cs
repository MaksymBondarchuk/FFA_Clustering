using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFA_Clustering
{
    public class JsonObject
    {
        public bool IsClustered;
        public List<ClusterPoint> Points;
        public List<Firefly> Fireflies;
    }
}
