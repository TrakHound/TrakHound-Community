// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Windows;
using System.Windows.Controls;

using TrakHound.API;
using TrakHound.Configurations;

namespace TrakHound_Dashboard.Pages.Dashboard.DeviceStatusTimes.Controls
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


        public int Status
        {
            get { return (int)GetValue(StatusProperty); }
            set { SetValue(StatusProperty, value); }
        }

        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register("Status", typeof(int), typeof(Row), new PropertyMetadata(-1));



        public double ActivePercentage
        {
            get { return (double)GetValue(ActivePercentageProperty); }
            set { SetValue(ActivePercentageProperty, value); }
        }

        public static readonly DependencyProperty ActivePercentageProperty =
            DependencyProperty.Register("ActivePercentage", typeof(double), typeof(Row), new PropertyMetadata(0d));


        public double IdlePercentage
        {
            get { return (double)GetValue(IdlePercentageProperty); }
            set { SetValue(IdlePercentageProperty, value); }
        }

        public static readonly DependencyProperty IdlePercentageProperty =
            DependencyProperty.Register("IdlePercentage", typeof(double), typeof(Row), new PropertyMetadata(0d));


        public double AlertPercentage
        {
            get { return (double)GetValue(AlertPercentageProperty); }
            set { SetValue(AlertPercentageProperty, value); }
        }

        public static readonly DependencyProperty AlertPercentageProperty =
            DependencyProperty.Register("AlertPercentage", typeof(double), typeof(Row), new PropertyMetadata(0d));




        public TimeSpan ActiveTime
        {
            get { return (TimeSpan)GetValue(ActiveTimeProperty); }
            set { SetValue(ActiveTimeProperty, value); }
        }

        public static readonly DependencyProperty ActiveTimeProperty =
            DependencyProperty.Register("ActiveTime", typeof(TimeSpan), typeof(Row), new PropertyMetadata(TimeSpan.Zero));


        public TimeSpan IdleTime
        {
            get { return (TimeSpan)GetValue(IdleTimeProperty); }
            set { SetValue(IdleTimeProperty, value); }
        }

        public static readonly DependencyProperty IdleTimeProperty =
            DependencyProperty.Register("IdleTime", typeof(TimeSpan), typeof(Row), new PropertyMetadata(TimeSpan.Zero));


        public TimeSpan AlertTime
        {
            get { return (TimeSpan)GetValue(AlertTimeProperty); }
            set { SetValue(AlertTimeProperty, value); }
        }

        public static readonly DependencyProperty AlertTimeProperty =
            DependencyProperty.Register("AlertTime", typeof(TimeSpan), typeof(Row), new PropertyMetadata(TimeSpan.Zero));

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

        public void UpdateData(Data.StatusInfo info)
        {
            if (info != null)
            {
                Connected = info.Connected == 1;
                if (!string.IsNullOrEmpty(info.DeviceStatus)) DeviceStatus = info.DeviceStatus;
            }
        }

        public void UpdateData(Data.TimersInfo info)
        {
            if (info != null)
            {
                double total = info.Total;
                double active = info.Active;
                double idle = info.Idle;
                double alert = info.Alert;

                if (total > 0)
                {
                    ActivePercentage = active / total;
                    IdlePercentage = idle / total;
                    AlertPercentage = alert / total;
                }

                if (active > idle && active > alert) Status = 2;
                else if (idle > alert && idle > active) Status = 1;
                else Status = 0;

                ActiveTime = TimeSpan.FromSeconds(Math.Min(active, double.MaxValue));
                IdleTime = TimeSpan.FromSeconds(Math.Min(idle, double.MaxValue));
                AlertTime = TimeSpan.FromSeconds(Math.Min(alert, double.MaxValue));
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
