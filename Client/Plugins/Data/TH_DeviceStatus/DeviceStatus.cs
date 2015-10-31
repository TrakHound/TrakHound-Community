// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Data;

using TH_Configuration;
using TH_Database;
using TH_Global;
using TH_PlugIns_Client_Data;

namespace TH_DeviceStatus
{
    public class DeviceStatus : Data_PlugIn
    {

        #region "Plugin"

        public string Name { get { return "Device Status"; } }

        //public int Priority { get { return 1; } }

        public void Initialize(Configuration config) 
        {

            DatabasePluginReader dpr = new DatabasePluginReader();

            // Initialize Database Configurations
            Global.Initialize(config.Databases);

            configuration = config;

        }

        public void Closing() { }

        public void Run()
        {
            bool connected = false;

            if (configuration.Databases.Databases.Count > 0)
            {
                connected = Global.Ping(configuration.Databases.Databases[0]);
            }

            SendConnectionData(connected);

            if (connected)
            {
                GetSnapShots();

                GetShifts();

                GetProductionStatusList();

                GetOEE();
            }
        }

        public void Update_DataEvent(DataEvent_Data de_d) { }

        public event DataEvent_Handler DataEvent;

        #endregion

        #region "Device Status"

        Configuration configuration;

        void SendConnectionData(bool connected)
        {
            TH_PlugIns_Client_Data.DataEvent_Data de_d = new DataEvent_Data();
            de_d.id = "DeviceStatus_Connection";
            de_d.data = connected;

            if (DataEvent != null) DataEvent(de_d);
        }

        void GetSnapShots()
        {
            DataTable snapshots_DT = Table.Get(configuration.Databases, TableNames.SnapShots);
            if (snapshots_DT != null)
            {
                Dictionary<string, Tuple<DateTime, string, string>> data = new Dictionary<string, Tuple<DateTime, string, string>>();

                foreach (DataRow row in snapshots_DT.Rows)
                {
                    string key = row["name"].ToString();

                    DateTime timestamp = DateTime.MinValue;
                    DateTime.TryParse(row["timestamp"].ToString(), out timestamp);

                    string value = row["value"].ToString();
                    string prevvalue = row["previous_value"].ToString();

                    data.Add(key, new Tuple<DateTime, string, string>(timestamp, value, prevvalue));
                }

                // Set shiftDate and shiftName for other functions in Device Status
                Tuple<DateTime, string, string> val = null;
                data.TryGetValue("Current Shift Name", out val);
                if (val != null) shiftName = val.Item2;
                else shiftName = null;

                val = null;
                data.TryGetValue("Current Shift Date", out val);
                if (val != null) shiftDate = val.Item2;
                else shiftDate = null;

                val = null;
                data.TryGetValue("Current Shift Id", out val);
                if (val != null) shiftId = val.Item2;
                else shiftId = null;

                // Local
                val = null;
                data.TryGetValue("Current Shift Begin", out val);
                if (val != null) shiftStart = val.Item2;
                else shiftStart = null;

                val = null;
                data.TryGetValue("Current Shift End", out val);
                if (val != null) shiftEnd = val.Item2;
                else shiftEnd = null;

                // UTC
                val = null;
                data.TryGetValue("Current Shift Begin UTC", out val);
                if (val != null) shiftStartUTC = val.Item2;
                else shiftStartUTC = null;

                val = null;
                data.TryGetValue("Current Shift End UTC", out val);
                if (val != null) shiftEndUTC = val.Item2;
                else shiftEndUTC = null;


                TH_PlugIns_Client_Data.DataEvent_Data de_d = new DataEvent_Data();
                de_d.id = "DeviceStatus_Snapshots";
                de_d.data = data;

                if (DataEvent != null) DataEvent(de_d);
            }
        }

        string shiftDate = null;
        string shiftName = null;
        string shiftId = null;
        string shiftStart = null;
        string shiftEnd = null;
        string shiftStartUTC = null;
        string shiftEndUTC = null;

