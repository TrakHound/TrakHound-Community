// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.IO;

using TH_Global;
using TH_Global.Functions;

namespace TH_Updater
{
    public static class Update
    {
        public static AppInfo Get(string appInfoUrl)
        {
            var info = AppInfo.Get(appInfoUrl);
            if (info != null)
            {
                string installed = GetInstalledVersion(info);
                string update = info.Version;

                if (UpdateNeeded(update, installed))
                {
                    Logger.Log("Downloading Update Files (" + info.Size + ")...");
                    string filesPath = Files.GetSetupFiles(info.DownloadUrl);
                    if (filesPath != null)
                    {
                        SetUpdatePath(info, filesPath);
                        SetUpdateVersion(info);
                        Logger.Log("Registry Keys Updated");
                    }
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

        public static void Run(AppInfo info)
        {
            string updatePath = GetUpdatePath(info);
            if (updatePath != null && Directory.Exists(updatePath))
            {
                WaitForClientClose();

                if (StopServerService())
                {
                    string installPath = GetInstallDirectory(info);
                    if (installPath != null)
                    {
                        foreach (var file in Directory.GetFiles(updatePath))
                        {
                            string filename = Path.GetFileName(file);
                            string destination = installPath + filename;

                            Logger.Log("Updating File : " + filename);

                            File.Copy(file, destination, true);
                        }

                        SetInstalledVersion(info);

                        Logger.Log("Installed Version Registry Key Set : " + info.Version);

                        StartServerService();
                    }

                    DeleteUpdatePath(info);
                    DeleteUpdateVersion(info);
                }
                else
                {
                    Logger.Log("Error: TrakHound Server Service could not be stopped");
                }
            }
        }

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

        #region "Registry"

        private static string GetUpdatePath(AppInfo info)
        {
            if (info.Name.ToLower() == "trakhound-bundle")
            {
                return Registry.GetKey("BUNDLE_UPDATE_PATH");
            }
            else if (info.Name.ToLower() == "trakhound-client")
            {
                return Registry.GetKey("CLIENT_UPDATE_PATH");
            }
            else if (info.Name.ToLower() == "trakhound-server")
            {
                return Registry.GetKey("SERVER_UPDATE_PATH");
            }

            return null;
        }

        private static void SetUpdatePath(AppInfo info, string path)
        {
            if (info.Name.ToLower() == "trakhound-bundle")
            {
                Registry.SetKey("BUNDLE_UPDATE_PATH", path);
            }
            else if (info.Name.ToLower() == "trakhound-client")
            {
                Registry.SetKey("CLIENT_UPDATE_PATH", path);
            }
            else if (info.Name.ToLower() == "trakhound-server")
            {
                Registry.SetKey("SERVER_UPDATE_PATH", path);
            }     
        }

        private static void DeleteUpdatePath(AppInfo info)
        {
            if (info.Name.ToLower() == "trakhound-bundle")
            {
                Registry.DeleteValue("BUNDLE_UPDATE_PATH");
            }
            else if (info.Name.ToLower() == "trakhound-client")
            {
                Registry.DeleteValue("CLIENT_UPDATE_PATH");
            }
            else if (info.Name.ToLower() == "trakhound-server")
            {
                Registry.DeleteValue("SERVER_UPDATE_PATH");
            }
        }


        private static string GetUpdateVersion(AppInfo info)
        {
            if (info.Name.ToLower() == "trakhound-bundle")
            {
                return Registry.GetKey("BUNDLE_VERSION_PATH");
            }
            else if (info.Name.ToLower() == "trakhound-client")
            {
                return Registry.GetKey("CLIENT_VERSION_PATH");
            }
            else if (info.Name.ToLower() == "trakhound-server")
            {
                return Registry.GetKey("SERVER_VERSION_PATH");
            }

            return null;
        }

        private static void SetUpdateVersion(AppInfo info)
        {
            if (info.Name.ToLower() == "trakhound-bundle")
            {
                Registry.SetKey("BUNDLE_UPDATE_VERSION", info.Version);
            }
            else if (info.Name.ToLower() == "trakhound-client")
            {
                Registry.SetKey("CLIENT_UPDATE_VERSION", info.Version);
            }
            else if (info.Name.ToLower() == "trakhound-server")
            {
                Registry.SetKey("SERVER_UPDATE_VERSION", info.Version);
            }
        }

        private static void DeleteUpdateVersion(AppInfo info)
        {
            if (info.Name.ToLower() == "trakhound-bundle")
            {
                Registry.DeleteValue("BUNDLE_UPDATE_VERSION");
            }
            else if (info.Name.ToLower() == "trakhound-client")
            {
                Registry.DeleteValue("CLIENT_UPDATE_VERSION");
            }
            else if (info.Name.ToLower() == "trakhound-server")
            {
                Registry.DeleteValue("SERVER_UPDATE_VERSION");
            }
        }


        private static string GetInstalledVersion(AppInfo info)
        {
            if (info.Name.ToLower() == "trakhound-bundle")
            {
                return Registry.GetKey("BUNDLE_INSTALLED_VERSION");
            }
            else if (info.Name.ToLower() == "trakhound-client")
            {
                return Registry.GetKey("CLIENT_INSTALLED_VERSION");
            }
            else if (info.Name.ToLower() == "trakhound-server")
            {
                return Registry.GetKey("SERVER_INSTALLED_VERSION");
            }

            return null;
        }

        private static void SetInstalledVersion(AppInfo info)
        {
            if (info.Name.ToLower() == "trakhound-bundle")
            {
                Registry.SetKey("BUNDLE_INSTALLED_VERSION", info.Version);
            }
            else if (info.Name.ToLower() == "trakhound-client")
            {
                Registry.SetKey("CLIENT_INSTALLED_VERSION", info.Version);
            }
            else if (info.Name.ToLower() == "trakhound-server")
            {
                Registry.SetKey("SERVER_INSTALLEDVERSION", info.Version);
            }
        }

        private static void DeleteInstalledVersion(AppInfo info)
        {
            if (info.Name.ToLower() == "trakhound-bundle")
            {
                Registry.DeleteValue("BUNDLE_INSTALLED_VERSION");
            }
            else if (info.Name.ToLower() == "trakhound-client")
            {
                Registry.DeleteValue("CLIENT_INSTALLED_VERSION");
            }
            else if (info.Name.ToLower() == "trakhound-server")
            {
                Registry.DeleteValue("SERVER_INSTALLED_VERSION");
            }
        }

        #endregion

        private static bool UpdateNeeded(string update, string installed)
        {
            bool result = true;

            if (installed != null)
            {
                Version installedVersion = GetVersionFromString(installed);
                Version updateVersion = GetVersionFromString(update);

                if (installedVersion != null && updateVersion != null)
                {
                    if (installedVersion >= updateVersion) result = false;
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

        private static string GetInstallDirectory(AppInfo info)
        {
            string result = null;

            if (info.Name.ToLower() == "trakhound-bundle")
            {
                result = Registry.GetKey("CLIENT_INSTALL_PATH");
            }
            else
            {
                result = Registry.GetKey("SERVER_INSTALL_PATH");
            }

            return result;
        }

    }
}
