// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

using TH_Global.TrakHound.Configurations;

namespace TH_DeviceManager
{
    /// <summary>
    /// Basic Device Information used to display in Device Manager Device Table
    /// </summary>
    public class DeviceListInfo : IComparable
    {
        public DeviceListInfo(DeviceConfiguration config)
        {
            Configuration = config;
        }

        public string UniqueId { get; set; }

        public int Index { get; set; }

        public Configuration Configuration { get; set; }

        #region "IComparable"

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            var i = obj as DeviceListInfo;
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

            var other = obj as DeviceListInfo;
            if (object.ReferenceEquals(other, null)) return false;

            return (this == other);
        }

        public override int GetHashCode()
        {
            char[] c = this.ToString().ToCharArray();
            return base.GetHashCode();
        }

        #region "Private"

        static bool EqualTo(DeviceListInfo i1, DeviceListInfo i2)
        {
            if (!object.ReferenceEquals(i1, null) && object.ReferenceEquals(i2, null)) return false;
            if (object.ReferenceEquals(i1, null) && !object.ReferenceEquals(i2, null)) return false;
            if (object.ReferenceEquals(i1, null) && object.ReferenceEquals(i2, null)) return true;

            return i1.UniqueId == i2.UniqueId && i1.Index == i2.Index;
        }

        static bool NotEqualTo(DeviceListInfo i1, DeviceListInfo i2)
        {
            if (!object.ReferenceEquals(i1, null) && object.ReferenceEquals(i2, null)) return true;
            if (object.ReferenceEquals(i1, null) && !object.ReferenceEquals(i2, null)) return true;
            if (object.ReferenceEquals(i1, null) && object.ReferenceEquals(i2, null)) return false;

            return i1.UniqueId != i2.UniqueId || i1.Index != i2.Index;
        }

        static bool LessThan(DeviceListInfo i1, DeviceListInfo i2)
        {
            if (i1.Index > i2.Index) return false;
            else return true;
        }

        static bool GreaterThan(DeviceListInfo i1, DeviceListInfo i2)
        {
            if (i1.Index < i2.Index) return false;
            else return true;
        }

        #endregion

        public static bool operator ==(DeviceListInfo i1, DeviceListInfo i2)
        {
            return EqualTo(i1, i2);
        }

        public static bool operator !=(DeviceListInfo i1, DeviceListInfo i2)
        {
            return NotEqualTo(i1, i2);
        }


        public static bool operator <(DeviceListInfo i1, DeviceListInfo i2)
        {
            return LessThan(i1, i2);
        }

        public static bool operator >(DeviceListInfo i1, DeviceListInfo i2)
        {
            return GreaterThan(i1, i2);
        }


        public static bool operator <=(DeviceListInfo i1, DeviceListInfo i2)
        {
            return LessThan(i1, i2) || EqualTo(i1, i2);
        }

        public static bool operator >=(DeviceListInfo i1, DeviceListInfo i2)
        {
            return GreaterThan(i1, i2) || EqualTo(i1, i2);
        }

        #endregion

    }
}
