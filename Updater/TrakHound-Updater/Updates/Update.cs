// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.IO;

using TH_Global;
using TH_Global.Functions;
using TH_Global.Updates;

namespace TrakHound_Updater
{
    public static class Update
    {

        public static AppInfo Get(string appInfoUrl)
        {
            Logger.Log("Getting AppInfo @ " + appInfoUrl, Logger.LogLineType.Notification);

            var info = AppInfo.Get(appInfoUrl);
            if (info != null)
            {
                Logger.Log("AppInfo retrieved for " + info.Name, Logger.LogLineType.Notification);

                string installed = InstalledVersion.Get(info);
                string update = info.Version;
                string queued = UpdateVersion.Get(info);

                UpdateUrl.Set(info);

                if (UpdateNeeded(update, installed, queued))
                {
                    Logger.Log("Downloading Update Files (" + info.Size + ")...", Logger.LogLineType.Notification);
                    string filesPath = Files.GetSetupFiles(info.DownloadUrl);
                    if (filesPath != null)
                    {
                        UpdatePath.Set(info, filesPath);
                        UpdateVersion.Set(info);
                        Logger.Log("Registry Keys Updated", Logger.LogLineType.Notification);
                    }
                    else Logger.Log("Error during GetSetupFiles()", Logger.LogLineType.Error);
                }
                else
                {
                    Logger.Log(info.Name + " Up to Date", Logger.LogLineType.Notification);

                    UpdatePath.Delete(info);
                    UpdateVersion.Delete(info);
                }
            }

            return info;
        }

        public static void Apply(AppInfo info)
        {
            Logger.Log("Applying Update..", Logger.LogLineType.Notification);

            string updatePath = UpdatePath.Get(info);
            if (updatePath != null && Directory.Exists(updatePath))
            {
                WaitForClientClose();

                // Check to see if Server is Running
                bool serverRunning = false;
                var serverStatus = Service_Functions.GetServiceStatus(ApplicationNames.TRAKHOUND_SERVER_SEVICE_NAME);
                if (serverStatus == ServiceStatus.Running) serverRunning = true;

                if (StopServerService())
                {
                    string installPath = InstallDirectory.Get(info);
                    if (installPath != null)
                    {
                        Logger.Log("Updating Files...", Logger.LogLineType.Notification);

                        // Update Files by copying entire folder
                        Files.DirectoryCopy(updatePath, installPath, true, true);

                        Logger.Log("Files Updated", Logger.LogLineType.Notification);

                        InstalledVersion.Set(info);

                        Logger.Log("Installed Version Registry Key Set : " + info.Version, Logger.LogLineType.Notification);

                        if (serverRunning) StartServerService();
                    }

                    // Clean up
                    FileSystem_Functions.DeleteDirectory(updatePath);
                    UpdatePath.Delete(info);
                    UpdateVersion.Delete(info);
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
            string[] names = Registry.GetKeyNames();
            if (names != null)
            {
                foreach (var name in names)
                {
                    Registry.DeleteValue(Registry.UPDATE_PATH, name);
                    Registry.DeleteValue(Registry.UPDATE_VERSION, name);
                    Registry.DeleteValue(Registry.INSTALL_VERSION, name);
                }
            }
        }

        public static void ClearUpdateFiles()
        {
            FileSystem_Functions.DeleteDirectory(FileLocations.Updates);
            FileLocations.CreateUpdatesDirectory();
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
            if (info.Name != null)
            {
                return info.Name.Replace(' ','_').ToLower();
            }
            return null;
        }

        private static class UpdatePath
        {
            public static string Get(AppInfo info)
            {
                string keyName = CreateKeyName(info);
                if (keyName != null) return Registry.GetValue(Registry.UPDATE_PATH, keyName);

                return null;
            }

            public static void Set(AppInfo info, string path)
            {
                string keyName = CreateKeyName(info);
                if (keyName != null) Registry.SetKey(Registry.UPDATE_PATH, path, keyName);
            }

            public static void Delete(AppInfo info)
            {
                string keyName = CreateKeyName(info);
                if (keyName != null) Registry.DeleteValue(Registry.UPDATE_PATH, keyName);
            }
        }
        
        private static class UpdateVersion
        {
            public static string Get(AppInfo info)
            {
                string keyName = CreateKeyName(info);
                if (keyName != null) return Registry.GetValue(Registry.UPDATE_VERSION, keyName);

                return null;
            }

            public static void Set(AppInfo info)
            {
                string keyName = CreateKeyName(info);
                if (keyName != null) Registry.SetKey(Registry.UPDATE_VERSION, info.Version, keyName);
            }

            public static void Delete(AppInfo info)
            {
                string keyName = CreateKeyName(info);
                if (keyName != null) Registry.DeleteValue(Registry.UPDATE_VERSION, keyName);
            }
        }
        
        private static class InstalledVersion
        {
            public static string Get(AppInfo info)
            {
                string keyName = CreateKeyName(info);
                if (keyName != null) return Registry.GetValue(Registry.INSTALL_VERSION, keyName);

                return null;
            }

            public static void Set(AppInfo info)
            {
                string keyName = CreateKeyName(info);
                if (keyName != null) Registry.SetKey(Registry.INSTALL_VERSION, info.Version, keyName);
            }

            public static void Delete(AppInfo info)
            {
                string keyName = CreateKeyName(info);
                if (keyName != null) Registry.DeleteValue(Registry.INSTALL_VERSION, keyName);
            }
        }

        private static class InstallDirectory
        {
            public static string Get(AppInfo info)
            {
                string keyName = CreateKeyName(info);
                if (keyName != null) return Registry.GetValue(Registry.INSTALL_PATH, keyName);

                return null;
            }
        }

        private static class UpdateUrl
        {
            public static void Set(AppInfo info)
            {
                string keyName = CreateKeyName(info);
                if (keyName != null && !String.IsNullOrEmpty(info.AppInfoUrl))
                {
                    Logger.Log("Update_URL Updated to : " + info.AppInfoUrl, Logger.LogLineType.Notification);
                    Registry.SetKey(Registry.UPDATE_URL, info.AppInfoUrl, keyName);
                }
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
