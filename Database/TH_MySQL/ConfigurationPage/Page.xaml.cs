using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Data;

using TH_Database;
using TH_Global;
using TH_Global.Functions;
using TH_Plugins.Database;
using TH_UserManagement;
using TH_UserManagement.Management;


namespace TH_MySQL.ConfigurationPage
{
    /// <summary>
    /// Interaction logic for Page.xaml
    /// </summary>
    public partial class Page : UserControl, IConfigurationPage
    {
        public Page()
        {
            InitializeComponent();
            DataContext = this;

            trakhoundserver_CHK.IsChecked = true;

            SetTrakHoundClourServer();
        }

        public string PageName { get { return "MySQL"; } }

        public ImageSource Image { get { return null; } }

        public event SettingChanged_Handler SettingChanged;

        public string prefix { get; set; }

        public void LoadConfiguration(DataTable dt)
        {
            Loading = true;

            configurationTable = dt;

            // Load UseTrakHoundCloudServer
            bool useTrakHoundCloudServer = false;
            string strUseTrakHoundCloudServer = Table_Functions.GetTableValue(prefix + "UseTrakHoundCloudServer", dt);
            if (strUseTrakHoundCloudServer != null)
            {
                bool.TryParse(strUseTrakHoundCloudServer, out useTrakHoundCloudServer);
                //trakhoundserver_CHK.IsChecked = useTrakHoundCloudServer;
            }
            trakhoundserver_CHK.IsChecked = useTrakHoundCloudServer;

            // Load Database Name
            DatabaseName = Table_Functions.GetTableValue(prefix + "Database", dt);

            // Load Server
            Server = Table_Functions.GetTableValue(prefix + "Server", dt);

            // Load Port
            Port = Table_Functions.GetTableValue(prefix + "Port", dt);

            // Load Username
            Username = Table_Functions.GetTableValue(prefix + "Username", dt);


            // Load PHP Server
            PhpServer = Table_Functions.GetTableValue(prefix + "PHP_Server", dt);

            // Load PHP Directory
            PhpDirectory = Table_Functions.GetTableValue(prefix + "PHP_Directory", dt);


            // Load Password
            password_TXT.PasswordText = Table_Functions.GetTableValue(prefix + "Password", dt);
            confirmpassword_TXT.PasswordText = Table_Functions.GetTableValue(prefix + "Password", dt);


            // Load UsePHP
            bool usePHP = false;
            string strUsePHP = Table_Functions.GetTableValue(prefix + "UsePHP", dt);
            if (strUsePHP != null)
            {
                bool.TryParse(strUsePHP, out usePHP);
                usephp_CHK.IsChecked = usePHP;
            }

            Loading = false;
        }

        public void SaveConfiguration(DataTable dt)
        {
            // Save TrakHound Cloud Server
            //Table_Functions.UpdateTableValue(UseTrakHoundCloudServer.ToString(), prefix + "UseTrakHoundCloudServer", dt);
            if (trakhoundserver_CHK.IsChecked == true) Table_Functions.UpdateTableValue("true", prefix + "UseTrakHoundCloudServer", dt);
            else Table_Functions.UpdateTableValue("false", prefix + "UseTrakHoundCloudServer", dt);

            // Save Database Name
            Table_Functions.UpdateTableValue(DatabaseName, prefix + "Database", dt);

            // Save Server
            Table_Functions.UpdateTableValue(Server, prefix + "Server", dt);

            // Save Port
            Table_Functions.UpdateTableValue(Port, prefix + "Port", dt);

            // Save Username
            Table_Functions.UpdateTableValue(Username, prefix + "Username", dt);

            // Save Password
            if (PasswordVerified || UseTrakHoundCloudServer)
            {
                Table_Functions.UpdateTableValue(password_TXT.PasswordText, prefix + "Password", dt);
            }

            // Save PHP Server
            Table_Functions.UpdateTableValue(PhpServer, prefix + "PHP_Server", dt);

            // Save PHP Directory
            Table_Functions.UpdateTableValue(PhpDirectory, prefix + "PHP_Directory", dt);

            // Save UsePHP
            Table_Functions.UpdateTableValue(UsePHPServer.ToString(), prefix + "UsePHP", dt);
        }

