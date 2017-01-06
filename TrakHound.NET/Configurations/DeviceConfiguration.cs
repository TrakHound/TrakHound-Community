// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

using TrakHound.API;
using TrakHound.Logging;
using TrakHound.Tools;

namespace TrakHound.Configurations
{

    public class DeviceConfiguration : IComparable
    {
        public DeviceConfiguration()
        {
            init();
        }

        void init()
        {
            Description = new Data.DescriptionInfo();
            CustomClasses = new List<object>();
        }

        #region "Properties"

        public Data.DescriptionInfo Description { get; set; }

        public Data.AgentInfo Agent { get; set; }

        public List<object> CustomClasses;

        public bool Enabled { get; set; }

        /// <summary>
        /// Used as a Globally Unique Id between any number of devices.
        /// Usually generated from a Guid.
        /// </summary>
        public string UniqueId { get; set; }

        /// <summary>
        /// Used to identify when the configuration has been edited.
        /// This should be updated each time the configuration is changed.
        /// </summary>
        public string UpdateId { get; set; }

        /// <summary>
        /// Contains the original Xml configuration file
        /// (ex. Plugins use this to read their custom configuration classes)
        /// </summary>
        public XmlDocument Xml { get; set; }

        /// <summary>
        /// Used as a general index to make navigation between Configurations easier
        /// </summary>
        public int Index { get; set; }

        #endregion

        #region "Methods"

        #region "Static"

        public static string GenerateUniqueID()
        {
            return Guid.NewGuid().ToString("B");
        }

        #region "Read"

        /// <summary>
        /// Reads an XML file to parse configuration information.
        /// </summary>
        /// <param name="ConfigFilePath">Path to XML File</param>
        /// <returns>Returns a Machine_Settings object containing information found.</returns>
        public static DeviceConfiguration Read(string path)
        {
            DeviceConfiguration result = null;

            if (File.Exists(path))
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(path);

                    result = Read(doc);

                    if (result.UniqueId == null)
                    {
                        result.UniqueId = GenerateUniqueID();
                        XML_Functions.SetInnerText(result.Xml, "/UniqueId", result.UniqueId);
                    }
                }
                catch (XmlException ex) { Logger.Log("XmlException :: " + ex.Message); }
                catch (Exception ex) { Logger.Log("Exception :: " + ex.Message); }
            }
            else
            {
                Logger.Log("Configuration File Not Found : " + path, LogLineType.Warning);
            }

