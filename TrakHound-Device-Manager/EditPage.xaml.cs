// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

using TrakHound;
using TrakHound.Configurations;
using TrakHound.Configurations.Converters;
using TrakHound.Plugins;
using TrakHound.Plugins.Server;
using TrakHound.Tools;
using TrakHound_UI;

namespace TrakHound_Device_Manager
{
    /// <summary>
    /// Page used to edit a device's Configuration object using a page/category based GUI
    /// </summary>
    public partial class EditPage : UserControl, IPage
    {
        public EditPage(DeviceConfiguration config, DeviceManager deviceManager)
        {
            InitializeComponent();
            DataContext = this;

            DeviceManager = deviceManager;

            if (config != null)
            {
                Configuration = config;
                ConfigurationTable = DeviceConfigurationConverter.XMLToTable(config.Xml);
            }

            LoadPages();
        }

        #region "IPage Interface"

        public string Title { get { return "Edit Device"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TrakHound-Device-Manager;component/Resources/Edit_02.png")); } }


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

        #endregion

        /// <summary>
        /// Parent DeviceManager object
        /// </summary>
        public DeviceManager DeviceManager { get; set; }

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

        /// <summary>
        /// Event to request to open the Edit Table Page
        /// </summary>
        public event DeviceSelected_Handler EditTableSelected;


        const System.Windows.Threading.DispatcherPriority PRIORITY_BACKGROUND = System.Windows.Threading.DispatcherPriority.Background;


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

        private void EditTable_Clicked(TrakHound_UI.Button bt)
        {
            EditTableSelected?.Invoke(Configuration);
        }

        #endregion

        #region "Dependency Properties"

        public bool PagesLoading
        {
            get { return (bool)GetValue(PagesLoadingProperty); }
            set { SetValue(PagesLoadingProperty, value); }
        }

        public static readonly DependencyProperty PagesLoadingProperty =
            DependencyProperty.Register("PagesLoading", typeof(bool), typeof(EditPage), new PropertyMetadata(false));

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

        #region "Load / Save Device"

        private void LoadPage(IConfigurationPage page)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!page.Loaded) page.LoadConfiguration(ConfigurationTable.Copy());
                page.Loaded = true;
            }), UI_Functions.PRIORITY_BACKGROUND, new object[] { });
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

                if (DeviceManager.CurrentUser != null)
                {
                    success = TrakHound.API.Devices.Update(DeviceManager.CurrentUser, dt);
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

            Dispatcher.BeginInvoke(new Action<bool>(Save_Finished), UI_Functions.PRIORITY_BACKGROUND, new object[] { success });
        }

        private void Save_Finished(bool success)
        {
            if (!success) TrakHound_UI.MessageBox.Show("Device did not save correctly. Try Again." + Environment.NewLine + @"A backup of the Device has been created in the 'C:\TrakHound\Temp directory'");

            RestorePages();

            SaveNeeded = false;
            Saving = false;

            // Update Device in Device Manager
            var args = new DeviceManager.DeviceUpdateArgs();
            args.Event = DeviceManager.DeviceUpdateEvent.Changed;
            DeviceManager.UpdateDevice(Configuration, args);
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
            PagesLoading = true;

            AddPage(new Pages.Cycles.Info());
            AddPage(new Pages.Description.Info());
            AddPage(new Pages.MTConnectConfig.Info());
            AddPage(new Pages.GeneratedEvents.Info());
            AddPage(new Pages.InstanceData.Info());
            AddPage(new Pages.Parts.Info());

            LoadPages_Finished();          
        }

        private void LoadPages_Finished()
        {
            PagesLoading = false;

            GetProbeData(ConfigurationTable);
        }

        private void SendDeviceManagerData(IConfigurationPage page)
        {
            var data = new EventData();
            data.Id = "DEVICE_MANAGER";
            data.Data02 = DeviceManager;

            page.GetSentData(data);
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

            if (info.Image != null) bt.Image = info.Image;
            else bt.Image = new BitmapImage(new Uri("pack://application:,,,/TrakHound_Device_Manager;component/Resources/Plug_01.png"));

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
                    
                    SendDeviceManagerData(page);

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

        //static List<Type> pluginPageTypes;

        //static List<IConfigurationInfo> pluginInfos = new List<IConfigurationInfo>();

        //public List<Type> GetPluginPageTypes()
        //{
        //    var result = pluginPageTypes;

        //    pluginInfos.Clear();

        //    if (result == null)
        //    {
        //        result = new List<Type>();

        //        string pluginsPath;

        //        // Load from System Directory first (easier for user to navigate to 'C:\TrakHound\')
        //        pluginsPath = FileLocations.Plugins;
        //        if (Directory.Exists(pluginsPath)) GetPluginPageTypes(pluginsPath, result);

        //        // Load from App root Directory (doesn't overwrite plugins found in System Directory)
        //        pluginsPath = AppDomain.CurrentDomain.BaseDirectory;
        //        if (Directory.Exists(pluginsPath)) GetPluginPageTypes(pluginsPath, result);

        //        // Load from Current Assembly
        //        GetPluginPageTypes(Assembly.GetEntryAssembly(), result);

        //        pluginPageTypes = result;
        //    }

        //    return result;
        //}

        //private void GetPluginPageTypes(Assembly assembly, List<Type> types)
        //{
        //    try
        //    {
        //        var plugins = Reader.FindPlugins<IConfigurationInfo>(assembly, new ConfigurationInfoPlugin.PluginContainer());
        //        foreach (var plugin in plugins)
        //        {
        //            var type = plugin.ConfigurationPageType;

        //            if (!types.Exists(x => x.FullName == type.FullName))
        //            {
        //                pluginInfos.Add(plugin);

        //                types.Add(type);
        //            }
        //        }
        //    }
        //    catch (Exception ex) { Logger.Log("LoadPlugins() : Exception : " + ex.Message, LogLineType.Error); }
        //}

        //private void GetPluginPageTypes(string path, List<Type> types)
        //{
        //    if (Directory.Exists(path))
        //    {
        //        try
        //        {
        //            var plugins = Reader.FindPlugins<IConfigurationInfo>(path, new ConfigurationInfoPlugin.PluginContainer());
        //            foreach (var plugin in plugins)
        //            {
        //                var type = plugin.ConfigurationPageType;

        //                if (!types.Exists(x => x.FullName == type.FullName))
        //                {
        //                    pluginInfos.Add(plugin);

        //                    types.Add(type);
        //                }
        //            }
        //        }
        //        catch (Exception ex) { Logger.Log("LoadPlugins() : Exception : " + ex.Message, LogLineType.Error); }

        //        // Search Subdirectories
        //        foreach (string directory in Directory.GetDirectories(path))
        //        {
        //            GetPluginPageTypes(directory, types);
        //        }
        //    }
        //}
        
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
            var data = new EventData();
            data.Id = "MTConnect_Probe_Header";
            data.Data02 = header;

            foreach (var page in ConfigurationPages)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    page.GetSentData(data);
                }), PRIORITY_BACKGROUND, new object[] { });
            }
        }

        private EventData GetProbeHeader()
        {
            if (probeHeader != null)
            {
                var data = new EventData();
                data.Id = "MTConnect_Probe_Header";
                data.Data02 = probeHeader;

                return data;
            }

            return null;
        }

        private void SendProbeDataItems(List<MTConnect.Application.Components.DataItem> items)
        {
            var data = new EventData();
            data.Id = "MTConnect_Probe_DataItems";
            data.Data02 = items;

            foreach (var page in ConfigurationPages)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    page.GetSentData(data);
                }), PRIORITY_BACKGROUND, new object[] { });     
            }
        }


        private EventData GetProbeDataItems()
        {
            if (probeData != null)
            {
                var data = new EventData();
                data.Id = "MTConnect_Probe_DataItems";
                data.Data02 = probeData;

                return data;
            }

            return null;           
        }

        #endregion

    }

}
