// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Xml;
using System.Reflection;

namespace TH_Configuration
{

    public class Configuration
    {

        public Configuration()
        {

            Agent = new Agent_Settings();
            FileLocations = new FileLocation_Settings();
            Description = new Description_Settings();
            Server = new Server_Settings();

            CustomClasses = new List<object>();

        }

        #region "SubClass Objects"

        public Agent_Settings Agent;
        public SQL_Settings SQL;
        public FileLocation_Settings FileLocations;
        public Description_Settings Description;
        public Server_Settings Server;

        public List<object> CustomClasses;

        #endregion

        #region "Properties"

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

        /// <summary>
        /// Full SQL database name that is used universally
        /// </summary>
        public string DataBaseName
        {
            get
            {
                return GetDatabaseName(SQL);


                //string Result = "";

                //List<string> Items = new List<string>();

                //if (SQL.Database_Prefix != null) Items.Add(SQL.Database_Prefix.ToLower());
                //if (Description.Customer_Name != null) Items.Add(Description.Customer_Name.ToLower());
                //if (Description.Machine_Type != null) Items.Add(Description.Machine_Type.ToLower());
                //if (Description.Control_Type != null) Items.Add(Description.Control_Type.ToLower());
                //if (Description.Manufacturer != null) Items.Add(Description.Manufacturer.ToLower());
                //if (Description.Machine_ID != null) Items.Add(Description.Machine_ID.ToLower());

                //for (int x = 0; x <= Items.Count - 1; x++)
                //{
                //    if (x > 0) Result += "_";
                //    Result += Items[x];
                //}

                //return Result;
            }
        }

        #endregion

        #region "Methods"

        static string GetUniqueID(Configuration config)
        {
            return config.DataBaseName + ";" + config.SQL.Server + ";" + config.SQL.Port.ToString();
        }

        public string GetDatabaseName(SQL_Settings sql)
        {
            string Result = "";

            List<string> Items = new List<string>();

            if (sql.Database_Prefix != null) Items.Add(sql.Database_Prefix.ToLower());
            if (Description.Customer_Name != null) Items.Add(Description.Customer_Name.ToLower());
            if (Description.Machine_Type != null) Items.Add(Description.Machine_Type.ToLower());
            if (Description.Control_Type != null) Items.Add(Description.Control_Type.ToLower());
            if (Description.Manufacturer != null) Items.Add(Description.Manufacturer.ToLower());
            if (Description.Machine_ID != null) Items.Add(Description.Machine_ID.ToLower());

            for (int x = 0; x <= Items.Count - 1; x++)
            {
                if (x > 0) Result += "_";
                Result += Items[x];
            }

            return Result;
        }


        /// <summary>
        /// Reads an XML file to parse configuration information.
        /// </summary>
        /// <param name="ConfigFilePath">Path to XML File</param>
        /// <returns>Returns a Machine_Settings object containing information found.</returns>
        public static Configuration ReadConfigFile(string ConfigFilePath)
        {

            Configuration Result = null;

            string RootPath;
            RootPath = System.IO.Path.GetDirectoryName(ConfigFilePath);
            RootPath += @"\";
            Console.WriteLine("Settings Root Path = " + RootPath);


            if (System.IO.File.Exists(ConfigFilePath))
            {

                XmlDocument doc = new XmlDocument();
                doc.Load(ConfigFilePath);

                Result = new Configuration();

                Result.SettingsRootPath = System.IO.Path.GetDirectoryName(ConfigFilePath) + @"\";
                Result.ConfigurationXML = doc;

                foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                {
                    switch (node.Name.ToLower())
                    {
                        case "agent": Result.Agent = Process_Agent(node); break;
                        case "sql": Result.SQL = Process_SQL(node); break;
                        case "description": Result.Description = Process_Description(node); break;
                        case "file_locations": Result.FileLocations = Process_File_Locations(node, Result.SettingsRootPath); break;
                        case "server": Result.Server = Process_Server(node); break;
                    }
                }

                Result.SQL.Database = Result.GetDatabaseName(Result.SQL);
                if (Result.SQL.AdminSQL != null) Result.SQL.AdminSQL.Database = Result.GetDatabaseName(Result.SQL.AdminSQL);

                string databaseNames = Result.SQL.Database;
                if (Result.SQL.AdminSQL != null) databaseNames += " :: " + Result.SQL.AdminSQL.Database;
                Console.WriteLine("Database Name = " + databaseNames);

                Result.UniqueId = GetUniqueID(Result);

                Console.WriteLine("Settings Successfully Read From : " + ConfigFilePath);

            }
            else
            {
                Console.WriteLine("Settings File Not Found : " + ConfigFilePath);
            }

            return Result;

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

        private static SQL_Settings Process_SQL(XmlNode Node)
        {

            SQL_Settings Result = new SQL_Settings();

            foreach (XmlNode Child in Node.ChildNodes)
            {
                if (Child.NodeType == XmlNodeType.Element)
                {
                    if (Child.InnerText != "")
                    {

                        Type Settings = typeof(SQL_Settings);
                        PropertyInfo info = Settings.GetProperty(Child.Name);

                        if (info != null)
                        {
                            Type t = info.PropertyType;
                            info.SetValue(Result, Convert.ChangeType(Child.InnerText, t), null);
                        }
                        else
                        {
                            if (Child.Name.ToLower() == "admin_sql")
                            {
                                Result.AdminSQL = Process_SQL(Child);
                            }
                        }

                    }
                }
            }

            return Result;

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


        private static Server_Settings Process_Server(XmlNode Node)
        {

            Server_Settings Result = new Server_Settings();

            foreach (XmlNode Child in Node.ChildNodes)
            {
                if (Child.NodeType == XmlNodeType.Element)
                {
                    if (Child.InnerText != "")
                    {

                        Type Settings = typeof(Server_Settings);
                        PropertyInfo info = Settings.GetProperty(Child.Name);

                        if (info != null)
                        {
                            Type t = info.PropertyType;
                            info.SetValue(Result, Convert.ChangeType(Child.InnerText, t), null);
                        }
                        else if (Child.Name.ToLower() == "tables")
                        {

                            Tables_Settings Tables = new Tables_Settings();

                            foreach (XmlNode TablesNode in Child.ChildNodes)
                            {
                                if (TablesNode.NodeType == XmlNodeType.Element)
                                {
                                    switch (TablesNode.Name.ToLower())
                                    {
                                        case "mtconnect":

                                            Result.Tables.MTConnect = Process_Tables_MTConnect(TablesNode);

                                            break;
                                    }
                                }
                            }

                            Result.Tables = Tables;

                        }

                    }
                }
            }

            return Result;

        }

        static Tables_MTConnect_Settings Process_Tables_MTConnect(XmlNode Node)
        {

            Tables_MTConnect_Settings Result = new Tables_MTConnect_Settings();

            foreach (XmlNode ChildNode in Node.ChildNodes)
            {
                if (ChildNode.NodeType == XmlNodeType.Element)
                {
                    switch (ChildNode.Name.ToLower())
                    {
                        case "probe":
                            Result.Probe = Convert.ToBoolean(ChildNode.InnerText);
                            break;

                        case "current":
                            Result.Current = Convert.ToBoolean(ChildNode.InnerText);
                            break;

                        case "sample":
                            Result.Sample = Convert.ToBoolean(ChildNode.InnerText);
                            break;

                    }
                }
            }

            return Result;

        }

        #endregion

    }

}
