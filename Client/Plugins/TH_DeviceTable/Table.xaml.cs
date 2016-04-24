// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Windows;
using System.Windows.Controls;

using TH_Configuration;
using TH_Plugins;

namespace TH_DeviceTable
{
    public partial class DeviceTable : UserControl
    {
        public DeviceTable()
        {
            InitializeComponent();
            //Devices_DG.DataContext = this;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Devices_DG.DataContext = this;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Devices_DG.DataContext = null;
        }

        private void DataGridMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var item = (DataGridMenuItem)sender;

            if (item.DataObject != null)
            {
                if (item.DataObject.GetType() == typeof(DeviceInfo))
                {
                    var info = (DeviceInfo)item.DataObject;

                    if (info.Configuration != null)
                    {
                        OpenDevicePage(info.Configuration);
                    }
                }
            }
        }

        private void OpenDevicePage(Configuration config)
        {
            var data = new EventData();
            data.Id = "DevicePage_Show";
            data.Data01 = config;
            if (SendData != null) SendData(data);
        }

        private void Refresh_Clicked(TH_WPF.Button bt)
        {
            Devices_DG.Items.Refresh();
        }
    }

    public class DataGridMenuItem : MenuItem
    {

        public object DataObject
        {
            get { return (object)GetValue(DataObjectProperty); }
            set { SetValue(DataObjectProperty, value); }
        }

        public static readonly DependencyProperty DataObjectProperty =
            DependencyProperty.Register("DataObject", typeof(object), typeof(DataGridMenuItem), new PropertyMetadata(null));

    }
}
