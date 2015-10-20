using System;
using System.Collections.Generic;
using System.Linq;

using TH_Configuration;

namespace TH_Database.Tables.Configurations
{
    public static class Shared
    {

        public const string tablename = "shared";

        public class SharedConfiguration
        {
            public static SharedConfiguration Create(string description, Configuration config, string author, string image_url, string keywords)
            {
                SharedConfiguration result = new SharedConfiguration();

                result.description = description;
                result.manufacturer = config.Description.Manufacturer;
                result.device_type = config.Description.Machine_Type;
                result.model = config.Description.Model;
                result.controller = config.Description.Control_Type;
                result.author = author;
                result.tablename = GetTableName(author, config);
                result.upload_date = DateTime.UtcNow;
                result.image_url = image_url;
                result.keywords = keywords;

                return result;
            }

            public static string GetTableName(string author, Configuration config)
            {
                string table = "shared_" + author + "_" + config.Description.Manufacturer + "_" + config.Description.Machine_Type + "_" + config.Description.Control_Type;
                return table.Replace(' ', '_');
            }

            public string description { get; set; }
            public string manufacturer { get; set; }
            public string device_type { get; set; }
            public string model { get; set; }
            public string controller { get; set; }
            public string author { get; set; }
            public bool certified { get; set; }
            public string tablename { get; set; }
            public DateTime upload_date { get; set; }
            public string image_url { get; set; }
            public string keywords { get; set; }
        }

        public static void CreateSharedTable(Database_Settings config)
        {
            ColumnDefinition[] Columns = new ColumnDefinition[]
            {
                new ColumnDefinition("description", DataType.LargeText),
                new ColumnDefinition("manufacturer", DataType.LargeText),
                new ColumnDefinition("device_type", DataType.LargeText),
                new ColumnDefinition("model", DataType.LargeText),
                new ColumnDefinition("controller", DataType.LargeText),
                new ColumnDefinition("author", DataType.LargeText),
                new ColumnDefinition("certified", DataType.Boolean),
                new ColumnDefinition("tablename", DataType.LargeText),
                new ColumnDefinition("upload_date", DataType.DateTime),
                new ColumnDefinition("image_url", DataType.LargeText),
                new ColumnDefinition("keywords", DataType.LargeText)
            };

            Table.Create(config, tablename, Columns, null);
        }

        public static void CreateSharedConfiguration(Database_Settings db, SharedConfiguration sharedConfig)
        {
            CreateSharedTable(db);

            List<string> columns = new List<string>();
            columns.Add("description");
            columns.Add("manufacturer");
            columns.Add("device_type");
            columns.Add("model");
            columns.Add("controller");
            columns.Add("author");
            columns.Add("certified");
            columns.Add("tablename");
            columns.Add("upload_date");
            columns.Add("image_url");
            columns.Add("keywords");

            List<object> values = new List<object>();
            values.Add(sharedConfig.description);
            values.Add(sharedConfig.manufacturer);
            values.Add(sharedConfig.device_type);
            values.Add(sharedConfig.model);
            values.Add(sharedConfig.controller);
            values.Add(sharedConfig.author);
            values.Add(sharedConfig.certified);
            values.Add(sharedConfig.tablename);
            values.Add(sharedConfig.upload_date);
            values.Add(sharedConfig.image_url);
            values.Add(sharedConfig.keywords);

            Row.Insert(db, tablename, columns.ToArray(), values.ToArray(), true);
        }

    }
}
