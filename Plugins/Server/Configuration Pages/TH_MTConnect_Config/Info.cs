// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

using TH_Plugins.Server;

namespace TH_MTConnect_Config
{
    public class Info : IConfigurationInfo
    {

        public Type ConfigurationPageType { get { return typeof(Page); } }

    }
}
