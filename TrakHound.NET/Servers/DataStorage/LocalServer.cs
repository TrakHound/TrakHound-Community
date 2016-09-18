// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;

using TrakHound.API;
using TrakHound.Configurations;
using TrakHound.Logging;
using TrakHound.Tools.Web;

namespace TrakHound.Servers.DataStorage
{
    public class LocalStorageServer
    {
        /// <summary>
        /// API Server Port
        /// </summary>
        public const int PORT = 8472; // ASCII Dec for 'T' and 'H'

        private HttpListener listener;

        private System.Timers.Timer backupTimer;

        private const int BACKUP_INTERVAL = 300000; // 5 Minutes


        public LocalStorageServer()
        {
            var apiMonitor = new ApiConfiguration.Monitor();
            apiMonitor.ApiConfigurationChanged += ApiMonitor_ApiConfigurationChanged;
        }

        private void ApiMonitor_ApiConfigurationChanged(ApiConfiguration config)
        {
            if (config != null && config.DataHost != ApiConfiguration.LOCAL_API_HOST) Stop();
            else Start();
        }

        public void Start()
        {
            Stop();

            listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:" + PORT + "/api/");
            listener.Start();

            ThreadPool.QueueUserWorkItem((o) =>
            {
                Console.WriteLine("TrakHound Data Server Started...");
                try
                {
                    while (listener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem((c) =>
                        {
                            var ctx = c as HttpListenerContext;

                            try
                            {
                                string rstr = ProcessRequest(ctx);
                                byte[] buf = Encoding.UTF8.GetBytes(rstr);
                                ctx.Response.ContentLength64 = buf.Length;
                                ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                            }
                            catch { } // suppress any exceptions
                            finally
                            {
                                // always close the stream
                                ctx.Response.OutputStream.Close();
                            }
                        }, listener.GetContext());
                    }
                }
                catch { } // suppress any exceptions
            });

            StartBackupTimer();
        }

        public void Stop()
        {
            if (listener != null)
            {
                try
                {
                    listener.Stop();
                    listener.Close();
                }
                catch (Exception ex) { }
            }

            StopBackupTimer();
        }


        private string ProcessRequest(HttpListenerContext context)
        {
            string result = null;

            try
            {
                string path = context.Request.Url.AbsolutePath;
                if (!string.IsNullOrEmpty(path) && path.Length > 1)
                {
                    // Remove beginning forward slash
                    path = path.Substring(1);

                    path = path.Substring("api/".Length);

                    // Split path by forward slashes
                    var paths = path.Split('/');
                    if (paths.Length > 1 && paths[0] != "/" && !string.IsNullOrEmpty(paths[0]))
                    {
                        switch (paths[0].ToLower())
                        {
                            case "data":

                                // Remove 'data' from path
                                path = path.Substring(paths[0].Length);

                                // Remove first forward slash
                                path = path.TrimStart('/');

                                // Process the Data Request and return response
                                if (context.Request.HttpMethod == "GET")
                                {
                                    paths = path.Split('/');
                                    if (paths.Length > 0)
                                    {
                                        switch (paths[0].ToLower())
                                        {
                                            case "get": result = Data.Get(context.Request.Url.ToString()); break;
                                        }
                                    }
                                }
                                else if (context.Request.HttpMethod == "POST")
                                {
                                    paths = path.Split('/');
                                    if (paths.Length > 0)
                                    {
                                        switch (paths[0].ToLower())
                                        {
                                            case "update": result = Data.Update(context); break;
                                        }
                                    }
                                }
                                else result = "Incorrect REQUEST METHOD";

                                break;

                            case "config":

                                var apiConfig = ApiConfiguration.Read();
                                if (apiConfig != null)
                                {
                                    string json = JSON.FromObject(apiConfig);
                                    if (json != null)
                                    {
                                        result = json;
                                    }
                                    else
                                    {
                                        result = "Error Reading API Configuration";
                                    }
                                }

                                break;
                        }
                    }
                }
            }
            catch (Exception ex) { Logger.Log("Error Processing Local Server Request :: " + ex.Message, LogLineType.Error); }

            return result;
        }


        private void StartBackupTimer()
        {
            try
            {
                var configs = DeviceConfiguration.ReadAll(FileLocations.Devices);
                if (configs != null)
                {
                    var backupInfos = Backup.Load(configs);
                    if (backupInfos != null && backupInfos.Count > 0)
                    {
                        Data.DeviceInfos.AddRange(backupInfos);
                    }
                }

                if (backupTimer != null) backupTimer.Stop();
                else
                {
                    backupTimer = new System.Timers.Timer();
                    backupTimer.Interval = BACKUP_INTERVAL;
                    backupTimer.Elapsed += BackupTimer_Elapsed;
                }

                backupTimer.Start();
            }
            catch (Exception ex) { Logger.Log("Error Starting Local Server Backup Timer :: " + ex.Message, LogLineType.Error); }
        }

        private void BackupTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Backup.Create(Data.DeviceInfos.ToList());
        }

        private void StopBackupTimer()
        {
            if (backupTimer != null) backupTimer.Stop();
        }


        private static class Data
        {
            public static List<API.Data.DeviceInfo> DeviceInfos = new List<API.Data.DeviceInfo>();

