// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using System;
using System.Windows;
using System.Windows.Controls;

using TrakHound.API;
using TrakHound.Configurations;

namespace TrakHound_Dashboard.Pages.Dashboard.ProductionStatus.Controls
{
    /// <summary>
    /// Interaction logic for Row.xaml
    /// </summary>
    public partial class Row : UserControl, IComparable
    {
        public Row(DeviceDescription device)
        {
            InitializeComponent();
            root.DataContext = this;

            Device = device;
        }

        #region "Dependency Properties"

        public DeviceDescription Device
        {
            get { return (DeviceDescription)GetValue(DeviceProperty); }
            set { SetValue(DeviceProperty, value); }
        }

        public static readonly DependencyProperty DeviceProperty =
            DependencyProperty.Register("Device", typeof(DeviceDescription), typeof(Row), new PropertyMetadata(null));

        public bool Connected
        {
            get { return (bool)GetValue(ConnectedProperty); }
            set { SetValue(ConnectedProperty, value); }
        }

        public static readonly DependencyProperty ConnectedProperty =
            DependencyProperty.Register("Connected", typeof(bool), typeof(Row), new PropertyMetadata(false));



        public string DeviceStatus
        {
            get { return (string)GetValue(DeviceStatusProperty); }
            set { SetValue(DeviceStatusProperty, value); }
        }

        public static readonly DependencyProperty DeviceStatusProperty =
            DependencyProperty.Register("DeviceStatus", typeof(string), typeof(Row), new PropertyMetadata(null));


        public string ProductionStatus
        {
            get { return (string)GetValue(ProductionStatusProperty); }
            set { SetValue(ProductionStatusProperty, value); }
        }

        public static readonly DependencyProperty ProductionStatusProperty =
            DependencyProperty.Register("ProductionStatus", typeof(string), typeof(Row), new PropertyMetadata(null));



        public TimeSpan DayRunTime
        {
            get { return (TimeSpan)GetValue(DayRunTimeProperty); }
            set { SetValue(DayRunTimeProperty, value); }
        }

        public static readonly DependencyProperty DayRunTimeProperty =
            DependencyProperty.Register("DayRunTime", typeof(TimeSpan), typeof(Row), new PropertyMetadata(TimeSpan.Zero));

        public double TotalRunTime
        {
            get { return (double)GetValue(TotalRunTimeProperty); }
            set { SetValue(TotalRunTimeProperty, value); }
        }

        public static readonly DependencyProperty TotalRunTimeProperty =
            DependencyProperty.Register("TotalRunTime", typeof(double), typeof(Row), new PropertyMetadata(0d));


        public TimeSpan DayOperatingTime
        {
            get { return (TimeSpan)GetValue(DayOperatingTimeProperty); }
            set { SetValue(DayOperatingTimeProperty, value); }
        }

        public static readonly DependencyProperty DayOperatingTimeProperty =
            DependencyProperty.Register("DayOperatingTime", typeof(TimeSpan), typeof(Row), new PropertyMetadata(TimeSpan.Zero));

        public double TotalOperatingTime
        {
            get { return (double)GetValue(TotalOperatingTimeProperty); }
            set { SetValue(TotalOperatingTimeProperty, value); }
        }

        public static readonly DependencyProperty TotalOperatingTimeProperty =
            DependencyProperty.Register("TotalOperatingTime", typeof(double), typeof(Row), new PropertyMetadata(0d));



        public TimeSpan DayCuttingTime
        {
            get { return (TimeSpan)GetValue(DayCuttingTimeProperty); }
            set { SetValue(DayCuttingTimeProperty, value); }
        }

        public static readonly DependencyProperty DayCuttingTimeProperty =
            DependencyProperty.Register("DayCuttingTime", typeof(TimeSpan), typeof(Row), new PropertyMetadata(TimeSpan.Zero));

        public double TotalCuttingTime
        {
            get { return (double)GetValue(TotalCuttingTimeProperty); }
            set { SetValue(TotalCuttingTimeProperty, value); }
        }

        public static readonly DependencyProperty TotalCuttingTimeProperty =
            DependencyProperty.Register("TotalCuttingTime", typeof(double), typeof(Row), new PropertyMetadata(0d));



        public TimeSpan DaySpindleTime
        {
            get { return (TimeSpan)GetValue(DaySpindleTimeProperty); }
            set { SetValue(DaySpindleTimeProperty, value); }
        }

        public static readonly DependencyProperty DaySpindleTimeProperty =
            DependencyProperty.Register("DaySpindleTime", typeof(TimeSpan), typeof(Row), new PropertyMetadata(TimeSpan.Zero));

        public double TotalSpindleTime
        {
            get { return (double)GetValue(TotalSpindleTimeProperty); }
            set { SetValue(TotalSpindleTimeProperty, value); }
        }

        public static readonly DependencyProperty TotalSpindleTimeProperty =
            DependencyProperty.Register("TotalSpindleTime", typeof(double), typeof(Row), new PropertyMetadata(0d));


        public int PartCount
        {
            get { return (int)GetValue(PartCountProperty); }
            set { SetValue(PartCountProperty, value); }
        }

