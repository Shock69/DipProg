using System;
using System.Collections.Generic;
using System.Windows.Forms;
using FileHelpers;
using System.Data;
using System.Linq;
using FCM;

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
            dataGridView2.DataSource = m_FuzzyData.Rows.OfType<DataRow>().Shuffle(new Random()).CopyToDataTable();
            dataGridView2.Update();
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
                List<double> data = new List<double>();
                for(int colNum = 0; colNum<m_LearningSample.Columns.Count; colNum++)
                {
                    if (!m_Descriptions[colNum].IsNotUse)
                    {
                        data.Add(Convert.ToDouble(row.Field<string>(colNum)));
                    }
                }
                points.Add(new ClusterPoint(data));
            }

            for (int rowNum = 0; rowNum < m_LearningSample.Columns.Count; rowNum++)
            {
                Random rnd = new Random();
                List<double> data = new List<double>();
                for (int colNum = 0; colNum < m_LearningSample.Columns.Count; colNum++)
                {
                    data.Add(rnd.NextDouble());
                }
                centroids.Add(new ClusterCentroid(data));
            }

                CMeansAlgorithm alg = new CMeansAlgorithm(points, centroids, 2);
            int iterations = alg.Run(Math.Pow(10, -5));

            double[,] matrix = alg.U;

            for (int j = 0; j < points.Count; j++)
            {
                for (int i = 0; i < centroids.Count; i++)
                {
                    ClusterPoint p = points[j];
                    Console.WriteLine("{0:00} Point: ({1};{2}) ClusterIndex: {3} Value: {4:0.000}", j + 1, p.Data[0], p.Data[1], p.ClusterIndex, matrix[j, i]);
                }
            }

            Console.WriteLine();
            Console.WriteLine("Iteration count: {0}", iterations);
        }
    }
}
