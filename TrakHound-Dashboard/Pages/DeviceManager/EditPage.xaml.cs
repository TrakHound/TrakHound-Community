// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Xml;

using TrakHound;
using TrakHound.API;
using TrakHound.API.Users;
using TrakHound.Configurations;
using TrakHound.Configurations.Converters;
using TrakHound.Logging;
using TrakHound.Plugins;
using TrakHound.Plugins.Server;
using TrakHound.Tools;
using TrakHound_UI;

namespace TrakHound_Dashboard.Pages.DeviceManager
{
    /// <summary>
    /// Page used to edit a device's Configuration object using a page/category based GUI
    /// </summary>
    public partial class EditPage : UserControl, IPage
    {
        public EditPage(UserConfiguration _userConfig, string _uniqueId)
        {
            InitializeComponent();
            DataContext = this;

            userConfig = _userConfig;
            uniqueId = _uniqueId;

            LoadDevice();

            LoadPages();
        }

        private UserConfiguration userConfig;
        private string uniqueId;

        #region "IPage Interface"

        public string Title { get { return "Edit Device"; } }

        public Uri Image { get { return new Uri("pack://application:,,,/TrakHound-Dashboard;component/Resources/Edit_02.png"); } }

        public bool ZoomEnabled { get { return false; } }

        public void SetZoom(double zoomPercentage) { }

        public void Opened() { }
        public bool Opening() { return true; }

        public void Closed() { }
        public bool Closing()
        {
            // If Save is needed, ask to save (Yes/No) or cancel
            if (SaveNeeded)
            {
                string text = "Save Changes to Device?";
                string title = "Save Changes";

                var result = TrakHound_UI.MessageBox.Show(text, title, MessageBoxButtons.YesNoCancel);

                // If Yes was clicked
                if (result == MessageBoxDialogResult.Yes && ConfigurationTable != null) Save(ConfigurationTable);
                // If cancel was clicked
                else if (result == MessageBoxDialogResult.Cancel) return false;
            }
            return true;
        }

        public event SendData_Handler SendData;

        public void GetSentData(EventData data)
        {
            Dispatcher.BeginInvoke(new Action<EventData>(UpdateLoggedInChanged), System.Windows.Threading.DispatcherPriority.DataBind, new object[] { data });

            foreach (var page in ConfigurationPages)
            {
                page.GetSentData(data);
            }
        }

        void UpdateLoggedInChanged(EventData data)
        {
            if (data != null)
            {
                if (data.Id == "USER_LOGIN")
                {
                    if (data.Data01 != null) userConfig = (UserConfiguration)data.Data01;
                }
                else if (data.Id == "USER_LOGOUT")
                {
                    userConfig = null;
                }
            }
        }

        private EventData CreateCurrentUserEventData()
        {
            var data = new EventData(this);
            data.Id = "USER_LOGIN";
            data.Data01 = userConfig;
            return data;
        }

        #endregion

        /// <summary>
        /// Configuration object that is being edited
        /// </summary>
        public DeviceConfiguration Configuration { get; set; }

        /// <summary>
        /// Configuration DataTable object that is being edited
        /// </summary>
        public DataTable ConfigurationTable { get; set; }

        /// <summary>
        /// Event to request to open the Device List Page
        /// </summary>
        public event PageSelected_Handler DeviceListSelected;


        #region "Toolbar"

        private void Save_Clicked(TrakHound_UI.Button bt)
        {
            bt.Focus();

            if (Configuration != null)
            {
                DataTable dt = DeviceConfigurationConverter.XMLToTable(Configuration.Xml);
                Save(dt);
            }
        }

        private void Restore_Clicked(TrakHound_UI.Button bt) { RestorePages(); }

        private void DeviceManager_Clicked(TrakHound_UI.Button bt)
        {
            DeviceListSelected?.Invoke();
        }

        #endregion

        #region "Dependency Properties"

        public bool DeviceLoading
        {
            get { return (bool)GetValue(DeviceLoadingProperty); }
            set { SetValue(DeviceLoadingProperty, value); }
        }

        public static readonly DependencyProperty DeviceLoadingProperty =
            DependencyProperty.Register("DeviceLoading", typeof(bool), typeof(EditPage), new PropertyMetadata(true));

