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

namespace PerformanceViewer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        SqlConnection con = new SqlConnection(@"Data Source=NKB-PC\SQLEXPRESS;AttachDbFilename=E:\FFWS\Database\GBMStationRF.mdf;Integrated Security=True;User Instance=True");
        SqlCommand cmd = new SqlCommand();
        DataSet ds = new DataSet();
        SqlDataAdapter ad = new SqlDataAdapter();
        DataTable dt = new DataTable();

        private void Form1_Load(object sender, EventArgs e)
        {
            con.Open();
            cmd = new SqlCommand("Select StationName from ForecastStation", con);
            ad.SelectCommand = cmd;
            ad.Fill(dt);
            ad.Dispose();
            cmd.Dispose();
            con.Close();
            
            foreach (DataRow dr in dt.Rows)
            {
                listStationBox.Items.Add(dr[0].ToString().ToUpperInvariant());
            }
            dataStatGridView.Rows.Add(5);
        }

        
        private void listStationBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ///---------------------------------------------Graph Preparation-----------------------------------------------
            string element = listStationBox.SelectedItem.ToString();
            dt.Rows.Clear();
            dt.Columns.Clear();
            cmd = new SqlCommand("Select Date, WLValue from GBMStationWL Where Station = @station Order by Date ASC", con);
            cmd.Parameters.AddWithValue("@station", element);
            ad.SelectCommand = cmd;
            ad.Fill(dt);
            ad.Dispose();
            cmd.Dispose();
            con.Close();
            List<DateTime> dateObs = new List<DateTime>();
            List<float> day0Value = new List<float>();
            foreach (DataRow dr in dt.Rows)
            {
                dateObs.Add(Convert.ToDateTime(dr[0]));
                day0Value.Add(Convert.ToSingle(dr[1]));
            }

            dt.Rows.Clear();
            dt.Columns.Clear();
            cmd = new SqlCommand("Select *from ForecastedWL Where StationName = @station Order by Date ASC", con);
            cmd.Parameters.AddWithValue("@station", element);
            ad.SelectCommand = cmd;
            ad.Fill(dt);
            ad.Dispose();
            cmd.Dispose();
            con.Close();
            List<DateTime> dateValue = new List<DateTime>();
            List<float> day1Value = new List<float>();
            List<float> day2Value = new List<float>();
            List<float> day3Value = new List<float>();
            List<float> day4Value = new List<float>();
            List<float> day5Value = new List<float>();

            foreach (DataRow dr in dt.Rows)
            {
                dateValue.Add(Convert.ToDateTime(dr[0]));
                day1Value.Add(Convert.ToSingle(dr[2]));
                day2Value.Add(Convert.ToSingle(dr[3]));
                day3Value.Add(Convert.ToSingle(dr[4]));
                day4Value.Add(Convert.ToSingle(dr[5]));
                day5Value.Add(Convert.ToSingle(dr[6]));
            }

            chartStation.Series[0].Points.Clear();
            chartStation.Series[1].Points.Clear();
            chartStation.Series[2].Points.Clear();
            chartStation.Series[3].Points.Clear();
            chartStation.Series[4].Points.Clear();
            chartStation.Series[5].Points.Clear();
            chartStation.Titles[0].Text = "5 Day Forecast Performance" + "\r\n" + "Station Name: " + element;

            List<DateTime> chartDate = new List<DateTime>();
            List<float> chartWL = new List<float>();
            chartWL.Add(day1Value.Max());
            chartWL.Add(day2Value.Max());
            chartWL.Add(day3Value.Max());
            chartWL.Add(day4Value.Max());
            chartWL.Add(day5Value.Max());
            chartWL.Add(day1Value.Min());
            chartWL.Add(day2Value.Min());
            chartWL.Add(day3Value.Min());
            chartWL.Add(day4Value.Min());
            chartWL.Add(day5Value.Min());

            for (int i = 0; i < dateObs.Count; i++)
            {
                chartStation.Series[0].Points.AddXY(dateObs[i], day0Value[i]);
            }

            for (int i = 0; i < dateValue.Count; i++)
            {
                chartStation.Series[1].Points.AddXY(dateValue[i].AddDays(1), (day1Value[i]));
                chartStation.Series[2].Points.AddXY(dateValue[i].AddDays(2), (day2Value[i]));
                chartStation.Series[3].Points.AddXY(dateValue[i].AddDays(3), (day3Value[i]));
                chartStation.Series[4].Points.AddXY(dateValue[i].AddDays(4), (day4Value[i]));
                chartStation.Series[5].Points.AddXY(dateValue[i].AddDays(5), (day5Value[i]));
            }
            chartStation.Titles[0].Font = new System.Drawing.Font("Lucida Bright", 12, FontStyle.Bold);
            chartStation.Titles[0].ForeColor = System.Drawing.Color.Green;
            chartStation.Legends[0].Position = new ElementPosition(16.0f, 16.0f, 70, 4);
            chartStation.Legends[0].BackColor = System.Drawing.Color.Transparent;
            chartStation.Legends[0].Font = new System.Drawing.Font("Lucida Bright", 8.0f, FontStyle.Regular);
            chartStation.Legends[0].BorderColor = System.Drawing.Color.Brown;
            chartStation.Legends[0].BorderDashStyle = ChartDashStyle.DashDot;
            chartStation.ChartAreas[0].AxisX.Maximum = Convert.ToDouble(dateValue.Last().ToOADate());
            chartStation.ChartAreas[0].AxisX.Minimum = Convert.ToDouble(dateValue.First().ToOADate());
            chartStation.ChartAreas[0].AxisX.MajorGrid.Interval = 15.0;
            chartStation.ChartAreas[0].AxisX.MajorTickMark.Interval = 30.0;
            chartStation.ChartAreas[0].AxisX.LabelStyle.Format = "yyyy-MM-dd hh:mm:ss";
            chartStation.ChartAreas[0].AxisY.Maximum = Math.Ceiling(chartWL.Max());
            chartStation.ChartAreas[0].AxisY.Minimum = Math.Floor(chartWL.Min());
            chartWL.Clear();


            //------------------------------------------------------------ MAE AND R-SQUARE Calculation -----------------------------------------

            dataStatGridView.Rows[0].Cells[0].Value = "Day 1";
            dataStatGridView.Rows[1].Cells[0].Value = "Day 2";
            dataStatGridView.Rows[2].Cells[0].Value = "Day 3";
            dataStatGridView.Rows[3].Cells[0].Value = "Day 4";
            dataStatGridView.Rows[4].Cells[0].Value = "Day 5";

            float[,] simData = new float[day1Value.Count, 5];
            for (int i = 0; i < day1Value.Count; i++)
            {
                simData[i, 0] = day1Value[i];
                simData[i, 1] = day2Value[i];
                simData[i, 2] = day3Value[i];
                simData[i, 3] = day4Value[i];
                simData[i, 4] = day5Value[i];
            }

            List<float> obsWL = new List<float>();
            List<float> simWL = new List<float>();
            for (int k = 0; k < 5; k++)
            {
                for (int i = 0; i < day1Value.Count; i++)
                {

                    for (int j = 0; j < day0Value.Count; j++)
                    {
                        if (dateValue[i].AddDays(k+1) == dateObs[j])
                        {

                            obsWL.Add(day0Value[j]);
                            simWL.Add(day1Value[i]);
                            break;
                        }
                    }
                }
                int n = obsWL.Count;
                float sumSim = 0;
                float sumObs = 0;
                float sumSim2 = 0;
                float sumObs2 = 0;
                float sumObsSim = 0;
                float diffAbs = 0;
                for (int i = 0; i < obsWL.Count; i++)
                {
                    diffAbs = diffAbs + Math.Abs(obsWL[i] - simWL[i]); 
                    sumObs = sumObs + obsWL[i];
                    sumSim = sumSim + simWL[i];
                    sumObs2 = sumObs2 + obsWL[i] * obsWL[i];
                    sumSim2 = sumSim2 + simWL[i] * simWL[i];
                    sumObsSim = sumObsSim + obsWL[i] * simWL[i];
                }
                obsWL.Clear();
                simWL.Clear();
                double mae= diffAbs/n;
                double rSquare = Math.Pow((n * sumObsSim - sumObs * sumSim) / (Math.Sqrt((n * sumObs2) - sumObs * sumObs) * Math.Sqrt((n * sumSim2) - sumSim * sumSim)), 2);
                dataStatGridView.Rows[k].Cells[1].Value = Math.Round(rSquare, 2);
                dataStatGridView.Rows[k].Cells[2].Value = Math.Round(mae, 2);
            }

            dateValue.Clear();
            dateObs.Clear();
            day0Value.Clear();
            day1Value.Clear();
            day2Value.Clear();
            day3Value.Clear();
            day4Value.Clear();
            day5Value.Clear();
        }
    }
}
