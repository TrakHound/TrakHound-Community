// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using TrakHound.Plugins.Server;

namespace TrakHound_Dashboard.Pages.DeviceManager.Pages.InstanceData
{
    public class Info : IConfigurationInfo
    {
        public string Title { get { return "Instance Data"; } }

        public Uri Image { get { return new Uri("pack://application:,,,/TrakHound-Dashboard;component/Resources/Hourglass_01.png"); } }

        public Type ConfigurationPageType { get { return typeof(Page); } }
    }
}
