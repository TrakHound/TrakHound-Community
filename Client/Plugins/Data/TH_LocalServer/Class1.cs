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
using TH_Device_Server;
using TH_PlugIns_Client_Control;
using TH_UserManagement.Management;

namespace TH_LocalServer
{
    public class LocalServer : Control_PlugIn
    {

        #region "PlugIn"

        #region "Descriptive"

        public string Title { get { return "Local Server"; } }

        public string Description { get { return "Run the TrakHound Server locally reading directly from the MConnect Agent(s)"; } }

        public ImageSource Image { get { return null; } }


        public string Author { get { return "TrakHound"; } }

        public string AuthorText { get { return "©2015 Feenux LLC. All Rights Reserved"; } }

        public ImageSource AuthorImage { get { return null; } }


        public string LicenseName { get { return "GPLv3"; } }

        public string LicenseText { get { return null; } }

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

        #region "Update Information"

        public string UpdateFileURL { get { return null; } }

        #endregion

        #region "Methods"

        const System.Windows.Threading.DispatcherPriority Priority = System.Windows.Threading.DispatcherPriority.Background;

        public void Initialize()
        {

        }

        public void Closing() 
        {
            foreach (LocalServer_Group group in Servers) if (group.server != null) group.server.Close();
        }

        #endregion

        #region "Events"

        public void Update_DataEvent(DataEvent_Data de_d)
        {

        }

        public event DataEvent_Handler DataEvent;

        public event PlugInTools.ShowRequested_Handler ShowRequested;

        #endregion

        #region "Device Properties"

        List<Configuration> devices = new List<Configuration>();
        public List<Configuration> Devices 
        {
            get { return devices; }
            set
            {
                devices = value;

                Servers.Clear();

                if (devices != null)
                {
                    foreach (Configuration device in devices)
                    {
                        Device_Server server = new Device_Server(device, false);
                        server.DataEvent += server_DataEvent;
                        server.AgentConnected += server_AgentConnected;
                        server.AgentDisconnected += server_AgentDisconnected;

                        LocalServer_Group group = new LocalServer_Group();
                        group.server = server;

                        Servers.Add(group);
                        server.Start(false);
                    }
                }
            }
        }

        void server_DataEvent(TH_PlugIns_Server.DataEvent_Data de_data)
        {
            if (de_data != null)
            {
                ProcessDataEvent(de_data);
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

        class LocalServer_Group
        {
            public Device_Server server;


            public DataTable OeeTable;
        }

        List<LocalServer_Group> Servers = new List<LocalServer_Group>();

        void server_AgentConnected(Device_Server server)
        {
            DataEvent_Data result = new DataEvent_Data();
            result.id = "StatusData_Connection";
            result.data01 = server.configuration;
            result.data02 = true;
            SendDataEvent(result);
        }

        void server_AgentDisconnected(Device_Server server)
        {
            DataEvent_Data result = new DataEvent_Data();
            result.id = "StatusData_Connection";
            result.data01 = server.configuration;
            result.data02 = false;
            SendDataEvent(result);
        }

        void ProcessDataEvent(TH_PlugIns_Server.DataEvent_Data de_data)
        {
            switch (de_data.id.ToLower())
            {
                case "snapshottable":

                    DataEvent_Data snapshots = new DataEvent_Data();
                    snapshots.id = "StatusData_Snapshots";
                    snapshots.data01 = de_data.data01;
                    snapshots.data02 = de_data.data02;

                    SendDataEvent(snapshots);

                    break;

                case "oeetable":

                    DataEvent_Data oee = new DataEvent_Data();
                    oee.id = "StatusData_Oee";
                    oee.data01 = de_data.data01;
                    oee.data02 = de_data.data02;

                    SendDataEvent(oee);

                    break;
            }


            //Configuration config = de_data.data01 as Configuration;
            //if (config != null)
            //{
            //    LocalServer_Group group = Servers.Find(x => x.server.configuration.UniqueId == config.UniqueId);
            //    if (group != null)
            //    {
            //        switch (de_data.id.ToLower())
            //        {
            //            case "snapshottable":

            //                DataEvent_Data snapshots = new DataEvent_Data();
            //                snapshots.id = "StatusData_Snapshots";
            //                snapshots.data01 = de_data.data01;
            //                snapshots.data02 = de_data.data02;

            //                SendDataEvent(snapshots);

            //                break;

            //            case "oeetable":

            //                //// Add new rows to OEE Table
            //                //DataTable dt = de_data.data02 as DataTable;
            //                //if (dt != null)
            //                //{
            //                //    if (group.OeeTable == null) 
            //                //    {
            //                //        group.OeeTable = new DataTable();
            //                //        foreach (DataColumn column in dt.Columns) group.OeeTable.Columns.Add(column);
            //                //        group.OeeTable.PrimaryKey = dt.PrimaryKey;
            //                //    }

            //                //    foreach (DataRow row in dt.Rows)
            //                //    {
            //                //        DataRow newRow = group.OeeTable.NewRow();

            //                //        foreach (DataColumn col in dt.Columns)
            //                //        {
            //                //            string x = col.ColumnName;

            //                //            newRow[x] = row[x];
            //                //        }

            //                //        group.OeeTable.Rows.Add(newRow);
            //                //    }
            //                //}

            //                //DataEvent_Data oee = new DataEvent_Data();
            //                //oee.id = "StatusData_Oee";
            //                //oee.data01 = de_data.data01;
            //                //oee.data02 = group.OeeTable;

            //                //SendDataEvent(oee);

            //                DataEvent_Data oee = new DataEvent_Data();
            //                oee.id = "StatusData_Oee";
            //                oee.data01 = de_data.data01;
            //                oee.data02 = de_data.data02;

            //                SendDataEvent(oee);

            //                break;
            //        }
            //    }
            //}
        }

        void SendDataEvent(DataEvent_Data de_d)
        {
            if (DataEvent != null) DataEvent(de_d);
        }


    }
}
