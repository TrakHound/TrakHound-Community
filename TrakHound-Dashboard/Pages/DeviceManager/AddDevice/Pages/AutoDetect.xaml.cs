// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MTConnect.Application.Components;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

        public bool ZoomEnabled { get { return false; } }

        public void SetZoom(double zoomPercentage) { }

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


        public bool DetailsShown
        {
            get { return (bool)GetValue(DetailsShownProperty); }
            set { SetValue(DetailsShownProperty, value); }
        }

        public static readonly DependencyProperty DetailsShownProperty =
            DependencyProperty.Register("DetailsShown", typeof(bool), typeof(AutoDetect), new PropertyMetadata(true));


        public string DetailsText
        {
            get { return (string)GetValue(DetailsTextProperty); }
            set { SetValue(DetailsTextProperty, value); }
        }

        public static readonly DependencyProperty DetailsTextProperty =
            DependencyProperty.Register("DetailsText", typeof(string), typeof(AutoDetect), new PropertyMetadata(null));


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
            LogItems.Clear();

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
                            AddtoLog(LogType.INFO, "Starting Auto Detect..");
                            AddtoLog(LogType.INFO, "IP Address Range : From " + start.ToString() + " to " + end.ToString());
                            AddtoLog(LogType.INFO, "Port Range : From " + portRangeStart.ToString() + " to " + portRangeStop.ToString());

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
            ping.PingError += Ping_PingError;
            ping.PingReplied += Ping_PingReplied;
            ping.Start();
        }

        private void Ping_PingError(IPAddress ip, string msg)
        {
            string format = "{0} : Exception : {1}";
            AddtoLog(LogType.PING, string.Format(format, ip.ToString(), msg));
        }

        private void Ping_PingReplied(IPAddress ip, System.Net.NetworkInformation.PingReply reply)
        {
            string format = "{0} : {1}ms : {2}";
            AddtoLog(LogType.PING, string.Format(format, ip.ToString(), reply.RoundtripTime, reply.Status.ToString()));

            if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    NetworkNodesFound++;
                    SearchProgressMaximum += (portRangeStop - portRangeStart); // Increment number of ports to probe

                }), System.Windows.Threading.DispatcherPriority.Background, new object[] { });

                for (var p = portRangeStart; p < ((portRangeStop - portRangeStart) + portRangeStart); p++)
                {
                    if (!stop.WaitOne(0, true))
                    {
                        var info = new TestInfo();
                        info.Address = reply.Address.ToString();
                        info.Port = p;

                        TestPort(info);
                    }
                    else break;
                }
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

                if (!stop.WaitOne(0, true))
                {
                    if (open) TestProbe(info.Address, info.Port);
                    else AddtoLog(LogType.PORT, "Port Closed :: " + info.Port);
                }

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    nodesChecked++;

                    SearchProgressValue++;

                    if (nodesChecked >= NetworkNodesFound * (portRangeStop - portRangeStart))
                    {
                        DevicesLoading = false;
                    }

                }), System.Windows.Threading.DispatcherPriority.Background, new object[] { });
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

            var probeReturn = Requests.Get(url, Math.Max(500, pingTimeout), 1);
            if (probeReturn != null && probeReturn.Devices != null)
            {
                AddtoLog(LogType.PROBE, "MTConnect Probe Successful : " + url);

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
                            }), System.Windows.Threading.DispatcherPriority.Background, new object[] { });
                        }
                        else
                        {
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                if (!stop.WaitOne(0, true)) DevicesAlreadyAdded++;

                            }), System.Windows.Threading.DispatcherPriority.Background, new object[] { });
                        }
                    }
                }
            }
            else
            {
                AddtoLog(LogType.PROBE, "MTConnect Probe Failed : " + url);
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
                if (currentUser != null) success = TrakHound.API.Devices.Update(currentUser, config);
                else success = DeviceConfiguration.Save(config);

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
                    }));
                }
            }

            Dispatcher.BeginInvoke(new Action(() =>
            {
                info.Loading = false;

            }), System.Windows.Threading.DispatcherPriority.Background, new object[] { });
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

        private void Search_Clicked(TrakHound_UI.Button bt)
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

        #region "Details Log"

        private ObservableCollection<LogItem> _logItems;
        public ObservableCollection<LogItem> LogItems
        {
            get
            {
                if (_logItems == null)
                {
                    _logItems = new ObservableCollection<LogItem>();
                }
                return _logItems;
            }
            set
            {
                _logItems = value;
            }
        }

        public enum LogType
        {
            INFO,
            PING,
            PORT,
            PROBE
        }

        public class LogItem : INotifyPropertyChanged
        {
            public LogItem(LogType type, string text)
            {
                Type = type;
                Text = text;
            }

            private LogType _type;
            public LogType Type
            {
                get { return _type; }
                set { SetField(ref _type, value, "Type"); }
            }

            private string _text;
            public string Text
            {
                get { return _text; }
                set { SetField(ref _text, value, "Text"); }
            }

            private bool _shown = true;
            public bool Shown
            {
                get { return _shown; }
                set { SetField(ref _shown, value, "Shown"); }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            protected bool SetField<T>(ref T field, T value, string propertyName)
            {
                if (EqualityComparer<T>.Default.Equals(field, value)) return false;
                field = value;
                OnPropertyChanged(propertyName);
                return true;
            }
        }

        private void AddtoLog(LogType type, string line)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                bool shown = false;

                switch (type)
                {
                    case LogType.PING: shown = DisplayPingLog; break;
                    case LogType.PORT: shown = DisplayPortLog; break;
                    case LogType.PROBE: shown = DisplayProbeLog; break;
                    default: shown = true; break;
                }

                var item = new LogItem(type, line);
                item.Shown = shown;

                LogItems.Add(item);           

            }), System.Windows.Threading.DispatcherPriority.Background, new object[] { });
        }


        public bool DisplayPingLog
        {
            get { return (bool)GetValue(DisplayPingLogProperty); }
            set { SetValue(DisplayPingLogProperty, value); }
        }

        public static readonly DependencyProperty DisplayPingLogProperty =
            DependencyProperty.Register("DisplayPingLog", typeof(bool), typeof(AutoDetect), new PropertyMetadata(true));

        public bool DisplayPortLog
        {
            get { return (bool)GetValue(DisplayPortLogProperty); }
            set { SetValue(DisplayPortLogProperty, value); }
        }

        public static readonly DependencyProperty DisplayPortLogProperty =
            DependencyProperty.Register("DisplayPortLog", typeof(bool), typeof(AutoDetect), new PropertyMetadata(true));

        public bool DisplayProbeLog
        {
            get { return (bool)GetValue(DisplayProbeLogProperty); }
            set { SetValue(DisplayProbeLogProperty, value); }
        }

        public static readonly DependencyProperty DisplayProbeLogProperty =
            DependencyProperty.Register("DisplayProbeLog", typeof(bool), typeof(AutoDetect), new PropertyMetadata(true));


        private void FilterLogItems()
        {
            lock(LogItems)
            {
                foreach (var logItem in LogItems)
                {
                    switch (logItem.Type)
                    {
                        case LogType.PING: logItem.Shown = DisplayPingLog; break;
                        case LogType.PORT: logItem.Shown = DisplayPortLog; break;
                        case LogType.PROBE: logItem.Shown = DisplayProbeLog; break;
                    }
                }
            }
        }

        private void LogFilterCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            FilterLogItems();
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
