// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Newtonsoft.Json;

namespace TH_Global.TrakHound
{
    public static partial class Data
    {
        public class DescriptionInfo
        {
            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("device_id")]
            public string DeviceId { get; set; }

            [JsonProperty("manufacturer")]
            public string Manufacturer { get; set; }

            [JsonProperty("model")]
            public string Model { get; set; }

            [JsonProperty("serial")]
            public string Serial { get; set; }

            [JsonProperty("controller")]
            public string Controller { get; set; }

            [JsonProperty("image_url")]
            public string ImageUrl { get; set; }

            [JsonProperty("logo_url")]
            public string LogoUrl { get; set; }
        }
    }
}
