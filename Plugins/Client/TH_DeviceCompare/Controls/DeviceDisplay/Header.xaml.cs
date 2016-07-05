// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Data;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Globalization;

using TH_Global.TrakHound.Configurations;
using TH_Global.Functions;
using TH_UserManagement.Management;

namespace TH_DeviceCompare.Controls.DeviceDisplay
{
    public enum HeaderViewType
    {
        Minimized,
        Small,
        Large
    }

    /// <summary>
    /// Interaction logic for Header.xaml
    /// </summary>
    public partial class Header : UserControl, IComparable
    {
        public Header()
        {
            InitializeComponent();
            root.DataContext = this;
        }

        public Header(DeviceConfiguration config)
        {
            InitializeComponent();
            root.DataContext = this;

            if (config != null)
            {
                DeviceDescription = config.Description;
                Index = config.Index;
            }

            // Load Device Logo
            if (config.FileLocations.Manufacturer_Logo_Path != null)
            {
                LoadManufacturerLogo(config.FileLocations.Manufacturer_Logo_Path);
            }

            // Load Device Image
            if (config.FileLocations.Image_Path != null)
            {
                LoadDeviceImage(config.FileLocations.Image_Path);
            }
        }

        public int Index { get; set; }

        public TH_DeviceCompare.DeviceDisplay ParentDisplay { get; set; }

        public Description_Settings DeviceDescription
        {
            get { return (Description_Settings)GetValue(DeviceDescriptionProperty); }
            set
            {
                SetValue(DeviceDescriptionProperty, value);

                string shortdescriptionText = null;
                string longdescriptionText = null;
                if (value != null)
                {
                    shortdescriptionText = FormatDeviceDescription(value.Description, 90);
                    longdescriptionText = FormatDeviceDescription(value.Description, 130);
                }
                SetValue(ShortDescriptionTextProperty, shortdescriptionText);
                SetValue(LongDescriptionTextProperty, longdescriptionText);
            }
        }

        public static readonly DependencyProperty DeviceDescriptionProperty =
            DependencyProperty.Register("DeviceDescription", typeof(Description_Settings), typeof(Header), new PropertyMetadata(null));



        public string ShortDescriptionText
        {
            get { return (string)GetValue(ShortDescriptionTextProperty); }
            set { SetValue(ShortDescriptionTextProperty, value); }
        }

        public static readonly DependencyProperty ShortDescriptionTextProperty =
            DependencyProperty.Register("ShortDescriptionText", typeof(string), typeof(Header), new PropertyMetadata(null));


        public string LongDescriptionText
        {
            get { return (string)GetValue(LongDescriptionTextProperty); }
            set { SetValue(LongDescriptionTextProperty, value); }
        }

        public static readonly DependencyProperty LongDescriptionTextProperty =
            DependencyProperty.Register("LongDescriptionText", typeof(string), typeof(Header), new PropertyMetadata(null));




        const System.Windows.Threading.DispatcherPriority priority = System.Windows.Threading.DispatcherPriority.Background;


        #region "Data"

        /// <summary>
        /// Update data using the Snapshots Table
        /// </summary>
        /// <param name="snapshotData"></param>
        public void UpdateData_Snapshots(object snapshotData)
        {
            DeviceStatus = DataTable_Functions.GetTableValue(snapshotData, "name", "Device Status", "value");

            //Update_Alert(snapshotData);

            //Update_Idle(snapshotData);

            //Update_Production(snapshotData);
        }

        /// <summary>
        /// Update data using the Variables Table
        /// </summary>
        /// <param name="varaibleData"></param>
        public void UpdateData_Variables(object variableData)
        {
            Update_Break(variableData);
        }

        //void Update_Production(object snapshotData)
        //{
        //    DataTable dt = snapshotData as DataTable;
        //    if (dt != null)
        //    {
        //        string value = DataTable_Functions.GetTableValue(dt, "name", "Production", "value");

        //        bool val = true;
        //        if (value != null) bool.TryParse(value, out val);

        //        Production = val;
        //    }
        //}

        //void Update_Idle(object snapshotData)
        //{
        //    DataTable dt = snapshotData as DataTable;
        //    if (dt != null)
        //    {
        //        string value = DataTable_Functions.GetTableValue(dt, "name", "Idle", "value");

