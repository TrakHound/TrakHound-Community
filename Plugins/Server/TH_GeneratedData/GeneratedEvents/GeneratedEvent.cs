// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

using TH_Configuration;
using TH_InstanceData;

namespace TH_GeneratedData.GeneratedEvents
{
    public class GeneratedEvent
    {
        public GeneratedEvent() { CaptureItems = new List<CaptureItem>(); }

        public string EventName { get; set; }

        public DateTime Timestamp { get; set; }
        public string Value { get; set; }
        public int Numval { get; set; }
        public string PreviousValue { get; set; }
        public int PreviousNumval { get; set; }

        public List<CaptureItem> CaptureItems { get; set; }

        public static List<GeneratedEvent> Process(DeviceConfiguration config, List<InstanceData> instanceDatas)
        {
            var result = new List<GeneratedEvent>();

            // Get GenDataConfiguration object from Configuration.CustomClasses List (if exists)
            var gec = GeneratedEventsConfiguration.Get(config);
            if (gec != null)
            {
                // Loop through each InstanceData object in instanceDatas
                foreach (var instanceData in instanceDatas)
                {
                    // Loop through all of the Events and process Event using instanceData object
                    foreach (var e in gec.Events)
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

                        var gei = new GeneratedEvent();
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
