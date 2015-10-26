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

using System.Collections.ObjectModel;

using TH_Configuration;
using TH_Database.Tables;
using TH_Global;
using TH_PlugIns_Client_Control;


namespace TrakHound_Client.Account_Management.Pages.Create
{
    /// <summary>
    /// Interaction logic for Page.xaml
    /// </summary>
    public partial class Page : UserControl, AboutPage
    {
        public Page()
        {
            InitializeComponent();
            DataContext = this;

            mw = Application.Current.MainWindow as MainWindow;

            // Fill Country List
            foreach (string country in Countries.Names) CountryList.Add(country);

            // Fill State List
            foreach (string state in States.Abbreviations) StateList.Add(state);
            
        }

        public TrakHound_Client.MainWindow mw;

        public void CleanForm()
        {
            firstname_TXT.Clear();
            lastname_TXT.Clear();

            username_TXT.Clear();

            password_TXT.Clear();
            confirmpassword_TXT.Clear();

            email_TXT.Clear();
            phone_TXT.Clear();

            address1_TXT.Clear();
            address2_TXT.Clear();

            city_TXT.Clear();

            int US = CountryList.ToList().FindIndex(x => x == "United States");
            if (US >= 0) country_COMBO.SelectedIndex = US;

            state_COMBO.SelectedIndex = -1;

            zipcode_TXT.Clear();
        }

        public string PageName { get { return "Create Account"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/AddUser_01.png")); } }



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

        


        private void password_TXT_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (password_TXT.Password != "") PasswordEntered = true;
            else PasswordEntered = false;

            PasswordShort = false;
            PasswordLong = false;

            VerifyPassword();
            ConfirmPassword();
        }

        private void confirmpassword_TXT_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (confirmpassword_TXT.Password != "") ConfirmPasswordEntered = true;
            else ConfirmPasswordEntered = false;

            ConfirmPassword();
        }



        ObservableCollection<string> countrylist;
        public ObservableCollection<string> CountryList
        {
            get
            {
                if (countrylist == null)
                    countrylist = new ObservableCollection<string>();
                return countrylist;
            }

            set
            {
                countrylist = value;
            }
        }

        ObservableCollection<string> statelist;
        public ObservableCollection<string> StateList
        {
            get
            {
                if (statelist == null)
                    statelist = new ObservableCollection<string>();
                return statelist;
            }

            set
            {
                statelist = value;
            }
        }

        public bool ShowStates
        {
            get { return (bool)GetValue(ShowStatesProperty); }
            set { SetValue(ShowStatesProperty, value); }
        }

        public static readonly DependencyProperty ShowStatesProperty =
            DependencyProperty.Register("ShowStates", typeof(bool), typeof(Page), new PropertyMetadata(false));

        


        private void country_COMBO_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender.GetType() == typeof(ComboBox))
            {
                ComboBox cb = (ComboBox)sender;

                if (cb.SelectedItem.ToString() == "United States") ShowStates = true;
                else ShowStates = false;

            }
        }

        private void state_COMBO_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void CreateAccount_Clicked(Controls.TH_Button bt)
        {
            CreateAccount();
        }

        void CreateAccount()
        {
            if (mw != null)
            {

                // Create new UserConfiguration object with new data
                UserConfiguration userConfig = new UserConfiguration();

                userConfig.first_name = firstname_TXT.Text;
                userConfig.last_name = lastname_TXT.Text;

                userConfig.username = username_TXT.Text;

                userConfig.company = company_TXT.Text;

                userConfig.email = email_TXT.Text;
                userConfig.phone = phone_TXT.Text;

                userConfig.address1 = address1_TXT.Text;
                userConfig.address2 = address2_TXT.Text;
                userConfig.city = city_TXT.Text;

                if (country_COMBO.SelectedItem != null) userConfig.country = country_COMBO.SelectedItem.ToString();
                if (state_COMBO.SelectedItem != null) userConfig.state = state_COMBO.SelectedItem.ToString();

                userConfig.zipcode = zipcode_TXT.Text;


                // If no userconfiguration database configuration found then use default TrakHound User Database
                if (mw.userDatabaseSettings == null)
                {
                    TH_Configuration.User.Management.CreateUser(userConfig, password_TXT.Password);
                }
                else
                {
                    Users.CreateUserTable(mw.userDatabaseSettings);
                    Users.CreateUser(userConfig, password_TXT.Password, mw.userDatabaseSettings);
                }

                ConfirmUserCreation();

            }     
        }

        System.Timers.Timer create_TIMER;

        void ConfirmUserCreation()
        {
            if (create_TIMER != null) create_TIMER.Enabled = false;

            create_TIMER = new System.Timers.Timer();
            create_TIMER.Interval = 2000;
            create_TIMER.Elapsed += create_TIMER_Elapsed;
            create_TIMER.Enabled = true;
        }

        void create_TIMER_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            create_TIMER.Enabled = false;

            this.Dispatcher.BeginInvoke(new Action(ConfirmUserCreation_GUI));
        }

        void ConfirmUserCreation_GUI()
        {

            mw.LoginMenu.username_TXT.Text = username_TXT.Text;
            mw.LoginMenu.password_TXT.Password = password_TXT.Password;

            if (mw.LoginMenu.Login())
            {
                CleanForm();
                mw.ClosePage("Create Account");
                mw.LoginMenu.Shown = true;
            }
            else
            {

            }


            //UserConfiguration successConfig = Users.Login(username_TXT.Text, password_TXT.Password, mw.userDatabaseSettings);
            //if (successConfig != null)
            //{
            //    mw.CurrentUser = successConfig;

            //    CleanForm();

            //    mw.ClosePage("Create Account");

            //    mw.LoginMenu.Shown = true;
            //}
            //else
            //{

            //}
        }



        #region "Username"

        public bool UsernameVerified
        {
            get { return (bool)GetValue(UsernameVerifiedProperty); }
            set { SetValue(UsernameVerifiedProperty, value); }
        }

        public static readonly DependencyProperty UsernameVerifiedProperty =
            DependencyProperty.Register("UsernameVerified", typeof(bool), typeof(Page), new PropertyMetadata(false));

        

        System.Timers.Timer username_TIMER;

        private void username_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (username_TIMER != null) username_TIMER.Enabled = false;

            username_TIMER = new System.Timers.Timer();
            username_TIMER.Interval = 500;
            username_TIMER.Elapsed += username_TIMER_Elapsed;
            username_TIMER.Enabled = true;
        }

        void username_TIMER_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            username_TIMER.Enabled = false;

            this.Dispatcher.BeginInvoke(new Action(VerifyUsername));
        }

        void VerifyUsername()
        {
            if (mw != null)
            {
                // If no userconfiguration database configuration found then use default TrakHound User Database
                if (mw.userDatabaseSettings == null)
                {
                    UsernameVerified = TH_Configuration.User.Management.VerifyUsername(username_TXT.Text);
                }
                else
                {
                    UsernameVerified = Users.VerifyUsername(username_TXT.Text, mw.userDatabaseSettings);
                }
            }
        }

        #endregion

        #region "Password"

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
            }
        }

        #endregion

    }
}
