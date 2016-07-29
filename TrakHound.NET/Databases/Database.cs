// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;

using TrakHound.Configurations;
using TrakHound.Plugins.Database;
using TrakHound.Logging;

namespace TrakHound.Databases
{
    public static class Global
    {

        public static List<IDatabasePlugin> Plugins;

        public static bool UseMultithreading = false;

        public static void Initialize(Database_Settings settings)
        {
            if (Global.Plugins != null)
            {
                foreach (Database_Configuration db in settings.Databases)
                {
                    foreach (var plugin in Global.Plugins)
                    {
                        try
                        {
                            if (Global.CheckType(plugin, db))
                            {
                                plugin.Initialize(db);
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Log("Initialize : Exception : " + ex.Message, LogLineType.Error);
                        }                       
                    }
                }
            }
        }

        public static bool Ping(Database_Configuration config, out string msg)
        {
            bool result = false;
            msg = null;

            if (Global.Plugins != null)
            {
                foreach (var plugin in Global.Plugins)
                {
                    try
                    {
                        if (CheckType(plugin, config))
                        {
                            result = plugin.Ping(config.Configuration, out msg);
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Ping : Exception : " + ex.Message, LogLineType.Error);
                    }     
                }
            }

            return result;
        }
        
        public const string DateString = "yyyy-MM-dd H:mm:ss";

        /// <summary>
        /// Returns DateString as a string in the format needed to be imported into MySQL table
        /// Returns null (as string for mysql) if it cannot parse
        /// </summary>
        /// <param name="DateString">DateTime as a string</param>
        /// <returns></returns>
        public static string ConvertDateStringtoSQL(string DateString)
        {
            string Result = "null";

            DateTime TS;
            if (DateTime.TryParse(DateString, out TS)) Result = TS.ToString(DateString);

            return Result;
        }

        public static bool CheckType(IDatabasePlugin plugin, Database_Configuration config)
        {
            bool match = false;

            if (plugin.Type.ToLower().Replace('_', ' ') == config.Type.ToLower().Replace('_', ' '))
            {
                match = true;
            }

            return match;
        }

        public static string GetTableName(string tablename, string databaseId)
        {
            if (!string.IsNullOrEmpty(databaseId)) return databaseId + "_" + tablename;
            return tablename;
        }

    }

    public static class Database
    {

        #region "Worker Info Classes"

        class CreateWorkerInfo
        {
            public object configuration { get; set; }

            public IDatabasePlugin plugin { get; set; }
        }

        class DropWorkerInfo
        {
            public object configuration { get; set; }

            public IDatabasePlugin plugin { get; set; }
        }

        #endregion

        public static void Create(Database_Settings settings)
        {
            if (Global.Plugins != null)
            {
                foreach (Database_Configuration db in settings.Databases)
                {
                    foreach (var plugin in Global.Plugins)
                    {
                        if (Global.CheckType(plugin, db))
                        {
                            CreateWorkerInfo info = new CreateWorkerInfo();
                            info.configuration = db.Configuration;
                            info.plugin = plugin;

                            if (Global.UseMultithreading) ThreadPool.QueueUserWorkItem(new WaitCallback(Create_Worker), info);
                            else Create_Worker(info);

                            break;
                        }
                    }
                }
            }
        }

        static void Create_Worker(object o)
        {
            try
            {
                if (o.GetType() == typeof(CreateWorkerInfo))
                {
                    CreateWorkerInfo info = (CreateWorkerInfo)o;

                    info.plugin.Database_Create(info.configuration);
                }
            }
            catch (Exception ex) { Logger.Log("Database.CreateWorker() : Exception : " + ex.Message, LogLineType.Error); }
        }

        public static void Drop(Database_Settings settings, string databaseName)
        {
            if (Global.Plugins != null)
            {
                foreach (Database_Configuration db in settings.Databases)
                {
                    foreach (var plugin in Global.Plugins)
                    {
                        if (Global.CheckType(plugin, db))
                        {
                            DropWorkerInfo info = new DropWorkerInfo();
                            info.configuration = db.Configuration;
                            info.plugin = plugin;

                            if (Global.UseMultithreading) ThreadPool.QueueUserWorkItem(new WaitCallback(Drop_Worker), info);
                            else Drop_Worker(info);

                            break;
                        }
                    }
                }
            }
        }

        static void Drop_Worker(object o)
        {
            try
            {
                if (o.GetType() == typeof(DropWorkerInfo))
                {
                    DropWorkerInfo info = (DropWorkerInfo)o;

                    info.plugin.Database_Drop(info.configuration);
                }
            }
            catch (Exception ex) { Logger.Log("Database.DropWorker() : Exception : " + ex.Message, LogLineType.Error); }
        }

    }

    public static class Table
    {

        #region "Worker Info Classes"

        class CreateWorkerInfo
        {
            public object configuration { get; set; }
            public string tablename { get; set; }
            public ColumnDefinition[] columnDefinitions { get; set; }
            public string[] primaryKey { get; set; }

            public IDatabasePlugin plugin { get; set; }
        }

        class ReplaceWorkerInfo
        {
            public object configuration { get; set; }
            public string tablename { get; set; }
            public ColumnDefinition[] columnDefinitions { get; set; }
            public string[] primaryKey { get; set; }

            public IDatabasePlugin plugin { get; set; }
        }

        class DropWorkerInfo1
        {
            public object configuration { get; set; }
            public string tablename { get; set; }

            public IDatabasePlugin plugin { get; set; }
        }

        class DropWorkerInfo2
        {
            public object configuration { get; set; }
            public string[] tablenames { get; set; }

            public IDatabasePlugin plugin { get; set; }
        }

        class TruncateWorkerInfo
        {
            public object configuration { get; set; }
            public string tablename { get; set; }

            public IDatabasePlugin plugin { get; set; }
        }

        #endregion

        public static void Create(Database_Settings settings, string tablename, ColumnDefinition[] columnDefinitions, string[] primaryKey)
        {
            if (Global.Plugins != null)
            {
                foreach (Database_Configuration db in settings.Databases)
                {
                    foreach (var plugin in Global.Plugins)
                    {
                        if (Global.CheckType(plugin, db))
                        {
                            CreateWorkerInfo info = new CreateWorkerInfo();
                            info.configuration = db.Configuration;
                            info.tablename = tablename;
                            info.columnDefinitions = columnDefinitions;
                            info.primaryKey = primaryKey;
                            info.plugin = plugin;

                            if (Global.UseMultithreading) ThreadPool.QueueUserWorkItem(new WaitCallback(Create_Worker), info);
                            else Create_Worker(info);

                            break;
                        }
                    }
                }
            }
        }

        static void Create_Worker(object o)
        {
            try
            {
                if (o.GetType() == typeof(CreateWorkerInfo))
                {
                    CreateWorkerInfo info = (CreateWorkerInfo)o;

                    info.plugin.Table_Create(info.configuration, info.tablename, info.columnDefinitions, info.primaryKey);
                }
            }
            catch (Exception ex) { Logger.Log("Table.CreateWorker() : Exception : " + ex.Message, LogLineType.Error); }
        }


        public static void Replace(Database_Settings settings, string tablename, ColumnDefinition[] columnDefinitions, string[] primaryKey)
        {
            if (Global.Plugins != null)
            {
                foreach (Database_Configuration db in settings.Databases)
                {
                    foreach (var plugin in Global.Plugins)
                    {
                        if (Global.CheckType(plugin, db))
                        {
                            ReplaceWorkerInfo info = new ReplaceWorkerInfo();
                            info.configuration = db.Configuration;
                            info.tablename = tablename;
                            info.columnDefinitions = columnDefinitions;
                            info.primaryKey = primaryKey;
                            info.plugin = plugin;

                            if (Global.UseMultithreading) ThreadPool.QueueUserWorkItem(new WaitCallback(Replace_Worker), info);
                            else Replace_Worker(info);

                            break;
                        }
                    }
                }
            }
        }

        static void Replace_Worker(object o)
        {
            try
            {
                if (o.GetType() == typeof(ReplaceWorkerInfo))
                {
                    ReplaceWorkerInfo info = (ReplaceWorkerInfo)o;

                    info.plugin.Table_Replace(info.configuration, info.tablename, info.columnDefinitions, info.primaryKey);
                }
            }
            catch (Exception ex) { Logger.Log("Table.ReplaceWorker() : Exception : " + ex.Message, LogLineType.Error); }
        }


        public static void Drop(Database_Settings settings, string tablename)
        {
            if (Global.Plugins != null)
            {
                foreach (Database_Configuration db in settings.Databases)
                {
                    foreach (var plugin in Global.Plugins)
                    {
                        if (Global.CheckType(plugin, db))
                        {
                            DropWorkerInfo1 info = new DropWorkerInfo1();
                            info.configuration = db.Configuration;
                            info.tablename = tablename;
                            info.plugin = plugin;

                            if (Global.UseMultithreading) ThreadPool.QueueUserWorkItem(new WaitCallback(Drop_Worker1), info);
                            else Drop_Worker1(info);

                            break;
                        }
                    }
                }
            }
        }

        static void Drop_Worker1(object o)
        {
            try
            {
                if (o.GetType() == typeof(DropWorkerInfo1))
                {
                    DropWorkerInfo1 info = (DropWorkerInfo1)o;

                    info.plugin.Table_Drop(info.configuration, info.tablename);
                }
            }
            catch (Exception ex) { Logger.Log("Table.DropWorker1() : Exception : " + ex.Message, LogLineType.Error); }
        }


        public static void Drop(Database_Settings settings, string[] tablenames)
        {
            if (Global.Plugins != null)
            {
                foreach (Database_Configuration db in settings.Databases)
                {
                    foreach (var plugin in Global.Plugins)
                    {
                        if (Global.CheckType(plugin, db))
                        {
                            DropWorkerInfo2 info = new DropWorkerInfo2();
                            info.configuration = db.Configuration;
                            info.tablenames = tablenames;
                            info.plugin = plugin;

                            if (Global.UseMultithreading) ThreadPool.QueueUserWorkItem(new WaitCallback(Drop_Worker2), info);
                            else Drop_Worker2(info);

                            break;
                        }
                    }
                }
            }
        }

        static void Drop_Worker2(object o)
        {
            try
            {
                if (o.GetType() == typeof(DropWorkerInfo2))
                {
                    DropWorkerInfo2 info = (DropWorkerInfo2)o;

                    info.plugin.Table_Drop(info.configuration, info.tablenames);
                }
            }
            catch (Exception ex) { Logger.Log("Table.DropWorker2() : Exception : " + ex.Message, LogLineType.Error); }
        }


        public static void Truncate(Database_Settings settings, string tablename)
        {
            if (Global.Plugins != null)
            {
                foreach (Database_Configuration db in settings.Databases)
                {
                    foreach (var plugin in Global.Plugins)
                    {
                        if (Global.CheckType(plugin, db))
                        {
                            TruncateWorkerInfo info = new TruncateWorkerInfo();
                            info.configuration = db.Configuration;
                            info.tablename = tablename;
                            info.plugin = plugin;

                            if (Global.UseMultithreading) ThreadPool.QueueUserWorkItem(new WaitCallback(Truncate_Worker), info);
                            else Truncate_Worker(info);

                            break;
                        }
                    }
                }
            }
        }

        static void Truncate_Worker(object o)
        {
            try
            {
                if (o.GetType() == typeof(TruncateWorkerInfo))
                {
                    TruncateWorkerInfo info = (TruncateWorkerInfo)o;

                    info.plugin.Table_Truncate(info.configuration, info.tablename);
                }
            }
            catch (Exception ex) { Logger.Log("Table.TruncateWorker() : Exception : " + ex.Message, LogLineType.Error); }
        }


        public static DataTable Get(Database_Settings settings, string tablename)
        {
            DataTable result = null;
            bool found = false;

            foreach (var database in settings.Databases)
            {
                if (Global.Plugins != null)
                {
                    foreach (var plugin in Global.Plugins)
                    {
                        if (Global.CheckType(plugin, database))
                        {
                            result = plugin.Table_Get(database.Configuration, tablename);
                            found = true;
                            break;
                        }
                    }
                }
                if (found) break;
            }

            return result;
        }

        public static DataTable Get(Database_Settings settings, string tablename, Int64 limit, Int64 offset)
        {
            DataTable result = null;
            bool found = false;

            foreach (var database in settings.Databases)
            {
                if (Global.Plugins != null)
                {
                    foreach (var plugin in Global.Plugins)
                    {
                        if (Global.CheckType(plugin, database))
                        {
                            result = plugin.Table_Get(database.Configuration, tablename, limit, offset);
                            found = true;
                            break;
                        }
                    }
                }
                if (found) break;
            }

            return result;
        }

        public static DataTable Get(Database_Settings settings, string tablename, string filterExpression)
        {
            DataTable result = null;
            bool found = false;

            foreach (var database in settings.Databases)
            {
                if (Global.Plugins != null)
                {
                    foreach (var plugin in Global.Plugins)
                    {
                        if (Global.CheckType(plugin, database))
                        {
                            result = plugin.Table_Get(database.Configuration, tablename, filterExpression);
                            found = true;
                            break;
                        }
                    }
                }
                if (found) break;
            }

            return result;
        }

        public static DataTable Get(Database_Settings settings, string tablename, string filterExpression, string columns)
        {
            DataTable result = null;
            bool found = false;

            foreach (var database in settings.Databases)
            {
                if (Global.Plugins != null)
                {
                    foreach (var plugin in Global.Plugins)
                    {
                        if (Global.CheckType(plugin, database))
                        {
                            result = plugin.Table_Get(database.Configuration, tablename, filterExpression, columns);
                            found = true;
                            break;
                        }
                    }
                }
                if (found) break;
            }

            return result;
        }

        public static DataTable[] Get(Database_Settings settings, string[] tablenames)
        {
            DataTable[] result = null;
            bool found = false;

            foreach (var database in settings.Databases)
            {
                if (Global.Plugins != null)
                {
                    foreach (var plugin in Global.Plugins)
                    {
                        if (Global.CheckType(plugin, database))
                        {
                            result = plugin.Table_Get(database.Configuration, tablenames);
                            found = true;
                            break;
                        }
                    }
                }
                if (found) break;
            }

            return result;
        }

        public static DataTable[] Get(Database_Settings settings, string[] tablenames, string[] filterExpressions)
        {
            DataTable[] result = null;
            bool found = false;

            foreach (var database in settings.Databases)
            {
                if (Global.Plugins != null)
                {
                    foreach (var plugin in Global.Plugins)
                    {
                        if (Global.CheckType(plugin, database))
                        {
                            result = plugin.Table_Get(database.Configuration, tablenames, filterExpressions);
                            found = true;
                            break;
                        }
                    }
                }
                if (found) break;
            }

            return result;
        }

        public static DataTable[] Get(Database_Settings settings, string[] tablenames, string[] filterExpressions, string[] columns)
        {
            DataTable[] result = null;
            bool found = false;

            foreach (var database in settings.Databases)
            {
                if (Global.Plugins != null)
                {
                    foreach (var plugin in Global.Plugins)
                    {
                        if (Global.CheckType(plugin, database))
                        {
                            result = plugin.Table_Get(database.Configuration, tablenames, filterExpressions, columns);
                            found = true;
                            break;
                        }
                    }
                }
                if (found) break;
            }

            return result;
        }


        public static string[] List(Database_Settings settings)
        {
            string[] result = null;
            bool found = false;

            foreach (var database in settings.Databases)
            {
                if (Global.Plugins != null)
                {
                    foreach (var plugin in Global.Plugins)
                    {
                        if (Global.CheckType(plugin, database))
                        {
                            result = plugin.Table_List(database.Configuration);
                            found = true;
                            break;
                        }
                    }
                }
                if (found) break;
            }

            return result;
        }

        public static string[] List(Database_Settings settings, string filterExpression)
        {
            string[] result = null;
            bool found = false;

            foreach (var database in settings.Databases)
            {
                if (Global.Plugins != null)
                {
                    foreach (var plugin in Global.Plugins)
                    {
                        if (Global.CheckType(plugin, database))
                        {
                            result = plugin.Table_List(database.Configuration, filterExpression);
                            found = true;
                            break;
                        }
                    }
                }
                if (found) break;
            }

            return result;
        }


        public static Int64 GetRowCount(Database_Settings settings, string tablename)
        {
            Int64 result = -1;
            bool found = false;

            foreach (var database in settings.Databases)
            {
                if (Global.Plugins != null)
                {
                    foreach (var plugin in Global.Plugins)
                    {
                        if (Global.CheckType(plugin, database))
                        {
                            result = plugin.Table_GetRowCount(database.Configuration, tablename);
                            found = true;
                            break;
                        }
                    }
                }
                if (found) break;
            }

            return result;
        }

        public static Int64 GetSize(Database_Settings settings, string tablename)
        {
            Int64 result = -1;
            bool found = false;

            foreach (var database in settings.Databases)
            {
                if (Global.Plugins != null)
                {
                    foreach (var plugin in Global.Plugins)
                    {
                        if (Global.CheckType(plugin, database))
                        {
                            result = plugin.Table_GetSize(database.Configuration, tablename);
                            found = true;
                            break;
                        }
                    }
                }
                if (found) break;
            }

            return result;
        }

    }

    public static class Column
    {

        #region "Worker Info Classes"

        class AddWorkerInfo
        {
            public object configuration { get; set; }
            public string tablename { get; set; }
            public ColumnDefinition columnDefinition { get; set; }

            public IDatabasePlugin plugin { get; set; }
        }

        #endregion

        public static List<string> Get(Database_Settings settings, string tablename)
        {
            List<string> result = new List<string>();
            bool found = false;

            foreach (var database in settings.Databases)
            {
                if (Global.Plugins != null)
                {
                    foreach (var plugin in Global.Plugins)
                    {
                        if (Global.CheckType(plugin, database))
                        {
                            result = plugin.Column_Get(database.Configuration, tablename);
                            found = true;
                            break;
                        }
                    }
                }
                if (found) break;
            }

            return result;
        }

        public static void Add(Database_Settings settings, string tablename, ColumnDefinition columnDefinition)
        {
            if (Global.Plugins != null)
            {
                foreach (Database_Configuration db in settings.Databases)
                {
                    foreach (var plugin in Global.Plugins)
                    {
                        if (Global.CheckType(plugin, db))
                        {
                            AddWorkerInfo info = new AddWorkerInfo();
                            info.configuration = db.Configuration;
                            info.tablename = tablename;
                            info.columnDefinition = columnDefinition;
                            info.plugin = plugin;

                            if (Global.UseMultithreading) ThreadPool.QueueUserWorkItem(new WaitCallback(AddWorker), info);
                            else AddWorker(info);

                            break;
                        }
                    }
                }
            }
        }

        static void AddWorker(object o)
        {
            try
            {
                if (o.GetType() == typeof(AddWorkerInfo))
                {
                    AddWorkerInfo info = (AddWorkerInfo)o;

                    info.plugin.Column_Add(info.configuration, info.tablename, info.columnDefinition);
                }
            }
            catch (Exception ex) { Logger.Log("Column.AddWorker() : Exception : " + ex.Message, LogLineType.Error); }
        }

    }

    public static class Row
    {

        #region "Worker Info Classes"

        class InsertWorkerInfo1
        {
            public object configuration { get; set; }
            public string tablename { get; set; }
            public object[] columns { get; set; }
            public object[] values { get; set; }
            public string[] primaryKey { get; set; }
            public bool update { get; set; }

            public IDatabasePlugin plugin { get; set; }
        }

        class InsertWorkerInfo2
        {
            public object configuration { get; set; }
            public string tablename { get; set; }
            public object[] columns { get; set; }
            public List<List<object>> values { get; set; }
            public string[] primaryKey { get; set; }
            public bool update { get; set; }

            public IDatabasePlugin plugin { get; set; }
        }

        class InsertWorkerInfo3
        {
            public object configuration { get; set; }
            public string tablename { get; set; }
            public List<object[]> columnsList { get; set; }
            public List<object[]> valuesList { get; set; }
            public string[] primaryKey { get; set; }
            public bool update { get; set; }

            public IDatabasePlugin plugin { get; set; }
        }

        class InsertWorkerInfo4
        {
            public object configuration { get; set; }
            public string query { get; set; }

            public IDatabasePlugin plugin { get; set; }
        }

        #endregion

        public static void Insert(Database_Settings settings, string tablename, object[] columns, object[] values, string[] primaryKey, bool update)
        {
            if (Global.Plugins != null && values.Length > 0)
            {
                foreach (Database_Configuration db in settings.Databases)
                {
                    foreach (var plugin in Global.Plugins)
                    {
                        if (Global.CheckType(plugin, db))
                        {
                            InsertWorkerInfo1 info = new InsertWorkerInfo1();
                            info.configuration = db.Configuration;
                            info.tablename = tablename;
                            info.columns = columns;
                            info.values = values;
                            info.primaryKey = primaryKey;
                            info.update = update;
                            info.plugin = plugin;

                            if (Global.UseMultithreading) ThreadPool.QueueUserWorkItem(new WaitCallback(InsertWorker1), info);
                            else InsertWorker1(info);

                            break;
                        }
                    }
                }
            }
        }

        static void InsertWorker1(object o)
        {
            try
            {
                if (o.GetType() == typeof(InsertWorkerInfo1))
                {
                    InsertWorkerInfo1 info = (InsertWorkerInfo1)o;

                    info.plugin.Row_Insert(info.configuration, info.tablename, info.columns, info.values, info.primaryKey, info.update);
                }
            }
            catch (Exception ex) { Logger.Log("Column.InsertWorker1() : Exception : " + ex.Message, LogLineType.Error); }
        }


        public static void Insert(Database_Settings settings, string tablename, object[] columns, List<List<object>> values, string[] primaryKey, bool update)
        {
            if (Global.Plugins != null && values.Count > 0)
            {
                foreach (Database_Configuration db in settings.Databases)
                {
                    foreach (var plugin in Global.Plugins)
                    {
                        if (Global.CheckType(plugin, db))
                        {
                            InsertWorkerInfo2 info = new InsertWorkerInfo2();
                            info.configuration = db.Configuration;
                            info.tablename = tablename;
                            info.columns = columns;
                            info.values = values;
                            info.primaryKey = primaryKey;
                            info.update = update;
                            info.plugin = plugin;

                            if (Global.UseMultithreading) ThreadPool.QueueUserWorkItem(new WaitCallback(InsertWorker2), info);
                            else InsertWorker2(info);

                            break;
                        }
                    }
                }
            }
        }

        static void InsertWorker2(object o)
        {
            try
            {
                if (o.GetType() == typeof(InsertWorkerInfo2))
                {
                    InsertWorkerInfo2 info = (InsertWorkerInfo2)o;

                    info.plugin.Row_Insert(info.configuration, info.tablename, info.columns, info.values, info.primaryKey, info.update);
                }
            }
            catch (Exception ex) { Logger.Log("Column.InsertWorker2() : Exception : " + ex.Message, LogLineType.Error); }
        }


        public static void Insert(Database_Settings settings, string tablename, List<object[]> columnsList, List<object[]> valuesList, string[] primaryKey, bool update)
        {
            if (Global.Plugins != null && valuesList.Count > 0)
            {
                foreach (Database_Configuration db in settings.Databases)
                {
                    foreach (var plugin in Global.Plugins)
                    {
                        if (Global.CheckType(plugin, db))
                        {
                            InsertWorkerInfo3 info = new InsertWorkerInfo3();
                            info.configuration = db.Configuration;
                            info.tablename = tablename;
                            info.columnsList = columnsList;
                            info.valuesList = valuesList;
                            info.primaryKey = primaryKey;
                            info.update = update;
                            info.plugin = plugin;

                            if (Global.UseMultithreading) ThreadPool.QueueUserWorkItem(new WaitCallback(InsertWorker3), info);
                            else InsertWorker3(info);

                            break;
                        }
                    }
                }
            }
        }

        static void InsertWorker3(object o)
        {
            try
            {
                if (o.GetType() == typeof(InsertWorkerInfo3))
                {
                    InsertWorkerInfo3 info = (InsertWorkerInfo3)o;

                    info.plugin.Row_Insert(info.configuration, info.tablename, info.columnsList, info.valuesList, info.primaryKey, info.update);
                }
            }
            catch (Exception ex) { Logger.Log("Column.InsertWorker3() : Exception : " + ex.Message, LogLineType.Error); }
        }


        public static void Insert(Database_Settings settings, string query)
        {
            if (Global.Plugins != null)
            {
                foreach (Database_Configuration db in settings.Databases)
                {
                    foreach (var plugin in Global.Plugins)
                    {
                        if (Global.CheckType(plugin, db))
                        {
                            InsertWorkerInfo4 info = new InsertWorkerInfo4();
                            info.configuration = db.Configuration;
                            info.query = query;
                            info.plugin = plugin;

                            if (Global.UseMultithreading) ThreadPool.QueueUserWorkItem(new WaitCallback(InsertWorker4), info);
                            else InsertWorker4(info);

                            break;
                        }
                    }
                }
            }
        }

        static void InsertWorker4(object o)
        {
            try
            {
                if (o.GetType() == typeof(InsertWorkerInfo4))
                {
                    InsertWorkerInfo4 info = (InsertWorkerInfo4)o;

                    info.plugin.Row_Insert(info.configuration, info.query);
                }
            }
            catch (Exception ex) { Logger.Log("Column.InsertWorker4() : Exception : " + ex.Message, LogLineType.Error); }
        }


        public static DataRow Get(Database_Settings settings, string tablename, string tableKey, string rowKey)
        {
            DataRow result = null;
            bool found = false;

            foreach (var database in settings.Databases)
            {
                if (Global.Plugins != null)
                {
                    foreach (var plugin in Global.Plugins)
                    {
                        if (Global.CheckType(plugin, database))
                        {
                            result = plugin.Row_Get(database.Configuration, tablename, tableKey, rowKey);
                            found = true;
                            break;
                        }
                    }
                }
                if (found) break;
            }

            return result;
        }

        public static DataRow Get(Database_Settings settings, string tablename, string query)
        {
            DataRow result = null;
            bool found = false;

            foreach (var database in settings.Databases)
            {
                if (Global.Plugins != null)
                {
                    foreach (var plugin in Global.Plugins)
                    {
                        if (Global.CheckType(plugin, database))
                        {
                            result = plugin.Row_Get(database.Configuration, tablename, query);
                            found = true;
                            break;
                        }
                    }
                }
                if (found) break;
            }

            return result;
        }


        // only returns whether row exists in first (primary) database
        public static bool Exists(Database_Settings settings, string tablename, string filterString)
        {
            bool result = false;
            bool found = false;

            foreach (var database in settings.Databases)
            {
                if (Global.Plugins != null)
                {
                    foreach (var plugin in Global.Plugins)
                    {
                        if (Global.CheckType(plugin, database))
                        {
                            result = plugin.Row_Exists(database.Configuration, tablename, filterString);
                            found = true;
                            break;
                        }
                    }
                }
                if (found) break;
            }

            return result;
        }

    }

    public static class Etc
    {

        #region "Worker Info Classes"

        class CustomCommandWorkerInfo
        {
            public object configuration { get; set; }
            public string commandText { get; set; }

            public IDatabasePlugin plugin { get; set; }
        }

        #endregion


        public static string CustomCommand(Database_Settings settings, string commandText)
        {
            string result = null;

            if (Global.Plugins != null)
            {
                if (settings.Databases.Count > 0)
                {
                    for (int x = 0; x <= settings.Databases.Count - 1; x++)
                    {
                        Database_Configuration db = settings.Databases[x];

                        foreach (var plugin in Global.Plugins)
                        {
                            if (Global.CheckType(plugin, db))
                            {
                                if (x == 0) result = plugin.CustomCommand(db.Configuration, commandText);
                                else
                                {
                                    CustomCommandWorkerInfo info = new CustomCommandWorkerInfo();
                                    info.configuration = db.Configuration;
                                    info.commandText = commandText;
                                    info.plugin = plugin;

                                    if (Global.UseMultithreading) ThreadPool.QueueUserWorkItem(new WaitCallback(CustomCommandWorker), info);
                                    else CustomCommandWorker(info);
                                }
                                break;
                            }
                        }
                    }
                }
            }

            return result;
        }

        static void CustomCommandWorker(object o)
        {
            try
            {
                if (o.GetType() == typeof(CustomCommandWorkerInfo))
                {
                    CustomCommandWorkerInfo info = (CustomCommandWorkerInfo)o;

                    info.plugin.CustomCommand(info.configuration, info.commandText);
                }
            }
            catch (Exception ex) { Logger.Log("Column.CustomCommandWorker() : Exception : " + ex.Message, LogLineType.Error); }
        }


        public static object GetValue(Database_Settings settings, string tablename, string column, string filterExpression)
        {
            object result = null;
            bool found = false;

            foreach (var database in settings.Databases)
            {
                if (Global.Plugins != null)
                {
                    foreach (var plugin in Global.Plugins)
                    {
                        if (Global.CheckType(plugin, database))
                        {
                            result = plugin.GetValue(database.Configuration, tablename, column, filterExpression);
                            found = true;
                            break;
                        }
                    }
                }
                if (found) break;
            }

            return result;
        }

        // May need some more work (return DataSet with table for each database?)
        public static DataTable GetGrants(Database_Settings settings, string username)
        {
            DataTable result = null;
            bool found = false;

            foreach (var database in settings.Databases)
            {
                if (Global.Plugins != null)
                {
                    foreach (var plugin in Global.Plugins)
                    {
                        if (Global.CheckType(plugin, database))
                        {
                            result = plugin.Table_Get(database.Configuration, username);
                            found = true;
                            break;
                        }
                    }
                }
                if (found) break;
            }

            return result;
        }

    }

}
