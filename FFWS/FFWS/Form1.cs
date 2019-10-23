using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Net;
using HtmlAgilityPack;
using System.Windows.Forms.DataVisualization.Charting;
using DHI.Generic.MikeZero.DFS;
using DHI.Generic.MikeZero;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Diagnostics;
using System.Timers;
using Microsoft.Reporting.WinForms;
using System.Globalization;
using Microsoft.Research.Science.Data;
using Microsoft.Research.Science.Data.Imperative;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;



namespace FFWS
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        SqlConnection con = new SqlConnection(@"Data Source=.\SQLEXPRESS;AttachDbFilename=E:\FFWS\Database\GBMStationRF.mdf;Integrated Security=True;User Instance=True");
        SqlCommand cmd = new SqlCommand();
        System.Data.DataSet ds = new System.Data.DataSet();
        SqlDataAdapter adapter = new SqlDataAdapter();
        StringBuilder logText = new StringBuilder();
        //DateTime startDate = new DateTime(2014, 01, 01, 09, 00, 00);
        DateTime endDate = DateTime.Today.AddHours(9);
        DataTable dt = new DataTable();

        private void Form1_Load(object sender, EventArgs e)
        {
            dataEntryGridView.Visible = false;
            listRFStationBox.Visible = false;
            lblWebpagelist.Visible = false;
            checkedWebsiteBox.Visible = false;
            btnSelectAll.Visible = false;
            btnClearAll.Visible = false;
            btnDownloadRF.Visible = false;
            btnUpdateTempRFDatabase.Visible = false;
            btnUpdateFinaldb.Visible = false;
            dataGridView1.Visible = false;
            dataGridView2.Visible = false;
            chart1.Visible = false;
            lblRFStartDate.Visible = false;
            lblRFEndDate.Visible = false;
            dateTimeRFStartPicker.Visible = false;
            dateTimeRFEndPicker.Visible = false;
            btnExportRFStationCSV.Visible = false;
            btnExportRFAllCSV.Visible = false;
            btnImportCSVRF.Visible = false;
            btnUpdatedataEntry.Visible = false;
            lblGISRFMapDate.Visible = false;
            dateTimeGISRFMapPicker.Visible = false;
            btnCreateGISRFText.Visible = false;
            btnUpdateGarbage.Visible = false;
            endDate = dateofForecast.Value;
            this.reportGBMStationRFViewer.RefreshReport();
        }

        private void btnDownloadtouch_Click(object sender, EventArgs e)
        {
            listRFStationBox.Visible = false;
            lblWebpagelist.Visible = true;
            checkedWebsiteBox.Visible = true;
            btnSelectAll.Visible = true;
            btnClearAll.Visible = true;
            btnDownloadRF.Visible = true;
            btnUpdateTempRFDatabase.Visible = true;
            btnUpdateFinaldb.Visible = true;
            dataGridView1.Visible = true;
            dataGridView2.Visible = false;
            chart1.Visible = false;
            lblRFStartDate.Visible = false;
            lblRFEndDate.Visible = false;
            dateTimeRFStartPicker.Visible = false;
            dateTimeRFEndPicker.Visible = false;
            btnExportRFStationCSV.Visible = false;
            btnExportRFAllCSV.Visible = false;
            btnImportCSVRF.Visible = false;
            dataEntryGridView.Visible = false;
            btnUpdatedataEntry.Visible = false;
            lblGISRFMapDate.Visible = false;
            dateTimeGISRFMapPicker.Visible = false;
            btnCreateGISRFText.Visible = false;
            btnUpdateGarbage.Visible = true;
            reportGBMStationRFViewer.Visible = false;
            lblReportViewerStartdate.Visible = false;
            lblReportViewerEndDate.Visible = false;
            dateTimeReportStartPicker.Visible = false;
            dateTimeReportEndPicker.Visible = false;
            btnViewReport.Visible = false;
        }
        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedWebsiteBox.Items.Count; i++)
            {
                checkedWebsiteBox.SetItemChecked(i, true);
            }
        }
        private void btnClearAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedWebsiteBox.Items.Count; i++)
            {
                checkedWebsiteBox.SetItemChecked(i, false);
            }
        }
        private void btnDownloadRF_Click(object sender, EventArgs e)
        {
            cmd = new SqlCommand(@"DELETE FROM TempGBMStationRF", con);
            con.Open();
            cmd.ExecuteNonQuery();
            con.Close();

            if (checkedWebsiteBox.GetItemCheckState(0) == CheckState.Checked)
            {
                try
                {
                    Console.WriteLine("http://202.54.31.7/citywx/max_min_rain.php");
                    WebBrowser wb = new WebBrowser();
                    wb.Navigate("http://202.54.31.7/citywx/max_min_rain.php");

                    while (wb.ReadyState != WebBrowserReadyState.Complete)
                    {
                        Application.DoEvents();
                    }

                    wb.Document.ExecCommand("SelectAll", false, null);
                    wb.Document.ExecCommand("Copy", false, null);
                    string downloadtext = Clipboard.GetText();
                    DateTime datadate = new DateTime(int.Parse(downloadtext.Substring(33, 4)), int.Parse(downloadtext.Substring(30, 2)), int.Parse(downloadtext.Substring(27, 2)), 09, 00, 00);
                    //Console.WriteLine(datadate.ToString());
                    string firststeptext = downloadtext.Replace("        Max/Min Temp /24 hr RF(mm)   ", "");
                    firststeptext = firststeptext.Substring(38, firststeptext.Length - 38);
                    firststeptext = firststeptext.Replace(", ", ",");
                    var textArray = firststeptext.Split(',');

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
                    Console.WriteLine(error.Message);
                    logText.AppendLine("Problem Website: http://202.54.31.7/citywx/max_min_rain.php" + error.Message);
                }
            }

            if (checkedWebsiteBox.GetItemCheckState(1) == CheckState.Checked)
            {
                try
                {

                    Console.WriteLine("http://www.wunderground.com/history/station/");
                    DateTime today = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 09, 00, 00);
                    WebClient client = new WebClient();
                    string[] stationNo = new string[] { "55591", "56312", "55578", "55279", "56247", "56144", "56146", "55773", "55299", "56651", "55696", "56137", "55228", "56106", "56116", "56739", "55664", "55472" };
                    string[] stationName = new string[] { "Lhasa", "Nyingchi", "Xigaze", "Baingoin", "Batang", "Dege", "Garze", "Pagri", "Nagqu", "Lijing", "Lhunze", "Qamdo", "Shiquanhe", "Sog Xian", "Dengqen", "Tengchong", "Tingri", "Xainza" };
                    for (int j = 0; j < 2; j++)
                    {
                        for (int i = 0; i < stationNo.Length; i++)
                        {

                            string htmlCode = client.DownloadString("http://www.wunderground.com/history/station/" + stationNo[i] + "/" + today.Year + "/" + today.Month + "/" + today.Day + "/MonthlyHistory.html");
                            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                            doc.LoadHtml(htmlCode);
                            HtmlNodeCollection tables = doc.DocumentNode.SelectNodes("//table");
                            //MessageBox.Show(tables.Count.ToString());
                            HtmlNodeCollection rows = tables[3].SelectNodes(".//tr");
                            HtmlNodeCollection col = rows[today.Day + 1].SelectNodes(".//td");

                            con.Open();
                            cmd = new SqlCommand("INSERT INTO TempGBMStationRF VALUES(@dataDate, @individual, @individual2, 1)", con);
                            cmd.Parameters.AddWithValue("@dataDate", today);
                            cmd.Parameters.AddWithValue("@individual", stationName[i].Trim());
                            cmd.Parameters.AddWithValue("@individual2", col[19].InnerText.Trim());
                            cmd.ExecuteNonQuery();
                            con.Close();
                            Console.WriteLine(today.ToString() + "," + stationName[i].Trim() + "," + col[19].InnerText.Trim());

                        }
                        today = today.AddHours(DateTime.Now.Hour);
                    }
                    logText.AppendLine("Website: http://www.wunderground.com/history/station, Downloaded Station= " + (stationNo.Length));
                }
                catch (Exception err)
                {
                    con.Close();
                    Console.WriteLine(err);
                    logText.AppendLine(err.Message);
                }
            }
            if (checkedWebsiteBox.GetItemCheckState(2) == CheckState.Checked)
            {
                try
                {
                    Console.WriteLine("http://www.mfd.gov.np/");
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
                        //sb.AppendLine(webDate.ToString() + "," + col[0].InnerText.Trim() + "," + col[3].InnerText.Trim() + "," + "2");

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
                catch (Exception)
                {
                    con.Close();
                    Console.WriteLine("http://www.mfd.gov.np/" + "not available");
                    logText.AppendLine("Problem Website: http://www.mfd.gov.np/" + "not available");
                }
            }
            if (checkedWebsiteBox.GetItemCheckState(3) == CheckState.Checked)
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    Console.WriteLine("http://202.54.31.7/citywx/city_weather.php?id=");
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
                            Console.WriteLine(stationName[i] + " data not found");
                            continue;
                        }
                    }
                    logText.AppendLine("Website: http://202.54.31.7/citywx/city_weather.php, Downloaded station: = " + count);

                }
                catch (Exception error)
                {
                    con.Close();
                    Console.WriteLine(error.Message);
                    logText.AppendLine("Problem Website: http://202.54.31.7/citywx/city_weather.php" + " data not downloaded");
                }
            }
            if (checkedWebsiteBox.GetItemCheckState(4) == CheckState.Checked)
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    Console.WriteLine("http://202.54.31.7/citywx/city_weather1.php?");
                    WebClient client = new WebClient();
                    string[] stationID = new string[] { "30002", "42220", "42314", "42309", "42308", "42423", "42415", "42527", "42410", "42406", "42516", "42515", "42623", "42619", "42724", "42726" };
                    string[] stationName = new string[] { "Anni", "Passighat", "Dibrugarh", "North Lakhimpur", "Itanagar", "Jorhat", "Tezpur", "Kohima", "Guwahati", "Dhubri", "Shillong", "Cherrapunji", "Imphal", "Silchar", "Agartala", "Aizwal" };
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
                            Console.WriteLine(stationName[i] + " data not found");
                            continue;
                        }
                    }
                    logText.AppendLine("Website: http://202.54.31.7/citywx/city_weather1.php, Downloaded station: = " + count);

                }
                catch (Exception error)
                {
                    con.Close();
                    Console.WriteLine(error.Message);
                    logText.AppendLine("Problem Website: http://202.54.31.7/citywx/city_weather1.php" + " data not downloaded");
                }
            }

            if (checkedWebsiteBox.GetItemCheckState(5) == CheckState.Checked)
            {
                try
                {
                    Console.WriteLine("http://www.ffwc.gov.bd/ffwc_charts/rainfall.php");
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
                    Console.WriteLine(errtor.Message);
                    logText.AppendLine("Problem Website: http://www.ffwc.gov.bd/ffwc_charts/rainfall.php" + " data not downloaded");
                }
            }
            if (checkedWebsiteBox.GetItemCheckState(6) == CheckState.Checked)
            {
                StringBuilder sb = new StringBuilder();
                WebClient client = new WebClient();
                string htmlCode = client.DownloadString("http://amssdelhi.gov.in/dynamic/weather/wxtable.html");
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(htmlCode);
                HtmlNodeCollection date = doc.DocumentNode.SelectNodes(".//b");
                string tableTitle = date[1].InnerText;
                var eachword = tableTitle.Split(' ');
                DateTime webDate = DateTime.Parse(eachword[7] + " " + eachword[8] + " " + eachword[9].Remove(eachword[9].IndexOf('\r'))).AddHours(9);
                HtmlNodeCollection tables = doc.DocumentNode.SelectNodes("//table");
                HtmlNodeCollection rows = tables[0].SelectNodes(".//tr");
                for (int i = 4; i < rows.Count - 4; i++)
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
            }
            if (checkedWebsiteBox.GetItemCheckState(7) == CheckState.Checked)
            {
                StringBuilder sb = new StringBuilder();
                WebClient client = new WebClient();
                string htmlCode = client.DownloadString("http://121.241.116.157/dynamic/weather/delhiregion.html");
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(htmlCode);

                HtmlNodeCollection tables = doc.DocumentNode.SelectNodes("//table");
                HtmlNodeCollection bigfont = tables[0].SelectNodes(".//big");
                DateTime webDate = new DateTime(int.Parse(bigfont[6].InnerText.Trim().Substring(6, 4)), int.Parse(bigfont[6].InnerText.Trim().Substring(3, 2)), int.Parse(bigfont[6].InnerText.Trim().Substring(0, 2)));
                HtmlNodeCollection rows = tables[0].SelectNodes(".//tr");

                for (int i = 2; i < rows.Count - 2; i++)
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
                    cmd.Parameters.AddWithValue("@dataDate", webDate.AddDays(1));
                    cmd.Parameters.AddWithValue("@individual", stationname);
                    cmd.Parameters.AddWithValue("@individual2", col[5].InnerText.Trim());
                    cmd.ExecuteNonQuery();
                    con.Close();
                    
                    Console.WriteLine(webDate.AddDays(1).ToString() + "," + stationname + "," + col[5].InnerText.Trim());

                }

            }

            if (checkedWebsiteBox.GetItemCheckState(8) == CheckState.Checked)
            {
                try
                {
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
                            //MessageBox.Show(tables[0].InnerText);
                            //HtmlNodeCollection dateval = tables[0].SelectNodes(".//span");
                            //MessageBox.Show(dateval[2].InnerText.Trim());
                            HtmlNodeCollection rows = tables[2].SelectNodes(".//tr");
                            HtmlNodeCollection col = rows[today.Day - 2].SelectNodes(".//td");
                            HtmlNodeCollection col2 = rows[today.Day - 1].SelectNodes(".//td");
                            //sb.AppendLine(DateTime.Parse(col[0].InnerText.Trim()).AddHours(24).ToString() + "," + stationName[i] + "," + col[1].InnerText.Trim() + "," + "5");

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
                            logText.AppendLine(stationName[i] + " " + error.Message);
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
            }

            if (checkedWebsiteBox.GetItemCheckState(9) == CheckState.Checked)
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    PdfReader reader = new PdfReader("http://www.imdguwahati.gov.in/dwr.pdf");
                    sb.Append(PdfTextExtractor.GetTextFromPage(reader, 1));
                    string text = sb.ToString();
                    var lines = text.Split(':');
                    var spacedtext = lines[0].Split(' ');
                    DateTime date = new DateTime();
                    try
                    {
                        if (spacedtext[22].Length == 9)
                        {
                            //MessageBox.Show("9");
                            date = DateTime.ParseExact(spacedtext[22].Trim(), "d-MMM-yy", System.Globalization.CultureInfo.InvariantCulture).AddHours(9);
                        }

                        else if (spacedtext[22].Length >= 10)
                        {
                            //MessageBox.Show("10");
                            date = DateTime.ParseExact(spacedtext[22].Trim().Substring(0, 9), "d-MMM-yy", System.Globalization.CultureInfo.InvariantCulture).AddHours(9);
                        }
                    }
                    catch (FormatException)
                    {
                        //MessageBox.Show("11");
                        date = DateTime.ParseExact(spacedtext[22].Trim().Substring(0, 8), "d-MMM-yy", System.Globalization.CultureInfo.InvariantCulture).AddHours(9);
                    }
                    //MessageBox.Show(date.ToString());
                    char[] dispertext = new char[] { '=', ',', '&' };
                    var stations = lines[1].Split(dispertext);
                    sb.Clear();

                    List<string> obtainedSt = new List<string>();
                    List<string> obtainedRF = new List<string>();
                    string[] givenSt = File.ReadAllLines(@"E:\FFWS\Batch\StationList_Guwahati.txt");
                    string[] zeroRF = new string[givenSt.Length];

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
                                    obtainedRF.Add((rain * 10).ToString());
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
                            if (String.Equals(givenSt[i].Trim(), obtainedSt[j].Trim(), StringComparison.CurrentCultureIgnoreCase) == true)
                            {

                                zeroRF[i] = obtainedRF[j];
                                break;
                            }
                            else
                            {
                                zeroRF[i] = "0";
                            }
                        }
                    }
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
                    logText.AppendLine("Problem Website: http://www.imdguwahati.gov.in/dwr.pdf, Error: " + error.Message);
                }
            }

            if (checkedWebsiteBox.GetItemCheckState(10) == CheckState.Checked)
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    PdfReader reader = new PdfReader(@"http://www.imdsikkim.gov.in/daily_Forecast.pdf");
                    sb.Append(PdfTextExtractor.GetTextFromPage(reader, 3));
                    string text = sb.ToString();
                    var tableText = text.Remove(0, text.IndexOf('0')).Split('\n');
                    var dateText = tableText[0].Split(' ');
                    DateTime date = DateTime.ParseExact(dateText[dateText.Length - 2], "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture).AddHours(9);
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
                    logText.AppendLine("Problem Website: http://www.imdsikkim.gov.in/daily_Forecast.pdf, Error: " + error.Message);
                }
            }

            if (checkedWebsiteBox.GetItemCheckState(11) == CheckState.Checked)
            {
                try
                {
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
                catch (Exception error)
                {
                    con.Close();
                    logText.AppendLine("Problem Website: http://121.241.116.157/dynamic/weather/Delhi.pdf, Error: " + error.Message);
                }
            }

            if (checkedWebsiteBox.GetItemCheckState(12) == CheckState.Checked)
            {
                try
                {
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
                            if (stationDate.Contains(DateTime.Today.AddHours(9)) != true)
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
                    logText.AppendLine("Problem Website: http://www.bmd.gov.bd/, Error: " + error.Message);
                }
            }

            if (checkedWebsiteBox.GetItemCheckState(13) == CheckState.Checked)
            {
                try
                {
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
                    logText.AppendLine("Problem Website: http://www.cwc.gov.in/, Error: " + error.Message);
                }
            }

            if (checkedWebsiteBox.GetItemCheckState(14) == CheckState.Checked)
            {

            }
            MessageBox.Show("Data downloaded and temporarily saved successfully");
        }

        private void checkedWebsiteBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            dt.Clear();
            dt.Columns.Clear();
            con.Open();
            cmd.Connection = con;
            var element = checkedWebsiteBox.SelectedIndex;
            cmd = new SqlCommand("Select Date, StationName, RFValue from TempGBMStationRF Where webID = @no", con);
            cmd.Parameters.AddWithValue("@no", element);
            adapter.SelectCommand = cmd;
            adapter.Fill(dt);
            adapter.Dispose();
            cmd.Dispose();
            cmd.Parameters.Clear();
            con.Close();
            dataGridView1.DataSource = dt;
            
        }
        private void btnUpdateTempRFDatabase_Click(object sender, EventArgs e)
        {
            int rowIndex = dataGridView1.CurrentRow.Index;
            var date = dataGridView1.Rows[rowIndex].Cells[0].Value;
            var stationName = dataGridView1.Rows[rowIndex].Cells[1].Value.ToString();
            var RFValue = dataGridView1.Rows[rowIndex].Cells[2].Value.ToString();
            cmd = new SqlCommand(@"UPDATE TempGBMStationRF SET  RFValue = @RFValue WHERE Date = @date AND stationName =  @stationName", con);
            cmd.Parameters.AddWithValue("@stationName", stationName);
            cmd.Parameters.AddWithValue("@date", date);
            cmd.Parameters.AddWithValue("@RFValue", RFValue);
            cmd.Connection = con;
            con.Open();
            cmd.ExecuteNonQuery();
            con.Close();
            cmd.Dispose();
            cmd.Parameters.Clear();
        }
        private void btnUpdateGarbage_Click(object sender, EventArgs e)
        {
            try
            {
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
                MessageBox.Show("Data preprocess completed successfully.");
            }
            catch (Exception error)
            {
                con.Close();
                MessageBox.Show("Temporary Database cannot be updated due to following error: " + error.Message);
            }
        }
        private void btnUpdateFinaldb_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime dateval = new DateTime(DateTime.Today.AddDays(-1).Year, DateTime.Today.AddDays(-1).Month, DateTime.Today.AddDays(-1).Day, 00, 00, 00);
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
                MessageBox.Show("Final Database updated successfully.");
            }
            catch (Exception error)
            {
                con.Close();
                MessageBox.Show("Error in updating final database. Error: " + error.Message);
            }
        }

        private void btnViewEditTouch_Click(object sender, EventArgs e)
        {
            lblWebpagelist.Visible = false;
            checkedWebsiteBox.Visible = false;
            btnSelectAll.Visible = false;
            btnClearAll.Visible = false;
            btnDownloadRF.Visible = false;
            btnUpdateTempRFDatabase.Visible = false;
            dataGridView1.Visible = false;
            dataGridView2.Visible = false;
            listRFStationBox.Visible = true;
            btnUpdateFinaldb.Visible = false;
            lblRFStartDate.Visible = false;
            lblRFEndDate.Visible = false;
            dateTimeRFStartPicker.Visible = false;
            dateTimeRFEndPicker.Visible = false;
            btnExportRFStationCSV.Visible = false;
            btnExportRFAllCSV.Visible = false;
            btnImportCSVRF.Visible = false;
            dataEntryGridView.Visible = false;
            btnUpdatedataEntry.Visible = false;
            lblGISRFMapDate.Visible = false;
            dateTimeGISRFMapPicker.Visible = false;
            btnCreateGISRFText.Visible = false;
            btnUpdateGarbage.Visible = false;
            reportGBMStationRFViewer.Visible = false;
            lblReportViewerStartdate.Visible = false;
            lblReportViewerEndDate.Visible = false;
            dateTimeReportStartPicker.Visible = false;
            dateTimeReportEndPicker.Visible = false;
            btnViewReport.Visible = false;

            dt.Clear();
            dt.Columns.Clear();
            con.Open();
            cmd= new SqlCommand("Select DISTINCT StationName from GBMStationRF Order by StationName asc", con);
            adapter.SelectCommand = cmd;
            adapter.Fill(dt);
            adapter.Dispose();
            cmd.Dispose();
            cmd.Parameters.Clear();
            con.Close();
            foreach (DataRow dr in dt.Rows)
            {
                listRFStationBox.Items.Add(dr[0]);
            }

        }
        private void listRFStationBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            chart1.Visible = true;
            dataGridView2.Visible = true;
            var element = listRFStationBox.SelectedItem;

            dt.Clear();
            dt.Columns.Clear();
            con.Open();
            cmd = new SqlCommand("Select Date, StationName, RFValue  FROM GBMStationRF Where StationName = @station ORDER By Date ASC", con);
            cmd.Parameters.AddWithValue("@station", element);
            adapter.SelectCommand = cmd;
            adapter.Fill(dt);
            adapter.Dispose();
            cmd.Dispose();
            cmd.Parameters.Clear();
            con.Close();
            dataGridView2.DataSource = dt;
            chart1.DataSource = dt;

            chart1.Series["Series1"].XValueMember = "Date";
            chart1.Series["Series1"].XValueType = ChartValueType.DateTime;
            chart1.Series["Series1"].YValueMembers = "RFValue";
            chart1.Series["Series1"].ChartType = SeriesChartType.Column;  // Set chart type like Bar chart, Pie chart
            chart1.Series["Series1"].IsValueShownAsLabel = false;
            chart1.Titles[0].Text = "Station Name: " + element;

        }

        private void btnImprtRFTouch_Click(object sender, EventArgs e)
        {
            chart1.Visible = false;
            lblWebpagelist.Visible = false;
            checkedWebsiteBox.Visible = false;
            btnSelectAll.Visible = false;
            btnClearAll.Visible = false;
            btnDownloadRF.Visible = false;
            btnUpdateTempRFDatabase.Visible = false;
            dataGridView1.Visible = false;
            dataGridView2.Visible = false;
            listRFStationBox.Visible = false;
            btnUpdateFinaldb.Visible = false;
            btnImportCSVRF.Visible = true;
            lblRFStartDate.Visible = false;
            lblRFEndDate.Visible = false;
            dateTimeRFStartPicker.Visible = false;
            dateTimeRFEndPicker.Visible = false;
            btnExportRFStationCSV.Visible = false;
            btnExportRFAllCSV.Visible = false;
            dataEntryGridView.Visible = false;
            btnUpdatedataEntry.Visible = false;
            lblGISRFMapDate.Visible = false;
            dateTimeGISRFMapPicker.Visible = false;
            btnCreateGISRFText.Visible = false;
            btnUpdateGarbage.Visible = false;
            reportGBMStationRFViewer.Visible = false;
            lblReportViewerStartdate.Visible = false;
            lblReportViewerEndDate.Visible = false;
            dateTimeReportStartPicker.Visible = false;
            dateTimeReportEndPicker.Visible = false;
            btnViewReport.Visible = false;

        }
        private void btnImportCSVRF_Click(object sender, EventArgs e)
        {
            string filepath;
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.ShowDialog();
            
            filepath = dialog.FileName;
            List<string> rfStation = new List<string>();
            List<string> tempdbDate = new List<string>();
            List<string> rfvalue = new List<string>();

            var csvfile = new StreamReader(File.OpenRead(filepath));
            while (!csvfile.EndOfStream)
            {
                var values = csvfile.ReadLine().Split(',');
                tempdbDate.Add(values[0]);
                rfStation.Add(values[1]);
                rfvalue.Add(values[2]);
            }

            con.Open();
            for (int i = 0; i < tempdbDate.Count; i++)
            {
                try
                {
                    cmd = new SqlCommand("INSERT INTO GBMStationRF VALUES(@dataDate, @individual, @individual2, '15')", con);
                    cmd.Parameters.AddWithValue("@dataDate", tempdbDate[i]);
                    cmd.Parameters.AddWithValue("@individual", rfStation[i]);
                    cmd.Parameters.AddWithValue("@individual2", rfvalue[i]);
                    cmd.ExecuteNonQuery();
                }
                catch (SqlException)
                {
                    cmd = new SqlCommand("Update GBMStationRF SET RFValue = @rfValue Where Date = @dataDate AND StationName= @station", con);
                    cmd.Parameters.AddWithValue("@dataDate", tempdbDate[i]);
                    cmd.Parameters.AddWithValue("@station", rfStation[i]);
                    cmd.Parameters.AddWithValue("@rfValue", rfvalue[i]);
                }

            }
            con.Close();
            MessageBox.Show("Database Updated.");
        }

        private void btnExportRFTouch_Click(object sender, EventArgs e)
        {
            chart1.Visible = false;
            lblWebpagelist.Visible = false;
            checkedWebsiteBox.Visible = false;
            btnSelectAll.Visible = false;
            btnClearAll.Visible = false;
            btnDownloadRF.Visible = false;
            btnUpdateTempRFDatabase.Visible = false;
            dataGridView1.Visible = false;
            dataGridView2.Visible = false;
            listRFStationBox.Visible = false;
            btnUpdateFinaldb.Visible = false;
            btnImportCSVRF.Visible = false;
            lblRFStartDate.Visible = true;
            lblRFEndDate.Visible = true;
            dateTimeRFStartPicker.Visible = true;
            dateTimeRFEndPicker.Visible = true;
            btnExportRFStationCSV.Visible = true;
            btnExportRFAllCSV.Visible = true;
            dataEntryGridView.Visible = false;
            btnUpdatedataEntry.Visible = false;
            lblGISRFMapDate.Visible = false;
            dateTimeGISRFMapPicker.Visible = false;
            btnCreateGISRFText.Visible = false;
            btnUpdateGarbage.Visible = false;
            reportGBMStationRFViewer.Visible = false;
            lblReportViewerStartdate.Visible = false;
            lblReportViewerEndDate.Visible = false;
            dateTimeReportStartPicker.Visible = false;
            dateTimeReportEndPicker.Visible = false;
            btnViewReport.Visible = false;
        }
        private void btnExportRFAllCSV_Click(object sender, EventArgs e)
        {
            DateTime startDate = dateTimeRFStartPicker.Value;
            DateTime endDate = dateTimeRFEndPicker.Value;
            cmd = new SqlCommand("Select Date, StationName, RFValue from GBMStationRF Where Date>= @startDate AND Date <= @endDate ORDER by Date ASC", con);
            cmd.Parameters.AddWithValue("@startDate", startDate);
            cmd.Parameters.AddWithValue("@endDate", endDate);
            dt.Clear();
            dt.Columns.Clear();
            con.Open();
            adapter.SelectCommand = cmd;
            adapter.Fill(dt);
            adapter.Dispose();
            cmd.Dispose();
            con.Close();
            
            StringBuilder sb = new StringBuilder();
            foreach (DataRow dr in dt.Rows)
            {
                sb.AppendLine(dr[0] + "," + dr[1].ToString().Trim() + "," + dr[2].ToString().Trim());
            }

            File.WriteAllText("E:\\Test.csv", sb.ToString());
            MessageBox.Show("All station's rainfall data created successfully.", "Data Export Completion", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void btnExportRFStationCSV_Click(object sender, EventArgs e)
        {
            try
            {
                dt.Clear();
                dt.Columns.Clear();
                cmd = new SqlCommand("Select DISTINCT StationName from GBMStationRF", con);
                con.Open();
                adapter.SelectCommand = cmd;
                adapter.Fill(dt);
                adapter.Dispose();
                cmd.Dispose();
                con.Close();
                List<string> stationName = new List<string>();
                foreach (DataRow dr in dt.Rows)
                {
                    stationName.Add(dr[0].ToString());
                }

                foreach (string element in stationName)
                {
                    DateTime startDate = dateTimeRFStartPicker.Value;
                    DateTime endDate = dateTimeRFEndPicker.Value;
                    cmd = new SqlCommand("Select Date, StationName, RFValue from GBMStationRF Where StationName= @stationName AND Date>= @startDate AND Date <= @endDate ORDER by Date ASC", con);
                    cmd.Parameters.AddWithValue("@startDate", startDate);
                    cmd.Parameters.AddWithValue("@endDate", endDate);
                    cmd.Parameters.AddWithValue("@stationName", element);
                    con.Open();
                    adapter.SelectCommand = cmd;
                    adapter.Fill(dt);
                    adapter.Dispose();
                    cmd.Dispose();
                    cmd.Parameters.Clear();
                    con.Close();
                    StringBuilder sb = new StringBuilder();

                    foreach (DataRow dr in dt.Rows)
                    {
                        sb.AppendLine(dr[0] + "," + dr[2].ToString().Trim());
                    }

                    File.WriteAllText("E:\\GBMRFData\\" + element.Trim() + ".csv", sb.ToString());
                }
                MessageBox.Show(stationName.Count.ToString() + " station's rainfall data created successfully.", "Data Export Completion", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
        }

        private void btnRFEntryTouch_Click(object sender, EventArgs e)
        {
            chart1.Visible = false;
            lblWebpagelist.Visible = false;
            checkedWebsiteBox.Visible = false;
            btnSelectAll.Visible = false;
            btnClearAll.Visible = false;
            btnDownloadRF.Visible = false;
            btnUpdateTempRFDatabase.Visible = false;
            dataGridView1.Visible = false;
            dataGridView2.Visible = false;
            listRFStationBox.Visible = false;
            btnUpdateFinaldb.Visible = false;
            btnImportCSVRF.Visible = false;
            lblRFStartDate.Visible = false;
            lblRFEndDate.Visible = false;
            dateTimeRFStartPicker.Visible = false;
            dateTimeRFEndPicker.Visible = false;
            btnExportRFStationCSV.Visible = false;
            btnExportRFAllCSV.Visible = false;
            dataEntryGridView.Visible = true;
            lblGISRFMapDate.Visible = false;
            dateTimeGISRFMapPicker.Visible = false;
            btnCreateGISRFText.Visible = false;
            btnUpdateGarbage.Visible = false;
            reportGBMStationRFViewer.Visible = false;
            lblReportViewerStartdate.Visible = false;
            lblReportViewerEndDate.Visible = false;
            dateTimeReportStartPicker.Visible = false;
            dateTimeReportEndPicker.Visible = false;
            btnViewReport.Visible = false;

            dataEntryGridView.ColumnCount = 3;
            dataEntryGridView.Columns[0].HeaderText = "Date";
            dataEntryGridView.Columns[0].ValueType = typeof(DateTime);
            dataEntryGridView.Columns[1].HeaderText = "StationName";
            dataEntryGridView.Columns[1].ValueType = typeof(string);
            ((DataGridViewTextBoxColumn)dataEntryGridView.Columns[1]).MaxInputLength = 50;
            dataEntryGridView.Columns[2].HeaderText = "RFValue";
            dataEntryGridView.Columns[2].ValueType = typeof(string);
            ((DataGridViewTextBoxColumn)dataEntryGridView.Columns[2]).MaxInputLength = 20;
            btnUpdatedataEntry.Visible = true;
        }
        private void btnUpdatedataEntry_Click(object sender, EventArgs e)
        {
            con.Open();
            foreach (DataGridViewRow dr in dataEntryGridView.Rows)
            {
                try
                {
                    cmd = new SqlCommand("INSERT INTO GBMStationRF VALUES(@dataDate, @individual, @individual2, '15')", con);
                    cmd.Parameters.AddWithValue("@dataDate", dr.Cells[0].Value);
                    cmd.Parameters.AddWithValue("@individual", dr.Cells[1]);
                    cmd.Parameters.AddWithValue("@individual2", dr.Cells[2]);
                    cmd.ExecuteNonQuery();
                }
                catch (SqlException)
                {
                    cmd = new SqlCommand("Update GBMStationRF SET RFValue = @rfValue Where Date = @dataDate AND StationName= @station", con);
                    cmd.Parameters.AddWithValue("@dataDate", dr.Cells[0].Value);
                    cmd.Parameters.AddWithValue("@station", dr.Cells[1]);
                    cmd.Parameters.AddWithValue("@rfValue", dr.Cells[2]);
                }
            }
            con.Close();

            MessageBox.Show("Data has been inserted in Database.");
        }

        private void btnGISRFTouch_Click(object sender, EventArgs e)
        {
            dataEntryGridView.Visible = false;
            listRFStationBox.Visible = false;
            lblWebpagelist.Visible = false;
            checkedWebsiteBox.Visible = false;
            btnSelectAll.Visible = false;
            btnClearAll.Visible = false;
            btnDownloadRF.Visible = false;
            btnUpdateTempRFDatabase.Visible = false;
            btnUpdateFinaldb.Visible = false;
            dataGridView1.Visible = false;
            dataGridView2.Visible = false;
            chart1.Visible = false;
            lblRFStartDate.Visible = false;
            lblRFEndDate.Visible = false;
            dateTimeRFStartPicker.Visible = false;
            dateTimeRFEndPicker.Visible = false;
            btnExportRFStationCSV.Visible = false;
            btnExportRFAllCSV.Visible = false;
            btnImportCSVRF.Visible = false;
            btnUpdatedataEntry.Visible = false;
            lblGISRFMapDate.Visible = true;
            dateTimeGISRFMapPicker.Visible = true;
            btnCreateGISRFText.Visible = true;
            btnUpdateGarbage.Visible = false;
            reportGBMStationRFViewer.Visible = false;
            lblReportViewerStartdate.Visible = false;
            lblReportViewerEndDate.Visible = false;
            dateTimeReportStartPicker.Visible = false;
            dateTimeReportEndPicker.Visible = false;
            btnViewReport.Visible = false;

            dateTimeGISRFMapPicker.Value = DateTime.Today;
        }
        private void btnCreateGISRFText_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Point");
            DateTime rftxtDate = dateTimeGISRFMapPicker.Value;
            DateTime rftxtDate2 = dateTimeGISRFMapPicker.Value.AddDays(1);
            cmd = new SqlCommand("Select Date, StationName, RFValue from GBMStationRF Where Date>= @startDate AND Date < @endDate AND RFValue!='-'", con);
            cmd.Parameters.AddWithValue("@startDate", rftxtDate);
            cmd.Parameters.AddWithValue("@endDate", rftxtDate2);
            dt.Clear();
            dt.Columns.Clear();
            con.Open();
            adapter.SelectCommand = cmd;
            adapter.Fill(dt);
            adapter.Dispose();
            cmd.Dispose();
            cmd.Parameters.Clear();
            con.Close();

            DataTable rftable = dt;

            dt.Clear();
            dt.Columns.Clear();
             cmd = new SqlCommand("Select *from StationLocation", con);
            con.Open();
            adapter.SelectCommand = cmd;
            adapter.Fill(dt);
            adapter.Dispose();
            cmd.Dispose();
            cmd.Parameters.Clear();
            con.Close();
            DataTable latlontable = dt;

            int x = 1;
            foreach (DataRow drRF in rftable.Rows)
            {
                foreach (DataRow drXY in latlontable.Rows)
                {
                    if (drRF[1].ToString().Trim() == drXY[0].ToString().Trim())
                    {
                        sb.AppendLine(x + " " + drXY[3] + " " + drXY[4] + " " + drRF[2].ToString().Trim() + " " + drRF[2].ToString().Trim());
                        x = x + 1;
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
            File.WriteAllText(@"E:\FFWS\RainMap\RF.txt", sb.ToString());
            MessageBox.Show("All station's rainfall data created successfully.", "Data Export Completion", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }

        private void btnDownloadWLTouch_Click(object sender, EventArgs e)
        {
            btnExportWLALLCSV.Visible = false;
            btnExportStationWL.Visible = false;
            dateTimeWLExportStartPicker.Visible = false;
            dateTimeWLExportEndPicker.Visible = false;
            lblWLExportStart.Visible = false;
            lblWLExportEnd.Visible = false;
            checkedWLWebsitelist.Visible = true;
            btnWLWebSelectAll.Visible = true;
            btnWLWebClear.Visible = true;
            dataTempWLGridView.Visible = true;
            btnDownloadWL.Visible = true;
            chartWLStation.Visible = false;
            dataWLStationGridView.Visible = false;
            btnImportWLCSV.Visible = false;
            listWLStationBox.Visible = false;
        }
        private void btnWLWebClear_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedWLWebsitelist.Items.Count; i++)
            {
                checkedWLWebsitelist.SetItemChecked(i, false);
            }

        }
        private void btnWLWebSelectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedWLWebsitelist.Items.Count; i++)
            {
                checkedWLWebsitelist.SetItemChecked(i, true);
            }
        }
        private void btnDownloadWL_Click(object sender, EventArgs e)
        {
            
            if (checkedWLWebsitelist.GetItemCheckState(0) == CheckState.Checked)
            {
                WebClient client = new WebClient();
                string htmlCode = client.DownloadString("http://www.ffwc.gov.bd/ffwc_charts/waterlevel.php");
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(htmlCode);

                try
                {
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
                            cmd = new SqlCommand("INSERT INTO GBMStationWL VALUES(@dataDate, @individual, @individual2, 0)", con);
                            cmd.Parameters.AddWithValue("@dataDate", dataDate);
                            cmd.Parameters.AddWithValue("@individual", cols[1].InnerText.Trim());
                            cmd.Parameters.AddWithValue("@individual2", cols[4].InnerText.Trim());
                            cmd.ExecuteNonQuery();

                            if (cols[1].InnerText == "Amalshid" || cols[1].InnerText == "Pankha" || cols[1].InnerText == "Noonkhawa" || cols[1].InnerText == "Bahadurabad" || cols[1].InnerText == "Hardinge-RB")
                            {
                                for (int x = 1; x <= 4; x++)
                                {
                                    cmd = new SqlCommand("INSERT INTO GBMStationWL VALUES(@dataDate, @individual, @individual2, 0)", con);
                                    cmd.Parameters.AddWithValue("@dataDate", dataDate.AddDays(-1).AddHours(x * 3));
                                    cmd.Parameters.AddWithValue("@individual", cols[1].InnerText.Trim());
                                    cmd.Parameters.AddWithValue("@individual2", double.Parse(cols[3].InnerText) + 0.125 * x * (double.Parse(cols[4].InnerText) - double.Parse(cols[3].InnerText)));
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                    MessageBox.Show("WL Data downloaded successfully.", "Data Download", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    con.Close();
                }
                catch (Exception error)
                {
                    MessageBox.Show(error.Message);
                    con.Close();
                }
            }
        }
        private void checkedWLWebsitelist_SelectedIndexChanged(object sender, EventArgs e)
        {
            dt.Clear();
            dt.Columns.Clear();
            con.Open();
            cmd.Connection = con;
            int element = checkedWLWebsitelist.SelectedIndex;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "Select Date, StationName, WLValue from TempGBMStationWL Where webID = @no";
            cmd.Parameters.AddWithValue("@no", element);
            adapter.SelectCommand = cmd;
            adapter.Fill(dt);
            adapter.Dispose();
            cmd.Dispose();
            cmd.Parameters.Clear();
            con.Close();
            dataTempWLGridView.DataSource =dt;
           

        }

        private void btnViewEditWLTouch_Click(object sender, EventArgs e)
        {
            btnExportWLALLCSV.Visible = false;
            btnExportStationWL.Visible = false;
            dateTimeWLExportStartPicker.Visible = false;
            dateTimeWLExportEndPicker.Visible = false;
            lblWLExportStart.Visible = false;
            lblWLExportEnd.Visible = false;
            checkedWLWebsitelist.Visible = false;
            btnWLWebSelectAll.Visible = false;
            btnWLWebClear.Visible = false;
            dataTempWLGridView.Visible = false;
            btnDownloadWL.Visible = false;     
            chartWLStation.Visible = false;
            dataWLStationGridView.Visible = false;
            btnImportWLCSV.Visible = false;
            listWLStationBox.Visible = true;

            dt.Clear();
            dt.Columns.Clear();
            con.Open();
            cmd = new SqlCommand("Select DISTINCT Station from GBMStationWL", con);
            adapter.SelectCommand = cmd;
            adapter.Fill(dt);
            adapter.Dispose();
            cmd.Dispose();
            cmd.Parameters.Clear();
            con.Close();
            foreach (DataRow dr in dt.Rows)
            {
                listWLStationBox.Items.Add(dr[0]);
            }

        }
        private void listWLStationBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            dt.Clear();
            dt.Columns.Clear();
            chartWLStation.Visible = true;
            dataWLStationGridView.Visible = true;
            var element = listWLStationBox.SelectedItem;
            con.Open();
            cmd = new SqlCommand("Select Date, Station, WLValue  FROM GBMStationWL Where Station = @station ORDER By Date ASC", con);
            cmd.Parameters.AddWithValue("@station", element);
            adapter.SelectCommand = cmd;
            adapter.Fill(dt);
            adapter.Dispose();
            cmd.Parameters.Clear();
            cmd.Dispose();
            con.Close();
            dataWLStationGridView.DataSource = dt;

            chartWLStation.DataSource = dt;
            chartWLStation.Series["Series1"].XValueMember = "Date";
            chartWLStation.Series["Series1"].XValueType = ChartValueType.DateTime;
            chartWLStation.Series["Series1"].YValueMembers = "WLValue";
            chartWLStation.Series["Series1"].ChartType = SeriesChartType.Line;
            chartWLStation.Series["Series1"].IsValueShownAsLabel = false;
            chartWLStation.Titles[0].Text = "Station Name: " + element;
        }

        private void btnImportWlTouch_Click(object sender, EventArgs e)
        {
            btnExportWLALLCSV.Visible = false;
            btnExportStationWL.Visible = false;
            dateTimeWLExportStartPicker.Visible = false;
            dateTimeWLExportEndPicker.Visible = false;
            lblWLExportStart.Visible = false;
            lblWLExportEnd.Visible = false;
            checkedWLWebsitelist.Visible = false;
            btnWLWebSelectAll.Visible = false;
            btnWLWebClear.Visible = false;
            dataTempWLGridView.Visible = false;
            btnDownloadWL.Visible = false;
            chartWLStation.Visible = false;
            dataWLStationGridView.Visible = false;
            listWLStationBox.Visible = false;
            btnImportWLCSV.Visible = true;
        }
        private void btnImportWLCSV_Click(object sender, EventArgs e)
        {
            try
            {
                List<string> wlStation = new List<string>();
                List<string> dbDate = new List<string>();
                List<string> wlvalue = new List<string>();
                List<int> webID = new List<int>();

                OpenFileDialog dialog = new OpenFileDialog();
                dialog.ShowDialog();
                string filepath = dialog.FileName;
                var csvfile = new StreamReader(File.OpenRead(filepath));
                while (!csvfile.EndOfStream)
                {
                    var values = csvfile.ReadLine().Split(',');
                    dbDate.Add(values[0]);
                    wlStation.Add(values[1]);
                    wlvalue.Add(values[2]);
                }

                con.Open();
                for(int k=0; k<wlStation.Count; k++)
                {
                    try
                    {
                        cmd = new SqlCommand("INSERT INTO GBMStationWL VALUES(@dataDate, @individual, @individual2, 15)", con);
                        cmd.Parameters.AddWithValue("@dataDate", dbDate[k]);
                        cmd.Parameters.AddWithValue("@individual", wlStation[k]);
                        cmd.Parameters.AddWithValue("@individual2", wlvalue[k]);
                        cmd.ExecuteNonQuery();
                    }
                    catch (SqlException)
                    {

                        cmd = new SqlCommand("Update GBMStationWL Set WLValue = @wlval Where Date = @dataDate AND Station = @station", con);
                        cmd.Parameters.AddWithValue("@dataDate", dbDate[k]);
                        cmd.Parameters.AddWithValue("@station", wlStation[k]);
                        cmd.Parameters.AddWithValue("@wlval", wlvalue[k]);
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("WL Data processed successfully from CSV file.");
                con.Close();
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
        }

        private void btnExportWlTouch_Click(object sender, EventArgs e)
        {
            checkedWLWebsitelist.Visible = false;
            btnWLWebSelectAll.Visible = false;
            btnWLWebClear.Visible = false;
            dataTempWLGridView.Visible = false;
            btnDownloadWL.Visible = false;
            chartWLStation.Visible = false;
            dataWLStationGridView.Visible = false;
            listWLStationBox.Visible = false;
            btnImportWLCSV.Visible = false;
            btnExportWLALLCSV.Visible = true;
            btnExportStationWL.Visible = true;
            dateTimeWLExportStartPicker.Visible = true;
            dateTimeWLExportEndPicker.Visible = true;
            lblWLExportStart.Visible = true;
            lblWLExportEnd.Visible = true;
        }
        private void btnExportStationWL_Click(object sender, EventArgs e)
        {
            try
            {
                dt.Clear();
                dt.Columns.Clear();
                cmd = new SqlCommand("Select DISTINCT Station from GBMStationWL", con);
                con.Open();
                adapter.SelectCommand = cmd;
                adapter.Fill(dt);
                adapter.Dispose();
                cmd.Dispose();
                con.Close();
                List<string> stationName = new List<string>();
                foreach (DataRow dr in dt.Rows)
                {
                    stationName.Add(dr[0].ToString());
                }
                Directory.CreateDirectory(@"E:\FFWS\Data\ExportedData_" + DateTime.Today.ToString("yyyy-MM-dd"));
                foreach (string element in stationName)
                {
                    dt.Clear();
                    dt.Columns.Clear();
                    DateTime startDate = dateTimeWLExportStartPicker.Value;
                    DateTime endDate = dateTimeWLExportEndPicker.Value;
                    cmd = new SqlCommand("Select Date, Station, WLValue from GBMStationWL Where Station= @stationName AND Date>= @startDate AND Date <= @endDate ORDER by Date ASC", con);
                    cmd.Parameters.AddWithValue("@startDate", startDate);
                    cmd.Parameters.AddWithValue("@endDate", endDate);
                    cmd.Parameters.AddWithValue("@stationName", element);
                    con.Open();
                    adapter.SelectCommand = cmd;
                    adapter.Fill(dt);
                    adapter.Dispose();
                    cmd.Dispose();
                    con.Close();

                    StringBuilder sb = new StringBuilder();

                    foreach (DataRow dr in dt.Rows)
                    {
                        sb.AppendLine(dr[0] + "," + dr[2]);
                    }

                    File.WriteAllText(@"E:\FFWS\Data\ExportedData_" + DateTime.Today.ToString("yyyy-MM-dd") + @"\" + element.Trim() + ".csv", sb.ToString());
                }
                MessageBox.Show(stationName.Count.ToString() + @" station's Water Level data created successfully and saved in E:\FFWS\Data\ExportedData_" + DateTime.Today.ToString("yyyy-MM-dd"), "Data Export Completion", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
        }
        private void btnExportWLALLCSV_Click(object sender, EventArgs e)
        {
            dt.Clear();
            dt.Columns.Clear();
            DateTime startDate = dateTimeWLExportStartPicker.Value;
            DateTime endDate = dateTimeWLExportEndPicker.Value;
            cmd = new SqlCommand("Select Date, StationName, WLValue from GBMStationWL Where Date>= @startDate AND Date <= @endDate ORDER by StationName, Date ASC", con);
            cmd.Parameters.AddWithValue("@startDate", startDate);
            cmd.Parameters.AddWithValue("@endDate", endDate);
            con.Open();
            adapter.SelectCommand = cmd;
            adapter.Fill(dt);
            adapter.Dispose();
            cmd.Dispose();
            con.Close();
            cmd.Parameters.Clear();

            StringBuilder sb = new StringBuilder();
            foreach (DataRow dr in dt.Rows)
            {
                sb.AppendLine(dr[0] + "," + dr[1].ToString().Trim() + "," + dr[2]);
            }

            File.WriteAllText(@"E:\Test.csv", sb.ToString());
            MessageBox.Show("All station's Water Level data created successfully.", "Data Export Completion", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnCreateRTRF_Click(object sender, EventArgs e)
        {
            dateTimertrfstartPicker.Value = new DateTime(2014, 01, 01);
            lblrtrfstartDate.Visible = true;
            lblrtrfendDate.Visible = true;
            dateTimertrfstartPicker.Visible = true;
            dateTimertrfendPicker.Visible = true;
            btnStartRTRFDFS0.Visible = true;
            btnCancelrtrfdfs0.Visible = true;
            dateTimertrfendPicker.Value = dateofForecast.Value.Date;
        }
        private void btnProcessWRFtoDFS0_Click(object sender, EventArgs e)
        {
            lblrtrfstartDate.Visible = false;
            lblrtrfendDate.Visible = false;
            dateTimertrfstartPicker.Visible = false;
            dateTimertrfendPicker.Visible = false;
            btnStartRTRFDFS0.Visible = false;
            btnCancelrtrfdfs0.Visible = false;

            try
            {
                float[, ,] gridR = new float[6, 159, 159];
                string wrfFile = @"E:\FFWS\WRFOut\wrfout_d01_" + dateofForecast.Value.Date.ToString("yyyy-MM-dd") + ".nc?openMode=readOnly";
                var dataset = Microsoft.Research.Science.Data.DataSet.Open(wrfFile);
                float[, ,] Xlong = dataset.GetData<float[, ,]>("XLONG");
                float[, ,] Xlat = dataset.GetData<float[, ,]>("XLAT");

                float[, ,] rain = dataset.GetData<float[, ,]>("RAINC");
                float[, ,] rainnc = dataset.GetData<float[, ,]>("RAINNC");

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
                    DateTime today = dateofForecast.Value.Date.AddHours(6);
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
                MessageBox.Show("WRF Data processing successfully Completed....", "WRF Processing", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message, "WRF Processing", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnWRFCorrection_Click(object sender, EventArgs e)
        {
            lblrtrfstartDate.Visible = false;
            lblrtrfendDate.Visible = false;
            dateTimertrfstartPicker.Visible = false;
            dateTimertrfendPicker.Visible = false;
            btnStartRTRFDFS0.Visible = false;
            btnCancelrtrfdfs0.Visible = false;
            try
            {
                dt.Clear();
                dt.Columns.Clear();
                StringBuilder sb = new StringBuilder();
                cmd = new SqlCommand("Select DISTINCT Catchment from WRFCatchStat", con);
                con.Open();
                adapter.SelectCommand = cmd;
                adapter.Fill(dt);
                adapter.Dispose();
                cmd.Dispose();
                con.Close();
                List<string> catchName = new List<string>();
                foreach (DataRow dr in dt.Rows)
                {
                    catchName.Add(dr[0].ToString().Trim());
                }
                foreach (string element in catchName)
                {
                    try
                    {
                        //-----------------------------------------------------------------Creating Merged Array---------------------------------------------------------------------------------------------
                        IDfsFile resFile = DfsFileFactory.DfsGenericOpenEdit(@"E:\FFWS\Model\NAM\MAR\" + element + "_Rainfall.dfs0");
                        IDfsFileInfo resfileInfo = resFile.FileInfo;
                        int noTimeSteps = resfileInfo.TimeAxis.NumberOfTimeSteps;
                        //MessageBox.Show(noTimeSteps.ToString());
                        DateTime testDate= new DateTime(2014, 01, 01);
                        //MessageBox.Show((DateTime.Today-testDate).TotalDays.ToString());

                        IDfsItemData<float> data;
                        float[] rfvalues = new float[10];
                        if (noTimeSteps >= Convert.ToInt32((DateTime.Today - testDate).TotalDays) + 3)
                        {
                            for (int j = 0; j < 4; j++)
                            {
                                data = (IDfsItemData<float>)resFile.ReadItemTimeStep(1, Convert.ToInt32((DateTime.Today - testDate).TotalDays) + 81 + j);
                                rfvalues[j] = Convert.ToSingle(data.Data[0]);
                            }
                        }
                        else
                        {
                            for (int j = 0; j < 4; j++)
                            {
                                data = (IDfsItemData<float>)resFile.ReadItemTimeStep(1, noTimeSteps - 4 + j);
                                rfvalues[j] = Convert.ToSingle(data.Data[0]);
                            }
                        }

                        string wrfRFpath = @"E:\FFWS\Datapro\WRF_Catch\" + element + ".txt";
                        var stationfile = new StreamReader(wrfRFpath);
                        int x = 0;
                        while (!stationfile.EndOfStream)
                        {
                            var values = stationfile.ReadLine().Split(',');
                            rfvalues[4 + x] = (float.Parse(values[2]));
                            x = x + 1;
                        }
                        //////------------------------------------------------------------------------Obtaining StationStat---------------------------------------------------------------------/////////////////////////
                        dt.Clear();
                        dt.Columns.Clear();
                        con.Open();
                        cmd = new SqlCommand("Select OneDay, TwoDay, ThreeDay, FourDay, FiveDay from WRFCatchStat Where Catchment=@catch AND Month=@month", con);
                        cmd.Parameters.AddWithValue("@catch", element);
                        cmd.Parameters.AddWithValue("@month", DateTime.Today.Month);
                        adapter.SelectCommand = cmd;
                        adapter.Fill(dt);
                        adapter.Dispose();
                        cmd.Dispose();
                        cmd.Parameters.Clear();
                        con.Close();

                        DataTable table = dt;
                        float[] statStation = new float[5];
                        statStation[0] = float.Parse(table.Rows[0].ItemArray[0].ToString());
                        statStation[1] = float.Parse(table.Rows[0].ItemArray[1].ToString());
                        statStation[2] = float.Parse(table.Rows[0].ItemArray[2].ToString());
                        statStation[3] = float.Parse(table.Rows[0].ItemArray[3].ToString());
                        statStation[4] = float.Parse(table.Rows[0].ItemArray[4].ToString());
                        //-------------------------------------------------------------Applying Correction---------------------------------------------------------------------------------
                        // Daily Maximum Correction
                        for (int j = 4; j < 10; j++)
                        {
                            if (rfvalues[j] > statStation[0]) { rfvalues[j] = statStation[0]; }
                            else { rfvalues[j] = rfvalues[j]; }
                        }
                        //2 Day accumulated rainfall correction
                        for (int j = 4; j < 10; j++)
                        {
                            if ((rfvalues[j] + rfvalues[j - 1]) > statStation[1]) { rfvalues[j] = statStation[1] - rfvalues[j - 1]; }
                            else { rfvalues[j] = rfvalues[j]; }
                        }
                        //3 Day accumulated rainfall correction
                        for (int j = 4; j < 10; j++)
                        {
                            if ((rfvalues[j] + rfvalues[j - 1] + rfvalues[j - 2]) > statStation[2]) { rfvalues[j] = statStation[2] - rfvalues[j - 1] - rfvalues[j - 2]; }
                            else { rfvalues[j] = rfvalues[j]; }
                        }
                        //4 Day accumulated rainfall correction
                        for (int j = 4; j < 10; j++)
                        {
                            if ((rfvalues[j] + rfvalues[j - 1] + rfvalues[j - 2] + rfvalues[j - 3]) > statStation[3]) { rfvalues[j] = statStation[3] - rfvalues[j - 1] - rfvalues[j - 2] - rfvalues[j - 3]; }
                            else { rfvalues[j] = rfvalues[j]; }
                        }
                        //5 Day accumulated rainfall correction
                        for (int j = 4; j < 10; j++)
                        {
                            if ((rfvalues[j] + rfvalues[j - 1] + rfvalues[j - 2] + rfvalues[j - 3] + rfvalues[j - 4]) > statStation[4]) { rfvalues[j] = statStation[4] - rfvalues[j - 1] - rfvalues[j - 2] - rfvalues[j - 3] - rfvalues[j - 4]; }
                            else { rfvalues[j] = rfvalues[j]; }
                        }

                        ///////------------------------------------------------------------------------Creating dfs0 files-------------------------------------------------------------------------------------------------------------------------------------------------------
                        DfsFactory factory = new DfsFactory();
                        string filename = @"E:\FFWS\Model\NAM\WRF-DFS0\" + element + ".dfs0";
                        DfsBuilder filecreator = DfsBuilder.Create(element, element, 2012);
                        filecreator.SetDataType(1);
                        filecreator.SetGeographicalProjection(factory.CreateProjectionUndefined());
                        //filecreator.SetTemporalAxis(factory.CreateTemporalEqCalendarAxis(DHI.Generic.MikeZero.eumUnit.eumUsec, new DateTime(selectDate.Year, selectDate.Month, selectDate.Day, 06, 00, 00), 0, 86400));
                        filecreator.SetTemporalAxis(factory.CreateTemporalNonEqCalendarAxis(eumUnit.eumUsec, new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 06, 00, 00)));
                        filecreator.SetItemStatisticsType(StatType.RegularStat);
                        DfsDynamicItemBuilder item = filecreator.CreateDynamicItemBuilder();
                        item.Set(element, eumQuantity.Create(eumItem.eumIRainfall, eumUnit.eumUmillimeter), DfsSimpleType.Float);
                        item.SetValueType(DataValueType.StepAccumulated);
                        item.SetAxis(factory.CreateAxisEqD0());
                        item.SetReferenceCoordinates(1f, 2f, 3f);
                        filecreator.AddDynamicItem(item.GetDynamicItemInfo());

                        filecreator.CreateFile(filename);
                        IDfsFile file = filecreator.GetFile();

                        float[] correctedWRF = new float[6];
                        //double dateval;
                        file.WriteItemTimeStep(1, 0, 0, new float[] { Convert.ToSingle(-1E-30) });

                        for (int y = 0; y < 6; y++)
                        {
                            double diffsecond = 86400 * (y+1);
                            file.WriteItemTimeStepNext(diffsecond, new float[] { Convert.ToSingle(Math.Round(rfvalues[4 + y], 2)) });
                        }
                        file.Close();
                        try
                        {
                            con.Open();
                            cmd = new SqlCommand("INSERT INTO WRFDataCorrected VALUES(@date, @catchment, @1day, @2day, @3day, @4day, @5day, @6day)", con);
                            cmd.Parameters.AddWithValue("@date", DateTime.Today.AddHours(6));
                            cmd.Parameters.AddWithValue("@catchment", element);
                            cmd.Parameters.AddWithValue("@1day", rfvalues[4]);
                            cmd.Parameters.AddWithValue("@2day", rfvalues[5]);
                            cmd.Parameters.AddWithValue("@3day", rfvalues[6]);
                            cmd.Parameters.AddWithValue("@4day", rfvalues[7]);
                            cmd.Parameters.AddWithValue("@5day", rfvalues[8]);
                            cmd.Parameters.AddWithValue("@6day", rfvalues[9]);
                            cmd.ExecuteNonQuery();
                            cmd.Dispose();
                            cmd.Parameters.Clear();
                            con.Close();
                        }
                        catch (SqlException)
                        {
                            con.Close();
                            con.Open();
                            cmd = new SqlCommand("UPDATE WRFDataRaw SET Day1=@1day, Day2=@2day, Day3=@3day, Day4=@4day, Day5=@5day, Day6=@6day Where Date=@date AND Catchment=@catchment", con);
                            cmd.Parameters.AddWithValue("@date", DateTime.Today.AddHours(6));
                            cmd.Parameters.AddWithValue("@catchment", element);
                            cmd.Parameters.AddWithValue("@1day", rfvalues[4]);
                            cmd.Parameters.AddWithValue("@2day", rfvalues[5]);
                            cmd.Parameters.AddWithValue("@3day", rfvalues[6]);
                            cmd.Parameters.AddWithValue("@4day", rfvalues[7]);
                            cmd.Parameters.AddWithValue("@5day", rfvalues[8]);
                            cmd.Parameters.AddWithValue("@6day", rfvalues[9]);
                            cmd.ExecuteNonQuery();
                            cmd.Dispose();
                            cmd.Parameters.Clear();
                            con.Close();
                        }

                    }
                    catch (Exception error)
                    {
                        MessageBox.Show(error.Message);
                        continue;
                    }

                }
                MessageBox.Show(catchName.Count + " station's WRF data corrected successfully.", "WRF Correction", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }

        }
        private void btnNAMSimulation_Click(object sender, EventArgs e)
        {
            lblrtrfstartDate.Visible = false;
            lblrtrfendDate.Visible = false;
            dateTimertrfstartPicker.Visible = false;
            dateTimertrfendPicker.Visible = false;
            btnStartRTRFDFS0.Visible = false;
            btnCancelrtrfdfs0.Visible = false;
            DateTime today = dateofForecast.Value.Date;
            string dateval = "         end = " + today.Year + ", " + today.Month + ", " + today.Day + ", 6, 0, 0";
            string[] alllines = File.ReadAllLines(@"E:\FFWS\Model\NAM\NAM.sim11");
            string path = @"E:\FFWS\Model\NAM\NAM.sim11";
            alllines[39] = dateval;
            File.WriteAllLines(path, alllines);
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = @"C:\Program Files\DHI\2014\bin\mike11.exe";
            start.Arguments = @"E:\FFWS\Model\NAM\NAM.sim11";
            Process exeProcess = Process.Start(start);
            exeProcess.WaitForExit();
            MessageBox.Show("NAM Model Simulation Completed.");
        }
        private void btnOpenMikeRR_Click(object sender, EventArgs e)
        {
            lblrtrfstartDate.Visible = false;
            lblrtrfendDate.Visible = false;
            dateTimertrfstartPicker.Visible = false;
            dateTimertrfendPicker.Visible = false;
            btnStartRTRFDFS0.Visible = false;
            btnCancelrtrfdfs0.Visible = false;
            Process.Start(@"E:\FFWS\Batch\MIKE11RR_Open.bat");
        }
        private void btnProcessNAM_Click(object sender, EventArgs e)
        {

            lblrtrfstartDate.Visible = false;
            lblrtrfendDate.Visible = false;
            dateTimertrfstartPicker.Visible = false;
            dateTimertrfendPicker.Visible = false;
            btnStartRTRFDFS0.Visible = false;
            btnCancelrtrfdfs0.Visible = false;
            progressBar1.Visible = true;

            IDfsFile resFile = DfsFileFactory.DfsGenericOpenEdit(@"E:\FFWS\Model\NAM\Result\NAM-edited.res11");
            IDfsFileInfo resfileInfo = resFile.FileInfo;
            IDfsItemData<float> data;
            int noTimeSteps = resfileInfo.TimeAxis.NumberOfTimeSteps;
            DateTime[] date = resFile.FileInfo.TimeAxis.GetDateTimes();
            DateTime StartDate = date[0];
            double[] timeSpan = new double[noTimeSteps];
            for (int j = 0; j < noTimeSteps; j++)
            {
                timeSpan[j] = resFile.ReadItemTimeStep(2, j).Time;
            }
            
            float[] values = new float[noTimeSteps];
            for (int j = 0; j < resFile.ItemInfo.Count; j++)
            {
                progressBar1.Value = ((j + 1) / resFile.ItemInfo.Count) * 100;
                IDfsSimpleDynamicItemInfo dynamicItemInfo = resFile.ItemInfo[j];
                string nameOftDynamicItem = dynamicItemInfo.Name;
                string checkname = nameOftDynamicItem.Substring(0, 6);
                if (checkname == "RunOff")
                {
                    string filename = @"E:\FFWS\Model\NAM\Result\Output\" + nameOftDynamicItem + ".dfs0";
                    DfsFactory factory = new DfsFactory();
                    DfsBuilder filecreator = DfsBuilder.Create(nameOftDynamicItem, nameOftDynamicItem, 2012);
                    filecreator.SetDataType(1);
                    filecreator.SetGeographicalProjection(factory.CreateProjectionUndefined());
                    filecreator.SetTemporalAxis(factory.CreateTemporalNonEqCalendarAxis(eumUnit.eumUsec, new DateTime(StartDate.Year, StartDate.Month, StartDate.Day, StartDate.Hour, StartDate.Minute, StartDate.Second)));
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
                        data = (IDfsItemData<float>)resFile.ReadItemTimeStep(j + 1, i);
                        values[i] = Convert.ToSingle(data.Data[0]);
                        file.WriteItemTimeStepNext(timeSpan[i]*3600.0, new float[] { values[i] });
                    }
                    file.Close();
                }
            }
            progressBar1.Value = 100;
            MessageBox.Show("Completed.");
            progressBar1.Visible = false;
        }

        private void btnRunBasin_Click(object sender, EventArgs e)
        {
            lblrtrfstartDate.Visible = false;
            lblrtrfendDate.Visible = false;
            dateTimertrfstartPicker.Visible = false;
            dateTimertrfendPicker.Visible = false;
            btnStartRTRFDFS0.Visible = false;
            btnCancelrtrfdfs0.Visible = false;

            DateTime hydrostartDate = new DateTime(2014, 01, 02, 06, 00, 00);
            DateTime hydroendDate = dateofForecast.Value.Date.AddDays(6).AddHours(6);
            string[] filepath = File.ReadAllLines(@"E:\FFWS\Model\MIKEHYDRO\GBM_MIKEHYDRO.mhydro");
            filepath[54] = "         StartTime = '" + hydrostartDate.Year.ToString("0000") + " " + hydrostartDate.Month.ToString("00") + " " + hydrostartDate.Day.ToString("00") + " 06:00:00'";
            filepath[55] = "         EndTime = '" + hydroendDate.Year.ToString("0000") + " " + hydroendDate.Month.ToString("00") + " " + hydroendDate.Day.ToString("00") + " 06:00:00'";
            File.WriteAllLines(@"E:\FFWS\Model\MIKEHYDRO\GBM_MIKEHYDRO.mhydro", filepath);
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = @"C:\Program Files\DHI\2014\bin\MIKEBASINapp.exe";
            start.Arguments = @"E:\FFWS\Model\MIKEHYDRO\GBM_MIKEHYDRO.mhydro";
            start.CreateNoWindow = true;
            Process exeProcess = Process.Start(start);
            exeProcess.WaitForExit();
            MessageBox.Show("MikeHydro Model Simulation Completed.");
        }
        private void btnStartRTRFDFS0_Click(object sender, EventArgs e)
        {
            dt.Clear();
            dt.Columns.Clear();
            SqlCommand cmd = new SqlCommand("Select DISTINCT StationName from StationLocation", con);
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
            MessageBox.Show(stationName.Count.ToString());
            foreach (string element in stationName)
            {
                dt.Clear();
                dt.Columns.Clear();
                cmd = new SqlCommand("Select Date, RFValue from GBMStationRF Where StationName= @stationName AND Date> @startdate AND Date<= @enddate ORDER by Date ASC", con);
                cmd.Parameters.AddWithValue("@stationName", element);
                cmd.Parameters.AddWithValue("@startdate", dateTimertrfstartPicker.Value);
                cmd.Parameters.AddWithValue("@enddate", dateTimertrfendPicker.Value);
                int noOfDays = (int)(dateTimertrfendPicker.Value - dateTimertrfstartPicker.Value).TotalDays;
                con.Open();
                adapter.SelectCommand = cmd;
                adapter.Fill(dt);
                adapter.Dispose();
                cmd.Dispose();
                con.Close();

                DataTable table = dt;
                StringBuilder sb = new StringBuilder();
                try
                {
            
                    DateTime dfsDate = DateTime.Parse(table.Rows[0].ItemArray[0].ToString());
                    DateTime Start = dateTimertrfstartPicker.Value.AddHours(dfsDate.Hour);
                    DateTime aladadate = dateTimertrfstartPicker.Value.AddHours(dfsDate.Hour);

                    //MessageBox.Show(element + "  "+dfsDate.ToString());
                    float[] values = new float[table.Rows.Count];
                    //double dateval;

                    int x = 0;
                    DateTime[] intervaldate = new DateTime[table.Rows.Count];
                    foreach (DataRow dr in table.Rows)
                    {
                        intervaldate[x] = Convert.ToDateTime(dr[0]);
                        try
                        {
                            values[x] = Convert.ToSingle(dr[1].ToString().Trim());
                        }
                        catch (FormatException)
                        {
                            values[x] = -1e-25f;
                        }
                        x++;
                    }

                    DfsFactory factory = new DfsFactory();
                    string filename = @"E:\FFWS\Model\NAM\RF-DFS0\" + element.Trim() + ".dfs0";
                    DfsBuilder filecreator = DfsBuilder.Create(element, element, 2012);
                    filecreator.SetDataType(1);
                    filecreator.SetGeographicalProjection(factory.CreateProjectionUndefined());
                    //filecreator.SetTemporalAxis(factory.CreateTemporalEqCalendarAxis(DHI.Generic.MikeZero.eumUnit.eumUsec, new DateTime(selectDate.Year, selectDate.Month, selectDate.Day, 06, 00, 00), 0, 86400));
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
                    IDfsFileInfo fileinfo= file.FileInfo;
                    fileinfo.DeleteValueFloat = -1e-25f;

                    DateTime datet = new DateTime();
                    for (int j = 0; j <= noOfDays; j++)
                    {
                        float ff = -1e-25f;
                        datet = Start;
                        for (int i = 0; i < values.Length; i++)
                        {
                            if (Start.Date == intervaldate[i].Date)
                            {
                                datet = intervaldate[i];
                                ff = values[i];
                            }
                        }
                        if (j == 0 && ff == -1e-25f)
                        {
                            ff = 0;
                        }
                        if (j == 1 && ff == -1e-25f)
                        {
                            ff = 0;
                        }
                        if (j == noOfDays && ff == -1e-25f)
                        {
                            ff = 0;
                        }
                        file.WriteItemTimeStepNext((datet - dfsDate).TotalSeconds, new float[] { ff });
                        Start = Start.AddDays(1);
                    }
                    file.Close();
                }

                catch (IndexOutOfRangeException)
                {
                    
                    //MessageBox.Show(element);
                    DateTime Start = dateTimertrfstartPicker.Value.AddHours(9);
                    DateTime aladadate = Start;
                    DfsFactory factory = new DfsFactory();
                    string filename = @"E:\FFWS\Model\NAM\RF-DFS0\" + element.Trim() + ".dfs0";
                    DfsBuilder filecreator = DfsBuilder.Create(element, element, 2012);
                    filecreator.SetDataType(1);
                    filecreator.SetGeographicalProjection(factory.CreateProjectionUndefined());
                    //filecreator.SetTemporalAxis(factory.CreateTemporalEqCalendarAxis(DHI.Generic.MikeZero.eumUnit.eumUsec, new DateTime(selectDate.Year, selectDate.Month, selectDate.Day, 06, 00, 00), 0, 86400));
                    filecreator.SetTemporalAxis(factory.CreateTemporalNonEqCalendarAxis(eumUnit.eumUsec, new DateTime(Start.Year, Start.Month, Start.Day, Start.Hour, Start.Minute, Start.Second)));
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
                        file.WriteItemTimeStepNext((Start - aladadate).TotalSeconds, new float[] { Convert.ToSingle(-1e-25f) });
                        Start = Start.AddDays(1);
                    }
                    file.Close();
                    continue;
                }
            }
            File.Copy(@"E:\FFWS\Model\NAM\RF-DFS0\N\LAKHIMPUR.dfs0", @"E:\FFWS\Model\NAM\RF-DFS0\N-LAKHIMPUR.dfs0", true);
            MessageBox.Show(stationName.Count.ToString() + " station's Water Level data created successfully.", "Data Export Completion", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void btnCancelrtrfdfs0_Click(object sender, EventArgs e)
        {
            lblrtrfstartDate.Visible = false;
            lblrtrfendDate.Visible = false;
            dateTimertrfstartPicker.Visible = false;
            dateTimertrfendPicker.Visible = false;
            btnStartRTRFDFS0.Visible = false;
            btnCancelrtrfdfs0.Visible = false;

        }

        private void btnGenerateBoundary_Click(object sender, EventArgs e) 
        {
            try
            {
                dt.Clear();
                 dt.Columns.Clear();
                 DateTime bndstartdate = dateofForecast.Value.Date.AddDays(-7).AddHours(6);
                 DateTime today = dateofForecast.Value.Date.AddHours(6);
                 string[] station = new string[] { "Amalshid", "Manu-RB", "Durgapur", "Gaibandha", "Kurigram", "Rohanpur", "Panchagarh", "Badarganj", "Dalia", "Nakuagaon", "Comilla", "Lourergorh", "Sarighat", "Faridpur","Dinajpur" };

                 foreach (string element in station)
                 {
                     dt.Columns.Clear();
                     con.Open();
                     cmd = new SqlCommand("Select Date, WLValue FROM GBMStationWL Where Station = @Title AND Date> = @Date AND Date < = @today ORDER By Date ASC", con);
                     cmd.Parameters.AddWithValue("@Title", element);
                     cmd.Parameters.AddWithValue("@Date", bndstartdate);
                     cmd.Parameters.AddWithValue("@today", today);
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
                     if (datadate.Contains(today) != true)
                     {
                         MessageBox.Show(element + "- HD Boundary cannot be created due to lack of observed data.");
                         Environment.Exit(1);
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
                     MessageBox.Show("Noonkhawa Boundary cannot be created due to lack of observed data at Bahadurabad.");
                     Environment.Exit(1);
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
                     MessageBox.Show("Noonkhawa Boundary cannot be created due to lack of observed data at Noonkhawa.");
                     Environment.Exit(1);
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
                 MessageBox.Show("Error in Noonkhawa Boundary. Error: " + error.Message);
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
                     MessageBox.Show("Pankha Boundary cannot be created due to lack of observed data at Hardinge-RB.");
                     Environment.Exit(1);
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
                     MessageBox.Show("Pankha Boundary cannot be created due to lack of observed data at Pankha.");
                     Environment.Exit(1);
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
             }
             catch (Exception error)
             {
                 con.Close();
                 MessageBox.Show("Error in Pankha Boundary Generation. Error: " + error.Message);
             }
        }

        private void btnViewEditBnd_Click(object sender, EventArgs e)
        {
            listFFBoundaryBox.Visible = true;
            lblrtrfstartDate.Visible = false;
            lblrtrfendDate.Visible = false;
            dateTimertrfstartPicker.Visible = false;
            dateTimertrfendPicker.Visible = false;
            btnStartRTRFDFS0.Visible = false;
            btnCancelrtrfdfs0.Visible = false;
            btnRunBasin.Visible = false;
            btnNAMSimulation.Visible = false;
            btnProcessWRFtoDFS0.Visible = false;
            lblNamModel.Visible = false;
            btnCreateRTRF.Visible = false;
            btnOpenMikeRR.Visible = false;
            btnWRFCorrection.Visible = false;
            btnProcessNAM.Visible = false;
            btnRunBasin.Visible = false;
            dataFFBoundaryGridView.Visible = true;
            progressBar1.Visible = false;
            lblBasinModel.Visible = false;
            btnGenerateBoundary.Visible = false;
            btnViewEditBnd.Visible = false;
            boundaryChart.Visible = true;
            btnsaveFFBoundary.Visible = true;
        }
        private void listFFBoundaryBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var element = listFFBoundaryBox.SelectedItem;
            IDfsFile resFile = DfsFileFactory.DfsGenericOpenEdit(@"E:\FFWS\Model\FF\HDBounds\" + element+ "-RT.dfs0");
            IDfsFileInfo resfileInfo = resFile.FileInfo;
            int noTimeSteps = resfileInfo.TimeAxis.NumberOfTimeSteps;
            IDfsItemData<float> data;
            float[] Qvalues = new float[noTimeSteps + 48];
            DateTime[] dataDate = new DateTime[noTimeSteps + 48];
            dataDate[0] = DateTime.Today.AddDays(-7).AddHours(6);
            for (int j = 0; j < noTimeSteps; j++)
            {
                data = (IDfsItemData<float>)resFile.ReadItemTimeStep(1, j);
                Qvalues[j] = Convert.ToSingle(data.Data[0]);
                dataDate[j] = dataDate[0].AddSeconds(resFile.ReadItemTimeStep(1, j).Time);
            }

            IDfsFile ffresFile = DfsFileFactory.DfsGenericOpenEdit(@"E:\FFWS\Model\FF\HDBounds\" + element + "-FF.dfs0");
            IDfsFileInfo ffresfileInfo = ffresFile.FileInfo;
            int ffnoTimeSteps = ffresfileInfo.TimeAxis.NumberOfTimeSteps;
            IDfsItemData<float> ffdata;
            //dataDate[noTimeSteps] = dataDate[noTimeSteps - 1].AddSeconds(10800);
            for (int j = 0; j < ffnoTimeSteps; j++)
            {
                ffdata = (IDfsItemData<float>)ffresFile.ReadItemTimeStep(1, j);
                Qvalues[noTimeSteps+j] = Convert.ToSingle(ffdata.Data[0]);
                dataDate[noTimeSteps + j] = dataDate[noTimeSteps-1].AddSeconds(ffresFile.ReadItemTimeStep(1, j).Time);
            }

            DataTable dt = new DataTable();
            dt.Columns.Add("Date");
            dt.Columns.Add("Boundary");

            for (int i = 0; i < Qvalues.Length; i++)
            {
                DataRow row = dt.NewRow();
                row[0] = dataDate[i];
                row[1] = Math.Round(Qvalues[i], 2);
                dt.Rows.Add(row);
            }
            dataFFBoundaryGridView.DataSource = dt;

            lblBoundaryName.Visible = true;
            lblBoundaryName.Text = "Station Name: " + listFFBoundaryBox.SelectedItem;
            boundaryChart.DataSource = dt;
            boundaryChart.Series["Series1"].XValueMember = "Date";
            boundaryChart.Series["Series1"].XValueType = ChartValueType.DateTime;
            boundaryChart.Series["Series1"].YValueMembers = "Boundary";
            boundaryChart.Series["Series1"].ChartType = SeriesChartType.Line;  // Set chart type like Bar chart, Pie chart
            boundaryChart.Series["Series1"].IsValueShownAsLabel = false;
            boundaryChart.Series["Series1"].BorderWidth = 2;
            boundaryChart.ChartAreas[0].AxisX.Title = "Date";
            string[] name = listFFBoundaryBox.SelectedItem.ToString().Split('-');
            if (name[0] == "Gaibandha" || name[0] == "Dinajpur" || name[0] == "Faridpur" || name[1] == "H")
            {
                boundaryChart.ChartAreas[0].AxisY.Title = "Water Level (m)";
            }
            else { boundaryChart.ChartAreas[0].AxisY.Title = "Discharge (cumec)"; }
            
            boundaryChart.ChartAreas[0].AxisY.Maximum = Qvalues.Max();
            boundaryChart.ChartAreas[0].AxisY.Minimum = Qvalues.Min();
            boundaryChart.ChartAreas[0].AxisY.MajorGrid.Interval = (Qvalues.Max() - Qvalues.Min()) / 5.0;
            boundaryChart.ChartAreas[0].AxisY.LabelStyle.Format = "0.00";
            //boundaryChart.SaveImage(@"D:\Savechart.jpg", ChartImageFormat.Jpeg);
        }

        private void btnsaveFFBoundary_Click(object sender, EventArgs e)
        {
            float[] Qvalues = new float[dataFFBoundaryGridView.Rows.Count-1];
            DateTime[] dataDate = new DateTime[dataFFBoundaryGridView.Rows.Count-1];
            for (int i = 0; i < dataFFBoundaryGridView.Rows.Count-1; i++)
            {
                dataDate[i] = Convert.ToDateTime(dataFFBoundaryGridView.Rows[i].Cells[0].Value);
                Qvalues[i] = Convert.ToSingle(dataFFBoundaryGridView.Rows[i].Cells[1].Value);
                //MessageBox.Show(listFFBoundaryBox.SelectedItem + "   " + dataDate[i]+"   "+ Qvalues[i]);
            }

            string[] element = listFFBoundaryBox.SelectedItem.ToString().Split('-');
            MessageBox.Show(element[0]);
            
            string rtfilename = @"E:\FFWS\Model\FF\test\" + listFFBoundaryBox.SelectedItem + "-RT.dfs0";
            DfsFactory rtfactory = new DfsFactory();
            DfsBuilder rtfilecreator = DfsBuilder.Create(element[0], element[0], 2012);
            rtfilecreator.SetDataType(1);
            rtfilecreator.SetGeographicalProjection(rtfactory.CreateProjectionUndefined());
            rtfilecreator.SetTemporalAxis(rtfactory.CreateTemporalNonEqCalendarAxis(eumUnit.eumUsec, new DateTime(dataDate[0].Year, dataDate[0].Month, dataDate[0].Day, dataDate[0].Hour, 00, 00)));
            rtfilecreator.SetItemStatisticsType(StatType.RegularStat);
            DfsDynamicItemBuilder rtitem = rtfilecreator.CreateDynamicItemBuilder();

            if (element[0] == "Gaibandha" || element[0] == "Dinajpur" || element[0] == "Faridpur" || element[1] == "H" )
            {
                rtitem.Set(element[0], eumQuantity.Create(eumItem.eumIWaterLevel, eumUnit.eumUmeter), DfsSimpleType.Float);
            }
            else { rtitem.Set(element[0], eumQuantity.Create(eumItem.eumIDischarge, eumUnit.eumUm3PerSec), DfsSimpleType.Float); }
            rtitem.SetValueType(DataValueType.Instantaneous);
            rtitem.SetAxis(rtfactory.CreateAxisEqD0());
            rtitem.SetReferenceCoordinates(1f, 2f, 3f);
            rtfilecreator.AddDynamicItem(rtitem.GetDynamicItemInfo());


            rtfilecreator.CreateFile(rtfilename);
            IDfsFile rtfile = rtfilecreator.GetFile();

            for (int i = 0; i < dataDate.Length-48; i++)
            {
                double secondInterval = (dataDate[i] - dataDate[0]).TotalSeconds;
                rtfile.WriteItemTimeStepNext(secondInterval, new float[] { Qvalues[i] });
            }
            rtfile.Close();
            MessageBox.Show(dataDate[dataDate.Length - 49].ToString());

            string fffilename = @"E:\FFWS\Model\FF\test\" + listFFBoundaryBox.SelectedItem + "-FF.dfs0";
            DfsFactory fffactory = new DfsFactory();
            DfsBuilder fffilecreator = DfsBuilder.Create(element[0], element[0], 2012);
            fffilecreator.SetDataType(1);
            fffilecreator.SetGeographicalProjection(fffactory.CreateProjectionUndefined());
            fffilecreator.SetTemporalAxis(fffactory.CreateTemporalNonEqCalendarAxis(eumUnit.eumUsec, new DateTime(dataDate[dataDate.Length - 48].Year, dataDate[dataDate.Length - 48].Month, dataDate[dataDate.Length - 48].Day, dataDate[dataDate.Length - 48].Hour, 00, 00)));
            fffilecreator.SetItemStatisticsType(StatType.RegularStat);
            DfsDynamicItemBuilder ffitem = fffilecreator.CreateDynamicItemBuilder();
            if (element[0] == "Gaibandha" || element[0] == "Dinajpur" || element[0] == "Faridpur" || element[1] == "H")
            {
                ffitem.Set(element[0], eumQuantity.Create(eumItem.eumIWaterLevel, eumUnit.eumUmeter), DfsSimpleType.Float);
            }
            else { ffitem.Set(element[0], eumQuantity.Create(eumItem.eumIDischarge, eumUnit.eumUm3PerSec), DfsSimpleType.Float); }
            ffitem.SetValueType(DataValueType.Instantaneous);
            ffitem.SetAxis(fffactory.CreateAxisEqD0());
            ffitem.SetReferenceCoordinates(1f, 2f, 3f);
            fffilecreator.AddDynamicItem(ffitem.GetDynamicItemInfo());


            fffilecreator.CreateFile(fffilename);
            IDfsFile fffile = fffilecreator.GetFile();

            for (int i = dataDate.Length - 48; i < dataDate.Length; i++)
            {
                double secondInterval = (dataDate[i] - dataDate[dataDate.Length - 48]).TotalSeconds;
                fffile.WriteItemTimeStepNext(secondInterval, new float[] { Qvalues[i] });
            }
            fffile.Close();
            
        }
        private void btnRunFFModel_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime today = dateofForecast.Value;
                string[] alllines = File.ReadAllLines(@"E:\FFWS\Model\FF\FF.sim11");
                alllines[38] = "         start = " + today.AddDays(-7).Year + ", " + today.AddDays(-7).Month + ", " + today.AddDays(-7).Day + ", 6, 0, 0";
                alllines[39] = "         end = " + today.Year + ", " + today.Month + ", " + today.Day + ", 6, 0, 0";
                alllines[72] = "         hd = 2, |.\\Results\\FF-HD.RES11|, false, " + today.AddDays(-7).Year + ", " + today.AddDays(-7).Month + ", " + today.AddDays(-7).Day + ", 6, 0, 0";
                alllines[75] = "         rr = 1, |.\\Results\\FF-RR.RES11|, false, " + today.AddDays(-7).Year + ", " + today.AddDays(-7).Month + ", " + today.AddDays(-7).Day + ", 6, 0, 0";
                File.WriteAllLines(@"E:\FFWS\Model\FF\FF.sim11", alllines);

                ProcessStartInfo start = new ProcessStartInfo();
                Process exeProcess = new Process();

                start.FileName = @"C:\Program Files\DHI\2014\bin\mike11.exe";
                start.Arguments = @"E:\FFWS\Model\FF\FF.sim11";
                exeProcess = Process.Start(start);
                exeProcess.WaitForExit();

                MessageBox.Show("HD Model Simulation Completed.");
            }
            catch (Exception error)
            {
                MessageBox.Show("HD Model cannot be simulated due to an error. Error: " + error.Message);
            }
        }
        private void btnUpdateForecastWL_Click(object sender, EventArgs e)
        {
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
            DateTime[] resultDate = new DateTime[resultInfo.Length - 1];
            List<string> forecastResult = new List<string>();


            for (int i = 0; i < resultInfo.Length - 1; i++)
            {
                var separatedText = resultInfo[i + 1].Split(',');
                resultDate[i] = DateTime.Parse(separatedText[0]);
                TimeSpan ts = resultDate[i] - DateTime.Today.AddHours(6);
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


        }

        private void btnExportForecastJpg_Click(object sender, EventArgs e)
        {
            dt.Clear();
            dt.Columns.Clear();
            cmd = new SqlCommand("Select StationName, RiverName, DangerLevel, RHWL from ForecastStation", con);
            con.Open();
            adapter.SelectCommand = cmd;
            adapter.Fill(dt);
            adapter.Dispose();
            cmd.Dispose();
            con.Close();
            List<string> stationName = new List<string>();
            List<string> riverName = new List<string>();
            List<float> dangerlevel = new List<float>();
            List<float> rhwl = new List<float>();
            foreach (DataRow dr in dt.Rows)
            {
                stationName.Add(dr[0].ToString().Trim());
                riverName.Add(dr[1].ToString().Trim());
                dangerlevel.Add(Convert.ToSingle(dr[2]));
                rhwl.Add(Convert.ToSingle(dr[3]));
                listForecastStationBox.Items.Add(dr[0].ToString().Trim());
            }

            int z = 0;
            foreach (string element in stationName)
            {
                chartStationForecast.Series[0].Points.Clear();
                chartStationForecast.Series[1].Points.Clear();
                chartStationForecast.Series[2].Points.Clear();
                chartStationForecast.Series[3].Points.Clear();

                IDfsFile resFile = DfsFileFactory.DfsGenericOpen(@"E:\FFWS\Model\FF\Forecastpro\Forecasts\" + element + ".dfs0");
                IDfsFileInfo resfileInfo = resFile.FileInfo;
                int noTimeSteps = resfileInfo.TimeAxis.NumberOfTimeSteps;
                IDfsItemData<float> data;
                //MessageBox.Show(noTimeSteps.ToString());
                float[] Obsvalues = new float[289];
                DateTime[] dataObsDate = new DateTime[289];
                dataObsDate[0] = DateTime.Today.AddDays(-7).AddHours(6);
                //MessageBox.Show(dataDate[0].ToString());

                for (int j = 0; j < 289; j++)
                {
                    data = (IDfsItemData<float>)resFile.ReadItemTimeStep(1, j);
                    Obsvalues[j] = Convert.ToSingle(data.Data[0]);
                    dataObsDate[j] = dataObsDate[0].AddHours(j + 1);
                }

                chartStationForecast.Titles[0].Text = "Station Name: " + element + "  River Name: " + riverName[z] + "\r\n" + "Forecast Date: " + dataObsDate[168];

                for (int i = 0; i < 169; i++)
                {
                    chartStationForecast.Series[0].Points.AddXY(dataObsDate[i], Obsvalues[i]);
                }

                for (int i = 0; i < 120; i++)
                {
                    chartStationForecast.Series[1].Points.AddXY(dataObsDate[169 + i], Obsvalues[169 + i]);
                }

                for (int i = 0; i < 289; i++)
                {
                    chartStationForecast.Series[2].Points.AddXY(dataObsDate[i], dangerlevel[z]);
                    chartStationForecast.Series[3].Points.AddXY(dataObsDate[i], rhwl[z]);
                }

                chartStationForecast.Series[0].ChartType = SeriesChartType.Line;  // Set chart type like Bar chart, Pie chart
                chartStationForecast.Series[0].IsValueShownAsLabel = false;
                chartStationForecast.Series[0].BorderWidth = 1;

                chartStationForecast.Series[1].ChartType = SeriesChartType.Point;
                chartStationForecast.Series[1].IsValueShownAsLabel = false;
                chartStationForecast.Series[1].MarkerSize = 2;

                chartStationForecast.Series[2].ChartType = SeriesChartType.Line;  // Set chart type like Bar chart, Pie chart
                chartStationForecast.Series[2].IsValueShownAsLabel = false;
                chartStationForecast.Series[2].BorderWidth = 2;

                chartStationForecast.Series[3].ChartType = SeriesChartType.Line;  // Set chart type like Bar chart, Pie chart
                chartStationForecast.Series[3].IsValueShownAsLabel = false;
                chartStationForecast.Series[3].BorderWidth = 2;
                chartStationForecast.Legends[0].Enabled = true;
                chartStationForecast.Legends[0].Position = new ElementPosition(70, (dangerlevel[z] + 1 / 10), 0, 0);

                chartStationForecast.ChartAreas[0].AxisY.Maximum = Math.Ceiling(rhwl[z]);
                chartStationForecast.ChartAreas[0].AxisY.Minimum = Math.Floor(Obsvalues.Min());

                chartStationForecast.ChartAreas[0].AxisY.MajorTickMark.Interval = (Math.Ceiling(rhwl[z]) - Math.Floor(Obsvalues.Min())) / 5.0;
                chartStationForecast.ChartAreas[0].AxisY.LabelStyle.Format = "0.0";

                chartStationForecast.SaveImage(@"E:\FFWS\ForecastJpg\" + element + ".jpg", ChartImageFormat.Jpeg);
                z++;

            }
            MessageBox.Show("Task Completed");
        }
        private void listForecastStationBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            chartStationForecast.Series[0].Points.Clear();
            chartStationForecast.Series[1].Points.Clear();
            chartStationForecast.Series[2].Points.Clear();
            chartStationForecast.Series[3].Points.Clear();


            string element = listForecastStationBox.SelectedItem.ToString();
            IDfsFile resFile = DfsFileFactory.DfsGenericOpen(@"E:\FFWS\Model\FF\Forecastpro\Forecasts\" + element + ".dfs0");
            IDfsFileInfo resfileInfo = resFile.FileInfo;
            int noTimeSteps = resfileInfo.TimeAxis.NumberOfTimeSteps;
            IDfsItemData<float> data;
            //MessageBox.Show(noTimeSteps.ToString());
            float[] Obsvalues = new float[289];
            DateTime[] dataObsDate = new DateTime[289];
            dataObsDate[0] = DateTime.Today.AddDays(-7).AddHours(6);
            //MessageBox.Show(dataDate[0].ToString());

            for (int j = 0; j < 289; j++)
            {
                data = (IDfsItemData<float>)resFile.ReadItemTimeStep(1, j);
                Obsvalues[j] = Convert.ToSingle(data.Data[0]);
                dataObsDate[j] = dataObsDate[0].AddHours(j + 1);
            }

            dt.Columns.Clear();
            cmd = new SqlCommand("Select RiverName, DangerLevel, RHWL from ForecastStation Where StationName=@station", con);
            cmd.Parameters.AddWithValue("@station", element);
            con.Open();
            adapter.SelectCommand = cmd;
            adapter.Fill(dt);
            adapter.Dispose();
            cmd.Dispose();
            cmd.Parameters.Clear();
            con.Close();
            string riverName = dt.Rows[0].ItemArray[0].ToString();
            float dangerlevel = float.Parse(dt.Rows[0].ItemArray[1].ToString());
            float rhwl = float.Parse(dt.Rows[0].ItemArray[2].ToString());
            //MessageBox.Show(element + " " + riverName + " " + dangerlevel + " " + rhwl);

            chartStationForecast.Titles[0].Text = "Station Name: " + element + "  River Name: " + riverName + "\r\n" + "Forecast Date: " + dataObsDate[168];

            for (int i = 0; i < 169; i++)
            {
                chartStationForecast.Series[0].Points.AddXY(dataObsDate[i], Obsvalues[i]);
            }

            for (int i = 0; i < 120; i++)
            {
                chartStationForecast.Series[1].Points.AddXY(dataObsDate[169 + i], Obsvalues[169 + i]);
            }

            for (int i = 0; i < 289; i++)
            {
                chartStationForecast.Series[2].Points.AddXY(dataObsDate[i], dangerlevel);
                chartStationForecast.Series[3].Points.AddXY(dataObsDate[i], rhwl);
            }

            chartStationForecast.Series[0].ChartType = SeriesChartType.Line;  // Set chart type like Bar chart, Pie chart
            chartStationForecast.Series[0].IsValueShownAsLabel = false;
            chartStationForecast.Series[0].BorderWidth = 2;

            chartStationForecast.Series[1].ChartType = SeriesChartType.Point;
            chartStationForecast.Series[1].IsValueShownAsLabel = false;
            chartStationForecast.Series[1].MarkerSize = 2;

            chartStationForecast.Series[2].ChartType = SeriesChartType.Line;  // Set chart type like Bar chart, Pie chart
            chartStationForecast.Series[2].IsValueShownAsLabel = false;
            chartStationForecast.Series[2].BorderWidth = 2;

            chartStationForecast.Series[3].ChartType = SeriesChartType.Line;  // Set chart type like Bar chart, Pie chart
            chartStationForecast.Series[3].IsValueShownAsLabel = false;
            chartStationForecast.Series[3].BorderWidth = 2;
            chartStationForecast.Legends[0].Enabled = true;
            chartStationForecast.Legends[0].Position = new ElementPosition(70, (dangerlevel + 1 / 10), 0, 0);

            chartStationForecast.ChartAreas[0].AxisY.Maximum = Math.Ceiling(rhwl);
            chartStationForecast.ChartAreas[0].AxisY.Minimum = Math.Floor(Obsvalues.Min());

            chartStationForecast.ChartAreas[0].AxisY.MajorTickMark.Interval = (Math.Ceiling(rhwl) - Math.Floor(Obsvalues.Min())) / 5.0;
            chartStationForecast.ChartAreas[0].AxisY.LabelStyle.Format = "0.0";
        }
        private void btnForecastView_Click(object sender, EventArgs e)
        {
            listForecastStationBox.Visible = true;
            chartStationForecast.Visible = true;
            btnCloseForecastChart.Visible = true;
            btnRunFFModel.Visible = false;
            btnSaveForecastWL.Visible = false;
            btnExportForecastJpg.Visible = false;
            btnForecastView.Visible = false;
            dt.Columns.Clear();
            cmd = new SqlCommand("Select StationName from ForecastStation", con);
            con.Open();
            adapter.SelectCommand = cmd;
            adapter.Fill(dt);
            adapter.Dispose();
            cmd.Dispose();
            con.Close();
            foreach (DataRow dr in dt.Rows)
            {
                listForecastStationBox.Items.Add(dr[0].ToString().Trim());
            }
        }
        private void btnCloseForecastChart_Click(object sender, EventArgs e)
        {
            listForecastStationBox.Visible = false;
            chartStationForecast.Visible = false;
            btnCloseForecastChart.Visible = false;
            btnRunFFModel.Visible = true;
            btnSaveForecastWL.Visible = true;
            btnExportForecastJpg.Visible = true;
            btnForecastView.Visible = true;
        }

        private void btnReportViewer_Click(object sender, EventArgs e)
        {
            dataEntryGridView.Visible = false;
            listRFStationBox.Visible = false;
            lblWebpagelist.Visible = false;
            checkedWebsiteBox.Visible = false;
            btnSelectAll.Visible = false;
            btnClearAll.Visible = false;
            btnDownloadRF.Visible = false;
            btnUpdateTempRFDatabase.Visible = false;
            btnUpdateFinaldb.Visible = false;
            dataGridView1.Visible = false;
            dataGridView2.Visible = false;
            chart1.Visible = false;
            lblRFStartDate.Visible = false;
            lblRFEndDate.Visible = false;
            dateTimeRFStartPicker.Visible = false;
            dateTimeRFEndPicker.Visible = false;
            btnExportRFStationCSV.Visible = false;
            btnExportRFAllCSV.Visible = false;
            btnImportCSVRF.Visible = false;
            btnUpdatedataEntry.Visible = false;
            lblGISRFMapDate.Visible = false;
            dateTimeGISRFMapPicker.Visible = false;
            btnCreateGISRFText.Visible = false;
            btnUpdateGarbage.Visible = false;
            reportGBMStationRFViewer.Visible = true;
            lblReportViewerStartdate.Visible = true;
            lblReportViewerEndDate.Visible = true;
            dateTimeReportStartPicker.Visible = true;
            dateTimeReportEndPicker.Visible = true;
            btnViewReport.Visible = true;
        }
        private void btnViewReport_Click(object sender, EventArgs e)
        {
            DateTime startDate = dateTimeRFStartPicker.Value;
            DateTime endDate = dateTimeRFEndPicker.Value;
            cmd = new SqlCommand("Select Date, StationName, RFValue from GBMStationRF Where Date>= @startDate AND Date <= @endDate ORDER by Date ASC", con);
            cmd.Parameters.AddWithValue("@startDate", dateTimeReportStartPicker.Value);
            cmd.Parameters.AddWithValue("@endDate", dateTimeReportEndPicker.Value);
            con.Open();
            adapter.SelectCommand = cmd;
            adapter.Fill(dt);
            adapter.Dispose();
            cmd.Dispose();
            cmd.Parameters.Clear();
            con.Close();
            
            reportGBMStationRFViewer.ProcessingMode = ProcessingMode.Local;
            ReportDataSource source = new ReportDataSource("RFDataReport", dt);
            reportGBMStationRFViewer.LocalReport.DataSources.Clear();
            reportGBMStationRFViewer.LocalReport.ReportPath = @"E:\FFWS\FFWS\Report1.rdlc";
            reportGBMStationRFViewer.LocalReport.DataSources.Add(source);
            reportGBMStationRFViewer.RefreshReport();
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            
            cmd = new SqlCommand(@"DELETE FROM TempGBMStationRF", con);
            cmd.Connection = con;
            con.Open();
            cmd.ExecuteNonQuery();

            
            con.Open();
            SqlDataAdapter ad = new SqlDataAdapter("Select * from TempGBMStationRF", con);
            SqlCommandBuilder cmdbl = new SqlCommandBuilder(ad);
            System.Data.DataSet ds = new System.Data.DataSet("TempGBMStationRF");
            ad.Fill(ds, "TempGBMStationRF");
            con.Close();


            ////////////////////-----------------------------------This portion will be changed--------------------------------///////////////////////////////////////////

            cmd = new SqlCommand("select * from TempGBMStationRF", con);
            con.Open();
            System.Data.DataSet ds1 = new System.Data.DataSet();
            DataTable table = new DataTable();
            SqlDataAdapter adapter = new SqlDataAdapter();
            adapter.SelectCommand = cmd;
            adapter.Fill(ds1, "TempGBMStationRF");


            StringBuilder sb = new StringBuilder();
            WebBrowser wb = new WebBrowser();
            wb.Navigate("http://202.54.31.7/citywx/max_min_rain.php");

            while (wb.ReadyState != WebBrowserReadyState.Complete)
            {
                Application.DoEvents();
            }


            wb.Document.ExecCommand("SelectAll", false, null);
            wb.Document.ExecCommand("Copy", false, null);

            string downloadtext = Clipboard.GetText();
            DateTime datadate = new DateTime(int.Parse(downloadtext.Substring(33, 4)), int.Parse(downloadtext.Substring(30, 2)), int.Parse(downloadtext.Substring(27, 2)), 09, 00, 00);
            //Console.WriteLine(datadate.ToString());
            string firststeptext = downloadtext.Replace("        Max/Min Temp /24 hr RF(mm)   ", "");
            firststeptext = firststeptext.Substring(38, firststeptext.Length - 38);
            firststeptext = firststeptext.Replace(", ", ",");
            var textArray = firststeptext.Split(',');
            for (int i = 0; i < textArray.Length - 1; i++)
            {
                var individualstation = textArray[i].Split('/');
                if (individualstation[0].Substring(individualstation[0].Length - 1, 1) == "-") { individualstation[0] = individualstation[0].Substring(0, individualstation[0].Length - 3); }
                else { individualstation[0] = individualstation[0].Substring(0, individualstation[0].Length - 5); }
                if (individualstation[2] == "NIL") { individualstation[2] = "0"; }
                else if (individualstation[2] == "NA" || individualstation[2] == "999.90 mm") { individualstation[2] = "-"; }
                else { individualstation[2] = individualstation[2].Substring(0, individualstation[2].Length - 3); }

                DataRow row = ds.Tables["TempGBMStationRF"].NewRow();
                row[0] = datadate;
                row[1] = individualstation[0];
                row[2] = individualstation[2];
                row[3] = 0;
                ds.Tables["TempGBMStationRF"].Rows.Add(row);
                ad.Update(ds, "TempGBMStationRF");
                con.Close();
                Console.WriteLine(datadate.ToString() + "," + individualstation[0] + "," + individualstation[2]);
            }
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
