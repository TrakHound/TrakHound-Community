// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

using TrakHound.Logging;
using TrakHound.Tools;

namespace TrakHound
{
    public static class FileLocations
    {

        public static string TrakHound = Environment.GetEnvironmentVariable("SystemDrive") + Path.DirectorySeparatorChar.ToString() + "TrakHound";

        public static string TrakHoundTemp = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TrakHound", "temp");

        public static string AppData = Path.Combine(TrakHound, "appdata");
        public static string Backup = Path.Combine(TrakHound, "Backup");
        public static string Databases = Path.Combine(TrakHound, "Databases");
        public static string Devices = Path.Combine(TrakHound, "Devices");
        public static string Plugins = Path.Combine(TrakHound, "Plugins");
        public static string Storage = Path.Combine(TrakHound, "Storage");
        public static string Updates = Path.Combine(TrakHound, "Updates");

        // Logging
        public static string Logs = Path.Combine(TrakHound, "Logs");
        public static string DebugLogs = Path.Combine(Logs, "Debug");
        public static string ErrorLogs = Path.Combine(Logs, "Error");
        public static string NotificationLogs = Path.Combine(Logs, "Notification");
        public static string WarningLogs = Path.Combine(Logs, "Warning");

        public static void CreateAllDirectories()
        {
            CreateAppDataDirectory();
            CreateBackupDirectory();
            CreateDatabasesDirectory();
            CreateDevicesDirectory();
            CreateLogsDirectory();
            CreatePluginsDirectory();
            CreateStorageDirectory();
            CreateUpdatesDirectory();
        }

        public static void CreateAppDataDirectory() { CreateDirectory(AppData, true); }
        public static void CreateBackupDirectory() { CreateDirectory(Backup); }
        public static void CreateDatabasesDirectory() { CreateDirectory(Databases); }
        public static void CreateDevicesDirectory() { CreateDirectory(Devices); }
        public static void CreateLogsDirectory()
        {
            CreateDirectory(Logs);
            CreateDirectory(DebugLogs);
            CreateDirectory(ErrorLogs);
            CreateDirectory(NotificationLogs);
            CreateDirectory(WarningLogs);
        }
        public static void CreatePluginsDirectory() { CreateDirectory(Plugins); }
        public static void CreateStorageDirectory() { CreateDirectory(Storage); }
        public static void CreateUpdatesDirectory() { CreateDirectory(Updates, true); }


        private static void CreateDirectory(string path, bool hidden = false)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    DirectoryInfo info = Directory.CreateDirectory(path);
                    if (hidden) info.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception :: " + ex.Message);
            }
        }

        #region "TempDirectory"

        public static string CreateTempPath()
        {
            CreateTempDirectory();

            string filename = String_Functions.RandomString(20);

            return Path.Combine(TrakHoundTemp, filename);
        }

        public static void CreateTempDirectory()
        {
            string tempDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            tempDirectory = Path.Combine(tempDirectory, "TrakHound");

            if (!Directory.Exists(tempDirectory)) Directory.CreateDirectory(tempDirectory);

            tempDirectory = Path.Combine(tempDirectory, "temp");

            if (!Directory.Exists(tempDirectory)) Directory.CreateDirectory(tempDirectory);
        }

        public static void CleanTempDirectory(int days)
        {
            // Only delete files older than the days set in parameter
            DateTime threshold = DateTime.Now - TimeSpan.FromDays(days);

            if (Directory.Exists(TrakHoundTemp))
            {
                string[] filePaths = Directory.GetFiles(TrakHoundTemp, "*.*", SearchOption.AllDirectories);
                if (filePaths.Length > 0)
                {
                    foreach (var filePath in filePaths)
                    {
                        var fileInfo = new FileInfo(filePath);
                        if (fileInfo != null)
                        {
                            if (fileInfo.LastWriteTime < threshold || days == 0)
                            {
                                try
                                {
                                    File.Delete(filePath);
                                }
                                catch (IOException ex) { Logger.Log("IO Exception :: " + ex.Message); }
                                catch (Exception ex) { Logger.Log("Exception :: " + ex.Message); }
                            }
                        }
                    }
                }

                CleanSubDirectories(TrakHoundTemp);
            }
        }

        private static void CleanSubDirectories(string path)
        {
            if (Directory.Exists(path))
            {
                foreach (var directory in Directory.GetDirectories(path))
                {
                    CleanSubDirectories(directory);
                    if (Directory.GetFiles(directory).Length == 0 && Directory.GetDirectories(directory).Length == 0)
                    {
                        Directory.Delete(directory, false);
                    }
                }
            }
        }

        #endregion

    }
}
