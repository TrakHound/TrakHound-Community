// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Linq;
using System.Collections.Generic;

using TH_Database;
using TH_Global.Functions;
using TH_Plugins.Database;

namespace TH_MySQL
{
    public static class MySQL_Tools
    {

        public const string DateString = "yyyy-MM-dd H:mm:ss";

        /// <summary>
        /// Returns DateString as a string in the format needed to be imported into MySQL table
        /// Returns null (as string for mysql) if it cannot parse
        /// </summary>
        /// <param name="DateString">DateTime as a string</param>
        /// <returns></returns>
        public static string ConvertDateStringtoMySQL(string DateString)
        {
            string Result = "null";

            DateTime TS;
            if (DateTime.TryParse(DateString, out TS)) Result = TS.ToString(MySQL_Tools.DateString);

            return Result;
        }

        //public static string ConvertToSafe(string s)
        //{
        //    string r = s;
        //    if (r.Contains(@"\")) r = r.Replace(@"\", @"\\");
        //    if (r.Contains("'")) r = r.Replace("'", "\'");
        //    //if (r.Contains("%")) r = r.Replace("%", @"\%");
        //    return r;
        //}


        public static string COLUMN_NAME_START = "`";
        public static string COLUMN_NAME_END = "`";

        public static object[] ConvertColumnDefinitions(ColumnDefinition[] columns)
        {
            List<object> result = new List<object>();

            foreach (ColumnDefinition coldef in columns.ToList())
            {
                string def = coldef.ColumnName + " " + ConvertColumnDataType(coldef.DataType);
                if (coldef.NotNull) def += " NOT NULL";

                result.Add(def);
            }

            return result.ToArray();
        }

        public static string ConvertColumnDefinition(ColumnDefinition column)
        {
            return COLUMN_NAME_START + column.ColumnName + COLUMN_NAME_END + " " + ConvertColumnDataType(column.DataType);
        }

        public const string VarChar = "varchar(90)";
        public const string BigInt = "bigint";
        public const string Double = "double";
        public const string Datetime = "datetime";
        public const string Bool = "boolean";

        public static string ConvertColumnDataType(DataType dataType)
        {
            if (dataType == DataType.Boolean) return Bool;

            if (dataType == DataType.Short) return BigInt;
            if (dataType == DataType.Long) return BigInt;
            if (dataType == DataType.Double) return Double;

            if (dataType == DataType.SmallText) return VarChar;
            if (dataType == DataType.MediumText) return VarChar;
            if (dataType == DataType.LargeText) return VarChar;

            if (dataType == DataType.DateTime) return Datetime;
            
            return null;
        }



        public static string Row_Insert_CreateQuery(string TableName, object[] Columns, object[] Values, bool Update)
        {
            //Create Columns string
            string cols = "";
            for (int x = 0; x <= Columns.Length - 1; x++)
            {
                cols += Columns[x].ToString().ToUpper();
                if (x < Columns.Length - 1) cols += ", ";
            }

            //Create Values string
            string vals = "";
            for (int x = 0; x <= Values.Length - 1; x++)
            {
                // Dont put the ' characters if the value is null
                if (Values[x] == null) vals += "null";
                else
                {
                    object val = Values[x];
                    if (val.GetType() == typeof(DateTime)) val = MySQL_Tools.ConvertDateStringtoMySQL(val.ToString());
                    else if (val.GetType() == typeof(string))
                    {
                        val = String_Functions.ToSpecial(val.ToString());
                    }

                    if (Values[x].ToString().ToLower() != "null") vals += "'" + val.ToString() + "'";
                    else vals += Values[x].ToString();
                }

                if (x < Values.Length - 1) vals += ", ";
            }

            //Create Update string
            string update = "";
            if (Update)
            {
                update = " ON DUPLICATE KEY UPDATE ";
                for (int x = 0; x <= Columns.Length - 1; x++)
                {
                    update += Columns[x].ToString().ToUpper();
                    update += "=";

                    object val = Values[x];
                    if (val != null)
                    {
                        if (val.GetType() == typeof(DateTime)) val = MySQL_Tools.ConvertDateStringtoMySQL(val.ToString());
                        else if (val.GetType() == typeof(string))
                        {
                            val = String_Functions.ToSpecial(val.ToString());
                        }

                        update += "'" + val.ToString() + "'";
                    }
                    else update += "null";

                    if (x < Columns.Length - 1) update += ", ";
                }
            }

            return "INSERT IGNORE INTO " + TableName + " (" + cols + ") VALUES (" + vals + ")" + update;
        }

