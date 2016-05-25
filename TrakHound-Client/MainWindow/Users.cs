// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

using TH_Configuration;
using TH_Global.Functions;
using TH_Global.TrakHound.Users;
//using TH_UserManagement.Management;

namespace TrakHound_Client
{
    public partial class MainWindow
    {

        #region "Dependency Properties"

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


        public Database_Settings UserDatabaseSettings
        {
            get { return (Database_Settings)GetValue(UserDatabaseSettingsProperty); }
            set { SetValue(UserDatabaseSettingsProperty, value); }
        }

        public static readonly DependencyProperty UserDatabaseSettingsProperty =
            DependencyProperty.Register("UserDatabaseSettings", typeof(Database_Settings), typeof(MainWindow), new PropertyMetadata(null));

        #endregion

        #region "Events and Event Handlers"

        public delegate void CurrentUserChanged_Handler(UserConfiguration userConfig);
        public event CurrentUserChanged_Handler CurrentUserChanged;

        private void Login_GRID_PreviewMouseDown(object sender, MouseButtonEventArgs e) { LoginMenu.Shown = true; }

        private void LoginMenu_CurrentUserChanged(UserConfiguration userConfig) { Login(userConfig); }

        private void LoginMenu_ShownChanged(bool val) { }

        private void LoginMenu_MyAccountClicked() { AccountManager_Open(); }

        private void LoginMenu_CreateClicked() { AccountManager_Open(); }

        #endregion

        private UserConfiguration _currentuser;
        public UserConfiguration CurrentUser
        {
            get { return _currentuser; }
            set
            {
                _currentuser = value;

                if (DeviceManager != null) DeviceManager.CurrentUser = _currentuser;

                if (_currentuser != null)
                {
                    CurrentUsername = String_Functions.UppercaseFirst(_currentuser.Username);
                    LoggedIn = true;
                }
                else
                {
                    LoggedIn = false;
                    CurrentUsername = null;
                }

                if (accountpage != null) accountpage.LoadUserConfiguration(_currentuser);

                Plugins_UpdateUser(_currentuser);

                if (CurrentUserChanged != null) CurrentUserChanged(_currentuser);
            }
        }

        private UserLoginFile.LoginData serverLoginData;

        private void Users_Initialize()
        {
            //UserManagementSettings.ReadConfiguration();

            //LoginMenu.rememberMeType = RememberMeType.Client;
            LoginMenu.LoadRememberMe();
        }

        private void Users_ClientClosing()
        {
            if (serverLoginData != null && CurrentUser != null) ServerUser_Logout();
        }


        private void Login(UserConfiguration userConfig)
        {
            if (userConfig != null) UserLoginFile.Create(userConfig);
            else UserLoginFile.Remove();


            //bool serverRunning = Service_Functions.IsServiceRunning("TrakHound Server");
            //if (serverRunning)
            //{
            //    serverLoginData = UserLoginFile.Read();
            //    if (serverLoginData != null)
            //    {
            //        if (userConfig == null) ServerUser_Ask_Logout();
            //        else if (serverLoginData.Username.ToLower() != userConfig.Username.ToLower())
            //        {
            //            ServerUser_Ask_Change(userConfig);
            //        }
            //    }
            //    else if (userConfig != null) ServerUser_Ask_Login(userConfig);
            //}
            //else
            //{
            //    if (userConfig != null) ServerUser_Login(userConfig);
            //    else ServerUser_Logout();
            //}          

            CurrentUser = userConfig;
        }

        private void ServerUser_Ask_Login(UserConfiguration userConfig)
        {
            var result = TH_WPF.MessageBox.Show("Login Server User?", "Login Server User", TH_WPF.MessageBoxButtons.YesNo);
            if (result == TH_WPF.MessageBoxDialogResult.Yes) ServerUser_Login(userConfig);
        }

        private void ServerUser_Ask_Change(UserConfiguration userConfig)
        {
            var result = TH_WPF.MessageBox.Show("Server is logged in as a different user. Change Server User?", "Change Server User", TH_WPF.MessageBoxButtons.YesNo);
            if (result == TH_WPF.MessageBoxDialogResult.Yes) ServerUser_Login(userConfig);
        }

        private void ServerUser_Ask_Logout()
        {
            var result = TH_WPF.MessageBox.Show("Server is still logged in. Logout Server User?", "Logout Server User", TH_WPF.MessageBoxButtons.YesNo);
            if (result == TH_WPF.MessageBoxDialogResult.Yes) ServerUser_Logout();
        }


        private void ServerUser_Login(UserConfiguration userConfig)
        {
            UserLoginFile.Create(userConfig);
        }

        private void ServerUser_Logout()
        {
            UserLoginFile.Remove();
        }

    }
}
