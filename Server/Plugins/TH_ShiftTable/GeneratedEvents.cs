// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;

using TH_Configuration;
using TH_GeneratedData.GeneratedEvents;

namespace TH_ShiftTable
{
    public class GenEventShiftItem
    {
        public GenEventShiftItem() { CaptureItems = new List<CaptureItem>(); }

        public string eventName { get; set; }
        public string eventValue { get; set; }
        public int eventNumVal { get; set; }

        public string shiftName { get; set; }

        public Segment segment { get; set; }

        public ShiftTime start { get; set; }
        public ShiftTime end { get; set; }

        public DateTime start_timestamp { get; set; }
        public DateTime end_timestamp { get; set; }

        public ShiftDate shiftDate { get; set; }

        public TimeSpan duration { get; set; }

        public List<CaptureItem> CaptureItems;

        public new string ToString()
        {
            return eventName + " = " + eventValue + " : " + duration.ToString() + " : " + start_timestamp.ToString("o") + " - " + end_timestamp.ToString("o") + " :: " + segment.beginTime + " - " + segment.endTime;
        }

        public static List<GenEventShiftItem> Get(Configuration config, List<GeneratedEvent> genEventItems)
        {
            var result = new List<GenEventShiftItem>();

            var sc = ShiftConfiguration.Get(config);
            if (sc != null)
            {
                List<ListInfo> nameInfos = GetListByName(genEventItems);

                var previousData = previousItems.Find(x => x.Id == config.UniqueId);
                if (previousData == null)
                {
                    previousData = new PreviousData();
                    previousData.Id = config.UniqueId;
                    previousItems.Add(previousData);
                }


                foreach (var nameInfo in nameInfos)
                {
                    GeneratedEvent previousItem = previousData.PreviousItems.Find(x => x.EventName.ToLower() == nameInfo.EventName.ToLower());

                    var sortedItems = nameInfo.GenEventItems.OrderBy(x => x.Timestamp).ToList();

                    //for (int i = 0; i <= nameInfo.genEventItems.Count - 1; i++)
                    for (int i = 0; i <= sortedItems.Count - 1; i++)
                    {
                        //var item = nameInfo.genEventItems[i];
                        var item = sortedItems[i];

                        if (previousItem != null)
                        {
                            // Skip items that are not newer (test to see if any real data is lost due to this)
                            if (item.Timestamp > previousItem.Timestamp)
                            {
                                List<GenEventShiftItem> items = GetItems(sc, previousItem, item);
                                result.AddRange(items);
                                previousItem = item;
                            }
                            else
                            {
                                Console.WriteLine(item.Timestamp.ToString("o") + " :: " + previousItem.Timestamp.ToString("o"));
                            }
                        }
                        else
                        {
                            previousData.PreviousItems.Add(item);
                            previousItem = item;
                        }

                        int prevIndex = previousData.PreviousItems.FindIndex(x => x.EventName.ToLower() == nameInfo.EventName.ToLower());
                        if (prevIndex >= 0) previousData.PreviousItems[prevIndex] = previousItem;
                    }
                }
            }

            return result;
        }

        // Used to hold information between Samples

        public class PreviousData
        {
            public PreviousData() { PreviousItems = new List<GeneratedEvent>(); }

            public string Id { get; set; }
            public List<GeneratedEvent> PreviousItems { get; set; }
        }

        public static List<PreviousData> previousItems = new List<PreviousData>();

        #region "Private"

        class ListInfo
        {
            public ListInfo() { GenEventItems = new List<GeneratedEvent>(); }

            //public string Title { get; set; }
            public string EventName { get; set; }
            public List<GeneratedEvent> GenEventItems { get; set; }
        }

        static List<GenEventShiftItem> GetItems(ShiftConfiguration stc, GeneratedEvent item1, GeneratedEvent item2)
        {
            var result = new List<GenEventShiftItem>();

            ShiftDate date1 = new ShiftDate(item1.Timestamp);
            ShiftDate date2 = new ShiftDate(item2.Timestamp);

            int daySpan = date2 - date1;

            for (int x = 0; x <= daySpan; x++)
            {
                DateTime dt = new DateTime(date1.year, date1.month, date1.day);
                ShiftDate date = new ShiftDate(dt.AddDays(x), false);

                foreach (Shift shift in stc.shifts)
                {
                    result.AddRange(GetItemsDuringShift(item1, item2, date, shift));
                }
            }


            // Debug $$$
            //foreach (var item in result) Console.WriteLine(item.eventName + " = " + item.eventValue + " : " + item.duration.ToString() + " : " + item1.Timestamp.ToString("o") + " - " + item2.Timestamp.ToString("o") + " :: " + item.segment.beginTime + " - " + item.segment.endTime);

            return result;
        }

