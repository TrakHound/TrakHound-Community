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
using TH_Database.Tables;
using TH_Database;
using TH_FTP;
using TH_Global;

namespace TrakHound_Client.Login
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
        }

        public TrakHound_Client.MainWindow mw;


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
            DependencyProperty.Register("ProfileImage", typeof(ImageSource), typeof(DropDown), new PropertyMetadata(new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/blank_profile_01.png"))));










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

        private static System.Drawing.Image CropImage (System.Drawing.Image img, System.Drawing.Rectangle cropArea)
        {
            System.Drawing.Bitmap bmpImage = new System.Drawing.Bitmap(img);
            return bmpImage.Clone(cropArea, bmpImage.PixelFormat);
        }

        static System.Drawing.Image CropImageToCenter(System.Drawing.Image img)
        {
            int width = img.Width;
            int height = img.Height;

            if (width > height)
            {
                int sqWidth = height;
                int widthOffset = (width - sqWidth) / 2;

                System.Drawing.Rectangle widthCropRect = new System.Drawing.Rectangle(new System.Drawing.Point(widthOffset, 0), new System.Drawing.Size(height, height));

                return CropImage(img, widthCropRect);
            }
            else if (height > width)
            {
                int sqHeight = width;
                int heightOffset = (height - sqHeight) / 2;

                System.Drawing.Rectangle heightCropRect = new System.Drawing.Rectangle(new System.Drawing.Point(0, heightOffset), new System.Drawing.Size(width, width));

                return CropImage(img, heightCropRect);
            }
            else return img;
        }

        private void ProfileImage_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

            if (LoggedIn && mw.userDatabaseSettings != null && currentUser != null)
            {
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

                dlg.InitialDirectory = FileLocations.TrakHound;
                dlg.Multiselect = false;
                dlg.Title = "Browse for Profile Image";
                dlg.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";

                dlg.ShowDialog();

                try
                {
                    string imagePath = dlg.FileName;

                    string username = "usermanager@feenux.com";
                    string password = "6=TB0P?@Do#Z";
                    string remoteFileName = "starwars.jpg";
                    string remotePath = "ftp://ftp.feenux.com/" + remoteFileName;
                    //string localPath = imagePath;

                    System.Drawing.Image img = CropImageToCenter(System.Drawing.Image.FromFile(imagePath));

                    img = TH_WPF.Image_Functions.SetImageSize(img, 120, 120);

                    string newPath = FileLocations.TrakHound + @"\temp";
                    if (!Directory.Exists(newPath)) Directory.CreateDirectory(newPath);

                    newPath += @"\" + remoteFileName;

                    img.Save(newPath);

                    string localPath = newPath;


                    if (FTP.Upload(username, password, remotePath, localPath))
                    {
                        Users.UpdateImageURL(remoteFileName, currentUser, mw.userDatabaseSettings);

                        LoadProfileImage(currentUser);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Browse_Button_Click() : " + ex.Message);
                }
            }
        }

        static Stream TestConvertImage(Stream originalStream, System.Drawing.Imaging.ImageFormat format)
        {
            var image = System.Drawing.Image.FromStream(originalStream);

            var stream = new MemoryStream();
            image.Save(stream, format);
            stream.Position = 0;
            return stream;
        }

        void LoadProfileImage(UserConfiguration userConfig)
        {
            if (userConfig.image_url != String.Empty)
            {
                using (WebClient webClient = new WebClient())
                {
                    byte[] data = webClient.DownloadData("http://www.feenux.com/trakhound/users/files/" + userConfig.image_url);

                    using (MemoryStream mem = new MemoryStream(data))
                    {
                        using (var imageStream = TestConvertImage(mem, System.Drawing.Imaging.ImageFormat.Jpeg))
                        {
                            using (var yourImage = System.Drawing.Image.FromStream(imageStream))
                            {
                                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(yourImage);
                                IntPtr bmpPt = bmp.GetHbitmap();
                                BitmapSource bmpSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmpPt, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                                bmpSource.Freeze();

                                ProfileImage = TH_WPF.Image_Functions.SetImageSize(bmpSource, 120, 120);
                            }
                        } 
                    }
                }
            }






            //if (userConfig.image_url != String.Empty)
            //{
            //    string username = "usermanager@feenux.com";
            //    string password = "TX_A!b}!ivMB";


            //    //string remoteFileName = userConfig.image_url;

            //    string remoteFileName = "starwars.jpg";


            //    string remotePath = "ftp://ftp.feenux.com/" + remoteFileName;

            //    System.Drawing.Image img = FTP.DownloadImageTest(username, password, remotePath);
            //    if (img != null)
            //    {
            //        Console.WriteLine("Image isn't null!");



            //    }

            //    //Stream stream = FTP.Download(username, password, remotePath);
            //    //if (stream != null)
            //    //{
            //    //    using (stream)
            //    //    {
            //    //        var imageSource = new BitmapImage();
            //    //        imageSource.BeginInit();
            //    //        imageSource.StreamSource = stream;
            //    //        imageSource.EndInit();

            //    //        ProfileImage = imageSource;
            //    //    }
            //    //}
            //}
        }

        public bool Login()
        {
            bool result = false;

            LoginError = false;

            if (mw.userDatabaseSettings != null)
            {
                Loading = true;

                UserConfiguration userConfig = Users.Login(username_TXT.Text, password_TXT.Password, mw.userDatabaseSettings);
                if (userConfig != null)
                {
                    Username = TH_Global.Formatting.UppercaseFirst(userConfig.username);
                    EmailAddress = userConfig.email;

                    username_TXT.Clear();
                    password_TXT.Clear();

                    LoadProfileImage(userConfig);
                    LoggedIn = true;
                    result = true;
                }
                else
                {
                    LoginError = true;
                    LoggedIn = false;
                    Username = null;
                }

                currentUser = userConfig;
                mw.CurrentUser = currentUser;

                Loading = false;
            }

            return result;
        }

        void SignOut()
        {
            LoggedIn = false;
            Username = null;
            currentUser = null;
            mw.CurrentUser = null;
        }

        void CreateAccount()
        {
            if (mw != null)
            {
                Account_Management.Pages.Create.Page page = new Account_Management.Pages.Create.Page();
                page.CleanForm();
                mw.AddPageAsTab(page, page.PageName, page.Image);

            }
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

    }
}
