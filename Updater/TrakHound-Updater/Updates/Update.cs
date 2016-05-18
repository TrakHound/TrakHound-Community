// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;

using System.Collections.Specialized;
using System.IO;

using TH_Global;
using TH_Global.Functions;
using TH_Global.Updates;

namespace TrakHound_Updater
{
    public static class Update
    {

        // Registry Key Values
        public const string UPDATE_PATH = "update_path";
        public const string INSTALL_PATH = "install_path";
        public const string UPDATE_URL = "update_url";
        public const string UPDATE_VERSION = "update_version";
        public const string INSTALL_VERSION = "install_version";
        public const string UPDATE_LAST_CHECKED = "update_last_checked";
        public const string UPDATE_LAST_INSTALLED = "update_last_installed";
        public const string UPDATE_HASH = "update_hash";

        public static AppInfo Get(string appInfoUrl, string appName)
        {
            Logger.Log("Getting AppInfo for " + appName + " @ " + appInfoUrl, Logger.LogLineType.Notification);

            var info = AppInfo.Get(appInfoUrl);
            if (info != null)
            {
                Logger.Log("AppInfo retrieved for " + info.Name, Logger.LogLineType.Notification);

                string installed = InstalledVersion.Get(info);
                string update = info.Version;
                string queued = UpdateVersion.Get(info);

                UpdateLastChecked.Set(info);

                if (CheckUpdateVerification(info))
                {
                    if (UpdateNeeded(update, installed, queued))
                    {
                        Logger.Log("Downloading Update Files (" + info.Size + ")...", Logger.LogLineType.Notification);
                        string filesPath = Files.GetSetupFiles(info);
                        if (filesPath != null)
                        {
                            UpdatePath.Set(info, filesPath);
                            UpdateVersion.Set(info);
                            Logger.Log("Registry Keys Updated", Logger.LogLineType.Notification);

                            var message = new WCF_Functions.MessageData();
                            message.Id = "update_ready";
                            message.Data01 = info.Name;
                            message.Data02 = info.Version;
                            MessageServer.SendCallback(message);
                        }
                        else
                        {
                            Logger.Log("Error during GetSetupFiles()", Logger.LogLineType.Error);

                            var message = new WCF_Functions.MessageData();
                            message.Id = "error";
                            message.Data01 = info.Name;
                            message.Data02 = "Error during File Download/Extract";
                            MessageServer.SendCallback(message);
                        }
                    }
                    else
                    {
                        Logger.Log(info.Name + " Up to Date", Logger.LogLineType.Notification);

                        var message = new WCF_Functions.MessageData();
                        message.Id = "up_to_date";
                        message.Data01 = info.Name;
                        MessageServer.SendCallback(message);

                        UpdatePath.Delete(info);
                        UpdateVersion.Delete(info);
                    }
                }
                else
                {
                    Logger.Log(info.Name + " Verification Failed", Logger.LogLineType.Notification);

                    var message = new WCF_Functions.MessageData();
                    message.Id = "error";
                    message.Data01 = info.Name;
                    message.Data02 = "Verification Failed." + Environment.NewLine + "Please Contact for more information (info@trakhound.org).";
                    MessageServer.SendCallback(message);

                    UpdatePath.Delete(info);
                    UpdateVersion.Delete(info);
                }
            }
            else
            {
                Logger.Log("Error retrieving AppInfo file @ " + appInfoUrl, Logger.LogLineType.Notification);

                var message = new WCF_Functions.MessageData();
                message.Id = "error";
                message.Data01 = appName;
                message.Data02 = "No Update Information Found";
                MessageServer.SendCallback(message);
            }

            return info;
        }

        private static bool CheckUpdateVerification(AppInfo info)
        {
            if (!String.IsNullOrEmpty(info.VerificationUrl))
            {
                string hash = UpdateHash.Get(info.Name);
                if (hash != null)
                {
                    var values = new NameValueCollection();
                    values["appname"] = info.Name;
                    values["hash"] = hash;

                    string url = info.VerificationUrl;

                    string responseString = TH_Global.Web.HTTP.POST(url, values);
                    if (responseString != null)
                    {
                        return ProcessVerificationResponse(responseString);
                    }
                }

                return false;
            }
            else return true;
        }

