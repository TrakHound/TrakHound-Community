// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Windows.Media;

using TH_Configuration;

namespace TH_StatusTable
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

        public Configuration Configuration { get; set; }

        public Description_Settings Description
        {
            get
            {
                if (Configuration != null) return Configuration.Description;
                return null;
            }
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

        private bool _manufacturerLogoLoading;
        public bool ManufacturerLogoLoading
        {
            get { return _manufacturerLogoLoading; }
            set
            {
                var val = _manufacturerLogoLoading;
                _manufacturerLogoLoading = value;
                if (val != _manufacturerLogoLoading) NotifyChanged("ManufacturerLogoLoading");
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
