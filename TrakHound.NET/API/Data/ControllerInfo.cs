// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using Newtonsoft.Json;

namespace TrakHound.API
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

            [JsonProperty("program_line")]
            public string ProgramLine { get; set; }

            [JsonProperty("program_block")]
            public string ProgramBlock { get; set; }
        }
    }
}
