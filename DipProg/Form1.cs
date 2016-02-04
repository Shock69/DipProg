using System;
using System.Collections.Generic;
using System.Windows.Forms;
using FileHelpers;
using System.Data;
using System.Linq;
using FCM;
using System.Text;
using System.Globalization;

namespace DipProg
{
    public partial class Form1 : Form
    {
        private List<Description> m_Descriptions;
        private DataTable m_Data;
        private DataTable m_FuzzyData;
        private DataTable m_TestingSample;
        private DataTable m_LearningSample;

        public Form1()
        {
            InitializeComponent();
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = @"d:\Home\Projects\DipProg\";

            if(ofd.ShowDialog() == DialogResult.OK)
            {
                DescriptionReader dr = new DescriptionReader(ofd.FileName+".xml");
                m_Descriptions = dr.GetDescriptions();

                var detector = new FileHelpers.Detection.SmartFormatDetector();
                var formats = detector.DetectFileFormat(ofd.FileName);
                var delimited = formats[0].ClassBuilderAsDelimited;

                m_Data = CommonEngine.CsvToDataTable(ofd.FileName, delimited.ClassName, delimited.Delimiter[0], false);

                dataGridView1.DataSource = m_Data;
                dataGridView1.AutoGenerateColumns = true;

                m_FuzzyData = m_Data.Copy();
                dataGridView2.AutoGenerateColumns = true;
                dataGridView2.DataSource = m_FuzzyData;
            }
        }

        private void fuzzificationToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Fuzzification fuzzification = new Fuzzification(m_Descriptions, m_Data);
            m_FuzzyData = fuzzification.Calculate();
            dataGridView2.AutoGenerateColumns = true;
            dataGridView2.DataSource = m_FuzzyData;
        }

        private void shuffleToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            DataTable tempDt = m_FuzzyData.Rows.OfType<DataRow>().Shuffle(new Random()).CopyToDataTable();
            m_FuzzyData = tempDt.AsEnumerable().CopyToDataTable();
            dataGridView2.DataSource = m_FuzzyData;
            dataGridView2.Update();
            m_LearningSample = m_FuzzyData.AsEnumerable().CopyToDataTable();
        }

        private void split3070ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int amount = m_FuzzyData.Rows.Count;
            int learningAmount = (int)Math.Round(amount * 0.7);
            int testingAmount = amount - learningAmount;

            m_LearningSample = m_FuzzyData.AsEnumerable().Take(learningAmount).CopyToDataTable();
            m_TestingSample = m_FuzzyData.AsEnumerable().Skip(learningAmount).CopyToDataTable();
        }

        private void cToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<ClusterPoint> points = new List<ClusterPoint>();
            List<ClusterCentroid> centroids = new List<ClusterCentroid>();

            foreach (DataRow row in m_LearningSample.Rows)
            {
                string tag = "";
                List<double> data = new List<double>();
                for(int colNum = 0; colNum<m_LearningSample.Columns.Count; colNum++)
                {
                    if (!m_Descriptions[colNum].IsNotUse)
                    {
                        data.Add(double.Parse(row.Field<string>(colNum), CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        tag = row.Field<string>(colNum);
                    }
                }
                points.Add(new ClusterPoint(data, tag));
            }

            Random rnd = new Random();
            for (int centroidNum = 0; centroidNum < 10; centroidNum++)
            {
                List<double> data = new List<double>();
                for (int colNum = 0; colNum < m_LearningSample.Columns.Count; colNum++)
                {
                    if (!m_Descriptions[colNum].IsNotUse)
                    {
                        data.Add(0.0/*rnd.NextDouble()*/);
                    }
                }
                centroids.Add(new ClusterCentroid(data));
            }

            CMeansAlgorithm alg = new CMeansAlgorithm(points, centroids, 2);
            int iterations = alg.Run(Math.Pow(10, -5));

            double[,] matrix = alg.U;

            for (int j = 0; j < points.Count; j++)
            {
                double max = -1.0;
                string output = string.Empty;
                for (int i = 0; i < centroids.Count; i++)
                {
                    if(max < matrix[j, i])
                    {
                        ClusterPoint p = points[j];
                        StringBuilder sb = new StringBuilder();
                        for (int k = 0; k < p.Data.Count; k++)
                        {
                            sb.AppendFormat("{0}; ", p.Data[k]);
                        }
                        max = matrix[j, i];
                        output = string.Format("{0:00} Point: ({1}) ClusterIndex: {2} Value: {3} Class {4}", j + 1, sb, p.ClusterIndex, matrix[j, i], p.Tag);                        
                    }
                }
                Console.WriteLine(output);
            }

            Console.WriteLine();
            Console.WriteLine("Iteration count: {0}", iterations);
        }
    }
}
