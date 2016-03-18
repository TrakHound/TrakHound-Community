// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

namespace TH_Global
{
    public static class FileLocations
    {

        public static string TrakHound = Environment.GetEnvironmentVariable("SystemDrive") + @"\TrakHound";

        //public static string TrakHoundImages = Environment.GetEnvironmentVariable("SystemDrive") + @"\TrakHound\Images";
        //public static string TrakHoundImages = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\TrakHound\images";

        public static string TrakHoundTemp = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\TrakHound\temp";

        public static string Devices = TrakHound + @"\Devices";
        public static string Databases = TrakHound + @"\Databases";
        public static string Plugins = TrakHound + @"\Plugins";

        public static string AppData = TrakHound + @"\appdata";

        public static void CreateAppDataDirectory()
        {
            try
            {
                if (!Directory.Exists(AppData))
                {
                    DirectoryInfo info = Directory.CreateDirectory(AppData);
                    info.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("TH_Global.FileLocations.CreateAppDataDirectory() :: Exception :: " + ex.Message);
            }
        }

        #region "TempDirectory"

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
