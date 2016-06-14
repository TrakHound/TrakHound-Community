// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Data;

using TH_Global.Functions;
using TH_Plugins.Database;

namespace TH_MySQL_Config
{
    public class Info : IConfigurationInfo
    {

        public string Type { get { return "MySQL"; } }

        public Type ConfigurationPageType { get { return typeof(Page); } }

        public object CreateConfigurationButton(DataTable dt)
        {
            var result = new Button();

            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    result.DatabaseName = DataTable_Functions.GetTableValue(dt, "address", "Database", "value");
                    result.Server = DataTable_Functions.GetTableValue(dt, "address", "Server", "value");
                }
            }

            return result;
        }

    }
}
