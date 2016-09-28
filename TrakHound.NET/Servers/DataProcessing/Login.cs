// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using TrakHound.API.Users;
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

        public delegate void CurrentUserChanged_Handler(UserConfiguration userConfig);
        public event CurrentUserChanged_Handler CurrentUserChanged;

        void SendCurrentUserChanged(UserConfiguration userConfig)
        {
            CurrentUserChanged?.Invoke(userConfig);  
        }


        public void Login()
        {
            //UserLoginFile.LoginData loginData = UserLoginFile.Read();
            var loginData = ServerCredentials.Read();
            Login(loginData);
        }

        public void Login(ServerCredentials.LoginData loginData)
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
            if (CurrentUser != null) server.SendPluginData("USER_LOGIN", CurrentUser);
            else server.SendPluginData("USER_LOGIN", null);
        }
    }
}
