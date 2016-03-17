using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Data;
using System.Collections.ObjectModel;

using TH_Configuration;
using TH_Database;
using TH_Global;
using TH_Global.Functions;
using TH_Plugins_Client;

namespace TH_StatusData
{
    public partial class StatusData
    {

        //int interval = 5000;

        //List<ManualResetEvent> stops = new List<ManualResetEvent>();

        //void Update_Start()
        //{
        //    Update_Stop();

        //    Console.WriteLine("StatusData :: All Previous Threads Stopped");

        //    if (Devices != null)
        //    {
        //        foreach (Configuration device in Devices.ToList())
        //        {
        //            Thread thread = new Thread(new ParameterizedThreadStart(Update_Worker));
        //            var stop = new ManualResetEvent(false);
        //            stops.Add(stop);

        //            var info = new UpdateInfo();
        //            info.Config = device;
        //            info.Stop = stop;

        //            thread.Start(info);
        //        }
        //    }
        //}

        //ManualResetEvent stop;

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

        

        private const int INTERVAL_MIN = 5000;
        private const int INTERVAL_MAX = 60000;

        List<UpdateInfo> UpdateInfos = new List<UpdateInfo>();

        void Start()
        {
            Stop();

            //Console.WriteLine("StatusData :: All Previous Threads Stopped");

            // Get Unique Database Connection Groups
            var groups = GetUniqueDatabaseGroups(Devices.ToList());
            //foreach (var group in groups)
            //{
            //    Console.WriteLine(group.Database.UniqueId);
            //    Console.WriteLine("----------------");
            //    foreach (var config in group.Configurations)
            //    {
            //        Console.WriteLine(config.Description.Description);
            //    }
            //}

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
                    int interval = INTERVAL_MIN;
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

                        // $$ DEBUG $$
                        connected = true;

                        if (connected)
                        {
                            // Reset the interval back to the Minimum
                            interval = INTERVAL_MIN;

                            var tables = GetTables(info.Group);

                            // Update each device using the retrieved tables
                            foreach (var config in info.Group.Configurations)
                            {
                                // Get the tables that match the Configuration.DatabaseId
                                var deviceTables = tables.FindAll(x => x.TableName.StartsWith(config.DatabaseId));
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
            else Console.WriteLine("CheckDatabaseConnection() :: Error :: " + config.Type + " :: " + config.UniqueId + " :: " + msg);

            return result;
        }

        private void SendConnectionData(Configuration config, bool connected)
        {
            var data = new DataEvent_Data();
            data.id = "StatusData_Connection";
            data.data01 = config;
            data.data02 = connected;

            SendDataEvent(data);
        }

        //private DataEvent_Data GetConnectionData(Database_Configuration config)
        //{
        //    bool connected = CheckDatabaseConnections(config);

        //    DataEvent_Data result = new DataEvent_Data();
        //    result.id = "StatusData_Connection";
        //    result.data01 = config;
        //    result.data02 = connected;

        //    return result;
        //}

        #endregion

        #region "Get Tables"

        private static string[] dataTableNames = new string[]
        {
            TableNames.SnapShots,
            TableNames.Variables,
            TableNames.GenEventValues,
            TableNames.ShiftSegments,
        };

        private static string[] dataTableFilters = new string[]
        {
            null,
            null,
            null,
            null,
        };

        private static string[] dataTableColumns = new string[]
        {
            "`NAME`, `VALUE`",
            "`VARIABLE`, `VALUE`",
            null,
            null,
        };

        private static List<DataTable> GetTables(DatabaseGroup group)
        {
            var result = new List<DataTable>();

            var stpw = new System.Diagnostics.Stopwatch();
            stpw.Start();

            var tableNames = new List<string>();

            //Get list of tablenames for all of the Configurations
            foreach (var config in group.Configurations)
            {
                foreach (var tableName in dataTableNames)
                {
                    tableNames.Add(config.DatabaseId + "_" + tableName);
                }  
            }

            // Create a Database_Settings object using the DatabaseGroup.Database
            var db = new Database_Settings();
            db.Databases.Add(group.Database);

            // Get Tables
            var tables = Table.Get(db, tableNames.ToArray(), dataTableFilters, dataTableColumns);
            if (tables != null) result = tables.ToList();

            stpw.Stop();
            Console.WriteLine("GetTables() :: " + group.Database.UniqueId + " :: " + stpw.ElapsedMilliseconds.ToString() + "ms");

            return result;
        }

        #endregion

        #region "Get Device Data"

        private void SendDataEvent(DataEvent_Data de_d)
        {
            if (de_d != null)
            {
                if (de_d.id != null)
                {
                    if (DataEvent != null) DataEvent(de_d);
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
            DataTable variables = GetTableFromList(config.DatabaseId + "_" + TableNames.Variables, list);
            DataTable snapshots = GetTableFromList(config.DatabaseId + "_" + TableNames.SnapShots, list);
            DataTable geneventvalues = GetTableFromList(config.DatabaseId + "_" + TableNames.GenEventValues, list);
            DataTable shifts = GetTableFromList(config.DatabaseId + "_" + TableNames.Shifts, list);
            DataTable oee = GetTableFromList(config.DatabaseId + "_" + TableNames.OEE, list);

            // Get Variable Data
            DataEvent_Data variableData = GetVariables(variables, config);
            // Send Variable Data
            SendDataEvent(variableData);

            DataEvent_Data availablityData = GetAvailability(variables, config);
            SendDataEvent(availablityData);

            if ((bool)availablityData.data02)
            {
                // Get Snapshot Data
                DataEvent_Data snapshotData = GetSnapShots(snapshots, config);
                // Send Snapshot Data
                SendDataEvent(snapshotData);

                // Get ShiftData from Variable Data
                ShiftData shiftData = GetShiftData(variables);


                // Get Gen Event Values
                DataEvent_Data genEventData = GetGenEventValues(geneventvalues, config);
                // Send Gen Event Values
                SendDataEvent(genEventData);

                ////Get Shift Data
                //DataEvent_Data shiftTableData = GetShifts(shiftData, config);
                //// Send Shift Data
                //SendDataEvent(shiftTableData);

                //// Get OEE Data
                //DataEvent_Data oeeData = GetOEE(oee, shiftData);
                //// Send OEE Data
                //SendDataEvent(oeeData);
            }
        }


        private static DataEvent_Data GetVariables(DataTable dt, Configuration config)
        {
            var result = new DataEvent_Data();

            if (dt != null)
            {
                var de_d = new DataEvent_Data();
                de_d.id = "StatusData_Variables";
                de_d.data01 = config;
                de_d.data02 = dt;

                result = de_d;
            }

            return result;
        }

        private DataEvent_Data GetAvailability(DataTable dt, Configuration config)
        {
            bool available = false;

            var val = DataTable_Functions.GetTableValue(dt, "variable", "device_available", "value");
            // If found then parse string for boolean
            if (val != null) bool.TryParse(val, out available);
            // If not found in table, assume that it is available (for compatibility purposes)
            else available = true;

            //Console.WriteLine(config.UniqueId + " : Available = " + available.ToString());

            var result = new DataEvent_Data();
            result.id = "StatusData_Availability";
            result.data01 = config;
            result.data02 = available;

            return result;
        }

        private static DataEvent_Data GetSnapShots(DataTable dt, Configuration config)
        {
            var result = new DataEvent_Data();

            if (dt != null)
            {
                DataEvent_Data de_d = new DataEvent_Data();
                de_d.id = "StatusData_Snapshots";
                de_d.data01 = config;
                de_d.data02 = dt;

                result = de_d;
            }

            return result;
        }

        private static DataEvent_Data GetGenEventValues(DataTable dt, Configuration config)
        {
            DataEvent_Data result = new DataEvent_Data();

            if (dt != null)
            {
                DataEvent_Data de_d = new DataEvent_Data();
                de_d.id = "StatusData_GenEventValues";
                de_d.data01 = config;
                de_d.data02 = dt;

                result = de_d;
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

        private static DataEvent_Data GetShifts(ShiftData shiftData, Configuration config)
        {
            DataEvent_Data result = new DataEvent_Data();

            if (shiftData.shiftDate != null && shiftData.shiftName != null)
            {
                DataTable shifts_DT = Table.Get(config.Databases_Client, TableNames.Shifts, "WHERE Date='" + shiftData.shiftDate + "' AND Shift='" + shiftData.shiftName + "'");
                if (shifts_DT != null)
                {
                    DataEvent_Data de_d = new DataEvent_Data();
                    de_d.id = "StatusData_ShiftData";
                    de_d.data01 = config;
                    de_d.data02 = shifts_DT;

                    result = de_d;
                }
            }

            return result;
        }


        #endregion






        //private void UpdateData(Configuration config)
        //{
        //    var tableNames = new string[]
        //        {
        //        TableNames.SnapShots,
        //        TableNames.Variables,
        //        TableNames.GenEventValues,
        //        };

        //    var tables = Table.Get(config.Databases_Client, tableNames);
        //    if (tables != null)
        //    {
        //        var list = tables.ToList();

        //        // Get Variable Data
        //        //DataEvent_Data variableData = GetVariables(list, config);
        //        // Send Variable Data
        //        //SendDataEvent(variableData);

        //        //DataEvent_Data availablityData = GetAvailability(config, (DataTable)variableData.data02);
        //        SendDataEvent(availablityData);
        //        if ((bool)availablityData.data02)
        //        {
        //            // Get Snapshot Data
        //            DataEvent_Data snapshotData = GetSnapShots(list, config);
        //            // Send Snapshot Data
        //            SendDataEvent(snapshotData);


        //            // Get ShiftData from Variable Data
        //            ShiftData shiftData = GetShiftData((DataTable)variableData.data02);


        //            // Get Gen Event Values
        //            DataEvent_Data genEventData = GetGenEventValues(list, config);
        //            // Send Gen Event Values
        //            SendDataEvent(genEventData);

        //            //Get Shift Data
        //            DataEvent_Data shiftTableData = GetShifts(config, shiftData);
        //            // Send Shift Data
        //            SendDataEvent(shiftTableData);

        //            // Get OEE Data
        //            DataEvent_Data oeeData = GetOEE(config, shiftData);
        //            // Send OEE Data
        //            SendDataEvent(oeeData);
        //        }
        //    }
        //}





        //private void UpdateData(Configuration config)
        //{
        //    // Get Variable Data
        //    DataEvent_Data variableData = GetVariables(config);
        //    // Send Variable Data
        //    SendDataEvent(variableData);

        //    DataEvent_Data availablityData = GetAvailability(config, (DataTable)variableData.data02);
        //    SendDataEvent(availablityData);
        //    if ((bool)availablityData.data02)
        //    {
        //        //GetAllTables(config);

        //        var tableNames = new string[]
        //        {
        //        TableNames.SnapShots,
        //        TableNames.Variables,
        //        TableNames.GenEventValues,
        //        };

        //        var tables = Table.Get(config.Databases_Client, tableNames);
        //        if (tables != null)
        //        {
        //            var list = tables.ToList();

        //            // Get Snapshot Data
        //            DataEvent_Data snapshotData = GetSnapShots(list, config);
        //            // Send Snapshot Data
        //            SendDataEvent(snapshotData);


        //            ShiftData shiftData = GetShiftData((DataTable)variableData.data02);


        //            GetVariables(list, config);
        //            GetGenEventValues(list, config);

        //        }


        //        //// Get Snapshot Data
        //        //DataEvent_Data snapshotData = GetSnapShots(config);
        //        //// Send Snapshot Data
        //        //SendDataEvent(snapshotData);


        //        //// Get ShiftData from Variable Data
        //        //ShiftData shiftData = GetShiftData((DataTable)variableData.data02);


        //        //// Get Gen Event Values
        //        //DataEvent_Data genEventData = GetGenEventValues(config);
        //        //// Send Gen Event Values
        //        //SendDataEvent(genEventData);

        //        //// Get Shift Data
        //        //DataEvent_Data shiftTableData = GetShifts(config, shiftData);
        //        //// Send Shift Data
        //        //SendDataEvent(shiftTableData);

        //        //// Get OEE Data
        //        //DataEvent_Data oeeData = GetOEE(config, shiftData);
        //        //// Send OEE Data
        //        //SendDataEvent(oeeData);

        //        // Get Production Status Data
        //        //DataEvent_Data productionStatusData = GetProductionStatusList(config, shiftData);
        //        // Send Production Status Data
        //        //SendDataEvent(productionStatusData);
        //    }
        //}



        //private static DataEvent_Data GetVariables(List<DataTable> tables, Configuration config)
        //{
        //    var result = new DataEvent_Data();

        //    DataTable dt = GetTableFromList(config.DatabaseId + "_" + TableNames.Variables, tables);
        //    if (dt != null)
        //    {
        //        DataEvent_Data de_d = new DataEvent_Data();
        //        de_d.id = "StatusData_Variables";
        //        de_d.data01 = config;
        //        de_d.data02 = dt;

        //        result = de_d;
        //    }

        //    return result;
        //}









        //static void GetAllTables(Configuration config)
        //{
        //    var tableNames = new string[]
        //    {
        //        TableNames.SnapShots,
        //        TableNames.Variables,
        //        TableNames.GenEventValues,
        //    };

        //    var tables = Table.Get(config.Databases_Client, tableNames);
        //    if (tables != null)
        //    {
        //        var list = tables.ToList();

        //        GetSnapShots(list, config);
        //        GetVariables(list, config);
        //        GetGenEventValues(list, config);

        //    }
        //}




        //private static DataEvent_Data GetSnapShots(Configuration config)
        //{
        //    var result = new DataEvent_Data();

        //    DataTable dt = Table.Get(config.Databases_Client, TableNames.SnapShots);
        //    if (dt != null)
        //    {
        //        DataEvent_Data de_d = new DataEvent_Data();
        //        de_d.id = "StatusData_Snapshots";
        //        de_d.data01 = config;
        //        de_d.data02 = dt;

        //        result = de_d;
        //    }

        //    return result;
        //}

        //private static DataEvent_Data GetSnapShots(List<DataTable> tables, Configuration config)
        //{
        //    var result = new DataEvent_Data();

        //    DataTable dt = GetTableFromList(TableNames.SnapShots, tables);
        //    if (dt != null)
        //    {
        //        DataEvent_Data de_d = new DataEvent_Data();
        //        de_d.id = "StatusData_Snapshots";
        //        de_d.data01 = config;
        //        de_d.data02 = dt;

        //        result = de_d;
        //    }

        //    return result;
        //}

        //private static DataEvent_Data GetVariables(Configuration config)
        //{
        //    var result = new DataEvent_Data();

        //    DataTable dt = Table.Get(config.Databases_Client, TableNames.Variables);
        //    if (dt != null)
        //    {
        //        DataEvent_Data de_d = new DataEvent_Data();
        //        de_d.id = "StatusData_Variables";
        //        de_d.data01 = config;
        //        de_d.data02 = dt;

        //        result = de_d;
        //    }

        //    return result;
        //}







        //private static DataEvent_Data GetShifts(List<DataTable> tables, Configuration config, ShiftData shiftData)
        //{
        //    DataEvent_Data result = new DataEvent_Data();

        //    if (shiftData.shiftDate != null && shiftData.shiftName != null)
        //    {
        //        DataTable shifts_DT = Table.Get(config.Databases_Client, TableNames.Shifts, "WHERE Date='" + shiftData.shiftDate + "' AND Shift='" + shiftData.shiftName + "'");
        //        if (shifts_DT != null)
        //        {
        //            DataEvent_Data de_d = new DataEvent_Data();
        //            de_d.id = "StatusData_ShiftData";
        //            de_d.data01 = config;
        //            de_d.data02 = shifts_DT;

        //            result = de_d;
        //        }
        //    }

        //    return result;
        //}

        static DataEvent_Data GetProductionStatusList(Configuration config, ShiftData shiftData)
        {
            DataEvent_Data result = new DataEvent_Data();

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
                    DataEvent_Data de_d = new DataEvent_Data();
                    de_d.id = "StatusData_ProductionStatus";
                    de_d.data01 = config;
                    de_d.data02 = dt;

                    result = de_d;
                }
            }

            return result;
        }

        static DataEvent_Data GetOEE(Configuration config, ShiftData shiftData)
        {
            DataEvent_Data result = new DataEvent_Data();

            if (shiftData.shiftId != null)
            {
                if (shiftData.shiftId.Contains("_"))
                {
                    string shiftQuery = shiftData.shiftId.Substring(0, shiftData.shiftId.LastIndexOf('_'));

                    DataTable dt = Table.Get(config.Databases_Client, TableNames.OEE, "WHERE Shift_Id LIKE '" + shiftQuery + "%'");
                    if (dt != null)
                    {
                        DataEvent_Data de_d = new DataEvent_Data();
                        de_d.id = "StatusData_OEE";
                        de_d.data01 = config;
                        de_d.data02 = dt;

                        result = de_d;
                    }
                }
            }

            return result;
        }

        

    }

}
