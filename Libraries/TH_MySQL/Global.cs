// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;

using System.Data;

using TH_Configuration;

namespace TH_MySQL
{
    public static class Global
    {

        #region "Databases"

        public static bool Database_Create(SQL_Settings SQL, string DatabaseName)
        {
            bool Result = false;

            if (SQL.PHP_Server != null)
            {
                Result = PHP.Database_Create(SQL, DatabaseName);
                if (SQL.AdminSQL != null) Global.Database_Create(SQL.AdminSQL, DatabaseName);
            }
            else
            {
                Result = MySQL.Database_Create(SQL, DatabaseName);
                if (SQL.AdminSQL != null) Global.Database_Create(SQL.AdminSQL, DatabaseName);
            }

            return Result;
        }

        public static bool Database_Drop(SQL_Settings SQL, string DatabaseName)
        {
            bool Result = false;

            if (SQL.PHP_Server != null)
            {
                Result = PHP.Database_Drop(SQL, DatabaseName);
                if (SQL.AdminSQL != null) Global.Database_Drop(SQL.AdminSQL, DatabaseName);
            }
            else
            {
                Result = MySQL.Database_Drop(SQL, DatabaseName);
                if (SQL.AdminSQL != null) Global.Database_Drop(SQL.AdminSQL, DatabaseName);
            }

            return Result;
        }

        #endregion

        #region "Tables"

        public static bool Table_Create(SQL_Settings SQL, string TableName, object[] ColumnDefinitions, string PrimaryKey)
        {
            bool Result = false;

            if (SQL.PHP_Server != null)
            {
                Result = PHP.Table_Create(SQL, TableName, ColumnDefinitions, PrimaryKey);
                if (SQL.AdminSQL != null) Global.Table_Create(SQL.AdminSQL, TableName, ColumnDefinitions, PrimaryKey);
            }
            else
            {
                Result = MySQL.Table_Create(SQL, TableName, ColumnDefinitions, PrimaryKey);
                if (SQL.AdminSQL != null) Global.Table_Create(SQL.AdminSQL, TableName, ColumnDefinitions, PrimaryKey);
            }

            return Result;
        }

        public static bool Table_AddColumn(SQL_Settings SQL, string TableName, string ColumnDefinition)
        {
            bool Result = false;

            if (SQL.PHP_Server != null)
            {
                Result = PHP.Table_AddColumn(SQL, TableName, ColumnDefinition);
                if (SQL.AdminSQL != null) Global.Table_AddColumn(SQL.AdminSQL, TableName, ColumnDefinition);
            }
            else
            {
                Result = MySQL.Table_AddColumn(SQL, TableName, ColumnDefinition);
                if (SQL.AdminSQL != null) Global.Table_AddColumn(SQL.AdminSQL, TableName, ColumnDefinition);
            }

            return Result;
        }


        public static bool Table_Drop(SQL_Settings SQL, string TableName)
        {
            bool Result = false;

            if (SQL.PHP_Server != null)
            {
                Result = PHP.Table_Drop(SQL, TableName);
                if (SQL.AdminSQL != null) Global.Table_Drop(SQL.AdminSQL, TableName);
            }
            else
            {
                Result = MySQL.Table_Drop(SQL, TableName);
                if (SQL.AdminSQL != null) Global.Table_Drop(SQL.AdminSQL, TableName);
            }

            return Result;
        }

        public static bool Table_Drop(SQL_Settings SQL, string[] TableNames)
        {
            bool Result = false;

            if (SQL.PHP_Server != null)
            {
                Result = PHP.Table_Drop(SQL, TableNames);
                if (SQL.AdminSQL != null) Global.Table_Drop(SQL.AdminSQL, TableNames);
            }
            else
            {
                Result = MySQL.Table_Drop(SQL, TableNames);
                if (SQL.AdminSQL != null) Global.Table_Drop(SQL.AdminSQL, TableNames);
            }

            return Result;
        }

        public static bool Table_Truncate(SQL_Settings SQL, string TableName)
        {
            bool Result = false;

            if (SQL.PHP_Server != null)
            {
                Result = PHP.Table_Truncate(SQL, TableName);
                if (SQL.AdminSQL != null) Global.Table_Truncate(SQL.AdminSQL, TableName);
            }
            else
            {
                Result = MySQL.Table_Truncate(SQL, TableName);
                if (SQL.AdminSQL != null) Global.Table_Truncate(SQL.AdminSQL, TableName);
            }

            return Result;
        }


