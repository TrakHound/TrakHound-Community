// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using System;

using TrakHound.Plugins.Server;

namespace TrakHound_Dashboard.Pages.DeviceManager.Pages.Cycles
{
    //public class Info : IConfigurationInfo
    public class Info
    {
        public string Title { get { return "Cycles"; } }

        public Uri Image { get { return new Uri("pack://application:,,,/TrakHound-Dashboard;component/Resources/Cycle_01.png"); } }

        public Type ConfigurationPageType { get { return typeof(Page); } }
    }
}
