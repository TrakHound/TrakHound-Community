using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;

using System.Data;

using TH_Global.Functions;

namespace TH_DeviceCompare
{
    public partial class DeviceCompare
    {

        #region "Values"

        void OEEValues_Update(DeviceDisplay dd, object oeedata)
        {
            DataTable dt = oeedata as DataTable;
            if (dt != null)
            {
                var oeeData = OEEData.FromDataTable(dt);

                OEE_Total(dd, oeeData);
                OEE_Availability(dd, oeeData);
                OEE_Performance(dd, oeeData);
            }
        }

        void OEE_Total(DeviceDisplay dd, OEEData oeeData)
        {
            string key = "oee_total";
            double value = oeeData.Oee;

            UpdateNumberDisplay(dd, key, value);
        }

        void OEE_Availability(DeviceDisplay dd, OEEData oeeData)
        {
            string key = "oee_availability";
            double value = oeeData.Availability;

            UpdateNumberDisplay(dd, key, value);
        }

        void OEE_Performance(DeviceDisplay dd, OEEData oeeData)
        {
            string key = "oee_performance";
            double value = oeeData.Performance;

            UpdateNumberDisplay(dd, key, value);
        }

        #endregion

        #region "Timeline"

        class OEE_TimelineInfo
        {
            public string id { get; set; }
            public string segmentTimes { get; set; }

            public double Oee { get; set; }
            public double Availability { get; set; }
            public double Performance { get; set; }
        }

        void Update_OEE_Timeline_OEEData(DeviceDisplay dd, object oeedata)
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

