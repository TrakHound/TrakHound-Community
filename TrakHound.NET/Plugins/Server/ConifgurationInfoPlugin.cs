// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace TrakHound.Plugins.Server
{
    public static class ConfigurationInfoPlugin
    {
        public class PluginContainer : ReaderContainer
        {
            [ImportMany(typeof(IConfigurationInfo))]
            public IEnumerable<Lazy<object>> Plugins { get; set; }
        }
    }
}
