using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;

using TH_Configuration;
using TH_Global;

namespace TH_Database
{
    public static class Database
    {

        #region "Worker Info Classes"

        class CreateWorkerInfo
        {
            public object configuration { get; set; }

            public Database_Plugin plugin { get; set; }
        }

        class DropWorkerInfo
        {
            public object configuration { get; set; }

            public Database_Plugin plugin { get; set; }
        }

        #endregion

        public static void Create(Database_Settings settings)
        {
            foreach (object database in settings.Databases)
            {
                if (database.GetType() == typeof(Lazy<Database_Plugin>))
                {
                    Lazy<Database_Plugin> lazyPlugin = (Lazy<Database_Plugin>)database;

                    if (lazyPlugin.IsValueCreated)
                    {
                        Database_Plugin plugin = lazyPlugin.Value;

                        CreateWorkerInfo info = new CreateWorkerInfo();
                        info.configuration = plugin.Configuration;

                        info.plugin = plugin;

                        ThreadPool.QueueUserWorkItem(new WaitCallback(Create_Worker), info);
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
            foreach (object database in settings.Databases)
            {
                if (database.GetType() == typeof(Lazy<Database_Plugin>))
                {
                    Lazy<Database_Plugin> lazyPlugin = (Lazy<Database_Plugin>)database;

                    if (lazyPlugin.IsValueCreated)
                    {
                        Database_Plugin plugin = lazyPlugin.Value;

                        DropWorkerInfo info = new DropWorkerInfo();
                        info.configuration = plugin.Configuration;
                        info.plugin = plugin;

                        ThreadPool.QueueUserWorkItem(new WaitCallback(Drop_Worker), info);
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
            public object[] columnDefinitions { get; set; }
            public string primaryKey { get; set; }

            public Database_Plugin plugin { get; set; }
        }

        class DropWorkerInfo1
        {
            public object configuration { get; set; }
            public string tablename { get; set; }

            public Database_Plugin plugin { get; set; }
        }

        class DropWorkerInfo2
        {
            public object configuration { get; set; }
            public string[] tablenames { get; set; }

            public Database_Plugin plugin { get; set; }
        }

        class TruncateWorkerInfo
        {
            public object configuration { get; set; }
            public string tablename { get; set; }

            public Database_Plugin plugin { get; set; }
        }

        #endregion

        public static void Create(Database_Settings settings, string tablename, object[] columnDefinitions, string primaryKey)
        {
            foreach (object database in settings.Databases)
            {
                if (database.GetType() == typeof(Lazy<Database_Plugin>))
                {
                    Lazy<Database_Plugin> lazyPlugin = (Lazy<Database_Plugin>)database;

                    if (lazyPlugin.IsValueCreated)
                    {
                        Database_Plugin plugin = lazyPlugin.Value;

                        CreateWorkerInfo info = new CreateWorkerInfo();
                        info.configuration = plugin.Configuration;
                        info.tablename = tablename;
                        info.columnDefinitions = columnDefinitions;
                        info.primaryKey = primaryKey;
                        info.plugin = plugin;

                        ThreadPool.QueueUserWorkItem(new WaitCallback(Create_Worker), info);
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
            foreach (object database in settings.Databases)
            {
                if (database.GetType() == typeof(Lazy<Database_Plugin>))
                {
                    Lazy<Database_Plugin> lazyPlugin = (Lazy<Database_Plugin>)database;

                    if (lazyPlugin.IsValueCreated)
                    {
                        Database_Plugin plugin = lazyPlugin.Value;

                        DropWorkerInfo1 info = new DropWorkerInfo1();
                        info.configuration = plugin.Configuration;
                        info.tablename = tablename;
                        info.plugin = plugin;

                        ThreadPool.QueueUserWorkItem(new WaitCallback(Drop_Worker1), info);
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
            foreach (object database in settings.Databases)
            {
                if (database.GetType() == typeof(Lazy<Database_Plugin>))
                {
                    Lazy<Database_Plugin> lazyPlugin = (Lazy<Database_Plugin>)database;

                    if (lazyPlugin.IsValueCreated)
                    {
                        Database_Plugin plugin = lazyPlugin.Value;

                        DropWorkerInfo2 info = new DropWorkerInfo2();
                        info.configuration = plugin.Configuration;
                        info.tablenames = tablenames;
                        info.plugin = plugin;

                        ThreadPool.QueueUserWorkItem(new WaitCallback(Drop_Worker2), info);
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
            foreach (object database in settings.Databases)
            {
                if (database.GetType() == typeof(Lazy<Database_Plugin>))
                {
                    Lazy<Database_Plugin> lazyPlugin = (Lazy<Database_Plugin>)database;

                    if (lazyPlugin.IsValueCreated)
                    {
                        Database_Plugin plugin = lazyPlugin.Value;

                        TruncateWorkerInfo info = new TruncateWorkerInfo();
                        info.configuration = plugin.Configuration;
                        info.tablename = tablename;
                        info.plugin = plugin;

                        ThreadPool.QueueUserWorkItem(new WaitCallback(Truncate_Worker), info);
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

            if (settings.Databases.Count > 0)
            {
                if (settings.Databases[0].GetType() == typeof(Database_Plugin))
                {
                    Database_Plugin plugin = (Database_Plugin)settings.Databases[0];

                    result = plugin.Table_Get(plugin.Configuration, tablename);
                }
            }

            return result;
        }

        public static DataTable Get(Database_Settings settings, string tablename, string filterExpression)
        {
            DataTable result = null;

            if (settings.Databases.Count > 0)
            {
                if (settings.Databases[0].GetType() == typeof(Database_Plugin))
                {
                    Database_Plugin plugin = (Database_Plugin)settings.Databases[0];

                    result = plugin.Table_Get(plugin.Configuration, tablename, filterExpression);
                }
            }

            return result;
        }

        public static DataTable Get(Database_Settings settings, string tablename, string filterExpression, string columns)
        {
            DataTable result = null;

            if (settings.Databases.Count > 0)
            {
                if (settings.Databases[0].GetType() == typeof(Database_Plugin))
                {
                    Database_Plugin plugin = (Database_Plugin)settings.Databases[0];

                    result = plugin.Table_Get(plugin.Configuration, tablename, filterExpression, columns);
                }
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
            public string columnDefinition { get; set; }

            public Database_Plugin plugin { get; set; }
        }

        #endregion

        public static List<string> Get(Database_Settings settings, string tablename)
        {
            List<string> result = new List<string>();

            if (settings.Databases.Count > 0)
            {
                if (settings.Databases[0].GetType() == typeof(Database_Plugin))
                {
                    Database_Plugin plugin = (Database_Plugin)settings.Databases[0];

                    result = plugin.Column_Get(plugin.Configuration, tablename);
                }
            }

            return result;
        }

        public static void Add(Database_Settings settings, string tablename, string columnDefinition)
        {
            foreach (object database in settings.Databases)
            {
                if (database.GetType() == typeof(Lazy<Database_Plugin>))
                {
                    Lazy<Database_Plugin> lazyPlugin = (Lazy<Database_Plugin>)database;

                    if (lazyPlugin.IsValueCreated)
                    {
                        Database_Plugin plugin = lazyPlugin.Value;

                        AddWorkerInfo info = new AddWorkerInfo();
                        info.configuration = plugin.Configuration;
                        info.tablename = tablename;
                        info.columnDefinition = columnDefinition;
                        info.plugin = plugin;

                        ThreadPool.QueueUserWorkItem(new WaitCallback(AddWorker), info);
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
            public bool update { get; set; }

            public Database_Plugin plugin { get; set; }
        }

        class InsertWorkerInfo2
        {
            public object configuration { get; set; }
            public string tablename { get; set; }
            public object[] columns { get; set; }
            public List<List<object>> values { get; set; }
            public bool update { get; set; }

            public Database_Plugin plugin { get; set; }
        }

        class InsertWorkerInfo3
        {
            public object configuration { get; set; }
            public string tablename { get; set; }
            public List<object[]> columnsList { get; set; }
            public List<object[]> valuesList { get; set; }
            public bool update { get; set; }

            public Database_Plugin plugin { get; set; }
        }

        class InsertWorkerInfo4
        {
            public object configuration { get; set; }
            public string query { get; set; }

            public Database_Plugin plugin { get; set; }
        }

        #endregion

        public static void Insert(Database_Settings settings, string tablename, object[] columns, object[] values, bool update)
        {
            foreach (object database in settings.Databases)
            {
                if (database.GetType() == typeof(Lazy<Database_Plugin>))
                {
                    Lazy<Database_Plugin> lazyPlugin = (Lazy<Database_Plugin>)database;

                    if (lazyPlugin.IsValueCreated)
                    {
                        Database_Plugin plugin = lazyPlugin.Value;

                        InsertWorkerInfo1 info = new InsertWorkerInfo1();
                        info.configuration = plugin.Configuration;
                        info.tablename = tablename;
                        info.columns = columns;
                        info.values = values;
                        info.update = update;
                        info.plugin = plugin;

                        ThreadPool.QueueUserWorkItem(new WaitCallback(InsertWorker1), info);
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

                    info.plugin.Row_Insert(info.configuration, info.tablename, info.columns, info.values, info.update);
                }
            }
            catch (Exception ex) { Logger.Log("Column.InsertWorker1() : Exception : " + ex.Message); }
        }


        public static void Insert(Database_Settings settings, string tablename, object[] columns, List<List<object>> values, bool update)
        {
            foreach (object database in settings.Databases)
            {
                if (database.GetType() == typeof(Lazy<Database_Plugin>))
                {
                    Lazy<Database_Plugin> lazyPlugin = (Lazy<Database_Plugin>)database;

                    if (lazyPlugin.IsValueCreated)
                    {
                        Database_Plugin plugin = lazyPlugin.Value;

                        InsertWorkerInfo2 info = new InsertWorkerInfo2();
                        info.configuration = plugin.Configuration;
                        info.tablename = tablename;
                        info.columns = columns;
                        info.values = values;
                        info.update = update;
                        info.plugin = plugin;

                        ThreadPool.QueueUserWorkItem(new WaitCallback(InsertWorker2), info);
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

                    info.plugin.Row_Insert(info.configuration, info.tablename, info.columns, info.values, info.update);
                }
            }
            catch (Exception ex) { Logger.Log("Column.InsertWorker2() : Exception : " + ex.Message); }
        }


        public static void Insert(Database_Settings settings, string tablename, List<object[]> columnsList, List<object[]> valuesList, bool update)
        {
            foreach (object database in settings.Databases)
            {
                if (database.GetType() == typeof(Lazy<Database_Plugin>))
                {
                    Lazy<Database_Plugin> lazyPlugin = (Lazy<Database_Plugin>)database;

                    if (lazyPlugin.IsValueCreated)
                    {
                        Database_Plugin plugin = lazyPlugin.Value;

                        InsertWorkerInfo3 info = new InsertWorkerInfo3();
                        info.configuration = plugin.Configuration;
                        info.tablename = tablename;
                        info.columnsList = columnsList;
                        info.valuesList = valuesList;
                        info.update = update;
                        info.plugin = plugin;

                        ThreadPool.QueueUserWorkItem(new WaitCallback(InsertWorker3), info);
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

                    info.plugin.Row_Insert(info.configuration, info.tablename, info.columnsList, info.valuesList, info.update);
                }
            }
            catch (Exception ex) { Logger.Log("Column.InsertWorker3() : Exception : " + ex.Message); }
        }


        public static void Insert(Database_Settings settings, string query)
        {
            foreach (object database in settings.Databases)
            {
                if (database.GetType() == typeof(Lazy<Database_Plugin>))
                {
                    Lazy<Database_Plugin> lazyPlugin = (Lazy<Database_Plugin>)database;

                    if (lazyPlugin.IsValueCreated)
                    {
                        Database_Plugin plugin = lazyPlugin.Value;

                        InsertWorkerInfo4 info = new InsertWorkerInfo4();
                        info.configuration = plugin.Configuration;
                        info.query = query;
                        info.plugin = plugin;

                        ThreadPool.QueueUserWorkItem(new WaitCallback(InsertWorker4), info);
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

            if (settings.Databases.Count > 0)
            {
                if (settings.Databases[0].GetType() == typeof(Database_Plugin))
                {
                    Database_Plugin plugin = (Database_Plugin)settings.Databases[0];

                    result = plugin.Row_Get(plugin.Configuration, tablename, tableKey, rowKey);
                }
            }

            return result;
        }

        public static DataRow Get(Database_Settings settings, string tablename, string query)
        {
            DataRow result = null;

            if (settings.Databases.Count > 0)
            {
                if (settings.Databases[0].GetType() == typeof(Database_Plugin))
                {
                    Database_Plugin plugin = (Database_Plugin)settings.Databases[0];

                    result = plugin.Row_Get(plugin.Configuration, tablename, query);
                }
            }

            return result;
        }


        // only returns whether row exists in first (primary) database
        public static bool Exists(Database_Settings settings, string tablename, string filterString)
        {
            bool result = false;

            if (settings.Databases.Count > 0)
            {
                if (settings.Databases[0].GetType() == typeof(Database_Plugin))
                {
                    Database_Plugin plugin = (Database_Plugin)settings.Databases[0];

                    result = plugin.Row_Exists(plugin.Configuration, tablename, filterString);
                }
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

            public Database_Plugin plugin { get; set; }
        }

        #endregion


        public static object[] CustomCommand(Database_Settings settings, string commandText)
        {
            object[] result = null;

            if (settings.Databases.Count > 0)
            {
                for (int x = 0; x <= settings.Databases.Count - 1; x++)
                {
                    object database = settings.Databases[x];

                    if (database.GetType() == typeof(Lazy<Database_Plugin>))
                    {
                        Lazy<Database_Plugin> lazyPlugin = (Lazy<Database_Plugin>)database;

                        if (lazyPlugin.IsValueCreated)
                        {
                            Database_Plugin plugin = lazyPlugin.Value;

                            if (x == 0) result = plugin.CustomCommand(plugin.Configuration, commandText);
                            else
                            {
                                CustomCommandWorkerInfo info = new CustomCommandWorkerInfo();
                                info.configuration = plugin.Configuration;
                                info.commandText = commandText;
                                info.plugin = plugin;

                                ThreadPool.QueueUserWorkItem(new WaitCallback(CustomCommandWorker), info);
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

            if (settings.Databases.Count > 0)
            {
                if (settings.Databases[0].GetType() == typeof(Database_Plugin))
                {
                    Database_Plugin plugin = (Database_Plugin)settings.Databases[0];

                    result = plugin.GetValue(plugin.Configuration, tablename, column, filterExpression);
                }
            }

            return result;
        }

        // May need some more work (return DataSet with table for each database?)
        public static DataTable GetGrants(Database_Settings settings, string username)
        {
            DataTable result = null;

            if (settings.Databases.Count > 0)
            {
                if (settings.Databases[0].GetType() == typeof(Database_Plugin))
                {
                    Database_Plugin plugin = (Database_Plugin)settings.Databases[0];

                    result = plugin.Table_Get(plugin.Configuration, username);
                }
            }

            return result;
        }

    }

    public class PluginReader
    {

        public IEnumerable<Lazy<Database_Plugin>> databasePlugins { get; set; }

        public List<Lazy<Database_Plugin>> plugins { get; set; }

        DatabasePlugs DBPLUGS;

        class DatabasePlugs
        {
            [ImportMany(typeof(Database_Plugin))]
            public IEnumerable<Lazy<Database_Plugin>> plugins { get; set; }
        }

        public List<Lazy<Database_Plugin>> GetPlugins()
        {
            string plugin_rootpath = FileLocations.Plugins;

            if (!Directory.Exists(plugin_rootpath)) Directory.CreateDirectory(plugin_rootpath);

            plugins = new List<Lazy<Database_Plugin>>();

            string pluginsPath;

            // Load from System Directory first (easier for user to navigate to 'C:\TrakHound\Plugins')
            pluginsPath = TH_Global.FileLocations.Plugins;
            if (Directory.Exists(pluginsPath)) GetPlugins(pluginsPath);

            // Load from App root Directory (doesn't overwrite plugins found in System Directory)
            pluginsPath = AppDomain.CurrentDomain.BaseDirectory + @"Plugins\";
            if (Directory.Exists(pluginsPath)) GetPlugins(pluginsPath);

            return plugins;
        }

        void GetPlugins(string Path)
        {
            Logger.Log("Searching for Database Plugins in '" + Path + "'");

            if (Directory.Exists(Path))
            {
                try
                {
                    DBPLUGS = new DatabasePlugs();

                    var PageCatalog = new DirectoryCatalog(Path);
                    var PageContainer = new CompositionContainer(PageCatalog);
                    PageContainer.SatisfyImportsOnce(DBPLUGS);

                    databasePlugins = DBPLUGS.plugins;

                    foreach (Lazy<Database_Plugin> DBP in databasePlugins)
                    {
                        if (plugins.ToList().Find(x => x.Value.Name.ToLower() == DBP.Value.Name.ToLower()) == null)
                        {
                            if (DBP.IsValueCreated) Logger.Log(DBP.Value.Name + " : PlugIn Found");
                            plugins.Add(DBP);
                        }
                        else
                        {
                            if (DBP.IsValueCreated) Logger.Log(DBP.Value.Name + " : PlugIn Already Found");
                        }
                    }
                }
                catch (Exception ex) { Logger.Log("GetPlugins() : Exception : " + ex.Message); }

                // Search Subdirectories
                foreach (string directory in Directory.GetDirectories(Path))
                {
                    GetPlugins(directory);
                }
            }
            else Logger.Log("Database PlugIns Directory Doesn't Exist (" + Path + ")");
        }

    }
}
