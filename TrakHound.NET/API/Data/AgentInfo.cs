// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using Newtonsoft.Json;

namespace TrakHound.API
{
    public static partial class Data
    {
        public class AgentInfo
        {
            /// <summary>
            /// MTConnect Agent Address
            /// </summary>
            [JsonProperty("address")]
            public string Address { get; set; }

            /// <summary>
            /// MTConnect Agent Port
            /// </summary>
            [JsonProperty("port")]
            public int Port { get; set; }

            /// <summary>
            /// MTConnect Device Name
            /// </summary>
            [JsonProperty("device_name")]
            public string DeviceName { get; set; }

            /// <summary>
            /// MTConnect Heartbeat
            /// </summary>
            [JsonProperty("heartbeat")]
            public int Heartbeat { get; set; }

            /// <summary>
            /// MTConnect Agent Proxy Address
            /// </summary>
            [JsonProperty("proxy_address")]
            public string ProxyAddress { get; set; }

            /// <summary>
            /// MTConnect Agent Proxy Port
            /// </summary>
            [JsonProperty("proxy_port")]
            public int ProxyPort { get; set; }
        }
    }
}
