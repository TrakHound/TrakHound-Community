// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using NLog;

namespace TrakHound.Updates
{
    // Example AppInfo.json file:
    // 
    // "name": "trakhound-bundle",
    // "version": "1.9.0",
    // "releaseType": "[Beta]",
    // "buildDate": "3/28/2016",
    // "operatingSystem": "Windows XP, 7, 8, 10",
    // "downloadUrl": "http://www.feenux.com/trakhound/files/trakhound-install-beta-v1.9.0.exe",
    // "newAppInfoUrl": "",
    // "verificationUrl": "",
    // "size": "8.02 MB",
    // "title": "TrakHound",
    // "subtitle": "Bundle",
    // "description": "TrakHound displays machine data that is collected using TrakHound Server.",				
    // "screenshots": "/trakhound/v3/downloads/images/client_dashboard_01.png;/trakhound/v3/downloads/images/client_device_manager_01.png;"

    /// <summary>
    /// Class to store the data from the 'appinfo' json file used to contain information about an application
    /// </summary>
    public class AppInfo
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Name of the application
        /// Format: No Spaces (Use dashes instead), lowercase
        /// Example: trakhound-bundle
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// String representation of the application's version
        /// Example: 1.2.3.4
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Type of application release
        /// Examples: Beta, Release, Custom, etc.
        /// </summary>
        public string ReleaseType { get; set; }

        /// <summary>
        /// Date that the application was built
        /// Example: 7/4/1776
        /// </summary>
        public string BuildDate { get; set; }

        /// <summary>
        /// Types of operating systems this application is compatible with
        /// Examples: Windows XP, Windows 7, Linux, etc.
        /// </summary>
        public string OperatingSystem { get; set; }

        /// <summary>
        /// Url to the application's setup files
        /// </summary>
        public string DownloadUrl { get; set; }

        /// <summary>
        /// Url to the AppInfoUrl to use next time (used for Auto Updates)
        /// </summary>
        public string AppInfoUrl { get; set; }

        /// <summary>
        /// Url to the Verification file (used for Auto Updates).
        /// This file must return the string 'True', '1', or 'Yes' when successful.
        /// If left blank, the verification process will be skipped
        /// </summary>
        public string VerificationUrl { get; set; }

        /// <summary>
        /// A string representing the size of the application's setup files
        /// </summary>
        public string Size { get; set; }

        /// <summary>
        /// Title of the application
        /// Example: TrakHound
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Subtitle of the application (used to describe the specific package)
        /// Example: Bundle
        /// </summary>
        public string SubTitle { get; set; }

        /// <summary>
        /// A description of the application
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// A list of semi-colon delimited urls to images of the application
        /// </summary>
        public string ScreenShots { get; set; }


        /// <summary>
        /// Downloads the AppInfo using the given Url, parses the file, and returns the AppInfo object
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static AppInfo Get(string url)
        {
            // Create local path to download 'appinfo' file to
            string path = FileLocations.CreateTempPath();

            // Download File
            logger.Info("Downloading AppInfo File...");
            if (Download(url, path))
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
                    catch (Exception ex)
                    {
                        logger.Error(ex);
                    }
                }
            }

            return result;
        }

        private static bool Download(string url, string path)
        {
            bool result = false;

            try
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadFile(url, path);
                    logger.Info("Download Complete");
                }

                result = true;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return result;
        }
    }
}