// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using TrakHound;
using TrakHound.API;
using TrakHound.API.Users;
using TrakHound.Configurations;
using TrakHound.Logging;
using TrakHound.Tools;

namespace TrakHound_Dashboard.Pages.Dashboard.Overview.Controls
{
    /// <summary>
    /// Interaction logic for Column.xaml
    /// </summary>
    public partial class Column : UserControl, IComparable
    {
        private const int OEE_HIGH = 70;
        private const int OEE_LOW = 50;

        public Column(DeviceDescription device, UserConfiguration userConfig)
        {
            InitializeComponent();
            root.DataContext = this;

            UserConfiguration = userConfig;

            Device = device;

            if (device.Description != null)
            {
                // Load Device Logo
                if (!string.IsNullOrEmpty(device.Description.LogoUrl)) LoadDeviceLogo(device.Description.LogoUrl);

                // Load Device Image
                if (!string.IsNullOrEmpty(device.Description.ImageUrl)) LoadDeviceImage(device.Description.ImageUrl);
            }
        }


        public int Index
        {
            get
            {
                if (Device != null) return Device.Index;
                else return -1;
            }
        }

        public UserConfiguration UserConfiguration { get; set; }

        #region "Dependency Properties"

        public DeviceDescription Device
        {
            get { return (DeviceDescription)GetValue(DeviceProperty); }
            set { SetValue(DeviceProperty, value); }
        }

        public static readonly DependencyProperty DeviceProperty =
            DependencyProperty.Register("Device", typeof(DeviceDescription), typeof(Column), new PropertyMetadata(null));



        public bool Connected
        {
            get { return (bool)GetValue(ConnectedProperty); }
            set { SetValue(ConnectedProperty, value); }
        }

        public static readonly DependencyProperty ConnectedProperty =
            DependencyProperty.Register("Connected", typeof(bool), typeof(Column), new PropertyMetadata(false));



        public string DeviceStatus
        {
            get { return (string)GetValue(DeviceStatusProperty); }
            set { SetValue(DeviceStatusProperty, value); }
        }

        public static readonly DependencyProperty DeviceStatusProperty =
            DependencyProperty.Register("DeviceStatus", typeof(string), typeof(Column), new PropertyMetadata(null));


        public TimeSpan DeviceStatusTime
        {
            get { return (TimeSpan)GetValue(DeviceStatusTimeProperty); }
            set { SetValue(DeviceStatusTimeProperty, value); }
        }

        public static readonly DependencyProperty DeviceStatusTimeProperty =
            DependencyProperty.Register("DeviceStatusTime", typeof(TimeSpan), typeof(Column), new PropertyMetadata(TimeSpan.FromSeconds(0)));



        public double Oee
        {
            get { return (double)GetValue(OeeProperty); }
            set
            {
                SetValue(OeeProperty, value);

                if (value > OEE_HIGH) OeeStatus = 2;
                else if (value > OEE_LOW) OeeStatus = 1;
                else OeeStatus = 0;
            }
        }

        public static readonly DependencyProperty OeeProperty =
            DependencyProperty.Register("Oee", typeof(double), typeof(Column), new PropertyMetadata(0d));

        public int OeeStatus
        {
            get { return (int)GetValue(OeeStatusProperty); }
            set { SetValue(OeeStatusProperty, value); }
        }

        public static readonly DependencyProperty OeeStatusProperty =
            DependencyProperty.Register("OeeStatus", typeof(int), typeof(Column), new PropertyMetadata(0));


        public double Availability
        {
            get { return (double)GetValue(AvailabilityProperty); }
            set
            {
                SetValue(AvailabilityProperty, value);

                if (value > OEE_HIGH) AvailabilityStatus = 2;
                else if (value > OEE_LOW) AvailabilityStatus = 1;
                else AvailabilityStatus = 0;
            }
        }

