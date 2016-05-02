// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using TH_Configuration;
using TH_Global;
using TH_Global.Functions;
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

        public Database_Settings UserDatabaseSettings { get; set; }



        public void Login()
        {
            UserConfiguration userConfig = null;

            UserLoginFile.LoginData loginData = UserLoginFile.Read();
            if (loginData != null)
            {
                userConfig = Users.LoginWithHash(loginData.Username, loginData.Password);
            }

            CurrentUser = userConfig;
        }

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
    }
}
