using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;

using TH_Configuration;

namespace TH_UserManagement
{
    public class UserManagementSettings
    {
        public UserManagementSettings()
        {
            //Users = new List<User_Settings>();
            Databases = new Database_Settings();
        }

        //public List<User_Settings> Users { get; set; }

        public Database_Settings Databases { get; set; }


        public static UserManagementSettings ReadConfiguration(string filepath)
        {
            UserManagementSettings result = null;

            string rootPath;
            rootPath = System.IO.Path.GetDirectoryName(filepath);
            rootPath += @"\";
            Console.WriteLine("User Configuration Root Path = " + rootPath);

            if (System.IO.File.Exists(filepath))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(filepath);

                result = new UserManagementSettings();

                foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                {
                    switch (node.Name.ToLower())
                    {
                        //case "users": result.Users = Process_Users(node); break;
                        case "databases": result.Databases = Process_Databases(node); break;
                    }
                }

                Console.WriteLine("User Configuration Successfully Read From : " + filepath);
            }
            else
            {
                Console.WriteLine("User Configuration File Not Found : " + filepath);
            }

            return result;
        }

        //static List<User_Settings> Process_Users(XmlNode node)
        //{
        //    List<User_Settings> result = new List<User_Settings>();

        //    foreach (XmlNode child in node.ChildNodes)
        //    {
        //        if (child.Name.ToLower() == "user" && child.NodeType == XmlNodeType.Element)
        //        {
        //            if (node.Attributes["id"] != null && node.Attributes["username"] != null && node.Attributes["password"] != null)
        //            {
        //                User_Settings user = new User_Settings();
        //                user.id = node.Attributes["id"].Value;
        //                user.username = node.Attributes["username"].Value;
        //                user.password = node.Attributes["password"].Value;
        //                result.Add(user);
        //            }
        //        }
        //    }

        //    return result;
        //}

        static Database_Settings Process_Databases(XmlNode node)
        {
            Database_Settings result = new Database_Settings();

            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element)
                {
                    Database_Configuration db = new Database_Configuration();
                    db.Type = child.Name;
                    db.Node = child;
                    result.Databases.Add(db);
                }
            }

            return result;
        }

    }
}