        public bool DeviceError
        {
            get { return (bool)GetValue(DeviceErrorProperty); }
            set { SetValue(DeviceErrorProperty, value); }
        }

        public static readonly DependencyProperty DeviceErrorProperty =
            DependencyProperty.Register("DeviceError", typeof(bool), typeof(EditPage), new PropertyMetadata(false));

        public object CurrentPage
        {
            get { return (object)GetValue(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        public static readonly DependencyProperty CurrentPageProperty =
            DependencyProperty.Register("CurrentPage", typeof(object), typeof(EditPage), new PropertyMetadata(null));

        public bool SaveNeeded
        {
            get { return (bool)GetValue(SaveNeededProperty); }
            set { SetValue(SaveNeededProperty, value); }
        }

        public static readonly DependencyProperty SaveNeededProperty =
            DependencyProperty.Register("SaveNeeded", typeof(bool), typeof(EditPage), new PropertyMetadata(false));

        public bool Saving
        {
            get { return (bool)GetValue(SavingProperty); }
            set { SetValue(SavingProperty, value); }
        }

        public static readonly DependencyProperty SavingProperty =
            DependencyProperty.Register("Saving", typeof(bool), typeof(EditPage), new PropertyMetadata(false));

        #endregion

        #region "Load Device Configuration"

        private Thread loadDeviceThread;

        private class LoadDeviceInfo
        {
            public LoadDeviceInfo(UserConfiguration _userConfig, string _uniqueId)
            {
                UserConfiguration = _userConfig;
                UniqueId = _uniqueId;
            }

            public UserConfiguration UserConfiguration { get; set; }
            public string UniqueId { get; set; }
        }

        /// <summary>
        /// Loads the Device's DeviceConfiguration
        /// </summary>
        private void LoadDevice()
        {
            DeviceLoading = true;

            if (loadDeviceThread != null) loadDeviceThread.Abort();

            if (userConfig != null)
            {
                loadDeviceThread = new Thread(new ParameterizedThreadStart(LoadUserDevice));
                loadDeviceThread.Start(new LoadDeviceInfo(userConfig, uniqueId));
            }
            else
            {
                loadDeviceThread = new Thread(new ParameterizedThreadStart(LoadLocalDevice));
                loadDeviceThread.Start(uniqueId);
            }
        }

        private void LoadUserDevice(object o)
        {
            if (o != null)
            {
                var loadDeviceInfo = (LoadDeviceInfo)o;

                var config = Devices.Get(loadDeviceInfo.UserConfiguration, loadDeviceInfo.UniqueId);
                if (config != null)
                {
                    Configuration = config;
                    ConfigurationTable = DeviceConfigurationConverter.XMLToTable(config.Xml);

                    // Reload Pages with new Device Configuration
                    Dispatcher.BeginInvoke(new Action(RestorePages), System.Windows.Threading.DispatcherPriority.Background, new object[] { });
                }
                else
                {
                    Dispatcher.BeginInvoke(new Action(() => DeviceError = true), System.Windows.Threading.DispatcherPriority.Background, new object[] { });
                }
            }

            Dispatcher.BeginInvoke(new Action(() => DeviceLoading = false), System.Windows.Threading.DispatcherPriority.Background, new object[] { });
        }

        private void LoadLocalDevice(object o)
        {
            if (o != null)
            {
                string uniqueId = (string)o;

                try
                {
                    string filename = Path.ChangeExtension(uniqueId, ".xml");
                    string path = Path.Combine(FileLocations.Devices, filename);

                    var config = DeviceConfiguration.Read(path);
                    if (config != null)
                    {
                        Configuration = config;
                        ConfigurationTable = DeviceConfigurationConverter.XMLToTable(config.Xml);

                        // Reload Pages with new Device Configuration
                        Dispatcher.BeginInvoke(new Action(RestorePages), System.Windows.Threading.DispatcherPriority.Background, new object[] { });
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(new Action(() => DeviceError = true), System.Windows.Threading.DispatcherPriority.Background, new object[] { });                  
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("Local Device Backup Error :: Exception :: " + ex.Message);
                }
            }

            Dispatcher.BeginInvoke(new Action(() => DeviceLoading = false), System.Windows.Threading.DispatcherPriority.Background, new object[] { });
        }

        private DeviceConfiguration GetLocalDeviceConfiguration(string uniqueId)
        {
            string filename = Path.ChangeExtension(uniqueId, ".xml");
            string path = Path.Combine(FileLocations.Devices, filename);

            return DeviceConfiguration.Read(path);
        }

        #endregion

        #region "Load / Save Device"

        private void LoadPage(IConfigurationPage page)
        {
            if (ConfigurationTable != null)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (!page.Loaded) page.LoadConfiguration(ConfigurationTable.Copy());
                    page.Loaded = true;
                }), System.Windows.Threading.DispatcherPriority.Background, new object[] { });
            }
        }

