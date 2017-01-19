// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using TrakHound.Configurations;

using TrakHound.API;

namespace TrakHound_Dashboard.Pages.DeviceManager
{
    public class DeviceListItem : DeviceDescription, INotifyPropertyChanged, IComparable
    {
        public DeviceListItem(DeviceDescription device)
        {
            Update(device);
        }

        private bool availability;
        public bool Availability
        {
            get { return availability; }
            set { SetField(ref availability, value, "Availability"); }
        }

        private DeviceComparisonTypes comparisonType;
        public DeviceComparisonTypes ComparisonType
        {
            get { return comparisonType; }
            set { SetField(ref comparisonType, value, "ComparisonType"); }
        }

        public void Update(DeviceDescription device)
        {
            UniqueId = device.UniqueId;
            Index = device.Index;
            Enabled = device.Enabled;
            Description = device.Description;
            Agent = device.Agent;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected bool SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #region "IComparable"

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            var i = obj as DeviceListItem;
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

            var other = obj as DeviceListItem;
            if (object.ReferenceEquals(other, null)) return false;

            return (this == other);
        }

        public override int GetHashCode()
        {
            char[] c = this.ToString().ToCharArray();
            return base.GetHashCode();
        }

        #region "Private"

        static bool EqualTo(DeviceListItem c1, DeviceListItem c2)
        {
            if (!object.ReferenceEquals(c1, null) && object.ReferenceEquals(c2, null)) return false;
            if (object.ReferenceEquals(c1, null) && !object.ReferenceEquals(c2, null)) return false;
            if (object.ReferenceEquals(c1, null) && object.ReferenceEquals(c2, null)) return true;

            bool uniqueId = c1.UniqueId == c2.UniqueId;

            if (c1 != null && c2 != null && c1.Description != null & c2.Description != null)
            {
                var type = c1.ComparisonType;
                switch (type)
                {
                    case DeviceComparisonTypes.CONTROLLER: return uniqueId && c1.Description.Controller == c2.Description.Controller;
                    case DeviceComparisonTypes.DESCRIPTION: return uniqueId && c1.Description.Description == c2.Description.Description;
                    case DeviceComparisonTypes.DEVICE_ID: return uniqueId && c1.Description.DeviceId == c2.Description.DeviceId;
                    case DeviceComparisonTypes.LOCATION: return uniqueId && c1.Description.Location == c2.Description.Location;
                    case DeviceComparisonTypes.MANUFACTURER: return uniqueId && c1.Description.Manufacturer == c2.Description.Manufacturer;
                }
            }

            return uniqueId && c1.Index == c2.Index;
        }

        static bool NotEqualTo(DeviceListItem c1, DeviceListItem c2)
        {
            if (!object.ReferenceEquals(c1, null) && object.ReferenceEquals(c2, null)) return true;
            if (object.ReferenceEquals(c1, null) && !object.ReferenceEquals(c2, null)) return true;
            if (object.ReferenceEquals(c1, null) && object.ReferenceEquals(c2, null)) return false;

            bool uniqueId = c1.UniqueId != c2.UniqueId;

            if (c1 != null && c2 != null && c1.Description != null & c2.Description != null)
            {
                var type = c1.ComparisonType;
                switch (type)
                {
                    case DeviceComparisonTypes.CONTROLLER: return uniqueId || c1.Description.Controller != c2.Description.Controller;
                    case DeviceComparisonTypes.DESCRIPTION: return uniqueId || c1.Description.Description != c2.Description.Description;
                    case DeviceComparisonTypes.DEVICE_ID: return uniqueId || c1.Description.DeviceId != c2.Description.DeviceId;
                    case DeviceComparisonTypes.LOCATION: return uniqueId || c1.Description.Location != c2.Description.Location;
                    case DeviceComparisonTypes.MANUFACTURER: return uniqueId || c1.Description.Manufacturer != c2.Description.Manufacturer;
                }
            }

            return uniqueId && c1.Index == c2.Index;
        }

        static bool LessThan(DeviceListItem c1, DeviceListItem c2)
        {
            if (c1 != null && c2 != null && c1.Description != null && c2.Description != null)
            {
                var type = c1.ComparisonType;
                switch (type)
                {
                    case DeviceComparisonTypes.CONTROLLER: return LessThan(c1, c2, "Controller");
                    case DeviceComparisonTypes.DESCRIPTION: return LessThan(c1, c2, "Description");
                    case DeviceComparisonTypes.DEVICE_ID: return LessThan(c1, c2, "DeviceId");
                    case DeviceComparisonTypes.LOCATION: return LessThan(c1, c2, "Location");
                    case DeviceComparisonTypes.MANUFACTURER: return LessThan(c1, c2, "Manufacturer");
                }
            }

            if (c1.Index > c2.Index) return false;
            else return true;
        }

        static bool LessThan(DeviceListItem c1, DeviceListItem c2, string propertyName)
        {
            var property = typeof(Data.DescriptionInfo).GetProperty(propertyName);
            if (property != null)
            {
                var p1 = property.GetValue(c1.Description, null);
                var p2 = property.GetValue(c2.Description, null);

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

        static bool GreaterThan(DeviceListItem c1, DeviceListItem c2)
        {
            if (c1 != null && c2 != null && c1.Description != null & c2.Description != null)
            {
                var type = c1.ComparisonType;
                switch (type)
                {
                    case DeviceComparisonTypes.CONTROLLER: return GreaterThan(c1, c2, "Controller");
                    case DeviceComparisonTypes.DESCRIPTION: return GreaterThan(c1, c2, "Description");
                    case DeviceComparisonTypes.DEVICE_ID: return GreaterThan(c1, c2, "DeviceId");
                    case DeviceComparisonTypes.LOCATION: return GreaterThan(c1, c2, "Location");
                    case DeviceComparisonTypes.MANUFACTURER: return GreaterThan(c1, c2, "Manufacturer");
                }
            }

            if (c1.Index < c2.Index) return false;
            else return true;
        }

        static bool GreaterThan(DeviceListItem c1, DeviceListItem c2, string propertyName)
        {
            var property = typeof(Data.DescriptionInfo).GetProperty(propertyName);
            if (property != null)
            {
                var p1 = property.GetValue(c1.Description, null);
                var p2 = property.GetValue(c2.Description, null);

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

        #endregion

        public static bool operator ==(DeviceListItem c1, DeviceListItem c2)
        {
            return EqualTo(c1, c2);
        }

        public static bool operator !=(DeviceListItem c1, DeviceListItem c2)
        {
            return NotEqualTo(c1, c2);
        }


        public static bool operator <(DeviceListItem c1, DeviceListItem c2)
        {
            return LessThan(c1, c2);
        }

        public static bool operator >(DeviceListItem c1, DeviceListItem c2)
        {
            return GreaterThan(c1, c2);
        }


        public static bool operator <=(DeviceListItem c1, DeviceListItem c2)
        {
            return LessThan(c1, c2) || EqualTo(c1, c2);
        }

        public static bool operator >=(DeviceListItem c1, DeviceListItem c2)
        {
            return GreaterThan(c1, c2) || EqualTo(c1, c2);
        }

        #endregion
    }
}
