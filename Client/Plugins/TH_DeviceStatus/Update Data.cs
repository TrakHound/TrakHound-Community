using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using TH_Configuration;
using TH_Global;
using TH_Global.Functions;
using TH_Plugins;
using TH_WPF;

namespace TH_StatusTable
{
    public partial class StatusTable
    {

        const System.Windows.Threading.DispatcherPriority PRIORITY_BACKGROUND = System.Windows.Threading.DispatcherPriority.Background;
        const System.Windows.Threading.DispatcherPriority PRIORITY_CONTEXT_IDLE = System.Windows.Threading.DispatcherPriority.ContextIdle;

        void Update(EventData data)
        {
            if (data != null && data.Id != null && data.Data01 != null)
            {
                Configuration config = data.Data01 as Configuration;
                if (config != null)
                {
                    int index = DeviceInfos.ToList().FindIndex(x => x.Configuration.UniqueId == config.UniqueId);
                    if (index >= 0)
                    {
                        DeviceInfo info = DeviceInfos[index];

                        UpdateDatabaseConnection(data, info);
                        UpdateAvailability(data, info);

                        UpdateStatus(data, info);
                    }
                }
            }
        }

        private void UpdateDatabaseConnection(EventData data, DeviceInfo info)
        {
            if (data.Id.ToLower() == "statusdata_connection")
            {
                if (data.Data02.GetType() == typeof(bool))
                {
                    info.Connected = (bool)data.Data02;
                }
            }
        }

        private void UpdateAvailability(EventData data, DeviceInfo info)
        {
            if (data.Id.ToLower() == "statusdata_availability")
            {
                if (data.Data02.GetType() == typeof(bool))
                {
                    info.Available = (bool)data.Data02;
                }
            }
        }