                int cellIndex = dd.ComparisonGroup.column.Cells.ToList().FindIndex(x => x.Link.ToLower() == "oeetimeline");
                if (cellIndex >= 0)
                {
                    TH_WPF.Histogram.Histogram oeeTimeline;

                    object ddData = dd.ComparisonGroup.column.Cells[cellIndex].Data;
                    if (ddData == null)
                    {
                        oeeTimeline = new TH_WPF.Histogram.Histogram();
                        oeeTimeline.Name = "OEE";
                        oeeTimeline.Height = 100;
                        oeeTimeline.Width = 180;
                        oeeTimeline.Margin = new Thickness(0, 5, 0, 5);

                        dd.ComparisonGroup.column.Cells[cellIndex].Data = oeeTimeline;
                    }
                    else oeeTimeline = (TH_WPF.Histogram.Histogram)ddData;

                    foreach (OEE_TimelineInfo info in infos)
                    {
                        TH_WPF.Histogram.DataBar db;

                        int dbIndex = oeeTimeline.DataBars.ToList().FindIndex(x => x.Id == info.id);
                        if (dbIndex < 0)
                        {
                            db = new TH_WPF.Histogram.DataBar();
                            db.Id = info.id;
                            db.SegmentTimes = info.segmentTimes;
                            oeeTimeline.AddDataBar(db);
                        }
                        else db = oeeTimeline.DataBars[dbIndex];

                        db.Value = info.Oee * 100;

                        var toolTip = new Controls.OeeTimelineToolTip();
                        toolTip.Times = db.SegmentTimes;
                        toolTip.Oee = info.Oee.ToString("P2");
                        toolTip.Availability = info.Availability.ToString("P2");
                        toolTip.Performance = info.Performance.ToString("P2");
                        db.ToolTipData = toolTip;
                    }
                }

            }
        }

        void Update_OEE_Timeline_ShiftData(DeviceDisplay dd, object shiftData)
        {
            int cellIndex = dd.ComparisonGroup.column.Cells.ToList().FindIndex(x => x.Link.ToLower() == "oeetimeline");
            if (cellIndex >= 0)
            {
                TH_WPF.Histogram.Histogram oeeTimeline;

                object ddData = dd.ComparisonGroup.column.Cells[cellIndex].Data;

                if (ddData != null)
                {
                    oeeTimeline = (TH_WPF.Histogram.Histogram)ddData;
                    //oeeTimeline = (TH_WPF.Histogram.Histogram)((Grid)ddData).Children[0];

                    // Get Segment Times (for ToolTip)
                    foreach (TH_WPF.Histogram.DataBar db in oeeTimeline.DataBars)
                    {
                        string segmentTimes = GetSegmentName(db.Id, shiftData);
                        if (segmentTimes != null)
                        {
                            if (db.SegmentTimes != segmentTimes) db.SegmentTimes = segmentTimes;
                        }
                    }
                }
            }
        }

        void Update_OEE_Timeline_SnapshotData(DeviceDisplay dd, object snapshotData)
        {
            List<OEE_TimelineInfo> infos = new List<OEE_TimelineInfo>();

            DataTable dt = snapshotData as DataTable;
            if (dt != null)
            {
                int cellIndex = dd.ComparisonGroup.column.Cells.ToList().FindIndex(x => x.Link.ToLower() == "oeetimeline");
                if (cellIndex >= 0)
                {
                    TH_WPF.Histogram.Histogram oeeTimeline;

                    object ddData = dd.ComparisonGroup.column.Cells[cellIndex].Data;

                    if (ddData != null)
                    {
                        //oeeTimeline = (Controls.HistogramDisplay)ddData;
                        oeeTimeline = (TH_WPF.Histogram.Histogram)ddData;
                        //oeeTimeline = (TH_WPF.Histogram.Histogram)((Grid)ddData).Children[0];

                        // Get Shift Name to check if still in the same shift as last update
                        string prev_shiftName = oeeTimeline.shiftName;
                        oeeTimeline.shiftName = DataTable_Functions.GetTableValue(dt, "name", "Current Shift Name", "value");
                        if (prev_shiftName != oeeTimeline.shiftName) oeeTimeline.DataBars.Clear();

                        // Get Current Segment
                        foreach (TH_WPF.Histogram.DataBar db in oeeTimeline.DataBars)
                        {
                            string currentShiftId = DataTable_Functions.GetTableValue(dt, "name", "Current Shift Id", "value");
                            if (currentShiftId == db.Id) db.CurrentSegment = true;
                            else db.CurrentSegment = false;
                        }
                    }
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

        #endregion

        public class OEEData
        {
            public OEEData()
            {
                ShiftSegments = new List<SegmentData>();
            }

            public List<SegmentData> ShiftSegments { get; set; }

            public class SegmentData
            {
                public string ShiftId { get; set; }

                public double PlannedProductionTime { get; set; }
                public double OperatingTime { get; set; }

                // TrakHound added version of IdealCycleTime
                // Represents OperatingTime taking into account Overrides (if overrides = 100%)
                public double IdealOperatingTime { get; set; }

                public double IdealCycleTime { get; set; }
                public double IdealRunRate { get; set; }
                public int TotalPieces { get; set; }

                public int GoodPieces { get; set; }


                public double Oee
                {
                    get
                    {
                        return Availability * Performance;
                        //return Availability * Performance * Quality;
                    }
                }

                public double Availability
                {
                    get
                    {
                        if (PlannedProductionTime > 0)
                        {
                            return Math.Min(1, OperatingTime / PlannedProductionTime);
                        }
                        return 0;
                    }
                }

                public double Performance
                {
                    get
                    {
                        if (OperatingTime > 0)
                        {
                            return Math.Min(1, IdealOperatingTime / OperatingTime);
                        }
                        return 0;
                    }
                }

                public double Quality
                {
                    get
                    {
                        if (TotalPieces > 0)
                        {
                            return Math.Min(1, GoodPieces / TotalPieces);
                        }
                        return 0;
                    }
                }
            }

            /// <summary>
            /// Used for when certain information is not available (ex. Quality is not available to be calculated)
            /// </summary>
            #region "User Configurable Constant Values"

            public double ConstantAvailability { get; set; }
            public double ConstantPerformance { get; set; }
            public double ConstantQuality { get; set; }

            #endregion


            public double Oee
            {
                get
                {
                    return Availability * Performance * Quality;
                }
            }

            public double Availability
            {
                get
                {
                    if (ConstantAvailability > 0) return ConstantAvailability;

                    double ppt = 0;
                    double ot = 0;

                    foreach (var segment in ShiftSegments)
                    {
                        ppt += segment.PlannedProductionTime;
                        ot += segment.OperatingTime;
                    }

                    if (ppt > 0)
                    {
                        return Math.Min(1, ot / ppt);
                    }
                    return 0;
                }
            }

            public double Performance
            {
                get
                {
                    if (ConstantPerformance > 0) return ConstantPerformance;

                    double ot = 0;
                    double iot = 0;

                    foreach (var segment in ShiftSegments)
                    {
                        ot += segment.OperatingTime;
                        iot += segment.IdealOperatingTime;
                    }

                    if (ot > 0)
                    {
                        return Math.Min(1, iot / ot);
                    }
                    return 0;
                }
            }

            public double Quality
            {
                get
                {
                    if (ConstantQuality > 0) return ConstantQuality;

                    double tp = 0;
                    double gp = 0;

                    foreach (var segment in ShiftSegments)
                    {
                        tp += segment.TotalPieces;
                        gp += segment.GoodPieces;
                    }

                    if (tp > 0)
                    {
                        return Math.Min(1, gp / tp);
                    }
                    return 0;
                }
            }

            public OEEData Copy()
            {
                var result = new OEEData();

                foreach (var info in typeof(OEEData).GetProperties())
                {
                    if (info.CanWrite)
                    {
                        object val = info.GetValue(this, null);
                        info.SetValue(result, val, null);
                    }
                }

                return result;
            }


            public static OEEData FromDataTable(DataTable dt)
            {
                var result = new OEEData();

                var dv = dt.AsDataView();
                var temp_dt = dv.ToTable(true, "shift_id");

                foreach (DataRow row in temp_dt.Rows)
                {
                    string shiftId = row[0].ToString();

                    // Sort Table for just rows matching the ShiftId
                    dv = dt.AsDataView();
                    dv.RowFilter = "shift_id='" + shiftId + "'";
                    var id_dt = dv.ToTable();

                    double plannedProductionTime = 0;
                    double operatingTime = 0;
                    double idealOperatingTime = 0;

                    foreach (DataRow idRow in id_dt.Rows)
                    {
                        plannedProductionTime += GetDoubleFromDataRow("planned_production_time", idRow);
                        operatingTime += GetDoubleFromDataRow("operating_time", idRow);
                        idealOperatingTime += GetDoubleFromDataRow("ideal_operating_time", idRow);
                    }

                    var data = new OEEData.SegmentData();
                    data.ShiftId = shiftId;
                    data.PlannedProductionTime = plannedProductionTime;
                    data.OperatingTime = operatingTime;
                    data.IdealOperatingTime = idealOperatingTime;
                    result.ShiftSegments.Add(data);
                }

                result.ConstantQuality = 1;

                return result;
            }
        }

    }
}
