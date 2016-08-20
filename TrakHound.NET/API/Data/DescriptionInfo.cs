// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Newtonsoft.Json;

namespace TrakHound.API
{
    public static partial class Data
    {
        public class DescriptionInfo
        {
            /// <summary>
            /// General Device Description (ex. 3 Axis VMC)
            /// </summary>
            [JsonProperty("description")]
            public string Description { get; set; }

            /// <summary>
            /// Type of Device (ex. Lathe, Machining Center, Grinder, etc)
            /// </summary>
            [JsonProperty("device_type")]
            public string DeviceType { get; set; }

            /// <summary>
            /// Custom Device Identifier (ex. VMC-02, A2, etc.)
            /// </summary>
            [JsonProperty("device_id")]
            public string DeviceId { get; set; }

            /// <summary>
            /// Name of the Manufacturer of the Device (ex. Okuma, Mazak, etc.)
            /// </summary>
            [JsonProperty("manufacturer")]
            public string Manufacturer { get; set; }

            /// <summary>
            /// Model name of the Device (ex. MULTUS 3000, LT2000, etc.)
            /// </summary>
            [JsonProperty("model")]
            public string Model { get; set; }

            /// <summary>
            /// Serial Number of the Device
            /// </summary>
            [JsonProperty("serial")]
            public string Serial { get; set; }

            /// <summary>
            /// Type of controller the Device has (ex. Fanuc, OSP, Mazatrol, Fagor, etc.)
            /// </summary>
            [JsonProperty("controller")]
            public string Controller { get; set; }

            /// <summary>
            /// Name of the building or cell of the Device
            /// </summary>
            [JsonProperty("location")]
            public string Location { get; set; }


            [JsonProperty("image_url")]
            public string ImageUrl { get; set; }

            [JsonProperty("logo_url")]
            public string LogoUrl { get; set; }
        }
    }
}
