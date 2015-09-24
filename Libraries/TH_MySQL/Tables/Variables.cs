using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;

using TH_MySQL;
// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using TH_Configuration;
using TH_Global;

namespace TH_MySQL.Tables
{
    /// <summary>
    /// Functions for the "Variables" table used for simple storage of variables
    /// </summary>
    public static class Variables
    {

        public static void CreateTable(SQL_Settings SQL)
        {
            object[] Columns = new object[] {
            "Variable " +  MySQL_Tools.VarChar,
            "Value " +  MySQL_Tools.VarChar,
            "Timestamp " +  MySQL_Tools.Datetime,          
            };

            Global.Table_Create(SQL, TableNames.Variables, Columns, "Variable");
        }

        public static void Update(SQL_Settings SQL, string variable, string value, DateTime timestamp)
        {
            List<string> columns = new List<string>();
            columns.Add("Variable");
            columns.Add("Value");
            columns.Add("Timestamp");

            List<object> values = new List<object>();
            values.Add(variable);
            values.Add(value);
            values.Add(TH_MySQL.MySQL_Tools.ConvertDateStringtoMySQL(timestamp.ToString()));

            Global.Row_Insert(SQL, TableNames.Variables, columns.ToArray(), values.ToArray());
        }

        public static VariableData Get(SQL_Settings SQL, string VariableName)
        {
            VariableData Result = null;

            DataRow Row = Global.Row_Get(SQL, TableNames.Variables, "variable", VariableName);

            if (Row != null)
            {
                Result = new VariableData();

                if (Row["variable"] != null) Result.variable = Row["variable"].ToString();
                if (Row["value"] != null) Result.value = Row["value"].ToString();
                if (Row["timestamp"] != null)
                {
                    DateTime timestamp = DateTime.MinValue;
                    DateTime.TryParse(Row["timestamp"].ToString(), out timestamp);
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
