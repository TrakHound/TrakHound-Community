// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MTConnect;
using MTConnect.Application.Components;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using TrakHound;
using TrakHound.API.Users;
using TrakHound.Configurations;
using TrakHound.Configurations.AutoGenerate;
using TrakHound.Tools;

namespace TrakHound_Device_Manager.AddDevice.Pages
{
    /// <summary>
    /// Interaction logic for Manual.xaml
    /// </summary>
    public partial class Manual : UserControl, IPage
    {
        public Manual()
        {
            InitializeComponent();
            DataContext = this;
        }

        public Page ParentPage
        {
            get { return (Page)GetValue(ParentPageProperty); }
            set { SetValue(ParentPageProperty, value); }
        }

        public static readonly DependencyProperty ParentPageProperty =
            DependencyProperty.Register("ParentPage", typeof(Page), typeof(Manual), new PropertyMetadata(null));

        private UserConfiguration currentUser;


        #region "IPage"

        public string Title { get { return "Manual"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TrakHound-Device-Manager;component/Resources/Edit_02.png")); } }


        public void Opened() { }
        public bool Opening() { return true; }

        public void Closed() { }
        public bool Closing() { return true; }

        public event SendData_Handler SendData;

        public void GetSentData(EventData data)
        {
            Dispatcher.BeginInvoke(new Action<EventData>(UpdateLoggedInChanged), UI_Functions.PRIORITY_DATA_BIND, new object[] { data });
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

        #endregion

        #region "Dependency Properties"

        public string Address
        {
            get { return (string)GetValue(AddressProperty); }
            set { SetValue(AddressProperty, value); }
        }

        public static readonly DependencyProperty AddressProperty =
            DependencyProperty.Register("Address", typeof(string), typeof(Manual), new PropertyMetadata(null));

        public string Port
        {
            get { return (string)GetValue(PortProperty); }
            set { SetValue(PortProperty, value); }
        }

        public static readonly DependencyProperty PortProperty =
            DependencyProperty.Register("Port", typeof(string), typeof(Manual), new PropertyMetadata(null));

        public string DeviceName
        {
            get { return (string)GetValue(DeviceNameProperty); }
            set { SetValue(DeviceNameProperty, value); }
        }

        public static readonly DependencyProperty DeviceNameProperty =
            DependencyProperty.Register("DeviceName", typeof(string), typeof(Manual), new PropertyMetadata(null));

        public bool Loading
        {
            get { return (bool)GetValue(LoadingProperty); }
            set { SetValue(LoadingProperty, value); }
        }

        public static readonly DependencyProperty LoadingProperty =
            DependencyProperty.Register("Loading", typeof(bool), typeof(Manual), new PropertyMetadata(false));

        #endregion

        #region "Test Connection"

        private void TestConnection_Clicked(TrakHound_UI.Button bt)
        {
            TestConnection();
        }

        public bool ConnectionTestLoading
        {
            get { return (bool)GetValue(ConnectionTestLoadingProperty); }
            set { SetValue(ConnectionTestLoadingProperty, value); }
        }

        public static readonly DependencyProperty ConnectionTestLoadingProperty =
            DependencyProperty.Register("ConnectionTestLoading", typeof(bool), typeof(Manual), new PropertyMetadata(false));


        public int ConnectionTestResult
        {
            get { return (int)GetValue(ConnectionTestResultProperty); }
            set { SetValue(ConnectionTestResultProperty, value); }
        }

        public static readonly DependencyProperty ConnectionTestResultProperty =
            DependencyProperty.Register("ConnectionTestResult", typeof(int), typeof(Manual), new PropertyMetadata(0));


        public string ConnectionTestResultText
        {
            get { return (string)GetValue(ConnectionTestResultTextProperty); }
            set { SetValue(ConnectionTestResultTextProperty, value); }
        }

        public static readonly DependencyProperty ConnectionTestResultTextProperty =
            DependencyProperty.Register("ConnectionTestResultText", typeof(string), typeof(Manual), new PropertyMetadata(null));


        private class TestConnectionInfo
        {
            public string Address { get; set; }
            public int Port { get; set; }
            public string DeviceName { get; set; }
        }

        private Thread testConnectionThread;

        private void TestConnection()
        {
            var info = new TestConnectionInfo();
            info.Address = Address;
            int port = 0;
            if (int.TryParse(Port, out port)) { }
            else port = 80;
            info.Port = port;
            info.DeviceName = DeviceName;

            if (!string.IsNullOrEmpty(info.Address) && port > 0 && !string.IsNullOrEmpty(info.DeviceName))
            {
                ConnectionTestLoading = true;
                ConnectionTestResult = 0;
                ConnectionTestResultText = null;

                if (testConnectionThread != null) testConnectionThread.Abort();

                testConnectionThread = new Thread(new ParameterizedThreadStart(TestConnection_Worker));
                testConnectionThread.Start(info);
            }
            else
            {
                ConnectionTestResult = -1;
                ConnectionTestResultText = "Incorrect Information. Be sure to enter in the IP Address, Port, and Device Name and Try Again.";
            }
        }

        private void TestConnection_Worker(object o)
        {
            if (o != null)
            {
                var info = (TestConnectionInfo)o;

                var returnInfo = new TestConnectionReturnInfo();

                string url = HTTP.GetUrl(info.Address, info.Port, info.DeviceName) + "probe";

                ReturnData returnData = Requests.Get(url, 5000, 1);

                if (returnData != null)
                {
                    probeDevice = returnData.Devices[0];

                    returnInfo.Success = true;
                    returnInfo.Message = "MTConnect Probe Successful @ " + url;
                }
                else returnInfo.Message = "MTConnect Probe Failed @ " + url;

                Dispatcher.BeginInvoke(new Action<TestConnectionReturnInfo>(TestConnection_GUI), UI_Functions.PRIORITY_BACKGROUND, new object[] { returnInfo });
            }
        }

        private class TestConnectionReturnInfo
        {
            public bool Success { get; set; }
            public string Message { get; set; }
        }

        private void TestConnection_GUI(TestConnectionReturnInfo info)
        {
            if (info.Success) ConnectionTestResult = 1;
            else ConnectionTestResult = -1;

            ConnectionTestResultText = info.Message;
            ConnectionTestLoading = false;
        }

        private void TestConnectionCancel_Clicked(TrakHound_UI.Button bt)
        {
            if (testConnectionThread != null) testConnectionThread.Abort();

            ConnectionTestLoading = false;
        }

        #endregion

        private Device probeDevice;

        private void AddDevice_Clicked(TrakHound_UI.Button bt)
        {
            AddDevice();
        }

        private class AddDeviceInfo
        {
            public string Address { get; set; }
            public int Port { get; set; }
            public string DeviceName { get; set; }
            public UserConfiguration CurrentUser { get; set; }
        }

        private void AddDevice()
        {
            Loading = true;

            var info = new AddDeviceInfo();
            info.Address = Address;
            info.DeviceName = DeviceName;
            int port = 80;
            int.TryParse(Port, out port);
            info.Port = port;
            info.CurrentUser = currentUser;

            ThreadPool.QueueUserWorkItem(new WaitCallback(AddDevice_Worker), info);

            //if (ParentPage.DeviceManager != null)
            //{
            //    info.CurrentUser = ParentPage.DeviceManager.CurrentUser;

            //    ThreadPool.QueueUserWorkItem(new WaitCallback(AddDevice_Worker), info);
            //}    
        }

        private void AddDevice_Worker(object o)
        {
            var info = (AddDeviceInfo)o;

            bool success = false;
            DeviceConfiguration config = null;

            if (info != null)
            {
                string url = HTTP.GetUrl(info.Address, info.Port, info.DeviceName) + "probe";

                var returnData = Requests.Get(url, 5000, 1);
                if (returnData != null)
                {
                    var probeData = new Configuration.ProbeData();
                    probeData.Address = info.Address;
                    probeData.Port = info.Port.ToString();
                    probeData.Device = returnData.Devices[0];

                    config = Configuration.Create(probeData);

                    // Add Device to user (or save to disk if local)
                    if (info.CurrentUser != null)
                    {
                        success = TrakHound.API.Devices.Update(info.CurrentUser, config);
                    }
                    else
                    {
                        success = DeviceConfiguration.Save(config);
                    }
                }   
            }

            Dispatcher.BeginInvoke(new Action(() =>
            {
                Loading = false;

                if (success)
                {
                    // Send message that device was added
                    var data = new EventData();
                    data.Id = "DEVICE_ADDED";
                    data.Data01 = new DeviceDescription(config);
                    SendData?.Invoke(data);

                    // Add to DeviceManager
                    //if (config != null) ParentPage.DeviceManager.AddDevice(config);

                    TrakHound_UI.MessageBox.Show("Device added successfully!", "Add Device Successful", TrakHound_UI.MessageBoxButtons.Ok);
                }
                else TrakHound_UI.MessageBox.Show("Error during Add Device. Please Try Again.", "Add Device Error", TrakHound_UI.MessageBoxButtons.Ok);

            }), UI_Functions.PRIORITY_BACKGROUND, new object[] { });
        }

    }
}
