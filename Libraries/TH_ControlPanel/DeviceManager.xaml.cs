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
            InitializeComponent();
            DataContext = this;

            LoadPlugins();

            InitializePages();
        }

        #region "User Login"

        UserConfiguration currentuser;
        public UserConfiguration CurrentUser
        {
            get { return currentuser; }
            set
            {
                currentuser = value;

                if (currentuser != null)
                {
                    LoadDevices();
                }
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
                        Global.Initialize(config.Databases);

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

        Thread save_THREAD;

        private void Save_Clicked(Button_02 bt)
        {
            bt.Focus();

            Saving = true;

            //DataTable dt = ConfigurationTable;

            if (SelectedDevice != null)
            {
                DataTable dt = Converter.XMLToTable(SelectedDevice.ConfigurationXML);
                dt.TableName = SelectedDevice.TableName;

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
            DataTable dt = (DataTable)o;

            if (dt != null)
            {
                SaveConfiguration(dt);

                if (SelectedDevice.Shared && SelectedDevice.SharedTableName != null && dt != null)
                {
                    MessageBoxResult result = MessageBox.Show("This configuration is Shared. Do you want to Update the Shared Configuration as well?", "Update Shared Configuration", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        if (Configurations.UpdateConfigurationTable(SelectedDevice.TableName, dt, userDatabaseSettings))
                        {
                            Shared.SharedListItem item = new Shared.SharedListItem();

                            item.upload_date = DateTime.Now;

                            if (SelectedDevice.Version != null)
                            {
                                Version version;
                                if (Version.TryParse(SelectedDevice.Version, out version))
                                {
                                    int major = version.Major;
                                    int minor = version.Minor;
                                    int build = version.Build;
                                    int revision = version.Revision;

                                    if (minor < 10) minor += 1;
                                    else
                                    {
                                        major += 1;
                                        minor = 0;
                                    }

                                    item.version = major.ToString() + "." + minor.ToString() + "." + build.ToString() + "." + revision.ToString();
                                }
                            }

                            Shared.UpdateSharedConfiguration_ToList(CurrentUser, item);
                            Configurations.UpdateConfigurationTable(SelectedDevice.SharedTableName, dt, null);
                        }
                    }
                }
            }

            this.Dispatcher.BeginInvoke(new Action(Save_Finished), background, null);
        }

        void Save_GUI(ConfigurationPage page)
        {
            page.SaveConfiguration(ConfigurationTable);
        }

        void Save_Finished()
        {
            if (SelectedDevice != null) SelectDevice(SelectedDevice);

            SaveNeeded = false;
            Saving = false;
        }

        public void SaveConfiguration(DataTable dt)
        {
            string tablename = null;

            if (dt != null)
            {
                tablename = dt.TableName;

                if (currentuser != null)
                {
                    // Reset Update ID
                    Table_Functions.UpdateTableValue(String_Functions.RandomString(20), "/UpdateId", dt);

                    // Add Unique Id (ONLY if one not already set)
                    if (Table_Functions.GetTableValue("/UniqueId", dt) == null) Table_Functions.UpdateTableValue(String_Functions.RandomString(20), "/UniqueId", dt);

                    Configurations.ClearConfigurationTable(tablename, userDatabaseSettings);

                    Configurations.UpdateConfigurationTable(tablename, dt, userDatabaseSettings);
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

                SelectedDeviceButton.Config = SelectedDevice;
            }
        }
      
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
            db.Config = config;

            //db.DeviceEnabled = config.Enabled;
            //db.enabled_CHK.IsChecked = config.Enabled;

            //db.Shared = config.Shared;

            //db.Description = config.Description.Description;
            //db.Manufacturer = config.Description.Manufacturer;
            //db.Model = config.Description.Model;
            //db.Serial = config.Description.Serial;
            //db.Id = config.Description.Machine_ID;

            db.Enabled += db_Enabled;
            db.Disabled += db_Disabled;
            db.RemoveClicked += db_RemoveClicked;
            db.ShareClicked += db_ShareClicked;
            db.Clicked += db_Clicked;

            ListButton lb = new ListButton();
            lb.ButtonContent = db;
            lb.ShowImage = false;
            lb.Selected += lb_Device_Selected;
            //lb.DataObject = config;

            db.Parent = lb;

            DeviceList.Add(lb);
        }

        void db_Enabled(DeviceButton bt)
        {
            if (bt.Config != null)
            {
                if (bt.Config.TableName != null) EnableDevice(bt.Config.TableName);

                bt.Config.Enabled = true;
                bt.DeviceEnabled = true;
            }
        }

        void db_Disabled(DeviceButton bt)
        {
            if (bt.Config != null)
            {
                if (bt.Config.TableName != null) DisableDevice(bt.Config.TableName);

                bt.Config.Enabled = false;
                bt.DeviceEnabled = false;
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

            //if (CurrentPage != null)
            //{
            //    if (CurrentPage.GetType() != typeof(Pages.AddShare.Page))
            //    {
            //        LoadAddSharePage(bt);
            //    }
            //}
            //else
            //{
            //    LoadAddSharePage(bt);
            //}

            LoadAddSharePage(bt);

            ToolbarShown = false;
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

        Thread enable_THREAD;

        void EnableDevice(string tableName)
        {
            if (enable_THREAD != null) enable_THREAD.Abort();

            enable_THREAD = new Thread(new ParameterizedThreadStart(EnableDevice_Worker));
            enable_THREAD.Start(tableName);
        }

        void EnableDevice_Worker(object o)
        {
            if (o != null)
            {
                string tableName = o.ToString();

                Remote.Configurations.UpdateConfigurationTable("/Enabled", "True", tableName);

                Remote.Configurations.UpdateConfigurationTable("/UpdateId", String_Functions.RandomString(20), tableName);
            }
        }

        #endregion

        #region "Disable Device"

        Thread disable_THREAD;

        void DisableDevice(string tableName)
        {
            if (disable_THREAD != null) disable_THREAD.Abort();

            disable_THREAD = new Thread(new ParameterizedThreadStart(DisableDevice_Worker));
            disable_THREAD.Start(tableName);
        }

        void DisableDevice_Worker(object o)
        {
            if (o != null)
            {
                string tableName = o.ToString();

                Remote.Configurations.UpdateConfigurationTable("/Enabled", "False", tableName);

                Remote.Configurations.UpdateConfigurationTable("/UpdateId", String_Functions.RandomString(20), tableName);
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
            PageListShown = true;
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

        void InitializePages()
        {
            PageListShown = false;

            PageList.Clear();

            ConfigurationPages = new List<ConfigurationPage>();

            ConfigurationPages.Add(new Pages.Description.Page());
            ConfigurationPages.Add(new Pages.Agent.Page());
            ConfigurationPages.Add(new Pages.Databases.Page());

            // Load configuration pages from plugins
            ConfigurationPages.AddRange(AddConfigurationPageButtons(Table_Plugins));

            // Create PageItem and add to PageList
            foreach (ConfigurationPage page in ConfigurationPages)
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


        #region "Add Device"

        private void AddDevice_GRID_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            foreach (TH_WPF.ListButton lb in DeviceList.OfType<TH_WPF.ListButton>()) lb.IsSelected = false;

            SelectedDevice = null;
            selectedPageIndex = 0;

            AddDevice();
        }

        void AddDevice()
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

        private void RefreshDevices_Clicked(Button_02 bt)
        {
            LoadDevices();
        }



       

    }
}
