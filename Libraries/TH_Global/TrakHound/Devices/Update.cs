// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;

using TH_Global.TrakHound.Configurations;
using TH_Global.TrakHound.Users;
using TH_Global.Web;

namespace TH_Global.TrakHound
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
                var table = Converter.XMLToTable(deviceConfig.ConfigurationXML);
                if (table != null)
                {
                    var infos = new List<DeviceInfo>();
                    infos.Add(new DeviceInfo(deviceConfig.UniqueId, table));

                    string json = JSON.FromObject(infos);
                    if (json != null)
                    {
                        Uri apiHost = ApiConfiguration.ApiHost;

                        string url = new Uri(apiHost, "devices/update/index.php").ToString();

                        var postDatas = new NameValueCollection();
                        postDatas["token"] = userConfig.SessionToken;
                        postDatas["sender_id"] = UserManagement.SenderId.Get();
                        postDatas["devices"] = json;

                        string response = HTTP.POST(url, postDatas);
                        if (response != null)
                        {
                            string[] x = response.Split('(', ')');
                            if (x != null && x.Length > 1)
                            {
                                string error = x[1];

                                Logger.Log("Update Device Failed : Error " + error, Logger.LogLineType.Warning);
                                result = false;
                            }
                            else
                            {
                                Logger.Log("Update Device Successful", Logger.LogLineType.Notification);
                                result = true;
                            }
                        }
                    }
                }
            }

            return result;
        }

    }
}
