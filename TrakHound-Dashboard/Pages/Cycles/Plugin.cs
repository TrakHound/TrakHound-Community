using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using TrakHound;
using TrakHound.Configurations;
using TrakHound.Plugins;
using TrakHound.Plugins.Client;
using TrakHound.Tools;
using TrakHound_UI;

namespace TrakHound_Dashboard.Pages.Cycles
{
    public partial class Plugin: IClientPlugin
    {

        #region "Descriptive"

        public string Title { get { return "Cycles"; } }

        public string Description { get { return null; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TrakHound-Dashboard;component/Resources/Cycle_01.png")); } }


        public string Author { get { return "TrakHound"; } }

        public string AuthorText { get { return "©2015 Feenux LLC. All Rights Reserved"; } }

        public ImageSource AuthorImage { get { return null; } }


        public string LicenseName { get { return "GPLv3"; } }

        public string LicenseText { get { return null; } }

        #endregion

        #region "Update Information"

        public string UpdateFileURL { get { return null; } }

        #endregion

        #region "Plugin Properties/Options"

        public string ParentPlugin { get { return null; } }
        public string ParentPluginCategory { get { return null; } }

        public bool AcceptsPlugins { get { return false; } }

        public bool OpenOnStartUp { get { return false; } }

        public bool ShowInAppMenu { get { return false; } }

        public List<PluginConfigurationCategory> SubCategories { get; set; }

        public List<IClientPlugin> Plugins { get; set; }

        #endregion

        #region "Methods"

        public void Initialize() { }

        public void Opened() { }
        public bool Opening() { return true; }

        public void Closed() { }
        public bool Closing() { return true; }

        #endregion

        #region "Events"

        public void GetSentData(EventData data)
        {
            this.Dispatcher.BeginInvoke(new Action<EventData>(Update), UI_Functions.PRIORITY_BACKGROUND, new object[] { data });

            this.Dispatcher.BeginInvoke(new Action<EventData>(UpdateDeviceAdded), UI_Functions.PRIORITY_BACKGROUND, new object[] { data });
            this.Dispatcher.BeginInvoke(new Action<EventData>(UpdateDeviceUpdated), UI_Functions.PRIORITY_BACKGROUND, new object[] { data });
            this.Dispatcher.BeginInvoke(new Action<EventData>(UpdateDeviceRemoved), UI_Functions.PRIORITY_BACKGROUND, new object[] { data });
        }

        public event SendData_Handler SendData;

        #endregion

        #region "Device Properties"

        private void AddDevice(DeviceConfiguration config)
        {
            var bt = new ListButton();
            bt.DataObject = config;
            bt.Selected += Device_Selected;
            DeviceList.Add(bt);
        }

        private void Device_Selected(ListButton bt)
        {
            if (bt.DataObject != null)
            {
                 Configuration = (DeviceConfiguration)bt.DataObject;
            }
        }

        void UpdateDeviceAdded(EventData data)
        {
            if (data != null)
            {
                if (data.Id == "DEVICE_ADDED" && data.Data01 != null)
                {
                    AddDevice((DeviceConfiguration)data.Data01);
                }
            }
        }

        void UpdateDeviceUpdated(EventData data)
        {
            if (data != null)
            {
                if (data.Id == "DEVICE_UPDATED" && data.Data01 != null)
                {
                    var config = (DeviceConfiguration)data.Data01;

                    //int i = Devices.ToList().FindIndex(x => x.UniqueId == config.UniqueId);
                    //if (i >= 0)
                    //{
                    //    Devices.RemoveAt(i);
                    //    Devices.Insert(i, config);
                    //}
                }
            }
        }

        void UpdateDeviceRemoved(EventData data)
        {
            if (data != null)
            {
                if (data.Id == "DEVICE_REMOVED" && data.Data01 != null)
                {
                    var config = (DeviceConfiguration)data.Data01;

                    //int i = Devices.ToList().FindIndex(x => x.UniqueId == config.UniqueId);
                    //if (i >= 0)
                    //{
                    //    Devices.RemoveAt(i);
                    //}
                }
            }
        }

        #endregion

        #region "Options"

        public IPage Options { get; set; }

        #endregion

    }
}
