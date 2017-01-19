// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using Newtonsoft.Json;
using System;

namespace TrakHound.API
{
    public static partial class Data
    {
        public class OverrideInfo
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("value")]
            public double Value { get; set; }

            [JsonProperty("timestamp")]
            public DateTime Timestamp { get; set; }
        }
    }
}