        public static readonly DependencyProperty AvailabilityProperty =
            DependencyProperty.Register("Availability", typeof(double), typeof(Column), new PropertyMetadata(0d));

        public int AvailabilityStatus
        {
            get { return (int)GetValue(AvailabilityStatusProperty); }
            set { SetValue(AvailabilityStatusProperty, value); }
        }

        public static readonly DependencyProperty AvailabilityStatusProperty =
            DependencyProperty.Register("AvailabilityStatus", typeof(int), typeof(Column), new PropertyMetadata(0));


        public double Performance
        {
            get { return (double)GetValue(PerformanceProperty); }
            set
            {
                SetValue(PerformanceProperty, value);

                if (value > OEE_HIGH) PerformanceStatus = 2;
                else if (value > OEE_LOW) PerformanceStatus = 1;
                else PerformanceStatus = 0;
            }
        }

        public static readonly DependencyProperty PerformanceProperty =
            DependencyProperty.Register("Performance", typeof(double), typeof(Column), new PropertyMetadata(0d));

        public int PerformanceStatus
        {
            get { return (int)GetValue(PerformanceStatusProperty); }
            set { SetValue(PerformanceStatusProperty, value); }
        }

        public static readonly DependencyProperty PerformanceStatusProperty =
            DependencyProperty.Register("PerformanceStatus", typeof(int), typeof(Column), new PropertyMetadata(0));


        public double Quality
        {
            get { return (double)GetValue(QualityProperty); }
            set
            {
                SetValue(QualityProperty, value);

                if (value > OEE_HIGH) QualityStatus = 2;
                else if (value > OEE_LOW) QualityStatus = 1;
                else QualityStatus = 0;
            }
        }

        public static readonly DependencyProperty QualityProperty =
            DependencyProperty.Register("Quality", typeof(double), typeof(Column), new PropertyMetadata(0d));

        public int QualityStatus
        {
            get { return (int)GetValue(QualityStatusProperty); }
            set { SetValue(QualityStatusProperty, value); }
        }

        public static readonly DependencyProperty QualityStatusProperty =
            DependencyProperty.Register("QualityStatus", typeof(int), typeof(Column), new PropertyMetadata(0));



        public double ActivePercentage
        {
            get { return (double)GetValue(ActivePercentageProperty); }
            set { SetValue(ActivePercentageProperty, value); }
        }

        public static readonly DependencyProperty ActivePercentageProperty =
            DependencyProperty.Register("ActivePercentage", typeof(double), typeof(Column), new PropertyMetadata(0d));

        public TimeSpan ActiveTime
        {
            get { return (TimeSpan)GetValue(ActiveTimeProperty); }
            set { SetValue(ActiveTimeProperty, value); }
        }

        public static readonly DependencyProperty ActiveTimeProperty =
            DependencyProperty.Register("ActiveTime", typeof(TimeSpan), typeof(Column), new PropertyMetadata(TimeSpan.Zero));


        public double IdlePercentage
        {
            get { return (double)GetValue(IdlePercentageProperty); }
            set { SetValue(IdlePercentageProperty, value); }
        }

        public static readonly DependencyProperty IdlePercentageProperty =
            DependencyProperty.Register("IdlePercentage", typeof(double), typeof(Column), new PropertyMetadata(0d));

        public TimeSpan IdleTime
        {
            get { return (TimeSpan)GetValue(IdleTimeProperty); }
            set { SetValue(IdleTimeProperty, value); }
        }

        public static readonly DependencyProperty IdleTimeProperty =
            DependencyProperty.Register("IdleTime", typeof(TimeSpan), typeof(Column), new PropertyMetadata(TimeSpan.Zero));


        public double AlertPercentage
        {
            get { return (double)GetValue(AlertPercentageProperty); }
            set { SetValue(AlertPercentageProperty, value); }
        }

        public static readonly DependencyProperty AlertPercentageProperty =
            DependencyProperty.Register("AlertPercentage", typeof(double), typeof(Column), new PropertyMetadata(0d));

