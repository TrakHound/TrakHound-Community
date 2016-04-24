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

using System.Globalization;

namespace TH_StatusTable
{
    /// <summary>
    /// Interaction logic for DeviceInfo.xaml
    /// </summary>
    public partial class DeviceInfo : UserControl, IComparable
    {
        public DeviceInfo()
        {
            InitializeComponent();
            DataContext = this;
        }


        public bool Connected
        {
            get { return (bool)GetValue(ConnectedProperty); }
            set { SetValue(ConnectedProperty, value); }
        }

        public static readonly DependencyProperty ConnectedProperty =
            DependencyProperty.Register("Connected", typeof(bool), typeof(DeviceInfo), new PropertyMetadata(false));




        public bool Available
        {
            get { return (bool)GetValue(AvailableProperty); }
            set { SetValue(AvailableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Available.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AvailableProperty =
            DependencyProperty.Register("Available", typeof(bool), typeof(DeviceInfo), new PropertyMetadata(false));





        public TH_Configuration.Configuration Configuration
        {
            get { return (TH_Configuration.Configuration)GetValue(ConfigurationProperty); }
            set { SetValue(ConfigurationProperty, value); }
        }

        public static readonly DependencyProperty ConfigurationProperty =
            DependencyProperty.Register("Configuration", typeof(TH_Configuration.Configuration), typeof(DeviceInfo), new PropertyMetadata(null));



        //public TH_Configuration.Configuration Configuration { get; set; }

        //public TH_Configuration.Description_Settings Description
        //{
        //    get
        //    {
        //        if (Configuration != null) return Configuration.Description;
        //        return null;
        //    }
        //}



        //public string DeviceDescription
        //{
        //    get { return (string)GetValue(DeviceDescriptionProperty); }
        //    set { SetValue(DeviceDescriptionProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for DeviceDescription.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty DeviceDescriptionProperty =
        //    DependencyProperty.Register("DeviceDescription", typeof(string), typeof(DeviceInfo), new PropertyMetadata(null));



        //public string DeviceModel
        //{
        //    get { return (string)GetValue(DeviceModelProperty); }
        //    set { SetValue(DeviceModelProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for DeviceModel.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty DeviceModelProperty =
        //    DependencyProperty.Register("DeviceModel", typeof(string), typeof(DeviceInfo), new PropertyMetadata(null));



        //public string DeviceManufacturer
        //{
        //    get { return (string)GetValue(DeviceManufacturerProperty); }
        //    set { SetValue(DeviceManufacturerProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for DeviceManufacturer.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty DeviceManufacturerProperty =
        //    DependencyProperty.Register("DeviceManufacturer", typeof(string), typeof(DeviceInfo), new PropertyMetadata(null));





        //public string DeviceDescription
        //{
        //    get
        //    {
        //        if (Description != null)
        //        {
        //            string t = FormatDeviceDescription(Description.Description);
        //            return t;
        //        }
        //        else return null;
        //    }
        //}


        private const double MAX_TEXT_WIDTH = 150;

        private string FormatDeviceDescription(string s)
        {
            string t = s;

            if (t != null)
            {
                double textWidth = GetFormattedText(t).Width;

                if (textWidth > MAX_TEXT_WIDTH)
                {
                    // Keep removing characters from the string until the max width is met
                    while (textWidth > MAX_TEXT_WIDTH)
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




        public ImageSource ManufacturerLogo
        {
            get { return (ImageSource)GetValue(ManufacturerLogoProperty); }
            set { SetValue(ManufacturerLogoProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ManufacturerLogo.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ManufacturerLogoProperty =
            DependencyProperty.Register("ManufacturerLogo", typeof(ImageSource), typeof(DeviceInfo), new PropertyMetadata(null));



        public bool Alert
        {
            get { return (bool)GetValue(AlertProperty); }
            set { SetValue(AlertProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Alert.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AlertProperty =
            DependencyProperty.Register("Alert", typeof(bool), typeof(DeviceInfo), new PropertyMetadata(false));




        public bool Idle
        {
            get { return (bool)GetValue(IdleProperty); }
            set { SetValue(IdleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Idle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IdleProperty =
            DependencyProperty.Register("Idle", typeof(bool), typeof(DeviceInfo), new PropertyMetadata(false));




        public bool Production
        {
            get { return (bool)GetValue(ProductionProperty); }
            set { SetValue(ProductionProperty, value); }
        }

        public static readonly DependencyProperty ProductionProperty =
            DependencyProperty.Register("Production", typeof(bool), typeof(DeviceInfo), new PropertyMetadata(false));




        public HourData[] HourDatas
        {
            get { return (HourData[])GetValue(HourDatasProperty); }
            set { SetValue(HourDatasProperty, value); }
        }

        public static readonly DependencyProperty HourDatasProperty =
            DependencyProperty.Register("HourDatas", typeof(HourData[]), typeof(DeviceInfo), new PropertyMetadata(null));




        #region "IComparable"

        private int Index
        {
            get
            {
                if (Configuration != null) return Configuration.Index;
                return 0;
            }
        }

        private string UniqueId
        {
            get
            {
                if (Configuration != null) return Configuration.UniqueId;
                return null;
            }
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            var i = obj as DeviceInfo;
            if (i != null)
            {
                if (i.Index > Index) return -1;
                else if (i.Index < Index) return 1;
                else return 0;
            }
            else return 1;
        }

        #endregion


    }
}
