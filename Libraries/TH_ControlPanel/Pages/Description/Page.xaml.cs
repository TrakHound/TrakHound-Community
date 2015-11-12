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

using System.Threading;

using System.Collections.ObjectModel;
using System.Data;
using System.IO;

using TH_Configuration;
using TH_Configuration.User;
using TH_Global;
using TH_Global.Functions;
using TH_PlugIns_Server;

namespace TH_DeviceManager.Pages.Description
{
    /// <summary>
    /// Interaction logic for Page.xaml
    /// </summary>
    public partial class Page : UserControl, ConfigurationPage
    {
        public Page()
        {
            InitializeComponent();
            DataContext = this;
        }

        #region "Page Interface"

        public string PageName { get { return "Description"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TH_DeviceManager;component/Resources/About_01.png")); } }

        public UserConfiguration currentUser { get; set; }

        public event SaveRequest_Handler SaveRequest;

        public event SettingChanged_Handler SettingChanged;

        public void LoadConfiguration(DataTable dt)
        {
            Loading = true;

            configurationTable = dt;

            // Load Description
            devicedescription_TXT.Text = Table_Functions.GetTableValue(dprefix + "Description", dt);

            // Load Type
            devicetype_TXT.Text = Table_Functions.GetTableValue(dprefix + "Device_Type", dt);

            // Load Manufacturer
            manufacturer_TXT.Text = Table_Functions.GetTableValue(dprefix + "Manufacturer", dt);

            // Load Id
            deviceid_TXT.Text = Table_Functions.GetTableValue(dprefix + "Device_ID", dt);

            // Load Model
            model_TXT.Text = Table_Functions.GetTableValue(dprefix + "Model", dt);

            // Load Serial
            serial_TXT.Text = Table_Functions.GetTableValue(dprefix + "Serial", dt);

            // Load Controller
            controller_TXT.Text = Table_Functions.GetTableValue(dprefix + "Controller", dt);

            // Load Company
            company_TXT.Text = Table_Functions.GetTableValue(dprefix + "Company", dt);

            // Load Manufacturer Logo
            manufacturerLogoFileName = Table_Functions.GetTableValue(fprefix + "Manufacturer_Logo_Path", dt);
            LoadManufacturerLogo(manufacturerLogoFileName);

            // Load Device Image
            deviceImageFileName = Table_Functions.GetTableValue(fprefix + "Image_Path", dt);
            LoadDeviceImage(deviceImageFileName);

            Loading = false;
        }

        public void SaveConfiguration(DataTable dt)
        {
            // Save Descritpion
            Table_Functions.UpdateTableValue(devicedescription_TXT.Text, dprefix + "Description", dt);

            // Save Type
            Table_Functions.UpdateTableValue(devicetype_TXT.Text, dprefix + "Device_Type", dt);

            // Save Manufacturer
            Table_Functions.UpdateTableValue(manufacturer_TXT.Text, dprefix + "Manufacturer", dt);

            // Save Id
            Table_Functions.UpdateTableValue(deviceid_TXT.Text, dprefix + "Device_ID", dt);

            // Save Model
            Table_Functions.UpdateTableValue(model_TXT.Text, dprefix + "Model", dt);

            // Save Serial
            Table_Functions.UpdateTableValue(serial_TXT.Text, dprefix + "Serial", dt);

            // Save Controller
            Table_Functions.UpdateTableValue(controller_TXT.Text, dprefix + "Controller", dt);

            // Save Company
            Table_Functions.UpdateTableValue(company_TXT.Text, dprefix + "Company", dt);

            // Save Manufacturer Logo
            Table_Functions.UpdateTableValue(manufacturerLogoFileName, fprefix + "Manufacturer_Logo_Path", dt);

            // Save Device Image
            Table_Functions.UpdateTableValue(deviceImageFileName, fprefix + "Image_Path", dt);
        }

        #endregion


        string dprefix = "/Description/";
        string fprefix = "/File_Locations/";

        DataTable configurationTable;


        public bool Loading
        {
            get { return (bool)GetValue(LoadingProperty); }
            set { SetValue(LoadingProperty, value); }
        }

        public static readonly DependencyProperty LoadingProperty =
            DependencyProperty.Register("Loading", typeof(bool), typeof(Page), new PropertyMetadata(false));


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










        private void devicedescription_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChangeSetting(dprefix + "Description", ((TextBox)sender).Text);
        }

        private void devicetype_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChangeSetting(dprefix + "Device_Type", ((TextBox)sender).Text);
        }

