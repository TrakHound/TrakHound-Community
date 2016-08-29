// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Reflection;
using System.Xml;

using TrakHound.API;
using TrakHound.Configurations;
using TrakHound.Logging;
using TrakHound.Tools;

namespace TrakHound.Servers.DataStorage
{
    public class Backup
    {
        public const string DEFAULT_DATABASE_NAME = "Local-Server-Backup.db";
        public static string DEFAULT_DATABASE_PATH = Path.Combine(FileLocations.Backup, DEFAULT_DATABASE_NAME);


        public static void Create(List<Data.DeviceInfo> deviceInfos)
        {
            var config = Configuration.Read();

            // Create the database file
            Database.Create(config);

            // Create connection
            var connection = Connection.Create(config);
            if (connection != null)
            {
                // Create the Hours table
                Table.CreateHours(connection);

                // Clean the Hours table to remove all but the current day's data
                Table.CleanHours(connection);

                // Insert each of the HourInfos into the Hours table
                foreach (var deviceInfo in deviceInfos)
                {
                    foreach (var hourInfo in deviceInfo.Hours)
                    {
                        Row.InsertHourInfo(connection, deviceInfo.UniqueId, hourInfo);
                    }
                }

                // Close the database connection
                Connection.Close(connection);

                Logger.Log("SQLite Data Backup Successful", LogLineType.Console);
            }
        }

        public static List<Data.DeviceInfo> Load(DeviceConfiguration[] deviceConfigs)
        {
            var config = Configuration.Read();

            if (File.Exists(config.SQliteDatabasePath))
            {
                var connection = Connection.Create(config);
                if (connection != null)
                {
                    DataTable hoursTable = Table.GetHours(connection, deviceConfigs);
                    if (hoursTable != null)
                    {
                        var hourRowInfos = new List<Row.HourRowInfo>();

                        // Get HourRowInfo objects from DataRows
                        foreach (DataRow row in hoursTable.Rows)
                        {
                            var hourRowInfo = Row.GetHourInfo(connection, row);
                            if (hourRowInfo != null) hourRowInfos.Add(hourRowInfo);
                        }

                        var result = new List<Data.DeviceInfo>();

                        // Create DeviceInfo object for each HourRowInfo
                        foreach (var hourRowInfo in hourRowInfos)
                        {
                            Data.DeviceInfo deviceInfo = null;
                            deviceInfo = result.Find(o => o.UniqueId == hourRowInfo.UniqueId);
                            if (deviceInfo == null)
                            {
                                deviceInfo = new Data.DeviceInfo();
                                deviceInfo.UniqueId = hourRowInfo.UniqueId;
                                result.Add(deviceInfo);
                            }

                            deviceInfo.Hours.Add(hourRowInfo.HourInfo);
                        }

                        return result;
                    }

                    Connection.Close(connection);
                }
            }
            else Logger.Log("Local Server Backup File Not Found @ " + config.SQliteDatabasePath);

            return null;
        }


        public class Configuration
        {
            private const string CONFIG_FILENAME = "backup_config.xml";
            private static string CONFIG_FILEPATH = Path.Combine(FileLocations.TrakHound, CONFIG_FILENAME);

            public Configuration()
            {
                SQliteDatabasePath = DEFAULT_DATABASE_PATH;
            }

            public string SQliteDatabasePath { get; set; }


            public static bool Create(Configuration config)
            {
                bool result = false;

                Remove();

                if (config != null)
                {
                    var xml = CreateDocument(config);
                    Tools.XML.Files.WriteDocument(xml, CONFIG_FILEPATH);
                }

                return result;
            }

