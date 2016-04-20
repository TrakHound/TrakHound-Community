// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.IO;

using TH_Global;
using TH_Global.Functions;

namespace TrakHound_Updater
{
    public static class Update
    {

        public static AppInfo Get(string appInfoUrl)
        {
            Logger.Log("Getting AppInfo..");

            var info = AppInfo.Get(appInfoUrl);
            if (info != null)
            {
                string installed = GetInstalledVersion(info);
                string update = info.Version;
                string queued = GetUpdateVersion(info);

                if (UpdateNeeded(update, installed, queued))
                {
                    Logger.Log("Downloading Update Files (" + info.Size + ")...");
                    string filesPath = Files.GetSetupFiles(info.DownloadUrl);
                    if (filesPath != null)
                    {
                        SetUpdatePath(info, filesPath);
                        SetUpdateVersion(info);
                        Logger.Log("Registry Keys Updated");
                    }
                    else Logger.Log("Error during GetSetupFiles()");
                }
                else
                {
                    Logger.Log(info.Name + " Up to Date");

                    DeleteUpdatePath(info);
                    DeleteUpdateVersion(info);
                }
            }

            return info;
        }

        public static void Apply(AppInfo info)
        {
            Logger.Log("Applying Update..");

            string updatePath = GetUpdatePath(info);
            if (updatePath != null && Directory.Exists(updatePath))
            {
                WaitForClientClose();

                // Check to see if Server is Running
                bool serverRunning = false;
                var serverStatus = Service_Functions.GetServiceStatus(ApplicationNames.TRAKHOUND_SERVER_SEVICE_NAME);
                if (serverStatus == ServiceStatus.Running) serverRunning = true;

                if (StopServerService())
                {
                    string installPath = GetInstallDirectory(info);
                    if (installPath != null)
                    {
                        Logger.Log("Updating Files...");

                        // Update Files by copying entire folder
                        Files.DirectoryCopy(updatePath, installPath, true, true);

                        Logger.Log("Files Updated");

                        SetInstalledVersion(info);

                        Logger.Log("Installed Version Registry Key Set : " + info.Version);

                        if (serverRunning) StartServerService();
                    }

                    // Clean up
                    FileSystem_Functions.DeleteDirectory(updatePath);
                    DeleteUpdatePath(info);
                    DeleteUpdateVersion(info);
                }
                else
                {
                    Logger.Log("Error: TrakHound Server Service could not be stopped");
                }
            }
        }


        #region "Application Interaction"

        private static bool StartServerService()
        {
            bool result = true;

            var service = Service_Functions.GetServiceController(ApplicationNames.TRAKHOUND_SERVER_SEVICE_NAME);
            if (service != null)
            {
                Logger.Log("Starting Server Service...");
                service.Start();
                service.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Running, new TimeSpan(0, 0, 30));
                if (service.Status != System.ServiceProcess.ServiceControllerStatus.Running) result = false;
                else Logger.Log("Server Service Started");
            }

            return result;
        }

        private static bool StopServerService()
        {
            bool result = true;

            var service = Service_Functions.GetServiceController(ApplicationNames.TRAKHOUND_SERVER_SEVICE_NAME);
            if (service != null)
            {
                Logger.Log("Stopping Server Service...");
                if (service.Status == System.ServiceProcess.ServiceControllerStatus.Running) service.Stop();
                service.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Stopped, new TimeSpan(0, 0, 30));
                if (service.Status != System.ServiceProcess.ServiceControllerStatus.Stopped) result = false;
                else Logger.Log("Server Service Stopped");
            }

