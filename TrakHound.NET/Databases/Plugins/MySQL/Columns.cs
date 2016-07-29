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

        public List<string> Column_Get(object settings, string tablename)
        {
            List<string> result = null;

            if (settings != null)
            {
                var config = Configuration.Get(settings);
                if (config != null)
                {
                    var query = "SELECT COLUMN_NAME FROM " + config.Database + ".INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + tablename + "'";

                    result = (List<string>)ExecuteQuery<List<string>>(config, query);
                }
            }

            return result;
        }

        public bool Column_Add(object settings, string tablename, ColumnDefinition columnDefinition)
        {
            bool result = false;

            if (settings != null)
            {
                var config = Configuration.Get(settings);
                if (config != null)
                {
                    var query = " IF (NOT EXISTS (SELECT * FROM sys.columns" +
                                 " WHERE Name = N'" + columnDefinition.ColumnName + "' AND Object_ID = Object_ID(N'" + tablename + "')))" +
                                 " ALTER TABLE " + tablename + " ADD " + ConvertColumnDefinition(columnDefinition);


                    result = (bool)ExecuteQuery<bool>(config, query);
                }
            }

            return result;
        }

    }
}
