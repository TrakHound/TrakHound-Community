using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using TrakHound;
using TrakHound.Tools;
using TrakHound.Logging;
using TrakHound.Configurations;
using TrakHound.Plugins;
using TrakHound.Plugins.Client;

namespace TrakHound_Overview.Pages.Dashboard.OverrideStatus
{
    public partial class Plugin : IClientPlugin
    {

        #region "Descriptive"

        public string Title { get { return "Override Status"; } }

        public string Description { get { return "Displays the Device's Feedrate, Rapidrate, and Spindle Overrides"; } }

        public ImageSource Image { get { return null; } }


        public string Author { get { return "TrakHound"; } }

        public string AuthorText { get { return "©2017 TrakHound Inc.. All Rights Reserved"; } }

        public ImageSource AuthorImage { get { return null; } }


        public string LicenseName { get { return "GPLv3"; } }

        public string LicenseText { get { return null; } }

        #endregion

        #region "Update Information"

        public string UpdateFileURL { get { return null; } }

        #endregion

        #region "Plugin Properties/Options"

        public string ParentPlugin { get { return "Dashboard"; } }
        public string ParentPluginCategory { get { return "Pages"; } }

        public bool AcceptsPlugins { get { return false; } }

        public bool OpenOnStartUp { get { return true; } }

        public bool ShowInAppMenu { get { return true; } }

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

            this.Dispatcher.BeginInvoke(new Action<EventData>(UpdateDeviceAdded), System.Windows.Threading.DispatcherPriority.DataBind, new object[] { data });
            this.Dispatcher.BeginInvoke(new Action<EventData>(UpdateDeviceUpdated), System.Windows.Threading.DispatcherPriority.DataBind, new object[] { data });
            this.Dispatcher.BeginInvoke(new Action<EventData>(UpdateDeviceRemoved), System.Windows.Threading.DispatcherPriority.DataBind, new object[] { data });
        }

        public event SendData_Handler SendData;

        #endregion

        private ObservableCollection<DeviceConfiguration> _devices;
        public ObservableCollection<DeviceConfiguration> Devices
        {
            get
            {
                if (_devices == null)
                {
                    _devices = new ObservableCollection<DeviceConfiguration>();
                    _devices.CollectionChanged += Devices_CollectionChanged;
                }
                return _devices;
            }
            set
            {
                _devices = value;
            }
        }

        public void Devices_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            //    {
            //        Rows.Clear();
            //    }

            //    if (e.NewItems != null)
            //    {
            //        foreach (DeviceConfiguration newConfig in e.NewItems)
            //        {
            //            if (newConfig != null) AddRow(newConfig);
            //        }
            //    }

            //    if (e.OldItems != null)
            //    {
            //        foreach (DeviceConfiguration oldConfig in e.OldItems)
            //        {
            //            Devices.Remove(oldConfig);

            //            int index = Rows.ToList().FindIndex(x => GetUniqueIdFromDeviceInfo(x) == oldConfig.UniqueId);
            //            if (index >= 0) Rows.RemoveAt(index);
            //        }
            //    }
            //}));


        }

        private static string GetUniqueIdFromDeviceInfo(TrakHound_Overview.Pages.Dashboard.OverrideStatus.Controls.Row row)
        {
            if (row != null && row.Configuration != null)
            {
                return row.Configuration.UniqueId;
            }
            return null;
        }


        public IPage Options { get; set; }


    }
}
