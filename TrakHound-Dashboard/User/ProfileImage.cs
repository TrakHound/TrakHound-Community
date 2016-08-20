// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using TrakHound;
using TrakHound.API.Users;
using TrakHound.Tools;
using TrakHound.Tools.Web;
using TrakHound_UI;

namespace TrakHound_Dashboard
{
    public partial class MainWindow
    {

        public ImageSource ProfileImage
        {
            get { return (ImageSource)GetValue(ProfileImageProperty); }
            set { SetValue(ProfileImageProperty, value); }
        }

        public static readonly DependencyProperty ProfileImageProperty =
            DependencyProperty.Register("ProfileImage", typeof(ImageSource), typeof(MainWindow), new PropertyMetadata(null));


        public ImageSource ProfileImageSmall
        {
            get { return (ImageSource)GetValue(ProfileImageSmallProperty); }
            set { SetValue(ProfileImageSmallProperty, value); }
        }

        public static readonly DependencyProperty ProfileImageSmallProperty =
            DependencyProperty.Register("ProfileImageSmall", typeof(ImageSource), typeof(MainWindow), new PropertyMetadata(null));


        public bool ProfileImageSet
        {
            get { return (bool)GetValue(ProfileImageSetProperty); }
            set { SetValue(ProfileImageSetProperty, value); }
        }

        public static readonly DependencyProperty ProfileImageSetProperty =
            DependencyProperty.Register("ProfileImageSet", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        public bool ProfileImageLoading
        {
            get { return (bool)GetValue(ProfileImageLoadingProperty); }
            set { SetValue(ProfileImageLoadingProperty, value); }
        }

        public static readonly DependencyProperty ProfileImageLoadingProperty =
            DependencyProperty.Register("ProfileImageLoading", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));


        Thread profileimage_THREAD;

        void LoadProfileImage(UserConfiguration userConfig)
        {
            ProfileImageLoading = true;
            ProfileImageSet = false;
            ProfileImage = null;
            ProfileImageSmall = null;

            if (profileimage_THREAD != null) profileimage_THREAD.Abort();

            profileimage_THREAD = new Thread(new ParameterizedThreadStart(LoadProfileImage_Worker));
            profileimage_THREAD.Start(userConfig);
        }

        void LoadProfileImage_Worker(object o)
        {
            if (o != null)
            {
                UserConfiguration userConfig = (UserConfiguration)o;

                if (userConfig != null && !string.IsNullOrEmpty(userConfig.ImageUrl))
                {
                    var img = TrakHound.API.Files.DownloadImage(userConfig, userConfig.ImageUrl);
                    if (img != null)
                    {
                        System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(img);

                        IntPtr bmpPt = bmp.GetHbitmap();

                        BitmapSource bmpSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmpPt, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                        bmpSource = Image_Functions.SetImageSize(bmpSource, 75, 75);
                        bmpSource.Freeze();

                        BitmapSource bmpSource_sm = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmpPt, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                        bmpSource_sm = Image_Functions.SetImageSize(bmpSource_sm, 30, 30);
                        bmpSource_sm.Freeze();

                        Dispatcher.BeginInvoke(new Action<BitmapSource>(LoadProfileImage_GUI), UI_Functions.PRIORITY_BACKGROUND, new object[] { bmpSource });
                        Dispatcher.BeginInvoke(new Action<BitmapSource>(LoadProfileImageSmall_GUI), UI_Functions.PRIORITY_BACKGROUND, new object[] { bmpSource_sm });
                    }
                }

                Dispatcher.BeginInvoke(new Action(LoadProfileImage_Finished), UI_Functions.PRIORITY_BACKGROUND, new object[] { });
            }
        }

        void LoadProfileImage_GUI(BitmapSource src)
        {
            if (src != null)
            {
                ProfileImage = src;
                ProfileImageSet = true;
            }
        }

        void LoadProfileImageSmall_GUI(BitmapSource src)
        {
            if (src != null)
            {
                ProfileImageSmall = src;
            }
        }

