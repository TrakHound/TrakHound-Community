// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;

using TrakHound.API.Users;
using TrakHound.Tools.Web;

namespace TrakHound.API
{
    public static partial class Data
    {

        public static bool Update(UserConfiguration userConfig, DeviceInfo deviceInfo)
        {
            var deviceInfos = new List<DeviceInfo>();
            deviceInfos.Add(deviceInfo);

            return Update(userConfig, deviceInfos);
        }
        
        public static bool Update(UserConfiguration userConfig, List<DeviceInfo> deviceInfos)
        {
            var json = Data.DeviceInfo.ListToJson(deviceInfos);
            if (!string.IsNullOrEmpty(json))
            {
                Uri apiHost = ApiConfiguration.DataApiHost;

                string url = new Uri(apiHost, "data/update/index.php").ToString();

                var postDatas = new NameValueCollection();
                postDatas["token"] = userConfig.SessionToken;
                postDatas["sender_id"] = UserManagement.SenderId.Get();
                postDatas["devices"] = json;

                string response = HTTP.POST(url, postDatas);
                if (response != null)
                {
                    return ApiError.ProcessResponse(response, "Update Device Data");
                }
            }

            return false;
        }

    }
}
