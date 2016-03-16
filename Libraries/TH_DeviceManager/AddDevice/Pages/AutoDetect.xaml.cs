// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

using TH_Configuration;
using TH_Global;
using TH_Global.Functions;
using TH_MTConnect.Components;
using TH_UserManagement.Management;

namespace TH_DeviceManager.AddDevice.Pages
{
    /// <summary>
    /// Page containing options for adding Devices that were automatically found on the network
    /// </summary>
    public partial class AutoDetect : UserControl, IPage
    {
        public AutoDetect()
        {
            InitializeComponent();
            DataContext = this;
        }

        #region "IPage"

        public string Title { get { return "Auto Detect"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TH_DeviceManager;component/Resources/options_gear_30px.png")); } }


        public void Opened() { }
        public bool Opening() { return true; }

        public void Closed() { }
        public bool Closing() { return true; }

        #endregion

        /// <summary>
        /// Parent AddDevice.Page object
        /// </summary>
        public Page ParentPage { get; set; }

        #region "Dependency Properties"

        /// <summary>
        /// Used to tell if the Devices are currently being loaded
        /// </summary>
        public bool DevicesLoading
        {
            get { return (bool)GetValue(DevicesLoadingProperty); }
            set { SetValue(DevicesLoadingProperty, value); }
        }

        public static readonly DependencyProperty DevicesLoadingProperty =
            DependencyProperty.Register("DevicesLoading", typeof(bool), typeof(AutoDetect), new PropertyMetadata(true));

        /// <summary>
        /// Number of Devices that were found that have already been added
        /// </summary>
        public int DevicesAlreadyAdded
        {
            get { return (int)GetValue(DevicesAlreadyAddedProperty); }
            set { SetValue(DevicesAlreadyAddedProperty, value); }
        }

        public static readonly DependencyProperty DevicesAlreadyAddedProperty =
            DependencyProperty.Register("DevicesAlreadyAdded", typeof(int), typeof(AutoDetect), new PropertyMetadata(0));

        /// <summary>
        /// Number of Devices that were found but haven't been added yet
        /// </summary>
        public int DevicesNotAdded
        {
            get { return (int)GetValue(DevicesNotAddedProperty); }
            set { SetValue(DevicesNotAddedProperty, value); }
        }

        public static readonly DependencyProperty DevicesNotAddedProperty =
            DependencyProperty.Register("DevicesNotAdded", typeof(int), typeof(AutoDetect), new PropertyMetadata(0));

        #endregion

        const System.Windows.Threading.DispatcherPriority PRIORITY_BACKGROUND = System.Windows.Threading.DispatcherPriority.Background;
      
        /// <summary>
        /// Search the Network for Devices
        /// </summary>
        public void FindDevices()
        {
            LoadCatalog();
        }

        #region "Device Catalog"

        private class CatalogInfo
        {
            public Shared.SharedListItem Item { get; set; }
            public System.Drawing.Image Image { get; set; }
        }

        List<CatalogInfo> catalogInfos = new List<CatalogInfo>();

        Thread LoadCatalog_THREAD;

        private void LoadCatalog()
        {
            if (ParentPage != null && ParentPage.DeviceManager != null)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    DevicesLoading = true;
                    DeviceInfos.Clear();
                    catalogInfos.Clear();
                    DevicesAlreadyAdded = 0;
                    DevicesNotAdded = 0;
                }));

                if (LoadCatalog_THREAD != null) LoadCatalog_THREAD.Abort();

