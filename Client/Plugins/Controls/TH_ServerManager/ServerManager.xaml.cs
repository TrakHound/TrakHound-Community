// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.IO;
using System.Xml;
using System.Data;
using System.Collections.ObjectModel;

using TH_PlugIns_Client_Control;
using TH_Device_Client;
using TH_Configuration;
using TH_Device_Server;
using TH_Global.Functions;
using TH_ServerManager.Controls;

using TH_GeneratedData;

namespace TH_ServerManager
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ServerManager : UserControl, Control_PlugIn
    {
        public ServerManager()
        {
            InitializeComponent();

            DataContext = this;

            servers = new List<Device_Server>();
        }

        #region "PlugIn"

        #region "Descriptive"

        public string Title { get { return "Server Manager"; } }

        public string Description { get { return "Manage Servers for temporary or testing purposes"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TH_ServerManager;component/Resources/Server_01.png")); } }


        public string Author { get { return "TrakHound"; } }

        public string AuthorText { get { return "©2015 Feenux LLC. All Rights Reserved"; } }

        public ImageSource AuthorImage { get { return new BitmapImage(new Uri("pack://application:,,,/TH_ServerManager;component/Resources/TrakHound_Logo_10_200px.png")); } }


        public string LicenseName { get { return "GPLv3"; } }

        public string LicenseText { get { return File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"\License\" + "License.txt"); } }

        #endregion

        #region "Update Information"

        public string UpdateFileURL { get { return "http://www.feenux.com/trakhound/appinfo/th/servermanager-appinfo.txt"; } }

        #endregion

        #region "Plugin Properties/Options"

        public string DefaultParent { get { return null; } }
        public string DefaultParentCategory { get { return null; } }

        public bool AcceptsPlugIns { get { return true; } }

        public bool OpenOnStartUp { get { return false; } }

        public bool ShowInAppMenu { get { return true; } }

        public List<PlugInConfigurationCategory> SubCategories { get; set; }

        public List<Control_PlugIn> PlugIns { get; set; }

        #endregion

        #region "Methods"

        public void Initialize() { }

        public void Update(ReturnData rd)
        {

        }

        public void Closing() 
        {

            foreach (Device_Server server in servers) server.Close();

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

        private List<Device_Client> lDevices;
        public List<Device_Client> Devices
        {
            get
            {
                return lDevices;
            }
            set
            {
                lDevices = value;


                foreach (Device_Client device in lDevices)
                {
                    AddServer(device.configuration);
                }

                CreateDeviceList(servers);

            }
        }

        private int lSelectedDeviceIndex;
        public int SelectedDeviceIndex
        {

            get { return lSelectedDeviceIndex; }

            set
            {
                lSelectedDeviceIndex = value;
            }

        }

        #endregion

        #region "Options"

        public OptionsPage Options { get; set; }

        #endregion

        public object RootParent { get; set; }

        #endregion

        #region "Devices"

        #region "Device List"

        ObservableCollection<DeviceItem> device_list;
        public ObservableCollection<DeviceItem> Device_List
        {
            get
            {
                if (device_list == null) device_list = new ObservableCollection<DeviceItem>();
                return device_list;
            }
            set
            {
                device_list = value;
            }
        }

        void CreateDeviceList(List<Device_Server> devices)
        {
            Device_List.Clear();

            for (int x = 0; x <= devices.Count - 1; x++)
            {
                Device_Server device = devices[x];

                DeviceItem DI = new DeviceItem();
                DI.Index = x;
                DI.Device_CompanyName = device.configuration.Description.Customer_Name;
                DI.Device_Description = device.configuration.Description.Description;
                DI.Device_Manufacturer = device.configuration.Description.Manufacturer;
                DI.Device_Model = device.configuration.Description.Model;
                DI.Device_ID = device.configuration.Description.Machine_ID;

                // Set Images
                //DI.Device_Image = Image_Functions.SetImageSize(Image_Functions.GetImageFromFile(device.configuration.FileLocations.Image_Path), 0, 50);
                //DI.Device_ManufacturerLogo = Image_Functions.SetImageSize(Image_Functions.GetImageFromFile(device.configuration.FileLocations.Manufacturer_Logo_Path), 100, 40);

                // Event handlers
                DI.Selected += DI_Selected;
                DI.StartClicked += DI_StartClicked;
                DI.StopClicked += DI_StopClicked;
                DI.DropTablesClicked += DI_DropTablesClicked;
                DI.StartFromLastClicked += DI_StartFromLastClicked;

                Device_List.Add(DI);
            }
        }


        void DI_StartFromLastClicked(int Index)
        {

        }

        void DI_DropTablesClicked(int Index)
        {

        }

        void DI_Selected(int Index)
        {

            foreach (DeviceItem DI in Device_List) if (DI.Index != Index) DI.IsSelected = false;

            Device_List[Index].IsSelected = true;

        }

        void DI_StartClicked(int Index)
        {
            servers[Index].Start();
        }

        void DI_StopClicked(int Index)
        {
            servers[Index].Close();
        }

        #endregion


        List<Device_Server> servers;

        void AddServer(Configuration config)
        {

            Device_Server server = new Device_Server(config, false);

            // Set handler for Connection Status
            server.StatusUpdated += Machine_StatusUpdated;
            server.ProcessingStatusChanged += server_ProcessingStatusChanged;
            

            server.DataEvent += server_DataEvent;

            servers.Add(server);

        }

        void server_DataEvent(TH_PlugIns_Server.DataEvent_Data de_data)
        {
            SendConnectionData(true);

            GetSnapShots(de_data);

        }



        void server_ProcessingStatusChanged(int index, string status)
        {
            //this.Dispatcher.Invoke(new Action<int, string>(server_ProcessingStatusChanged_GUI), Priority, new object[] { index, status });
        }

        void server_ProcessingStatusChanged_GUI(int index, string status)
        {
            //DeviceItem device = Device_List[index];

            //device.Device_ProcessingStatus = status;
        }


        void Machine_StatusUpdated(int Index, Device_Server.ConnectionStatus Status)
        {

            //this.Dispatcher.Invoke(new Action<int, Device_Server.ConnectionStatus>(Machine_StatusUpdated_GUI), Priority, new object[] { Index, Status });

        }

        void Machine_StatusUpdated_GUI(int Index, Device_Server.ConnectionStatus Status)
        {

            //DeviceItem device = Device_List[Index];

            //device.Device_ConnectionStatus = Status;

        }

        #endregion

        #region "Data Conversion"

        string shiftDate = null;
        string shiftName = null;
        string shiftId = null;
        string shiftStart = null;
        string shiftEnd = null;
        string shiftStartUTC = null;
        string shiftEndUTC = null;

        void SendConnectionData(bool connected)
        {
            DataEvent_Data de_d = new DataEvent_Data();
            de_d.id = "DeviceStatus_Connection";
            de_d.data = connected;

            if (DataEvent != null) DataEvent(de_d);
        }

        void GetSnapShots(TH_PlugIns_Server.DataEvent_Data de_d)
        {
            if (de_d.id.ToLower() == "snapshotitems")
            {
                if (de_d.data != null)
                {
                    List<GeneratedData.SnapShotItem> items = (List<GeneratedData.SnapShotItem>)de_d.data;

                    Dictionary<string, Tuple<DateTime, string, string>> data = new Dictionary<string, Tuple<DateTime, string, string>>();

                    foreach (GeneratedData.SnapShotItem item in items)
                    {
                        string key = item.name;

                        DateTime timestamp = item.timestamp;

                        string value = item.value;
                        string prevvalue = item.previous_value;

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


                    DataEvent_Data de_data = new DataEvent_Data();
                    de_data.id = "DeviceStatus_Snapshots";
                    de_data.data = data;

                    if (DataEvent != null) DataEvent(de_data);
                }
            }
        }


        #endregion

    }
}
