// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TrakHound.API
{
    public partial class Data
    {
        public class CyclesInfo
        {
            public CyclesInfo()
            {
                Cycles = new List<CycleInfo>();
            }

            public class CycleInfo
            {
                [JsonProperty("cycle_id")]
                public string CycleId { get; set; }

                [JsonProperty("instance_id")]
                public string InstanceId { get; set; }

                [JsonProperty("shift_id")]
                public string ShiftId { get; set; }

                [JsonProperty("cycle_name")]
                public string CycleName { get; set; }

                [JsonProperty("cycle_event")]
                public string CycleEvent { get; set; }

                [JsonProperty("start")]
                public DateTime Start { get; set; }

                [JsonProperty("end")]
                public DateTime End { get; set; }

                [JsonProperty("duration")]
                public double Duration { get; set; }
            }

            [JsonProperty("cycles")]
            public List<CycleInfo> Cycles { get; set; }

        }
    }   
}
