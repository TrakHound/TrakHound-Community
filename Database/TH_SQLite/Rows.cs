using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;

using TH_Database;
using TH_Global.Functions;

namespace TH_SQLite
{
    public partial class Plugin
    {

        public bool Row_Insert(object settings, string tablename, object[] columns, object[] values, string[] primaryKey, bool update)
        {
            bool result = false;

            if (settings != null)
            {
                var config = SQLite_Configuration.Get(settings);
                if (config != null)
                {
                    var query = Row_Insert_CreateQuery(tablename, columns, values, update);
                    result = (bool)ExecuteQuery<bool>(config, query);
                }
            }

            return result;
        }

        public bool Row_Insert(object settings, string tablename, object[] columns, List<List<object>> valueList, string[] primaryKey, bool update)
        {
            bool result = false;

            if (settings != null)
            {
                var config = SQLite_Configuration.Get(settings);
                if (config != null)
                {
                    var query = Row_Insert_CreateQuery(tablename, columns, valueList, update);
                    result = (bool)ExecuteQuery<bool>(config, query);
                }
            }

            return result;
        }

        public bool Row_Insert(object settings, string tablename, List<object[]> columnsList, List<object[]> valuesList, string[] primaryKey, bool update)
        {
            bool result = false;

            if (settings != null)
            {
                var config = SQLite_Configuration.Get(settings);
                if (config != null)
                {
                    var query = Row_Insert_CreateQuery(tablename, columnsList.ToArray(), valuesList.ToArray(), update);

                    result = (bool)ExecuteQuery<bool>(config, query);
                }
            }

            return result;
        }

        public bool Row_Insert(object settings, string query)
        {
            bool result = false;

            if (settings != null)
            {
                var config = SQLite_Configuration.Get(settings);
                if (config != null)
                {
                    result = (bool)ExecuteQuery<bool>(config, query);
                }
            }

            return result;
        }


        public DataRow Row_Get(object settings, string tablename, string tableKey, string rowKey)
        {
            DataRow result = null;

            if (settings != null)
            {
                var config = SQLite_Configuration.Get(settings);
                if (config != null)
                {
                    var query = "SELECT * FROM " + tablename + " WHERE " + tableKey + " = '" + rowKey + "'";

                    result = (DataRow)ExecuteQuery<DataRow>(config, query);
                }
            }

            return result;
        }

        public DataRow Row_Get(object settings, string tablename, string query)
        {
            DataRow result = null;

            if (settings != null)
            {
                var config = SQLite_Configuration.Get(settings);
                if (config != null)
                {
                    query = "SELECT * FROM " + tablename + " " + query;

                    result = (DataRow)ExecuteQuery<DataRow>(config, query);
                }
            }

            return result;
        }


        public bool Row_Exists(object settings, string tablename, string filterString)
        {
            bool result = false;

            if (settings != null)
            {
                var config = SQLite_Configuration.Get(settings);
                if (config != null)
                {
                    var query = "SELECT EXISTS(SELECT 1 FROM " + tablename + " WHERE " + filterString + " LIMIT 1);";

                    var val = (int)ExecuteQuery<int>(config, query);
                    if (val >= 0)
                    {
                        bool.TryParse(val.ToString(), out result);
                    }
                }
            }

            return result;
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
                    if (val.GetType() == typeof(DateTime)) val = ConvertToDateTime(val.ToString());

                    if (Values[x].ToString().ToLower() != "null") vals += "'" + ConvertToSafe(val.ToString()) + "'";
                    else vals += Values[x].ToString();
                }

                if (x < Values.Length - 1) vals += ", ";
            }

            //Create Update string
            //string update = "";
            //if (Update)
            //{
            //    update = " ON DUPLICATE KEY UPDATE ";
            //    for (int x = 0; x <= Columns.Length - 1; x++)
            //    {
            //        update += Columns[x].ToString().ToUpper();
            //        update += "=";

            //        object val = Values[x];
            //        if (val != null)
            //        {
            //            if (val.GetType() == typeof(DateTime)) val = ConvertToDateTime(val.ToString());

            //            update += "'" + ConvertToSafe(val.ToString()) + "'";
            //        }
            //        else update += "null";

            //        if (x < Columns.Length - 1) update += ", ";
            //    }
            //}

            //return "INSERT IGNORE INTO " + TableName + " (" + cols + ") VALUES (" + vals + ")" + update;

            return "INSERT OR REPLACE INTO " + TableName + " (" + cols + ") VALUES (" + vals + ")";
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
                        if (val.GetType() == typeof(DateTime)) val = ConvertToDateTime(val.ToString());

                        if (val.ToString().ToLower() != "null") vals += "'" + ConvertToSafe(val.ToString()) + "'";
                        else vals += val.ToString();
                    }


                    if (x < ValueSet.Count - 1) vals += ", ";
                }

                vals += ")";

                if (i < Values.Count - 1) vals += ",";

            }

            ////Create Update string
            //string update = "";
            //if (Update)
            //{
            //    update = " ON DUPLICATE KEY UPDATE ";
            //    for (int x = 0; x <= Columns.Length - 1; x++)
            //    {
            //        update += Columns[x].ToString().ToUpper();
            //        update += "=";

            //        update += "VALUES(" + Columns[x].ToString().ToUpper() + ")";
            //        if (x < Columns.Length - 1) update += ", ";
            //    }
            //}

            //return "INSERT IGNORE INTO " + TableName + " (" + cols + ") " + vals + update;

            return "INSERT OR REPLACE INTO " + TableName + " (" + cols + ") " + vals;
        }
    }
}