        public static readonly DependencyProperty PartCountProperty =
            DependencyProperty.Register("PartCount", typeof(int), typeof(Row), new PropertyMetadata(null));

        #endregion

        public DateTime CurrentTime { get; set; }

        public int Index
        {
            get
            {
                if (Device != null) return Device.Index;
                else return -1;
            }
        }

        public void UpdateData(Data.OeeInfo info)
        {
            if (info != null)
            {
                PartCount = info.TotalPieces;
            }
        }

        public void UpdateData(Data.StatusInfo info)
        {
            if (info != null)
            {
                Connected = info.Connected == 1;
                if (!string.IsNullOrEmpty(info.DeviceStatus)) DeviceStatus = info.DeviceStatus;
                if (!string.IsNullOrEmpty(info.ProductionStatus)) ProductionStatus = info.ProductionStatus;

                DayRunTime = TimeSpan.FromSeconds(info.DayRun);
                DayOperatingTime = TimeSpan.FromSeconds(info.DayOperating);
                DayCuttingTime = TimeSpan.FromSeconds(info.DayCutting);
                DaySpindleTime = TimeSpan.FromSeconds(info.DaySpindle);

                TotalRunTime = info.TotalRun / 3600;
                TotalOperatingTime = info.TotalOperating / 3600;
                TotalCuttingTime = info.TotalCutting / 3600;
                TotalSpindleTime = info.TotalSpindle / 3600;
            }
        }

        public delegate void Clicked_Handler(Row row);
        public event Clicked_Handler Clicked;

        private void UserControl_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Clicked?.Invoke(this);
        }

        public DeviceComparisonTypes ComparisonType { get; set; }

