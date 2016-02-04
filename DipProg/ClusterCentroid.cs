using System.Collections.Generic;

namespace FCM
{
    public class ClusterCentroid : ClusterPoint
    {
        ///

        /// Basic constructor
        ///
        /// Centroid x-coord
        /// Centroid y-coord
        public ClusterCentroid(List<double> data)
            : base(data)
        {
        }
    }
}