        public TimeSpan AlertTime
        {
            get { return (TimeSpan)GetValue(AlertTimeProperty); }
            set { SetValue(AlertTimeProperty, value); }
        }

        public static readonly DependencyProperty AlertTimeProperty =
            DependencyProperty.Register("AlertTime", typeof(TimeSpan), typeof(Column), new PropertyMetadata(TimeSpan.Zero));



        public string ProductionStatus
        {
            get { return (string)GetValue(ProductionStatusProperty); }
            set { SetValue(ProductionStatusProperty, value); }
        }

        public static readonly DependencyProperty ProductionStatusProperty =
            DependencyProperty.Register("ProductionStatus", typeof(string), typeof(Column), new PropertyMetadata(null));



        public double ProductionPercentage
        {
            get { return (double)GetValue(ProductionPercentageProperty); }
            set { SetValue(ProductionPercentageProperty, value); }
        }

        public static readonly DependencyProperty ProductionPercentageProperty =
            DependencyProperty.Register("ProductionPercentage", typeof(double), typeof(Column), new PropertyMetadata(0d));

        public TimeSpan ProductionTime
        {
            get { return (TimeSpan)GetValue(ProductionTimeProperty); }
            set { SetValue(ProductionTimeProperty, value); }
        }

        public static readonly DependencyProperty ProductionTimeProperty =
            DependencyProperty.Register("ProductionTime", typeof(TimeSpan), typeof(Column), new PropertyMetadata(TimeSpan.Zero));



        public double SetupPercentage
        {
            get { return (double)GetValue(SetupPercentageProperty); }
            set { SetValue(SetupPercentageProperty, value); }
        }

        public static readonly DependencyProperty SetupPercentageProperty =
            DependencyProperty.Register("SetupPercentage", typeof(double), typeof(Column), new PropertyMetadata(0d));

        public TimeSpan SetupTime
        {
            get { return (TimeSpan)GetValue(SetupTimeProperty); }
            set { SetValue(SetupTimeProperty, value); }
        }

        public static readonly DependencyProperty SetupTimeProperty =
            DependencyProperty.Register("SetupTime", typeof(TimeSpan), typeof(Column), new PropertyMetadata(TimeSpan.Zero));



        public double TeardownPercentage
        {
            get { return (double)GetValue(TeardownPercentageProperty); }
            set { SetValue(TeardownPercentageProperty, value); }
        }

        public static readonly DependencyProperty TeardownPercentageProperty =
            DependencyProperty.Register("TeardownPercentage", typeof(double), typeof(Column), new PropertyMetadata(0d));

        public TimeSpan TeardownTime
        {
            get { return (TimeSpan)GetValue(TeardownTimeProperty); }
            set { SetValue(TeardownTimeProperty, value); }
        }

        public static readonly DependencyProperty TeardownTimeProperty =
            DependencyProperty.Register("TeardownTime", typeof(TimeSpan), typeof(Column), new PropertyMetadata(TimeSpan.Zero));



        public double MaintenancePercentage
        {
            get { return (double)GetValue(MaintenancePercentageProperty); }
            set { SetValue(MaintenancePercentageProperty, value); }
        }

        public static readonly DependencyProperty MaintenancePercentageProperty =
            DependencyProperty.Register("MaintenancePercentage", typeof(double), typeof(Column), new PropertyMetadata(0d));

        public TimeSpan MaintenanceTime
        {
            get { return (TimeSpan)GetValue(MaintenanceTimeProperty); }
            set { SetValue(MaintenanceTimeProperty, value); }
        }

        public static readonly DependencyProperty MaintenanceTimeProperty =
            DependencyProperty.Register("MaintenanceTime", typeof(TimeSpan), typeof(Column), new PropertyMetadata(TimeSpan.Zero));



        public double ProcessDevelopmentPercentage
        {
            get { return (double)GetValue(ProcessDevelopmentPercentageProperty); }
            set { SetValue(ProcessDevelopmentPercentageProperty, value); }
        }