        private static string FormatVerficationResponse(string s)
        {
            return s.Replace("/n", "").Trim();
        }

        private static bool ProcessVerificationResponse(string s)
        {
            bool result = false;

            string response = FormatVerficationResponse(s);
            switch (response)
            {
                case "1": result = true; break;
                case "true": result = true; break;
                case "True": result = true; break;
                case "yes": result = true; break;
                case "Yes": result = true; break;
            }

            return result;
        }



        public static void Apply(AppInfo info)
        {
            Apply(info.Name);
        }

        public static void Apply(string appName)
        {
            Logger.Log("Applying Update..", Logger.LogLineType.Notification);

            string updatePath = UpdatePath.Get(appName);
            if (updatePath != null && Directory.Exists(updatePath))
            {
                WaitForClientClose();

                // Check to see if Server is Running
                bool serverRunning = false;
                var serverStatus = Service_Functions.GetServiceStatus(ApplicationNames.TRAKHOUND_SERVER_SEVICE_NAME);
                if (serverStatus == ServiceStatus.Running) serverRunning = true;

                if (StopServerService())
                {
                    string installPath = InstallDirectory.Get(appName);
                    if (installPath != null)
                    {
                        Logger.Log("Updating Files...", Logger.LogLineType.Notification);

                        // Update Files by copying entire folder
                        Files.DirectoryCopy(updatePath, installPath, true, true);

                        Logger.Log("Files Updated", Logger.LogLineType.Notification);

                        string version = UpdateVersion.Get(appName);
                        InstalledVersion.Set(appName, version);
                        UpdateLastInstalled.Set(appName);

                        Logger.Log("Installed Version Registry Key Set", Logger.LogLineType.Notification);

                        if (serverRunning) StartServerService();
                    }

                    // Clean up
                    FileSystem_Functions.DeleteDirectory(updatePath);
                    UpdatePath.Delete(appName);
                    UpdateVersion.Delete(appName);
                }
                else
                {
                    Logger.Log("Error: TrakHound Server Service could not be stopped", Logger.LogLineType.Error);
                }
            }
            else Logger.Log("Update Path Not Found : Aborting Update..", Logger.LogLineType.Warning);
        }


        public static void ClearAll()
        {
            ClearRegistryKeys();
            ClearUpdateFiles();
        }

        public static void ClearRegistryKeys()
        {
            string[] names = Registry_Functions.GetKeyNames();
            if (names != null)
            {
                foreach (var name in names)
                {
                    Registry_Functions.DeleteValue(UPDATE_PATH, name);
                    Registry_Functions.DeleteValue(UPDATE_VERSION, name);
                    Registry_Functions.DeleteValue(INSTALL_VERSION, name);
                }
            }

            Logger.Log("Update Registy Keys Cleared Successfully!", Logger.LogLineType.Notification);
        }

        public static void ClearUpdateFiles()
        {
            FileSystem_Functions.DeleteDirectory(FileLocations.Updates);
            FileLocations.CreateUpdatesDirectory();

            Logger.Log("Update Queue Files Cleared Successfully!", Logger.LogLineType.Notification);
        }

        #region "Application Interaction"

        private static bool StartServerService()
        {
            bool result = true;

            var service = Service_Functions.GetServiceController(ApplicationNames.TRAKHOUND_SERVER_SEVICE_NAME);
            if (service != null)
            {
                Logger.Log("Starting Server Service...", Logger.LogLineType.Notification);
                service.Start();
                service.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Running, new TimeSpan(0, 0, 30));
                if (service.Status != System.ServiceProcess.ServiceControllerStatus.Running) result = false;
                else Logger.Log("Server Service Started", Logger.LogLineType.Notification);
            }