        #region "IComparable"

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            var i = obj as Row;
            if (i != null)
            {
                if (i > this) return -1;
                else if (i < this) return 1;
                else return 0;
            }
            else return 1;
        }

        #region "Private"

        static bool EqualTo(Row r1, Row r2)
        {
            if (!object.ReferenceEquals(r1, null) && object.ReferenceEquals(r2, null)) return false;
            if (object.ReferenceEquals(r1, null) && !object.ReferenceEquals(r2, null)) return false;
            if (object.ReferenceEquals(r1, null) && object.ReferenceEquals(r2, null)) return true;

            bool uniqueId = r1.Device.UniqueId == r2.Device.UniqueId;

            if (r1 != null && r2 != null && r1.Device.Description != null & r2.Device.Description != null)
            {
                var type = r1.ComparisonType;
                switch (type)
                {
                    case DeviceComparisonTypes.CONTROLLER: return uniqueId && r1.Device.Description.Controller == r2.Device.Description.Controller;
                    case DeviceComparisonTypes.DESCRIPTION: return uniqueId && r1.Device.Description.Description == r2.Device.Description.Description;
                    case DeviceComparisonTypes.DEVICE_ID: return uniqueId && r1.Device.Description.DeviceId == r2.Device.Description.DeviceId;
                    case DeviceComparisonTypes.LOCATION: return uniqueId && r1.Device.Description.Location == r2.Device.Description.Location;
                    case DeviceComparisonTypes.MANUFACTURER: return uniqueId && r1.Device.Description.Manufacturer == r2.Device.Description.Manufacturer;
                }
            }

            return uniqueId && r1.Index == r2.Index;
        }

        static bool NotEqualTo(Row r1, Row r2)
        {
            if (!object.ReferenceEquals(r1, null) && object.ReferenceEquals(r2, null)) return true;
            if (object.ReferenceEquals(r1, null) && !object.ReferenceEquals(r2, null)) return true;
            if (object.ReferenceEquals(r1, null) && object.ReferenceEquals(r2, null)) return false;

            bool uniqueId = r1.Device.UniqueId != r2.Device.UniqueId;

            if (r1 != null && r2 != null && r1.Device.Description != null & r2.Device.Description != null)
            {
                var type = r1.ComparisonType;
                switch (type)
                {
                    case DeviceComparisonTypes.CONTROLLER: return uniqueId || r1.Device.Description.Controller != r2.Device.Description.Controller;
                    case DeviceComparisonTypes.DESCRIPTION: return uniqueId || r1.Device.Description.Description != r2.Device.Description.Description;
                    case DeviceComparisonTypes.DEVICE_ID: return uniqueId || r1.Device.Description.DeviceId != r2.Device.Description.DeviceId;
                    case DeviceComparisonTypes.LOCATION: return uniqueId || r1.Device.Description.Location != r2.Device.Description.Location;
                    case DeviceComparisonTypes.MANUFACTURER: return uniqueId || r1.Device.Description.Manufacturer != r2.Device.Description.Manufacturer;
                }
            }

            return uniqueId && r1.Index == r2.Index;
        }

        static bool LessThan(Row r1, Row r2)
        {
            if (r1 != null && r2 != null && r1.Device.Description != null && r2.Device.Description != null)
            {
                var type = r1.ComparisonType;
                switch (type)
                {
                    case DeviceComparisonTypes.CONTROLLER: return LessThan(r1, r2, "Controller");
                    case DeviceComparisonTypes.DESCRIPTION: return LessThan(r1, r2, "Description");
                    case DeviceComparisonTypes.DEVICE_ID: return LessThan(r1, r2, "DeviceId");
                    case DeviceComparisonTypes.LOCATION: return LessThan(r1, r2, "Location");
                    case DeviceComparisonTypes.MANUFACTURER: return LessThan(r1, r2, "Manufacturer");
                }
            }

            if (r1.Index > r2.Index) return false;
            else return true;
        }

        static bool LessThan(Row r1, Row r2, string propertyName)
        {
            var property = typeof(Data.DescriptionInfo).GetProperty(propertyName);
            if (property != null)
            {
                var p1 = property.GetValue(r1.Device.Description, null);
                var p2 = property.GetValue(r2.Device.Description, null);

                string s1 = p1 != null ? p1 as string : null;
                string s2 = p2 != null ? p2 as string : null;

                // Check for null values and put them at the bottom of the list
                if (string.IsNullOrEmpty(s1) && string.IsNullOrEmpty(s2)) return false;
                if (string.IsNullOrEmpty(s1) && !string.IsNullOrEmpty(s2)) return false;
                if (!string.IsNullOrEmpty(s1) && string.IsNullOrEmpty(s2)) return true;

                // Evaluate property comparison
                return string.Compare(s1, s2) <= 0;
            }

            return false;
        }

        static bool GreaterThan(Row r1, Row r2)
        {
            if (r1 != null && r2 != null && r1.Device.Description != null & r2.Device.Description != null)
            {
                var type = r1.ComparisonType;
                switch (type)
                {
                    case DeviceComparisonTypes.CONTROLLER: return GreaterThan(r1, r2, "Controller");
                    case DeviceComparisonTypes.DESCRIPTION: return GreaterThan(r1, r2, "Description");
                    case DeviceComparisonTypes.DEVICE_ID: return GreaterThan(r1, r2, "DeviceId");
                    case DeviceComparisonTypes.LOCATION: return GreaterThan(r1, r2, "Location");
                    case DeviceComparisonTypes.MANUFACTURER: return GreaterThan(r1, r2, "Manufacturer");
                }
            }

            if (r1.Index < r2.Index) return false;
            else return true;
        }

        static bool GreaterThan(Row r1, Row r2, string propertyName)
        {
            var property = typeof(Data.DescriptionInfo).GetProperty(propertyName);
            if (property != null)
            {
                var p1 = property.GetValue(r1.Device.Description, null);
                var p2 = property.GetValue(r2.Device.Description, null);

                string s1 = p1 != null ? p1 as string : null;
                string s2 = p2 != null ? p2 as string : null;

                // Check for null values and put them at the bottom of the list
                if (string.IsNullOrEmpty(s1) && string.IsNullOrEmpty(s2)) return true;
                if (string.IsNullOrEmpty(s1) && !string.IsNullOrEmpty(s2)) return true;
                if (!string.IsNullOrEmpty(s1) && string.IsNullOrEmpty(s2)) return false;

                // Evaluate property comparison
                return string.Compare(s1, s2) >= 0;
            }

            return false;
        }

        //static bool EqualTo(Row r1, Row r2)
        //{
        //    if (!object.ReferenceEquals(r1, null) && object.ReferenceEquals(r2, null)) return false;
        //    if (object.ReferenceEquals(r1, null) && !object.ReferenceEquals(r2, null)) return false;
        //    if (object.ReferenceEquals(r1, null) && object.ReferenceEquals(r2, null)) return true;

        //    return r1.Index == r2.Index;
        //}

        //static bool NotEqualTo(Row r1, Row r2)
        //{
        //    if (!object.ReferenceEquals(r1, null) && object.ReferenceEquals(r2, null)) return true;
        //    if (object.ReferenceEquals(r1, null) && !object.ReferenceEquals(r2, null)) return true;
        //    if (object.ReferenceEquals(r1, null) && object.ReferenceEquals(r2, null)) return false;

        //    return r1.Index != r2.Index;
        //}

        //static bool LessThan(Row r1, Row r2)
        //{
        //    if (r1.Index > r2.Index) return false;
        //    else return true;
        //}

        //static bool GreaterThan(Row r1, Row r2)
        //{
        //    if (r1.Index < r2.Index) return false;
        //    else return true;
        //}

        #endregion

        public static bool operator ==(Row r1, Row r2)
        {
            return EqualTo(r1, r2);
        }

        public static bool operator !=(Row r1, Row r2)
        {
            return NotEqualTo(r1, r2);
        }


        public static bool operator <(Row r1, Row r2)
        {
            return LessThan(r1, r2);
        }

        public static bool operator >(Row r1, Row r2)
        {
            return GreaterThan(r1, r2);
        }


        public static bool operator <=(Row r1, Row r2)
        {
            return LessThan(r1, r2) || EqualTo(r1, r2);
        }

        public static bool operator >=(Row r1, Row r2)
        {
            return GreaterThan(r1, r2) || EqualTo(r1, r2);
        }

        #endregion
    }
}
