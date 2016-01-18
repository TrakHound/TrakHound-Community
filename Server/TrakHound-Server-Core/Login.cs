// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Threading;

using TH_Configuration;
using TH_Database;
using TH_Device_Server;
using TH_Global;
using TH_Global.Functions;
using TH_UserManagement;
using TH_UserManagement.Management;

namespace TrakHound_Server_Core
{
    public partial class Server
    {

        UserConfiguration currentuser;
        public UserConfiguration CurrentUser
        {
            get { return currentuser; }
            set
            {
                currentuser = value;

                SendCurrentUserChanged(currentuser);
            }
        }

        void SendCurrentUserChanged(UserConfiguration userConfig)
        {
            if (CurrentUserChanged != null) CurrentUserChanged(userConfig);
        }

        public delegate void CurrentUserChanged_Handler(UserConfiguration userConfig);
        public event CurrentUserChanged_Handler CurrentUserChanged;

        Database_Settings userDatabaseSettings;

        public void Login(UserConfiguration userConfig)
        {
            CurrentUser = userConfig;

            Logger.Log(String_Functions.UppercaseFirst(userConfig.username) + " Logged in Successfully");
        }

        public void Logout()
        {
            Stop();
            CurrentUser = null;
        }

        void ReadUserManagementSettings()
        {
            DatabasePluginReader dpr = new DatabasePluginReader();

            string localPath = AppDomain.CurrentDomain.BaseDirectory + "UserConfiguration.Xml";
            string systemPath = TH_Global.FileLocations.TrakHound + @"\" + "UserConfiguration.Xml";

            string configPath;

            // systemPath takes priority (easier for user to navigate to)
            if (File.Exists(systemPath)) configPath = systemPath;
            else configPath = localPath;

            //Logger.Log(configPath);

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

        #region "Remember Me Monitor"

        System.Timers.Timer rememberMe_TIMER;

        void RememberMeMonitor_Start()
        {
            rememberMe_TIMER = new System.Timers.Timer();
            rememberMe_TIMER.Interval = 5000;
            rememberMe_TIMER.Elapsed += rememberMe_TIMER_Elapsed;
            rememberMe_TIMER.Enabled = true;
        }

        void rememberMe_TIMER_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(RememberMeMonitor_Worker));
        }

        void RememberMeMonitor_Worker(object o)
        {
           UserConfiguration rememberMe = RememberMe.Get(RememberMeType.Server, userDatabaseSettings);
           if (rememberMe != null && CurrentUser == null)
            {
                if (rememberMe_TIMER != null) rememberMe_TIMER.Enabled = false;
                Login(rememberMe);
            }
        }

        #endregion

    }
}
