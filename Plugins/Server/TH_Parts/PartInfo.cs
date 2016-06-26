// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

using TH_Configuration;
using TH_GeneratedData.GeneratedEvents;
using TH_Shifts;

using TH_Global.Functions;

namespace TH_Parts
{
    public class PartInfo
    {
        public string ShiftId { get; set; }

        public string Id { get; set; }

        public DateTime Timestamp { get; set; }

        public int Count { get; set; }


        public static PartInfo Get(DeviceConfiguration config, GeneratedEvent genEventItem)
        {
            var pc = PartsConfiguration.Get(config);
            if (pc != null)
            {
                if (genEventItem.EventName == pc.PartsEventName &&
                genEventItem.Value == String_Functions.UppercaseFirst(pc.PartsEventValue.Replace('_', ' ')))
                {
                    var captureItem = genEventItem.CaptureItems.Find(x => x.name == pc.PartsCaptureItemLink);
                    if (captureItem != null)
                    {
                        int count = 0;
                        if (int.TryParse(captureItem.value, out count))
                        {
                            DateTime timestamp = genEventItem.Timestamp;

                            string shiftId = null;
                            var shiftInfo = CurrentShiftInfo.Get(config, timestamp);
                            if (shiftInfo != null) shiftId = shiftInfo.id;

                            var info = new PartInfo();
                            info.ShiftId = shiftId;
                            info.Id = String_Functions.RandomString(80);
                            info.Timestamp = timestamp;

                            if (pc.CalculationType == CalculationType.Incremental)
                            {
                                info.Count = count;
                            }
                            else if (pc.CalculationType == CalculationType.Total)
                            {
                                int previousCount = 0;
                                int.TryParse(captureItem.previous_value, out previousCount);

                                info.Count = count - previousCount;
                            }

                            return info;
                        }
                    }
                }
            }

            return null;
        }
    }
}