        private void manufacturer_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChangeSetting(dprefix + "Manufactuter", ((TextBox)sender).Text);
        }

        private void deviceid_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChangeSetting(dprefix + "Device_ID", ((TextBox)sender).Text);
        }

        private void model_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChangeSetting(dprefix + "Model", ((TextBox)sender).Text);
        }

        private void serial_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChangeSetting(dprefix + "Serial", ((TextBox)sender).Text);
        }

        private void controller_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChangeSetting(dprefix + "Controller", ((TextBox)sender).Text);
        }

        private void company_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChangeSetting(dprefix + "Company", ((TextBox)sender).Text);
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

        private void manufacturerlogo_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            string path = UploadManufacturerLogo();

            if (path != null)
            {
                manufacturerLogoFileName = path;

                LoadManufacturerLogo(manufacturerLogoFileName);

                ChangeSetting(fprefix + "Manufacturer_Logo_Path", manufacturerLogoFileName);
            }
        }

        private void ManufacturerLogo_Clear_MouseDown(object sender, MouseButtonEventArgs e)
        {
            manufacturerLogoFileName = null;
            ManufacturerLogo = null;
            ManufacturerLogoSet = false;
            ChangeSetting(fprefix + "Manufacturer_Logo_Path", null);
        }



        private void deviceimage_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            string path = UploadDeviceImage();

            if (path != null)
            {
                deviceImageFileName = path;

                LoadDeviceImage(deviceImageFileName);

                ChangeSetting(fprefix + "Image_Path", deviceImageFileName);
            }
        }

        private void DeviceImage_Clear_MouseDown(object sender, MouseButtonEventArgs e)
        {
            deviceImageFileName = null;
            DeviceImage = null;
            DeviceImageSet = false;
            ChangeSetting(fprefix + "Image_Path", null);
        }



        const System.Windows.Threading.DispatcherPriority priority = System.Windows.Threading.DispatcherPriority.Background;

        #region "Manufacturer Logo"

        string manufacturerLogoFileName;

        public ImageSource ManufacturerLogo
        {
            get { return (ImageSource)GetValue(ManufacturerLogoProperty); }
            set { SetValue(ManufacturerLogoProperty, value); }
        }

        public static readonly DependencyProperty ManufacturerLogoProperty =
            DependencyProperty.Register("ManufacturerLogo", typeof(ImageSource), typeof(Page), new PropertyMetadata(null));

        public bool ManufacturerLogoSet
        {
            get { return (bool)GetValue(ManufacturerLogoSetProperty); }
            set { SetValue(ManufacturerLogoSetProperty, value); }
        }

        public static readonly DependencyProperty ManufacturerLogoSetProperty =
            DependencyProperty.Register("ManufacturerLogoSet", typeof(bool), typeof(Page), new PropertyMetadata(false));

        public bool ManufacturerLogoLoading
        {
            get { return (bool)GetValue(ManufacturerLogoLoadingProperty); }
            set { SetValue(ManufacturerLogoLoadingProperty, value); }
        }

        public static readonly DependencyProperty ManufacturerLogoLoadingProperty =
            DependencyProperty.Register("ManufacturerLogoLoading", typeof(bool), typeof(Page), new PropertyMetadata(false));


        Thread LoadManufacturerLogo_THREAD;

        void LoadManufacturerLogo(string filename)
        {
            ManufacturerLogoLoading = true;

            if (LoadManufacturerLogo_THREAD != null) LoadManufacturerLogo_THREAD.Abort();

            LoadManufacturerLogo_THREAD = new Thread(new ParameterizedThreadStart(LoadManufacturerLogo_Worker));
            LoadManufacturerLogo_THREAD.Start(filename);
        }

        void LoadManufacturerLogo_Worker(object o)
        {
            if (o != null)
            {
                string filename = o.ToString();

                System.Drawing.Image img = Images.GetImage(filename);

                this.Dispatcher.BeginInvoke(new Action<System.Drawing.Image>(LoadManufacturerLogo_GUI), priority, new object[] { img });
            }
            else this.Dispatcher.BeginInvoke(new Action<System.Drawing.Image>(LoadManufacturerLogo_GUI), priority, new object[] { null });
        }

        void LoadManufacturerLogo_GUI(System.Drawing.Image img)
        {
            if (img != null)
            {
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(img);

                IntPtr bmpPt = bmp.GetHbitmap();
                BitmapSource bmpSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmpPt, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                bmpSource.Freeze();

                if (bmpSource.PixelWidth > bmpSource.PixelHeight)
                {
                    ManufacturerLogo = TH_WPF.Image_Functions.SetImageSize(bmpSource, 180);
                }
                else
                {
                    ManufacturerLogo = TH_WPF.Image_Functions.SetImageSize(bmpSource, 0, 80);
                }

                ManufacturerLogoSet = true;
            }
            else
            {
                ManufacturerLogo = null;
                ManufacturerLogoSet = false;
            }

            ManufacturerLogoLoading = false;
        }

        string UploadManufacturerLogo()
        {
            string result = null;

            string imagePath = TH_Configuration.User.Images.OpenImageBrowse("Select a Manufacturer Logo");
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

        #region "Device Image"

        string deviceImageFileName;

        public ImageSource DeviceImage
        {
            get { return (ImageSource)GetValue(DeviceImageProperty); }
            set { SetValue(DeviceImageProperty, value); }
        }

        public static readonly DependencyProperty DeviceImageProperty =
            DependencyProperty.Register("DeviceImage", typeof(ImageSource), typeof(Page), new PropertyMetadata(null));

        public bool DeviceImageSet
        {
            get { return (bool)GetValue(DeviceImageSetProperty); }
            set { SetValue(DeviceImageSetProperty, value); }
        }

        public static readonly DependencyProperty DeviceImageSetProperty =
            DependencyProperty.Register("DeviceImageSet", typeof(bool), typeof(Page), new PropertyMetadata(false));

        public bool DeviceImageLoading
        {
            get { return (bool)GetValue(DeviceImageLoadingProperty); }
            set { SetValue(DeviceImageLoadingProperty, value); }
        }

        public static readonly DependencyProperty DeviceImageLoadingProperty =
            DependencyProperty.Register("DeviceImageLoading", typeof(bool), typeof(Page), new PropertyMetadata(false));


        Thread LoadDeviceImage_THREAD;

        void LoadDeviceImage(string filename)
        {
            DeviceImageLoading = true;

            if (LoadDeviceImage_THREAD != null) LoadDeviceImage_THREAD.Abort();

            LoadDeviceImage_THREAD = new Thread(new ParameterizedThreadStart(LoadDeviceImage_Worker));
            LoadDeviceImage_THREAD.Start(filename);
        }

        void LoadDeviceImage_Worker(object o)
        {
            if (o != null)
            {
                string filename = o.ToString();

                System.Drawing.Image img = Images.GetImage(filename);

                this.Dispatcher.BeginInvoke(new Action<System.Drawing.Image>(LoadDeviceImage_GUI), priority, new object[] { img });
            }
            else this.Dispatcher.BeginInvoke(new Action<System.Drawing.Image>(LoadDeviceImage_GUI), priority, new object[] { null });
        }

        void LoadDeviceImage_GUI(System.Drawing.Image img)
        {
            if (img != null)
            {
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(img);

                IntPtr bmpPt = bmp.GetHbitmap();
                BitmapSource bmpSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmpPt, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                bmpSource.Freeze();

                if (bmpSource.PixelWidth > bmpSource.PixelHeight)
                {
                    DeviceImage = TH_WPF.Image_Functions.SetImageSize(bmpSource, 180);
                }
                else
                {
                    DeviceImage = TH_WPF.Image_Functions.SetImageSize(bmpSource, 0, 180);
                }

                DeviceImageSet = true;
            }
            else
            {
                DeviceImage = null;
                DeviceImageSet = false;
            }

            DeviceImageLoading = false;
        }

        string UploadDeviceImage()
        {
            string result = null;

            string imagePath = TH_Configuration.User.Images.OpenImageBrowse("Select a Device Image");
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




    }
}
