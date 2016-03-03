// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Windows;
using System.Windows.Controls;
using TH_Configuration;
using TH_UserManagement.Management;

using TH_DeviceManager.Controls;

namespace TH_DeviceManager
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class DeviceManagerList : UserControl
    {
        public DeviceManagerList()
        {
            InitializeComponent();
            DataContext = this;
        }

        UserConfiguration currentuser;
        public UserConfiguration CurrentUser
        {
            get { return currentuser; }
            set
            {
                currentuser = value;
                LoadDevices();
            }
        }

        public delegate void DeviceEditSelected_Handler(Configuration config);
        public event DeviceEditSelected_Handler DeviceEditSelected;
        public event DeviceEditSelected_Handler DeviceEditTableSelected;


        const System.Windows.Threading.DispatcherPriority PRIORITY_BACKGROUND = System.Windows.Threading.DispatcherPriority.Background;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TH_WPF.MessageBox.Show("Click");
        }







        #region "Datarow Buttons"

        private void ClientEnabled_Checked(object sender, RoutedEventArgs e)
        {
            EnableDevice((DataGridCellCheckBox)sender, DeviceManagerType.Client);
        }

        private void ClientEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            DisableDevice((DataGridCellCheckBox)sender, DeviceManagerType.Client);
        }

        private void ServerEnabled_Checked(object sender, RoutedEventArgs e)
        {
            EnableDevice((DataGridCellCheckBox)sender, DeviceManagerType.Server);
        }

        private void ServerEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            DisableDevice((DataGridCellCheckBox)sender, DeviceManagerType.Server);
        }

        private void Edit_Clicked(TH_WPF.Button bt)
        {
            if (bt.DataObject != null)
            {
                var info = (DeviceInfo)bt.DataObject;

                if (DeviceEditSelected != null) DeviceEditSelected(info.Configuration);
            }
        }

        #endregion

        #region "Toolbar Buttons"

        private void Edit_Mulitple_Clicked(TH_WPF.Button bt)
        {
            foreach (var device in Device_DG.SelectedItems)
            {
                var info = (DeviceInfo)device;

                if (DeviceEditSelected != null) DeviceEditSelected(info.Configuration);
            }
        }

        private void Refresh_Clicked(TH_WPF.Button bt)
        {
            LoadDevices();
        }

        #endregion

        #region "Context Menu Buttons"

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            foreach (var device in Device_DG.SelectedItems)
            {
                var info = (DeviceInfo)device;

                if (DeviceEditSelected != null) DeviceEditSelected(info.Configuration);
            }
        }

        private void EditTable_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (DataGridMenuItem)sender;
            if (menuItem.DataObject != null)
            {
                var deviceInfo = (DeviceInfo)menuItem.DataObject;

                if (deviceInfo.Configuration != null)
                {
                    if (DeviceEditTableSelected != null) DeviceEditTableSelected(deviceInfo.Configuration);
                }
            }
        }

        private void Option2MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (Controls.DataGridMenuItem)sender;
            if (menuItem.DataObject != null)
            {
                var deviceInfo = (DeviceInfo)menuItem.DataObject;

                TH_WPF.MessageBox.Show("Option2 :: " + deviceInfo.Description);
            }
        }

        #endregion

        private void Button_Clicked(TH_WPF.Button bt)
        {

        }


    }

}
