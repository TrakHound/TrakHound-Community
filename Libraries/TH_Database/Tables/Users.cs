using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using TH_Configuration;

namespace TH_Database.Tables
{
    public static class Users
    {

        public const string tablename = "users";

        public static void CreateTable(Database_Settings config)
        {
            ColumnDefinition[] Columns = new ColumnDefinition[]
            {
                new ColumnDefinition("tablename", DataType.LargeText),
                new ColumnDefinition("username", DataType.LargeText),
                new ColumnDefinition("password", DataType.LargeText)
            };

            Table.Create(config, tablename, Columns, "tablename");
        }

        public static void Update(Database_Settings config, string username, string password, string deviceId)
        {
            List<string> columns = new List<string>();
            columns.Add("tablename");
            columns.Add("username");
            columns.Add("password");
            
            List<object> values = new List<object>();
            values.Add(username + "_" + deviceId + "_configuration");
            values.Add(username);
            values.Add(password);

            Row.Insert(config, tablename, columns.ToArray(), values.ToArray(), true);
        }

    }
}