        public static DataTable Table_Get(SQL_Settings SQL, string TableName)
        {
            DataTable Result = null;

            if (SQL.PHP_Server != null)
            {
                Result = PHP.Table_Get(SQL, TableName);
            }
            else
            {
                Result = MySQL.Table_Get(SQL, TableName);
            }

            return Result;
        }

        public static DataTable Table_Get(SQL_Settings SQL, string TableName, string FilterExpression)
        {
            DataTable Result = null;

            if (SQL.PHP_Server != null)
            {
                Result = PHP.Table_Get(SQL, TableName, FilterExpression);
            }
            else
            {
                Result = MySQL.Table_Get(SQL, TableName, FilterExpression);
            }

            return Result;
        }

        public static DataTable Table_Get(SQL_Settings SQL, string TableName, string FilterExpression, string Columns)
        {
            DataTable Result = null;

            if (SQL.PHP_Server != null)
            {
                Result = PHP.Table_Get(SQL, TableName, FilterExpression, Columns);
            }
            else
            {
                Result = MySQL.Table_Get(SQL, TableName, FilterExpression, Columns);
            }

            return Result;
        }


        public static List<string> Table_GetColumns(SQL_Settings SQL, string TableName)
        {
            List<string> Result = null;

            if (SQL.PHP_Server != null)
            {
                Result = PHP.Table_GetColumns(SQL, TableName);
            }
            else
            {
                Result = MySQL.Table_GetColumns(SQL, TableName);
            }

            return Result;
        }


        public static string[] Table_List(SQL_Settings SQL)
        {
            if (SQL.PHP_Server != null)
            {
                return PHP.Table_List(SQL);
            }
            else
            {
                return MySQL.Table_List(SQL);
            }
        }

        public static Int64 Table_RowCount(SQL_Settings SQL, string TableName)
        {
            Int64 Result = -1;

            if (SQL.PHP_Server != null)
            {
                Result = PHP.Table_RowCount(SQL, TableName);
            }
            else
            {
                Result = MySQL.Table_RowCount(SQL, TableName);
            }

            return Result;
        }

        public static Int64 Table_Size(SQL_Settings SQL, string TableName)
        {
            Int64 Result = -1;

            if (SQL.PHP_Server != null)
            {
                Result = PHP.Table_Size(SQL, TableName);
            }
            else
            {
                Result = MySQL.Table_Size(SQL, TableName);
            }

            return Result;
        }



        public static bool TableExists(SQL_Settings SQL, string TableName)
        {
            bool Result = false;

            string[] Tables = null;

            if (SQL.PHP_Server != null)
            {
                Tables = PHP.Table_List(SQL);
            }
            else
            {
                Tables = MySQL.Table_List(SQL);
            }

            if (Tables != null)
            {
                if (Tables.ToList<string>().Contains(TableName)) Result = true;
            }

            return Result;
        }

        public static bool TableExists(SQL_Settings SQL, List<string> TableNames)
        {
            bool Result = false;

            string[] Tables = null;

            if (SQL.PHP_Server != null)
            {
                Tables = PHP.Table_List(SQL);
            }
            else
            {
                Tables = MySQL.Table_List(SQL);
            }

            if (Tables != null)
            {
                Result = true;

                foreach (string TableName in TableNames)
                {
                    if (!Tables.ToList<string>().Contains(TableName))
                    {
                        Result = false;
                        break;
                    }
                }
            }

            return Result;
        }




        #endregion

        #region "Rows"

        public static bool Row_Insert(SQL_Settings SQL, string TableName, object[] Columns, object[] Values)
        {
            bool Result = false;

            if (SQL.PHP_Server != null)
            {
                PHP.Row_Insert(SQL, TableName, Columns, Values, true);
                if (SQL.AdminSQL != null) Global.Row_Insert(SQL.AdminSQL, TableName, Columns, Values, true);
            }
            else
            {
                MySQL.Row_Insert(SQL, TableName, Columns, Values, true);
                if (SQL.AdminSQL != null) Global.Row_Insert(SQL.AdminSQL, TableName, Columns, Values, true);
            }

            return Result;
        }

