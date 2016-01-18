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
using System.Threading;
using System.IO;

using TH_Configuration;
using TH_Global;
using TH_Global.Functions;
using TH_UserManagement;
using TH_WPF;

using TH_UserManagement.Management;


namespace TH_UserManagement
{
    /// <summary>
    /// Interaction logic for Page.xaml
    /// </summary>
    public partial class MyAccountPage : UserControl, TH_Global.Page
    {
        public MyAccountPage()
        {
            InitializeComponent();
            DataContext = this;

            // Fill Country List
            foreach (string country in Countries.Names) CountryList.Add(country);

            // Fill State List
            foreach (string state in States.Abbreviations) StateList.Add(state);

            PageName = "Create Account";
            Image = new BitmapImage(new Uri("pack://application:,,,/TH_UserManagement;component/Resources/AddUser_01.png"));
        }

        public Database_Settings UserDatabaseSettings = null;

        public UserConfiguration CurrentUser
        {
            get { return (UserConfiguration)GetValue(CurrentUserProperty); }
            set 
            {               
                SetValue(CurrentUserProperty, value);
            }
        }

        public static readonly DependencyProperty CurrentUserProperty =
            DependencyProperty.Register("CurrentUser", typeof(UserConfiguration), typeof(MyAccountPage), new PropertyMetadata(null));


        public delegate void UserChanged_Handler(UserConfiguration userConfig);
        public event UserChanged_Handler UserChanged;


        public string PageName { get; set; }

        public ImageSource Image { get; set; }

        BitmapImage NoProfileImage = new BitmapImage(new Uri("pack://application:,,,/TH_UserManagement;component/Resources/blank_profile_01.png"));

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadUserConfiguration(CurrentUser, UserDatabaseSettings);
        }


        public void LoadUserConfiguration(UserConfiguration userConfig, Database_Settings userDatabaseSettings)
        {
            CurrentUser = userConfig;
            UserDatabaseSettings = userDatabaseSettings;
                 
            if (this.IsLoaded)
            {
                if (userConfig != null)
                {
                    FirstName = String_Functions.UppercaseFirst(userConfig.first_name);
                    LastName = String_Functions.UppercaseFirst(userConfig.last_name);

                    Username = String_Functions.UppercaseFirst(userConfig.username);
                    Email = userConfig.email;

                    Company = String_Functions.UppercaseFirst(userConfig.company);
                    Phone = userConfig.phone;
                    Address1 = userConfig.address1;
                    Address2 = userConfig.address2;
                    City = String_Functions.UppercaseFirst(userConfig.city);

                    int country = CountryList.ToList().FindIndex(x => x == String_Functions.UppercaseFirst(userConfig.country));
                    if (country >= 0) country_COMBO.SelectedIndex = country;

                    int state = CountryList.ToList().FindIndex(x => x == String_Functions.UppercaseFirst(userConfig.state));
                    if (state >= 0) state_COMBO.SelectedIndex = state;

                    ZipCode = userConfig.zipcode;

                    LoadProfileImage(userConfig);
                }
                else
                {
                    CleanForm();
                }
            }
        }


        void SetPageType(UserConfiguration userConfig)
        {
            if (userConfig != null)
            {
                PageName = "Edit Account";
                Image = new BitmapImage(new Uri("pack://application:,,,/TH_UserManagement;component/Resources/blank_profile_01_sm.png"));
            }
            else
            {
                PageName = "Create Account";
                Image = new BitmapImage(new Uri("pack://application:,,,/TH_UserManagement;component/Resources/AddUser_01.png"));
            }
        }

        public void CleanForm()
        {
            UsernameVerified = false;
            PasswordEntered = false;
            ConfirmPasswordEntered = false;

            password_TXT.PasswordText = null;
            confirmpassword_TXT.PasswordText = null;

            FirstName = null;
            LastName = null;
            Username = null;
            Email = null;
            Phone = null;
            Address1 = null;
            Address2 = null;
            City = null;
            ZipCode = null;

            int US = CountryList.ToList().FindIndex(x => x == "United States");
            if (US >= 0) country_COMBO.SelectedIndex = US;

            state_COMBO.SelectedIndex = -1;

            ClearProfileImage();
        }