        public static readonly DependencyProperty ProcessDevelopmentPercentageProperty =
            DependencyProperty.Register("ProcessDevelopmentPercentage", typeof(double), typeof(Column), new PropertyMetadata(0d));

        public TimeSpan ProcessDevelopmentTime
        {
            get { return (TimeSpan)GetValue(ProcessDevelopmentTimeProperty); }
            set { SetValue(ProcessDevelopmentTimeProperty, value); }
        }

        public static readonly DependencyProperty ProcessDevelopmentTimeProperty =
            DependencyProperty.Register("ProcessDevelopmentTime", typeof(TimeSpan), typeof(Column), new PropertyMetadata(TimeSpan.Zero));




        public string EmergencyStop
        {
            get { return (string)GetValue(EmergencyStopProperty); }
            set { SetValue(EmergencyStopProperty, value); }
        }

        public static readonly DependencyProperty EmergencyStopProperty =
            DependencyProperty.Register("EmergencyStop", typeof(string), typeof(Column), new PropertyMetadata(null));

        public string ControllerMode
        {
            get { return (string)GetValue(ControllerModeProperty); }
            set { SetValue(ControllerModeProperty, value); }
        }

        public static readonly DependencyProperty ControllerModeProperty =
            DependencyProperty.Register("ControllerMode", typeof(string), typeof(Column), new PropertyMetadata(null));

        public string ExecutionMode
        {
            get { return (string)GetValue(ExecutionModeProperty); }
            set { SetValue(ExecutionModeProperty, value); }
        }

        public static readonly DependencyProperty ExecutionModeProperty =
            DependencyProperty.Register("ExecutionMode", typeof(string), typeof(Column), new PropertyMetadata(null));

        public string Program
        {
            get { return (string)GetValue(ProgramProperty); }
            set { SetValue(ProgramProperty, value); }
        }

        public static readonly DependencyProperty ProgramProperty =
            DependencyProperty.Register("Program", typeof(string), typeof(Column), new PropertyMetadata(null));

        public string SystemStatus
        {
            get { return (string)GetValue(SystemStatusProperty); }
            set { SetValue(SystemStatusProperty, value); }
        }

        public static readonly DependencyProperty SystemStatusProperty =
            DependencyProperty.Register("SystemStatus", typeof(string), typeof(Column), new PropertyMetadata(null));

        public string SystemMessage
        {
            get { return (string)GetValue(SystemMessageProperty); }
            set { SetValue(SystemMessageProperty, value); }
        }

        public static readonly DependencyProperty SystemMessageProperty =
            DependencyProperty.Register("SystemMessage", typeof(string), typeof(Column), new PropertyMetadata(null));


        public int PartCount
        {
            get { return (int)GetValue(PartCountProperty); }
            set { SetValue(PartCountProperty, value); }
        }

        public static readonly DependencyProperty PartCountProperty =
            DependencyProperty.Register("PartCount", typeof(int), typeof(Column), new PropertyMetadata(0));


        public int WidthStatus
        {
            get { return (int)GetValue(WidthStatusProperty); }
            set { SetValue(WidthStatusProperty, value); }
        }

        public static readonly DependencyProperty WidthStatusProperty =
            DependencyProperty.Register("WidthStatus", typeof(int), typeof(Column), new PropertyMetadata(0));

        #endregion

        #region "Images"

        public ImageSource DeviceImage
        {
            get { return (ImageSource)GetValue(DeviceImageProperty); }
            set { SetValue(DeviceImageProperty, value); }
        }

        public static readonly DependencyProperty DeviceImageProperty =
            DependencyProperty.Register("DeviceImage", typeof(ImageSource), typeof(Column), new PropertyMetadata(null));

        public ImageSource DeviceLogo
        {
            get { return (ImageSource)GetValue(DeviceLogoProperty); }
            set { SetValue(DeviceLogoProperty, value); }
        }

