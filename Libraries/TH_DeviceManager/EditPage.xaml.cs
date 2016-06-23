// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

using TH_Configuration;
using TH_Global;
using TH_Global.Functions;
using TH_Plugins;
using TH_Plugins.Server;
using TH_UserManagement.Management;
using TH_WPF;

namespace TH_DeviceManager
{
    /// <summary>
    /// Page used to edit a device's Configuration object using a page/category based GUI
    /// </summary>
    public partial class EditPage : UserControl, IPage
    {
        public EditPage(Configuration config)
        {
            InitializeComponent();
            DataContext = this;

            if (config != null)
            {
                Configuration = config;
                ConfigurationTable = Converter.XMLToTable(config.ConfigurationXML);
            }

            LoadPages();
        }

        #region "IPage Interface"

        public string Title { get { return "Edit Device"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TH_DeviceManager;component/Resources/Edit_02.png")); } }


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

                var result = TH_WPF.MessageBox.Show(text, title, MessageBoxButtons.YesNoCancel);

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
        public Configuration Configuration { get; set; }

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

        private void Save_Clicked(TH_WPF.Button bt)
        {
            bt.Focus();

            if (Configuration != null)
            {
                DataTable dt = Converter.XMLToTable(Configuration.ConfigurationXML);
                dt.TableName = Configuration.TableName;

                Save(dt);
            }
        }

        private void Restore_Clicked(TH_WPF.Button bt) { RestorePages(); }

        private void DeviceManager_Clicked(TH_WPF.Button bt)
        {
            if (DeviceListSelected != null) DeviceListSelected();
        }

        private void EditTable_Clicked(TH_WPF.Button bt)
        {
            if (EditTableSelected != null) EditTableSelected(Configuration);
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

            DataTable dt = (DataTable)o;

            if (dt != null)
            {
                string tablename = null;

                if (dt != null)
                {
                    tablename = dt.TableName;

                    // Reset Update ID
                    Table_Functions.UpdateTableValue(String_Functions.RandomString(20), "/ClientUpdateId", dt);
                    Table_Functions.UpdateTableValue(String_Functions.RandomString(20), "/ServerUpdateId", dt);

                    if (DeviceManager.CurrentUser != null)
                    {
                        // Create backup in temp directory
                        XmlDocument backupXml = Converter.TableToXML(dt);
                        if (backupXml != null)
                        {
                            string temp_filename = DeviceManager.CurrentUser.Username + String_Functions.RandomString(20) + ".xml";

                            FileLocations.CreateTempDirectory();

                            string localPath = FileLocations.TrakHoundTemp + @"\" + temp_filename;

                            try { backupXml.Save(localPath); }
                            catch (Exception ex) { Logger.Log("Error during Configuration Xml Backup", Logger.LogLineType.Warning); }
                        }

                        success = Configurations.ClearConfigurationTable(tablename);
                        if (success) success = Configurations.UpdateConfigurationTable(tablename, dt);
                    }
                    // If not logged in Save to File in 'C:\TrakHound\'
                    else
                    {
                        success = Configuration.Save(dt);
                    }
                }

                ConfigurationTable = dt.Copy();

                XmlDocument xml = Converter.TableToXML(dt);
                if (xml != null)
                {
                    Configuration = Configuration.Read(xml);
                    Configuration.TableName = tablename;
                }
            }

            this.Dispatcher.BeginInvoke(new Action<bool>(Save_Finished), UI_Functions.PRIORITY_BACKGROUND, new object[] { success });
        }

        private void Save_Finished(bool success)
        {
            if (!success) TH_WPF.MessageBox.Show("Device did not save correctly. Try Again." + Environment.NewLine + @"A backup of the Device has been created in the 'C:\TrakHound\Temp directory'");

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

        public List<IConfigurationPage> ConfigurationPages = new List<IConfigurationPage>();

        private void LoadPages()
        {
            PagesLoading = true;

            if (pluginPageTypes == null)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(LoadPages_Worker));
            }
            else
            {
                foreach (var info in pluginInfos)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        AddPage(info);

                    }), UI_Functions.PRIORITY_BACKGROUND, new object[] { });
                }

