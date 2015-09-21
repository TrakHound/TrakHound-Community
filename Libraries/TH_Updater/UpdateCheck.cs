using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using System.IO;
using System.Net;

using Newtonsoft.Json;

namespace TH_Updater
{
    public class UpdateCheck
    {
        /// <summary>
        /// Start the Check for a Software Update (starts seperate Thread)
        /// </summary>
        /// <param name="url">URL for the 'appinfo' file</param>
        public void Start(string url)
        {
            Thread worker = new Thread(new ParameterizedThreadStart(CheckVersion));
            worker.Start(url);
        }

        /// <summary>
        /// Performs the Update Check
        /// </summary>
        /// <param name="url">URL for the 'appinfo' file</param>
        void CheckVersion(object url)
        {
            if (url != null)
            {
                // Create local path to download 'appinfo' file to
                string localFile = Tools.CreateLocalFileName();

                // Download File
                Tools.DownloadFile(url.ToString(), localFile);

                // Parse File as AppInfo class
                AppInfo info = ParseFile(localFile);
                if (info != null)
                {
                    // Raise AppInfoReceived Event
                    if (AppInfoReceived != null) AppInfoReceived(info);
                }
            } 
        }

        public delegate void AppInfoReceived_Handler(AppInfo info);
        public event AppInfoReceived_Handler AppInfoReceived;

        /// <summary>
        /// Class to store the data from the 'appinfo' json file
        /// </summary>
        public class AppInfo
        {
            public string version;
            public string releaseType;
            public string buildDate;
            public string operatingSystem;
            public string downloadUrl;
            public string updateUrl;
            public string size;
        }

        static AppInfo ParseFile(string localFile)
        {
            AppInfo Result = null;

            try
            {
                if (File.Exists(localFile))
                {
                    using (StreamReader r = new StreamReader(localFile))
                    {
                        string json = r.ReadToEnd();

                        JsonSerializer serializer = new JsonSerializer();
                        try
                        {
                            Result = (AppInfo)serializer.Deserialize(new JsonTextReader(new StringReader(json)), typeof(AppInfo));
                        }
                        catch
                        {
                            Console.WriteLine("Error During AppInfo File JSON Parse : " + localFile);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during Update Check ParseFile() : " + ex.Message);
            }

            return Result;
        }

    }
}
