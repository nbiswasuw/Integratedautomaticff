using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using DHI.Generic.MikeZero.DFS;
using DHI.Generic.MikeZero;
using Microsoft.Research.Science.Data;
using Microsoft.Research.Science.Data.Imperative;
using System.Diagnostics;
using System.Net;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Drawing;


namespace AutomaticFF
{
    class Program
    {

        static void Main(string[] args)
        {
            ///-------------------------------------------------Initializing Global Parameters---------------------------------------------------//////
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.BackgroundColor = ConsoleColor.White;
            Console.WriteLine("Automated Flood Forecasting System of FMG, IWM.");
            
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("------@@@@ Model Simulation Module @@@-------");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Parameters is initiating.....");
            SqlConnection con = new SqlConnection(@"Data Source=NKB-PC\SQLEXPRESS;AttachDbFilename=E:\FFWS\Database\GBMStationRF.mdf;Integrated Security=True;User Instance=True");
            SqlCommand cmd = new SqlCommand();
            System.Data.DataSet ds = new System.Data.DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            //StringBuilder logText = new StringBuilder();
            DateTime startDate = new DateTime(2014, 01, 01, 09, 00, 00);
            DateTime endDate = DateTime.Today.AddHours(9);
            DataTable dt = new DataTable();
            
             ///----------------------------------------------------Creating Realtime Rainfall Dfs0 for NAM Model-------------------------------------////
             try
             {
                 Console.ForegroundColor = ConsoleColor.Cyan;
                 Console.WriteLine("Realtime rainfall time series files writing....");
                 cmd = new SqlCommand("Select DISTINCT StationName from StationLocation", con);
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

                 foreach (string element in stationName)
                 {
                     cmd = new SqlCommand("Select Date, RFValue from GBMStationRF Where StationName= @stationName AND Date> @startdate AND Date<= @enddate ORDER by Date ASC", con);
                     cmd.Parameters.AddWithValue("@stationName", element);
                     cmd.Parameters.AddWithValue("@startdate", startDate);
                     cmd.Parameters.AddWithValue("@enddate", endDate);
                     int noOfDays = (int)(endDate - startDate).TotalDays;
                     con.Open();
                     adapter.SelectCommand = cmd;
                     adapter.Fill(dt);
                     adapter.Dispose();
                     cmd.Dispose();
                     con.Close();
                     cmd.Parameters.Clear();
                     float[] values = new float[dt.Rows.Count];
                     int x = 0;
                     DateTime[] intervaldate = new DateTime[dt.Rows.Count];
                     foreach (DataRow dr in dt.Rows)
                     {
                         intervaldate[x] = Convert.ToDateTime(dr[1]);
                         try
                         {
                             values[x] = Convert.ToSingle(dr[2].ToString().Trim());
                         }
                         catch (FormatException)
                         {
                             values[x] = -1e-25f;
                         }
                         x++;
                     }

                     DateTime dfsDate = new DateTime();
                     try
                     {
                         dfsDate = DateTime.Parse(dt.Rows[0].ItemArray[1].ToString());
                     }
                     catch (IndexOutOfRangeException)
                     {
                         dfsDate = startDate;
                     }
                     DateTime firstDate = startDate.AddHours(-9).AddHours(dfsDate.Hour);
                     DateTime everyDate = firstDate;
                     dt.Clear();

                     DfsFactory factory = new DfsFactory();
                     string filename = @"E:\FFWS\Model\NAM\RF-DFS0\" + element.Trim() + ".dfs0";
                     DfsBuilder filecreator = DfsBuilder.Create(element, element, 2012);
                     filecreator.SetDataType(1);
                     filecreator.SetGeographicalProjection(factory.CreateProjectionUndefined());
                     filecreator.SetTemporalAxis(factory.CreateTemporalNonEqCalendarAxis(eumUnit.eumUsec, new DateTime(dfsDate.Year, dfsDate.Month, dfsDate.Day, dfsDate.Hour, dfsDate.Minute, dfsDate.Second)));
                     filecreator.SetItemStatisticsType(StatType.RegularStat);
                     DfsDynamicItemBuilder item = filecreator.CreateDynamicItemBuilder();
                     item.Set(element, eumQuantity.Create(eumItem.eumIRainfall, eumUnit.eumUmillimeter), DfsSimpleType.Float);
                     item.SetValueType(DataValueType.StepAccumulated);
                     item.SetAxis(factory.CreateAxisEqD0());
                     item.SetReferenceCoordinates(1f, 2f, 3f);
                     filecreator.AddDynamicItem(item.GetDynamicItemInfo());

                     filecreator.CreateFile(filename);
                     IDfsFile file = filecreator.GetFile();
                     IDfsFileInfo fileinfo = file.FileInfo;
                     fileinfo.DeleteValueFloat = -1e-25f;

                     for (int j = 0; j <= noOfDays; j++)
                     {
                         float ff = -1e-25f;
                         for (int i = 0; i < values.Length; i++)
                         {
                             if (everyDate.Date == intervaldate[i].Date)
                             {
                                 ff = values[i];
                             }
                         }
                         if (j == 0 && ff != -1e-25f)
                         {
                             ff = -1e-25f;
                         }
                         if (j == 1 && ff == -1e-25f)
                         {
                             ff = 0;
                         }
                         if (j == noOfDays && ff == -1e-25f)
                         {
                             ff = 0;
                         }
                         file.WriteItemTimeStepNext((everyDate - dfsDate).TotalSeconds, new float[] { ff });
                         everyDate = everyDate.AddDays(1);
                     }
                     file.Close();

                 }

                 File.Copy(@"E:\FFWS\Model\NAM\RF-DFS0\N\LAKHIMPUR.dfs0", @"E:\FFWS\Model\NAM\RF-DFS0\N-LAKHIMPUR.dfs0", true);
                 Console.ForegroundColor = ConsoleColor.Green;
                 Console.WriteLine(stationName.Count.ToString() + " station's Rainfall data created successfully.");
             }
             catch (Exception error)
             {
                 Console.ForegroundColor = ConsoleColor.Red;
                 Console.WriteLine("Realtime DFS0 files for NAM Model cannot created for an error. Error: " + error.Message);
                 Console.ReadKey();
                 Environment.Exit(1);
             }
            
             ///---------------------------------------------------------------Creating WRF DFS0 Files for NAM Model----------------------------------------////
             try
             {
                 Console.ForegroundColor = ConsoleColor.Cyan;
                 Console.WriteLine("WRF-DFS0 Data processing started....");
                 float[, ,] gridR = new float[6, 159, 159];
                 var dataset = Microsoft.Research.Science.Data.DataSet.Open(@"E:\Rain_MAP\WRF\WRFOut\wrfout_d01.nc?openMode=readOnly");
                 float[, ,] Xlong = dataset.GetData<float[, ,]>("XLONG");
                 float[, ,] Xlat = dataset.GetData<float[, ,]>("XLAT");

                 float[, ,] rain = dataset.GetData<float[, ,]>("RAINC");
                 float[, ,] rainnc = dataset.GetData<float[, ,]>("RAINNC");
                 string startDateText = dataset.GetAttr(0, "START_DATE").ToString();
                
                 for (int k = 0; k < 6; k++)
                 {
                     for (int i = 0; i < 159; i++)
                     {
                         for (int j = 0; j < 159; j++)
                         {
                             gridR[k, i, j] = rain[(k + 1) * 4, i, j] + rainnc[(k + 1) * 4, i, j];
                         }
                     }
                 }

                 string[] gsMapPointInfo = File.ReadAllLines(@"E:\FFWS\Batch\GBM_WRF.txt");
                 string[] catchment = new string[gsMapPointInfo.Length];
                 int[] gridI = new int[gsMapPointInfo.Length];
                 int[] gridJ = new int[gsMapPointInfo.Length];

                 for (int i = 0; i < gsMapPointInfo.Length; i++)
                 {
                     var parsed = gsMapPointInfo[i].Split(',');
                     catchment[i] = parsed[0];
                     gridI[i] = int.Parse(parsed[1]);
                     gridJ[i] = int.Parse(parsed[2]);
                 }
                 string[] catchmentName = catchment.Distinct().ToArray();
                 StringBuilder sb = new StringBuilder();

                 foreach (string element in catchmentName)
                 {
                     DateTime today = DateTime.Parse(startDateText.Substring(0, 10) + " " + startDateText.Substring(11, 8)).AddHours(6);
                     float[] catchrain = { -1e-25f, 0, 0, 0, 0, 0, 0 };
                     int count = 0;
                     for (int i = 0; i < catchment.Length; i++)
                     {
                         if (element == catchment[i])
                         {
                             count = count + 1;
                             catchrain[1] = catchrain[1] + gridR[0, gridI[i], gridJ[i]];
                             catchrain[2] = catchrain[2] + gridR[1, gridI[i], gridJ[i]] - gridR[0, gridI[i], gridJ[i]];
                             catchrain[3] = catchrain[3] + gridR[2, gridI[i], gridJ[i]] - gridR[1, gridI[i], gridJ[i]];
                             catchrain[4] = catchrain[4] + gridR[3, gridI[i], gridJ[i]] - gridR[2, gridI[i], gridJ[i]];
                             catchrain[5] = catchrain[5] + gridR[4, gridI[i], gridJ[i]] - gridR[3, gridI[i], gridJ[i]];
                             catchrain[6] = catchrain[6] + gridR[5, gridI[i], gridJ[i]] - gridR[4, gridI[i], gridJ[i]];
                         }
                     }
                     for (int i = 1; i < 7; i++)
                     {
                         catchrain[i] = catchrain[i] / count;
                     }

                     DateTime dfsDate = today;
                     DfsFactory factory = new DfsFactory();
                     string filename = @"E:\FFWS\Model\NAM\WRF-DFS0\" + element + ".dfs0";
                     DfsBuilder filecreator = DfsBuilder.Create(element, element, 2012);
                     filecreator.SetDataType(1);
                     filecreator.SetGeographicalProjection(factory.CreateProjectionUndefined());

                     filecreator.SetTemporalAxis(factory.CreateTemporalNonEqCalendarAxis(eumUnit.eumUsec, new DateTime(dfsDate.Year, dfsDate.Month, dfsDate.Day, dfsDate.Hour, dfsDate.Minute, dfsDate.Second)));
                     filecreator.SetItemStatisticsType(StatType.RegularStat);
                     DfsDynamicItemBuilder item = filecreator.CreateDynamicItemBuilder();
                     item.Set(element, eumQuantity.Create(eumItem.eumIRainfall, eumUnit.eumUmillimeter), DfsSimpleType.Float);
                     item.SetValueType(DataValueType.StepAccumulated);
                     item.SetAxis(factory.CreateAxisEqD0());
                     item.SetReferenceCoordinates(1f, 2f, 3f);
                     filecreator.AddDynamicItem(item.GetDynamicItemInfo());

                     filecreator.CreateFile(filename);
                     IDfsFile file = filecreator.GetFile();
                     IDfsFileInfo fileinfo = file.FileInfo;
                     fileinfo.DeleteValueFloat = -1e-25f;

                     for (int i = 0; i < 7; i++)
                     {
                         file.WriteItemTimeStepNext((today - dfsDate).TotalSeconds, new float[] { catchrain[i] });
                         today = today.AddDays(1);
                     }
                     file.Close();
                 }
                 Console.ForegroundColor = ConsoleColor.Green;
                 Console.WriteLine(catchmentName.Length + " station's WRF Rainfall data created successfully.");
             }
             catch (Exception error)
             {
                 Console.ForegroundColor = ConsoleColor.Red;
                 Console.WriteLine("WRF DFS0 cannot created for an error. Error: " + error.Message);
                 Console.ReadKey();
                 Environment.Exit(1);
             }

             ///-------------------------------------------------------Changing NAM Model's .sim11 file's text----------------------------------------------/// 
             try
             {
                 Console.ForegroundColor = ConsoleColor.Cyan;
                 Console.WriteLine("NAM Model simulation initiating....");
                 DateTime today = DateTime.Now;
                 string dateval = "         end = " + today.Year + ", " + today.Month + ", " + today.Day + ", 6, 0, 0";
                 List<string> alllines = new List<string>();
                 FileStream filepath = new FileStream(@"E:\FFWS\Model\NAM\NAM.sim11", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                 string path = @"E:\FFWS\Model\NAM\NAM.sim11";
                 var reader = new StreamReader(filepath);
                 while (!reader.EndOfStream)
                 {
                     var line = reader.ReadLine();
                     alllines.Add(line);
                 }
                 filepath.Close();
                 filepath.Dispose();
                 alllines[39] = dateval;
                 File.WriteAllLines(path, alllines);
             }
             catch (Exception error)
             {
                 Console.ForegroundColor = ConsoleColor.Red;
                 Console.WriteLine("Model simulation cannot started due to an error. Error: " + error.Message);
                 Console.ReadKey();
                 Environment.Exit(1);
             }
           
             ///------------------------------------------------NAM Model Simulation File--------------------------------------------------------------///
             try
             {
                 Console.ForegroundColor = ConsoleColor.Cyan;
                 Console.WriteLine("NAM Model simulation started.");
                 ProcessStartInfo start = new ProcessStartInfo();
                 start.FileName = @"C:\Program Files\DHI\2014\bin\mike11.exe";
                 start.Arguments = @"E:\FFWS\Model\NAM\NAM.sim11";
                 Process exeProcess = Process.Start(start);
                 exeProcess.WaitForExit();
                 Console.ForegroundColor = ConsoleColor.Green;
                 Console.WriteLine("NAM Model Simulation Completed.");

             }
             catch (Exception error)
             {
                 Console.ForegroundColor = ConsoleColor.Red;
                 Console.WriteLine("NAM Model Simulation have not simulated due to an error. Error: " + error.Message);
                 Console.ReadKey();
                 Environment.Exit(1);
             }
            
             /// -----------------------------------------------------NAM Model Result Processing---------------------------------------------------------///
             try
             {
                 Console.ForegroundColor = ConsoleColor.Cyan;
                 Console.WriteLine("NAM Result Processing Started.");
                 
                 IDfsFile resFile = DfsFileFactory.DfsGenericOpenEdit(@"E:\FFWS\Model\NAM\Result\NAM-edited.res11");
                 IDfsFileInfo resfileInfo = resFile.FileInfo;
                 int noTimeSteps = resfileInfo.TimeAxis.NumberOfTimeSteps;
                 DateTime[] date = resFile.FileInfo.TimeAxis.GetDateTimes();
                 DateTime sDate = date[0];
                 List<DateTime> dfsDate = new List<DateTime>();
                 for (int i = 0; i < noTimeSteps; i++)
                 {
                     dfsDate.Add(sDate.AddHours(resFile.ReadItemTimeStep(1, i).Time));
                 }
                 int itemCounter = 0;
                 List<string> runoffItems = new List<string>();
                 List<int> ItemIndex = new List<int>();
                 for (int j = 0; j < resFile.ItemInfo.Count; j++)
                 {
                     if (resFile.ItemInfo[j].Quantity.ItemDescription == "Discharge")
                     {
                         runoffItems.Add(resFile.ItemInfo[j].Name);
                         itemCounter = itemCounter + 1;
                         ItemIndex.Add(j);
                     }
                 }
                 IDfsItemData<float> data;
                 float[,] values = new float[noTimeSteps, itemCounter];
                 for (int i = 0; i < noTimeSteps; i++)
                 {
                     for (int j = 0; j < itemCounter; j++)
                     { 
                         data = (IDfsItemData<float>)resFile.ReadItemTimeStep(ItemIndex[j] + 1, i);
                         values[i, j] = Convert.ToSingle(data.Data[0]);
                     }
                 }

                 for (int j = 0; j < runoffItems.Count; j++)
                 {
                     string filename = @"E:\FFWS\Model\NAM\Result\Output\" + runoffItems[j] + ".dfs0";
                     DfsFactory factory = new DfsFactory();
                     DfsBuilder filecreator = DfsBuilder.Create(runoffItems[j], runoffItems[j], 2012);
                     filecreator.SetDataType(1);
                     filecreator.SetGeographicalProjection(factory.CreateProjectionUndefined());
                     filecreator.SetTemporalAxis(factory.CreateTemporalNonEqCalendarAxis(eumUnit.eumUsec, new DateTime(sDate.Year, sDate.Month, sDate.Day, sDate.Hour, sDate.Minute, sDate.Second)));
                     filecreator.SetItemStatisticsType(StatType.RegularStat);
                     DfsDynamicItemBuilder item = filecreator.CreateDynamicItemBuilder();
                     item.Set("Discharge item", eumQuantity.Create(eumItem.eumIDischarge, eumUnit.eumUm3PerSec), DfsSimpleType.Float);
                     item.SetValueType(DataValueType.Instantaneous);
                     item.SetAxis(factory.CreateAxisEqD0());
                     item.SetReferenceCoordinates(1f, 2f, 3f);
                     filecreator.AddDynamicItem(item.GetDynamicItemInfo());

                     filecreator.CreateFile(filename);
                     IDfsFile file = filecreator.GetFile();
                     for (int i = 0; i < noTimeSteps; i++)
                     {
                         file.WriteItemTimeStepNext((dfsDate[i]-dfsDate[0]).TotalSeconds, new float[] { values[i, j] });
                     }
                     file.Close();

                 }

                 Console.ForegroundColor = ConsoleColor.Green;
                 Console.WriteLine("NAM Result processing completed.");
             }
             catch (Exception error)
             {
                 Console.ForegroundColor = ConsoleColor.Red;
                 Console.WriteLine("NAM Result cannot be processed due to an error. Error: " + error.Message);
                 Console.ReadKey();
                 Environment.Exit(1);
             }
            
             ///------------------------------------------------------------MIKEHydro Model Simulation -------------------------------------------------------------///
             try
             {
                 Console.ForegroundColor = ConsoleColor.Cyan;
                 Console.WriteLine("MikeHydro Module is initiating...");
                 DateTime hydrostartDate = new DateTime(2014, 01, 02, 06, 00, 00);
                 DateTime hydroendDate = DateTime.Today.AddDays(6).AddHours(6);
                 string[] filepath = File.ReadAllLines(@"E:\FFWS\Model\MIKEHYDRO\GBM_MIKEHYDRO.mhydro");
                 filepath[54] = "         StartTime = '" + hydrostartDate.Year.ToString("0000") + " " + hydrostartDate.Month.ToString("00") + " " + hydrostartDate.Day.ToString("00") + " 06:00:00'";
                 filepath[55] = "         EndTime = '" + hydroendDate.Year.ToString("0000") + " " + hydroendDate.Month.ToString("00") + " " + hydroendDate.Day.ToString("00") + " 06:00:00'";
                 File.WriteAllLines(@"E:\FFWS\Model\MIKEHYDRO\GBM_MIKEHYDRO.mhydro", filepath);
                 Console.WriteLine("MikeHydro Model Simulation started.");
                 ProcessStartInfo start = new ProcessStartInfo();
                 start.FileName = @"C:\Program Files\DHI\2014\bin\MIKEBASINapp.exe";
                 start.Arguments = @"E:\FFWS\Model\MIKEHYDRO\GBM_MIKEHYDRO.mhydro";
                 start.CreateNoWindow = true;
                 Process exeProcess = Process.Start(start);
                 exeProcess.WaitForExit();
                 Console.ForegroundColor = ConsoleColor.Green;
                 Console.WriteLine("MikeHydro Model Simulation Completed.");
             }
             catch (Exception error)
             {
                 Console.ForegroundColor = ConsoleColor.Red;
                 Console.WriteLine("MikeHydro Model Simulation can not be completed due to an error. Error: " + error.Message);
                 Console.ReadKey();
                 Environment.Exit(1);
             }
            
             ///------------------------------------------------------------Brahmaputra HD Model Boundary Generation -------------------------------------------------------------///
             try
             {
                 Console.ForegroundColor = ConsoleColor.Cyan;
                 Console.WriteLine("Brahmaputra Basin HD Model boundary generating...");

                 int[] nodeNumber = new int[] { 899, 2686, 2856, 2866, 2331, 3806, 2231, 3831 };

                 IDfsFile resFile = DfsFileFactory.DfsGenericOpenEdit(@"E:\FFWS\Model\MIKEHYDRO\GBM_MIKEHYDRO.mhydro - Result Files\RiverBasin_GBM.dfs0");
                 IDfsFileInfo resfileInfo = resFile.FileInfo;
                 int noTimeSteps = resfileInfo.TimeAxis.NumberOfTimeSteps;
                 DateTime[] date = resFile.FileInfo.TimeAxis.GetDateTimes();
                 DateTime hdStartDate = date[0];
                 double[] timeSpan = new double[noTimeSteps];
                 for (int j = 0; j < noTimeSteps; j++)
                 {
                     timeSpan[j] = resFile.ReadItemTimeStep(899, j).Time;
                 }
                 foreach (int element in nodeNumber)
                 {
                     IDfsItemData<float> data;
                     float[] QSimvalues = new float[noTimeSteps];

                     for (int j = 0; j < noTimeSteps; j++)
                     {
                         data = (IDfsItemData<float>)resFile.ReadItemTimeStep(element, j);
                         QSimvalues[j] = Convert.ToSingle(data.Data[0]);
                     }

                     DfsFactory factory = new DfsFactory();
                     string filename = @"E:\FFWS\Model\BrahmaputraHD\HD_Bound\" + element + ".dfs0";
                     DfsBuilder filecreator = DfsBuilder.Create(element.ToString(), element.ToString(), 2014);
                     filecreator.SetDataType(1);
                     filecreator.SetGeographicalProjection(factory.CreateProjectionUndefined());
                     filecreator.SetTemporalAxis(factory.CreateTemporalNonEqCalendarAxis(eumUnit.eumUsec, new DateTime(hdStartDate.Year, hdStartDate.Month, hdStartDate.Day, hdStartDate.Hour, hdStartDate.Minute, hdStartDate.Second)));
                     filecreator.SetItemStatisticsType(StatType.RegularStat);
                     DfsDynamicItemBuilder item = filecreator.CreateDynamicItemBuilder();
                     item.Set(element.ToString(), eumQuantity.Create(eumItem.eumIDischarge, eumUnit.eumUm3PerSec), DfsSimpleType.Float);
                     item.SetValueType(DataValueType.Instantaneous);
                     item.SetAxis(factory.CreateAxisEqD0());
                     item.SetReferenceCoordinates(1f, 2f, 3f);
                     filecreator.AddDynamicItem(item.GetDynamicItemInfo());

                     filecreator.CreateFile(filename);
                     IDfsFile file = filecreator.GetFile();
                     IDfsFileInfo fileinfo = file.FileInfo;

                     for (int j = 0; j < noTimeSteps; j++)
                     {
                         file.WriteItemTimeStepNext(timeSpan[j], new float[] { QSimvalues[j] });
                     }
                     file.Close();
                 }
                 

                 Console.ForegroundColor = ConsoleColor.Green;
                 Console.WriteLine("Boundary generation completed successfully.");
             }
             catch (Exception error)
             {
                 Console.ForegroundColor = ConsoleColor.Red;
                 Console.WriteLine("Brahmaputra HD Model boundary can not generated due to an error. Error: " + error.Message);
                 Console.ReadKey();
                 Environment.Exit(1);
             }

             ///------------------------------------------------------------Brahmaputra HD Model Simulation -------------------------------------------------------------///
             try
             {
                 Console.ForegroundColor = ConsoleColor.Cyan;
                 Console.WriteLine("Brahmaputra HD Model is initiating...");
                 
                 DateTime today = DateTime.Now;
                 string[] alllines = File.ReadAllLines(@"E:\FFWS\Model\BrahmaputraHD\Brahmaputra_SIM.sim11");
                 alllines[38] = "         start = " + today.AddDays(-7).Year + ", " + today.AddDays(-7).Month + ", " + today.AddDays(-7).Day + ", 6, 0, 0";
                 alllines[39] = "         end = " + today.AddDays(5).Year + ", " + today.AddDays(5).Month + ", " + today.AddDays(5).Day + ", 6, 0, 0";
                 alllines[72] = @"         hd = 2, |.\Result\Brahmaputra.res11|, false, " + today.AddDays(-7).Year + ", " + today.AddDays(-7).Month + ", " + today.AddDays(-7).Day + ", 6, 0, 0";
                 File.WriteAllLines(@"E:\FFWS\Model\BrahmaputraHD\Brahmaputra_SIM.sim11", alllines);

                 Console.ForegroundColor = ConsoleColor.Cyan;
                 Console.WriteLine("HD model Simulation started.");

                 ProcessStartInfo start = new ProcessStartInfo();
                 Process exeProcess = new Process();

                 start.FileName = @"C:\Program Files\DHI\2014\bin\mike11.exe";
                 start.Arguments = @"E:\FFWS\Model\BrahmaputraHD\Brahmaputra_SIM.sim11";
                 exeProcess = Process.Start(start);
                 exeProcess.WaitForExit();
                 Console.ForegroundColor = ConsoleColor.Green;
                 Console.WriteLine("Brahmaputra HD Model Simulation Completed.");
             }
             catch (Exception error)
             {
                 Console.ForegroundColor = ConsoleColor.Red;
                 Console.WriteLine("Brahmaputra HD Model cannot be Simulated due to an error. Error: " + error.Message);
                 Console.ReadKey();
                 Environment.Exit(1);
             }
            
             ///------------------------------------------------------------Noonkhawa Boundary Generation from Brahmaputra HD Model -------------------------------------------------------------///
             try
             {
                 Console.ForegroundColor = ConsoleColor.Cyan;
                 Console.WriteLine("Noonkhawa Boundary is Generating...");
                 DateTime today = DateTime.Today.AddHours(6);
                 
                 dt.Clear();
                 dt.Columns.Clear();
                 con.Open();
                 cmd = new SqlCommand("Select Date, WLValue FROM GBMStationWL Where Station = 'Noonkhawa' AND Date >= @sDate AND Date <= @today ORDER By Date ASC", con);
                 cmd.Parameters.AddWithValue("@sDate", DateTime.Today.AddDays(-7).AddHours(6));
                 cmd.Parameters.AddWithValue("@today", today);
                 adapter.SelectCommand = cmd;
                 adapter.Fill(dt);
                 adapter.Dispose();
                 cmd.Dispose();
                 con.Close();

                 float[] hindWL = new float[dt.Rows.Count];
                 DateTime[] hindDate = new DateTime[dt.Rows.Count];
                 
                 int k = 0;
                 foreach (DataRow dr in dt.Rows)
                 {
                     hindDate[k] = Convert.ToDateTime(dr[0]);
                     hindWL[k] = Convert.ToSingle(dr[1]);
                     k++;
                 }
                 if (hindDate.Contains(DateTime.Today.AddHours(6)) != true)
                 {
                     Console.WriteLine("Noonkhawa Boundary from Brahmaputra HD Model cannot be created due to lack of observed data at Noonkhawa.");
                     Console.WriteLine("The program will now exit. Fill observed data and then try again. Press any key to continue....");
                     Console.ReadKey();
                     Environment.Exit(1);
                 }
                 float obsWL = hindWL.Last();
                 dt.Clear();
                 dt.Columns.Clear();
                 
                 IDfsFile resFile = DfsFileFactory.DfsGenericOpen(@"E:\FFWS\Model\BrahmaputraHD\Result\Brahmaputra.res11");
                 DateTime[] date = resFile.FileInfo.TimeAxis.GetDateTimes();
                 DateTime hdstartDate = date[0];
                 IDfsFileInfo resfileInfo = resFile.FileInfo;
                 IDfsItemData<float> data;
                 IDfsSimpleDynamicItemInfo dynamicItemInfo = resFile.ItemInfo[0];
                 int Wcounter = dynamicItemInfo.ElementCount;
                 int noTimeSteps = resfileInfo.TimeAxis.NumberOfTimeSteps;
                 DateTime[] dfsDate = new DateTime[noTimeSteps];
                 float[] dfsWLData = new float[noTimeSteps];
                 List<DateTime> foreDate = new List<DateTime>();
                 List<float> foreWL = new List<float>();
                 
                 for (int i = 0; i < noTimeSteps; i++)
                 {
                     dfsDate[i] = hdstartDate.AddHours(resFile.ReadItemTimeStep(1, i).Time);
                     data = (IDfsItemData<float>)resFile.ReadItemTimeStep(1, i);
                     TimeSpan sComp = dfsDate[i] - DateTime.Today.AddHours(6);
                     if (sComp.TotalHours>=0)
                     {
                         foreDate.Add(dfsDate[i]);
                         foreWL.Add(Convert.ToSingle(data.Data[Wcounter - 1]));
                     }
                 }
                 float correction = foreWL[0] - obsWL;
                 float[] foreQ = new float[foreWL.Count];
                 for (int i = 0; i < foreWL.Count; i++)
                 {
                     foreWL[i] = foreWL[i] - correction;
                     foreQ[i] = Convert.ToSingle(21.0 * Math.Pow((foreWL[i] - 15.9), 3.3));
                 }

                 dischargeDFS0("NoonkhawaFF_Basin", foreDate, foreQ);
                 waterLevelDFS0("Noonkhawa-RT", hindDate, hindWL);
                 waterLevelDFS0("Noonkhawa-FF", foreDate.ToArray(), foreWL.ToArray()); 

                 Console.ForegroundColor = ConsoleColor.Green;
                 Console.WriteLine("Noonkhawa boundary successfuly generated.");
             }
             catch (Exception error)
             {
                 Console.ForegroundColor = ConsoleColor.Red;
                 Console.WriteLine("Noonkhawa boundary cannot be generated due to an error. Error: " + error.Message);
                 Console.ReadKey();
                 Environment.Exit(1);
             }
            
             ///------------------------------------------------Generating HD Model Boundary--------------------------------------------------------------
             try
             {
                 dt.Columns.Clear();
                 Console.ForegroundColor = ConsoleColor.Cyan;
                 Console.WriteLine("HD Model Boundary generation started.");
                 DateTime bndstartdate = DateTime.Today.AddDays(-7).AddHours(6);
                 DateTime ttoday = DateTime.Today.AddHours(6);
                 string[] station = new string[] { "Amalshid", "Manu-RB", "Durgapur", "Gaibandha", "Kurigram", "Rohanpur", "Panchagarh", "Badarganj", "Dalia", "Nakuagaon", "Comilla", "Lourergorh", "Sarighat", "Faridpur","Dinajpur" };

                 foreach (string element in station)
                 {
                     dt.Columns.Clear();
                     con.Open();
                     cmd = new SqlCommand("Select Date, WLValue FROM GBMStationWL Where Station = @Title AND Date> = @Date AND Date < = @today ORDER By Date ASC", con);
                     cmd.Parameters.AddWithValue("@Title", element);
                     cmd.Parameters.AddWithValue("@Date", bndstartdate);
                     cmd.Parameters.AddWithValue("@today", ttoday);
                     adapter.SelectCommand = cmd;
                     adapter.Fill(dt);
                     adapter.Dispose();
                     cmd.Dispose();
                     con.Close();

                     double[] values = new double[dt.Rows.Count];
                     float[] QObsvalues = new float[dt.Rows.Count];
                     DateTime[] datadate = new DateTime[dt.Rows.Count];
                     int k = 0;
                     foreach (DataRow dr in dt.Rows)
                     {
                         datadate[k] = Convert.ToDateTime(dr[0]);
                         values[k] = Convert.ToDouble(dr[1]);
                         k++;
                     }
                     dt.Clear();
                     if (datadate.Contains(DateTime.Today.AddHours(6)) != true)
                     {
                         Console.ForegroundColor = ConsoleColor.Red;
                         Console.WriteLine(element + " HD Boundary cannot be created due to lack of observed data.");
                         Console.WriteLine("The program will now copy previous data . . . . ");
                         List<DateTime> avlblDate = datadate.ToList();
                         List<double> avlblValues = values.ToList();
                         avlblDate.Add(DateTime.Today.AddHours(6));
                         avlblValues.Add(values.Last());
                         datadate = avlblDate.ToArray();
                         values = avlblValues.ToArray();
                     }
                     else
                     {
                         if (element == "Amalshid")
                         {
                             for (int x = 0; x < values.Length; x++)
                             {
                                 if (values[x] <= 14.75)
                                 {
                                     QObsvalues[x] = Convert.ToSingle(2.43 * Math.Pow((values[x] - 2.2), 2.53) + 3.39 * Math.Pow((values[x] - 5.96), 2.55));
                                 }
                                 else { QObsvalues[x] = Convert.ToSingle(4.55 * Math.Pow((values[x] - 6.95), 2.81) + 3.39 * Math.Pow((values[x] - 5.96), 2.55)); }
                             }

                             IDfsFile resFile = DfsFileFactory.DfsGenericOpenEdit(@"E:\FFWS\Model\MIKEHYDRO\GBM_MIKEHYDRO.mhydro - Result Files\RiverBasin_GBM.dfs0");
                             IDfsFileInfo resfileInfo = resFile.FileInfo;
                             int noTimeSteps = resfileInfo.TimeAxis.NumberOfTimeSteps;
                             IDfsItemData<float> data;
                             float[] QSimvalues = new float[49];

                             for (int j = 0; j < 49; j++)
                             {
                                 data = (IDfsItemData<float>)resFile.ReadItemTimeStep(334, noTimeSteps - 49 + j);   //Assuming start date of NAM and Mike Basin 01/01/2014 
                                 QSimvalues[j] = Convert.ToSingle(data.Data[0]);
                             }
                             // Applying Correction
                             float correction = QObsvalues[QObsvalues.Length - 1] - QSimvalues[0];
                             for (int j = 0; j < QSimvalues.Length; j++)
                             {
                                 QSimvalues[j] = QSimvalues[j] + correction;
                             }
                             QSimvalues[QSimvalues.Length - 1] = QSimvalues[QSimvalues.Length - 2];
                             ///----------------------------------------------------------------------------writing two dfs0-----------------------------------------------------///
                             dischargeObsDFS0(element, datadate, QObsvalues);
                             dischargeForeDFS0(element, QSimvalues);
                         }

                         else if (element == "Manu-RB")
                         {
                             for (int x = 0; x < values.Length; x++)
                             {
                                 QObsvalues[x] = Convert.ToSingle(10.0 * Math.Pow((values[x] - 11.8), 2.25));
                             }
                             //obtaining forecasted Discharge
                             IDfsFile resFile = DfsFileFactory.DfsGenericOpenEdit(@"E:\FFWS\Model\NAM\Result\Output\RunOff, NEX-28, 2355.950.dfs0");
                             IDfsFileInfo resfileInfo = resFile.FileInfo;
                             int noTimeSteps = resfileInfo.TimeAxis.NumberOfTimeSteps;
                             IDfsItemData<float> data;
                             float[] QSimvalues = new float[49];

                             for (int j = 0; j < 49; j++)
                             {
                                 data = (IDfsItemData<float>)resFile.ReadItemTimeStep(1, noTimeSteps - 49 + j);
                                 QSimvalues[j] = Convert.ToSingle(data.Data[0]);
                             }

                             // Applying Correction
                             float correction = QObsvalues[QObsvalues.Length - 1] / QSimvalues[0];
                             for (int j = 0; j < QSimvalues.Length; j++)
                             {
                                 QSimvalues[j] = QSimvalues[j] * correction;
                             }

                             ///----------------------------------------------------------------------------writing two dfs0-----------------------------------------------------///
                             dischargeObsDFS0(element, datadate, QObsvalues);
                             dischargeForeDFS0(element, QSimvalues);
                         }

                         else if (element == "Durgapur")
                         {
                             for (int x = 0; x < values.Length; x++)
                             {
                                 if (values[x] > 10.3)
                                 {
                                     QObsvalues[x] = Convert.ToSingle(124.0 * Math.Pow((values[x] - 10.3), 2.24));
                                 }
                                 else { QObsvalues[x] = 1; }
                             }
                             //obtaining forecasted Discharge
                             IDfsFile resFile = DfsFileFactory.DfsGenericOpenEdit(@"E:\FFWS\Model\NAM\Result\Output\RunOff, NEX-20, 2317.970.dfs0");
                             IDfsFileInfo resfileInfo = resFile.FileInfo;
                             int noTimeSteps = resfileInfo.TimeAxis.NumberOfTimeSteps;
                             IDfsItemData<float> data;
                             float[] QSimvalues = new float[49];

                             for (int j = 0; j < 49; j++)
                             {
                                 data = (IDfsItemData<float>)resFile.ReadItemTimeStep(1, noTimeSteps - 49 + j);
                                 QSimvalues[j] = Convert.ToSingle(data.Data[0]);
                             }

                             // Applying Correction
                             float correction = QObsvalues[QObsvalues.Length - 1] / QSimvalues[0];
                             for (int j = 0; j < QSimvalues.Length; j++)
                             {
                                 QSimvalues[j] = QSimvalues[j] * correction;
                             }
                             ///----------------------------------------------------------------------------writing two dfs0-----------------------------------------------------///
                             dischargeObsDFS0(element, datadate, QObsvalues);
                             dischargeForeDFS0(element, QSimvalues);
                             ///----------------------------------------------------------------------------writing two water level dfs0-----------------------------------------------------///
                             for (int i = 0; i < QObsvalues.Length; i++)
                             {
                                 QObsvalues[i] = Convert.ToSingle(10.3 + Math.Pow((Convert.ToDouble(QObsvalues[i]) / 124.0), (1 / 2.24)));
                             }
                             waterLevelObsDfs0(element, datadate, QObsvalues);
                             for (int i = 0; i < QSimvalues.Length; i++)
                             {
                                 QSimvalues[i] = Convert.ToSingle(10.3 + Math.Pow((Convert.ToDouble(QSimvalues[i]) / 124.0), (1 / 2.24)));
                             }

                             waterLevelForeDFS0(element, QSimvalues);
                         }

                         else if (element == "Gaibandha")
                         {
                             for (int x = 0; x < values.Length; x++)
                             {
                                 QObsvalues[x] = Convert.ToSingle(values[x]);
                             }
                             //obtaining forecasted Discharge
                             IDfsFile resFile1 = DfsFileFactory.DfsGenericOpenEdit(@"E:\FFWS\Model\NAM\Result\Output\RunOff, NW-05, 794.600.dfs0");
                             IDfsFile resFile2 = DfsFileFactory.DfsGenericOpenEdit(@"E:\FFWS\Model\NAM\Result\Output\RunOff, NW-11, 862.600.dfs0");
                             IDfsFileInfo resfileInfo = resFile1.FileInfo;
                             int noTimeSteps = resfileInfo.TimeAxis.NumberOfTimeSteps;
                             IDfsItemData<float> data1;
                             IDfsItemData<float> data2;
                             float[] QSimvalues = new float[49];

                             for (int j = 0; j < 49; j++)
                             {
                                 data1 = (IDfsItemData<float>)resFile1.ReadItemTimeStep(1, noTimeSteps - 49 + j);
                                 data2 = (IDfsItemData<float>)resFile2.ReadItemTimeStep(1, noTimeSteps - 49 + j);
                                 QSimvalues[j] = Convert.ToSingle(17.0 + Math.Pow((Convert.ToSingle(data1.Data[0]) + Convert.ToSingle(data2.Data[0])) / 2.0, (1.0 / 3.0)));
                             }

                             // Applying Correction
                             float correction = Convert.ToSingle(values[values.Length - 1]) - QSimvalues[0];
                             for (int j = 0; j < QSimvalues.Length; j++)
                             {
                                 QSimvalues[j] = QSimvalues[j] + correction;
                             }

                             waterLevelObsDfs0(element.Trim(), datadate, QObsvalues);
                             waterLevelForeDFS0(element.Trim(), QSimvalues);
                         }

                         else if (element == "Kurigram")
                         {
                             for (int x = 0; x < values.Length; x++)
                             {
                                 QObsvalues[x] = Convert.ToSingle(310.0 * Math.Pow((values[x] - 22.85), 2.95));
                             }
                             //obtaining forecasted Discharge
                             IDfsFile resFile = DfsFileFactory.DfsGenericOpenEdit(@"E:\FFWS\Model\MIKEHYDRO\GBM_MIKEHYDRO.mhydro - Result Files\RiverBasin_GBM.dfs0");
                             IDfsFileInfo resfileInfo = resFile.FileInfo;
                             int noTimeSteps = resfileInfo.TimeAxis.NumberOfTimeSteps;
                             IDfsItemData<float> data;
                             float[] QSimvalues = new float[49];

                             for (int j = 0; j < 49; j++)
                             {
                                 data = (IDfsItemData<float>)resFile.ReadItemTimeStep(551, noTimeSteps - 49 + j);
                                 QSimvalues[j] = Convert.ToSingle(data.Data[0]);
                             }

                             // Applying Correction
                             float correction = QObsvalues[QObsvalues.Length - 1] / QSimvalues[0];
                             for (int j = 0; j < QSimvalues.Length; j++)
                             {
                                 QSimvalues[j] = QSimvalues[j] * correction;
                             }

                             QSimvalues[QSimvalues.Length - 1] = QSimvalues[QSimvalues.Length - 2];

                             dischargeObsDFS0(element.Trim(), datadate, QObsvalues);
                             dischargeForeDFS0(element.Trim(), QSimvalues);

                             for (int i = 0; i < QObsvalues.Length; i++)
                             {
                                 QObsvalues[i] = Convert.ToSingle(22.85 + Math.Pow(Convert.ToDouble(QObsvalues[i]) / 310.0, (1 / 2.95)));
                             }
                             waterLevelObsDfs0(element.Trim(), datadate, QObsvalues);
                             for (int i = 0; i < QSimvalues.Length; i++)
                             {
                                 QSimvalues[i] = Convert.ToSingle(22.85 + Math.Pow(Convert.ToDouble(QSimvalues[i]) / 310.0, (1 / 2.95)));
                             }
                             waterLevelForeDFS0(element.Trim(), QSimvalues);
                         }

                         else if (element == "Rohanpur")
                         {
                             for (int x = 0; x < values.Length; x++)
                             {
                                 QObsvalues[x] = Convert.ToSingle(5.0 * Math.Pow((values[x] - 11.42), 2.7));
                             }
                             //obtaining forecasted Discharge
                             IDfsFile resFile = DfsFileFactory.DfsGenericOpenEdit(@"E:\FFWS\Model\MIKEHYDRO\GBM_MIKEHYDRO.mhydro - Result Files\RiverBasin_GBM.dfs0");
                             IDfsFileInfo resfileInfo = resFile.FileInfo;
                             int noTimeSteps = resfileInfo.TimeAxis.NumberOfTimeSteps;
                             IDfsItemData<float> data;
                             float[] QSimvalues = new float[49];

                             for (int j = 0; j < 49; j++)
                             {
                                 data = (IDfsItemData<float>)resFile.ReadItemTimeStep(1644, noTimeSteps - 49 + j);
                                 QSimvalues[j] = Convert.ToSingle(data.Data[0]);
                             }

                             // Applying Correction
                             float correction = QObsvalues[QObsvalues.Length - 1] / QSimvalues[0];
                             for (int j = 0; j < QSimvalues.Length; j++)
                             {
                                 QSimvalues[j] = QSimvalues[j] * correction;
                             }

                             QSimvalues[QSimvalues.Length - 1] = QSimvalues[QSimvalues.Length - 2];

                             dischargeObsDFS0(element.Trim(), datadate, QObsvalues);
                             dischargeForeDFS0(element.Trim(), QSimvalues);

                             for (int i = 0; i < QObsvalues.Length; i++)
                             {
                                 QObsvalues[i] = Convert.ToSingle(11.42 + Math.Pow(Convert.ToDouble(QObsvalues[i]) / 5.0, (1 / 2.7)));
                             }
                             waterLevelObsDfs0(element.Trim(), datadate, QObsvalues);
                             for (int i = 0; i < QSimvalues.Length; i++)
                             {
                                 QSimvalues[i] = Convert.ToSingle(11.42 + Math.Pow(Convert.ToDouble(QSimvalues[i]) / 5.0, (1 / 2.7)));
                             }
                             waterLevelForeDFS0(element.Trim(), QSimvalues);
                         }

                         else if (element == "Panchagarh")
                         {
                             for (int x = 0; x < values.Length; x++)
                             {
                                 if (values[x] <= 67.3) { QObsvalues[x] = 0; }
                                 else { QObsvalues[x] = Convert.ToSingle(47.4 * Math.Pow((values[x] - 67.3), 2.23)); }
                             }

                             //obtaining forecasted Discharge
                             IDfsFile resFile = DfsFileFactory.DfsGenericOpenEdit(@"E:\FFWS\Model\NAM\Result\Output\RunOff, NW02U, 668.000.dfs0");
                             IDfsFileInfo resfileInfo = resFile.FileInfo;
                             int noTimeSteps = resfileInfo.TimeAxis.NumberOfTimeSteps;
                             IDfsItemData<float> data;
                             float[] QSimvalues = new float[49];

                             for (int j = 0; j < 49; j++)
                             {
                                 data = (IDfsItemData<float>)resFile.ReadItemTimeStep(1, noTimeSteps - 49 + j);
                                 QSimvalues[j] = Convert.ToSingle(data.Data[0]);
                             }

                             // Applying Correction
                             float correction = QObsvalues[QObsvalues.Length - 1] / QSimvalues[0];
                             for (int j = 0; j < QSimvalues.Length; j++)
                             {
                                 QSimvalues[j] = QSimvalues[j] * correction;
                             }

                             dischargeObsDFS0(element.Trim(), datadate, QObsvalues);
                             dischargeForeDFS0(element.Trim(), QSimvalues);
                         }

                         else if (element == "Badarganj")
                         {
                             for (int x = 0; x < values.Length; x++)
                             {
                                 if (values[x] <= 29.5)
                                 {
                                     QObsvalues[x] = Convert.ToSingle(20.39 * Math.Pow((values[x] - 27.5), 2.72));
                                 }
                                 else { QObsvalues[x] = Convert.ToSingle(130.44 * Math.Pow((values[x] - 28.45), 1.52)); }
                             }

                             //obtaining forecasted Discharge
                             IDfsFile resFile1 = DfsFileFactory.DfsGenericOpenEdit(@"E:\FFWS\Model\NAM\Result\Output\RunOff, NW-1L, 749.500.dfs0");
                             IDfsFile resFile2 = DfsFileFactory.DfsGenericOpenEdit(@"E:\FFWS\Model\NAM\Result\Output\RunOff, NW-1M, 374.530.dfs0");
                             IDfsFile resFile3 = DfsFileFactory.DfsGenericOpenEdit(@"E:\FFWS\Model\NAM\Result\Output\RunOff, NW-1U, 110.400.dfs0");
                             IDfsFileInfo resfileInfo = resFile1.FileInfo;
                             int noTimeSteps = resfileInfo.TimeAxis.NumberOfTimeSteps;
                             IDfsItemData<float> data1;
                             IDfsItemData<float> data2;
                             IDfsItemData<float> data3;

                             float[] QSimvalues = new float[49];

                             for (int j = 0; j < 49; j++)
                             {
                                 data1 = (IDfsItemData<float>)resFile1.ReadItemTimeStep(1, noTimeSteps - 49 + j);
                                 data2 = (IDfsItemData<float>)resFile2.ReadItemTimeStep(1, noTimeSteps - 49 + j);
                                 data3 = (IDfsItemData<float>)resFile3.ReadItemTimeStep(1, noTimeSteps - 49 + j);
                                 QSimvalues[j] = Convert.ToSingle(data1.Data[0]) + Convert.ToSingle(data2.Data[0]) + Convert.ToSingle(data3.Data[0]);
                             }

                             // Applying Correction
                             float correction;
                             correction = QObsvalues[QObsvalues.Length - 1] / QSimvalues[0];
                             for (int j = 0; j < QSimvalues.Length; j++)
                             {
                                 QSimvalues[j] = QSimvalues[j] * correction;
                             }

                             dischargeObsDFS0(element.Trim(), datadate, QObsvalues);
                             dischargeForeDFS0(element.Trim(), QSimvalues);
                         }

                         else if (element == "Dalia")
                         {
                             for (int x = 0; x < values.Length; x++)
                             {
                                 QObsvalues[x] = Convert.ToSingle(3.64 * Math.Pow((values[x] - 48.49), 5.73));
                             }

                             //obtaining forecasted Discharge
                             IDfsFile resFile = DfsFileFactory.DfsGenericOpenEdit(@"E:\FFWS\Model\MIKEHYDRO\GBM_MIKEHYDRO.mhydro - Result Files\RiverBasin_GBM.dfs0");
                             IDfsFileInfo resfileInfo = resFile.FileInfo;
                             int noTimeSteps = resfileInfo.TimeAxis.NumberOfTimeSteps;
                             IDfsItemData<float> data;
                             float[] QSimvalues = new float[49];

                             for (int j = 0; j < 49; j++)
                             {
                                 data = (IDfsItemData<float>)resFile.ReadItemTimeStep(535, noTimeSteps - 49 + j);
                                 QSimvalues[j] = Convert.ToSingle(data.Data[0]);
                             }

                             // Applying Correction
                             float correction = QObsvalues[QObsvalues.Length - 1] / QSimvalues[0];
                             for (int j = 0; j < QSimvalues.Length; j++)
                             {
                                 QSimvalues[j] = QSimvalues[j] * correction;
                             }
                             QSimvalues[QSimvalues.Length - 1] = QSimvalues[QSimvalues.Length - 2];

                             dischargeObsDFS0(element.Trim(), datadate, QObsvalues);
                             dischargeForeDFS0(element.Trim(), QSimvalues);
                         }

                         else if (element == "Nakuagaon")
                         {
                             for (int x = 0; x < values.Length; x++)
                             {
                                 QObsvalues[x] = Convert.ToSingle(27.2 * Math.Pow((values[x] - 19.6), 1.69));
                             }
                             //obtaining forecasted Discharge
                             IDfsFile resFile = DfsFileFactory.DfsGenericOpenEdit(@"E:\FFWS\Model\NAM\Result\Output\RunOff, NEX-19A, 461.700.dfs0");
                             IDfsFileInfo resfileInfo = resFile.FileInfo;
                             int noTimeSteps = resfileInfo.TimeAxis.NumberOfTimeSteps;
                             IDfsItemData<float> data;
                             float[] QSimvalues = new float[49];

                             for (int j = 0; j < 49; j++)
                             {
                                 data = (IDfsItemData<float>)resFile.ReadItemTimeStep(1, noTimeSteps - 49 + j);
                                 QSimvalues[j] = Convert.ToSingle(data.Data[0]);
                                 //listBox1.Items.Add("Simulated: " + QSimvalues[j]);
                             }

                             // Applying Correction
                             float correction = QObsvalues[QObsvalues.Length - 1] - QSimvalues[0];
                             for (int j = 0; j < QSimvalues.Length; j++)
                             {
                                 QSimvalues[j] = QSimvalues[j] + correction;
                                 //listBox1.Items.Add("Correction: "+ correction);
                             }

                             dischargeObsDFS0(element.Trim(), datadate, QObsvalues);
                             dischargeForeDFS0(element.Trim(), QSimvalues);
                         }
                         else if (element == "Comilla")
                         {
                             for (int x = 0; x < values.Length; x++)
                             {
                                 if (values[x] <= 6.72)
                                 {
                                     QObsvalues[x] = 0;
                                 }
                                 else if (values[x] <= 12.0 && values[x] > 6.72)
                                 {
                                     QObsvalues[x] = Convert.ToSingle(33.56 * Math.Pow((values[x] - 6.72), 1.59));
                                 }
                                 else { QObsvalues[x] = Convert.ToSingle(3.58 * Math.Pow((values[x] - 6.72), 2.93)); }
                             }

                             //obtaining forecasted Discharge
                             IDfsFile resFile = DfsFileFactory.DfsGenericOpenEdit(@"E:\FFWS\Model\NAM\Result\Output\RunOff, SE-44, 454.650.dfs0");
                             IDfsFileInfo resfileInfo = resFile.FileInfo;
                             int noTimeSteps = resfileInfo.TimeAxis.NumberOfTimeSteps;
                             IDfsItemData<float> data;
                             float[] QSimvalues = new float[49];

                             for (int j = 0; j < 49; j++)
                             {
                                 data = (IDfsItemData<float>)resFile.ReadItemTimeStep(1, noTimeSteps - 49 + j);
                                 QSimvalues[j] = Convert.ToSingle(data.Data[0]);
                             }

                             // Applying Correction
                             float correction = QObsvalues[QObsvalues.Length - 1] / QSimvalues[0];
                             for (int j = 0; j < QSimvalues.Length; j++)
                             {
                                 QSimvalues[j] = QSimvalues[j] * correction;
                             }

                             dischargeObsDFS0(element.Trim(), datadate, QObsvalues);
                             dischargeForeDFS0(element.Trim(), QSimvalues);
                         }
                         else if (element == "Lourergorh")
                         {
                             for (int x = 0; x < values.Length; x++)
                             {
                                 if (values[x] <= 6.83)
                                 {
                                     QObsvalues[x] = Convert.ToSingle(17.31 * Math.Pow((values[x] - 3.5), 2.9));
                                 }
                                 else { QObsvalues[x] = Convert.ToSingle(125.0 * Math.Pow((values[x] - 5.0), 2.5)); }
                             }

                             //obtaining forecasted Discharge
                             IDfsFile resFile = DfsFileFactory.DfsGenericOpenEdit(@"E:\FFWS\Model\NAM\Result\Output\RunOff, NEX-48, 2512.330.dfs0");
                             IDfsFileInfo resfileInfo = resFile.FileInfo;
                             int noTimeSteps = resfileInfo.TimeAxis.NumberOfTimeSteps;
                             IDfsItemData<float> data;
                             float[] QSimvalues = new float[49];

                             for (int j = 0; j < 49; j++)
                             {
                                 data = (IDfsItemData<float>)resFile.ReadItemTimeStep(1, noTimeSteps - 49 + j);
                                 QSimvalues[j] = Convert.ToSingle(data.Data[0]);
                             }

                             // Applying Correction
                             float correction = QObsvalues[QObsvalues.Length - 1] / QSimvalues[0];
                             for (int j = 0; j < QSimvalues.Length; j++)
                             {
                                 QSimvalues[j] = QSimvalues[j] * correction;
                             }

                             dischargeObsDFS0(element.Trim(), datadate, QObsvalues);
                             dischargeForeDFS0(element.Trim(), QSimvalues);
                         }
                         else if (element == "Sarighat")
                         {
                             for (int x = 0; x < values.Length; x++)
                             {
                                 QObsvalues[x] = Convert.ToSingle(3.34 * Math.Pow((values[x] - 3.92), 2.49));
                             }
                             //obtaining forecasted Discharge
                             IDfsFile resFile = DfsFileFactory.DfsGenericOpenEdit(@"E:\FFWS\Model\NAM\Result\Output\RunOff, NEX-02, 895.990.dfs0");
                             IDfsFileInfo resfileInfo = resFile.FileInfo;
                             int noTimeSteps = resfileInfo.TimeAxis.NumberOfTimeSteps;
                             IDfsItemData<float> data;
                             float[] QSimvalues = new float[49];

                             for (int j = 0; j < 49; j++)
                             {
                                 data = (IDfsItemData<float>)resFile.ReadItemTimeStep(1, noTimeSteps - 49 + j);
                                 QSimvalues[j] = Convert.ToSingle(data.Data[0]);
                             }

                             // Applying Correction
                             float correction = QObsvalues[QObsvalues.Length - 1] / QSimvalues[0];
                             for (int j = 0; j < QSimvalues.Length; j++)
                             {
                                 QSimvalues[j] = QSimvalues[j] * correction;
                             }

                             dischargeObsDFS0(element.Trim(), datadate, QObsvalues);
                             dischargeForeDFS0(element.Trim(), QSimvalues);
                         }

                         else if (element == "Faridpur")
                         {
                             for (int x = 0; x < values.Length; x++)
                             {
                                 QObsvalues[x] = Convert.ToSingle(values[x]);
                             }

                             IDfsFile resFile = DfsFileFactory.DfsGenericOpenEdit(@"E:\FFWS\Model\NAM\Result\Output\RunOff, SW-05, 620.000.dfs0");
                             IDfsFileInfo resfileInfo = resFile.FileInfo;
                             int noTimeSteps = resfileInfo.TimeAxis.NumberOfTimeSteps;
                             IDfsItemData<float> data;
                             double[] QSimvalues = new double[49];
                             float[] WLSimu = new float[49];
                             for (int j = 0; j < 49; j++)
                             {
                                 data = (IDfsItemData<float>)resFile.ReadItemTimeStep(1, noTimeSteps - 49 + j);
                                 QSimvalues[j] = Convert.ToSingle(data.Data[0]);
                                 if (QSimvalues[j] <= 10.7)
                                 {
                                     WLSimu[j] = Convert.ToSingle(0.09 + Math.Pow((QSimvalues[j] / 1.23), (1 / 2.46)));
                                 }
                                 else { WLSimu[j] = Convert.ToSingle(1.10 + Math.Pow((QSimvalues[j] / 1.65), (1 / 2.39))); }
                             }

                             // Applying Correction
                             float correction = QObsvalues[QObsvalues.Length - 1] - WLSimu[0];
                             for (int j = 0; j < WLSimu.Length; j++)
                             {
                                 WLSimu[j] = WLSimu[j] + correction;
                             }

                             waterLevelObsDfs0(element.Trim(), datadate, QObsvalues);
                             waterLevelForeDFS0(element.Trim(), WLSimu);
                         }

                         else if (element == "Dinajpur")
                         {
                             for (int x = 0; x < values.Length; x++)
                             {
                                 QObsvalues[x] = Convert.ToSingle(values[x]);
                             }
                             IDfsFile resFile1 = DfsFileFactory.DfsGenericOpenEdit(@"E:\FFWS\Model\NAM\Result\Output\RunOff, NW-07, 313.528.dfs0");
                             IDfsFile resFile2 = DfsFileFactory.DfsGenericOpenEdit(@"E:\FFWS\Model\NAM\Result\Output\RunOff, NW-16L, 326.900.dfs0");
                             IDfsFile resFile3 = DfsFileFactory.DfsGenericOpenEdit(@"E:\FFWS\Model\NAM\Result\Output\RunOff, NW-16U, 110.400.dfs0");
                             IDfsFileInfo resfileInfo = resFile1.FileInfo;
                             int noTimeSteps = resfileInfo.TimeAxis.NumberOfTimeSteps;
                             IDfsItemData<float> data1;
                             IDfsItemData<float> data2;
                             IDfsItemData<float> data3;
                             double[] QSimvalues = new double[49];
                             float[] WLSimu = new float[49];
                             for (int j = 0; j < 49; j++)
                             {
                                 data1 = (IDfsItemData<float>)resFile1.ReadItemTimeStep(1, noTimeSteps - 49 + j);
                                 data2 = (IDfsItemData<float>)resFile2.ReadItemTimeStep(1, noTimeSteps - 49 + j);
                                 data3 = (IDfsItemData<float>)resFile3.ReadItemTimeStep(1, noTimeSteps - 49 + j);
                                 QSimvalues[j] = Convert.ToSingle(data1.Data[0]) + Convert.ToSingle(data2.Data[0]) + Convert.ToSingle(data3.Data[0]);
                                 WLSimu[j] = Convert.ToSingle(25.0 + Math.Pow((QSimvalues[j] / 12.5), (1 / 1.3)));
                             }

                             // Applying Correction
                             float correction = QObsvalues[QObsvalues.Length - 1] - WLSimu[0];
                             for (int j = 0; j < WLSimu.Length; j++)
                             {
                                 WLSimu[j] = WLSimu[j] + correction;
                             }
                             waterLevelObsDfs0(element.Trim(), datadate, QObsvalues);
                             waterLevelForeDFS0(element.Trim(), WLSimu);
                         }
                     }
                 }
                 Console.ForegroundColor = ConsoleColor.Green;
                 Console.WriteLine("Other Boundaries(Except Noonkhawa and Pankha) completed.");
             }
             catch (Exception error)
             {
                 con.Close();
                 Console.ForegroundColor = ConsoleColor.Red;
                 Console.WriteLine("Other Boundaries(Except Noonkhawa and Pankha) cannot be generated due to an error. Error: " + error.Message);
                 Console.ReadKey();
             }
            
             ///----------------------------------------------------------------Noonkhawa Boundary generation----------------------------------------------------
             try
             {
                 dt.Clear();
                 dt.Columns.Clear();
                 DateTime bndstartdate = DateTime.Today.AddDays(-7).AddHours(18);
                 con.Open();
                 cmd = new SqlCommand("Select Date, WLValue FROM GBMStationWL Where Station = 'Bahadurabad' AND Date> = @Date AND Date < = @today ORDER By Date ASC", con);
                 DateTime today = DateTime.Today.AddHours(6);
                 cmd.Parameters.AddWithValue("@Date", bndstartdate);
                 cmd.Parameters.AddWithValue("@today", DateTime.Today.AddHours(6));
                 adapter.SelectCommand = cmd;
                 adapter.Fill(dt);
                 adapter.Dispose();
                 cmd.Dispose();
                 con.Close();

                 float[] WLBahObs = new float[dt.Rows.Count + 1];
                 float[] QBahObs = new float[dt.Rows.Count + 1];
                 DateTime[] Bahadate = new DateTime[dt.Rows.Count + 1];

                 int x = 0;
                 foreach (DataRow dr in dt.Rows)
                 {
                     Bahadate[x] = Convert.ToDateTime(dr[0]);
                     WLBahObs[x] = Convert.ToSingle(dr[1]);
                     x++;
                 }

                 if (Bahadate.Contains(DateTime.Today.AddHours(6)) != true)
                 {
                     Console.WriteLine("Noonkhawa HD Boundary cannot be created due to lack of observed data at Bahadurabad.");
                     Console.WriteLine("The program will now copy previous data . . . . ");
                     List<DateTime> avlblDate = Bahadate.ToList();
                     List<float> avlblValues = WLBahObs.ToList();
                     avlblDate.Add(DateTime.Today.AddHours(6));
                     avlblValues.Add(WLBahObs[x]);
                     Bahadate = avlblDate.ToArray();
                     WLBahObs = avlblValues.ToArray();
                 }

                 dt.Clear();
                 dt.Columns.Clear();
                 con.Open();
                 cmd = new SqlCommand("Select Date, WLValue FROM GBMStationWL Where Station = 'Noonkhawa' AND Date > @Date AND Date < = @today ORDER By Date ASC", con);
                 cmd.Parameters.AddWithValue("@today", DateTime.Today.AddHours(6));
                 cmd.Parameters.AddWithValue("@Date", bndstartdate);
                 adapter.SelectCommand = cmd;
                 adapter.Fill(dt);
                 adapter.Dispose();
                 cmd.Dispose();
                 con.Close();

                 float[] WLNoonObs = new float[dt.Rows.Count];
                 DateTime[] DateNoonObs = new DateTime[dt.Rows.Count];

                 x = 0;
                 foreach (DataRow dr in dt.Rows)
                 {
                     DateNoonObs[x] = Convert.ToDateTime(dr[0]);
                     WLNoonObs[x] = Convert.ToSingle(dr[1]);
                     x++;
                 }
                 dt.Clear();

                 if (DateNoonObs.Contains(DateTime.Today.AddHours(6)) != true)
                 {
                     Console.WriteLine("Noonkhawa Boundary cannot be created due to lack of observed data at Noonkhawa.");
                     Console.WriteLine("The program will now copy previous data . . . . ");
                     List<DateTime> avlblDate = DateNoonObs.ToList();
                     List<float> avlblValues = WLNoonObs.ToList();
                     avlblDate.Add(DateTime.Today.AddHours(6));
                     avlblValues.Add(WLNoonObs[x]);
                     DateNoonObs = avlblDate.ToArray();
                     WLNoonObs = avlblValues.ToArray();
                 }

                 float correction = WLNoonObs[WLNoonObs.Length - 1] - WLBahObs[WLBahObs.Length - 2];
                 WLBahObs[WLBahObs.Length - 1] = WLNoonObs[WLNoonObs.Length - 1] - correction;
                 Bahadate[Bahadate.Length - 1] = DateTime.Today.AddHours(18);

                 for (int i = 0; i < WLBahObs.Length; i++)
                 {
                     if (WLBahObs[i] <= 18.139) { QBahObs[i] = Convert.ToSingle(310.0 * Math.Pow((WLBahObs[i] - 10.15), 2.2)); }
                     else { QBahObs[i] = Convert.ToSingle(135.0 * Math.Pow((WLBahObs[i] - 12.15), 3.0)); }
                     Bahadate[i] = Bahadate[i].AddHours(-12);
                 }

                 IDfsFile resFile = DfsFileFactory.DfsGenericOpenEdit(@"E:\FFWS\Model\MIKEHYDRO\GBM_MIKEHYDRO.mhydro - Result Files\RiverBasin_GBM.dfs0");
                 IDfsFileInfo resfileInfo = resFile.FileInfo;
                 int noTimeSteps = resfileInfo.TimeAxis.NumberOfTimeSteps;
                 IDfsItemData<float> data1;
                 IDfsItemData<float> data2;


                 DateTime[] kuriDate = new DateTime[104];
                 float[] QSimvalues = new float[104];

                 bndstartdate = bndstartdate.AddHours(-12);
                 for (int j = 0; j < 104; j++)
                 {
                     kuriDate[j] = bndstartdate.AddHours(j * 3);
                     data1 = (IDfsItemData<float>)resFile.ReadItemTimeStep(551, noTimeSteps - 104 + j);
                     data2 = (IDfsItemData<float>)resFile.ReadItemTimeStep(535, noTimeSteps - 104 + j);
                     QSimvalues[j] = Convert.ToSingle(data1.Data[0]) + Convert.ToSingle(data2.Data[0]);
                 }
                 float[] QHindNoon = new float[QBahObs.Length];

                 // Obtaining Discharge data by comparing with Kurigram and Dalia station 
                 for (int j = 0; j < QBahObs.Length; j++)
                 {
                     for (int i = 0; i < QSimvalues.Length; i++)
                     {
                         if (Bahadate[j] == kuriDate[i])
                         {
                             QHindNoon[j] = QBahObs[j] - QSimvalues[i];
                         }
                     }
                 }
                 //Reading forecasted discharge of Noonkhawa Station
                 IDfsItemData<float> data3;
                 float[] QforeNoon = new float[49];
                 for (int j = 0; j < 49; j++)
                 {
                     kuriDate[j] = bndstartdate.AddHours(j * 3);

                     data3 = (IDfsItemData<float>)resFile.ReadItemTimeStep(200, noTimeSteps - 49 + j);
                     QforeNoon[j] = Convert.ToSingle(data3.Data[0]);
                 }
                 float Qcorr = QHindNoon[QHindNoon.Length - 1] / QforeNoon[0];
                 for (int j = 0; j < QforeNoon.Length; j++)
                 {
                     QforeNoon[j] = QforeNoon[j] * Qcorr;
                 }
                 QforeNoon[QforeNoon.Length - 1] = QforeNoon[QforeNoon.Length - 2];

                 dischargeObsDFS0("Noonkhawa", Bahadate, QHindNoon);
                 dischargeForeDFS0("Noonkhawa", QforeNoon);

             }
             catch (Exception error)
             {
                 con.Close();
                 Console.ForegroundColor = ConsoleColor.Red;
                 Console.WriteLine("Error in Noonkhawa Boundary. Error: " + error.Message);
                 Console.ReadKey();
             }
            
             ///------------------------------------------------------------------------PANKHA Boundary Generation---------------------------------------------------
             try
             {
                 dt.Clear();
                 dt.Columns.Clear();
                 DateTime bndstartdate = DateTime.Today.AddDays(-6).AddHours(6);
                 con.Open();
                 cmd = new SqlCommand("Select Date, WLValue FROM GBMStationWL Where Station = 'Hardinge-RB' AND Date> = @Date AND Date < = @today ORDER By Date ASC", con);
                 cmd.Parameters.AddWithValue("@today", DateTime.Today.AddHours(6));
                 cmd.Parameters.AddWithValue("@Date", bndstartdate);
                 adapter.SelectCommand = cmd;
                 adapter.Fill(dt);
                 adapter.Dispose();
                 cmd.Dispose();
                 con.Close();

                 float[] WLHBObs = new float[dt.Rows.Count + 5];
                 float[] QHBObs = new float[dt.Rows.Count + 5];
                 DateTime[] HBdate = new DateTime[dt.Rows.Count + 5];

                 int x = 0;
                 foreach (DataRow dr in dt.Rows)
                 {
                     HBdate[x] = Convert.ToDateTime(dr[0]);
                     WLHBObs[x] = Convert.ToSingle(dr[1]);
                     x++;
                 }
                 dt.Clear();
                 dt.Columns.Clear();

                 if (HBdate.Contains(DateTime.Today.AddHours(6)) != true)
                 {
                     Console.WriteLine("Pankha Boundary cannot be created due to lack of observed data at Hardinge-RB.");
                     Console.WriteLine("The program will now copy previous data . . . . ");
                     List<DateTime> avlblDate = HBdate.ToList();
                     List<float> avlblValues = WLHBObs.ToList();
                     avlblDate.Add(DateTime.Today.AddHours(6));
                     avlblValues.Add(WLHBObs[x]);
                     HBdate = avlblDate.ToArray();
                     WLHBObs = avlblValues.ToArray();
                 }

                 con.Open();
                 cmd = new SqlCommand("Select Date, WLValue FROM GBMStationWL Where Station = 'Pankha' AND Date > @Date AND Date < = @today ORDER By Date ASC", con);
                 cmd.Parameters.AddWithValue("@today", DateTime.Today.AddHours(6));
                 cmd.Parameters.AddWithValue("@Date", bndstartdate);
                 adapter.SelectCommand = cmd;
                 adapter.Fill(dt);
                 adapter.Dispose();
                 cmd.Dispose();
                 con.Close();

                 float[] WLPanObs = new float[dt.Rows.Count];
                 List<DateTime> DatePanObs = new List<DateTime>();

                 x = 0;
                 foreach (DataRow dr in dt.Rows)
                 {
                     DatePanObs.Add(Convert.ToDateTime(dr[0]));
                     WLPanObs[x] = Convert.ToSingle(dr[1]);
                     x++;
                 }

                 if (DatePanObs.Contains(DateTime.Today.AddHours(6)) != true)
                 {
                     Console.WriteLine("Pankha Boundary cannot be created due to lack of observed data at Pankha.");
                     Console.WriteLine("The program will now copy previous data . . . . ");
                     List<float> avlblValues = WLPanObs.ToList();
                     DatePanObs.Add(DateTime.Today.AddHours(6));
                     avlblValues.Add(WLPanObs[x]);
                     WLPanObs = avlblValues.ToArray();
                 }

                 dt.Clear();
                 dt.Columns.Clear();

                 float correction = WLPanObs[WLPanObs.Length - 6] - WLHBObs[WLHBObs.Length - 6];
                 WLHBObs[WLHBObs.Length - 5] = WLPanObs[WLPanObs.Length - 5] - correction;
                 WLHBObs[WLHBObs.Length - 4] = WLPanObs[WLPanObs.Length - 4] - correction;
                 WLHBObs[WLHBObs.Length - 3] = WLPanObs[WLPanObs.Length - 3] - correction;
                 WLHBObs[WLHBObs.Length - 2] = WLPanObs[WLPanObs.Length - 2] - correction;
                 WLHBObs[WLHBObs.Length - 1] = WLPanObs[WLPanObs.Length - 1] - correction;
                 HBdate[HBdate.Length - 5] = HBdate[HBdate.Length - 6].AddHours(3);
                 HBdate[HBdate.Length - 4] = HBdate[HBdate.Length - 5].AddHours(3);
                 HBdate[HBdate.Length - 3] = HBdate[HBdate.Length - 4].AddHours(3);
                 HBdate[HBdate.Length - 2] = HBdate[HBdate.Length - 3].AddHours(3);
                 HBdate[HBdate.Length - 1] = HBdate[HBdate.Length - 2].AddHours(12);

                 for (int i = 0; i < WLHBObs.Length; i++)
                 {

                     if (WLHBObs[i] <= 10.66) { QHBObs[i] = Convert.ToSingle(13.85 * Math.Pow((WLHBObs[i] - 1.41), 2.993)); }
                     else { QHBObs[i] = Convert.ToSingle(125.2 * Math.Pow((WLHBObs[i] - 5.8), 2.817)); }
                     HBdate[i] = HBdate[i].AddHours(-24);
                 }

                 IDfsFile resFile = DfsFileFactory.DfsGenericOpenEdit(@"E:\FFWS\Model\MIKEHYDRO\GBM_MIKEHYDRO.mhydro - Result Files\RiverBasin_GBM.dfs0");
                 IDfsFileInfo resfileInfo = resFile.FileInfo;
                 int noTimeSteps = resfileInfo.TimeAxis.NumberOfTimeSteps;
                 IDfsItemData<float> data;

                 DateTime[] kuriDate = new DateTime[104];
                 float[] QSimvalues = new float[104];

                 bndstartdate = bndstartdate.AddHours(-24);
                 for (int j = 0; j < 104; j++)
                 {
                     kuriDate[j] = bndstartdate.AddHours(j * 3);
                     data = (IDfsItemData<float>)resFile.ReadItemTimeStep(1644, noTimeSteps - 104 + j);
                     QSimvalues[j] = Convert.ToSingle(data.Data[0]);
                 }
                 float[] QHindPan = new float[QHBObs.Length];

                 for (int j = 0; j < QHBObs.Length; j++)
                 {
                     for (int i = 0; i < QSimvalues.Length; i++)
                     {
                         if (HBdate[j] == kuriDate[i])
                         {
                             QHindPan[j] = QHBObs[j] - QSimvalues[i];
                         }
                     }
                 }
                 //Reading forecasted discharge of Pankha Station
                 IDfsItemData<float> data3;
                 DateTime[] kuri1Date = new DateTime[49];
                 float[] QforePan = new float[49];
                 for (int j = 0; j < 49; j++)
                 {
                     kuri1Date[j] = bndstartdate.AddHours(j * 3);
                     data3 = (IDfsItemData<float>)resFile.ReadItemTimeStep(1773, noTimeSteps - 49 + j);
                     QforePan[j] = Convert.ToSingle(data3.Data[0]);
                 }
                 float Qcorr = QHindPan[QHindPan.Length - 1] / QforePan[0];
                 //check.AppendLine("Discharge Correction: " + Qcorr);
                 for (int j = 0; j < QforePan.Length; j++)
                 {
                     QforePan[j] = QforePan[j] * Qcorr;
                 }

                 QforePan[QforePan.Length - 1] = QforePan[QforePan.Length - 2];

                 dischargeObsDFS0("Pankha", HBdate, QHindPan);
                 dischargeForeDFS0("Pankha", QforePan);

                 Console.ForegroundColor = ConsoleColor.Green;
                 Console.WriteLine("All the HD-Model Boundaries are completed.");
             }
             catch (Exception error)
             {
                 con.Close();
                 Console.ForegroundColor = ConsoleColor.Red;
                 Console.WriteLine("Error in Pankha Boundary Generation. Error: " + error.Message);
                 Console.ReadKey();
             }
            
             ///-------------------------------------------------Water Level Boundary Generation for FloodWatch Model---------------------------------------------------------------
             try
             {
                 string filename = @"E:\FFWS\ModelOutput\forFFWC\BndEstimate.txt";
                 StringBuilder sb = new StringBuilder();
                 try
                 {
                     string[] station = new string[] { "Amalshid", "Manu-RB", "Dinajpur", "Durgapur", "Gaibandha", "Kurigram", "Noonkhawa", "Pankha", "Faridpur", "Rohanpur", "Panchagarh", "Badarganj", "Dalia", "Nakuagaon", "Comilla", "Lourergorh", "Sarighat" };
                     DateTime forecastday = DateTime.Today;
                     string labeltext = forecastday.ToShortDateString();
                     sb.AppendLine("SL No." + "\t" + "Name" + "\t" + "\t\t" + "Forecast Water Level at " + forecastday.ToShortDateString());
                     //sb.AppendLine("SL No." + "\t" + "Name" + "\t" + "\t" + "River" + "\t\t" + "Forecast Water Level at " + forecastday.ToShortDateString());
                     sb.AppendLine("-------------------------------------------------------------------------");
                     sb.AppendLine("\t" + "\t\t" + "Today" + "\t" + "24 hour" + "\t" + "48 hour" + "\t" + "72 hour" + "\t" + "96 hour" + "\t" + "120 hour");
                     sb.AppendLine("\t\t\t" + "----------------------------------------------" + "\r\n");

                     int x = 0;
                     foreach (string element in station)
                     {
                         double[] QSimvalues = new double[6];
                         float[] WLSimvalues = new float[6];
                         if (element == "Gaibandha" || element == "Faridpur" || element == "Dinajpur")
                         {
                             IDfsFile obsBndFile = DfsFileFactory.DfsGenericOpenEdit(@"E:\FFWS\Model\FF\HDBounds\" + element + "-RT.dfs0");
                             IDfsFileInfo obsBndFileInfo = obsBndFile.FileInfo;
                             int count = obsBndFileInfo.TimeAxis.NumberOfTimeSteps;
                             IDfsItemData<float> obsData = (IDfsItemData<float>)obsBndFile.ReadItemTimeStep(1, count - 1);
                             QSimvalues[0] = Convert.ToSingle(obsData.Data[0]);

                             IDfsFile bndFile = DfsFileFactory.DfsGenericOpenEdit(@"E:\FFWS\Model\FF\HDBounds\" + element + "-FF.dfs0");
                             IDfsItemData<float> data;
                             for (int j = 1; j < 6; j++)
                             {
                                 data = (IDfsItemData<float>)bndFile.ReadItemTimeStep(1, 7 + j * 8);
                                 QSimvalues[j] = Convert.ToSingle(data.Data[0]);
                             }
                         }
                         else
                         {
                             IDfsFile obsBndFile = DfsFileFactory.DfsGenericOpenEdit(@"E:\FFWS\Model\FF\HDBounds\" + element + "-GQ-RT.dfs0");
                             IDfsFileInfo obsBndFileInfo = obsBndFile.FileInfo;
                             int count = obsBndFileInfo.TimeAxis.NumberOfTimeSteps;
                             IDfsItemData<float> obsData = (IDfsItemData<float>)obsBndFile.ReadItemTimeStep(1, count - 1);
                             QSimvalues[0] = Convert.ToSingle(obsData.Data[0]);

                             IDfsFile bndFile = DfsFileFactory.DfsGenericOpenEdit(@"E:\FFWS\Model\FF\HDBounds\" + element + "-GQ-FF.dfs0");
                             IDfsItemData<float> data;
                             for (int j = 1; j < 6; j++)
                             {
                                 data = (IDfsItemData<float>)bndFile.ReadItemTimeStep(1, 7 + j * 8);
                                 QSimvalues[j] = Convert.ToSingle(data.Data[0]);
                             }
                         }
                         for (int i = 0; i < 6; i++)
                         {
                             if (element == "Amalshid")
                             {
                                 if (QSimvalues[i] <= 407.25)
                                 {
                                     WLSimvalues[i] = Convert.ToSingle(4.5 + Math.Pow((QSimvalues[i] / 28.0), (1 / 1.7)));
                                 }
                                 else if (QSimvalues[i] > 407.25 && QSimvalues[i] <= 2182.42)
                                 {
                                     WLSimvalues[i] = Convert.ToSingle(5.75 + Math.Pow((QSimvalues[i] / 34.4), (1 / 1.91)));
                                 }
                                 else
                                 {
                                     WLSimvalues[i] = Convert.ToSingle(8.3 + Math.Pow((QSimvalues[i] / 39.0), (1 / 2.2)));
                                 }
                             }
                             else if (element == "Manu-RB")
                             {
                                 WLSimvalues[i] = Convert.ToSingle(11.8 + Math.Pow((QSimvalues[i] / 10.0), (1 / 2.25)));
                             }
                             else if (element == "Durgapur")
                             {
                                 WLSimvalues[i] = Convert.ToSingle(10.3 + Math.Pow((QSimvalues[i] / 124.0), (1 / 2.24)));
                             }
                             else if (element == "Gaibandha")
                             {
                                 WLSimvalues[i] = Convert.ToSingle(17.0 + Math.Pow((QSimvalues[i] / 2.0), (1.0 / 3.0)));
                             }
                             else if (element == "Kurigram")
                             {
                                 WLSimvalues[i] = Convert.ToSingle(22.85 + Math.Pow((QSimvalues[i] / 310.0), (1.0 / 2.95)));
                             }
                             else if (element == "Rohanpur")
                             {
                                 WLSimvalues[i] = Convert.ToSingle(11.4 + Math.Pow((QSimvalues[i] / 5.0), (1.0 / 2.7)));
                             }
                             else if (element == "Panchagarh")
                             {
                                 WLSimvalues[i] = Convert.ToSingle(67.3 + Math.Pow((QSimvalues[i] / 47.4), (1.0 / 2.23)));
                             }
                             else if (element == "Badarganj")
                             {
                                 WLSimvalues[i] = Convert.ToSingle(28.45 + Math.Pow((QSimvalues[i] / 130.44), (1.0 / 1.52)));
                             }
                             else if (element == "Dalia")
                             {
                                 WLSimvalues[i] = Convert.ToSingle(48.49 + Math.Pow((QSimvalues[i] / 3.64), (1.0 / 5.73)));
                             }
                             else if (element == "Nakuagaon")
                             {
                                 WLSimvalues[i] = Convert.ToSingle(19.6 + Math.Pow((QSimvalues[i] / 27.2), (1.0 / 1.69)));
                             }
                             else if (element == "Comilla")
                             {
                                 WLSimvalues[i] = Convert.ToSingle(6.72 + Math.Pow((QSimvalues[i] / 33.56), (1.0 / 1.59)));
                             }
                             else if (element == "Lourergorh")
                             {
                                 WLSimvalues[i] = Convert.ToSingle(5.0 + Math.Pow((QSimvalues[i] / 125.0), (1.0 / 2.5)));
                             }
                             else if (element == "Sarighat")
                             {
                                 WLSimvalues[i] = Convert.ToSingle(3.92 + Math.Pow((QSimvalues[i] / 3.34), (1.0 / 2.49)));
                             }
                             else if (element == "Faridpur")
                             {
                                 WLSimvalues[i] = Convert.ToSingle(0.09 + Math.Pow((QSimvalues[i] / 1.23), (1 / 2.46)));
                             }
                             else if (element == "Dinajpur")
                             {
                                 WLSimvalues[i] = Convert.ToSingle(25.0 + Math.Pow((QSimvalues[i] / 12.5), (1 / 1.3)));
                             }
                             else if (element == "Noonkhawa")
                             {
                                 WLSimvalues[i] = Convert.ToSingle(15.9 + Math.Pow((QSimvalues[i] / 21.0), (1 / 3.3)));
                             }
                             else if (element == "Pankha")
                             {
                                 WLSimvalues[i] = Convert.ToSingle(10.3 + Math.Pow((QSimvalues[i] / 29.9), (1 / 3.0)));
                             }
                             else if (element == "Gaibandha")
                             {
                                 WLSimvalues[i] = Convert.ToSingle(QSimvalues[i]);
                             }
                             else if (element == "Faridpur")
                             {
                                 WLSimvalues[i] = Convert.ToSingle(QSimvalues[i]);
                             }
                             else if (element == "Dinajpur")
                             {
                                 WLSimvalues[i] = Convert.ToSingle(QSimvalues[i]);
                             }
                         }

                         dt.Columns.Clear();
                         dt.Clear();
                         con.Open();
                         cmd = new SqlCommand("Select Date, WLValue FROM GBMStationWL Where Station = @station AND Date = @today", con);
                         cmd.Parameters.AddWithValue("@station", element);
                         cmd.Parameters.AddWithValue("@today", DateTime.Today.AddHours(6));
                         adapter.SelectCommand = cmd;
                         adapter.Fill(dt);
                         adapter.Dispose();
                         cmd.Dispose();
                         con.Close();
                         float correction = WLSimvalues[0] - Convert.ToSingle(dt.Rows[0].ItemArray[1]);

                         for (int i = 0; i < 6; i++)
                         {
                             WLSimvalues[i] = WLSimvalues[i] - correction;
                         }
                         if (element == "Manu-RB" || element == "Pankha" || element == "Dalia" || element == "Comilla")
                         {
                             sb.AppendLine((x + 1) + "\t" + element + "\t\t" + WLSimvalues[0].ToString("0.00") + "\t" + WLSimvalues[1].ToString("0.00") + "\t" + WLSimvalues[2].ToString("0.00") + "\t" + WLSimvalues[3].ToString("0.00") + "\t" + WLSimvalues[4].ToString("0.00") + "\t" + WLSimvalues[5].ToString("0.00"));
                         }
                         else
                         {
                             sb.AppendLine((x + 1) + "\t" + element + "\t" + WLSimvalues[0].ToString("0.00") + "\t" + WLSimvalues[1].ToString("0.00") + "\t" + WLSimvalues[2].ToString("0.00") + "\t" + WLSimvalues[3].ToString("0.00") + "\t" + WLSimvalues[4].ToString("0.00") + "\t" + WLSimvalues[5].ToString("0.00"));
                         }
                         x = x + 1;
                     }

                 }
                 catch (Exception error)
                 {
                     Console.WriteLine("Error in Water Level generation for FFWC Boundary. Error: " + error.Message);
                 }

                 ////------------------------------------------------- Rainfall Boundary Generation for FloodWatch model ------------------------------------------------
                 try
                 {
                     sb.AppendLine("======================================================================" + "\r\n");
                     sb.AppendLine("Boundary Rainfall Forecast" + "\r\n");
                     sb.AppendLine("-----------------------------------------------------------------------" + "\r\n");
                     sb.AppendLine("Catchment" + "\t\t\t\t" + "Forecasted Rainfall" + "\r\n");
                     sb.AppendLine("-----------------------------------------------------------------------");

                     string[] catchRainInfo = File.ReadAllLines(@"E:\FFWS\Batch\FFWCBoundary.txt");
                     string[] catchment = new string[catchRainInfo.Length];
                     string[] catchDfs0 = new string[catchRainInfo.Length];

                     for (int i = 0; i < catchRainInfo.Length; i++)
                     {
                         var separatedText = catchRainInfo[i].Split(',');
                         catchment[i] = separatedText[0];
                         catchDfs0[i] = separatedText[1];
                     }
                     string[] regeionName = catchment.Distinct().ToArray();

                     foreach (string element in regeionName)
                     {
                         float[] rainfall = new float[] { 0, 0, 0, 0, 0 };
                         int count = 0;
                         for (int i = 0; i < catchment.Length; i++)
                         {
                             if (element == catchment[i])
                             {
                                 count = count + 1;
                                 IDfsFile catchFile = DfsFileFactory.DfsGenericOpenEdit(@"E:\FFWS\Model\NAM\WRF-DFS0\" + catchDfs0[i]);
                                 IDfsItemData<float> data;
                                 for (int j = 1; j < 6; j++)
                                 {
                                     data = (IDfsItemData<float>)catchFile.ReadItemTimeStep(1, j);
                                     rainfall[j - 1] = rainfall[j - 1] + Convert.ToSingle(data.Data[0]);
                                 }
                             }
                         }
                         int serial = 0;
                         if (element == "NW North") { serial = 18; }
                         else if (element == "NW South") { serial = 19; }
                         else if (element == "NC North") { serial = 20; }
                         else if (element == "NC South") { serial = 21; }
                         else if (element == "NE North") { serial = 22; }
                         else if (element == "NE South") { serial = 23; }
                         else if (element == "SW") { serial = 24; }
                         else if (element == "SC") { serial = 25; }
                         else if (element == "SE") { serial = 26; }

                         for (int i = 0; i < 5; i++)
                         {
                             rainfall[i] = rainfall[i] / count;
                         }
                         if (element == "SW" || element == "SC" || element == "SE")
                         {
                             sb.AppendLine((serial - 17) + "\t" + element + "\t\t\t" + Math.Round(rainfall[0], 0) + "\t" + Math.Round(rainfall[1], 0) + "\t" + Math.Round(rainfall[2], 0) + "\t" + Math.Round(rainfall[3], 0) + "\t" + Math.Round(rainfall[4], 0));
                         }
                         else
                         {
                             sb.AppendLine((serial - 17) + "\t" + element + "\t\t" + Math.Round(rainfall[0], 0) + "\t" + Math.Round(rainfall[1], 0) + "\t" + Math.Round(rainfall[2], 0) + "\t" + Math.Round(rainfall[3], 0) + "\t" + Math.Round(rainfall[4], 0));
                         }
                     }
                     sb.AppendLine("======================================================================" + "\r\n");
                     File.WriteAllText(filename, sb.ToString());
                 }
                 catch (Exception error)
                 {
                     Console.WriteLine("Error in Rainfall generation for FFWC Boundary. Error: " + error.Message);
                 }
             }
             catch (Exception error)
             {
                 Console.WriteLine("BndEstimate text file cannot be generated due to an error. Error: " + error.Message);
             }
            
             ///-------------------------------------------------------------------------HD Model Simulation started------------------------------------------------
             try
             {
                 Console.ForegroundColor = ConsoleColor.Cyan;
                 Console.WriteLine("HD model is initiating....");
                 //Change .sim File end date
                 DateTime today = DateTime.Now;
                 string[] alllines = File.ReadAllLines(@"E:\FFWS\Model\FF\FF.sim11");
                 alllines[38] = "         start = " + today.AddDays(-7).Year + ", " + today.AddDays(-7).Month + ", " + today.AddDays(-7).Day + ", 6, 0, 0";
                 alllines[39] = "         end = " + today.Year + ", " + today.Month + ", " + today.Day + ", 6, 0, 0";
                 alllines[72] = "         hd = 2, |.\\Results\\FF-HD.RES11|, false, " + today.AddDays(-7).Year + ", " + today.AddDays(-7).Month + ", " + today.AddDays(-7).Day + ", 6, 0, 0";
                 alllines[75] = "         rr = 1, |.\\Results\\FF-RR.RES11|, false, " + today.AddDays(-7).Year + ", " + today.AddDays(-7).Month + ", " + today.AddDays(-7).Day + ", 6, 0, 0";
                 File.WriteAllLines(@"E:\FFWS\Model\FF\FF.sim11", alllines);
                
                 Console.ForegroundColor = ConsoleColor.Cyan;
                 Console.WriteLine("HD model Simulation started.");
                
                 ProcessStartInfo start = new ProcessStartInfo();
                 Process exeProcess = new Process();

                 start.FileName = @"C:\Program Files\DHI\2014\bin\mike11.exe";
                 start.Arguments = @"E:\FFWS\Model\FF\FF.sim11";
                 exeProcess = Process.Start(start);
                 exeProcess.WaitForExit();
                 Console.ForegroundColor = ConsoleColor.Green;
                 Console.WriteLine("HD Model Simulation Completed.");
                 Console.ForegroundColor = ConsoleColor.Cyan;
                 Console.WriteLine("Model Post Process Module is initiating......");
                 start.FileName = @"E:\FFWS\Programs\ResultAnalysis.exe";
                 exeProcess = Process.Start(start);
             }
             catch (Exception error)
             {
                 Console.ForegroundColor = ConsoleColor.Red;
                 Console.WriteLine("HD Model cannot be simulated due to an error. Error: " + error.Message);
                 Console.ReadKey();
             }
        }

        private static void waterLevelDFS0(string element, DateTime[] hindDate, float[] hindWL)
        {
            DfsFactory factory = new DfsFactory();
            string filename = @"E:\FFWS\Model\FF\HDBounds\" + element + ".dfs0";
            DfsBuilder filecreator = DfsBuilder.Create(element, element, 2014);
            filecreator.SetDataType(1);
            filecreator.SetGeographicalProjection(factory.CreateProjectionUndefined());
            filecreator.SetTemporalAxis(factory.CreateTemporalNonEqCalendarAxis(eumUnit.eumUsec, new DateTime(hindDate[0].Year, hindDate[0].Month, hindDate[0].Day, hindDate[0].Hour, hindDate[0].Minute, hindDate[0].Second)));
            filecreator.SetItemStatisticsType(StatType.RegularStat);
            DfsDynamicItemBuilder item = filecreator.CreateDynamicItemBuilder();
            item.Set(element, eumQuantity.Create(eumItem.eumIWaterLevel, eumUnit.eumUmeter), DfsSimpleType.Float);
            item.SetValueType(DataValueType.Instantaneous);
            item.SetAxis(factory.CreateAxisEqD0());
            item.SetReferenceCoordinates(1f, 2f, 3f);
            filecreator.AddDynamicItem(item.GetDynamicItemInfo());

            filecreator.CreateFile(filename);
            IDfsFile file = filecreator.GetFile();
            IDfsFileInfo fileinfo = file.FileInfo;

            for (int j = 0; j < hindWL.Length; j++)
            {
                file.WriteItemTimeStepNext((hindDate[j] - hindDate[0]).TotalSeconds, new float[] { hindWL[j] });
            }
            file.Close();
        }
        private static void dischargeDFS0(string element, List<DateTime> foreDate, float[] foreQ)
        {
            DfsFactory factory = new DfsFactory();
            string filename = @"E:\FFWS\Model\FF\HDBounds\" + element + ".dfs0";
            DfsBuilder filecreator = DfsBuilder.Create(element, element, 2014);
            filecreator.SetDataType(1);
            filecreator.SetGeographicalProjection(factory.CreateProjectionUndefined());
            filecreator.SetTemporalAxis(factory.CreateTemporalNonEqCalendarAxis(eumUnit.eumUsec, new DateTime(foreDate[0].Year, foreDate[0].Month, foreDate[0].Day, foreDate[0].Hour, foreDate[0].Minute, foreDate[0].Second)));
            filecreator.SetItemStatisticsType(StatType.RegularStat);
            DfsDynamicItemBuilder item = filecreator.CreateDynamicItemBuilder();
            item.Set(element, eumQuantity.Create(eumItem.eumIDischarge, eumUnit.eumUm3PerSec), DfsSimpleType.Float);
            item.SetValueType(DataValueType.Instantaneous);
            item.SetAxis(factory.CreateAxisEqD0());
            item.SetReferenceCoordinates(1f, 2f, 3f);
            filecreator.AddDynamicItem(item.GetDynamicItemInfo());

            filecreator.CreateFile(filename);
            IDfsFile file = filecreator.GetFile();
            IDfsFileInfo fileinfo = file.FileInfo;

            for (int j = 0; j < foreQ.Length; j++)
            {
                file.WriteItemTimeStepNext((foreDate[j] - foreDate[0]).TotalSeconds, new float[] { foreQ[j] });
            }
            file.Close();
        }
        private static void waterLevelForeDFS0(string element, float[] QSimvalues)
        {
            string ffwlfilename = @"E:\FFWS\Model\FF\HDBounds\" + element + "-FF.dfs0";
            DfsFactory ffwlfactory = new DfsFactory();
            DfsBuilder ffwlfilecreator = DfsBuilder.Create(element, element, 2012);
            ffwlfilecreator.SetDataType(1);
            ffwlfilecreator.SetGeographicalProjection(ffwlfactory.CreateProjectionUndefined());

            ffwlfilecreator.SetTemporalAxis(ffwlfactory.CreateTemporalNonEqCalendarAxis(eumUnit.eumUsec, new DateTime(DateTime.Today.AddHours(6).Year, DateTime.Today.AddHours(6).Month, DateTime.Today.AddHours(6).Day, DateTime.Today.AddHours(6).Hour, 00, 00)));
            ffwlfilecreator.SetItemStatisticsType(StatType.RegularStat);
            DfsDynamicItemBuilder ffwlitem = ffwlfilecreator.CreateDynamicItemBuilder();
            ffwlitem.Set(element, eumQuantity.Create(eumItem.eumIWaterLevel, eumUnit.eumUmeter), DfsSimpleType.Float);
            ffwlitem.SetValueType(DataValueType.Instantaneous);
            ffwlitem.SetAxis(ffwlfactory.CreateAxisEqD0());
            ffwlitem.SetReferenceCoordinates(1f, 2f, 3f);
            ffwlfilecreator.AddDynamicItem(ffwlitem.GetDynamicItemInfo());


            ffwlfilecreator.CreateFile(ffwlfilename);
            IDfsFile ffwlfile = ffwlfilecreator.GetFile();

            for (int i = 0; i < QSimvalues.Length; i++)
            {
                double secondInterval = 10800 * i;
                ffwlfile.WriteItemTimeStepNext(secondInterval, new float[] { QSimvalues[i] });
            }
            ffwlfile.Close();
        }
        private static void waterLevelObsDfs0(string element, DateTime[] datadate, float[] QObsvalues)
        {
            string rtwlfilename = @"E:\FFWS\Model\FF\HDBounds\" + element + "-RT.dfs0";
            DfsFactory rtwlfactory = new DfsFactory();
            DfsBuilder rtwlfilecreator = DfsBuilder.Create(element, element, 2012);
            rtwlfilecreator.SetDataType(1);
            rtwlfilecreator.SetGeographicalProjection(rtwlfactory.CreateProjectionUndefined());

            rtwlfilecreator.SetTemporalAxis(rtwlfactory.CreateTemporalNonEqCalendarAxis(eumUnit.eumUsec, new DateTime(datadate[0].Year, datadate[0].Month, datadate[0].Day, datadate[0].Hour, 00, 00)));
            rtwlfilecreator.SetItemStatisticsType(StatType.RegularStat);
            DfsDynamicItemBuilder rtwlitem = rtwlfilecreator.CreateDynamicItemBuilder();
            rtwlitem.Set(element, eumQuantity.Create(eumItem.eumIWaterLevel, eumUnit.eumUmeter), DfsSimpleType.Float);
            rtwlitem.SetValueType(DataValueType.Instantaneous);
            rtwlitem.SetAxis(rtwlfactory.CreateAxisEqD0());
            rtwlitem.SetReferenceCoordinates(1f, 2f, 3f);
            rtwlfilecreator.AddDynamicItem(rtwlitem.GetDynamicItemInfo());


            rtwlfilecreator.CreateFile(rtwlfilename);
            IDfsFile rtwlfile = rtwlfilecreator.GetFile();

            for (int i = 0; i < datadate.Length; i++)
            {
                double secondInterval = (datadate[i] - datadate[0]).TotalSeconds;
                rtwlfile.WriteItemTimeStepNext(secondInterval, new float[] { QObsvalues[i] });
            }
            rtwlfile.Close();
        }
        private static void dischargeForeDFS0(string element, float[] QSimvalues)
        {
            string fffilename = @"E:\FFWS\Model\FF\HDBounds\" + element + "-GQ-FF.dfs0";
            DfsFactory fffactory = new DfsFactory();
            DfsBuilder fffilecreator = DfsBuilder.Create(element, element, 2012);
            fffilecreator.SetDataType(1);
            fffilecreator.SetGeographicalProjection(fffactory.CreateProjectionUndefined());
            fffilecreator.SetTemporalAxis(fffactory.CreateTemporalNonEqCalendarAxis(eumUnit.eumUsec, new DateTime(DateTime.Today.AddHours(6).Year, DateTime.Today.AddHours(6).Month, DateTime.Today.AddHours(6).Day, DateTime.Today.AddHours(6).Hour, 00, 00)));
            fffilecreator.SetItemStatisticsType(StatType.RegularStat);
            DfsDynamicItemBuilder ffitem = fffilecreator.CreateDynamicItemBuilder();
            ffitem.Set(element, eumQuantity.Create(eumItem.eumIDischarge, eumUnit.eumUm3PerSec), DfsSimpleType.Float);
            ffitem.SetValueType(DataValueType.Instantaneous);
            ffitem.SetAxis(fffactory.CreateAxisEqD0());
            ffitem.SetReferenceCoordinates(1f, 2f, 3f);
            fffilecreator.AddDynamicItem(ffitem.GetDynamicItemInfo());


            fffilecreator.CreateFile(fffilename);
            IDfsFile fffile = fffilecreator.GetFile();

            for (int i = 1; i < QSimvalues.Length; i++)
            {
                double secondInterval = 10800 * i;
                fffile.WriteItemTimeStepNext(secondInterval, new float[] { QSimvalues[i] });
            }
            fffile.Close();
        }
        private static void dischargeObsDFS0(string element, DateTime[] datadate, float[] QObsvalues)
        {
            string rtfilename = @"E:\FFWS\Model\FF\HDBounds\" + element.Trim() + "-GQ-RT.dfs0";
            DfsFactory rtfactory = new DfsFactory();
            DfsBuilder rtfilecreator = DfsBuilder.Create(element, element, 2012);
            rtfilecreator.SetDataType(1);
            rtfilecreator.SetGeographicalProjection(rtfactory.CreateProjectionUndefined());
            rtfilecreator.SetTemporalAxis(rtfactory.CreateTemporalNonEqCalendarAxis(eumUnit.eumUsec, new DateTime(datadate[0].Year, datadate[0].Month, datadate[0].Day, datadate[0].Hour, 00, 00)));
            rtfilecreator.SetItemStatisticsType(StatType.RegularStat);
            DfsDynamicItemBuilder rtitem = rtfilecreator.CreateDynamicItemBuilder();
            rtitem.Set(element, eumQuantity.Create(eumItem.eumIDischarge, eumUnit.eumUm3PerSec), DfsSimpleType.Float);
            rtitem.SetValueType(DataValueType.Instantaneous);
            rtitem.SetAxis(rtfactory.CreateAxisEqD0());
            rtitem.SetReferenceCoordinates(1f, 2f, 3f);
            rtfilecreator.AddDynamicItem(rtitem.GetDynamicItemInfo());


            rtfilecreator.CreateFile(rtfilename);
            IDfsFile rtfile = rtfilecreator.GetFile();

            for (int i = 0; i < datadate.Length; i++)
            {
                double secondInterval = (datadate[i] - datadate[0]).TotalSeconds;
                rtfile.WriteItemTimeStepNext(secondInterval, new float[] { QObsvalues[i] });
            }
            rtfile.Close();
        }
    }
}
