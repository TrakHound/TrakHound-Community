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
using System.IO;

using TH_Configuration;
using TH_Database;
using TH_DeviceManager;
using TH_Global;
using TH_PlugIns_Server;
using TH_WPF;
using TH_UserManagement;
using TH_UserManagement.Management;

namespace TrakHound_Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Device_Manager : Window
    {
        public Device_Manager()
        {
            devicemanager = new DeviceManager(DeviceManagerType.Server);

            InitializeComponent();
            DataContext = this;

            CurrentPage = devicemanager;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        DeviceManager devicemanager;

        public object CurrentPage
        {
            get { return (object)GetValue(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        public static readonly DependencyProperty CurrentPageProperty =
            DependencyProperty.Register("CurrentPage", typeof(object), typeof(Device_Manager), new PropertyMetadata(null));

        public object TempPage
        {
            get { return (object)GetValue(TempPageProperty); }
            set 
            {
                SetValue(TempPageProperty, value);
            }
        }

        public static readonly DependencyProperty TempPageProperty =
            DependencyProperty.Register("TempPage", typeof(object), typeof(Device_Manager), new PropertyMetadata(null));

        #region "User Login"

        UserConfiguration currentuser;
        public UserConfiguration CurrentUser
        {
            get { return currentuser; }
            set
            {
                currentuser = value;

                devicemanager.CurrentUser = currentuser;
            }
        }

        public Database_Settings userDatabaseSettings;

        void ReadUserManagementSettings()
        {
            DatabasePluginReader dpr = new DatabasePluginReader();

            string localPath = AppDomain.CurrentDomain.BaseDirectory + "UserConfiguration.Xml";
            string systemPath = TH_Global.FileLocations.TrakHound + @"\" + "UserConfiguration.Xml";

            string configPath;

            // systemPath takes priority (easier for user to navigate to)
            if (File.Exists(systemPath)) configPath = systemPath;
            else configPath = localPath;

            Logger.Log(configPath);

            UserManagementSettings userSettings = UserManagementSettings.ReadConfiguration(configPath);

            if (userSettings != null)
            {
                if (userSettings.Databases.Databases.Count > 0)
                {
                    userDatabaseSettings = userSettings.Databases;
                    Global.Initialize(userDatabaseSettings);
                }
            }
        }

        #endregion

        void LoginMenu_MyAccountClicked()
        {
            TH_UserManagement.Create.Page page = new TH_UserManagement.Create.Page();
            page.LoadUserConfiguration(CurrentUser, userDatabaseSettings);

            TempPage = page;
        }

        private void Back_Clicked(Button_04 bt)
        {
            TempPage = null;
        }

    }
}
