// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;

using TH_Configuration;
using TH_Database;
using TH_Global;
using TH_Global.Functions;
using TH_Plugins;

namespace TH_StatusData
{
    public partial class StatusData
    {
        
        private class DatabaseGroup
        {
            public DatabaseGroup()
            {
                Configurations = new List<Configuration>();
            }

            public Database_Configuration Database { get; set; }
            public List<Configuration> Configurations { get; set; }
        }

        private static List<DatabaseGroup> GetUniqueDatabaseGroups(List<Configuration> configs)
        {
            var result = new List<DatabaseGroup>();

            foreach (var config in configs)
            {
                foreach (var database in config.Databases_Client.Databases)
                {
                    var group = result.Find(x => x.Database.UniqueId == database.UniqueId);
                    if (group == null)
                    {
                        group = new DatabaseGroup();
                        group.Database = database;
                        result.Add(group);
                    }

                    group.Configurations.Add(config);
                }
            }

            return result;
        }

        private class UpdateInfo
        {
            public DatabaseGroup Group { get; set; }
            public Thread Thread { get; set; }
            public ManualResetEvent Stop { get; set; }
        }

        private int interval_min = 5000;
        private const int INTERVAL_MAX = 60000;

        List<UpdateInfo> UpdateInfos = new List<UpdateInfo>();

        void Start()
        {
            Stop();

            // Get Unique Database Connection Groups
            var groups = GetUniqueDatabaseGroups(Devices.ToList());

            // Create an UpdateInfo object for each DatabaseGroup and start it's update thread
            foreach (var group in groups)
            {
                var thread = new Thread(new ParameterizedThreadStart(Update));
                var stop = new ManualResetEvent(false);

                var info = new UpdateInfo();
                info.Group = group;
                info.Thread = thread;
                info.Stop = stop;
                UpdateInfos.Add(info);

                thread.Start(info);
            }
        }

        void Stop()
        {
            foreach (var info in UpdateInfos)
            {
                if (info.Stop != null) info.Stop.Set();
            }
        }

        void Abort()
        {
            foreach (var info in UpdateInfos)
            {
                if (info.Thread != null) info.Thread.Abort();
            }
        }


        void Update(object o)
        {
            if (o != null)
            {
                var info = (UpdateInfo)o;

                if (info.Stop != null)
                {
                    int interval = Properties.Settings.Default.DatabaseReadInterval;
                    bool first = true;

                    while (!info.Stop.WaitOne(0, true))
                    {
                        // Get Database Connction Status
                        bool connected = CheckConnectionStatus(info.Group.Database);

                        // Send Connection status to each Device using this Database Connection
                        foreach (var config in info.Group.Configurations)
                        {
                            SendConnectionData(config, connected);
                        }

                        if (connected)
                        {
                            // Reset the interval back to the Minimum
                            interval = Properties.Settings.Default.DatabaseReadInterval;

                            var tables = GetTables(info.Group);

                            // Update each device using the retrieved tables
                            foreach (var config in info.Group.Configurations)
                            {
                                List<DataTable> deviceTables = null;

                                // Get the tables that match the Configuration.DatabaseId
                                if (!String.IsNullOrEmpty(config.DatabaseId))
                                {
                                    deviceTables = tables.FindAll(x => x.TableName.StartsWith(config.DatabaseId));
                                }
                                else deviceTables = tables;

                                if (deviceTables != null)
                                {
                                    UpdateDeviceData(config, deviceTables);
                                }
                            }
                        }
                        else
                        {
                            // Increase the interval by 50% until interval == interval_max
                            if (!first) interval = Math.Min(Convert.ToInt32(interval + (interval * 0.5)), INTERVAL_MAX);
                            first = false;
                        }

                        Thread.Sleep(interval);
                    }
                }
            }
        }

        #region "Database Connection"

