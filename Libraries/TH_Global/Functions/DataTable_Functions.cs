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

        public static string GetTableValue(string key, DataTable dt)
        {
            string result = null;

            DataRow row = dt.Rows.Find(key);
            if (row != null)
            {
                result = row["value"].ToString();
            }

            return result;
        }

        public static string RemoveTableRow(string key, DataTable dt)
        {
            string result = null;

            DataRow row = dt.Rows.Find(key);
            if (row != null)
            {
                dt.Rows.Remove(row);
            }

            return result;
        }

        public static void UpdateTableValue(string value, string key, DataTable dt)
        {
            DataRow row = dt.Rows.Find(key);
            if (row != null)
            {
                row["value"] = value;
            }
            else
            {
                row = dt.NewRow();
                row["address"] = key;
                row["value"] = value;
                dt.Rows.Add(row);
            }
        }

        public static void UpdateTableValue(string value, string attributes, string key, DataTable dt)
        {
            DataRow row = dt.Rows.Find(key);
            if (row != null)
            {
                row["value"] = value;
            }
            else
            {
                row = dt.NewRow();
                row["address"] = key;
                row["value"] = value;
                row["attributes"] = attributes;
                dt.Rows.Add(row);
            }
        }

    }
}