                LoadCatalog_THREAD = new Thread(new ThreadStart(LoadCatalog_Worker));
                LoadCatalog_THREAD.Start();
            }
        }

        private void LoadCatalog_Worker()
        {
            var sharedItems = Shared.GetSharedList();

            foreach (var item in sharedItems)
            {
                var info = new CatalogInfo();
                info.Item = item;
                info.Image = GetCatalogImage(item);

                catalogInfos.Add(info);
            }

            FindNodes();
        }

        private System.Drawing.Image GetCatalogImage(Shared.SharedListItem item)
        {
            System.Drawing.Image img = null;

            if (item.image_url != null)
            {
                // Just use Remote.Images (don't look for local)
                img = TH_UserManagement.Management.Remote.Images.GetImage(item.image_url);
            }

            return img;
        }

        #endregion

        #region "Network Nodes"

        private void FindNodes()
        {
            this.Dispatcher.BeginInvoke(new Action(DeviceInfos.Clear));

            var pingNodes = new Network_Functions.PingNodes();
            pingNodes.PingSuccessful += PingNodes_PingSuccessful;
            pingNodes.Finished += PingNodes_Finished;
            pingNodes.Start();
        }

        private void PingNodes_PingSuccessful(System.Net.NetworkInformation.PingReply reply)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(RunProbe), reply.Address);
        }

        private void PingNodes_Finished()
        {
            Dispatcher.BeginInvoke(new Action(() => { DevicesLoading = false; }));
        }

        #endregion

        #region "MTConnect Probe"

        private void RunProbe(object o)
        {
            var ip = (IPAddress)o;

            var ports = new int[] { 5000, 5001, 5002, 5003, 5004, 5005, 5006, 5007, 5008, 5009, 5010 };

            foreach (var port in ports)
            {
                RunProbe(ip, port);
            }
        }

        private bool RunProbe(IPAddress ip, int port)
        {
            bool result = false;

            string url = TH_MTConnect.HTTP.GetUrl(ip.ToString(), port, null);

            var probe = Requests.Get(url, null, 2000, 1);
            if (probe != null)
            {
                result = true;

                foreach (var device in probe.devices)
                {
                    this.Dispatcher.BeginInvoke(new Action<IPAddress, int, Device>(AddDeviceInfo), PRIORITY_BACKGROUND, new object[] { ip, port, device });
                }
            }

            return result;
        }

        #endregion

        #region "Device Infos"

        private ObservableCollection<DeviceInfo> _deviceInfos;
        public ObservableCollection<DeviceInfo> DeviceInfos
        {
            get
            {
                if (_deviceInfos == null) _deviceInfos = new ObservableCollection<DeviceInfo>();
                return _deviceInfos;
            }
            set
            {
                _deviceInfos = value;
            }
        }

        private void AddDeviceInfo(IPAddress address, int port, Device device)
        {
            // Check if already in DeviceManagerList.Devices
            bool alreadyAdded = ParentPage.DeviceManager.Devices.ToList().Exists(x =>
            x.Agent.Address == address.ToString() &&
            x.Agent.Port == port &&
            x.Agent.DeviceName == device.name
            );

            if (!alreadyAdded)
            {
                // Look for matching shared configuration
                var catalogInfo = catalogInfos.Find(x => GetLinkTagMatchValue(device, x));
                if (catalogInfo != null)
                {
                    var info = new DeviceInfo();
                    info.Id = String_Functions.RandomString(80);
                    info.IPAddress = address.ToString();
                    info.Port = port;
                    info.Device = device;
                    info.Image = GetSourceFromImage(catalogInfo.Image);
                    info.SharedListItem = catalogInfo.Item;

                    DeviceInfos.Add(info);

                    DevicesNotAdded = DeviceInfos.Count;
                }
            }
            else
            {
                DevicesAlreadyAdded += 1;
            }
        }

        private ImageSource GetSourceFromImage(System.Drawing.Image img)
        {
            BitmapImage bitmap = null;

            if (img != null)
            {
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(img);

                IntPtr bmpPt = bmp.GetHbitmap();
                BitmapSource bmpSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmpPt, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                bmpSource.Freeze();

                if (bmpSource.PixelWidth > bmpSource.PixelHeight)
                {
                    bitmap = TH_WPF.Image_Functions.SetImageSize(bmpSource, 100);
                }
                else
                {
                    bitmap = TH_WPF.Image_Functions.SetImageSize(bmpSource, 0, 40);
                }
            }

            return bitmap;
        }

        /// <summary>
        /// Matching Syntax:
        /// [MANUFACTURER].[MODEL].[DEVICENAME]
        /// </summary>
        /// <param name="device"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        private static bool GetLinkTagMatchValue(Device device, CatalogInfo info)
        {
            if (info != null && info.Item != null && info.Item.link_tag != null && device != null)
            {
                string x = info.Item.link_tag.ToLower();

                // Add ability to link to multiple values. Separate values with semi colons.

                if (x == ToLower(device.description.manufacturer)) return true;
                if (x == ToLower(device.description.manufacturer) + "." + ToLower(device.description.model)) return true;
                if (x == ToLower(device.description.manufacturer) + "." + ToLower(device.description.model) + "." + ToLower(device.name)) return true;
                if (x == ToLower(device.description.manufacturer) + "." + ToLower(device.name)) return true;
                if (x == ToLower(device.name)) return true;
            }

            return false;
        }

        private static bool MatchLike(string s1, string s2)
        {
            if (s1 != null && s2 != null)
            {
                if (s1.StartsWith("%") && s1.EndsWith("%")) return MatchContains(s1, s2);
                if (s1.StartsWith("%")) return MatchStartsWith(s1, s2);
                if (s1.EndsWith("%")) return MatchEndsWith(s1, s2);
            }
            return false;
        }

        private static bool MatchContains(string s1, string s2)
        {
            if (s1 != null && s2 != null)
            {
                var x = s1.ToLower();
                var y = s2.ToLower();

                return x.Contains(y);
            }
            return false;
        }

        private static bool MatchStartsWith(string s1, string s2)
        {
            if (s1 != null && s2 != null)
            {
                var x = s1.ToLower();
                var y = s2.ToLower();

                return x.StartsWith(y);
            }
            return false;
        }

        private static bool MatchEndsWith(string s1, string s2)
        {
            if (s1 != null && s2 != null)
            {
                var x = s1.ToLower();
                var y = s2.ToLower();

                return x.EndsWith(y);
            }
            return false;
        }

        private static string ToLower(string s)
        {
            if (s != null) return s.ToLower();
            return s;
        }

        #endregion

        #region "Add Device"

        class AddDevice_Return
        {
            public DeviceInfo DeviceInfo { get; set; }
            public Configuration Configuration { get; set; }
            public bool Success { get; set; }
        }

        void AddDevice(DeviceInfo info)
        {
            if (ParentPage != null && ParentPage.DeviceManager != null)
            {
                info.Loading = true;
                Devices_DG.Items.Refresh();

                ThreadPool.QueueUserWorkItem(new WaitCallback(AddDevice_Worker), info);
            }
        }

        void AddDevice_Worker(object o)
        {
            var result = new AddDevice_Return();
            result.Success = false;

            if (o != null)
            {
                var info = (DeviceInfo)o;

                result.DeviceInfo = info;

                if (info.SharedListItem != null)
                {
                    string tablename = info.SharedListItem.tablename;

                    if (tablename != null)
                    {
                        // Update the number of times this Shared Configuration has been downloaded
                        Shared.UpdateDownloads(info.SharedListItem);

                        // Download the shared configuration from the TrakHound Device Catalog
                        DataTable dt = TH_UserManagement.Management.Remote.Configurations.GetTable(tablename);
                        if (dt != null)
                        {
                            XmlDocument xml = Converter.TableToXML(dt);
                            if (xml != null)
                            {
                                Configuration config = Configuration.Read(xml);
                                if (config != null)
                                {
                                    // Update Configuration to be Enabled, use the selected Agent Settings, and
                                    // if no databases are set then create new SQLite database configs
                                    UpdateEnabled(config);
                                    UpdateAgentConfiguration(info, config);
                                    UpdateDatabaseConfiguration(config);

                                    result.Configuration = config;

                                    if (UserManagementSettings.Database != null)
                                    {
                                        SaveLocalImage(config.FileLocations.Manufacturer_Logo_Path);
                                        SaveLocalImage(config.FileLocations.Image_Path);
                                    }

                                    // Add page to user (or save to disk if local)
                                    if (ParentPage.DeviceManager.CurrentUser != null)
                                    {
                                        result.Success = Configurations.AddConfigurationToUser(ParentPage.DeviceManager.CurrentUser, config);

                                        result.Configuration.TableName = config.TableName;
                                    }
                                    else
                                    {
                                        result.Success = SaveLocalConfigurationToUser(config);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            this.Dispatcher.BeginInvoke(new Action<AddDevice_Return>(AddDevice_GUI), PRIORITY_BACKGROUND, new object[] { result });
        }


        void SaveLocalImage(string filename)
        {
            if (filename != null)
            {
                var image = TH_UserManagement.Management.Remote.Images.GetImage(filename);
                if (image != null)
                {
                    string savePath = FileLocations.TrakHoundTemp + "\\" + filename;

                    image.Save(savePath);
                }
            }
        }

        private void UpdateEnabled(Configuration config)
        {
            config.ClientEnabled = true;
            XML_Functions.SetInnerText(config.ConfigurationXML, "/ClientEnabled", "True");

            config.ServerEnabled = true;
            XML_Functions.SetInnerText(config.ConfigurationXML, "/ServerEnabled", "True");
        }

        private void UpdateAgentConfiguration(DeviceInfo info, Configuration config)
        {
            // Save IP Address
            config.Agent.Address = info.IPAddress;
            XML_Functions.SetInnerText(config.ConfigurationXML, "/Agent/Address", info.IPAddress);

            // Save Port
            config.Agent.Port = info.Port;
            XML_Functions.SetInnerText(config.ConfigurationXML, "/Agent/Port", info.Port.ToString());

            // Save DeviceName
            config.Agent.DeviceName = info.Device.name;
            XML_Functions.SetInnerText(config.ConfigurationXML, "/Agent/DeviceName", info.Device.name);

            // Save Heartbeat
            config.Agent.DeviceName = info.Device.name;
            XML_Functions.SetInnerText(config.ConfigurationXML, "/Agent/Heartbeat", "5000");
        }

        private void UpdateDatabaseConfiguration(Configuration config)
        {
            // If NO databases are configured then add a SQLite database for both client and server
            if (config.Databases_Client.Databases.Count == 0 && config.Databases_Server.Databases.Count == 0)
            {
                config.DatabaseId = Configuration.GenerateDatabaseId();

                AddDatabaseConfiguration("/Databases_Client", config);
                AddDatabaseConfiguration("/Databases_Server", config);
            }
        }

        private void AddDatabaseConfiguration(string prefix, Configuration config)
        {
            XML_Functions.AddNode(config.ConfigurationXML, prefix + "/SQLite");
            XML_Functions.SetAttribute(config.ConfigurationXML, prefix + "/SQLite", "id", "00");

            string path = FileLocations.Databases + "\\" + config.UniqueId + ".xml";

            XML_Functions.SetInnerText(config.ConfigurationXML, prefix + "/SQLite/DatabasePath", path);
        }

        private bool SaveLocalConfigurationToUser(Configuration config)
        {
            bool result = false;

            // Set new Unique Id
            string uniqueId = Configuration.GenerateUniqueID();
            config.UniqueId = uniqueId;
            XML_Functions.SetInnerText(config.ConfigurationXML, "UniqueId", uniqueId);

            // Set new FilePath
            config.FilePath = uniqueId;
            XML_Functions.SetInnerText(config.ConfigurationXML, "FilePath", config.FilePath);

            result = Configuration.Save(config);

            return result;
        }


        void AddDevice_GUI(AddDevice_Return result)
        {
            if (result.Success && result.Configuration != null)
            {
                // Add to DeviceManager
                ParentPage.DeviceManager.AddDevice(result.Configuration);

                // Remove from AutoDetect list
                int index = DeviceInfos.ToList().FindIndex(x => x.Id == result.DeviceInfo.Id);
                if (index >= 0) DeviceInfos.RemoveAt(index);

                // Increment counters
                DevicesNotAdded = DeviceInfos.Count;
                DevicesAlreadyAdded += 1;
            }
            else
            {
                if (result.DeviceInfo != null) result.DeviceInfo.Loading = false;
                TH_WPF.MessageBox.Show("Add device failed. Try Again.", "Add device failed", TH_WPF.MessageBoxButtons.Ok);
            }
        }

        #endregion

        #region "Buttons"

        private void Add_Clicked(TH_WPF.Button bt)
        {
            if (bt.DataObject != null)
            {
                var info = (DeviceInfo)bt.DataObject;

                AddDevice(info);
            }
        }

        private void Refresh_Clicked(TH_WPF.Button bt)
        {
            LoadCatalog();
        }

        private void AddAll_Clicked(TH_WPF.Button bt)
        {
            foreach (var info in DeviceInfos)
            {
                AddDevice(info);
            }
        }

        #endregion

        private void DeviceManager_Clicked(TH_WPF.Button bt) { if (ParentPage != null) ParentPage.OpenDeviceList(); }

        private void AddDevicesManually_Clicked(TH_WPF.Button bt) { if (ParentPage != null) ParentPage.ShowManual(); }

    }
}
