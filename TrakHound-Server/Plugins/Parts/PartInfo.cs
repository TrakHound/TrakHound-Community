// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

using TrakHound_Server.Plugins.GeneratedEvents;

namespace TrakHound_Server.Plugins.Parts
{

    public class PartInfo
    {
        public string Id { get; set; }

        public DateTime Timestamp { get; set; }

        public long Sequence { get; set; }

        public int Count { get; set; }


        public static PartInfo Process(PartCountEvent partCountEvent, GeneratedEvent gEvent, long _lastSequence)
        {
            if (partCountEvent.ValueType == ValueType.CAPTURE_ITEM)
            {
                return ProcessCaptureItemMethod(partCountEvent, gEvent, _lastSequence);
            }
            else if (partCountEvent.ValueType == ValueType.STATIC_INCREMENT)
            {
                return ProcessStaticIncrementMethod(partCountEvent, gEvent, _lastSequence);
            }

            return null;
        }

        private static PartInfo ProcessCaptureItemMethod(PartCountEvent partCountEvent, GeneratedEvent gEvent, long _lastSequence)
        {
            long sequence = gEvent.CurrentValue.Sequence;

            if (sequence > _lastSequence)
            {
                if (!string.IsNullOrEmpty(partCountEvent.CaptureItemLink))
                {
                    var captureItem = gEvent.CaptureItems.Find(x => x.Name == partCountEvent.CaptureItemLink);
                    if (captureItem != null && captureItem.Sequence > _lastSequence)
                    {
                        int count = 0;
                        int.TryParse(captureItem.Value, out count);
                        if (count > 0)
                        {
                            DateTime timestamp = gEvent.CurrentValue.Timestamp;

                            // Create new PartInfo object
                            var info = new PartInfo();
                            info.Id = Guid.NewGuid().ToString();
                            info.Timestamp = timestamp;
                            info.Sequence = captureItem.Sequence;

                            // Calculate Increment Value based on CalculationType
                            if (partCountEvent.CalculationType == CalculationType.INCREMENTAL)
                            {
                                info.Count = count;
                            }
                            else if (partCountEvent.CalculationType == CalculationType.TOTAL)
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

        private static PartInfo ProcessStaticIncrementMethod(PartCountEvent partCountEvent, GeneratedEvent gEvent, long _lastSequence)
        {
            long sequence = gEvent.CurrentValue.ChangedSequence;

            // Create new PartInfo object
            var info = new PartInfo();
            info.Id = Guid.NewGuid().ToString();
            info.Timestamp = gEvent.CurrentValue.Timestamp;
            info.Sequence = sequence;

            info.Count = partCountEvent.StaticIncrementValue;

            return info;
        }
    }
}