            public static string Get(string url)
            {
                string response = null;

                try
                {
                    var uri = new Uri(url);

                    // Get Devices Parameter
                    string json = HttpUtility.ParseQueryString(uri.Query).Get("devices");
                    if (!string.IsNullOrEmpty(json))
                    {
                        var devices = JSON.ToType<List<API.Data.DeviceListItem>>(json);
                        if (devices != null)
                        {
                            var deviceInfos = new List<API.Data.DeviceInfo>();

                            foreach (var device in devices)
                            {
                                var deviceInfo = DeviceInfos.Find(o => o.UniqueId == device.UniqueId);
                                if (deviceInfo != null) deviceInfos.Add(deviceInfo);
                            }

                            if (deviceInfos.Count > 0) response = API.Data.DeviceInfo.ListToJson(deviceInfos);
                        }
                    }
                    else
                    {
                        response = API.Data.DeviceInfo.ListToJson(DeviceInfos);
                    }
                }
                catch (Exception ex) { Logger.Log("Error Getting Local Server Data :: " + ex.Message, LogLineType.Error); }

                return response;
            }

            public static string Update(HttpListenerContext context)
            {
                string response = null;

                try
                {
                    using (var reader = new StreamReader(context.Request.InputStream))
                    {
                        var body = reader.ReadToEnd();

                        string json = HTTP.GetPostValue(body, "devices");
                        if (!string.IsNullOrEmpty(json))
                        {
                            var devices = JSON.ToType<List<API.Data.DeviceInfo>>(json);
                            if (devices != null && devices.Count > 0)
                            {
                                foreach (var device in devices)
                                {
                                    bool newInfo = false;

                                    int i = DeviceInfos.FindIndex(o => o.UniqueId == device.UniqueId);
                                    if (i < 0)
                                    {
                                        DeviceInfos.Add(device);
                                        i = DeviceInfos.FindIndex(o => o.UniqueId == device.UniqueId);
                                        newInfo = true;
                                    }

                                    var info = DeviceInfos[i];

                                    object obj = null;

                                    API.Data.StatusInfo status = null;
                                    obj = device.GetClass("status");
                                    if (obj != null)
                                    {
                                        info.AddClass("status", obj);
                                        status = (API.Data.StatusInfo)obj;
                                    }

                                    obj = device.GetClass("controller");
                                    if (obj != null) info.AddClass("controller", obj);

                                    // Get HourInfos for current day
                                    List<API.Data.HourInfo> hours = null;
                                    obj = info.GetClass("hours");
                                    if (obj != null)
                                    {
                                        info.RemoveClass("hours");
                                        hours = (List<API.Data.HourInfo>)obj;
                                    }

                                    // Add new HourInfo objects and then combine them into the current list
                                    obj = device.GetClass("hours");
                                    if (obj != null)
                                    {
                                        if (hours == null) hours = new List<API.Data.HourInfo>();

                                        hours.AddRange((List<API.Data.HourInfo>)obj);
                                    }

                                    if (hours != null)
                                    {
                                        hours = hours.FindAll(o => TestHourDate(o));
                                        hours = API.Data.HourInfo.CombineHours(hours);

                                        // Add Hours
                                        info.AddClass("hours", hours);

                                        // Add OEE
                                        var oee = API.Data.HourInfo.GetOeeInfo(hours);
                                        if (oee != null) info.AddClass("oee", oee);

                                        // Add Timers
                                        var timers = new API.Data.TimersInfo();
                                        timers.Total = hours.Select(o => o.TotalTime).Sum();

                                        timers.Active = hours.Select(o => o.Active).Sum();
                                        timers.Idle = hours.Select(o => o.Idle).Sum();
                                        timers.Alert = hours.Select(o => o.Alert).Sum();

                                        timers.Production = hours.Select(o => o.Production).Sum();
                                        timers.Setup = hours.Select(o => o.Setup).Sum();
                                        timers.Teardown = hours.Select(o => o.Teardown).Sum();
                                        timers.Maintenance = hours.Select(o => o.Maintenance).Sum();
                                        timers.ProcessDevelopment = hours.Select(o => o.ProcessDevelopment).Sum();

                                        info.AddClass("timers", timers);

                                        // Set Part Count
                                        //if (status != null)
                                        //{
                                        //    for (var x = 0; x < hours.Count; x++)
                                        //    {
                                        //        if (newInfo)
                                        //        {
                                        //            if (x < hours.Count - 1 || hours[x].TotalPieces == 0) status.PartCount += hours[x].TotalPieces;
                                        //        }
                                        //        else
                                        //        {
                                        //            status.PartCount += hours[x].TotalPieces;
                                        //        }
                                        //    }
                                        //}

                                        response = "Devices Updated Successfully";
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex) { Logger.Log("Error Updating Local Server Data :: " + ex.Message, LogLineType.Error); }

                return response;
            }

            private static bool TestHourDate(API.Data.HourInfo hourInfo)
            {
                // Probably a more elegant way of getting the Time Zone Offset could be done here
                int timeZoneOffset = Convert.ToInt32((DateTime.UtcNow - DateTime.Now).TotalHours);

                string currentLocalDay = DateTime.Now.ToString(API.Data.HourInfo.DateFormat);
                string currentUtcDay = DateTime.UtcNow.ToString(API.Data.HourInfo.DateFormat);

                // Get the adjusted hour based on the timezone
                int adjHourEnd = 24 - timeZoneOffset;

                if (currentLocalDay != currentUtcDay)
                {
                    return hourInfo.Date == currentUtcDay || (hourInfo.Date == currentLocalDay && hourInfo.Hour >= adjHourEnd);
                }
                else return hourInfo.Date == currentUtcDay;
            }
        }

    }
}
