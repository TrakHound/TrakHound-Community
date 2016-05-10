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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using TH_Global.Functions;
using TH_MTConnect.Components;
using TH_Plugins;
using TH_Plugins.Server;
using TH_UserManagement.Management;

namespace TH_InstanceTable.ConfigurationPage
{
    /// <summary>
    /// Interaction logic for Page.xaml
    /// </summary>
    public partial class Page : UserControl, IConfigurationPage
    {
        public Page()
        {
            InitializeComponent();
            DataContext = this;
        }

        public string Title { get { return "Instance Data"; } }

        private BitmapImage _image;
        public ImageSource Image
        {
            get
            {
                if (_image == null)
                {
                    _image = new BitmapImage(new Uri("pack://application:,,,/TH_InstanceTable;component/Resources/Hourglass_01.png"));
                    _image.Freeze();
                }

                return _image;
            }
        }

        public bool Loaded { get; set; }

        public event SettingChanged_Handler SettingChanged;

        public event SendData_Handler SendData;

        public void GetSentData(EventData data)
        {
            GetProbeData(data);
        }

        public void LoadConfiguration(DataTable dt)
        {
            Loading = true;

            ConditionItems.Clear();
            EventItems.Clear();
            SampleItems.Clear();

            configurationTable = dt;

            string prefix = "/InstanceTable/DataItems/";
            string s;
            bool val;

            // Load Conditions
            s = Table_Functions.GetTableValue(prefix + "Conditions", dt);
            val = false;
            if (bool.TryParse(s, out val)) conditions_CHK.IsChecked = val;

            // Load Events
            s = Table_Functions.GetTableValue(prefix + "Events", dt);
            val = false;
            if (bool.TryParse(s, out val)) events_CHK.IsChecked = val;

            // Load Samples
            s = Table_Functions.GetTableValue(prefix + "Samples", dt);
            val = false;
            if (bool.TryParse(s, out val)) samples_CHK.IsChecked = val;

            //LoadAgentSettings(dt);

            // Load Omit items using MTConnect Probe Data
            if (probeData != null) LoadProbeData(probeData);

            configurationTable = dt;

            Loading = false;
        }

        public void SaveConfiguration(DataTable dt)
        {
                // Remove old rows
                DataTable_Functions.TrakHound.DeleteRows("/InstanceTable/*", "address", dt);


                string prefix = "/InstanceTable/DataItems/";

                // Save Conditions
                Table_Functions.UpdateTableValue(conditions_CHK.IsChecked.ToString(), prefix + "Conditions", dt);

                // Save Events
                Table_Functions.UpdateTableValue(events_CHK.IsChecked.ToString(), prefix + "Events", dt);

                // Save Samples
                Table_Functions.UpdateTableValue(samples_CHK.IsChecked.ToString(), prefix + "Samples", dt);


                prefix = "/InstanceTable/DataItems/Omit/";

                foreach (Controls.CheckBox chk in ConditionItems)
                {
                    if (chk.IsChecked == false) Table_Functions.UpdateTableValue(null, prefix + chk.Id, dt);
                    else Table_Functions.RemoveTableRow(prefix + chk.Id, dt);
                }

                foreach (Controls.CheckBox chk in EventItems)
                {
                    if (chk.IsChecked == false) Table_Functions.UpdateTableValue(null, prefix + chk.Id, dt);
                    else Table_Functions.RemoveTableRow(prefix + chk.Id, dt);
                }

                foreach (Controls.CheckBox chk in SampleItems)
                {
                    if (chk.IsChecked == false) Table_Functions.UpdateTableValue(null, prefix + chk.Id, dt);
                    else Table_Functions.RemoveTableRow(prefix + chk.Id, dt);
                }
        }

        DataTable configurationTable;

        #region "Help"

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

        #endregion

        bool Loading;

        const System.Windows.Threading.DispatcherPriority priority = System.Windows.Threading.DispatcherPriority.Background;

        void ChangeSetting(string address, string name, string val)
        {
            if (!Loading)
            {
                string newVal = val;
                string oldVal = null;

                if (configurationTable != null)
                {
                    oldVal = Table_Functions.GetTableValue(address, configurationTable);
                }

                if (SettingChanged != null) SettingChanged(name, oldVal, newVal);
            }
        }

        private void events_CHK_Checked(object sender, RoutedEventArgs e)
        {
            ChangeSetting("/InstanceTable/DataItems/Events", "Events", "True");
        }

        private void events_CHK_Unchecked(object sender, RoutedEventArgs e)
        {
            ChangeSetting("/InstanceTable/DataItems/Events", "Events", "False");
        }

        private void samples_CHK_Checked(object sender, RoutedEventArgs e)
        {
            ChangeSetting("/InstanceTable/DataItems/Samples", "Samples", "True");
        }

        private void samples_CHK_Unchecked(object sender, RoutedEventArgs e)
        {
            ChangeSetting("/InstanceTable/DataItems/Samples", "Samples", "False");
        }

