// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace TH_Plugins.Database
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
