using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Data.SqlClient;
using System.Net;
using System.Collections.Specialized;

namespace FFDataProcessor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        string filePath = "";
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.ShowDialog();
            filePath = dialog.FileName;
            string[] ffDataFile = File.ReadAllLines(filePath);
            var vari = ffDataFile[0].Split(';');
            string[][] jaggedString = new string[vari.Count()][];
            for (int i = 0; i < jaggedString.Length; i++)
            {
                jaggedString[i] = new string[ffDataFile.Length];
            }

            int x = 0;
            foreach (string element in ffDataFile)
            {
                var separatedText = element.Split(';');
                for (int j = 0; j < separatedText.Length; j++)
                {
                    jaggedString[j][x] = separatedText[j];
                }
                x = x + 1;
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < jaggedString.Length; i++)
            {
                if (jaggedString[i][0] == "WATER LEVEL")
                {
                    for (int j = 2; j < jaggedString[i].Length; j++)
                    {
                        if (jaggedString[i][j] != "-9999")
                        {
                            sb.AppendLine(jaggedString[0][j] + "," + jaggedString[i][1].Substring(0, jaggedString[i][1].Length - 6) + "," + jaggedString[i][j]);
                        }
                        else { continue; }
                    }
                }
                else { continue; }
            }
            File.WriteAllText(filePath.Substring(0, filePath.Length-4) + ".txt", sb.ToString());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SqlConnection con = new SqlConnection(@"Data Source=NKB-PC\SQLEXPRESS;AttachDbFilename=E:\FFWS\Database\GBMStationRF.mdf;Integrated Security=True;User Instance=True");
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter ad = new SqlDataAdapter();

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.ShowDialog();
            filePath = dialog.FileName;
            string[] ffDataFile = File.ReadAllLines(filePath);
            List<DateTime> dataDate = new List<DateTime>();
            List<string> stationName = new List<string>();
            List<string> wlValue = new List<string>();
            foreach (string element in ffDataFile)
            {
                var separatedText = element.Split(',');
                dataDate.Add(DateTime.Parse(separatedText[0]));
                stationName.Add(separatedText[1]);
                wlValue.Add(separatedText[2]);
            }
            MessageBox.Show("Data read Completed.");
            con.Open();
            for (int i = 0; i < dataDate.Count; i++)
            {
                try
                {
                    cmd = new SqlCommand("INSERT INTO GBMStationWL VALUES(@dataDate, @individual, @individual2, 15)", con); 
                    cmd.Parameters.AddWithValue("@dataDate", dataDate[i]);
                    cmd.Parameters.AddWithValue("@individual", stationName[i]);
                    cmd.Parameters.AddWithValue("@individual2", wlValue[i]);
                    cmd.ExecuteNonQuery();
                }
                catch (SqlException)
                {
                    cmd = new SqlCommand("Update FFWCData SET WLValue = @Value Where Date = @dataDate AND Station= @station", con);
                    cmd.Parameters.AddWithValue("@dataDate", dataDate[i]);
                    cmd.Parameters.AddWithValue("@station", stationName[i]);
                    cmd.Parameters.AddWithValue("@Value", wlValue[i]);
                }
            }
            con.Close();
            MessageBox.Show("Database Updated.");
        }
    }
}
