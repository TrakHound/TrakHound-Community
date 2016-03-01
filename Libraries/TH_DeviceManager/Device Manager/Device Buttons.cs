using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TH_Configuration;
using TH_WPF;

using TH_DeviceManager.Controls;

namespace TH_DeviceManager
{
    public partial class DeviceManager
    {

        public bool DeviceListShown
        {
            get { return (bool)GetValue(DeviceListShownProperty); }
            set { SetValue(DeviceListShownProperty, value); }
        }

        public static readonly DependencyProperty DeviceListShownProperty =
            DependencyProperty.Register("DeviceListShown", typeof(bool), typeof(DeviceManager), new PropertyMetadata(false));


        public bool DeviceListOptionsShown
        {
            get { return (bool)GetValue(DeviceListOptionsShownProperty); }
            set { SetValue(DeviceListOptionsShownProperty, value); }
        }

        public static readonly DependencyProperty DeviceListOptionsShownProperty =
            DependencyProperty.Register("DeviceListOptionsShown", typeof(bool), typeof(DeviceManager), new PropertyMetadata(false));


        ObservableCollection<ListButton> devicelist;
        public ObservableCollection<ListButton> DeviceList
        {
            get
            {
                if (devicelist == null)
                    devicelist = new ObservableCollection<ListButton>();
                return devicelist;
            }

            set
            {
                devicelist = value;
            }
        }

        List<ListButton> addeddevicelist;
        public List<ListButton> AddedDeviceList
        {
            get
            {
                if (addeddevicelist == null)
                    addeddevicelist = new List<ListButton>();
                return addeddevicelist;
            }

            set
            {
                addeddevicelist = value;
            }
        }

        List<ListButton> shareddevicelist;
        public List<ListButton> SharedDeviceList
        {
            get
            {
                if (shareddevicelist == null)
                    shareddevicelist = new List<ListButton>();
                return shareddevicelist;
            }

            set
            {
                shareddevicelist = value;
            }
        }

        private void ShowAdded_Checked(object sender, RoutedEventArgs e)
        {
            ShowAddedDevices();
        }

        private void ShowShared_Checked(object sender, RoutedEventArgs e)
        {
            ShowSharedDevices();
        }

        void ShowAddedDevices()
        {
            DeviceList.Clear();
            foreach (var bt in AddedDeviceList) DeviceList.Add(bt);
        }

        void ShowSharedDevices()
        {
            DeviceList.Clear();
            foreach (var bt in SharedDeviceList) DeviceList.Add(bt);
        }

        void AddDeviceButton(string uniqueId, string tableName)
        {
            ListButton lb;

            int index = DeviceList.ToList().FindIndex(x => GetDeviceButtonUniqueId(x) == uniqueId);
            if (index >= 0)
            {

            }
            else
            {
                Controls.DeviceButton db = new Controls.DeviceButton();
                db.devicemanager = this;

                // Create a temporary Configuration to hold the button's place in the list
                Configuration config = Configuration.CreateBlank();
                config.UniqueId = uniqueId;
                config.TableName = tableName;
                db.Config = config;

                db.Enabled += db_Enabled;
                db.Disabled += db_Disabled;
                db.RemoveClicked += db_RemoveClicked;
                db.ShareClicked += db_ShareClicked;
                db.CopyClicked += db_CopyClicked;
                db.Clicked += db_Clicked;

                lb = new ListButton();
                lb.ButtonContent = db;
                lb.ShowImage = false;
                lb.Selected += lb_Device_Selected;

                db.Parent = lb;

                DeviceList.Add(lb);
            }
        }

        void AddDeviceButton(Configuration config)
        {
            ListButton lb;

            int index = AddedDeviceList.ToList().FindIndex(x => GetDeviceButtonUniqueId(x) == config.UniqueId);
            if (index >= 0)
            {
                lb = AddedDeviceList[index];
                Controls.DeviceButton db = lb.ButtonContent as Controls.DeviceButton;
                if (db != null)
                {
                    db.Config = config;
                }
            }
            else
            {
                Controls.DeviceButton db = new Controls.DeviceButton();
                db.devicemanager = this;

                db.Config = config;

                db.Enabled += db_Enabled;
                db.Disabled += db_Disabled;
                db.RemoveClicked += db_RemoveClicked;
                db.ShareClicked += db_ShareClicked;
                db.CopyClicked += db_CopyClicked;
                db.Clicked += db_Clicked;

                lb = new ListButton();
                lb.ButtonContent = db;
                lb.ShowImage = false;
                lb.Selected += lb_Device_Selected;

                db.Parent = lb;

                AddedDeviceList.Add(lb);
            }
        }

        void AddSharedDeviceButton(Configuration config)
        {
            ListButton lb;

            int index = SharedDeviceList.ToList().FindIndex(x => GetDeviceButtonUniqueId(x) == config.UniqueId);
            if (index >= 0)
            {
                lb = SharedDeviceList[index];
                Controls.DeviceButton db = lb.ButtonContent as Controls.DeviceButton;
                if (db != null)
                {
                    db.Config = config;
                }
            }
            else
            {
                Controls.DeviceButton db = new Controls.DeviceButton();
                db.devicemanager = this;

                db.Config = config;

                db.Enabled += db_Enabled;
                db.Disabled += db_Disabled;
                db.RemoveClicked += db_RemoveClicked;
                db.ShareClicked += db_ShareClicked;
                db.CopyClicked += db_CopyClicked;
                db.Clicked += db_Clicked;

                lb = new ListButton();
                lb.ButtonContent = db;
                lb.ShowImage = false;
                lb.Selected += lb_Device_Selected;

                db.Parent = lb;

                SharedDeviceList.Add(lb);
            }
        }



        string GetDeviceButtonUniqueId(ListButton lb)
        {
            string result = null;

            if (lb != null)
            {
                if (lb.ButtonContent != null)
                {
                    Configuration config = lb.ButtonContent as Configuration;
                    if (config != null)
                    {
                        result = config.UniqueId;
                    }
                }
            }

            return result;
        }

        void db_Enabled(DeviceButton bt)
        {
            if (bt.Config != null)
            {
                //if (bt.Config.TableName != null) EnableDevice(bt, bt.Config.TableName);
                EnableDevice(bt);
            }
        }

        void db_Disabled(DeviceButton bt)
        {
            if (bt.Config != null)
            {
                //if (bt.Config.TableName != null) DisableDevice(bt, bt.Config.TableName);
                DisableDevice(bt);
            }
        }

        void db_RemoveClicked(DeviceButton bt)
        {
            if (bt.Config != null)
            {
                RemoveDevice(bt);
            }

            if (DeviceList.Count == 0) AddDevice();
        }

        void db_ShareClicked(DeviceButton bt)
        {
            PageListShown = false;

            LoadAddSharePage(bt);

            ToolbarShown = false;
        }

        void db_CopyClicked(DeviceButton bt)
        {
            if (bt.Config != null)
            {
                CopyDevice(bt.Config);
                //CopyDevice(bt.Config);
            }

            db_Clicked(bt);
        }

        void db_Clicked(DeviceButton bt)
        {
            if (bt.Parent != null)
            {
                if (bt.Parent.GetType() == typeof(ListButton))
                {
                    ListButton lb = (ListButton)bt.Parent;

                    SelectedDeviceButton = bt;

                    lb_Device_Selected(lb);
                }
            }
        }

    }
}
