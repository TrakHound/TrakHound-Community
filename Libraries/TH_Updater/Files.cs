// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.IO;
using System.Net;

using TH_Global;
using TH_Global.Functions;

namespace TH_Updater
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
            }
            return null;
        }

        private static string ExtractFiles(string path)
        {
            string extractPath = RunSetupExtract(path);
            if (extractPath != null)
            {
                string filesPath = extractPath + "\\program files\\TrakHound\\TrakHound";

                if (Directory.Exists(filesPath)) return filesPath;
            }

            return null;
        }

        private static string RunSetupExtract(string path)
        {
            string extractPath = CreateUpdatesPath();

            try
            {
                Logger.Log("Extracting Setup Files...");

                var info = new ProcessStartInfo();
                info.FileName = path;
                info.Arguments = "/a /s /v\"/qn TARGETDIR=\\\"" + extractPath + "\\\"\"";

                var process = new Process();
                process.StartInfo = info;
                process.Start();
                process.WaitForExit();

                Logger.Log("Setup Files Extracted Successfully");

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
