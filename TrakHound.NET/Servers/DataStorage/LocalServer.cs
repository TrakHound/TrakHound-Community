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

            try
            {
                listener = new HttpListener();
                listener.Prefixes.Add("http://localhost:" + PORT + "/api/");
                listener.Start();

                ThreadPool.QueueUserWorkItem((o) =>
                {
                    Console.WriteLine("TrakHound Data Server Started...");

                    while (listener.IsListening)
                    {
                        try
                        {
                            //var context = listener.GetContext();

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
                                catch (Exception ex)
                                {
                                    Logger.Log("Local Data Server :: Exception :: " + ex.Message, LogLineType.Warning);
                                }
                                finally
                                {
                                    // always close the stream
                                    ctx.Response.OutputStream.Close();
                                }
                            }, listener.GetContext());
                        }
                        catch (ObjectDisposedException ex)
                        {
                            Logger.Log("Local Data Server :: ObjectDisposedException :: " + ex.Message, LogLineType.Warning);
                        }
                        catch (HttpListenerException ex)
                        {
                            if (ex.ErrorCode != 995)
                            {
                                Logger.Log("Local Data Server :: HttpListenerException :: " + ex.ErrorCode + " :: " + ex.Message, LogLineType.Warning);
                            }
                        }
                        catch (InvalidOperationException ex)
                        {
                            Logger.Log("Local Data Server :: InvalidOperationException :: " + ex.Message, LogLineType.Warning);
                        }
                        catch (Exception ex)
                        {
                            Logger.Log("Local Data Server :: Exception :: " + ex.Message, LogLineType.Warning);
                        }
                    }
                });

                StartBackupTimer();
            }
            catch (Exception ex)
            {
                Logger.Log("Local Data Server :: Error starting server :: Exception :: " + ex.Message, LogLineType.Warning);
                Logger.Log("Local Data Server :: Error starting server :: Restarting Data Server in 5 Seconds..");

                Thread.Sleep(5000);

                Start();
            }
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

                            case "devices":

                                // Remove 'data' from path
                                path = path.Substring(paths[0].Length);

                                // Remove first forward slash
                                path = path.TrimStart('/');

                                if (context.Request.HttpMethod == "GET")
                                {
                                    paths = path.Split('/');
                                    if (paths.Length > 0)
                                    {
                                        switch (paths[0].ToLower())
                                        {
                                            case "list": result = Data.List(context.Request.Url.ToString()); break;
                                        }
                                    }
                                }

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
                if (backupTimer != null) backupTimer.Stop();
                else
                {
                    backupTimer = new System.Timers.Timer();
                    backupTimer.Interval = 10000;
                    backupTimer.Elapsed += BackupTimer_Elapsed;
                }

                backupTimer.Start();
            }
            catch (Exception ex) { Logger.Log("Error Starting Local Server Backup Timer :: " + ex.Message, LogLineType.Error); }
        }

        private void BackupTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var timer = (System.Timers.Timer)sender;
            timer.Interval = BACKUP_INTERVAL;

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
                                if (deviceInfo != null)
                                {
                                    deviceInfos.Add(deviceInfo);
                                }
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

            private static bool first = true;

            private static object _lock = new object();

            public static string Update(HttpListenerContext context)
            {
                string response = null;

                try
                {
                    lock (_lock)
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
                                    // Load backup data on first pass
                                    if (first)
                                    {
                                        var backupInfos = Backup.Load(devices.ToArray());
                                        if (backupInfos != null && backupInfos.Count > 0)
                                        {
                                            DeviceInfos.AddRange(backupInfos);
                                        }
                                    }

                                    foreach (var device in devices)
                                    {
                                        int i = DeviceInfos.FindIndex(o => o.UniqueId == device.UniqueId);
                                        if (i < 0)
                                        {
                                            DeviceInfos.Add(device);
                                            i = DeviceInfos.FindIndex(o => o.UniqueId == device.UniqueId);
                                        }

                                        var info = DeviceInfos[i];

                                        object obj = null;

                                        obj = device.GetClass("status");
                                        if (obj != null) { info.AddClass("status", obj); }

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

                                            response = "Devices Updated Successfully";
                                        }
                                    }

                                    first = false;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex) { Logger.Log("Error Updating Local Server Data :: " + ex.Message, LogLineType.Error); }

                return response;
            }

            public static string List(string url)
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
                                if (deviceInfo != null)
                                {
                                    var info = new API.Data.DeviceInfo();
                                    info.UniqueId = deviceInfo.UniqueId;
                                    info.Enabled = deviceInfo.Enabled;
                                    info.Index = deviceInfo.Index;
                                    info.Description = deviceInfo.Description;

                                    deviceInfos.Add(info);
                                }
                            }

                            if (deviceInfos.Count > 0) response = API.Data.DeviceInfo.ListToJson(deviceInfos);
                        }
                    }
                    else
                    {
                        var deviceInfos = new List<API.Data.DeviceInfo>();

                        foreach (var deviceInfo in DeviceInfos)
                        {
                            var info = new API.Data.DeviceInfo();
                            info.UniqueId = deviceInfo.UniqueId;
                            info.Enabled = deviceInfo.Enabled;
                            info.Index = deviceInfo.Index;
                            info.Description = deviceInfo.Description;

                            deviceInfos.Add(info);
                        }

                        if (deviceInfos.Count > 0) response = API.Data.DeviceInfo.ListToJson(deviceInfos);
                    }
                }
                catch (Exception ex) { Logger.Log("Error Getting Local Server Devices :: " + ex.Message, LogLineType.Error); }

                return response;
            }

            private static bool TestHourDate(API.Data.HourInfo hourInfo)
            {
                // Probably a more elegant way of getting the Time Zone Offset could be done here
                int timeZoneOffset = Convert.ToInt32((DateTime.UtcNow - DateTime.Now).TotalHours);

                string fromDate = DateTime.Now.ToString(API.Data.HourInfo.DateFormat); ;
                int fromHour = timeZoneOffset;

                string toDate = DateTime.Now.AddDays(1).ToString(API.Data.HourInfo.DateFormat);
                int toHour = 24 - timeZoneOffset;

                if ((hourInfo.Date == fromDate && hourInfo.Hour >= fromHour) ||
                    (hourInfo.Date == toDate && hourInfo.Hour <= toHour)) return true;
                else return false;
            }
        }

        //#region "Device Monitor"

        ///// <summary>
        ///// Devices Monitor is used to monitor when devices are Changed, Added, or Removed.
        ///// 'Changed' includes whether device was Enabled or Disabled.
        ///// Monitor runs at a fixed interval of 5 seconds and compares Devices with list of tables for current user
        ///// </summary>

        //private Thread deviceMonitorThread;
        //private ManualResetEvent monitorstop = null;

        //void LoadDevices()
        //{
        //    Data.DeviceInfos.Clear();

        //    DevicesMonitor_Initialize();
        //}

        //void AddDevice(DeviceConfiguration config)
        //{
        //    var deviceInfo = new API.Data.DeviceInfo();

        //    deviceInfo.UniqueId = config.UniqueId;
        //    deviceInfo.Enabled = config.Enabled;
        //    deviceInfo.Index = config.Index;

        //    deviceInfo.Description = config.Description;

        //    Data.DeviceInfos.Add(deviceInfo);
        //}

        //void DevicesMonitor_Initialize()
        //{
        //    if (deviceMonitorThread != null) deviceMonitorThread.Abort();

        //    deviceMonitorThread = new Thread(new ThreadStart(DevicesMonitor_Start));
        //    deviceMonitorThread.Start();
        //}

        //void DevicesMonitor_Start()
        //{
        //    monitorstop = new ManualResetEvent(false);

        //    while (!monitorstop.WaitOne(0, true))
        //    {
        //        DevicesMonitor_Worker();

        //        Thread.Sleep(2000);
        //    }
        //}

        //void DevicesMonitor_Stop()
        //{
        //    if (monitorstop != null) monitorstop.Set();
        //    if (deviceMonitorThread != null) deviceMonitorThread.Abort();
        //}

        //void DevicesMonitor_Worker()
        //{
        //    CheckLocalDevices();
        //}


        //private void CheckLocalDevices()
        //{
        //    // Retrieves a list of devices by reading the local 'Devices' folder
        //    var configs = DeviceConfiguration.ReadAll(FileLocations.Devices).ToList();
        //    if (configs != null)
        //    {
        //        if (configs.Count > 0)
        //        {
        //            foreach (DeviceConfiguration config in configs)
        //            {
        //                if (config != null)
        //                {
        //                    int i = Data.DeviceInfos.FindIndex(x => x.UniqueId == config.UniqueId);
        //                    if (i >= 0) // Device is already part of list
        //                    {
        //                        var device = Data.DeviceInfos[i];
        //                        if (config.Enabled)
        //                        {
        //                            device.Index = config.Index;
        //                            device.Description = config.Description;
        //                        }
        //                        else Data.DeviceInfos.RemoveAt(i);
        //                    }
        //                    else // Add Device
        //                    {
        //                        if (config.Enabled) AddDevice(config);
        //                    }
        //                }
        //            }

        //            // Find devices that were removed
        //            foreach (var device in Data.DeviceInfos.ToList())
        //            {
        //                if (!configs.Exists(x => x.UniqueId == device.UniqueId))
        //                {
        //                    int i = Data.DeviceInfos.FindIndex(x => x.UniqueId == device.UniqueId);
        //                    if (i >= 0) Data.DeviceInfos.RemoveAt(i);
        //                }
        //            }
        //        }
        //        else RemoveAllDevices();
        //    }
        //    else
        //    {
        //        RemoveAllDevices();
        //    }
        //}

        //private void RemoveAllDevices()
        //{
        //    if (Data.DeviceInfos != null)
        //    {
        //        Data.DeviceInfos.Clear();
        //    }
        //}

        //#endregion

    }
}
