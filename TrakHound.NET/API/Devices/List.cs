// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;

using TrakHound.API.Users;
using TrakHound.Configurations;
using TrakHound.Tools.Web;

namespace TrakHound.API
{
    public static partial class Devices
    {

        // Example URL (GET)
        // -----------------------------------------------------

        // ../api/devices/list?token=01234&sender_id=56789   				  (List all devices for the user)
        // OR
        // ../api/devices/list?token=01234&sender_id=56789&unique_id=ABCDEFG   (List only the device with the specified 'unique_id')

        // -----------------------------------------------------

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

        /// <summary>
        /// List a single DeviceDescription
        /// </summary>
        /// <param name="userConfig">UserConfiguration object for the current user</param>
        /// <param name="deviceUniqueId">The Unique ID of the device to return</param>
        /// <returns></returns>
        public static DeviceDescription List(UserConfiguration userConfig, string deviceUniqueId, Uri apiHost)
        {
            if (!string.IsNullOrEmpty(deviceUniqueId))
            {
                string url = new Uri(apiHost, "devices/list/index.php").ToString();

                if (userConfig != null)
                {
                    string format = "{0}?token={1}&sender_id={2}&unique_id={3}";

                    string token = userConfig.SessionToken;
                    string senderId = UserManagement.SenderId.Get();

                    url = string.Format(format, url, token, senderId, deviceUniqueId);
                }

                string response = HTTP.GET(url);
                if (response != null)
                {
                    bool success = ApiError.ProcessResponse(response, "List Devices");
                    if (success)
                    {
                        var deviceInfos = JSON.ToType<List<Data.DeviceInfo>>(response);
                        if (deviceInfos != null && deviceInfos.Count > 0)
                        {
                            return new DeviceDescription(deviceInfos[0]);
                        }
                    }
                }
            }

            return null;
        }

        public static List<DeviceDescription> List(UserConfiguration userConfig, string[] deviceUniqueIds, Uri apiHost)
        {
            if (deviceUniqueIds != null && deviceUniqueIds.Length > 0)
            {
                string url = new Uri(apiHost, "devices/list/index.php").ToString();

                var getDeviceInfos = new List<GetDeviceInfo>();
                foreach (var deviceUniqueId in deviceUniqueIds) getDeviceInfos.Add(new GetDeviceInfo(deviceUniqueId));

                string json = JSON.FromObject(getDeviceInfos);
                if (json != null)
                {
                    var postDatas = new NameValueCollection();

                    if (userConfig != null)
                    {
                        postDatas["token"] = userConfig.SessionToken;
                        postDatas["sender_id"] = UserManagement.SenderId.Get();
                    }

                    postDatas["devices"] = json;

                    string response = HTTP.POST(url, postDatas);
                    if (response != null)
                    {
                        bool success = ApiError.ProcessResponse(response, "List Devices");
                        if (success)
                        {
                            var deviceInfos = JSON.ToType<List<Data.DeviceInfo>>(response);
                            if (deviceInfos != null)
                            {
                                var devices = new List<DeviceDescription>();

                                foreach (var deviceInfo in deviceInfos)
                                {
                                    devices.Add(new DeviceDescription(deviceInfo));
                                }

                                return devices;
                            }
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Get a list of DeviceDescriptions of all of the user's devices
        /// </summary>
        /// <param name="userConfig">UserConfiguration object for the current user</param>
        /// <returns></returns>
        public static List<DeviceDescription> List(UserConfiguration userConfig, Uri apiHost)
        {
            string url = new Uri(apiHost, "devices/list/index.php").ToString();

            if (userConfig != null)
            {
                string format = "{0}?token={1}&sender_id={2}";

                string token = userConfig.SessionToken;
                string senderId = UserManagement.SenderId.Get();

                url = string.Format(format, url, token, senderId);
            }

            string response = HTTP.GET(url);
            if (response != null)
            {
                bool success = ApiError.ProcessResponse(response, "List Devices");
                if (success)
                {
                    var deviceInfos = JSON.ToType<List<Data.DeviceInfo>>(response);
                    if (deviceInfos != null)
                    {
                        var devices = new List<DeviceDescription>();

                        foreach (var deviceInfo in deviceInfos)
                        {
                            devices.Add(new DeviceDescription(deviceInfo));
                        }

                        return devices;
                    }
                }
            }

            return null;
        }

    }

}