        void LoadProfileImage_Finished()
        {
            ProfileImageLoading = false;
        }

        private void ProfileImage_UploadClicked(ImageBox sender)
        {
            UpdateProfileImage();
        }

        private class UpdateProfileImageInfo
        {
            public string ImagePath { get; set; }
            public string UserSessionToken { get; set; }
        }

        void UpdateProfileImage()
        {
            if (CurrentUser != null)
            {

                // Show OpenFileDialog for selecting new Profile Image
                string imagePath = OpenImageBrowse();
                if (imagePath != null)
                {
                    ProfileImageLoading = true;

                    var info = new UpdateProfileImageInfo();
                    info.ImagePath = imagePath;
                    info.UserSessionToken = CurrentUser.SessionToken;

                    ThreadPool.QueueUserWorkItem(new WaitCallback(UpdateProfileImage_Worker), info);
                }
            }
        }

        private void UpdateProfileImage_Worker(object o)
        {
            var updateInfo = (UpdateProfileImageInfo)o;

            var info = new EditUserInfo();
            info.SessionToken = updateInfo.UserSessionToken;

            UploadProfileImage(info, updateInfo.ImagePath);

            var userConfig = UserManagement.EditUser(info, "TrakHound Client Edit User");

            Dispatcher.BeginInvoke(new Action<UserConfiguration>(UpdateProfileImage_Finished), UI_Functions.PRIORITY_BACKGROUND, new object[] { userConfig });
        }

        private void UpdateProfileImage_Finished(UserConfiguration userConfig)
        {
            LoadProfileImage(userConfig);

            CurrentUser = userConfig;

            ProfileImageLoading = false;
        }

        public static string OpenImageBrowse()
        {
            string result = null;

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.InitialDirectory = FileLocations.TrakHound;
            dlg.Multiselect = false;
            dlg.Title = "Browse for Profile Image";
            dlg.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";

            Nullable<bool> dialogResult = dlg.ShowDialog();

            if (dialogResult == true)
            {
                if (dlg.FileName != null) result = dlg.FileName;
            }

            return result;
        }

        public static System.Drawing.Image ProcessImage(string path)
        {
            System.Drawing.Image result = null;

            if (File.Exists(path))
            {
                System.Drawing.Image img = Image_Functions.CropImageToCenter(System.Drawing.Image.FromFile(path));

                result = Image_Functions.SetImageSize(img, 200, 200);
            }

            return result;
        }

        void UploadProfileImage(EditUserInfo info, string path)
        {
            if (path != null)
            {
                // Crop and Resize image
                System.Drawing.Image img = ProcessImage(path);
                if (img != null)
                {
                    // Generate random file name for processed/temp image (to be saved in temp folder)
                    string newFilename = String_Functions.RandomString(20);

                    // Get file extension of original file
                    string ext = Path.GetExtension(path);

                    // Make sure Temp directory exists
                    FileLocations.CreateTempDirectory();

                    // Create new full path of temp file
                    string localPath = Path.Combine(FileLocations.TrakHoundTemp, newFilename);
                    if (ext != null) localPath += "." + ext;

                    // Save the processed image to the new temp path
                    img.Save(localPath);

                    // Create a temp UserConfiguration object to pass the current SessionToken to the Files.Upload() method
                    var userConfig = new UserConfiguration();
                    userConfig.SessionToken = info.SessionToken;

                    // Set the HTTP Content Type based on the type of image
                    string contentType = null;
                    if (ext == "jpg") contentType = "image/jpeg";
                    else if (ext == "png") contentType = "image/png";
                    else if (ext == "gif") contentType = "image/gif";

                    var fileData = new HTTP.FileContentData("uploadimage", localPath, contentType);

                    // Upload File
                    var uploadInfos = TrakHound.API.Files.Upload(userConfig, fileData);
                    if (uploadInfos != null && uploadInfos.Length > 0)
                    {
                        string fileId = uploadInfos[0].Id;

                        info.ImageUrl = fileId;
                    }
                }
            }
        }
        
    }
}
