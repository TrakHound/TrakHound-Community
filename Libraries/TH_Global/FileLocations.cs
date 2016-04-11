// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

namespace TH_Global
{
    public static class FileLocations
    {

        public static string TrakHound = Environment.GetEnvironmentVariable("SystemDrive") + @"\TrakHound";

        public static string TrakHoundTemp = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\TrakHound\temp";

        public static string AppData = TrakHound + @"\appdata";
        public static string Databases = TrakHound + @"\Databases";
        public static string Devices = TrakHound + @"\Devices";
        public static string Plugins = TrakHound + @"\Plugins";
        public static string Updates = TrakHound + @"\Updates";

        // Logging
        public static string Logs = TrakHound + @"\Logs";
        public static string DebugLogs = Logs + @"\Debug";
        public static string ErrorLogs = Logs + @"\Error";
        public static string NotificationLogs = Logs + @"\Notification";
        public static string WarningLogs = Logs + @"\Warning";

        public static void CreateAllDirectories()
        {
            CreateAppDataDirectory();
            CreateDatabasesDirectory();
            CreateDevicesDirectory();
            CreateLogsDirectory();
            CreatePluginsDirectory();
            CreateUpdatesDirectory();
        }

        public static void CreateAppDataDirectory() { CreateDirectory(AppData, true); }
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
                Console.WriteLine("TH_Global.FileLocations.CreateDirectory() :: Exception :: " + ex.Message);
            }
        }

        #region "TempDirectory"

        public static string CreateTempPath()
        {
            CreateTempDirectory();

            string filename = Functions.String_Functions.RandomString(20);

            return TrakHoundTemp + "\\" + filename;
        }

        public static void CreateTempDirectory()
        {
            string tempDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            tempDirectory = tempDirectory + "\\" + "TrakHound";

            if (!Directory.Exists(tempDirectory)) Directory.CreateDirectory(tempDirectory);

            tempDirectory = tempDirectory + "\\" + "temp";

            if (!Directory.Exists(tempDirectory)) Directory.CreateDirectory(tempDirectory);
        }

        public static void CleanTempDirectory(int days)
        {
            if (days == 0)
            {
                // Delete everything
                if (!Directory.Exists(TrakHoundTemp)) Directory.Delete(TrakHoundTemp, true);
            }
            else
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
                                if (fileInfo.LastWriteTime < threshold) File.Delete(filePath);
                            }
                        }
                    }

                    CleanSubDirectories(TrakHoundTemp);
                }
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
