// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

using TrakHound.API.Users;
using TrakHound.Tools.Web;

namespace TrakHound.API
{
    public static partial class Devices
    {

        // Example URL (GET)
        // -----------------------------------------------------

        // ../api/devices/check?token=01234&sender_id=56789   				  (Check all devices for the user)
        // OR
        // ../api/devices/check?token=01234&sender_id=56789&unique_id=ABCDEFG   (Check only the device with the specified 'unique_id')

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
        /// Check the status (Update Id) of a single Device
        /// </summary>
        /// <param name="userConfig">UserConfiguration object for the current user</param>
        /// <param name="deviceUniqueId">The Unique ID of the device to return</param>
        /// <returns></returns>
        public static CheckInfo Check(UserConfiguration userConfig, string deviceUniqueId)
        {
            if (!string.IsNullOrEmpty(deviceUniqueId))
            {
                Uri apiHost = ApiConfiguration.AuthenticationApiHost;

                string url = new Uri(apiHost, "devices/check/index.php").ToString();

                string format = "{0}?token={1}&sender_id={2}{3}";

                string token = userConfig.SessionToken;
                string senderId = UserManagement.SenderId.Get();
                string uniqueId = "unique_id=" + deviceUniqueId;

                url = string.Format(format, url, token, senderId, uniqueId);

                string response = HTTP.GET(url);
                if (response != null)
                {
                    bool success = ApiError.ProcessResponse(response, "Check Device");
                }
            }

            return null;
        }

        /// <summary>
        /// Check the status (Update Id) for a list of devices
        /// </summary>
        /// <param name="userConfig">UserConfiguration object for the current user</param>
        /// <returns></returns>
        public static List<CheckInfo> Check(UserConfiguration userConfig)
        {
            Uri apiHost = ApiConfiguration.AuthenticationApiHost;

            string url = new Uri(apiHost, "devices/check/index.php").ToString();

            string format = "{0}?token={1}&sender_id={2}";

            string token = userConfig.SessionToken;
            string senderId = UserManagement.SenderId.Get();

            url = string.Format(format, url, token, senderId);

            string response = HTTP.GET(url);
            if (response != null)
            {
                bool success = ApiError.ProcessResponse(response, "Check Devices");
                if (success)
                {
                    if (success)
                    {
                        if (response == "No Devices Found") return null;
                        else return JSON.ToType<List<CheckInfo>>(response);
                    }
                }
            }

            return null;
        }

    }

}
