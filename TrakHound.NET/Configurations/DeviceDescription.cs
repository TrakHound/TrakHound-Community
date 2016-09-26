// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using TrakHound.API;

namespace TrakHound.Configurations
{
    public class DeviceDescription : IComparable
    {
        public DeviceDescription() { }

        public DeviceDescription(Data.DeviceInfo deviceInfo)
        {
            UniqueId = deviceInfo.UniqueId;
            Enabled = deviceInfo.Enabled;
            Index = deviceInfo.Index;
            Description = deviceInfo.Description;
            Agent = deviceInfo.Agent;
        }

        public DeviceDescription(DeviceConfiguration deviceConfig)
        {
            UniqueId = deviceConfig.UniqueId;
            Enabled = deviceConfig.Enabled;
            Index = deviceConfig.Index;
            Description = deviceConfig.Description;
            Agent = deviceConfig.Agent;
        }


        public string UniqueId { get; set; }

        public bool Enabled { get; set; }

        private int _index = -1;
        public int Index
        {
            get { return _index; }
            set { _index = value; }
        }

        public Data.DescriptionInfo Description { get; set; }

        public Data.AgentInfo Agent { get; set; }


        #region "IComparable"

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            var i = obj as DeviceDescription;
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

            var other = obj as DeviceDescription;
            if (object.ReferenceEquals(other, null)) return false;

            return (this == other);
        }

        public override int GetHashCode()
        {
            char[] c = this.ToString().ToCharArray();
            return base.GetHashCode();
        }

        #region "Private"

        static bool EqualTo(DeviceDescription c1, DeviceDescription c2)
        {
            if (!object.ReferenceEquals(c1, null) && object.ReferenceEquals(c2, null)) return false;
            if (object.ReferenceEquals(c1, null) && !object.ReferenceEquals(c2, null)) return false;
            if (object.ReferenceEquals(c1, null) && object.ReferenceEquals(c2, null)) return true;

            return c1.UniqueId == c2.UniqueId && c1.Index == c2.Index;
        }

        static bool NotEqualTo(DeviceDescription c1, DeviceDescription c2)
        {
            if (!object.ReferenceEquals(c1, null) && object.ReferenceEquals(c2, null)) return true;
            if (object.ReferenceEquals(c1, null) && !object.ReferenceEquals(c2, null)) return true;
            if (object.ReferenceEquals(c1, null) && object.ReferenceEquals(c2, null)) return false;

            return c1.UniqueId != c2.UniqueId || c1.Index != c2.Index;
        }

        static bool LessThan(DeviceDescription c1, DeviceDescription c2)
        {
            if (c1.Index > c2.Index) return false;
            else return true;
        }

        static bool GreaterThan(DeviceDescription c1, DeviceDescription c2)
        {
            if (c1.Index < c2.Index) return false;
            else return true;
        }

        #endregion

        public static bool operator ==(DeviceDescription c1, DeviceDescription c2)
        {
            return EqualTo(c1, c2);
        }

        public static bool operator !=(DeviceDescription c1, DeviceDescription c2)
        {
            return NotEqualTo(c1, c2);
        }


        public static bool operator <(DeviceDescription c1, DeviceDescription c2)
        {
            return LessThan(c1, c2);
        }

        public static bool operator >(DeviceDescription c1, DeviceDescription c2)
        {
            return GreaterThan(c1, c2);
        }


        public static bool operator <=(DeviceDescription c1, DeviceDescription c2)
        {
            return LessThan(c1, c2) || EqualTo(c1, c2);
        }

        public static bool operator >=(DeviceDescription c1, DeviceDescription c2)
        {
            return GreaterThan(c1, c2) || EqualTo(c1, c2);
        }

        #endregion
    }
}
