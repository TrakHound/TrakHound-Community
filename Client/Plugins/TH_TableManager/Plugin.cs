using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

using TH_Configuration;
using TH_Database;
using TH_Global;
using TH_Global.Functions;
using TH_Plugins_Client;
using TH_WPF;

using TH_TableManager.Controls;

namespace TH_TableManager
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
                    _image = new BitmapImage(new Uri("pack://application:,,,/TH_TableManager;component/Resources/Table_01.png"));
                    _image.Freeze();
                }

                return _image;
            }
        }


        public string Author { get { return "TrakHound"; } }

        public string AuthorText { get { return "©2015 Feenux LLC. All Rights Reserved"; } }

        private BitmapImage _authorImage;
        public ImageSource AuthorImage
        {
            get
            {
                if (_authorImage == null)
                {
                    _authorImage = new BitmapImage(new Uri("pack://application:,,,/TH_TableManager;component/Resources/TrakHound_Logo_10_200px.png"));
                    _authorImage.Freeze();
                }

                return _authorImage;
            }
        }


        public string LicenseName { get { return "GPLv3"; } }

        public string LicenseText { get { return File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"\License\" + "License.txt"); } }

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

        public void Update_DataEvent(DataEvent_Data de_d)
        {
            UpdateLoggedInChanged(de_d);

            UpdateDevicesLoading(de_d);
        }

        public event DataEvent_Handler DataEvent;

        public event TH_Plugins_Client.PluginTools.ShowRequested_Handler ShowRequested;

        #endregion

        #region "Device Properties"

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
                DeviceList.Clear();
            }

            if (e.NewItems != null)
            {
                foreach (Configuration newConfig in e.NewItems)
                {
                    if (newConfig != null) AddDeviceButton(newConfig);
                }
            }

            if (e.OldItems != null)
            {
                foreach (Configuration oldConfig in e.OldItems)
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
                if (bt.DataObject.GetType() == typeof(Configuration))
                {
                    return ((Configuration)bt.DataObject).UniqueId;
                }
            }
            return null;
        }

        private void AddDeviceButton(Configuration config)
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

        public TH_Global.IPage Options { get; set; }

        #endregion

    }
}
