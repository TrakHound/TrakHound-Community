// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;

namespace TH_Global.Functions
{
    public static class DataTable_Functions
    {

        public static string GetRowValue(string column, DataRow row)
        {
            string result = null;

            if (row.Table.Columns.Contains(column))
            {
                if (row[column] != null)
                {
                    result = row[column].ToString();
                }
            }

            return result;
        }

        public static bool GetBooleanFromRow(string key, DataRow row)
        {
            bool result = false;

            string val = null;
            if (row.Table.Columns.Contains(key)) if (row[key] != null) val = row[key].ToString();

            if (val != null) bool.TryParse(val, out result);

            return result;
        }

        public static double GetDoubleFromRow(string key, DataRow row)
        {
            double result = 0;

            string val = null;
            if (row.Table.Columns.Contains(key)) if (row[key] != null) val = row[key].ToString();

            if (val != null) double.TryParse(val, out result);

            return result;
        }

        public static DateTime GetDateTimeFromRow(string key, DataRow row)
        {
            DateTime result = DateTime.MinValue;

            string val = null;
            if (row.Table.Columns.Contains(key)) if (row[key] != null) val = row[key].ToString();

            if (val != null) DateTime.TryParse(val, out result);

            return result;
        }

        public static bool RowExists(DataTable dt, string keyColumn, string key)
        {
            bool result = false;

            if (dt.Columns.Contains(keyColumn))
            {
                string filter = keyColumn + "='" + key + "'";
                var rows = GetRows(dt, filter);
                if (rows != null && rows.Length > 0) result = true;
            }

            return result;
        }

        public static string GetTableValue(object table, string keyColumn, string key, string returnColumn)
        {
            string result = null;

            DataTable dt = table as DataTable;
            if (dt != null)
            {
                if (dt.Columns.Contains(keyColumn) && dt.Columns.Contains(returnColumn))
                {
                    string filter = keyColumn + "='" + key + "'";
                    var rows = GetRows(dt, filter);
                    if (rows != null)
                    {
                        if (rows.Length > 0)
                        {
                            var o = rows[0][returnColumn];

                            result = o.ToString();
                        }
                    }
                }
            }

            return result;
        }

        public static bool GetBooleanTableValue(object table, string keyColumn, string key, string returnColumn)
        {
            bool result = false;

            DataTable dt = table as DataTable;
            if (dt != null)
            {
                if (dt.Columns.Contains(keyColumn))
                {
                    string filter = keyColumn + "='" + key + "'";
                    var rows = GetRows(dt, filter);
                    if (rows != null)
                    {
                        if (rows.Length > 0)
                        {
                            result = GetBooleanFromRow(returnColumn, rows[0]);
                        }
                    }
                }
            }

            return result;
        }

        public static DateTime GetDateTimeTableValue(object table, string keyColumn, string key, string returnColumn)
        {
            DateTime result = DateTime.MinValue;

            DataTable dt = table as DataTable;
            if (dt != null)
            {
                if (dt.Columns.Contains(keyColumn))
                {
                    string filter = keyColumn + "='" + key + "'";
                    var rows = GetRows(dt, filter);
                    if (rows != null)
                    {
                        if (rows.Length > 0)
                        {
                            result = GetDateTimeFromRow(returnColumn, rows[0]);
                        }
                    }
                }
            }

            return result;
        }

        public static string GetTableValue(object table, string filter, string returnColumn)
        {
            string result = null;

            DataTable dt = table as DataTable;
            if (dt != null)
            {
                var rows = GetRows(dt, filter);
                if (rows != null)
                {
                    if (rows.Length > 0)
                    {
                        result = rows[0][returnColumn].ToString();
                    }
                }
            }

            return result;
        }

        public static List<DataRow> GetTableRows(object table, string keyColumn, string key)
        {
            List<DataRow> result = new List<DataRow>();

            DataTable dt = table as DataTable;
            if (dt != null)
            {
                //DataView dv = dt.AsDataView();
                //dv.RowFilter = keyColumn + "='" + key + "'";
                //DataTable temp_dt = dv.ToTable();


                string filter = keyColumn + "='" + key + "'";
                return GetRows(dt, filter).ToList();


                //foreach (DataRow row in temp_dt.Rows) result.Add(row);
                //temp_dt.Dispose();
            }

            return result;
        }

        public static void UpdateTableValue(object table, string keyColumn, string key, string valueColumn, string value)
        {
            DataTable dt = table as DataTable;
            if (dt != null)
            {
                DataRow[] rows = dt.Select(keyColumn + "='" + key + "'");
                if (rows.Length > 0 && valueColumn != null)
                {
                    foreach (var row in rows)
                    {
                        if (row.Table.Columns.Contains(valueColumn))
                        {
                            row[valueColumn] = value;
                        }
                    }
                }
                else
                {
                    DataRow row = dt.NewRow();
                    row[keyColumn] = key;
                    if (valueColumn != null) row[valueColumn] = value;
                    dt.Rows.Add(row);
                }
            }
        }

