using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

using TH_Global;
using TH_Global.Functions;

namespace TH_UserManagement.Management
{
    public static class UserLoginFile
    {
        public static bool Create(UserConfiguration userConfig)
        {
            bool result = false;

            TH_Global.FileLocations.CreateAppDataDirectory();
            string path = TH_Global.FileLocations.AppData + @"\nigolresu";

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
            public string Password { get; set; }
        }

        public static LoginData Read()
        {
            LoginData result = null;

            string path = TH_Global.FileLocations.AppData + @"\nigolresu";
            if (File.Exists(path))
            {
                try
                {
                    XmlDocument xml = new XmlDocument();
                    xml.Load(path);

                    string username = XML_Functions.GetInnerText(xml, "Username");
                    string password = XML_Functions.GetInnerText(xml, "Password");

                    if (username != null && password != null)
                    {
                        result = new LoginData();
                        result.Username = username;
                        result.Password = password;
                    }
                }
                catch (Exception ex) { Logger.Log("TH_UserManagement.UserLoginFile.WriteDocument() :: Exception :: " + ex.Message); }
            }

            return result;
        }


        private static XmlDocument CreateDocument(UserConfiguration userConfig)
        {
            var result = new XmlDocument();

            XmlNode docNode = result.CreateXmlDeclaration("1.0", "UTF-8", null);
            result.AppendChild(docNode);

            XmlNode root = result.CreateElement("UserLogin");
            result.AppendChild(root);

            // Username
            XmlNode username = result.CreateElement("Username");
            username.InnerText = userConfig.username;
            root.AppendChild(username);

            // Password
            XmlNode password = result.CreateElement("Password");
            password.InnerText = userConfig.hash;
            root.AppendChild(password);

            return result;
        }

        public static void Remove()
        {
            string path = TH_Global.FileLocations.AppData + @"\nigolresu";
            Remove(path);
        }

        private static void Remove(string path)
        {
            if (File.Exists(path)) File.Delete(path);
        }

        private static void WriteDocument(XmlDocument doc, string path)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = false;

            try
            {
                using (XmlWriter writer = XmlWriter.Create(path, settings))
                {
                    doc.Save(writer);
                }
            }
            catch (Exception ex) { Logger.Log("TH_UserManagement.UserLoginFile.WriteDocument() :: Exception :: " + ex.Message); }
        }


    }
}