        //        bool val = true;
        //        if (value != null) bool.TryParse(value, out val);

        //        Idle = val;
        //    }
        //}

        //void Update_Alert(object snapshotData)
        //{
        //    DataTable dt = snapshotData as DataTable;
        //    if (dt != null)
        //    {
        //        string value = DataTable_Functions.GetTableValue(dt, "name", "Alert", "value");

        //        bool val = true;
        //        if (value != null) bool.TryParse(value, out val);

        //        Alert = val;
        //    }
        //}

        void Update_Break(object variableData)
        {
            DataTable dt = variableData as DataTable;
            if (dt != null)
            {
                string value = DataTable_Functions.GetTableValue(dt, "variable", "shift_type", "value");
                if (value != null)
                {
                    if (value.ToLower() == "break")
                    {
                        ScheduledDowntime = true;

                        ScheduledDowntime_Text = "Break";

                        string b = DataTable_Functions.GetTableValue(dt, "variable", "shift_begintime", "value");
                        string e = DataTable_Functions.GetTableValue(dt, "variable", "shift_endtime", "value");

                        DateTime begin = DateTime.MinValue;
                        DateTime end = DateTime.MinValue;

                        if (DateTime.TryParse(b, out begin) && DateTime.TryParse(e, out end))
                        {
                            string breakTimes = "(" + begin.ToShortTimeString() + " - " + end.ToShortTimeString() + ")";
                            ScheduledDowntime_Times = breakTimes;
                        }
                    }
                    else ScheduledDowntime = false;
                }
                else ScheduledDowntime = false;
            }
        }

        #endregion


        #region "Status"

        public bool Connected
        {
            get { return (bool)GetValue(ConnectedProperty); }
            set { SetValue(ConnectedProperty, value); }
        }

        public static readonly DependencyProperty ConnectedProperty =
            DependencyProperty.Register("Connected", typeof(bool), typeof(Header), new PropertyMetadata(false));


        public bool Loading
        {
            get { return (bool)GetValue(LoadingProperty); }
            set { SetValue(LoadingProperty, value); }
        }

        public static readonly DependencyProperty LoadingProperty =
            DependencyProperty.Register("Loading", typeof(bool), typeof(Header), new PropertyMetadata(false));


        public string ConnectionText
        {
            get { return (string)GetValue(ConnectionTextProperty); }
            set { SetValue(ConnectionTextProperty, value); }
        }

        public static readonly DependencyProperty ConnectionTextProperty =
            DependencyProperty.Register("ConnectionText", typeof(string), typeof(Header), new PropertyMetadata(null));


        public string DeviceStatus
        {
            get { return (string)GetValue(DeviceStatusProperty); }
            set { SetValue(DeviceStatusProperty, value); }
        }

        public static readonly DependencyProperty DeviceStatusProperty =
            DependencyProperty.Register("DeviceStatus", typeof(string), typeof(Header), new PropertyMetadata(null));



        //public bool Production
        //{
        //    get { return (bool)GetValue(ProductionProperty); }
        //    set { SetValue(ProductionProperty, value); }
        //}

        //public static readonly DependencyProperty ProductionProperty =
        //    DependencyProperty.Register("Production", typeof(bool), typeof(Header), new PropertyMetadata(false));


        //public bool Idle
        //{
        //    get { return (bool)GetValue(IdleProperty); }
        //    set { SetValue(IdleProperty, value); }
        //}

        //public static readonly DependencyProperty IdleProperty =
        //    DependencyProperty.Register("Idle", typeof(bool), typeof(Header), new PropertyMetadata(false));


        //public bool Alert
        //{
        //    get { return (bool)GetValue(AlertProperty); }
        //    set { SetValue(AlertProperty, value); }
        //}

        //public static readonly DependencyProperty AlertProperty =
        //    DependencyProperty.Register("Alert", typeof(bool), typeof(Header), new PropertyMetadata(false));



        public bool ScheduledDowntime
        {
            get { return (bool)GetValue(ScheduledDowntimeProperty); }
            set { SetValue(ScheduledDowntimeProperty, value); }
        }

        public static readonly DependencyProperty ScheduledDowntimeProperty =
            DependencyProperty.Register("ScheduledDowntime", typeof(bool), typeof(Header), new PropertyMetadata(false));


