// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using TrakHound;
using TrakHound.Configurations;
using TrakHound.Databases;
using TrakHound.Plugins;
using TrakHound.Plugins.Client;
using TrakHound_UI;

using TrakHound_Dashboard.Pages.TableManager.Controls;

namespace TrakHound_Dashboard.Pages.TableManager
{
    public partial class Plugin : IClientPlugin
    {

        #region "Descriptive"

        public string Title { get { return "Table Manager"; } }

        public string Description { get { return "Display and Manage Database Tables associated with Device"; } }

        private BitmapImage _image;
        public ImageSource Image
        {
            get
            {
                if (_image == null)
                {
                    _image = new BitmapImage(new Uri("pack://application:,,,/TrakHound-Dashboard;component/Resources/Table_01.png"));
                    _image.Freeze();
                }

                return _image;
            }
        }


        public string Author { get { return "TrakHound"; } }

        public string AuthorText { get { return "©2015 Feenux LLC. All Rights Reserved"; } }

        public ImageSource AuthorImage { get { return null; } }


        public string LicenseName { get { return "GPLv3"; } }

        public string LicenseText { get { return null; } }

        #endregion

        #region "Update Information"

        public string UpdateFileURL { get { return "http://www.feenux.com/trakhound/appinfo/th/tablemanager-appinfo.json"; } }

        #endregion

        #region "Plugin Properties/Options"

        public string DefaultParent { get { return null; } }
        public string DefaultParentCategory { get { return null; } }

        public bool AcceptsPlugins { get { return true; } }

        public bool OpenOnStartUp { get { return false; } }

        public bool ShowInAppMenu { get { return true; } }

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
            UpdateLoggedInChanged(data);

            UpdateDevicesLoading(data);
        }

        public event SendData_Handler SendData;

        #endregion

        #region "Device Properties"

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
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                DeviceList.Clear();
            }

            if (e.NewItems != null)
            {
                foreach (DeviceConfiguration newConfig in e.NewItems)
                {
                    if (newConfig != null) AddDeviceButton(newConfig);
                }
            }

            if (e.OldItems != null)
            {
                foreach (DeviceConfiguration oldConfig in e.OldItems)
                {
                    Devices.Remove(oldConfig);

                    int index = DeviceList.ToList().FindIndex(x => GetUniqueIdFromListButton(x) == oldConfig.UniqueId);
                    if (index >= 0) DeviceList.RemoveAt(index);
                }
            }
        }

        private static string GetUniqueIdFromListButton(ListButton bt)
        {
            if (bt != null && bt.DataObject != null)
            {
                if (bt.DataObject.GetType() == typeof(DeviceConfiguration))
                {
                    return ((DeviceConfiguration)bt.DataObject).UniqueId;
                }
            }
            return null;
        }

        private void AddDeviceButton(DeviceConfiguration config)
        {
            Global.Initialize(config.Databases_Client);

            Controls.DeviceButton db = new DeviceButton();
            db.Description = config.Description.Description;
            db.Manufacturer = config.Description.Manufacturer;
            db.Model = config.Description.Model;
            db.Serial = config.Description.Serial;
            db.Id = config.Description.Device_ID;

            db.Clicked += db_Clicked;

            ListButton lb = new ListButton();
            lb.ButtonContent = db;
            lb.ShowImage = false;
            lb.Selected += lb_Device_Selected;
            lb.DataObject = config;

            db.Parent = lb;

            DeviceList.Add(lb);
        }

        void db_Clicked(DeviceButton bt)
        {
            if (bt.Parent != null)
            {
                if (bt.Parent.GetType() == typeof(ListButton))
                {
                    ListButton lb = (ListButton)bt.Parent;

                    lb_Device_Selected(lb);
                }
            }
        }

        #endregion

        #region "Options"

        public IPage Options { get; set; }

        #endregion

    }
}
