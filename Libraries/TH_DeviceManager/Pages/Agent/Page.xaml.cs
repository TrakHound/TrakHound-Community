// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

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

using System.Collections.ObjectModel;
using System.Data;
using System.Threading;

using TH_Configuration;
using TH_Global;
using TH_Global.Functions;
using TH_MTConnect.Components;
using TH_Plugins_Server;
using TH_UserManagement;
using TH_UserManagement.Management;

namespace TH_DeviceManager.Pages.Agent
{
    /// <summary>
    /// Interaction logic for Page.xaml
    /// </summary>
    public partial class Page : UserControl, ConfigurationPage
    {
        public Page()
        {
            InitializeComponent();
            DataContext = this;
            ((StackPanel)ProxySettings.PageContent).DataContext = this;
            ((StackPanel)AgentInfo.PageContent).DataContext = this;
        }

        #region "Page Interface"

        public string PageName { get { return "Agent"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TH_DeviceManager;component/Resources/MTConnect_01.png")); } }

        public UserConfiguration currentUser { get; set; }

        public event SaveRequest_Handler SaveRequest;

        public event SettingChanged_Handler SettingChanged;

        public void LoadConfiguration(DataTable dt)
        {
            Loading = true;

            configurationTable = dt;

            // Load Cloud Settings
            bool cloud = false;
            string cloud_str = Table_Functions.GetTableValue("/UseTrakHoundCloud", dt);
            if (cloud_str != null)
            {
                bool.TryParse(cloud_str, out cloud);
            }

            UseTrakHoundCloud = cloud;


            // Load IP Address
            IpAddress = Table_Functions.GetTableValue(prefix + "IP_Address", dt);

            // Load Port
            Port = Table_Functions.GetTableValue(prefix + "Port", dt);

            // Load Device Name
            DeviceName = Table_Functions.GetTableValue(prefix + "Device_Name", dt);

            // Load Heartbeat
            int heartbeat;
            if (int.TryParse(Table_Functions.GetTableValue(prefix + "Heartbeat", dt), out heartbeat)) Heartbeat = heartbeat;

            // Load Proxy Address
            ProxyAddress = Table_Functions.GetTableValue(prefix + "ProxyAddress", dt);

            // Load Proxy Port
            ProxyPort = Table_Functions.GetTableValue(prefix + "ProxyPort", dt);

            if (!String.IsNullOrEmpty(ProxyAddress) || !String.IsNullOrEmpty(ProxyPort)) ProxySettings.IsExpanded = true;
            else ProxySettings.IsExpanded = false;

            MTCDeviceList.Clear();

            // Agent Info
            GetAgentInfo();

            Loading = false;
        }

        public void SaveConfiguration(DataTable dt)
        {
            // Save IP Address
            Table_Functions.UpdateTableValue(IpAddress, prefix + "IP_Address", dt);

            // Save Port
            Table_Functions.UpdateTableValue(Port, prefix + "Port", dt);

            // Save Device Name
            Table_Functions.UpdateTableValue(DeviceName, prefix + "Device_Name", dt);

            // Save Heartbeat
            Table_Functions.UpdateTableValue(Heartbeat.ToString(), prefix + "Heartbeat", dt);


            // Save Proxy Address
            Table_Functions.UpdateTableValue(ProxyAddress, prefix + "ProxyAddress", dt);

            // Save Proxy Port
            Table_Functions.UpdateTableValue(ProxyPort, prefix + "ProxyPort", dt);
            
        }

        public Page_Type PageType { get; set; }

        #endregion


        string prefix = "/Agent/";

        DataTable configurationTable;


        public bool UseTrakHoundCloud
        {
            get { return (bool)GetValue(UseTrakHoundCloudProperty); }
            set { SetValue(UseTrakHoundCloudProperty, value); }
        }

        public static readonly DependencyProperty UseTrakHoundCloudProperty =
            DependencyProperty.Register("UseTrakHoundCloud", typeof(bool), typeof(Page), new PropertyMetadata(false));


        public bool Loading
        {
            get { return (bool)GetValue(LoadingProperty); }
            set { SetValue(LoadingProperty, value); }
        }

        public static readonly DependencyProperty LoadingProperty =
            DependencyProperty.Register("Loading", typeof(bool), typeof(Page), new PropertyMetadata(false));

        const System.Windows.Threading.DispatcherPriority priority = System.Windows.Threading.DispatcherPriority.Background;

        #region "Properties"

        public string IpAddress
        {
            get { return (string)GetValue(IpAddressProperty); }
            set { SetValue(IpAddressProperty, value); }
        }

        public static readonly DependencyProperty IpAddressProperty =
            DependencyProperty.Register("IpAddress", typeof(string), typeof(Page), new PropertyMetadata(null));


        public string Port
        {
            get { return (string)GetValue(PortProperty); }
            set { SetValue(PortProperty, value); }
        }

        public static readonly DependencyProperty PortProperty =
            DependencyProperty.Register("Port", typeof(string), typeof(Page), new PropertyMetadata(null));


        public string DeviceName
        {
            get { return (string)GetValue(DeviceNameProperty); }
            set { SetValue(DeviceNameProperty, value); }
        }

        public static readonly DependencyProperty DeviceNameProperty =
            DependencyProperty.Register("DeviceName", typeof(string), typeof(Page), new PropertyMetadata(null));



        public string ProxyAddress
        {
            get { return (string)GetValue(ProxyAddressProperty); }
            set { SetValue(ProxyAddressProperty, value); }
        }

        public static readonly DependencyProperty ProxyAddressProperty =
            DependencyProperty.Register("ProxyAddress", typeof(string), typeof(Page), new PropertyMetadata(null));


        public string ProxyPort
        {
            get { return (string)GetValue(ProxyPortProperty); }
            set { SetValue(ProxyPortProperty, value); }
        }

        public static readonly DependencyProperty ProxyPortProperty =
            DependencyProperty.Register("ProxyPort", typeof(string), typeof(Page), new PropertyMetadata(null));

        #endregion

        #region "Test Connection"

        private void TestConnection_Clicked(TH_WPF.Button bt)
        {
            TestConnection();
        }

        #region "Connection"

        public bool ConnectionTestLoading
        {
            get { return (bool)GetValue(ConnectionTestLoadingProperty); }
            set { SetValue(ConnectionTestLoadingProperty, value); }
        }

        public static readonly DependencyProperty ConnectionTestLoadingProperty =
            DependencyProperty.Register("ConnectionTestLoading", typeof(bool), typeof(Page), new PropertyMetadata(false));

        void TestConnection()
        {
            ConnectionTestLoading = true;

            string ip = null;
            int port = -1;

            if (IpAddress != null)
            {
                // Get IP Address or URL
                ip = IpAddress;
                if (ip.Length > 7)
                {
                    if (ip != String.Empty) if (ip.Substring(0, 7).ToLower() == "http://") ip = ip.Substring(7);
                }

                // Get Port
                if (Port != null)
                {
                    int.TryParse(Port, out port);
                }

                // Proxy Settings
                TH_MTConnect.HTTP.ProxySettings proxy = null;                
                if (ProxyPort != null)
                {
                    int proxyPort = -1;
                    if (int.TryParse(ProxyPort, out proxyPort))
                    {
                        proxy = new TH_MTConnect.HTTP.ProxySettings();
                        proxy.Address = ProxyAddress;
                        proxy.Port = proxyPort;
                    }
                }

                tryPortIndex = 0;

                MTCDeviceList.Clear();
                MessageList.Clear();
                ClearAgentInfo();

                RunProbe(ip, proxy, port, DeviceName);
            }
        }

        class Probe_Info
        {
            public string address;
            public int port;
            public string deviceName;
            public TH_MTConnect.HTTP.ProxySettings proxy;
        }

        void RunProbe(string address, TH_MTConnect.HTTP.ProxySettings proxy, int port, string deviceName)
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
                    string url = TH_MTConnect.HTTP.GetUrl(info.address, info.port, info.deviceName);

                    ReturnData returnData = TH_MTConnect.Components.Requests.Get(url, info.proxy, 2000, 1);

                    if (returnData != null)
                    {
                        // Update port
                        if (info.port > 0)
                        {
                            this.Dispatcher.BeginInvoke(new Action<int>(UpdatePort), new object[] { info.port });
                        }

                        this.Dispatcher.BeginInvoke(new Action<ReturnData>(AddDevices), priority, new object[] { returnData });

                        this.Dispatcher.BeginInvoke(new Action<bool>(UpdateConnectionTestLoading), priority, new object[] { false });

                        //this.Dispatcher.BeginInvoke(new Action<string, int>(GetAgentInfo), priority, new object[] { info.address, info.port });
                    }
                    else
                    {
                        this.Dispatcher.BeginInvoke(new Action<string>(AddMessage), new object[] { info.address + ":" + info.port.ToString() });

                        // Run Probe again using other ports
                        if (tryPortIndex < tryPorts.Length - 1)
                        {
                            RunProbe(info.address, info.proxy, tryPorts[tryPortIndex], info.deviceName);
                            tryPortIndex += 1;
                        }
                        else this.Dispatcher.BeginInvoke(new Action<bool>(UpdateConnectionTestLoading), priority, new object[] { false });
                    }
                }
            }

