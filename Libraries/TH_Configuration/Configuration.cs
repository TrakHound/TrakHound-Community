// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.Reflection;
using System.Drawing;
using System.Data;
using System.IO;

using TH_Global;

namespace TH_Configuration
{

    public class Configuration
    {

        public Agent_Settings Agent;
        public Database_Settings Databases_Client;
        public Database_Settings Databases_Server;
        public FileLocation_Settings FileLocations;
        public Description_Settings Description;
        public List<object> CustomClasses;

        public Configuration()
        {
            init();
        }

        void init()
        {
            Agent = new Agent_Settings();
            FileLocations = new FileLocation_Settings();
            Description = new Description_Settings();
            Databases_Client = new Database_Settings();
            Databases_Server = new Database_Settings();
            CustomClasses = new List<object>();
        }



        #region "Properties"

        public bool ClientEnabled { get; set; }

        public bool ServerEnabled { get; set; }

        #region "Remote Configurations"

        public bool UseTrakHoundCloud { get; set; }

        public bool Remote { get; set; }

        public string ClientUpdateId { get; set; }

        public string ServerUpdateId { get; set; }

        public string TableName { get; set; }

        public string Version { get; set; }

        public string SharedId { get; set; }

        public string SharedTableName { get; set; }

        #endregion

        #region "Images"

        public Image Manufacturer_Logo { get; set; }

        public Image Device_Image { get; set; }

        #endregion

        /// <summary>
        /// Used as a UniqueId between any number of devices
        /// Composed of Database name, IP address, and Port (so it should always be unique)
        /// </summary>
        public string UniqueId { get; set; }

        /// <summary>
        /// Used as a general index to make navigation between Configurations easier
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Contains the original XML configuration file
        /// (ex. Plugins use this to read their custom configuration classes)
        /// </summary>
        public XmlDocument ConfigurationXML { get; set; }

        /// <summary>
        /// The full path of the configuration file
        /// </summary>
        public string SettingsRootPath { get; set; }

        #endregion

        #region "Methods"

        static Random random = new Random();
        public static string RandomString(int size)
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

        static string GetUniqueID()
        {
            return RandomString(80);
        }


        /// <summary>
        /// Reads an XML file to parse configuration information.
        /// </summary>
        /// <param name="ConfigFilePath">Path to XML File</param>
        /// <returns>Returns a Machine_Settings object containing information found.</returns>
        public static Configuration Read(string path)
        {
            Configuration result = null;

            //string RootPath;
            //RootPath = Path.GetDirectoryName(path);
            //RootPath += @"\";
            //Logger.Log("Configuration File Root Path = " + RootPath);

            if (File.Exists(path))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);

                result = Read(doc);

                Logger.Log("Configuration Successfully Read From : " + path);
            }
            else
            {
                Logger.Log("Configuration File Not Found : " + path);
            }

