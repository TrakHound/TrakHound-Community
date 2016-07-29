using System;
using Newtonsoft.Json;

using TrakHound.Tools.Web;

namespace TrakHound.API
{
    public static partial class Bugs
    {
        public class BugInfo
        {
            public BugInfo() { }

            public BugInfo(Exception ex)
            {
                Subject = ex.Message;

                var exceptionInfo = new ExceptionInfo(ex);
                string json = JSON.FromObject(exceptionInfo);
                Message = json;

                Timestamp = DateTime.UtcNow;
            }

            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("type")]
            public int Type { get; set; }

            [JsonProperty("application")]
            public string Application { get; set; }

            [JsonProperty("subject")]
            public string Subject { get; set; }

            [JsonProperty("message")]
            public string Message { get; set; }

            [JsonProperty("timestamp")]
            public DateTime Timestamp { get; set; }


            private class ExceptionInfo
            {
                public ExceptionInfo(Exception ex)
                {
                    Source = ex.Source;
                    StackTrace = ex.StackTrace;
                    if (ex.TargetSite != null) TargetSite = ex.TargetSite.Name;
                    HelpLink = ex.HelpLink;
                    if (ex.InnerException != null) InnerException = new ExceptionInfo(ex.InnerException);
                }

                [JsonProperty("source")]
                public string Source { get; set; }

                [JsonProperty("stack_trace")]
                public string StackTrace { get; set; }

                [JsonProperty("target_site")]
                public string TargetSite { get; set; }

                [JsonProperty("help_link")]
                public string HelpLink { get; set; }

                [JsonProperty("inner_exception")]
                public ExceptionInfo InnerException { get; set; }
            }
        }
    }
}
