using System;
using System.Collections.Generic;
using System.Collections;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using System.Data;

namespace TH_Functions
    {
   
    public static class Functions
        {

        static public void GetChildNodes(XmlNode N, ArrayList Nodes, ArrayList Addresses)
            {
            XmlNodeList CNodes = N.ChildNodes;

            foreach (XmlNode Node in CNodes)
                {

                string address = "";
                XmlNode Parent = Node;

                do
                    {
                    address = Parent.Name + "/" + address;
                    Parent = Parent.ParentNode;

                    if (Parent == null) break;

                    } while (Parent.Name != "Device");

                if (address.Length > 0)
                    {
                    if (address[0] != Convert.ToChar("/")) address = "/" + address;
                    if (address.Length > 1)
                        {
                        if (address[address.Length - 1] == Convert.ToChar("/")) address = address.Remove(address.Length - 1);
                        }
                    }

                Nodes.Add(Node);
                Addresses.Add(address);

                GetChildNodes(Node, Nodes, Addresses);
                }
            }

        //static public void GetChildNodes(XmlNode N, ref ArrayList Nodes, ref ArrayList Addresses)
        //{
        //    XmlNodeList CNodes = N.ChildNodes;

        //    foreach (XmlNode Node in CNodes)
        //    {

        //        string address = "";
        //        XmlNode Parent = Node;

        //        do
        //        {
        //            address = Parent.Name + "/" + address;
        //            Parent = Parent.ParentNode;

        //            if (Parent == null) break;

        //        } while (Parent.Name != "Device");

        //        if (address.Length > 0)
        //        {
        //            if (address[0] != Convert.ToChar("/")) address = "/" + address;
        //            if (address.Length > 1)
        //            {
        //                if (address[address.Length - 1] == Convert.ToChar("/")) address = address.Remove(address.Length - 1);
        //            }
        //        }

        //        Nodes.Add(Node);
        //        Addresses.Add(address);

        //        GetChildNodes(Node, ref Nodes, ref Addresses);
        //    }
        //}

        static public void AssignProperties(Object obj, XmlNode Node)
            {
            foreach (System.Reflection.PropertyInfo info in obj.GetType().GetProperties())
                {
                string Value = GetAttribute(Node, info.Name);
                if (Value != "")
                    {
                    Type t = info.PropertyType;
                    info.SetValue(obj, Convert.ChangeType(Value, t), null);
                    }
                    
                }
            }

        static public string GetAttribute(XmlNode Node, string AttributeName)
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


        /// <summary>
        /// Used to get a datatable from a dataset given the tablename as a string. Works like .Find should if it existed.
        /// </summary>
        /// <param name="DS">DataSet to search</param>
        /// <param name="TableName">DataTable Tablename searching for</param>
        /// <returns></returns>
        static public DataTable GetDataTable(DataSet DS, string TableName)
            {

            DataTable Result = null;

            if (DS.Tables.Contains(TableName))
                {

                DataTable table = DS.Tables[DS.Tables.IndexOf(TableName)];

                if (table != null) Result = table;

                }

            return Result;

            }

        public static bool IsInteger(this string s)
            {
            float output;
            return float.TryParse(s, out output);
            }

        public static bool IsDouble(this string s)
            {
            double output;
            return double.TryParse(s, out output);
            }

        public static bool IsNumber(this object value)
            {

            bool Result = false;

            if (value is sbyte
                || value is byte
                || value is short
                || value is ushort
                || value is int
                || value is Int32
                || value is uint
                || value is long
                || value is ulong
                || value is float
                || value is double
                || value is decimal)
                {
                Result = true;
                }

            return Result;

            }

        public static XElement GetXElement(this XmlNode node)
            {
            XDocument xDoc = new XDocument();
            using (XmlWriter xmlWriter = xDoc.CreateWriter())
                node.WriteTo(xmlWriter);
            return xDoc.Root;
            }

        public static Tuple<string, int> SearchRowLimitList(List<Tuple<string, int>> SearchList, string FindItem)
            {

            Tuple<string, int> Result = null;

            foreach (Tuple<string, int> Item in SearchList)
                if (Item.Item1.ToLower() == FindItem.ToLower()) Result = Item;

            return Result;

            }

        public static string ReplaceUnderscore(string line)
        {

            return line.Replace('_', ' ');

        }
      
        }
    }
