using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using TH_Configuration;
using TH_Plugins;
using TH_Plugins.Client;

namespace TH_DeviceTable
{
    public partial class DeviceTable : IClientPlugin
    {

        #region "Descriptive"

        public string Title { get { return "Device Table"; } }

        public string Description { get { return "Compare Devices using a Table Format"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TH_DeviceTable;component/Resources/List_01.png")); } }


        public string Author { get { return "TrakHound"; } }

        public string AuthorText { get { return "©2016 Feenux LLC. All Rights Reserved"; } }

        public ImageSource AuthorImage { get { return null; } }


        public string LicenseName { get { return "GPLv3"; } }

        public string LicenseText { get { return null; } }

        #endregion

        #region "Update Information"

        public string UpdateFileURL { get { return null; } }

        #endregion

        #region "Plugin Properties/Options"

        public string DefaultParent { get { return "Dashboard"; } }
        public string DefaultParentCategory { get { return "Pages"; } }

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
        }

        public event SendData_Handler SendData;

        #endregion

        private ObservableCollection<Configuration> _devices;
        public ObservableCollection<Configuration> Devices
        {
            get
            {
                if (_devices == null)
                {
                    _devices = new ObservableCollection<Configuration>();
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
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                DeviceInfos.Clear();
            }

            if (e.NewItems != null)
            {
                foreach (Configuration newConfig in e.NewItems)
                {
                    if (newConfig != null) AddDevice(newConfig);
                }
            }

            if (e.OldItems != null)
            {
                foreach (Configuration oldConfig in e.OldItems)
                {
                    Devices.Remove(oldConfig);

                    int index = DeviceInfos.ToList().FindIndex(x => GetUniqueIdFromDeviceInfo(x) == oldConfig.UniqueId);
                    if (index >= 0) DeviceInfos.RemoveAt(index);
                }
            }
        }

        private static string GetUniqueIdFromDeviceInfo(DeviceInfo info)
        {
            if (info != null && info.Configuration != null)
            {
                return info.Configuration.UniqueId;
            }
            return null;
        }

        //private void AddDeviceInfo(Configuration config)
        //{
        //    Global.Initialize(config.Databases_Client);

        //    Controls.DeviceButton db = new DeviceButton();
        //    db.Description = config.Description.Description;
        //    db.Manufacturer = config.Description.Manufacturer;
        //    db.Model = config.Description.Model;
        //    db.Serial = config.Description.Serial;
        //    db.Id = config.Description.Device_ID;

        //    db.Clicked += db_Clicked;

        //    ListButton lb = new ListButton();
        //    lb.ButtonContent = db;
        //    lb.ShowImage = false;
        //    lb.Selected += lb_Device_Selected;
        //    lb.DataObject = config;

        //    db.Parent = lb;

        //    DeviceList.Add(lb);
        //}

        //ObservableCollection<Configuration> devices;
        //public ObservableCollection<Configuration> Devices
        //{
        //    get { return devices; }
        //    set
        //    {
        //        devices = value;

        //        if (devices != null)
        //        {
        //            //DeviceDisplays = new List<DeviceDisplay>();
        //            //RowHeaders.Clear();
        //            //Rows.Clear();

        //            DeviceInfos.Clear();

        //            foreach (var device in devices)
        //            {
        //                AddDevice(device);
        //                //CreateDeviceDisplay(device);
        //            }
        //        }
        //    }
        //}

        public TH_Global.IPage Options { get; set; }

    }
}
