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

        public static double GetDoubleFromRow(string key, DataRow row)
        {
            double result = 0;

            string val = null;
            if (row.Table.Columns.Contains(key)) if (row[key] != null) val = row[key].ToString();

            if (val != null) double.TryParse(val, out result);

            return result;
        }

        public static string GetTableValue(object table, string keyColumn, string key, string returnColumn)
        {
            string result = null;

            DataTable dt = table as DataTable;
            if (dt != null)
            {
                if (dt.Columns.Contains(keyColumn))
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

        public static void UpdateTableValue(object table, string keyColumn, string key, string valueColumn, string value)
        {
            DataTable dt = table as DataTable;
            if (dt != null)
            {
                DataRow[] rows = dt.Select(keyColumn + "='" + key + "'");
                if (rows.Length > 0)
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
                    row[valueColumn] = value;
                    dt.Rows.Add(row);
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
