// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Threading;
using TrakHound.API.Users;
using TrakHound.Tools;

namespace TrakHound_Dashboard
{
    public partial class MainWindow
    {
        private class Login_Info
        {
            public string username { get; set; }
            public string password { get; set; }
        }

        #region "Username and Password entered by user"

        public void Login(string username, string password)
        {
            ProfileImage = null;
            UserLoading = true;
            UserLoadingText = username;

            var info = new Login_Info();
            info.username = username;
            info.password = password;

            ThreadPool.QueueUserWorkItem(new WaitCallback(Login_Worker), info);
        }

        void Login_Worker(object o)
        {
            if (o != null)
            {
                Login_Info info = (Login_Info)o;

                UserConfiguration userConfig = null;

                userConfig = UserManagement.CreateTokenLogin(info.username, info.password, "TrakHound Client Login");
                if (userConfig != null)
                {
                    Properties.Settings.Default.LoginRememberToken = userConfig.Token;
                    Properties.Settings.Default.Save();
                }

                Dispatcher.BeginInvoke(new Action<UserConfiguration>(Login_Finished), UI_Functions.PRIORITY_BACKGROUND, new object[] { userConfig });
            }
        }

        #endregion

        #region "Auto Login (Remember Me) using Token"

        public void TokenLogin(string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                ProfileImage = null;
                UserLoading = true;
                UserLoadingText = "Logging in";

                ThreadPool.QueueUserWorkItem(new WaitCallback(TokenLogin_Worker), token);
            }
        }

        void TokenLogin_Worker(object o)
        {
            if (o != null)
            {
                string token = o.ToString();

                UserConfiguration userConfig = null;

                userConfig = UserManagement.TokenLogin(token, "TrakHound Client Token Login");

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    Login_Finished(userConfig);

                    if (userConfig == null) TrakHound_UI.MessageBox.Show("User Login Failed. Please Try Again.", "Login Failed", TrakHound_UI.MessageBoxButtons.Ok);


                }), UI_Functions.PRIORITY_BACKGROUND, new object[] { });
            }
        }

        #endregion


        // Update GUI after login using separate thread
        void Login_Finished(UserConfiguration userConfig)
        {
            CurrentUser = userConfig;

            UserLoadingText = null;
            UserLoading = false;

            LoginPassword = null;

            if (userConfig != null)
            {
                LoginUsername = null;
                UserLoginError = false;
            }
            else
            {
                UserLoginError = true;
            }
        }


        // Set Login File UserConfiguration
        private void Login(UserConfiguration userConfig)
        {
            if (userConfig != null) ServerCredentials.Create(userConfig);
            else ServerCredentials.Remove();
        }

    }
}
