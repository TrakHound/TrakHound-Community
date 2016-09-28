// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MTConnect.Application.Components;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

using TrakHound;
using TrakHound.API.Users;
using TrakHound.Configurations;
using TrakHound.Configurations.AutoGenerate;
using TrakHound.Tools;

namespace TrakHound_Dashboard.Pages.DeviceManager.AddDevice.Pages
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

            PingTimeout = Math.Max(100, Properties.Settings.Default.AutoDetectPingTimeout);

            StartPort = Properties.Settings.Default.AutoDetectStartPort;
            EndPort = Properties.Settings.Default.AutoDetectEndPort;

            StartAddress = Properties.Settings.Default.AutoDetectStartAddress;
            EndAddress = Properties.Settings.Default.AutoDetectEndAddress;

            if (string.IsNullOrEmpty(StartAddress) || string.IsNullOrEmpty(EndAddress))
            {
                SetDefaultAddressRange();
            }
        }

        #region "IPage"

        public string Title { get { return "Auto Detect"; } }

        public Uri Image { get { return new Uri("pack://application:,,,/TrakHound-Dashboard;component/Resources/Options_01.png"); } }


        public void Opened() { }
        public bool Opening() { return true; }

        public void Closed() { }
        public bool Closing()
        {
            // Save Auto Detect Setttings

            Properties.Settings.Default.AutoDetectStartPort = StartPort;
            Properties.Settings.Default.AutoDetectEndPort = EndPort;

            Properties.Settings.Default.AutoDetectPingTimeout = PingTimeout;

            Properties.Settings.Default.AutoDetectStartAddress = StartAddress;
            Properties.Settings.Default.AutoDetectEndAddress = EndAddress;

            Properties.Settings.Default.Save();

            Cancel();
            return true;
        }

        public event SendData_Handler SendData;


        private ObservableCollection<DeviceDescription> _devices;
        /// <summary>
        /// Collection of TrakHound.Configurations.Configuration objects that represent the active devices
        /// </summary>
        public ObservableCollection<DeviceDescription> Devices
        {
            get
            {
                if (_devices == null)
                    _devices = new ObservableCollection<DeviceDescription>();
                return _devices;
            }

            set
            {
                _devices = value;
            }
        }

        private UserConfiguration currentUser;


        public void GetSentData(EventData data)
        {
            Dispatcher.BeginInvoke(new Action<EventData>(UpdateLoggedInChanged), System.Windows.Threading.DispatcherPriority.DataBind, new object[] { data });
            Dispatcher.BeginInvoke(new Action<EventData>(UpdateDevicesLoading), System.Windows.Threading.DispatcherPriority.Normal, new object[] { data });

            Dispatcher.BeginInvoke(new Action<EventData>(UpdateDeviceAdded), System.Windows.Threading.DispatcherPriority.DataBind, new object[] { data });
            Dispatcher.BeginInvoke(new Action<EventData>(UpdateDeviceUpdated), System.Windows.Threading.DispatcherPriority.DataBind, new object[] { data });
            Dispatcher.BeginInvoke(new Action<EventData>(UpdateDeviceRemoved), System.Windows.Threading.DispatcherPriority.DataBind, new object[] { data });
        }

        void UpdateLoggedInChanged(EventData data)
        {
            if (data != null)
            {
                if (data.Id == "USER_LOGIN")
                {
                    if (data.Data01 != null) currentUser = (UserConfiguration)data.Data01;
                }
                else if (data.Id == "USER_LOGOUT")
                {
                    currentUser = null;
                }
            }
        }

        void UpdateDevicesLoading(EventData data)
        {
            if (data != null)
            {
                if (data.Id == "DEVICES_LOADING")
                {
                    DevicesLoading = true;
                    ClearDevices();
                }

                if (data.Id == "DEVICES_LOADED")
                {
                    DevicesLoading = false;
                }
            }
        }

        void UpdateDeviceAdded(EventData data)
        {
            if (data != null)
            {
                if (data.Id == "DEVICE_ADDED" && data.Data01 != null)
                {
                    Devices.Add((DeviceDescription)data.Data01);
                }
            }
        }

        void UpdateDeviceUpdated(EventData data)
        {
            if (data != null)
            {
                if (data.Id == "DEVICE_UPDATED" && data.Data01 != null)
                {
                    var device = (DeviceDescription)data.Data01;

                    int i = Devices.ToList().FindIndex(x => x.UniqueId == device.UniqueId);
                    if (i >= 0)
                    {
                        Devices.RemoveAt(i);
                        Devices.Insert(i, device);
                    }
                }
            }
        }

        void UpdateDeviceRemoved(EventData data)
        {
            if (data != null)
            {
                if (data.Id == "DEVICE_REMOVED" && data.Data01 != null)
                {
                    var device = (DeviceDescription)data.Data01;

                    int i = Devices.ToList().FindIndex(x => x.UniqueId == device.UniqueId);
                    if (i >= 0)
                    {
                        Devices.RemoveAt(i);
                    }
                }
            }
        }

        private void ClearDevices()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Devices.Clear();
            }
            ), System.Windows.Threading.DispatcherPriority.DataBind, new object[] { });
        }

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
            DependencyProperty.Register("DevicesLoading", typeof(bool), typeof(AutoDetect), new PropertyMetadata(false));

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

        /// <summary>
        /// Number of Network Nodes found using Ping
        /// </summary>
        public int NetworkNodesFound
        {
            get { return (int)GetValue(NetworkNodesFoundProperty); }
            set { SetValue(NetworkNodesFoundProperty, value); }
        }

        public static readonly DependencyProperty NetworkNodesFoundProperty =
            DependencyProperty.Register("NetworkNodesFound", typeof(int), typeof(AutoDetect), new PropertyMetadata(0));


        public double SearchProgressValue
        {
            get { return (double)GetValue(SearchProgressValueProperty); }
            set { SetValue(SearchProgressValueProperty, value); }
        }

        public static readonly DependencyProperty SearchProgressValueProperty =
            DependencyProperty.Register("SearchProgressValue", typeof(double), typeof(AutoDetect), new PropertyMetadata(0d));


        public double SearchProgressMaximum
        {
            get { return (double)GetValue(SearchProgressMaximumProperty); }
            set { SetValue(SearchProgressMaximumProperty, value); }
        }

        public static readonly DependencyProperty SearchProgressMaximumProperty =
            DependencyProperty.Register("SearchProgressMaximum", typeof(double), typeof(AutoDetect), new PropertyMetadata(1d));


        public string StartAddress
        {
            get { return (string)GetValue(StartAddressProperty); }
            set { SetValue(StartAddressProperty, value); }
        }

        public static readonly DependencyProperty StartAddressProperty =
            DependencyProperty.Register("StartAddress", typeof(string), typeof(AutoDetect), new PropertyMetadata(null));

        public string EndAddress
        {
            get { return (string)GetValue(EndAddressProperty); }
            set { SetValue(EndAddressProperty, value); }
        }

        public static readonly DependencyProperty EndAddressProperty =
            DependencyProperty.Register("EndAddress", typeof(string), typeof(AutoDetect), new PropertyMetadata(null));


        public int StartPort
        {
            get { return (int)GetValue(StartPortProperty); }
            set { SetValue(StartPortProperty, value); }
        }

        public static readonly DependencyProperty StartPortProperty =
            DependencyProperty.Register("StartPort", typeof(int), typeof(AutoDetect), new PropertyMetadata(5000));


        public int EndPort
        {
            get { return (int)GetValue(EndPortProperty); }
            set { SetValue(EndPortProperty, value); }
        }

        public static readonly DependencyProperty EndPortProperty =
            DependencyProperty.Register("EndPort", typeof(int), typeof(AutoDetect), new PropertyMetadata(5020));


        public int PingTimeout
        {
            get { return (int)GetValue(PingTimeoutProperty); }
            set { SetValue(PingTimeoutProperty, value); }
        }

        public static readonly DependencyProperty PingTimeoutProperty =
            DependencyProperty.Register("PingTimeout", typeof(int), typeof(AutoDetect), new PropertyMetadata(100));

        #endregion

        const System.Windows.Threading.DispatcherPriority PRIORITY_BACKGROUND = System.Windows.Threading.DispatcherPriority.Background;

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

        #region "Find Devices"

        int nodesChecked = 0;

        int portRangeStart = 5000;
        int portRangeStop = 5020;

        int pingTimeout = 100;

        List<IPAddress> addressRange;

        ManualResetEvent stop;
        Network_Functions.PingNodes ping;

        public void FindDevices()
        {
            DevicesLoading = true;
            DevicesAlreadyAdded = 0;
            DevicesNotAdded = 0;
            NetworkNodesFound = 0;
            nodesChecked = 0;
            DeviceInfos.Clear();
            addressRange = null;

            pingTimeout = PingTimeout;

            var hostIp = Network_Functions.GetHostIP();
            var hostSubnet = Network_Functions.GetSubnetMask(hostIp);

            if (hostIp != null && hostSubnet != null)
            {
                var ipNetwork = IPNetwork.Parse(hostIp, hostSubnet);
                if (ipNetwork != null)
                {
                    var ips = IPNetwork.ListIPAddress(ipNetwork).ToList();
                    if (ips != null && ips.Count > 0)
                    {
                        // Get Start Address
                        IPAddress start = null;
                        IPAddress.TryParse(StartAddress, out start);

                        // Get End Address
                        IPAddress end = null;
                        IPAddress.TryParse(EndAddress, out end);

                        if (start != null && end != null)
                        {
                            var range = new Network_Functions.IPAddressRange(start, end);

                            addressRange = ips.FindAll(o => range.IsInRange(o));

                            int localhostIndex = addressRange.FindIndex(o => o.ToString() == hostIp.ToString());
                            if (localhostIndex >= 0) addressRange[localhostIndex] = IPAddress.Loopback;
                        }
                    }
                }
            }

            portRangeStart = StartPort;
            portRangeStop = EndPort;

            SearchProgressValue = 0;
            SearchProgressMaximum = 0;

            stop = new ManualResetEvent(false);

            ThreadPool.QueueUserWorkItem(new WaitCallback(FindDevices_Worker));
        }

        public void FindDevices_Worker(object o)
        {
            ping = new Network_Functions.PingNodes(addressRange, pingTimeout);
            ping.PingSuccessful += Ping_PingSuccessful;
            ping.Start();
        }

        private void Ping_PingSuccessful(System.Net.NetworkInformation.PingReply reply)
        {
            Console.WriteLine(reply.Address + " Ping Returned Successful");

            Dispatcher.BeginInvoke(new Action(() =>
            {
                NetworkNodesFound++;
                SearchProgressMaximum += (portRangeStop - portRangeStart); // Increment number of ports to probe

            }), UI_Functions.PRIORITY_BACKGROUND, new object[] { });

            for (var p = portRangeStart; p < ((portRangeStop - portRangeStart) + portRangeStart); p++)
            {
                if (!stop.WaitOne(0, true))
                {
                    var info = new TestInfo();
                    info.Address = reply.Address.ToString();
                    info.Port = p;

                    ThreadPool.QueueUserWorkItem(new WaitCallback(TestPort), info);
                }
                else break;
            }
        }

        private class TestInfo
        {
            public string Address { get; set; }
            public int Port { get; set; }
        }

        private void TestPort(object o)
        {
            if (o != null)
            {
                var info = (TestInfo)o;

                bool open = Network_Functions.IsPortOpen(info.Address, info.Port, pingTimeout);

                if (open && !stop.WaitOne(0, true))
                {
                    TestProbe(info.Address, info.Port);
                }

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    nodesChecked++;

                    SearchProgressValue++;

                    if (nodesChecked >= NetworkNodesFound * (portRangeStop - portRangeStart))
                    {
                        DevicesLoading = false;
                    }

                }), UI_Functions.PRIORITY_BACKGROUND, new object[] { });
            }
        }

        private void TestProbe(object o)
        {
            var info = (TestInfo)o;

            if (!stop.WaitOne(0, true))
            {
                TestProbe(info.Address, info.Port);
            }
        }

        private class AgentConfiguration
        {
            public string Address { get; set; }
            public int Port { get; set; }
            public string DeviceName { get; set; }

            public static AgentConfiguration Read(DeviceConfiguration config)
            {
                var result = new AgentConfiguration();
                result.Address = XML_Functions.GetInnerText(config.Xml, "//Agent/Address");

                int port = 80;
                int.TryParse(XML_Functions.GetInnerText(config.Xml, "//Agent/Port"), out port);
                result.Port = port;

                result.DeviceName = XML_Functions.GetInnerText(config.Xml, "//Agent/DeviceName");

                return result;
            }
        }

        private void TestProbe(string address, int port)
        {
            string url = "http://" + address + ":" + port;

            var probeReturn = Requests.Get(url, 500, 1);
            if (probeReturn != null && probeReturn.Devices != null)
            {
                foreach (var device in probeReturn.Devices)
                {
                    if (stop.WaitOne(0, true)) break;

                    if (Devices != null)
                    {
                        bool match = false;

                        // Check Device List to see if the Device has already been added
                        foreach (var addedDevice in Devices.ToList())
                        {
                            if (addedDevice.Agent != null && 
                                addedDevice.Agent.Address == address &&
                                addedDevice.Agent.Port == port &&
                                addedDevice.Agent.DeviceName == device.Name)
                            {
                                match = true;
                                break;
                            }
                        }

                        // If Device is not already added then add it
                        if (!match)
                        {
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                if (!stop.WaitOne(0, true))
                                {
                                    DevicesNotAdded++;

                                    var info = new DeviceInfo(address, port, device);
                                    DeviceInfos.Add(info);
                                }
                            }), UI_Functions.PRIORITY_BACKGROUND, new object[] { });
                        }
                        else
                        {
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                if (!stop.WaitOne(0, true)) DevicesAlreadyAdded++;

                            }), UI_Functions.PRIORITY_BACKGROUND, new object[] { });
                        }
                    }
                }
            }
        }

        #endregion

        #region "Add Device"

        private void AddDevice(DeviceInfo info)
        {
            info.Loading = true;

            ThreadPool.QueueUserWorkItem(new WaitCallback(AddDevice_Worker), info);
        }

        private void AddDevice_Worker(object o)
        {
            var info = (DeviceInfo)o;

            bool success = false;

            if (info != null && info.Device != null)
            {
                var probeData = new Configuration.ProbeData();
                probeData.Address = info.Address;
                probeData.Port = info.Port.ToString();
                probeData.Device = info.Device;

                var config = Configuration.Create(probeData);

                // Add Device to user (or save to disk if local)
                if (currentUser != null)
                {
                    success = TrakHound.API.Devices.Update(currentUser, config);
                }
                else
                {
                    success = DeviceConfiguration.Save(config);
                }

                if (success)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        // Send message that device was added
                        var data = new EventData(this);
                        data.Id = "DEVICE_ADDED";
                        data.Data01 = new DeviceDescription(config);
                        SendData?.Invoke(data);

                        int i = DeviceInfos.ToList().FindIndex(x => x.Id == info.Id);
                        if (i >= 0) DeviceInfos.RemoveAt(i);

                        // Increment counters
                        DevicesNotAdded = DeviceInfos.Count;
                        DevicesAlreadyAdded += 1;

                    }), UI_Functions.PRIORITY_BACKGROUND, new object[] { });
                }
            }

            Dispatcher.BeginInvoke(new Action(() =>
            {
                info.Loading = false;

            }), UI_Functions.PRIORITY_BACKGROUND, new object[] { });
        }

        #endregion
        
        #region "Buttons"

        private void Add_Clicked(TrakHound_UI.Button bt)
        {
            if (bt.DataObject != null)
            {
                var info = (DeviceInfo)bt.DataObject;
                if (!info.Loading) AddDevice(info);
            }
        }

        private void Refresh_Clicked(TrakHound_UI.Button bt)
        {
            FindDevices();
        }

        private void AddAll_Clicked(TrakHound_UI.Button bt)
        {
            foreach (var info in DeviceInfos)
            {
                AddDevice(info);
            }
        }

        #endregion

        private void DeviceManager_Clicked(TrakHound_UI.Button bt) { if (ParentPage != null) ParentPage.OpenDeviceList(); }

        private void AddDevicesManually_Clicked(TrakHound_UI.Button bt) { if (ParentPage != null) ParentPage.ShowManual(); }

        private void ResetDefault_Clicked(TrakHound_UI.Button bt)
        {
            PingTimeout = 100;
            StartPort = 5000;
            EndPort = 5020;

            SetDefaultAddressRange();
        }

        private void SetDefaultAddressRange()
        {
            var hostIp = Network_Functions.GetHostIP();
            var hostSubnet = Network_Functions.GetSubnetMask(hostIp);

            if (hostIp != null && hostSubnet != null)
            {
                var ipNetwork = IPNetwork.Parse(hostIp, hostSubnet);
                if (ipNetwork != null)
                {
                    var ips = IPNetwork.ListIPAddress(ipNetwork).ToList();
                    if (ips != null && ips.Count > 0)
                    {
                        StartAddress = ips[0].ToString();
                        EndAddress = ips[ips.Count - 1].ToString();
                    }
                }
            }
        }

        private void Cancel_Clicked(TrakHound_UI.Button bt)
        {
            Cancel();
        }

        private void Cancel()
        {
            if (stop != null) stop.Set();
            if (ping != null) ping.Cancel();
            DevicesLoading = false;
        }
    }
}
