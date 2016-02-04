using System.Collections.Generic;

namespace FCM
{
    public class ClusterPoint
    {
        /// Gets or sets X-coord of the point
        ///
        public List<double> Data { get; set; }

        ///
        /// Gets or sets some additional data for point
        ///
        public object Tag { get; set; }

        ///
        /// Gets or sets cluster index
        ///
        public double ClusterIndex { get; set; }

        ///
        /// Basic constructor
        ///
        /// X-coord
        /// Y-coord
        public ClusterPoint(List<double> data)
        {
            Data = data;
            ClusterIndex = -1;
        }

        ///
        /// Basic constructor
        ///
        /// X-coord
        /// Y-coord
        public ClusterPoint(List<double> data, object tag)
        {
            Data = data;
            Tag = tag;
            ClusterIndex = -1;
        }
    }
}