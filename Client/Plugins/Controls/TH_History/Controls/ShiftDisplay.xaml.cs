using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Data;
using System.Collections.ObjectModel;

using TH_Global.Functions;

namespace TH_History.Controls
{
    /// <summary>
    /// Interaction logic for ShiftDisplay.xaml
    /// </summary>
    public partial class ShiftDisplay : UserControl
    {
        public ShiftDisplay()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void LoadData(DataTable oee_dt, DataTable shift_dt)
        {
            // Create Shift Info for entire day
            day_ShiftInfo.LoadData(GetData(oee_dt));

            ShiftInfos.Clear();

            // Create Shift Infos for each shift during day
            List<ShiftData> shiftDatas = GetShiftNames(shift_dt);
            var distinctShiftDatas = shiftDatas.Select(x => x.ShiftId).Distinct();

            foreach (string shiftId in distinctShiftDatas.ToList())
            {
                ShiftInfos.Add(CreateShiftInfo(GetShiftData(oee_dt, shiftId)));
            }
        }

        private ObservableCollection<Controls.ShiftInfo> shiftinfos;
        public ObservableCollection<Controls.ShiftInfo> ShiftInfos
        {
            get
            {
                if (shiftinfos == null)
                    shiftinfos = new ObservableCollection<Controls.ShiftInfo>();

                return shiftinfos;
            }

            set
            {
                shiftinfos = value;
            }
        }

        ShiftInfo CreateShiftInfo(ShiftInfo.Data data)
        {
            var info = new ShiftInfo();
            info.LoadData(data);
            return info;
        }


        class ShiftData
        {
            public string Name { get; set; }
            public string FullId { get; set; }
            public string ShiftId { get; set; }
        }

        List<ShiftData> GetShiftNames(DataTable shifts_dt)
        {
            List<ShiftData> result = new List<ShiftData>();

            foreach (DataRow row in shifts_dt.Rows)
            {
                var shiftData = new ShiftData();
                shiftData.Name = DataTable_Functions.GetRowValue("SHIFT", row);

                string id = DataTable_Functions.GetRowValue("ID", row);
                if (id != null)
                {
                    shiftData.FullId = id;
                    int index = id.LastIndexOf('_');
                    if (index > 0) shiftData.ShiftId = id.Substring(0, index);
                }

                result.Add(shiftData);
            }

            return result;
        }


        ShiftInfo.Data GetShiftData(DataTable oee_dt, string shiftId)
        {
            DataView dv = oee_dt.AsDataView();
            dv.RowFilter = "SHIFT_ID LIKE '" + shiftId + "*'";
            DataTable temp_dt = dv.ToTable();

            return GetData(temp_dt);
        }

        ShiftInfo.Data GetData(DataTable oee_dt)
        {
            List<double> oees = new List<double>();

            foreach (DataRow row in oee_dt.Rows)
            {
                string shiftId = row["SHIFT_ID"].ToString();

                string oee_str = row["OEE"].ToString();
                double oee = 0;
                double.TryParse(oee_str, out oee);

                oees.Add(oee);
            }

            var data = new ShiftInfo.Data();
            data.OEEAverage = Math.Round(oees.Average(), 2);
            data.OEEMedian = Math.Round(Math_Functions.GetMedian(oees.ToArray()), 2);
            data.OEEStandardDeviation = Math.Round(Math_Functions.StdDev(oees.ToArray()), 2);

            return data;
        }


    }
}
