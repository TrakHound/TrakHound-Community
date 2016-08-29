// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

using TrakHound.API.Users;
using TrakHound.Tools.Web;

namespace TrakHound.API
{
    public static partial class Data
    {
        // Command Placeholders
        // 0 = Description
        // 1 = Status
        // 2 = Controller
        // 3 = Oee
        // 4 = Timers
        // 5 = Hours

        public static List<DeviceInfo> Get() { return Get(null, null, 5000, null); }

        public static List<DeviceInfo> Get(string command) { return Get(null, null, 5000, command); }

        public static List<DeviceInfo> Get(int timeout) { return Get(null, null, timeout, null); }

        public static List<DeviceInfo> Get(int timeout, string command) { return Get(null, null, timeout, command); }

        public static List<DeviceInfo> Get(UserConfiguration userConfig, int timeout) { return Get(userConfig, null, timeout, null); }

        public static List<DeviceInfo> Get(UserConfiguration userConfig, int timeout, string command) { return Get(userConfig, null, timeout, command); }

        public static List<DeviceInfo> Get(List<string> uniqueIds) { return Get(null, uniqueIds, 5000, null); }

        public static List<DeviceInfo> Get(List<string> uniqueIds, string command) { return Get(null, uniqueIds, 5000, command); }

        public static List<DeviceInfo> Get(List<string> uniqueIds, int timeout) { return Get(null, uniqueIds, timeout, null); }

        public static List<DeviceInfo> Get(List<string> uniqueIds, int timeout, string command) { return Get(null, uniqueIds, timeout, command); }

        public static List<DeviceInfo> Get(UserConfiguration userConfig, List<string> uniqueIds, int timeout) { return Get(userConfig, uniqueIds, timeout, null); }

        public static List<DeviceInfo> Get(UserConfiguration userConfig, List<string> uniqueIds, int timeout, string command)
        {
            Uri apiHost = ApiConfiguration.DataApiHost;

            string url = new Uri(apiHost, "data/get/index.php").ToString();

            string devices = "";

            // List Devices to Get data for
            // If no devices are listed then ALL devices are retrieved
            if (uniqueIds != null)
            {
                var deviceItems = new List<DeviceListItem>();

                foreach (var uniqueId in uniqueIds) deviceItems.Add(new DeviceListItem(uniqueId));

                string json = JSON.FromList<DeviceListItem>(deviceItems);
                if (!string.IsNullOrEmpty(json))
                {
                    devices = "&devices=" + json;
                }
            }

            string cmd = "";
            if (command != null) cmd = "&command=" +command;

            // Create GET request parameters
            if (userConfig != null)
            {
                string format = "{0}?token={1}&sender_id={2}{3}{4}";

                string token = userConfig.SessionToken;
                string senderId = UserManagement.SenderId.Get();
                
                url = string.Format(format, url, token, senderId, devices, cmd);
            }
            else
            {
                string format = "{0}?{1}{2}";
                url = string.Format(format, url, devices, cmd);
            }
           
            // Setup HTTP Information for GET request
            var httpInfo = new HTTP.HTTPInfo();
            httpInfo.Url = url;
            httpInfo.Timeout = timeout;
            httpInfo.MaxAttempts = 1;


            string response = HTTP.GET(httpInfo);
            if (response != null)
            {
                bool success = ApiError.ProcessResponse(response, "Get Device Data");
                if (success)
                {
                    var deviceInfos = JSON.ToType<List<DeviceInfo>>(response);
                    if (deviceInfos != null)
                    {
                        return deviceInfos;
                    }
                }
            }

            return null;
        }

        public class DeviceListItem
        {
            public DeviceListItem(string uniqueId)
            {
                UniqueId = uniqueId;
            }

            [JsonProperty("unique_id")]
            public string UniqueId { get; set; }
        }

    }
}
