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

using TH_Configuration;
using TH_Global;
using TH_Global.Functions;
using TH_MTC_Data.Components;
using TH_MTC_Requests;
using TH_PlugIns_Server;
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
        }

        #region "Page Interface"

        public string PageName { get { return "Agent"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TH_DeviceManager;component/Resources/Agent_02.png")); } }

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

            // Load Current Heartbeat
            int currentHeartbeat;
            if (int.TryParse(Table_Functions.GetTableValue(prefix + "Current_Heartbeat", dt), out currentHeartbeat)) CurrentHeartbeat = currentHeartbeat;

            // Load Sample Heartbeat
            int sampleHeartbeat;
            if (int.TryParse(Table_Functions.GetTableValue(prefix + "Sample_Heartbeat", dt), out sampleHeartbeat)) SampleHeartbeat = sampleHeartbeat;

            MTCDeviceList.Clear();

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

            //// Save IP Address
            //Table_Functions.UpdateTableValue(ipaddress_TXT.Text, prefix + "IP_Address", dt);

            //// Save Port
            //Table_Functions.UpdateTableValue(port_TXT.Text, prefix + "Port", dt);

            //// Save Device Name
            //Table_Functions.UpdateTableValue(devicename_TXT.Text, prefix + "Device_Name", dt);

            // Save Current Heartbeat
            Table_Functions.UpdateTableValue(CurrentHeartbeat.ToString(), prefix + "Current_Heartbeat", dt);

            // Save Sample Heartbeat
            Table_Functions.UpdateTableValue(SampleHeartbeat.ToString(), prefix + "Sample_Heartbeat", dt);
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
                //ip = ipaddress_TXT.Text;
                if (ip.Length > 7)
                {
                    if (ip != String.Empty) if (ip.Substring(0, 7).ToLower() == "http://") ip = ip.Substring(7);
                }

                // Get Port
                if (Port != null)
                {
                    int.TryParse(Port, out port);
                }

                tryPortIndex = 0;

                MTCDeviceList.Clear();
                MessageList.Clear();

                RunProbe(ip, port, DeviceName);
            }
        }

        void RunProbe(string url, int port, string deviceName)
        {
            // Run a Probe request
            Probe probe = new Probe();

            probe.Address = url;
            probe.Port = port;
            probe.DeviceName = deviceName;

            probe.ProbeFinished += probe_ProbeFinished;
            probe.ProbeError += probe_ProbeError;
            probe.Start();
        }

        void probe_ProbeFinished(ReturnData returnData, Probe sender)
        {
            // Update port
            if (sender.Port > 0)
            {
                this.Dispatcher.BeginInvoke(new Action<int>(UpdatePort), new object[] { sender.Port });
            }

            this.Dispatcher.BeginInvoke(new Action<ReturnData>(AddDevices), priority, new object[] { returnData });

            this.Dispatcher.BeginInvoke(new Action<bool>(UpdateConnectionTestLoading), priority, new object[] { false });
        }

        void UpdateConnectionTestLoading(bool loading)
        {
            ConnectionTestLoading = loading;
        }

        int[] tryPorts = new int[] { 5000, 5001, 5002, 5003, 5004, 5005 };
        int tryPortIndex;

        void probe_ProbeError(Probe.ErrorData errorData)
        {
            Logger.Log("Probe Error :: " + errorData.message);

            //this.Dispatcher.BeginInvoke(new Action<string>(AddMessage), new object[] { errorData.probe.URL });

            if (errorData.probe != null)
            {
                // Run Probe again using other ports
                if (tryPortIndex < tryPorts.Length - 1)
                {
                    RunProbe(errorData.probe.Address, tryPorts[tryPortIndex], errorData.probe.DeviceName);
                    tryPortIndex += 1;
                }
                else this.Dispatcher.BeginInvoke(new Action<bool>(UpdateConnectionTestLoading), priority, new object[] { false });

                errorData.probe.Stop();
            }
            else this.Dispatcher.BeginInvoke(new Action<bool>(UpdateConnectionTestLoading), priority, new object[] { false });
        }

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
                radio.Style = (Style)TryFindResource("TH_RadioButton_Style");
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

        #region "Current Heartbeat"

        public int CurrentHeartbeat
        {
            get { return (int)GetValue(CurrentHeartbeatProperty); }
            set { SetValue(CurrentHeartbeatProperty, value); }
        }

        public static readonly DependencyProperty CurrentHeartbeatProperty =
            DependencyProperty.Register("CurrentHeartbeat", typeof(int), typeof(Page), new PropertyMetadata(1000));


        public TimeSpan CurrentHeartbeat_TimeSpan
        {
            get { return (TimeSpan)GetValue(CurrentHeartbeat_TimeSpanProperty); }
            set { SetValue(CurrentHeartbeat_TimeSpanProperty, value); }
        }

        public static readonly DependencyProperty CurrentHeartbeat_TimeSpanProperty =
            DependencyProperty.Register("CurrentHeartbeat_TimeSpan", typeof(TimeSpan), typeof(Page), new PropertyMetadata(TimeSpan.FromMilliseconds(1000)));


        private void currentHeartbeat_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (currentHeartbeat_TXT.Text != String.Empty)
            {
                TimeSpan ts = GetTimeSpanFromString(currentHeartbeat_TXT.Text);
                CurrentHeartbeat_TimeSpan = ts;
                if (ts.TotalMilliseconds < int.MaxValue)
                {
                    CurrentHeartbeat = Convert.ToInt32(ts.TotalMilliseconds);
                }
                ChangeSetting("Current_Heartbeat", CurrentHeartbeat.ToString());
            }
        }

        private void CurrentSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            CurrentHeartbeat_TimeSpan = TimeSpan.FromMilliseconds(CurrentHeartbeat);
            ChangeSetting("Current_Heartbeat", CurrentHeartbeat.ToString());
        }

        private void currentHeartbeat_TXT_LostFocus(object sender, RoutedEventArgs e)
        {
            currentHeartbeat_TXT.Clear();
        }

        #endregion

        #region "Sample Heartbeat"

        public int SampleHeartbeat
        {
            get { return (int)GetValue(SampleHeartbeatProperty); }
            set { SetValue(SampleHeartbeatProperty, value); }
        }

        public static readonly DependencyProperty SampleHeartbeatProperty =
            DependencyProperty.Register("SampleHeartbeat", typeof(int), typeof(Page), new PropertyMetadata(1000));

        public TimeSpan SampleHeartbeat_TimeSpan
        {
            get { return (TimeSpan)GetValue(SampleHeartbeat_TimeSpanProperty); }
            set { SetValue(SampleHeartbeat_TimeSpanProperty, value); }
        }

        public static readonly DependencyProperty SampleHeartbeat_TimeSpanProperty =
            DependencyProperty.Register("SampleHeartbeat_TimeSpan", typeof(TimeSpan), typeof(Page), new PropertyMetadata(TimeSpan.FromMilliseconds(1000)));


        private void sampleHeartbeat_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sampleHeartbeat_TXT.Text != String.Empty)
            {
                TimeSpan ts = GetTimeSpanFromString(sampleHeartbeat_TXT.Text);
                SampleHeartbeat_TimeSpan = ts;
                if (ts.TotalMilliseconds < int.MaxValue)
                {
                    SampleHeartbeat = Convert.ToInt32(ts.TotalMilliseconds);
                }
                ChangeSetting("Sample_Heartbeat", SampleHeartbeat.ToString());
            }
        }

        private void SampleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SampleHeartbeat_TimeSpan = TimeSpan.FromMilliseconds(SampleHeartbeat);
            ChangeSetting("Sample_Heartbeat", SampleHeartbeat.ToString());
        }

        private void sampleHeartbeat_TXT_LostFocus(object sender, RoutedEventArgs e)
        {
            sampleHeartbeat_TXT.Clear();
        }

        #endregion

        TimeSpan GetTimeSpanFromString(string s)
        {
            TimeSpan result = TimeSpan.Zero;
            if (TimeSpan.TryParse(currentHeartbeat_TXT.Text, out result)) return result;
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
