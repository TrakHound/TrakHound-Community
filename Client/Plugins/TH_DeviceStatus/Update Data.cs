using System;
using System.Collections.ObjectModel;
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

                        //UpdateOEE(data, info);

                        //UpdateProductionStatus(data, info);

                        //UpdatePartCount(data, info);
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
            if (data.Id.ToLower() == "statusdata_shiftdata")
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    UpdateProductionStatusTime(data.Data02, info);
                }), PRIORITY_BACKGROUND, new object[] { });
            }

            //if (data.Id.ToLower() == "statusdata_snapshots")
            //{
            //    Dispatcher.BeginInvoke(new Action(() =>
            //    {
            //        bool b;
            //        string s;

            //        // Alert
            //        b = false;
            //        s = GetTableValue(data.Data02, "Alert");
            //        bool.TryParse(s, out b);
            //        info.Alert = b;

            //        // Idle
            //        b = false;
            //        s = GetTableValue(data.Data02, "Idle");
            //        bool.TryParse(s, out b);
            //        info.Idle = b;

            //        // Production
            //        b = false;
            //        s = GetTableValue(data.Data02, "Production");
            //        bool.TryParse(s, out b);
            //        info.Production = b;

            //    }), PRIORITY_BACKGROUND, new object[] { });
            //}
        }

        private void UpdateProductionStatusTime(object shiftData, DeviceInfo info)
        {
            DataTable dt = shiftData as DataTable;
            if (dt != null)
            {
                // Get Total Time for this shift (all segments)
                //double totalSeconds = GetTime("TOTALTIME", dt);

                var status = new int[24];

                foreach (DataRow row in dt.Rows)
                {

                    HourInfo hour = GetHourInfo(row);

                    double alertSeconds = 0;
                    double idleSeconds = 0;
                    double productionSeconds = 0;

                    // Get List of variables from 'Shifts' table and collect the total number of seconds
                    foreach (DataColumn column in dt.Columns)
                    {
                        switch (column.ColumnName.ToLower())
                        {
                            case "alert__true":

                                alertSeconds = GetTime(column.ColumnName, dt);
                                break;

                            case "idle__true":

                                idleSeconds = GetTime(column.ColumnName, dt);
                                break;

                            case "production__true":

                                productionSeconds = GetTime(column.ColumnName, dt);
                                break;
                        }
                    }

                    if (alertSeconds >= idleSeconds && alertSeconds >= productionSeconds) status[hour.Start] = 0;
                    else if (idleSeconds > alertSeconds && idleSeconds >= productionSeconds) status[hour.Start] = 1;
                    else status[hour.Start] = 2;



                }

                //for (var x = 0; x <= 23; x++)
                //{
                //    //int hour = x;

                //    double alertSeconds = 0;
                //    double idleSeconds = 0;
                //    double productionSeconds = 0;

                //    // Get List of variables from 'Shifts' table and collect the total number of seconds
                //    foreach (DataColumn column in dt.Columns)
                //    {
                //        switch (column.ColumnName.ToLower())
                //        {
                //            case "alert__true":

                //                alertSeconds = GetTime(column.ColumnName, dt);
                //                break;

                //            case "idle__true":

                //                idleSeconds = GetTime(column.ColumnName, dt);
                //                break;

                //            case "production__true":

                //                productionSeconds = GetTime(column.ColumnName, dt);
                //                break;
                //        }
                //    }

                //    if (alertSeconds >= idleSeconds && alertSeconds >= productionSeconds) status[x] = 0;
                //    else if (idleSeconds > alertSeconds && idleSeconds >= productionSeconds) status[x] = 1;
                //    else status[x] = 2;
                //}

                info.StatusLevel = status;
            }
        }


        class HourInfo
        {
            public int Start { get; set; }
            public int End { get; set; }
        }

        private static HourInfo GetHourInfo(DataRow row)
        {
            string start = DataTable_Functions.GetRowValue("START", row);
            string end = DataTable_Functions.GetRowValue("END", row);

            DateTime s = DateTime.MinValue;
            DateTime e = DateTime.MinValue;

            DateTime.TryParse(start, out s);
            DateTime.TryParse(end, out e);

            var info = new HourInfo();
            info.Start = s.Hour;
            info.End = e.Hour;

            return info;
        }


        //private void UpdateOEE(EventData data, DeviceInfo info)
        //{
        //    if (data.Id.ToLower() == "statusdata_oee")
        //    {
        //        Dispatcher.BeginInvoke(new Action(() =>
        //        {
        //            var dt = data.Data02 as DataTable;
        //            if (dt != null)
        //            {
        //                var oeeData = OEEData.FromDataTable(dt);

        //                info.Oee = oeeData.Oee;
        //                info.Availability = oeeData.Availability;
        //                info.Performance = oeeData.Performance;
        //            }
        //        }), PRIORITY_BACKGROUND, new object[] { });
        //    }
        //}


        //private void UpdateProductionStatus(EventData data, DeviceInfo info)
        //{
        //    if (data.Id.ToLower() == "statusdata_snapshots")
        //    {
        //        Dispatcher.BeginInvoke(new Action(() =>
        //        {
        //            info.ProductionStatus = GetTableValue(data.Data02, "Production Status");

        //            var i = info.ProductionStatusInfos.Find(x => x.Value == info.ProductionStatus);
        //            if (i != null)
        //            {
        //                info.ProductionStatusBrush = i.Brush;
        //                info.ProductionStatusBrushHover = i.HoverBrush;
        //            }
        //        }), PRIORITY_BACKGROUND, new object[] { });
        //    }

        //    if (data.Id.ToLower() == "statusdata_shiftdata")
        //    {
        //        Dispatcher.BeginInvoke(new Action<object, DeviceInfo>(UpdateProductionStatusTime), PRIORITY_BACKGROUND, new object[] { data.Data02, info });
        //    }

        //    if (data.Id.ToLower() == "statusdata_geneventvalues")
        //    {
        //        Dispatcher.BeginInvoke(new Action<object, DeviceInfo>(UpdateProductionStatusTimes_GenEventValues), PRIORITY_BACKGROUND, new object[] { data.Data02, info });
        //    }
        //}

        //// Get list of Production Status variables to get numval for each to set colors
        //void UpdateProductionStatusTimes_GenEventValues(object geneventvalues, DeviceInfo info)
        //{
        //    DataTable dt = geneventvalues as DataTable;
        //    if (dt != null)
        //    {
        //        DataView dv = dt.AsDataView();
        //        dv.RowFilter = "EVENT = 'production_status'";
        //        DataTable temp_dt = dv.ToTable();

        //        if (temp_dt != null)
        //        {
        //            foreach (DataRow row in temp_dt.Rows)
        //            {
        //                string val = row["VALUE"].ToString();

        //                // Get the Numval 
        //                int numval = -1;
        //                int.TryParse(row["NUMVAL"].ToString(), out numval);

        //                if (!info.ProductionStatusInfos.ToList().Exists(x => x.Value == val))
        //                {
        //                    var i = new DeviceInfo.ProductionStatusInfo();
        //                    i.Value = val;
        //                    i.Numval = numval;
        //                    info.ProductionStatusInfos.Add(i);

        //                    info.ProductionStatusInfos = info.ProductionStatusInfos.OrderByDescending(x => x.Numval).ToList();
        //                }
        //            }

        //            // Set Bar Colors
        //            foreach (var i in info.ProductionStatusInfos)
        //            {
        //                if (i.Numval == info.ProductionStatusInfos.Count - 1)
        //                {
        //                    i.Brush = Brush_Functions.GetSolidBrushFromResource(this, "StatusGreen");
        //                    i.HoverBrush = Brush_Functions.GetSolidBrushFromResource(this, "StatusGreen_Hover");
        //                }
        //                else if (i.Numval == 0)
        //                {
        //                    i.Brush = Brush_Functions.GetSolidBrushFromResource(this, "StatusRed");
        //                    i.HoverBrush = Brush_Functions.GetSolidBrushFromResource(this, "StatusRed_Hover");
        //                }
        //                else
        //                {
        //                    i.Brush = Brush_Functions.GetSolidBrushFromResource(this, "StatusBlue");
        //                    i.HoverBrush = Brush_Functions.GetSolidBrushFromResource(this, "StatusBlue_Hover");
        //                }
        //            }

        //            temp_dt.Dispose();
        //        }
        //    }
        //}

        //private void UpdateProductionStatusTime(object shiftData, DeviceInfo info)
        //{
        //    DataTable dt = shiftData as DataTable;
        //    if (dt != null)
        //    {
        //        // Get Total Time for this shift (all segments)
        //        double totalSeconds = GetTime("TOTALTIME", dt);

        //        // Get List of variables from 'Shifts' table and collect the total number of seconds
        //        foreach (DataColumn column in dt.Columns)
        //        {
        //            if (column.ColumnName.Contains("PRODUCTION_STATUS") || column.ColumnName.Contains("Production_Status") || column.ColumnName.Contains("production_status"))
        //            {
        //                // Get Value name from Column Name
        //                string valueName = null;
        //                if (column.ColumnName.Contains("__"))
        //                {
        //                    int i = column.ColumnName.IndexOf("__") + 2;
        //                    if (i < column.ColumnName.Length)
        //                    {
        //                        string name = column.ColumnName.Substring(i);
        //                        name = name.Replace('_', ' ');

        //                        valueName = name;
        //                    }
        //                }

        //                if (valueName != null && info.ProductionStatus != null && valueName.ToLower() == info.ProductionStatus.ToLower())
        //                {
        //                    // Get Seconds for Value
        //                    double seconds = GetTime(column.ColumnName, dt);

        //                    // Update TimeDisplay
        //                    info.ProductionStatusTotal = totalSeconds;
        //                    info.ProductionStatusSeconds = seconds;

        //                    break;
        //                }
        //            }
        //        }
        //    }
        //}


        //private bool useSnapshotForParts = false;

        //private void UpdatePartCount(EventData data, DeviceInfo info)
        //{
        //    // Use Snapshot table if Part Count is given as a total for the day
        //    if (data.Id.ToLower() == "statusdata_snapshots")
        //    {
        //        Dispatcher.BeginInvoke(new Action(() =>
        //        {
        //            int count = 0;

        //            string val = GetTableValue(data.Data02, "Part Count");
        //            if (val != null)
        //            {
        //                useSnapshotForParts = true;

        //                int.TryParse(val, out count);

        //                info.PartCount = count;
        //            }
        //        }), PRIORITY_BACKGROUND, new object[] { });
        //    }

        //    // Use the Parts table is Part Count is given as DISCRETE (number of parts per event) and not the total for the day
        //    if (data.Id.ToLower() == "statusdata_parts" && data.Data02 != null && !useSnapshotForParts)
        //    {
        //        Dispatcher.BeginInvoke(new Action(() =>
        //        {
        //            var dt = data.Data02 as DataTable;
        //            if (dt != null && dt.Columns.Contains("count"))
        //            {
        //                int count = 0;

        //                foreach (DataRow row in dt.Rows)
        //                {
        //                    string val = row["count"].ToString();

        //                    int i = 0;
        //                    if (int.TryParse(val, out i)) count += i;
        //                }

        //                info.PartCount = count;
        //            }
        //        }), PRIORITY_BACKGROUND, new object[] { });
        //    }
        //}


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
