// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MTConnect.Application.Streams;

using TrakHound.Configurations;
using TrakHound.Shifts;
using TrakHound.Tools;

namespace TrakHound.Server.Plugins.Shifts
{
    public class ShiftRowInfo
    {
        public ShiftRowInfo()
        {
            GenEventRowInfos = new List<GenEventRowInfo>();
        }

        //public string Id { get; set; }
        public ShiftId Id { get; set; }

        public ShiftDate Date { get; set; }
        public string Shift { get; set; }

        public int SegmentId { get; set; }

        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public DateTime StartUtc { get; set; }
        public DateTime EndUtc { get; set; }

        //public ShiftTime Start { get; set; }
        //public ShiftTime End { get; set; }

        //public ShiftTime StartUtc { get; set; }
        //public ShiftTime EndUtc { get; set; }

        public string Type { get; set; }

        public double TotalTime { get; set; }

        public List<GenEventRowInfo> GenEventRowInfos { get; set; }

        public ShiftRowInfo Copy()
        {
            ShiftRowInfo Result = new ShiftRowInfo();

            Result.Id = Id;
            Result.Date = Date;
            Result.Shift = Shift;
            Result.SegmentId = SegmentId;
            Result.Start = Start;
            Result.End = End;
            Result.StartUtc = StartUtc;
            Result.EndUtc = EndUtc;
            Result.Type = Type;
            Result.GenEventRowInfos = GenEventRowInfos;

            return Result;
        }

        //public static List<ShiftRowInfo> Get(DeviceConfiguration config, List<GenEventShiftItem> genEventShiftItems, ReturnData currentData)
        //{
        //    var result = new List<ShiftRowInfo>();

        //    var sc = ShiftConfiguration.Get(config);

        //    IEnumerable<ShiftDate> shiftDates = genEventShiftItems.Select(x => x.shiftDate).Distinct();
        //    foreach (ShiftDate shiftDate in shiftDates)
        //    {
        //        List<GenEventShiftItem> sameDates = genEventShiftItems.FindAll(x => x.shiftDate == shiftDate);

        //        IEnumerable<string> shiftNames = sameDates.Select(x => x.shiftName).Distinct();
        //        foreach (string shiftName in shiftNames)
        //        {
        //            List<GenEventShiftItem> sameShifts = sameDates.FindAll(x => x.shiftName == shiftName);

        //            foreach (var item in sameShifts)
        //            {
        //                var segment = item.segment;

        //                var sri = new ShiftRowInfo();
        //                sri.Id = new ShiftId(Tools.GetShiftId(shiftDate, segment));
        //                sri.Date = shiftDate;
        //                sri.Shift = shiftName;
        //                sri.SegmentId = segment.id;
        //                sri.Type = segment.type;

        //                sri.Start = Tools.GetDateTimeFromShiftTime(segment.beginTime, shiftDate, segment.beginDayOffset);
        //                sri.End = Tools.GetDateTimeFromShiftTime(segment.endTime, shiftDate, segment.endDayOffset);

        //                //sri.StartUtc = item.start_timestamp.ToUniversalTime();
        //                //sri.EndUtc = item.end_timestamp.ToUniversalTime();

        //                double totalTime = 0;

        //                if (sc != null)
        //                {
        //                    IEnumerable<string> eventNames = genEventShiftItems.Select(x => x.eventName).Distinct();
        //                    foreach (string eventName in eventNames.ToList())
        //                    {
        //                        List<GenEventShiftItem> sameNames = genEventShiftItems.FindAll(x => (x.shiftDate == shiftDate &&
        //                                x.shiftName == shiftName &&
        //                                x.segment == segment &&
        //                                x.eventName == eventName));

        //                        totalTime = 0;

        //                        IEnumerable<string> eventValues = sameNames.Select(x => x.eventValue).Distinct();
        //                        foreach (string eventValue in eventValues.ToList())
        //                        {
        //                            TimeSpan duration = TimeSpan.Zero;

        //                            // Get list of GenEventShiftItem objects for this Event
        //                            List<GenEventShiftItem> genItems = sameNames.FindAll(x => x.eventValue == eventValue);
        //                            if (genItems != null)
        //                            {
        //                                int eventNumVal = 0;

        //                                foreach (GenEventShiftItem genItem in genItems)
        //                                {
        //                                    duration += genItem.duration;
        //                                    eventNumVal = genItem.eventNumVal;
        //                                }

        //                                var geri = new GenEventRowInfo();
        //                                geri.ColumnName = Tools.FormatColumnName(eventName, eventNumVal, eventValue);
        //                                geri.EventName = FormatEventName(eventName);
        //                                geri.EventValue = eventValue;
        //                                geri.EventNumValue = eventNumVal;
        //                                geri.Seconds = duration.TotalSeconds;

        //                                totalTime += geri.Seconds;

        //                                sri.GenEventRowInfos.Add(geri);
        //                            }
        //                        }
        //                    }
        //                }