        private void RestorePages()
        {
            if (ConfigurationTable != null)
            {
                if (ConfigurationPages != null)
                {
                    foreach (IConfigurationPage page in ConfigurationPages)
                    {
                        Dispatcher.BeginInvoke(new Action(() => { page.Loaded = false; }));
                    }

                    if (CurrentPage != null) LoadPage((IConfigurationPage)CurrentPage);

                    GetProbeData(ConfigurationTable);
                }
            }

            SaveNeeded = false;
            DeviceError = false;
        }

        private void Save(DataTable dt)
        {
            Saving = true;

            if (dt != null)
            {
                if (ConfigurationPages != null)
                {
                    foreach (IConfigurationPage page in ConfigurationPages)
                    {
                        if (page.Loaded) page.SaveConfiguration(dt);
                        page.Loaded = false;
                    }
                }

                ThreadPool.QueueUserWorkItem(new WaitCallback(Save_Worker), dt);
            }
        }

        private void Save_Worker(object o)
        {
            bool success = false;

            var dt = (DataTable)o;

            if (dt != null)
            {
                // Reset Update ID
                DataTable_Functions.UpdateTableValue(dt, "address", "/UpdateId", "value", Guid.NewGuid().ToString());

                if (userConfig != null)
                {
                    success = Devices.Update(userConfig, dt);
                }
                // If not logged in Save to File in 'C:\TrakHound\'
                else
                {
                    success = DeviceConfiguration.Save(dt);
                }
            }

            ConfigurationTable = dt.Copy();

            XmlDocument xml = DeviceConfigurationConverter.TableToXML(dt);
            if (xml != null)
            {
                Configuration = DeviceConfiguration.Read(xml);
            }

            Dispatcher.BeginInvoke(new Action<bool>(Save_Finished), System.Windows.Threading.DispatcherPriority.Background, new object[] { success });
        }

        private void Save_Finished(bool success)
        {
            if (!success) TrakHound_UI.MessageBox.Show("Device did not save correctly. Try Again." + Environment.NewLine + @"A backup of the Device has been created in the 'C:\TrakHound\Temp directory'");

            RestorePages();

            SaveNeeded = false;
            Saving = false;

            var data = new EventData(this);
            data.Id = "DEVICE_UPDATED";
            data.Data01 = new DeviceDescription(Configuration);
            SendData?.Invoke(data);
        }

        #endregion

        #region "Page List"

        ObservableCollection<ListButton> pagelist;
        public ObservableCollection<ListButton> PageList
        {
            get
            {
                if (pagelist == null)
                    pagelist = new ObservableCollection<ListButton>();
                return pagelist;
            }

            set
            {
                pagelist = value;
            }
        }

        public List<IConfigurationPage> ConfigurationPages = new List<IConfigurationPage>();

        private void LoadPages()
        {
            GetPluginPageInfos();
            var pageInfos = PluginPageInfos;

            foreach (var pageInfo in pageInfos)
            {
                AddPage(pageInfo);
            }

            LoadPages_Finished();          
        }

        private void LoadPages_Finished()
        {
            GetProbeData(ConfigurationTable);
        }

        private static IConfigurationPage CreatePage(IConfigurationInfo info)
        {
            object o = Activator.CreateInstance(info.ConfigurationPageType);
            var page = (IConfigurationPage)o;
            return page;
        }

        private void AddPage(IConfigurationInfo info)
        {
            AddPageButton(info);
        }