        #region "Form Properties"

        public string FirstName
        {
            get { return (string)GetValue(FirstNameProperty); }
            set { SetValue(FirstNameProperty, value); }
        }

        public static readonly DependencyProperty FirstNameProperty =
            DependencyProperty.Register("FirstName", typeof(string), typeof(MyAccountPage), new PropertyMetadata(null));


        public string LastName
        {
            get { return (string)GetValue(LastNameProperty); }
            set { SetValue(LastNameProperty, value); }
        }

        public static readonly DependencyProperty LastNameProperty =
            DependencyProperty.Register("LastName", typeof(string), typeof(MyAccountPage), new PropertyMetadata(null));


        public string Username
        {
            get { return (string)GetValue(UsernameProperty); }
            set { SetValue(UsernameProperty, value); }
        }

        public static readonly DependencyProperty UsernameProperty =
            DependencyProperty.Register("Username", typeof(string), typeof(MyAccountPage), new PropertyMetadata(null));


        public string Company
        {
            get { return (string)GetValue(CompanyProperty); }
            set { SetValue(CompanyProperty, value); }
        }

        public static readonly DependencyProperty CompanyProperty =
            DependencyProperty.Register("Company", typeof(string), typeof(MyAccountPage), new PropertyMetadata(null));


        public string Email
        {
            get { return (string)GetValue(EmailProperty); }
            set { SetValue(EmailProperty, value); }
        }

        public static readonly DependencyProperty EmailProperty =
            DependencyProperty.Register("Email", typeof(string), typeof(MyAccountPage), new PropertyMetadata(null));


        public string Phone
        {
            get { return (string)GetValue(PhoneProperty); }
            set { SetValue(PhoneProperty, value); }
        }

        public static readonly DependencyProperty PhoneProperty =
            DependencyProperty.Register("Phone", typeof(string), typeof(MyAccountPage), new PropertyMetadata(null));


        public string Address1
        {
            get { return (string)GetValue(Address1Property); }
            set { SetValue(Address1Property, value); }
        }

        public static readonly DependencyProperty Address1Property =
            DependencyProperty.Register("Address1", typeof(string), typeof(MyAccountPage), new PropertyMetadata(null));


        public string Address2
        {
            get { return (string)GetValue(Address2Property); }
            set { SetValue(Address2Property, value); }
        }

        public static readonly DependencyProperty Address2Property =
            DependencyProperty.Register("Address2", typeof(string), typeof(MyAccountPage), new PropertyMetadata(null));


        public string City
        {
            get { return (string)GetValue(CityProperty); }
            set { SetValue(CityProperty, value); }
        }

        public static readonly DependencyProperty CityProperty =
            DependencyProperty.Register("City", typeof(string), typeof(MyAccountPage), new PropertyMetadata(null));


        public string ZipCode
        {
            get { return (string)GetValue(ZipCodeProperty); }
            set { SetValue(ZipCodeProperty, value); }
        }

        public static readonly DependencyProperty ZipCodeProperty =
            DependencyProperty.Register("ZipCode", typeof(string), typeof(MyAccountPage), new PropertyMetadata(null));


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
            DependencyProperty.Register("ShowStates", typeof(bool), typeof(MyAccountPage), new PropertyMetadata(false));


