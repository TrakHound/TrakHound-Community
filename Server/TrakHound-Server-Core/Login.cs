// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.IO;

using TH_Configuration;
using TH_Global;
using TH_Global.Functions;
using TH_Global.TrakHound.Users;

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

        public Database_Settings UserDatabaseSettings { get; set; }

        public delegate void CurrentUserChanged_Handler(UserConfiguration userConfig);
        public event CurrentUserChanged_Handler CurrentUserChanged;

        void SendCurrentUserChanged(UserConfiguration userConfig)
        {
            if (CurrentUserChanged != null) CurrentUserChanged(userConfig);  
        }


        public void Login()
        {
            UserConfiguration userConfig = null;

            UserLoginFile.LoginData loginData = UserLoginFile.Read();
            if (loginData != null)
            {
                userConfig = UserManagement.TokenLogin(loginData.Token);
            }

            CurrentUser = userConfig;
        }

        public void Login(UserConfiguration userConfig)
        {
            CurrentUser = userConfig;

            Logger.Log(String_Functions.UppercaseFirst(userConfig.Username) + " Logged in Successfully");
        }

        public void Logout()
        {
            Stop();
            CurrentUser = null;
        }

        #region "Local User ID"

        private const string LOCAL_USER_ID = "local_user_id";

        private string GetLoginRegistyKey()
        {
            string localUserId = Registry_Functions.GetValue(LOCAL_USER_ID);
            if (localUserId == null)
            {
                // Generate new random Local User ID if not already set in Registry
                // (Should only need to be set once)
                localUserId = String_Functions.RandomString(10);

                Registry_Functions.SetKey(LOCAL_USER_ID, localUserId);
            }

            return localUserId;
        }

        #endregion

        #region "UserLoginFileMonitor"

        private void UserLoginFileMonitor_Start()
        {
            string dir = FileLocations.AppData;
            string filename = "nigolresu";

            var watcher = new FileSystemWatcher(dir, filename);
            watcher.Changed += UserLoginFileMonitor_Changed;
            watcher.Created += UserLoginFileMonitor_Changed;
            watcher.Deleted += UserLoginFileMonitor_Changed;
            watcher.EnableRaisingEvents = true;
        }

        private void UserLoginFileMonitor_Changed(object sender, FileSystemEventArgs e)
        {
            //if (IsRunnning) Login();
            Login();
        }

        #endregion

    }
}
