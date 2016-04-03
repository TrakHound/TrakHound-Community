// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.IO;
using System.Net;

using TH_Global;
using TH_Global.Functions;

namespace TrakHound_Updater
{
    public static class Files
    {

        public static bool Download(string url, string path)
        {
            bool result = false;

            try
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadFile(url, path);
                    Logger.Log("Download Complete");
                }

                result = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during Update DownloadFile() : " + ex.Message);
            }

            return result;
        }

        
        public static string GetSetupFiles(string url)
        {
            string setupPath = CreateUpdatesPath() + ".exe";

            // Download Setup.exe (InstallShield) file
            if (Download(url, setupPath))
            {
                if (File.Exists(setupPath))
                {
                    return ExtractFiles(setupPath);
                }
                else Logger.Log("Setup File Doesn't Exist :: " + setupPath);
            }
            return null;
        }

        private static string ExtractFiles(string path)
        {
            string extractPath = RunSetupExtract(path);
            if (extractPath != null)
            {
                string filesPath = extractPath + "\\program files\\TrakHound\\TrakHound";

                if (Directory.Exists(filesPath))
                {
                    string copyPath = CreateUpdatesPath();

                    // Copy extracted files to a new folder in the root of C:\TrakHound\Updates
                    DirectoryCopy(filesPath, copyPath, true);

                    // Delete original directory
                    Directory.Delete(extractPath, true);

                    return copyPath;
                }
                else Logger.Log("Files Directory Not Found :: " + filesPath);
            }
            return null;
        }

        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs = false, bool overwrite = false)
        {
            // Get the subdirectories for the specified directory.
            var dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, overwrite);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs, overwrite);
                }
            }
        }

        private static string RunSetupExtract(string path)
        {
            string extractPath = CreateUpdatesPath();
            string tempPath = CreateUpdatesPath();

            try
            {
                Logger.Log("Extracting Setup Files...");

                string args = path + " /a /s /b\"" + tempPath + "\"" + " /v\"/qn TARGETDIR=\\\"" + extractPath + "\\\"\"";

                var info = new ProcessStartInfo();
                info.FileName = "cmd.exe";
                info.Arguments = "/C " + args;
                info.UseShellExecute = true;
                info.CreateNoWindow = true;
                info.ErrorDialog = false;
                info.Verb = "runas";
                info.WindowStyle = ProcessWindowStyle.Hidden;

                var process = new Process();
                process.StartInfo = info;
                process.Start();
                process.WaitForExit();

                Logger.Log("Setup Files Extracted Successfully");

                // Delete the Setup.Exe file after it has been extracted
                if (File.Exists(path)) File.Delete(path);

                // Delete the temp path used for caching the MSI file
                if (Directory.Exists(tempPath)) Directory.Delete(tempPath, true);

                return extractPath;
            }
            catch (Exception ex)
            {
                Logger.Log("Error Extracting Setup Files :: " + ex.Message);
            }

            return null;
        }

        public static string CreateUpdatesPath()
        {
            FileLocations.CreateUpdatesDirectory();

            string filename = String_Functions.RandomString(20);

            return FileLocations.Updates + "\\" + filename;
        }

    }
}
