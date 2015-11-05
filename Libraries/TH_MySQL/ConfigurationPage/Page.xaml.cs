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

using TH_Configuration.User;
using TH_Database;
using TH_Global;
using TH_Global.Functions;

namespace TH_MySQL.ConfigurationPage
{
    /// <summary>
    /// Interaction logic for Page.xaml
    /// </summary>
    public partial class Page : UserControl, DatabaseConfigurationPage
    {
        public Page()
        {
            InitializeComponent();
            DataContext = this;
        }

        public string PageName { get { return "MySQL"; } }

        public ImageSource Image { get { return null; } }

        public event SettingChanged_Handler SettingChanged;

        public string prefix { get; set; }

        public void LoadConfiguration(DataTable dt)
        {
            Loading = true;

            configurationTable = dt;

            // Load Database Name
            databasename_TXT.Text = Table_Functions.GetTableValue(prefix + "Database", dt);

            // Load Server
            server_TXT.Text = Table_Functions.GetTableValue(prefix + "Server", dt);

            // Load Port
            port_TXT.Text = Table_Functions.GetTableValue(prefix + "Port", dt);

            // Load Username
            username_TXT.Text = Table_Functions.GetTableValue(prefix + "Username", dt);

            // Load UsePHP
            bool usePHP = false;
            string strUsePHP = Table_Functions.GetTableValue(prefix + "UsePHP", dt);
            if (strUsePHP != null)
            {
                bool.TryParse(strUsePHP, out usePHP);
                usephp_CHK.IsChecked = usePHP;
                //UsePHPServer = usePHP;
            }

            // Load PHP Server
            phpserver_TXT.Text = Table_Functions.GetTableValue(prefix + "PHP_Server", dt);

            // Load PHP Directory
            phpdirectory_TXT.Text = Table_Functions.GetTableValue(prefix + "PHP_Directory", dt);

            Loading = false;
        }

        public void SaveConfiguration(DataTable dt)
        {
            // Save Database Name
            Table_Functions.UpdateTableValue(databasename_TXT.Text, prefix + "Database", dt);

            // Save Server
            Table_Functions.UpdateTableValue(server_TXT.Text, prefix + "Server", dt);

            // Save Port
            Table_Functions.UpdateTableValue(port_TXT.Text, prefix + "Port", dt);

            // Save Username
            Table_Functions.UpdateTableValue(username_TXT.Text, prefix + "Username", dt);

            // Save Password
            if (PasswordVerified)
            {
                Table_Functions.UpdateTableValue(password_TXT.Password, prefix + "Password", dt);
            }

            // Save UsePHP
            Table_Functions.UpdateTableValue(UsePHPServer.ToString(), prefix + "UsePHP", dt);

            // Save PHP Server
            Table_Functions.UpdateTableValue(phpserver_TXT.Text, prefix + "PHP_Server", dt);

            // Save PHP Directory
            Table_Functions.UpdateTableValue(phpdirectory_TXT.Text, prefix + "PHP_Directory", dt);
            
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

        
        #region "Password"

        private void password_TXT_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!Loading)
            {
                if (password_TXT.Password != "") PasswordEntered = true;
                else PasswordEntered = false;

                PasswordShort = false;
                PasswordLong = false;

                VerifyPassword();
                ConfirmPassword(); 
            }
        }

        private void confirmpassword_TXT_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!Loading)
            {
                if (confirmpassword_TXT.Password != "") ConfirmPasswordEntered = true;
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

            if (!TH_Configuration.User.Management.VerifyPasswordMinimum(pwd)) PasswordShort = true;
            else if (!TH_Configuration.User.Management.VerifyPasswordMaximum(pwd)) PasswordLong = true;
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
                if (password_TXT.Password == confirmpassword_TXT.Password) PasswordVerified = true;
                else PasswordVerified = false;

                if (PasswordVerified) ChangeSetting(prefix + "Password", password_TXT.Password);
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

    }
}