            //this.Dispatcher.BeginInvoke(new Action<bool>(UpdateConnectionTestLoading), priority, new object[] { false });
        }

        

        //void RunProbe(string url, int port, string deviceName)
        //{
        //    // Run a Probe request
        //    Probe probe = new Probe();

        //    probe.Address = url;
        //    probe.Port = port;
        //    probe.DeviceName = deviceName;

        //    probe.ProbeFinished += probe_ProbeFinished;
        //    probe.ProbeError += probe_ProbeError;
        //    probe.Start();
        //}

        //void probe_ProbeFinished(ReturnData returnData, Probe sender)
        //{
        //    // Update port
        //    if (sender.Port > 0)
        //    {
        //        this.Dispatcher.BeginInvoke(new Action<int>(UpdatePort), new object[] { sender.Port });
        //    }

        //    this.Dispatcher.BeginInvoke(new Action<ReturnData>(AddDevices), priority, new object[] { returnData });

        //    this.Dispatcher.BeginInvoke(new Action<bool>(UpdateConnectionTestLoading), priority, new object[] { false });
        //}

        void UpdateConnectionTestLoading(bool loading)
        {
            ConnectionTestLoading = loading;

            GetAgentInfo();
        }

        int[] tryPorts = new int[] { 5000, 5001, 5002, 5003, 5004, 5005 };
        int tryPortIndex;

