// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Windows;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Controls;

using TH_Configuration;
using TH_Global.Functions;
using TH_Plugins;

namespace TH_DevicePage.Controls
{
    /// <summary>
    /// Interaction logic for OeeTimeline.xaml
    /// </summary>
    public partial class OeeTimeline : UserControl
    {
        public OeeTimeline()
        {
            InitializeComponent();
            root.DataContext = this;
        }


        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(OeeTimeline), new PropertyMetadata(null));



        public double MaxBarWidth
        {
            get { return (double)GetValue(MaxBarWidthProperty); }
            set { SetValue(MaxBarWidthProperty, value); }
        }

        public static readonly DependencyProperty MaxBarWidthProperty =
            DependencyProperty.Register("MaxBarWidth", typeof(double), typeof(OeeTimeline), new PropertyMetadata(20d));



        public void Update(EventData data)
        {
            if (data != null && data.Data01 != null && data.Data01.GetType() == typeof(Configuration))
            {
                // OEE Table Data
                if (data.Id.ToLower() == "statusdata_oee")
                {
                    // OEE Values
                    this.Dispatcher.BeginInvoke(new Action<object>(Update_OEEData), DevicePage.Priority_Background, new object[] { data.Data02 });
                }

                // Variables Table Data
                if (data.Id.ToLower() == "statusdata_variables")
                {
                    // OEE Timeline / Histogram
                    this.Dispatcher.BeginInvoke(new Action<object>(Update_VariablesData), DevicePage.Priority_Background, new object[] { data.Data02 });
                }

                // Shifts Table Data
                if (data.Id.ToLower() == "statusdata_shiftdata")
                {
                    // OEE Timeline / Histogram
                    this.Dispatcher.BeginInvoke(new Action<object>(Update_ShiftData), DevicePage.Priority_Background, new object[] { data.Data02 });
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



                    int dbIndex = oee_timeline.DataBars.ToList().FindIndex(x => x.Id == info.id);
                    if (dbIndex < 0)
                    {
                        db = new TH_WPF.Histogram.DataBar();
                        db.Id = info.id;

                        var tt = new OeeToolTip();
                        tt.Times = info.segmentTimes;
                        db.ToolTipData = tt;

                        oee_timeline.AddDataBar(db);

                    }
                    else db = oee_timeline.DataBars[dbIndex];

                    db.Value = info.Oee * 100;

                    // Update ToolTip
                    if (db.ToolTipData != null)
                    {
                        var toolTip = (OeeToolTip)db.ToolTipData;
                        toolTip.Oee = info.Oee.ToString("P2");
                        toolTip.Availability = info.Availability.ToString("P2");
                        toolTip.Performance = info.Performance.ToString("P2");
                    }
                }
            }
        }

        void Update_ShiftData(object shiftData)
        {
            // Get Segment Times (for ToolTip)
            foreach (TH_WPF.Histogram.DataBar db in oee_timeline.DataBars)
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
                string prev_shiftName = oee_timeline.Id;
                oee_timeline.Id = DataTable_Functions.GetTableValue(dt, "variable", "shift_name", "value");
                if (prev_shiftName != oee_timeline.Id) oee_timeline.DataBars.Clear();

                // Get Current Segment
                foreach (TH_WPF.Histogram.DataBar db in oee_timeline.DataBars)
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