        void GetShifts()
        {
            if (shiftDate != null && shiftName != null)
            {
                DataTable shifts_DT = Table.Get(configuration.Databases, TableNames.Shifts, "WHERE Date='" + shiftDate + "' AND Shift='" + shiftName + "'");
                if (shifts_DT != null)
                {
                    List<Dictionary<string, string>> data = new List<Dictionary<string, string>>();

                    foreach (DataRow row in shifts_DT.Rows)
                    {
                        Dictionary<string, string> rowdata = new Dictionary<string, string>();

                        foreach (DataColumn column in row.Table.Columns)
                        {
                            string key = column.ColumnName;
                            string value = row[column].ToString();

                            rowdata.Add(key, value);
                        }

                        data.Add(rowdata);
                    }

                    TH_PlugIns_Client_Data.DataEvent_Data de_d = new DataEvent_Data();
                    de_d.id = "DeviceStatus_Shifts";
                    de_d.data = data;

                    if (DataEvent != null) DataEvent(de_d);
                }
            } 
        }

        void GetProductionStatusList()
        {
            //DateTime start = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
            //DateTime end = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);

            DateTime start = DateTime.MinValue;
            DateTime.TryParse(shiftStartUTC, out start);

            DateTime end = DateTime.MinValue;
            DateTime.TryParse(shiftEndUTC, out end);

            if (end < start) end = end.AddDays(1);

            if (start > DateTime.MinValue && end > DateTime.MinValue)
            {
                string filter = "WHERE TIMESTAMP BETWEEN '" + start.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + end.ToString("yyyy-MM-dd HH:mm:ss") + "'";

                string tableName = TableNames.Gen_Events_TablePrefix + "production_status";


                DataTable table = Table.Get(configuration.Databases, tableName, filter);
                if (table != null)
                {
                    List<Dictionary<string, string>> data = new List<Dictionary<string, string>>();

                    foreach (DataRow row in table.Rows)
                    {
                        Dictionary<string, string> rowdata = new Dictionary<string, string>();

                        foreach (DataColumn column in row.Table.Columns)
                        {
                            string key = column.ColumnName;
                            string value = row[column].ToString();

                            rowdata.Add(key, value);
                        }

                        data.Add(rowdata);
                    }

                    TH_PlugIns_Client_Data.DataEvent_Data de_d = new DataEvent_Data();
                    de_d.id = "DeviceStatus_ProductionStatus";
                    de_d.data = data;

                    if (DataEvent != null) DataEvent(de_d);
                }
            }
        }

        void GetOEE()
        {
            if (shiftId != null)
            {
                if (shiftId.Contains("_"))
                {
                    string shiftQuery = shiftId.Substring(0, shiftId.LastIndexOf('_'));

                    DataTable dt = Table.Get(configuration.Databases, TableNames.OEE, "WHERE Shift_Id LIKE '" + shiftQuery + "%'");
                    if (dt != null)
                    {
                        List<Dictionary<string, string>> data = new List<Dictionary<string, string>>();

                        foreach (DataRow row in dt.Rows)
                        {
                            Dictionary<string, string> rowdata = new Dictionary<string, string>();

                            foreach (DataColumn column in row.Table.Columns)
                            {
                                string key = column.ColumnName;
                                string value = row[column].ToString();

                                rowdata.Add(key, value);
                            }

                            string shiftname = "";
                            if (row.Table.Columns.Contains("shift_id")) shiftname = row["shift_id"].ToString();

                            data.Add(rowdata);
                        }

                        TH_PlugIns_Client_Data.DataEvent_Data de_d = new DataEvent_Data();
                        de_d.id = "DeviceStatus_OEE";
                        de_d.data = data;

                        if (DataEvent != null) DataEvent(de_d);
                    }
                }
            }  
        }

        #endregion

    }

}
