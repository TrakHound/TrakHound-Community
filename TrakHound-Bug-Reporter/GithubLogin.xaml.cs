using System;
using System.Threading;
using System.Windows;

using TH_GitHub;
using TH_Global.Web;

namespace TrakHound_Bug_Reporter
{
    /// <summary>
    /// Interaction logic for GithubLogin.xaml
    /// </summary>
    public partial class GithubLogin : Window
    {
        public GithubLogin()
        {
            InitializeComponent();            
            root.DataContext = this;
        }


        public bool Loading
        {
            get { return (bool)GetValue(LoadingProperty); }
            set { SetValue(LoadingProperty, value); }
        }

        public static readonly DependencyProperty LoadingProperty =
            DependencyProperty.Register("Loading", typeof(bool), typeof(GithubLogin), new PropertyMetadata(false));


        public string Username
        {
            get { return (string)GetValue(UsernameProperty); }
            set { SetValue(UsernameProperty, value); }
        }

        public static readonly DependencyProperty UsernameProperty =
            DependencyProperty.Register("Username", typeof(string), typeof(GithubLogin), new PropertyMetadata(null));


        public string Password
        {
            get { return (string)GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }

        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register("Password", typeof(string), typeof(GithubLogin), new PropertyMetadata(null));


        public bool RememberLogin
        {
            get { return (bool)GetValue(RememberLoginProperty); }
            set { SetValue(RememberLoginProperty, value); }
        }

        public static readonly DependencyProperty RememberLoginProperty =
            DependencyProperty.Register("RememberLogin", typeof(bool), typeof(GithubLogin), new PropertyMetadata(false));



        public new static HTTP.HeaderData Show()
        {
            var window = new GithubLogin();
            bool? dialogResult = window.ShowDialog();

            if (dialogResult == true)
            {
                return window.LoginHeaderData;
            }

            //if (dialogResult == true && !string.IsNullOrEmpty(window.Username) && !string.IsNullOrEmpty(window.Password))
            //{
            //    return GetLoginHeader(window.Username, window.Password, window.RememberLogin);
            //}

            return null;
        }

        public HTTP.HeaderData LoginHeaderData { get; set; }

        private class LoginInfo
        {
            public LoginInfo(string username, string password, bool rememberToken)
            {
                Username = username;
                Password = password;
                RememberToken = rememberToken;
            }

            public string Username { get; set; }
            public string Password { get; set; }
            public bool RememberToken { get; set; }
        }

        private void Login_Clicked(TH_WPF.Button bt)
        {
            var info = new LoginInfo(Username, Password, RememberLogin);

            ThreadPool.QueueUserWorkItem(new WaitCallback(Login_Worker), info);
        }

        private void Login_Worker(object o)
        {
            if (o != null)
            {
                var info = (LoginInfo)o;

                var headerData = GetLoginHeader(info.Username, info.Password, info.RememberToken);

                Dispatcher.BeginInvoke(new Action<HTTP.HeaderData>(Login_GUI), new object[] { headerData });
            }
        }

        public static HTTP.HeaderData GetLoginHeader(string username, string password, bool rememberToken)
        {
            var credentials = new Authentication.Crendentials();
            credentials.Username = username;
            credentials.Password = password;

            if (rememberToken)
            {
                var oAuth2Token = OAuth2Token.Get(credentials);
                if (oAuth2Token != null && !string.IsNullOrEmpty(oAuth2Token.Token))
                {
                    Properties.Settings.Default.Github_Username = username;
                    Properties.Settings.Default.Github_Token = oAuth2Token.Token;
                    Properties.Settings.Default.Save();
                }
            }

            return Authentication.GetBasicHeader(credentials);
        }

        private void Login_GUI(HTTP.HeaderData headerData)
        {
            Loading = false;

            if (headerData != null) // Login Successful
            {
                LoginHeaderData = headerData;

                this.DialogResult = true;
                Close();
            }
            else // Error during login
            {
                TH_WPF.MessageBox.Show("Error During GitHub Login. Check login credentials and try again.", "Login Error", TH_WPF.MessageBoxButtons.Ok);
            }
        }
    }
}
