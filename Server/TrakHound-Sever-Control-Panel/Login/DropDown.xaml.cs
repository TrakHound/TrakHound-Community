// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

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
using System.IO;
using System.Net;

using TH_Configuration;
using TH_Configuration.User;
using TH_Database.Tables;
using TH_Database;
//using TH_FTP;
using TH_Global;
using TH_Global.Functions;

namespace TrakHound_Server_Control_Panel.Login
{
    /// <summary>
    /// Interaction logic for DropDown.xaml
    /// </summary>
    public partial class DropDown : UserControl
    {
        public DropDown()
        {
            InitializeComponent();

            DataContext = this;

            mw = Application.Current.MainWindow as MainWindow;

            Root_GRID.Width = 0;
            Root_GRID.Height = 0;

            // Remember Me
            //UserConfiguration RememberUser = Management.GetRememberMe(Management.RememberMeType.Client);
            //if (RememberUser != null)
            //{
            //    Login(RememberUser);

            //    currentUser = RememberUser;
            //    //mw.CurrentUser = currentUser;
            //}
        }

        public TrakHound_Server_Control_Panel.MainWindow mw;


        public bool Shown
        {
            get { return (bool)GetValue(ShownProperty); }
            set
            {
                SetValue(ShownProperty, value);
                if (ShownChanged != null) ShownChanged(value);
            }
        }

        public static readonly DependencyProperty ShownProperty =
            DependencyProperty.Register("Shown", typeof(bool), typeof(DropDown), new PropertyMetadata(false));

        public void Hide()
        {
            if (!IsMouseOver) Shown = false;
        }

        public delegate void ShownChanged_Handler(bool val);
        public event ShownChanged_Handler ShownChanged;

        public delegate void Clicked_Handler();



        


        public bool LoggedIn
        {
            get { return (bool)GetValue(LoggedInProperty); }
            set { SetValue(LoggedInProperty, value); }
        }

        public static readonly DependencyProperty LoggedInProperty =
            DependencyProperty.Register("LoggedIn", typeof(bool), typeof(DropDown), new PropertyMetadata(false));




        public bool UsernameEntered
        {
            get { return (bool)GetValue(UsernameEnteredProperty); }
            set { SetValue(UsernameEnteredProperty, value); }
        }

        public static readonly DependencyProperty UsernameEnteredProperty =
            DependencyProperty.Register("UsernameEntered", typeof(bool), typeof(DropDown), new PropertyMetadata(false));

        

        public bool PasswordEntered
        {
            get { return (bool)GetValue(PasswordEnteredProperty); }
            set { SetValue(PasswordEnteredProperty, value); }
        }

        public static readonly DependencyProperty PasswordEnteredProperty =
            DependencyProperty.Register("PasswordEntered", typeof(bool), typeof(DropDown), new PropertyMetadata(false));
      

        private void password_TXT_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (password_TXT.Password != "") PasswordEntered = true;
            else PasswordEntered = false;
        }


        public bool Loading
        {
            get { return (bool)GetValue(LoadingProperty); }
            set { SetValue(LoadingProperty, value); }
        }

        public static readonly DependencyProperty LoadingProperty =
            DependencyProperty.Register("Loading", typeof(bool), typeof(DropDown), new PropertyMetadata(false));




        public bool LoginError
        {
            get { return (bool)GetValue(LoginErrorProperty); }
            set { SetValue(LoginErrorProperty, value); }
        }

        public static readonly DependencyProperty LoginErrorProperty =
            DependencyProperty.Register("LoginError", typeof(bool), typeof(DropDown), new PropertyMetadata(false));




        public string Fullname
        {
            get { return (string)GetValue(FullnameProperty); }
            set { SetValue(FullnameProperty, value); }
        }

        public static readonly DependencyProperty FullnameProperty =
            DependencyProperty.Register("Fullname", typeof(string), typeof(DropDown), new PropertyMetadata(null));


        public string Firstname
        {
            get { return (string)GetValue(FirstnameProperty); }
            set { SetValue(FirstnameProperty, value); }
        }

        public static readonly DependencyProperty FirstnameProperty =
            DependencyProperty.Register("Firstname", typeof(string), typeof(DropDown), new PropertyMetadata(null));

        public string Lastname
        {
            get { return (string)GetValue(LastnameProperty); }
            set { SetValue(LastnameProperty, value); }
        }

        public static readonly DependencyProperty LastnameProperty =
            DependencyProperty.Register("Lastname", typeof(string), typeof(DropDown), new PropertyMetadata(null));

        

        public string Username
        {
            get { return (string)GetValue(UsernameProperty); }
            set { SetValue(UsernameProperty, value); }
        }

        public static readonly DependencyProperty UsernameProperty =
            DependencyProperty.Register("Username", typeof(string), typeof(DropDown), new PropertyMetadata(null));


        public string EmailAddress
        {
            get { return (string)GetValue(EmailAddressProperty); }
            set { SetValue(EmailAddressProperty, value); }
        }

        public static readonly DependencyProperty EmailAddressProperty =
            DependencyProperty.Register("EmailAddress", typeof(string), typeof(DropDown), new PropertyMetadata(null));

        


        public ImageSource ProfileImage
        {
            get { return (ImageSource)GetValue(ProfileImageProperty); }
            set { SetValue(ProfileImageProperty, value); }
        }

        public static readonly DependencyProperty ProfileImageProperty =
            DependencyProperty.Register("ProfileImage", typeof(ImageSource), typeof(DropDown), new PropertyMetadata(new BitmapImage(new Uri("pack://application:,,,/TrakHound-Server-Control-Panel;component/Resources/blank_profile_01.png"))));



        UserConfiguration currentUser;

        private void Login_Clicked(Controls.TH_Button bt)
        {
            Login();
        }

