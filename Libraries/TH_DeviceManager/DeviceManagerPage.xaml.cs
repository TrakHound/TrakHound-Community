// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading;
using System.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;

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

            LoadPages();

            //var pages = CreatePages();

            Configuration = config;
            ConfigurationTable = Converter.XMLToTable(config.ConfigurationXML);

            //AddPages(pages);

            //ConfigurationPages = pages;

            //LoadConfiguration();
        }

        public string PageName { get { return "Edit Device"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TH_DeviceManager;component/Resources/Edit_02.png")); } }

        public event EventHandler PageOpened;
        public event CancelEventHandler PageOpening;

        public event EventHandler PageClosed;
        public event CancelEventHandler PageClosing;



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

        public DeviceManagerList ParentManager { get; set; }

        public Configuration Configuration { get; set; }
        public DataTable ConfigurationTable { get; set; }

        public DeviceManagerType ManagerType { get; set; }
        public DeviceManagerType SelectedManagerType { get; set; }

        public bool Saving
        {
            get { return (bool)GetValue(SavingProperty); }
            set { SetValue(SavingProperty, value); }
        }

        public static readonly DependencyProperty SavingProperty =
            DependencyProperty.Register("Saving", typeof(bool), typeof(DeviceManagerPage), new PropertyMetadata(false));

        public bool PagesLoading
        {
            get { return (bool)GetValue(PagesLoadingProperty); }
            set { SetValue(PagesLoadingProperty, value); }
        }

        public static readonly DependencyProperty PagesLoadingProperty =
            DependencyProperty.Register("PagesLoading", typeof(bool), typeof(DeviceManagerPage), new PropertyMetadata(false));

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

        private void DeviceManager_Clicked(TH_WPF.Button bt)
        {
            if (ParentManager != null) ParentManager.Open();
        }

        private void EditTable_Clicked(TH_WPF.Button bt)
        {
            if (ParentManager != null) ParentManager.OpenEditTable(Configuration);
        }

    }

}
