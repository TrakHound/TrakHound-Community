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
using MTConnect;
using MTConnect.Application.Components;
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

        #region "New Find Devices"

        int nodesChecked = 0;

        ManualResetEvent stop;

        public void FindDevices()
        {
            DevicesLoading = true;
            DevicesAlreadyAdded = 0;
            DevicesNotAdded = 0;
            NetworkNodesFound = 0;
            nodesChecked = 0;
            DeviceInfos.Clear();

            SearchProgressValue = 0;
            SearchProgressMaximum = 0;

            stop = new ManualResetEvent(false);

            ThreadPool.QueueUserWorkItem(new WaitCallback(FindDevices_Worker));
        }

        public void FindDevices_Worker(object o)
        {
            var host = Network_Functions.GetHostIP();
            if (host != null)
            {
                var ping = new Network_Functions.PingNodes();
                ping.PingSuccessful += Ping_PingSuccessful;
                ping.Start();
            }
        }

        private void Ping_PingSuccessful(System.Net.NetworkInformation.PingReply reply)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                NetworkNodesFound++;
                SearchProgressMaximum += 20; // Increment number of ports to probe (20)

            }), UI_Functions.PRIORITY_BACKGROUND, new object[] { });

            for (var p = 5000; p < 5020; p++)
            {
                if (!stop.WaitOne(0, true))
                {
                    var info = new TestProbeInfo();
                    info.Address = reply.Address.ToString();
                    info.Port = p;

                    ThreadPool.QueueUserWorkItem(new WaitCallback(TestProbe), info);
                }
            }
        }

        private class TestProbeInfo
        {
            public string Address { get; set; }
            public int Port { get; set; }
        }

        private void TestProbe(object o)
        {
            var info = (TestProbeInfo)o;

            TestProbe(info.Address, info.Port);

            Dispatcher.BeginInvoke(new Action(() =>
            {
                nodesChecked++;

                SearchProgressValue++;

                if (nodesChecked >= NetworkNodesFound * 20)
                {
                    DevicesLoading = false;
                }

            }), UI_Functions.PRIORITY_BACKGROUND, new object[] { });
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

            var probeReturn = Requests.Get(url, 1000, 1);
            if (probeReturn != null && probeReturn.Devices != null)
            {
                foreach (var device in probeReturn.Devices)
                {
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
                                DevicesNotAdded++;

                                var info = new DeviceInfo(address, port, device);
                                DeviceInfos.Add(info);

                            }), UI_Functions.PRIORITY_BACKGROUND, new object[] { });
                        }
                        else
                        {
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                DevicesAlreadyAdded++;

                            }), UI_Functions.PRIORITY_BACKGROUND, new object[] { });
                        }
                    }
                }
            }
        }

        #endregion

        #region "New Add Device"

        private void AddDevice(DeviceInfo info)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(AddDevice_Worker), info);
        }

        private void AddDevice_Worker(object o)
        {
            var info = (DeviceInfo)o;

            bool success = false;

            if (info != null && info.Device != null)
            {
                var probeData = new TH_AutoGenerate.Configuration.ProbeData();
                probeData.Address = info.Address;
                probeData.Port = info.Port.ToString();
                probeData.Device = info.Device;

                var config = TH_AutoGenerate.Configuration.Create(probeData);

                // Add Device to user (or save to disk if local)
                if (ParentPage.DeviceManager.CurrentUser != null)
                {
                    var userConfig = UserConfiguration.FromNewUserConfiguration(ParentPage.DeviceManager.CurrentUser);

                    success = Configurations.AddConfigurationToUser(userConfig, config);
                }
                else
                {
                    success = DeviceConfiguration.Save(config);
                }
            }

            if (success)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    int i = DeviceInfos.ToList().FindIndex(x => x.Id == info.Id);
                    if (i >= 0) DeviceInfos.RemoveAt(i);

                }), UI_Functions.PRIORITY_BACKGROUND, new object[] { });
            }
            else
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    info.Loading = false;

                }), UI_Functions.PRIORITY_BACKGROUND, new object[] { });
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
            FindDevices();
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

        private void Cancel_Clicked(TH_WPF.Button bt)
        {
            if (stop != null) stop.Set();
            DevicesLoading = false;
        }
    }
}
