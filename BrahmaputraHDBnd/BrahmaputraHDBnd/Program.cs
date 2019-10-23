using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DHI.Generic.MikeZero.DFS;
using DHI.Generic.MikeZero;

namespace BrahmaputraHDBnd
{
    class Program
    {
        static void Main(string[] args)
        {
            int[] nodeNumber = new int[] { 899, 2686, 2856, 2866, 2331, 3806, 2231, 3831 };

            IDfsFile resFile = DfsFileFactory.DfsGenericOpenEdit(@"E:\FFWS\Model\MIKEHYDRO\GBM_MIKEHYDRO.mhydro - Result Files\RiverBasin_GBM.dfs0");
            IDfsFileInfo resfileInfo = resFile.FileInfo;
            int noTimeSteps = resfileInfo.TimeAxis.NumberOfTimeSteps;
            DateTime[] date = resFile.FileInfo.TimeAxis.GetDateTimes();
            DateTime startDate = date[0];
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
                string filename = @"E:\FFWS\Model\BrahmaputraHD\Boundary\" + element + ".dfs0";
                DfsBuilder filecreator = DfsBuilder.Create(element.ToString(), element.ToString(), 2014);
                filecreator.SetDataType(1);
                filecreator.SetGeographicalProjection(factory.CreateProjectionUndefined());
                filecreator.SetTemporalAxis(factory.CreateTemporalNonEqCalendarAxis(eumUnit.eumUsec, new DateTime(startDate.Year, startDate.Month, startDate.Day, startDate.Hour, startDate.Minute, startDate.Second)));
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
        }
    }
}
