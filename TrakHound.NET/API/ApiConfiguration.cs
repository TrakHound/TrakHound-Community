// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.Xml;

using TrakHound.Logging;

namespace TrakHound.API
{
    public class ApiConfiguration
    {
        private const string DEFAULT_API_HOST = "http://TrakHound.com/api/";
        public static Uri ApiHost = new Uri(DEFAULT_API_HOST);

        [JsonProperty("host")]
        public string Host { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }


        private const string CONFIG_FILENAME = "api_config.xml";
        private static string CONFIG_FILEPATH = System.IO.Path.Combine(FileLocations.TrakHound, CONFIG_FILENAME);

        public static void Set(ApiConfiguration apiConfig)
        {
            if (apiConfig != null && !string.IsNullOrEmpty(apiConfig.Host) && !string.IsNullOrEmpty(apiConfig.Path))
            {
                try
                {
                    var baseUri = new Uri(apiConfig.Host);

                    if (!apiConfig.Path.EndsWith("/")) apiConfig.Path += "/";

                    ApiHost = new Uri(baseUri, apiConfig.Path);
                }
                catch (Exception ex) { Logger.Log("API Configuration Error : Exception : " + ex.Message); }
            }
            else
            {
                ApiHost = new Uri(DEFAULT_API_HOST);
            }

            Logger.Log("TrakHound API Configuration Host set to " + ApiHost);
        }

        public static bool Create(ApiConfiguration config)
        {
            bool result = false;

            Remove();

            if (config != null)
            {
                var xml = CreateDocument(config);
                Tools.XML.Files.WriteDocument(xml, CONFIG_FILEPATH);
            }

            return result;
        }

        public static ApiConfiguration Read()
        {
            var result = new ApiConfiguration();

            if (File.Exists(CONFIG_FILEPATH))
            {
                try
                {
                    var xml = new XmlDocument();
                    xml.Load(CONFIG_FILEPATH);

                    foreach (XmlNode node in xml.DocumentElement.ChildNodes)
                    {
                        if (node.NodeType == XmlNodeType.Element)
                        {
                            if (node.InnerText != "")
                            {
                                Type c = typeof(ApiConfiguration);
                                PropertyInfo info = c.GetProperty(node.Name);

                                if (info != null)
                                {
                                    Type t = info.PropertyType;
                                    info.SetValue(result, Convert.ChangeType(node.InnerText, t), null);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex) { Logger.Log("Exception :: " + ex.Message); }
            }

            return result;
        }

        public static void Remove()
        {
            if (File.Exists(CONFIG_FILEPATH)) File.Delete(CONFIG_FILEPATH);
        }

        private static XmlDocument CreateDocument(ApiConfiguration config)
        {
            var result = new XmlDocument();

            XmlNode docNode = result.CreateXmlDeclaration("1.0", "UTF-8", null);
            result.AppendChild(docNode);

            XmlNode root = result.CreateElement("ApiConfiguration");
            result.AppendChild(root);

            foreach (var info in typeof(ApiConfiguration).GetProperties())
            {
                XmlNode node = result.CreateElement(info.Name);
                var val = info.GetValue(config, new object[] { });
                if (val != null) node.InnerText = val.ToString();
                root.AppendChild(node);
            }

            return result;
        }


        public class Monitor
        {
            public delegate void ApiConfigurationChanged_Handler(ApiConfiguration config);
            public event ApiConfigurationChanged_Handler ApiConfigurationChanged;

            public Monitor()
            {
                string dir = FileLocations.TrakHound;

                var watcher = new FileSystemWatcher(dir, CONFIG_FILENAME);
                watcher.Changed += File_Changed;
                watcher.Created += File_Changed;
                watcher.Deleted += File_Changed;
                watcher.EnableRaisingEvents = true;
            }

            private void File_Changed(object sender, FileSystemEventArgs e)
            {
                var apiConfig = Read();

                if (ApiConfigurationChanged != null) ApiConfigurationChanged(apiConfig);
            }
        }
    }
}
