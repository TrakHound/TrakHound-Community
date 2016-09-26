// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

using TrakHound.Configurations;
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


        public static PartInfo Get(DeviceConfiguration config, GeneratedEvent genEvent)
        {
            var pc = Configuration.Get(config);
            if (pc != null)
            {
                if (genEvent.EventName == pc.PartsEventName && genEvent.CurrentValue != null &&
                genEvent.CurrentValue.Value == String_Functions.UppercaseFirst(pc.PartsEventValue.Replace('_', ' ')))
                {
                    DateTime timestamp = genEvent.CurrentValue.Timestamp;

                    var info = new PartInfo();
                    info.Id = Guid.NewGuid().ToString();
                    info.Timestamp = timestamp;

                    foreach (var captureItem in genEvent.CaptureItems)
                    {
                        int count = 0;
                        if (int.TryParse(captureItem.Value, out count))
                        {
                            info.Sequence = captureItem.Sequence;

                            if (pc.CalculationType == CalculationType.Incremental)
                            {
                                info.Count += count;
                            }
                            else if (pc.CalculationType == CalculationType.Total)
                            {
                                int previousCount = 0;
                                int.TryParse(captureItem.PreviousValue, out previousCount);

                                info.Count += count - previousCount;
                            } 
                        }
                    }

                    return info;
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
