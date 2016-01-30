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

using System.Threading;

using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

using System.IO;
using System.Collections.ObjectModel;
using System.Xml;
using System.Data;

using TH_Configuration;
using TH_Database;
using TH_Global;
using TH_Global.Functions;
using TH_Plugins_Server;
using TH_UserManagement.Management;
using TH_WPF;

using TH_DeviceManager.Controls;

namespace TH_DeviceManager
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class DeviceManager : UserControl
    {
        public DeviceManager()
        {
            init();
        }

        public DeviceManager(DeviceManagerType type)
        {
            init();

            ManagerType = type;

            if (type == DeviceManagerType.Server) LoadPlugins();

            InitializePages(type);
        }

        void init()
        {
            InitializeComponent();
            DataContext = this;
        }

        public DeviceManagerType ManagerType { get; set; }

        #region "User Login"

        UserConfiguration currentuser;
        public UserConfiguration CurrentUser
        {
            get { return currentuser; }
            set
            {
                currentuser = value;

                if (currentuser != null) LoggedIn = true;
                else LoggedIn = false;

                LoadDevices();

                AddDevice_Initialize();
                CopyDevice_Initialize();
            }
        }

        public bool LoggedIn
        {
            get { return (bool)GetValue(LoggedInProperty); }
            set { SetValue(LoggedInProperty, value); }
        }

        public static readonly DependencyProperty LoggedInProperty =
            DependencyProperty.Register("LoggedIn", typeof(bool), typeof(DeviceManager), new PropertyMetadata(false));


        Database_Settings userDatabaseSettings;
        public Database_Settings UserDatabaseSettings
        {
            get { return (Database_Settings)GetValue(UserDatabaseSettingsProperty); }
            set 
            { 
                SetValue(UserDatabaseSettingsProperty, value);
                userDatabaseSettings = value;

                if (descriptionPage != null) descriptionPage.userDatabaseSettings = userDatabaseSettings;
            }
        }

        public static readonly DependencyProperty UserDatabaseSettingsProperty =
            DependencyProperty.Register("UserDatabaseSettings", typeof(Database_Settings), typeof(DeviceManager), new PropertyMetadata(null, new PropertyChangedCallback(UserDatabaseSettings_PropertyChanged)));

        private static void UserDatabaseSettings_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ((DeviceManager)obj).userDatabaseSettings = e.NewValue as Database_Settings;
        }

        #endregion


        const System.Windows.Threading.DispatcherPriority background = System.Windows.Threading.DispatcherPriority.Background;
        const System.Windows.Threading.DispatcherPriority contextidle = System.Windows.Threading.DispatcherPriority.ContextIdle;


        #region "Device Management"

        Configuration selecteddevice;
        public Configuration SelectedDevice
        {
            get { return selecteddevice; }
            set
            {
                selecteddevice = value;
            }
        }

        DeviceButton selecteddevicebutton;
        public DeviceButton SelectedDeviceButton
        {
            get { return selecteddevicebutton; }
            set
            {
                selecteddevicebutton = value;
            }
        }

        DataTable configurationtable;
        public DataTable ConfigurationTable
        {
            get { return configurationtable; }
            set
            {
                configurationtable = value;
            }
        }

        #region "Load Devices"

        public bool DevicesLoading
        {
            get { return (bool)GetValue(DevicesLoadingProperty); }
            set { SetValue(DevicesLoadingProperty, value); }
        }

        public static readonly DependencyProperty DevicesLoadingProperty =
            DependencyProperty.Register("DevicesLoading", typeof(bool), typeof(DeviceManager), new PropertyMetadata(false));


        #region "Configuration Files"

        List<Configuration> configurations;

        static List<Configuration> ReadConfigurationFile()
        {
            List<Configuration> Result = new List<Configuration>();

            string configPath;

            string localPath = AppDomain.CurrentDomain.BaseDirectory + "Configuration.Xml";
            string systemPath = TH_Global.FileLocations.TrakHound + @"\" + "Configuration.Xml";

            // systemPath takes priority (easier for user to navigate to)
            if (File.Exists(systemPath)) configPath = systemPath;
            else configPath = localPath;

            Logger.Log(configPath);

            if (System.IO.File.Exists(configPath))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(configPath);

                foreach (XmlNode Node in doc.DocumentElement.ChildNodes)
                {
                    if (Node.NodeType == XmlNodeType.Element)
                    {
                        switch (Node.Name.ToLower())
                        {
                            case "devices":
                                foreach (XmlNode ChildNode in Node.ChildNodes)
                                {
                                    if (ChildNode.NodeType == XmlNodeType.Element)
                                    {
                                        switch (ChildNode.Name.ToLower())
                                        {
                                            case "device":

                                                Configuration device = ProcessDevice(ChildNode);
                                                if (device != null)
                                                {
                                                    Result.Add(device);
                                                }
                                                break;
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }

                Logger.Log("Configuration File Successfully Read From : " + configPath);
            }
            else Logger.Log("Configuration File Not Found : " + configPath);

            return Result;
        }

        static Configuration ProcessDevice(XmlNode node)
        {
            Configuration Result = null;

            string configPath = null;

            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.NodeType == XmlNodeType.Element)
                {
                    if (childNode.Name.ToLower() == "configuration_path")
                    {
                        configPath = childNode.InnerText;
                    }
                }
            }

            if (configPath != null)
            {
                configPath = GetConfigurationPath(configPath);

                Logger.Log("Reading Device Configuration File @ '" + configPath + "'");

                if (File.Exists(configPath))
                {
                    Configuration config = new Configuration();
                    config = Configuration.ReadConfigFile(configPath);

                    if (config != null)
                    {
                        Logger.Log("Device Congifuration Read Successfully!");

                        // Initialize Database Configurations
                        Global.Initialize(config.Databases_Client);
                        Global.Initialize(config.Databases_Server);

                        Result = config;
                    }
                    else Logger.Log("Error Occurred While Reading : " + configPath);
                }
                else Logger.Log("Can't find Device Configuration file @ " + configPath);
            }
            else Logger.Log("No Device Congifuration found");

            return Result;

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
                else Logger.Log(path + " Not Found");


                // Check local app Path
                path = AppDomain.CurrentDomain.BaseDirectory + "Configuration Files\\" + original;
                if (File.Exists(path)) return path;
                else Logger.Log(path + " Not Found");

                // if no files exist return null
                return null;
            }
            else return path;
        }

        #endregion


        Thread loaddevices_THREAD;

        public void LoadDevices()
        {
            CurrentPage = null;
            PageListShown = false;
            DevicesLoading = true;
            DeviceListShown = false;
            DeviceList.Clear();

            if (loaddevices_THREAD != null) loaddevices_THREAD.Abort();

            loaddevices_THREAD = new Thread(new ThreadStart(LoadDevices_Worker));
            loaddevices_THREAD.Start();      
        }

        void LoadDevices_Worker()
        {
            List<Configuration> configs = new List<Configuration>();

            if (currentuser != null)
            {
                configs = Configurations.GetConfigurationsListForUser(currentuser, userDatabaseSettings);
            }
            // If not logged in Read from File in 'C:\TrakHound\'
            else
            {
                //Configurations = ReadConfigurationFile();
            }

            this.Dispatcher.BeginInvoke(new Action<List<Configuration>>(LoadDevices_GUI), background, new object[] { configs });
        }

        void LoadDevices_GUI(List<Configuration> configs)
        {
            configurations = configs;

            if (configs != null)
            {
               var orderedConfigs = configs.OrderBy(x => x.Description.Manufacturer).ThenBy(x => x.Description.Description).ThenBy(x => x.Description.Device_ID);

                // Create DevicesList based on Configurations
               foreach (Configuration config in orderedConfigs)
                {
                    this.Dispatcher.BeginInvoke(new Action<Configuration>(AddDeviceButton), background, new object[] { config });
                }
            }

            this.Dispatcher.BeginInvoke(new Action(LoadDevices_Finished), background, null);
        }

        void LoadDevices_Finished()
        {
            DeviceListShown = true;
            DevicesLoading = false;
        }

        #endregion

        #region "Load Configuration"

        void LoadConfiguration()
        {
            if (ConfigurationTable != null)
            {
                if (ConfigurationPages != null)
                {
                    foreach (ConfigurationPage page in ConfigurationPages)
                    {
                        page.LoadConfiguration(ConfigurationTable);
                    }
                }
            }
        }

        #endregion

        #region "Save Configuration"

        private void Save_Clicked(TH_WPF.Button bt)
        {
            bt.Focus();

            if (SelectedDevice != null)
            {
                DataTable dt = Converter.XMLToTable(SelectedDevice.ConfigurationXML);
                dt.TableName = SelectedDevice.TableName;

                Save(dt);
            } 
        }

        Thread save_THREAD;

        public void Save(DataTable dt)
        {
            Saving = true;

            if (dt != null)
            {
                if (ConfigurationPages != null)
                {
                    foreach (ConfigurationPage page in ConfigurationPages)
                    {
                        page.SaveConfiguration(dt);
                    }
                }

                if (save_THREAD != null) save_THREAD.Abort();

                save_THREAD = new Thread(new ParameterizedThreadStart(Save_Worker));
                save_THREAD.Start(dt);
            }
        }

        void Save_Worker(object o)
        {
            bool success = false;

            DataTable dt = (DataTable)o;

            if (dt != null)
            {
                string tablename = null;

                if (dt != null)
                {
                    tablename = dt.TableName;

                    if (currentuser != null)
                    {
                        // Reset Update ID
                        if (ManagerType == DeviceManagerType.Client) Table_Functions.UpdateTableValue(String_Functions.RandomString(20), "/ClientUpdateId", dt);
                        else if (ManagerType == DeviceManagerType.Server) Table_Functions.UpdateTableValue(String_Functions.RandomString(20), "/ServerUpdateId", dt);

                        // Add Unique Id (ONLY if one not already set)
                        if (Table_Functions.GetTableValue("/UniqueId", dt) == null) Table_Functions.UpdateTableValue(String_Functions.RandomString(20), "/UniqueId", dt);

                        // Create backup in temp directory
                        XmlDocument backupXml = Converter.TableToXML(dt);
                        if (backupXml != null)
                        {
                            string temp_filename = currentuser.username + String_Functions.RandomString(20) + ".xml";

                            //string tempdir = FileLocations.TrakHound + @"\temp";
                            //if (!Directory.Exists(tempdir)) Directory.CreateDirectory(tempdir);

                            TH_Global.FileLocations.CreateTempDirectory();

                            string localPath = TH_Global.FileLocations.TrakHoundTemp + @"\" + temp_filename;

                            try { backupXml.Save(localPath); }
                            catch (Exception ex) { Logger.Log("Error during Configuration Xml Backup"); }                
                        }

                        success = Configurations.ClearConfigurationTable(tablename, userDatabaseSettings);
                        if (success) success = Configurations.UpdateConfigurationTable(tablename, dt, userDatabaseSettings);
                    }
                    // If not logged in Save to File in 'C:\TrakHound\'
                    else
                    {

                    }
                }

                ConfigurationTable = dt.Copy();

                XmlDocument xml = Converter.TableToXML(dt);
                if (xml != null)
                {
                    SelectedDevice = Configuration.ReadConfigFile(xml);
                    SelectedDevice.TableName = tablename;

                    if (SelectedDeviceButton != null)
                    {
                        SelectedDeviceButton.Config = SelectedDevice;
                    }
                }
            }

            this.Dispatcher.BeginInvoke(new Action<bool>(Save_Finished), background, new object[] { success });
        }

        void Save_GUI(ConfigurationPage page)
        {
            page.SaveConfiguration(ConfigurationTable);
        }

        void Save_Finished(bool success)
        {
            if (!success) TH_WPF.MessageBox.Show("Device did not save correctly. Try Again." + Environment.NewLine + @"A backup of the Device has been created in the 'C:\TrakHound\Temp directory'");

            if (SelectedDevice != null) SelectDevice(SelectedDevice);

            SaveNeeded = false;
            Saving = false;
        }

        #endregion

        #region "Remove Device"

        class RemoveDevice_Info
        {
            public DeviceButton bt { get; set; }
            public bool success { get; set; }
        }

        void RemoveDevice(DeviceButton bt)
        {
            bool? result = TH_WPF.MessageBox.Show("Are you sure you want to permanently remove this device?", "Remove Device", TH_WPF.MessageBoxButtons.YesNo);
            if (result == true)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(RemoveDevice_Worker), bt);
            }
        }

        void RemoveDevice_Worker(object o)
        {
            RemoveDevice_Info info = new RemoveDevice_Info();

            if (o != null)
            {
                DeviceButton bt = (DeviceButton)o;

                info.bt = bt;

                if (bt.Config != null)
                {
                    if (bt.Config.TableName != null)
                    {
                        info.success = Configurations.RemoveConfigurationTable(bt.Config.TableName, userDatabaseSettings);
                    }
                }
            }

            this.Dispatcher.BeginInvoke(new Action<RemoveDevice_Info>(RemoveDevice_Finshed), priority, new object[] { info });
        }

        void RemoveDevice_Finshed(RemoveDevice_Info info)
        {
            if (info.success)
            {
                if (info.bt != null)
                {
                    DeviceButton bt = info.bt;

                    if (bt.Parent != null)
                    {
                        if (bt.Parent.GetType() == typeof(ListButton))
                        {
                            ListButton lb = (ListButton)bt.Parent;

                            if (DeviceList.Contains(lb)) DeviceList.Remove(lb);
                        }
                    }
                }
            }
            else
            {
                TH_WPF.MessageBox.Show("An error occured while attempting to Remove Device. Please try again.", "Remove Device Error", MessageBoxButtons.Ok);
                LoadDevices();
            }
        }

        #endregion

        #region "Add Device"

        private void AddDevice_GRID_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            foreach (TH_WPF.ListButton lb in DeviceList.OfType<TH_WPF.ListButton>()) lb.IsSelected = false;

            SelectedDevice = null;
            selectedPageIndex = 0;

            AddDevice();
        }

        Pages.AddDevice.Page addPage;

        void AddDevice_Initialize()
        {
            addPage = new Pages.AddDevice.Page();
            addPage.deviceManager = this;
            addPage.DeviceAdded += page_DeviceAdded;
            addPage.currentuser = CurrentUser;
            addPage.userDatabaseSettings = userDatabaseSettings;
            addPage.LoadCatalog();
        }

        public void AddDevice()
        {
            PageListShown = false;
            ToolbarShown = false;

            if (CurrentPage != null)
            {
                if (CurrentPage.GetType() != typeof(Pages.AddDevice.Page))
                {
                    CurrentPage = addPage;
                }
            }
            else CurrentPage = addPage;
        }

        void page_DeviceAdded(Configuration config)
        {
            AddDeviceButton(config);
        }

        #endregion

        #region "Copy Device"

        Pages.CopyDevice.Page copyPage;

        void CopyDevice_Initialize()
        {
            copyPage = new Pages.CopyDevice.Page();
            //copyPage.deviceManager = this;
            //copyPage.DeviceAdded += page_DeviceAdded;
            copyPage.currentuser = CurrentUser;
            copyPage.userDatabaseSettings = userDatabaseSettings;
            //copyPage.LoadCatalog();
        }

        public void CopyDevice(Configuration config)
        {
            PageListShown = false;
            ToolbarShown = false;

            copyPage.LoadConfiguration(config);

            if (CurrentPage != null)
            {
                if (CurrentPage.GetType() != typeof(Pages.CopyDevice.Page))
                {
                    CurrentPage = copyPage;
                }
            }
            else CurrentPage = copyPage;
        }

        //Thread CopyDevice_THREAD;

        //void CopyDevice(Configuration config)
        //{
        //    bool? result = TH_WPF.MessageBox.Show("Create a copy of this device?", "Copy Device", TH_WPF.MessageBoxButtons.YesNo);
        //    if (result == true)
        //    {
        //        if (CopyDevice_THREAD != null) CopyDevice_THREAD.Abort();

        //        CopyDevice_THREAD = new Thread(new ParameterizedThreadStart(CopyDevice_Worker));
        //        CopyDevice_THREAD.Start(config);
        //    }
        //}

        //void CopyDevice_Worker(object o)
        //{
        //    bool success = false;

        //    if (o != null)
        //    {
        //        Configuration config = (Configuration)o;

        //        if (currentuser != null)
        //        {
        //            success = Configurations.AddConfigurationToUser(currentuser, config, userDatabaseSettings);
        //        }
        //        else
        //        {
        //            success = false;
        //        }

        //        this.Dispatcher.BeginInvoke(new Action<bool, Configuration>(CopyDevice_GUI), priority, new object[] { success, config });
        //    }
        //}

        //void CopyDevice_GUI(bool success, Configuration config)
        //{
        //    if (success) AddDeviceButton(config);
        //    else
        //    {
        //        TH_WPF.MessageBox.Show("Error during Device Copy. Please try again", "Device Copy Error", MessageBoxButtons.Ok);
        //    }
        //}

        #endregion

        #region "Device Buttons"

        public bool DeviceListShown
        {
            get { return (bool)GetValue(DeviceListShownProperty); }
            set { SetValue(DeviceListShownProperty, value); }
        }

        public static readonly DependencyProperty DeviceListShownProperty =
            DependencyProperty.Register("DeviceListShown", typeof(bool), typeof(DeviceManager), new PropertyMetadata(false));


        ObservableCollection<ListButton> devicelist;
        public ObservableCollection<ListButton> DeviceList
        {
            get
            {
                if (devicelist == null)
                    devicelist = new ObservableCollection<ListButton>();
                return devicelist;
            }

            set
            {
                devicelist = value;
            }
        }

        void AddDeviceButton(string uniqueId, string tableName)
        {
            ListButton lb;

            int index = DeviceList.ToList().FindIndex(x => GetDeviceButtonUniqueId(x) == uniqueId);
            if (index >= 0)
            {

            }
            else
            {
                Controls.DeviceButton db = new Controls.DeviceButton();
                db.devicemanager = this;

                // Create a temporary Configuration to hold the button's place in the list
                Configuration config = Configuration.CreateBlank();
                config.UniqueId = uniqueId;
                config.TableName = tableName;
                db.Config = config;

                db.Enabled += db_Enabled;
                db.Disabled += db_Disabled;
                db.RemoveClicked += db_RemoveClicked;
                db.ShareClicked += db_ShareClicked;
                db.CopyClicked += db_CopyClicked;
                db.Clicked += db_Clicked;

                lb = new ListButton();
                lb.ButtonContent = db;
                lb.ShowImage = false;
                lb.Selected += lb_Device_Selected;

                db.Parent = lb;

                DeviceList.Add(lb);
            }   
        }

        void AddDeviceButton(Configuration config)
        {
            ListButton lb;

            int index = DeviceList.ToList().FindIndex(x => GetDeviceButtonUniqueId(x) == config.UniqueId);
            if (index >= 0)
            {
                lb = DeviceList[index];
                Controls.DeviceButton db = lb.ButtonContent as Controls.DeviceButton;
                if (db != null)
                {
                    db.Config = config;
                }
            }
            else
            {
                Controls.DeviceButton db = new Controls.DeviceButton();
                db.devicemanager = this;

                db.Config = config;

                db.Enabled += db_Enabled;
                db.Disabled += db_Disabled;
                db.RemoveClicked += db_RemoveClicked;
                db.ShareClicked += db_ShareClicked;
                db.CopyClicked += db_CopyClicked;
                db.Clicked += db_Clicked;

                lb = new ListButton();
                lb.ButtonContent = db;
                lb.ShowImage = false;
                lb.Selected += lb_Device_Selected;

                db.Parent = lb;

                DeviceList.Add(lb);
            }   
        }

        string GetDeviceButtonUniqueId(ListButton lb)
        {
            string result = null;

            if (lb != null)
            {
                if (lb.ButtonContent != null)
                {
                    Configuration config = lb.ButtonContent as Configuration;
                    if (config != null)
                    {
                        result = config.UniqueId;
                    }
                }
            }

            return result;
        }

        void db_Enabled(DeviceButton bt)
        {
            if (bt.Config != null)
            {
                if (bt.Config.TableName != null) EnableDevice(bt, bt.Config.TableName);
            }
        }

        void db_Disabled(DeviceButton bt)
        {
            if (bt.Config != null)
            {
                if (bt.Config.TableName != null) DisableDevice(bt, bt.Config.TableName);
            }
        }

        void db_RemoveClicked(DeviceButton bt)
        {
            if (bt.Config != null)
            {
                RemoveDevice(bt);
            }

            if (DeviceList.Count == 0) AddDevice();
        }

        void db_ShareClicked(DeviceButton bt)
        {
            PageListShown = false;

            LoadAddSharePage(bt);

            ToolbarShown = false;
        }

        void db_CopyClicked(DeviceButton bt)
        {
            if (bt.Config != null)
            {
                CopyDevice(bt.Config);
                //CopyDevice(bt.Config);
            }

            db_Clicked(bt);
        }

        void db_Clicked(DeviceButton bt)
        {
            if (bt.Parent != null)
            {
                if (bt.Parent.GetType() == typeof(ListButton))
                {
                    ListButton lb = (ListButton)bt.Parent;

                    SelectedDeviceButton = bt;

                    lb_Device_Selected(lb);
                }
            }
        }


        #region "Enable Device"

        class EnableDevice_Info
        {
            public DeviceButton bt { get; set; }
            public string tablename { get; set; }
            public bool success { get; set; }
        }

        void EnableDevice(DeviceButton bt, string tableName)
        {
            bt.EnableLoading = true;

            EnableDevice_Info info = new EnableDevice_Info();
            info.bt = bt;
            info.tablename = tableName;

            ThreadPool.QueueUserWorkItem(new WaitCallback(EnableDevice_Worker), info);
        }

        void EnableDevice_Worker(object o)
        {
            if (o != null)
            {
                EnableDevice_Info info = (EnableDevice_Info)o;

                if (ManagerType == DeviceManagerType.Client) info.success = Configurations.UpdateConfigurationTable("/ClientEnabled", "True", info.tablename, userDatabaseSettings);
                else if (ManagerType == DeviceManagerType.Server) info.success = Configurations.UpdateConfigurationTable("/ServerEnabled", "True", info.tablename, userDatabaseSettings);

                // Reset Update ID
                if (info.success)
                {
                    if (ManagerType == DeviceManagerType.Client)
                    {
                        var updateId = String_Functions.RandomString(20);
                        info.success = Configurations.UpdateConfigurationTable("/ClientUpdateId", updateId, info.tablename, userDatabaseSettings);
                        info.bt.Config.ClientUpdateId = updateId;
                        XML_Functions.SetInnerText(info.bt.Config.ConfigurationXML, "ClientUpdateId", updateId);
                    }
                    else if (ManagerType == DeviceManagerType.Server)
                    {
                        var updateId = String_Functions.RandomString(20);
                        info.success = Configurations.UpdateConfigurationTable("/ServerUpdateId", updateId, info.tablename, userDatabaseSettings);
                        info.bt.Config.ClientUpdateId = updateId;
                        XML_Functions.SetInnerText(info.bt.Config.ConfigurationXML, "ServerUpdateId", updateId);
                        //info.success = Configurations.UpdateConfigurationTable("/ServerUpdateId", String_Functions.RandomString(20), info.tablename, userDatabaseSettings);
                    }
                }

                this.Dispatcher.BeginInvoke(new Action<EnableDevice_Info>(EnableDevice_Finished), priority, new object[] { info });
            }
        }

        void EnableDevice_Finished(EnableDevice_Info info)
        {
            if (info.bt != null)
            {
                if (info.success)
                {
                    if (ManagerType == DeviceManagerType.Client)
                    {
                        info.bt.Config.ClientEnabled = true;
                        XML_Functions.SetInnerText(info.bt.Config.ConfigurationXML, "ClientEnabled", "true");
                    }
                    else if (ManagerType == DeviceManagerType.Server)
                    {
                        info.bt.Config.ServerEnabled = true;
                        XML_Functions.SetInnerText(info.bt.Config.ConfigurationXML, "ServerEnabled", "true");
                    }
                    info.bt.DeviceEnabled = true;
                }

                info.bt.EnableLoading = false;
            }
        }

        #endregion

        #region "Disable Device"

        void DisableDevice(DeviceButton bt, string tableName)
        {
            bt.EnableLoading = true;

            EnableDevice_Info info = new EnableDevice_Info();
            info.bt = bt;
            info.tablename = tableName;

            ThreadPool.QueueUserWorkItem(new WaitCallback(DisableDevice_Worker), info);
        }

        void DisableDevice_Worker(object o)
        {
            if (o != null)
            {
                EnableDevice_Info info = (EnableDevice_Info)o;

                if (ManagerType == DeviceManagerType.Client) info.success = Configurations.UpdateConfigurationTable("/ClientEnabled", "False", info.tablename, userDatabaseSettings);
                else if (ManagerType == DeviceManagerType.Server) info.success = Configurations.UpdateConfigurationTable("/ServerEnabled", "False", info.tablename, userDatabaseSettings);

                // Reset Update ID
                if (info.success)
                {
                    if (ManagerType == DeviceManagerType.Client) info.success = Configurations.UpdateConfigurationTable("/ClientUpdateId", String_Functions.RandomString(20), info.tablename, userDatabaseSettings);
                    else if (ManagerType == DeviceManagerType.Server) info.success = Configurations.UpdateConfigurationTable("/ServerUpdateId", String_Functions.RandomString(20), info.tablename, userDatabaseSettings);
                }

                this.Dispatcher.BeginInvoke(new Action<EnableDevice_Info>(DisableDevice_Finished), priority, new object[] { info });
            }
        }

        void DisableDevice_Finished(EnableDevice_Info info)
        {
            if (info.bt != null)
            {
                if (info.success && info.bt != null)
                {
                    if (ManagerType == DeviceManagerType.Client)
                    {
                        info.bt.Config.ClientEnabled = false;
                        XML_Functions.SetInnerText(info.bt.Config.ConfigurationXML, "ClientEnabled", "false");
                    }
                    else if (ManagerType == DeviceManagerType.Server)
                    {
                        info.bt.Config.ServerEnabled = false;
                        XML_Functions.SetInnerText(info.bt.Config.ConfigurationXML, "ServerEnabled", "false");
                    }
                    info.bt.DeviceEnabled = false;
                }

                info.bt.EnableLoading = false;
            }
        }

        #endregion


        #endregion

        #region "Select Device"

        public bool DeviceLoading
        {
            get { return (bool)GetValue(DeviceLoadingProperty); }
            set { SetValue(DeviceLoadingProperty, value); }
        }

        public static readonly DependencyProperty DeviceLoadingProperty =
            DependencyProperty.Register("DeviceLoading", typeof(bool), typeof(DeviceManager), new PropertyMetadata(false));

        
        Thread selectDevice_THREAD;

        void lb_Device_Selected(TH_WPF.ListButton lb)
        {
            Controls.DeviceButton db = (Controls.DeviceButton)lb.ButtonContent;
            if (db != null)
            {
                if (db.Config != null)
                {
                    if (SelectedDevice != db.Config)
                    {
                        SelectedDevice = db.Config;

                        SelectDevice(db.Config);
                    }
                }
            }

            foreach (TH_WPF.ListButton olb in DeviceList.OfType<TH_WPF.ListButton>()) if (olb != lb) olb.IsSelected = false;
            lb.IsSelected = true;
        }

        void SelectDevice(Configuration config)
        {
            //if (SaveNeeded)
            //{
            //    bool? save = TH_WPF.MessageBox.Show("Do you want to Save changes?", "Save Changed", MessageBoxButtons.YesNo);
            //    if (save == true)
            //    {

            //    }
            //}

            if (config != null)
            {
                DeviceLoading = true;

                if (selectDevice_THREAD != null) selectDevice_THREAD.Abort();

                selectDevice_THREAD = new Thread(new ParameterizedThreadStart(SelectDevice_Worker));
                selectDevice_THREAD.Start(config);
            }
        }

        void SelectDevice_Worker(object o)
        {
            Configuration config = (Configuration)o;

            DataTable dt = TH_Configuration.Converter.XMLToTable(config.ConfigurationXML);
            if (dt != null)
            {
                dt.TableName = config.TableName;

                if (ConfigurationPages != null)
                {
                    foreach (ConfigurationPage page in ConfigurationPages)
                    {
                        this.Dispatcher.BeginInvoke(new Action<DataTable, ConfigurationPage>(SelectDevice_GUI), background, new object[] { dt, page });
                    }
                }
            }

            this.Dispatcher.BeginInvoke(new Action<DataTable>(SelectDevice_Finished), background, new object[] { dt });
        }

        void SelectDevice_GUI(DataTable dt, ConfigurationPage page)
        {
            this.Dispatcher.BeginInvoke(new Action<DataTable>(page.LoadConfiguration), contextidle, new object[] { dt });
        }

        void SelectDevice_Finished(DataTable dt)
        {
            ConfigurationTable = dt;

            if (PageList.Count > 0)
            {
                if (PageList.Count > selectedPageIndex) Page_Selected((ListButton)PageList[selectedPageIndex]);
                else Page_Selected((ListButton)PageList[0]);
            }
            
            DeviceLoading = false;
            if (!PageListShown) PageListShown = true;
            SaveNeeded = false;
        }

        #endregion

        void LoadAddSharePage(DeviceButton bt)
        {
            if (bt.Parent != null)
            {
                if (bt.Parent.GetType() == typeof(ListButton))
                {
                    ListButton lb = (ListButton)bt.Parent;

                    foreach (TH_WPF.ListButton olb in DeviceList.OfType<TH_WPF.ListButton>()) if (olb != lb) olb.IsSelected = false;
                    lb.IsSelected = true;

                    SelectedDevice = null;
                    selectedPageIndex = 0;

                    if (bt.Config != null)
                    {
                        Pages.AddShare.Page page = new Pages.AddShare.Page();
                        page.devicemanager = this;
                        page.currentuser = CurrentUser;
                        page.LoadConfiguration(bt.Config);
                        page.configurationtable = ConfigurationTable;
                        CurrentPage = page;
                    }
                }
            }
        }

        #endregion

        #region "Pages"

        public object CurrentPage
        {
            get { return (object)GetValue(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        public static readonly DependencyProperty CurrentPageProperty =
            DependencyProperty.Register("CurrentPage", typeof(object), typeof(DeviceManager), new PropertyMetadata(null));


        ObservableCollection<object> pagelist;
        public ObservableCollection<object> PageList
        {
            get
            {
                if (pagelist == null)
                    pagelist = new ObservableCollection<object>();
                return pagelist;
            }

            set
            {
                pagelist = value;
            }
        }

        List<ConfigurationPage> ConfigurationPages;

        int selectedPageIndex = 0;

        Pages.Description.Page descriptionPage;

        void InitializePages(DeviceManagerType type)
        {
            PageList.Clear();

            bool useTrakHoundCloud = false;

            ConfigurationPages = new List<ConfigurationPage>();

            //ConfigurationPages.Add(new Pages.General.Page());
            descriptionPage = new Pages.Description.Page();
            //description.userDatabaseSettings = UserDatabaseSettings;
            ConfigurationPages.Add(descriptionPage);

            // Agent
            if (type == DeviceManagerType.Server || useTrakHoundCloud) ConfigurationPages.Add(new Pages.Agent.Page());

            // Databases
            if (type == DeviceManagerType.Server || !useTrakHoundCloud) ConfigurationPages.Add(new Pages.Databases.Page());

            // Load configuration pages from plugins
            if (type == DeviceManagerType.Server) ConfigurationPages.AddRange(AddConfigurationPageButtons(Plugins));

            // Create PageItem and add to PageList
            foreach (ConfigurationPage page in ConfigurationPages)
            {
                if (ManagerType == DeviceManagerType.Client) page.PageType = TH_Plugins_Server.Page_Type.Client;
                else if (ManagerType == DeviceManagerType.Server) page.PageType = TH_Plugins_Server.Page_Type.Server;

                this.Dispatcher.BeginInvoke(new Action<ConfigurationPage>(AddPageButton), priority, new object[] { page });
            }
        }

        void AddPageButton(ConfigurationPage page)
        {
            page.SettingChanged += page_SettingChanged;

            PageItem item = new PageItem();
            item.Text = page.PageName;
            item.Clicked += item_Clicked;

            if (page.Image != null) item.Image = page.Image;
            else item.Image = new BitmapImage(new Uri("pack://application:,,,/TH_DeviceManager;component/Resources/Plug_01.png"));

            ListButton bt = new ListButton();
            bt.ButtonContent = item;
            bt.ShowImage = false;
            bt.Selected += Page_Selected;
            bt.DataObject = page;
            bt.Height = 100;
            bt.Width = 100;
            bt.MinWidth = 100;

            item.Parent = bt;

            PageList.Add(bt);
        }

        void item_Clicked(PageItem item)
        {
            if (item.Parent != null)
            {
                if (item.Parent.GetType() == typeof(ListButton))
                {
                    ListButton lb = (ListButton)item.Parent;
                    Page_Selected(lb);
                }
            }
        }

        void Page_Selected(ListButton lb)
        {
            foreach (ListButton olb in PageList) if (olb != lb) olb.IsSelected = false;
            lb.IsSelected = true;

            selectedPageIndex = PageList.IndexOf(lb);

            if (lb.DataObject != null)
            {
                if (CurrentPage != null)
                {
                    if (CurrentPage.GetType() != lb.DataObject.GetType())
                    {
                        CurrentPage = lb.DataObject;
                    }
                }
                else CurrentPage = lb.DataObject;
            }
        }


        public bool SaveNeeded
        {
            get { return (bool)GetValue(SaveNeededProperty); }
            set { SetValue(SaveNeededProperty, value); }
        }

        public static readonly DependencyProperty SaveNeededProperty =
            DependencyProperty.Register("SaveNeeded", typeof(bool), typeof(DeviceManager), new PropertyMetadata(false));


        void page_SettingChanged(string name, string oldVal, string newVal)
        {
            SaveNeeded = true;
        }

        private void Restore_Clicked(TH_WPF.Button bt)
        {
            SelectDevice(SelectedDevice);
        }


        void PageItem_Clicked(object data)
        {
            if (data != null)
            {
                if (CurrentPage != null)
                {
                    if (CurrentPage.GetType() != data.GetType())
                    {
                        CurrentPage = data;
                    }
                }
                else CurrentPage = data;

                ToolbarShown = true;
            }
        }

        List<ConfigurationPage> AddConfigurationPageButtons(List<IServerPlugin> plugins)
        {
            List<ConfigurationPage> result = new List<ConfigurationPage>();

            foreach (IServerPlugin plugin in plugins)
            {
                try
                {
                    Type config_type = plugin.Config_Page;
                    object o = Activator.CreateInstance(config_type);

                    ConfigurationPage page = (ConfigurationPage)o;

                    result.Add(page);
                }
                catch (Exception ex) { Logger.Log("AddConfigurationPageButtons() :: Exception :: " + ex.Message); }
            }

            return result;
        }

        void AddConfigurationPageButton(IServerPlugin tp)
        {
            if (tp != null)
            {
                Type config_type = tp.Config_Page;
                object o = Activator.CreateInstance(config_type);
                ConfigurationPage page = (ConfigurationPage)o;

                PageItem item = new PageItem();
                item.Text = page.PageName;

                if (page.Image != null) item.Image = page.Image;
                else item.Image = new BitmapImage(new Uri("pack://application:,,,/TH_DeviceManager;component/Resources/Plug_01.png"));

                PageList.Add(item);
            }
        }

        #endregion

        #region "PlugIns"

        IEnumerable<Lazy<IServerPlugin>> plugins { get; set; }

        public List<IServerPlugin> Plugins { get; set; }

        Plugs PLUGS;

        class Plugs
        {
            [ImportMany(typeof(IServerPlugin))]
            public IEnumerable<Lazy<IServerPlugin>> Plugins { get; set; }
        }

        void LoadPlugins()
        {
            string plugin_rootpath = FileLocations.Plugins + @"\Server";

            if (!Directory.Exists(plugin_rootpath)) Directory.CreateDirectory(plugin_rootpath);

            Plugins = new List<IServerPlugin>();

            string pluginsPath;

            // Load from System Directory first (easier for user to navigate to 'C:\TrakHound\')
            pluginsPath = TH_Global.FileLocations.Plugins + @"\Server\";
            if (Directory.Exists(pluginsPath)) LoadPlugins(pluginsPath);

            // Load from App root Directory (doesn't overwrite plugins found in System Directory)
            pluginsPath = AppDomain.CurrentDomain.BaseDirectory;
            if (Directory.Exists(pluginsPath)) LoadPlugins(pluginsPath);

            pluginsPath = AppDomain.CurrentDomain.BaseDirectory + @"Plugins\";
            if (Directory.Exists(pluginsPath)) LoadPlugins(pluginsPath);

        }

        void LoadPlugins(string Path)
        {
            Logger.Log("Searching for Server Plugins in '" + Path + "'");
            if (Directory.Exists(Path))
            {
                try
                {
                    PLUGS = new Plugs();

                    var PageCatalog = new DirectoryCatalog(Path);
                    var PageContainer = new CompositionContainer(PageCatalog);
                    PageContainer.SatisfyImportsOnce(PLUGS);

                    plugins = PLUGS.Plugins;

                    foreach (Lazy<IServerPlugin> lplugin in plugins)
                    {
                        IServerPlugin plugin = lplugin.Value;

                        if (Plugins.ToList().Find(x => x.Name.ToLower() == plugin.Name.ToLower()) == null)
                        {
                            Logger.Log(plugin.Name + " : Plugin Found");
                            Plugins.Add(plugin);
                        }
                        else
                        {
                            Logger.Log(plugin.Name + " : Plugin Already Found");
                        }
                    }
                }
                catch (Exception ex) { Logger.Log("LoadPlugins() : Exception : " + ex.Message); }

                // Search Subdirectories
                foreach (string directory in Directory.GetDirectories(Path))
                {
                    LoadPlugins(directory);
                }
            }
            else Logger.Log("Plugins Directory Doesn't Exist (" + Path + ")");
        }


        List<ConfigurationPage> PluginConfigurationPages;

        void ProcessTablePlugins(DataTable dt)
        {
            if (Plugins != null && dt != null)
            {
                foreach (Lazy<IServerPlugin> lplugin in Plugins.ToList())
                {
                    try
                    {
                        IServerPlugin plugin = lplugin.Value;

                        Type config_type = plugin.Config_Page;

                        object o = Activator.CreateInstance(config_type);

                        ConfigurationPage page = (ConfigurationPage)o;
                    }
                    catch (Exception ex) { Logger.Log("Plugin Exception! : " + ex.Message); }
                }
            }
        }

        void PlugIns_Closing()
        {
            if (Plugins != null)
            {
                foreach (Lazy<IServerPlugin> lplugin in Plugins.ToList())
                {
                    try
                    {
                        IServerPlugin plugin = lplugin.Value;
                        plugin.Closing();
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Plugin Exception! : " + ex.Message);
                    }
                }
            }
        }

        #endregion


        public bool Saving
        {
            get { return (bool)GetValue(SavingProperty); }
            set { SetValue(SavingProperty, value); }
        }

        public static readonly DependencyProperty SavingProperty =
            DependencyProperty.Register("Saving", typeof(bool), typeof(DeviceManager), new PropertyMetadata(false));


        public bool PageListShown
        {
            get { return (bool)GetValue(PageListShownProperty); }
            set { SetValue(PageListShownProperty, value); }
        }

        public static readonly DependencyProperty PageListShownProperty =
            DependencyProperty.Register("PageListShown", typeof(bool), typeof(DeviceManager), new PropertyMetadata(false));


        public bool ToolbarShown
        {
            get { return (bool)GetValue(ToolbarShownProperty); }
            set { SetValue(ToolbarShownProperty, value); }
        }

        public static readonly DependencyProperty ToolbarShownProperty =
            DependencyProperty.Register("ToolbarShown", typeof(bool), typeof(DeviceManager), new PropertyMetadata(false));


        private void TableList_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PageListShown = false;
        }


        const System.Windows.Threading.DispatcherPriority priority = System.Windows.Threading.DispatcherPriority.Background;

        private void RefreshDevices_Clicked(TH_WPF.Button bt)
        {
            LoadDevices();
        }

        private void IndexUp_Clicked(TH_WPF.Button bt)
        {
            if (SelectedDevice != null)
            {
                if (SelectedDevice.Index > 0)
                {
                    SetDeviceIndex(SelectedDevice.Index - 1, SelectedDevice.TableName);
                }
            }
        }

        private void IndexDown_Clicked(TH_WPF.Button bt)
        {
            if (SelectedDevice != null)
            {
                if (SelectedDevice.Index < DeviceList.Count - 1)
                {
                    SetDeviceIndex(SelectedDevice.Index + 1, SelectedDevice.TableName);
                }
            }
        }

        #region "Set Device Index"

        class SetDeviceIndex_Info
        {
            public string tablename { get; set; }
            public int index { get; set; }
        }

        Thread deviceindex_THREAD;

        void SetDeviceIndex(int index, string tablename)
        {
            if (tablename != null)
            {
                SetDeviceIndex_Info info = new SetDeviceIndex_Info();
                info.tablename = tablename;
                info.index = index;

                if (deviceindex_THREAD != null) deviceindex_THREAD.Abort();

                deviceindex_THREAD = new Thread(new ParameterizedThreadStart(EnableDevice_Worker));
                deviceindex_THREAD.Start(info);
            }
        }

        void SetDeviceIndex_Worker(object o)
        {
            if (o != null)
            {
                SetDeviceIndex_Info info = (SetDeviceIndex_Info)o;

                Configurations.UpdateConfigurationTable("/Index", info.index.ToString(), info.tablename, userDatabaseSettings);

                // Reset Update ID
                if (ManagerType == DeviceManagerType.Client) Configurations.UpdateConfigurationTable("/ClientUpdateId", String_Functions.RandomString(20), info.tablename, userDatabaseSettings);
                else if (ManagerType == DeviceManagerType.Server) Configurations.UpdateConfigurationTable("/ServerUpdateId", String_Functions.RandomString(20), info.tablename, userDatabaseSettings);
            }
        }

        #endregion

    }

    public enum DeviceManagerType
    {
        Client = 0,
        Server = 1
    }

}