        //void probe_ProbeError(Probe.ErrorData errorData)
        //{
        //    Logger.Log("Probe Error :: " + errorData.message);

        //    //this.Dispatcher.BeginInvoke(new Action<string>(AddMessage), new object[] { errorData.probe.URL });

        //    if (errorData.probe != null)
        //    {
        //        // Run Probe again using other ports
        //        if (tryPortIndex < tryPorts.Length - 1)
        //        {
        //            RunProbe(errorData.probe.Address, tryPorts[tryPortIndex], errorData.probe.DeviceName);
        //            tryPortIndex += 1;
        //        }
        //        else this.Dispatcher.BeginInvoke(new Action<bool>(UpdateConnectionTestLoading), priority, new object[] { false });

        //        errorData.probe.Stop();
        //    }
        //    else this.Dispatcher.BeginInvoke(new Action<bool>(UpdateConnectionTestLoading), priority, new object[] { false });
        //}

        #endregion

        #region "Message List"

        ObservableCollection<object> messagelist;
        public ObservableCollection<object> MessageList
        {
            get
            {
                if (messagelist == null)
                    messagelist = new ObservableCollection<object>();
                return messagelist;
            }

            set
            {
                messagelist = value;
            }
        }

        void AddMessage(string url)
        {
            Controls.MessageItem item = new Controls.MessageItem();
            item.URL = url;
            MessageList.Add(item);
        }

