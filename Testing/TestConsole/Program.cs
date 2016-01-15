using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TH_MicrosoftSQL;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
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
