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

using TH_Configuration;
using TH_Global.Functions;
using TH_Plugins_Client;

namespace TH_DeviceCompare_OEE.Timeline
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class Plugin
    {
        public Plugin()
        {
            InitializeComponent();
            root.DataContext = this;
        }

        const System.Windows.Threading.DispatcherPriority Priority_Background = System.Windows.Threading.DispatcherPriority.Background;

        const System.Windows.Threading.DispatcherPriority Priority_Context = System.Windows.Threading.DispatcherPriority.ContextIdle;

        void Update(DataEvent_Data de_d)
        {
            if (de_d != null && de_d.data01 != null && de_d.data01.GetType() == typeof(Configuration))
            {
                // OEE Table Data
                if (de_d.id.ToLower() == "statusdata_oee")
                {
                    // OEE Values
                    this.Dispatcher.BeginInvoke(new Action<object>(Update_OEEData), Priority_Context, new object[] { de_d.data02 });
                }

                // Variables Table Data
                if (de_d.id.ToLower() == "statusdata_variables")
                {
                    // OEE Timeline / Histogram
                    this.Dispatcher.BeginInvoke(new Action<object>(Update_VariablesData), Priority_Context, new object[] { de_d.data02 });
                }

                // Shifts Table Data
                if (de_d.id.ToLower() == "statusdata_shiftdata")
                {
                    // OEE Timeline / Histogram
                    this.Dispatcher.BeginInvoke(new Action<object>(Update_ShiftData), Priority_Context, new object[] { de_d.data02 });
                }
            }
        }

        class OEE_TimelineInfo
        {
            public string id { get; set; }
            public string segmentTimes { get; set; }

            public double Oee { get; set; }
            public double Availability { get; set; }
            public double Performance { get; set; }
        }

        void Update_OEEData(object oeedata)
        {
            List<OEE_TimelineInfo> infos = new List<OEE_TimelineInfo>();

            DataTable dt = oeedata as DataTable;
            if (dt != null)
            {
                var oeeData = OEEData.FromDataTable(dt);

                foreach (var segment in oeeData.ShiftSegments)
                {
                    OEE_TimelineInfo info = new OEE_TimelineInfo();
                    info.id = segment.ShiftId;

                    info.Oee = segment.Oee;
                    info.Availability = segment.Availability;
                    info.Performance = segment.Performance;

                    infos.Add(info);
                }

                foreach (OEE_TimelineInfo info in infos)
                {
                    TH_WPF.Histogram.DataBar db;

                    int dbIndex = histogram.DataBars.ToList().FindIndex(x => x.Id == info.id);
                    if (dbIndex < 0)
                    {
                        db = new TH_WPF.Histogram.DataBar();
                        db.Id = info.id;
                        histogram.AddDataBar(db);
                    }
                    else db = histogram.DataBars[dbIndex];

                    db.Value = info.Oee * 100;

                    var toolTip = new OeeToolTip();
                    toolTip.Times = info.segmentTimes;
                    toolTip.Oee = info.Oee.ToString("P2");
                    toolTip.Availability = info.Availability.ToString("P2");
                    toolTip.Performance = info.Performance.ToString("P2");
                    db.ToolTipData = toolTip;
                }
            }
        }

        void Update_ShiftData(object shiftData)
        {
            // Get Segment Times (for ToolTip)
            foreach (TH_WPF.Histogram.DataBar db in histogram.DataBars)
            {
                string segmentTimes = GetSegmentName(db.Id, shiftData);
                if (segmentTimes != null)
                {
                    if (db.ToolTipData != null)
                    {
                        var tooltip = (OeeToolTip)db.ToolTipData;
                        if (tooltip.Times != segmentTimes) tooltip.Times = segmentTimes;
                    }
                }
            }
        }

        void Update_VariablesData(object variableData)
        {
            List<OEE_TimelineInfo> infos = new List<OEE_TimelineInfo>();

            DataTable dt = variableData as DataTable;
            if (dt != null)
            {
                // Get Shift Name to check if still in the same shift as last update
                string prev_shiftName = histogram.Id;
                histogram.Id = DataTable_Functions.GetTableValue(dt, "variable", "shift_name", "value");
                if (prev_shiftName != histogram.Id) histogram.DataBars.Clear();

                // Get Current Segment
                foreach (TH_WPF.Histogram.DataBar db in histogram.DataBars)
                {
                    string currentShiftId = DataTable_Functions.GetTableValue(dt, "variable", "shift_id", "value");
                    if (currentShiftId == db.Id) db.IsSelected = true;
                    else db.IsSelected = false;
                }
            }
        }

        string GetSegmentName(string shiftId, object shiftData)
        {
            string Result = null;

            DataTable dt = shiftData as DataTable;
            if (dt != null)
            {
                DataView dv = dt.AsDataView();
                dv.RowFilter = "id='" + shiftId + "'";
                DataTable temp_dt = dv.ToTable();

                // Should be max of one row
                if (temp_dt.Rows.Count > 0)
                {
                    DataRow row = temp_dt.Rows[0];

                    //Get Segment start Time
                    string start = DataTable_Functions.GetRowValue("start", row);

                    // Get Segment end Time
                    string end = DataTable_Functions.GetRowValue("end", row);

                    // Create Segment Times string
                    if (start != null && end != null)
                    {
                        DateTime timestamp = DateTime.MinValue;
                        DateTime.TryParse(start, out timestamp);
                        if (timestamp > DateTime.MinValue) start = timestamp.ToShortTimeString();

                        timestamp = DateTime.MinValue;
                        DateTime.TryParse(end, out timestamp);
                        if (timestamp > DateTime.MinValue) end = timestamp.ToShortTimeString();

                        Result = start + " - " + end;
                    }
                }
            }

            return Result;
        }

    }
}