        public static bool Row_Insert(SQL_Settings SQL, string TableName, object[] Columns, List<List<object>> Values)
        {
            bool Result = false;

            if (SQL.PHP_Server != null)
            {
                PHP.Row_Insert(SQL, TableName, Columns, Values, true);
                if (SQL.AdminSQL != null) Global.Row_Insert(SQL.AdminSQL, TableName, Columns, Values, true);
            }
            else
            {
                MySQL.Row_Insert(SQL, TableName, Columns, Values, true);
                if (SQL.AdminSQL != null) Global.Row_Insert(SQL.AdminSQL, TableName, Columns, Values, true);
            }

            return Result;
        }

        public static bool Row_Insert(SQL_Settings SQL, string TableName, object[] Columns, object[] Values, bool Update)
        {
            bool Result = false;

            if (SQL.PHP_Server != null)
            {
                PHP.Row_Insert(SQL, TableName, Columns, Values, Update);
                if (SQL.AdminSQL != null) Global.Row_Insert(SQL.AdminSQL, TableName, Columns, Values, Update);
            }
            else
            {
                MySQL.Row_Insert(SQL, TableName, Columns, Values, Update);
                if (SQL.AdminSQL != null) Global.Row_Insert(SQL.AdminSQL, TableName, Columns, Values, Update);
            }

            return Result;
        }

        public static bool Row_Insert(SQL_Settings SQL, string TableName, object[] Columns, List<List<object>> Values, bool Update)
        {
            bool Result = false;

            if (SQL.PHP_Server != null)
            {
                PHP.Row_Insert(SQL, TableName, Columns, Values, Update);
                if (SQL.AdminSQL != null) Global.Row_Insert(SQL.AdminSQL, TableName, Columns, Values, Update);
            }
            else
            {
                MySQL.Row_Insert(SQL, TableName, Columns, Values, Update);
                if (SQL.AdminSQL != null) Global.Row_Insert(SQL.AdminSQL, TableName, Columns, Values, Update);
            }

            return Result;
        }


        public static DataRow Row_Get(SQL_Settings SQL, string TableName, string TableKey, string RowKey)
        {
            DataRow Result = null;

            if (SQL.PHP_Server != null)
            {
                Result = PHP.Row_Get(SQL, TableName, TableKey, RowKey);
            }
            else
            {
                Result = MySQL.Row_Get(SQL, TableName, TableKey, RowKey);
            }

            return Result;
        }

        public static DataRow Row_Get(SQL_Settings SQL, string TableName, string Query)
        {
            DataRow Result = null;

            if (SQL.PHP_Server != null)
            {
                Result = PHP.Row_Get(SQL, TableName, Query);
            }
            else
            {
                Result = MySQL.Row_Get(SQL, TableName, Query);
            }

            return Result;
        }


        public static bool Row_Exists(SQL_Settings SQL, string TableName, string FilterString)
        {
            bool Result = false;

            if (SQL.PHP_Server != null)
            {
                Result = PHP.Row_Exists(SQL, TableName, FilterString);
            }
            else
            {
                Result = MySQL.Row_Exists(SQL, TableName, FilterString);
            }

            return Result;
        }

        #endregion


        public static object Value_Get(SQL_Settings SQL, string TableName, string Column, string FilterExpression)
        {
            object Result = null;

            if (SQL.PHP_Server != null)
            {
                Result = PHP.Value_Get(SQL, TableName, Column, FilterExpression);
            }
            else
            {
                Result = MySQL.Value_Get(SQL, TableName, Column, FilterExpression);
            }

            return Result;
        }

        public static object[] CustomCommand(SQL_Settings SQL, string CommandText)
        {
            object[] Result = null;

            if (SQL.PHP_Server != null)
            {
                Result = PHP.CustomCommand(SQL, CommandText);
            }
            else
            {
                Result = MySQL.CustomCommand(SQL, CommandText);
            }

            return Result;
        }


        public static DataTable GetGrants(SQL_Settings SQL, string UserName)
        {
            DataTable Result = null;

            if (SQL.PHP_Server != null)
            {
                Result = PHP.GetGrants(SQL, UserName);
            }
            else
            {
                Result = MySQL.GetGrants(SQL, UserName);
            }

            return Result;
        }

    }
}
