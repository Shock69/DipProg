using System;
using System.Collections.Generic;
using System.Windows.Forms;
using FileHelpers;
using System.Data;

namespace DipProg
{
    public partial class Form1 : Form
    {
        private List<Description> m_Descriptions;
        private DataTable m_Data;

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

        private void fuzzificationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Fuzzification fuzzification = new Fuzzification(m_Descriptions, m_Data);
            fuzzification.Calculate();
        }
    }
}
