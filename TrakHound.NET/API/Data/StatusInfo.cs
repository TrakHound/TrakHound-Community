// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Newtonsoft.Json;

namespace TrakHound.API
{
    public static partial class Data
    {
        public class StatusInfo
        {
            [JsonProperty("connected")]
            public int Connected { get; set; }

            [JsonProperty("device_status")]
            public string DeviceStatus { get; set; }

            [JsonProperty("production_status")]
            public string ProductionStatus { get; set; }

            [JsonProperty("device_status_timer")]
            public double DeviceStatusTimer { get; set; }

            [JsonProperty("production_status_timer")]
            public double ProductionStatusTimer { get; set; }

            [JsonProperty("part_count")]
            public int PartCount { get; set; }
        }
    }
}