        #endregion

        #region "Device List"

        ObservableCollection<RadioButton> mtcdevicelist;
        public ObservableCollection<RadioButton> MTCDeviceList
        {
            get
            {
                if (mtcdevicelist == null)
                    mtcdevicelist = new ObservableCollection<RadioButton>();
                return mtcdevicelist;
            }

            set
            {
                mtcdevicelist = value;
            }
        }

        void AddDevices(ReturnData returnData)
        {
            MTCDeviceList.Clear();

            foreach (Device device in returnData.devices)
            {
                RadioButton radio = new RadioButton();
                radio.Content = device.name;
                radio.GroupName = "MTCDevices";
                radio.Margin = new Thickness(0, 5, 0, 5);
                radio.Checked += radio_Checked;

                MTCDeviceList.Add(radio);
                if (MTCDeviceList.Count == 1) radio.IsChecked = true;
            }
        }

        void radio_Checked(object sender, RoutedEventArgs e)
        {
            DeviceName = ((RadioButton)sender).Content.ToString();
            //devicename_TXT.Text = ((RadioButton)sender).Content.ToString();
        }

        #endregion

        void UpdatePort(int port)
        {
            Port = port.ToString();
            //port_TXT.Text = port.ToString();
        }

        #endregion

        #region "Agent Info"

        #region "Properties"

        public string InstanceId
        {
            get { return (string)GetValue(InstanceIdProperty); }
            set { SetValue(InstanceIdProperty, value); }
        }

        public static readonly DependencyProperty InstanceIdProperty =
            DependencyProperty.Register("InstanceId", typeof(string), typeof(Page), new PropertyMetadata(null));


        public string Sender
        {
            get { return (string)GetValue(SenderProperty); }
            set { SetValue(SenderProperty, value); }
        }

        public static readonly DependencyProperty SenderProperty =
            DependencyProperty.Register("Sender", typeof(string), typeof(Page), new PropertyMetadata(null));


        public string Version
        {
            get { return (string)GetValue(VersionProperty); }
            set { SetValue(VersionProperty, value); }
        }

        public static readonly DependencyProperty VersionProperty =
            DependencyProperty.Register("Version", typeof(string), typeof(Page), new PropertyMetadata(null));


        public string BufferSize
        {
            get { return (string)GetValue(BufferSizeProperty); }
            set { SetValue(BufferSizeProperty, value); }
        }

        public static readonly DependencyProperty BufferSizeProperty =
            DependencyProperty.Register("BufferSize", typeof(string), typeof(Page), new PropertyMetadata(null));


        public string AssetBufferSize
        {
            get { return (string)GetValue(AssetBufferSizeProperty); }
            set { SetValue(AssetBufferSizeProperty, value); }
        }

        public static readonly DependencyProperty AssetBufferSizeProperty =
            DependencyProperty.Register("AssetBufferSize", typeof(string), typeof(Page), new PropertyMetadata(null));


        public string AssetCount
        {
            get { return (string)GetValue(AssetCountProperty); }
            set { SetValue(AssetCountProperty, value); }
        }

        public static readonly DependencyProperty AssetCountProperty =
            DependencyProperty.Register("AssetCount", typeof(string), typeof(Page), new PropertyMetadata(null));


        public string DeviceCount
        {
            get { return (string)GetValue(DeviceCountProperty); }
            set { SetValue(DeviceCountProperty, value); }
        }

        public static readonly DependencyProperty DeviceCountProperty =
            DependencyProperty.Register("DeviceCount", typeof(string), typeof(Page), new PropertyMetadata(null));
       

