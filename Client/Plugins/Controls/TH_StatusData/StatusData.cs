using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Data;

using TH_Configuration;
using TH_Database;
using TH_Global;
using TH_PlugIns_Client_Control;
using TH_UserManagement.Management;

namespace TH_StatusData
{
    public class StatusData : Control_PlugIn
    {

        public StatusData()
        {
            update_TIMER = new System.Timers.Timer();
            update_TIMER.Interval = 2000;
            update_TIMER.Elapsed += update_TIMER_Elapsed;
            update_TIMER.Enabled = true;
        }

        #region "PlugIn"

        #region "Descriptive"

        public string Title { get { return "Status Data"; } }

        public string Description { get { return "Retrieve Data from database(s) related to device status"; } }

        public ImageSource Image { get { return null; } }


        public string Author { get { return "TrakHound"; } }

        public string AuthorText { get { return "©2015 Feenux LLC. All Rights Reserved"; } }

        public ImageSource AuthorImage { get { return null; } }


        public string LicenseName { get { return "GPLv3"; } }

        public string LicenseText { get { return null; } }

        #endregion

        #region "Update Information"

        public string UpdateFileURL { get { return null; } }

        #endregion

        #region "Plugin Properties/Options"

        public string DefaultParent { get { return null; } }
        public string DefaultParentCategory { get { return null; } }

        public bool AcceptsPlugIns { get { return false; } }

        public bool OpenOnStartUp { get { return false; } }

        public bool ShowInAppMenu { get { return false; } }

        public List<PlugInConfigurationCategory> SubCategories { get; set; }

        public List<Control_PlugIn> PlugIns { get; set; }

        #endregion

        #region "Methods"

        public void Initialize() { }

        //public void Update(ReturnData rd) { }

        public void Closing() { if (update_TIMER != null) update_TIMER.Enabled = false; }

        #endregion

        #region "Events"

        public void Update_DataEvent(DataEvent_Data de_d)
        {

        }

        public event DataEvent_Handler DataEvent;

        public event PlugInTools.ShowRequested_Handler ShowRequested;

        #endregion

        #region "Devices"

        private List<Configuration> lDevices;
        public List<Configuration> Devices
        {
            get { return lDevices; }
            set
            {
                lDevices = value;
            }
        }

        #endregion

        #region "Options"

        public OptionsPage Options { get; set; }

        #endregion

        #region "User"

        public UserConfiguration CurrentUser { get; set; }

        public Database_Settings UserDatabaseSettings { get; set; }

        #endregion

        public object RootParent { get; set; }

        #endregion

        System.Timers.Timer update_TIMER;

