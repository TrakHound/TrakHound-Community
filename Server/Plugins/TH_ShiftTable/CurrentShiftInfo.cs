// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

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

        public DateTime shiftStart { get; set; }
        public DateTime shiftEnd { get; set; }

        public Segment segment { get; set; }

        public static CurrentShiftInfo Get(Configuration config, TH_MTC_Data.Streams.ReturnData rd)
        {
            CurrentShiftInfo Result = null;

            ShiftConfiguration sc = ShiftConfiguration.Get(config);
            if (sc != null)
            {
                // Create ShiftDate object for header timestamp
                //ShiftDate date = new ShiftDate(rd.header.creationTime);

                DateTime ts = rd.header.creationTime.ToLocalTime();

                bool found = false;

                // Loop through each shift
                foreach (Shift shift in sc.shifts)
                {
                    DateTime shiftStart = DateTime.MaxValue;
                    DateTime shiftEnd = DateTime.MinValue;

                    // Loop through each segment
                    foreach (Segment segment in shift.segments)
                    {
                        ShiftDate date = new ShiftDate(rd.header.creationTime);
                        if (segment.beginDayOffset > 0 && segment.endDayOffset > 0)
                        {
                            date = date - segment.endDayOffset;
                        }

                        // Get DateTime objects from ShiftTime and ShiftDate along with segment.dayOffset
                        DateTime segmentStart = Tools.GetDateTimeFromShiftTime(segment.beginTime, date, segment.beginDayOffset);
                        DateTime segmentEnd = Tools.GetDateTimeFromShiftTime(segment.endTime, date, segment.endDayOffset);


                        // Account for cases such where start & end are not during the same day as segmentStart and segmentEnd
                        // Usually where dayOffset for segment is > 0 and segmentduringtype = 3;
                        // Example ----------------
                        // start        = 12:05 AM 9/28/2015
                        // end          = 12:10 AM 9/28/2015
                        // segmentStart = 12:00 AM 9/29/2015
                        // segmentEnd   = 12:30 AM 9/29/2015
                        // ------------------------
                        if ((new ShiftDate(ts, false)) != new ShiftDate(segmentStart, false) && new ShiftDate(ts, false) != new ShiftDate(segmentEnd, false) && new ShiftDate(segmentStart, false) == new ShiftDate(segmentEnd, false))
                        {
                            segmentStart = segmentStart.Subtract(new TimeSpan(24, 0, 0));
                            segmentEnd = segmentEnd.Subtract(new TimeSpan(24, 0, 0));
                        }

                        // Account for cases such where End is the next day compared to Start
                        // Example ----------------
                        // segmentStart = 11:00 AM 9/28/2015
                        // segmentEnd   = 12:00 AM 9/28/2015
                        // ------------------------
                        //if (segmentEnd < segmentStart) segmentEnd = segmentEnd.AddDays(1);


                        //Console.WriteLine(segmentStart.ToString() + " :: " + shiftStart.ToString());
                        //Console.WriteLine(segmentEnd.ToString() + " :: " + shiftEnd.ToString());



                        // Set Shift Times 
                        shiftStart = Tools.GetDateTimeFromShiftTime(shift.beginTime, date);
                        shiftEnd = Tools.GetDateTimeFromShiftTime(shift.endTime, date);

                        if (shiftEnd < shiftStart) shiftEnd = shiftEnd.AddDays(1);

                        //if (segmentStart < shiftStart) shiftStart = segmentStart;
                        //if (segmentEnd > shiftEnd) shiftEnd = segmentEnd;

                        if (ts >= segmentStart && ts < segmentEnd)
                        {
                            Result = new CurrentShiftInfo();
                            Result.currentTime = Tools.GetDateTimeFromShiftTime(new ShiftTime(ts), date, segment.endDayOffset);
                            Result.segment = segment;

                            Result.shiftStart = shiftStart;
                            Result.shiftEnd = shiftEnd;

                            Result.name = shift.name;
                            Result.shift = shift;

                            Result.date = date.ToString();

                            //Result.date = new ShiftDate(rd.header.creationTime).ToString();

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
