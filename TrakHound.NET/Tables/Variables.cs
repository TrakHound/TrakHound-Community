// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Data;

using TrakHound.DataManagement;
using TrakHound.Plugins.Database;

namespace TrakHound.Tables
{
    /// <summary>
    /// Functions for the "Variables" table used for simple storage of variables
    /// </summary>
    public static class Variables
    {
        static string[] primaryKey = { "Variable" };

        public static List<VariableData> Data = new List<VariableData>();

        public class VariableData
        {
            public VariableData() { }

            public VariableData(string variable, string value, DateTime timestamp)
            {
                Variable = variable;
                Value = value;
                Timestamp = timestamp;
            }

            public string UniqueId { get; set; }

            public string Variable { get; set; }

            public string Value { get; set; }
            //public string PreviousValue { get; set; }

            public DateTime Timestamp { get; set; }
        }


        public static void CreateTable(string tablePrefix = "")
        {
            CreateTable(Database.Configuration, tablePrefix);
        }

        public static void CreateTable(DataManagement.Configuration config, string tablePrefix = "")
        {
            var columns = new ColumnDefinition[]
            {
                new ColumnDefinition("Variable", DataType.LargeText),
                new ColumnDefinition("Value", DataType.LargeText),
                new ColumnDefinition("Timestamp", DataType.DateTime)
            };

            Table.Create(config, tablePrefix + Names.Variables, columns, primaryKey);
        }


        //public static void Update(string variable, string value, DateTime timestamp, string tablePrefix = "")
        //{
        //    Update(Database.Configuration, variable, value, timestamp, tablePrefix);
        //}

        //public static void Update(DataManagement.Configuration config, string variable, string value, DateTime timestamp, string tablePrefix = "")
        //{
        //    List<string> columns = new List<string>();
        //    columns.Add("Variable");
        //    columns.Add("Value");
        //    columns.Add("Timestamp");

        //    List<object> values = new List<object>();
        //    values.Add(variable);
        //    values.Add(value);
        //    values.Add(timestamp);

        //    string query = Row.CreateInsertQuery(tablePrefix + Names.Variables, columns.ToArray(), values.ToArray(), true);
        //    Server.TrakHound.Servers.ProcessServer.Server.DatabaseQueue.AddToQueue(query);
        //}

        public static void Update(VariableData info, string tablePrefix = "")
        {
            var infos = new VariableData[1];
            infos[0] = info;

            Update(Database.Configuration, infos, tablePrefix);
        }

        public static void Update(VariableData[] infos, string tablePrefix = "")
        {
            Update(Database.Configuration, infos, tablePrefix);
        }

        public static void Update(DataManagement.Configuration config, VariableData[] infos, string tablePrefix = "")
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
                    var match = Data.Find(x => x.Variable == info.Variable && x.UniqueId == info.UniqueId);
                    if (match == null)
                    {
                        Data.Add(info);
                        match = info;
                    }

                    if (match.Value != info.Value)
                    {
                        match.Value = info.Value;

                        var rowValues = new List<object>();
                        rowValues.Add(info.Variable);
                        rowValues.Add(info.Value);
                        rowValues.Add(info.Timestamp);
                        tableValues.Add(rowValues);
                    }
                }

                if (tableValues.Count > 0)
                {
                    string query = Row.CreateInsertQuery(tablePrefix + Names.Variables, columns.ToArray(), tableValues, true);
                    //TrakHound.Servers.Processor.Server.DatabaseQueue.AddToQueue(query);
                }
            }  
        }

        //public static void Update(string[] variables, string[] values, DateTime[] timestamps, string tablePrefix = "")
        //{
        //    Update(Database.Configuration, variables, values, timestamps, tablePrefix);
        //}

        //public static void Update(DataManagement.Configuration config, string[] variables, string[] values, DateTime[] timestamps, string tablePrefix = "")
        //{
        //    if (variables.Length == values.Length && variables.Length == timestamps.Length)
        //    {
        //        List<string> columns = new List<string>();
        //        columns.Add("Variable");
        //        columns.Add("Value");
        //        columns.Add("Timestamp");

        //        var tableValues = new List<List<object>>();

        //        for (var x = 0; x <= variables.Length - 1; x++)
        //        {
        //            var rowValues = new List<object>();
        //            rowValues.Add(variables[x]);
        //            rowValues.Add(values[x]);
        //            rowValues.Add(timestamps[x]);
        //            tableValues.Add(rowValues);
        //        }

        //        if (tableValues.Count > 0)
        //        {
        //            string query = Row.CreateInsertQuery(tablePrefix + Names.Variables, columns.ToArray(), tableValues, true);
        //            Server.TrakHound.Servers.ProcessServer.Server.DatabaseQueue.AddToQueue(query);
        //        }
        //    }
        //}


        public static VariableData Get(string variableName, string tablePrefix = "")
        {
            return Get(Database.Configuration, variableName, tablePrefix);
        }

        public static VariableData Get(DataManagement.Configuration config, string variableName, string tablePrefix = "")
        {
            VariableData Result = null;

            DataRow row = Row.Get(config, tablePrefix + Names.Variables, "variable", variableName);

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

    }
}