            return result;
        }

        public static DeviceConfiguration Read(XmlDocument xml)
        {
            var result = new DeviceConfiguration();

            result.Xml = xml;

            foreach (XmlNode node in xml.DocumentElement.ChildNodes)
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    if (node.InnerText != "")
                    {
                        switch (node.Name.ToLower())
                        {
                            case "description": result.Description = ProcessDescription(node); break;

                            case "agent": result.Agent = ProcessAgent(node); break;

                            default:

                                Type Settings = typeof(DeviceConfiguration);
                                PropertyInfo info = Settings.GetProperty(node.Name);

                                if (info != null)
                                {
                                    Type t = info.PropertyType;
                                    info.SetValue(result, Convert.ChangeType(node.InnerText, t), null);
                                }

                                break;
                        }
                    }
                }
            }

            return result;
        }

        public static DeviceConfiguration[] ReadAll(string path)
        {
            var result = new List<DeviceConfiguration>();

            if (Directory.Exists(path))
            {
                foreach (var file in Directory.GetFiles(path, "*.xml", SearchOption.TopDirectoryOnly))
                {
                    var config = Read(file);
                    if (config != null)
                    {
                        // Check to make sure Unique Id is not already used by another file.
                        // If so, generate a new one. This can happen if the file was manually copied
                        if (result.Exists(x => x.UniqueId == config.UniqueId))
                        {
                            config.UniqueId = GenerateUniqueID();

                            Save(config);
                        }

                        result.Add(config);
                    }
                }
            }
            else
            {
                Logger.Log("Configuration File Directory Not Found : " + path, LogLineType.Warning);
            }

            return result.ToArray();
        }

        private static Data.DescriptionInfo ProcessDescription(XmlNode Node)
        {
            var result = new Data.DescriptionInfo();

            foreach (XmlNode child in Node.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element)
                {
                    if (child.InnerText != "")
                    {
                        Type Settings = typeof(Data.DescriptionInfo);
                        PropertyInfo info = Settings.GetProperty(child.Name);

                        if (info != null)
                        {
                            Type t = info.PropertyType;
                            info.SetValue(result, Convert.ChangeType(child.InnerText, t), null);
                        }
                    }
                }
            }

            return result;
        }

        private static Data.AgentInfo ProcessAgent(XmlNode Node)
        {
            var result = new Data.AgentInfo();

            foreach (XmlNode child in Node.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element)
                {
                    if (child.InnerText != "")
                    {
                        Type Settings = typeof(Data.AgentInfo);
                        PropertyInfo info = Settings.GetProperty(child.Name);

                        if (info != null)
                        {
                            Type t = info.PropertyType;
                            info.SetValue(result, Convert.ChangeType(child.InnerText, t), null);
                        }
                    }
                }
            }

            return result;
        }

        #endregion

        #region "Save"

        public static bool Save(DeviceConfiguration config)
        {
            return Save(config, FileLocations.Devices);
        }

        public static bool Save(DeviceConfiguration config, string path)
        {
            bool result = false;

            result = Save(config.Xml, path);

            return result;
        }

        public static bool Save(DataTable table)
        {
            return Save(table, FileLocations.Devices);
        }

        public static bool Save(DataTable table, string path)
        {
            bool result = false;

            XmlDocument xml = TableToXml(table);

            result = Save(xml, path);

            return result;
        }

        public static bool Save(XmlDocument xml)
        {
            return Save(xml, FileLocations.Devices);
        }

        public static bool Save(XmlDocument xml, string path)
        {
            bool result = false;

            if (xml != null)
            {
                try
                {
                    string filePath = XML_Functions.GetInnerText(xml, "UniqueId");

                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                    xml.Save(Path.Combine(path, Path.ChangeExtension(filePath, ".xml")));

                    result = true;
                }
                catch (Exception ex) { Logger.Log("Error during Configuration Xml Save : " + ex.Message, LogLineType.Warning); }
            }

            return result;
        }

        #endregion

        #region "Table"



        public static void EditTable(DataTable table, string address, object value)
        {
            DataTable_Functions.UpdateTableValue(table, "address", address, "value", value == null ? string.Empty : value.ToString());
        }

        public static void EditTable(DataTable table, string address, object value, string attributes)
        {
            DataTable_Functions.UpdateTableValue(table, "address", address, "value", value == null ? string.Empty : value.ToString());
            DataTable_Functions.UpdateTableValue(table, "address", address, "attributes", attributes);
        }

        public static string GetTableValue(DataTable table, string address)
        {
            return DataTable_Functions.GetTableValue(table, "address", address, "value");
        }

        public static string GetTableAttributes(DataTable table, string address)
        {
            return DataTable_Functions.GetTableValue(table, "address", address, "attributes");
        }

        #endregion

        #region "Conversion"

        #region "ToXml"

        public static XmlDocument TableToXml(DataTable table)
        {
            var result = new XmlDocument();

            // Insert XML Declaration
            var xmlDeclaration = result.CreateXmlDeclaration("1.0", "UTF-8", null);
            var root = result.DocumentElement;
            result.InsertBefore(xmlDeclaration, root);

            var configuration = result.CreateElement(string.Empty, "DeviceConfiguration", string.Empty);
            result.AppendChild(configuration);

            foreach (DataRow row in table.Rows)
            {
                XmlNode node = AddAddress(result, row["address"].ToString());
                if (node != null)
                {
                    // Set Inner Text (Value) and prevent from adding closing tag by checking for ""
                    if (row["value"] != null) if (row["Value"].ToString() != "") node.InnerText = row["value"].ToString();

                    // Set Attributes
                    if (row["attributes"] != null)
                    {
                        string[] attributes = row["attributes"].ToString().Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string attribute in attributes.ToList())
                        {
                            string[] split = attribute.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries);
                            if (split.Length > 1)
                            {
                                string name = split[0];
                                string val = split[1];

                                XmlAttribute a = result.CreateAttribute(name);
                                a.Value = val;
                                node.Attributes.Append(a);
                            }
                        }
                    }
                }
            }

            return result;
        }

        private static XmlNode AddAddress(XmlDocument doc, string address)
        {
            string[] nodes = address.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            XmlNode parent = doc.DocumentElement;

            string xpath = "/" + parent.Name;

            for (int x = 0; x <= nodes.Length - 1; x++)
            {
                string node = nodes[x];

                string node_id = "";

                // Check for node with 'id' attribute, if so then use that in xpath expression
                if (node.Contains("||"))
                {
                    int idIndex = node.IndexOf("||");

                    node_id = @"[@id='" + node.Substring(idIndex + 2) + "']";
                    node = node.Substring(0, idIndex);
                }

                xpath += "/" + node + node_id;

                XmlNode child = parent.SelectSingleNode(xpath);

                if (child == null)
                {
                    child = doc.CreateElement(node);
                    parent.AppendChild(child);
                }
                // Make sure last node in address gets added (even though there may be another node with the same 'name')
                else if (x == nodes.Length - 1)
                {
                    child = doc.CreateElement(node);
                    parent.AppendChild(child);
                }

                parent = child;
            }

            return parent;
        }

        #endregion

        private class TableConverter
        {
            private class TableInfo
            {
                public TableInfo() { attributes = new List<Attribute>(); }

                public string address { get; set; }
                public string name { get; set; }

                public List<Attribute> attributes;

                public string value { get; set; }
            }

            private class Attribute
            {
                public string name { get; set; }
                public string value { get; set; }
            }

            public static DataTable Create(XmlNode xml)
            {
                var data = CreateData(xml);

                return CreateTable(data);
            }

            private static List<TableInfo> CreateData(XmlNode xml)
            {
                var result = new List<TableInfo>();

                foreach (XmlNode node in xml.ChildNodes)
                {
                    if (node.Name.ToLower() == "deviceconfiguration" && node.NodeType == XmlNodeType.Element)
                    {
                        foreach (XmlNode child in node.ChildNodes)
                        {
                            if (child.NodeType == XmlNodeType.Element)
                            {
                                GetData(child, result);
                            }
                        }
                    }
                }

                return result;
            }

            private static DataTable CreateTable(List<TableInfo> infos)
            {
                var result = new DataTable();

                // Add Columns based on properties of TableInfo class
                foreach (var col in CreateColumns())
                {
                    result.Columns.Add(col);
                }

                if (result.Columns.Contains("address")) result.PrimaryKey = new DataColumn[] { result.Columns["address"] };

                // Add Rows
                foreach (var info in infos)
                {
                    DataRow row = result.NewRow();

                    foreach (var i in typeof(TableInfo).GetProperties())
                    {
                        if (row.Table.Columns.Contains(i.Name)) row[i.Name] = i.GetValue(info, null);
                    }

                    // Add Xml Attributes
                    string attVal = "";
                    foreach (var a in info.attributes) attVal += a.name + "||" + a.value + ";";
                    if (row.Table.Columns.Contains("attributes")) row["attributes"] = attVal;

                    result.Rows.Add(row);
                }

                return result;
            }

            private static List<DataColumn> CreateColumns()
            {
                var result = new List<DataColumn>();

                foreach (var info in typeof(TableInfo).GetProperties())
                {
                    var col = new DataColumn();
                    col.ColumnName = info.Name;
                    col.DataType = info.PropertyType;

                    result.Add(col);
                }

                // Add Column for Xml Attributes
                var attCol = new DataColumn();
                attCol.ColumnName = "attributes";
                result.Add(attCol);

                return result;
            }

            private static void GetData(XmlNode node, List<TableInfo> infos)
            {
                if (!HasChildren(node) || HasAttributes(node))
                {
                    TableInfo info = new TableInfo();
                    info.name = node.Name;
                    info.address = GetFullAddress(node);
                    if (!HasChildren(node)) info.value = node.InnerText;

                    foreach (XmlAttribute att in node.Attributes)
                    {
                        Attribute a = new Attribute();
                        a.name = att.Name;
                        a.value = att.Value;
                        info.attributes.Add(a);
                    }

                    infos.Add(info);
                }

                foreach (XmlNode child in node.ChildNodes)
                {
                    if (child.NodeType == XmlNodeType.Element) GetData(child, infos);
                }
            }

            private static bool HasChildren(XmlNode node)
            {
                bool result = false;

                foreach (XmlNode child in node.ChildNodes)
                {
                    if (child.NodeType == XmlNodeType.Element)
                    {
                        result = true;
                        break;
                    }
                }

                return result;
            }

            private static bool HasAttributes(XmlNode node)
            {
                if (node.Attributes.Count == 0) return false;
                return true;
            }

            private static string GetFullAddress(XmlNode Node)
            {
                string Result = "";
                XmlNode node = Node;

                do
                {
                    if (node.Attributes["id"] != null) Result = node.Name + "||" + node.Attributes["id"].Value + "/" + Result;
                    else Result = node.Name + "/" + Result;

                    node = node.ParentNode;

                    if (node == null) break;

                } while (node.Name != "DeviceConfiguration");

                if (Result.Length > 0)
                {
                    if (Result[0] != Convert.ToChar("/")) Result = "/" + Result;
                    if (Result.Length > 1)
                    {
                        if (Result[Result.Length - 1] == Convert.ToChar("/")) Result = Result.Remove(Result.Length - 1);
                    }
                }

                return Result;
            }
        }

        #endregion

        #endregion

        public DataTable ToTable()
        {
            DataTable result = null;

            try
            {
                result = TableConverter.Create(Xml);
            }
            catch (Exception ex)
            {
                Logger.Log("DeviceConfiguration.ToTable() :: Exception :: " + ex.Message, LogLineType.Warning);
            }

            return result;
        }

        #endregion

        #region "IComparable"

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            var i = obj as DeviceConfiguration;
            if (i != null)
            {
                if (i > this) return -1;
                else if (i < this) return 1;
                else return 0;
            }
            else return 1;
        }

        public override bool Equals(object obj)
        {

            var other = obj as DeviceConfiguration;
            if (object.ReferenceEquals(other, null)) return false;

            return (this == other);
        }

        public override int GetHashCode()
        {
            char[] c = this.ToString().ToCharArray();
            return base.GetHashCode();
        }

        #region "Private"

        static bool EqualTo(DeviceConfiguration c1, DeviceConfiguration c2)
        {
            if (!object.ReferenceEquals(c1, null) && object.ReferenceEquals(c2, null)) return false;
            if (object.ReferenceEquals(c1, null) && !object.ReferenceEquals(c2, null)) return false;
            if (object.ReferenceEquals(c1, null) && object.ReferenceEquals(c2, null)) return true;

            return c1.UniqueId == c2.UniqueId && c1.Index == c2.Index;
        }

        static bool NotEqualTo(DeviceConfiguration c1, DeviceConfiguration c2)
        {
            if (!object.ReferenceEquals(c1, null) && object.ReferenceEquals(c2, null)) return true;
            if (object.ReferenceEquals(c1, null) && !object.ReferenceEquals(c2, null)) return true;
            if (object.ReferenceEquals(c1, null) && object.ReferenceEquals(c2, null)) return false;

            return c1.UniqueId != c2.UniqueId || c1.Index != c2.Index;
        }

        static bool LessThan(DeviceConfiguration c1, DeviceConfiguration c2)
        {
            if (c1.Index > c2.Index) return false;
            else return true;
        }

        static bool GreaterThan(DeviceConfiguration c1, DeviceConfiguration c2)
        {
            if (c1.Index < c2.Index) return false;
            else return true;
        }

        #endregion

        public static bool operator ==(DeviceConfiguration c1, DeviceConfiguration c2)
        {
            return EqualTo(c1, c2);
        }

        public static bool operator !=(DeviceConfiguration c1, DeviceConfiguration c2)
        {
            return NotEqualTo(c1, c2);
        }


        public static bool operator <(DeviceConfiguration c1, DeviceConfiguration c2)
        {
            return LessThan(c1, c2);
        }

        public static bool operator >(DeviceConfiguration c1, DeviceConfiguration c2)
        {
            return GreaterThan(c1, c2);
        }


        public static bool operator <=(DeviceConfiguration c1, DeviceConfiguration c2)
        {
            return LessThan(c1, c2) || EqualTo(c1, c2);
        }

        public static bool operator >=(DeviceConfiguration c1, DeviceConfiguration c2)
        {
            return GreaterThan(c1, c2) || EqualTo(c1, c2);
        }

        #endregion

    }

}
