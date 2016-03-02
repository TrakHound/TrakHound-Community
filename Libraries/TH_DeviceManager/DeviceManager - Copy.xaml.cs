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


        const System.Windows.Threading.DispatcherPriority PRIORITY_BACKGROUND = System.Windows.Threading.DispatcherPriority.Background;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TH_WPF.MessageBox.Show("Click");
        }

        private void Edit_Clicked(TH_WPF.Button bt)
        {
            if (bt.DataObject != null)
            {
                var info = (DeviceInfo)bt.DataObject;

                if (DeviceEditSelected != null) DeviceEditSelected(info.Configuration);
            } 
        }

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

        private void Option1MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (Controls.DataGridMenuItem)sender;
            if (menuItem.DataObject != null)
            {
                var deviceInfo = (DeviceInfo)menuItem.DataObject;

                TH_WPF.MessageBox.Show("Option1 :: " + deviceInfo.Description);
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

    }

    //public enum DeviceManagerType
    //{
    //    Client = 0,
    //    Server = 1
    //}

}