        public static readonly DependencyProperty DeviceLogoProperty =
            DependencyProperty.Register("DeviceLogo", typeof(ImageSource), typeof(Column), new PropertyMetadata(null));
        

        #region "Device Logo"

        public void LoadDeviceLogo(string fileId)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(LoadDeviceLogo_Worker), fileId);
        }

        void LoadDeviceLogo_Worker(object o)
        {
            BitmapSource result = null;

            if (o != null)
            {
                string fileId = o.ToString();

                System.Drawing.Image img = null;

                if (UserConfiguration != null) img = Files.DownloadImage(UserConfiguration, fileId);
                else
                {
                    string path = Path.Combine(FileLocations.Storage, fileId);
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
                                    result = TrakHound_UI.Functions.Images.SetImageSize(result, 300);
                                }
                                else
                                {
                                    result = TrakHound_UI.Functions.Images.SetImageSize(result, 0, 80);
                                }

                                result.Freeze();
                            }
                        }
                    }
                    catch (Exception ex) { Logger.Log("Error Loading Device Logo :: " + ex.Message, LogLineType.Error); }  
                }
            }

            Dispatcher.BeginInvoke(new Action<BitmapSource>(LoadDeviceLogo_GUI), System.Windows.Threading.DispatcherPriority.DataBind, new object[] { result });
        }

        void LoadDeviceLogo_GUI(BitmapSource img)
        {
            DeviceLogo = img;
        }

        #endregion

        #region "Device Image"

        public void LoadDeviceImage(string fileId)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(LoadDeviceImage_Worker), fileId);
        }

        void LoadDeviceImage_Worker(object o)
        {
            BitmapSource result = null;

            if (o != null)
            {
                string fileId = o.ToString();

                System.Drawing.Image img = null;

                if (UserConfiguration != null) img = Files.DownloadImage(UserConfiguration, fileId);
                else
                {
                    string path = Path.Combine(FileLocations.Storage, fileId);
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
                                    result = TrakHound_UI.Functions.Images.SetImageSize(result, 300);
                                }
                                else
                                {
                                    result = TrakHound_UI.Functions.Images.SetImageSize(result, 0, 150);
                                }

                                result.Freeze();
                            }
                        }
                    }
                    catch (Exception ex) { Logger.Log("Error Loading Device Image :: " + ex.Message, LogLineType.Error); }
                }
            }

            Dispatcher.BeginInvoke(new Action<BitmapSource>(LoadDeviceImage_GUI), System.Windows.Threading.DispatcherPriority.DataBind, new object[] { result });
        }

        void LoadDeviceImage_GUI(BitmapSource img)
        {
            DeviceImage = img;
        }

        #endregion

        #endregion

        public void UpdateData(Data.ControllerInfo info)
        {
            if (info != null)
            {
                if (!string.IsNullOrEmpty(info.EmergencyStop)) EmergencyStop = info.EmergencyStop;
                if (!string.IsNullOrEmpty(info.ControllerMode)) ControllerMode = info.ControllerMode;
                if (!string.IsNullOrEmpty(info.ExecutionMode)) ExecutionMode = info.ExecutionMode;
                Program = info.ProgramName;
                SystemStatus = info.SystemStatus;
                SystemMessage = info.SystemMessage;
            }
        }

        public void UpdateData(Data.StatusInfo info)
        {
            if (info != null)
            {
                Connected = info.Connected == 1;
                if (!string.IsNullOrEmpty(info.DeviceStatus)) DeviceStatus = info.DeviceStatus;
                if (!string.IsNullOrEmpty(info.ProductionStatus)) ProductionStatus = info.ProductionStatus;

                DeviceStatusTime = TimeSpan.FromSeconds(info.DeviceStatusTimer);
            }
        }

        public void UpdateData(Data.OeeInfo info)
        {
            if (info != null)
            {
                Oee = info.Oee * 100;
                Availability = info.Availability * 100;
                Performance = info.Performance * 100;
                Quality = info.Quality * 100;
                PartCount = info.TotalPieces;
            }
        }

        public void UpdateData(Data.TimersInfo info)
        {
            if (info != null)
            {
                double total = info.Total;

                // Device Status
                double active = info.Active;
                double idle = info.Idle;
                double alert = info.Alert;

                if (total > 0)
                {
                    ActivePercentage = (active / total) * 100;
                    IdlePercentage = (idle / total) * 100;
                    AlertPercentage = (alert / total) * 100;
                }

                ActiveTime = TimeSpan.FromSeconds(active);
                IdleTime = TimeSpan.FromSeconds(idle);
                AlertTime = TimeSpan.FromSeconds(alert);

                // Production Status
                double production = info.Production;
                double setup = info.Setup;
                double teardown = info.Teardown;
                double maintenance = info.Maintenance;
                double processDevelopment = info.ProcessDevelopment;

                if (total > 0)
                {
                    ProductionPercentage = (production / total) * 100;
                    SetupPercentage = (setup / total) * 100;
                    TeardownPercentage = (teardown / total) * 100;
                    MaintenancePercentage = (maintenance / total) * 100;
                    ProcessDevelopmentPercentage = (processDevelopment / total) * 100;
                }

                ProductionTime = TimeSpan.FromSeconds(production);
                SetupTime = TimeSpan.FromSeconds(setup);
                TeardownTime = TimeSpan.FromSeconds(teardown);
                MaintenanceTime = TimeSpan.FromSeconds(maintenance);
                ProcessDevelopmentTime = TimeSpan.FromSeconds(processDevelopment);
            }
        }


        public delegate void Clicked_Handler(Column column);
        public event Clicked_Handler Clicked;

        private void UserControl_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Clicked?.Invoke(this);
        }

        #region "IComparable"

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            var i = obj as Column;
            if (i != null)
            {
                if (i > this) return -1;
                else if (i < this) return 1;
                else return 0;
            }
            else return 1;
        }

        #region "Private"

        static bool EqualTo(Column c1, Column c2)
        {
            if (!object.ReferenceEquals(c1, null) && object.ReferenceEquals(c2, null)) return false;
            if (object.ReferenceEquals(c1, null) && !object.ReferenceEquals(c2, null)) return false;
            if (object.ReferenceEquals(c1, null) && object.ReferenceEquals(c2, null)) return true;

            return c1.Index == c2.Index;
        }

        static bool NotEqualTo(Column c1, Column c2)
        {
            if (!object.ReferenceEquals(c1, null) && object.ReferenceEquals(c2, null)) return true;
            if (object.ReferenceEquals(c1, null) && !object.ReferenceEquals(c2, null)) return true;
            if (object.ReferenceEquals(c1, null) && object.ReferenceEquals(c2, null)) return false;

            return c1.Index != c2.Index;
        }

        static bool LessThan(Column c1, Column c2)
        {
            if (c1.Index > c2.Index) return false;
            else return true;
        }

        static bool GreaterThan(Column c1, Column c2)
        {
            if (c1.Index < c2.Index) return false;
            else return true;
        }

        #endregion

        public static bool operator ==(Column c1, Column c2)
        {
            return EqualTo(c1, c2);
        }

        public static bool operator !=(Column c1, Column c2)
        {
            return NotEqualTo(c1, c2);
        }


        public static bool operator <(Column c1, Column c2)
        {
            return LessThan(c1, c2);
        }

        public static bool operator >(Column c1, Column c2)
        {
            return GreaterThan(c1, c2);
        }


        public static bool operator <=(Column c1, Column c2)
        {
            return LessThan(c1, c2) || EqualTo(c1, c2);
        }

        public static bool operator >=(Column c1, Column c2)
        {
            return GreaterThan(c1, c2) || EqualTo(c1, c2);
        }

        #endregion
    }

}