        //                sri.TotalTime = totalTime;

        //                result.Add(sri);
        //            }
        //        }
        //    }

        //    return result;
        //}

        public static List<ShiftRowInfo> Get(DeviceConfiguration config, List<GenEventShiftItem> genEventShiftItems, ReturnData currentData)
        {
            var result = new List<ShiftRowInfo>();

            var sc = ShiftConfiguration.Get(config);

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
                        var sri = new ShiftRowInfo();
                        //sri.Id = Tools.GetShiftId(shiftDate, segment);
                        sri.Id = new ShiftId(Tools.GetShiftId(shiftDate, segment));
                        sri.Date = shiftDate;
                        sri.Shift = shiftName;
                        sri.SegmentId = segment.id;



                        sri.Start = Tools.GetDateTimeFromShiftTime(segment.beginTime, shiftDate, segment.beginTime.dayOffset);
                        sri.End = Tools.GetDateTimeFromShiftTime(segment.endTime, shiftDate, segment.endTime.dayOffset);


                        //sri.Start = new DateTime(shiftDate.Source.Year, shiftDate.Source.Month, shiftDate.Source.Day, segment.beginTime.hour, segment.beginTime.minute, segment.beginTime.second);
                        //sri.End = new DateTime(shiftDate.Source.Year, shiftDate.Source.Month, shiftDate.Source.Day, segment.endTime.hour, segment.endTime.minute, segment.endTime.second);

                        //sri.StartUtc = new DateTime(shiftDate.Source.Year, shiftDate.Source.Month, shiftDate.Source.Day, segment.beginTime.hour, segment.beginTime.minute, segment.beginTime.second);
                        //sri.EndUtc = new DateTime(shiftDate.Source.Year, shiftDate.Source.Month, shiftDate.Source.Day, segment.beginTime.hour, segment.beginTime.minute, segment.beginTime.second);



                        // Create new ShiftDate object that gets the actual date and not the shift adjusted date
                        //var date = new ShiftDate(shiftDate.Source);

                        //sri.Start = Tools.GetDateTimeFromShiftTime(segment.beginTime, date, 0);
                        //sri.End = Tools.GetDateTimeFromShiftTime(segment.endTime, date, 0);

                        //sri.StartUtc = Tools.GetDateTimeFromShiftTime(segment.beginTime.ToUTC(), date, 0);
                        //sri.EndUtc = Tools.GetDateTimeFromShiftTime(segment.endTime.ToUTC(), date, 0);

                        sri.Type = segment.type;

                        double totalTime = 0;

                        if (sc != null)
                        {
                            IEnumerable<string> eventNames = genEventShiftItems.Select(x => x.eventName).Distinct();
                            foreach (string eventName in eventNames.ToList())
                            {
                                List<GenEventShiftItem> sameNames = genEventShiftItems.FindAll(x => (x.shiftDate == shiftDate &&
                                        x.shiftName == shiftName &&
                                        x.segment == segment &&
                                        x.eventName == eventName));

                                totalTime = 0;

                                IEnumerable<string> eventValues = sameNames.Select(x => x.eventValue).Distinct();
                                foreach (string eventValue in eventValues.ToList())
                                {
                                    TimeSpan duration = TimeSpan.Zero;

                                    // Get list of GenEventShiftItem objects for this Event
                                    List<GenEventShiftItem> items = sameNames.FindAll(x => x.eventValue == eventValue);
                                    if (items != null)
                                    {
                                        int eventNumVal = 0;

                                        foreach (GenEventShiftItem item in items)
                                        {
                                            duration += item.duration;
                                            eventNumVal = item.eventNumVal;
                                        }

                                        var geri = new GenEventRowInfo();
                                        geri.ColumnName = Tools.FormatColumnName(eventName, eventNumVal, eventValue);
                                        geri.EventName = FormatEventName(eventName);
                                        geri.EventValue = eventValue;
                                        geri.EventNumValue = eventNumVal;
                                        geri.Seconds = duration.TotalSeconds;

                                        totalTime += geri.Seconds;

                                        sri.GenEventRowInfos.Add(geri);
                                    }
                                }
                            }
                        }

                        sri.TotalTime = totalTime;

                        result.Add(sri);
                    }
                }
            }

            return result;
        }

        private static string FormatEventName(string eventName)
        {
            string[] words = eventName.Split('_');
            if (words != null && words.Length > 0)
            {
                var builder = new StringBuilder();

                for (var i = 0; i < words.Length; i++)
                {
                    builder.Append(String_Functions.UppercaseFirst(words[i]));
                    if (i < words.Length - 1) builder.Append(" ");
                }

                return builder.ToString();
            }

            return eventName;
        }
    }

    
    public class GenEventRowInfo
    {
        public string EventName { get; set; }
        public string EventValue { get; set; }
        public int EventNumValue { get; set; }

        public string ColumnName { get; set; }
        public double Seconds { get; set; }
    }
}
