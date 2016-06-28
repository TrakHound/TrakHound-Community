// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows.Media;
using System.Windows;
using System.Globalization;

using TH_Configuration;

namespace TH_DeviceTable
{
    public class DeviceInfo : INotifyPropertyChanged, IComparable
    {
        private bool _connected;
        public bool Connected
        {
            get { return _connected; }
            set
            {
                var val = _connected;
                _connected = value;
                if (val != _connected) NotifyChanged("Connected");
            }
        }

        private bool _available;
        public bool Available
        {
            get { return _available; }
            set
            {
                var val = _available;
                _available = value;
                if (val != _available) NotifyChanged("Available");
            }
        }

        public DeviceConfiguration Configuration { get; set; }

        public Description_Settings Description
        {
            get
            {
                if (Configuration != null) return Configuration.Description;
                return null;
            }
        }

        public string DeviceDescription
        {
            get
            {
                if (Description != null)
                {
                    string t = FormatDeviceDescription(Description.Description);
                    return t;
                }
                else return null;
            }
        }

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

        #region "Image"

        private ImageSource _manufacturerLogo;
        public ImageSource ManufacturerLogo
        {
            get { return _manufacturerLogo; }
            set
            {
                var val = _manufacturerLogo;
                _manufacturerLogo = value;
                if (val != _manufacturerLogo) NotifyChanged("ManufacturerLogo");
            }
        }

        #endregion

        #region "Status"

        private string _deviceStatus;
        public string DeviceStatus
        {
            get { return _deviceStatus; }
            set
            {
                var val = _deviceStatus;
                _deviceStatus = value;
                if (val != _deviceStatus) NotifyChanged("DeviceStatus");
            }
        }

        //private bool _alert;
        //public bool Alert
        //{
        //    get { return _alert; }
        //    set
        //    {
        //        var val = _alert;
        //        _alert = value;
        //        if (val != _alert) NotifyChanged("Alert");
        //    }
        //}

        //private bool _idle;
        //public bool Idle
        //{
        //    get { return _idle; }
        //    set
        //    {
        //        var val = _idle;
        //        _idle = value;
        //        if (val != _idle) NotifyChanged("Idle");
        //    }
        //}

        //private bool _production;
        //public bool Production
        //{
        //    get { return _production; }
        //    set
        //    {
        //        var val = _production;
        //        _production = value;
        //        if (val != _production) NotifyChanged("Production");
        //    }
        //}

        #endregion

        #region "OEE"

        private double _oee;
        public double Oee
        {
            get { return _oee; }
            set
            {
                var val = _oee;
                _oee = value;
                if (val != _oee) NotifyChanged("Oee");

                if (_oee >= 0.75) OeeStatus = 2;
                else if (_oee >= 0.5) OeeStatus = 1;
                else OeeStatus = 0;
            }
        }

        private int _oeeStatus;
        public int OeeStatus
        {
            get { return _oeeStatus; }
            set
            {
                var val = _oeeStatus;
                _oeeStatus = value;
                if (val != _oeeStatus) NotifyChanged("OeeStatus");
            }
        }

        private double _availability;
        public double Availability
        {
            get { return _availability; }
            set
            {
                var val = _availability;
                _availability = value;
                if (val != _availability) NotifyChanged("Availability");

                if (_availability >= 0.75) AvailabilityStatus = 2;
                else if (_availability >= 0.5) AvailabilityStatus = 1;
                else AvailabilityStatus = 0;
            }
        }

        private int _availabilityStatus;
        public int AvailabilityStatus
        {
            get { return _availabilityStatus; }
            set
            {
                var val = _availabilityStatus;
                _availabilityStatus = value;
                if (val != _availabilityStatus) NotifyChanged("AvailabilityStatus");
            }
        }

        private double _performance;
        public double Performance
        {
            get { return _performance; }
            set
            {
                var val = _performance;
                _performance = value;
                if (val != _performance) NotifyChanged("Performance");

                if (_performance >= 0.75) PerformanceStatus = 2;
                else if (_performance >= 0.5) PerformanceStatus = 1;
                else PerformanceStatus = 0;
            }
        }

        private int _performanceStatus;
        public int PerformanceStatus
        {
            get { return _performanceStatus; }
            set
            {
                var val = _performanceStatus;
                _performanceStatus = value;
                if (val != _performanceStatus) NotifyChanged("PerformanceStatus");
            }
        }

        #endregion

        #region "Production Status"

        public class ProductionStatusInfo
        {
            public string Value { get; set; }
            public int Numval { get; set; }
            public Brush Brush { get; set; }
            public Brush HoverBrush { get; set; }
        }

