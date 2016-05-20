// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Specialized;
using System.DirectoryServices;
using System.Linq;
using System.Security.Principal;
using System.Drawing;

using System.Net;
using System.IO;

using TH_Global.Functions;
using TH_Global.Web;

namespace TH_Global.TrakHound.Users
{
    public static class UserManagement
    {
        public static UserConfiguration CreateUser(CreateUserInfo info, string note = "")
        {
            string json = JSON.FromObject(info);
            if (!string.IsNullOrEmpty(json))
            {
                UserConfiguration result = null;

                string url = "https://www.feenux.com/trakhound/api/users/create/";

                var postDatas = new NameValueCollection();
                postDatas["user"] = json;
                postDatas["note"] = note;

                string response = HTTP.POST(url, postDatas);
                if (response != null)
                {
                    result = UserConfiguration.Get(response);

                    return result;
                }
            }

            return null;
        }

        public static UserConfiguration EditUser(EditUserInfo info, string note = "")
        {
            string json = JSON.FromObject(info);
            if (!string.IsNullOrEmpty(json))
            {
                UserConfiguration result = null;

                string url = "https://www.feenux.com/trakhound/api/users/edit/";

                var postDatas = new NameValueCollection();
                postDatas["user"] = json;
                postDatas["token"] = info.SessionToken;
                postDatas["sender_id"] = SenderId.Get();
                postDatas["note"] = note;

                string response = HTTP.POST(url, postDatas);
                if (response != null)
                {
                    result = UserConfiguration.Get(response);

                    return result;
                }
            }

            return null;
        }


        public static UserConfiguration BasicLogin(string id, string password, string note = "")
        {
            UserConfiguration result = null;

            if (id != null && id.Length > 0)
            {
                string url = "https://www.feenux.com/trakhound/api/login/";

                var postDatas = new NameValueCollection();
                postDatas["id"] = id;
                postDatas["password"] = password;
                postDatas["sender_id"] = SenderId.Get();
                postDatas["note"] = note;

                string response = HTTP.POST(url, postDatas);
                if (response != null)
                {
                    result = UserConfiguration.Get(response);
                }
            }

            return result;
        }

        public static UserConfiguration CreateTokenLogin(string id, string password, string note = "")
        {
            UserConfiguration result = null;

            if (id != null && id.Length > 0)
            {
                string url = "https://www.feenux.com/trakhound/api/login/";

                string senderId = SenderId.Get();

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

        public static UserConfiguration TokenLogin(string token, string note = "")
        {
            UserConfiguration result = null;

            if (token != null && token.Length > 0)
            {
                string senderId = SenderId.Get();

                string url = "https://www.feenux.com/trakhound/api/login/?token=" + token + "&sender_id=" + senderId + "&note=" + note;

                string response = HTTP.GET(url);
                if (response != null)
                {
                    result = UserConfiguration.Get(response);
                }
            }

            return result;
        }


        public static bool Logout(string token = null)
        {
            bool result = false;
       
            string url = "https://www.feenux.com/trakhound/api/logout/?";
            string senderId = SenderId.Get();

            url += "sender_id=" + SenderId.Get();
            if (token != null) url += "&token=" + token;

            string response = HTTP.GET(url);
            if (!string.IsNullOrEmpty(response)) result = true;

            return result;
        }


        public static class ProfileImage
        {

            public static UserConfiguration Set(string token, string path)
            {
                UserConfiguration result = null;

                if (!string.IsNullOrEmpty(token))
                {
                    string url = "https://www.feenux.com/trakhound/api/profile_image/set/";
                    string senderId = SenderId.Get();

                    var postDatas = new NameValueCollection();
                    postDatas["token"] = token;
                    postDatas["sender_id"] = SenderId.Get();

                    string response = HTTP.UploadFile(url, path, "file", "image/jpeg", postDatas);
                    if (response != null)
                    {
                        result = UserConfiguration.Get(response);

                        return result;
                    }
                }

                return result;
            }

            public static Image Get(string filename)
            {
                Image result = null;

                if (!string.IsNullOrEmpty(filename))
                {
                    string localPath = FileLocations.TrakHoundTemp + "\\" + filename;

                    // Look for file in local cache
                    if (File.Exists(localPath))
                    {
                        result = Image_Functions.GetImageFromFile(localPath);
                    }
                    // If not in local cache (already downloaded) then download it
                    else
                    {
                        const int MAX_ATTEMPTS = 3;
                        int attempts = 0;
                        bool success = false;

                        while (attempts < MAX_ATTEMPTS && !success)
                        {
                            attempts += 1;

                            using (var webClient = new WebClient())
                            {
                                try
                                {
                                    byte[] data = webClient.DownloadData("https://www.feenux.com/trakhound/files/profile_images/" + filename);

                                    using (MemoryStream mem = new MemoryStream(data))
                                    {
                                        result = Image.FromStream(mem);

                                        string localSavePath = FileLocations.TrakHoundTemp + "\\" + filename;

                                        result.Save(localSavePath);
                                    }

                                    success = true;
                                }
                                catch (Exception ex) { Logger.Log("Exception : " + ex.Message); }
                            }
                        }
                    }
                }

                return result;
            }

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
