// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;

using TH_Configuration;

namespace TH_ShiftTable
{
    public class ShiftRowInfo
    {
        public ShiftRowInfo()
        {
            genEventRowInfos = new List<GenEventRowInfo>();
        }

        public string id { get; set; }

        public ShiftDate date { get; set; }
        public string shift { get; set; }

        public int segmentId { get; set; }

        public ShiftTime start { get; set; }
        public ShiftTime end { get; set; }

        public ShiftTime start_utc { get; set; }
        public ShiftTime end_utc { get; set; }

        public string type { get; set; }

        public int totalTime { get; set; }

        public List<GenEventRowInfo> genEventRowInfos { get; set; }

        public ShiftRowInfo Copy()
        {
            ShiftRowInfo Result = new ShiftRowInfo();
            Result.id = id;
            Result.date = date;
            Result.shift = shift;
            Result.segmentId = segmentId;
            Result.start = start;
            Result.end = end;
            Result.start_utc = start_utc;
            Result.end_utc = end_utc;
            Result.type = type;
            Result.genEventRowInfos = genEventRowInfos;
            return Result;
        }

        public static List<ShiftRowInfo> Get(Configuration config, List<GenEventShiftItem> genEventShiftItems, TH_MTC_Data.Streams.ReturnData currentData)
        {
            List<ShiftRowInfo> Result = new List<ShiftRowInfo>();

            ShiftConfiguration sc = ShiftConfiguration.Get(config);

            IEnumerable<ShiftDate> shiftDates = genEventShiftItems.Select(x => x.shiftDate).Distinct();
            foreach (ShiftDate shiftDate in shiftDates)
            {
                List<GenEventShiftItem> sameDates = genEventShiftItems.FindAll(x => x.shiftDate == shiftDate);

                IEnumerable<string> shiftNames = sameDates.Select(x => x.shiftName).Distinct();
                foreach (string shiftName in shiftNames)
                {
                    List<GenEventShiftItem> sameShifts = sameDates.FindAll(x => x.shiftName == shiftName);

                    IEnumerable<Segment> segments = sameShifts.Select(x => x.segment).Distinct();
                    foreach (Segment segment in segments)
                    {
                        ShiftRowInfo sri = new ShiftRowInfo();
                        sri.id = Tools.GetShiftId(shiftDate, segment);
                        sri.date = shiftDate;
                        sri.shift = shiftName;
                        sri.segmentId = segment.id;

                        sri.start = segment.beginTime;
                        sri.end = segment.endTime;

                        sri.start_utc = segment.beginTime.ToUTC();
                        sri.end_utc = segment.endTime.ToUTC();

                        sri.type = segment.type;

                        sri.totalTime = Tools.GetTotalShiftSeconds(sri, currentData);

                        if (sc != null)
                        {
                            IEnumerable<string> eventNames = genEventShiftItems.Select(x => x.eventName).Distinct();
                            foreach (string eventName in eventNames.ToList())
                            {
                                //if (sc.generatedEvents.Find(x => x.name.ToLower() == eventName.ToLower()) != null)
                                //{
                                    List<GenEventShiftItem> sameNames = genEventShiftItems.FindAll(x => (x.shiftDate == shiftDate &&
                                            x.shiftName == shiftName &&
                                            x.segment == segment &&
                                            x.eventName == eventName));

                                    IEnumerable<string> eventValues = sameNames.Select(x => x.eventValue).Distinct();
                                    foreach (string eventValue in eventValues.ToList())
                                    {
                                        TimeSpan duration = TimeSpan.Zero;

                                        // Get list of GenEventShiftItem objects that satisfy all of the filters
                                        List<GenEventShiftItem> items = sameNames.FindAll(x => x.eventValue == eventValue);
                                        if (items != null)
                                        {
                                            int eventNumVal = 0;

                                            foreach (GenEventShiftItem item in items)
                                            {
                                                duration += item.duration;
                                                eventNumVal = item.eventNumVal;
                                            }

                                            GenEventRowInfo geri = new GenEventRowInfo();
                                            geri.columnName = Tools.FormatColumnName(eventName, eventNumVal, eventValue);
                                            geri.seconds = Convert.ToInt32(duration.TotalSeconds);

                                            sri.genEventRowInfos.Add(geri);
                                        }
                                    }
                                //}
                            }
                        }

                        Result.Add(sri);

                    }
                }
            }

            return Result;

        }

    }

    public class GenEventRowInfo
    {
        public string columnName { get; set; }
        public int seconds { get; set; }
    }

}
