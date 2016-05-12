// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Globalization;
using System.Text;

using TH_Global.Web;

namespace TH_GitHub
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
            //string encoded = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(token));

            var header = new HTTP.HeaderData();
            header.Id = "Authorization";
            //header.Text = "Token " + token;
            header.Text = string.Format(CultureInfo.InvariantCulture, "Token {0}", token);
            return header;
        }

        
        //public static string CreateToken(string username)
        //{
        //    string format = "'scopes':['{0}'],'client_id':'{1}','client_secret':'{2}', 'note':'{3}','note_url':'{4}'";

        //    string scopes = "public_repo";
        //    string client_id = "a1178c3ffdfd1adea560";
        //    string client_secret = "acf7e9e80eab5f238271ad8a2e0863025ad326ba";
        //    string note = "TrakHound";
        //    string note_url = "http://www.trakhound.org";

        //    string data = string.Format(format, scopes, client_id, client_secret, note, note_url);
        //    data = "{" + data + "}";

        //    byte[] bytes = Encoding.UTF8.GetBytes(data);

        //    return HTTP.POST(AUTHENTICATION_URL, bytes);
        //}

    }
}