        void AddPageButton(IConfigurationInfo info)
        {
            var bt = new ListButton();
            bt.Text = info.Title;

            if (info.Image != null) bt.Image = new BitmapImage(info.Image);
            else bt.Image = new BitmapImage(new Uri("pack://application:,,,/TrakHound_Dashboard.Pages.DeviceManager;component/Resources/Plug_01.png"));

            bt.Selected += Page_Selected;
            bt.DataObject = info;

            if (info.Title == "Description") Page_Selected(bt);

            PageList.Add(bt);
            PageList.Sort();
        }

        private void page_SendData(EventData data)
        {
            if (data != null && data.Id != null)
            {
                if (data.Id.ToLower() == "editpage_requestprobe")
                {
                    if (data.Data01 != null && data.Data02 != null && data.Data03 != null)
                    {
                        string address = (string)data.Data01;
                        int id = (int)data.Data02;
                        string deviceName = (string)data.Data03;

                        GetProbeData(address, id, deviceName);
                    }
                }
            }
        }

        void page_SettingChanged(string name, string oldVal, string newVal)
        {
            SaveNeeded = true;
        }

        void Page_Selected(ListButton lb)
        {
            if (lb.DataObject != null)
            {
                var info = (IConfigurationInfo)lb.DataObject;

                var page = ConfigurationPages.Find(x => x.Title == info.Title);
                if (page == null)
                {
                    page = CreatePage(info);
                    page.SendData += page_SendData;
                    page.SettingChanged += page_SettingChanged;

                    page.GetSentData(GetProbeHeader());
                    page.GetSentData(GetProbeDataItems());
                    page.GetSentData(CreateCurrentUserEventData());
                    
                    ConfigurationPages.Add(page);
                }

                LoadPage(page);

                foreach (ListButton olb in PageList.ToList()) if (olb != lb) olb.IsSelected = false;
                lb.IsSelected = true;

                CurrentPage = page;
            }
        }

        #endregion

        #region "Plugins"

        public static List<IConfigurationInfo> PluginPageInfos { get; set; }

        public static void GetPluginPageInfos()
        {
            var infos = PluginPageInfos;

            if (infos == null)
            {
                infos = new List<IConfigurationInfo>();

                string pluginsPath;

                // Load from System Directory first (easier for user to navigate to 'C:\TrakHound\')
                pluginsPath = FileLocations.Plugins;
                if (Directory.Exists(pluginsPath)) GetPluginPageInfos(pluginsPath, infos);

                // Load from App root Directory (doesn't overwrite plugins found in System Directory)
                pluginsPath = AppDomain.CurrentDomain.BaseDirectory;
                if (Directory.Exists(pluginsPath)) GetPluginPageInfos(pluginsPath, infos);

                // Load from Running Assemblies
                GetPluginPageInfos(Assembly.GetExecutingAssembly(), infos);
                GetPluginPageInfos(Assembly.GetEntryAssembly(), infos);
                GetPluginPageInfos(Assembly.GetCallingAssembly(), infos);

                PluginPageInfos = infos;
            }
        }
        
        private static void GetPluginPageInfos(Assembly assembly, List<IConfigurationInfo> infos)
        {
            try
            {
                var plugins = Reader.FindPlugins<IConfigurationInfo>(assembly, new ConfigurationInfoPlugin.PluginContainer());
                foreach (var plugin in plugins)
                {
                    if (!infos.Exists(x => x.Title == plugin.Title))
                    {
                        infos.Add(plugin);
                    }
                }
            }
            catch (Exception ex) { Logger.Log("LoadPlugins() : Exception : " + ex.Message, LogLineType.Error); }
        }

        private static void GetPluginPageInfos(string path, List<IConfigurationInfo> infos)
        {
            if (Directory.Exists(path))
            {
                try
                {
                    var plugins = Reader.FindPlugins<IConfigurationInfo>(path, new ConfigurationInfoPlugin.PluginContainer());
                    foreach (var plugin in plugins)
                    {
                        if (!infos.Exists(x => x.Title == plugin.Title))
                        {
                            infos.Add(plugin);
                        }
                    }
                }
                catch (Exception ex) { Logger.Log("LoadPlugins() : Exception : " + ex.Message, LogLineType.Error); }

                // Search Subdirectories
                foreach (string directory in Directory.GetDirectories(path))
                {
                    GetPluginPageInfos(directory, infos);
                }
            }
        }
        
        #endregion

        #region "MTC Data Items"  

        public class ProbeDataItem
        {
            public string id { get; set; }
            public string name { get; set; }

