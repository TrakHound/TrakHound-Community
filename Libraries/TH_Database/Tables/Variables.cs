// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Data;

using TH_Configuration;
using TH_Global;

namespace TH_Database.Tables
{
    /// <summary>
    /// Functions for the "Variables" table used for simple storage of variables
    /// </summary>
    public static class Variables
    {
        static string[] primaryKey = { "Variable" };

        public static void CreateTable(Database_Settings config, string TablePrefix = "")
        {
            ColumnDefinition[] Columns = new ColumnDefinition[]
            {
                new ColumnDefinition("Variable", DataType.LargeText),
                new ColumnDefinition("Value", DataType.LargeText),
                new ColumnDefinition("Timestamp", DataType.DateTime)
            };

            Table.Create(config, TablePrefix + TableNames.Variables, Columns, primaryKey);
        }

        public static void Update(Database_Settings config, string variable, string value, DateTime timestamp, string TablePrefix = "")
        {
            List<string> columns = new List<string>();
            columns.Add("Variable");
            columns.Add("Value");
            columns.Add("Timestamp");

            List<object> values = new List<object>();
            values.Add(variable);
            values.Add(value);
            values.Add(timestamp);

            Row.Insert(config, TablePrefix + TableNames.Variables, columns.ToArray(), values.ToArray(), primaryKey, true);
        }

        public static VariableData Get(Database_Settings config, string VariableName, string TablePrefix = "")
        {
            VariableData Result = null;

            DataRow row = Row.Get(config, TablePrefix + TableNames.Variables, "variable", VariableName);

            if (row != null)
            {
                Result = new VariableData();

                if (row["variable"] != null) Result.variable = row["variable"].ToString();
                if (row["value"] != null) Result.value = row["value"].ToString();
                if (row["timestamp"] != null)
                {
                    DateTime timestamp = DateTime.MinValue;
                    DateTime.TryParse(row["timestamp"].ToString(), out timestamp);
                    if (timestamp > DateTime.MinValue) Result.timestamp = timestamp;
                }
            }

            return Result;
        }

        public class VariableData
        {
            public string variable { get; set; }
            public string value { get; set; }
            public DateTime timestamp { get; set; }
        }

    }
}
