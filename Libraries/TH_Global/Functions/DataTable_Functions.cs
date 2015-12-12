using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static string GetTableValue(object table, string keyColumn, string key, string returnColumn)
        {
            string result = null;

            DataTable dt = table as DataTable;
            if (dt != null)
            {
                DataView dv = dt.AsDataView();
                dv.RowFilter = keyColumn + "='" + key + "'";
                DataTable temp_dt = dv.ToTable(false, returnColumn);
                if (temp_dt != null)
                {
                    if (temp_dt.Rows.Count > 0)
                    {
                        result = temp_dt.Rows[0][0].ToString();
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
                DataView dv = dt.AsDataView();
                dv.RowFilter = keyColumn + "='" + key + "'";
                DataTable temp_dt = dv.ToTable();
                foreach (DataRow row in temp_dt.Rows) result.Add(row);
            }

            return result;
        }


        //public static string GetTableValue(string key, DataTable dt)
        //{
        //    string result = null;

        //    DataRow row = dt.Rows.Find(key);
        //    if (row != null)
        //    {
        //        result = row["value"].ToString();
        //    }

        //    return result;
        //}

        //public static string RemoveTableRow(string key, DataTable dt)
        //{
        //    string result = null;

        //    DataRow row = dt.Rows.Find(key);
        //    if (row != null)
        //    {
        //        dt.Rows.Remove(row);
        //    }

        //    return result;
        //}

        //public static void UpdateTableValue(string value, string key, DataTable dt)
        //{
        //    DataRow row = dt.Rows.Find(key);
        //    if (row != null)
        //    {
        //        row["value"] = value;
        //    }
        //    else
        //    {
        //        row = dt.NewRow();
        //        row["address"] = key;
        //        row["value"] = value;
        //        dt.Rows.Add(row);
        //    }
        //}

        //public static void UpdateTableValue(string value, string attributes, string key, DataTable dt)
        //{
        //    DataRow row = dt.Rows.Find(key);
        //    if (row != null)
        //    {
        //        row["value"] = value;
        //    }
        //    else
        //    {
        //        row = dt.NewRow();
        //        row["address"] = key;
        //        row["value"] = value;
        //        row["attributes"] = attributes;
        //        dt.Rows.Add(row);
        //    }
        //}

    }
}