            public string display { get; set; }

            public string category { get; set; }
            public string type { get; set; }

            public override string ToString()
            {
                return display;
            }
        }

        private List<MTConnect.Application.Components.DataItem> probeData = new List<MTConnect.Application.Components.DataItem>();

        private MTConnect.Application.Headers.Devices probeHeader;

        void GetProbeData(DataTable dt)
        {
            LoadAgentSettings(dt);
        }

        void GetProbeData(string address, int port, string deviceName)
        {
            RunProbe(address, null, port, deviceName);
        }

        void LoadAgentSettings(DataTable dt)
        {
            string prefix = "/Agent/";

            string ip = DataTable_Functions.GetTableValue(dt, "address", prefix + "Address", "value");

            string p = DataTable_Functions.GetTableValue(dt, "address", prefix + "Port", "value");

            string devicename = DataTable_Functions.GetTableValue(dt, "address", prefix + "DeviceName", "value");

            string proxyAddress = devicename = DataTable_Functions.GetTableValue(dt, "address", prefix + "ProxyAddress", "value");
            string proxyPort = devicename = DataTable_Functions.GetTableValue(dt, "address", prefix + "ProxyPort", "value");

            int port;
            int.TryParse(p, out port);

            // Proxy Settings
            MTConnect.HTTP.ProxySettings proxy = null;
            if (proxyPort != null)
            {
                int proxy_p = -1;
                if (int.TryParse(proxyPort, out proxy_p))
                {
                    proxy = new MTConnect.HTTP.ProxySettings();
                    proxy.Address = proxyAddress;
                    proxy.Port = proxy_p;
                }
            }

            RunProbe(ip, proxy, port, devicename);
        }

        class Probe_Info
        {
            public string address;
            public int port;
            public string deviceName;
            public MTConnect.HTTP.ProxySettings proxy;
        }

        void RunProbe(string address, MTConnect.HTTP.ProxySettings proxy, int port, string deviceName)
        {
            var info = new Probe_Info();
            info.address = address;
            info.port = port;
            info.deviceName = deviceName;
            info.proxy = proxy;

            ThreadPool.QueueUserWorkItem(new WaitCallback(RunProbe_Worker), info);
        }

        void RunProbe_Worker(object o)
        {
            if (o != null)
            {
                var info = o as Probe_Info;
                if (info != null)
                {
                    string url = MTConnect.HTTP.GetUrl(info.address, info.port, info.deviceName) + "probe";

                    var returnData = MTConnect.Application.Components.Requests.Get(url, info.proxy, 2000, 1);
                    if (returnData != null)
                    {
                        probeHeader = returnData.Header;
                        SendProbeHeader(returnData.Header);

                        foreach (var device in returnData.Devices)
                        {
                            var dataItems = device.GetAllDataItems();

                            SendProbeDataItems(dataItems);
                            probeData = dataItems;
                        }
                    }
                }
            }
        }

        private void SendProbeHeader(MTConnect.Application.Headers.Devices header)
        {
            var data = new EventData(this);
            data.Id = "MTConnect_Probe_Header";
            data.Data02 = header;

            foreach (var page in ConfigurationPages)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    page.GetSentData(data);
                }), System.Windows.Threading.DispatcherPriority.Background, new object[] { });
            }
        }

        private EventData GetProbeHeader()
        {
            if (probeHeader != null)
            {
                var data = new EventData(this);
                data.Id = "MTConnect_Probe_Header";
                data.Data02 = probeHeader;

                return data;
            }

            return null;
        }

        private void SendProbeDataItems(List<MTConnect.Application.Components.DataItem> items)
        {
            var data = new EventData(this);
            data.Id = "MTConnect_Probe_DataItems";
            data.Data02 = items;

            foreach (var page in ConfigurationPages)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    page.GetSentData(data);
                }), System.Windows.Threading.DispatcherPriority.Background, new object[] { });     
            }
        }


        private EventData GetProbeDataItems()
        {
            if (probeData != null)
            {
                var data = new EventData(this);
                data.Id = "MTConnect_Probe_DataItems";
                data.Data02 = probeData;

                return data;
            }

            return null;           
        }

        #endregion

        private void Reload_Clicked(TrakHound_UI.Button bt)
        {
            LoadDevice();
        }
    }

}
