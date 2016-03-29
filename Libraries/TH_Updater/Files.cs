// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.IO;
using System.Net;

using TH_Global;

namespace TH_Updater
{
    static class Files
    {

        public static bool Download(string url, string path)
        {
            bool result = false;

            try
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadFile(url, path);
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
            string setupPath = FileLocations.CreateTempPath();

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
            string extractPath = FileLocations.CreateTempPath();

            try
            {
                var info = new ProcessStartInfo();
                info.FileName = path;
                info.Arguments = "/a /s /v\"/qn TARGETDIR=\\" + extractPath + "\\\"";

                var process = new Process();
                process.StartInfo = info;
                process.Start();
                process.WaitForExit();

                return extractPath;
            }
            catch (Exception ex)
            {
                Logger.Log("Error Extracting Setup Files :: " + ex.Message);
            }

            return null;
        }

    }
}
