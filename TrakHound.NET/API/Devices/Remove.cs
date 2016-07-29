// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

using TrakHound.API;
using TrakHound.API.Users;
using TrakHound.Logging;
using TrakHound.Tools.Web;

namespace TrakHound.API
{
    public static partial class Devices
    {

        // Example Post Data
        // -----------------------------------------------------

        // name = 'token'
        // value = 01234 (Session Token)

        // name = 'sender_id'
        // value = 56789 (Sender ID)

        // (Optional)
        // name = 'devices'
        // value =  [
        //	{ "unique_id": "ABCDEFG" },
        //  { "unique_id": "HIJKLMN" }
        //	]
        // -----------------------------------------------------

        private class RemoveDeviceInfo
        {
            public RemoveDeviceInfo(string uniqueId)
            {
                UniqueId = uniqueId;
            }

            [JsonProperty("unique_id")]
            public string UniqueId { get; set; }

            public static string ArrayToJSON(string[] uniqueIds)
            {
                if (uniqueIds != null && uniqueIds.Length > 0)
                {
                    var infos = new List<RemoveDeviceInfo>();

                    foreach (var uniqueId in uniqueIds)
                    {
                        infos.Add(new RemoveDeviceInfo(uniqueId));
                    }

                    return JSON.FromList<RemoveDeviceInfo>(infos);
                }

                return null;
            }
        }

        /// <summary>
        /// Remove Specified Devices from User
        /// </summary>
        /// <param name="userConfig">UserConfiguration object for the current user</param>
        /// <param name="deviceUniqueId">The Unique ID of the device to return</param>
        /// <returns></returns>
        public static bool Remove(UserConfiguration userConfig, string[] deviceUniqueIds)
        {
            if (userConfig != null && deviceUniqueIds != null && deviceUniqueIds.Length > 0)
            {
                Uri apiHost = ApiConfiguration.ApiHost;

                string url = new Uri(apiHost, "devices/remove/index.php").ToString();

                var postDatas = new NameValueCollection();
                postDatas["token"] = userConfig.SessionToken;
                postDatas["sender_id"] = UserManagement.SenderId.Get();

                var devices = RemoveDeviceInfo.ArrayToJSON(deviceUniqueIds);
                postDatas["devices"] = devices;

                string response = HTTP.POST(url, postDatas);
                if (response != null)
                {
                    return ApiError.ProcessResponse(response, "Remove Devices");

                    //string[] x = response.Split('(', ')');
                    //if (x != null && x.Length > 1)
                    //{
                    //    string error = x[1];

                    //    Logger.Log("Remove Device Failed : Error " + error, LogLineType.Error);
                    //    return false;
                    //}
                    //else
                    //{
                    //    Logger.Log("Remove Device Successful", LogLineType.Notification);
                    //    return true;
                    //}
                }
            }

            return false;
        }

        /// <summary>
        /// Remove all of the user's devices
        /// </summary>
        /// <param name="userConfig">UserConfiguration object for the current user</param>
        /// <returns></returns>
        public static bool Remove(UserConfiguration userConfig)
        {
            if (userConfig != null)
            {
                Uri apiHost = ApiConfiguration.ApiHost;

                string url = new Uri(apiHost, "devices/remove/index.php").ToString();

                var postDatas = new NameValueCollection();
                postDatas["token"] = userConfig.SessionToken;
                postDatas["sender_id"] = UserManagement.SenderId.Get();

                string response = HTTP.POST(url, postDatas);
                if (response != null)
                {
                    return ApiError.ProcessResponse(response, "Remove Devices");

                    //string[] x = response.Split('(', ')');
                    //if (x != null && x.Length > 1)
                    //{
                    //    string error = x[1];

                    //    Logger.Log("Remove Device Failed : Error " + error, LogLineType.Error);
                    //    return false;
                    //}
                    //else
                    //{
                    //    Logger.Log("Remove Device Successful", LogLineType.Notification);
                    //    return true;
                    //}
                }
            }

            return false;
        }

    }
}
