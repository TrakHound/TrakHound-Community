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
        ServerGroup serverGroup;

        public Device_Manager(ServerGroup group)
        {
            devicemanager = new DeviceManager(DeviceManagerType.Server);

            serverGroup = group;

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

        //UserConfiguration currentuser;
        //public UserConfiguration CurrentUser
        //{
        //    get { return currentuser; }
        //    set
        //    {
        //        currentuser = value;

        //        devicemanager.CurrentUser = currentuser;
        //    }
        //}

        public UserConfiguration CurrentUser
        {
            get { return (UserConfiguration)GetValue(CurrentUserProperty); }
            set 
            { 
                SetValue(CurrentUserProperty, value);
                devicemanager.CurrentUser = value;

                Loading = false;
            }
        }

        public static readonly DependencyProperty CurrentUserProperty =
            DependencyProperty.Register("CurrentUser", typeof(UserConfiguration), typeof(Device_Manager), new PropertyMetadata(null));


        public bool Loading
        {
            get { return (bool)GetValue(LoadingProperty); }
            set { SetValue(LoadingProperty, value); }
        }

        public static readonly DependencyProperty LoadingProperty =
            DependencyProperty.Register("Loading", typeof(bool), typeof(Device_Manager), new PropertyMetadata(false));


        Database_Settings userDatabaseSettings;
        public Database_Settings UserDatabaseSettings
        {
            get { return UserDatabaseSettings; }
            set
            {
                userDatabaseSettings = value;

                //if (devicemanager != null) devicemanager.UserDatabaseSettings = userDatabaseSettings;
            }
        }

        //void ReadUserManagementSettings()
        //{
        //    //DatabasePluginReader dpr = new DatabasePluginReader();

        //    string localPath = AppDomain.CurrentDomain.BaseDirectory + "UserConfiguration.Xml";
        //    string systemPath = TH_Global.FileLocations.TrakHound + @"\" + "UserConfiguration.Xml";

        //    string configPath;

        //    // systemPath takes priority (easier for user to navigate to)
        //    if (File.Exists(systemPath)) configPath = systemPath;
        //    else configPath = localPath;

        //    Logger.Log(configPath);

        //    UserManagementSettings userSettings = UserManagementSettings.ReadConfiguration(configPath);

        //    if (userSettings != null)
        //    {
        //        if (userSettings.Database.Databases.Count > 0)
        //        {
        //            userDatabaseSettings = userSettings.Database;
        //            Global.Initialize(userDatabaseSettings);
        //        }
        //    }
        //}

        #endregion

        void LoginMenu_MyAccountClicked()
        {
            TH_UserManagement.MyAccountPage page = new TH_UserManagement.MyAccountPage();
            page.LoadUserConfiguration(CurrentUser);

            TempPage = page;
        }

        private void Back_Clicked(TH_WPF.Button bt)
        {
            TempPage = null;
        }

        private void LoginButton_Clicked(TH_WPF.Button bt)
        {
            if (serverGroup != null) serverGroup.Login();
        }

    }
}
