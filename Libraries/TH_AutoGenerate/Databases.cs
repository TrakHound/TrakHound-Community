using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;

using TH_Configuration;
using TH_Global;
using TH_Global.Functions;

using MTConnect.Application.Components;

namespace TH_AutoGenerate
{
    public static class Databases
    {

        public static void Add(DataTable dt)
        {
            DataTable_Functions.UpdateTableValue(dt, "address", "/DatabaseId", "value", DeviceConfiguration.GenerateDatabaseId());

            AddDatabaseConfiguration("/Databases_Client", dt);
            AddDatabaseConfiguration("/Databases_Server", dt);
        }

        private static void AddDatabaseConfiguration(string prefix, DataTable dt)
        {
            string path = FileLocations.Databases + "\\TrakHound.db";

            DataTable_Functions.UpdateTableValue(dt, "address", prefix + "/SQLite||00", "attributes", "id||00;");
            DataTable_Functions.UpdateTableValue(dt, "address", prefix + "/SQLite||00/DatabasePath", "value", path);
        }

    }
}
