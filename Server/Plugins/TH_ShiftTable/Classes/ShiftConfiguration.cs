using System;
using System.Collections.Generic;

namespace TH_ShiftTable
{
    public class ShiftConfiguration
    {
        public ShiftConfiguration()
        {
            segments = new List<SegmentConfiguration>();
            days = new List<DayOfWeek>();
        }

        public int id { get; set; }

        public string name { get; set; }

        public ShiftTime beginTime { get; set; }
        public ShiftTime endTime { get; set; }

        public List<SegmentConfiguration> segments;

        public List<DayOfWeek> days;
    }

    public class SegmentConfiguration
    {
        public int id { get; set; }

        public string type { get; set; }

        public ShiftTime beginTime { get; set; }
        public ShiftTime endTime { get; set; }

        public ShiftConfiguration shiftConfiguration { get; set; }

        public int dayvalue { get; set; }

        #region "operator overrides"

        public static bool operator ==(SegmentConfiguration sc1, SegmentConfiguration sc2)
        {
            if (ReferenceEquals(sc1, null) && ReferenceEquals(sc2, null)) return true;
            else if (ReferenceEquals(sc1, null) && !ReferenceEquals(sc2, null)) return false;
            else if (!ReferenceEquals(sc1, null) && ReferenceEquals(sc2, null)) return false;
            else if (sc1.id == sc2.id) return true;
            else return false;
        }

        public static bool operator !=(SegmentConfiguration sc1, SegmentConfiguration sc2)
        {
            if (ReferenceEquals(sc1, null) && ReferenceEquals(sc2, null)) return false;
            else if (ReferenceEquals(sc1, null) && !ReferenceEquals(sc2, null)) return true;
            else if (!ReferenceEquals(sc1, null) && ReferenceEquals(sc2, null)) return true;
            else if (sc1.id != sc2.id) return true;
            else return false;
        }

        #endregion
    }
}
