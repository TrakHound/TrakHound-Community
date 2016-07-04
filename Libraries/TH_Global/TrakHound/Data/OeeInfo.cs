// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Newtonsoft.Json;

namespace TH_Global.TrakHound
{
    public static partial class Data
    {
        public class OeeInfo
        {
            [JsonProperty("oee")]
            public double Oee { get; set; }

            [JsonProperty("availability")]
            public double Availability { get; set; }

            [JsonProperty("performance")]
            public double Performance { get; set; }

            [JsonProperty("quality")]
            public double Quality { get; set; }
        }
    }
}