        private static bool CheckConnectionStatus(Database_Configuration config)
        {
            bool result = false;
            string msg = null;

            bool ping = TH_Database.Global.Ping(config, out msg);

            if (ping) { result = true; }
            else Logger.Log("CheckDatabaseConnection() :: Error :: " + config.Type + " :: " + config.UniqueId + " :: " + msg, Logger.LogLineType.Error);

            return result;
        }

        private void SendConnectionData(Configuration config, bool connected)
        {
            var data = new EventData();
            data.Id = "StatusData_Connection";
            data.Data01 = config;
            data.Data02 = connected;

            SendDataEvent(data);
        }

        #endregion

        #region "Get Tables"

        private static string[] dataTableNames = new string[]
        {
            TableNames.SnapShots,
            TableNames.Variables,
            TableNames.GenEventValues,
            TableNames.ShiftSegments,
            TableNames.Status,
        };

        private static string[] dataTableFilters = new string[]
        {
            null,
            null,
            null,
            null,
            null,
        };

        private static string[] dataTableColumns = new string[]
        {
            "NAME, VALUE",
            "VARIABLE, VALUE",
            null,
            null,
            null,
        };

        private static string GetTableName(string tablename, string id)
        {
            if (!String.IsNullOrEmpty(id)) return id + "_" + tablename;
            return tablename;
        }

        private static List<DataTable> GetTables(DatabaseGroup group)
        {
            var result = new List<DataTable>();

            var tableNames = new List<string>();

            var columns = new List<string>();
            var filters = new List<string>();

            //Get list of tablenames for all of the Configurations
            foreach (var config in group.Configurations)
            {
                foreach (var tableName in dataTableNames)
                {
                    tableNames.Add(GetTableName(tableName, config.DatabaseId));
                }

                columns.AddRange(dataTableColumns);
                filters.AddRange(dataTableFilters);
            }

            // Create a Database_Settings object using the DatabaseGroup.Database
            var db = new Database_Settings();
            db.Databases.Add(group.Database);

            // Get Tables
            var tables = Table.Get(db, tableNames.ToArray(), filters.ToArray(), columns.ToArray());
            if (tables != null) result = tables.ToList();

            return result;
        }

        #endregion

        #region "Get Device Data"

        private void SendDataEvent(EventData data)
        {
            if (data != null)
            {
                if (data.Id != null)
                {
                    if (SendData != null) SendData(data);
                }
            }
        }

        private static DataTable GetTableFromList(string tablename, List<DataTable> tables)
        {
            var table = tables.Find(x => x.TableName.ToLower() == tablename.ToLower());
            if (table != null) return table;
            return null;
        }


        private void UpdateDeviceData(Configuration config, List<DataTable> tables)
        {
            var list = tables.ToList();

            // Assign tables
            DataTable variables = GetTableFromList(GetTableName(TableNames.Variables, config.DatabaseId), list);
            DataTable snapshots = GetTableFromList(GetTableName(TableNames.SnapShots, config.DatabaseId), list);
            DataTable geneventvalues = GetTableFromList(GetTableName(TableNames.GenEventValues, config.DatabaseId), list);
            DataTable shifts = GetTableFromList(GetTableName(TableNames.Shifts, config.DatabaseId), list);
            DataTable shiftSegments = GetTableFromList(GetTableName(TableNames.ShiftSegments, config.DatabaseId), list);
            DataTable oee = GetTableFromList(GetTableName(TableNames.OEE, config.DatabaseId), list);
            DataTable status = GetTableFromList(GetTableName(TableNames.Status, config.DatabaseId), list);

            // Get Variable Data
            EventData variableData = GetVariables(variables, config);
            // Send Variable Data
            SendDataEvent(variableData);

            EventData availablityData = GetAvailability(variables, config);
            SendDataEvent(availablityData);

            //if ((bool)availablityData.Data02)
            //{
                // Get Snapshot Data
                EventData snapshotData = GetSnapShots(snapshots, config);
                // Send Snapshot Data
                SendDataEvent(snapshotData);

                //Get Shift Segment List
                EventData shiftSegmentData = GetShiftSegments(shiftSegments, config);
                // Send Shift Segment List
                SendDataEvent(shiftSegmentData);

                // Get ShiftData from Variable Data
                ShiftData shiftData = GetShiftData(variables);


                // Get Gen Event Values
                EventData genEventData = GetGenEventValues(geneventvalues, config);
                // Send Gen Event Values
                SendDataEvent(genEventData);

                //Get Shift Data
                EventData shiftTableData = GetShifts(shiftData, config);
                // Send Shift Data
                SendDataEvent(shiftTableData);

                // Get OEE Data
                EventData oeeData = GetOEE(config, shiftData);
                // Send OEE Data
                SendDataEvent(oeeData);

                // Get Parts Data
                EventData partsData = GetParts(config, shiftData);
                // Send Parts Data
                SendDataEvent(partsData);

                // Get Status Data
                EventData statusData = GetStatus(status, config);
                // Send Status Data
                SendDataEvent(statusData);
            //}
        }