            return result;
        }

        private static void WaitForClientClose()
        {
            Process[] processes = Process.GetProcessesByName(ApplicationNames.TRAKHOUND_CLIENT_PROCESS_NAME);
            foreach (var process in processes)
            {
                Logger.Log("Waiting for " + process.ProcessName + " to Close...");
                process.WaitForExit();
                Logger.Log(process.ProcessName + " Closed");
            }
        }

        #endregion

        #region "Registry"

        private const string TRAKHOUND_BUNDLE = "trakhound-bundle";
        private const string TRAKHOUND_CLIENT = "trakhound-client";
        private const string TRAKHOUND_SERVER = "trakhound-server";


        private const string BUNDLE_UPDATE_PATH = "BUNDLE_UPDATE_PATH";
        private const string CLIENT_UPDATE_PATH = "CLIENT_UPDATE_PATH";
        private const string SERVER_UPDATE_PATH = "SERVER_UPDATE_PATH";

        private static string GetUpdatePath(AppInfo info)
        {
            if (info.Name.ToLower() == TRAKHOUND_BUNDLE)
            {
                return Registry.GetValue(BUNDLE_UPDATE_PATH);
            }
            else if (info.Name.ToLower() == TRAKHOUND_CLIENT)
            {
                return Registry.GetValue(CLIENT_UPDATE_PATH);
            }
            else if (info.Name.ToLower() == TRAKHOUND_SERVER)
            {
                return Registry.GetValue(SERVER_UPDATE_PATH);
            }

            return null;
        }

        private static void SetUpdatePath(AppInfo info, string path)
        {
            if (info.Name.ToLower() == TRAKHOUND_BUNDLE)
            {
                Registry.SetKey(BUNDLE_UPDATE_PATH, path);
            }
            else if (info.Name.ToLower() == TRAKHOUND_CLIENT)
            {
                Registry.SetKey(CLIENT_UPDATE_PATH, path);
            }
            else if (info.Name.ToLower() == TRAKHOUND_SERVER)
            {
                Registry.SetKey(SERVER_UPDATE_PATH, path);
            }     
        }

        private static void DeleteUpdatePath(AppInfo info)
        {
            if (info.Name.ToLower() == TRAKHOUND_BUNDLE)
            {
                Registry.DeleteValue(BUNDLE_UPDATE_PATH);
            }
            else if (info.Name.ToLower() == TRAKHOUND_CLIENT)
            {
                Registry.DeleteValue(CLIENT_UPDATE_PATH);
            }
            else if (info.Name.ToLower() == TRAKHOUND_SERVER)
            {
                Registry.DeleteValue(SERVER_UPDATE_PATH);
            }
        }


        private const string BUNDLE_UPDATE_VERSION = "BUNDLE_UPDATE_VERSION";
        private const string CLIENT_UPDATE_VERSION = "CLIENT_UPDATE_VERSION";
        private const string SERVER_UPDATE_VERSION = "SERVER_UPDATE_VERSION";

        private static string GetUpdateVersion(AppInfo info)
        {
            if (info.Name.ToLower() == TRAKHOUND_BUNDLE)
            {
                return Registry.GetValue(BUNDLE_UPDATE_VERSION);
            }
            else if (info.Name.ToLower() == TRAKHOUND_CLIENT)
            {
                return Registry.GetValue(CLIENT_UPDATE_VERSION);
            }
            else if (info.Name.ToLower() == TRAKHOUND_SERVER)
            {
                return Registry.GetValue(SERVER_UPDATE_VERSION);
            }

            return null;
        }

        private static void SetUpdateVersion(AppInfo info)
        {
            if (info.Name.ToLower() == TRAKHOUND_BUNDLE)
            {
                Registry.SetKey(BUNDLE_UPDATE_VERSION, info.Version);
            }
            else if (info.Name.ToLower() == TRAKHOUND_CLIENT)
            {
                Registry.SetKey(CLIENT_UPDATE_VERSION, info.Version);
            }
            else if (info.Name.ToLower() == TRAKHOUND_SERVER)
            {
                Registry.SetKey(SERVER_UPDATE_VERSION, info.Version);
            }
        }

        private static void DeleteUpdateVersion(AppInfo info)
        {
            if (info.Name.ToLower() == TRAKHOUND_BUNDLE)
            {
                Registry.DeleteValue(BUNDLE_UPDATE_VERSION);
            }
            else if (info.Name.ToLower() == TRAKHOUND_CLIENT)
            {
                Registry.DeleteValue(CLIENT_UPDATE_VERSION);
            }
            else if (info.Name.ToLower() == TRAKHOUND_SERVER)
            {
                Registry.DeleteValue(SERVER_UPDATE_VERSION);
            }
        }


        private const string BUNDLE_INSTALLED_VERSION = "BUNDLE_INSTALLED_VERSION";
        private const string CLIENT_INSTALLED_VERSION = "CLIENT_INSTALLED_VERSION";
        private const string SERVER_INSTALLED_VERSION = "SERVER_INSTALLED_VERSION";

        private static string GetInstalledVersion(AppInfo info)
        {
            if (info.Name.ToLower() == TRAKHOUND_BUNDLE)
            {
                return Registry.GetValue(BUNDLE_INSTALLED_VERSION);
            }
            else if (info.Name.ToLower() == TRAKHOUND_CLIENT)
            {
                return Registry.GetValue(CLIENT_INSTALLED_VERSION);
            }
            else if (info.Name.ToLower() == TRAKHOUND_SERVER)
            {
                return Registry.GetValue(SERVER_INSTALLED_VERSION);
            }

            return null;
        }

        private static void SetInstalledVersion(AppInfo info)
        {
            if (info.Name.ToLower() == TRAKHOUND_BUNDLE)
            {
                Registry.SetKey(BUNDLE_INSTALLED_VERSION, info.Version);
            }
            else if (info.Name.ToLower() == TRAKHOUND_CLIENT)
            {
                Registry.SetKey(CLIENT_INSTALLED_VERSION, info.Version);
            }
            else if (info.Name.ToLower() == TRAKHOUND_SERVER)
            {
                Registry.SetKey(SERVER_INSTALLED_VERSION, info.Version);
            }
        }

        private static void DeleteInstalledVersion(AppInfo info)
        {
            if (info.Name.ToLower() == TRAKHOUND_BUNDLE)
            {
                Registry.DeleteValue(BUNDLE_INSTALLED_VERSION);
            }
            else if (info.Name.ToLower() == TRAKHOUND_CLIENT)
            {
                Registry.DeleteValue(CLIENT_INSTALLED_VERSION);
            }
            else if (info.Name.ToLower() == TRAKHOUND_SERVER)
            {
                Registry.DeleteValue(SERVER_INSTALLED_VERSION);
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


        private const string BUNDLE_INSTALL_PATH = "BUNDLE_INSTALL_PATH";
        private const string CLIENT_INSTALL_PATH = "CLIENT_INSTALL_PATH";
        private const string SERVER_INSTALL_PATH = "SERVER_INSTALL_PATH";

        private static string GetInstallDirectory(AppInfo info)
        {
            string result = null;

            if (info.Name.ToLower() == TRAKHOUND_BUNDLE)
            {
                result = Registry.GetValue(BUNDLE_INSTALL_PATH);
            }
            else if (info.Name.ToLower() == TRAKHOUND_CLIENT)
            {
                result = Registry.GetValue(CLIENT_INSTALL_PATH);
            }
            else if(info.Name.ToLower() == TRAKHOUND_SERVER)
            {
                result = Registry.GetValue(SERVER_INSTALL_PATH);
            }

            return result;
        }

    }
}
