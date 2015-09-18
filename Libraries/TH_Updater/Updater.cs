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
    public class Updater
    {

        public void Start(string url)
        {
            Thread worker = new Thread(new ParameterizedThreadStart(CheckVersion));
            worker.Start(url);
        }

        void CheckVersion(object url)
        {
            if (url != null)
            {
                //string directory = AppDomain.CurrentDomain.BaseDirectory + "temp";
                //if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

                string localFile = CreateLocalFileName();

                // Download File
                DownloadFile(url.ToString(), localFile);

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
        /// Downloads json file with Application Information
        /// </summary>
        static void DownloadFile(string url, string localFile)
        {
            try
            {
                WebClient webClient = new WebClient();
                webClient.DownloadFile(url, localFile); 
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during Update Check DownloadFile() : " + ex.Message);
            }
        }

        public class AppInfo
        {
            public string version;
            public string releaseType;
            public string buildDate;
            public string operatingSystem;
            public string url;
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

        #region "Local File Management"

        static Random random = new Random();
        static string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }

        static string CreateLocalFileName()
        {
            string directory = AppDomain.CurrentDomain.BaseDirectory + "temp";
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

            string file = RandomString(20);

            string path = directory + "\\" + file;

            if (File.Exists(path)) CreateLocalFileName();

            return path;
        }

        #endregion

    }
}
