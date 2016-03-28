using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

using TH_Configuration;
using TH_UserManagement.Management;

namespace TH_DevicePage.Controls
{
    /// <summary>
    /// Interaction logic for DescriptionPanel.xaml
    /// </summary>
    public partial class DescriptionPanel : UserControl
    {
        public DescriptionPanel()
        {
            InitializeComponent();
            root.DataContext = this;
        }


        public TH_Configuration.Configuration Configuration
        {
            get { return (TH_Configuration.Configuration)GetValue(ConfigurationProperty); }
            set
            {
                SetValue(ConfigurationProperty, value);
                LoadImages(value);
            }
        }

        public static readonly DependencyProperty ConfigurationProperty =
            DependencyProperty.Register("Configuration", typeof(TH_Configuration.Configuration), typeof(DescriptionPanel), new PropertyMetadata(null, new PropertyChangedCallback(Configuration_PropertyChanged)));

        private static void Configuration_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var dp = obj as DescriptionPanel;
            if (dp != null && e.NewValue != null)
            {
                var config = (Configuration)e.NewValue;

                dp.LoadImages(config);
            }
        }

        public void LoadImages(Configuration config)
        {
            LoadManufacturerLogo(config.FileLocations.Manufacturer_Logo_Path);
            LoadDeviceImage(config.FileLocations.Image_Path);
        }


        #region "Images"

        public ImageSource Device_Image
        {
            get { return (ImageSource)GetValue(Device_ImageProperty); }
            set { SetValue(Device_ImageProperty, value); }
        }

        public static readonly DependencyProperty Device_ImageProperty =
            DependencyProperty.Register("Device_Image", typeof(ImageSource), typeof(DescriptionPanel), new PropertyMetadata(null));


        public ImageSource Manufacturer_Logo
        {
            get { return (ImageSource)GetValue(Manufacturer_LogoProperty); }
            set { SetValue(Manufacturer_LogoProperty, value); }
        }

        public static readonly DependencyProperty Manufacturer_LogoProperty =
            DependencyProperty.Register("Manufacturer_Logo", typeof(ImageSource), typeof(DescriptionPanel), new PropertyMetadata(null));


        #region "Manufacturer Logo"

        public bool ManufacturerLogoLoading
        {
            get { return (bool)GetValue(ManufacturerLogoLoadingProperty); }
            set { SetValue(ManufacturerLogoLoadingProperty, value); }
        }

        public static readonly DependencyProperty ManufacturerLogoLoadingProperty =
            DependencyProperty.Register("ManufacturerLogoLoading", typeof(bool), typeof(DescriptionPanel), new PropertyMetadata(false));


        public void LoadManufacturerLogo(string filename)
        {
            ManufacturerLogoLoading = true;

            ThreadPool.QueueUserWorkItem(new WaitCallback(LoadManufacturerLogo_Worker), filename);
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
                                result = TH_WPF.Image_Functions.SetImageSize(result, 250);
                            }
                            else
                            {
                                result = TH_WPF.Image_Functions.SetImageSize(result, 0, 75);
                            }

                            result.Freeze();
                        }
                    }
                }
            }

            this.Dispatcher.BeginInvoke(new Action<BitmapSource>(LoadManufacturerLogo_GUI), DevicePage.Priority_Background, new object[] { result });
        }

        void LoadManufacturerLogo_GUI(BitmapSource img)
        {
            Manufacturer_Logo = img;

            ManufacturerLogoLoading = false;
        }

        #endregion

        #region "Device Image"

        public bool DeviceImageLoading
        {
            get { return (bool)GetValue(DeviceImageLoadingProperty); }
            set { SetValue(DeviceImageLoadingProperty, value); }
        }

        public static readonly DependencyProperty DeviceImageLoadingProperty =
            DependencyProperty.Register("DeviceImageLoading", typeof(bool), typeof(DescriptionPanel), new PropertyMetadata(false));


        Thread LoadDeviceImage_THREAD;

        public void LoadDeviceImage(string filename)
        {
            DeviceImageLoading = true;

            ThreadPool.QueueUserWorkItem(new WaitCallback(LoadDeviceImage_Worker), filename);
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
                                result = TH_WPF.Image_Functions.SetImageSize(result, 150);
                            }
                            else
                            {
                                result = TH_WPF.Image_Functions.SetImageSize(result, 0, 150);
                            }

                            result.Freeze();
                        }
                    }
                }
            }

            this.Dispatcher.BeginInvoke(new Action<BitmapSource>(LoadDeviceImage_GUI), DevicePage.Priority_Background, new object[] { result });
        }

        void LoadDeviceImage_GUI(BitmapSource img)
        {
            Device_Image = img;

            DeviceImageLoading = false;
        }

        #endregion

        #endregion

    }
}
