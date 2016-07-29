// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Newtonsoft.Json;

namespace TrakHound.API
{
    public static partial class Data
    {
        public class DeviceInfo
        {
            public DeviceInfo()
            {
                Description = new DescriptionInfo();
                Status = new StatusInfo();
                Controller = new ControllerInfo();
                Oee = new OeeInfo();
                Timers = new TimersInfo();
            }


            [JsonProperty("unique_id")]
            public string UniqueId { get; set; }


            [JsonProperty("description")]
            public DescriptionInfo Description { get; set; }

            [JsonProperty("status")]
            public StatusInfo Status { get; set; }

            [JsonProperty("controller")]
            public ControllerInfo Controller { get; set; }

            [JsonProperty("oee")]
            public OeeInfo Oee { get; set; }

            [JsonProperty("timers")]
            public TimersInfo Timers { get; set; }
        }
    }
}
