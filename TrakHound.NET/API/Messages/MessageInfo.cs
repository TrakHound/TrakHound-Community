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
