// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

using TH_Configuration;
using TH_InstanceTable;

using TH_GeneratedData.GeneratedEvents;

namespace TH_GeneratedData
{
    public class GeneratedEventItem
    {
        public GeneratedEventItem() { CaptureItems = new List<CaptureItem>(); }

        public string EventName { get; set; }

        public DateTime Timestamp { get; set; }
        public string Value { get; set; }
        public int Numval { get; set; }
        public string PreviousValue { get; set; }
        public int PreviousNumval { get; set; }

        public List<CaptureItem> CaptureItems { get; set; }

        public static List<GeneratedEventItem> Process(Configuration config, List<InstanceData> instanceDatas)
        {
            var result = new List<GeneratedEventItem>();

            // Get GenDataConfiguration object from Configuration.CustomClasses List (if exists)
            var gdc = GeneratedDataConfiguration.Get(config);
            if (gdc != null)
            {
                // Loop through each InstanceData object in instanceDatas
                foreach (InstanceData instanceData in instanceDatas)
                {
                    // Loop through all of the Events and process Event using instanceData object
                    foreach (Event e in gdc.GeneratedEventsConfiguration.Events)
                    {
                        Return eventReturn = e.Process(instanceData);

                        e.PreviousValue = e.CurrentValue;

                        e.CurrentValue = eventReturn;

                        if (e.PreviousValue != null)
                        {
                            if (e.CurrentValue.Value != e.PreviousValue.Value)
                            {
                                TimeSpan ts = e.CurrentValue.TimeStamp - e.PreviousValue.TimeStamp;

                                eventReturn.Duration = ts.TotalSeconds;
                            }
                        }

                        GeneratedEventItem gei = new GeneratedEventItem();
                        gei.EventName = e.Name;
                        gei.Timestamp = e.CurrentValue.TimeStamp;
                        gei.Value = e.CurrentValue.Value;
                        gei.Numval = e.CurrentValue.NumVal;

                        gei.CaptureItems.AddRange(e.CurrentValue.CaptureItems);

                        if (e.PreviousValue != null)
                        {
                            gei.PreviousValue = e.PreviousValue.Value;
                            gei.PreviousNumval = e.PreviousValue.NumVal;

                            // Just add all of the events even if they are the same (that way variables for a User Interface
                            // are able to be seen (and graphed) in real time even if they dont change for a while)
                            result.Add(gei);
                        }
                    }
                }
            }

            return result;
        }
    }
}
