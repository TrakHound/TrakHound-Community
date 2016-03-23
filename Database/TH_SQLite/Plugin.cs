using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.SQLite;

using TH_Configuration;
using TH_Database;
using TH_Global;
using TH_Global.Functions;

namespace TH_SQLite
{
    public partial class Plugin : IDatabasePlugin
    {

        public string Name { get { return "SQLite Database Plugin"; } }

        public string Type { get { return "SQLite"; } }


        public Type Config_Page { get { return typeof(ConfigurationPage.Page); } }

        public object CreateConfigurationButton(DataTable dt)
        {
            ConfigurationPage.Button result = new ConfigurationPage.Button();

            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    result.DatabasePath = GetTableValue("database_path", dt);
                }
            }

            return result;
        }

        string GetTableValue(string name, DataTable dt)
        {
            string result = null;

            DataRow[] rows = dt.Select("Name = '" + name + "'");
            if (rows.Length > 0)
            {
                result = rows[0]["Value"].ToString();
            }

            return result;
        }


        public void Initialize(Database_Configuration config)
        {
            var c = SQLite_Configuration.ReadXML(config.Node);

            config.UniqueId = GetUniqueId(c);
            config.Configuration = c;

            Logger.Log(Type + " Successfully Initialized : " + GetDatabasePath(c));
        }

        public bool Ping(object settings, out string msg)
        {
            bool result = false;

            msg = null;

            if (settings != null)
            {
                var config = SQLite_Configuration.Get(settings);
                if (config != null)
                {
                    if (System.IO.File.Exists(GetDatabasePath(config))) result = true;
                }
            }

            return result;
        }

        public bool CheckPermissions(object settings, Application_Type type)
        {
            return true;
        }

        private static string GetUniqueId(object settings)
        {
            if (settings != null)
            {
                var config = SQLite_Configuration.Get(settings);
                if (config != null)
                {
                    return config.DatabasePath;
                }
            }
            return null;
        }


        static string GetConnectionString(SQLite_Configuration config)
        {
            return "Data Source=" + GetDatabasePath(config) + "; Version=3; Pooling=True; Max Pool Size=150";
        }

        static object ExecuteQuery<T>(SQLite_Configuration config, string query)
        {
            object result = null;

            using (var connection = new SQLiteConnection(GetConnectionString(config)))
            {
                try
                {
                    connection.Open();

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        //command.ExecuteNonQuery();

                        result = ProcessResult<T>(command);
                    }
                }
                catch (SQLiteException sqex)
                {
                    Logger.Log("SQLiteException :: " + sqex.Message);
                    if (typeof(T) == typeof(bool)) result = false;
                }
                catch (Exception ex)
                {
                    Logger.Log("Exception :: " + ex.Message);
                    if (typeof(T) == typeof(bool)) result = false;
                }
                finally
                {
                    connection.Close();
                }
            }
            
            return result;
        }

        static object ProcessResult<T>(SQLiteCommand command)
        {
            object result = null;

            // Object
            if (typeof(T) == typeof(object))
            {
                result = command.ExecuteScalar();
            }

            // Boolean
            if (typeof(T) == typeof(bool))
            {
                object o = command.ExecuteNonQuery();
                var val = (int)(-1);
                if (o != null) int.TryParse(o.ToString(), out val);

                if (val >= 0) result = true;
                else result = false;
            }

            // int
            if (typeof(T) == typeof(int))
            {
                object o = command.ExecuteScalar();
                var val = (int)(-1);
                if (o != null) int.TryParse(o.ToString(), out val);
                result = val;
            }

            // Int64
            if (typeof(T) == typeof(Int64))
            {
                object o = command.ExecuteScalar();
                var val = (Int64)(-1);
                if (o != null) Int64.TryParse(o.ToString(), out val);
                result = val;
            }

            // string
            if (typeof(T) == typeof(string))
            {
                object o = command.ExecuteScalar();
                if (o != null) result = o.ToString();
            }

            // DataTable
            if (typeof(T) == typeof(DataTable))
            {
                var dt = new DataTable();
                using (var a = new SQLiteDataAdapter(command))
                {
                    a.Fill(dt);
                }
                result = dt;
            }

            // DataRow
            if (typeof(T) == typeof(DataRow))
            {
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        int columnCount = reader.FieldCount;

                        DataTable temp_dt = new DataTable();

                        for (int x = 0; x <= columnCount - 1; x++)
                        {
                            temp_dt.Columns.Add(reader.GetName(x));
                        }

                        DataRow row = temp_dt.NewRow();

                        object[] values = new object[columnCount];

                        while (reader.Read())
                        {
                            for (int j = 0; j <= values.Length - 1; j++)
                            {
                                values[j] = reader[j];
                            }
                        }

                        row.ItemArray = values;

                        result = row;
                    }
                }
            }

            // string[]
            if (typeof(T) == typeof(string[]))
            {
                var tables = new List<string>();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read()) tables.Add(reader[0].ToString());
                    }
                }
                result = tables.ToArray();
            }

            // List<string>
            if (typeof(T) == typeof(List<string>))
            {
                var tables = new List<string>();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read()) tables.Add(reader[0].ToString());
                    }
                }
                result = tables;
            }

            return result;
        }


        public const string dateString = "yyyy-MM-dd H:mm:ss";

        public static string ConvertToDateTime(string s)
        {
            string result = "null";

            DateTime TS;
            if (DateTime.TryParse(s, out TS)) result = TS.ToString(dateString);

            return result;
        }

        public static string COLUMN_NAME_START = "`";
        public static string COLUMN_NAME_END = "`";

        public static object[] ConvertColumnDefinitions(ColumnDefinition[] columns)
        {
            List<object> result = new List<object>();

            foreach (ColumnDefinition coldef in columns.ToList())
            {
                string def = ConvertColumnDefinition(coldef);
                if (coldef.NotNull) def += " NOT NULL";

                result.Add(def);
            }

            return result.ToArray();
        }

        public static string ConvertColumnDefinition(ColumnDefinition column)
        {
            return COLUMN_NAME_START + column.ColumnName + COLUMN_NAME_END + " " + ConvertColumnDataType(column.DataType);
        }

        public const string VarChar = "varchar(1000)";
        public const string BigInt = "bigint";
        public const string Double = "double";
        public const string Datetime = "varchar(90)";
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

        static string ConvertValue(object o)
        {
            if (o != null)
            {
                var val = o.ToString();
                if (o.GetType() == typeof(DateTime)) val = ConvertToDateTime(val);

                val = "'" + val + "'";

                return val;
            }
            else
            {
                return "null";
            }
        }

        static string ConvertToSafe(string s)
        {
            string r = s;
            if (r.Contains("'")) r = r.Replace("'", "\'");
            return r;
        }

    }
}