        void update_TIMER_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Devices != null)
            {
                foreach (Configuration device in Devices)
                {
                    if (DataEvent != null)
                    {
                        // Get Connection Status
                        DataEvent_Data connected = GetConnectionData(device);

                        // Send Connection Status
                        SendDataEvent(connected);


                        // Get Snapshot Data
                        Snapshot_Return snapshotData = GetSnapShots(device);

                        // Send Snapshot Data
                        SendDataEvent(snapshotData.de_data);


                        // Get Shift Data
                        DataEvent_Data shiftData = GetShifts(device, snapshotData.shiftData);
                        
                        // Send Shift Data
                        SendDataEvent(shiftData);


                        // Get OEE Data
                        DataEvent_Data oeeData = GetOEE(device, snapshotData.shiftData);

                        // Send OEE Data
                        SendDataEvent(oeeData);
                    }
                }
            }       
        }

        class Snapshot_Return
        {
            public Snapshot_Return() { shiftData = new ShiftData(); }

            public DataEvent_Data de_data { get; set; }
            public ShiftData shiftData { get; set; }
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

        static DataEvent_Data GetConnectionData(Configuration config)
        {
            DataEvent_Data result = new DataEvent_Data();
            result.id = "StatusData_Connection";
            result.data01 = config;
            result.data02 = CheckDatabaseConnections(config);

            return result;
        }

        void SendDataEvent(DataEvent_Data de_d)
        {
            if (DataEvent != null) DataEvent(de_d);
        }

        static bool CheckDatabaseConnections(Configuration config)
        {
            bool result = false;

            foreach (Database_Configuration db_config in config.Databases.Databases)
            {
                if (TH_Database.Global.Ping(db_config)) { result = true; break; }
            }

            return result;
        }


        static Snapshot_Return GetSnapShots(Configuration config)
        {
            Snapshot_Return result = new Snapshot_Return();

            DataTable snapshots_DT = Table.Get(config.Databases, TableNames.SnapShots);
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
                if (val != null) result.shiftData.shiftName = val.Item2;
                else result.shiftData.shiftName = null;

                val = null;
                data.TryGetValue("Current Shift Date", out val);
                if (val != null) result.shiftData.shiftDate = val.Item2;
                else result.shiftData.shiftDate = null;

                val = null;
                data.TryGetValue("Current Shift Id", out val);
                if (val != null) result.shiftData.shiftId = val.Item2;
                else result.shiftData.shiftId = null;

                // Local
                val = null;
                data.TryGetValue("Current Shift Begin", out val);
                if (val != null) result.shiftData.shiftStart = val.Item2;
                else result.shiftData.shiftStart = null;

                val = null;
                data.TryGetValue("Current Shift End", out val);
                if (val != null) result.shiftData.shiftEnd = val.Item2;
                else result.shiftData.shiftEnd = null;

                // UTC
                val = null;
                data.TryGetValue("Current Shift Begin UTC", out val);
                if (val != null) result.shiftData.shiftStartUTC = val.Item2;
                else result.shiftData.shiftStartUTC = null;

                val = null;
                data.TryGetValue("Current Shift End UTC", out val);
                if (val != null) result.shiftData.shiftEndUTC = val.Item2;
                else result.shiftData.shiftEndUTC = null;


                DataEvent_Data de_d = new DataEvent_Data();
                de_d.id = "StatusData_Snapshots";
                de_d.data01 = config;
                de_d.data02 = data;

                result.de_data = de_d;
            }

            return result;
        }

        static DataEvent_Data GetShifts(Configuration config, ShiftData shiftData)
        {
            DataEvent_Data result = new DataEvent_Data();

            if (shiftData.shiftDate != null && shiftData.shiftName != null)
            {
                DataTable shifts_DT = Table.Get(config.Databases, TableNames.Shifts, "WHERE Date='" + shiftData.shiftDate + "' AND Shift='" + shiftData.shiftName + "'");
                if (shifts_DT != null)
                {
                    DataEvent_Data de_d = new DataEvent_Data();
                    de_d.id = "StatusData_ShiftData";
                    de_d.data01 = config;
                    de_d.data02 = shifts_DT;

                    //List<Dictionary<string, string>> data = new List<Dictionary<string, string>>();

                    //foreach (DataRow row in shifts_DT.Rows)
                    //{
                    //    Dictionary<string, string> rowdata = new Dictionary<string, string>();

                    //    foreach (DataColumn column in row.Table.Columns)
                    //    {
                    //        string key = column.ColumnName;
                    //        string value = row[column].ToString();

                    //        rowdata.Add(key, value);
                    //    }

                    //    data.Add(rowdata);
                    //}

                    //DataEvent_Data de_d = new DataEvent_Data();
                    //de_d.id = "DeviceStatus_Shifts";
                    //de_d.data01 = config;
                    //de_d.data02 = data;

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


                DataTable table = Table.Get(config.Databases, tableName, filter);
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

                    DataEvent_Data de_d = new DataEvent_Data();
                    de_d.id = "DeviceStatus_ProductionStatus";
                    de_d.data01 = config;
                    de_d.data02 = data;

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

                    DataTable dt = Table.Get(config.Databases, TableNames.OEE, "WHERE Shift_Id LIKE '" + shiftQuery + "%'");
                    if (dt != null)
                    {
                        DataEvent_Data de_d = new DataEvent_Data();
                        de_d.id = "StatusData_OEE";
                        de_d.data01 = config;
                        de_d.data02 = dt;

                        //List<Dictionary<string, string>> data = new List<Dictionary<string, string>>();

                        //foreach (DataRow row in dt.Rows)
                        //{
                        //    Dictionary<string, string> rowdata = new Dictionary<string, string>();

                        //    foreach (DataColumn column in row.Table.Columns)
                        //    {
                        //        string key = column.ColumnName;
                        //        string value = row[column].ToString();

                        //        rowdata.Add(key, value);
                        //    }

                        //    string shiftname = "";
                        //    if (row.Table.Columns.Contains("shift_id")) shiftname = row["shift_id"].ToString();

                        //    data.Add(rowdata);
                        //}

                        //DataEvent_Data de_d = new DataEvent_Data();
                        //de_d.id = "DeviceStatus_OEE";
                        //de_d.data01 = config;
                        //de_d.data02 = data;

                        result = de_d;
                    }
                }
            }

            return result;
        }

    }

}
