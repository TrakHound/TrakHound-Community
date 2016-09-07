// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Newtonsoft.Json;
using System;

namespace TrakHound.API
{
    public static partial class Data
    {
        public class CycleInfo
        {
            [JsonProperty("cycle_id")]
            public string CycleId { get; set; }

            [JsonProperty("cycle_instance_id")]
            public string CycleInstanceId { get; set; }

            [JsonProperty("cycle_name")]
            public string CycleName { get; set; }

            [JsonProperty("cycle_event")]
            public string CycleEvent { get; set; }

            [JsonProperty("production_type")]
            public string ProductionType { get; set; }

            [JsonProperty("start")]
            public DateTime Start { get; set; }

            [JsonProperty("stop")]
            public DateTime Stop { get; set; }

            [JsonProperty("duration")]
            public double Duration { get; set; }
        }
    }
}
