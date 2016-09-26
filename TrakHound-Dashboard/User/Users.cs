// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Windows;

using TrakHound;
using TrakHound.API.Users;

namespace TrakHound_Dashboard
{
    public partial class MainWindow
    {
        private void Users_Initialize()
        {
            string token = Properties.Settings.Default.LoginRememberToken;

            if (!string.IsNullOrEmpty(token)) TokenLogin(token);
            else CurrentUser = null;
        }


        public delegate void CurrentUserChanged_Handler(UserConfiguration userConfig);
        public event CurrentUserChanged_Handler CurrentUserChanged;


        private bool firstLogin = true;

        private UserConfiguration _currentuser;
        public UserConfiguration CurrentUser
        {
            get { return (UserConfiguration)GetValue(CurrentUserProperty); }
            set
            {
                var previousUser = _currentuser;

                SetValue(CurrentUserProperty, value);

                _currentuser = value;

                if (previousUser != _currentuser || firstLogin)
                {
                    LoadDevices();

                    // Update other pages
                    if (accountpage != null) accountpage.LoadUserConfiguration(_currentuser);

                    SendCurrentUser();

                    // Login Server user (set login file)
                    Login(_currentuser);

                    // Restart Message monitor
                    StartMessageMonitor();
                    messageCenter.ClearMessages();

                    // Load the Profile Image (Nav Menu)
                    LoadProfileImage(_currentuser);

                    // Raise CurrentUserChanged Event
                    CurrentUserChanged?.Invoke(_currentuser);
                }

                firstLogin = false;
            }
        }

        public static readonly DependencyProperty CurrentUserProperty =
            DependencyProperty.Register("CurrentUser", typeof(UserConfiguration), typeof(MainWindow), new PropertyMetadata(null));


        public bool UserLoading
        {
            get { return (bool)GetValue(UserLoadingProperty); }
            set { SetValue(UserLoadingProperty, value); }
        }

        public static readonly DependencyProperty UserLoadingProperty =
            DependencyProperty.Register("UserLoading", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));


        public string UserLoadingText
        {
            get { return (string)GetValue(UserLoadingTextProperty); }
            set { SetValue(UserLoadingTextProperty, value); }
        }

        public static readonly DependencyProperty UserLoadingTextProperty =
            DependencyProperty.Register("UserLoadingText", typeof(string), typeof(MainWindow), new PropertyMetadata(null));


        public bool UserLoginError
        {
            get { return (bool)GetValue(UserLoginErrorProperty); }
            set { SetValue(UserLoginErrorProperty, value); }
        }

        public static readonly DependencyProperty UserLoginErrorProperty =
            DependencyProperty.Register("UserLoginError", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        /// <summary>
        /// Send the Current User as a message to other pages
        /// </summary>
        /// <param name="userConfig"></param>
        private void SendCurrentUser()
        {
            var data = CreateCurrentUserMessage();
            Plugin_SendData(data);
        }

        private void SendCurrentUser(IPage page)
        {
            var data = CreateCurrentUserMessage();
            page.GetSentData(data);
        }

        private EventData CreateCurrentUserMessage()
        {
            var data = new EventData(this);

            if (_currentuser != null)
            {
                data.Id = "USER_LOGIN";
                data.Data01 = _currentuser;
            }
            else
            {
                data.Id = "USER_LOGOUT";
            }

            return data;
        }

    }
}
