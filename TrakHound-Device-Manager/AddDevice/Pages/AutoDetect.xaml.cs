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

using TrakHound.Configurations;
using TrakHound;
using TrakHound.Tools;
using MTConnect;
using MTConnect.Application.Components;
using TrakHound.Configurations.AutoGenerate;

namespace TrakHound_Device_Manager.AddDevice.Pages
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

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TrakHound-Device-Manager;component/Resources/Options_01.png")); } }


        public void Opened() { }
        public bool Opening() { return true; }

        public void Closed() { }
        public bool Closing()
        {
            Console.WriteLine("Auto Detect : Closing()");
            Cancel();
            return true;
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

            portRangeStart = StartPort;
            portRangeStop = EndPort;

            SearchProgressValue = 0;
            SearchProgressMaximum = 0;

            stop = new ManualResetEvent(false);

            ThreadPool.QueueUserWorkItem(new WaitCallback(FindDevices_Worker));
        }

        public void FindDevices_Worker(object o)
        {
            ping = new Network_Functions.PingNodes(100);
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

                bool open = Network_Functions.IsPortOpen(info.Address, info.Port, TimeSpan.FromMilliseconds(100));

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
                result.Address = XML_Functions.GetInnerText(config.ConfigurationXML, "//Agent/Address");

                int port = 80;
                int.TryParse(XML_Functions.GetInnerText(config.ConfigurationXML, "//Agent/Port"), out port);
                result.Port = port;

                result.DeviceName = XML_Functions.GetInnerText(config.ConfigurationXML, "//Agent/DeviceName");

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

                    if (ParentPage.DeviceManager != null && ParentPage.DeviceManager.Devices != null)
                    {
                        bool match = false;

                        // Check Device Manager to see if the Device has already been added
                        foreach (var addedDevice in ParentPage.DeviceManager.Devices.ToList())
                        {
                            var agentConfig = AgentConfiguration.Read(addedDevice);
                            if (agentConfig.Address == address && 
                                agentConfig.Port == port &&
                                agentConfig.DeviceName == device.Name)
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
                if (ParentPage.DeviceManager.CurrentUser != null)
                {
                    success = TrakHound.API.Devices.Update(ParentPage.DeviceManager.CurrentUser, config);
                }
                else
                {
                    success = DeviceConfiguration.Save(config);
                }

                if (success)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        // Add to DeviceManager
                        ParentPage.DeviceManager.AddDevice(config);

                        int i = DeviceInfos.ToList().FindIndex(x => x.Id == info.Id);
                        if (i >= 0) DeviceInfos.RemoveAt(i);

                        // Increment counters
                        DevicesNotAdded = DeviceInfos.Count;
                        DevicesAlreadyAdded += 1;

                    }), UI_Functions.PRIORITY_BACKGROUND, new object[] { });
                }
            }

            if (!success)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    info.Loading = false;

                }), UI_Functions.PRIORITY_BACKGROUND, new object[] { });
            }
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

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("AutoDetect : Unloaded()");
        }
    }
}