        private void country_COMBO_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender.GetType() == typeof(ComboBox))
            {
                ComboBox cb = (ComboBox)sender;

                if (cb.SelectedItem != null)
                {
                    if (cb.SelectedItem.ToString() == "United States") ShowStates = true;
                    else ShowStates = false;
                }
            }
        }

        #region "Password"

        public bool PasswordEntered
        {
            get { return (bool)GetValue(PasswordEnteredProperty); }
            set { SetValue(PasswordEnteredProperty, value); }
        }

        public static readonly DependencyProperty PasswordEnteredProperty =
            DependencyProperty.Register("PasswordEntered", typeof(bool), typeof(MyAccountPage), new PropertyMetadata(false));


        private void password_TXT_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (password_TXT.PasswordText != "") PasswordEntered = true;
            else PasswordEntered = false;

            PasswordShort = false;
            PasswordLong = false;

            VerifyPassword();
            ConfirmPassword();
        }

        #endregion

        #region "Confirm Password"

        public bool ConfirmPasswordEntered
        {
            get { return (bool)GetValue(ConfirmPasswordEnteredProperty); }
            set { SetValue(ConfirmPasswordEnteredProperty, value); }
        }

        public static readonly DependencyProperty ConfirmPasswordEnteredProperty =
            DependencyProperty.Register("ConfirmPasswordEntered", typeof(bool), typeof(MyAccountPage), new PropertyMetadata(false));

        private void confirmpassword_TXT_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (confirmpassword_TXT.PasswordText != "") ConfirmPasswordEntered = true;
            else ConfirmPasswordEntered = false;

            ConfirmPassword();
        }

        #endregion

        #endregion

        #region "Apply / Create"

        public bool Saving
        {
            get { return (bool)GetValue(SavingProperty); }
            set { SetValue(SavingProperty, value); }
        }

        public static readonly DependencyProperty SavingProperty =
            DependencyProperty.Register("Saving", typeof(bool), typeof(MyAccountPage), new PropertyMetadata(false));
      

        private void Apply_Clicked(TH_WPF.Button bt)
        {
            UpdateUser(CreateUserConfiguration());
        }

        UserConfiguration CreateUserConfiguration()
        {
            // Create new UserConfiguration object with new data
            UserConfiguration userConfig = new UserConfiguration();

            userConfig.first_name = FirstName;
            userConfig.last_name = LastName;

            userConfig.username = Username;

            userConfig.company = Company;

            userConfig.email = Email;
            userConfig.phone = Phone;

            userConfig.address1 = Address1;
            userConfig.address2 = Address2;
            userConfig.city = City;

            if (country_COMBO.SelectedItem != null) userConfig.country = country_COMBO.SelectedItem.ToString();
            if (state_COMBO.SelectedItem != null) userConfig.state = state_COMBO.SelectedItem.ToString();

            userConfig.zipcode = ZipCode;

            return userConfig;
        }


        class UpdateUser_Info
        {
            public UserConfiguration userConfig { get; set; }
            public Database_Settings userDatabaseSettings { get; set; }
            public string password { get; set; }
        }

        class UpdateUser_Return
        {
            public bool success { get; set; }
            public UserConfiguration userConfig { get; set; }
        }

        Thread updateuser_THREAD;

        void UpdateUser(UserConfiguration userConfig)
        {
            UpdateUser_Info info = new UpdateUser_Info();
            info.userConfig = userConfig;
            info.userDatabaseSettings = UserDatabaseSettings;
            info.password = password_TXT.PasswordText;

            Saving = true;

            if (updateuser_THREAD != null) updateuser_THREAD.Abort();

            updateuser_THREAD = new Thread(new ParameterizedThreadStart(UpdateUser_Worker));
            updateuser_THREAD.Start(info);
        }

        void UpdateUser_Worker(object o)
        {
            UpdateUser_Return result = new UpdateUser_Return();

            if (o != null)
            {
                UpdateUser_Info info = (UpdateUser_Info)o;

                if (info.userConfig != null)
                {
                    bool success = Users.CreateUser(info.userConfig, info.password, info.userDatabaseSettings);

                    // Upload Profile Image
                    if (success && profileImageChanged)
                    {
                        if (profileImage != null)
                        {
                            success = UploadProfileImage(profileImage, info.userDatabaseSettings);
                        }
                        if (success) success = Users.UpdateImageURL(profileImageFilename, info.userConfig, info.userDatabaseSettings);
                    }
                             
                    result.success = success;
                    result.userConfig = info.userConfig;
                }
            }

            this.Dispatcher.BeginInvoke(new Action<UpdateUser_Return>(UpdateUser_GUI), priority, new object[] { result });
        }

        void UpdateUser_GUI(UpdateUser_Return result)
        {
            if (result.success)
            {
                if (UserChanged != null) UserChanged(result.userConfig);
            }
            else
            {
                TH_WPF.MessageBox.Show("Error during User Creation! Try Again.");
            }

            Saving = false;
        }

        #endregion


        #region "Username Verification"

        public bool UsernameVerified
        {
            get { return (bool)GetValue(UsernameVerifiedProperty); }
            set { SetValue(UsernameVerifiedProperty, value); }
        }

        public static readonly DependencyProperty UsernameVerifiedProperty =
            DependencyProperty.Register("UsernameVerified", typeof(bool), typeof(MyAccountPage), new PropertyMetadata(false));

        public string UsernameMessage
        {
            get { return (string)GetValue(UsernameMessageProperty); }
            set { SetValue(UsernameMessageProperty, value); }
        }

        public static readonly DependencyProperty UsernameMessageProperty =
            DependencyProperty.Register("UsernameMessage", typeof(string), typeof(MyAccountPage), new PropertyMetadata(null));
    

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
            // If no userconfiguration database configuration found then use default TrakHound User Database
            if (UserDatabaseSettings == null)
            {
                VerifyUsernameReturn usernameReturn = TH_UserManagement.Management.Remote.Users.VerifyUsername(Username);
                if (usernameReturn != null)
                {
                    UsernameVerified = usernameReturn.available;
                    UsernameMessage = usernameReturn.message;
                }
                else
                {
                    UsernameVerified = false;
                    UsernameMessage = null;
                }
            }
        }

        #endregion

        #region "Password"

        public bool ShowChangePassword
        {
            get { return (bool)GetValue(ShowChangePasswordProperty); }
            set { SetValue(ShowChangePasswordProperty, value); }
        }

        public static readonly DependencyProperty ShowChangePasswordProperty =
            DependencyProperty.Register("ShowChangePassword", typeof(bool), typeof(MyAccountPage), new PropertyMetadata(false));

        private void ChangePassword_Clicked(TH_WPF.Button bt)
        {
            ShowChangePassword = true;
        }

        public bool PasswordVerified
        {
            get { return (bool)GetValue(PasswordVerifiedProperty); }
            set { SetValue(PasswordVerifiedProperty, value); }
        }

        public static readonly DependencyProperty PasswordVerifiedProperty =
            DependencyProperty.Register("PasswordVerified", typeof(bool), typeof(MyAccountPage), new PropertyMetadata(false));


        public bool PasswordShort
        {
            get { return (bool)GetValue(PasswordShortProperty); }
            set { SetValue(PasswordShortProperty, value); }
        }

        public static readonly DependencyProperty PasswordShortProperty =
            DependencyProperty.Register("PasswordShort", typeof(bool), typeof(MyAccountPage), new PropertyMetadata(false));


        public bool PasswordLong
        {
            get { return (bool)GetValue(PasswordLongProperty); }
            set { SetValue(PasswordLongProperty, value); }
        }

        public static readonly DependencyProperty PasswordLongProperty =
            DependencyProperty.Register("PasswordLong", typeof(bool), typeof(MyAccountPage), new PropertyMetadata(false));

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
            }
        }

        #endregion


        #region "Profile Image"

        public ImageSource ProfileImage
        {
            get { return (ImageSource)GetValue(ProfileImageProperty); }
            set { SetValue(ProfileImageProperty, value); }
        }

        public static readonly DependencyProperty ProfileImageProperty =
            DependencyProperty.Register("ProfileImage", typeof(ImageSource), typeof(MyAccountPage), new PropertyMetadata(null));


        public bool ProfileImageSet
        {
            get { return (bool)GetValue(ProfileImageSetProperty); }
            set { SetValue(ProfileImageSetProperty, value); }
        }

        public static readonly DependencyProperty ProfileImageSetProperty =
            DependencyProperty.Register("ProfileImageSet", typeof(bool), typeof(MyAccountPage), new PropertyMetadata(false));


        public bool ProfileImageLoading
        {
            get { return (bool)GetValue(ProfileImageLoadingProperty); }
            set { SetValue(ProfileImageLoadingProperty, value); }
        }

        public static readonly DependencyProperty ProfileImageLoadingProperty =
            DependencyProperty.Register("ProfileImageLoading", typeof(bool), typeof(MyAccountPage), new PropertyMetadata(false));

        const System.Windows.Threading.DispatcherPriority priority = System.Windows.Threading.DispatcherPriority.Background;

        Thread profileimage_THREAD;

        void LoadProfileImage(UserConfiguration userConfig)
        {
            ProfileImageLoading = true;
            ProfileImageSet = false;
            ProfileImage = null;


            if (profileimage_THREAD != null) profileimage_THREAD.Abort();

            profileimage_THREAD = new Thread(new ParameterizedThreadStart(LoadProfileImage_Worker));
            profileimage_THREAD.Start(userConfig);
        }

        void LoadProfileImage_Worker(object o)
        {
            if (o != null)
            {
                UserConfiguration userConfig = (UserConfiguration)o;

                if (userConfig != null)
                {
                    System.Drawing.Image img = ProfileImages.GetProfileImage(userConfig, UserDatabaseSettings);

                    this.Dispatcher.BeginInvoke(new Action<System.Drawing.Image>(LoadProfileImage_GUI), priority, new object[] { img });
                }

                this.Dispatcher.BeginInvoke(new Action(LoadProfileImage_Finished), priority, new object[] { });
            }
        }

        void LoadProfileImage_GUI(System.Drawing.Image img)
        {
            if (img != null)
            {
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(img);

                IntPtr bmpPt = bmp.GetHbitmap();
                BitmapSource bmpSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmpPt, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                bmpSource.Freeze();

                ProfileImage = TH_WPF.Image_Functions.SetImageSize(bmpSource, 200, 200);

                ProfileImageSet = true;
            }
        }

        void LoadProfileImage_Finished()
        {
            ProfileImageLoading = false;
        }

        string profileImageFilename;

        System.Drawing.Image profileImage;

        bool profileImageChanged = false;

        void SetProfileImage()
        {
            // Show OpenFileDialog for selecting new Profile Image
            string imagePath = ProfileImages.OpenImageBrowse();
            if (imagePath != null)
            {
                // Crop and Resize image
                System.Drawing.Image img = ProfileImages.ProcessImage(imagePath);
                if (img != null)
                {
                    profileImageFilename = imagePath;
                    profileImageChanged = true;

                    img = TH_Global.Functions.Image_Functions.CropImageToCenter(img);

                    profileImage = img;

                    ProfileImage = TH_Global.Functions.Image_Functions.SourceFromImage(img);

                    if (ProfileImage != null) ProfileImageSet = true;
                    else ProfileImageSet = false;
                }
            }
        }

        bool UploadProfileImage(System.Drawing.Image profileImg, Database_Settings userDatabaseSettings)
        {
            bool result = false;

            if (profileImage != null)
            {
                // Crop and Resize image
                System.Drawing.Image img = ProfileImages.ProcessImage(profileImg);
                if (img != null)
                {
                    string newFilename = String_Functions.RandomString(20);

                    profileImageFilename = newFilename;

                    string tempdir = FileLocations.TrakHound + @"\temp";
                    if (!Directory.Exists(tempdir)) Directory.CreateDirectory(tempdir);

                    string localPath = tempdir + @"\" + newFilename;

                    img.Save(localPath);

                    result = ProfileImages.UploadProfileImage(newFilename, localPath, userDatabaseSettings);
                }
            }

            return result;
        }

        void ClearProfileImage()
        {
            ProfileImage = null;
            ProfileImageSet = false;
            ProfileImageLoading = false;

            profileImage = null;
            profileImageFilename = null;
        }

        private void ProfileImage_UploadClicked(ImageBox sender)
        {
            SetProfileImage();
        }

        private void ProfileImage_ClearClicked(ImageBox sender)
        {
            profileImageChanged = true;

            ClearProfileImage();
        }

        #endregion

        private void PrivacyPolicy_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(e.Uri.ToString());
            }
            catch (Exception ex) { TH_WPF.MessageBox.Show("Error Opening Privacy Policy. Please try again or open a browser and navigate to 'http://www.feenux.com/trakhound/docs/privacypolicy_desktopapp.html'"); }
        }

    }
}
