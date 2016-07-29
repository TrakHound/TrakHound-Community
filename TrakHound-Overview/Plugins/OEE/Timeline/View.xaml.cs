// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using TrakHound.Configurations;
using TrakHound.Tools;
using TrakHound.Plugins;
using TrakHound.Plugins.Client;

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

        void Update(EventData data)
        {
            if (data != null && data.Data01 != null && data.Data01.GetType() == typeof(DeviceConfiguration))
            {
                // OEE Table Data
                if (data.Id.ToLower() == "statusdata_oee_segments")
                {
                    this.Dispatcher.BeginInvoke(new Action<object>(Update_OEEData), Priority_Context, new object[] { data.Data02 });
                }

                // Variables Table Data
                if (data.Id.ToLower() == "statusdata_variables")
                {
                    this.Dispatcher.BeginInvoke(new Action<object>(Update_VariablesData), Priority_Context, new object[] { data.Data02 });
                }

                // Shift Segments Table Data
                if (data.Id.ToLower() == "statusdata_shiftsegments")
                {
                    this.Dispatcher.BeginInvoke(new Action<object>(Update_ShiftSegmentsData), Priority_Context, new object[] { data.Data02 });
                }
            }
        }

        class OEE_TimelineInfo
        {
            public OEE_TimelineInfo()
            {
                HourInfos = new List<HourInfo>();
                Id = String_Functions.RandomString(20);
            }

            // Links to Timeline.Databar
            public string Id { get; set; }

            public string Title
            {
                get
                {
                    if (HourInfos != null)
                    {
                        var infos = HourInfos.OrderBy(x => x.Start).ToList();
                        var first = infos[0];
                        var last = infos[infos.Count - 1];

                        return first.Start.ToShortTimeString() + " - " + last.End.ToShortTimeString();
                    }
                    return null;
                }
            }

            public List<HourInfo> HourInfos { get; set; }

            public double Oee { get; set; }
            public double Availability { get; set; }
            public double Performance { get; set; }
        }

        void Update_OEEData(object oeedata)
        {
            var dt = oeedata as DataTable;
            if (dt != null && timelineInfos != null)
            {
                var oeeData = OEEData.FromDataTable(dt);

                foreach (OEE_TimelineInfo info in timelineInfos)
                {
                    var oee = new OEEData();
                    oee.ConstantQuality = 1;

                    foreach (var hourInfo in info.HourInfos)
                    {
                        var matches = oeeData.ShiftSegments.FindAll(x => TestShiftSegment(x.ShiftId, hourInfo.ShiftIdSuffix));
                        oee.ShiftSegments.AddRange(matches);
                    }

                    //foreach (var segment in oee.ShiftSegments) TH_Global.Logger.Log(segment.ShiftId);

                    //foreach (var segment in oeeData.ShiftSegments)
                    //{
                    //    var match = info.HourInfos.Find(x => TestShiftSegment(segment.ShiftId, x.ShiftIdSuffix));
                    //    if (match != null)
                    //    {
                    //        oee.ShiftSegments.Add(segment);
                    //    }
                    //}

                    int index = histogram.DataBars.ToList().FindIndex(x => x.Id == info.Id);
                    if (index >= 0)
                    {
                        var db = histogram.DataBars[index];

                        db.Value = oee.Oee * 100;

                        int status = 0;
                        if (oee.Oee >= 0.75) status = 2;
                        else if (oee.Oee >= 0.5) status = 1;
                        else status = 0;

                        db.DataObject = status;

                        // Update ToolTip
                        if (db.ToolTipData != null)
                        {
                            var toolTip = (OeeToolTip)db.ToolTipData;
                            toolTip.Oee = oee.Oee.ToString("P2");
                            toolTip.Availability = oee.Availability.ToString("P2");
                            toolTip.Performance = oee.Performance.ToString("P2");
                        }
                    }
                }
            }
        }

        private List<OEE_TimelineInfo> timelineInfos = new List<OEE_TimelineInfo>();
        private bool newShift = true;

        public class HourInfo
        {
            public DateTime Start { get; set; }
            public DateTime End { get; set; }

            public int StartHour { get; set; }
            public int EndHour { get; set; }

            public string StartText { get; set; }
            public string EndText { get; set; }

            public string ShiftIdSuffix { get; set; }
            public string ShiftIdTest { get; set; }

            public static HourInfo Get(DataRow row, string date)
            {
                string start = DataTable_Functions.GetRowValue("START", row);
                string end = DataTable_Functions.GetRowValue("END", row);

                string sshiftId = DataTable_Functions.GetRowValue("SHIFT_ID", row);
                string ssegmentId = DataTable_Functions.GetRowValue("SEGMENT_ID", row);

                int shiftId = -1;
                int segmentId = -1;
                int.TryParse(sshiftId, out shiftId);
                int.TryParse(ssegmentId, out segmentId);

                string shiftIdSuffix = null;
                string shiftIdTest = null;
                if (shiftId >= 0 && segmentId >= 0) shiftIdSuffix = "_" + shiftId.ToString("00") + "_" + segmentId.ToString("00");
                if (shiftId >= 0 && segmentId >= 0) shiftIdTest = "_" + shiftId.ToString("00") + "_";

                DateTime s = DateTime.MinValue;
                DateTime e = DateTime.MinValue;

                DateTime.TryParse(date + " " + start, out s);
                DateTime.TryParse(date + " " + end, out e);

                var info = new HourInfo();
                info.Start = s;
                info.End = e;

                info.StartHour = s.Hour;
                info.EndHour = e.Hour;

                info.StartText = start;
                info.EndText = end;

                info.ShiftIdSuffix = shiftIdSuffix;
                info.ShiftIdTest = shiftIdTest;

                return info;
            }
        }

        private static bool TestShiftId(string s1, string s2)
        {
            if (s1 == null || s2 == null) return false;
            else if (s1.Contains(s2)) return true;
            else return false;
        }

        private static bool TestShiftSegment(string s1, string s2)
        {
            if (s1 == null || s2 == null) return false;
            if (s1.EndsWith(s2)) return true;
            else return false;
        }

        void Update_ShiftSegmentsData(object shiftSegmentsData)
        {
            var dt = shiftSegmentsData as DataTable;
            if (dt != null && currentShiftId != null && currentShiftDate != null && newShift)
            {
                timelineInfos.Clear();

                var segments = new List<HourInfo>();

                // Get Hour Infos for each Row / Segment
                foreach (DataRow row in dt.Rows) segments.Add(HourInfo.Get(row, currentShiftDate));

                //segments = segments.FindAll(x => TestShiftId(currentShiftId, x.ShiftIdTest));

                var hours = segments.GroupBy(x => x.StartHour, (key, group) => group.First()).ToList();

                // Create OEE_TimelineInfos for each Hour
                foreach (var hour in hours)
                {
                    var info = new OEE_TimelineInfo();

                    var sameHours = segments.FindAll(s => s.StartHour == hour.StartHour);
                    if (sameHours != null)
                    {
                        foreach (var sameHour in sameHours)
                        {
                            info.HourInfos.Add(sameHour);
                        }
                    }

                    timelineInfos.Add(info);
                }

                histogram.DataBars.Clear();

                foreach (var info in timelineInfos)
                {
                    //TrakHound_UI.Histogram.DataBar db;

                    int dbIndex = histogram.DataBars.ToList().FindIndex(x => x.Id == info.Id);
                    if (dbIndex < 0)
                    {
                        var db = new TrakHound_UI.Histogram.DataBar();
                        db.Id = info.Id;

                        var tt = new OeeToolTip();
                        tt.Times = info.Title;
                        db.ToolTipData = tt;

                        histogram.AddDataBar(db);
                    }
                }

                newShift = false;
            } 
        }

        //void Update_ShiftData(object shiftData)
        //{
        //    // Get Segment Times (for ToolTip)
        //    foreach (TrakHound_UI.Histogram.DataBar db in histogram.DataBars)
        //    {
        //        string segmentTimes = GetSegmentName(db.Id, shiftData);
        //        if (segmentTimes != null)
        //        {
        //            if (db.ToolTipData != null)
        //            {
        //                var tooltip = (OeeToolTip)db.ToolTipData;
        //                if (tooltip.Times != segmentTimes) tooltip.Times = segmentTimes;
        //            }
        //        }
        //    }
        //}


        string currentShiftId = null;
        string currentShiftName = null;
        string currentShiftDate = null;

        void Update_VariablesData(object variableData)
        {
            var infos = new List<OEE_TimelineInfo>();

            var dt = variableData as DataTable;
            if (dt != null)
            {
                currentShiftId = DataTable_Functions.GetTableValue(dt, "variable", "shift_id", "value");

                currentShiftDate = DataTable_Functions.GetTableValue(dt, "variable", "shift_date", "value");

                // Get Shift Name to check if still in the same shift as last update
                string prev_currentShift = currentShiftName;
                currentShiftName = DataTable_Functions.GetTableValue(dt, "variable", "shift_name", "value");
                if (prev_currentShift != currentShiftName) newShift = true;
            }
        }

        //string GetSegmentName(string shiftId, object shiftData)
        //{
        //    string Result = null;

        //    DataTable dt = shiftData as DataTable;
        //    if (dt != null)
        //    {
        //        DataView dv = dt.AsDataView();
        //        dv.RowFilter = "id='" + shiftId + "'";
        //        DataTable temp_dt = dv.ToTable();

        //        // Should be max of one row
        //        if (temp_dt.Rows.Count > 0)
        //        {
        //            DataRow row = temp_dt.Rows[0];

        //            //Get Segment start Time
        //            string start = DataTable_Functions.GetRowValue("start", row);

        //            // Get Segment end Time
        //            string end = DataTable_Functions.GetRowValue("end", row);

        //            // Create Segment Times string
        //            if (start != null && end != null)
        //            {
        //                DateTime timestamp = DateTime.MinValue;
        //                DateTime.TryParse(start, out timestamp);
        //                if (timestamp > DateTime.MinValue) start = timestamp.ToShortTimeString();

        //                timestamp = DateTime.MinValue;
        //                DateTime.TryParse(end, out timestamp);
        //                if (timestamp > DateTime.MinValue) end = timestamp.ToShortTimeString();

        //                Result = start + " - " + end;
        //            }
        //        }
        //    }

        //    return Result;
        //}

    }
}
