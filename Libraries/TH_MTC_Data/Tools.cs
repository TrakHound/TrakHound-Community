// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Globalization;
using System.Xml;
using System.Data;

using TH_Global.Functions;

namespace TH_MTC_Data
{

    public delegate void StreamResponse_Handler(string responseString);
    public delegate void Error_Handler(Error error);

    public delegate void Connection_Handler();

    public enum MTC_Stream_Status
    {
        Stopped = 0,
        Started = 1
    }

    public static class XML
    {
        public static string GetAttribute(XmlNode Node, string AttributeName)
        {
            if (Node.Attributes != null)
            {
                var nameAttribute = Node.Attributes[AttributeName];
                if (nameAttribute != null)
                    return nameAttribute.Value;
                else
                    return "";
            }
            else
                return "";
        }

        public static void AssignProperties(Object obj, XmlNode Node)
        {
            foreach (System.Reflection.PropertyInfo info in obj.GetType().GetProperties())
            {
                string Value = GetAttribute(Node, info.Name);
                if (Value != "")
                {
                    Type t = info.PropertyType;

                    // Make sure DateTime gets set as UTC
                    if (t == typeof(DateTime))
                    {
                        DateTime dt = DateTime_Functions.ConvertStringToUTC(Value);
                        info.SetValue(obj, dt, null);
                    }
                    else
                    {
                        info.SetValue(obj, Convert.ChangeType(Value, t), null);
                    }

                    
                }
            }
        }

        public static string GetFullAddress(XmlNode Node)
        {
            string Result = "";

            do
            {
                Result = Node.Name + "/" + Result;
                Node = Node.ParentNode;

                if (Node == null) break;

            } while (Node.Name != "Device");

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

    public static class Tables
    {
        public static void CreateTableColumns(object obj, DataTable DT, string prefix)
        {
            System.Reflection.PropertyInfo[] info = obj.GetType().GetProperties();
            string[] ColumnNames = new string[info.Length];
            for (int x = 0; x <= info.Length - 1; x++)
            {
                ColumnNames[x] = prefix + info[x].Name;
            }
            for (int x = 0; x <= ColumnNames.Length - 1; x++)
            {
                DataColumn NewCol = new DataColumn();
                NewCol.ColumnName = ColumnNames[x];
                if (!DT.Columns.Contains(NewCol.ColumnName)) DT.Columns.Add(NewCol);
            }
        }

        #region "Event Types"

        public static DataTable GetEventTypes()
        {
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(Properties.Resources.EventDataValues);

            List<TableInfo> infos = GetData(xml);

            return CreateTable(infos);
        }

        public class TableInfo
        {
            public TableInfo() { }

            public string name { get; set; }
            public string value { get; set; }

        }

        static List<TableInfo> GetData(XmlNode xml)
        {
            List<TableInfo> result = new List<TableInfo>();

            foreach (XmlNode node in xml.ChildNodes)
            {
                if (node.Name.ToLower() == "eventvalues" && node.NodeType == XmlNodeType.Element)
                {
                    foreach (XmlNode child in node.ChildNodes)
                    {
                        if (child.NodeType == XmlNodeType.Element)
                        {
                            string name = child.Name;

                            foreach (XmlNode valueNode in child.ChildNodes)
                            {
                                TableInfo info = new TableInfo();
                                info.name = name;
                                info.value = valueNode.Name;
                                result.Add(info);
                            }
                        }
                    }
                }
            }

            return result;
        }

        static DataTable CreateTable(List<TableInfo> infos)
        {
            DataTable result = new DataTable();

            result.Columns.Add("NAME");
            result.Columns.Add("VALUE");

            foreach (TableInfo info in infos)
            {
                DataRow row = result.NewRow();
                row["NAME"] = info.name;
                row["VALUE"] = info.value;
                result.Rows.Add(row);
            }

            return result;
        }

        #endregion

    }

    

}
