// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Globalization;
using System.Text;

using TrakHound.Tools.Web;

namespace TrakHound.GitHub
{
    public static class Authentication
    {
        public const string GITHUB_API_URL = "https://api.github.com/";
        public const string AUTHENTICATION_URL = "https://api.github.com/authorizations";

        public class Crendentials
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public static HTTP.HeaderData GetBasicHeader(Crendentials credentials)
        {
            string encoded = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(credentials.Username + ":" + credentials.Password));

            var header = new HTTP.HeaderData();
            header.Id = "Authorization";
            header.Text = "Basic " + encoded;
            return header;
        }

        public static HTTP.HeaderData GetOAuth2Header(string token)
        {
            var header = new HTTP.HeaderData();
            header.Id = "Authorization";
            header.Text = string.Format(CultureInfo.InvariantCulture, "Token {0}", token);
            return header;
        }

    }
}
