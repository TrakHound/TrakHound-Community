// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using TH_Configuration;
using TH_Plugins_Client;

namespace TH_DeviceCompare
{
    public partial class DeviceCompare : UserControl, IClientPlugin
    {

        #region "Descriptive"

        public string Title { get { return "Device Compare"; } }

        public string Description { get { return "Compare Device Status and Data in a 'side-by-side' view"; } }

        private BitmapImage _image;
        public ImageSource Image
        {
            get
            {
                if (_image == null)
                {
                    _image = new BitmapImage(new Uri("pack://application:,,,/TH_DeviceCompare;component/Resources/Compare_01.png"));
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
                    _authorImage = new BitmapImage(new Uri("pack://application:,,,/TH_DeviceCompare;component/Resources/TrakHound_Logo_10_200px.png"));
                    _authorImage.Freeze();
                }

                return _authorImage;
            }
        }

        public string LicenseName { get { return "GPLv3"; } }

        public string LicenseText { get { return File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"\License\" + "License.txt"); } }

        #endregion

        #region "Update Information"

        public string UpdateFileURL { get { return "http://www.feenux.com/trakhound/appinfo/th/devicecompare-appinfo.json"; } }

        #endregion

        #region "Plugin Properties/Options"

        public string DefaultParent { get { return "Dashboard"; } }
        public string DefaultParentCategory { get { return "Pages"; } }

        public bool AcceptsPlugins { get { return true; } }

        public bool OpenOnStartUp { get { return true; } }

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

        public void Show()
        {
            if (ShowRequested != null)
            {
                PluginShowInfo info = new PluginShowInfo();
                info.Page = this;
                ShowRequested(info);
            }
        }

        #endregion

        #region "Events"

        public void Update_DataEvent(DataEvent_Data de_d)
        {
            UpdateData(de_d);
        }

        public event DataEvent_Handler DataEvent;

        public event PluginTools.ShowRequested_Handler ShowRequested;

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

        #endregion

        public TH_Global.IPage Options { get; set; }

    }
}
