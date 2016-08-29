// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

using TrakHound.Configurations;
using TrakHound_Server.Plugins.Instances;

namespace TrakHound_Server.Plugins.GeneratedEvents
{
    public class GeneratedEvent
    {
        public GeneratedEvent() { CaptureItems = new List<CaptureItem>(); }

        public string EventName { get; set; }

        public class ValueData
        {
            public string Id { get; set; }
            public DateTime Timestamp { get; set; }
            public string Value { get; set; }
            public int Numval { get; set; }

            public ValueData()
            {
                Id = Guid.NewGuid().ToString();
            }

            internal ValueData(Return eventReturn)
            {
                Id = Guid.NewGuid().ToString();

                Value = eventReturn.Value;
                Timestamp = eventReturn.TimeStamp;
                Numval = eventReturn.NumVal;
            }
        }

        public ValueData CurrentValue { get; set; }

        public ValueData PreviousValue { get; set; }


        //public string PreviousId { get; set; }
        //public DateTime PreviousTimestamp { get; set; }
        //public string PreviousValue { get; set; }
        //public int PreviousNumval { get; set; }

        public List<CaptureItem> CaptureItems { get; set; }

        public static List<GeneratedEvent> Process(DeviceConfiguration config, List<InstanceData> instanceDatas)
        {
            var result = new List<GeneratedEvent>();

            // Get Configuration object from Configuration.CustomClasses List (if exists)
            var gec = Configuration.Get(config);
            if (gec != null)
            {
                if (instanceDatas != null)
                {
                    // Loop through each InstanceData object in instanceDatas
                    foreach (var instanceData in instanceDatas)
                    {
                        // Loop through all of the Events and process Event using instanceData object
                        foreach (var e in gec.Events)
                        {
                            var genEvent = ProcessEvent(e, instanceData);
                            result.Add(genEvent);
                        }
                    }
                }
                else
                {
                    foreach (var e in gec.Events)
                    {
                        var genEvent = ProcessEvent(e, null);
                        result.Add(genEvent);
                    }
                }
            }

            return result;
        }

        private static GeneratedEvent ProcessEvent(Event e, InstanceData instanceData)
        {
            var result = new GeneratedEvent();
            result.EventName = e.Name;

            if (e.CurrentValue != null)
            {
                result.CurrentValue = new ValueData(e.CurrentValue);
            }

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

                result.PreviousValue = new ValueData(e.PreviousValue);
            }

            if (e.CurrentValue != null)
            {
                result.CurrentValue = new ValueData(e.CurrentValue);
                result.CaptureItems.AddRange(e.CurrentValue.CaptureItems);
            }

            return result;
        }
    }
}