            public static Configuration Read()
            {
                var result = new Configuration();

                if (File.Exists(CONFIG_FILEPATH))
                {
                    try
                    {
                        var xml = new XmlDocument();
                        xml.Load(CONFIG_FILEPATH);

                        foreach (XmlNode node in xml.DocumentElement.ChildNodes)
                        {
                            if (node.NodeType == XmlNodeType.Element)
                            {
                                if (node.InnerText != "")
                                {
                                    Type c = typeof(Configuration);
                                    PropertyInfo info = c.GetProperty(node.Name);

                                    if (info != null)
                                    {
                                        Type t = info.PropertyType;
                                        info.SetValue(result, Convert.ChangeType(node.InnerText, t), null);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex) { Logger.Log("Exception :: " + ex.Message, LogLineType.Error); }
                }

                return result;
            }

            public static void Remove()
            {
                if (File.Exists(CONFIG_FILEPATH)) File.Delete(CONFIG_FILEPATH);
            }

            private static XmlDocument CreateDocument(Configuration config)
            {
                var result = new XmlDocument();

                XmlNode docNode = result.CreateXmlDeclaration("1.0", "UTF-8", null);
                result.AppendChild(docNode);

                XmlNode root = result.CreateElement("BackupConfiguration");
                result.AppendChild(root);

                foreach (var info in typeof(Configuration).GetProperties())
                {
                    XmlNode node = result.CreateElement(info.Name);
                    var val = info.GetValue(config, new object[] { });
                    if (val != null) node.InnerText = val.ToString();
                    root.AppendChild(node);
                }

                return result;
            }
        }


        public static class Connection
        {
            public static SQLiteConnection Create(Configuration config)
            {
                var connection = new SQLiteConnection(GetConnectionString(config));

                try
                {
                    connection.BusyTimeout = 5000;
                    connection.Open();

                    return connection;
                }
                catch (ObjectDisposedException ex)
                {
                    Logger.Log("ObjectDisposedException :: " + ex.Message, LogLineType.Error);
                }
                catch (InvalidOperationException ex)
                {
                    Logger.Log("InvalidOperationException :: " + ex.Message, LogLineType.Error);
                }
                catch (SQLiteException ex)
                {
                    Logger.Log("SQLiteException :: " + ex.Message, LogLineType.Error);
                }
                catch (Exception ex)
                {
                    Logger.Log("Exception :: " + ex.Message, LogLineType.Error);
                }

                return null;
            }

            public static void Close(SQLiteConnection connection)
            {
                try
                {
                    if (connection != null)
                    {
                        connection.Close();
                        connection.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("Exception :: " + ex.Message, LogLineType.Error);
                }
            }

            private static string GetConnectionString(Configuration config)
            {
                return "Data Source=" + config.SQliteDatabasePath + "; Version=3;";
            }
        }

        public static class Query
        {
            public static void Run(SQLiteConnection connection, string query)
            {
                try
                {
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                catch (ObjectDisposedException ex)
                {
                    Logger.Log("ObjectDisposedException :: " + ex.Message, LogLineType.Error);
                }
                catch (InvalidOperationException ex)
                {
                    Logger.Log("InvalidOperationException :: " + ex.Message, LogLineType.Error);
                }
                catch (SQLiteException ex)
                {
                    Logger.Log("SQLiteException :: " + ex.Message, LogLineType.Error);
                }
                catch (Exception ex)
                {
                    Logger.Log("Exception :: " + ex.Message, LogLineType.Error);
                }
            }

            public static DataTable GetDataTable(SQLiteConnection connection, string query)
            {
                try
                {
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        var dt = new DataTable();
                        using (var a = new SQLiteDataAdapter(command))
                        {
                            a.Fill(dt);
                        }
                        foreach (DataRow row in dt.Rows)
                        {
                            ConvertRowFromSafe(row);
                        }
                        return dt;
                    }
                }
                catch (ObjectDisposedException ex)
                {
                    Logger.Log("ObjectDisposedException :: " + ex.Message, LogLineType.Error);
                }
                catch (InvalidOperationException ex)
                {
                    Logger.Log("InvalidOperationException :: " + ex.Message, LogLineType.Error);
                }
                catch (SQLiteException ex)
                {
                    Logger.Log("SQLiteException :: " + ex.Message, LogLineType.Error);
                }
                catch (Exception ex)
                {
                    Logger.Log("Exception :: " + ex.Message, LogLineType.Error);
                }

                return null;
            }

            private static void ConvertRowFromSafe(DataRow row)
            {
                for (var x = 0; x <= row.ItemArray.Length - 1; x++)
                {
                    if (row.Table.Columns[x].DataType == typeof(string))
                    {
                        row[x] = String_Functions.FromSpecial(row.ItemArray[x].ToString());
                    }
                }
            }
        }


        public static class Database
        {
            public static void Create(Configuration config)
            {
                if (!File.Exists(config.SQliteDatabasePath))
                {
                    SQLiteConnection.CreateFile(config.SQliteDatabasePath);
                    Logger.Log("SQLite Database File Created : " + config.SQliteDatabasePath, LogLineType.Notification);
                }
            }
        }

        public static class Table
        {
            public static void CreateHours(SQLiteConnection connection)
            {
                string query = "CREATE TABLE IF NOT EXISTS `hours` (" +

                    "`unique_id` varchar(90), " +

                    "`date` varchar(90), " +
                    "`hour` int(2), " +

                    "`planned_production_time` double, " +
                    "`operating_time` double, " +
                    "`ideal_operating_time` double, " +
                    "`total_pieces` int(10), " +
                    "`good_pieces` int(10), " +

                    "`total_time` double, " +
                    "`active` double, " +
                    "`idle` double, " +
                    "`alert` double, " +

                    "`production` double, " +
                    "`setup` double, " +
                    "`teardown` double, " +
                    "`maintenance` double, " +
                    "`process_development` double, " +

                    "PRIMARY KEY (`unique_id`, `date`, `hour`))";

                Query.Run(connection, query);
            }

            public static void CleanHours(SQLiteConnection connection)
            {
                // Probably a more elegant way of getting the Time Zone Offset could be done here
                //int timeZoneOffset = (DateTime.UtcNow - DateTime.Now).Hours;

                string currentLocalDay = DateTime.Now.ToString(API.Data.HourInfo.DateFormat);
                //string currentUtcDay = DateTime.UtcNow.ToString(API.Data.HourInfo.DateFormat);

                string query = "DELETE FROM `hours` WHERE date < '" + currentLocalDay + "'";

                Query.Run(connection, query);
            }

            public static DataTable GetHours(SQLiteConnection connection, DeviceConfiguration[] deviceConfigs)
            {
                string query = "SELECT * FROM `hours` WHERE ";

                for (var x = 0; x < deviceConfigs.Length; x++)
                {
                    query += "`unique_id`='" + deviceConfigs[x].UniqueId + "'";
                    if (x < deviceConfigs.Length - 1) query += " OR ";
                }

                return Query.GetDataTable(connection, query);
            }
        }

        public static class Row
        {
            public static void InsertHourInfo(SQLiteConnection connection, string uniqueId, Data.HourInfo hourInfo)
            {
                string columnFormat = "`{0}`";
                string valueFormat = "'{0}'";

                string query = "INSERT OR REPLACE INTO `hours` (" +

                    string.Format(columnFormat, "unique_id") + ", " +

                    string.Format(columnFormat, "date") + ", " +
                    string.Format(columnFormat, "hour") + ", " +

                    string.Format(columnFormat, "planned_production_time") + ", " +
                    string.Format(columnFormat, "operating_time") + ", " +
                    string.Format(columnFormat, "ideal_operating_time") + ", " +
                    string.Format(columnFormat, "total_pieces") + ", " +
                    string.Format(columnFormat, "good_pieces") + ", " +

                    string.Format(columnFormat, "total_time") + ", " +
                    string.Format(columnFormat, "active") + ", " +
                    string.Format(columnFormat, "idle") + ", " +
                    string.Format(columnFormat, "alert") + ", " +

                    string.Format(columnFormat, "production") + ", " +
                    string.Format(columnFormat, "setup") + ", " +
                    string.Format(columnFormat, "teardown") + ", " +
                    string.Format(columnFormat, "maintenance") + ", " +
                    string.Format(columnFormat, "process_development") +

                    ") VALUES (" +

                    string.Format(valueFormat, uniqueId) + ", " +

                    string.Format(valueFormat, hourInfo.Date) + ", " +
                    string.Format(valueFormat, hourInfo.Hour) + ", " +

                    string.Format(valueFormat, hourInfo.PlannedProductionTime) + ", " +
                    string.Format(valueFormat, hourInfo.OperatingTime) + ", " +
                    string.Format(valueFormat, hourInfo.IdealOperatingTime) + ", " +
                    string.Format(valueFormat, hourInfo.TotalPieces) + ", " +
                    string.Format(valueFormat, hourInfo.GoodPieces) + ", " +

                    string.Format(valueFormat, hourInfo.TotalTime) + ", " +
                    string.Format(valueFormat, hourInfo.Active) + ", " +
                    string.Format(valueFormat, hourInfo.Idle) + ", " +
                    string.Format(valueFormat, hourInfo.Alert) + ", " +

                    string.Format(valueFormat, hourInfo.Production) + ", " +
                    string.Format(valueFormat, hourInfo.Setup) + ", " +
                    string.Format(valueFormat, hourInfo.Teardown) + ", " +
                    string.Format(valueFormat, hourInfo.Maintenance) + ", " +
                    string.Format(valueFormat, hourInfo.ProcessDevelopment) +

                    ")";


                Query.Run(connection, query);
            }

            public class HourRowInfo
            {
                public string UniqueId { get; set; }
                public Data.HourInfo HourInfo { get; set; }
            }

            public static HourRowInfo GetHourInfo(SQLiteConnection connection, DataRow row)
            {
                // Get Unique Id
                string uniqueId = DataTable_Functions.GetRowValue("unique_id", row);

                if (!string.IsNullOrEmpty(uniqueId))
                {
                    var result = new HourRowInfo();
                    result.UniqueId = uniqueId;

                    var hourInfo = new Data.HourInfo();

                    hourInfo.Date = DataTable_Functions.GetRowValue("date", row);
                    hourInfo.Hour = DataTable_Functions.GetIntegerFromRow("hour", row);

                    hourInfo.PlannedProductionTime = DataTable_Functions.GetDoubleFromRow("planned_production_time", row);
                    hourInfo.OperatingTime = DataTable_Functions.GetDoubleFromRow("operating_time", row);
                    hourInfo.IdealOperatingTime = DataTable_Functions.GetDoubleFromRow("ideal_operating_time", row);
                    hourInfo.TotalPieces = DataTable_Functions.GetIntegerFromRow("total_pieces", row);
                    hourInfo.GoodPieces = DataTable_Functions.GetIntegerFromRow("good_pieces", row);

                    hourInfo.TotalTime = DataTable_Functions.GetDoubleFromRow("total_time", row);

                    hourInfo.Active = DataTable_Functions.GetDoubleFromRow("active", row);
                    hourInfo.Idle = DataTable_Functions.GetDoubleFromRow("idle", row);
                    hourInfo.Alert = DataTable_Functions.GetDoubleFromRow("alert", row);

                    hourInfo.Production = DataTable_Functions.GetDoubleFromRow("production", row);
                    hourInfo.Setup = DataTable_Functions.GetDoubleFromRow("setup", row);
                    hourInfo.Teardown = DataTable_Functions.GetDoubleFromRow("teardown", row);
                    hourInfo.Maintenance = DataTable_Functions.GetDoubleFromRow("maintenance", row);
                    hourInfo.ProcessDevelopment = DataTable_Functions.GetDoubleFromRow("process_development", row);

                    result.HourInfo = hourInfo;

                    return result;
                }

                return null;
            }
        }
    }
}
