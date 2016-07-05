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

using TH_Global.TrakHound.Configurations;
using TH_Global;
using TH_Global.Functions;
using TH_UserManagement.Management;
using TH_WPF;

using TH_DeviceManager.Controls;

namespace TH_DeviceManager
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class DeviceManagerTable : UserControl, IPage
    {
        public DeviceManagerTable(Configuration config)
        {
            InitializeComponent();
            DataContext = this;

            Configuration = config;
            ConfigurationTable = TH_Global.TrakHound.Configurations.Converter.XMLToTable(config.ConfigurationXML);
        }

        public string Title { get { return "Edit Device Table"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TH_DeviceManager;component/Resources/Table_01.png")); } }


        public void Opened() { }
        public bool Opening() { return true; }

        public void Closed() { }
        public bool Closing() { return true; }



        public Configuration Configuration { get; set; }
        //public DataTable ConfigurationTable { get; set; }

        public DataTable ConfigurationTable
        {
            get { return (DataTable)GetValue(ConfigurationTableProperty); }
            set { SetValue(ConfigurationTableProperty, value); }
        }

        public static readonly DependencyProperty ConfigurationTableProperty =
            DependencyProperty.Register("ConfigurationTable", typeof(DataTable), typeof(DeviceManagerTable), new PropertyMetadata(null));



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
            DependencyProperty.Register("Saving", typeof(bool), typeof(DeviceManagerTable), new PropertyMetadata(false));

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

    }

}
