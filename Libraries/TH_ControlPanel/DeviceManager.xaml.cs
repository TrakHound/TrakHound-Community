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
using TH_PlugIns_Server;
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

                LoadDevices();
            }
        }

        public Database_Settings userDatabaseSettings;

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
                        Console.WriteLine("Device Congifuration Read Successfully!");

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
                if (userDatabaseSettings == null)
                {
                    configs = Remote.Configurations.GetConfigurationsForUser(currentuser);
                }
                else
                {
                    //Configurations = TH_Database.Tables.Users.GetConfigurationsForUser(currentuser, userDatabaseSettings);
                }
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
                configs.OrderBy(x => x.Index);

                // Create DevicesList based on Configurations
                foreach (Configuration config in configs)
                {
                    this.Dispatcher.BeginInvoke(new Action<Configuration>(CreateDeviceButton), background, new object[] { config });
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

        private void Save_Clicked(Button_02 bt)
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

                            string tempdir = FileLocations.TrakHound + @"\temp";
                            if (!Directory.Exists(tempdir)) Directory.CreateDirectory(tempdir);

                            string localPath = tempdir + @"\" + temp_filename;

                            try { backupXml.Save(temp_filename); }
                            catch (Exception ex) { Console.WriteLine("Error during Configuration Xml Backup"); }                
                        }



                        success = Configurations.ClearConfigurationTable(tablename, userDatabaseSettings);
                        if (success) success = Configurations.UpdateConfigurationTable(tablename, dt, userDatabaseSettings);

                        // If DeviceManager is opened as 'Server' then clear the old data so 
                        //if (ManagerType == DeviceManagerType.Server)
                        //{
                        //    success = Configurations.ClearConfigurationTable(tablename, userDatabaseSettings);
                        //    if (success) success = Configurations.UpdateConfigurationTable(tablename, dt, userDatabaseSettings);
                        //}
                        //else
                        //{
                        //    success = Configurations.UpdateConfigurationTable(tablename, dt, userDatabaseSettings);
                        //}
  
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
            if (!success) MessageBox.Show("Device did not save correctly. Try Again." + Environment.NewLine + @"A backup of the Device has been created in the 'C:\TrakHound\Temp directory'");

            if (SelectedDevice != null) SelectDevice(SelectedDevice);

            SaveNeeded = false;
            Saving = false;
        }

        //public void SaveConfiguration(DataTable dt)
        //{
        //    string tablename = null;

        //    if (dt != null)
        //    {
        //        tablename = dt.TableName;

        //        if (currentuser != null)
        //        {
        //            // Reset Update ID
        //            Table_Functions.UpdateTableValue(String_Functions.RandomString(20), "/UpdateId", dt);

        //            // Add Unique Id (ONLY if one not already set)
        //            if (Table_Functions.GetTableValue("/UniqueId", dt) == null) Table_Functions.UpdateTableValue(String_Functions.RandomString(20), "/UniqueId", dt);

        //            Configurations.ClearConfigurationTable(tablename, userDatabaseSettings);

        //            Configurations.UpdateConfigurationTable(tablename, dt, userDatabaseSettings);
        //        }
        //        // If not logged in Save to File in 'C:\TrakHound\'
        //        else
        //        {

        //        }
        //    }

        //    ConfigurationTable = dt.Copy();

        //    XmlDocument xml = Converter.TableToXML(dt);
        //    if (xml != null)
        //    {
        //        SelectedDevice = Configuration.ReadConfigFile(xml);
        //        SelectedDevice.TableName = tablename;

        //        if (SelectedDeviceButton != null)
        //        {
        //            SelectedDeviceButton.Config = SelectedDevice;
        //        }

                
        //    }
        //}
      
        #endregion

        #region "Remove Device"

        bool RemoveDevice(Configuration config)
        {
            bool result = false;

            if (config != null)
            {
                if (config.TableName != null)
                {
                    this.Cursor = Cursors.Wait;
                    if (Configurations.RemoveConfigurationTable(config.TableName, userDatabaseSettings)) result = true;
                    this.Cursor = Cursors.Arrow;
                }
            }

            return result;
        }

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


        void CreateDeviceButton(Configuration config)
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

            ListButton lb = new ListButton();
            lb.ButtonContent = db;
            lb.ShowImage = false;
            lb.Selected += lb_Device_Selected;

            db.Parent = lb;

            DeviceList.Add(lb);
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
                MessageBoxResult result = MessageBox.Show("Are you sure you want to permanently remove this device?", "Remove Device", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                if (result == MessageBoxResult.Yes)
                {
                    if (RemoveDevice(bt.Config))
                    {
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
            }
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

        Thread enable_THREAD;

        void EnableDevice(DeviceButton bt, string tableName)
        {
            bt.EnableLoading = true;

            EnableDevice_Info info = new EnableDevice_Info();
            info.bt = bt;
            info.tablename = tableName;

            if (enable_THREAD != null) enable_THREAD.Abort();

            enable_THREAD = new Thread(new ParameterizedThreadStart(EnableDevice_Worker));
            enable_THREAD.Start(info);
        }

        void EnableDevice_Worker(object o)
        {
            if (o != null)
            {
                EnableDevice_Info info = (EnableDevice_Info)o;

                if (ManagerType == DeviceManagerType.Client) info.success = Remote.Configurations.UpdateConfigurationTable("/ClientEnabled", "True", info.tablename);
                else if (ManagerType == DeviceManagerType.Server) info.success = Remote.Configurations.UpdateConfigurationTable("/ServerEnabled", "True", info.tablename);

                // Reset Update ID
                if (info.success)
                {
                    if (ManagerType == DeviceManagerType.Client) info.success = Remote.Configurations.UpdateConfigurationTable("/ClientUpdateId", String_Functions.RandomString(20), info.tablename);
                    else if (ManagerType == DeviceManagerType.Server) info.success = Remote.Configurations.UpdateConfigurationTable("/ServerUpdateId", String_Functions.RandomString(20), info.tablename);
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
                    if (ManagerType == DeviceManagerType.Client) info.bt.Config.ClientEnabled = true;
                    if (ManagerType == DeviceManagerType.Server) info.bt.Config.ServerEnabled = true;
                    info.bt.DeviceEnabled = true;
                }

                info.bt.EnableLoading = false;
            }
        }

        #endregion

        #region "Disable Device"

        Thread disable_THREAD;

        void DisableDevice(DeviceButton bt, string tableName)
        {
            bt.EnableLoading = true;

            EnableDevice_Info info = new EnableDevice_Info();
            info.bt = bt;
            info.tablename = tableName;

            if (disable_THREAD != null) disable_THREAD.Abort();

            disable_THREAD = new Thread(new ParameterizedThreadStart(DisableDevice_Worker));
            disable_THREAD.Start(info);
        }

        void DisableDevice_Worker(object o)
        {
            if (o != null)
            {
                EnableDevice_Info info = (EnableDevice_Info)o;

                if (ManagerType == DeviceManagerType.Client) info.success = Remote.Configurations.UpdateConfigurationTable("/ClientEnabled", "False", info.tablename);
                else if (ManagerType == DeviceManagerType.Server) info.success = Remote.Configurations.UpdateConfigurationTable("/ServerEnabled", "False", info.tablename);

                // Reset Update ID
                if (info.success)
                {
                    if (ManagerType == DeviceManagerType.Client) info.success = Remote.Configurations.UpdateConfigurationTable("/ClientUpdateId", String_Functions.RandomString(20), info.tablename);
                    else if (ManagerType == DeviceManagerType.Server) info.success = Remote.Configurations.UpdateConfigurationTable("/ServerUpdateId", String_Functions.RandomString(20), info.tablename);
                }

                this.Dispatcher.BeginInvoke(new Action<EnableDevice_Info>(DisableDevice_Finished), priority, new object[] { info });



                //string tableName = o.ToString();

                //if (ManagerType == DeviceManagerType.Client) Remote.Configurations.UpdateConfigurationTable("/ClientEnabled", "False", tableName);
                //else if (ManagerType == DeviceManagerType.Server) Remote.Configurations.UpdateConfigurationTable("/ServerEnabled", "False", tableName);

                ////Remote.Configurations.UpdateConfigurationTable("/Enabled", "False", tableName);

                //// Reset Update ID
                //if (ManagerType == DeviceManagerType.Client) Remote.Configurations.UpdateConfigurationTable("/ClientUpdateId", String_Functions.RandomString(20), tableName);
                //else if (ManagerType == DeviceManagerType.Server) Remote.Configurations.UpdateConfigurationTable("/ServerUpdateId", String_Functions.RandomString(20), tableName);
            }
        }

        void DisableDevice_Finished(EnableDevice_Info info)
        {
            if (info.bt != null)
            {
                if (info.success && info.bt != null)
                {
                    if (ManagerType == DeviceManagerType.Client) info.bt.Config.ClientEnabled = false;
                    if (ManagerType == DeviceManagerType.Server) info.bt.Config.ServerEnabled = false;
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

                        InitializePages(ManagerType);

                        SelectDevice(db.Config);
                    }
                }
            }

            foreach (TH_WPF.ListButton olb in DeviceList.OfType<TH_WPF.ListButton>()) if (olb != lb) olb.IsSelected = false;
            lb.IsSelected = true;
        }

        void SelectDevice(Configuration config)
        {
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

                    //if (lb.DataObject != null)
                    //{
                    //    if (lb.DataObject.GetType() == typeof(Configuration))
                    //    {
                    //        Configuration config = (Configuration)lb.DataObject;

                    //        Pages.AddShare.Page page = new Pages.AddShare.Page();
                    //        page.devicemanager = this;
                    //        page.currentuser = CurrentUser;
                    //        page.LoadConfiguration(config);
                    //        page.configurationtable = ConfigurationTable;
                    //        CurrentPage = page;
                    //    }
                    //}
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

        void InitializePages(DeviceManagerType type)
        {
            //PageListShown = false;

            PageList.Clear();

            bool useTrakHoundCloud = false;
            if (SelectedDevice != null) useTrakHoundCloud = SelectedDevice.UseTrakHoundCloud;

            ConfigurationPages = new List<ConfigurationPage>();

            ConfigurationPages.Add(new Pages.Overview.Page());
            ConfigurationPages.Add(new Pages.Description.Page());

            // Agent
            if (type == DeviceManagerType.Server || useTrakHoundCloud) ConfigurationPages.Add(new Pages.Agent.Page());

            // Databases
            if (type == DeviceManagerType.Server || !useTrakHoundCloud) ConfigurationPages.Add(new Pages.Databases.Page());

            // Load configuration pages from plugins
            if (type == DeviceManagerType.Server) ConfigurationPages.AddRange(AddConfigurationPageButtons(Table_Plugins));

            // Create PageItem and add to PageList
            foreach (ConfigurationPage page in ConfigurationPages)
            {
                if (ManagerType == DeviceManagerType.Client) page.PageType = TH_PlugIns_Server.Page_Type.Client;
                else if (ManagerType == DeviceManagerType.Server) page.PageType = TH_PlugIns_Server.Page_Type.Server;

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

        private void Restore_Clicked(Button_02 bt)
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

        List<ConfigurationPage> AddConfigurationPageButtons(List<Table_PlugIn> plugins)
        {
            List<ConfigurationPage> result = new List<ConfigurationPage>();

            foreach (Table_PlugIn plugin in plugins)
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

        void AddConfigurationPageButton(Table_PlugIn tp)
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

        public IEnumerable<Lazy<Table_PlugIn>> TablePlugIns { get; set; }

        public List<Table_PlugIn> Table_Plugins { get; set; }

        TablePlugs TPLUGS;

        class TablePlugs
        {
            [ImportMany(typeof(Table_PlugIn))]
            public IEnumerable<Lazy<Table_PlugIn>> PlugIns { get; set; }
        }

        void LoadPlugins()
        {
            string plugin_rootpath = FileLocations.Plugins + @"\Server";

            if (!Directory.Exists(plugin_rootpath)) Directory.CreateDirectory(plugin_rootpath);

            Table_Plugins = new List<Table_PlugIn>();

            string pluginsPath;

            // Load from System Directory first (easier for user to navigate to 'C:\TrakHound\')
            pluginsPath = TH_Global.FileLocations.Plugins + @"\Server\";
            if (Directory.Exists(pluginsPath)) LoadTablePlugins(pluginsPath);

            // Load from App root Directory (doesn't overwrite plugins found in System Directory)
            pluginsPath = AppDomain.CurrentDomain.BaseDirectory + @"Plugins\";
            if (Directory.Exists(pluginsPath)) LoadTablePlugins(pluginsPath);

        }

        void LoadTablePlugins(string Path)
        {
            Logger.Log("Searching for Table Plugins in '" + Path + "'");
            if (Directory.Exists(Path))
            {
                try
                {
                    TPLUGS = new TablePlugs();

                    var PageCatalog = new DirectoryCatalog(Path);
                    var PageContainer = new CompositionContainer(PageCatalog);
                    PageContainer.SatisfyImportsOnce(TPLUGS);

                    TablePlugIns = TPLUGS.PlugIns;

                    foreach (Lazy<Table_PlugIn> ltp in TablePlugIns)
                    {
                        Table_PlugIn tp = ltp.Value;

                        if (Table_Plugins.ToList().Find(x => x.Name.ToLower() == tp.Name.ToLower()) == null)
                        {
                            Logger.Log(tp.Name + " : PlugIn Found");
                            Table_Plugins.Add(tp);
                        }
                        else
                        {
                            Logger.Log(tp.Name + " : PlugIn Already Found");
                        }
                    }
                }
                catch (Exception ex) { Logger.Log("LoadTablePlugins() : Exception : " + ex.Message); }

                // Search Subdirectories
                foreach (string directory in Directory.GetDirectories(Path))
                {
                    LoadTablePlugins(directory);
                }
            }
            else Logger.Log("Table PlugIns Directory Doesn't Exist (" + Path + ")");
        }


        List<ConfigurationPage> PluginConfigurationPages;

        void ProcessTablePlugins(DataTable dt)
        {
            if (TablePlugIns != null && dt != null)
            {
                foreach (Lazy<Table_PlugIn> ltp in TablePlugIns.ToList())
                {
                    try
                    {
                        Table_PlugIn tp = ltp.Value;

                        Type config_type = tp.Config_Page;

                        object o = Activator.CreateInstance(config_type);

                        ConfigurationPage page = (ConfigurationPage)o;
                    }
                    catch (Exception ex) { Logger.Log("Plugin Exception! : " + ex.Message); }
                }
            }
        }

        void TablePlugIns_Closing()
        {
            if (TablePlugIns != null)
            {
                foreach (Lazy<Table_PlugIn> ltp in TablePlugIns.ToList())
                {
                    try
                    {
                        Table_PlugIn tp = ltp.Value;
                        tp.Closing();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Plugin Exception! : " + ex.Message);
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


        #region "Add Device"

        private void AddDevice_GRID_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            foreach (TH_WPF.ListButton lb in DeviceList.OfType<TH_WPF.ListButton>()) lb.IsSelected = false;

            SelectedDevice = null;
            selectedPageIndex = 0;

            AddDevice();
        }

        public void AddDevice()
        {
            PageListShown = false;
            ToolbarShown = false;

            if (CurrentPage != null)
            {
                if (CurrentPage.GetType() != typeof(Pages.AddDevice.Page))
                {
                    AddDevice_Page();
                }
            }
            else AddDevice_Page();
        }

        void AddDevice_Page()
        {
            Pages.AddDevice.Page page = new Pages.AddDevice.Page();
            page.deviceManager = this;
            page.DeviceAdded += page_DeviceAdded;
            page.currentuser = CurrentUser;
            page.LoadCatalog();

            CurrentPage = page;
        }

        void page_DeviceAdded()
        {

            LoadDevices();

        }

        #endregion

        #region "Copy Device"

        Thread CopyDevice_THREAD;

        void CopyDevice(Configuration config)
        {

            if (CopyDevice_THREAD != null) CopyDevice_THREAD.Abort();

            CopyDevice_THREAD = new Thread(new ParameterizedThreadStart(CopyDevice_Worker));
            CopyDevice_THREAD.Start(config);
        }

        void CopyDevice_Worker(object o)
        {
            bool success = false;

            if (o != null)
            {
                Configuration config = (Configuration)o;

                if (currentuser != null)
                {
                    success = Configurations.AddConfigurationToUser(currentuser, config, userDatabaseSettings);
                }
                else
                {
                    success = false;
                }
            }

            this.Dispatcher.BeginInvoke(new Action<bool>(CopyDevice_GUI), priority, new object[] { success });
        }

        void CopyDevice_GUI(bool success)
        {

            if (success) LoadDevices();
            else
            {
                MessageBox.Show("Error during Device Copy. Please try again", "Device Copy Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            
        }




        #endregion


        private void RefreshDevices_Clicked(Button_02 bt)
        {
            LoadDevices();
        }

        private void IndexUp_Clicked(Button_02 bt)
        {
            if (SelectedDevice != null)
            {
                if (SelectedDevice.Index > 0)
                {
                    SetDeviceIndex(SelectedDevice.Index - 1, SelectedDevice.TableName);
                }
            }
        }

        private void IndexDown_Clicked(Button_02 bt)
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

                Remote.Configurations.UpdateConfigurationTable("/Index", info.index.ToString(), info.tablename);

                // Reset Update ID
                if (ManagerType == DeviceManagerType.Client) Remote.Configurations.UpdateConfigurationTable("/ClientUpdateId", String_Functions.RandomString(20), info.tablename);
                else if (ManagerType == DeviceManagerType.Server) Remote.Configurations.UpdateConfigurationTable("/ServerUpdateId", String_Functions.RandomString(20), info.tablename);
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
