// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;

using System;
using TrakHound.Tools.Web;
using TrakHound;
using TrakHound.API.Users;
using TrakHound.API;

namespace TrakHound.Server.Plugins.CloudData
{
    public static class Database
    {

        public static void Update(UserConfiguration userConfig, List<Data.DeviceInfo> deviceInfos)
        {
            if (userConfig != null)
            {
                var json = JSON.FromList<Data.DeviceInfo>(deviceInfos);
                if (json != null)
                {
                    var values = new NameValueCollection();
                    values["token"] = userConfig.SessionToken;
                    values["sender_id"] = UserManagement.SenderId.Get();
                    values["devices"] = json;

                    string url = new Uri(ApiConfiguration.ApiHost, "data/update/index.php").ToString();

                    var info = new SendDataInfo(url, values);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(SendData), info);
                }
            }
        }

        //public static void Update(UserConfiguration userConfig, List<UpdateData> updateDatas)
        //{
        //    var json = JSON.FromList<UpdateData>(updateDatas);
        //    if (json != null)
        //    {
        //        var values = new NameValueCollection();
        //        values["token"] = userConfig.SessionToken;
        //        values["session_id"] = UserManagement.SenderId.Get();
        //        values["devices"] = json;

        //        string url = new Uri(ApiConfiguration.ApiHost, "api/data/update/").ToString();

        //        var info = new SendDataInfo(url, values);
        //        ThreadPool.QueueUserWorkItem(new WaitCallback(SendData), info);



        //        //values["data"] = json;

        //        //string url = new Uri(ApiConfiguration.ApiHost, "api/data/update/").ToString();

        //        //var info = new SendDataInfo(url, values);
        //        //ThreadPool.QueueUserWorkItem(new WaitCallback(SendData), info);
        //    }
        //}

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

                //var httpInfo = new HTTP.HTTPInfo();
                //httpInfo.Url = info.Url;
                //httpInfo.Data = HTTP.CreatePostBytes(info.Values);
                //httpInfo.GetResponse = false;

                string response = HTTP.POST(info.Url, info.Values);
                if (!string.IsNullOrEmpty(response))
                {
                    ApiError.ProcessResponse(response, "Update Cloud Data");
                }
            }
        }
    }
}
