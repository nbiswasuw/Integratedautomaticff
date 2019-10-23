using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Drawing;
using DHI.Generic.MikeZero.DFS;

namespace ResultAnalysis
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.BackgroundColor = ConsoleColor.White;
            Console.WriteLine("Automated Flood Forecasting System of FMG, IWM.");

            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("------@@@@ Model Post Process Module @@@-------");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Parameters is initiating.....");
            SqlConnection con = new SqlConnection(@"Data Source=NKB-PC\SQLEXPRESS;AttachDbFilename=E:\FFWS\Database\GBMStationRF.mdf;Integrated Security=True;User Instance=True");
            SqlCommand cmd = new SqlCommand();
            System.Data.DataSet ds = new System.Data.DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            DataTable dt = new DataTable();
            ///------------------------------------------------------HD Model Result File Processing -----------------------------------------------------------------
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("HD Model Results processing....");
                Console.ResetColor();

                IDfsFile resFile = DfsFileFactory.DfsGenericOpen(@"E:\FFWS\Model\FF\Results\FF-HD.RES11");
                DateTime[] date = resFile.FileInfo.TimeAxis.GetDateTimes();
                DateTime sDate = date[0];
                IDfsFileInfo resfileInfo = resFile.FileInfo;
                IDfsItemData<float> data;
                int noTimeSteps = resfileInfo.TimeAxis.NumberOfTimeSteps;

                StringBuilder sb = new StringBuilder();

                List<DateTime> dfsDate = new List<DateTime>();
                for (int i = 0; i < noTimeSteps; i++)
                {
                    dfsDate.Add(sDate.AddHours(resFile.ReadItemTimeStep(1, i).Time));
                }
                List<float> dfsWLData = new List<float>();

                for (int i = 0; i < noTimeSteps; i++)
                {
                    int Wcounter = 0;
                    int nodeWCount = 0;

                    for (int j = 0; j < resFile.ItemInfo.Count; j++)
                    {
                        IDfsSimpleDynamicItemInfo dynamicItemInfo = resFile.ItemInfo[j];
                        string nameOftDynamicItem = dynamicItemInfo.Name;
                        string WLname = nameOftDynamicItem.Substring(0, 11);
                        if (WLname == "Water Level")
                        {
                            Wcounter = dynamicItemInfo.ElementCount;
                            data = (IDfsItemData<float>)resFile.ReadItemTimeStep(j + 1, i);
                            for (int z = 0; z < Wcounter; z++)
                            {
                                dfsWLData.Add(Convert.ToSingle(data.Data[z]));
                                nodeWCount = nodeWCount + 1;
                            }
                        }
                    }
                    sb.Append("\n" + dfsDate[i].ToString("yyyy-MM-dd HH:mm:ss"));

                    for (int k = 0; k < nodeWCount; k++)
                    {
                        sb.Append("," + Math.Round(dfsWLData[k], 2));
                    }
                    dfsWLData.Clear();
                }
                File.WriteAllText(@"E:\FFWS\Model\FF\Results\Results.csv", sb.ToString());
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("HD Model Results processing completed.");
            }
            catch (Exception error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("HD Model Results processing cannot be completed due to error. Press any key to exit. Error: " + error.Message);
                Console.ResetColor();
                Console.ReadKey();
                Environment.Exit(1);
            }
            ///----------------------------------------------------Flood Map Text files Processing Started--------------------------------------------------------
            try
            {

                try
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("Flood Map text files processing started....");
                    Console.ResetColor();

                    string[] resultInfo = File.ReadAllLines(@"E:\FFWS\Model\FF\Results\Results.csv");
                    DateTime[] dfsDate = new DateTime[resultInfo.Length - 1];
                    float[,] values = new float[6, 4803];

                    DateTime[] forecastDate = new DateTime[] { DateTime.Today.AddHours(6), DateTime.Today.AddDays(1).AddHours(6), DateTime.Today.AddDays(1).AddHours(6), DateTime.Today.AddDays(1).AddHours(6), DateTime.Today.AddDays(1).AddHours(6), DateTime.Today.AddDays(1).AddHours(6) };
                    for (int i = 0; i < resultInfo.Length - 1; i++)
                    {
                        var separatedText = resultInfo[i + 1].Split(',');
                        dfsDate[i] = DateTime.Parse(separatedText[0]);

                        if (dfsDate[i] == forecastDate[0])
                        {
                            for (int j = 1; j < separatedText.Length; j++)
                            {
                                values[0, j - 1] = float.Parse(separatedText[j]);
                            }
                        }

                        if (dfsDate[i] == forecastDate[1])
                        {
                            for (int j = 1; j < separatedText.Length; j++)
                            {
                                values[1, j - 1] = float.Parse(separatedText[j]);
                            }
                        }

                        if (dfsDate[i] == forecastDate[2])
                        {
                            for (int j = 1; j < separatedText.Length; j++)
                            {
                                values[2, j - 1] = float.Parse(separatedText[j]);
                            }
                        }

                        if (dfsDate[i] == forecastDate[3])
                        {
                            for (int j = 1; j < separatedText.Length; j++)
                            {
                                values[3, j - 1] = float.Parse(separatedText[j]);
                            }
                        }

                        if (dfsDate[i] == forecastDate[4])
                        {
                            for (int j = 1; j < separatedText.Length; j++)
                            {
                                values[4, j - 1] = float.Parse(separatedText[j]);
                            }
                        }

                        if (dfsDate[i] == forecastDate[5])
                        {
                            for (int j = 1; j < separatedText.Length; j++)
                            {
                                values[5, j - 1] = float.Parse(separatedText[j]);
                            }
                        }

                    }

                    StringBuilder sb = new StringBuilder();
                    for (int j = 0; j < 6; j++)
                    {
                        for (int i = 0; i < 4803; i++)
                        {
                            sb.AppendLine(values[j, i].ToString());
                        }
                        File.WriteAllText(@"E:\FFWS\Model\FF\GISTXT\Day" + j + ".txt", sb.ToString());
                        sb.Clear();
                    }
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Flood Map text files generated Suceesssfully.");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("Structure based forecast text files are processing....");

                    string[] StructureNodeInfo = File.ReadAllLines(@"E:\FFWS\Batch\StructureBased_NodeInfo.csv");
                    string[] nodeTitle = new string[StructureNodeInfo.Length];
                    int[] nodeItem = new int[StructureNodeInfo.Length];
                    for (int i = 0; i < StructureNodeInfo.Length; i++)
                    {
                        var separatedText = StructureNodeInfo[i].Split(',');
                        nodeTitle[i] = separatedText[0];
                        nodeItem[i] = int.Parse(separatedText[1]);
                    }
                    for (int j = 0; j < 6; j++)
                    {
                        for (int i = 0; i < nodeItem.Length; i++)
                        {
                            sb.Append(values[j, (nodeItem[i] - 1)] + ",");
                        }
                        sb.Append("\r\n");
                    }
                    File.WriteAllText(@"E:\FFWS\ModelOutput\StructureFF\Water_Profile.txt", sb.ToString());
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Structure based forecast text files generated Suceesssfully.");

                }
                catch (Exception error)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Structure based forecast text files cannot be generated due to an error. Error: " + error.Message);
                }
            }
            catch (Exception error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Flood Map Text files and Structure based forecast cannot be generated due to an error. Error: " + error.Message);
                Console.ReadKey();
            }

            ///-----------------------------------------------------------------------Observed and Forecast .csv Files Processing-----------------------------------------
            try
            {
                dt.Clear();
                dt.Columns.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Now observed and forecast text files are generating....");
                cmd = new SqlCommand("Select StationName from ForecastStation", con);
                con.Open();
                adapter.SelectCommand = cmd;
                adapter.Fill(dt);
                adapter.Dispose();
                cmd.Dispose();
                con.Close();

                List<string> stationName = new List<string>();
                foreach (DataRow dr in dt.Rows)
                {
                    stationName.Add(dr[0].ToString().Trim());
                }
                dt.Clear();
                List<string> webStation = new List<string>();
                List<string> webSerial = new List<string>();
                string[] stationSerialfile = File.ReadAllLines(@"E:\FFWS\Batch\Station_WebSerial.txt");
                foreach (string element in stationSerialfile)
                {
                    var separatedText = element.Split(',');
                    webStation.Add(separatedText[0]);
                    webSerial.Add(separatedText[1]);
                }
                StringBuilder sob = new StringBuilder();
                for (int a = 0; a < webStation.Count; a++)
                {
                    cmd = new SqlCommand("Select Date, WLValue FROM GBMStationWL Where Station = @station AND Date >= @startDate AND Date <= @endDate ORDER By Date ASC", con);
                    cmd.Parameters.AddWithValue("@station", webStation[a]);
                    cmd.Parameters.AddWithValue("@endDate", DateTime.Today.AddHours(6));
                    cmd.Parameters.AddWithValue("@startDate", DateTime.Today.AddHours(6).AddDays(-7));
                    con.Open();
                    adapter.SelectCommand = cmd;
                    adapter.Fill(ds, "Obstable");
                    adapter.Dispose();
                    cmd.Dispose();
                    con.Close();

                    List<string> obsDate = new List<string>();
                    List<string> obsWaterL = new List<string>();

                    foreach (DataRow dr in ds.Tables["Obstable"].Rows)
                    {
                        obsDate.Add(dr[0].ToString());
                        obsWaterL.Add(dr[1].ToString());
                    }
                    for (int k = 0; k < obsDate.Count; k++)
                    {
                        sob.AppendLine(webSerial[a] + "," + obsDate[k].ToString() + "," + obsWaterL[k]);
                    }

                }

                string[] itemStationFile = File.ReadAllLines(@"E:\FFWS\Batch\StationItemInfo.txt");
                string[] station = new string[itemStationFile.Length];
                string[] itemDFS0 = new string[itemStationFile.Length];

                for (int i = 0; i < itemStationFile.Length; i++)
                {
                    var sepText = itemStationFile[i].Split(',');
                    station[i] = sepText[0];
                    itemDFS0[i] = sepText[1];
                }

                string[] resultInfo = File.ReadAllLines(@"E:\FFWS\Model\FF\Results\Results.csv");
                DateTime[] dfsDate = new DateTime[resultInfo.Length - 1];
                List<string> forecastResult = new List<string>();


                for (int i = 0; i < resultInfo.Length - 1; i++)
                {
                    var separatedText = resultInfo[i + 1].Split(',');
                    dfsDate[i] = DateTime.Parse(separatedText[0]);
                    TimeSpan ts = dfsDate[i] - DateTime.Today.AddHours(6);
                    if (ts.TotalHours >= 0)
                    {
                        forecastResult.Add(resultInfo[i + 1]);

                    }
                }

                string[,] values = new string[forecastResult.Count, 4804];
                for (int i = 0; i < forecastResult.Count; i++)
                {
                    var separatedText = forecastResult[i].Split(',');
                    for (int j = 0; j < separatedText.Length; j++)
                    {
                        values[i, j] = separatedText[j];
                    }
                }
                forecastResult.Clear();
                StringBuilder sb = new StringBuilder();
                foreach (string element in stationName)
                {
                    string serial = "";
                    for (int i = 0; i < webStation.Count; i++)
                    {
                        if (webStation[i] == element)
                        {
                            serial = webSerial[i];
                        }
                    }
                    //-------------------------------------Obtaining Stations data from forecast--------------------------------------------------

                    List<float> Qvalues = new List<float>();
                    DateTime[] dataDate = new DateTime[121];
                    dataDate[0] = DateTime.Today.AddHours(6);
                    for (int i = 0; i < station.Length; i++)
                    {
                        if (element.Equals(station[i], StringComparison.InvariantCultureIgnoreCase))
                        {
                            for (int j = 0; j < 121; j++)
                            {
                                Qvalues.Add(float.Parse(values[j, int.Parse(itemDFS0[i])]));
                                dataDate[j] = DateTime.Parse(values[j, 0]);
                            }
                        }
                    }
                    dt.Clear();
                    dt.Columns.Clear();
                    //MessageBox.Show(Qvalues.Length.ToString());
                    cmd = new SqlCommand("Select WLValue FROM GBMStationWL Where Station = @station AND Date = @endDate", con);
                    cmd.Parameters.AddWithValue("@station", element);
                    cmd.Parameters.AddWithValue("@endDate", dataDate[0]);
                    con.Open();
                    adapter.SelectCommand = cmd;
                    adapter.Fill(dt);
                    adapter.Dispose();
                    cmd.Dispose();
                    con.Close();
                    float obsWL = 0;
                    try
                    {
                        obsWL = Convert.ToSingle(dt.Rows[0].ItemArray[0]);
                    }

                    catch (IndexOutOfRangeException)
                    {
                        obsWL = Qvalues[0];
                    }
                    float correction = Qvalues[0] - obsWL;
                    ds.Tables.Clear();

                    for (int j = 0; j < Qvalues.Count; j++)
                    {
                        Qvalues[j] = Qvalues[j] - correction;

                    }
                    for (int j = 0; j < Qvalues.Count; j++)
                    {
                        sb.AppendLine(serial + "," + dataDate[j].ToString("yyyy-MM-dd hh:mm") + "," + Qvalues[j]);
                    }

                    try
                    {
                        con.Open();
                        cmd = new SqlCommand("INSERT INTO ForecastedWL VALUES(@Date, @station, @1day, @2day, @3day, @4day, @5day)", con);
                        cmd.Parameters.AddWithValue("@date", DateTime.Today.AddHours(6));
                        cmd.Parameters.AddWithValue("@station", element);
                        cmd.Parameters.AddWithValue("@1day", Qvalues[24]);
                        cmd.Parameters.AddWithValue("@2day", Qvalues[48]);
                        cmd.Parameters.AddWithValue("@3day", Qvalues[72]);
                        cmd.Parameters.AddWithValue("@4day", Qvalues[96]);
                        cmd.Parameters.AddWithValue("@5day", Qvalues[120]);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        con.Close();
                    }
                    catch (SqlException)
                    {
                        con.Close();
                        con.Open();
                        cmd = new SqlCommand("UPDATE ForecastedWL SET Day1=@1day, Day2=@2day, Day3=@3day, Day4=@4day, Day5=@5day Where Date=@date AND StationName=@station", con);
                        cmd.Parameters.AddWithValue("@date", DateTime.Today.AddHours(6));
                        cmd.Parameters.AddWithValue("@station", element);
                        cmd.Parameters.AddWithValue("@1day", Qvalues[24]);
                        cmd.Parameters.AddWithValue("@2day", Qvalues[48]);
                        cmd.Parameters.AddWithValue("@3day", Qvalues[72]);
                        cmd.Parameters.AddWithValue("@4day", Qvalues[96]);
                        cmd.Parameters.AddWithValue("@5day", Qvalues[120]);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        con.Close();
                    }
                    Qvalues.Clear();
                }
                File.WriteAllText(@"E:\FFWS\Data\ForecastData\Forecast_" + DateTime.Today.ToString("yyyy-MM-dd") + ".csv", sb.ToString());
                File.WriteAllText(@"E:\FFWS\Data\ForecastData\Observed_" + DateTime.Today.ToString("yyyy-MM-dd") + ".csv", sob.ToString());
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Observed and forecast textfiles have been created successfully.");
            }
            catch (Exception error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Observed and forecast textfiles cannot created due to an error. Error: " + error.Message);
                Console.ReadKey();
            }

            ///--------------------------------------------------------------Flood Map-XYZ text Files Processing ------------------------------------------------------
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("GIS text files is processing....");

                StringBuilder floodmapText = new StringBuilder();
                string[] floodMapFile = File.ReadAllLines(@"E:\FFWS\Batch\FloodMap_XY.txt");
                string[] floodMapX = new string[floodMapFile.Length];
                string[] floodMapY = new string[floodMapFile.Length];
                for (int i = 0; i < floodMapFile.Length; i++)
                {
                    var separatedText = floodMapFile[i].Split(',');
                    floodMapX[i] = separatedText[0];
                    floodMapY[i] = separatedText[1];
                }
                for (int i = 0; i < 6; i++)
                {
                    floodmapText.AppendLine("Point");
                    string[] floodMapDayfile = File.ReadAllLines(@"E:\FFWS\Model\FF\GISTXT\Day" + i + ".txt");
                    string[] floodmapZ = new string[floodMapDayfile.Length];
                    for (int j = 0; j < floodMapDayfile.Length; j++)
                    {
                        floodmapZ[j] = floodMapDayfile[j];
                    }

                    for (int j = 0; j < floodMapX.Length; j++)
                    {
                        floodmapText.AppendLine((j + 1) + " " + floodMapX[j] + " " + floodMapY[j] + " " + floodmapZ[j] + " " + floodmapZ[j]);
                    }
                    floodmapText.AppendLine("END");
                    File.WriteAllText(@"E:\FloodMap\GIS\XYZ" + i + ".txt", floodmapText.ToString());
                    floodmapText.Clear();
                }
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Flooad Map text files have been created successfully.");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Python Script is initializing.....");
                Console.ResetColor();
                Process.Start(@"E:\FloodMap\Supper_model\Flood_Map.py");
            }
            catch (Exception error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Flood Map text files cannot be created due to an error. Error " + error.Message);
                Console.ReadKey();
            }

            ///------------------------------------------------------------Structure Based Forecast Preparation ----------------------------------------------
            try
            {
                dt.Columns.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Structure based forecast Images are processing....");

                Chart chartStructureFF = new Chart();
                chartStructureFF.ChartAreas.Add("chartArea");
                chartStructureFF.Legends.Add("legend");
                chartStructureFF.Titles.Add("chartTitle");
                chartStructureFF.Series.Add("Crest Level");
                chartStructureFF.Series.Add("Location");
                chartStructureFF.Series.Add("Today WL");
                chartStructureFF.Series.Add("Day1 WL");
                chartStructureFF.Series.Add("Day2 WL");
                chartStructureFF.Series.Add("Day3 WL");
                chartStructureFF.Series.Add("Day4 WL");
                chartStructureFF.Series.Add("Day5 WL");
                chartStructureFF.Height = 660;
                chartStructureFF.Width = 1200;

                chartStructureFF.Titles[0].Font = new System.Drawing.Font("Lucida Bright", 16, FontStyle.Bold);
                chartStructureFF.Titles[0].ForeColor = System.Drawing.Color.Green;
                chartStructureFF.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dot;
                chartStructureFF.ChartAreas[0].AxisX.Title = "Chainage (km)";
                chartStructureFF.ChartAreas[0].AxisX.TitleFont = new System.Drawing.Font("Lucida Bright", 11, FontStyle.Bold);
                chartStructureFF.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dot;
                chartStructureFF.ChartAreas[0].AxisY.TitleFont = new System.Drawing.Font("Lucida Bright", 11, FontStyle.Bold);
                chartStructureFF.ChartAreas[0].AxisY.Title = "Level (mPWD)";

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
                chartStructureFF.ChartAreas[0].AxisX.Minimum = 0.0;
                chartStructureFF.ChartAreas[0].AxisX.LabelStyle.Format = "0.0";
                chartStructureFF.ChartAreas[0].AxisY.MajorGrid.Interval = 2.0;
                chartStructureFF.ChartAreas[0].AxisY.MajorTickMark.Interval = 2.0;
                chartStructureFF.ChartAreas[0].AxisY.LabelStyle.Interval = 2.0;
                chartStructureFF.ChartAreas[0].AxisY.LabelStyle.Format = "0.0";

                chartStructureFF.Series[0].ChartType = SeriesChartType.Line;  // Set chart type like Bar chart, Pie chart
                chartStructureFF.Series[0].Color = System.Drawing.Color.Black;
                chartStructureFF.Series[0].IsValueShownAsLabel = false;
                chartStructureFF.Series[0].BorderWidth = 1;

                chartStructureFF.Series[1].ChartType = SeriesChartType.Point;
                chartStructureFF.Series[1].IsVisibleInLegend = false;
                chartStructureFF.Series[1].LabelAngle = -90;
                chartStructureFF.Series[1].LabelForeColor = System.Drawing.Color.Blue;
                chartStructureFF.Series[1].Font = new System.Drawing.Font("Lucida Bright", 12, FontStyle.Bold);
                chartStructureFF.Series[1].SmartLabelStyle.Enabled = false;

                chartStructureFF.Series[2].ChartType = SeriesChartType.Line;  // Set chart type like Bar chart, Pie chart
                chartStructureFF.Series[2].IsValueShownAsLabel = false;
                chartStructureFF.Series[2].BorderWidth = 2;

                chartStructureFF.Series[3].ChartType = SeriesChartType.Line;  // Set chart type like Bar chart, Pie chart
                chartStructureFF.Series[3].IsValueShownAsLabel = false;
                chartStructureFF.Series[3].BorderWidth = 2;

                chartStructureFF.Series[4].ChartType = SeriesChartType.Line;  // Set chart type like Bar chart, Pie chart
                chartStructureFF.Series[4].IsValueShownAsLabel = false;
                chartStructureFF.Series[4].BorderWidth = 2;

                chartStructureFF.Series[5].ChartType = SeriesChartType.Line;  // Set chart type like Bar chart, Pie chart
                chartStructureFF.Series[5].IsValueShownAsLabel = false;
                chartStructureFF.Series[5].BorderWidth = 2;

                chartStructureFF.Series[6].ChartType = SeriesChartType.Line;  // Set chart type like Bar chart, Pie chart
                chartStructureFF.Series[6].IsValueShownAsLabel = false;
                chartStructureFF.Series[6].BorderWidth = 2;

                chartStructureFF.Series[7].ChartType = SeriesChartType.Line;  // Set chart type like Bar chart, Pie chart
                chartStructureFF.Series[7].IsValueShownAsLabel = false;
                chartStructureFF.Series[7].BorderWidth = 2;

                chartStructureFF.Legends[0].Position = new ElementPosition(15.0f, 80.0f, 75, 8);
                chartStructureFF.Legends[0].BackColor = System.Drawing.Color.Transparent;
                chartStructureFF.Legends[0].Font = new System.Drawing.Font("Lucida Bright", 11, FontStyle.Regular);
                chartStructureFF.Legends[0].BorderColor = System.Drawing.Color.Brown;
                chartStructureFF.Legends[0].BorderDashStyle = ChartDashStyle.DashDot;
                try
                {
                    cmd = new SqlCommand("Select Chainage, CrestLevel from CLDhakaMawa", con);
                    con.Open();
                    adapter.SelectCommand = cmd;
                    adapter.Fill(dt);
                    adapter.Dispose();
                    cmd.Dispose();
                    con.Close();

                    chartStructureFF.Titles[0].Text = @"Structure Based Forecast\nWater level Profile Forecast Along Dhaka-Mawa Road\nForecast Date= " + DateTime.Today.ToString("yyyy-MM-dd");
                    foreach (DataRow dr in dt.Rows)
                    {
                        chartStructureFF.Series[0].Points.AddXY(dr[0], dr[1]);
                    }
                    dt.Clear();

                    string[] locationName = new string[] { "Babu-Bazar", "Dhaleswari bridge", "Nimtoli bazar", "Srinagar", "Mawa" };
                    float[] locationPosition = new float[] { 0f, 10.92f, 14.69f, 25.82f, 32.0f };
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
                    chartStructureFF.ChartAreas[0].AxisX.LabelStyle.Interval = 5.0;
                    chartStructureFF.ChartAreas[0].AxisX.MajorTickMark.Interval = 5.0;

                    chartStructureFF.ChartAreas[0].AxisY.Minimum = 2.0;
                    chartStructureFF.ChartAreas[0].AxisY.Maximum = 14.0;
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
                    adapter.Fill(dt);
                    adapter.Dispose();
                    cmd.Dispose();
                    con.Close();

                    chartStructureFF.Titles[0].Text = @"Structure Based Forecast\nWater level Profile Forecast Along Jamuna Right Bank\nForecast Date= " + DateTime.Today.ToString("yyyy-MM-dd");
                    foreach (DataRow dr in dt.Rows)
                    {
                        chartStructureFF.Series[0].Points.AddXY(dr[0], dr[1]);
                    }
                    dt.Clear();

                    string[] locationName = new string[] { "Gaibandha", "Sariakandi", "Kazipur", "Sirajganj Gauge", "Jamuna Bridge", "Belkuchi", "Enayetpur" };
                    float[] locationPosition = new float[] { 27.05559457f, 83.32890404f, 118.3479595f, 144.5735253f, 154.9055796f, 163.8322879f, 170.9565421f };
                    for (int i = 0; i < locationName.Length; i++)
                    {
                        chartStructureFF.Series[1].Points.AddXY(locationPosition[i], 25.0 - i);
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
                    chartStructureFF.ChartAreas[0].AxisY.Maximum = 31.0;
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
                    adapter.Fill(dt);
                    adapter.Dispose();
                    cmd.Dispose();
                    con.Close();

                    chartStructureFF.Titles[0].Text = @"Structure Based Forecast\nWater level Profile Forecast Along Meghna Dhonagoda Embankment\nForecast Date= " + DateTime.Today.ToString("yyyy-MM-dd");
                    foreach (DataRow dr in dt.Rows)
                    {
                        chartStructureFF.Series[0].Points.AddXY(dr[0], dr[1]);
                    }
                    dt.Clear();

                    string[] locationName = new string[] { "Kalipur Pump Station", "Eklaspur Pump Station", "Udamdi Pump Station", "Matlab Gauge Station" };
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
                    adapter.Fill(dt);
                    adapter.Dispose();
                    cmd.Dispose();
                    con.Close();

                    chartStructureFF.Titles[0].Text = @"Structure Based Forecast\nWater level Profile Forecast Along PIRDP Embankment\nForecast Date= " + DateTime.Today.ToString("yyyy-MM-dd");
                    foreach (DataRow dr in dt.Rows)
                    {
                        chartStructureFF.Series[0].Points.AddXY(dr[0], dr[1]);
                    }
                    dt.Clear();

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

                ProcessStartInfo start = new ProcessStartInfo();
                Process exeProcess = new Process();
                start.FileName = @"E:\FFWS\Programs\StrFFUpload.py";
                start.CreateNoWindow = false;
                exeProcess = Process.Start(start);
                exeProcess.WaitForExit();

            }
            catch (Exception error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("StructureFF-Images  cannot be created due to an error. Error " + error.Message);
                Console.ReadKey();
            }


            ////------------------------------------ Generation of Forecast Hydrographs ----------------------------------------------------------------
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Forecast Hydrographs are processing....");
                dt.Columns.Clear();
                dt.Rows.Clear();
                Chart chartStationForecast = new Chart();
                chartStationForecast.ChartAreas.Add("chartArea");
                chartStationForecast.Legends.Add("legend");
                chartStationForecast.Titles.Add("chartTitle");
                chartStationForecast.Series.Add("Observed WL");
                chartStationForecast.Series.Add("forecast WL");
                chartStationForecast.Series.Add("Danger Level");
                chartStationForecast.Series.Add("RHWL");

                chartStationForecast.Height = 600;
                chartStationForecast.Width = 1200;
                chartStationForecast.Titles[0].Font = new System.Drawing.Font("Lucida Bright", 16, FontStyle.Bold);
                chartStationForecast.Titles[0].ForeColor = System.Drawing.Color.Green;
                chartStationForecast.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dot;
                chartStationForecast.ChartAreas[0].AxisX.TitleFont = new System.Drawing.Font("Lucida Bright", 11, FontStyle.Bold);
                chartStationForecast.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dot;
                chartStationForecast.ChartAreas[0].AxisY.TitleFont = new System.Drawing.Font("Lucida Bright", 11, FontStyle.Bold);
                chartStationForecast.ChartAreas[0].AxisY.Title = "Water Level (mPWD)";
                chartStationForecast.Legends[0].Position = new ElementPosition(15.0f, 80.0f, 75, 8);
                chartStationForecast.Legends[0].BackColor = System.Drawing.Color.Transparent;
                chartStationForecast.Legends[0].Font = new System.Drawing.Font("Lucida Bright", 11, FontStyle.Regular);
                chartStationForecast.Legends[0].BorderColor = System.Drawing.Color.Brown;
                chartStationForecast.Legends[0].BorderDashStyle = ChartDashStyle.DashDot;

                con.Open();
                cmd = new SqlCommand("SELECT StationName, RiverName, DangerLevel, RHWL from ForecastStation", con);
                adapter.SelectCommand = cmd;
                adapter.Fill(dt);
                adapter.Dispose();
                cmd.Dispose();
                con.Close();
                List<string> stationName = new List<string>();
                List<string> riverName = new List<string>();
                List<float> dangerLevel = new List<float>();
                List<float> rhWLevel = new List<float>();

                foreach (DataRow dr in dt.Rows)
                {
                    stationName.Add(dr[0].ToString());
                    riverName.Add(dr[1].ToString());
                    dangerLevel.Add(float.Parse(dr[2].ToString()));
                    rhWLevel.Add(float.Parse(dr[3].ToString()));
                }
                dt.Clear();
                dt.Columns.Clear();
                dt.Rows.Clear();

                for (int k = 0; k < stationName.Count; k++)
                {
                    string element = stationName[k].Trim();
                    DateTime[] foreDate = new DateTime[5];
                    float[] foreWL = new float[5];
                    con.Open();
                    cmd = new SqlCommand("Select Day1, Day2, Day3, Day4, Day5 from ForecastedWL Where StationName = @station AND Date = @date", con);
                    cmd.Parameters.AddWithValue("@station", element);
                    cmd.Parameters.AddWithValue("@date", DateTime.Today.AddHours(6));
                    adapter.SelectCommand = cmd;
                    adapter.Fill(dt);
                    adapter.Dispose();
                    cmd.Dispose();
                    con.Close();
                    for (int i = 0; i < 5; i++)
                    {
                        foreWL[i] = (float.Parse(dt.Rows[0].ItemArray[i].ToString()));
                        foreDate[i] = DateTime.Today.AddHours(6).AddDays(i + 1);
                    }
                    dt.Clear();
                    dt.Columns.Clear();
                    dt.Rows.Clear();

                    con.Open();
                    cmd = new SqlCommand("Select Date, WLValue from GBMStationWL Where Station = @station AND Date>= @startDate AND Date<=@endDate", con);
                    cmd.Parameters.AddWithValue("@startDate", DateTime.Today.AddDays(-7).AddHours(6));
                    cmd.Parameters.AddWithValue("@endDate", DateTime.Today.AddHours(6));
                    cmd.Parameters.AddWithValue("@station", element);
                    adapter.SelectCommand = cmd;
                    adapter.Fill(dt);
                    adapter.Dispose();
                    cmd.Dispose();
                    con.Close();

                    List<DateTime> obsDate = new List<DateTime>();
                    List<float> obsWL = new List<float>();
                    foreach (DataRow dr in dt.Rows)
                    {
                        obsDate.Add(Convert.ToDateTime(dr[0]));
                        obsWL.Add(Convert.ToSingle(dr[1]));
                    }
                    dt.Clear();
                    dt.Columns.Clear();
                    dt.Rows.Clear();

                    chartStationForecast.Series[0].Points.Clear();
                    chartStationForecast.Series[1].Points.Clear();
                    chartStationForecast.Series[2].Points.Clear();
                    chartStationForecast.Series[3].Points.Clear();

                    chartStationForecast.Titles[0].Text = "5 Day Forecast" + "\r\n" + "Station Name: " + element + "  River Name: " + riverName[k] + "\r\n" + "Forecast Date: " + DateTime.Today.AddHours(6);

                    List<DateTime> chartDate = new List<DateTime>();
                    List<float> chartWL = new List<float>();

                    for (int i = 0; i < obsDate.Count; i++)
                    {
                        chartDate.Add(obsDate[i]);
                        chartWL.Add(obsWL[i]);
                        chartStationForecast.Series[0].Points.AddXY(obsDate[i], obsWL[i]);
                    }
                    if (obsDate.Count > 0)
                    {
                        chartStationForecast.Series[1].Points.AddXY(obsDate[obsDate.Count - 1], (obsWL[obsWL.Count - 1]));
                    }
                    else
                    {
                        chartStationForecast.Series[1].Points.AddXY(DateTime.Today.AddHours(6), (foreWL[0]));
                    }
                    for (int i = 0; i < 5; i++)
                    {
                        chartDate.Add(foreDate[i]);
                        chartWL.Add(foreWL[i]);
                        chartStationForecast.Series[1].Points.AddXY(foreDate[i], (foreWL[i]));
                    }

                    for (int i = 0; i < 13; i++)
                    {
                        chartStationForecast.Series[2].Points.AddXY(DateTime.Today.AddHours(6).AddDays(-7).AddDays(i), dangerLevel[k]);
                        chartStationForecast.Series[3].Points.AddXY(DateTime.Today.AddHours(6).AddDays(-7).AddDays(i), rhWLevel[k]);
                    }

                    chartWL.Add(dangerLevel[k]);
                    chartWL.Add(rhWLevel[k]);

                    chartStationForecast.Series[0].ChartType = SeriesChartType.Spline;  // Set chart type like Bar chart, Pie chart
                    chartStationForecast.Series[0].IsValueShownAsLabel = false;
                    chartStationForecast.Series[0].BorderWidth = 2;
                    chartStationForecast.Series[0].Color = Color.Blue;
                    chartStationForecast.Series[0].MarkerSize = 6;
                    chartStationForecast.Series[0].MarkerStyle = MarkerStyle.Circle;
                    chartStationForecast.Series[0].MarkerColor = Color.Black;

                    chartStationForecast.Series[1].ChartType = SeriesChartType.Spline;
                    chartStationForecast.Series[1].IsValueShownAsLabel = false;
                    chartStationForecast.Series[1].Color = Color.Red;
                    chartStationForecast.Series[1].BorderWidth = 2;
                    chartStationForecast.Series[1].MarkerSize = 6;
                    chartStationForecast.Series[1].MarkerStyle = MarkerStyle.Circle;
                    chartStationForecast.Series[1].MarkerColor = Color.Black;


                    chartStationForecast.Series[2].ChartType = SeriesChartType.Line;  // Set chart type like Bar chart, Pie chart
                    chartStationForecast.Series[2].IsValueShownAsLabel = false;
                    chartStationForecast.Series[2].BorderWidth = 3;
                    chartStationForecast.Series[2].Color = Color.DarkRed;

                    chartStationForecast.Series[3].ChartType = SeriesChartType.Line;  // Set chart type like Bar chart, Pie chart
                    chartStationForecast.Series[3].IsValueShownAsLabel = false;
                    chartStationForecast.Series[3].BorderWidth = 2;
                    chartStationForecast.Series[3].Color = Color.Black;

                    chartStationForecast.ChartAreas[0].AxisX.Maximum = Convert.ToDouble(DateTime.Today.AddHours(6).AddDays(5).ToOADate());
                    chartStationForecast.ChartAreas[0].AxisX.Minimum = Convert.ToDouble(DateTime.Today.AddHours(6).AddDays(-7).ToOADate());
                    chartStationForecast.ChartAreas[0].AxisX.MajorGrid.Interval = 1.0;
                    chartStationForecast.ChartAreas[0].AxisX.MajorTickMark.Interval = 2.0;
                    chartStationForecast.ChartAreas[0].AxisX.LabelStyle.Format = "yyyy-MM-dd hh:mm:ss";
                    chartStationForecast.ChartAreas[0].AxisY.Maximum = Math.Ceiling(chartWL.Max());
                    chartStationForecast.ChartAreas[0].AxisY.Minimum = Math.Floor(chartWL.Min());

                    chartStationForecast.ChartAreas[0].AxisY.MajorTickMark.Interval = (Math.Ceiling(chartWL.Max()) - Math.Floor(chartWL.Min())) / 5.0;
                    chartStationForecast.ChartAreas[0].AxisY.LabelStyle.Format = "0.0";
                    chartStationForecast.SaveImage(@"E:\FFWS\ModelOutput\ForecastJpg\" + element + ".png", ChartImageFormat.Png);
                    chartDate.Clear();
                    chartWL.Clear();
                    obsWL.Clear();
                    obsDate.Clear();
                }
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Forecast Hydrographs are genereated successfully.");
            }
            catch (Exception error)
            {
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Forecast Hydrograph cannot be created due to an error. Error: " + error.Message);
                Console.ReadKey();
            }
        }
    }
}
           