        private Application_Type _applicationType;
        public Application_Type ApplicationType
        {
            get { return _applicationType; }
            set
            {
                _applicationType = value;

                SetTrakHoundClourServer();
            }
        }

        public IDatabasePlugin Plugin { get { return new TH_MySQL.Plugin(); } }

        private void TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            UIElement txt = (UIElement)sender;

            if (txt.IsMouseCaptured || txt.IsKeyboardFocused)
            {
                ChangeSetting(null, null);
            }
        }

        private void databasename_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChangeSetting(prefix + "Database", ((TextBox)sender).Text);
        }

        private void server_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChangeSetting(prefix + "Server", ((TextBox)sender).Text);
        }

        private void port_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChangeSetting(prefix + "Port", ((TextBox)sender).Text);
        }

        private void username_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChangeSetting(prefix + "Username", ((TextBox)sender).Text);
        }

        void ChangeSetting(string name, string val)
        {
            if (!Loading)
            {
                string newVal = val;
                string oldVal = null;

                if (configurationTable != null)
                {
                    oldVal = Table_Functions.GetTableValue(name, configurationTable);
                }

                if (SettingChanged != null) SettingChanged(name, oldVal, newVal);
            }
        }

        public bool Loading
        {
            get { return (bool)GetValue(LoadingProperty); }
            set { SetValue(LoadingProperty, value); }
        }

        public static readonly DependencyProperty LoadingProperty =
            DependencyProperty.Register("Loading", typeof(bool), typeof(Page), new PropertyMetadata(false));

        DataTable configurationTable;


        private void Help_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender.GetType() == typeof(Rectangle))
            {
                Rectangle rect = (Rectangle)sender;

                if (rect.ToolTip != null)
                {
                    if (rect.ToolTip.GetType() == typeof(ToolTip))
                    {
                        ToolTip tt = (ToolTip)rect.ToolTip;
                        tt.IsOpen = true;
                    }
                }
            }
        }

        private void Help_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender.GetType() == typeof(Rectangle))
            {
                Rectangle rect = (Rectangle)sender;

                if (rect.ToolTip != null)
                {
                    if (rect.ToolTip.GetType() == typeof(ToolTip))
                    {
                        ToolTip tt = (ToolTip)rect.ToolTip;
                        tt.IsOpen = true;
                    }
                }
            }
        }

        private void Help_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender.GetType() == typeof(Rectangle))
            {
                Rectangle rect = (Rectangle)sender;

                if (rect.ToolTip != null)
                {
                    if (rect.ToolTip.GetType() == typeof(ToolTip))
                    {
                        ToolTip tt = (ToolTip)rect.ToolTip;
                        tt.IsOpen = false;
                    }
                }
            }
        }

        #region "Properties"

        public string DatabaseName
        {
            get { return (string)GetValue(DatabaseNameProperty); }
            set { SetValue(DatabaseNameProperty, value); }
        }

        public static readonly DependencyProperty DatabaseNameProperty =
            DependencyProperty.Register("DatabaseName", typeof(string), typeof(Page), new PropertyMetadata(null));


        public string Server
        {
            get { return (string)GetValue(ServerProperty); }
            set { SetValue(ServerProperty, value); }
        }

        public static readonly DependencyProperty ServerProperty =
            DependencyProperty.Register("Server", typeof(string), typeof(Page), new PropertyMetadata(null));


        public string Port
        {
            get { return (string)GetValue(PortProperty); }
            set { SetValue(PortProperty, value); }
        }

        public static readonly DependencyProperty PortProperty =
            DependencyProperty.Register("Port", typeof(string), typeof(Page), new PropertyMetadata(null));


        public string Username
        {
            get { return (string)GetValue(UsernameProperty); }
            set { SetValue(UsernameProperty, value); }
        }

        public static readonly DependencyProperty UsernameProperty =
            DependencyProperty.Register("Username", typeof(string), typeof(Page), new PropertyMetadata(null));





        public string PhpServer
        {
            get { return (string)GetValue(PhpServerProperty); }
            set { SetValue(PhpServerProperty, value); }
        }

        public static readonly DependencyProperty PhpServerProperty =
            DependencyProperty.Register("PhpServer", typeof(string), typeof(Page), new PropertyMetadata(null));


        public string PhpDirectory
        {
            get { return (string)GetValue(PhpDirectoryProperty); }
            set { SetValue(PhpDirectoryProperty, value); }
        }

        public static readonly DependencyProperty PhpDirectoryProperty =
            DependencyProperty.Register("PhpDirectory", typeof(string), typeof(Page), new PropertyMetadata(null));

        

        #endregion

        #region "Password"

        private void password_TXT_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var txt = (PasswordBox)sender;

            if (txt.IsMouseCaptured || txt.IsKeyboardFocused)
            {
                if (password_TXT.PasswordText != "") PasswordEntered = true;
                else PasswordEntered = false;

                PasswordShort = false;
                PasswordLong = false;

                ConfirmPassword();
            }
        }

        private void confirmpassword_TXT_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var txt = (PasswordBox)sender;

            if (txt.IsMouseCaptured || txt.IsKeyboardFocused)
            {
                if (confirmpassword_TXT.PasswordText != "") ConfirmPasswordEntered = true;
                else ConfirmPasswordEntered = false;

                ConfirmPassword();
            }
        }


        public bool PasswordEntered
        {
            get { return (bool)GetValue(PasswordEnteredProperty); }
            set { SetValue(PasswordEnteredProperty, value); }
        }

        public static readonly DependencyProperty PasswordEnteredProperty =
            DependencyProperty.Register("PasswordEntered", typeof(bool), typeof(Page), new PropertyMetadata(false));


        public bool ConfirmPasswordEntered
        {
            get { return (bool)GetValue(ConfirmPasswordEnteredProperty); }
            set { SetValue(ConfirmPasswordEnteredProperty, value); }
        }

        public static readonly DependencyProperty ConfirmPasswordEnteredProperty =
            DependencyProperty.Register("ConfirmPasswordEntered", typeof(bool), typeof(Page), new PropertyMetadata(false));


        public bool PasswordVerified
        {
            get { return (bool)GetValue(PasswordVerifiedProperty); }
            set { SetValue(PasswordVerifiedProperty, value); }
        }

        public static readonly DependencyProperty PasswordVerifiedProperty =
            DependencyProperty.Register("PasswordVerified", typeof(bool), typeof(Page), new PropertyMetadata(false));


        public bool PasswordShort
        {
            get { return (bool)GetValue(PasswordShortProperty); }
            set { SetValue(PasswordShortProperty, value); }
        }

        public static readonly DependencyProperty PasswordShortProperty =
            DependencyProperty.Register("PasswordShort", typeof(bool), typeof(Page), new PropertyMetadata(false));


        public bool PasswordLong
        {
            get { return (bool)GetValue(PasswordLongProperty); }
            set { SetValue(PasswordLongProperty, value); }
        }

        public static readonly DependencyProperty PasswordLongProperty =
            DependencyProperty.Register("PasswordLong", typeof(bool), typeof(Page), new PropertyMetadata(false));

        System.Timers.Timer password_TIMER;

        void VerifyPassword()
        {
            if (password_TIMER != null) password_TIMER.Enabled = false;

            password_TIMER = new System.Timers.Timer();
            password_TIMER.Interval = 500;
            password_TIMER.Elapsed += password_TIMER_Elapsed;
            password_TIMER.Enabled = true;
        }

        void password_TIMER_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            password_TIMER.Enabled = false;

            this.Dispatcher.BeginInvoke(new Action(VerifyPassword_GUI));
        }

        void VerifyPassword_GUI()
        {
            System.Security.SecureString pwd = password_TXT.SecurePassword;

            if (!Security_Functions.VerifyPasswordMinimum(pwd)) PasswordShort = true;
            else if (!Security_Functions.VerifyPasswordMaximum(pwd)) PasswordLong = true;
        }

        System.Timers.Timer confirmpassword_TIMER;

        void ConfirmPassword()
        {
            if (confirmpassword_TIMER != null) confirmpassword_TIMER.Enabled = false;

            confirmpassword_TIMER = new System.Timers.Timer();
            confirmpassword_TIMER.Interval = 500;
            confirmpassword_TIMER.Elapsed += confirmpassword_TIMER_Elapsed;
            confirmpassword_TIMER.Enabled = true;
        }

        void confirmpassword_TIMER_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            confirmpassword_TIMER.Enabled = false;

            this.Dispatcher.BeginInvoke(new Action(ConfirmPassword_GUI));
        }

        void ConfirmPassword_GUI()
        {
            if (PasswordEntered && ConfirmPasswordEntered)
            {
                if (password_TXT.PasswordText == confirmpassword_TXT.PasswordText) PasswordVerified = true;
                else PasswordVerified = false;

                if (PasswordVerified) ChangeSetting(prefix + "Password", password_TXT.PasswordText);
            }
        }

        #endregion

        #region "PHP"

        public bool UsePHPServer
        {
            get { return (bool)GetValue(UsePHPServerProperty); }
            set 
            { 
                SetValue(UsePHPServerProperty, value);
            }
        }

        public static readonly DependencyProperty UsePHPServerProperty =
            DependencyProperty.Register("UsePHPServer", typeof(bool), typeof(Page), new PropertyMetadata(false));
  

        private void phpserver_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChangeSetting(prefix + "PHP_Server", ((TextBox)sender).Text);
        }

        private void phpdirectory_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChangeSetting(prefix + "PHP_Directory", ((TextBox)sender).Text);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UsePHPServer = true;
            ChangeSetting(prefix + "UsePHP", UsePHPServer.ToString());
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            UsePHPServer = false;
            ChangeSetting(prefix + "UsePHP", UsePHPServer.ToString());
        }

        #endregion

        #region "TrakHound Cloud Server"

        void SetTrakHoundClourServer()
        {
            if (trakhoundserver_CHK.IsChecked == true)
            {
                UseTrakHoundCloudServer = true;

                Server = "localhost";
                Port = null;

                if (ApplicationType == Application_Type.Client)
                {
                    Username = "feenuxco_reader";
                    password_TXT.PasswordText = "#reader";
                    confirmpassword_TXT.PasswordText = "#reader";
                }
                else
                {
                    Username = "feenuxco_th";
                    password_TXT.PasswordText = "#mtconnect";
                    confirmpassword_TXT.PasswordText = "#mtconnect";
                }

                UsePHPServer = true;
                usephp_CHK.IsChecked = true;
                PhpServer = "www.feenux.com";
                PhpDirectory = "php";
            }
            else
            {
                // Only clear data if previously set to UseTrakHoundCloudServer (this prevents cloud server settings from being displayed to user)
                if (UseTrakHoundCloudServer)
                {
                    UseTrakHoundCloudServer = false;

                    Server = null;
                    Port = null;

                    Username = null;
                    password_TXT.PasswordText = null;
                    confirmpassword_TXT.PasswordText = null;

                    UsePHPServer = false;
                    usephp_CHK.IsChecked = false;
                    PhpServer = null;
                    PhpDirectory = null;
                }
            }
        }

        public bool UseTrakHoundCloudServer
        {
            get { return (bool)GetValue(UseTrakHoundCloudServerProperty); }
            set 
            {
                SetValue(UseTrakHoundCloudServerProperty, value);
            }
        }

        public static readonly DependencyProperty UseTrakHoundCloudServerProperty =
            DependencyProperty.Register("UseTrakHoundCloudServer", typeof(bool), typeof(Page), new PropertyMetadata(false));


        private void trakhoundserver_CHK_Checked(object sender, RoutedEventArgs e)
        {
            SetTrakHoundClourServer();

            ChangeSetting(prefix + "UseTrakHoundServer", UseTrakHoundCloudServer.ToString());
        }

        private void trakhoundserver_CHK_Unchecked(object sender, RoutedEventArgs e)
        {
            SetTrakHoundClourServer();

            ChangeSetting(prefix + "UseTrakHoundServer", UseTrakHoundCloudServer.ToString());
        }

        #endregion

    }
}
