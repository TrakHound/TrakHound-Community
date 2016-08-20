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

        public void Logout()
        {
            ProfileImage = null;
            UserLoading = true;
            UserLoadingText = "Logging out";

            ThreadPool.QueueUserWorkItem(new WaitCallback(Logout_Worker));
        }

        void Logout_Worker(object o)
        {
            bool success = UserManagement.Logout();

            Properties.Settings.Default.LoginRememberToken = null;
            Properties.Settings.Default.Save();

            Dispatcher.BeginInvoke(new Action<bool>(Logout_Finished), UI_Functions.PRIORITY_BACKGROUND, new object[] { success });
        }

        void Logout_Finished(bool success)
        {
            if (!success) TrakHound_UI.MessageBox.Show("Error During User Logout");

            CurrentUser = null;

            UserLoadingText = null;
            UserLoading = false;
        }

    }
}
