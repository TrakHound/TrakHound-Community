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
using TH_MySQL;

using TH_ServerManager.Controls;

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

            servers = new List<ServerGroup>();

            ReadMachines();

            CreateDeviceList(servers);

        }

        #region "PlugIn"

        #region "Descriptive"

        public string Title { get { return "Server Manager"; } }

        public string Description { get { return "Manage Servers for temporary or testing purposes"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TH_ServerManager;component/Resources/Server_01.png")); } }


        public string Author { get { return "TrakHound"; } }

        public string AuthorText { get { return "©2015 Feenux LLC. All Rights Reserved"; } }

        public ImageSource AuthorImage { get { return new BitmapImage(new Uri("pack://application:,,,/TH_ServerManager;component/Resources/TrakHound_Logo_10_200px.png")); } }


        public string LicenseName { get { return "GPLv2"; } }

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

        public List<PlugInConfigurationCategory> SubCategories { get; set; }

        public List<Control_PlugIn> PlugIns { get; set; }

        #endregion

        #region "Methods"

        public void Initialize() { }

        public void Update(ReturnData rd)
        {

        }

        public void Closing() { }

        #endregion

        #region "Events"

        public void Update_DataEvent(DataEvent_Data de_d)
        {

        }

        public event DataEvent_Handler DataEvent;

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

        class ServerGroup
        {
            public Device_Server server;
            public string output;
            //public List<Console_Item> console_lines;

            ObservableCollection<Console_Item> console_output;
            public ObservableCollection<Console_Item> Console_Output
            {
                get
                {
                    if (console_output == null) console_output = new ObservableCollection<Console_Item>();
                    return console_output;
                }
                set
                {
                    console_output = value;
                }
            }
        }

        List<ServerGroup> servers;

        ServerGroup selectedServer;

        private void ReadMachines()
        {

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

            string MachinesListFilePath = System.IO.Path.GetDirectoryName(assembly.Location) + @"\" + "Machines.Xml";

            Console.WriteLine(MachinesListFilePath);

            if (System.IO.File.Exists(MachinesListFilePath))
            {

                XmlDocument doc = new XmlDocument();
                doc.Load(MachinesListFilePath);

                int Index = 0;

                foreach (XmlNode Node in doc.DocumentElement.ChildNodes)
                {
                    // Sort through Machine Types
                    switch (Node.Name.ToLower())
                    {

                        case "device":
                            ProcessDevice(Index, Node);
                            Index += 1;
                            break;
                    }

                }

                Console.WriteLine("Machines File Successfully Read From : " + MachinesListFilePath);

            }
            else
            {
                Console.WriteLine("Machines File Not Found : " + MachinesListFilePath);
            }

        }

        private void ProcessDevice(int Index, XmlNode Node)
        {

            string SettingsPath = null;

            foreach (XmlNode ChildNode in Node.ChildNodes)
            {

                switch (ChildNode.Name.ToLower())
                {
                    case "settings_path": SettingsPath = ChildNode.InnerText; break;
                }

            }

            if (SettingsPath != null)
            {
                SettingsPath = GetConfigurationPath(SettingsPath);

                Console.WriteLine("Reading Device Configuration File @ '" + SettingsPath + "'");


                if (File.Exists(SettingsPath))
                {

                    Configuration config = new Configuration();

                    config = Configuration.ReadConfigFile(SettingsPath);
                    config.Index = Index;

                    Device_Server server = new Device_Server(config);
                    server.configurationPath = SettingsPath;
                    server.updateConfigurationFile = false;

                    // Set handler for Connection Status
                    server.StatusUpdated += Machine_StatusUpdated;
                    server.ProcessingStatusChanged += server_ProcessingStatusChanged;
                    //server.Output_LineAdded += server_Output_LineAdded;

                    //OutputWriter outputWriter = new OutputWriter();
                    //outputWriter.Index = config.Index;
                    //outputWriter.LineWritten += outputWriter_LineWritten;

                    ServerGroup deviceserver = new ServerGroup();
                    deviceserver.server = server;
                    //device.outputWriter = outputWriter;

                    servers.Add(deviceserver);

                }

            }

        }

        static string GetConfigurationPath(string path)
        {
            // If not full path, try System Dir ('C:\TrakHound\') and then local App Dir
            if (!System.IO.Path.IsPathRooted(path))
            {
                // Remove initial Backslash if contained in "configuration_path"
                if (path[0] == '\\' && path.Length > 1) path.Substring(1);

                string original = path;

                // Check System Path
                path = TH_Global.FileLocations.TrakHound + "\\Configuration Files\\" + original;
                if (File.Exists(path)) return path;
                else Console.WriteLine(path + " Not Found");


                // Check local app Path
                path = AppDomain.CurrentDomain.BaseDirectory + "Configuration Files\\" + original;
                if (File.Exists(path)) return path;
                else Console.WriteLine(path + " Not Found");

                // if no files exist return null
                return null;
            }
            else return path;
        }




        void server_ProcessingStatusChanged(int index, string status)
        {
            this.Dispatcher.Invoke(new Action<int, string>(server_ProcessingStatusChanged_GUI), Priority, new object[] { index, status });
        }

        void server_ProcessingStatusChanged_GUI(int index, string status)
        {
            DeviceItem device = Device_List[index];

            device.Device_ProcessingStatus = status;
        }


        void Machine_StatusUpdated(int Index, Device_Server.ConnectionStatus Status)
        {

            this.Dispatcher.Invoke(new Action<int, Device_Server.ConnectionStatus>(Machine_StatusUpdated_GUI), Priority, new object[] { Index, Status });

        }

        void Machine_StatusUpdated_GUI(int Index, Device_Server.ConnectionStatus Status)
        {

            DeviceItem device = Device_List[Index];

            device.Device_ConnectionStatus = Status;

        }

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

        void CreateDeviceList(List<ServerGroup> devices)
        {
            Device_List.Clear();

            for (int x = 0; x <= devices.Count - 1; x++)
            {
                Device_Server device = devices[x].server;

                DeviceItem DI = new DeviceItem();
                DI.Index = x;
                DI.Device_CompanyName = device.configuration.Description.Customer_Name;
                DI.Device_Description = device.configuration.Description.Description;
                DI.Device_Manufacturer = device.configuration.Description.Manufacturer;
                DI.Device_Model = device.configuration.Description.Model;
                DI.Device_ID = device.configuration.Description.Machine_ID;

                // Set Images
                DI.Device_Image = TH_Functions.Image_Functions.SetImageSize(TH_Functions.Image_Functions.GetImageFromFile(device.configuration.FileLocations.Image_Path), 0, 50);
                DI.Device_ManufacturerLogo = TH_Functions.Image_Functions.SetImageSize(TH_Functions.Image_Functions.GetImageFromFile(device.configuration.FileLocations.Manufacturer_Logo_Path), 100, 40);

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
            servers[Index].server.Start(false);
        }

        void DI_DropTablesClicked(int Index)
        {
            ServerGroup device = servers[Index];

            if (MessageBox.Show("Delete ALL tables in " + device.server.configuration.DataBaseName + "?", "Delete Tables", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                string[] tablenames = Global.Table_List(device.server.configuration.SQL);

                Global.Table_Drop(device.server.configuration.SQL, tablenames);
            }

        }

        void DI_Selected(int Index)
        {

            foreach (DeviceItem DI in Device_List) if (DI.Index != Index) DI.IsSelected = false;

            Device_List[Index].IsSelected = true;

            selectedServer = servers[Index];

            Console_Output = selectedServer.Console_Output;

            this.Dispatcher.BeginInvoke(new Action<Configuration>(LoadTablesList), Priority, new object[] { selectedServer.server.configuration });

            ReadDeviceSettings();

        }

        void DI_StartClicked(int Index)
        {
            servers[Index].server.Start();
        }

        void DI_StopClicked(int Index)
        {
            servers[Index].server.Close();
        }

        #region "Console Output"

        public class Console_Item
        {
            public int Row { get; set; }
            public DateTime Timestamp { get; set; }
            public string Text { get; set; }
        }

        ObservableCollection<Console_Item> console_output;
        public ObservableCollection<Console_Item> Console_Output
        {
            get
            {
                if (console_output == null) console_output = new ObservableCollection<Console_Item>();
                return console_output;
            }
            set
            {
                console_output = value;
            }
        }

        void server_Output_LineAdded(int index, string line)
        {
            ServerGroup server = servers[index];

            Console_Item ci = new Console_Item();
            ci.Row = server.Console_Output.Count;
            ci.Timestamp = DateTime.Now;
            ci.Text = line;

            this.Dispatcher.BeginInvoke(new Action<ServerGroup, Console_Item>(UpdateConsoleOutput), new object[] { server, ci });
        }

        void UpdateConsoleOutput(ServerGroup server, Console_Item ci)
        {
            server.Console_Output.Add(ci);
        }

        private void Console_DG_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (sender.GetType() == typeof(DataGrid))
            {
                DataGrid dg = (DataGrid)sender;
                dg.SelectedItem = e.Row.Item;
                dg.ScrollIntoView(e.Row.Item);
            }
        }

        #endregion

        #region "Device Settings"

        void ReadDeviceSettings()
        {

            if (selectedServer != null)
            {

                ReadDeviceSettings_Server();

                ReadDeviceSettings_InstanceTable();

            }

        }

        #region "Server"

        void ReadDeviceSettings_Server()
        {

            ReadDeviceSettings_Server_Tables();

        }

        #region "Tables"

        void ReadDeviceSettings_Server_Tables()
        {

            ReadDeviceSettings_Server_Tables_MTConnect();

        }

        #region "MTConnect"

        void ReadDeviceSettings_Server_Tables_MTConnect()
        {

            Server_Tables_MTConnect_Probe = selectedServer.server.configuration.Server.Tables.MTConnect.Probe;
            Server_Tables_MTConnect_Current = selectedServer.server.configuration.Server.Tables.MTConnect.Current;
            Server_Tables_MTConnect_Sample = selectedServer.server.configuration.Server.Tables.MTConnect.Sample;

        }

        #region "Server_Tables_MTConnect_Probe"

        public bool Server_Tables_MTConnect_Probe
        {
            get { return (bool)GetValue(Server_Tables_MTConnect_ProbeProperty); }
            set { SetValue(Server_Tables_MTConnect_ProbeProperty, value); }
        }

        public static readonly DependencyProperty Server_Tables_MTConnect_ProbeProperty =
            DependencyProperty.Register("Server_Tables_MTConnect_Probe", typeof(bool), typeof(ServerManager), new PropertyMetadata(false));

        private void Server_Tables_MTConnect_Probe_Checked(object sender, RoutedEventArgs e)
        {
            if (selectedServer != null) selectedServer.server.configuration.Server.Tables.MTConnect.Probe = true;
        }

        private void Server_Tables_MTConnect_Probe_Unchecked(object sender, RoutedEventArgs e)
        {
            if (selectedServer != null) selectedServer.server.configuration.Server.Tables.MTConnect.Probe = false;
        }

        #endregion

        #region "Server_Tables_MTConnect_Current"

        public bool Server_Tables_MTConnect_Current
        {
            get { return (bool)GetValue(Server_Tables_MTConnect_CurrentProperty); }
            set { SetValue(Server_Tables_MTConnect_CurrentProperty, value); }
        }

        public static readonly DependencyProperty Server_Tables_MTConnect_CurrentProperty =
            DependencyProperty.Register("Server_Tables_MTConnect_Current", typeof(bool), typeof(ServerManager), new PropertyMetadata(false));

        private void Server_Tables_MTConnect_Current_Checked(object sender, RoutedEventArgs e)
        {
            if (selectedServer != null) selectedServer.server.configuration.Server.Tables.MTConnect.Probe = true;
        }

        private void Server_Tables_MTConnect_Current_Unchecked(object sender, RoutedEventArgs e)
        {
            if (selectedServer != null) selectedServer.server.configuration.Server.Tables.MTConnect.Probe = false;
        }

        #endregion

        #region "Server_Tables_MTConnect_Sample"

        public bool Server_Tables_MTConnect_Sample
        {
            get { return (bool)GetValue(Server_Tables_MTConnect_SampleProperty); }
            set { SetValue(Server_Tables_MTConnect_SampleProperty, value); }
        }

        public static readonly DependencyProperty Server_Tables_MTConnect_SampleProperty =
            DependencyProperty.Register("Server_Tables_MTConnect_Sample", typeof(bool), typeof(ServerManager), new PropertyMetadata(false));

        private void Server_Tables_MTConnect_Sample_Checked(object sender, RoutedEventArgs e)
        {
            if (selectedServer != null) selectedServer.server.configuration.Server.Tables.MTConnect.Probe = true;
        }

        private void Server_Tables_MTConnect_Sample_Unchecked(object sender, RoutedEventArgs e)
        {
            if (selectedServer != null) selectedServer.server.configuration.Server.Tables.MTConnect.Probe = false;
        }

        #endregion

        #endregion

        #endregion

        #endregion

        #region "InstanceTable"

        public bool ConfigurationFound_InstanceTable
        {
            get { return (bool)GetValue(ConfigurationFound_InstanceTableProperty); }
            set { SetValue(ConfigurationFound_InstanceTableProperty, value); }
        }

        public static readonly DependencyProperty ConfigurationFound_InstanceTableProperty =
            DependencyProperty.Register("ConfigurationFound_InstanceTable", typeof(bool), typeof(ServerManager), new PropertyMetadata(false));


        TH_InstanceTable.InstanceTable.InstanceConfiguration instanceConfiguration;


        void ReadDeviceSettings_InstanceTable()
        {

            instanceConfiguration = null;

            var obj = selectedServer.server.configuration.CustomClasses.Find(x => x.GetType().ToString().ToLower() == "th_instancetable.instancetable+instanceconfiguration");

            if (obj != null)
            {
                instanceConfiguration = (TH_InstanceTable.InstanceTable.InstanceConfiguration)obj;

                ReadDeviceSettings_InstanceTable_DataItems();
            }

            ConfigurationFound_InstanceTable = obj != null;

        }

        #region "DataItems"

        void ReadDeviceSettings_InstanceTable_DataItems()
        {

            InstanceTable_DataItems_Conditions = instanceConfiguration.DataItems.Conditions;
            InstanceTable_DataItems_Events = instanceConfiguration.DataItems.Events;
            InstanceTable_DataItems_Samples = instanceConfiguration.DataItems.Samples;




            //InstanceTable_DataItems_Omit_LIST.Items.Clear();
            //foreach (string omitvar in instanceConfiguration.DataItems.Omit) InstanceTable_DataItems_Omit_LIST.Items.Add(omitvar);

        }

        #region "InstanceTable_DataItems_Conditions"

        public bool InstanceTable_DataItems_Conditions
        {
            get { return (bool)GetValue(InstanceTable_DataItems_ConditionsProperty); }
            set { SetValue(InstanceTable_DataItems_ConditionsProperty, value); }
        }

        public static readonly DependencyProperty InstanceTable_DataItems_ConditionsProperty =
            DependencyProperty.Register("InstanceTable_DataItems_Conditions", typeof(bool), typeof(ServerManager), new PropertyMetadata(false));

        private void InstanceTable_DataItems_Conditions_Checked(object sender, RoutedEventArgs e)
        {
            if (selectedServer != null && instanceConfiguration != null) instanceConfiguration.DataItems.Conditions = true;
        }

        private void InstanceTable_DataItems_Conditions_Unchecked(object sender, RoutedEventArgs e)
        {
            if (selectedServer != null && instanceConfiguration != null) instanceConfiguration.DataItems.Conditions = false;
        }

        #endregion

        #region "InstanceTable_DataItems_Events"

        public bool InstanceTable_DataItems_Events
        {
            get { return (bool)GetValue(InstanceTable_DataItems_EventsProperty); }
            set { SetValue(InstanceTable_DataItems_EventsProperty, value); }
        }

        public static readonly DependencyProperty InstanceTable_DataItems_EventsProperty =
            DependencyProperty.Register("InstanceTable_DataItems_Events", typeof(bool), typeof(ServerManager), new PropertyMetadata(false));

        private void InstanceTable_DataItems_Events_Checked(object sender, RoutedEventArgs e)
        {
            if (selectedServer != null && instanceConfiguration != null) instanceConfiguration.DataItems.Events = true;
        }

        private void InstanceTable_DataItems_Events_Unchecked(object sender, RoutedEventArgs e)
        {
            if (selectedServer != null && instanceConfiguration != null) instanceConfiguration.DataItems.Events = false;
        }

        #endregion

        #region "InstanceTable_DataItems_Samples"

        public bool InstanceTable_DataItems_Samples
        {
            get { return (bool)GetValue(InstanceTable_DataItems_SamplesProperty); }
            set { SetValue(InstanceTable_DataItems_SamplesProperty, value); }
        }

        public static readonly DependencyProperty InstanceTable_DataItems_SamplesProperty =
            DependencyProperty.Register("InstanceTable_DataItems_Samples", typeof(bool), typeof(ServerManager), new PropertyMetadata(false));

        private void InstanceTable_DataItems_Samples_Checked(object sender, RoutedEventArgs e)
        {
            if (selectedServer != null && instanceConfiguration != null) instanceConfiguration.DataItems.Samples = true;
        }

        private void InstanceTable_DataItems_Samples_Unchecked(object sender, RoutedEventArgs e)
        {
            if (selectedServer != null && instanceConfiguration != null) instanceConfiguration.DataItems.Samples = false;
        }

        #endregion

        #endregion

        #endregion

        #endregion

        #region "Tables"

        ObservableCollection<TH_WPF.ListButton> tables_list;
        public ObservableCollection<TH_WPF.ListButton> Tables_List
        {
            get
            {
                if (tables_list == null)
                    tables_list = new ObservableCollection<TH_WPF.ListButton>();
                return tables_list;
            }

            set
            {
                tables_list = value;
            }
        }

        public DataView Tables_DataView
        {
            get { return (DataView)GetValue(Tables_DataViewProperty); }
            set { SetValue(Tables_DataViewProperty, value); }
        }

        public static readonly DependencyProperty Tables_DataViewProperty =
            DependencyProperty.Register("Tables_DataView", typeof(DataView), typeof(ServerManager), new PropertyMetadata(null));

        const System.Windows.Threading.DispatcherPriority Priority = System.Windows.Threading.DispatcherPriority.Background;

        void LoadTablesList(Configuration config)
        {
            Tables_List.Clear();

            string[] tableNames = Global.Table_List(config.SQL);

            foreach (string tableName in tableNames)
            {
                TH_WPF.ListButton lb = new TH_WPF.ListButton();
                lb.Text = tableName;
                lb.Selected += lb_Selected;
                Tables_List.Add(lb);
            }
        }

        void lb_Selected(TH_WPF.ListButton LB)
        {
            foreach (TH_WPF.ListButton olb in Tables_List.OfType<TH_WPF.ListButton>()) if (olb != LB) olb.IsSelected = false;
            LB.IsSelected = true;

            this.Dispatcher.BeginInvoke(new Action<string, Configuration>(LoadTable), Priority, new object[] { LB.Text, selectedServer.server.configuration });
            //LoadTable(LB.Text, selectedServer.server.configuration);
        }

        void bt_Click(object sender, RoutedEventArgs e)
        {
            Button bt = (Button)sender;

            LoadTable(bt.Content.ToString(), selectedServer.server.configuration);
        }

        void LoadTable(string tableName, Configuration config)
        {

            DataTable dt = Global.Table_Get(config.SQL, tableName, "LIMIT 1000");

            if (dt != null) Tables_DataView = dt.AsDataView();

        }

        #endregion

        private void TH_TabHeader_Clicked(Controls.TH_TabHeader header)
        {

            if (header.TabParent != null)
            {
                Main_TABCONTROL.SelectedItem = header.TabParent;
            }

        }


    }
}