        public string ScheduledDowntime_Text
        {
            get { return (string)GetValue(ScheduledDowntime_TextProperty); }
            set { SetValue(ScheduledDowntime_TextProperty, value); }
        }

        public static readonly DependencyProperty ScheduledDowntime_TextProperty =
            DependencyProperty.Register("ScheduledDowntime_Text", typeof(string), typeof(Header), new PropertyMetadata(null));


        public string ScheduledDowntime_Times
        {
            get { return (string)GetValue(ScheduledDowntime_TimesProperty); }
            set { SetValue(ScheduledDowntime_TimesProperty, value); }
        }

        public static readonly DependencyProperty ScheduledDowntime_TimesProperty =
            DependencyProperty.Register("ScheduledDowntime_Times", typeof(string), typeof(Header), new PropertyMetadata(null));



        public string LastUpdatedTimestamp
        {
            get { return (string)GetValue(LastUpdatedTimestampProperty); }
            set { SetValue(LastUpdatedTimestampProperty, value); }
        }

        public static readonly DependencyProperty LastUpdatedTimestampProperty =
            DependencyProperty.Register("LastUpdatedTimestamp", typeof(string), typeof(Header), new PropertyMetadata("Never"));

        #endregion

        #region "Minimize / Collapse"

        public HeaderViewType ViewType
        {
            get { return (HeaderViewType)GetValue(ViewTypeProperty); }
            set
            {
                int requested = (int)value;

                int safe = Math.Max(0, requested);
                safe = Math.Min(2, safe);

                SetValue(ViewTypeProperty, (HeaderViewType)safe);

                switch (safe)
                {
                    case 0: AnimateHeight(30); break;
                    case 1: AnimateHeight(150); break;
                    case 2: AnimateHeight(300); break;
                }
            }
        }

        public static readonly DependencyProperty ViewTypeProperty =
            DependencyProperty.Register("ViewType", typeof(HeaderViewType), typeof(Header), new PropertyMetadata(HeaderViewType.Large));
        

        private void AnimateHeight(double to)
        {
            DoubleAnimation animation = new DoubleAnimation();

            animation.From = root.RenderSize.Height;
            animation.To = to;
            animation.Duration = new Duration(TimeSpan.FromMilliseconds(200));
            root.BeginAnimation(HeightProperty, animation);
        }

        #endregion

        #region "Images"

        public ImageSource Device_Image
        {
            get { return (ImageSource)GetValue(Device_ImageProperty); }
            set { SetValue(Device_ImageProperty, value); }
        }

        public static readonly DependencyProperty Device_ImageProperty =
            DependencyProperty.Register("Device_Image", typeof(ImageSource), typeof(Header), new PropertyMetadata(null));


        public ImageSource Manufacturer_Logo
        {
            get { return (ImageSource)GetValue(Manufacturer_LogoProperty); }
            set { SetValue(Manufacturer_LogoProperty, value); }
        }

        public static readonly DependencyProperty Manufacturer_LogoProperty =
            DependencyProperty.Register("Manufacturer_Logo", typeof(ImageSource), typeof(Header), new PropertyMetadata(null));


        #region "Manufacturer Logo"

        public bool ManufacturerLogoLoading
        {
            get { return (bool)GetValue(ManufacturerLogoLoadingProperty); }
            set { SetValue(ManufacturerLogoLoadingProperty, value); }
        }

        public static readonly DependencyProperty ManufacturerLogoLoadingProperty =
            DependencyProperty.Register("ManufacturerLogoLoading", typeof(bool), typeof(Header), new PropertyMetadata(false));


        Thread LoadManufacturerLogo_THREAD;

