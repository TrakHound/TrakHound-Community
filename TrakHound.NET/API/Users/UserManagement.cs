// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using System;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Net;

using TrakHound.Logging;
using TrakHound.Tools;
using TrakHound.Tools.Web;

namespace TrakHound.API.Users
{
    public static class UserManagement
    {
        /// <summary>
        /// Used to Create a new User Account
        /// </summary>
        public static UserConfiguration CreateUser(CreateUserInfo info, string note = "")
        {
            string json = JSON.FromObject(info);
            if (!string.IsNullOrEmpty(json))
            {
                UserConfiguration result = null;

                string url = new Uri(ApiConfiguration.AuthenticationApiHost, "users/create/").ToString();

                var postDatas = new NameValueCollection();
                postDatas["sender_id"] = UserManagement.SenderId.Get();
                postDatas["user"] = json;
                postDatas["note"] = note;

                string response = HTTP.POST(url, postDatas);
                if (response != null)
                {
                    var success = ApiError.ProcessResponse(response, "Create User");
                    if (success)
                    {
                        result = UserConfiguration.Get(response);

                        return result;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Used to Edit an existing user account
        /// </summary>
        public static UserConfiguration EditUser(EditUserInfo info, string note = "")
        {
            string json = JSON.FromObject(info);
            if (!string.IsNullOrEmpty(json))
            {
                UserConfiguration result = null;

                string url = new Uri(ApiConfiguration.AuthenticationApiHost, "users/edit/index.php").ToString();

                var postDatas = new NameValueCollection();
                postDatas["user"] = json;
                postDatas["token"] = info.SessionToken;
                postDatas["sender_id"] = SenderId.Get();
                postDatas["note"] = note;

                string response = HTTP.POST(url, postDatas);
                if (response != null)
                {
                    var success = ApiError.ProcessResponse(response, "Edit User");
                    if (success)
                    {
                        result = UserConfiguration.Get(response);

                        return result;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Basic User login using (Username or Email Address) and Plain Text Password
        /// </summary>
        public static UserConfiguration BasicLogin(string id, string password, string note = "")
        {
            UserConfiguration result = null;

            if (id != null && id.Length > 0)
            {
                string url = new Uri(ApiConfiguration.AuthenticationApiHost, "users/login/index.php").ToString();

                var postDatas = new NameValueCollection();
                postDatas["id"] = id;
                postDatas["password"] = password;
                postDatas["sender_id"] = SenderId.Get();
                postDatas["note"] = note;

                string response = HTTP.POST(url, postDatas);
                if (response != null)
                {
                    var success = ApiError.ProcessResponse(response, "User Basic Login");
                    if (success)
                    {
                        result = UserConfiguration.Get(response);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// User Login that Creates and Returns a Remember Token
        /// </summary>
        public static UserConfiguration CreateTokenLogin(string id, string password, string note = "")
        {
            UserConfiguration result = null;

            if (id != null && id.Length > 0)
            {
                string url = new Uri(ApiConfiguration.AuthenticationApiHost, "users/login/index.php").ToString();

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
                    var success = ApiError.ProcessResponse(response, "User Create Token Login");
                    if (success)  result = UserConfiguration.Get(response);
                }
            }

            return result;
        }

        /// <summary>
        /// User Login using Remember Token
        /// </summary>
        public static UserConfiguration TokenLogin(string token, string note = "")
        {
            UserConfiguration result = null;

            if (token != null && token.Length > 0)
            {
                string senderId = SenderId.Get();

                string url = new Uri(ApiConfiguration.AuthenticationApiHost, "users/login/?token=" + token + "&sender_id=" + senderId + "&note=" + note).ToString();

                string response = HTTP.GET(url);
                if (response != null)
                {
                    var success = ApiError.ProcessResponse(response, "User Token Login");
                    if (success)
                    {
                        result = UserConfiguration.Get(response);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Used to Logout a currently logged in user
        /// </summary>
        public static bool Logout(string token = null)
        {
            bool result = false;

            string url = new Uri(ApiConfiguration.AuthenticationApiHost, "users/logout/index.php").ToString();

            string senderId = SenderId.Get();

            url += "?sender_id=" + SenderId.Get();
            if (token != null) url += "&token=" + token;

            string response = HTTP.GET(url);
            if (!string.IsNullOrEmpty(response))
            {
                result = ApiError.ProcessResponse(response, "User Logout");
            }

            return result;
        }

        public static bool CheckUsernameAvailability(string username)
        {
            bool result = false;

            if (!string.IsNullOrEmpty(username))
            {
                string url = new Uri(ApiConfiguration.AuthenticationApiHost, "users/checkusername/index.php").ToString();
                url += "?username=" + username;

                string response = HTTP.GET(url);
                if (response != null)
                {
                    result = ApiError.ProcessResponse(response, "Username Check");
                }
            }

            return result;
        }


        public static class ProfileImage
        {
            public static UserConfiguration Set(string token, string path)
            {
                UserConfiguration result = null;

                if (!string.IsNullOrEmpty(token))
                {
                    string url = "https://www.feenux.com/trakhound/api/profile_image/set/index.php";
                    string senderId = SenderId.Get();

                    var postDatas = new NameValueCollection();
                    postDatas["token"] = token;
                    postDatas["sender_id"] = SenderId.Get();
                }

                return result;
            }

            public static Image Get(string filename)
            {
                Image result = null;

                if (!string.IsNullOrEmpty(filename))
                {
                    string localPath = Path.Combine(FileLocations.TrakHoundTemp, filename);

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

                                        string localSavePath = Path.Combine(FileLocations.TrakHoundTemp, filename);

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

                ServerCredentials.Create(userConfig);
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
                return Network_Functions.GetMacAddress();
            }
        } 

    }
}
