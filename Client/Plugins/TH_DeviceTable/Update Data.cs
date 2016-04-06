// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;

using TH_Configuration;
using TH_Global;
using TH_Global.Functions;
using TH_Plugins;

namespace TH_DeviceTable
{
    public partial class DeviceTable
    {

        const System.Windows.Threading.DispatcherPriority PRIORITY_BACKGROUND = System.Windows.Threading.DispatcherPriority.Background;
        const System.Windows.Threading.DispatcherPriority PRIORITY_CONTEXT_IDLE = System.Windows.Threading.DispatcherPriority.ContextIdle;

        void Update(EventData data)
        {
            if (data != null)
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

                        UpdateOEE(data, info);

                        UpdateProductionStatus(data, info);
                    }
                }
            }
        }

        private void Refresh()
        {
            //if (Devices_DG.Items.NeedsRefresh)
            //{
            //    Devices_DG.Items.Refresh();
            //}
        }

        private void UpdateDatabaseConnection(EventData data, DeviceInfo info)
        {
            if (data.Id.ToLower() == "statusdata_connection")
            {
                if (data.Data02.GetType() == typeof(bool))
                {
                    info.Connected = (bool)data.Data02;
                }

                //bool connected;
                //bool.TryParse(data.Data02.ToString(), out connected);

                //info.Connected = connected;

                Refresh();
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

                //bool connected;
                //bool.TryParse(data.Data02.ToString(), out connected);

                //info.Connected = connected;

                Refresh();
            }
        }

        private void UpdateOEE(EventData data, DeviceInfo info)
        {
            if (data.Id.ToLower() == "statusdata_oee")
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    var dt = data.Data02 as DataTable;
                    if (dt != null)
                    {
                        var oeeData = OEEData.FromDataTable(dt);

                        info.Oee = oeeData.Oee;
                        info.Availability = oeeData.Availability;
                        info.Performance = oeeData.Performance;

                        Refresh();
                    }
                }), PRIORITY_BACKGROUND, new object[] { });
            }

        }

        private void UpdateProductionStatus(EventData data, DeviceInfo info)
        {
            if (data.Id.ToLower() == "statusdata_snapshots")
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    info.ProductionStatus = GetTableValue(data.Data02, "Production Status");
                    Refresh();
                }), PRIORITY_BACKGROUND, new object[] { });
            }

            if (data.Id.ToLower() == "statusdata_shiftdata")
            {
                //UpdateProductionStatusValue(data.Data02, info);
                Dispatcher.BeginInvoke(new Action<object, DeviceInfo>(UpdateProductionStatusValue), PRIORITY_BACKGROUND, new object[] { data.Data02, info });
            }
        }

        void UpdateProductionStatusValue(object shiftData, DeviceInfo info)
        {
            DataTable dt = shiftData as DataTable;
            if (dt != null)
            {
                // Get Total Time for this shift (all segments)
                double totalSeconds = GetTime("TOTALTIME", dt);

                // Get List of variables from 'Shifts' table and collect the total number of seconds
                foreach (DataColumn column in dt.Columns)
                {
                    if (column.ColumnName.Contains("PRODUCTION_STATUS") || column.ColumnName.Contains("Production_Status") || column.ColumnName.Contains("production_status"))
                    {
                        // Get Value name from Column Name
                        string valueName = null;
                        if (column.ColumnName.Contains("__"))
                        {
                            int i = column.ColumnName.IndexOf("__") + 2;
                            if (i < column.ColumnName.Length)
                            {
                                string name = column.ColumnName.Substring(i);
                                name = name.Replace('_', ' ');

                                valueName = name;
                            }
                        }

                        if (valueName != null && info.ProductionStatus != null && valueName.ToLower() == info.ProductionStatus.ToLower())
                        {
                            // Get Seconds for Value
                            double seconds = GetTime(column.ColumnName, dt);

                            // Update TimeDisplay
                            info.ProductionStatusTotal = totalSeconds;
                            info.ProductionStatusSeconds = seconds;

                            Refresh();

                            break;
                        }
                    }
                }
            }
        }

        static double GetTime(string columnName, DataTable dt)
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
