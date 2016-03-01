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

using System.Threading;

using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

using System.IO;
using System.Collections.ObjectModel;
using System.Xml;
using System.Data;

using TH_Configuration;
using TH_Database;
using TH_Global;
using TH_Global.Functions;
using TH_Plugins_Server;
using TH_UserManagement.Management;
using TH_WPF;

using TH_DeviceManager.Controls;

namespace TH_DeviceManager
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class DeviceManager : UserControl
    {
        public DeviceManager()
        {
            init();
        }

        public DeviceManager(DeviceManagerType type)
        {
            init();

            ManagerType = type;
            SelectedManagerType = type;

            LoadPlugins();

            if (type == DeviceManagerType.Client) ShowClient_RADIO.IsChecked = true;
            else ShowServer_RADIO.IsChecked = true;
        }

        void init()
        {
            InitializeComponent();
            DataContext = this;
        }

        public DeviceManagerType ManagerType { get; set; }
        public DeviceManagerType SelectedManagerType { get; set; }


        const System.Windows.Threading.DispatcherPriority background = System.Windows.Threading.DispatcherPriority.Background;
        const System.Windows.Threading.DispatcherPriority contextidle = System.Windows.Threading.DispatcherPriority.ContextIdle;

       
        private void Save_Clicked(TH_WPF.Button bt)
        {
            bt.Focus();

            if (SelectedDevice != null)
            {
                DataTable dt = Converter.XMLToTable(SelectedDevice.ConfigurationXML);
                dt.TableName = SelectedDevice.TableName;

                Save(dt);
            } 
        }

        void LoadAddSharePage(DeviceButton bt)
        {
            if (bt.Parent != null)
            {
                if (bt.Parent.GetType() == typeof(ListButton))
                {
                    ListButton lb = (ListButton)bt.Parent;

                    foreach (TH_WPF.ListButton olb in DeviceList.OfType<TH_WPF.ListButton>()) if (olb != lb) olb.IsSelected = false;
                    lb.IsSelected = true;

                    SelectedDevice = null;
                    selectedPageIndex = 0;

                    if (bt.Config != null)
                    {
                        Pages.AddShare.Page page = new Pages.AddShare.Page();
                        page.devicemanager = this;
                        page.currentuser = CurrentUser;
                        page.LoadConfiguration(bt.Config);
                        page.configurationtable = ConfigurationTable;
                        CurrentPage = page;
                    }
                }
            }
        }

        public bool Saving
        {
            get { return (bool)GetValue(SavingProperty); }
            set { SetValue(SavingProperty, value); }
        }

        public static readonly DependencyProperty SavingProperty =
            DependencyProperty.Register("Saving", typeof(bool), typeof(DeviceManager), new PropertyMetadata(false));


        public bool PageListShown
        {
            get { return (bool)GetValue(PageListShownProperty); }
            set { SetValue(PageListShownProperty, value); }
        }

        public static readonly DependencyProperty PageListShownProperty =
            DependencyProperty.Register("PageListShown", typeof(bool), typeof(DeviceManager), new PropertyMetadata(false));


        public bool ToolbarShown
        {
            get { return (bool)GetValue(ToolbarShownProperty); }
            set { SetValue(ToolbarShownProperty, value); }
        }

        public static readonly DependencyProperty ToolbarShownProperty =
            DependencyProperty.Register("ToolbarShown", typeof(bool), typeof(DeviceManager), new PropertyMetadata(false));


        private void TableList_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PageListShown = false;
        }


        const System.Windows.Threading.DispatcherPriority priority = System.Windows.Threading.DispatcherPriority.Background;

        private void RefreshDevices_Clicked(TH_WPF.Button bt)
        {
            LoadDevices();
        }

        private void IndexUp_Clicked(TH_WPF.Button bt)
        {
            if (SelectedDevice != null)
            {
                if (SelectedDevice.Index > 0)
                {
                    SetDeviceIndex(SelectedDevice.Index - 1, SelectedDevice.TableName);
                }
            }
        }

        private void IndexDown_Clicked(TH_WPF.Button bt)
        {
            if (SelectedDevice != null)
            {
                if (SelectedDevice.Index < DeviceList.Count - 1)
                {
                    SetDeviceIndex(SelectedDevice.Index + 1, SelectedDevice.TableName);
                }
            }
        }

        #region "Set Device Index"

        class SetDeviceIndex_Info
        {
            public string tablename { get; set; }
            public int index { get; set; }
        }

        Thread deviceindex_THREAD;

        void SetDeviceIndex(int index, string tablename)
        {
            if (tablename != null)
            {
                SetDeviceIndex_Info info = new SetDeviceIndex_Info();
                info.tablename = tablename;
                info.index = index;

                if (deviceindex_THREAD != null) deviceindex_THREAD.Abort();

                deviceindex_THREAD = new Thread(new ParameterizedThreadStart(EnableDevice_Worker));
                deviceindex_THREAD.Start(info);
            }
        }

        void SetDeviceIndex_Worker(object o)
        {
            if (o != null)
            {
                SetDeviceIndex_Info info = (SetDeviceIndex_Info)o;

                Configurations.UpdateConfigurationTable("/Index", info.index.ToString(), info.tablename);

                // Reset Update ID
                if (ManagerType == DeviceManagerType.Client) Configurations.UpdateConfigurationTable("/ClientUpdateId", String_Functions.RandomString(20), info.tablename);
                else if (ManagerType == DeviceManagerType.Server) Configurations.UpdateConfigurationTable("/ServerUpdateId", String_Functions.RandomString(20), info.tablename);
            }
        }

        #endregion


    }

    public enum DeviceManagerType
    {
        Client = 0,
        Server = 1
    }

}
