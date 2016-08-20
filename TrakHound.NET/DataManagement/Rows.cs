// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Data;

using TrakHound.Tools;

namespace TrakHound.DataManagement
{
    public static class Row
    {

        public static bool Insert(object settings, string tablename, object[] columns, object[] values, string[] primaryKey, bool update)
        {
            bool result = false;

            if (settings != null)
            {
                var config = Configuration.Get(settings);
                if (config != null)
                {
                    var query = CreateInsertQuery(tablename, columns, values, update);
                    result = (bool)Query.Execute<bool>(config, query);                  
                }
            }

            return result;
        }

        public static bool Insert(object settings, string tablename, object[] columns, List<List<object>> valueList, string[] primaryKey, bool update)
        {
            bool result = false;

            if (settings != null)
            {
                var config = Configuration.Get(settings);
                if (config != null)
                {
                    var query = CreateInsertQuery(tablename, columns, valueList, update);
                    result = (bool)Query.Execute<bool>(config, query);
                }
            }

            return result;
        }

        public static bool Insert(object settings, string tablename, List<object[]> columnsList, List<object[]> valuesList, string[] primaryKey, bool update)
        {
            bool result = false;

            if (settings != null)
            {
                var config = Configuration.Get(settings);
                if (config != null)
                {
                    var query = CreateInsertQuery(tablename, columnsList.ToArray(), valuesList.ToArray(), update);

                    result = (bool)Query.Execute<bool>(config, query);
                }
            }

            return result;
        }

        public static bool Insert(object settings, string query)
        {
            bool result = false;

            if (settings != null)
            {
                var config = Configuration.Get(settings);
                if (config != null)
                {
                    result = (bool)Query.Execute<bool>(config, query);
                }
            }

            return result;
        }


        public static DataRow Get(object settings, string tablename, string tableKey, string rowKey)
        {
            DataRow result = null;

            if (settings != null)
            {
                var config = Configuration.Get(settings);
                if (config != null)
                {
                    var query = "SELECT * FROM " + tablename + " WHERE `" + tableKey + "` = '" + rowKey + "'";

                    result = (DataRow)Query.Execute<DataRow>(config, query);
                }
            }

            return result;
        }

        public static DataRow Get(object settings, string tablename, string query)
        {
            DataRow result = null;

            if (settings != null)
            {
                var config = Configuration.Get(settings);
                if (config != null)
                {
                    query = "SELECT * FROM " + tablename + " " + query;

                    result = (DataRow)Query.Execute<DataRow>(config, query);
                }
            }

            return result;
        }


        public static bool Exists(object settings, string tablename, string filterString)
        {
            bool result = false;

            if (settings != null)
            {
                var config = Configuration.Get(settings);
                if (config != null)
                {
                    var query = "SELECT EXISTS(SELECT 1 FROM " + tablename + " WHERE " + filterString + " LIMIT 1);";

                    var val = (int)Query.Execute<int>(config, query);
                    if (val >= 0)
                    {
                        bool.TryParse(val.ToString(), out result);
                    }
                }
            }

            return result;
        }


        public static string CreateInsertQuery(string TableName, object[] Columns, object[] Values, bool Update)
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
                    if (val.GetType() == typeof(DateTime)) val = Query.ConvertToDateTime(val.ToString());
                    else if (val.GetType() == typeof(string))
                    {
                        val = String_Functions.ToSpecial(val.ToString());
                    }


                    if (Values[x].ToString().ToLower() != "null") vals += "'" + val.ToString() + "'";
                    else vals += Values[x].ToString();
                }

                if (x < Values.Length - 1) vals += ", ";
            }

            return "INSERT OR REPLACE INTO " + TableName + " (" + cols + ") VALUES (" + vals + ")";
        }

        public static string CreateInsertQuery(string TableName, object[] Columns, List<List<object>> Values, bool Update)
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
                        if (val.GetType() == typeof(DateTime)) val = Query.ConvertToDateTime(val.ToString());
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

            return "INSERT OR REPLACE INTO " + TableName + " (" + cols + ") " + vals;
        }
    }
}
