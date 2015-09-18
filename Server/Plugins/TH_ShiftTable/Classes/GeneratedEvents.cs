using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TH_ShiftTable
{
    public class GenEventShiftItem
    {
        public GenEventShiftItem() { CaptureItems = new List<TH_GeneratedData.GeneratedData.GeneratedEvents.CaptureItem>(); }

        public string eventName { get; set; }
        public string eventValue { get; set; }
        public int eventNumVal { get; set; }

        public string shiftName { get; set; }

        public SegmentConfiguration segment { get; set; }

        public ShiftTime start { get; set; }
        public ShiftTime end { get; set; }

        public DateTime start_timestamp { get; set; }
        public DateTime end_timestamp { get; set; }

        public ShiftDate shiftDate { get; set; }

        public TimeSpan duration { get; set; }

        public List<TH_GeneratedData.GeneratedData.GeneratedEvents.CaptureItem> CaptureItems;
    }

    public class GeneratedEventConfiguration
    {
        public string name { get; set; }
    }
}
