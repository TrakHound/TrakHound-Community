using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrakHound_DataManagement
{
    public static class Connection
    {

        public static string GetConnectionString(Configuration config)
        {
            return "Data Source=" + Database.GetPath(config) + "; Version=3; Pooling=True; Max Pool Size=300;";
        }

    }
}
