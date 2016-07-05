// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Xml;
using System.Reflection;

using TH_Global.TrakHound.Configurations;

namespace TH_Shifts
{
    public class ShiftConfiguration
    {
        public ShiftConfiguration()
        {
            shifts = new List<Shift>();
            generatedEvents = new List<GeneratedEventConfiguration>();
        }

        public List<Shift> shifts;

        public List<GeneratedEventConfiguration> generatedEvents;


        public static ShiftConfiguration Read(XmlDocument configXML)
        {
            var result = new ShiftConfiguration();

            XmlNodeList nodes = configXML.SelectNodes("//Shifts");
            if (nodes != null && nodes.Count == 0) nodes = configXML.SelectNodes("//ShiftData");
            if (nodes != null)
            {
                if (nodes.Count > 0)
                {
                    XmlNode node = nodes[0];

                    foreach (XmlNode child in node.ChildNodes)
                    {
                        if (child.NodeType == XmlNodeType.Element && child.Name.ToLower() == "shift")
                        {
                            result.shifts.Add(ProcessShift(child));

                            //Type Setting = typeof(ShiftConfiguration);
                            //PropertyInfo info = Setting.GetProperty(Child.Name);

                            //if (info != null)
                            //{
                            //    Type t = info.PropertyType;
                            //    info.SetValue(result, Convert.ChangeType(Child.InnerText, t), null);
                            //}
                            //else
                            //{
                            //    switch (Child.Name.ToLower())
                            //    {
                            //        case "shifts":

                            //            foreach (XmlNode shiftNode in Child.ChildNodes)
                            //            {
                            //                if (shiftNode.Name.ToLower() == "shift")
                            //                {
                            //                    if (shiftNode.NodeType == XmlNodeType.Element)
                            //                    {
                            //                        result.shifts.Add(ProcessShift(shiftNode));
                            //                    }
                            //                }
                            //            }

                            //            break;

                            //        case "generatedevents":

                            //            foreach (XmlNode eventNode in Child.ChildNodes)
                            //            {

                            //                if (eventNode.Name.ToLower() == "event")
                            //                {
                            //                    if (eventNode.NodeType == XmlNodeType.Element)
                            //                    {
                            //                        result.generatedEvents.Add(ProcessGeneratedEvent(eventNode));
                            //                    }
                            //                }

                            //            }

                            //            break;
                            //    }
                            //}
                        }
                    }
                }
            }

            return result;
        }

        public static ShiftConfiguration Get(DeviceConfiguration configuration)
        {
            ShiftConfiguration result = null;

            var customClass = configuration.CustomClasses.Find(x => x.GetType() == typeof(ShiftConfiguration));
            if (customClass != null) result = (ShiftConfiguration)customClass;

            return result;
        }

        #region "Private"

        static Shift ProcessShift(XmlNode node)
        {
            Shift result = null;

            if (node.Attributes != null)
            {
                if (node.Attributes["name"] != null &&
                    node.Attributes["id"] != null &&
                    node.Attributes["begintime"] != null &&
                    node.Attributes["endtime"] != null
                    )
                {

                    result = new Shift();

                    int id;
                    int.TryParse(node.Attributes["id"].Value, out id);
                    result.id = id;

                    result.name = node.Attributes["name"].Value;

                    DateTime beginTime = DateTime.MinValue;
                    if (DateTime.TryParse(node.Attributes["begintime"].Value, out beginTime))
                    {
                        result.beginTime = new ShiftTime(beginTime, false);
                    }

                    DateTime endTime = DateTime.MinValue;
                    if (DateTime.TryParse(node.Attributes["endtime"].Value, out endTime))
                    {
                        result.endTime = new ShiftTime(endTime, false);
                    }

                    foreach (XmlNode child in node.ChildNodes)
                    {
                        if (child.NodeType == XmlNodeType.Element)
                        {
                            switch (child.Name.ToLower())
                            {
                                case "segments":

                                    result.breaks = ProcessSegments(child, result);

                                    break;

                                case "days":

                                    result.days = ProcessDays(child);

                                    break;
                            }
                        }
                    }

                    result.segments = CreateSegments(result);
                }
            }

            return result;
        }

        static List<Segment> ProcessSegments(XmlNode node, Shift shift)
        {
            var result = new List<Segment>();

            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element && child.Attributes != null)
                {
                    if (child.Attributes["id"] != null && child.Attributes["begintime"] != null && child.Attributes["endtime"] != null)
                    {
                        if (child.Name == "Work")
                        {
                            var sc = new Segment();
                            int id;
                            int.TryParse(child.Attributes["id"].Value, out id);
                            sc.id = id;
                            sc.type = child.Name;
                            sc.shift = shift;

                            ShiftTime begin;
                            ShiftTime end;

                            if (ShiftTime.TryParse(child.Attributes["begintime"].Value, out begin) &&
                            ShiftTime.TryParse(child.Attributes["endtime"].Value, out end))
                            {
                                sc.beginTime = begin;
                                sc.endTime = end;

                                result.Add(sc);
                            }
                        }
                    }
                }
            }

            //// Save Segments
            //List<Segment> segments = CreateSegments(s);

            //foreach (Segment segment in segments)
            //{
            //    string segadr = adr + "/Segments/" + String_Functions.UppercaseFirst(segment.type) + "||" + segment.id.ToString("00");

