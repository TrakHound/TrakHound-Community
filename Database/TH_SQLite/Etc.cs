using System.Data;

using TH_Database;

namespace TH_SQLite
{
    public partial class Plugin
    {

        public string CustomCommand(object settings, string query)
        {
            string result = null;

            if (settings != null)
            {
                var config = SQLite_Configuration.Get(settings);
                if (config != null)
                {
                    result = (string)ExecuteQuery<string>(config, query);
                }
            }

            return result;
        }

        public object GetValue(object settings, string tablename, string column, string filterExpression)
        {
            object result = null;

            if (settings != null)
            {
                var config = SQLite_Configuration.Get(settings);
                if (config != null)
                {
                    var query = "SELECT " + column + " FROM " + tablename + " " + filterExpression;

                    result = (object)ExecuteQuery<object>(config, query);
                }
            }

            return result;
        }

        public DataTable GetGrants(object settings)
        {
            DataTable result = null;

            return result;
        }

    }
}