        private void conditions_CHK_Checked(object sender, RoutedEventArgs e)
        {
            ChangeSetting("/InstanceTable/DataItems/Conditions", "Conditions", "True");
        }

        private void conditions_CHK_Unchecked(object sender, RoutedEventArgs e)
        {
            ChangeSetting("/InstanceTable/DataItems/Conditions", "Conditions", "False");
        }

        private List<DataItem> probeData;

        void GetProbeData(EventData data)
        {
            if (data != null && data.Id != null && data.Data02 != null)
            {
                if (data.Id.ToLower() == "mtconnect_probe_dataitems")
                {
                    var dataItems = (List<DataItem>)data.Data02;

                    probeData = dataItems;

                    if (configurationTable != null) LoadProbeData(dataItems);
                }
            }
        }

        private void LoadProbeData(List<DataItem> items)
        {
            //Conditions
            foreach (var item in items.FindAll(x => x.Category == DataItemCategory.CONDITION))
                this.Dispatcher.BeginInvoke(new Action<DataItem>(AddConditionItem), priority, new object[] { item });

            // Events
            foreach (var item in items.FindAll(x => x.Category == DataItemCategory.EVENT))
                this.Dispatcher.BeginInvoke(new Action<DataItem>(AddEventItem), priority, new object[] { item });

            // Samples
            foreach (var item in items.FindAll(x => x.Category == DataItemCategory.SAMPLE))
                this.Dispatcher.BeginInvoke(new Action<DataItem>(AddSampleItem), priority, new object[] { item });
        }

        #region "Omit Data Items"

        public bool OmitItemsLoading
        {
            get { return (bool)GetValue(OmitItemsLoadingProperty); }
            set { SetValue(OmitItemsLoadingProperty, value); }
        }

        public static readonly DependencyProperty OmitItemsLoadingProperty =
            DependencyProperty.Register("OmitItemsLoading", typeof(bool), typeof(Page), new PropertyMetadata(false));
        

        ObservableCollection<Controls.CheckBox> eventitems;
        public ObservableCollection<Controls.CheckBox> EventItems
        {
            get
            {
                if (eventitems == null)
                    eventitems = new ObservableCollection<Controls.CheckBox>();
                return eventitems;
            }

            set
            {
                eventitems = value;
            }
        }

        ObservableCollection<Controls.CheckBox> sampleitems;
        public ObservableCollection<Controls.CheckBox> SampleItems
        {
            get
            {
                if (sampleitems == null)
                    sampleitems = new ObservableCollection<Controls.CheckBox>();
                return sampleitems;
            }

            set
            {
                sampleitems = value;
            }
        }

        ObservableCollection<Controls.CheckBox> conditionitems;
        public ObservableCollection<Controls.CheckBox> ConditionItems
        {
            get
            {
                if (conditionitems == null)
                    conditionitems = new ObservableCollection<Controls.CheckBox>();
                return conditionitems;
            }

            set
            {
                conditionitems = value;
            }
        }


        void AddConditionItem(DataItem dataItem)
        {
            if (ConditionItems.ToList().Find(x => x.Id == dataItem.Id) == null)
            {
                Controls.CheckBox chk = new Controls.CheckBox();

                // Set text
                if (dataItem.Name == null) chk.Content = dataItem.Id;
                else chk.Content = dataItem.Id + " : " + dataItem.Name;

                chk.Id = dataItem.Id;
                chk.Name = dataItem.Name;

                // Set Checked based on whether it is found in 'Omit' list
                string prefix = "/InstanceTable/DataItems/Omit/";
                string s = Table_Functions.GetTableValue(prefix + dataItem.Id, configurationTable);
                if (s != null) chk.IsChecked = false;
                else chk.IsChecked = true;

                chk.Checked += Omit_Checked;
                chk.Unchecked += Omit_Unchecked;

                ConditionItems.Add(chk);
            }
        }

        void AddEventItem(DataItem dataItem)
        {
            if (EventItems.ToList().Find(x => x.Id == dataItem.Id) == null)
             {
                 Controls.CheckBox chk = new Controls.CheckBox();

                 // Set text
                 if (dataItem.Name == null) chk.Content = dataItem.Id;
                 else chk.Content = dataItem.Id + " : " + dataItem.Name;

                 chk.Id = dataItem.Id;
                 chk.Name = dataItem.Name;

                 // Set Checked based on whether it is found in 'Omit' list
                 string prefix = "/InstanceTable/DataItems/Omit/";
                 string s = Table_Functions.GetTableValue(prefix + dataItem.Id, configurationTable);
                 if (s != null) chk.IsChecked = false;
                 else chk.IsChecked = true;

                 chk.Checked += Omit_Checked;
                 chk.Unchecked += Omit_Unchecked;

                 EventItems.Add(chk);
             }
        }

