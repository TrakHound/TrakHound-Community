// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Newtonsoft.Json;

namespace TH_Global.TrakHound
{
    public static partial class Data
    {
        public class ControllerInfo
        {
            [JsonProperty("availability")]
            public string Availability { get; set; }

            [JsonProperty("controller_mode")]
            public string ControllerMode { get; set; }

            [JsonProperty("emergency_stop")]
            public string EmergencyStop { get; set; }

            [JsonProperty("execution_mode")]
            public string ExecutionMode { get; set; }

            [JsonProperty("system_status")]
            public string SystemStatus { get; set; }

            [JsonProperty("system_message")]
            public string SystemMessage { get; set; }

            [JsonProperty("program_name")]
            public string ProgramName { get; set; }
        }
    }
}