        public List<ProductionStatusInfo> ProductionStatusInfos = new List<ProductionStatusInfo>();

        private string _productionStatus;
        public string ProductionStatus
        {
            get { return _productionStatus; }
            set
            {
                var val = _productionStatus;
                _productionStatus = value;
                if (val != _productionStatus) NotifyChanged("ProductionStatus");
            }
        }

        private double _productionStatusTotal;
        public double ProductionStatusTotal
        {
            get { return _productionStatusTotal; }
            set
            {
                var val = _productionStatusTotal;
                _productionStatusTotal = value;
                if (val != _productionStatusTotal) NotifyChanged("ProductionStatusTotal");
            }
        }

        private double _productionStatusSeconds;
        public double ProductionStatusSeconds
        {
            get { return _productionStatusSeconds; }
            set
            {
                var val = _productionStatusSeconds;
                _productionStatusSeconds = value;
                if (val != _productionStatusSeconds) NotifyChanged("ProductionStatusSeconds");
            }
        }


        private Brush _productionStatusBrush;
        public Brush ProductionStatusBrush
        {
            get { return _productionStatusBrush; }
            set
            {
                var val = _productionStatusBrush;
                _productionStatusBrush = value;
                if (val != _productionStatusBrush) NotifyChanged("ProductionStatusBrush");
            }
        }

        private Brush _productionStatusBrushHover;
        public Brush ProductionStatusBrushHover
        {
            get { return _productionStatusBrushHover; }
            set
            {
                var val = _productionStatusBrushHover;
                _productionStatusBrushHover = value;
                if (val != _productionStatusBrushHover) NotifyChanged("ProductionStatusBrushHover");
            }
        }

        #endregion

        #region "Part Count"

        private int _partCount;
        public int PartCount
        {
            get { return _partCount; }
            set
            {
                var val = _partCount;
                _partCount = value;
                if (val != _partCount) NotifyChanged("PartCount");
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

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
                if (i > this) return -1;
                else if (i < this) return 1;
                else return 0;
            }
            else return 1;
        }

        public override bool Equals(object obj)
        {

            var other = obj as DeviceInfo;
            if (object.ReferenceEquals(other, null)) return false;

            return (this == other);
        }

        public override int GetHashCode()
        {
            char[] c = this.ToString().ToCharArray();
            return base.GetHashCode();
        }

        #region "Private"

        static bool EqualTo(DeviceInfo o1, DeviceInfo o2)
        {
            if (!object.ReferenceEquals(o1, null) && object.ReferenceEquals(o2, null)) return false;
            if (object.ReferenceEquals(o1, null) && !object.ReferenceEquals(o2, null)) return false;
            if (object.ReferenceEquals(o1, null) && object.ReferenceEquals(o2, null)) return true;

            return o1.UniqueId == o2.UniqueId && o1.Index == o2.Index;
        }

        static bool NotEqualTo(DeviceInfo o1, DeviceInfo o2)
        {
            if (!object.ReferenceEquals(o1, null) && object.ReferenceEquals(o2, null)) return true;
            if (object.ReferenceEquals(o1, null) && !object.ReferenceEquals(o2, null)) return true;
            if (object.ReferenceEquals(o1, null) && object.ReferenceEquals(o2, null)) return false;

            return o1.UniqueId != o2.UniqueId || o1.Index != o2.Index;
        }

        static bool LessThan(DeviceInfo o1, DeviceInfo o2)
        {
            if (o1.Index > o2.Index) return false;
            else return true;
        }

        static bool GreaterThan(DeviceInfo o1, DeviceInfo o2)
        {
            if (o1.Index < o2.Index) return false;
            else return true;
        }

        #endregion

        public static bool operator ==(DeviceInfo o1, DeviceInfo o2)
        {
            return EqualTo(o1, o2);
        }

        public static bool operator !=(DeviceInfo o1, DeviceInfo o2)
        {
            return NotEqualTo(o1, o2);
        }


        public static bool operator <(DeviceInfo o1, DeviceInfo o2)
        {
            return LessThan(o1, o2);
        }

        public static bool operator >(DeviceInfo o1, DeviceInfo o2)
        {
            return GreaterThan(o1, o2);
        }


        public static bool operator <=(DeviceInfo o1, DeviceInfo o2)
        {
            return LessThan(o1, o2) || EqualTo(o1, o2);
        }

        public static bool operator >=(DeviceInfo o1, DeviceInfo o2)
        {
            return GreaterThan(o1, o2) || EqualTo(o1, o2);
        }

        #endregion

    }
}