                LoadPages_Finished();
            }

            GetProbeData(ConfigurationTable);
        }

        private void LoadPages_Worker(object o)
        {
            var types = GetPluginPageTypes();

            foreach (var info in pluginInfos)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    AddPage(info);

                }), UI_Functions.PRIORITY_BACKGROUND, new object[] { });
            }

            Dispatcher.BeginInvoke(new Action(LoadPages_Finished), UI_Functions.PRIORITY_BACKGROUND, new object[] { });
        }

        private void LoadPages_Finished()
        {
            PagesLoading = false;
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

        bool first = true;

        void AddPageButton(IConfigurationInfo info)
        {
            var bt = new ListButton();
            bt.Text = info.Title;

            if (info.Image != null) bt.Image = info.Image;
            else bt.Image = new BitmapImage(new Uri("pack://application:,,,/TH_DeviceManager;component/Resources/Plug_01.png"));

            bt.Selected += Page_Selected;
            bt.DataObject = info;

            if (first) Page_Selected(bt);
            first = false;

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
                    ConfigurationPages.Add(page);
                }

                LoadPage(page);

                foreach (ListButton olb in PageList) if (olb != lb) olb.IsSelected = false;
                lb.IsSelected = true;

                CurrentPage = page;
            }
        }

        #endregion

        #region "Plugins"

        static List<Type> pluginPageTypes;

        static List<IConfigurationInfo> pluginInfos = new List<IConfigurationInfo>();

        public List<Type> GetPluginPageTypes()
        {
            var result = pluginPageTypes;

            pluginInfos.Clear();

            if (result == null)
            {
                result = new List<Type>();

                string pluginsPath;

                // Load from System Directory first (easier for user to navigate to 'C:\TrakHound\')
                pluginsPath = FileLocations.Plugins;
                if (Directory.Exists(pluginsPath)) GetPluginPageTypes(pluginsPath, result);

                // Load from App root Directory (doesn't overwrite plugins found in System Directory)
                pluginsPath = AppDomain.CurrentDomain.BaseDirectory;
                if (Directory.Exists(pluginsPath)) GetPluginPageTypes(pluginsPath, result);

                pluginPageTypes = result;
            }

            return result;
        }

        private void GetPluginPageTypes(string path, List<Type> types)
        {
            if (Directory.Exists(path))
            {
                try
                {
                    var plugins = Reader.FindPlugins<IConfigurationInfo>(path, new ConfigurationInfoPlugin.PluginContainer());
                    foreach (var plugin in plugins)
                    {
                        var type = plugin.ConfigurationPageType;

                        if (!types.Exists(x => x.FullName == type.FullName))
                        {
                            pluginInfos.Add(plugin);

                            types.Add(type);
                        }
                    }
                }
                catch (Exception ex) { Logger.Log("LoadPlugins() : Exception : " + ex.Message, Logger.LogLineType.Error); }

                // Search Subdirectories
                foreach (string directory in Directory.GetDirectories(path))
                {
                    GetPluginPageTypes(directory, types);
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

            string ip = Table_Functions.GetTableValue(prefix + "Address", dt);
            // Get deprecated value if new value is not found
            if (String.IsNullOrEmpty(ip)) ip = Table_Functions.GetTableValue(prefix + "IP_Address", dt);

            string p = Table_Functions.GetTableValue(prefix + "Port", dt);

            string devicename = Table_Functions.GetTableValue(prefix + "DeviceName", dt);
            // Get deprecated value if new value is not found
            if (String.IsNullOrEmpty(devicename)) devicename = Table_Functions.GetTableValue(prefix + "Device_Name", dt);

            string proxyAddress = Table_Functions.GetTableValue(prefix + "ProxyAddress", dt);
            string proxyPort = Table_Functions.GetTableValue(prefix + "ProxyPort", dt);

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

        #endregion

    }

}