        #endregion

        Thread agentInfo_THREAD;

        void GetAgentInfo()
        {
            if (agentInfo_THREAD != null) agentInfo_THREAD.Abort();

            string ip = null;
            int port = -1;

            // Get IP Address or URL
            ip = IpAddress;
            if (ip != null)
            {
                if (ip.Length > 7)
                {
                    if (ip != String.Empty) if (ip.Substring(0, 7).ToLower() == "http://") ip = ip.Substring(7);
                }

                // Get Port
                if (Port != null)
                {
                    int.TryParse(Port, out port);
                }

                var info = new Probe_Info();
                info.address = ip;
                info.port = port;

                TH_MTConnect.HTTP.ProxySettings proxy = null;
                if (ProxyPort != null)
                {
                    int proxyPort = -1;
                    if (int.TryParse(ProxyPort, out proxyPort))
                    {
                        proxy = new TH_MTConnect.HTTP.ProxySettings();
                        proxy.Address = ProxyAddress;
                        proxy.Port = proxyPort;
                    }
                }

                info.proxy = proxy;

                agentInfo_THREAD = new Thread(new ParameterizedThreadStart(GetAgentInfo_Worker));
                agentInfo_THREAD.Start(info);
            }
        }

        void GetAgentInfo_Worker(object o)
        {
            if (o != null)
            {
                var info = o as Probe_Info;
                if (info != null)
                {
                    string url = TH_MTConnect.HTTP.GetUrl(info.address, info.port, info.deviceName);

                    ReturnData returnData = TH_MTConnect.Components.Requests.Get(url, info.proxy, 2000, 1);

                    this.Dispatcher.BeginInvoke(new Action<ReturnData>(GetAgentInfo_GUI), priority, new object[] { returnData });
                }
            }
        }

        void GetAgentInfo_GUI(ReturnData returnData)
        {
            if (returnData != null)
            {
                var header = returnData.header;
                if (returnData.header != null)
                {
                    InstanceId = header.instanceId.ToString();
                    Sender = header.sender;
                    Version = header.version;
                    BufferSize = String_Functions.FileSizeSuffix(header.bufferSize);
                    AssetBufferSize = String_Functions.FileSizeSuffix(header.assetBufferSize);
                    AssetCount = header.assetCount.ToString();
                    DeviceCount = returnData.devices.Count.ToString();
                }
            }
        }

        void ClearAgentInfo()
        {
            InstanceId = null;
            Sender = null;
            Version = null;
            BufferSize = null;
            AssetBufferSize = null;
            AssetCount = null;
            DeviceCount = null;
        }

        #endregion

