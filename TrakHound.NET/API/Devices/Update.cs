// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;

using TrakHound.API.Users;
using TrakHound.Configurations;
using TrakHound.Tools;
using TrakHound.Tools.Web;

namespace TrakHound.API
{
    public static partial class Devices
    {

        // Example POST Data
        // -----------------------------------------------------

        // name = 'token'
        // value = Session Token

        // name = 'sender_id'
        // value = Sender ID

        // name = 'devices'
        // value =  [{
        //	
        //	 "unique_id": "987654321",
        //	 "data": [
        //		{ "address": "/ClientEnabled", "value": "true", "" },
        //		{ "address": "/ServerEnabled", "value": "true", "" },
        //		{ "address": "/UniqueId", "value": "987654321", "" }
        //		]
        //	}, 
        //	{
        //	 "unique_id": "123456789",
        //	 "data": [
        //		{ "address": "/ClientEnabled", "value": "true", "" },
        //		{ "address": "/ServerEnabled", "value": "true", "" },
        //		{ "address": "/UniqueId", "value": "123456789", "" }
        //		]
        // }]
        // -----------------------------------------------------

        public static bool Update(UserConfiguration userConfig, DeviceConfiguration deviceConfig)
        {
            bool result = false;

            if (userConfig != null)
            {
                var table = deviceConfig.ToTable();
                if (table != null)
                {
                    var infos = new List<DeviceInfo>();
                    infos.Add(new DeviceInfo(deviceConfig.UniqueId, table));

                    string json = JSON.FromObject(infos);
                    if (json != null)
                    {
                        Uri apiHost = ApiConfiguration.AuthenticationApiHost;

                        string url = new Uri(apiHost, "devices/update/index.php").ToString();

                        var postDatas = new NameValueCollection();
                        postDatas["token"] = userConfig.SessionToken;
                        postDatas["sender_id"] = UserManagement.SenderId.Get();
                        postDatas["devices"] = json;

                        string response = HTTP.POST(url, postDatas);
                        if (response != null)
                        {
                            result = ApiError.ProcessResponse(response, "Update Devices");
                        }
                    }
                }
            }

            return result;
        }

        public static bool Update(UserConfiguration userConfig, DataTable table)
        {
            string uniqueId = DataTable_Functions.GetTableValue(table, "address", "/UniqueId", "value");
            if (!string.IsNullOrEmpty(uniqueId))
            {
                var deviceInfo = new DeviceInfo(uniqueId, table);

                return Update(userConfig, deviceInfo);
            }

            return false;
        }

        public static bool Update(UserConfiguration userConfig, DeviceInfo deviceInfo)
        {
            var deviceInfos = new List<DeviceInfo>();
            deviceInfos.Add(deviceInfo);

            return Update(userConfig, deviceInfos, false);
        }

        public static bool Update(UserConfiguration userConfig, List<DeviceInfo> deviceInfos, bool replace = true)
        {
            bool result = false;

            if (userConfig != null)
            {
                string json = JSON.FromObject(deviceInfos);
                if (json != null)
                {
                    Uri apiHost = ApiConfiguration.AuthenticationApiHost;

                    string url = new Uri(apiHost, "devices/update/index.php").ToString();

                    var postDatas = new NameValueCollection();
                    postDatas["token"] = userConfig.SessionToken;
                    postDatas["sender_id"] = UserManagement.SenderId.Get();
                    postDatas["devices"] = json;
                    postDatas["replace"] = replace.ToString();

                    string response = HTTP.POST(url, postDatas);
                    if (response != null)
                    {
                        result = ApiError.ProcessResponse(response, "Update Devices");
                    }
                }
            }

            return result;
        }

        public static bool Update(UserConfiguration userConfig, string deviceUniqueId, DeviceInfo.Row deviceRow)
        {
            bool result = false;

            if (userConfig != null)
            {
                var infos = new List<DeviceInfo>();
                infos.Add(new DeviceInfo(deviceUniqueId, deviceRow));

                string json = JSON.FromObject(infos);
                if (json != null)
                {
                    Uri apiHost = ApiConfiguration.AuthenticationApiHost;

                    string url = new Uri(apiHost, "devices/update/index.php").ToString();

                    var postDatas = new NameValueCollection();
                    postDatas["token"] = userConfig.SessionToken;
                    postDatas["sender_id"] = UserManagement.SenderId.Get();
                    postDatas["devices"] = json;
                    postDatas["replace"] = "false";

                    string response = HTTP.POST(url, postDatas);
                    if (response != null)
                    {
                        result = ApiError.ProcessResponse(response, "Update Devices");
                    }
                }
            }

            return result;
        }
    }
    
}
