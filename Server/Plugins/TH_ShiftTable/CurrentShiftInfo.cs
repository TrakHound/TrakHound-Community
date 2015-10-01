using System;

using TH_Configuration;

namespace TH_ShiftTable
{
    public class CurrentShiftInfo
    {
        public string id { get; set; }
        public string name { get; set; }
        public string date { get; set; }

        public DateTime currentTime { get; set; }

        public Shift shift { get; set; }

        public Segment segment { get; set; }

        public static CurrentShiftInfo Get(Configuration config, TH_MTC_Data.Streams.ReturnData rd)
        {
            CurrentShiftInfo Result = null;

            ShiftConfiguration sc = ShiftConfiguration.Get(config);
            if (sc != null)
            {
                // Create ShiftDate object for header timestamp
                ShiftDate date = new ShiftDate(rd.header.creationTime);

                DateTime ts = rd.header.creationTime.ToLocalTime();

                bool found = false;

                // Loop through each shift
                foreach (Shift shift in sc.shifts)
                {
                    // Loop through each segment
                    foreach (Segment segment in shift.segments)
                    {
                        // Get DateTime objects from ShiftTime and ShiftDate along with segment.dayOffset
                        DateTime segmentStart = Tools.GetDateTimeFromShiftTime(segment.beginTime, date, segment.beginDayOffset);
                        DateTime segmentEnd = Tools.GetDateTimeFromShiftTime(segment.endTime, date, segment.endDayOffset);

                        if (ts >= segmentStart && ts < segmentEnd)
                        {
                            Result = new CurrentShiftInfo();
                            Result.currentTime = ts;
                            Result.segment = segment;

                            Result.name = shift.name;
                            Result.shift = shift;
                            Result.date = new ShiftDate(rd.header.creationTime).ToString();

                            Result.id = Tools.GetShiftId(date, segment);

                            found = true;
                        }
                        if (found) break;
                    }
                    if (found) break;
                }
            }
            return Result;
        }
    }
}
