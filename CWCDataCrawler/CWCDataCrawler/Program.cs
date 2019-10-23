using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using HtmlAgilityPack;
using System.Data.SqlClient;
using System.Data;

namespace CWCDataCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            SqlConnection con = new SqlConnection(@"Data Source=NKB-PC\SQLEXPRESS;AttachDbFilename=E:\FFWS\Database\GBMStationRF.mdf;Integrated Security=True;User Instance=True");
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            try
            {
                con.Open();
                StringBuilder sb = new StringBuilder();
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
                        Console.WriteLine("WL-" + stationName + "," + dateWL.ToString("yyyy-MM-dd HH:mm:ss") + "," + valueWL);
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
                        Console.WriteLine("RF-" + stationName + "," + dateRF.ToString("yyyy-MM-dd HH:mm:ss") + "," + valueRF);
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
                //File.WriteAllText(@"D:\cwcFinal.txt", sb.ToString());
                Console.WriteLine("Data Successfully downloaded.");
                Console.ReadKey();
            }
            catch (Exception error)
            {
                Console.WriteLine("CWC Data cannot be downloaded due to an error. Error: " + error.Message);
                Console.ReadKey();
            }
        }
    }
}