        void AddSampleItem(DataItem dataItem)
        {
            if (SampleItems.ToList().Find(x => x.Id == dataItem.Id) == null)
            {
                Controls.CheckBox chk = new Controls.CheckBox();

                // Set text
                if (dataItem.Name == null) chk.Content = dataItem.Id;
                else chk.Content = dataItem.Id + " : " + dataItem.Name;

                chk.Id = dataItem.Id;
                chk.Name = dataItem.Name;

                // Set Checked based on whether it is found in 'Omit' list
                string prefix = "/InstanceTable/DataItems/Omit/";
                string s = Table_Functions.GetTableValue(prefix + dataItem.Id, configurationTable);
                if (s != null) chk.IsChecked = false;
                else chk.IsChecked = true;

                chk.Checked += Omit_Checked;
                chk.Unchecked += Omit_Unchecked;

                SampleItems.Add(chk);
            }
        }


        void Omit_Checked(object sender, RoutedEventArgs e)
        {
            Controls.CheckBox chk = (Controls.CheckBox)sender;
            ChangeSetting("/InstanceTable/DataItems/Omit/" + chk.Id, chk.Id, "True");
        }

        void Omit_Unchecked(object sender, RoutedEventArgs e)
        {
            Controls.CheckBox chk = (Controls.CheckBox)sender;
            ChangeSetting("/InstanceTable/DataItems/Omit/" + chk.Id, chk.Id, "False");
        }



        //void LoadAgentSettings(DataTable dt)
        //{
        //    OmitItemsLoading = true;

        //    string prefix = "/Agent/";

        //    string ip = Table_Functions.GetTableValue(prefix + "Address", dt);
        //    // Get deprecated value if new value is not found
        //    if (String.IsNullOrEmpty(ip)) ip = Table_Functions.GetTableValue(prefix + "IP_Address", dt);

        //    string p = Table_Functions.GetTableValue(prefix + "Port", dt);

        //    string devicename = Table_Functions.GetTableValue(prefix + "DeviceName", dt);
        //    // Get deprecated value if new value is not found
        //    if (String.IsNullOrEmpty(devicename)) devicename = Table_Functions.GetTableValue(prefix + "Device_Name", dt);

        //    string proxyAddress = Table_Functions.GetTableValue(prefix + "ProxyAddress", dt);
        //    string proxyPort = Table_Functions.GetTableValue(prefix + "ProxyPort", dt);

        //    int port;
        //    int.TryParse(p, out port);

        //    // Proxy Settings
        //    TH_MTConnect.HTTP.ProxySettings proxy = null;
        //    if (proxyPort != null)
        //    {
        //        int proxy_p = -1;
        //        if (int.TryParse(proxyPort, out proxy_p))
        //        {
        //            proxy = new TH_MTConnect.HTTP.ProxySettings();
        //            proxy.Address = proxyAddress;
        //            proxy.Port = proxy_p;
        //        }
        //    }

        //    RunProbe(ip, proxy, port, devicename);
        //}

        //Thread runProbe_THREAD;

        //class Probe_Info
        //{
        //    public string address;
        //    public int port;
        //    public string deviceName;
        //    public TH_MTConnect.HTTP.ProxySettings proxy;
        //}

        //void RunProbe(string address, TH_MTConnect.HTTP.ProxySettings proxy, int port, string deviceName)
        //{
        //    if (runProbe_THREAD != null) runProbe_THREAD.Abort();

        //    var info = new Probe_Info();
        //    info.address = address;
        //    info.port = port;
        //    info.deviceName = deviceName;
        //    info.proxy = proxy;

        //    runProbe_THREAD = new Thread(new ParameterizedThreadStart(RunProbe_Worker));
        //    runProbe_THREAD.Start(info);
        //}

        //void RunProbe_Worker(object o)
        //{
        //    if (o != null)
        //    {
        //        var info = o as Probe_Info;
        //        if (info != null)
        //        {
        //            string url = TH_MTConnect.HTTP.GetUrl(info.address, info.port, info.deviceName);

        //            ReturnData returnData = TH_MTConnect.Components.Requests.Get(url, info.proxy, 2000, 1);
        //            if (returnData != null)
        //            {
        //                foreach (Device device in returnData.Devices)
        //                {
        //                    DataItemCollection dataItems = Tools.GetDataItemsFromDevice(device);

        //                    // Conditions
        //                    foreach (DataItem dataItem in dataItems.Conditions) this.Dispatcher.BeginInvoke(new Action<DataItem>(AddConditionItem), priority, new object[] { dataItem });

        //                    // Events
        //                    foreach (DataItem dataItem in dataItems.Events) this.Dispatcher.BeginInvoke(new Action<DataItem>(AddEventItem), priority, new object[] { dataItem });

        //                    // Samples
        //                    foreach (DataItem dataItem in dataItems.Samples) this.Dispatcher.BeginInvoke(new Action<DataItem>(AddSampleItem), priority, new object[] { dataItem });
        //                }
        //            }
        //            else
        //            {

        //            }

        //            // Set 'Loading' to false
        //            this.Dispatcher.BeginInvoke(new Action(OmitProbeFinished), priority, null);
        //        }
        //    }
        //}

        //void OmitProbeFinished()
        //{
        //    OmitItemsLoading = false;
        //}

        #endregion

    }

}
