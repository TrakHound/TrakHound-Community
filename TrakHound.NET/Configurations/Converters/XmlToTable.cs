// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Data;
using System.Xml;

namespace TrakHound.Configurations.Converters
{
    public static class XmlToTable
    {
        
        public class TableInfo
        {
            public TableInfo() { attributes = new List<Attribute>(); }

            public string address { get; set; }
            public string name { get; set; }

            public List<Attribute> attributes;

            public string value { get; set; }
        }

        public class Attribute
        {
            public string name { get; set; }
            public string value { get; set; }
        }


        public static List<TableInfo> XMLToTable_CreateData(XmlNode xml)
        {
            List<TableInfo> result = new List<TableInfo>();

            foreach (XmlNode node in xml.ChildNodes)
            {
                //if (node.Name.ToLower() == "settings" && node.NodeType == XmlNodeType.Element)
                if (node.Name.ToLower() == "deviceconfiguration" && node.NodeType == XmlNodeType.Element)
                {
                    foreach (XmlNode child in node.ChildNodes)
                    {
                        if (child.NodeType == XmlNodeType.Element)
                        {
                            XMLToTable_GetData(child, result);
                        }
                    }
                }
            }

            return result;
        }

        static void XMLToTable_GetData(XmlNode node, List<TableInfo> infos)
        {
            if (!HasChildren(node) || HasAttributes(node))
            {
                TableInfo info = new TableInfo();
                info.name = node.Name;
                info.address = XMLToTable_GetFullAddress(node);
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
                if (child.NodeType == XmlNodeType.Element) XMLToTable_GetData(child, infos);
            }
        }

        static bool HasChildren(XmlNode node)
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

        static bool HasAttributes(XmlNode node)
        {
            if (node.Attributes.Count == 0) return false;
            return true;
        }

        static string XMLToTable_GetFullAddress(XmlNode Node)
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
            //} while (node.Name != "Settings");

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


        public static DataTable XMLToTable_CreateTable(List<TableInfo> infos)
        {
            DataTable result = new DataTable();

            // Add Columns based on properties of TableInfo class
            foreach (DataColumn col in XMLToTable_CreateColumns())
            {
                result.Columns.Add(col);
            }

            if (result.Columns.Contains("address")) result.PrimaryKey = new DataColumn[] { result.Columns["address"] };

            // Add Rows
            foreach (TableInfo info in infos)
            {
                DataRow row = result.NewRow();

                foreach (System.Reflection.PropertyInfo i in typeof(TableInfo).GetProperties())
                {
                    if (row.Table.Columns.Contains(i.Name)) row[i.Name] = i.GetValue(info, null);
                }

                // Add Xml Attributes
                string attVal = "";
                foreach (Attribute a in info.attributes) attVal += a.name + "||" + a.value + ";";
                if (row.Table.Columns.Contains("attributes")) row["attributes"] = attVal;

                result.Rows.Add(row);
            }

            return result;
        }

        static List<DataColumn> XMLToTable_CreateColumns()
        {
            List<DataColumn> result = new List<DataColumn>();

            foreach (System.Reflection.PropertyInfo info in typeof(TableInfo).GetProperties())
            {
                DataColumn col = new DataColumn();
                col.ColumnName = info.Name;
                col.DataType = info.PropertyType;

                result.Add(col);
            }

            // Add Column for Xml Attributes
            DataColumn attCol = new DataColumn();
            attCol.ColumnName = "attributes";
            result.Add(attCol);

            return result;
        }

    }
}
