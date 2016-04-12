// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Data;

using TH_Configuration;
using TH_Global;
using TH_Plugins.Database;

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

        public static void Update(Database_Settings config, VariableData[] infos, string TablePrefix = "")
        {
            if (infos.Length > 0)
            {
                List<string> columns = new List<string>();
                columns.Add("Variable");
                columns.Add("Value");
                columns.Add("Timestamp");

                var tableValues = new List<List<object>>();

                foreach (var info in infos)
                {
                    var rowValues = new List<object>();
                    rowValues.Add(info.Variable);
                    rowValues.Add(info.Value);
                    rowValues.Add(info.Timestamp);
                    tableValues.Add(rowValues);
                }

                Row.Insert(config, TablePrefix + TableNames.Variables, columns.ToArray(), tableValues, primaryKey, true);
            }  
        }

        public static void Update(Database_Settings config, string[] variables, string[] values, DateTime[] timestamps, string TablePrefix = "")
        {
            if (variables.Length == values.Length && variables.Length == timestamps.Length)
            {
                List<string> columns = new List<string>();
                columns.Add("Variable");
                columns.Add("Value");
                columns.Add("Timestamp");

                var tableValues = new List<List<object>>();

                for (var x = 0; x <= variables.Length - 1; x++)
                {
                    var rowValues = new List<object>();
                    rowValues.Add(variables[x]);
                    rowValues.Add(values[x]);
                    rowValues.Add(timestamps[x]);
                    tableValues.Add(rowValues);
                }

                Row.Insert(config, TablePrefix + TableNames.Variables, columns.ToArray(), tableValues, primaryKey, true);
            }
        }

        public static VariableData Get(Database_Settings config, string VariableName, string TablePrefix = "")
        {
            VariableData Result = null;

            DataRow row = Row.Get(config, TablePrefix + TableNames.Variables, "variable", VariableName);

            if (row != null)
            {
                Result = new VariableData();

                if (row["variable"] != null) Result.Variable = row["variable"].ToString();
                if (row["value"] != null) Result.Value = row["value"].ToString();
                if (row["timestamp"] != null)
                {
                    DateTime timestamp = DateTime.MinValue;
                    DateTime.TryParse(row["timestamp"].ToString(), out timestamp);
                    if (timestamp > DateTime.MinValue) Result.Timestamp = timestamp;
                }
            }

            return Result;
        }

        public class VariableData
        {
            public VariableData() { }

            public VariableData(string variable, string value, DateTime timestamp)
            {
                Variable = variable;
                Value = value;
                Timestamp = timestamp;
            }

            public string Variable { get; set; }
            public string Value { get; set; }
            public DateTime Timestamp { get; set; }
        }

    }
}
