// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;

using TrakHound.API;
using TrakHound.API.Users;
using TrakHound.Logging;
using TrakHound.Tools.Web;

namespace TrakHound_Server.Plugins.CloudData
{
    public class UpdateQueue
    {
        private static List<Data.DeviceInfo> queuedInfos = new List<Data.DeviceInfo>();

        private Thread queueThread;

        private ManualResetEvent stop;

        private bool started = false;


        public void Add(Data.DeviceInfo deviceInfo)
        {
            if (deviceInfo != null)
            {
                int index = queuedInfos.FindIndex(o => GetUniqueId(o) == deviceInfo.UniqueId);
                if (index >= 0) queuedInfos[index] = deviceInfo;
                else queuedInfos.Add(deviceInfo);
            }
        }

        // Not sure why a null value is getting added to queuedInfos but it is
        private static string GetUniqueId(Data.DeviceInfo deviceInfo)
        {
            if (deviceInfo != null) return deviceInfo.UniqueId;
            else return null;
        }

        public void Remove(Data.DeviceInfo deviceInfo)
        {
            if (deviceInfo != null)
            {
                int index = queuedInfos.FindIndex(o => o.UniqueId == deviceInfo.UniqueId);
                if (index >= 0) queuedInfos.RemoveAt(index);
            }
        }


        public void Start()
        {
            if (!started)
            {
                stop = new ManualResetEvent(false);

                started = true;

                if (queueThread != null) queueThread.Abort();

                queueThread = new Thread(new ThreadStart(Update));
                queueThread.Start();
            }
        }

        private void Update()
        {
            while (!stop.WaitOne(0, true))
            {
                ProcessQueue();

                Thread.Sleep(ApiConfiguration.UpdateInterval);
            }

            Console.WriteLine("UpdateQueue Loop Exited");
        }

        public void Stop()
        {
            stop.Set();
            started = false;

            Logger.Log("CloudData Queue Stopped");
        }

        private void RequestTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var timer = (System.Timers.Timer)sender;

            timer.Enabled = false;
            ProcessQueue();
            if (stop != null && !stop.WaitOne(0, true)) timer.Enabled = true;
        }

        private void ProcessQueue()
        {
            Console.WriteLine("ProcessQueue() :: Count = " + queuedInfos.Count);

            if (queuedInfos.Count > 0)
            {
                // List of infos to actually send to API
                var temp = new List<Data.DeviceInfo>();

                long bufferSize = 0;

                foreach (var queuedInfo in queuedInfos.ToList())
                {
                    queuedInfo.CombineHours();

                    // Get json size
                    long size = 0;
                    string json = queuedInfo.ToJson();
                    if (json != null) size = json.Length;
                    bufferSize += size;

                    // Only add if less than buffersize
                    if (bufferSize <= ApiConfiguration.BufferSize) temp.Add(queuedInfo);
                    else break;
                }

                Update(Plugin.currentUser, temp);

                foreach (var queuedInfo in temp)
                {
                    Console.WriteLine(queuedInfo.UniqueId + " :: Count = " + queuedInfo.Classes.Count);

                    var match = queuedInfos.Find(o => o.UniqueId == queuedInfo.UniqueId);
                    if (match != null) match.ClearClasses();
                }
            }
        }


        public static void Update(UserConfiguration userConfig, List<Data.DeviceInfo> deviceInfos)
        {
            if (ApiConfiguration.DataApiHost.ToString() != ApiConfiguration.LOCAL_API_HOST) // Remote
            {
                var json = Data.DeviceInfo.ListToJson(deviceInfos);
                if (json != null)
                {
                    var values = new NameValueCollection();
                    if (userConfig != null) values["token"] = userConfig.SessionToken;
                    values["sender_id"] = UserManagement.SenderId.Get();
                    values["devices"] = json;

                    string url = new Uri(ApiConfiguration.DataApiHost, "data/update/index.php").ToString();

                    var info = new SendDataInfo(url, values);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(SendData), info);
                }
            }
            else // Local
            {
                var json = Data.DeviceInfo.ListToJson(deviceInfos);
                if (json != null)
                {
                    var values = new NameValueCollection();
                    values["devices"] = json;

                    string url = new Uri(ApiConfiguration.DataApiHost, "data/update/index.php").ToString();

                    // Send to local server
                    var info = new SendDataInfo(url, values);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(SendData), info);

                    if (userConfig != null)
                    {
                        values["token"] = userConfig.SessionToken;
                        values["sender_id"] = UserManagement.SenderId.Get();

                        // Send to TrakHound Cloud (for Mobile App)
                        var cloudUri = new Uri(ApiConfiguration.CLOUD_API_HOST);
                        url = new Uri(cloudUri, "data/update/index.php").ToString();

                        var cloudInfo = new SendDataInfo(url, values);
                        ThreadPool.QueueUserWorkItem(new WaitCallback(SendData), cloudInfo);
                    }
                }
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
                httpInfo.ContentData = HTTP.PostContentData.FromNamedValueCollection(info.Values);
                httpInfo.MaxAttempts = 2;
                httpInfo.Timeout = 2000;

                string response = HTTP.POST(httpInfo);
                if (!string.IsNullOrEmpty(response))
                {
                    ApiError.ProcessResponse(response, "Update Cloud Data : " + info.Url + " @ " + DateTime.Now.ToLongTimeString());
                }
            }
        }
    }
    
}
