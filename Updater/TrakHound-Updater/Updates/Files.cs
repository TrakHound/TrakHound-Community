// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;

using TH_Global;
using TH_Global.Functions;
using TH_Global.Updates;

namespace TrakHound_Updater
{
    public static class Files
    {

        private class WebClientWithData : WebClient
        {
            public AppInfo AppInfo { get; set; }
        }

        public static bool Download(AppInfo info, string path)
        {
            bool result = false;

            try
            {
                using (var webClient = new WebClientWithData())
                {
                    webClient.AppInfo = info;

                    webClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;
                    webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;

                    var syncObj = new object();
                    lock (syncObj)
                    {
                        webClient.DownloadFileAsync(new Uri(info.DownloadUrl), path, syncObj);

                        //This blocks the thread until download completes
                        Monitor.Wait(syncObj);
                    }
                }

                result = true;
            }
            catch (Exception ex)
            {
                Logger.Log("Error during Update DownloadFile() : " + ex.Message, Logger.LogLineType.Error);
            }

            return result;
        }

        private static void WebClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            lock (e.UserState)
            {
                //releases blocked thread
                Monitor.Pulse(e.UserState);
            }

            var webClient = (WebClientWithData)sender;

            var message = new WCF_Functions.MessageData();
            message.Id = "download_completed";
            message.Data01 = webClient.AppInfo.Name;

            MessageServer.SendCallback(message);
        }

        private static void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            var webClient = (WebClientWithData)sender;

            var message = new WCF_Functions.MessageData();
            message.Id = "download_progress_percentage";
            message.Data01 = webClient.AppInfo.Name;
            message.Data02 = e.ProgressPercentage;

            MessageServer.SendCallback(message);
        }

        public static string GetSetupFiles(AppInfo info)
        {
            string setupPath = CreateUpdatesPath() + ".exe";

            // Download Setup.exe (InstallShield) file
            if (Download(info, setupPath))
            {
                if (File.Exists(setupPath))
                {
                    return ExtractFiles(setupPath);
                }
                else Logger.Log("Setup File Doesn't Exist :: " + setupPath, Logger.LogLineType.Warning);
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
                else Logger.Log("Files Directory Not Found :: " + filesPath, Logger.LogLineType.Warning);
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
                Logger.Log("Extracting Setup Files...", Logger.LogLineType.Notification);

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

                Logger.Log("Setup Files Extracted Successfully", Logger.LogLineType.Notification);

                // Delete the Setup.Exe file after it has been extracted
                if (File.Exists(path)) File.Delete(path);

                // Delete the temp path used for caching the MSI file
                if (Directory.Exists(tempPath)) Directory.Delete(tempPath, true);

                return extractPath;
            }
            catch (Exception ex)
            {
                Logger.Log("Error Extracting Setup Files :: " + ex.Message, Logger.LogLineType.Error);
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
