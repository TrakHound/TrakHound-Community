// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

using TrakHound.Plugins.Server;

namespace TrakHound_Dashboard.Pages.DeviceManager.Pages.Description
{
    public class Info : IConfigurationInfo
    {
        public string Title { get { return "Description"; } }

        public Uri Image { get { return new Uri("pack://application:,,,/TrakHound-Dashboard;component/Resources/About_01.png"); } }

        public Type ConfigurationPageType { get { return typeof(Page); } }
    }
}
