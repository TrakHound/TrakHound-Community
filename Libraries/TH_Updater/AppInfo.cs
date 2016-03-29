// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;

using TH_Global;

namespace TH_Updater
{
    /// <summary>
    /// Class to store the data from the 'appinfo' json file
    /// </summary>

    // Example AppInfo.json file:
    // 
    // "name": "trakhound-bundle",
    // "version": "1.9.0",
    // "releaseType": "[Beta]",
    // "buildDate": "3/28/2016",
    // "operatingSystem": "Windows XP, 7, 8, 10",
    // "downloadUrl": "http://www.feenux.com/trakhound/files/trakhound-install-beta-v1.9.0.exe",
    // "size": "8.02 MB",
    // "title": "TrakHound",
    // "subtitle": "Bundle",
    // "description": "TrakHound displays machine data that is collected using TrakHound Server.",				
    // "screenshots": "/trakhound/v3/downloads/images/client_dashboard_01.png;/trakhound/v3/downloads/images/client_device_manager_01.png;"

    public class AppInfo
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string ReleaseType { get; set; }
        public string BuildDate { get; set; }
        public string OperatingSystem { get; set; }
        public string DownloadUrl { get; set; }
        public string Size { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string Description { get; set; }
        public string ScreenShots { get; set; }

        public static AppInfo Get(string url)
        {
            // Create local path to download 'appinfo' file to
            string path = FileLocations.CreateTempPath();

            // Download File
            if (Files.Download(url, path))
            {
                // Parse File as AppInfo class
                var info = Parse(path);

                // Delete temp file
                if (File.Exists(path)) File.Delete(path);

                return info;
            }
            else return null;
        }

        private static AppInfo Parse(string path)
        {
            AppInfo result = null;

            try
            {
                if (File.Exists(path))
                {
                    using (var r = new StreamReader(path))
                    {
                        string json = r.ReadToEnd();

                        var serializer = new JsonSerializer();
                        try
                        {
                            result = (AppInfo)serializer.Deserialize(new JsonTextReader(new StringReader(json)), typeof(AppInfo));
                        }
                        catch
                        {
                            Console.WriteLine("Error During AppInfo File JSON Parse : " + path);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during Update Check ParseFile() : " + ex.Message);
            }

            return result;
        }
    }
}
