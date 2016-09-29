// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Xml;

using TrakHound.Logging;
using TrakHound.Tools;

namespace TrakHound.API.Users
{
    public static class ServerCredentials
    {
        public const string SERVER_CREDENTIALS_FILENAME = "server_credentials.xml";

        public static bool Create(UserConfiguration userConfig)
        {
            bool result = false;

            string path = Path.Combine(FileLocations.TrakHound, SERVER_CREDENTIALS_FILENAME);

            Remove(path);

            if (userConfig != null)
            {
                var xml = CreateDocument(userConfig);
                WriteDocument(xml, path);
            }

            return result;
        }

        public class LoginData
        {
            public string Username { get; set; }
            public string Token { get; set; }
        }

        public static LoginData Read()
        {
            LoginData result = null;

            string path = Path.Combine(FileLocations.TrakHound, SERVER_CREDENTIALS_FILENAME);
            if (File.Exists(path))
            {
                try
                {
                    var xml = new XmlDocument();
                    xml.Load(path);

                    string username = XML_Functions.GetInnerText(xml, "Username");
                    string token = XML_Functions.GetInnerText(xml, "Token");

                    if (username != null && token != null)
                    {
                        result = new LoginData();
                        result.Username = username;
                        result.Token = token;
                    }
                }
                catch (Exception ex) { Logger.Log("Exception :: " + ex.Message); }
            }

            return result;
        }


        private static XmlDocument CreateDocument(UserConfiguration userConfig)
        {
            var result = new XmlDocument();

            XmlNode docNode = result.CreateXmlDeclaration("1.0", "UTF-8", null);
            result.AppendChild(docNode);

            XmlNode root = result.CreateElement("ServerCredentials");
            result.AppendChild(root);

            // Username
            XmlNode username = result.CreateElement("Username");
            username.InnerText = userConfig.Username;
            root.AppendChild(username);

            // Token
            XmlNode token = result.CreateElement("Token");
            token.InnerText = userConfig.Token;
            root.AppendChild(token);

            return result;
        }

        public static void Remove()
        {
            string path = Path.Combine(FileLocations.TrakHound, SERVER_CREDENTIALS_FILENAME);
            Remove(path);
        }

        private static void Remove(string path)
        {
            if (File.Exists(path)) File.Delete(path);
        }

        private static void WriteDocument(XmlDocument doc, string path)
        {
            var settings = new XmlWriterSettings();
            settings.Indent = false;

            try
            {
                using (var writer = XmlWriter.Create(path, settings))
                {
                    doc.Save(writer);
                }
            }
            catch (Exception ex) { Logger.Log("Exception :: " + ex.Message); }
        }


        public class Monitor
        {
            public delegate void UserChanged_Handler(LoginData loginData);
            public event UserChanged_Handler UserChanged;

            public Monitor()
            {
                string dir = FileLocations.TrakHound;
                if (Directory.Exists(dir))
                {
                    var watcher = new FileSystemWatcher(dir, SERVER_CREDENTIALS_FILENAME);
                    watcher.Changed += File_Changed;
                    watcher.Created += File_Changed;
                    watcher.Deleted += File_Changed;
                    watcher.EnableRaisingEvents = true;
                }
            }

            private void File_Changed(object sender, FileSystemEventArgs e)
            {
                var loginData = Read();

                UserChanged?.Invoke(loginData);
            }
        }
    }
    
}
