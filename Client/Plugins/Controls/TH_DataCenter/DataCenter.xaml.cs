// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.IO;

using TH_Configuration;
using TH_Device_Client;
using TH_PlugIns_Client_Control;

namespace TH_DataCenter
{
    /// <summary>
    /// Interaction logic for DataCenter.xaml
    /// </summary>
    public partial class DataCenter : UserControl, Control_PlugIn
    {
        public DataCenter()
        {
            InitializeComponent();
        }


        #region "PlugIn"

        #region "Descriptive"

        public string Title { get { return "Data Center"; } }

        public string Description { get { return "Digital Signage for Device Monitoring"; } }

        //public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TH_DeviceCompare;component/Resources/Compare_01.png")); } }
        public ImageSource Image { get { return null; } }


        public string Author { get { return "TrakHound"; } }

        public string AuthorText { get { return "©2015 Feenux LLC. All Rights Reserved"; } }

        public ImageSource AuthorImage { get { return new BitmapImage(new Uri("pack://application:,,,/TH_DeviceCompare;component/Resources/TrakHound_Logo_10_200px.png")); } }


        public string LicenseName { get { return "GPLv2"; } }

        public string LicenseText { get { return File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"\License\" + "License.txt"); } }

        #endregion

        #region "Plugin Properties/Options"

        public string DefaultParent { get { return null; } }
        public string DefaultParentCategory { get { return null; } }

        public bool AcceptsPlugIns { get { return true; } }

        public bool OpenOnStartUp { get { return true; } }

        public List<PlugInConfigurationCategory> SubCategories { get; set; }

        public List<Control_PlugIn> PlugIns { get; set; }

        #endregion

        #region "Methods"

        public void Initialize() { }

        public void Update(ReturnData rd)
        {

        }

        public void Closing() { }

        #endregion

        #region "Events"

        public void Update_DataEvent(DataEvent_Data de_d)
        {

        }

        public event DataEvent_Handler DataEvent;

        #endregion

        #region "Device Properties"

        private List<Device_Client> lDevices;
        public List<Device_Client> Devices
        {
            get
            {
                return lDevices;
            }
            set
            {
                lDevices = value;
            }
        }

        private int lSelectedDeviceIndex;
        public int SelectedDeviceIndex
        {
            get { return lSelectedDeviceIndex; }

            set
            {
                lSelectedDeviceIndex = value;
            }
        }

        #endregion

        #region "Options"

        public OptionsPage Options { get; set; }

        #endregion

        public object RootParent { get; set; }

        #endregion

        #region "Data Center"

        void FullScreen_View()
        {
            FullScreen fs = new FullScreen();
            fs.FullScreenClosing += fs_FullScreenClosing;

            Root_GRID.Children.Remove(Content_GRID);

            fs.WindowContent = Content_GRID;

            fs.Show();
        }

        void fs_FullScreenClosing(object windowcontent)
        {
            if (windowcontent != null)
            {
                UIElement uie = windowcontent as UIElement;
                Root_GRID.Children.Insert(0, uie);
            }
        }

        #endregion

        private void FullScreen_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            FullScreen_View();
        }

    }
}
