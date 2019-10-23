using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;

namespace StructureFF
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SqlConnection con = new SqlConnection(@"Data Source=.\SQLEXPRESS;AttachDbFilename=E:\FFWS\Database\GBMStationRF.mdf;Integrated Security=True;User Instance=True");
            SqlCommand cmd = new SqlCommand();
            DataTable table = new DataTable();
            SqlDataAdapter adapter = new SqlDataAdapter();


            for (int i = 0; i < 8; i++)
            {
                chartStructureFF.Series[i].Points.Clear();
            }
            string[] forecastWLinfo = File.ReadAllLines(@"E:\FFWS\ModelOutput\StructureFF\Water_Profile.txt");
            float[,] forecastWL = new float[6, 67];
            for (int i = 0; i < 6; i++)
            {
                var sepaText = forecastWLinfo[i].Split(',');
                for (int j = 0; j < sepaText.Length - 1; j++)
                {
                    forecastWL[i, j] = float.Parse(sepaText[j]);
                }
            }
            chartStructureFF.Series[0].ChartType = SeriesChartType.Line;  // Set chart type like Bar chart, Pie chart
            chartStructureFF.Series[0].IsValueShownAsLabel = false;
            chartStructureFF.Series[0].BorderWidth = 1;
            chartStructureFF.Legends[0].Position = new ElementPosition(15.0f, 93.0f, 75, 8);
            chartStructureFF.Series[1].IsVisibleInLegend = false;
            chartStructureFF.Series[1].LabelAngle = -90;
            chartStructureFF.Series[1].LabelForeColor = System.Drawing.Color.Blue;
            chartStructureFF.Series[1].SmartLabelStyle.Enabled = false;

            try
            {
                cmd = new SqlCommand("Select Chainage, CrestLevel from CLDhakaMawa", con);
                con.Open();
                adapter.SelectCommand = cmd;
                adapter.Fill(table);
                adapter.Dispose();
                cmd.Dispose();
                con.Close();

                chartStructureFF.Titles[0].Text = @"Structure Based Forecast\nWater level Profile Forecast Along Dhaka-Mawa Road\nForecast Date= " + DateTime.Today.ToString("yyyy-MM-dd");
                foreach (DataRow dr in table.Rows)
                {
                    chartStructureFF.Series[0].Points.AddXY(dr[0], dr[1]);
                }
                table.Clear();

                string[] locationName = new string[] { "Babu-Bazar", "Dhaleswari bridge", "Nimtoli bazar", "Srinagar", "Mawa" };
                float[] locationPosition = new float[] { 0f, 10.92f, 14.69f, 25.82f, 32.0f};
                for (int i = 0; i < locationName.Length; i++)
                {
                    chartStructureFF.Series[1].Points.AddXY(locationPosition[i], 10);
                    chartStructureFF.Series[1].Points[i].Label = locationName[i];
                }

                float[] stationChainage = new float[] { 0, 4.74f, 11.02f, 14.8f, 22.0f, 26.0f, 32.23f };
                int[] stationSerial = new int[] { 0, 1, 2, 64, 65, 66, 63 };
                for (int i = 0; i < 6; i++)
                {
                    for (int j = 0; j < stationChainage.Length; j++)
                    {
                        chartStructureFF.Series[2 + i].Points.AddXY(stationChainage[j], forecastWL[i, stationSerial[j]]);
                    }
                }

                chartStructureFF.ChartAreas[0].AxisX.MajorGrid.Interval = 5.0;
                chartStructureFF.ChartAreas[0].AxisX.Maximum = 35.0;
                chartStructureFF.ChartAreas[0].AxisY.Minimum = 2.0;
                chartStructureFF.ChartAreas[0].AxisY.Maximum = 14.0;
                chartStructureFF.ChartAreas[0].AxisX.LabelStyle.Interval = 5.0;
                chartStructureFF.ChartAreas[0].AxisX.MajorTickMark.Interval = 5.0;
                chartStructureFF.SaveImage(@"E:\FFWS\ModelOutput\StructureFF\Dhaka-Mawa.png", ChartImageFormat.Png);

            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }

            try
            {
                for (int i = 0; i < 8; i++)
                {
                    chartStructureFF.Series[i].Points.Clear();
                }

                cmd = new SqlCommand("Select Chainage, CrestLevel from CLJamunaRB", con);
                con.Open();
                adapter.SelectCommand = cmd;
                adapter.Fill(table);
                adapter.Dispose();
                cmd.Dispose();
                con.Close();

                chartStructureFF.Titles[0].Text = @"Structure Based Forecast\nWater level Profile Forecast Along Jamuna Right Bank\nForecast Date= " + DateTime.Today.ToString("yyyy-MM-dd");
                foreach (DataRow dr in table.Rows)
                {
                    chartStructureFF.Series[0].Points.AddXY(dr[0], dr[1]);
                }
                table.Clear();

                string[] locationName = new string[] { "Gaibadha", "Sariakandi", "Kazipur", "Sirajganj_WL_gauge", "Jamuna Bridge", "Balkuchi", "Enayetpur" };
                float[] locationPosition = new float[] { 27.05559457f, 83.32890404f, 118.3479595f, 144.5735253f, 154.9055796f, 163.8322879f, 170.9565421f };
                for (int i = 0; i < locationName.Length; i++)
                {
                    chartStructureFF.Series[1].Points.AddXY(locationPosition[i], 25.0-i);
                    chartStructureFF.Series[1].Points[i].Label = locationName[i];
                }

                float[] stationChainage = new float[] { 0, 2.72f, 6.11f, 9.84f, 12.79f, 18.67f, 24.67f, 29.53f, 35.99f, 43.51f, 51.04f, 59.52f, 62.92f, 69.48f, 78.76f, 88.66f, 95.62f, 104.5f, 113.1f, 116.61f, 120.63f, 126.29f, 132.62f, 139.3f, 144.96f, 149.6f, 159.1f, 160.24f, 160.8f, 161.93f, 164.48f, 165.55f, 169.29f, 173.81f, 182.02f };
                for (int i = 0; i < 6; i++)
                {
                    for (int j = 0; j < stationChainage.Length; j++)
                    {
                        chartStructureFF.Series[2 + i].Points.AddXY(stationChainage[j], forecastWL[i, 5 + j]);
                    }
                }
                chartStructureFF.ChartAreas[0].AxisX.MajorGrid.Interval = 10.0;
                chartStructureFF.ChartAreas[0].AxisX.Maximum = 190.0;
                chartStructureFF.ChartAreas[0].AxisY.Minimum = 5.0;
                chartStructureFF.ChartAreas[0].AxisY.Maximum = 30.0;
                chartStructureFF.ChartAreas[0].AxisX.LabelStyle.Interval = 10.0;
                chartStructureFF.ChartAreas[0].AxisX.MajorTickMark.Interval = 10.0;

                chartStructureFF.SaveImage(@"E:\FFWS\ModelOutput\StructureFF\Jamuna-RB.png", ChartImageFormat.Png);
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }

            try
            {
                for (int i = 0; i < 8; i++)
                {
                    chartStructureFF.Series[i].Points.Clear();
                }

                cmd = new SqlCommand("Select Chainage, CrestLevel from CLMDIP", con);
                con.Open();
                adapter.SelectCommand = cmd;
                adapter.Fill(table);
                adapter.Dispose();
                cmd.Dispose();
                con.Close();

                chartStructureFF.Titles[0].Text = @"Structure Based Forecast\nWater level Profile Forecast Along Meghna Dhonagoda Embankment\nForecast Date= " + DateTime.Today.ToString("yyyy-MM-dd");
                foreach (DataRow dr in table.Rows)
                {
                    chartStructureFF.Series[0].Points.AddXY(dr[0], dr[1]);
                }
                table.Clear();

                string[] locationName = new string[] { "Kalipur Pump Station", "Eklaspur Pump Station", "Udamdi Pump Station", "Matlab WL Gauge Station"};
                float[] locationPosition = new float[] { 8.93f, 22.69f, 35.34f, 38.77f };
                for (int i = 0; i < locationName.Length; i++)
                {
                    chartStructureFF.Series[1].Points.AddXY(locationPosition[i], 8);
                    chartStructureFF.Series[1].Points[i].Label = locationName[i];
                }

                float[] stationChainage = new float[] { 0f, 2.66f, 8.38f, 11.62f, 21.46f, 30.65f, 51.2f };
                for (int i = 0; i < 6; i++)
                {
                    for (int j = 0; j < stationChainage.Length; j++)
                    {
                        chartStructureFF.Series[2 + i].Points.AddXY(stationChainage[j], forecastWL[i, 58 + j]);
                    }
                }

                chartStructureFF.ChartAreas[0].AxisX.MajorGrid.Interval = 5.0;
                chartStructureFF.ChartAreas[0].AxisX.Maximum = 55.0;
                chartStructureFF.ChartAreas[0].AxisY.Minimum = 2.0;
                chartStructureFF.ChartAreas[0].AxisY.Maximum = 14.0;
                chartStructureFF.ChartAreas[0].AxisX.LabelStyle.Interval = 5.0;
                chartStructureFF.ChartAreas[0].AxisX.MajorTickMark.Interval = 5.0;
                chartStructureFF.SaveImage(@"E:\FFWS\ModelOutput\StructureFF\MDIP.png", ChartImageFormat.Png);
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }

            try
            {
                for (int i = 0; i < 8; i++)
                {
                    chartStructureFF.Series[i].Points.Clear();
                }

                cmd = new SqlCommand("Select Chainage, CrestLevel from CLPIRDP Order by Chainage ASC", con);
                con.Open();
                adapter.SelectCommand = cmd;
                adapter.Fill(table);
                adapter.Dispose();
                cmd.Dispose();
                con.Close();

                chartStructureFF.Titles[0].Text = @"Structure Based Forecast\nWater level Profile Forecast Along PIRDP Embankment\nForecast Date= " + DateTime.Today.ToString("yyyy-MM-dd");
                foreach (DataRow dr in table.Rows)
                {
                    chartStructureFF.Series[0].Points.AddXY(dr[0], dr[1]);
                }
                table.Clear();

                string[] locationName = new string[] { "Bera pump station", "Kaitala Pump Station", "Kashinathpur" };
                float[] locationPosition = new float[] { 19.59f, 32.18f, 38.75f };
                for (int i = 0; i < locationName.Length; i++)
                {
                    chartStructureFF.Series[1].Points.AddXY(locationPosition[i], 14);
                    chartStructureFF.Series[1].Points[i].Label = locationName[i];
                }

                float[] stationChainage = new float[] { 0f, 2.83f, 5.33f, 7.56f, 9.68f, 11.667f, 14.58f, 14.69f, 15.209f, 16.337f, 17.605f, 20.0f, 23.0f, 27.0f, 35.0f, 39.15f };
                for (int i = 0; i < 6; i++)
                {
                    for (int j = 0; j < stationChainage.Length; j++)
                    {
                        chartStructureFF.Series[2 + i].Points.AddXY(stationChainage[j], forecastWL[i, 40 + j]);
                    }
                }

                chartStructureFF.ChartAreas[0].AxisX.MajorGrid.Interval = 5.0;
                chartStructureFF.ChartAreas[0].AxisX.Maximum = 40.0;
                chartStructureFF.ChartAreas[0].AxisY.Minimum = 6.0;
                chartStructureFF.ChartAreas[0].AxisY.Maximum = 18.0;
                chartStructureFF.ChartAreas[0].AxisX.LabelStyle.Interval = 5.0;
                chartStructureFF.ChartAreas[0].AxisX.MajorTickMark.Interval = 5.0;
                chartStructureFF.SaveImage(@"E:\FFWS\ModelOutput\StructureFF\PIRDP.png", ChartImageFormat.Png);
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

    }
}