        private void UpdateStatus(EventData data, DeviceInfo info)
        {
            if (data.Id.ToLower() == "statusdata_snapshots")
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    bool b;
                    string s;

                    // Alert
                    b = false;
                    s = GetTableValue(data.Data02, "Alert");
                    bool.TryParse(s, out b);
                    info.Alert = b;

                    // Idle
                    b = false;
                    s = GetTableValue(data.Data02, "Idle");
                    bool.TryParse(s, out b);
                    info.Idle = b;

                    // Production
                    b = false;
                    s = GetTableValue(data.Data02, "Production");
                    bool.TryParse(s, out b);
                    info.Production = b;

                }), PRIORITY_BACKGROUND, new object[] { });
            }

            if (data.Id.ToLower() == "statusdata_shiftsegments")
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    UpdateShiftSegments(data.Data02, info);
                }), PRIORITY_BACKGROUND, new object[] { });
            }

            if (data.Id.ToLower() == "statusdata_shiftdata")
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    UpdateShiftTimes(data.Data02, info);
                }), PRIORITY_BACKGROUND, new object[] { });
            }
        }


        /// <summary>
        ///  Create SegmentData objects for each shift segment
        /// </summary>
        /// <param name="shiftSegmentData"></param>
        /// <param name="info"></param>
        private void UpdateShiftSegments(object shiftSegmentData, DeviceInfo info)
        {
            DataTable dt = shiftSegmentData as DataTable;
            if (dt != null && info.HourDatas == null)
            {
                var hourDatas = new HourData[24];

                var segments = new List<HourInfo>();
                foreach (DataRow row in dt.Rows) segments.Add(HourInfo.Get(row));

                var hours = segments.GroupBy(x => x.StartHour, (key, group) => group.First()).ToList();

                for (var x = 0; x <= hours.Count - 1; x++)
                {
                    var hour = hours[x];

                    var hourData = new HourData();

                    HourInfo first = null;
                    HourInfo last = null;

                    var sameHours = segments.FindAll(s => s.StartHour == hour.StartHour);
                    if (sameHours != null)
                    {
                        for (var y = 0; y <= sameHours.Count - 1; y++)
                        {
                            var sameHour = sameHours[y];

                            if (y == 0) first = sameHour;
                            if (y >= 11) last = sameHour;

                            var data = new SegmentData();
                            data.Title = sameHour.Start.ToShortTimeString() + " - " + sameHour.End.ToShortTimeString();
                            data.HourInfo = sameHour;
                            data.Status = -1;
                            hourData.SegmentDatas[y] = data;
                        }
                    }

                    // Set Hour Data
                    if (first != null && last != null) hourData.Title = first.Start.ToShortTimeString() + " - " + last.End.ToShortTimeString();
                    hourData.Status = -1;
                    hourDatas[hour.StartHour] = hourData;
                }

                info.HourDatas = hourDatas;
            }
        }


        private class SegmentInfo
        {
            public HourInfo HourInfo { get; set; }

            public double TotalSeconds { get; set; }

            public double ProductionSeconds { get; set; }
            public double IdleSeconds { get; set; }
            public double AlertSeconds { get; set; }

            public static SegmentInfo Get(DataRow row)
            {
                var info = new SegmentInfo();
                info.HourInfo = HourInfo.Get(row);

                info.TotalSeconds = GetTime("totaltime", row);

                info.ProductionSeconds = GetTime("production__true", row);
                info.IdleSeconds = GetTime("idle__true", row);
                info.AlertSeconds = GetTime("alert__true", row);

                return info;
            }
        }

        private void UpdateShiftTimes(object shiftData, DeviceInfo info)
        {
            DataTable dt = shiftData as DataTable;
            if (dt != null && info.HourDatas != null)
            {
                // Get Segment Infos
                var segmentInfos = new List<SegmentInfo>();
                foreach (DataRow row in dt.Rows) segmentInfos.Add(SegmentInfo.Get(row));

                // Loop through both dimensions of the info.HourDatas array to set times
                for (var x = 0; x <= info.HourDatas.Length - 1; x++)
                {
                    double totalSeconds = 0;

                    double productionSeconds = 0;
                    double idleSeconds = 0;
                    double alertSeconds = 0;

                    if (x <= info.HourDatas.Length - 1)
                    {
                        var hourData = info.HourDatas[x];
                        if (hourData != null)
                        {

                            for (var y = 0; y <= 11; y++)
                            {
                                var data = hourData.SegmentDatas[y];
                                if (data != null)
                                {
                                    // Look for match in segmentInfos list
                                    var match = segmentInfos.Find(m => m.HourInfo.Start == data.HourInfo.Start && m.HourInfo.End == data.HourInfo.End);
                                    if (match != null)
                                    {
                                        data.TotalSeconds = match.TotalSeconds;
                                        totalSeconds += match.TotalSeconds;

                                        // Set status times for the segment
                                        data.ProductionSeconds = match.ProductionSeconds;
                                        data.IdleSeconds = match.IdleSeconds;
                                        data.AlertSeconds = match.AlertSeconds;

                                        // Inrement totals for the hour
                                        productionSeconds += match.ProductionSeconds;
                                        idleSeconds += match.IdleSeconds;
                                        alertSeconds += match.AlertSeconds;

                                        // Set status based on the which status had the most seconds
                                        if (data.AlertSeconds == 0 && data.IdleSeconds == 0 && data.ProductionSeconds == 0) data.Status = -1;
                                        else if (data.AlertSeconds >= data.IdleSeconds && data.AlertSeconds >= data.ProductionSeconds) data.Status = 0;
                                        else if (data.IdleSeconds > data.AlertSeconds && data.IdleSeconds >= data.ProductionSeconds) data.Status = 1;
                                        else data.Status = 2;
                                    }
                                }
                            }

                            hourData.TotalSeconds = totalSeconds;
                            hourData.ProductionSeconds = productionSeconds;
                            hourData.IdleSeconds = idleSeconds;
                            hourData.AlertSeconds = alertSeconds;

                            // Set status based on the which status had the most seconds
                            if (hourData.AlertSeconds == 0 && hourData.IdleSeconds == 0 && hourData.ProductionSeconds == 0) hourData.Status = -1;
                            else if (hourData.AlertSeconds >= hourData.IdleSeconds && hourData.AlertSeconds >= hourData.ProductionSeconds) hourData.Status = 0;
                            else if (hourData.IdleSeconds > hourData.AlertSeconds && hourData.IdleSeconds >= hourData.ProductionSeconds) hourData.Status = 1;
                            else hourData.Status = 2;
                        }
                    }
                }
            }
        }



        private static double GetTime(string columnName, DataTable dt)
        {
            double result = 0;

            if (dt != null)
            {
                foreach (DataRow row in dt.Rows)
                {
                    double d = DataTable_Functions.GetDoubleFromRow(columnName, row);
                    if (d >= 0) result += d;
                }
            }

            return result;
        }

        private static double GetTime(string columnName, DataRow row)
        {
            double result = 0;

            double d = DataTable_Functions.GetDoubleFromRow(columnName, row);
            if (d >= 0) result = d;

            return result;
        }

        private string GetTableValue(object obj, string key)
        {
            var dt = obj as DataTable;
            if (dt != null)
            {
                return DataTable_Functions.GetTableValue(dt, "name", key, "value");
            }
            return null;
        }


    }
}
