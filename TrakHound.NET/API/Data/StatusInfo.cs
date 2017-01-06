// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

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


            // Run Timers -------------------------

            [JsonProperty("day_run")]
            public double DayRun { get; set; }

            [JsonProperty("day_operating")]
            public double DayOperating { get; set; }

            [JsonProperty("day_cutting")]
            public double DayCutting { get; set; }

            [JsonProperty("day_spindle")]
            public double DaySpindle { get; set; }


            [JsonProperty("total_run")]
            public double TotalRun { get; set; }

            [JsonProperty("total_operating")]
            public double TotalOperating { get; set; }

            [JsonProperty("total_cutting")]
            public double TotalCutting { get; set; }

            [JsonProperty("total_spindle")]
            public double TotalSpindle { get; set; }

            // ----------------------------------
        }
    }
}
