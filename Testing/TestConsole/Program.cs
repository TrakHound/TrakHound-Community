using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

//using TH_MicrosoftSQL;
using TH_MTConnect.Components;

using System.Data.SQLite;
//using TH_SQLite;

using TH_Configuration;
using TH_DeviceManager;
using TH_Global.Functions;


namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //string path = @"C:\TrakHound\Devices\APMZSRSQDJHVHTNTILHQ.xml";

            string dbPath = @"C:\TrakHound\Databases\Test1\Test2\test.db";

            //var config = new SQLite_Configuration();
            //config.DatabasePath = dbPath;

            //var plugin = new Plugin();

            //plugin.Database_Create(config);

            //var columns = new TH_Database.ColumnDefinition[]
            //{
            //    new TH_Database.ColumnDefinition() { ColumnName = "First", DataType = TH_Database.DataType.LargeText },
            //    new TH_Database.ColumnDefinition() { ColumnName = "Second", DataType = TH_Database.DataType.LargeText },
            //    new TH_Database.ColumnDefinition() { ColumnName = "Third", DataType = TH_Database.DataType.LargeText },
            //    new TH_Database.ColumnDefinition() { ColumnName = "Fourth", DataType = TH_Database.DataType.LargeText },
            //};

            //plugin.Table_Replace(config, "snapshots", columns, null);

            //var dt = plugin.Table_Get(config, "snapshots");

            //TH_Global.Functions.DataTable_Functions.WriteRowstoConsole("snapshots", dt);



            //var sql = "SELECT * FROM snapshots";
            //    var command = new SQLiteCommand(sql, conn);
            // var reader = command.ExecuteReader();
            // while (reader.Read())
            // {
            //     PrintReaderRow(reader);
            // }



            ////dbPath = Environment.ExpandEnvironmentVariables(dbPath);

            string dir = Environment.ExpandEnvironmentVariables(dbPath);
            dir = Path.GetDirectoryName(dir);

            Console.WriteLine(dir);

            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            else Console.WriteLine(dbPath + " CREATED");

            if (!File.Exists(dbPath)) SQLiteConnection.CreateFile(dbPath);

            string strconn = "Data Source=" + dbPath + "; Version=3; Pooling=True; Max Pool Size=150;";

            ////for (var x = 0; x < 5; x++)
            ////{
            //using (var conn = new SQLiteConnection(strconn))
            //{
            //    try
            //    {
            //        Console.WriteLine("Opened");
            //        conn.Open();

            //        var sql = "DELETE IF EXISTS FROM snapshots";
            //        using (var command = new SQLiteCommand(sql, conn))
            //        {
            //            command.ExecuteNonQuery();
            //        }

            //        //sql = "CREATE TABLE IF NOT EXISTS snapshots (`TIMESTAMP` datetime,`NAME` varchar(1000),`VALUE` varchar(1000),`PREVIOUS_TIMESTAMP` datetime,`PREVIOUS_VALUE` varchar(1000), PRIMARY KEY('NAME'))";
            //        //using (var command = new SQLiteCommand(sql, conn))
            //        //{
            //        //    command.ExecuteNonQuery();
            //        //}

            //    }
            //    catch (SQLiteException sqex)
            //    {
            //        Console.WriteLine(sqex.Message);
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine(ex.Message);
            //    }
            //    finally
            //    {
            //        conn.Close();
            //        Console.WriteLine("Closed");
            //    }
            //}




            var conn = new SQLiteConnection(strconn);
            conn.Open();

            string sql;
            SQLiteCommand command;
            SQLiteDataReader reader;

            sql = "DROP TABLE IF EXISTS test_table; ";
            //command = new SQLiteCommand(sql, conn);
            //command.ExecuteNonQuery();

            //sql += "CREATE TABLE IF NOT EXISTS snapshots (`TIMESTAMP` datetime,`NAME` varchar(1000),`VALUE` varchar(1000),`PREVIOUS_TIMESTAMP` datetime,`PREVIOUS_VALUE` varchar(1000), PRIMARY KEY('NAME'))";
            //command = new SQLiteCommand(sql, conn);
            //command.ExecuteNonQuery();


            //var addColumns = new List<string>()
            //{
            //    "oee",
            //    "availability",
            //    "performance",
            //    "quality"
            //};

            sql += "CREATE TABLE IF NOT EXISTS test_table (name varchar(90), value int, PRIMARY KEY(name)); ";
            //sql += "CREATE TABLE IF NOT EXISTS test_table2 (name varchar(90), value int, PRIMARY KEY(name)); ";
            //sql += "CREATE TABLE IF NOT EXISTS test_table3 (name varchar(90), value int, PRIMARY KEY(name)); ";
            //sql += "CREATE TABLE IF NOT EXISTS test_table4 (name varchar(90), value int, PRIMARY KEY(name)); ";
            //sql += "CREATE TABLE IF NOT EXISTS test_table5 (timestamp datetime, name varchar(90), value double, PRIMARY KEY(name)); ";
            command = new SQLiteCommand(sql, conn);
            command.ExecuteNonQuery();


            //sql = "PRAGMA table_info(test_table)";
            //command = new SQLiteCommand(sql, conn);
            //reader = command.ExecuteReader();

            //// Add any missing columns
            //foreach (var column in addColumns)
            //{
            //    sql = "SELECT * FROM ;
            //    command = new SQLiteCommand(sql, conn);
            //    reader = command.ExecuteReader();
            //    while (reader.Read())
            //    {
            //        PrintReaderRow(reader);
            //    }
            //}





            //sql = "INSERT OR REPLACE INTO test_table (name, value) VALUES ('patrick', 1)";
            //command = new SQLiteCommand(sql, conn);
            //command.ExecuteNonQuery();

            //sql = "INSERT OR REPLACE INTO test_table (name, value) VALUES ('bob', 2)";
            //command = new SQLiteCommand(sql, conn);
            //command.ExecuteNonQuery();

            //sql = "INSERT OR REPLACE INTO test_table (name, value) VALUES ('dave', 3)";
            //command = new SQLiteCommand(sql, conn);
            //command.ExecuteNonQuery();

            //sql = "INSERT OR REPLACE INTO test_table5 (name, timestamp, value) VALUES ('john', '2016-02-27 23:15:00', 4)";
            //command = new SQLiteCommand(sql, conn);
            //command.ExecuteNonQuery();


            sql = "INSERT OR REPLACE INTO test_table (name, value) VALUES ('patrick', 1); ";
            sql += "INSERT OR REPLACE INTO test_table (name, value) VALUES ('bob', 2); ";
            sql += "INSERT OR REPLACE INTO test_table (name, value) VALUES ('dave', 3); ";
            sql += "INSERT OR REPLACE INTO test_table (name, value) VALUES ('john', 4); ";
            command = new SQLiteCommand(sql, conn);
            command.ExecuteNonQuery();
            //var config = Configuration.Read(path);

            //config.ClientEnabled = false;

            ////var nodes = new Network_Functions.PingNodes();

            //sql = "SELECT * FROM snapshots";
            //command = new SQLiteCommand(sql, conn);
            //reader = command.ExecuteReader();
            //while (reader.Read())
            //{
            //    PrintReaderRow(reader);
            //}

            //sql = "SELECT name FROM sqlite_master WHERE type='table' AND name LIKE '%2'";
            //command = new SQLiteCommand(sql, conn);
            //reader = command.ExecuteReader();
            //while (reader.Read())
            //{
            //    PrintReaderRow(reader);
            //}

            //sql = "SELECT name FROM sqlite_master WHERE type='table'";
            //command = new SQLiteCommand(sql, conn);
            //reader = command.ExecuteReader();
            //while (reader.Read())
            //{
            //    PrintReaderRow(reader);
            //}

            //sql = "SELECT Count(*) FROM test_table";
            //command = new SQLiteCommand(sql, conn);
            //reader = command.ExecuteReader();
            //while (reader.Read())
            //{
            //    PrintReaderRow(reader);
            //}

            //sql = "PRAGMA table_info(test_table)";
            //command = new SQLiteCommand(sql, conn);
            //reader = command.ExecuteReader();

            //var columns = new List<string>();

            //while (reader.Read())
            //{
            //    PrintReaderRow(reader);
            //    columns.Add(reader["name"].ToString());
            //}



            //foreach (var column in addColumns)
            //{
            //    if (!columns.Exists(x => x == column))
            //    {
            //        sql = "ALTER TABLE test_table ADD COLUMN " + column;
            //        command = new SQLiteCommand(sql, conn);
            //        command.ExecuteNonQuery();
            //    }
            //}

            conn.Close();

            var dm = new DeviceManager();
            dm.LoadDevices();


            Console.ReadLine();
        }

        static void PrintReaderRow(SQLiteDataReader reader)
        {
            string print = "";

            for (var x = 0; x <= reader.FieldCount - 1; x++)
            {
                print += reader.GetName(x) + ": " + reader[x] + "; ";
            }

            Console.WriteLine(print);
        }

    }
}
