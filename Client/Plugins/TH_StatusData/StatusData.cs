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
//using TH_UserManagement.Management;

namespace TH_StatusData
{
    public partial class StatusData
    {

        int interval = 5000;

        //ManualResetEvent stop = null;

        List<ManualResetEvent> stops = new List<ManualResetEvent>();

        void Update_Start()
        {
            Update_Stop();

            Console.WriteLine("StatusData :: All Previous Threads Stopped");

            if (Devices != null)
            {
                //stop = new ManualResetEvent(false);

                foreach (Configuration device in Devices.ToList())
                {
                    Thread thread = new Thread(new ParameterizedThreadStart(Update_Worker));
                    var stop = new ManualResetEvent(false);
                    stops.Add(stop);

                    var info = new UpdateInfo();
                    info.Config = device;
                    info.Stop = stop;

                    thread.Start(info);
                }
            }
        }

        void Update_Stop()
        {
            // Set Update thread to stop
            //if (stop != null) stop.Set();

            foreach (var stop in stops)
            {
                if (stop != null) stop.Set();
            }

            stops.Clear();
        }

        class UpdateInfo
        {
            public Configuration Config { get; set; }
            public ManualResetEvent Stop { get; set; }
        }





        //int interval_min = 3000;
        //    int interval_max = 60000;
        //    int interval = interval_min;

        //    bool first = true;

        //    if (UseDatabases)
        //    {
        //        while (!dbsuccess)
        //        {
        //            // Ping Database connection for each Database Configuration
        //            dbsuccess = true;
        //            string msg = null;

        //            foreach (Database_Configuration db_config in configuration.Databases_Server.Databases)
        //            {
        //                if (!TH_Database.Global.Ping(db_config, out msg))
        //                {
        //                    dbsuccess = false;
        //                    break;
        //                }
        //            }

        //            if (dbsuccess) UpdateProcessingStatus("Database Connections Established");
        //            else
        //            {
        //                // Increase the interval by 25% until interval == interval_max
        //                if (!first) interval = Math.Min(Convert.ToInt32(interval + (interval * 0.25)), interval_max);
        //                first = false;

        //                WriteToConsole("Error in Database Connection... Retrying in " + interval.ToString() + "ms", ConsoleOutputType.Error);
        //                if (msg != null) WriteToConsole(msg, ConsoleOutputType.Error);

        //                // Sleep the current thread for the calculated interval
        //                System.Threading.Thread.Sleep(interval);
        //            }
        //        }
        //    } 



        const int INTERVAL_MIN = 3000;
        const int INTERVAL_MAX = 60000;

        void Update_Worker(object o)
        {
            if (o != null)
            {
                var info = (UpdateInfo)o;

                Configuration config = info.Config;

                int interval = INTERVAL_MIN;
                bool first = true;

                while (!info.Stop.WaitOne(0, true))
                {
                    // Get Connection Status
                    DataEvent_Data connected = GetConnectionData(config);

                    // Send Connection Status
                    SendDataEvent(connected);

                    if (connected != null && connected.data02 != null && connected.data02.GetType() == typeof(bool))
                    {
                        if ((bool)connected.data02 == true)
                        {
                            // Reset the interval back to the Minimum
                            interval = INTERVAL_MIN;

                            UpdateData(config);
                        }
                        else
                        {
                            // Increase the interval by 50% until interval == interval_max
                            if (!first) interval = Math.Min(Convert.ToInt32(interval + (interval * 0.5)), INTERVAL_MAX);
                            first = false;
                        }
                    }

                    Thread.Sleep(interval);
                }
            }
        }

        private void UpdateData(Configuration config)
        {

            // Get Snapshot Data
            DataEvent_Data snapshotData = GetSnapShots(config);
            // Send Snapshot Data
            SendDataEvent(snapshotData);

            // Get Variable Data
            DataEvent_Data variableData = GetVariables(config);
            // Send Variable Data
            SendDataEvent(variableData);


            // Get ShiftData from Variable Data
            ShiftData shiftData = GetShiftData((DataTable)variableData.data02);


            // Get Gen Event Values
            DataEvent_Data genEventData = GetGenEventValues(config);
            // Send Gen Event Values
            SendDataEvent(genEventData);

            // Get Shift Data
            DataEvent_Data shiftTableData = GetShifts(config, shiftData);
            // Send Shift Data
            SendDataEvent(shiftTableData);

            // Get OEE Data
            DataEvent_Data oeeData = GetOEE(config, shiftData);
            // Send OEE Data
            SendDataEvent(oeeData);

            // Get Production Status Data
            DataEvent_Data productionStatusData = GetProductionStatusList(config, shiftData);
            // Send Production Status Data
            SendDataEvent(productionStatusData);

        }



        //class Snapshot_Return
        //{
        //    public Snapshot_Return() { shiftData = new ShiftData(); }

        //    public DataEvent_Data de_data { get; set; }
        //    public ShiftData shiftData { get; set; }
        //}


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

        static DataEvent_Data GetConnectionData(Configuration config)
        {
            bool connected = CheckDatabaseConnections(config);

            DataEvent_Data result = new DataEvent_Data();
            result.id = "StatusData_Connection";
            result.data01 = config;
            result.data02 = connected;

            return result;
        }

        void SendDataEvent(DataEvent_Data de_d)
        {
            if (de_d != null)
            {
                if (de_d.id != null)
                {
                    if (DataEvent != null) DataEvent(de_d);
                }
            }
        }

        static bool CheckDatabaseConnections(Configuration config)
        {
            bool result = false;
            string msg = null;

            foreach (Database_Configuration db_config in config.Databases_Client.Databases)
            {
                bool ping = TH_Database.Global.Ping(db_config, out msg);

                if (ping) { result = true; break; }
                else Console.WriteLine("CheckDatabaseConnection() :: Error :: " + config.Description + " :: " + db_config.Type + " :: " + msg);
            }

            return result;
        }


        static DataEvent_Data GetSnapShots(Configuration config)
        {
            var result = new DataEvent_Data();

            DataTable dt = Table.Get(config.Databases_Client, TableNames.SnapShots);
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

        static DataEvent_Data GetVariables(Configuration config)
        {
            var result = new DataEvent_Data();

            DataTable dt = Table.Get(config.Databases_Client, TableNames.Variables);
            if (dt != null)
            {
                DataEvent_Data de_d = new DataEvent_Data();
                de_d.id = "StatusData_Variables";
                de_d.data01 = config;
                de_d.data02 = dt;

                result = de_d;
            }

            return result;
        }

        static ShiftData GetShiftData(DataTable dt)
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

        static DataEvent_Data GetShifts(Configuration config, ShiftData shiftData)
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

        static DataEvent_Data GetGenEventValues(Configuration config)
        {
            DataEvent_Data result = new DataEvent_Data();

            DataTable shifts_DT = Table.Get(config.Databases_Client, TableNames.GenEventValues);
            if (shifts_DT != null)
            {
                DataEvent_Data de_d = new DataEvent_Data();
                de_d.id = "StatusData_GenEventValues";
                de_d.data01 = config;
                de_d.data02 = shifts_DT;

                result = de_d;
            }

            return result;
        }

    }

}