            //    string segattr = "";
            //    segattr += "id||" + segment.id.ToString("00") + ";";
            //    //segattr += "begintime||" + segment.begintime.To24HourString() + ";";
            //    //segattr += "endtime||" + segment.endtime.To24HourString() + ";";
            //    segattr += "begintime||" + segment.begintime.ToFullString() + ";";
            //    segattr += "endtime||" + segment.endtime.ToFullString() + ";";

            //    //Table_Functions.UpdateTableValue(null, segattr, segadr, dt);
            //    DataTable_Functions.UpdateTableValue(dt, "address", segadr, "attributes", segattr);
            //}

            return result;
        }

        static List<Segment> CreateSegments(Shift shift)
        {
            var result = new List<Segment>();

            var interval = new ShiftTime();
            interval.minute = 5;

            ShiftTime begintime = shift.beginTime.Copy();
            ShiftTime endtime = shift.endTime.Copy();
            //if (endtime < begintime) endtime.dayOffset = begintime.dayOffset + 1;
            if (endtime < begintime) endtime.hour += 24;

            ShiftTime time = begintime.Copy();

            int nextBreakIndex = 0;

            List<Segment> breaks = shift.breaks;
            if (breaks != null)
            {
                // Make sure the Segments(breaks) have values set
                List<Segment> sortedBreaks = new List<Segment>();
                foreach (Segment b in breaks) if (b.beginTime != null || b.endTime != null) sortedBreaks.Add(b);

                sortedBreaks.Sort((a, b) => a.beginTime.CompareTo(b.beginTime));

                breaks = sortedBreaks;
            }

            int id = 0;

            while (time < endtime)
            {
                ShiftTime prev = time.Copy();

                bool breakfound = false;
                string type = "Work";

                if (breaks != null)
                {
                    if (nextBreakIndex < breaks.Count)
                    {
                        // If break is same hour but less than 'time' (ex. time=11:00 and break.begintime=11:30)
                        if (time.hour == breaks[nextBreakIndex].beginTime.hour && time < breaks[nextBreakIndex].beginTime)
                        {
                            time = breaks[nextBreakIndex].beginTime;
                            breakfound = true;
                        }
                        else if (time >= breaks[nextBreakIndex].beginTime)
                        {
                            time = breaks[nextBreakIndex].endTime;
                            nextBreakIndex += 1;
                            breakfound = true;
                            type = "Break";
                        }
                    }
                }

                if (!breakfound)
                {
                    time = time.AddShiftTime(interval);

                    if (time > endtime) time = endtime.Copy();
                }

                var segment = new Segment();
                segment.beginTime = prev.Copy();
                segment.endTime = time.Copy();
                segment.type = type;
                segment.id = id;
                segment.shift = shift;
                result.Add(segment);

                id += 1;
            }

            return result;
        }

        static List<DayOfWeek> ProcessDays(XmlNode node)
        {
            List<DayOfWeek> Result = new List<DayOfWeek>();

            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element && child.InnerText != null)
                {
                    if (child.Name.ToLower() == "day")
                    {
                        switch (child.InnerText.ToLower())
                        {
                            case "sunday":
                            case "sun": Result.Add(DayOfWeek.Sunday); break;
                            case "monday":
                            case "mon": Result.Add(DayOfWeek.Monday); break;
                            case "tuesday":
                            case "tue": Result.Add(DayOfWeek.Tuesday); break;
                            case "wednesday":
                            case "wed": Result.Add(DayOfWeek.Wednesday); break;
                            case "thursday":
                            case "thur": Result.Add(DayOfWeek.Thursday); break;
                            case "friday":
                            case "fri": Result.Add(DayOfWeek.Friday); break;
                            case "saturday":
                            case "sat": Result.Add(DayOfWeek.Saturday); break;
                        }
                    }
                }
            }

            return Result;
        }

        static GeneratedEventConfiguration ProcessGeneratedEvent(XmlNode node)
        {
            GeneratedEventConfiguration Result = null;

            if (node.InnerText != null)
            {
                Result = new GeneratedEventConfiguration();
                Result.name = node.InnerText.ToUpper();
            }

            return Result;
        }

        #endregion
    }

    public class Shift
    {
        public Shift()
        {
            segments = new List<Segment>();
            days = new List<DayOfWeek>();
        }

        public int id { get; set; }

        public string name { get; set; }

        public ShiftTime beginTime { get; set; }
        public ShiftTime endTime { get; set; }

        public List<Segment> segments;
        public List<Segment> breaks;

        public List<DayOfWeek> days;
    }

    public class Segment
    {
        public int id { get; set; }

        public string type { get; set; }

        public ShiftTime beginTime { get; set; }
        public ShiftTime endTime { get; set; }

        public Shift shift { get; set; }

        public int beginDayOffset { get; set; }
        public int endDayOffset { get; set; }

        #region "operator overrides"

        public static bool operator ==(Segment sc1, Segment sc2)
        {
            if (ReferenceEquals(sc1, null) && ReferenceEquals(sc2, null)) return true;
            else if (ReferenceEquals(sc1, null) && !ReferenceEquals(sc2, null)) return false;
            else if (!ReferenceEquals(sc1, null) && ReferenceEquals(sc2, null)) return false;
            else if (sc1.id == sc2.id) return true;
            else return false;
        }

        public static bool operator !=(Segment sc1, Segment sc2)
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
