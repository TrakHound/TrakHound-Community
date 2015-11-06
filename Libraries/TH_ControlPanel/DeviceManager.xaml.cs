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
using TH_PlugIns_Server;
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
                    //CurrentUsername = TH_Global.Formatting.UppercaseFirst(currentuser.username);

                    LoadDevices();

                    //LoggedIn = true;
                }
                //else
                //{
                //    LoggedIn = false;
                //    CurrentUsername = null;
                //}

                //if (CurrentUserChanged != null) CurrentUserChanged(currentuser);
            }
        }

        public Database_Settings userDatabaseSettings;

        //#region "Properties"

        //public string CurrentUsername
        //{
        //    get { return (string)GetValue(CurrentUsernameProperty); }
        //    set { SetValue(CurrentUsernameProperty, value); }
        //}

        //public static readonly DependencyProperty CurrentUsernameProperty =
        //    DependencyProperty.Register("CurrentUsername", typeof(string), typeof(ControlPanel), new PropertyMetadata(null));


        //public ImageSource ProfileImage
        //{
        //    get { return (ImageSource)GetValue(ProfileImageProperty); }
        //    set { SetValue(ProfileImageProperty, value); }
        //}

        //public static readonly DependencyProperty ProfileImageProperty =
        //    DependencyProperty.Register("ProfileImage", typeof(ImageSource), typeof(ControlPanel), new PropertyMetadata(null));


        //public bool LoggedIn
        //{
        //    get { return (bool)GetValue(LoggedInProperty); }
        //    set { SetValue(LoggedInProperty, value); }
        //}

        //public static readonly DependencyProperty LoggedInProperty =
        //    DependencyProperty.Register("LoggedIn", typeof(bool), typeof(ControlPanel), new PropertyMetadata(false));

        //#endregion

        //public delegate void CurrentUserChanged_Handler(UserConfiguration userConfig);
        //public event CurrentUserChanged_Handler CurrentUserChanged;

        //UserConfiguration currentuser;
        //public UserConfiguration CurrentUser
        //{
        //    get { return currentuser; }
        //    set
        //    {
        //        currentuser = value;

        //        if (currentuser != null)
        //        {
        //            CurrentUsername = TH_Global.Formatting.UppercaseFirst(currentuser.username);

        //            LoadDevices();

        //            LoggedIn = true;
        //        }
        //        else
        //        {
        //            LoggedIn = false;
        //            CurrentUsername = null;
        //        }

        //        if (CurrentUserChanged != null) CurrentUserChanged(currentuser);
        //    }
        //}

        //public Database_Settings userDatabaseSettings;

        //void ReadUserManagementSettings()
        //{
        //    DatabasePluginReader dpr = new DatabasePluginReader();

        //    string localPath = AppDomain.CurrentDomain.BaseDirectory + "UserConfiguration.Xml";
        //    string systemPath = TH_Global.FileLocations.TrakHound + @"\" + "UserConfiguration.Xml";

        //    string configPath;

        //    // systemPath takes priority (easier for user to navigate to)
        //    if (File.Exists(systemPath)) configPath = systemPath;
        //    else configPath = localPath;

        //    Logger.Log(configPath);

        //    UserManagementSettings userSettings = UserManagementSettings.ReadConfiguration(configPath);

        //    if (userSettings != null)
        //    {
        //        if (userSettings.Databases.Databases.Count > 0)
        //        {
        //            userDatabaseSettings = userSettings.Databases;
        //            Global.Initialize(userDatabaseSettings);
        //        }
        //    }

        //}

        #endregion

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

        DataTable configurationtable;
        public DataTable ConfigurationTable
        {
            get { return configurationtable; }
            set
            {
                configurationtable = value;
            }
        }

        public void LoadDevices()
        {
            DeviceListShown = false;
            DeviceList.Clear();

            if (currentuser != null)
            {
                if (userDatabaseSettings == null)
                {
                    Configurations = TH_Configuration.User.Management.GetConfigurationsForUser(currentuser);
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

            if (Configurations != null)
            {
                // Create DevicesList based on Configurations
                foreach (Configuration config in Configurations)
                {
                    CreateDeviceButton(config);
                }


                // Update Device Management Page
                //if (devicemanagementpage == null) devicemanagementpage = new Pages.DeviceManagement.Page();
                //devicemanagementpage.LoadDevices(Configurations);

                DeviceListShown = true;
            }
        }

        #region "Configuration Files"

        List<Configuration> Configurations;

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

        void LoadConfiguration()
        {
            SaveNeeded = false;

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

        public void SaveConfiguration()
        {
            DataTable dt = ConfigurationTable;

            if (dt != null)
            {
                if (currentuser != null)
                {
                    if (SelectedDevice != null)
                    {
                        //string tablename = TH_Configuration.User.Management.GetConfigurationTableName(currentuser, SelectedDevice);
                        string tablename = dt.TableName;

                        if (userDatabaseSettings == null)
                        {
                            TH_Configuration.User.Management.ClearConfigurationTable(currentuser, tablename);
                            //TH_Configuration.User.Management.CreateConfigurationTable(currentuser, SelectedDevice);
                            TH_Configuration.User.Management.UpdateConfigurationTable(currentuser, tablename, dt);
                        }
                        else
                        {
                            //TH_Database.Tables.Users.Configuration_UpdateRows(currentuser, userDatabaseSettings, SelectedDevice);
                        }
                    }
                }
                // If not logged in Save to File in 'C:\TrakHound\'
                else
                {

                }
            }
        }

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
            db.Description = config.Description.Description;
            db.Manufacturer = config.Description.Manufacturer;
            db.Model = config.Description.Model;
            db.Serial = config.Description.Serial;
            db.Id = config.Description.Machine_ID;

            ListButton lb = new ListButton();
            lb.ButtonContent = db;
            lb.ShowImage = false;
            lb.Selected += lb_Device_Selected;
            lb.DataObject = config;
            DeviceList.Add(lb);
        }

        void lb_Device_Selected(TH_WPF.ListButton lb)
        {
            Configuration config = (Configuration)lb.DataObject;

            if (config != null)
            {
                if (SelectedDevice != config)
                {
                    SelectedDevice = config;

                    ConfigurationTable = TH_Configuration.Converter.XMLToTable(config.ConfigurationXML);
                    if (ConfigurationTable != null)
                    {
                        ConfigurationTable.TableName = config.TableName;

                        if (ConfigurationPages != null)
                        {
                            foreach (ConfigurationPage page in ConfigurationPages)
                            {
                                this.Dispatcher.BeginInvoke(new Action<DataTable>(page.LoadConfiguration), new object[] { ConfigurationTable });
                            }
                        }
                    }
                        
                    PageListShown = true;
                }
            }

            foreach (TH_WPF.ListButton olb in DeviceList.OfType<TH_WPF.ListButton>()) if (olb != lb) olb.IsSelected = false;
            lb.IsSelected = true;
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

                PageList.Add(bt);
            }
        }

        void Page_Selected(ListButton LB)
        {
            foreach (ListButton olb in PageList) if (olb != LB) olb.IsSelected = false;
            LB.IsSelected = true;

            if (LB.DataObject != null)
            {
                if (CurrentPage != null)
                {
                    if (CurrentPage.GetType() != LB.DataObject.GetType())
                    {
                        CurrentPage = LB.DataObject;
                    }
                }
                else CurrentPage = LB.DataObject;
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
            LoadConfiguration();
        }

        private void Save_Clicked(Button_02 bt)
        {
            if (ConfigurationTable != null)
            {
                if (ConfigurationPages != null)
                {
                    foreach (ConfigurationPage page in ConfigurationPages)
                    {
                        page.SaveConfiguration(configurationtable);
                    }
                }

                SaveConfiguration();
            }

            SaveNeeded = false;
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
            }
        }

        //void Description_Clicked()
        //{
        //    if (CurrentPage != null)
        //    {
        //        if (CurrentPage.GetType() != typeof(Pages.DescriptionConfiguration))
        //        {
        //            CurrentPage = new Pages.DescriptionConfiguration();
        //        }
        //    }
        //    else CurrentPage = new Pages.DescriptionConfiguration();
        //}

        //void Agent_Clicked()
        //{
        //    if (CurrentPage != null)
        //    {
        //        if (CurrentPage.GetType() != typeof(Pages.Agent.Page))
        //        {
        //            CurrentPage = new Pages.AgentConfiguration();
        //        }
        //    }
        //    else CurrentPage = new Pages.AgentConfiguration();
        //}



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

            //TablePlugIns = Table_Plugins;
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
                        //page.SettingChanged += page_SettingChanged;
                        

                        //configurationPages.Add(page);

                    }
                    catch (Exception ex) { Logger.Log("Plugin Exception! : " + ex.Message); }
                }
            }
        }


        //void TablePlugIns_Initialize(Configuration config)
        //{
        //    if (TablePlugIns != null && config != null)
        //    {
        //        foreach (Lazy<Table_PlugIn> ltp in TablePlugIns.ToList())
        //        {
        //            try
        //            {
        //                Table_PlugIn tp = ltp.Value;




        //                //tp.Initialize(config);

        //                //AddConfigurationPage(tp);
        //            }
        //            catch (Exception ex)
        //            {
        //                Console.WriteLine("Plugin Exception! : " + ex.Message);
        //            }
        //        }
        //    }
        //}

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


        public bool PageListShown
        {
            get { return (bool)GetValue(PageListShownProperty); }
            set { SetValue(PageListShownProperty, value); }
        }

        public static readonly DependencyProperty PageListShownProperty =
            DependencyProperty.Register("PageListShown", typeof(bool), typeof(DeviceManager), new PropertyMetadata(false));


        const System.Windows.Threading.DispatcherPriority Priority_Background = System.Windows.Threading.DispatcherPriority.Background;


        private void TableList_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PageListShown = false;
        }

        private void AddDevice_GRID_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (CurrentPage != null)
            {
                if (CurrentPage.GetType() != typeof(Pages.AddDevice.Page))
                {
                    Pages.AddDevice.Page page = new Pages.AddDevice.Page();
                    page.deviceManager = this;

                    CurrentPage = page;
                }
            }
            else
            {
                Pages.AddDevice.Page page = new Pages.AddDevice.Page();
                page.deviceManager = this;

                CurrentPage = page;
            }
        }

    }
}
