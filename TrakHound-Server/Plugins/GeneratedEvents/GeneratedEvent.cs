// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

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
            public long Sequence { get; set; }
            public DateTime ChangedTimestamp { get; set; }
            public long ChangedSequence { get; set; }
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
                Sequence = eventReturn.Sequence;
                ChangedTimestamp = eventReturn.ChangedTimeStamp;
                ChangedSequence = eventReturn.ChangedSequence;
                Numval = eventReturn.NumVal;
            }
        }

        public ValueData CurrentValue { get; set; }

        public ValueData PreviousValue { get; set; }

        public List<CaptureItem> CaptureItems { get; set; }

        public TimeSpan Duration
        {
            get
            {
                if (CurrentValue != null && PreviousValue != null)
                {
                    if (CurrentValue.Timestamp > DateTime.MinValue && PreviousValue.Timestamp > DateTime.MinValue)
                    {
                        return CurrentValue.Timestamp - PreviousValue.Timestamp;
                    }
                }

                return TimeSpan.Zero;
            }
        }


        public static List<GeneratedEvent> Process(DeviceConfiguration config, List<Instance> instances)
        {
            var result = new List<GeneratedEvent>();

            // Get Configuration object from Configuration.CustomClasses List (if exists)
            var gec = Configuration.Get(config);
            if (gec != null)
            {
                if (instances != null)
                {
                    var _instances = instances.FindAll(o => o != null);

                    // Loop through each InstanceData object in instanceDatas
                    foreach (var instance in _instances)
                    {
                        // Loop through all of the Events and process Event using instanceData object
                        foreach (var e in gec.Events)
                        {
                            var genEvent = ProcessEvent(e, instance);
                            result.Add(genEvent);
                        }
                    }
                }
            }

            return result;
        }

        private static GeneratedEvent ProcessEvent(Event e, Instance instance)
        {
            // Process Event using InstanceData
            Return eventReturn = e.Process(instance);

            if (e.PreviousValue == null) e.PreviousValue = e.Default.Copy();
            if (e.CurrentValue != null)
            {
                e.PreviousValue = e.CurrentValue.Copy();
                e.PreviousValue.Id = e.CurrentValue.Id;
            }

            if (eventReturn != e.CurrentValue)
            {
                e.CurrentValue = eventReturn.Copy();
                e.CurrentValue.ChangedTimeStamp = instance.Timestamp;
                e.CurrentValue.ChangedSequence = instance.Sequence;
            }

            e.CurrentValue.TimeStamp = instance.Timestamp;
            e.CurrentValue.Sequence = instance.Sequence;

            // Set Duration
            if (e.CurrentValue.TimeStamp > DateTime.MinValue && e.PreviousValue.TimeStamp > DateTime.MinValue)
            {
                eventReturn.Duration = (e.CurrentValue.TimeStamp - e.PreviousValue.TimeStamp).TotalSeconds;
            }
            else
            {
                eventReturn.Duration = 0;
            }

            // Create new GeneratedEvent object
            var result = new GeneratedEvent();
            result.EventName = e.Name;
            result.PreviousValue = new ValueData(e.PreviousValue);
            result.CurrentValue = new ValueData(e.CurrentValue);
            result.CaptureItems.AddRange(e.CurrentValue.CaptureItems);

            return result;
        }
    }
}
