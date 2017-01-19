// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using System;
using TrakHound.Tools.Web;

namespace TrakHound.GitHub
{

    public static class Issues
    {
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

        public static void Create(string repositoryUrl, Issue issue, Authentication.Crendentials credentials)
        {
            string format = "\"title\": \"{0}\", \"body\": \"{1}\"";

            string body = ComposeBody(issue);

            string data = string.Format(format, issue.Title, body);
            data = "{ " + data + " }";

            var postData = new HTTP.PostContentData("parameters", data, "application/json");
            var postDatas = new HTTP.PostContentData[1];
            postDatas[0] = postData;

            var headers = new HTTP.HeaderData[1];

            HTTP.POST(repositoryUrl, postDatas, headers, "TrakHound");
        }

        public static bool Create(string repositoryUrl, Issue issue, HTTP.HeaderData loginHeader)
        {
            string format = "\"title\": \"{0}\", \"body\": \"{1}\"";

            string body = ComposeBody(issue);

            string data = string.Format(format, issue.Title, body);
            data = "{ " + data + " }";

            var postData = new HTTP.PostContentData("parameters", data, "application/json");
            var postDatas = new HTTP.PostContentData[1];
            postDatas[0] = postData;

            var headers = new HTTP.HeaderData[1];
            headers[0] = loginHeader;


            return !string.IsNullOrEmpty(HTTP.POST(repositoryUrl, postDatas, headers, "TrakHound"));
        }

        private static string ComposeBody(Issue issue)
        {
            string result = "";

            string n = Environment.NewLine;

            result = issue.Content;

            return result;
        }

    }
}
