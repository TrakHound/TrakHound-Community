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

        static string GetDatabaseName(Configuration config)
        {
            return config.Database;
        }

        public bool Database_Create(object settings)
        {
            bool result = false;

            if (settings != null)
            {
                var config = Configuration.Get(settings);
                if (config != null)
                {
                    var query = "IF (NOT EXISTS (SELECT name FROM master.sys.databases WHERE name = N'" + config.Database + "')) CREATE DATABASE " + config.Database;

                    result = (bool)ExecuteQuery<bool>(config, query, false);
                }
            }

            return result;
        }

        public bool Database_Drop(object settings)
        {
            bool result = false;

            if (settings != null)
            {
                var config = Configuration.Get(settings);
                if (config != null)
                {
                    var query = "IF (EXISTS (SELECT name FROM master.sys.databases WHERE name = N'" + config.Database + "')) DROP DATABASE " + config.Database;

                    result = (bool)ExecuteQuery<bool>(config, query, false);
                }
            }

            return result;
        }

    }
}
