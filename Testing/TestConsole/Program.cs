using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TH_MicrosoftSQL;
using TH_MTConnect.Components;

using System.Data.SQLite;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {

            string dbPath = @"C:\TrakHound\Databases\test.db";

            if (!System.IO.File.Exists(dbPath)) SQLiteConnection.CreateFile(dbPath);

            string strconn = "Data Source=" + dbPath + "; Version=3;";

            var conn = new SQLiteConnection(strconn);
            conn.Open();

            string sql;
            SQLiteCommand command;
            SQLiteDataReader reader;

            //sql = "DROP TABLE IF EXISTS test_table";
            //command = new SQLiteCommand(sql, conn);
            //command.ExecuteNonQuery();

            var addColumns = new List<string>()
            {
                "oee",
                "availability",
                "performance",
                "quality"
            };

            sql = "CREATE TABLE IF NOT EXISTS test_table1 (name varchar(90), value int, PRIMARY KEY(name)); ";
            sql += "CREATE TABLE IF NOT EXISTS test_table2 (name varchar(90), value int, PRIMARY KEY(name)); ";
            sql += "CREATE TABLE IF NOT EXISTS test_table3 (name varchar(90), value int, PRIMARY KEY(name)); ";
            sql += "CREATE TABLE IF NOT EXISTS test_table4 (name varchar(90), value int, PRIMARY KEY(name)); ";
            sql += "CREATE TABLE IF NOT EXISTS test_table5 (timestamp datetime, name varchar(90), value double, PRIMARY KEY(name)); ";
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

            sql = "INSERT OR REPLACE INTO test_table5 (name, timestamp, value) VALUES ('john', '2016-02-27 23:15:00', 4)";
            command = new SQLiteCommand(sql, conn);
            command.ExecuteNonQuery();


            //sql = "INSERT OR REPLACE INTO test_table (name, value) VALUES ('patrick', 1); ";
            //sql += "INSERT OR REPLACE INTO test_table (name, value) VALUES ('bob', 2); ";
            //sql += "INSERT OR REPLACE INTO test_table (name, value) VALUES ('dave', 3); ";
            //sql += "INSERT OR REPLACE INTO test_table (name, value) VALUES ('john', 4); ";
            //command = new SQLiteCommand(sql, conn);
            //command.ExecuteNonQuery();


            sql = "SELECT * FROM test_table5";
            command = new SQLiteCommand(sql, conn);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                PrintReaderRow(reader);
            }

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

        static void Probe()
        {

            var result = Requests.Get("http://agent.mtconnect.org/probe", new TH_MTConnect.HTTP.ProxySettings("134.249.161.16", 80), 5000, 1);

            if (result != null) Console.WriteLine("Probe Successful");
            else Console.WriteLine("Probe Failed");

        }

        static void MySQL()
        {
            var plugin = new TH_MicrosoftSQL.Plugin();

            var config = new TH_MicrosoftSQL.SQL_Configuration();

            config.Database = "local_test_01";
            config.Server = "192.168.1.10";
            config.Port = 1433;
            config.Username = "trakhoundtest";
            config.Password = "ethan123";

            //Console.WriteLine(plugin.Table_GetSize(config, "exampletable").ToString());

            //Console.WriteLine(plugin.Ping(config).ToString());

            //Console.WriteLine(plugin.Database_Create(config).ToString());

            //var cols = new List<TH_Database.ColumnDefinition>();
            //cols.Add(new TH_Database.ColumnDefinition("timestamp", TH_Database.DataType.DateTime));
            //cols.Add(new TH_Database.ColumnDefinition("name", TH_Database.DataType.MediumText));
            //cols.Add(new TH_Database.ColumnDefinition("value", TH_Database.DataType.Double));

            //Console.WriteLine(plugin.Table_Create(config, "exampletable", cols.ToArray(), "name").ToString());


            ////var table = plugin.Table_Get(config, "exampletable");
            ////TH_Global.Functions.DataTable_Functions.WriteRowstoConsole("exampletable", table);

            //string[] tables = plugin.Table_List(config);

            //foreach (var table in tables) Console.WriteLine(table);

            //List<string> columns = plugin.Column_Get(config, "exampletable");
            //if (columns != null) foreach (var column in columns) Console.WriteLine(column);

            string tablename = "exampletable";
            var columns = new List<object>();
            columns.Add("timestamp");
            columns.Add("name");
            columns.Add("value");

            var values = new List<object>();
            values.Add("2015-01-20 01:02:03");
            values.Add("testname");
            values.Add("777");

            bool update = false;

            Console.WriteLine(plugin.Row_Insert(config, tablename, columns.ToArray(), values.ToArray(), "name", update).ToString());

            var table = plugin.Table_Get(config, "exampletable");
            TH_Global.Functions.DataTable_Functions.WriteRowstoConsole("exampletable", table);

            Console.ReadLine();
        }
    }
}
