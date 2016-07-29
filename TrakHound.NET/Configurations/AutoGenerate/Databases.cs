// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.


using System.Data;
using System.IO;

using TrakHound.Tools;

namespace TrakHound.Configurations.AutoGenerate
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
            string path = Path.Combine(FileLocations.Databases, "TrakHound.db");

            DataTable_Functions.UpdateTableValue(dt, "address", prefix + "/SQLite||00", "attributes", "id||00;");
            DataTable_Functions.UpdateTableValue(dt, "address", prefix + "/SQLite||00/DatabasePath", "value", path);
        }

    }
}
