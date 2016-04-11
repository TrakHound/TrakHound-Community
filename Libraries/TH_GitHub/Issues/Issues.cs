using System;
using System.Text;
using TH_Global.Web;

namespace TH_GitHub
{

    public static class Issues
    {
        public const string REPOSITORY_URL = "https://api.github.com/repos/TrakHound/TrakHound/issues";

        public enum IssueType
        {
            Exception,
            UserSubmitted
        }

        public class Issue
        {
            public DateTime Timestamp { get; set; }

            public string Title { get; set; }
            public string Content { get; set; }
            public IssueType Type { get; set; }

            // Optional Information
            public string Comments { get; set; }

            // Credentials
            public string Username { get; set; }
            public string Token { get; set; }
        }

        public static void Create(Issue issue, Authentication.Crendentials credentials)
        {
            string format = "\"title\": \"{0}\", \"body\": \"{1}\"";

            string body = ComposeBody(issue);

            string postData = string.Format(format, issue.Title, body);
            postData = "{ " + postData + " }";

            var headers = new HTTP.HeaderData[1];
            headers[0] = Authentication.GetHeaderData(credentials);

            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            HTTP.POST(REPOSITORY_URL, byteArray, headers, "TrakHound");
        }

        private static string ComposeBody(Issue issue)
        {
            string result = "";

            string n = Environment.NewLine;

            //result = issue.Type.ToString() + n + n + issue.Content + n + n + issue.Comments;
            result = issue.Content;

            return result;
        }

    }
}
