// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using TH_Configuration;

namespace TH_StatusTable
{
    public class DeviceInfo : Control, INotifyPropertyChanged, IComparable
    {
        static DeviceInfo()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DeviceInfo), new FrameworkPropertyMetadata(typeof(DeviceInfo)));
        }

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

        public Configuration Configuration { get; set; }

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

        private bool _alert;
        public bool Alert
        {
            get { return _alert; }
            set
            {
                var val = _alert;
                _alert = value;
                if (val != _alert) NotifyChanged("Alert");
            }
        }

        private bool _idle;
        public bool Idle
        {
            get { return _idle; }
            set
            {
                var val = _idle;
                _idle = value;
                if (val != _idle) NotifyChanged("Idle");
            }
        }

        private bool _production;
        public bool Production
        {
            get { return _production; }
            set
            {
                var val = _production;
                _production = value;
                if (val != _production) NotifyChanged("Production");
            }
        }

        #endregion

        private HourData[] _hourdatas;
        public HourData[] HourDatas
        {
            get
            {
                return _hourdatas;
            }
            set
            {
                var val = _hourdatas;
                _hourdatas = value;

                if (val != _hourdatas) NotifyChanged("HourDatas");
            }
        }


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
                if (i.Index > Index) return -1;
                else if (i.Index < Index) return 1;
                else return 0;
            }
            else return 1;
        }

        #endregion

    }
}
