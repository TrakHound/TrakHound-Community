// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

using TrakHound.Tools;
using TrakHound_Server.Plugins.GeneratedEvents;

namespace TrakHound_Server.Plugins.Parts
{
    public class PartInfo
    {
        public string Id { get; set; }

        public DateTime Timestamp { get; set; }

        public long Sequence { get; set; }

        public int Count { get; set; }


        public static PartInfo Get(PartCountEvent partCountEvent, GeneratedEvent genEvent, long lastSequence)
        {
            if (genEvent.EventName == partCountEvent.EventName && genEvent.CurrentValue != null &&
                genEvent.CurrentValue.Value == String_Functions.UppercaseFirst(partCountEvent.EventValue.Replace('_', ' ')))
            {
                if (!string.IsNullOrEmpty(partCountEvent.CaptureItemLink))
                {
                    var captureItem = genEvent.CaptureItems.Find(x => x.Name == partCountEvent.CaptureItemLink);
                    if (captureItem != null && captureItem.Sequence > lastSequence)
                    {
                        int count = 0;
                        if (int.TryParse(captureItem.Value, out count))
                        {
                            DateTime timestamp = genEvent.CurrentValue.Timestamp;

                            var info = new PartInfo();
                            info.Id = Guid.NewGuid().ToString();
                            info.Timestamp = timestamp;
                            info.Sequence = captureItem.Sequence;

                            if (partCountEvent.CalculationType == CalculationType.Incremental)
                            {
                                info.Count = count;
                            }
                            else if (partCountEvent.CalculationType == CalculationType.Total)
                            {
                                int previousCount = 0;
                                int.TryParse(captureItem.PreviousValue, out previousCount);

                                // If Part Count is less than stored value then assume
                                // it has been reset and needs to be incremented the entire new amount
                                if (count < previousCount) info.Count = count;
                                else info.Count = count - previousCount;
                            }

                            return info;
                        }
                    }
                }
            }

            return null;
        }
        
        public class SequenceInfo
        {
            public string UniqueId { get; set; }

            public long Sequence { get; set; }
        }
    }
}
