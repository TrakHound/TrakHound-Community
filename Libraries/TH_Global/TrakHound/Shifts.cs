using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;

using TH_Global.Functions;

namespace TH_Global.Shifts
{
    public class ShiftId
    {
        public ShiftId(string id)
        {
            Id = id;
        }

        public string Id { get; set; }

        public string Date
        {
            get
            {
                if (Id != null && Id.Length >= 8) return Id.Substring(0, 8);
                else return null;
            }
        }

        public string FormattedDate
        {
            get
            {
                string s = Date;
                if (s != null && s.Length >= 8)
                {
                    string year = s.Substring(0, 4);
                    string month = s.Substring(4, 2);
                    string day = s.Substring(6, 2);

                    string format = "{0}-{1}-{2}";
                    s = string.Format(format, month, day, year);

                    DateTime dt = DateTime.MinValue;
                    if (DateTime.TryParse(s, out dt))
                    {
                        return dt.ToString("yyyy/MM/dd");
                    }
                }
                return null;
            }
        }

        public int Shift
        {
            get
            {
                if (Id != null && Id.Length >= 11)
                {
                    int result = -1;
                    if (int.TryParse(Id.Substring(9, 2), out result))
                    {
                        return result;
                    }
                }
                return -1;
            }
        }

        public int Segment
        {
            get
            {
                if (Id != null && Id.Length >= 14)
                {
                    int result = -1;
                    if (int.TryParse(Id.Substring(12), out result))
                    {
                        return result;
                    }
                }
                return -1;
            }
        }

        public override string ToString() { return Id; }

        public string GetShiftName(DataTable segmentsTable)
        {
            return DataTable_Functions.GetTableValue(segmentsTable, "SHIFT_ID", Shift.ToString(), "SHIFT");
        }

        public string GetSegmentStart(DataTable segmentsTable)
        {
            string format = "SHIFT_ID='{0}' AND SEGMENT_ID='{1}'";
            string filter = string.Format(format, Shift, Segment);
            return DataTable_Functions.GetTableValue(segmentsTable, filter, "START");
        }

        public string GetSegmentEnd(DataTable segmentsTable)
        {
            string format = "SHIFT_ID='{0}' AND SEGMENT_ID='{1}'";
            string filter = string.Format(format, Shift, Segment);
            return DataTable_Functions.GetTableValue(segmentsTable, filter, "END");
        }

        public string GetSegmentType(DataTable segmentsTable)
        {
            string format = "SHIFT_ID='{0}' AND SEGMENT_ID='{1}'";
            string filter = string.Format(format, Shift, Segment);
            return DataTable_Functions.GetTableValue(segmentsTable, filter, "TYPE");
        }

        public DateTime GetSegmentStartTime(DataTable segmentsTable)
        {
            // Get first segment in shift to test if in different day
            string format = "SHIFT_ID='{0}' AND SEGMENT_ID='{1}'";
            string filter = string.Format(format, Shift, 0);
            string shiftStart = DataTable_Functions.GetTableValue(segmentsTable, filter, "START");
            string test = FormattedDate + " " + shiftStart;
            DateTime startDate = DateTime.MinValue;
            DateTime.TryParse(test, out startDate);


            // Get segment start
            format = "SHIFT_ID='{0}' AND SEGMENT_ID='{1}'";
            filter = string.Format(format, Shift, Segment);
            string start = DataTable_Functions.GetTableValue(segmentsTable, filter, "START");
            test = FormattedDate + " " + start;
            DateTime date = DateTime.MinValue;
            DateTime.TryParse(test, out date);

            if (date < startDate) date = date.AddDays(1);

            return date;
        }

    }
}
