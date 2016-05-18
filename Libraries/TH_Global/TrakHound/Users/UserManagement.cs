// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Specialized;
using System.DirectoryServices;
using System.Linq;
using System.Security.Principal;

using TH_Global.Web;

namespace TH_Global.TrakHound.Users
{
    public static class UserManagement
    {

        public static UserConfiguration BasicLogin(string id, string password)
        {
            UserConfiguration result = null;

            if (id != null && id.Length > 0)
            {
                string url = "https://www.feenux.com/trakhound/api/login/";

                var postDatas = new NameValueCollection();
                postDatas["id"] = id;
                postDatas["password"] = password;
                postDatas["sender_id"] = SenderId.Get();

                string response = HTTP.POST(url, postDatas);
                if (response != null)
                {
                    result = UserConfiguration.Get(response);
                }
            }

            return result;
        }

        public static UserConfiguration CreateTokenLogin(string id, string password, string senderId, string note = "")
        {
            UserConfiguration result = null;

            if (id != null && id.Length > 0)
            {
                string url = "https://www.feenux.com/trakhound/api/login/";

                var postDatas = new NameValueCollection();
                postDatas["id"] = id;
                postDatas["password"] = password;
                postDatas["remember"] = "1";
                postDatas["sender_id"] = senderId;
                postDatas["note"] = note;

                string response = HTTP.POST(url, postDatas);
                if (response != null)
                {
                    result = UserConfiguration.Get(response);
                }
            }

            return result;
        }

        public static UserConfiguration TokenLogin(string token)
        {
            UserConfiguration result = null;

            if (token != null && token.Length > 0)
            {
                string senderId = SenderId.Get();

                string url = "https://www.feenux.com/trakhound/api/login/?token=" + token + "&sender_id=" + senderId;

                string response = HTTP.GET(url);
                if (response != null)
                {
                    result = UserConfiguration.Get(response);
                }
            }

            return result;
        }

        public static class Token
        {
            public static void Save(UserConfiguration userConfig)
            {
                Properties.Settings.Default.UserToken = userConfig.Token;
                Properties.Settings.Default.Save();

                UserLoginFile.Create(userConfig);
            }

            public static string Get()
            {
                return Properties.Settings.Default.UserToken;
            }
        }

        public static class SenderId
        {
            public static string Get()
            {
                var result = Properties.Settings.Default.SenderId;

                if (string.IsNullOrEmpty(result))
                {
                    // Create a new Sender Id using the Windows Security ID 
                    result = new SecurityIdentifier((byte[])new DirectoryEntry(string.Format("WinNT://{0},Computer", Environment.MachineName)).Children.Cast<DirectoryEntry>().First().InvokeGet("objectSID"), 0).AccountDomainSid.ToString();

                    Properties.Settings.Default.SenderId = result;
                    Properties.Settings.Default.Save();
                }

                return result;
            }
        } 

    }
}