        static List<ListInfo> GetListByName(List<GeneratedEvent> genEventItems)
        {
            var result = new List<ListInfo>();

            // Get a list of all of the distinct (by Event Name) genEventItems
            IEnumerable<string> distinctNames = genEventItems.Select(x => x.EventName).Distinct();

            // Loop through the distinct event names  
            foreach (string distinctName in distinctNames.ToList())
            {
                var info = new ListInfo();
                info.EventName = distinctName;

                info.GenEventItems = genEventItems.FindAll(x => x.EventName.ToLower() == distinctName.ToLower());
                result.Add(info);
            }

            return result;
        }

        static List<GenEventShiftItem> GetItemsDuringShift(GeneratedEvent item1, GeneratedEvent item2, ShiftDate date, Shift shift)
        {
            var result = new List<GenEventShiftItem>();

            foreach (Segment segment in shift.segments)
            {
                GenEventShiftItem gesi = GetItemDuringSegment(item1, item2, date, segment);
                if (gesi != null) result.Add(gesi);
            }

            return result;
        }

        static GenEventShiftItem GetItemDuringSegment(GeneratedEvent item1, GeneratedEvent item2, ShiftDate date, Segment segment)
        {
            GenEventShiftItem result = null;

            // -1 = Timestamp does not fall within segment          
            //  0 = Timestamps span entire segment
            //  1 = Start timestamp is more than segment.beginTime
            //  2 = End timestamp is less than segment.endTime
            //  3 = Both timestamps are within segment
            int type = GetItemDuringSegmentType(item1, item2, segment, date);

            if (type >= 0)
            {
                var gesi = new GenEventShiftItem();
                gesi.eventName = item1.EventName;
                gesi.eventValue = item1.Value;
                gesi.eventNumVal = item1.Numval;

                gesi.shiftName = segment.shift.name;
                gesi.segment = segment;

                gesi.CaptureItems = item1.CaptureItems;

                // Get Times for segment and convert timestamps to Local
                var sst = SegmentShiftTimes.Get(item1.Timestamp, item2.Timestamp, date, segment);

                if (segment.beginTime.dayOffset == segment.endTime.dayOffset) gesi.shiftDate = date - segment.endTime.dayOffset;
                else gesi.shiftDate = date;

                // Calculate Start and End timestamps based on the DuringSegmentType
                switch (type)
                {
                    case 0:
                        gesi.start_timestamp = sst.segmentStart;
                        gesi.end_timestamp = sst.segmentEnd;
                        break;

                    case 1:
                        gesi.start_timestamp = sst.start;
                        gesi.end_timestamp = sst.segmentEnd;
                        break;

                    case 2:
                        gesi.start_timestamp = sst.segmentStart;
                        gesi.end_timestamp = sst.end;
                        break;

                    case 3:
                        gesi.start_timestamp = sst.start;
                        gesi.end_timestamp = sst.end;
                        break;
                }

                // Calculate duration of GeneratedEventShiftItem
                TimeSpan duration = gesi.end_timestamp - gesi.start_timestamp;
                gesi.duration = duration;

                result = gesi;
            }

            return result;
        }

        static int GetItemDuringSegmentType(GeneratedEvent item1, GeneratedEvent item2, Segment segment, ShiftDate date)
        {
            // Get Times for segment and convert timestamps to Local
            SegmentShiftTimes sst = SegmentShiftTimes.Get(item1.Timestamp, item2.Timestamp, date, segment);

            int type = -1;

            // Timestamp does not fall within segment
            if ((sst.start < sst.segmentStart && sst.end < sst.segmentStart) || (sst.start > sst.segmentEnd && sst.end > sst.segmentEnd))
                type = -1;

            // Timestamps span entire segment
            else if (sst.start <= sst.segmentStart && sst.end >= sst.segmentEnd)
                type = 0;

            // Start timestamp is more than segment.beginTime
            else if ((sst.start >= sst.segmentStart && sst.start < sst.segmentEnd) && sst.end > sst.segmentEnd)
                type = 1;

            // End timestamp is less than segment.endTime
            else if (sst.start <= sst.segmentStart && (sst.end < sst.segmentEnd && sst.end > sst.segmentStart))
                type = 2;

            // Both timestamps are within segment
            else if ((sst.start >= sst.segmentStart && sst.start < sst.segmentEnd) && sst.end < sst.segmentEnd)
                type = 3;

            return type;
        }

        #endregion

    }

    public class GeneratedEventConfiguration
    {
        public string name { get; set; }
    }
}
