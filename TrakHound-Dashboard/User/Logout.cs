using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Collections.Specialized;

using TrakHound.Configurations;
using TrakHound;
using TrakHound.Tools;
using TrakHound.API;
using TrakHound.API.Users;
using TrakHound.Tools.Web;
using TrakHound_UI;

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
