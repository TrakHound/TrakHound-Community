// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace TH_Plugins.Server
{
    public static class ServerPlugin
    {

        public const string PLUGIN_EXTENSION = ".splugin";

        public class PluginContainer : ReaderContainer
        {
            [ImportMany(typeof(IServerPlugin))]
            public IEnumerable<Lazy<object>> Plugins { get; set; }
        }

    }
}