        public static string[] GetDistinctValues(DataTable table, string columnName)
        {
            return table.AsEnumerable().Select(x => x.Field<string>(columnName)).Distinct().ToArray();
        }

        public static DataRow[] GetRows(DataTable table, string filter)
        {
            return table.Select(filter);
        }


        public static class TrakHound
        {
            public const string ATTRIBUTE_ID_DELIMITER = "||";
            public const string ATTRIBUTE_ID_EOL = ";";

            public static string GetRowAttribute(string name, DataRow row)
            {
                string attributes = row["attributes"].ToString();

                if (attributes.Contains(name))
                {
                    int index = GetIndexOfId(name, attributes);
                    if (index >= 0)
                    {
                        int b = attributes.IndexOf(ATTRIBUTE_ID_DELIMITER, index) + 2;
                        int c = attributes.IndexOf(ATTRIBUTE_ID_EOL, index);

                        if (b >= 0 && (c - b) > 0)
                        {
                            return attributes.Substring(b, c - b);
                        }
                    }
                }

                return null;
            }

            private static int GetIndexOfId(string id, string attributes)
            {
                int result = -1;

                int start = 0;

                while (result < 0 && start >= 0)
                {
                    start = attributes.IndexOf(id, start);
                    if (start >= 0 && start < attributes.Length - 2)
                    {
                        // If next characters are the id delimiter
                        if (attributes.Substring(start + id.Length, 2) == ATTRIBUTE_ID_DELIMITER)
                        {
                            result = start;
                        }
                        else start++;
                    }
                }

                return result;
            }

            public static int GetUnusedAddressId(string prefix, DataTable dt)
            {
                int id = 0;
                string adr = prefix + "||";
                string test = adr + id.ToString("00");

                if (dt != null)
                {
                    while (DataTable_Functions.RowExists(dt, "address", test))
                    {
                        id += 1;
                        test = adr + id.ToString("00");
                    }
                }

                return id;
            }

            /// <summary>
            /// Get the last node in the Address column. Returns just the name and omits any Id's.
            /// </summary>
            /// <param name="row"></param>
            /// <returns></returns>
            public static string GetLastNode(DataRow row)
            {
                string result = null;

                string adr = row["address"].ToString();

                if (adr.Contains('/'))
                {
                    string s = adr;

                    // Remove Last forward slash
                    if (s[s.Length - 1] == '/') s = s.Substring(0, s.Length - 1);

                    // Get index of last forward slash
                    int slashIndex = s.LastIndexOf('/') + 1;
                    if (slashIndex < s.Length) s = s.Substring(slashIndex);

                    // Remove Id
                    if (s.Contains("||"))
                    {
                        int separatorIndex = s.LastIndexOf("||");
                        s = s.Substring(0, separatorIndex);
                    }

                    result = s;
                }

                return result;
            }

            public static void DeleteRows(string like, string likeColumn, DataTable dt)
            {
                if (dt.Columns.Contains(likeColumn))
                {
                    string filter = likeColumn + " LIKE '" + like + "'";
                    DataView dv = dt.AsDataView();
                    dv.RowFilter = filter;
                    DataTable temp_dt = dv.ToTable();
                    if (temp_dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in temp_dt.Rows)
                        {
                            DataRow dbRow = dt.Rows.Find(row[likeColumn]);
                            if (dbRow != null) dt.Rows.Remove(dbRow);
                        }
                    }
                }
            }
        }


        public static void WriteRowstoConsole(string Title, DataTable DT)
        {

            Console.WriteLine(Title + " ------------------------------------------------------");

            string Columns = "";
            foreach (DataColumn Col in DT.Columns)
                Columns += Col.ColumnName + " : ";

            Console.WriteLine("Columns ::: " + Columns);

            foreach (DataRow Row in DT.Rows)
            {
                string Display = "";
                for (int x = 0; x <= Row.ItemArray.Length - 1; x++)
                {
                    Display += Row.ItemArray[x].ToString() + " : ";
                }
                Console.WriteLine(Display);

            }

            Console.WriteLine(" --------------------------------------------------------------");

        }

        public static void WriteRowValuesToConsole(object[] ItemArray)
        {
            string full = "";
            for (int x = 0; x <= ItemArray.Length - 1; x++)
                full += ItemArray[x].ToString() + " : ";

            Console.WriteLine(full);

        }

    }
}
