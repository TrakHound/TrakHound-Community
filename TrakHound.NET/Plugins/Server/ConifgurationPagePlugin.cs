// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace TrakHound.Plugins.Server
{
    public static class ConfigurationPagePlugin
    {
        public class PluginContainer : ReaderContainer
        {
            [ImportMany(typeof(IConfigurationPage))]
            public IEnumerable<Lazy<object>> Plugins { get; set; }
        }
    }
}