            return result;
        }

        private static bool StopServerService()
        {
            bool result = true;

            var service = Service_Functions.GetServiceController(ApplicationNames.TRAKHOUND_SERVER_SEVICE_NAME);
            if (service != null)
            {
                Logger.Log("Stopping Server Service...", Logger.LogLineType.Notification);
                if (service.Status == System.ServiceProcess.ServiceControllerStatus.Running) service.Stop();
                service.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Stopped, new TimeSpan(0, 0, 30));
                if (service.Status != System.ServiceProcess.ServiceControllerStatus.Stopped) result = false;
                else Logger.Log("Server Service Stopped", Logger.LogLineType.Notification);
            }

            return result;
        }

        private static void WaitForClientClose()
        {
            Process[] processes = Process.GetProcessesByName(ApplicationNames.TRAKHOUND_CLIENT_PROCESS_NAME);
            foreach (var process in processes)
            {
                Logger.Log("Waiting for " + process.ProcessName + " to Close...", Logger.LogLineType.Notification);
                process.WaitForExit();
                Logger.Log(process.ProcessName + " Closed", Logger.LogLineType.Notification);
            }
        }

        #endregion

        #region "Registry"

        private static string CreateKeyName(AppInfo info)
        {
            return CreateKeyName(info.Name);
        }

        private static string CreateKeyName(string appName)
        {
            if (appName != null)
            {
                return appName.Replace(' ','_').ToLower();
            }
            return null;
        }

        private static class UpdatePath
        {
            public static string Get(AppInfo info) { return Get(info.Name); }
            public static void Set(AppInfo info, string path) { Set(info.Name, path); }
            public static void Delete(AppInfo info) { Delete(info.Name); }

            public static string Get(string appName)
            {
                string keyName = CreateKeyName(appName);
                if (keyName != null) return Registry_Functions.GetValue(UPDATE_PATH, keyName);

                return null;
            }

            public static void Set(string appName, string path)
            {
                string keyName = CreateKeyName(appName);
                if (keyName != null) Registry_Functions.SetKey(UPDATE_PATH, path, keyName);
            }

            public static void Delete(string appName)
            {
                string keyName = CreateKeyName(appName);
                if (keyName != null) Registry_Functions.DeleteValue(UPDATE_PATH, keyName);
            }
        }
        
        private static class UpdateVersion
        {
            public static string Get(AppInfo info) { return Get(info.Name); }
            public static void Set(AppInfo info) { Set(info.Name, info.Version); }
            public static void Delete(AppInfo info) { Delete(info.Name); }

            public static string Get(string appName)
            {
                string keyName = CreateKeyName(appName);
                if (keyName != null) return Registry_Functions.GetValue(UPDATE_VERSION, keyName);

                return null;
            }

            public static void Set(string appName, string version)
            {
                string keyName = CreateKeyName(appName);
                if (keyName != null) Registry_Functions.SetKey(UPDATE_VERSION, version, keyName);
            }

            public static void Delete(string appName)
            {
                string keyName = CreateKeyName(appName);
                if (keyName != null) Registry_Functions.DeleteValue(UPDATE_VERSION, keyName);
            }
        }
        
        private static class InstalledVersion
        {
            public static string Get(AppInfo info) { return Get(info.Name); }
            public static void Set(AppInfo info) { Set(info.Name, info.Version); }
            public static void Delete(AppInfo info) { Delete(info.Name); }

            public static string Get(string appName)
            {
                string keyName = CreateKeyName(appName);
                if (keyName != null) return Registry_Functions.GetValue(INSTALL_VERSION, keyName);

                return null;
            }

            public static void Set(string appName, string version)
            {
                string keyName = CreateKeyName(appName);
                if (keyName != null) Registry_Functions.SetKey(INSTALL_VERSION, version, keyName);
            }

            public static void Delete(string appName)
            {
                string keyName = CreateKeyName(appName);
                if (keyName != null) Registry_Functions.DeleteValue(INSTALL_VERSION, keyName);
            }
        }

        private static class InstallDirectory
        {
            public static string Get(AppInfo info)
            {
                return Get(info.Name);
            }

            public static string Get(string appName)
            {
                string keyName = CreateKeyName(appName);
                if (keyName != null) return Registry_Functions.GetValue(INSTALL_PATH, keyName);

                return null;
            }
        }

        private static class UpdateUrl
        {
            public static string Get(AppInfo info) { return Get(info.Name); }
            public static void Set(AppInfo info) { Set(info.Name, info.AppInfoUrl); }

            public static string Get(string appName)
            {
                string keyName = CreateKeyName(appName);
                if (keyName != null) return Registry_Functions.GetValue(UPDATE_URL, keyName);

                return null;
            }

            public static void Set(string appName, string url)
            {
                string keyName = CreateKeyName(appName);
                if (keyName != null && !String.IsNullOrEmpty(url))
                {
                    Logger.Log("Update_URL Updated to : " + url, Logger.LogLineType.Notification);
                    Registry_Functions.SetKey(UPDATE_URL, url, keyName);
                }
            }
        }

        private static class UpdateLastChecked
        {
            public static string Get(AppInfo info) { return Get(info.Name); }
            public static void Set(AppInfo info) { Set(info.Name); }
            public static void Delete(AppInfo info) { Delete(info.Name); }

            public static string Get(string appName)
            {
                string keyName = CreateKeyName(appName);
                if (keyName != null) return Registry_Functions.GetValue(UPDATE_LAST_CHECKED, keyName);

                return null;
            }

            public static void Set(string appName)
            {
                string keyName = CreateKeyName(appName);
                if (keyName != null) Registry_Functions.SetKey(UPDATE_LAST_CHECKED, DateTime.Now.ToString(), keyName);
            }

            public static void Delete(string appName)
            {
                string keyName = CreateKeyName(appName);
                if (keyName != null) Registry_Functions.DeleteValue(UPDATE_LAST_CHECKED, keyName);
            }
        }

        private static class UpdateLastInstalled
        {
            public static string Get(AppInfo info) { return Get(info.Name); }
            public static void Set(AppInfo info) { Set(info.Name); }
            public static void Delete(AppInfo info) { Delete(info.Name); }

            public static string Get(string appName)
            {
                string keyName = CreateKeyName(appName);
                if (keyName != null) return Registry_Functions.GetValue(UPDATE_LAST_INSTALLED, keyName);

                return null;
            }

            public static void Set(string appName)
            {
                string keyName = CreateKeyName(appName);
                if (keyName != null) Registry_Functions.SetKey(UPDATE_LAST_INSTALLED, DateTime.Now.ToString(), keyName);
            }

            public static void Delete(string appName)
            {
                string keyName = CreateKeyName(appName);
                if (keyName != null) Registry_Functions.DeleteValue(UPDATE_LAST_INSTALLED, keyName);
            }
        }

        private static class UpdateHash
        {
            public static string Get(AppInfo info) { return Get(info.Name); }

            public static string Get(string appName)
            {
                string keyName = CreateKeyName(appName);
                if (keyName != null) return Registry_Functions.GetValue(UPDATE_HASH, keyName);

                return null;
            }
        }

        #endregion

        private static bool UpdateNeeded(string update, string installed, string queued)
        {
            bool result = true;

            if (installed != null)
            {
                Version installedVersion = GetVersionFromString(installed);
                Version updateVersion = GetVersionFromString(update);
                Version queuedVersion = GetVersionFromString(queued);

                if (installedVersion != null && updateVersion != null)
                {
                    if (installedVersion >= updateVersion || queuedVersion >= updateVersion) result = false;
                }
            }

            return result;
        }

        private static Version GetVersionFromString(string input)
        {
            Version version;
            if (Version.TryParse(input, out version)) return version;
            return null;
        }

    }
}
