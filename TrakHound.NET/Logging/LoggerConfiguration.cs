//  Copyright 2016 Feenux LLC
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

using System;
using System.IO;
using System.Reflection;
using System.Xml;

using TrakHound.Tools.XML;

namespace TrakHound.Logging
{
    public class LoggerConfiguration
    {
        public LoggerConfiguration()
        {
            QueueWriteInterval = 10000;

            Debug = false;
            Error = false;
            Notification = false;
            Warning = false;

            DebugRecycleDays = 7;
            ErrorRecycleDays = 7;
            NotificationRecycleDays = 1;
            WarningRecycleDays = 1;
        }

        /// <summary>
        /// Sets the interval (in ms) that the queue writes to the log file (Minimum is 500ms)
        /// </summary>
        public int QueueWriteInterval { get; set; }


        /// <summary>
        /// Toggle Debug data collection On/Off
        /// </summary>
        public bool Debug { get; set; }

        /// <summary>
        /// Toggle Error data collection On/Off
        /// </summary>
        public bool Error { get; set; }

        /// <summary>
        /// Toggle Notification data collection On/Off
        /// </summary>
        public bool Notification { get; set; }

        /// <summary>
        /// Toggle Warning data collection On/Off
        /// </summary>
        public bool Warning { get; set; }


        /// <summary>
        /// Set number of days to keep Debug files
        /// </summary>
        public int DebugRecycleDays { get; set; }

        /// <summary>
        /// Set number of days to keep Error files
        /// </summary>
        public int ErrorRecycleDays { get; set; }

        /// <summary>
        /// Set number of days to keep Notification files
        /// </summary>
        public int NotificationRecycleDays { get; set; }

        /// <summary>
        /// Set number of days to keep Warning files
        /// </summary>
        public int WarningRecycleDays { get; set; }


        #region "Configuration File"

        public const string CONFIG_FILENAME = "logger_config.xml";
        public static string ConfigFilePath = Path.Combine(FileLocations.Logs, CONFIG_FILENAME);
        //public static string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CONFIG_FILENAME);

        public static bool Create(LoggerConfiguration config)
        {
            bool result = false;

            Remove();

            if (config != null)
            {
                var xml = CreateDocument(config);
                Files.WriteDocument(xml, ConfigFilePath);
            }

            return result;
        }

        public static LoggerConfiguration Read()
        {
            var result = new LoggerConfiguration();

            if (File.Exists(ConfigFilePath))
            {
                try
                {
                    var xml = new XmlDocument();
                    xml.Load(ConfigFilePath);

                    foreach (XmlNode node in xml.DocumentElement.ChildNodes)
                    {
                        if (node.NodeType == XmlNodeType.Element)
                        {
                            if (node.InnerText != "")
                            {
                                Type Settings = typeof(LoggerConfiguration);
                                PropertyInfo info = Settings.GetProperty(node.Name);

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
            if (File.Exists(ConfigFilePath)) File.Delete(ConfigFilePath);
        }


        private static XmlDocument CreateDocument(LoggerConfiguration config)
        {
            var result = new XmlDocument();

            XmlNode docNode = result.CreateXmlDeclaration("1.0", "UTF-8", null);
            result.AppendChild(docNode);

            XmlNode root = result.CreateElement("LoggerConfiguration");
            result.AppendChild(root);

            foreach (var info in typeof(LoggerConfiguration).GetProperties())
            {
                XmlNode node = result.CreateElement(info.Name);
                var val = info.GetValue(config, new object[] { });
                if (val != null) node.InnerText = val.ToString();
                root.AppendChild(node);
            }

            return result;
        }

        #endregion

    }
}
