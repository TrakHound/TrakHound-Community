using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.SqlClient;

using TrakHound.Configurations;
using TrakHound.Databases;
using TrakHound;
using TrakHound.Logging;
using TrakHound.Tools;
using TrakHound.Plugins;
using TrakHound.Plugins.Database;

namespace TrakHound.Databases.Plugins.MySQL
{
    public partial class Plugin
    {

        public bool Row_Insert(object settings, string tablename, object[] columns, object[] values, string[] primaryKey, bool update)
        {
            bool result = false;

            if (settings != null)
            {
                var config = Configuration.Get(settings);
                if (config != null)
                {
                    var query = "";
                    var i = 0;

                    if (primaryKey != null)
                    {
                        query = "IF (EXISTS (SELECT 1 FROM " + tablename + " WHERE";

                        for (i = 0; i <= primaryKey.Length - 1; i++)
                        {
                            int keyIndex = columns.ToList().FindIndex(x => String_Functions.ToLower(x) == String_Functions.ToLower(primaryKey[i]));
                            if (keyIndex >= 0) query += " [" + primaryKey[i] + "] = " + ConvertValue(values[keyIndex]);

                            if (i < primaryKey.Length - 1) query += " AND ";
                        }

                        query += "))";

                        query += " BEGIN";
                        query += " UPDATE " + tablename + " SET";

                        i = 0;
                        for (i = 0; i <= columns.Length - 1; i++)
                        {
                            if (i <= values.Length - 1)
                            {
                                query += " [" + String_Functions.ToString(columns[i]) + "] = " + ConvertValue(values[i]);
                                if (i < columns.Length - 1) query += ",";
                            }
                            else throw new Exception();
                        }

                        query += " WHERE";
                        i = 0;
                        for (i = 0; i <= primaryKey.Length - 1; i++)
                        {
                            int keyIndex = columns.ToList().FindIndex(x => String_Functions.ToLower(x) == String_Functions.ToLower(primaryKey[i]));
                            if (keyIndex >= 0) query += " [" + primaryKey[i] + "] = " + ConvertValue(values[keyIndex]);

                            if (i < primaryKey.Length - 1) query += " AND ";
                        }

                        query += " END";
                        query += " ELSE";
                        query += " BEGIN";

                        query += " INSERT INTO " + tablename + "(";

                        i = 0;
                        for (i = 0; i <= columns.Length - 1; i++)
                        {
                            query += " [" + columns[i].ToString() + "]";
                            if (i < columns.Length - 1) query += ",";
                        }

                        query += ")";
                        query += " VALUES (";

                        i = 0;
                        for (i = 0; i <= values.Length - 1; i++)
                        {
                            query += ConvertValue(values[i]);
                            if (i < values.Length - 1) query += ",";
                        }

                        query += ")";
                        query += " END";
                    }
                    else
                    {
                        query += " INSERT INTO " + tablename + "(";

                        i = 0;
                        for (i = 0; i <= columns.Length - 1; i++)
                        {
                            query += " [" + columns[i].ToString() + "]";
                            if (i < columns.Length - 1) query += ",";
                        }

                        query += ")";
                        query += " VALUES (";

                        i = 0;
                        for (i = 0; i <= values.Length - 1; i++)
                        {
                            query += ConvertValue(values[i]);
                            if (i < values.Length - 1) query += ",";
                        }

                        query += ")";
                    }

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
                var config = Configuration.Get(settings);
                if (config != null)
                {
                    var query = "";

                    for (var m = 0; m <= valueList.Count - 1; m++)
                    {
                        var values = valueList[m].ToArray();
                        var i = 0;

                        if (primaryKey != null)
                        {
                            query += "IF (EXISTS (SELECT 1 FROM " + tablename + " WHERE";

                            for (i = 0; i <= primaryKey.Length - 1; i++)
                            {
                                int keyIndex = columns.ToList().FindIndex(x => String_Functions.ToLower(x) == String_Functions.ToLower(primaryKey[i]));
                                if (keyIndex >= 0) query += " [" + primaryKey[i] + "] = " + ConvertValue(values[keyIndex]);

                                if (i < primaryKey.Length - 1) query += " AND ";
                            }

                            query += "))";

                            query += " BEGIN";
                            query += " UPDATE " + tablename + " SET";

                            i = 0;
                            for (i = 0; i <= columns.Length - 1; i++)
                            {
                                if (i <= values.Length - 1)
                                {
                                    query += " [" + String_Functions.ToString(columns[i]) + "] = " + ConvertValue(values[i]);

                                    if (i < columns.Length - 1) query += ",";
                                }
                                else throw new Exception();
                            }

                            query += " WHERE";
                            i = 0;
                            for (i = 0; i <= primaryKey.Length - 1; i++)
                            {
                                int keyIndex = columns.ToList().FindIndex(x => String_Functions.ToLower(x) == String_Functions.ToLower(primaryKey[i]));
                                if (keyIndex >= 0) query += " [" + primaryKey[i] + "] = " + ConvertValue(values[keyIndex]);

                                if (i < primaryKey.Length - 1) query += " AND ";
                            }

                            query += " END";
                            query += " ELSE";
                            query += " BEGIN";

                            query += " INSERT INTO " + tablename + "(";

                            i = 0;
                            for (i = 0; i <= columns.Length - 1; i++)
                            {
                                query += " [" + columns[i].ToString() + "]";
                                if (i < columns.Length - 1) query += ",";
                            }

                            query += ")";
                            query += " VALUES (";

                            i = 0;
                            for (i = 0; i <= values.Length - 1; i++)
                            {
                                query += ConvertValue(values[i]);
                                if (i < values.Length - 1) query += ",";
                            }

                            query += ")";
                            query += " END; ";
                        }
                        else
                        {
                            query += " INSERT INTO " + tablename + "(";

                            for (i = 0; i <= columns.Length - 1; i++)
                            {
                                query += " [" + columns[i].ToString() + "]";
                                if (i < columns.Length - 1) query += ",";
                            }

                            query += ")";
                            query += " VALUES (";

                            i = 0;
                            for (i = 0; i <= values.Length - 1; i++)
                            {
                                query += ConvertValue(values[i]);
                                if (i < values.Length - 1) query += ",";
                            }

                            query += ");";
                        }
                    }

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
                var config = Configuration.Get(settings);
                if (config != null)
                {
                    var query = "";

                    for (var m = 0; m <= columnsList.Count - 1; m++)
                    {
                        var columns = columnsList[m].ToArray();
                        var values = valuesList[m].ToArray();

                        var i = 0;

                        if (primaryKey != null)
                        {
                            query += "IF (EXISTS (SELECT 1 FROM " + tablename + " WHERE";

                            for (i = 0; i <= primaryKey.Length - 1; i++)
                            {
                                int keyIndex = columns.ToList().FindIndex(x => String_Functions.ToLower(x) == String_Functions.ToLower(primaryKey[i]));
                                if (keyIndex >= 0) query += " [" + primaryKey[i] + "] = " + ConvertValue(values[keyIndex]);

                                if (i < primaryKey.Length - 1) query += " AND ";
                            }

                            query += "))";

                            query += " BEGIN";
                            query += " UPDATE " + tablename + " SET";

                            i = 0;
                            for (i = 0; i <= columns.Length - 1; i++)
                            {
                                if (i <= values.Length - 1)
                                {
                                    query += " [" + String_Functions.ToString(columns[i]) + "] = " + ConvertValue(values[i]);

                                    if (i < columns.Length - 1) query += ",";
                                }
                                else throw new Exception();
                            }

                            query += " WHERE";
                            i = 0;
                            for (i = 0; i <= primaryKey.Length - 1; i++)
                            {
                                int keyIndex = columns.ToList().FindIndex(x => String_Functions.ToLower(x) == String_Functions.ToLower(primaryKey[i]));
                                if (keyIndex >= 0) query += " [" + primaryKey[i] + "] = " + ConvertValue(values[keyIndex]);

                                if (i < primaryKey.Length - 1) query += " AND ";
                            }

                            query += " END";
                            query += " ELSE";
                            query += " BEGIN";

                            query += " INSERT INTO " + tablename + "(";

                            i = 0;
                            for (i = 0; i <= columns.Length - 1; i++)
                            {
                                query += " [" + columns[i].ToString() + "]";
                                if (i < columns.Length - 1) query += ",";
                            }

                            query += ")";
                            query += " VALUES (";

                            i = 0;
                            for (i = 0; i <= values.Length - 1; i++)
                            {
                                query += ConvertValue(values[i]);
                                if (i < values.Length - 1) query += ",";
                            }

                            query += ")";
                            query += " END;";
                        }
                        else
                        {
                            query += " INSERT INTO " + tablename + "(";

                            for (i = 0; i <= columns.Length - 1; i++)
                            {
                                query += " [" + columns[i].ToString() + "]";
                                if (i < columns.Length - 1) query += ",";
                            }

                            query += ")";
                            query += " VALUES (";

                            i = 0;
                            for (i = 0; i <= values.Length - 1; i++)
                            {
                                query += ConvertValue(values[i]);
                                if (i < values.Length - 1) query += ",";
                            }

                            query += "); ";
                        }
                    }

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
                var config = Configuration.Get(settings);
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
                var config = Configuration.Get(settings);
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
                var config = Configuration.Get(settings);
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
                var config = Configuration.Get(settings);
                if (config != null)
                {
                    var query = "IF (EXISTS (SELECT 1 FROM " + tablename + " " + filterString + "))";
                    query += " BEGIN";
                    query += " PRINT '1'";
                    query += " END";
                    query += " ELSE";
                    query += " BEGIN";
                    query += " PRINT '0'";
                    query += " END";

                    var val = (int)ExecuteQuery<int>(config, query);
                    if (val >= 0)
                    {
                        bool.TryParse(val.ToString(), out result);
                    }
                }
            }

            return result;
        }

    }
}
