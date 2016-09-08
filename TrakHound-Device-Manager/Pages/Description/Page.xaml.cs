// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Data;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using TrakHound.API;
using TrakHound.API.Users;
using TrakHound.Plugins;
using TrakHound.Plugins.Server;
using TrakHound.Tools;
using TrakHound.Logging;
using TrakHound;
using TrakHound.Tools.Web;
using TrakHound_UI.Functions;

namespace TrakHound_Device_Manager.Pages.Description
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
                    _image = new BitmapImage(new Uri("pack://application:,,,/TrakHound-Device-Manager;component/Resources/About_01.png"));
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
            if (data != null && data.Id != null)
            {
                if (data.Id == "DEVICE_MANAGER")
                {
                    if (data.Data02 != null)
                    {
                        deviceManager = (DeviceManager)data.Data02;
                    }
                    else deviceManager = null;
                }
            }
        }


        public void LoadConfiguration(DataTable dt)
        {
            Loading = true;

            configurationTable = dt;

            // Load Description
            DeviceDescription = DataTable_Functions.GetTableValue(dt, "address", dprefix + "Description", "value");

            // Load Type
            DeviceType = DataTable_Functions.GetTableValue(dt, "address", dprefix + "DeviceType", "value");
            if (DeviceId == null) DeviceId = DataTable_Functions.GetTableValue(dt, "address", dprefix + "Device_Type", "value"); // Try deprecated value if not already found

            // Load Manufacturer
            Manufacturer = DataTable_Functions.GetTableValue(dt, "address", dprefix + "Manufacturer", "value");

            // Load Id
            DeviceId = DataTable_Functions.GetTableValue(dt, "address", dprefix + "DeviceId", "value");
            if (DeviceId == null) DeviceId = DataTable_Functions.GetTableValue(dt, "address", dprefix + "Device_ID", "value"); // Try deprecated value if not already found

            // Load Model
            Model = DataTable_Functions.GetTableValue(dt, "address", dprefix + "Model", "value");

            // Load Serial
            Serial = DataTable_Functions.GetTableValue(dt, "address", dprefix + "Serial", "value");

            // Load Controller
            Controller = DataTable_Functions.GetTableValue(dt, "address", dprefix + "Controller", "value");

            // Load Location
            Location = DataTable_Functions.GetTableValue(dt, "address", dprefix + "Location", "value");


            // Load Manufacturer Logo
            manufacturerLogoFileName = DataTable_Functions.GetTableValue(dt, "address", dprefix + "LogoUrl", "value");
            if (manufacturerLogoFileName == null) manufacturerLogoFileName = DataTable_Functions.GetTableValue(dt, "address", dprefix + "Manufacturer_Logo_Path", "value"); // Try deprecated value if not already found
            LoadManufacturerLogo(manufacturerLogoFileName);

            // Load Device Image
            deviceImageFileName = DataTable_Functions.GetTableValue(dt, "address", dprefix + "ImageUrl", "value");
            if (deviceImageFileName == null) deviceImageFileName = DataTable_Functions.GetTableValue(dt, "address", dprefix + "Image_Path", "value"); // Try deprecated value if not already found
            LoadDeviceImage(deviceImageFileName);

            Loading = false;
        }

        public void SaveConfiguration(DataTable dt)
        {
            // Save Descritpion
            DataTable_Functions.UpdateTableValue(dt, "address", dprefix + "Description", "value", DeviceDescription);

            // Save Type
            DataTable_Functions.UpdateTableValue(dt, "address", dprefix + "DeviceType", "value", DeviceType);

            // Save Manufacturer
            DataTable_Functions.UpdateTableValue(dt, "address", dprefix + "Manufacturer", "value", Manufacturer);

            // Save Id
            DataTable_Functions.UpdateTableValue(dt, "address", dprefix + "DeviceId", "value", DeviceId);

            // Save Model
            DataTable_Functions.UpdateTableValue(dt, "address", dprefix + "Model", "value", Model);

            // Save Serial
            DataTable_Functions.UpdateTableValue(dt, "address", dprefix + "Serial", "value", Serial);

            // Save Controller
            DataTable_Functions.UpdateTableValue(dt, "address", dprefix + "Controller", "value", Controller);

            // Save Location
            DataTable_Functions.UpdateTableValue(dt, "address", dprefix + "Location", "value", Location);


            // Save Device Logo
            DataTable_Functions.UpdateTableValue(dt, "address", dprefix + "LogoUrl", "value", manufacturerLogoFileName);

            // Save Device Image
            DataTable_Functions.UpdateTableValue(dt, "address", dprefix + "ImageUrl", "value", deviceImageFileName);
        }

        #endregion


        string dprefix = "/Description/";

        DataTable configurationTable;

        DeviceManager deviceManager;

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

                SettingChanged?.Invoke(name, oldVal, newVal);
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
            ChangeSetting(dprefix + "Description", ((TrakHound_UI.TextBox)sender).Text);
        }

        private void devicetype_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChangeSetting(dprefix + "DeviceType", ((TrakHound_UI.TextBox)sender).Text);
        }

        private void manufacturer_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChangeSetting(dprefix + "Manufacturer", ((TrakHound_UI.TextBox)sender).Text);
        }

        private void deviceid_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChangeSetting(dprefix + "DeviceId", ((TrakHound_UI.TextBox)sender).Text);
        }

        private void model_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChangeSetting(dprefix + "Model", ((TrakHound_UI.TextBox)sender).Text);
        }

        private void serial_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChangeSetting(dprefix + "Serial", ((TrakHound_UI.TextBox)sender).Text);
        }

        private void controller_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChangeSetting(dprefix + "Controller", ((TrakHound_UI.TextBox)sender).Text);
        }


        private void Help_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender.GetType() == typeof(System.Windows.Shapes.Rectangle))
            {
                var rect = (System.Windows.Shapes.Rectangle)sender;

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
            if (sender.GetType() == typeof(System.Windows.Shapes.Rectangle))
            {
                var rect = (System.Windows.Shapes.Rectangle)sender;

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
            if (sender.GetType() == typeof(System.Windows.Shapes.Rectangle))
            {
                var rect = (System.Windows.Shapes.Rectangle)sender;

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


        private class ImageInfo
        {
            public UserConfiguration UserConfig { get; set; }
            public string FileId { get; set; }
        }

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

        public string ManufacturerLogoLoadingText
        {
            get { return (string)GetValue(ManufacturerLogoLoadingTextProperty); }
            set { SetValue(ManufacturerLogoLoadingTextProperty, value); }
        }

        public static readonly DependencyProperty ManufacturerLogoLoadingTextProperty =
            DependencyProperty.Register("ManufacturerLogoLoadingText", typeof(string), typeof(Page), new PropertyMetadata("Loading.."));


        #region "Load"

        void LoadManufacturerLogo(string fileId)
        {
            if (!string.IsNullOrEmpty(fileId))
            {
                ManufacturerLogoLoading = true;
                ManufacturerLogoLoadingText = "Downloading..";

                var info = new ImageInfo();
                info.FileId = fileId;
                if (deviceManager != null) info.UserConfig = deviceManager.CurrentUser;

                ThreadPool.QueueUserWorkItem(new WaitCallback(LoadManufacturerLogo_Worker), info);
            }
            else ManufacturerLogoLoading = false;
        }

        void LoadManufacturerLogo_Worker(object o)
        {
            BitmapSource result = null;

            if (o != null)
            {
                var info = (ImageInfo)o;

                if (info.FileId != null)
                {
                    System.Drawing.Image img = null;

                    if (img == null && info.UserConfig != null) img = Files.DownloadImage(info.UserConfig, info.FileId);
                    else
                    {
                        string path = Path.Combine(FileLocations.Storage, info.FileId);
                        if (File.Exists(path)) img = System.Drawing.Image.FromFile(path);
                    }

                    if (img != null)
                    {
                        try
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
                        catch (Exception ex) { Logger.Log("Error Loading Device Logo :: " + ex.Message, LogLineType.Error); }
                    }
                }
            }

            Dispatcher.BeginInvoke(new Action<BitmapSource>(LoadManufacturerLogo_GUI), priority, new object[] { result });
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

        #endregion

        #region "Upload"

        private void UploadManufacturerLogo()
        {
            string imagePath = Images.OpenImageBrowse("Select a Device/Manufacturer Logo");
            if (imagePath != null)
            {
                if (File.Exists(imagePath))
                {
                    var info = new ImageInfo();
                    if (deviceManager != null) info.UserConfig = deviceManager.CurrentUser;
                    info.FileId = imagePath;

                    ManufacturerLogoLoading = true;
                    ManufacturerLogoLoadingText = "Uploading..";

                    ThreadPool.QueueUserWorkItem(new WaitCallback(UploadManufacturerLogo_Worker), info);
                }
            }
        }

        private void UploadManufacturerLogo_Worker(object o)
        {
            if (o != null)
            {
                var info = (ImageInfo)o;

                string fileId = null;

                if (info.UserConfig != null)
                {
                    string contentType = null;

                    try
                    {
                        string ext = Path.GetExtension(info.FileId);

                        if (ext == "jpg" || ext == ".jpg") contentType = "image/jpeg";
                        else if (ext == "png" || ext == ".png") contentType = "image/png";
                        else if (ext == "gif" || ext == ".gif") contentType = "image/gif";

                        var img = System.Drawing.Image.FromFile(info.FileId);
                        if (img != null)
                        {
                            if (img.Width > img.Height) img = Image_Functions.SetImageSize(img, Math.Min(300, img.Width), Math.Min(300, img.Height));
                            else img = Image_Functions.SetImageSize(img, 0, Math.Min(150, img.Height));

                            string tempPath = Path.ChangeExtension(Guid.NewGuid().ToString(), ext);
                            tempPath = Path.Combine(FileLocations.TrakHoundTemp, tempPath);

                            // Make sure Temp directory exists
                            FileLocations.CreateTempDirectory();

                            img.Save(tempPath);
                            img.Dispose();

                            var fileData = new HTTP.FileContentData("uploadImage", tempPath, contentType);

                            var fileInfos = Files.Upload(deviceManager.CurrentUser, fileData);
                            if (fileInfos != null && fileInfos.Length > 0)
                            {
                                fileId = fileInfos[0].Id;
                            }
                        }
                    }
                    catch (Exception ex) { Logger.Log("Failed to Upload Image", LogLineType.Error); }
                }
                else
                {
                    string filename = Path.ChangeExtension(Guid.NewGuid().ToString(), ".image");

                    string destinationPath = Path.Combine(FileLocations.Storage, filename);

                    FileLocations.CreateStorageDirectory();

                    File.Copy(info.FileId, destinationPath);

                    fileId = filename;
                }

                Dispatcher.BeginInvoke(new Action<string>(UploadManufacturerLogo_GUI), UI_Functions.PRIORITY_BACKGROUND, new object[] { fileId });
            }
        }

        private void UploadManufacturerLogo_GUI(string fileId)
        {
            if (fileId != null)
            {
                manufacturerLogoFileName = fileId;

                LoadManufacturerLogo(manufacturerLogoFileName);

                ChangeSetting(dprefix + "LogoUrl", manufacturerLogoFileName);
            }
            else ManufacturerLogoLoading = false;
        }

        #endregion


        private void ManufacturerLogo_UploadClicked(TrakHound_UI.ImageBox sender)
        {
            UploadManufacturerLogo();
        }

        private void ManufacturerLogo_ClearClicked(TrakHound_UI.ImageBox sender)
        {
            manufacturerLogoFileName = null;
            ManufacturerLogo = null;
            ManufacturerLogoSet = false;
            ChangeSetting(dprefix + "LogoUrl", null);
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

        public string DeviceImageLoadingText
        {
            get { return (string)GetValue(DeviceImageLoadingTextProperty); }
            set { SetValue(DeviceImageLoadingTextProperty, value); }
        }

        public static readonly DependencyProperty DeviceImageLoadingTextProperty =
            DependencyProperty.Register("DeviceImageLoadingText", typeof(string), typeof(Page), new PropertyMetadata("Loading.."));


        #region "Load"

        void LoadDeviceImage(string fileId)
        {
            if (!string.IsNullOrEmpty(fileId))
            {
                DeviceImageLoading = true;
                DeviceImageLoadingText = "Downloading..";

                var info = new ImageInfo();
                info.FileId = fileId;
                if (deviceManager != null) info.UserConfig = deviceManager.CurrentUser;

                ThreadPool.QueueUserWorkItem(new WaitCallback(LoadDeviceImage_Worker), info);
            }
            else DeviceImageLoading = false;
        }

        void LoadDeviceImage_Worker(object o)
        {
            BitmapSource result = null;

            if (o != null)
            {
                var info = (ImageInfo)o;

                if (info.FileId != null)
                {
                    System.Drawing.Image img = null;
                    if (info.UserConfig != null) img = Files.DownloadImage(info.UserConfig, info.FileId);
                    else
                    {
                        string path = Path.Combine(FileLocations.Storage, info.FileId);
                        if (File.Exists(path)) img = System.Drawing.Image.FromFile(path);
                    }

                    if (img != null)
                    {
                        try
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
                        catch (Exception ex) { Logger.Log("Error Loading Device Image :: " + ex.Message, LogLineType.Error); }
                    }
                }
            }

            Dispatcher.BeginInvoke(new Action<BitmapSource>(LoadDeviceImage_GUI), priority, new object[] { result });
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

        #endregion

        #region "Upload"

        private void UploadDeviceImage()
        {
            string imagePath = Images.OpenImageBrowse("Select a Device Image");
            if (imagePath != null)
            {
                if (File.Exists(imagePath))
                {
                    var info = new ImageInfo();
                    if (deviceManager != null) info.UserConfig = deviceManager.CurrentUser;
                    info.FileId = imagePath;

                    DeviceImageLoading = true;
                    DeviceImageLoadingText = "Uploading..";

                    ThreadPool.QueueUserWorkItem(new WaitCallback(UploadDeviceImage_Worker), info);
                }
            }
        }

        private void UploadDeviceImage_Worker(object o)
        {
            if (o != null)
            {
                var info = (ImageInfo)o;

                string fileId = null;

                if (info.UserConfig != null)
                {
                    string contentType = null;

                    try
                    {
                        string ext = Path.GetExtension(info.FileId);

                        if (ext == "jpg" || ext == ".jpg") contentType = "image/jpeg";
                        else if (ext == "png" || ext == ".png") contentType = "image/png";
                        else if (ext == "gif" || ext == ".gif") contentType = "image/gif";

                        var img = System.Drawing.Image.FromFile(info.FileId);
                        if (img != null)
                        {
                            if (img.Width > img.Height) img = Image_Functions.SetImageSize(img, 300, 300);
                            else img = Image_Functions.SetImageSize(img, 0, 150);

                            string tempPath = Path.ChangeExtension(String_Functions.RandomString(20), ext);
                            tempPath = Path.Combine(FileLocations.TrakHoundTemp, tempPath);

                            // Make sure Temp directory exists
                            FileLocations.CreateTempDirectory();

                            img.Save(tempPath);
                            img.Dispose();

                            var fileData = new HTTP.FileContentData("uploadImage", tempPath, contentType);

                            var fileInfos = Files.Upload(deviceManager.CurrentUser, fileData);
                            if (fileInfos != null && fileInfos.Length > 0)
                            {
                                fileId = fileInfos[0].Id;
                            }
                        }
                    }
                    catch (Exception ex) { Logger.Log("Failed to Upload Image", LogLineType.Error); }
                }
                else
                {
                    //string filename = String_Functions.RandomString(20);
                    string filename = Path.ChangeExtension(Guid.NewGuid().ToString(), ".image");

                    string destinationPath = Path.Combine(FileLocations.Storage, filename);

                    FileLocations.CreateStorageDirectory();

                    File.Copy(info.FileId, destinationPath);

                    fileId = filename;
                }

                Dispatcher.BeginInvoke(new Action<string>(UploadDeviceImage_GUI), UI_Functions.PRIORITY_BACKGROUND, new object[] { fileId });
            }
        }

        private void UploadDeviceImage_GUI(string fileId)
        {
            if (fileId != null)
            {
                deviceImageFileName = fileId;

                LoadDeviceImage(deviceImageFileName);

                ChangeSetting(dprefix + "ImageUrl", deviceImageFileName);

            }
            else DeviceImageLoading = false;
        }

        #endregion


        private void DeviceImage_UploadClicked(TrakHound_UI.ImageBox sender)
        {
            UploadDeviceImage();
        }

        private void DeviceImage_ClearClicked(TrakHound_UI.ImageBox sender)
        {
            deviceImageFileName = null;
            DeviceImage = null;
            DeviceImageSet = false;
            ChangeSetting(dprefix + "ImageUrl", null);
        }

        #endregion

    }
}
