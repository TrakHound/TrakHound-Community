// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
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

            string id = DeviceConfiguration.GenerateDatabaseId();
            string path = Path.Combine(FileLocations.Databases, Path.ChangeExtension(id, ".db"));

            DataTable_Functions.UpdateTableValue(dt, "address", "/Databases_Client/SQLite||00", "attributes", "id||00;");
            DataTable_Functions.UpdateTableValue(dt, "address", "/Databases_Client/SQLite||00/DatabasePath", "value", path);

            DataTable_Functions.UpdateTableValue(dt, "address", "/Databases_Server/SQLite||00", "attributes", "id||00;");
            DataTable_Functions.UpdateTableValue(dt, "address", "/Databases_Server/SQLite||00/DatabasePath", "value", path);
        }

    }
}
