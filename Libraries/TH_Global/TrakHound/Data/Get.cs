// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

using TH_Global.TrakHound.Users;
using TH_Global.Web;

namespace TH_Global.TrakHound
{
    public static partial class Data
    {

        public static List<DeviceInfo> Get(UserConfiguration userConfig, List<string> uniqueIds)
        {

            Uri apiHost = ApiConfiguration.ApiHost;

            string url = new Uri(apiHost, "data/get/index.php").ToString();

            string format = "{0}?token={1}&sender_id={2}{3}";

            string token = userConfig.SessionToken;
            string senderId = UserManagement.SenderId.Get();
            string devices = "";

            // List Devices to Get data for
            // If no devices are listed then ALL devices are retrieved
            if (uniqueIds != null)
            {
                string json = JSON.FromList<string>(uniqueIds);
                if (!string.IsNullOrEmpty(json))
                {
                    devices = "devices=" + json;
                }
            }

            url = string.Format(format, url, token, senderId, devices);

            string response = HTTP.GET(url);
            if (response != null)
            {
                var deviceInfos = JSON.ToType<List<DeviceInfo>>(response);
                if (deviceInfos != null)
                {
                    return deviceInfos;
                }
            }

            return null;
        }

    }
}
