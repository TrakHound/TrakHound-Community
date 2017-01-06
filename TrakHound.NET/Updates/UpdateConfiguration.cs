// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Reflection;
using System.Xml;

using TrakHound.Tools;

namespace TrakHound.Updates
{
    public class UpdateConfiguration
    {
        public const string UPDATER_PIPE_NAME = "trakhound-updater";

        public UpdateConfiguration()
        {
            UpdateCheckInterval = 60;

            Enabled = true;
            EnableMessageServer = true;
        }

        /// <summary>
        /// Sets the interval that the updater checks for available updates (time given in minutes)
        /// </summary>
        public int UpdateCheckInterval { get; set; }

        /// <summary>
        /// Toggle whether the updater automatically checks for updates or not
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Toggle whether the message server is run or not.
        /// This is used to allow communication between the Updater and a different application
        /// </summary>
        public bool EnableMessageServer { get; set; }


        #region "Configuration File"

        public const string CONFIG_FILENAME = "update_config.xml";
        public static string CONFIG_FILEPATH = Path.Combine(FileLocations.TrakHound, CONFIG_FILENAME);


        public static bool Create(UpdateConfiguration config)
        {
            bool result = false;

            Remove();

            if (config != null)
            {
                var xml = CreateDocument(config);
                XML_Functions.WriteDocument(xml, CONFIG_FILEPATH);
            }

            return result;
        }

        public static UpdateConfiguration Read()
        {
            var result = new UpdateConfiguration();

            var xml = XML_Functions.ReadDocument(CONFIG_FILEPATH);
            if (xml != null)
            {
                foreach (XmlNode node in xml.DocumentElement.ChildNodes)
                {
                    if (node.NodeType == XmlNodeType.Element)
                    {
                        if (node.InnerText != "")
                        {
                            Type Settings = typeof(UpdateConfiguration);
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

            return result;
        }

        public static void Remove()
        {
            if (File.Exists(CONFIG_FILEPATH)) File.Delete(CONFIG_FILEPATH);
        }


        private static XmlDocument CreateDocument(UpdateConfiguration config)
        {
            var result = new XmlDocument();

            XmlNode docNode = result.CreateXmlDeclaration("1.0", "UTF-8", null);
            result.AppendChild(docNode);

            XmlNode root = result.CreateElement("UpdateConfiguration");
            result.AppendChild(root);

            foreach (var info in typeof(UpdateConfiguration).GetProperties())
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
