// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

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
using TH_Global;
using TH_Global.Functions;
using TH_Plugins;
using TH_Plugins.Server;
using TH_UserManagement;
using TH_UserManagement.Management;

namespace TH_DeviceManager.Pages.Description
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
        }

        #region "Page Interface"

        public string Title { get { return "Description"; } }

        private BitmapImage _image;
        public ImageSource Image
        {
            get
            {
                if (_image == null)
                {
                    _image = new BitmapImage(new Uri("pack://application:,,,/TH_DeviceManager;component/Resources/About_01.png"));
                    _image.Freeze();
                }

                return _image;
            }
        }

        public bool Loaded { get; set; }

        public event SettingChanged_Handler SettingChanged;


        public event SendData_Handler SendData;

        public void GetSentData(EventData data)
        {

        }


        public void LoadConfiguration(DataTable dt)
        {
            Loading = true;

            configurationTable = dt;

            // Load Description
            DeviceDescription = Table_Functions.GetTableValue(dprefix + "Description", dt);

            // Load Type
            DeviceType = Table_Functions.GetTableValue(dprefix + "Device_Type", dt);

            // Load Manufacturer
            Manufacturer = Table_Functions.GetTableValue(dprefix + "Manufacturer", dt);

            // Load Id
            DeviceId = Table_Functions.GetTableValue(dprefix + "Device_ID", dt);

            // Load Model
            Model = Table_Functions.GetTableValue(dprefix + "Model", dt);

            // Load Serial
            Serial = Table_Functions.GetTableValue(dprefix + "Serial", dt);

            // Load Controller
            Controller = Table_Functions.GetTableValue(dprefix + "Controller", dt);

            // Load Company
            Company = Table_Functions.GetTableValue(dprefix + "Company", dt);

            // Load Location
            Location = Table_Functions.GetTableValue(dprefix + "Location", dt);


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
            Table_Functions.UpdateTableValue(DeviceDescription, dprefix + "Description", dt);

            // Save Type
            Table_Functions.UpdateTableValue(DeviceType, dprefix + "Device_Type", dt);

            // Save Manufacturer
            Table_Functions.UpdateTableValue(Manufacturer, dprefix + "Manufacturer", dt);

            // Save Id
            Table_Functions.UpdateTableValue(DeviceId, dprefix + "Device_ID", dt);

            // Save Model
            Table_Functions.UpdateTableValue(Model, dprefix + "Model", dt);

            // Save Serial
            Table_Functions.UpdateTableValue(Serial, dprefix + "Serial", dt);

            // Save Controller
            Table_Functions.UpdateTableValue(Controller, dprefix + "Controller", dt);

            // Save Company
            Table_Functions.UpdateTableValue(Company, dprefix + "Company", dt);

            // Save Location
            Table_Functions.UpdateTableValue(Location, dprefix + "Location", dt);


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

        #region "Properties"

        public string DeviceDescription
        {
            get { return (string)GetValue(DeviceDescriptionProperty); }
            set { SetValue(DeviceDescriptionProperty, value); }
        }

        public static readonly DependencyProperty DeviceDescriptionProperty =
            DependencyProperty.Register("DeviceDescription", typeof(string), typeof(Page), new PropertyMetadata(null));


        public string DeviceType
        {
            get { return (string)GetValue(DeviceTypeProperty); }
            set { SetValue(DeviceTypeProperty, value); }
        }

        public static readonly DependencyProperty DeviceTypeProperty =
            DependencyProperty.Register("DeviceType", typeof(string), typeof(Page), new PropertyMetadata(null));


        public string Manufacturer
        {
            get { return (string)GetValue(ManufacturerProperty); }
            set { SetValue(ManufacturerProperty, value); }
        }

        public static readonly DependencyProperty ManufacturerProperty =
            DependencyProperty.Register("Manufacturer", typeof(string), typeof(Page), new PropertyMetadata(null));


        public string DeviceId
        {
            get { return (string)GetValue(DeviceIdProperty); }
            set { SetValue(DeviceIdProperty, value); }
        }

        public static readonly DependencyProperty DeviceIdProperty =
            DependencyProperty.Register("DeviceId", typeof(string), typeof(Page), new PropertyMetadata(null));


        public string Model
        {
            get { return (string)GetValue(ModelProperty); }
            set { SetValue(ModelProperty, value); }
        }

        public static readonly DependencyProperty ModelProperty =
            DependencyProperty.Register("Model", typeof(string), typeof(Page), new PropertyMetadata(null));


        public string Serial
        {
            get { return (string)GetValue(SerialProperty); }
            set { SetValue(SerialProperty, value); }
        }

        public static readonly DependencyProperty SerialProperty =
            DependencyProperty.Register("Serial", typeof(string), typeof(Page), new PropertyMetadata(null));

        public string Controller
        {
            get { return (string)GetValue(ControllerProperty); }
            set { SetValue(ControllerProperty, value); }
        }

        public static readonly DependencyProperty ControllerProperty =
            DependencyProperty.Register("Controller", typeof(string), typeof(Page), new PropertyMetadata(null));


        public string Company
        {
            get { return (string)GetValue(CompanyProperty); }
            set { SetValue(CompanyProperty, value); }
        }

        public static readonly DependencyProperty CompanyProperty =
            DependencyProperty.Register("Company", typeof(string), typeof(Page), new PropertyMetadata(null));


        public string Location
        {
            get { return (string)GetValue(LocationProperty); }
            set { SetValue(LocationProperty, value); }
        }

        public static readonly DependencyProperty LocationProperty =
            DependencyProperty.Register("Location", typeof(string), typeof(Page), new PropertyMetadata(null));

        #endregion

        private void TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            UIElement txt = (UIElement)sender;

            if (txt.IsMouseCaptured || txt.IsKeyboardFocused)
            {
                ChangeSetting(null, null);
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
            BitmapSource result = null;

            if (o != null)
            {
                string filename = o.ToString();

                System.Drawing.Image img = Images.GetImage(filename);
                if (img != null)
                {
                    var bmp = new System.Drawing.Bitmap(img);
                    if (bmp != null)
                    {
                        IntPtr bmpPt = bmp.GetHbitmap();
                        result = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmpPt, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                        if (result != null)
                        {
                            if (result.PixelWidth > result.PixelHeight)
                            {
                                result = Image_Functions.SetImageSize(result, 180);
                            }
                            else
                            {
                                result = Image_Functions.SetImageSize(result, 0, 80);
                            }

                            result.Freeze();
                        }

                        bmp.Dispose();
                    }

                    img.Dispose();
                }
            }

            this.Dispatcher.BeginInvoke(new Action<BitmapSource>(LoadManufacturerLogo_GUI), priority, new object[] { result });
        }

        void LoadManufacturerLogo_GUI(BitmapSource img)
        {
            if (img != null)
            {
                ManufacturerLogo = img;
                ManufacturerLogoSet = true;
            }
            else
            {
                ManufacturerLogo = null;
                ManufacturerLogoSet = false;
            }

            ManufacturerLogoLoading = false;
        }

        //void LoadManufacturerLogo_Worker(object o)
        //{
        //    if (o != null)
        //    {
        //        string filename = o.ToString();

        //        System.Drawing.Image img = Images.GetImage(filename);

        //        this.Dispatcher.BeginInvoke(new Action<System.Drawing.Image>(LoadManufacturerLogo_GUI), priority, new object[] { img });
        //    }
        //    else this.Dispatcher.BeginInvoke(new Action<System.Drawing.Image>(LoadManufacturerLogo_GUI), priority, new object[] { null });
        //}



        //void LoadManufacturerLogo_GUI(System.Drawing.Image img)
        //{
        //    if (img != null)
        //    {
        //        System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(img);

        //        IntPtr bmpPt = bmp.GetHbitmap();
        //        BitmapSource bmpSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmpPt, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

        //        bmpSource.Freeze();

        //        if (bmpSource.PixelWidth > bmpSource.PixelHeight)
        //        {
        //            ManufacturerLogo = TH_WPF.Image_Functions.SetImageSize(bmpSource, 180);
        //        }
        //        else
        //        {
        //            ManufacturerLogo = TH_WPF.Image_Functions.SetImageSize(bmpSource, 0, 80);
        //        }

        //        ManufacturerLogoSet = true;
        //    }
        //    else
        //    {
        //        ManufacturerLogo = null;
        //        ManufacturerLogoSet = false;
        //    }

        //    ManufacturerLogoLoading = false;
        //}

        string UploadManufacturerLogo()
        {
            string result = null;

            string imagePath = Images.OpenImageBrowse("Select a Manufacturer Logo");
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
                    Images.UploadImage(localPath);

                    result = filename;
                }
            }

            return result;
        }

        private void ManufacturerLogo_UploadClicked(TH_WPF.ImageBox sender)
        {
            string path = UploadManufacturerLogo();

            if (path != null)
            {
                manufacturerLogoFileName = path;

                LoadManufacturerLogo(manufacturerLogoFileName);

                ChangeSetting(fprefix + "Manufacturer_Logo_Path", manufacturerLogoFileName);
            }
        }

        private void ManufacturerLogo_ClearClicked(TH_WPF.ImageBox sender)
        {
            manufacturerLogoFileName = null;
            ManufacturerLogo = null;
            ManufacturerLogoSet = false;
            ChangeSetting(fprefix + "Manufacturer_Logo_Path", null);
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
            BitmapSource result = null;

            if (o != null)
            {
                string filename = o.ToString();

                System.Drawing.Image img = Images.GetImage(filename);
                if (img != null)
                {
                    var bmp = new System.Drawing.Bitmap(img);
                    if (bmp != null)
                    {
                        IntPtr bmpPt = bmp.GetHbitmap();
                        result = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmpPt, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                        if (result != null)
                        {
                            if (result.PixelWidth > result.PixelHeight)
                            {
                                result = Image_Functions.SetImageSize(result, 180);
                            }
                            else
                            {
                                result = Image_Functions.SetImageSize(result, 0, 180);
                            }

                            result.Freeze();
                        }

                        bmp.Dispose();
                    }

                    img.Dispose();
                }
            }

            this.Dispatcher.BeginInvoke(new Action<BitmapSource>(LoadDeviceImage_GUI), priority, new object[] { result });
        }

        void LoadDeviceImage_GUI(BitmapSource img)
        {
            if (img != null)
            {
                DeviceImage = img;
                DeviceImageSet = true;
            }
            else
            {
                DeviceImage = null;
                DeviceImageSet = false;
            }

            DeviceImageLoading = false;
        }

        //void LoadDeviceImage_Worker(object o)
        //{
        //    if (o != null)
        //    {
        //        string filename = o.ToString();

        //        System.Drawing.Image img = Images.GetImage(filename);

        //        this.Dispatcher.BeginInvoke(new Action<System.Drawing.Image>(LoadDeviceImage_GUI), priority, new object[] { img });
        //    }
        //    else this.Dispatcher.BeginInvoke(new Action<System.Drawing.Image>(LoadDeviceImage_GUI), priority, new object[] { null });
        //}

        //void LoadDeviceImage_GUI(System.Drawing.Image img)
        //{
        //    if (img != null)
        //    {
        //        System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(img);

        //        IntPtr bmpPt = bmp.GetHbitmap();
        //        BitmapSource bmpSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmpPt, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

        //        bmpSource.Freeze();

        //        if (bmpSource.PixelWidth > bmpSource.PixelHeight)
        //        {
        //            DeviceImage = TH_WPF.Image_Functions.SetImageSize(bmpSource, 180);
        //        }
        //        else
        //        {
        //            DeviceImage = TH_WPF.Image_Functions.SetImageSize(bmpSource, 0, 180);
        //        }

        //        DeviceImageSet = true;
        //    }
        //    else
        //    {
        //        DeviceImage = null;
        //        DeviceImageSet = false;
        //    }

        //    DeviceImageLoading = false;
        //}

        string UploadDeviceImage()
        {
            string result = null;

            string imagePath = Images.OpenImageBrowse("Select a Device Image");
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
                    Images.UploadImage(localPath);

                    result = filename;
                }
            }

            return result;
        }


        private void DeviceImage_UploadClicked(TH_WPF.ImageBox sender)
        {
            string path = UploadDeviceImage();

            if (path != null)
            {
                deviceImageFileName = path;

                LoadDeviceImage(deviceImageFileName);

                ChangeSetting(fprefix + "Image_Path", deviceImageFileName);
            }
        }

        private void DeviceImage_ClearClicked(TH_WPF.ImageBox sender)
        {
            deviceImageFileName = null;
            DeviceImage = null;
            DeviceImageSet = false;
            ChangeSetting(fprefix + "Image_Path", null);
        }

        #endregion





    }
}
