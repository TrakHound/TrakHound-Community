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

namespace TH_Device_Server.TableManagement
{
    /// <summary>
    /// Functions for the "Variables" table used for simple storage of variables
    /// </summary>
    public static class Variables
    {

        public static void CreateTable(SQL_Settings SQL)
        {
            object[] Columns = new object[] {
            "variable_name " +  MySQL_Tools.VarChar,
            "value " +  MySQL_Tools.VarChar,
            "last_changed " +  MySQL_Tools.VarChar,
            "prev_value " +  MySQL_Tools.VarChar           
            };

            Global.Table_Create(SQL, TableNames.Variables, Columns, "variable_name");
        }

        public static void UpdateRow(SQL_Settings SQL, string VariableName, string Value)
        {
            DataRow OldRow = Global.Row_Get(SQL, TableNames.ConfigTableName, "variable_name", VariableName);
            string prev_Value = null;
            string lastchanged = null;

            object[] Columns;
            object[] Values;

            if (OldRow != null)
            {
                if (OldRow["prev_value"].ToString() == Value)
                {
                    prev_Value = OldRow["prev_value"].ToString();
                    lastchanged = OldRow["last_changed"].ToString();
                }
                else
                {
                    prev_Value = OldRow["value"].ToString();
                    lastchanged = DateTime.Now.ToString();
                }

                Columns = new object[] { "variable_name", "value", "last_changed", "prev_value" };

                Values = new object[] { VariableName, Value, lastchanged, prev_Value };
            }
            else
            {
                Columns = new object[] { "variable_name", "value" };

                Values = new object[] { VariableName, Value };
            }

            Global.Row_Insert(SQL, TableNames.Variables, Columns, Values);

        }

        public static string GetValue(SQL_Settings SQL, string VariableName)
        {
            string Result = null;

            DataRow Row = Global.Row_Get(SQL, TableNames.Variables, "variable_name", VariableName);

            if (Row != null)
            {
                if (Row["value"] != null) Result = Row["value"].ToString();
            }

            return Result;
        }

    }
}