        public static string Row_Insert_CreateQuery(string TableName, object[] Columns, List<List<object>> Values, bool Update)
        {
            //Create Columns string
            string cols = "";
            for (int x = 0; x <= Columns.Length - 1; x++)
            {
                cols += Columns[x].ToString().ToUpper();
                if (x < Columns.Length - 1) cols += ", ";
            }

            //Create Values string
            string vals = "VALUES ";
            for (int i = 0; i <= Values.Count - 1; i++)
            {
                vals += "(";

                for (int x = 0; x <= Values[i].Count - 1; x++)
                {

                    List<object> ValueSet = Values[i];

                    // Dont put the ' characters if the value is null
                    if (ValueSet[x] == null) vals += "null";
                    else
                    {
                        object val = ValueSet[x];
                        if (val.GetType() == typeof(DateTime)) val = MySQL_Tools.ConvertDateStringtoMySQL(val.ToString());
                        else if (val.GetType() == typeof(string))
                        {
                            val = String_Functions.ToSpecial(val.ToString());
                        }

                        if (val.ToString().ToLower() != "null") vals += "'" + val.ToString() + "'";
                        else vals += val.ToString();
                    }


                    if (x < ValueSet.Count - 1) vals += ", ";
                }

                vals += ")";

                if (i < Values.Count - 1) vals += ",";

            }

            //Create Update string
            string update = "";
            if (Update)
            {
                update = " ON DUPLICATE KEY UPDATE ";
                for (int x = 0; x <= Columns.Length - 1; x++)
                {
                    update += Columns[x].ToString().ToUpper();
                    update += "=";

                    update += "VALUES(" + Columns[x].ToString().ToUpper() + ")";
                    if (x < Columns.Length - 1) update += ", ";
                }
            }

            return "INSERT IGNORE INTO " + TableName + " (" + cols + ") " + vals + update;
        }




        //bool CheckSQLPermissions()
        //{

        //    bool Result = false;

        //    DataTable Grants_DT = Global.GetGrants(Settings.SQL, Settings.SQL.Username);
        //    if (Grants_DT != null)
        //        {

        //        foreach (DataRow Row in Grants_DT.Rows)
        //            {
        //            if (Row[0] != null)
        //                {

        //                string Field = Row[0].ToString();

        //                TH_Console.Log(Field);

        //                string databasename = Settings.SQL.Database;
        //                databasename = databasename.Replace("_", "\\_").ToString().ToLower();

        //                if (

        //                    Field.Contains("ALL PRIVILEGES") ||

        //                    (

        //                    Field.Contains("ALTER") &&
        //                    Field.Contains("CREATE") &&
        //                    Field.Contains("DELETE") &&
        //                    Field.Contains("DROP") &&
        //                    Field.Contains("INSERT") &&
        //                    Field.Contains("SELECT") &&
        //                    Field.Contains("UPDATE") &&
        //                    (Field.Contains(databasename) || Field.Contains("%"))

        //                    )

        //                    )

        //                    {

        //                    Settings.Log.AddLine(Logger.ErrorClass.SQL, Logger.ErrorSubClass.None, "Correct Permissions for " + Settings.SQL.Username + " @ " + Settings.SQL.Database);
        //                    Result = true;

        //                    }
        //                else
        //                    {

        //                    Settings.Log.AddLine(Logger.ErrorClass.SQL, Logger.ErrorSubClass.None, "Incorrect Permissions for " + Settings.SQL.Username + " @ " + Settings.SQL.Database);

        //                    }

        //                }

        //            }

        //        }

        //    return Result;

        //}

    }
}
