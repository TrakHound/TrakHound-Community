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

using System.Data;
using System.Threading;
using System.IO;

using TH_Configuration;
using TH_Configuration.User;
using TH_Global;
using TH_Global.Functions;

namespace TH_DeviceManager.Pages.AddShare
{
    /// <summary>
    /// Interaction logic for Page.xaml
    /// </summary>
    public partial class Page : UserControl
    {
        public Page()
        {
            InitializeComponent();
            DataContext = this;
        }

        Configuration configuration;

        public DataTable configurationtable;

        public UserConfiguration currentuser;

        public DeviceManager devicemanager;

        public void LoadConfiguration(Configuration config)
        {
            configuration = config;

            if (config != null)
            {
                if (config.Description != null)
                {
                    Description_Settings d = config.Description;

                    manufacturer_TXT.Text = d.Manufacturer;
                    devicetype_TXT.Text = d.Device_Type; ;
                    model_TXT.Text = d.Model;
                    controller_TXT.Text = d.Controller;
                }
            }

            if (currentuser != null)
            {
                author_TXT.Text = TH_Global.Formatting.UppercaseFirst(currentuser.username);

                imageFileName = currentuser.image_url;

                LoadImage(currentuser.image_url);
            }
        }

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

        private void description_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void devicetype_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void manufacturer_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void model_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void controller_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void author_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void version_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void tags_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void dependencies_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Share_Clicked(TH_WPF.Button_01 bt)
        {
            Management.SharedListItem item = new Management.SharedListItem();

            item.description = description_TXT.Text;
            item.device_type = devicetype_TXT.Text;
            item.manufacturer = manufacturer_TXT.Text;
            item.model = model_TXT.Text;
            item.controller = controller_TXT.Text;
            item.author = author_TXT.Text;

            item.upload_date = DateTime.Now;

            item.version = "1.0.0.0";

            item.image_url = imageFileName;

            item.version = version_TXT.Text;
            item.tags = tags_TXT.Text;
            item.dependencies = dependencies_TXT.Text;
            
            if (currentuser != null && configuration != null && configurationtable != null)
            {
                item.id = configuration.UniqueId;

                string tablename = "shared_" + String_Functions.RandomString(20);

                // Save Shared
                Table_Functions.UpdateTableValue("True", "/Shared", configurationtable);

                // Save Shared Tablename
                Table_Functions.UpdateTableValue(tablename, "/SharedTableName", configurationtable);

                if (Management.CreateSharedConfiguration(currentuser, tablename, configurationtable, item))
                {

                }
                else
                {

                }

                if (devicemanager != null)
                {
                    devicemanager.ConfigurationTable = configurationtable;

                    devicemanager.SaveConfiguration(configurationtable);
                }
                    
                    
                    
            }

        }

        const System.Windows.Threading.DispatcherPriority priority = System.Windows.Threading.DispatcherPriority.Background;

        #region "Image"

        string imageFileName;

        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(Page), new PropertyMetadata(null));

        public bool ImageSet
        {
            get { return (bool)GetValue(ImageSetProperty); }
            set { SetValue(ImageSetProperty, value); }
        }

        public static readonly DependencyProperty ImageSetProperty =
            DependencyProperty.Register("ImageSet", typeof(bool), typeof(Page), new PropertyMetadata(false));

        public bool ImageLoading
        {
            get { return (bool)GetValue(ImageLoadingProperty); }
            set { SetValue(ImageLoadingProperty, value); }
        }

        public static readonly DependencyProperty ImageLoadingProperty =
            DependencyProperty.Register("ImageLoading", typeof(bool), typeof(Page), new PropertyMetadata(false));


        Thread LoadImage_THREAD;

        void LoadImage(string filename)
        {
            ImageLoading = true;

            if (LoadImage_THREAD != null) LoadImage_THREAD.Abort();

            LoadImage_THREAD = new Thread(new ParameterizedThreadStart(LoadImage_Worker));
            LoadImage_THREAD.Start(filename);
        }

        void LoadImage_Worker(object o)
        {
            if (o != null)
            {
                string filename = o.ToString();

                System.Drawing.Image img = Images.GetImage(filename);

                this.Dispatcher.BeginInvoke(new Action<System.Drawing.Image>(LoadImage_GUI), priority, new object[] { img });
            }
            else this.Dispatcher.BeginInvoke(new Action<System.Drawing.Image>(LoadImage_GUI), priority, new object[] { null });
        }

        void LoadImage_GUI(System.Drawing.Image img)
        {
            if (img != null)
            {
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(img);

                IntPtr bmpPt = bmp.GetHbitmap();
                BitmapSource bmpSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmpPt, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                bmpSource.Freeze();

                if (bmpSource.PixelWidth > bmpSource.PixelHeight)
                {
                    Image = TH_WPF.Image_Functions.SetImageSize(bmpSource, 180);
                }
                else
                {
                    Image = TH_WPF.Image_Functions.SetImageSize(bmpSource, 0, 180);
                }

                ImageSet = true;
            }
            else
            {
                Image = null;
                ImageSet = false;
            }

            ImageLoading = false;
        }

        string UploadImage()
        {
            string result = null;

            string imagePath = TH_Configuration.User.Images.OpenImageBrowse("Select a Image to Respresent this Configuration");
            if (imagePath != null)
            {
                string filename = String_Functions.RandomString(20);

                string tempdir = FileLocations.TrakHound + @"\temp";
                if (!Directory.Exists(tempdir)) Directory.CreateDirectory(tempdir);

                string localPath = tempdir + @"\" + filename;

                File.Copy(imagePath, localPath);

                if (File.Exists(localPath))
                {
                    System.Drawing.Image img = System.Drawing.Image.FromFile(localPath);

                    if (img.Width > 500 || img.Height > 500)
                    {
                        img = Image_Functions.SetImageSize(img, 500, 500);

                        filename = String_Functions.RandomString(20);
                        localPath = tempdir + @"\" + filename;

                        img.Save(localPath);
                    }
                }

                if (File.Exists(localPath))
                {
                    TH_Configuration.User.Images.UploadImage(localPath);

                    result = filename;
                }
            }

            return result;
        }

        #endregion

        private void Clear_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Image = null;
            ImageSet = false;
            imageFileName = null;
        }

        private void Upload_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            imageFileName = UploadImage();

            LoadImage(imageFileName);
        }
        
    }
}