        private static EventData GetVariables(DataTable dt, Configuration config)
        {
            var result = new EventData();

            if (dt != null)
            {
                var data = new EventData();
                data.Id = "StatusData_Variables";
                data.Data01 = config;
                data.Data02 = dt;

                result = data;
            }

            return result;
        }

        private static EventData GetShiftSegments(DataTable dt, Configuration config)
        {
            var result = new EventData();

            if (dt != null)
            {
                var data = new EventData();
                data.Id = "StatusData_ShiftSegments";
                data.Data01 = config;
                data.Data02 = dt;

                result = data;
            }

            return result;
        }

        private EventData GetAvailability(DataTable dt, Configuration config)
        {
            bool available = false;

            var val = DataTable_Functions.GetTableValue(dt, "variable", "device_available", "value");
            // If found then parse string for boolean
            if (val != null) bool.TryParse(val, out available);
            // If not found in table, assume that it is available (for compatibility purposes)
            else available = true;

            var result = new EventData();
            result.Id = "StatusData_Availability";
            result.Data01 = config;
            result.Data02 = available;

            return result;
        }

        private static EventData GetSnapShots(DataTable dt, Configuration config)
        {
            var result = new EventData();

            if (dt != null)
            {
                var data = new EventData();
                data.Id = "StatusData_Snapshots";
                data.Data01 = config;
                data.Data02 = dt;

                result = data;
            }

            return result;
        }

        private static EventData GetGenEventValues(DataTable dt, Configuration config)
        {
            var result = new EventData();

            if (dt != null)
            {
                var data = new EventData();
                data.Id = "StatusData_GenEventValues";
                data.Data01 = config;
                data.Data02 = dt;

                result = data;
            }

            return result;
        }

        private static EventData GetStatus(DataTable dt, Configuration config)
        {
            var result = new EventData();

            if (dt != null)
            {
                var data = new EventData();
                data.Id = "StatusData_Status";
                data.Data01 = config;
                data.Data02 = dt;

                result = data;
            }

            return result;
        }

        class ShiftData
        {
            public string shiftDate = null;
            public string shiftName = null;
            public string shiftId = null;
            public string shiftStart = null;
            public string shiftEnd = null;
            public string shiftStartUTC = null;
            public string shiftEndUTC = null;
        }

        private static ShiftData GetShiftData(DataTable dt)
        {
            var result = new ShiftData();

            if (dt != null)
            {
                result.shiftName = DataTable_Functions.GetTableValue(dt, "variable", "shift_name", "value");
                result.shiftDate = DataTable_Functions.GetTableValue(dt, "variable", "shift_date", "value");
                result.shiftId = DataTable_Functions.GetTableValue(dt, "variable", "shift_id", "value");
                result.shiftStart = DataTable_Functions.GetTableValue(dt, "variable", "shift_begintime", "value");
                result.shiftEnd = DataTable_Functions.GetTableValue(dt, "variable", "shift_endtime", "value");
                result.shiftStartUTC = DataTable_Functions.GetTableValue(dt, "variable", "shift_begintime_utc", "value");
                result.shiftEndUTC = DataTable_Functions.GetTableValue(dt, "variable", "shift_endtime_utc", "value");
            }

            return result;
        }

