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

        public string CustomCommand(object settings, string query)
        {
            string result = null;

            if (settings != null)
            {
                var config = Configuration.Get(settings);
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
                var config = Configuration.Get(settings);
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
