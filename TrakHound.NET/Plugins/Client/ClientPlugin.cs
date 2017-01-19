// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace TrakHound.Plugins.Client
{
    public static class ClientPlugin
    {
        public class PluginContainer : ReaderContainer
        {
            [ImportMany(typeof(IClientPlugin))]
            public IEnumerable<Lazy<object>> Plugins { get; set; }
        }
    }

    public class PluginCategory
    {
        public string Name { get; set; }
        public List<IClientPlugin> Plugins { get; set; }
    }
}