            return result;
        }

        public static Configuration[] ReadAll(string path)
        {
            var result = new List<Configuration>();

            Logger.Log("Reading Configuration Files from : " + path);

            if (Directory.Exists(path))
            {
                foreach (var file in Directory.GetFiles(path, "*.xml", SearchOption.TopDirectoryOnly))
                {
                    var config = Read(file);
                    if (config != null) result.Add(config);
                }
            }
            else
            {
                Logger.Log("Configuration File Directory Not Found : " + path);
            }

            return result.ToArray();
        }

        public static Configuration Read(XmlDocument xml)
        {
            Configuration result = null;

            result = new Configuration();

            result.ConfigurationXML = xml;

            foreach (XmlNode node in xml.DocumentElement.ChildNodes)
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    if (node.InnerText != "")
                    {
                        Type Settings = typeof(Configuration);
                        PropertyInfo info = Settings.GetProperty(node.Name);

                        if (info != null)
                        {
                            Type t = info.PropertyType;
                            info.SetValue(result, Convert.ChangeType(node.InnerText, t), null);
                        }
                        else
                        {
                            switch (node.Name.ToLower())
                            {
                                case "agent": result.Agent = Process_Agent(node); break;
                                case "databases_client": result.Databases_Client = Process_Databases(node); break;
                                case "databases_server": result.Databases_Server = Process_Databases(node); break;
                                case "description": result.Description = Process_Description(node); break;
                                case "file_locations": result.FileLocations = Process_File_Locations(node, result.SettingsRootPath); break;
                            }
                        }
                    }
                } 
            }

            return result;
        }

        private static Agent_Settings Process_Agent(XmlNode Node)
        {

            Agent_Settings Result = new Agent_Settings();

            List<Tuple<string, int>> RowLimits = new List<Tuple<string, int>>();
            List<string> OmitInstance = new List<string>();

            foreach (XmlNode Child in Node.ChildNodes)
            {
                if (Child.NodeType == XmlNodeType.Element)
                {
                    if (Child.InnerText != "")
                    {
                        Type Settings = typeof(Agent_Settings);
                        PropertyInfo info = Settings.GetProperty(Child.Name);

                        if (info != null)
                        {
                            Type t = info.PropertyType;
                            info.SetValue(Result, Convert.ChangeType(Child.InnerText, t), null);
                        }
                        else
                        {
                            if (Child.Name.ToLower() == "simulation_sample_files")
                            {
                                foreach (XmlNode simNode in Child.ChildNodes)
                                {
                                    if (simNode.NodeType == XmlNodeType.Element)
                                    {
                                        if (simNode.Name.ToLower() == "simulation_sample_path")
                                        {
                                            Result.Simulation_Sample_Files.Add(simNode.InnerText);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return Result;

        }

        private static Database_Settings Process_Databases(XmlNode node)
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

        private static Description_Settings Process_Description(XmlNode Node)
        {

            Description_Settings Result = new Description_Settings();

            List<Tuple<string, string>> Custom = new List<Tuple<string, string>>();

            foreach (XmlNode Child in Node.ChildNodes)
            {
                if (Child.NodeType == XmlNodeType.Element)
                {
                    if (Child.InnerText != "")
                    {
                        Type Settings = typeof(Description_Settings);
                        PropertyInfo info = Settings.GetProperty(Child.Name);

                        if (info != null)
                        {
                            Type t = info.PropertyType;
                            info.SetValue(Result, Convert.ChangeType(Child.InnerText, t), null);
                        }
                        else
                        {
                            // Allow for category nodes
                            CreateAddInfoAddress(Child, Child.Name, Custom);
                        }
                    }
                }
            }

            Result.Custom = Custom;

            return Result;

        }

        static void CreateAddInfoAddress(XmlNode Node, string address, List<Tuple<string, string>> List)
        {

            // Had to check for ChildNode count == 1 and then see if it is #text (wierd??!!??)
            if (Node.ChildNodes.Count > 1)
            {
                foreach (XmlNode CustomChild in Node.ChildNodes)
                {
                    if (CustomChild.NodeType == XmlNodeType.Element)
                    {
                        string New_address = address + "-" + CustomChild.Name;
                        CreateAddInfoAddress(CustomChild, New_address, List);
                    }
                }
            }
            else if (Node.ChildNodes.Count == 1)
            {
                if (Node.ChildNodes[0].NodeType == XmlNodeType.Text)
                {
                    List.Add(new Tuple<string, string>(address, Node.InnerText));
                }
                else
                {
                    List.Add(new Tuple<string, string>(address, Node.InnerText));
                }
            }
        }


        private static FileLocation_Settings Process_File_Locations(XmlNode Node, string rootPath)
        {

            FileLocation_Settings Result = new FileLocation_Settings();

            foreach (XmlNode Child in Node.ChildNodes)
            {
                if (Child.NodeType == XmlNodeType.Element)
                {
                    if (Child.InnerText != "")
                    {

                        Type Settings = typeof(FileLocation_Settings);
                        PropertyInfo info = Settings.GetProperty(Child.Name);

                        string FilePath = Child.InnerText;
                        if (!System.IO.Path.IsPathRooted(FilePath)) FilePath = rootPath + FilePath;

                        if (info != null)
                        {
                            Type t = info.PropertyType;
                            info.SetValue(Result, Convert.ChangeType(FilePath, t), null);
                        }

                    }
                }
            }

            return Result;
        }


        public static void Process_CustomClass<T>(object customClass, XmlNode Node)
        {

            foreach (XmlNode Child in Node.ChildNodes)
            {
                if (Child.NodeType == XmlNodeType.Element)
                {
                    if (Child.InnerText != "")
                    {

                        Type Setting = typeof(T);
                        PropertyInfo info = Setting.GetProperty(Child.Name);

                        if (info != null)
                        {
                            Type t = info.PropertyType;
                            info.SetValue(customClass, Convert.ChangeType(Child.InnerText, t), null);
                        }

                    }
                }
            }

        }


        #region "Static"

        public static Configuration CreateBlank()
        {
            XmlDocument xml = new XmlDocument();
                   
            xml.LoadXml(Properties.Resources.BlankConfiguration);

            Configuration result = Read(xml);

            return result;
        }



        #endregion

        #endregion

    }

}
