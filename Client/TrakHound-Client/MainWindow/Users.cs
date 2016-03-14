using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;

using TH_Configuration;
using TH_Database;
using TH_Global;
using TH_Global.Functions;
using TH_UserManagement.Management;

namespace TrakHound_Client
{
    public partial class MainWindow
    {

        #region "Properties"

        public string CurrentUsername
        {
            get { return (string)GetValue(CurrentUsernameProperty); }
            set { SetValue(CurrentUsernameProperty, value); }
        }

        public static readonly DependencyProperty CurrentUsernameProperty =
            DependencyProperty.Register("CurrentUsername", typeof(string), typeof(MainWindow), new PropertyMetadata(null));


        public ImageSource ProfileImage
        {
            get { return (ImageSource)GetValue(ProfileImageProperty); }
            set { SetValue(ProfileImageProperty, value); }
        }

        public static readonly DependencyProperty ProfileImageProperty =
            DependencyProperty.Register("ProfileImage", typeof(ImageSource), typeof(MainWindow), new PropertyMetadata(null));


        public bool LoggedIn
        {
            get { return (bool)GetValue(LoggedInProperty); }
            set { SetValue(LoggedInProperty, value); }
        }

        public static readonly DependencyProperty LoggedInProperty =
            DependencyProperty.Register("LoggedIn", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        #endregion

        public delegate void CurrentUserChanged_Handler(UserConfiguration userConfig);
        public event CurrentUserChanged_Handler CurrentUserChanged;

        private void Login_GRID_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            LoginMenu.Shown = true;
        }

        private void LoginMenu_CurrentUserChanged(UserConfiguration userConfig)
        {
            CurrentUser = userConfig;
        }

        private void LoginMenu_ShownChanged(bool val)
        {

        }

        private void LoginMenu_MyAccountClicked()
        {
            AccountManager_Open();
        }

        private void LoginMenu_CreateClicked()
        {
            AccountManager_Open();
        }

        UserConfiguration currentuser;
        public UserConfiguration CurrentUser
        {
            get { return currentuser; }
            set
            {
                currentuser = value;

                if (DeviceManager != null) DeviceManager.CurrentUser = currentuser;

                if (currentuser != null)
                {
                    CurrentUsername = String_Functions.UppercaseFirst(currentuser.username);
                    LoggedIn = true;
                }
                else
                {
                    LoggedIn = false;
                    CurrentUsername = null;
                }

                if (accountpage != null) accountpage.LoadUserConfiguration(currentuser);

                Plugins_UpdateUser(currentuser);

                if (CurrentUserChanged != null) CurrentUserChanged(currentuser);
            }
        }

        public Database_Settings UserDatabaseSettings
        {
            get { return (Database_Settings)GetValue(UserDatabaseSettingsProperty); }
            set { SetValue(UserDatabaseSettingsProperty, value); }
        }

        public static readonly DependencyProperty UserDatabaseSettingsProperty =
            DependencyProperty.Register("UserDatabaseSettings", typeof(Database_Settings), typeof(MainWindow), new PropertyMetadata(null));

        

        //Database_Settings userDatabaseSettings;
        //public Database_Settings UserDatabaseSettings
        //{
        //    get { return (Database_Settings)GetValue(UserDatabaseSettingsProperty); }
        //    set
        //    {
        //        SetValue(UserDatabaseSettingsProperty, value);

        //        userDatabaseSettings = value;

        //        if (LoginMenu != null) LoginMenu.UserDatabaseSettings = value;

        //        if (accountpage != null) accountpage.UserDatabaseSettings = value;
        //    }
        //}

        //public static readonly DependencyProperty UserDatabaseSettingsProperty =
        //    DependencyProperty.Register("UserDatabaseSettings", typeof(Database_Settings), typeof(MainWindow), new PropertyMetadata(null));

        void ReadUserManagementSettings()
        {
            //DatabasePluginReader dpr = new DatabasePluginReader();

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
                if (UserManagementSettings.Database != null)
                {
                    Global.Initialize(UserManagementSettings.Database);
                    UserDatabaseSettings = UserManagementSettings.Database;
                }

                //if (userSettings.Databases.Databases.Count > 0)
                //{
                //    UserDatabaseSettings = userSettings.Databases;
                //    Global.Initialize(UserDatabaseSettings);
                //}
            }
        }


    }
}
