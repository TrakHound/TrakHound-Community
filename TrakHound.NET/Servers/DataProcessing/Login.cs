// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using TrakHound.API.Users;
using TrakHound.Configurations;
using TrakHound.Logging;
using TrakHound.Tools;

namespace TrakHound.Servers.DataProcessing
{
    public partial class ProcessingServer
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
            UserLoginFile.LoginData loginData = UserLoginFile.Read();
            Login(loginData);
        }

        public void Login(UserLoginFile.LoginData loginData)
        {
            UserConfiguration userConfig = null;

            if (loginData != null)
            {
                userConfig = UserManagement.TokenLogin(loginData.Token);
            }

            CurrentUser = userConfig;

            if (userConfig != null) Logger.Log(String_Functions.UppercaseFirst(userConfig.Username) + " Logged in Successfully");
        }

        public void Login(UserConfiguration userConfig)
        {
            CurrentUser = userConfig;

            if (userConfig != null) Logger.Log(String_Functions.UppercaseFirst(userConfig.Username) + " Logged in Successfully");
        }

        public void Logout()
        {
            Stop();
            CurrentUser = null;
        }

        private void UpdateLoginInformation(DeviceServer server)
        {
            if (CurrentUser != null) server.SendPluginsData("USER_LOGIN", CurrentUser);
            else server.SendPluginsData("USER_LOGIN", null);
        }
    }
}
