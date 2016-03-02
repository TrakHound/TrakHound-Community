// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading;
using System.Data;

using TH_Configuration;
using TH_Global.Functions;
using TH_UserManagement.Management;
using TH_WPF;

using TH_DeviceManager.Controls;

namespace TH_DeviceManager
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class DeviceManagerPage : UserControl
    {
        public DeviceManagerPage(Configuration config, DeviceManagerType type)
        {
            InitializeComponent();
            DataContext = this;

            Configuration = config;
            ConfigurationTable = TH_Configuration.Converter.XMLToTable(config.ConfigurationXML);

            ManagerType = type;
            SelectedManagerType = type;

            LoadPlugins();

            if (type == DeviceManagerType.Client) ShowClient_RADIO.IsChecked = true;
            else ShowServer_RADIO.IsChecked = true;
        }

        public Configuration Configuration { get; set; }
        public DataTable ConfigurationTable { get; set; }

        public DeviceManagerType ManagerType { get; set; }
        public DeviceManagerType SelectedManagerType { get; set; }

        const System.Windows.Threading.DispatcherPriority PRIORITY_BACKGROUND = System.Windows.Threading.DispatcherPriority.Background;

        public enum DeviceUpdateEvent
        {
            Added,
            Changed,
            Removed
        }
        public class DeviceUpdateArgs
        {
            public DeviceUpdateEvent Event { get; set; }
        }
        public delegate void DeviceUpdated_Handler(Configuration config, DeviceUpdateArgs args);
        public event DeviceUpdated_Handler DeviceUpdated;

        #region "Properties"

        public bool Saving
        {
            get { return (bool)GetValue(SavingProperty); }
            set { SetValue(SavingProperty, value); }
        }

        public static readonly DependencyProperty SavingProperty =
            DependencyProperty.Register("Saving", typeof(bool), typeof(DeviceManagerPage), new PropertyMetadata(false));

        #endregion

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

        #region "Set Device Index (Needs to be finished)"

        //private void IndexUp_Clicked(TH_WPF.Button bt)
        //{
        //    if (SelectedDevice != null)
        //    {
        //        if (SelectedDevice.Index > 0)
        //        {
        //            SetDeviceIndex(SelectedDevice.Index - 1, SelectedDevice.TableName);
        //        }
        //    }
        //}

        //private void IndexDown_Clicked(TH_WPF.Button bt)
        //{
        //    if (SelectedDevice != null)
        //    {
        //        if (SelectedDevice.Index < DeviceList.Count - 1)
        //        {
        //            SetDeviceIndex(SelectedDevice.Index + 1, SelectedDevice.TableName);
        //        }
        //    }
        //}

        //class SetDeviceIndex_Info
        //{
        //    public string tablename { get; set; }
        //    public int index { get; set; }
        //}

        //Thread deviceindex_THREAD;

        //void SetDeviceIndex(int index, string tablename)
        //{
        //    if (tablename != null)
        //    {
        //        SetDeviceIndex_Info info = new SetDeviceIndex_Info();
        //        info.tablename = tablename;
        //        info.index = index;

        //        if (deviceindex_THREAD != null) deviceindex_THREAD.Abort();

        //        deviceindex_THREAD = new Thread(new ParameterizedThreadStart(EnableDevice_Worker));
        //        deviceindex_THREAD.Start(info);
        //    }
        //}

        //void SetDeviceIndex_Worker(object o)
        //{
        //    if (o != null)
        //    {
        //        SetDeviceIndex_Info info = (SetDeviceIndex_Info)o;

        //        Configurations.UpdateConfigurationTable("/Index", info.index.ToString(), info.tablename);

        //        // Reset Update ID
        //        if (ManagerType == DeviceManagerType.Client) Configurations.UpdateConfigurationTable("/ClientUpdateId", String_Functions.RandomString(20), info.tablename);
        //        else if (ManagerType == DeviceManagerType.Server) Configurations.UpdateConfigurationTable("/ServerUpdateId", String_Functions.RandomString(20), info.tablename);
        //    }
        //}

        #endregion

    }

}
