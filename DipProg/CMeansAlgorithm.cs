using System;
using System.Collections.Generic;

namespace FCM
{
    public sealed class CMeansAlgorithm
    {
        ///

        /// Array containing all points used by the algorithm
        ///
        private readonly List<ClusterPoint> m_Points;

        ///
        /// Array containing all clusters handled by the algorithm
        ///
        private readonly List<ClusterCentroid> m_Clusters;

        ///
        /// Gets or sets membership matrix
        ///
        public double[,] U;

        ///
        /// Gets or sets the current fuzzyness factor
        ///
        private readonly double m_Fuzzyness;

        ///
        /// Algorithm precision
        ///
        private readonly double m_Eps = Math.Pow(10, -5);

        ///
        /// Gets or sets objective function
        ///
        private double J { get; set; }

        ///
        /// Gets or sets log message
        ///
        public string Log { get; set; }

        ///
        /// Initialize the algorithm with points and initial clusters
        ///
        /// Points
        /// Clusters
        /// The fuzzyness factor to be used
        public CMeansAlgorithm(List<ClusterPoint> points, List<ClusterCentroid> clusters, float fuzzy)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }

            if (clusters == null)
            {
                throw new ArgumentNullException("clusters");
            }

            m_Points = points;
            m_Clusters = clusters;

            U = new double[m_Points.Count, m_Clusters.Count];
            //Uk = new double[this.Points.Count, this.Clusters.Count];

            m_Fuzzyness = fuzzy;

            // Iterate through all points to create initial U matrix
            for (int i = 0; i < m_Points.Count; i++)
            {
                ClusterPoint p = m_Points[i];
                double sum = 0.0;

                for (int j = 0; j < m_Clusters.Count; j++)
                {
                    ClusterCentroid c = m_Clusters[j];
                    var diff = CalculateEulerDistance(p, c);
                    U[i, j] = (diff == 0) ? m_Eps : diff;
                    sum += U[i, j];
                }

                double sum2 = 0.0;
                for (int j = 0; j < m_Clusters.Count; j++)
                {
                    U[i, j] = 1.0 / Math.Pow(U[i, j] / sum, 2.0 / (m_Fuzzyness - 1.0));
                    sum2 += U[i, j];
                }

                for (int j = 0; j < m_Clusters.Count; j++)
                {
                    U[i, j] = U[i, j] / sum2;
                }
            }

            RecalculateClusterIndexes();
        }

        ///
        /// Recalculates cluster indexes
        ///
        private void RecalculateClusterIndexes()
        {
            for (int i = 0; i < m_Points.Count; i++)
            {
                double max = -1.0;
                var p = m_Points[i];

                for (int j = 0; j < m_Clusters.Count; j++)
                {
                    if (max < U[i, j])
                    {
                        max = U[i, j];
                        p.ClusterIndex = (max == 0.5) ? 0.5 : j;
                    }
                }
            }
        }

        ///
        /// Perform one step of the algorithm
        ///
        public void Step()
        {
            for (int c = 0; c < m_Clusters.Count; c++)
            {
                for (int h = 0; h < m_Points.Count; h++)
                {
                    double top = CalculateEulerDistance(m_Points[h], m_Clusters[c]);
                    if (top < 1.0) top = m_Eps;

                    // Bottom is the sum of distances from this data point to all clusters.
                    double sumTerms = 0.0;
                    for (int ck = 0; ck < m_Clusters.Count; ck++)
                    {
                        double thisDistance = CalculateEulerDistance(m_Points[h], m_Clusters[ck]);
                        if (thisDistance < 1.0) thisDistance = m_Eps;
                        sumTerms += Math.Pow(top / thisDistance, 2.0 / (m_Fuzzyness - 1.0));
                    }

                    // Then the MF can be calculated as...
                    U[h, c] = 1.0 / sumTerms;
                }
            }

            RecalculateClusterIndexes();
        }

        ///
        /// Calculates Euler's distance between point and centroid
        ///
        /// Point
        /// Centroid
        /// Calculated distance
        private double CalculateEulerDistance(ClusterPoint p, ClusterCentroid c)
        {
            double sum = 0;
            for(int i=0; i<p.Data.Count; i++)
            {
                sum += Math.Pow(p.Data[i] - c.Data[i], 2);
            }
            return Math.Sqrt(sum);
        }

        ///
        /// Calculate the objective function
        ///
        /// The objective function as double value
        private double CalculateObjectiveFunction()
        {
            double jk = 0;

            for (int i = 0; i < m_Points.Count; i++)
            {
                for (int j = 0; j < m_Clusters.Count; j++)
                {
                    jk += Math.Pow(U[i, j], m_Fuzzyness) * Math.Pow(CalculateEulerDistance(m_Points[i], m_Clusters[j]), 2);
                }
            }
            return jk;
        }

        ///
        /// Calculates the centroids of the clusters
        ///
        private void CalculateClusterCenters()
        {
            for (int j = 0; j < m_Clusters.Count; j++)
            {
                ClusterCentroid c = m_Clusters[j];
                List<double> uData = new List<double>();
                for (int k = 0; k < c.Data.Count; k++)
                {
                    uData.Add(0);
                }
                double l = 0.0;

                for (int i = 0; i < m_Points.Count; i++)
                {
                    ClusterPoint p = m_Points[i];

                    double uu = Math.Pow(U[i, j], m_Fuzzyness);
                    for(int k = 0; k < c.Data.Count; k++)
                    {
                        uData[k] += uu * c.Data[k];
                    }
                    l += uu;
                }

                for (int k = 0; k < c.Data.Count; k++)
                {
                    c.Data[k] = uData[k] / l;
                }

                Log += string.Format("Cluster Centroid: ({0})" + Environment.NewLine, c.Data);
            }
        }

        ///
        /// Perform a complete run of the algorithm until the desired accuracy is achieved.
        /// For demonstration issues, the maximum Iteration counter is set to 20.
        ///
        /// Algorithm accuracy
        /// The number of steps the algorithm needed to complete
        public int Run(double accuracy)
        {
            int i = 0;
            int maxIterations = 20;
            do
            {
                i++;
                J = CalculateObjectiveFunction();
                CalculateClusterCenters();
                Step();
                double jnew = CalculateObjectiveFunction();
                if (Math.Abs(J - jnew) < accuracy) break;
            }
            while (maxIterations > i);
            return i;
        }
    }
}