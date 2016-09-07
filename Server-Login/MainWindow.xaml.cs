// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Threading;
using System.Windows;

using TrakHound.API;
using TrakHound.API.Users;
using TrakHound.Configurations;
using TrakHound.Tools;

namespace Server_Login
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            ApiConfiguration.Set(ApiConfiguration.Read());
        }

        public Database_Settings userDatabaseSettings;

        UserConfiguration currentuser;
        public UserConfiguration CurrentUser
        {
            get { return currentuser; }
            set
            {
                currentuser = value;

                CurrentUserChanged?.Invoke(currentuser);
            }
        }

        public delegate void CurrentUserChanged_Handler(UserConfiguration userConfig);
        public event CurrentUserChanged_Handler CurrentUserChanged;

        public delegate void Clicked_Handler();

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
            if (currentuser != null) CurrentUser = currentuser;
        }

        #region "Properties"

        public string LoginUsername
        {
            get { return (string)GetValue(LoginUsernameProperty); }
            set { SetValue(LoginUsernameProperty, value); }
        }

        public static readonly DependencyProperty LoginUsernameProperty =
            DependencyProperty.Register("LoginUsername", typeof(string), typeof(MainWindow), new PropertyMetadata(null));


        public string LoginPassword
        {
            get { return (string)GetValue(LoginPasswordProperty); }
            set { SetValue(LoginPasswordProperty, value); }
        }

        public static readonly DependencyProperty LoginPasswordProperty =
            DependencyProperty.Register("LoginPassword", typeof(string), typeof(MainWindow), new PropertyMetadata(null));


        public bool IsUsernameFocused
        {
            get { return (bool)GetValue(IsUsernameFocusedProperty); }
            set { SetValue(IsUsernameFocusedProperty, value); }
        }

        public static readonly DependencyProperty IsUsernameFocusedProperty =
            DependencyProperty.Register("IsUsernameFocused", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));


        public bool IsPasswordFocused
        {
            get { return (bool)GetValue(IsPasswordFocusedProperty); }
            set { SetValue(IsPasswordFocusedProperty, value); }
        }

        public static readonly DependencyProperty IsPasswordFocusedProperty =
            DependencyProperty.Register("IsPasswordFocused", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        public bool UserLoginError
        {
            get { return (bool)GetValue(UserLoginErrorProperty); }
            set { SetValue(UserLoginErrorProperty, value); }
        }

        public static readonly DependencyProperty UserLoginErrorProperty =
            DependencyProperty.Register("UserLoginError", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));
        

        public bool Loading
        {
            get { return (bool)GetValue(LoadingProperty); }
            set
            {
                SetValue(LoadingProperty, value);
            }
        }

        public static readonly DependencyProperty LoadingProperty =
            DependencyProperty.Register("Loading", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));


        public string LoadingMessage
        {
            get { return (string)GetValue(LoadingMessageProperty); }
            set { SetValue(LoadingMessageProperty, value); }
        }

        public static readonly DependencyProperty LoadingMessageProperty =
            DependencyProperty.Register("LoadingMessage", typeof(string), typeof(MainWindow), new PropertyMetadata(null));


        public bool LoginError
        {
            get { return (bool)GetValue(LoginErrorProperty); }
            set { SetValue(LoginErrorProperty, value); }
        }

        public static readonly DependencyProperty LoginErrorProperty =
            DependencyProperty.Register("LoginError", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        #endregion

        #region "Login"

        class Login_Info
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public void Login(string username, string password)
        {
            Loading = true;
            LoadingMessage = String_Functions.UppercaseFirst(username);
            LoginError = false;

            Login_Info info = new Login_Info();
            info.Username = username;
            info.Password = password;

            ThreadPool.QueueUserWorkItem(new WaitCallback(Login_Worker), info);
        }

        void Login_Worker(object o)
        {
            if (o != null)
            {
                Login_Info info = (Login_Info)o;

                var userConfig = UserManagement.CreateTokenLogin(info.Username, info.Password, "TrakHound Server Login");

                if (userConfig != null) UserLoginFile.Create(userConfig);
                else UserLoginFile.Remove();

                Dispatcher.BeginInvoke(new Action<UserConfiguration>(Login_Finished), UI_Functions.PRIORITY_BACKGROUND, new object[] { userConfig });
            }
        }

        void Login_Finished(UserConfiguration userConfig)
        {
            // If login was successful
            if (userConfig != null)
            {
                currentuser = userConfig;
                Close();
            }
            else
            {
                LoginError = true;
                LoginUsername = null;
                LoginPassword = null;
            }

            Loading = false;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (currentuser != null) CurrentUser = currentuser;
        }

        #endregion

        private void Login_Clicked(TrakHound_UI.Button bt)
        {
            Login(LoginUsername, LoginPassword);
            LoginPassword = null;
        }

        private void Username_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter) IsUsernameFocused = true;
        }

        private void Password_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter) Login(LoginUsername, LoginPassword);
        }

        private void LoginUsername_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) { UserLoginError = false; }

        private void LoginPassword_PasswordChanged(object sender, RoutedEventArgs e) { UserLoginError = false; }

    }
}