        public void LoadManufacturerLogo(string filename)
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
                    }
                }
            }

            this.Dispatcher.BeginInvoke(new Action<BitmapSource>(LoadManufacturerLogo_GUI), priority, new object[] { result });
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
            DependencyProperty.Register("DeviceImageLoading", typeof(bool), typeof(Header), new PropertyMetadata(false));


        Thread LoadDeviceImage_THREAD;

        public void LoadDeviceImage(string filename)
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
                                result = Image_Functions.SetImageSize(result, 250);
                            }
                            else
                            {
                                result = Image_Functions.SetImageSize(result, 0, 250);
                            }

                            result.Freeze();
                        }
                    }
                }
            }

            this.Dispatcher.BeginInvoke(new Action<BitmapSource>(LoadDeviceImage_GUI), priority, new object[] { result });
        }
        
        void LoadDeviceImage_GUI(BitmapSource img)
        {
            Device_Image = img;

            DeviceImageLoading = false;
        }

        #endregion

        #endregion

        #region "IsSelected"

        public delegate void Clicked_Handler(int index);
        public event Clicked_Handler Clicked;

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(Header), new PropertyMetadata(false));

        private void Control_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Clicked != null) Clicked(Index);
        }

        #endregion

        #region "IComparable"

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            if (obj.GetType() == typeof(Header))
            {
                var i = obj as Header;
                if (i != null)
                {
                    if (i > this) return -1;
                    else if (i < this) return 1;
                    else return 0;
                }
                else return 1;
            }
            else return 1;
        }

        #region "Private"

        static bool EqualTo(Header c1, Header c2)
        {
            if (!object.ReferenceEquals(c1, null) && object.ReferenceEquals(c2, null)) return false;
            if (object.ReferenceEquals(c1, null) && !object.ReferenceEquals(c2, null)) return false;
            if (object.ReferenceEquals(c1, null) && object.ReferenceEquals(c2, null)) return true;

            return c1.Index == c2.Index;
        }

        static bool NotEqualTo(Header c1, Header c2)
        {
            if (!object.ReferenceEquals(c1, null) && object.ReferenceEquals(c2, null)) return true;
            if (object.ReferenceEquals(c1, null) && !object.ReferenceEquals(c2, null)) return true;
            if (object.ReferenceEquals(c1, null) && object.ReferenceEquals(c2, null)) return false;

            return c1.Index != c2.Index;
        }

        static bool LessThan(Header c1, Header c2)
        {
            if (c1.Index > c2.Index) return false;
            else return true;
        }

        static bool GreaterThan(Header c1, Header c2)
        {
            if (c1.Index < c2.Index) return false;
            else return true;
        }

        #endregion

        public static bool operator ==(Header c1, Header c2)
        {
            return EqualTo(c1, c2);
        }

        public static bool operator !=(Header c1, Header c2)
        {
            return NotEqualTo(c1, c2);
        }


        public static bool operator <(Header c1, Header c2)
        {
            return LessThan(c1, c2);
        }

        public static bool operator >(Header c1, Header c2)
        {
            return GreaterThan(c1, c2);
        }


        public static bool operator <=(Header c1, Header c2)
        {
            return LessThan(c1, c2) || EqualTo(c1, c2);
        }

        public static bool operator >=(Header c1, Header c2)
        {
            return GreaterThan(c1, c2) || EqualTo(c1, c2);
        }

        #endregion

        #region "Text Overflow Formatter"

        //private const double MAX_TEXT_WIDTH = 95;

        private static string FormatDeviceDescription(string s, int maxWidth)
        {
            string t = s;

            if (t != null)
            {
                double textWidth = GetFormattedText(t).Width;

                if (textWidth > maxWidth)
                {
                    // Keep removing characters from the string until the max width is met
                    while (textWidth > maxWidth)
                    {
                        t = t.Substring(0, t.Length - 1);
                        textWidth = GetFormattedText(t).Width;
                    }

                    // Make sure the last character is not a space
                    if (t[t.Length - 1] == ' ' && s.Length > t.Length + 2) t = s.Substring(0, t.Length + 2);

                    // Add the ...
                    t = t + "...";
                }
                else t = s;
            }

            return t;
        }

        private static FormattedText GetFormattedText(string s)
        {
            return new FormattedText(
                        s,
                        CultureInfo.GetCultureInfo("en-us"),
                        FlowDirection.LeftToRight,
                        new Typeface("Arial"),
                        12,
                        Brushes.Black);
        }

        #endregion

    }

    public class DesignTime_Header : Header
    {
        public DesignTime_Header()
        {
            root.DataContext = this;

            Connected = true;
            //Production = true;
            //Idle = false;
            //Alert = false;
            ScheduledDowntime = true;
            LastUpdatedTimestamp = DateTime.Now.ToString();
           
            var d = new Description_Settings();
            d.Description = "Device Description";
            d.Manufacturer = "Manufacturer";
            d.Model = "Model";
            d.Serial = "Serial";
            d.Device_ID = "01";

            DeviceDescription = d;
        }
    }
}
