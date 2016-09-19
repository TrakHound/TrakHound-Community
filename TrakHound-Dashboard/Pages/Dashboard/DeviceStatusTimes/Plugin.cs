// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using TrakHound;
using TrakHound.Plugins;
using TrakHound.Plugins.Client;
using TrakHound.Tools;

namespace TrakHound_Dashboard.Pages.Dashboard.DeviceStatusTimes
{
    public partial class DeviceStatusTimes : IClientPlugin
    {

        #region "Descriptive"

        public string Title { get { return "Device Status Times"; } }

        public string Description { get { return "View Device Status Times and Percentages for the current day"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TrakHound-Dashboard;component/Resources/Status_Percentage_01.png")); } }

        #endregion

        #region "Plugin Properties/Options"

        public string ParentPlugin { get { return "Dashboard"; } }
        public string ParentPluginCategory { get { return "Pages"; } }

        public bool OpenOnStartUp { get { return true; } }

        public List<PluginConfigurationCategory> SubCategories { get; set; }

        public List<IClientPlugin> Plugins { get; set; }

        #endregion

        #region "Methods"

        public void Initialize() { }

        public bool Opening() { return true; }

        public void Opened() { }

        public bool Closing() { return true; }

        public void Closed() { }

        #endregion

        #region "Events"

        public void GetSentData(EventData data)
        {
            Update(data);

            Dispatcher.BeginInvoke(new Action<EventData>(UpdateDeviceAdded), UI_Functions.PRIORITY_DATA_BIND, new object[] { data });
            Dispatcher.BeginInvoke(new Action<EventData>(UpdateDeviceUpdated), UI_Functions.PRIORITY_DATA_BIND, new object[] { data });
            Dispatcher.BeginInvoke(new Action<EventData>(UpdateDeviceRemoved), UI_Functions.PRIORITY_DATA_BIND, new object[] { data });
        }

        public event SendData_Handler SendData;

        #endregion
        
        public IPage Options { get; set; }

    }
}
