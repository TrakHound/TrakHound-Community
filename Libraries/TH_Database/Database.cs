using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;

using TH_Configuration;
using TH_Global;

namespace TH_Database
{
    public static class Global
    {

        public static List<Lazy<IDatabasePlugin>> Plugins;

        public static bool UseMultithreading = true;

        public static void Initialize(Database_Settings settings)
        {
            if (Global.Plugins != null)
            {
                foreach (Database_Configuration db in settings.Databases)
                {
                    foreach (Lazy<IDatabasePlugin> ldp in Global.Plugins.ToList())
                    {
                        try
                        {
                            IDatabasePlugin dp = ldp.Value;

                            if (Global.CheckType(dp, db))
                            {
                                dp.Initialize(db);
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Log("Initialize : Exception : " + ex.Message);
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
                foreach (Lazy<IDatabasePlugin> ldp in Global.Plugins.ToList())
                {
                    try
                    {
                        IDatabasePlugin dp = ldp.Value;

                        if (CheckType(dp, config))
                        {
                            result = dp.Ping(config.Configuration, out msg);
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Ping : Exception : " + ex.Message);
                    }     
                }
            }

            return result;
        }

        public static bool CheckPermissions(Database_Configuration config, Application_Type type)
        {
            bool result = false;

            if (Global.Plugins != null)
            {
                foreach (Lazy<IDatabasePlugin> ldp in Global.Plugins.ToList())
                {
                    try
                    {
                        IDatabasePlugin dp = ldp.Value;

                        if (Global.CheckType(dp, config))
                        {
                            result = dp.CheckPermissions(config.Configuration, type);
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("CheckPermissions : Exception : " + ex.Message);
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
                    foreach (Lazy<IDatabasePlugin> ldp in Global.Plugins)
                    {
                        IDatabasePlugin dp = ldp.Value;

                        if (Global.CheckType(dp, db))
                        {
                            CreateWorkerInfo info = new CreateWorkerInfo();
                            info.configuration = db.Configuration;
                            info.plugin = dp;

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
            catch (Exception ex) { Logger.Log("Database.CreateWorker() : Exception : " + ex.Message); }
        }

        public static void Drop(Database_Settings settings, string databaseName)
        {
            if (Global.Plugins != null)
            {
                foreach (Database_Configuration db in settings.Databases)
                {
                    foreach (Lazy<IDatabasePlugin> ldp in Global.Plugins)
                    {
                        IDatabasePlugin dp = ldp.Value;

                        if (Global.CheckType(dp, db))
                        {
                            DropWorkerInfo info = new DropWorkerInfo();
                            info.configuration = db.Configuration;
                            info.plugin = dp;

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
            catch (Exception ex) { Logger.Log("Database.DropWorker() : Exception : " + ex.Message); }
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
                    foreach (Lazy<IDatabasePlugin> ldp in Global.Plugins)
                    {
                        IDatabasePlugin dp = ldp.Value;

                        if (Global.CheckType(dp, db))
                        {
                            CreateWorkerInfo info = new CreateWorkerInfo();
                            info.configuration = db.Configuration;
                            info.tablename = tablename;
                            info.columnDefinitions = columnDefinitions;
                            info.primaryKey = primaryKey;
                            info.plugin = dp;

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
            catch (Exception ex) { Logger.Log("Table.CreateWorker() : Exception : " + ex.Message); }
        }


        public static void Drop(Database_Settings settings, string tablename)
        {
            if (Global.Plugins != null)
            {
                foreach (Database_Configuration db in settings.Databases)
                {
                    foreach (Lazy<IDatabasePlugin> ldp in Global.Plugins)
                    {
                        IDatabasePlugin dp = ldp.Value;

                        if (Global.CheckType(dp, db))
                        {
                            DropWorkerInfo1 info = new DropWorkerInfo1();
                            info.configuration = db.Configuration;
                            info.tablename = tablename;
                            info.plugin = dp;

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
            catch (Exception ex) { Logger.Log("Table.DropWorker1() : Exception : " + ex.Message); }
        }


        public static void Drop(Database_Settings settings, string[] tablenames)
        {
            if (Global.Plugins != null)
            {
                foreach (Database_Configuration db in settings.Databases)
                {
                    foreach (Lazy<IDatabasePlugin> ldp in Global.Plugins)
                    {
                        IDatabasePlugin dp = ldp.Value;

                        if (Global.CheckType(dp, db))
                        {
                            DropWorkerInfo2 info = new DropWorkerInfo2();
                            info.configuration = db.Configuration;
                            info.tablenames = tablenames;
                            info.plugin = dp;

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
            catch (Exception ex) { Logger.Log("Table.DropWorker2() : Exception : " + ex.Message); }
        }


        public static void Truncate(Database_Settings settings, string tablename)
        {
            if (Global.Plugins != null)
            {
                foreach (Database_Configuration db in settings.Databases)
                {
                    foreach (Lazy<IDatabasePlugin> ldp in Global.Plugins)
                    {
                        IDatabasePlugin dp = ldp.Value;

                        if (Global.CheckType(dp, db))
                        {
                            TruncateWorkerInfo info = new TruncateWorkerInfo();
                            info.configuration = db.Configuration;
                            info.tablename = tablename;
                            info.plugin = dp;

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
            catch (Exception ex) { Logger.Log("Table.TruncateWorker() : Exception : " + ex.Message); }
        }


        public static DataTable Get(Database_Settings settings, string tablename)
        {
            DataTable result = null;
            bool found = false;

            foreach (var database in settings.Databases)
            {
                if (Global.Plugins != null)
                {
                    foreach (Lazy<IDatabasePlugin> ldp in Global.Plugins)
                    {
                        IDatabasePlugin dp = ldp.Value;

                        if (Global.CheckType(dp, database))
                        {
                            result = dp.Table_Get(database.Configuration, tablename);
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
                    foreach (Lazy<IDatabasePlugin> ldp in Global.Plugins)
                    {
                        IDatabasePlugin dp = ldp.Value;

                        if (Global.CheckType(dp, database))
                        {
                            result = dp.Table_Get(database.Configuration, tablename, limit, offset);
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
                    foreach (Lazy<IDatabasePlugin> ldp in Global.Plugins)
                    {
                        IDatabasePlugin dp = ldp.Value;

                        if (Global.CheckType(dp, database))
                        {
                            result = dp.Table_Get(database.Configuration, tablename, filterExpression);
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
                    foreach (Lazy<IDatabasePlugin> ldp in Global.Plugins)
                    {
                        IDatabasePlugin dp = ldp.Value;

                        if (Global.CheckType(dp, database))
                        {
                            result = dp.Table_Get(database.Configuration, tablename, filterExpression, columns);
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
                    foreach (Lazy<IDatabasePlugin> ldp in Global.Plugins)
                    {
                        IDatabasePlugin dp = ldp.Value;

                        if (Global.CheckType(dp, database))
                        {
                            result = dp.Table_List(database.Configuration);
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
                    foreach (Lazy<IDatabasePlugin> ldp in Global.Plugins)
                    {
                        IDatabasePlugin dp = ldp.Value;

                        if (Global.CheckType(dp, database))
                        {
                            result = dp.Table_List(database.Configuration, filterExpression);
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
                    foreach (Lazy<IDatabasePlugin> ldp in Global.Plugins)
                    {
                        IDatabasePlugin dp = ldp.Value;

                        if (Global.CheckType(dp, database))
                        {
                            result = dp.Table_GetRowCount(database.Configuration, tablename);
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
                    foreach (Lazy<IDatabasePlugin> ldp in Global.Plugins)
                    {
                        IDatabasePlugin dp = ldp.Value;

                        if (Global.CheckType(dp, database))
                        {
                            result = dp.Table_GetSize(database.Configuration, tablename);
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
                    foreach (Lazy<IDatabasePlugin> ldp in Global.Plugins)
                    {
                        IDatabasePlugin dp = ldp.Value;

                        if (Global.CheckType(dp, database))
                        {
                            result = dp.Column_Get(database.Configuration, tablename);
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
                    foreach (Lazy<IDatabasePlugin> ldp in Global.Plugins)
                    {
                        IDatabasePlugin dp = ldp.Value;

                        if (Global.CheckType(dp, db))
                        {
                            AddWorkerInfo info = new AddWorkerInfo();
                            info.configuration = db.Configuration;
                            info.tablename = tablename;
                            info.columnDefinition = columnDefinition;
                            info.plugin = dp;

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
            catch (Exception ex) { Logger.Log("Column.AddWorker() : Exception : " + ex.Message); }
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
                    foreach (Lazy<IDatabasePlugin> ldp in Global.Plugins)
                    {
                        IDatabasePlugin dp = ldp.Value;

                        if (Global.CheckType(dp, db))
                        {
                            InsertWorkerInfo1 info = new InsertWorkerInfo1();
                            info.configuration = db.Configuration;
                            info.tablename = tablename;
                            info.columns = columns;
                            info.values = values;
                            info.primaryKey = primaryKey;
                            info.update = update;
                            info.plugin = dp;

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
            catch (Exception ex) { Logger.Log("Column.InsertWorker1() : Exception : " + ex.Message); }
        }


        public static void Insert(Database_Settings settings, string tablename, object[] columns, List<List<object>> values, string[] primaryKey, bool update)
        {
            if (Global.Plugins != null && values.Count > 0)
            {
                foreach (Database_Configuration db in settings.Databases)
                {
                    foreach (Lazy<IDatabasePlugin> ldp in Global.Plugins)
                    {
                        IDatabasePlugin dp = ldp.Value;

                        if (Global.CheckType(dp, db))
                        {
                            InsertWorkerInfo2 info = new InsertWorkerInfo2();
                            info.configuration = db.Configuration;
                            info.tablename = tablename;
                            info.columns = columns;
                            info.values = values;
                            info.primaryKey = primaryKey;
                            info.update = update;
                            info.plugin = dp;

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
            catch (Exception ex) { Logger.Log("Column.InsertWorker2() : Exception : " + ex.Message); }
        }


        public static void Insert(Database_Settings settings, string tablename, List<object[]> columnsList, List<object[]> valuesList, string[] primaryKey, bool update)
        {
            if (Global.Plugins != null && valuesList.Count > 0)
            {
                foreach (Database_Configuration db in settings.Databases)
                {
                    foreach (Lazy<IDatabasePlugin> ldp in Global.Plugins)
                    {
                        IDatabasePlugin dp = ldp.Value;

                        if (Global.CheckType(dp, db))
                        {
                            InsertWorkerInfo3 info = new InsertWorkerInfo3();
                            info.configuration = db.Configuration;
                            info.tablename = tablename;
                            info.columnsList = columnsList;
                            info.valuesList = valuesList;
                            info.primaryKey = primaryKey;
                            info.update = update;
                            info.plugin = dp;

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
            catch (Exception ex) { Logger.Log("Column.InsertWorker3() : Exception : " + ex.Message); }
        }


        public static void Insert(Database_Settings settings, string query)
        {
            if (Global.Plugins != null)
            {
                foreach (Database_Configuration db in settings.Databases)
                {
                    foreach (Lazy<IDatabasePlugin> ldp in Global.Plugins)
                    {
                        IDatabasePlugin dp = ldp.Value;

                        if (Global.CheckType(dp, db))
                        {
                            InsertWorkerInfo4 info = new InsertWorkerInfo4();
                            info.configuration = db.Configuration;
                            info.query = query;
                            info.plugin = dp;

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
            catch (Exception ex) { Logger.Log("Column.InsertWorker4() : Exception : " + ex.Message); }
        }


        public static DataRow Get(Database_Settings settings, string tablename, string tableKey, string rowKey)
        {
            DataRow result = null;
            bool found = false;

            foreach (var database in settings.Databases)
            {
                if (Global.Plugins != null)
                {
                    foreach (Lazy<IDatabasePlugin> ldp in Global.Plugins)
                    {
                        IDatabasePlugin dp = ldp.Value;

                        if (Global.CheckType(dp, database))
                        {
                            result = dp.Row_Get(database.Configuration, tablename, tableKey, rowKey);
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
                    foreach (Lazy<IDatabasePlugin> ldp in Global.Plugins)
                    {
                        IDatabasePlugin dp = ldp.Value;

                        if (Global.CheckType(dp, database))
                        {
                            result = dp.Row_Get(database.Configuration, tablename, query);
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
                    foreach (Lazy<IDatabasePlugin> ldp in Global.Plugins)
                    {
                        IDatabasePlugin dp = ldp.Value;

                        if (Global.CheckType(dp, database))
                        {
                            result = dp.Row_Exists(database.Configuration, tablename, filterString);
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

                        foreach (Lazy<IDatabasePlugin> ldp in Global.Plugins)
                        {
                            IDatabasePlugin dp = ldp.Value;

                            if (Global.CheckType(dp, db))
                            {
                                if (x == 0) result = dp.CustomCommand(db.Configuration, commandText);
                                else
                                {
                                    CustomCommandWorkerInfo info = new CustomCommandWorkerInfo();
                                    info.configuration = db.Configuration;
                                    info.commandText = commandText;
                                    info.plugin = dp;

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
            catch (Exception ex) { Logger.Log("Column.CustomCommandWorker() : Exception : " + ex.Message); }
        }


        public static object GetValue(Database_Settings settings, string tablename, string column, string filterExpression)
        {
            object result = null;
            bool found = false;

            foreach (var database in settings.Databases)
            {
                if (Global.Plugins != null)
                {
                    foreach (Lazy<IDatabasePlugin> ldp in Global.Plugins)
                    {
                        IDatabasePlugin dp = ldp.Value;

                        if (Global.CheckType(dp, database))
                        {
                            result = dp.GetValue(database.Configuration, tablename, column, filterExpression);
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
                    foreach (Lazy<IDatabasePlugin> ldp in Global.Plugins)
                    {
                        IDatabasePlugin dp = ldp.Value;

                        if (Global.CheckType(dp, database))
                        {
                            result = dp.Table_Get(database.Configuration, username);
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

    public class DatabasePluginReader
    {

        public DatabasePluginReader()
        {
            GetPlugins();
        }

        public IEnumerable<Lazy<IDatabasePlugin>> databasePlugins { get; set; }

        public List<Lazy<IDatabasePlugin>> plugins { get; set; }

        DatabasePlugs DBPLUGS;

        class DatabasePlugs
        {
            [ImportMany(typeof(IDatabasePlugin))]
            public IEnumerable<Lazy<IDatabasePlugin>> PlugIns { get; set; }
        }

        public void GetPlugins()
        {
            string plugin_rootpath = FileLocations.Plugins;

            if (!Directory.Exists(plugin_rootpath)) Directory.CreateDirectory(plugin_rootpath);

            plugins = new List<Lazy<IDatabasePlugin>>();

            string pluginsPath;

            // Load from System Directory first (easier for user to navigate to 'C:\TrakHound\Plugins')
            pluginsPath = TH_Global.FileLocations.Plugins;
            if (Directory.Exists(pluginsPath)) FindPlugins(pluginsPath);

            // Load from App root Directory (doesn't overwrite plugins found in System Directory)
            //pluginsPath = AppDomain.CurrentDomain.BaseDirectory + @"Plugins\";
            pluginsPath = AppDomain.CurrentDomain.BaseDirectory;
            if (Directory.Exists(pluginsPath)) FindPlugins(pluginsPath);


            Console.WriteLine("Database Plugins --------------------------");
            Console.WriteLine(plugins.Count.ToString() + " Plugins Found");
            Console.WriteLine("------------------------------");
            foreach (Lazy<IDatabasePlugin> lplugin in plugins)
            {
                IDatabasePlugin plugin = lplugin.Value;

                string name = plugin.Name;
                string version = null;

                // Version Info
                Assembly assembly = Assembly.GetAssembly(plugin.GetType());
                if (assembly != null)
                {
                    Version v = assembly.GetName().Version;
                    version = "v" + v.Major.ToString() + "." + v.Minor.ToString() + "." + v.Build.ToString() + "." + v.Revision.ToString();
                }

                Console.WriteLine(plugin.Name + " : " + version);
            }
            Console.WriteLine("----------------------------------------");


            Global.Plugins = plugins;

        }

        void FindPlugins(string Path)
        {
            Console.WriteLine("Searching for Database Plugins in : " + Path);

            if (Directory.Exists(Path))
            {
                try
                {
                    DBPLUGS = new DatabasePlugs();

                    var PageCatalog = new DirectoryCatalog(Path);
                    var PageContainer = new CompositionContainer(PageCatalog);
                    PageContainer.SatisfyImportsOnce(DBPLUGS);

                    databasePlugins = DBPLUGS.PlugIns;

                    foreach (Lazy<IDatabasePlugin> DBP in databasePlugins.ToList())
                    {
                        if (plugins.ToList().Find(x => x.Value.Name.ToLower() == DBP.Value.Name.ToLower()) == null)
                        {
                            plugins.Add(DBP);
                        }
                    }
                }
                catch (System.Reflection.ReflectionTypeLoadException rtex)
                {
                    Console.WriteLine("DatabasePluginReader.GetPlugins() : ReflectionTypeLoadException : " + rtex.Message);

                    foreach (var lex in rtex.LoaderExceptions)
                    {
                        Console.WriteLine("DatabasePluginReader.GetPlugins() : LoaderException : " + lex.Message);
                    }                  
                }
                catch (Exception ex)
                {
                    Console.WriteLine("DatabasePluginReader.GetPlugins() : Exception : " + ex.Message);
                }

                // Search Subdirectories
                foreach (string directory in Directory.GetDirectories(Path))
                {
                    FindPlugins(directory);
                }
            }
        }

    }
}