        private static EventData GetShifts(ShiftData shiftData, Configuration config)
        {
            var result = new EventData();

            if (shiftData.shiftDate != null && shiftData.shiftName != null)
            {
                //DataTable shifts_DT = Table.Get(config.Databases_Client, GetTableName(TableNames.Shifts, config.DatabaseId), "WHERE Date='" + shiftData.shiftDate + "' AND Shift='" + shiftData.shiftName + "'");
                DataTable shifts_DT = Table.Get(config.Databases_Client, GetTableName(TableNames.Shifts, config.DatabaseId), "WHERE Date='" + shiftData.shiftDate + "'");
                if (shifts_DT != null)
                {
                    var data = new EventData();
                    data.Id = "StatusData_ShiftData";
                    data.Data01 = config;
                    data.Data02 = shifts_DT;

                    result = data;
                }
            }

            return result;
        }


        #endregion
        
        static EventData GetProductionStatusList(Configuration config, ShiftData shiftData)
        {
            var result = new EventData();

            DateTime start = DateTime.MinValue;
            DateTime.TryParse(shiftData.shiftStartUTC, out start);

            DateTime end = DateTime.MinValue;
            DateTime.TryParse(shiftData.shiftEndUTC, out end);

            if (end < start) end = end.AddDays(1);

            if (start > DateTime.MinValue && end > DateTime.MinValue)
            {
                string filter = "WHERE TIMESTAMP BETWEEN '" + start.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + end.ToString("yyyy-MM-dd HH:mm:ss") + "'";

                string tableName = TableNames.Gen_Events_TablePrefix + "production_status";


                DataTable dt = Table.Get(config.Databases_Client, tableName, filter);
                if (dt != null)
                {
                    var data = new EventData();
                    data.Id = "StatusData_ProductionStatus";
                    data.Data01 = config;
                    data.Data02 = dt;

                    result = data;
                }
            }

            return result;
        }

        static EventData GetOEE(Configuration config, ShiftData shiftData)
        {
            var result = new EventData();

            if (shiftData.shiftId != null)
            {
                if (shiftData.shiftId.Contains("_"))
                {
                    //string shiftQuery = shiftData.shiftId.Substring(0, shiftData.shiftId.LastIndexOf('_')); // Get Current Shift
                    string shiftQuery = shiftData.shiftId.Substring(0, shiftData.shiftId.IndexOf('_')); // Get Whole day

                    DataTable dt = Table.Get(config.Databases_Client, GetTableName(TableNames.OEE, config.DatabaseId), "WHERE Shift_Id LIKE '" + shiftQuery + "%'");
                    if (dt != null)
                    {
                        var data = new EventData();
                        data.Id = "StatusData_OEE";
                        data.Data01 = config;
                        data.Data02 = dt;

                        result = data;
                    }
                }
            }

            return result;
        }

        static EventData GetParts(Configuration config, ShiftData shiftData)
        {
            var result = new EventData();

            if (shiftData.shiftId != null)
            {
                if (shiftData.shiftId.Contains("_"))
                {
                    string shiftQuery = shiftData.shiftId.Substring(0, shiftData.shiftId.LastIndexOf('_'));

                    DataTable dt = Table.Get(config.Databases_Client, GetTableName(TableNames.Parts, config.DatabaseId), "WHERE SHIFT_ID LIKE '" + shiftQuery + "%'");
                    if (dt != null)
                    {
                        var data = new EventData();
                        data.Id = "StatusData_Parts";
                        data.Data01 = config;
                        data.Data02 = dt;

                        result = data;
                    }
                }
            }

            return result;
        }

    }

}
