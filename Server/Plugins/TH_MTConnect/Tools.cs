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
using System.Net;
using System.IO;

namespace TH_MTConnect
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

    public static class DateTime_Functions
    {

        /// <summary>
        /// Convert string to UTC DateTime (DateTime.TryParse seems to always convert to local time even with DateTimeStyle.AssumeUniveral)
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static DateTime ConvertStringToUTC(string s)
        {
            DateTime result = DateTime.MinValue;

            try
            {
                string sYear = s.Substring(0, 4);
                string sMonth = s.Substring(5, 2);
                string sDay = s.Substring(8, 2);

                string sHour = s.Substring(11, 2);
                string sMinute = s.Substring(14, 2);
                string sSecond = s.Substring(17, 2);

                int year = Convert.ToInt16(sYear);
                int month = Convert.ToInt16(sMonth);
                int day = Convert.ToInt16(sDay);

                int hour = Convert.ToInt16(sHour);
                int minute = Convert.ToInt16(sMinute);
                int second = Convert.ToInt16(sSecond);

                result = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);

                // Get number of fraction characters
                int start = 20;
                int end = 20;
                int n = 20;

                if (s.Length > 20)
                {
                    char c = s[n];

                    while (Char.IsNumber(c))
                    {
                        n += 1;
                        if (n > s.Length - 1) break;
                        else c = s[n];
                    }

                    end = n;

                    string sFraction = s.Substring(start, end - start);
                    double fraction = Convert.ToDouble("." + sFraction);
                    int millisecond = System.Math.Min(999, Convert.ToInt32(System.Math.Round(fraction, 3) * 1000));
                    result = new DateTime(year, month, day, hour, minute, second, millisecond, DateTimeKind.Utc);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ConvertStringToUTC() : Input = " + s + " : Exception : " + ex.Message);
            }

            return result;
        }

        public static DateTime ConvertDateTimeToUTC(DateTime dt)
        {
            int year = dt.Year;
            int month = dt.Month;
            int day = dt.Day;

            int hour = dt.Hour;
            int minute = dt.Minute;
            int second = dt.Second;
            int millisecond = dt.Millisecond;

            return new DateTime(year, month, day, hour, minute, second, millisecond, DateTimeKind.Utc);
        }

    }

    public static class HTTP
    {

        public static string GetData(string url, ProxySettings proxySettings = null, int timeout = 10000, int maxAttempts = 3)
        //public static string GetData(string url, int timeout = 10000, int maxAttempts = 3)
        {

            string result = null;

            int attempts = 0;
            bool success = false;
            string message = null;

            // Try to receive data for number of connectionAttempts
            while (attempts < maxAttempts && !success)
            {
                attempts += 1;

                try
                {
                    // Create HTTP request and define Header info
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.Timeout = timeout;

                    // PR 01222016 - Added Proxy settings
                    // Get Default System Proxy (Windows Internet Settings -> Proxy Settings)
                    var proxy = WebRequest.GetSystemWebProxy();

                    // Get Custom Proxy Settings from Argument (overwrite default proxy settings)
                    if (proxySettings != null)
                    {
                        if (proxySettings.Address != null && proxySettings.Port > 0)
                        {
                            var customProxy = new WebProxy(proxySettings.Address, proxySettings.Port);
                            customProxy.BypassProxyOnLocal = false;
                            proxy = customProxy; 
                        }
                    }

                    request.Proxy = proxy;

                    // Get HTTP resonse and return as string
                    using (var response = (HttpWebResponse)request.GetResponse())
                    using (var s = response.GetResponseStream())
                    using (var reader = new StreamReader(s))
                    {
                        result = reader.ReadToEnd();
                        success = true;
                    }
                }
                catch (WebException wex) { message = wex.Message; }
                catch (Exception ex) { message = ex.Message; }

                if (!success) System.Threading.Thread.Sleep(1000);
            }

            return result;
        }

        public static string GetUrl(string address, int port, string deviceName)
        {
            string url = null;

            if (address != null)
            {
                url = "http://";

                // Add Ip Address
                string ip = address;

                // Add Port
                string lport = null;
                // If port is in ip address
                if (ip.Contains(":"))
                {
                    int colonindex = ip.LastIndexOf(':');
                    int slashindex = -1;

                    // Get index of last forward slash
                    if (ip.Contains("/")) slashindex = ip.IndexOf('/', colonindex);

                    // Get port based on indexes
                    if (slashindex > colonindex) lport = ":" + ip.Substring(colonindex + 1, slashindex - colonindex - 1) + "/";
                    else lport = ":" + ip.Substring(colonindex + 1) + "/";

                    ip = ip.Substring(0, colonindex);
                }
                else
                {
                    if (port > 0) lport = ":" + port.ToString() + "/";
                }

                url += ip;
                url += lport;

                // Add Device Name
                string ldeviceName = null;
                if (deviceName != String.Empty)
                {
                    if (lport != null) ldeviceName = deviceName;
                    else ldeviceName = "/" + deviceName;
                    //ldeviceName += "/";
                }
                url += ldeviceName;

                if (url[url.Length - 1] != '/') url += "/";
            }

            return url;
        }

        public class ProxySettings
        {
            public ProxySettings() { }
            public ProxySettings(string address, int port)
            {
                Address = address;
                Port = port;
            }

            public string Address { get; set; }
            public int Port { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
        }

    }

}
