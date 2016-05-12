// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;

using TH_Configuration;
using TH_Global.Web;

namespace TH_Mobile
{
    public static class Database
    {
        private const string PHP_URL = "https://www.feenux.com/trakhound/api/mobile/";

        public static void CreateTable(string userId, Configuration config)
        {
            if (!string.IsNullOrEmpty(userId) && config != null)
            {
                var values = new NameValueCollection();
                values["user_id"] = userId;
                values["database_id"] = config.DatabaseId;

                values["unique_id"] = config.UniqueId;
                values["description"] = config.Description.Description;
                values["device_id"] = config.Description.Device_ID;
                values["manufacturer"] = config.Description.Manufacturer;
                values["model"] = config.Description.Model;
                values["serial"] = config.Description.Serial;
                values["controller"] = config.Description.Controller;
                values["logo_url"] = config.FileLocations.Manufacturer_Logo_Path;
                values["image_url"] = config.FileLocations.Image_Path;

                string url = PHP_URL + "create/";

                var info = new SendDataInfo(url, values);
                ThreadPool.QueueUserWorkItem(new WaitCallback(SendData), info);
            }
        }

        public static void Update(List<UpdateData> updateDatas)
        {
            var json = JSON.FromList<UpdateData>(updateDatas);
            if (json != null)
            {
                var values = new NameValueCollection();
                values["data"] = json;

                string url = PHP_URL + "update/";

                var info = new SendDataInfo(url, values);
                ThreadPool.QueueUserWorkItem(new WaitCallback(SendData), info);
            }
        }

        private class SendDataInfo
        {
            public SendDataInfo(string url, NameValueCollection values)
            {
                Url = url;
                Values = values;
            }

            public string Url { get; set; }
            public NameValueCollection Values { get; set; }
        }

        private static void SendData(object o)
        {
            if (o != null)
            {
                var info = (SendDataInfo)o;

                var httpInfo = new HTTP.HTTPInfo();
                httpInfo.Url = info.Url;
                httpInfo.Data = HTTP.CreatePostBytes(info.Values);
                httpInfo.GetResponse = false;
                HTTP.POST(httpInfo);
            }
        }
    }
}
