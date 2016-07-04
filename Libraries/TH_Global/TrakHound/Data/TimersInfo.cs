// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Newtonsoft.Json;

namespace TH_Global.TrakHound
{
    public static partial class Data
    {
        public class TimersInfo
        {
            [JsonProperty("total")]
            public double Total { get; set; }

            [JsonProperty("active")]
            public double Active { get; set; }

            [JsonProperty("idle")]
            public double Idle { get; set; }

            [JsonProperty("alert")]
            public double Alert { get; set; }
        }
    }
}
