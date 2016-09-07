// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Newtonsoft.Json;

namespace TrakHound.API
{
    public static partial class Devices
    {
        public class CheckInfo
        {
            [JsonProperty("unique_id")]
            public string UniqueId { get; set; }

            [JsonProperty("update_id")]
            public string UpdateId { get; set; }

            [JsonProperty("enabled")]
            public bool Enabled { get; set; }
        }
    }
}
