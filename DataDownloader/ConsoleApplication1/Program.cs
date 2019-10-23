using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Net;
using HtmlAgilityPack;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Diagnostics;
using System.Timers;
using System.Threading;
using System.Windows.Forms;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.Globalization;

namespace ConsoleApplication1
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            ///------------------------------------------Initializing Parameters---------------------------------------------------------------
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.BackgroundColor = ConsoleColor.White;
            Console.WriteLine("Automated Flood Forecasting System of FMG Division, IWM.");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("-----------@@@ Data Download Module @@@----------");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Realtime Data Downloading started....");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Parameters is initiating......");
            Console.ResetColor();

            //--------------------------------------------------------Connecting database----------------------------------------------------

            SqlConnection con = new SqlConnection(@"Data Source=NKB-PC\SQLEXPRESS;AttachDbFilename=E:\FFWS\Database\GBMStationRF.mdf;Integrated Security=True;User Instance=True");
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            SqlDataAdapter ad = new SqlDataAdapter();

            StringBuilder logText = new StringBuilder();
            ///----------------------------------------------Clearing Temporary Database-----------------------------------------------------------
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Temporary database is clearing....");
                Console.ResetColor();

                cmd.CommandText = @"DELETE FROM TempGBMStationRF";
                cmd.Connection = con;
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception err)
            {
                con.Close();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Temporary database cannot be cleared, no data will be downloaded. Error: " + err.Message);
                Console.ResetColor();
                Console.ReadKey();
                Environment.Exit(1);
                logText.AppendLine("Temporary database cannot be cleared, no data will be downloaded. Error: " + err.Message);
            }
            
            ///-------------------------------------------------Guwahati Pdf--------------------------------------------------------------------------
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Data Download started....");
                Console.WriteLine("1st Website: http://www.imdguwahati.gov.in/dwr.pdf");
                Console.ResetColor();

                logText.AppendLine("Data Download started....");

                StringBuilder sb = new StringBuilder();
                PdfReader reader = new PdfReader("http://www.imdguwahati.gov.in/dwr.pdf"); //grabbing .pdf file using itextshap.pdf
                sb.Append(PdfTextExtractor.GetTextFromPage(reader, 1));  //getting all texts from .pdf file page number 1
                string text = sb.ToString(); //Extracting texts from stringbuilder
                var lines = text.Split(':');
                var spacedtext = lines[0].Split(' ');
                DateTime date = new DateTime();

                // Obtaining Date Time that was written in the PDF
                try
                {
                    if (spacedtext[22].Length == 9)
                    {
                        date = DateTime.ParseExact(spacedtext[22].Trim(), "d-MMM-yy", System.Globalization.CultureInfo.InvariantCulture).AddHours(9);
                    }

                    else if (spacedtext[22].Length >= 10)
                    {
                        date = DateTime.ParseExact(spacedtext[22].Trim().Substring(0, 9), "d-MMM-yy", System.Globalization.CultureInfo.InvariantCulture).AddHours(9);
                    }
                }
                catch (FormatException)
                {
                    date = DateTime.ParseExact(spacedtext[22].Trim().Substring(0, 8), "d-MMM-yy", System.Globalization.CultureInfo.InvariantCulture).AddHours(9);
                }

                char[] dispertext = new char[] { '=', ',', '&' };
                var stations = lines[1].Split(dispertext);
                sb.Clear();

                List<string> obtainedSt = new List<string>();
                List<string> obtainedRF = new List<string>();
                string[] givenSt = File.ReadAllLines(@"E:\FFWS\Batch\StationList_Guwahati.txt"); //Obtaining Given Station List
                string[] zeroRF = new string[givenSt.Length];
                for (int i = 0; i < zeroRF.Length; i++)
                {
                    zeroRF[i] = "0";
                }
                    for (int i = 0; i < stations.Length - 1; i++)
                    {
                        for (int j = i; j < stations.Length - 1; j++)
                        {
                            try
                            {
                                var values = stations[j].Trim().Split(' ');
                                int rain = int.Parse(values[0].Trim());
                                if (j == i || stations[i] == "")
                                {
                                    break;
                                }
                                else
                                {
                                    obtainedSt.Add(stations[i].Trim());
                                    obtainedRF.Add((rain * 10).ToString());  //obatining rf value and station name by comparing with the existing station Name
                                    break;
                                }
                            }
                            catch (FormatException)
                            {
                                continue;
                            }
                        }
                    }
                for (int i = 0; i < givenSt.Length; i++)
                {
                    for (int j = 0; j < obtainedSt.Count; j++)
                    {
                        if (String.Equals(givenSt[i].Trim(), obtainedSt[j].Trim(), StringComparison.CurrentCultureIgnoreCase) == true) //Comparison of string ignoring case sensitivity
                        {

                            zeroRF[i] = obtainedRF[j];
                            break;
                        }
                    }
                }

                // Inserting rf data into Temporary Database
                con.Open();
                for (int i = 0; i < givenSt.Length; i++)
                {
                    cmd = new SqlCommand("INSERT INTO TempGBMStationRF VALUES(@dataDate, @station, @obtainedRF, 8)", con);
                    cmd.Parameters.AddWithValue("@dataDate", date);
                    cmd.Parameters.AddWithValue("@station", givenSt[i].Trim());
                    cmd.Parameters.AddWithValue("@obtainedRF", zeroRF[i]);
                    cmd.ExecuteNonQuery();
                    Console.WriteLine(date.ToString() + "," + givenSt[i].Trim() + "," + zeroRF[i]);
                }
                con.Close();
                logText.AppendLine("Website: http://www.imdguwahati.gov.in/dwr.pdf, Downloaded Station: " + givenSt.Length);
            }
            catch (Exception error)
            {
                con.Close();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Problem in 1st Website: http://www.imdguwahati.gov.in/dwr.pdf, Error: " + error.Message);
                Console.ResetColor();
                logText.AppendLine("Problem Website: http://www.imdguwahati.gov.in/dwr.pdf, Error: " + error.Message);
            }

            ///------------------------------------CityWX Maxmin Moving Data--------------------------------------------------------------------------
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("2nd Website: http://202.54.31.7/citywx/max_min_rain.php");
                Console.ResetColor();

                WebBrowser wb = new WebBrowser();
                wb.Navigate("http://202.54.31.7/citywx/max_min_rain.php");
                while (wb.ReadyState != WebBrowserReadyState.Complete)
                {
                    Application.DoEvents(); // Waiting for the whole webpage page to load
                }

                wb.Document.ExecCommand("SelectAll", false, null);
                wb.Document.ExecCommand("Copy", false, null);
                string downloadtext = Clipboard.GetText(); //getting copied text
                DateTime datadate = new DateTime(int.Parse(downloadtext.Substring(33, 4)), int.Parse(downloadtext.Substring(30, 2)), int.Parse(downloadtext.Substring(27, 2)), 09, 00, 00); //Parsing DateTime from obtained text
                string firststeptext = downloadtext.Replace("        Max/Min Temp /24 hr RF(mm)   ", "");  //replacing "        Max/Min Temp /24 hr RF(mm)   "
                firststeptext = firststeptext.Substring(38, firststeptext.Length - 38);
                firststeptext = firststeptext.Replace(", ", ",");
                var textArray = firststeptext.Split(',');

                // Collecting rainfall for each station and store it to Temporary Database
                con.Open();
                for (int i = 0; i < textArray.Length - 1; i++)
                {
                    var individualstation = textArray[i].Split('/');
                    if (individualstation[0].Substring(individualstation[0].Length - 1, 1) == "-") { individualstation[0] = individualstation[0].Substring(0, individualstation[0].Length - 3); }
                    else { individualstation[0] = individualstation[0].Substring(0, individualstation[0].Length - 5); }
                    if (individualstation[2] == "NIL") { individualstation[2] = "0"; }
                    else if (individualstation[2] == "NA" || individualstation[2] == "999.90 mm") { individualstation[2] = "-"; }
                    else { individualstation[2] = individualstation[2].Substring(0, individualstation[2].Length - 3); }

                    cmd = new SqlCommand("INSERT INTO TempGBMStationRF VALUES(@dataDate, @individual, @individual2, 0)", con);
                    cmd.Parameters.AddWithValue("@dataDate", datadate);
                    cmd.Parameters.AddWithValue("@individual", individualstation[0].Trim());
                    cmd.Parameters.AddWithValue("@individual2", individualstation[2].Trim());
                    cmd.ExecuteNonQuery();
                    Console.WriteLine(datadate.ToString() + "," + individualstation[0].Trim() + "," + individualstation[2].Trim());

                }

                con.Close();
                logText.AppendLine("Website: http://202.54.31.7/citywx/max_min_rain.php, Downloaded Station= " + (textArray.Length - 1));
            }
            catch (Exception error)
            {
                con.Close();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Problem in 2nd Website: http://202.54.31.7/citywx/max_min_rain.php" + error.Message);
                Console.ResetColor();
                logText.AppendLine("Problem Website: http://202.54.31.7/citywx/max_min_rain.php" + error.Message);
            }
            
            ///---------------------------------------------------Wunderground Stations-----------------------------------------------------------
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("3rd Website: http://www.wunderground.com");
                Console.ResetColor();

                DateTime today = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 00, 00, 00);
                WebClient client = new WebClient();
                string[] stationNo = new string[] { "55591", "56312", "55578", "55279", "56247", "56144", "56146", "55773", "55299", "56651", "55696", "56137", "55228", "56106", "56116", "56739", "55664", "55472" };
                string[] stationName = new string[] { "Lhasa", "Nyingchi", "Xigaze", "Baingoin", "Batang", "Dege", "Garze", "Pagri", "Nagqu", "Lijing", "Lhunze", "Qamdo", "Shiquanhe", "Sog Xian", "Dengqen", "Tengchong", "Tingri", "Xainza" };
                for (int j = 0; j < 2; j++)
                {
                    for (int i = 0; i < stationNo.Length; i++)
                    {
                        try
                        {
                            string htmlCode = client.DownloadString("http://www.wunderground.com/history/station/" + stationNo[i] + "/" + today.Year + "/" + today.Month + "/" + today.Day + "/MonthlyHistory.html");
                            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                            doc.LoadHtml(htmlCode);
                            HtmlNodeCollection tables = doc.DocumentNode.SelectNodes("//table");
                            HtmlNodeCollection rows = tables[3].SelectNodes(".//tr");
                            HtmlNodeCollection col = rows[today.Day + 1].SelectNodes(".//td");

                            con.Open();
                            cmd = new SqlCommand("INSERT INTO TempGBMStationRF VALUES(@dataDate, @individual, @individual2, 1)", con);
                            cmd.Parameters.AddWithValue("@dataDate", today);
                            cmd.Parameters.AddWithValue("@individual", stationName[i].Trim());
                            cmd.Parameters.AddWithValue("@individual2", float.Parse(col[19].InnerText.Trim())*25.4);
                            cmd.ExecuteNonQuery();
                            con.Close();
                            Console.WriteLine(today.ToString() + "," + stationName[i].Trim() + "," + float.Parse(col[19].InnerText.Trim())*25.4);
                        }
                        catch (Exception)
                        {
                            con.Close();
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Problem in 3rd Website: " + stationName[i] + " data not found.");
                            Console.ResetColor();
                        }

                    }
                    today = today.AddHours(DateTime.Now.Hour);
                }
                logText.AppendLine("Website: http://www.wunderground.com/, Downloaded Station= " + (stationNo.Length));
            }
            catch (Exception err)
            {
                con.Close();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Problem in 3rd Website: http://www.wunderground.com/, Error: " + err.Message);
                Console.ResetColor();
                logText.AppendLine("Problem Website: http://www.wunderground.com/, Error: " + err.Message);
            }
            
            ///-----------------------------------------------MFD Data---------------------------------------------------------------------
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("4th Website: http://www.mfd.gov.np/");
                Console.ResetColor();

                DateTime today = DateTime.Today.AddDays(-1);
                StringBuilder sb = new StringBuilder();
                WebClient client = new WebClient();
                string htmlCode = client.DownloadString("http://www.mfd.gov.np/");
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(htmlCode);
                HtmlNodeCollection tables = doc.DocumentNode.SelectNodes("//table");
                HtmlNodeCollection header = doc.DocumentNode.SelectNodes(".//em");
                string hour = header[2].InnerText.Trim().Substring(11, 2);
                
                DateTime webDate = DateTime.Parse(header[2].InnerText.Trim().Substring(0, 10)).AddHours(double.Parse(hour) + 5 / 4);
                HtmlNodeCollection rows = tables[0].SelectNodes(".//tr");
                HtmlNodeCollection col;
                for (int i = 1; i < rows.Count - 1; i++)
                {
                    col = rows[i].SelectNodes(".//td");
                    con.Open();
                    cmd = new SqlCommand("INSERT INTO TempGBMStationRF VALUES(@dataDate, @individual, @individual2, 2)", con);
                    cmd.Parameters.AddWithValue("@dataDate", webDate);
                    cmd.Parameters.AddWithValue("@individual", col[0].InnerText.Trim());
                    cmd.Parameters.AddWithValue("@individual2", col[3].InnerText.Trim());
                    cmd.ExecuteNonQuery();
                    con.Close();
                    Console.WriteLine(webDate.ToString() + "," + col[0].InnerText.Trim() + "," + col[3].InnerText.Trim());
                }
                logText.AppendLine("Website: http://www.mfd.gov.np/, Downloaded Station= " + (rows.Count));
            }
            catch (Exception error)
            {
                con.Close();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Problem in 4th Website: http://www.mfd.gov.np/. Error: " + error.Message);
                Console.ResetColor();
                logText.AppendLine("Problem Website: http://www.mfd.gov.np/. Error: " + error.Message);
            }

            ///---------------------------------------------------City Weather Data----------------------------------------------------------
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("5th Website: http://202.54.31.7/citywx/city_weather.php");
                Console.ResetColor();

                StringBuilder sb = new StringBuilder();
                WebClient client = new WebClient();
                string[] stationID = new string[] { "42348", "42339", "42165", "42542", "42343", "42452", "42435", "42170", "42328", "42540", "42123", "42112", "42146", "42111", "42148", "99911", "42147", "42116", "99912", "42114", "99918", "42103", "42099", "42101", "42131", "99915", "42137", "00010", "99916", "42071", "99917", "42350", "42178", "42177", "42075", "42057", "42097", "42027", "42056", "42034", "42026", "42054", "42028", "42045", "42043", "42048", "42031", "42044", "42379", "42479", "42475", "42369", "42189", "42463", "42260", "42139", "42187", "42366", "42262", "42273", "42375", "42066", "42083", "42063", "42062", "42065", "8205", "42081", "42106", "42079" };
                string[] stationName = new string[] { "Jaipur", "Jodhpur", "Bikaner", "Udaipur", "Ajmer", "Kota", "Barmer", "Churu", "Jaisalmer", "Mount Abu", "Sriganganagar", "Mussorie", "Nainital", "Dehradun", "Pantnagar", "Pithoragarh", "Mukteshwar", "Joshimath", "Almora", "Tehri", "Haridwar", "Ambala", "Ludhiana", "Patiala", "Hissar", "Kurukshetra", "Karnal", "Chandigarh", "Sirsa", "Amritsar", "Anandpur Sahib", "Bhiwani", "Gurgaon", "Narnaul", "Jalandhar", "Pathankot", "Bhatinda", "Srinagar", "Jammu", "Leh", "Gulmarg", "Katra", "Pahalgam", "Banihal", "Batote", "Bhaderwah", "Kupwara", "Qazigund", "Gorakhpur", "Varanasi", "Allahabad", "Lucknow", "Bareilly", "Jhansi", "Agra", "Meerut", "Moradabad", "Kanpur", "Aligarh", "Bahraich", "Sultanpur", "Kalpa", "Shimla", "Keylong", "Dharamsala", "Manali", "Chamba", "Kullu", "Solan", "Sundernagar" };
                int count = 0;
                for (int i = 0; i < stationID.Length; i++)
                {
                    try
                    {
                        string htmlCode = client.DownloadString("http://202.54.31.7/citywx/city_weather.php?id=" + stationID[i]);
                        HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                        doc.LoadHtml(htmlCode);
                        HtmlNodeCollection boldtext = doc.DocumentNode.SelectNodes(".//b");
                        DateTime webDate = DateTime.Parse(boldtext[1].InnerText.Substring(7, 12)).AddHours(9);
                        HtmlNodeCollection tables = doc.DocumentNode.SelectNodes("//table");
                        HtmlNodeCollection rows = tables[0].SelectNodes(".//tr");
                        HtmlNodeCollection col = rows[6].SelectNodes(".//td");
                        //sb.AppendLine(DateTime.Today + "," + stationName[i] + "," + col[1].InnerText.Trim() + "," + "3");

                        con.Open();
                        cmd = new SqlCommand("INSERT INTO TempGBMStationRF VALUES(@dataDate, @individual, @individual2, 3)", con);
                        cmd.Parameters.AddWithValue("@dataDate", webDate);
                        cmd.Parameters.AddWithValue("@individual", stationName[i]);
                        cmd.Parameters.AddWithValue("@individual2", col[1].InnerText.Trim());
                        cmd.ExecuteNonQuery();
                        con.Close();

                        count = count + 1;
                        Console.WriteLine(webDate.ToString() + "," + stationName[i] + "," + col[1].InnerText.Trim());
                    }
                    catch (Exception)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Problem in 5th Website: " + stationName[i] + " data not found.");
                        Console.ResetColor();
                        continue;
                    }
                }
                logText.AppendLine("Website: http://202.54.31.7/citywx/city_weather.php, Downloaded station: = " + count);

            }
            catch (Exception error)
            {
                con.Close(); 
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Problem in 5th Website: http://202.54.31.7/citywx/city_weather.php. Error: " + error.Message);
                Console.ResetColor();
                logText.AppendLine("Problem Website: http://202.54.31.7/citywx/city_weather.php. Error: " + error.Message);
            }
            
            ///-----------------------------------------------City Weather1 Data----------------------------------------------------------------
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("6th Website: http://202.54.31.7/citywx/city_weather1.php");
                Console.ResetColor();

                StringBuilder sb = new StringBuilder();
                WebClient client = new WebClient();
                string[] stationID = new string[] { "30002", "42220", "42314", "42309", "42308", "42423", "42415", "42527", "42410","42406", "42516","42515", "42623", "42619", "42724", "42726"};
                string[] stationName = new string[] { "Anni", "Passighat", "Dibrugarh", "North Lakhimpur", "Itanagar", "Jorhat", "Tezpur", "Kohima", "Guwahati","Dhubri", "Shillong","Cherrapunji", "Imphal","Silchar", "Agartala", "Aizwal" };
                int count = 0;
                for (int i = 0; i < stationID.Length; i++)
                {
                    try
                    {
                        string htmlCode = client.DownloadString("http://202.54.31.7/citywx/city_weather1.php?id=" + stationID[i]);
                        HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                        doc.LoadHtml(htmlCode);
                        HtmlNodeCollection boldtext = doc.DocumentNode.SelectNodes(".//b");
                        DateTime webDate = DateTime.Parse(boldtext[1].InnerText.Substring(7, 12)).AddHours(9);
                        HtmlNodeCollection tables = doc.DocumentNode.SelectNodes("//table");
                        HtmlNodeCollection rows = tables[0].SelectNodes(".//tr");
                        HtmlNodeCollection col = rows[6].SelectNodes(".//td");
                        //sb.AppendLine(DateTime.Today + "," + stationName[i] + "," + col[1].InnerText.Trim() + "," + "3");

                        con.Open();
                        cmd = new SqlCommand("INSERT INTO TempGBMStationRF VALUES(@dataDate, @individual, @individual2, 3)", con);
                        cmd.Parameters.AddWithValue("@dataDate", webDate);
                        cmd.Parameters.AddWithValue("@individual", stationName[i]);
                        cmd.Parameters.AddWithValue("@individual2", col[1].InnerText.Trim());
                        cmd.ExecuteNonQuery();
                        con.Close();

                        count = count + 1;
                        Console.WriteLine(webDate.ToString() + "," + stationName[i] + "," + col[1].InnerText.Trim());
                    }
                    catch (Exception)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Problem in 6th Website: " + stationName[i] + " data not found");
                        Console.ResetColor();
                        continue;
                    }
                }
                logText.AppendLine("Website: http://202.54.31.7/citywx/city_weather1.php, Downloaded station: = " + count);

            }
            catch (Exception error)
            {
                con.Close();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Problem in 6th Website: http://202.54.31.7/citywx/city_weather1.php" + error.Message);
                Console.ResetColor();
                logText.AppendLine("Problem Website: http://202.54.31.7/citywx/city_weather1.php" + " data not downloaded");
            }

            ///-----------------------------------------------------FFWC Data---------------------------------------------------------------
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("7th Website: http://www.ffwc.gov.bd/");
                Console.ResetColor();

                StringBuilder sb = new StringBuilder();
                WebClient client = new WebClient();
                string htmlCode = client.DownloadString("http://www.ffwc.gov.bd/ffwc_charts/rainfall.php");
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(htmlCode);
                HtmlNodeCollection tables = doc.DocumentNode.SelectNodes("//table");
                HtmlNodeCollection rows = tables[0].SelectNodes(".//tr");
                HtmlNodeCollection col = rows[1].SelectNodes(".//td");
                //MessageBox.Show(col[2].InnerText.Trim());
                DateTime webDate = new DateTime(int.Parse(col[2].InnerText.Trim().Substring(6, 4)), int.Parse(col[2].InnerText.Trim().Substring(3, 2)), int.Parse(col[2].InnerText.Trim().Substring(0, 2)));

                for (int i = 0; i < rows.Count - 3; ++i)
                {
                    HtmlNodeCollection cols = rows[i + 3].SelectNodes(".//td");
                    if (cols.Count > 4 && cols[4].InnerText != "NP")
                    {
                        con.Open();
                        cmd = new SqlCommand("INSERT INTO TempGBMStationRF VALUES(@dataDate, @individual, @individual2, 4)", con);
                        cmd.Parameters.AddWithValue("@dataDate", webDate.AddHours(9));
                        cmd.Parameters.AddWithValue("@individual", cols[0].InnerText.Trim());
                        cmd.Parameters.AddWithValue("@individual2", cols[4].InnerText.Trim());
                        cmd.ExecuteNonQuery();
                        con.Close();
                        Console.WriteLine(webDate.AddHours(9).ToString() + "," + cols[0].InnerText.Trim() + "," + cols[4].InnerText);
                    }
                }
                logText.AppendLine("Website: http://www.ffwc.gov.bd/ffwc_charts/rainfall.php, Downloaded Station= " + (rows.Count - 3));
            }
            catch (Exception errtor)
            {
                con.Close();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Problem in 7th Website: http://www.ffwc.gov.bd/ffwc_charts/rainfall.php" + errtor.Message);
                Console.ResetColor();
                logText.AppendLine("Problem Website: http://www.ffwc.gov.bd/ffwc_charts/rainfall.php" + " data not downloaded");
            }

            ///----------------------------------------------------------------Hydrology Nepal Data---------------------------------------------------------
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("8th Website: http://hydrology.gov.np/");
                Console.ResetColor();

                DateTime today = DateTime.Today;
                StringBuilder sb = new StringBuilder();
                WebClient client = new WebClient();
                string[] deviceID = new string[] { "37", "56", "71", "72", "70", "69", "68", "28", "25", "26", "27", "23", "57", "58", "59", "60", "61", "64", "62", "63", "29", "30", "31", "33", "34", "35", "36", "67", "66", "65", "38", "39", "47", "45", "46", "44", "42", "43", "40", "94", "4", "9", "19", "15", "13", "16", "18", "5", "8", "51", "10", "20", "6", "7", "11", "17", "49", "74", "75", "76", "77", "78", "79", "80", "82", "91", "97", "53" };
                string[] stationID = new string[] { "68", "74", "84", "86", "87", "88", "89", "53", "54", "55", "56", "57", "71", "72", "73", "75", "76", "77", "78", "79", "60", "61", "62", "64", "65", "66", "67", "81", "82", "83", "43", "44", "45", "46", "47", "48", "49", "51", "52", "107", "19", "20", "22", "23", "25", "26", "27", "28", "30", "31", "32", "33", "34", "35", "36", "90", "70", "91", "92", "93", "94", "95", "96", "97", "99", "104", "108", "69" };
                string[] stationName = new string[] { "Karnali At Chisapani", "Bheri At Samaijighat", "Jajarkot", "Dailekh", "Karnali At Asaraghat", "Seti At Dipayal", "Mangalsen", "Babai At Chepang", "Gularia", "Tulsipur", "Ghorahi", "Rampur-Kalimati", "Ranijaruwa", "Salyan Bazar", "Luwamjyula", "Tharmare", "Jyamire", "Padampur", "Ambapur", "Ratmata", "West Rapti At Kusum", "West Rapti At Bagasoti", "Mari At Nayagaon", "Nepalgunj", "Dhakeri", "Lamahi", "Bijuwartar", "Libang Gaon", "Sulichour", "Swargadwari", "Budhigandaki At Arughat", "Kaligandaki At Kumalgaon", "Trishuli At Betrawati", "Narayani At Narayanghat", "Jomsom", "Beni", "Danda", "Gorkha", "East Rapti At Rajaiya", "Ghalekharkha", "Bagmati At Khokana", "Marin Khola At Kusumtar", "Thankot", "Godavari", "Babarmahal", "Nagarkot", "Budhanilkantha", "Lele", "Sindhulimadi", "Sindhuligadhi", "Bagmati At Bhorleni", "Bagmati At Karmaiya", "Chisapanigadhi", "Daman", "Garuda", "Sundarijal", "Koshi At Chatara", "Tamor At Mulghat", "Dhankuta", "Jiri", "Tamakoshi At Busti", "Sunkoshi At Pachuwarghat", "Tumlingtar", "Arun At Turkeghat", "Okhaldhunga", "Bhote Koshi At Bahrabise", "Dudh Koshi At Rabuawabazar", "Kankai At Mainachuli" };
                
                con.Open();

                for (int i = 0; i < deviceID.Length; i++)
                {
                    try
                    {
                        string htmlCode = client.DownloadString("http://hydrology.gov.np/new/bull3/index.php/hydrology/station/graph_view?deviceId=" + deviceID[i] + "&stationId=" + stationID[i] + "&categoryId=5&startDate=" + today.ToShortDateString() + "&type=daily");
                        HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                        doc.LoadHtml(htmlCode);
                        HtmlNodeCollection tables = doc.DocumentNode.SelectNodes("//table");
                        HtmlNodeCollection rows = tables[2].SelectNodes(".//tr");
                        HtmlNodeCollection col = rows[today.Day - 2].SelectNodes(".//td");
                        HtmlNodeCollection col2 = rows[today.Day - 1].SelectNodes(".//td");

                        cmd = new SqlCommand("INSERT INTO TempGBMStationRF VALUES(@dataDate, @individual, @individual2, 5)", con);
                        cmd.Parameters.AddWithValue("@dataDate", DateTime.Parse(col[0].InnerText.Trim()).AddHours(24));
                        cmd.Parameters.AddWithValue("@individual", stationName[i].Trim());
                        cmd.Parameters.AddWithValue("@individual2", col[1].InnerText.Trim());
                        cmd.ExecuteNonQuery();
                        
                        cmd = new SqlCommand("INSERT INTO TempGBMStationRF VALUES(@dataDate, @individual, @individual2, 5)", con);
                        cmd.Parameters.AddWithValue("@dataDate", DateTime.Parse(col2[0].InnerText.Trim()).AddHours(DateTime.Now.Hour));
                        cmd.Parameters.AddWithValue("@individual", stationName[i].Trim());
                        cmd.Parameters.AddWithValue("@individual2", col2[1].InnerText.Trim());
                        cmd.ExecuteNonQuery();

                        Console.WriteLine(DateTime.Parse(col[0].InnerText.Trim()).AddHours(24).ToString() + "," + stationName[i].Trim() + "," + col[1].InnerText.Trim());
                        Console.WriteLine(DateTime.Parse(col2[0].InnerText.Trim()).AddHours(DateTime.Now.Hour) + "," + stationName[i].Trim() + "," + col2[1].InnerText.Trim());

                    }
                    catch (FileNotFoundException error)
                    {
                        con.Close();
                        Console.WriteLine("Problem in 8th Website: http://hydrology.gov.np/, " + stationName[i] + " data not found.");
                        logText.AppendLine( stationName[i] + " " + error.Message);
                        continue;
                    }
                }
                logText.AppendLine("Website: http://hydrology.gov.np/new/bull3/index.php/hydrology/station, Downloaded Station= " + (deviceID.Length));
                con.Close();
            }
            catch (Exception error)
            {
                con.Close();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Problem in 8th Website: http://hydrology.gov.np/, Error: " + error.Message);
                Console.ResetColor();
                logText.AppendLine("Problem Website: http://hydrology.gov.np/new/bull3/index.php/hydrology/station, Error: " + error.Message);
            }
            
            ///--------------------------------------------Data from AMSS-Delhi Webpage------------------------------------------------------
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("9th Website: http://amssdelhi.gov.in/");
                Console.ResetColor();

                WebClient client = new WebClient();
                string htmlCode = client.DownloadString("http://amssdelhi.gov.in/dynamic/weather/wxtable.html");
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(htmlCode);

                HtmlNodeCollection date = doc.DocumentNode.SelectNodes(".//b");
                string titleText = date[1].InnerText;
                var tableTitle = titleText.Split('\r');
                DateTime webDate = DateTime.Parse(tableTitle[0].Substring(46, tableTitle[0].Length-46)).AddHours(9);
                HtmlNodeCollection tables = doc.DocumentNode.SelectNodes("//table");
                HtmlNodeCollection rows = tables[0].SelectNodes(".//tr");
                for (int i = 6; i < rows.Count - 6; i++)
                {
                    HtmlNodeCollection col = rows[i].SelectNodes(".//td");

                    con.Open();
                    cmd = new SqlCommand("INSERT INTO TempGBMStationRF VALUES(@dataDate, @individual, @individual2, 6)", con);
                    cmd.Parameters.AddWithValue("@dataDate", webDate);
                    cmd.Parameters.AddWithValue("@individual", col[0].InnerText.Trim());
                    cmd.Parameters.AddWithValue("@individual2", col[7].InnerText.Trim());
                    cmd.ExecuteNonQuery();
                    con.Close();
                    Console.WriteLine(webDate.ToString() + "," + col[0].InnerText.Trim() + "," + col[7].InnerText.Trim());
                }
                logText.AppendLine("Website: http://amssdelhi.gov.in/dynamic/weather/wxtable.html, Downloaded Station= " + (rows.Count - 6));
                
            }
            catch (Exception error)
            {
                con.Close();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Problem in 9th Website: http://amssdelhi.gov.in/dynamic/weather/wxtable.html, Error: " + error.Message);
                Console.ResetColor();
                logText.AppendLine("Problem Website: http://amssdelhi.gov.in/dynamic/weather/wxtable.html, Error: " + error.Message);
                //Console.ReadKey();
            }
            
            ///----------------------------------------------------Delhi Region Table----------------------------------------------
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("10th Website: http://121.241.116.157/dynamic/weather/delhiregion.html");
                Console.ResetColor();

                WebClient client = new WebClient();
                string htmlCode = client.DownloadString("http://121.241.116.157/dynamic/weather/delhiregion.html");
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(htmlCode);

                HtmlNodeCollection tables = doc.DocumentNode.SelectNodes("//table");
                HtmlNodeCollection bigfont = tables[0].SelectNodes(".//b");
                DateTime webDate = new DateTime();
                for (int i = 30; i < 45; i++)
                {
                    try
                    {
                        webDate = DateTime.ParseExact(bigfont[0].InnerText.Substring(i, 10), "dd.MM.yyyy", CultureInfo.InvariantCulture);
                        break;
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }

                HtmlNodeCollection rows = tables[0].SelectNodes(".//tr");
                for (int i = 2; i < rows.Count - 1; i++)
                {
                    HtmlNodeCollection col = rows[i].SelectNodes(".//td");
                    string stationname = col[0].InnerText.Trim();
                    if (stationname.Length > 7 && stationname.Substring(0, 6) == "NEW   ")
                    {
                        stationname = "NEW DELHI (PALAM AP)";
                    }
                    else if (stationname.Length > 7 && stationname.Substring(0, 7) == "UDAIPUR")
                    {
                        stationname = "UDAIPUR AP";
                    }
                    else if (stationname == @"&nbsp; BHUNTAR AP")
                    {
                        stationname = "BHUNTAR AP";
                    }

                    con.Open();
                    cmd = new SqlCommand("INSERT INTO TempGBMStationRF VALUES(@dataDate, @individual, @individual2, 7)", con);
                    cmd.Parameters.AddWithValue("@dataDate", webDate);
                    cmd.Parameters.AddWithValue("@individual", stationname.Trim());
                    cmd.Parameters.AddWithValue("@individual2", col[5].InnerText.Trim());
                    cmd.ExecuteNonQuery();
                    con.Close();
                    Console.WriteLine(webDate.ToString() + "," + stationname.Trim() + "," + col[5].InnerText.Trim());

                }
                logText.AppendLine("Website: http://121.241.116.157/dynamic/weather/delhiregion.html, Downloaded Station: " + (rows.Count - 2));
            
            }
            catch (Exception error)
            {
                con.Close();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Problem in 10th Website: http://121.241.116.157/dynamic/weather/delhiregion.html, Error: " + error.Message);
                Console.ResetColor();
                logText.AppendLine("Problem Website: http://121.241.116.157/dynamic/weather/delhiregion.html, Error: " + error.Message);
            }

            ///----------------------------------------------------------- IMD Sikkim PDF Data----------------------------------------------
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("11th Website: http://www.imdsikkim.gov.in/daily_Forecast.pdf");
                Console.ResetColor();

                StringBuilder sb = new StringBuilder();
                PdfReader reader = new PdfReader(@"http://www.imdsikkim.gov.in/daily_Forecast.pdf"); // Grabbing .pdf file
                sb.Append(PdfTextExtractor.GetTextFromPage(reader, 3));
                string text = sb.ToString(); // Convert to string collected from the pdf
                var tableText = text.Remove(0, text.IndexOf('0')).Split('\n');
                var dateText = tableText[0].Split(' ');
                DateTime date = DateTime.ParseExact(dateText[dateText.Length - 2], "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture).AddHours(9); // Date Time obtaining
                int counter = 0;
                for (int i = 11; i < tableText.Length - 2; i++)
                {
                    con.Open();
                    string station;
                    string rain;
                    var lines = tableText[i].Split(' ');
                    if (lines.Length == 5)
                    {
                        counter = counter + 1;
                        station = lines[0].Trim();
                        rain = lines[lines.Length - 2];
                        cmd = new SqlCommand("INSERT INTO TempGBMStationRF VALUES(@dataDate, @individual, @individual2, 9)", con);
                        cmd.Parameters.AddWithValue("@dataDate", date);
                        cmd.Parameters.AddWithValue("@individual", station);
                        cmd.Parameters.AddWithValue("@individual2", rain);
                        cmd.ExecuteNonQuery();
                        Console.WriteLine(date.ToString() + "," + station + "," + rain);
                    }
                    else if (lines.Length == 6)
                    {
                        counter = counter + 1;
                        station = lines[0].Trim() + " " + lines[1].Trim();
                        rain = lines[lines.Length - 2];
                        cmd = new SqlCommand("INSERT INTO TempGBMStationRF VALUES(@dataDate, @individual, @individual2, 9)", con);
                        cmd.Parameters.AddWithValue("@dataDate", date);
                        cmd.Parameters.AddWithValue("@individual", station);
                        cmd.Parameters.AddWithValue("@individual2", rain);
                        cmd.ExecuteNonQuery();
                        Console.WriteLine(date.ToString() + "," + station + "," + rain);
                    }
                    con.Close();

                }
                logText.AppendLine("Website: http://www.imdsikkim.gov.in/daily_Forecast.pdf, Downloaded Station: " + counter);
            }

            catch (Exception error)
            {
                con.Close();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Problem in 11th Website: http://www.imdsikkim.gov.in/daily_Forecast.pdf, Error: " + error.Message);
                Console.ResetColor();
                logText.AppendLine("Problem Website: http://www.imdsikkim.gov.in/daily_Forecast.pdf, Error: " + error.Message);
            }
            
            ///---------------------------------------------------------- Weather Delhi PDF --------------------------------------------
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("12th Website: http://121.241.116.157/dynamic/weather/Delhi.pdf");
                Console.ResetColor();

                StringBuilder sb = new StringBuilder();
                PdfReader reader = new PdfReader(@"http://121.241.116.157/dynamic/weather/Delhi.pdf");
                sb.Append(PdfTextExtractor.GetTextFromPage(reader, 1));
                string text = sb.ToString();
                string[] delimiters = new string[] { "\n", " ", ":" };
                var splittedText = text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                sb.Clear();

                List<string> stringlist = new List<string>(splittedText);
                stringlist.Remove("Sports");
                stringlist.Remove("Cplx");
                stringlist.Remove("Univ");
                stringlist.Remove("NOIDA");
                stringlist.Remove("Road");
                stringlist[stringlist.IndexOf("Yamuna")] = "Yamuna Sports Cplx";
                stringlist[stringlist.IndexOf("Delhi")] = "Delhi University";
                stringlist[stringlist.IndexOf("NCMRWF")] = "NCMRWF NOIDA";
                stringlist[stringlist.IndexOf("Lodhi")] = "Lodhi Road";
                string[] textDelhi = stringlist.ToArray();

                DateTime date = DateTime.ParseExact(textDelhi[19], "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture).AddHours(9);

                con.Open();

                int counter = 0;
                int x = 0;
                for (int i = 27; i < textDelhi.Length; i++)
                {
                    if (textDelhi[i].Trim() != "Max" && textDelhi[i].Trim() != "R/F" && textDelhi[i].Trim() != "**" && textDelhi[i].Trim() != "Min" && textDelhi[i].Trim() != "trace" && textDelhi[i].Trim() != "TRACE" && textDelhi[i].Trim() != "Trace")
                    {
                        try
                        {
                            float foul = float.Parse(textDelhi[i].Trim());
                            continue;
                        }
                        catch (FormatException)
                        {

                            for (int j = x; j < textDelhi.Length; j++)
                            {
                                if (textDelhi[j].Trim() == "R/F")
                                {
                                    counter = counter + 1;
                                    cmd = new SqlCommand("INSERT INTO TempGBMStationRF VALUES(@dataDate, @individual, @individual2, 10)", con);
                                    cmd.Parameters.AddWithValue("@dataDate", date);
                                    cmd.Parameters.AddWithValue("@individual", textDelhi[i].Trim());
                                    cmd.Parameters.AddWithValue("@individual2", textDelhi[j + 1]);
                                    cmd.ExecuteNonQuery();
                                    Console.WriteLine(date + "," + textDelhi[i].Trim() + "," + textDelhi[j + 1]);
                                    x = j + 1;
                                    break;
                                }
                            }

                            continue;
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
                con.Close();
                logText.AppendLine("Website: http://121.241.116.157/dynamic/weather/Delhi.pdf, Downloaded Station: " + counter);
            }
            catch(Exception error)
            {
                con.Close();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Problem in 12th Website: http://121.241.116.157/dynamic/weather/Delhi.pdf, Error: " + error.Message);
                Console.ResetColor();
                logText.AppendLine("Problem Website: http://121.241.116.157/dynamic/weather/Delhi.pdf, Error: " + error.Message);
            }
            
            ///---------------------------------------------------------------------Bangladesh Meteorological Board data--------------------------------------------------------------
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("13th Website: http://www.bmd.gov.bd/");
                Console.ResetColor();

                StringBuilder sb = new StringBuilder();
                WebClient client = new WebClient();
                int[] stationIndex = new int[] { 42, 45, 57, 64, 65, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 18, 58, 59, 61, 62, 63, 25, 78, 79, 80, 81, 83, 84, 85, 86, 87, 89, 90, 91, 92, 93, 95, 96, 97 };
                string[] stationName = new string[] { "Sandwip", "Sitakundu", "Tangail", "Rangamati", "Comilla", "Chandpur", "Maijdi Court", "Feni", "Hatiya", "Cox Bazar", "Kutubdia", "Teknaf", "Saint Martin", "Dighinala", "Bandarban", "Mymensingh", "Faridpur", "Madaripur", "Gopalganj", "Netrokona", "Nikli", "Srimangal", "Ishurdi", "Bogra", "Badalgachhi", "Tarash", "Dinajpur", "Sayedpur", "Rajarhat", "Dimla", "Tetulia", "Mongla", "Satkhira", "Jessore", "Chuadanga", "Kumarkhali", "Patuakhali", "Khepupara", "Bhola" };

                
                List<DateTime> stationDate = new List<DateTime>();
                List<float> stationRF = new List<float>();

                con.Open();
                int counter = 0;
                for (int j = 0; j < stationIndex.Length; j++)
                {
                    try
                    {
                        string htmlCode = client.DownloadString("http://www.bmd.gov.bd/?/wchart/=" + stationIndex[j]);
                        HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                        doc.LoadHtml(htmlCode);
                        HtmlNodeCollection tables = doc.DocumentNode.SelectNodes("//script");
                        string[] text = tables[4].InnerText.Split('[', ']');
                        var dataText = text[2].Split(new string[] { "{", "}" }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < dataText.Length; i++)
                        {
                            if (dataText[i].Length >= 2 && dataText[i].Substring(0, 2) != "  " && dataText[i].Substring(0, 1) != "\t")
                            {
                                var lastStepText = dataText[i].Split(',', ':');
                                stationDate.Add(DateTime.Parse(lastStepText[3].Trim().Substring(1, lastStepText[3].Trim().Length - 2)).AddHours(9));
                                stationRF.Add(float.Parse(lastStepText[1].Trim()));
                                counter = counter + 1;
                            }
                        }
                        if(stationDate.Contains(DateTime.Today.AddHours(9)) != true)
                        {
                            stationDate.Add(DateTime.Today.AddHours(9));
                            stationRF.Add(0f);
                        }
                        for (int i = 0; i < stationDate.Count; i++)
                        {
                            Console.WriteLine(stationDate[i] + "," + stationName[j] + "-BMD" + "," + stationRF[i]);
                            cmd = new SqlCommand("INSERT INTO TempGBMStationRF VALUES(@dataDate, @individual, @individual2, 11)", con);
                            cmd.Parameters.AddWithValue("@dataDate", stationDate[i]);
                            cmd.Parameters.AddWithValue("@individual", (stationName[j] + "-BMD"));
                            cmd.Parameters.AddWithValue("@individual2", stationRF[i]);
                            cmd.ExecuteNonQuery();
                        }
                        stationDate.Clear();
                        stationRF.Clear();
                    }
                    catch (Exception)
                    {
                        con.Close();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Problem in 13th Website: http://www.bmd.gov.bd/, Error: " + stationName[j] + " data not found.");
                        Console.ResetColor();
                        logText.AppendLine("Problem Website: http://www.bmd.gov.bd/, Error: " + stationName[j] + " data not found.");
                        continue;
                    }
                }
                con.Close();
                logText.AppendLine("Website: http://www.bmd.gov.bd/, Downloaded Station: " + counter);
            }
            catch (Exception error)
            {
                con.Close();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Problem in 13th Website: http://www.bmd.gov.bd/, Error: " + error.Message);
                Console.ResetColor();
                logText.AppendLine("Problem Website: http://www.bmd.gov.bd/, Error: " + error.Message);
            }

            ///------------------------------------------------Central Water Commission(CWC) Data Download----------------------------------------------/////
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("http://www.cwc.gov.in/");
                Console.ResetColor();
                con.Open();
                string[] stationCode = File.ReadAllLines(@"E:\FFWS\Batch\CWCStationID.txt");
                foreach (string element in stationCode)
                {
                    WebRequest req = WebRequest.Create("http://www.india-water.gov.in/ffs/data-flow-list-based/flood-forecasted-site/");
                    string postData = "lstStation=" + element;

                    byte[] send = Encoding.Default.GetBytes(postData);
                    req.Method = "POST";
                    req.ContentType = "application/x-www-form-urlencoded";
                    req.ContentLength = send.Length;

                    Stream sout = req.GetRequestStream();
                    sout.Write(send, 0, send.Length);
                    sout.Flush();
                    sout.Close();

                    WebResponse res = req.GetResponse();
                    StreamReader sr = new StreamReader(res.GetResponseStream());
                    string returnvalue = sr.ReadToEnd();

                    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(returnvalue);
                    HtmlNode header = doc.DocumentNode.SelectSingleNode("//h4");
                    string stationName = header.InnerText.Replace("Site Name : ", "").Trim() + "-CWC";

                    HtmlNodeCollection tables = doc.DocumentNode.SelectNodes("//table");
                    HtmlNodeCollection rows = tables[1].SelectNodes(".//tr");
                    HtmlNodeCollection colWL = rows[1].SelectNodes(".//td");
                    HtmlNodeCollection colRF = rows[3].SelectNodes(".//td");
                    if (colWL[0].InnerText.Trim() != "")
                    {
                        DateTime dateWL = DateTime.ParseExact(colWL[0].InnerText.Replace("Date: ", "").Trim(), "dd-MM-yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture).AddHours(0.5);
                        float valueWL = float.Parse(colWL[1].InnerText.Replace("Value: ", "").Replace(" Meters (m)", ""));
                        //sb.AppendLine("WL-" + stationName + "," + dateWL.ToString("yyyy-MM-dd HH:mm:ss") + "," + valueWL);
                        Console.WriteLine(stationName + "," + dateWL.ToString("yyyy-MM-dd HH:mm:ss") + "," + "WL-" + valueWL);
                        try
                        {
                            cmd = new SqlCommand("INSERT INTO GBMStationWL VALUES(@dataDate, @station, @wl, 1)", con);
                            cmd.Parameters.AddWithValue("@dataDate", dateWL);
                            cmd.Parameters.AddWithValue("@station", stationName);
                            cmd.Parameters.AddWithValue("@wl", valueWL);
                            cmd.ExecuteNonQuery();
                        }
                        catch (SqlException)
                        {
                            cmd = new SqlCommand("Update GBMStationWL SET WLValue = @wl Where Date = @dataDate AND Station = @station", con);
                            cmd.Parameters.AddWithValue("@dataDate", dateWL);
                            cmd.Parameters.AddWithValue("@station", stationName);
                            cmd.Parameters.AddWithValue("@wl", valueWL);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    if (colRF[0].InnerText.Trim() != "NOT AVAILABLE")
                    {
                        DateTime dateRF = DateTime.ParseExact(colRF[0].InnerText.Replace("Date: ", ""), "dd-MM-yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture).AddHours(0.5);
                        float valueRF = float.Parse(colRF[1].InnerText.Replace("Value: ", "").Replace(" Milimiters (mm)", ""));
                        //sb.AppendLine("RF-" + stationName + "," + dateRF.ToString("yyyy-MM-dd HH:mm:ss") + "," + valueRF);
                        Console.WriteLine(stationName + "," + dateRF.ToString("yyyy-MM-dd HH:mm:ss") + "," + "RF-" + valueRF);
                        try
                        {
                            cmd = new SqlCommand("INSERT INTO GBMStationRF VALUES(@dataDate, @station, @rf, 12)", con);
                            cmd.Parameters.AddWithValue("@dataDate", dateRF);
                            cmd.Parameters.AddWithValue("@station", stationName);
                            cmd.Parameters.AddWithValue("@rf", valueRF);
                            cmd.ExecuteNonQuery();
                        }
                        catch (SqlException)
                        {
                            cmd = new SqlCommand("Update GBMStationRF SET RFValue = @rf Where Date = @dataDate AND StationName = @station", con);
                            cmd.Parameters.AddWithValue("@dataDate", dateRF);
                            cmd.Parameters.AddWithValue("@station", stationName);
                            cmd.Parameters.AddWithValue("@rf", valueRF);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                con.Close();
                logText.AppendLine("Website: http://www.cwc.gov.in/, Downloaded Station: " + stationCode.Length);

            }
            catch (Exception error)
            {
                con.Close();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Problem Website: http://www.cwc.gov.in/, Error: " + error.Message);
                Console.ResetColor();
                logText.AppendLine("Problem Website: http://www.cwc.gov.in/, Error: " + error.Message);
            }
            
            ///-------------------------------------- Correcting irrelevant Values in the Temporary Database--------------------------
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Data preprocess started........");
                logText.AppendLine("Data preprocess started........");
                con.Open();
                cmd = new SqlCommand(@"UPDATE TempGBMStationRF SET  RFValue = '0' WHERE RFValue = 'NIL'", con);
                cmd.ExecuteNonQuery();
                cmd = new SqlCommand(@"UPDATE TempGBMStationRF SET  RFValue = '0' WHERE RFValue = 'NILL'", con);
                cmd.ExecuteNonQuery();
                cmd = new SqlCommand(@"UPDATE TempGBMStationRF SET  RFValue = '0' WHERE RFValue = 'N'", con);
                cmd.ExecuteNonQuery();
                cmd = new SqlCommand(@"UPDATE TempGBMStationRF SET  RFValue = '0.1' WHERE RFValue = 'TRACE'", con);
                cmd.ExecuteNonQuery();
                cmd = new SqlCommand(@"UPDATE TempGBMStationRF SET  RFValue = '0.1' WHERE RFValue = 'Traces'", con);
                cmd.ExecuteNonQuery();
                cmd = new SqlCommand(@"UPDATE TempGBMStationRF SET  RFValue = '-' WHERE RFValue = '999'", con);
                cmd.ExecuteNonQuery();
                cmd = new SqlCommand(@"UPDATE TempGBMStationRF SET  RFValue = '-' WHERE RFValue = '9999'", con);
                cmd.ExecuteNonQuery();
                cmd = new SqlCommand(@"UPDATE TempGBMStationRF SET  RFValue = '-' WHERE RFValue = 'NA'", con);
                cmd.ExecuteNonQuery();
                cmd = new SqlCommand(@"UPDATE TempGBMStationRF SET  RFValue = '-' WHERE RFValue = '999.99'", con);
                cmd.ExecuteNonQuery();
                cmd = new SqlCommand(@"UPDATE TempGBMStationRF SET  RFValue = '0.1' WHERE RFValue = 'TR'", con);
                cmd.ExecuteNonQuery();
                cmd = new SqlCommand(@"UPDATE TempGBMStationRF SET  RFValue = '0.1' WHERE RFValue = 'TR.'", con);
                cmd.ExecuteNonQuery();
                cmd = new SqlCommand(@"UPDATE TempGBMStationRF SET  RFValue = '0.1' WHERE RFValue = 'T'", con);
                cmd.ExecuteNonQuery();
                cmd = new SqlCommand(@"UPDATE TempGBMStationRF SET  RFValue = '-' WHERE RFValue = 'N/A'", con);
                cmd.ExecuteNonQuery();
                cmd = new SqlCommand(@"UPDATE TempGBMStationRF SET  RFValue = '-' WHERE RFValue = '*'", con);
                cmd.ExecuteNonQuery();
                cmd = new SqlCommand(@"UPDATE TempGBMStationRF SET  RFValue = '-' WHERE RFValue = '**'", con);
                cmd.ExecuteNonQuery();
                con.Close();
                Console.WriteLine("Data preprocess completed successfully.");
                logText.AppendLine("Data preprocess completed successfully.");
            }
            catch (Exception error)
            {
                con.Close();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(error.Message);
                Console.ResetColor();
                logText.AppendLine(error.Message);
            }

            /// ------------------------------------ Copying data from Temporary Database to the final Database ----------------------------
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Final Database updating started.......");
                logText.AppendLine("Final Database updating started.......");

                DateTime dateval = new DateTime(DateTime.Today.AddDays(-1).Year, DateTime.Today.AddDays(-1).Month, DateTime.Today.AddDays(-1).Day, 00, 00, 00);
                DataSet dataSet = new DataSet();

                con.Open();
                cmd = new SqlCommand(@"DELETE GBMStationRF Where WebID='5' AND Date > @dateval", con);
                cmd.Parameters.AddWithValue("@dateval", dateval);
                cmd.ExecuteNonQuery();
                con.Close();

                con.Open();
                cmd = new SqlCommand(@"DELETE GBMStationRF Where WebID='1' AND Date > @dateval", con);
                cmd.Parameters.AddWithValue("@dateval", dateval);
                cmd.ExecuteNonQuery();
                con.Close();

                con.Open();
                cmd = new SqlCommand(@"DELETE GBMStationRF Where WebID='0' AND Date > @dateval", con);
                cmd.Parameters.AddWithValue("@dateval", DateTime.Today);
                cmd.ExecuteNonQuery();
                con.Close();

                con.Open();
                cmd = new SqlCommand(@"DELETE GBMStationRF Where WebID='2' AND Date > @dateval", con);
                cmd.Parameters.AddWithValue("@dateval", DateTime.Today);
                cmd.ExecuteNonQuery();
                con.Close();

                con.Open();
                cmd = new SqlCommand(@"DELETE GBMStationRF Where WebID='3' AND Date > @dateval", con);
                cmd.Parameters.AddWithValue("@dateval", DateTime.Today);
                cmd.ExecuteNonQuery();
                con.Close();

                con.Open();
                cmd = new SqlCommand(@"DELETE GBMStationRF Where WebID='4' AND Date > @dateval", con);
                cmd.Parameters.AddWithValue("@dateval", DateTime.Today);
                cmd.ExecuteNonQuery();
                con.Close();

                con.Open();
                cmd = new SqlCommand(@"DELETE GBMStationRF Where WebID='6' AND Date > @dateval", con);
                cmd.Parameters.AddWithValue("@dateval", DateTime.Today);
                cmd.ExecuteNonQuery();
                con.Close();

                con.Open();
                cmd = new SqlCommand(@"DELETE GBMStationRF Where WebID='7' AND Date >= @dateval", con);
                cmd.Parameters.AddWithValue("@dateval", DateTime.Today);
                cmd.ExecuteNonQuery();
                con.Close();

                con.Open();
                cmd = new SqlCommand("Select *from TempGBMStationRF", con);
                DataTable rawdata = new DataTable();
                SqlDataAdapter dataAdapter = new SqlDataAdapter();
                dataAdapter.SelectCommand = cmd;
                dataAdapter.Fill(rawdata);
                dataAdapter.Dispose();
                cmd.Dispose();
                con.Close();

                con.Open();
                foreach (DataRow dr in rawdata.Rows)
                {
                    try
                    {
                        float rf = Convert.ToSingle(dr[2]);
                        try
                        {
                            cmd = new SqlCommand("INSERT INTO GBMStationRF VALUES(@dataDate, @station, @rfValue, @webID)", con);
                            cmd.Parameters.AddWithValue("@dataDate", dr[0]);
                            cmd.Parameters.AddWithValue("@station", dr[1]);
                            cmd.Parameters.AddWithValue("@rfValue", dr[2]);
                            cmd.Parameters.AddWithValue("@webID", dr[3]);
                            cmd.ExecuteNonQuery();
                        }
                        catch (SqlException)
                        {
                            cmd = new SqlCommand("Update GBMStationRF SET RFValue = @rfValue,  WebID = @webID Where Date = @dataDate AND StationName= @station", con);
                            cmd.Parameters.AddWithValue("@dataDate", dr[0]);
                            cmd.Parameters.AddWithValue("@station", dr[1]);
                            cmd.Parameters.AddWithValue("@rfValue", dr[2]);
                            cmd.Parameters.AddWithValue("@webID", dr[3]);
                        }
                    }
                    catch (FormatException)
                    {
                        try
                        {
                            cmd = new SqlCommand("INSERT INTO GBMStationRF VALUES(@dataDate, @station, @rfValue, @webID)", con);
                            cmd.Parameters.AddWithValue("@dataDate", dr[0]);
                            cmd.Parameters.AddWithValue("@station", dr[1]);
                            cmd.Parameters.AddWithValue("@rfValue", "-");
                            cmd.Parameters.AddWithValue("@webID", dr[3]);
                            cmd.ExecuteNonQuery();
                        }
                        catch (SqlException)
                        {
                            cmd = new SqlCommand("Update GBMStationRF SET RFValue = @rfValue,  WebID = @webID Where Date = @dataDate AND StationName= @station", con);
                            cmd.Parameters.AddWithValue("@dataDate", dr[0]);
                            cmd.Parameters.AddWithValue("@station", dr[1]);
                            cmd.Parameters.AddWithValue("@rfValue", "-");
                            cmd.Parameters.AddWithValue("@webID", dr[3]);
                        } 
                    }
                }
                con.Close();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Final Database updated successfully.");
                logText.AppendLine("Final Database updated successfully.");

            }
            catch (Exception error)
            {
                con.Close();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error in updating final database. Error: " + error.Message);
                Console.ResetColor();
                logText.AppendLine("Error in updating final database. Error: " + error.Message);
            }

            /// ------------------------------ Preparing textfile for RainMap -------------------------------------------
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Realtime Rain Map is Preparing.....");
                logText.AppendLine("Realtime Rain Map is Preparing.....");

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Point");
                DateTime rftxtDate = DateTime.Today;
                DateTime rftxtDate2 = rftxtDate.AddDays(1);

                cmd = new SqlCommand("Select Date, StationName, RFValue from GBMStationRF Where Date>= @startDate AND Date < @endDate", con);
                cmd.Parameters.AddWithValue("@startDate", rftxtDate);
                cmd.Parameters.AddWithValue("@endDate", rftxtDate2);
                con.Open();
                dt.Clear();
                dt.Columns.Clear();
                ad.SelectCommand = cmd;
                ad.Fill(dt);
                ad.Dispose();
                cmd.Dispose();
                con.Close();
                cmd.Parameters.Clear();
                DataTable rftable = dt;
                cmd = new SqlCommand("Select *from StationLocation", con);
                con.Open();
                dt.Clear();
                dt.Columns.Clear();
                ad.SelectCommand = cmd;
                ad.Fill(dt);
                ad.Dispose();
                cmd.Dispose();
                con.Close();
                cmd.Parameters.Clear();
                DataTable latlontable =dt;
                
                ///------------------------------------------------------------------For Checking purpose--------------------------------------------------//
                StringBuilder testStation = new StringBuilder();
                List<string> rqrdStation = new List<string>();
                List<string> rainfall = new List<string>();

                foreach (DataRow drXY in latlontable.Rows)
                {
                    rqrdStation.Add(drXY[0].ToString().Trim());
                    rainfall.Add("-");
                }

                for (int i = 0; i < rqrdStation.Count; i++)
                {
                    foreach (DataRow drRF in rftable.Rows)
                    {
                        if (string.Equals(rqrdStation[i], drRF[1].ToString().Trim(), StringComparison.CurrentCultureIgnoreCase) == true)
                        {
                            rainfall[i] = drRF[2].ToString().Trim();
                        }
                    }
                }
                for (int i = 0; i < rqrdStation.Count; i++)
                {
                    testStation.AppendLine(rqrdStation[i] + "\t" + rainfall[i]);
                }
                File.WriteAllText(@"E:\Rain_MAP\Real_Rain\others\DataLog_" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt", testStation.ToString());

                //-------------------------------------------------------//

                int x = 1;
                foreach (DataRow drRF in rftable.Rows)
                {
                    foreach (DataRow drXY in latlontable.Rows)
                    {
                        
                        if (string.Equals(drRF[1].ToString().Trim(), drXY[0].ToString().Trim(), StringComparison.CurrentCultureIgnoreCase) == true)
                        {
                            if (drRF[2].ToString().Trim() == "-")
                            {
                                drRF[2] = "0";
                            }
                            sb.AppendLine(x + " " + drXY[3] + " " + drXY[4] + " " + drRF[2].ToString().Trim() + " " + drRF[2].ToString().Trim());
                            x = x + 1;
                            break;
                        }
                        
                    }
                }
                double[] dummyBTM_X = new double[] { -562191.83, -525791.83, -482691.83, -437291.83, -360291.83, -194391.83, -81191.83, 36408.17, 173908.17, 264308.17, 406208.17, 539308.17, 633808.17, 769408.17, 919508.17, 1046608.17, 1172308.17, 1249808.17, 1195508.17, 1085208.17, 1002908.17, 900122, 870332, 863912, 844496.756, 792182, 779702, 779132, 769995.483, 750916.881 };
                double[] dummyBTM_Y = new double[] { 1517560.44, 1506960.44, 1473060.44, 1435860.44, 1417660.44, 1438560.44, 1408960.44, 1340360.44, 1308460.44, 1363260.44, 1347760.44, 1350660.44, 1410760.44, 1460560.44, 1429260.44, 1390160.44, 1301060.44, 1194360.44, 1021060.44, 1003360.44, 922460.44, 794159.244, 688038.756, 637058.756, 633274, 602639.244, 572728.756, 535189.244, 400164.353, 372593.074 };
                for (int i = 0; i < dummyBTM_X.Length; i++)
                {
                    sb.AppendLine(x + " " + dummyBTM_X[i] + " " + dummyBTM_Y[i] + " 0 0");
                    x++;
                }
                File.WriteAllText(@"E:\Rain_MAP\Real_Rain\others\RF.txt", sb.ToString());
                Console.WriteLine("All station's rainfall data created successfully.", "Data Export Completion", MessageBoxButtons.OK, MessageBoxIcon.Information);

                Process.Start(@"E:\Rain_MAP\Real_Rain\Rain_MAP.py");
                Console.WriteLine("Realtime Rain Map is Completed.");
                logText.AppendLine("Realtime Rain Map is Completed.");
            }
            catch (Exception error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Problem in Realtime Rain Map: " + error.Message);
                Console.ResetColor();
                logText.AppendLine("Problem in Realtime Rain Map, Error: " + error.Message);
            }            
           ///--------------------------------------------------------------------- FFWC Water Level Data Downloading ----------------------------
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("FFWC Water Level data is downloading.....");
                WebClient client = new WebClient();
                string htmlCode = client.DownloadString("http://www.ffwc.gov.bd/ffwc_charts/waterlevel.php");
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(htmlCode);
                HtmlNodeCollection tables = doc.DocumentNode.SelectNodes("//table");
                HtmlNodeCollection rows = tables[0].SelectNodes(".//tr");
                HtmlNodeCollection col = rows[1].SelectNodes(".//td");
                string dateTimeText = "2015-" + col[4].InnerText.Trim();
                DateTime dataDate = DateTime.ParseExact(dateTimeText, "yyyy-dd-MM", CultureInfo.InvariantCulture).AddHours(6);

                con.Open();
                for (int i = 0; i < rows.Count - 3; ++i)
                {
                    HtmlNodeCollection cols = rows[i + 3].SelectNodes(".//td");
                    if (cols.Count > 4 && cols[4].InnerText != "NP")
                    {
                        try
                        {
                            cmd = new SqlCommand("INSERT INTO GBMStationWL VALUES(@dataDate, @individual, @individual2, 0)", con);
                            cmd.Parameters.AddWithValue("@dataDate", dataDate);
                            cmd.Parameters.AddWithValue("@individual", cols[1].InnerText.Trim());
                            cmd.Parameters.AddWithValue("@individual2", cols[4].InnerText.Trim());
                            cmd.ExecuteNonQuery();
                        }
                        catch (SqlException)
                        {
                            cmd = new SqlCommand("Update GBMStationWL SET WLValue = @individual2 Where Date = @dataDate AND Station = @individual", con);
                            cmd.Parameters.AddWithValue("@dataDate", dataDate);
                            cmd.Parameters.AddWithValue("@individual", cols[1].InnerText.Trim());
                            cmd.Parameters.AddWithValue("@individual2", cols[4].InnerText.Trim());
                            cmd.ExecuteNonQuery();
                        }

                        if (cols[1].InnerText == "Amalshid" || cols[1].InnerText == "Pankha" || cols[1].InnerText == "Noonkhawa" || cols[1].InnerText == "Bahadurabad" || cols[1].InnerText == "Hardinge-RB")
                        {
                            for (int x = 1; x <= 4; x++)
                            {
                                try
                                {
                                    cmd = new SqlCommand("INSERT INTO GBMStationWL VALUES(@dataDate, @individual, @individual2, 0)", con);
                                    cmd.Parameters.AddWithValue("@dataDate", dataDate.AddDays(-1).AddHours(x * 3));
                                    cmd.Parameters.AddWithValue("@individual", cols[1].InnerText.Trim());
                                    cmd.Parameters.AddWithValue("@individual2", double.Parse(cols[3].InnerText) + 0.125 * x * (double.Parse(cols[4].InnerText) - double.Parse(cols[3].InnerText)));
                                    cmd.ExecuteNonQuery();
                                }
                                catch(SqlException)
                                {
                                    cmd = new SqlCommand("Update GBMStationWL SET WLValue = @individual2 Where Date = @dataDate AND Station = @individual", con);
                                    cmd.Parameters.AddWithValue("@dataDate", dataDate.AddDays(-1).AddHours(x * 3));
                                    cmd.Parameters.AddWithValue("@individual", cols[1].InnerText.Trim());
                                    cmd.Parameters.AddWithValue("@individual2", double.Parse(cols[3].InnerText) + 0.125 * x * (double.Parse(cols[4].InnerText) - double.Parse(cols[3].InnerText)));
                                }
                            }
                        }
                    }
                }
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("WL Data downloaded and Final database updated.");
                con.Close();
            }
            catch (Exception error)
            {
                con.Close();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("WL Data is not available at FFWC Website and thus No Model can be initiated. Error: " + error.Message);
                Console.ResetColor();
                Console.ReadKey();
            }
            
            /// -------------------------------------------- Initiating AutomaticFF Module ------------------------------------------------
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Model Simulation is initiating......");
                ProcessStartInfo start = new ProcessStartInfo();
                Process exeProcess = new Process();
                start.FileName = @"E:\FFWS\Programs\AutomaticFF.exe";
                start.CreateNoWindow = false;
                exeProcess = Process.Start(start);
            }
            catch(Exception error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No Model is initiated due to an error. Error: " + error.Message);
                Console.ResetColor();
                Console.ReadKey();
            }
            File.WriteAllText(@"E:\FFWS\Data\DataDownloadLog\log_" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt", logText.ToString());
        }
    }
}