        private void ipaddress_TXT_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender.GetType() == typeof(TextBox))
            {
                TextBox txt = (TextBox)sender;
                txt.Text = FormatIPAddress(txt.Text);
            }
        }

        string FormatIPAddress(string input)
        {
            string result = null;

            result = GetURL(input);

            return result;
        }

        string GetURL(string input)
        {
            string result = null;
            Uri uri;
            if (Uri.TryCreate(input, UriKind.RelativeOrAbsolute, out uri))
            {
                result = uri.ToString();
            }
            return result;
        }

        private void TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            UIElement txt = (UIElement)sender;

            if (txt.IsMouseCaptured || txt.IsKeyboardFocused)
            {
                ChangeSetting(null, null);
            }
        }

        private void ipaddress_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChangeSetting("IP_Address", ((TextBox)sender).Text);
        }

        private void port_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChangeSetting("Port", ((TextBox)sender).Text);
        }

        private void devicename_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChangeSetting("Device_Name", ((TextBox)sender).Text);
        }

        private void Help_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender.GetType() == typeof(Rectangle))
            {
                Rectangle rect = (Rectangle)sender;

                if (rect.ToolTip != null)
                {
                    if (rect.ToolTip.GetType() == typeof(ToolTip))
                    {
                        ToolTip tt = (ToolTip)rect.ToolTip;
                        tt.IsOpen = true;
                    }
                }
            }
        }

        private void Help_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender.GetType() == typeof(Rectangle))
            {
                Rectangle rect = (Rectangle)sender;

                if (rect.ToolTip != null)
                {
                    if (rect.ToolTip.GetType() == typeof(ToolTip))
                    {
                        ToolTip tt = (ToolTip)rect.ToolTip;
                        tt.IsOpen = true;
                    }
                }
            }
        }

        private void Help_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender.GetType() == typeof(Rectangle))
            {
                Rectangle rect = (Rectangle)sender;

                if (rect.ToolTip != null)
                {
                    if (rect.ToolTip.GetType() == typeof(ToolTip))
                    {
                        ToolTip tt = (ToolTip)rect.ToolTip;
                        tt.IsOpen = false;
                    }
                }
            }
        }

        void ChangeSetting(string name, string val)
        {
            if (!Loading)
            {
                string newVal = val;
                string oldVal = null;

                if (configurationTable != null)
                {
                    oldVal = Table_Functions.GetTableValue(prefix + name, configurationTable);
                }

                if (SettingChanged != null) SettingChanged(name, oldVal, newVal);
            }
        }

        #region "Heartbeat"

        public int Heartbeat
        {
            get { return (int)GetValue(HeartbeatProperty); }
            set { SetValue(HeartbeatProperty, value); }
        }

        public static readonly DependencyProperty HeartbeatProperty =
            DependencyProperty.Register("Heartbeat", typeof(int), typeof(Page), new PropertyMetadata(5000));


        public TimeSpan Heartbeat_TimeSpan
        {
            get { return (TimeSpan)GetValue(Heartbeat_TimeSpanProperty); }
            set { SetValue(Heartbeat_TimeSpanProperty, value); }
        }

        public static readonly DependencyProperty Heartbeat_TimeSpanProperty =
            DependencyProperty.Register("Heartbeat_TimeSpan", typeof(TimeSpan), typeof(Page), new PropertyMetadata(TimeSpan.FromMilliseconds(5000)));


        private void heartbeat_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (heartbeat_TXT.Text != String.Empty)
            {
                TimeSpan ts = GetTimeSpanFromString(heartbeat_TXT.Text);
                Heartbeat_TimeSpan = ts;
                if (ts.TotalMilliseconds < int.MaxValue)
                {
                    Heartbeat = Convert.ToInt32(ts.TotalMilliseconds);
                }
                ChangeSetting("Heartbeat", Heartbeat.ToString());
            }
        }

        private void CurrentSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Heartbeat_TimeSpan = TimeSpan.FromMilliseconds(Heartbeat);
            ChangeSetting("Heartbeat", Heartbeat.ToString());
        }

        private void heartbeat_TXT_LostFocus(object sender, RoutedEventArgs e)
        {
            heartbeat_TXT.Clear();
        }

        #endregion

        TimeSpan GetTimeSpanFromString(string s)
        {
            TimeSpan result = TimeSpan.Zero;
            if (TimeSpan.TryParse(heartbeat_TXT.Text, out result)) return result;
            else
            {
                s = s.Trim();
                //Milliseconds
                if (s.Length > 2)
                {
                    string unit = s.Substring(s.Length - 2, 2);
                    if (unit == "ms")
                    {
                        double ms;
                        if (double.TryParse(s.Substring(0, s.Length - 2), out ms))
                        {
                            result = TimeSpan.FromMilliseconds(ms);
                        }
                    }
                }
                //Seconds
                if (result == TimeSpan.Zero && s.Length > 1)
                {
                    string unit = s.Substring(s.Length - 1, 1);
                    if (unit == "s")
                    {
                        double seconds;
                        if (double.TryParse(s.Substring(0, s.Length - 1), out seconds))
                        {
                            result = TimeSpan.FromSeconds(seconds);
                        }
                    }
                }
            }
            return result;
        }

    }
}
