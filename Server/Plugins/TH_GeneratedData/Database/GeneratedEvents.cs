// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Data;
using System.Linq;

using TH_Configuration;
using TH_Database;
using TH_Global;
using TH_Plugins.Database;

using TH_GeneratedData.GeneratedEvents;

namespace TH_GeneratedData.Database
{
    public static class GeneratedEvents
    {

        private static string[] genEventValuesPrimaryKey = { "EVENT", "VALUE" };

        public static void CreateValueTable(Configuration config, List<Event> events)
        {
            var columns = new List<ColumnDefinition>()
            {
                new ColumnDefinition("EVENT", DataType.LargeText),
                new ColumnDefinition("VALUE", DataType.LargeText),
                new ColumnDefinition("NUMVAL", DataType.Long)
            };

            Table.Replace(config.Databases_Server, GetTableName(config, TableNames.GenEventValues), columns.ToArray(), genEventValuesPrimaryKey);

            if (events != null)
            {
                List<string> insertColumns = new List<string>();
                insertColumns.Add("EVENT");
                insertColumns.Add("VALUE");
                insertColumns.Add("NUMVAL");

                List<List<object>> rowValues = new List<List<object>>();

                foreach (var e in events)
                {
                    foreach (Value v in e.Values)
                    {
                        List<object> values = new List<object>();
                        values.Add(e.Name);
                        values.Add(v.Result.Value);
                        values.Add(v.Result.NumVal);
                        rowValues.Add(values);
                    }

                    // Add Default Value
                    List<object> defaultValues = new List<object>();
                    defaultValues.Add(e.Name);
                    defaultValues.Add(e.Default.Value);
                    defaultValues.Add(e.Default.NumVal);
                    rowValues.Add(defaultValues);
                }

                Row.Insert(config.Databases_Server, GetTableName(config, TableNames.GenEventValues), insertColumns.ToArray(), rowValues, genEventValuesPrimaryKey, true);
            }
        }



        public const string GenTablePrefix = TableNames.Gen_Events_TablePrefix;
        static string[] genEventsPrimaryKey = { "TIMESTAMP" };

        public static void CreateEventTable(Configuration config, Event e)
        {
            List<ColumnDefinition> columns = new List<ColumnDefinition>()
            {
                new ColumnDefinition("TIMESTAMP", DataType.DateTime),
                new ColumnDefinition("VALUE", DataType.LargeText),
                new ColumnDefinition("NUMVAL", DataType.Long)
            };

            foreach (var item in e.CaptureItems)
            {
                string columnName = FormatCaptureItemColumn(item.name);
                columns.Add(new ColumnDefinition(columnName, DataType.LargeText));
            }

            ColumnDefinition[] ColArray = columns.ToArray();

            string tableName;
            if (config.DatabaseId != null) tableName = config.DatabaseId + "_" + GenTablePrefix + e.Name;
            else tableName = GenTablePrefix + e.Name;

            Table.Create(config.Databases_Server, tableName, ColArray, genEventsPrimaryKey);
        }

        public static void InsertEventItems(Configuration config, List<GeneratedEventItem> generatedEventItems)
        {
            IEnumerable<string> distinctEventNames = generatedEventItems.Select(x => x.EventName).Distinct();

            foreach (string eventName in distinctEventNames)
            {
                // Set Columns to Update (include Name so that it can Update the row instead of creating a new one)
                List<string> columns = new List<string>();
                columns.Add("Timestamp");
                columns.Add("Value");
                columns.Add("Numval");

                // Add columns to update for CaptureItems
                var gdc = GeneratedDataConfiguration.Get(config);
                if (gdc != null)
                {
                    var e = gdc.GeneratedEventsConfiguration.Events.Find(x => x.Name.ToLower() == eventName.ToLower());
                    if (e != null)
                    {
                        foreach (var item in e.CaptureItems) columns.Add(FormatCaptureItemColumn(item.name));
                    }
                }

                List<List<object>> rowValues = new List<List<object>>();

                List<GeneratedEventItem> eventItems = generatedEventItems.FindAll(x => x.EventName == eventName);
                if (eventItems != null)
                {
                    foreach (var eventItem in eventItems)
                    {
                        if (eventItem.Value != eventItem.PreviousValue)
                        {
                            List<object> values = new List<object>();

                            values.Add(eventItem.Timestamp);
                            values.Add(eventItem.Value);
                            values.Add(eventItem.Numval);

                            foreach (var item in eventItem.CaptureItems) values.Add(FormatCaptureItemValue(item.value));

                            rowValues.Add(values);
                        }
                    }

                    string tableName;
                    if (config.DatabaseId != null) tableName = config.DatabaseId + "_" + GenTablePrefix + eventName;
                    else tableName = GenTablePrefix + eventName;

                    Row.Insert(config.Databases_Server, tableName, columns.ToArray(), rowValues, genEventsPrimaryKey, true);
                }
            }
        }



        private static string FormatCaptureItemColumn(string val)
        {
            string Result = val.Replace(' ', '_').ToUpper();

            return Result;
        }

        private static string FormatCaptureItemValue(string val)
        {
            string Result = val;

            return Result;
        }

        private static string GetTableName(Configuration config, string tablename)
        {
            if (config.DatabaseId != null) return config.DatabaseId + "_" + tablename;
            else return tablename;
        }

    }
}
