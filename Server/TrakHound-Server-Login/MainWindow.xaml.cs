// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using TH_Configuration;
using TH_Global.Functions;
using TH_Global.TrakHound.Users;

namespace TrakHound_Server_Login
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
        }

        public Database_Settings userDatabaseSettings;

        UserConfiguration currentuser;
        public UserConfiguration CurrentUser
        {
            get { return currentuser; }
            set
            {
                currentuser = value;

                if (CurrentUserChanged != null) CurrentUserChanged(currentuser);
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

        public bool UsernameEntered
        {
            get { return (bool)GetValue(UsernameEnteredProperty); }
            set { SetValue(UsernameEnteredProperty, value); }
        }

        public static readonly DependencyProperty UsernameEnteredProperty =
            DependencyProperty.Register("UsernameEntered", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));


        public bool PasswordEntered
        {
            get { return (bool)GetValue(PasswordEnteredProperty); }
            set { SetValue(PasswordEnteredProperty, value); }
        }

        public static readonly DependencyProperty PasswordEnteredProperty =
            DependencyProperty.Register("PasswordEntered", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));



        private void password_TXT_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (password_TXT.Password != "") PasswordEntered = true;
            else PasswordEntered = false;
        }

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

        const System.Windows.Threading.DispatcherPriority priority = System.Windows.Threading.DispatcherPriority.Background;


        #region "Login"

        //RememberMeType rememberMeType { get { return RememberMeType.Server; } }

        public bool RememberMe
        {
            get { return (bool)GetValue(RememberMeProperty); }
            set { SetValue(RememberMeProperty, value); }
        }

        public static readonly DependencyProperty RememberMeProperty =
            DependencyProperty.Register("RememberMe", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));


        private void CheckBox_Checked(object sender, RoutedEventArgs e) { RememberMe = true; }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e) { RememberMe = false; }



        private void Login_Clicked(TH_WPF.Button bt)
        {
            LoginUser(username_TXT.Text, password_TXT.Password);
            password_TXT.Clear();
        }

        class Login_Info
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public bool RememberMe { get; set; }
        }

        Thread login_THREAD;

        public void LoginUser(string username, string password)
        {
            Loading = true;
            LoadingMessage = String_Functions.UppercaseFirst(username);
            LoginError = false;

            Login_Info info = new Login_Info();
            info.Username = username;
            info.Password = password;
            info.RememberMe = RememberMe;

            if (login_THREAD != null) login_THREAD.Abort();

            login_THREAD = new Thread(new ParameterizedThreadStart(Login_Worker));
            login_THREAD.Start(info);
        }

        void Login_Worker(object o)
        {
            if (o != null)
            {
                Login_Info info = (Login_Info)o;

                var userConfig = UserManagement.CreateTokenLogin(info.Username, info.Password, "TrakHound Server Login");

                if (userConfig != null) UserLoginFile.Create(userConfig);
                else UserLoginFile.Remove();

                this.Dispatcher.BeginInvoke(new Action<UserConfiguration>(Login_Finished), priority, new object[] { userConfig });
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
                username_TXT.Clear();
                password_TXT.Clear();
            }

            Loading = false;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (currentuser != null) CurrentUser = currentuser;
        }

        #endregion


        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (username_TXT.IsFocused)
                {
                    password_TXT.Focus();
                }

                if (password_TXT.IsFocused)
                {
                    LoginUser(username_TXT.Text, password_TXT.Password);
                    password_TXT.Clear();
                }
            }
        }

        private void username_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (username_TXT.Text != String.Empty) UsernameEntered = true;
            else UsernameEntered = false;
        }

        private void password_TXT_GotFocus(object sender, RoutedEventArgs e)
        {
            password_TXT.Password = "";
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UsernameFocus();
        }

        System.Timers.Timer focus_TIMER;

        void UsernameFocus()
        {
            if (focus_TIMER != null) focus_TIMER.Enabled = false;

            focus_TIMER = new System.Timers.Timer();
            focus_TIMER.Interval = 200;
            focus_TIMER.Elapsed += focus_TIMER_Elapsed;
            focus_TIMER.Enabled = true;
        }

        void focus_TIMER_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            focus_TIMER.Enabled = false;

            this.Dispatcher.BeginInvoke(new Action(UsernameFocus_GUI));
        }

        void UsernameFocus_GUI()
        {
            username_TXT.Focus();
        }

    }
}
