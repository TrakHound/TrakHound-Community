// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace TrakHound.DataManagement
{
    public static class Connection
    {

        public static string GetConnectionString(Configuration config)
        {
            return "Data Source=" + Database.GetPath(config) + "; Version=3; Pooling=True; Max Pool Size=300;";
        }

    }
}