        private void SignOut_Clicked(Controls.TH_Button bt)
        {
            SignOut();
        }

        private void Create_Clicked(Controls.TH_Button bt)
        {
            Shown = false;
            CreateAccount();
        }

        private void MyAccount_Clicked(Controls.TH_Button bt)
        {
            Shown = false;
            //if (mw != null) mw.MyAccount_Open();
        }

        private void ProfileImage_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ChangeProfileImage();
        }

        void LoadProfileImage(UserConfiguration userConfig)
        {
            if (userConfig != null)
            {
                System.Drawing.Image img = ProfileImages.GetProfileImage(userConfig);

                if (img != null)
                {
                    System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(img);

                    IntPtr bmpPt = bmp.GetHbitmap();
                    BitmapSource bmpSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmpPt, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                    bmpSource.Freeze();

                    ProfileImage = TH_WPF.Image_Functions.SetImageSize(bmpSource, 120, 120);
                    //mw.ProfileImage = TH_WPF.Image_Functions.SetImageSize(bmpSource, 200, 200);
                }
            }
        }

        void ChangeProfileImage()
        {
            if (LoggedIn && currentUser != null)
            {

                // Show OpenFileDialog for selecting new Profile Image
                string imagePath = ProfileImages.OpenImageBrowse();
                if (imagePath != null)
                {
                    // Crop and Resize image
                    System.Drawing.Image img = ProfileImages.ProcessImage(imagePath);
                    if (img != null)
                    {
                        string filename = String_Functions.RandomString(20);

                        string tempdir = FileLocations.TrakHound + @"\temp";
                        if (!Directory.Exists(tempdir)) Directory.CreateDirectory(tempdir);

                        string localPath = tempdir + @"\" + filename;

                        img.Save(localPath);

                        if (ProfileImages.UploadProfileImage(filename, localPath))
                        {
                            Management.UpdateImageURL(filename, currentUser);

                            LoadProfileImage(currentUser);
                        }
                    }
                }
            }
        }

        public bool Login()
        {
            bool result = false;

            LoginError = false;
            Loading = true;
            ProfileImage = new BitmapImage(new Uri("pack://application:,,,/TrakHound-Server-Control-Panel;component/Resources/blank_profile_01.png"));


            UserConfiguration userConfig = null;

            // If no userconfiguration database configuration found then use default TrakHound User Database
            if (mw.userDatabaseSettings == null)
            {
                userConfig = TH_Configuration.User.Management.Login(username_TXT.Text, password_TXT.Password);
            }
            else
            {
                userConfig = Users.Login(username_TXT.Text, password_TXT.Password, mw.userDatabaseSettings);
            }

            // If login was successful
            if (userConfig != null)
            {
                Login(userConfig);

                result = true;

                //if (RememberMe) Management.SetRememberMe(userConfig, Management.RememberMeType.Client);

                //Fullname = TH_Global.Formatting.UppercaseFirst(userConfig.first_name) + " " + TH_Global.Formatting.UppercaseFirst(userConfig.last_name);
                //Firstname = TH_Global.Formatting.UppercaseFirst(userConfig.first_name);
                //Lastname = TH_Global.Formatting.UppercaseFirst(userConfig.last_name);

                //Username = TH_Global.Formatting.UppercaseFirst(userConfig.username);
                //EmailAddress = userConfig.email;

                //username_TXT.Clear();
                //password_TXT.Clear();

                //LoadProfileImage(userConfig);
                //LoggedIn = true;
                //result = true;
            }
            else
            {
                LoginError = true;
                LoggedIn = false;

                Fullname = null;
                Firstname = null;
                Lastname = null;

                Username = null;
            }


            currentUser = userConfig;
            mw.CurrentUser = currentUser;

            Loading = false;

            return result;
        }

        void Login(UserConfiguration userConfig)
        {
            if (RememberMe) Management.SetRememberMe(userConfig, Management.RememberMeType.Client);

            Fullname = TH_Global.Formatting.UppercaseFirst(userConfig.first_name) + " " + TH_Global.Formatting.UppercaseFirst(userConfig.last_name);
            Firstname = TH_Global.Formatting.UppercaseFirst(userConfig.first_name);
            Lastname = TH_Global.Formatting.UppercaseFirst(userConfig.last_name);

            Username = TH_Global.Formatting.UppercaseFirst(userConfig.username);
            EmailAddress = userConfig.email;

            username_TXT.Clear();
            password_TXT.Clear();

            LoadProfileImage(userConfig);
            LoggedIn = true;
        }

        void SignOut()
        {
            LoggedIn = false;

            Fullname = null;
            Firstname = null;
            Lastname = null;

            Username = null;
            currentUser = null;
            ProfileImage = new BitmapImage(new Uri("pack://application:,,,/TrakHound-Server-Control-Panel;component/Resources/blank_profile_01.png"));
            //mw.CurrentUser = null;
        }

        void CreateAccount()
        {
            //if (mw != null)
            //{
            //    Account_Management.Pages.Create.Page page = new Account_Management.Pages.Create.Page();
            //    page.CleanForm();
            //    mw.AddPageAsTab(page, page.PageName, page.Image);

            //}
        }

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
                    Login();
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
            password_TXT.Password = null;
        }



        #region "Remember Me"

        public bool RememberMe
        {
            get { return (bool)GetValue(RememberMeProperty); }
            set { SetValue(RememberMeProperty, value); }
        }

        public static readonly DependencyProperty RememberMeProperty =
            DependencyProperty.Register("RememberMe", typeof(bool), typeof(DropDown), new PropertyMetadata(false));

        void LoadRememberMe()
        {




        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            RememberMe = true;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            RememberMe = false;
        }

        #endregion


    }
}
