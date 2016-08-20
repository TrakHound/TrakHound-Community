// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Newtonsoft.Json;

namespace TrakHound.API
{
    public static partial class Messages
    {
        public class MessageInfo
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("type")]
            public MessageType Type { get; set; }

            [JsonProperty("subject")]
            public string Subject { get; set; }

            [JsonProperty("message")]
            public string Message { get; set; }

            [JsonProperty("sent_timestamp")]
            public DateTime? SentTimestamp { get; set; }

            [JsonProperty("read_timestamp")]
            public DateTime? ReadTimestamp { get; set; }
        }
    }

}
