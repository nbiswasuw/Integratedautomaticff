using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DHI.Generic.MikeZero.DFS;
using System.IO;
using System.Windows.Forms;

namespace HDResultRead
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "Mike HD Result Files|*.RES11";

                if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
                {
                    IDfsFile resFile = DfsFileFactory.DfsGenericOpen(dialog.FileName);
                    DateTime[] date = resFile.FileInfo.TimeAxis.GetDateTimes();
                    DateTime startDate = date[0];
                    IDfsFileInfo resfileInfo = resFile.FileInfo;
                    IDfsItemData<float> data;

                    int noTimeSteps = resfileInfo.TimeAxis.NumberOfTimeSteps;
                    DateTime[] dfsDate = new DateTime[noTimeSteps];
                    List<float> dfsWLData = new List<float>();
                    List<float> dfsQData = new List<float>();

                    for (int i = 0; i < noTimeSteps; i++)
                    {
                        dfsDate[i] = startDate.AddHours(resFile.ReadItemTimeStep(1, i).Time);
                    }

                    int totalWNode = 0;
                    int totalQNode = 0;
                    for (int i = 0; i < noTimeSteps; i++)
                    {
                        int Wcounter = 0;
                        int nodeWCount = 0;

                        int Qcounter = 0;
                        int nodeQCount = 0;
                        for (int j = 0; j < resFile.ItemInfo.Count; j++)
                        {
                            IDfsSimpleDynamicItemInfo dynamicItemInfo = resFile.ItemInfo[j];
                            string nameOftDynamicItem = dynamicItemInfo.Name;
                            string WLname = nameOftDynamicItem.Substring(0, 11);
                            string Qname = nameOftDynamicItem.Substring(0, 9);
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

                            else if (Qname == "Discharge")
                            {
                                Qcounter = dynamicItemInfo.ElementCount;
                                data = (IDfsItemData<float>)resFile.ReadItemTimeStep(j + 1, i);
                                for (int z = 0; z < Qcounter; z++)
                                {
                                    dfsQData.Add(Convert.ToSingle(data.Data[z]));
                                    nodeQCount = nodeQCount + 1;
                                }
                            }
                        }
                        Console.WriteLine(i);
                        totalWNode = nodeWCount;
                        totalQNode = nodeQCount;
                    }
                    for (int i = 0; i < noTimeSteps; i++)
                    {
                        for (int j = 0; j < totalWNode; j++)
                        {
                            sb.AppendLine(dfsDate[i] + "," + (j + 1) + "," + dfsWLData[i * totalWNode + j]);
                        }
                        File.AppendAllText(dialog.FileName.Substring(0, dialog.FileName.Length - 6) + "_WL.csv", sb.ToString());
                        sb.Clear();
                    }

                    for (int i = 0; i < noTimeSteps; i++)
                    {
                        for (int j = 0; j < totalQNode; j++)
                        {
                            sb.AppendLine(dfsDate[i] + "," + (j + 1) + "," + dfsQData[i * totalQNode + j]);
                        }
                        File.AppendAllText(dialog.FileName.Substring(0, dialog.FileName.Length - 6) + "_Q.csv", sb.ToString());
                        sb.Clear();
                    }
                    Console.WriteLine("Result file processed suceesssfully.");
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                }
            }
            catch (Exception error)
            {
                Console.WriteLine("HD Model Result files cannot be processed due to an error. Error: " + error.Message);
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
        
        
    }
}
