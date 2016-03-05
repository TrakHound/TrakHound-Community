using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TH_DeviceManager.AddDevice.Controls
{
    /// <summary>
    /// Interaction logic for DeviceItem.xaml
    /// </summary>
    public partial class DeviceItem : UserControl
    {
        public DeviceItem()
        {
            InitializeComponent();
            DataContext = this;
        }

        #region "Network Properties"

        public string IPAddress
        {
            get { return (string)GetValue(IPAddressProperty); }
            set { SetValue(IPAddressProperty, value); }
        }

        public static readonly DependencyProperty IPAddressProperty =
            DependencyProperty.Register("IPAddress", typeof(string), typeof(DeviceItem), new PropertyMetadata(null));

        public string Port
        {
            get { return (string)GetValue(PortProperty); }
            set { SetValue(PortProperty, value); }
        }

        public static readonly DependencyProperty PortProperty =
            DependencyProperty.Register("Port", typeof(string), typeof(DeviceItem), new PropertyMetadata(null));

        #endregion

        #region "Device Properties"

        public string DeviceName
        {
            get { return (string)GetValue(DeviceNameProperty); }
            set { SetValue(DeviceNameProperty, value); }
        }

        public static readonly DependencyProperty DeviceNameProperty =
            DependencyProperty.Register("DeviceName", typeof(string), typeof(DeviceItem), new PropertyMetadata(null));

        public string DeviceManufacturer
        {
            get { return (string)GetValue(DeviceManufacturerProperty); }
            set { SetValue(DeviceManufacturerProperty, value); }
        }

        public static readonly DependencyProperty DeviceManufacturerProperty =
            DependencyProperty.Register("DeviceManufacturer", typeof(string), typeof(DeviceItem), new PropertyMetadata(null));


        public string DeviceModel
        {
            get { return (string)GetValue(DeviceModelProperty); }
            set { SetValue(DeviceModelProperty, value); }
        }

        public static readonly DependencyProperty DeviceModelProperty =
            DependencyProperty.Register("DeviceModel", typeof(string), typeof(DeviceItem), new PropertyMetadata(null));


        public string DeviceSerial
        {
            get { return (string)GetValue(DeviceSerialProperty); }
            set { SetValue(DeviceSerialProperty, value); }
        }

        public static readonly DependencyProperty DeviceSerialProperty =
            DependencyProperty.Register("DeviceSerial", typeof(string), typeof(DeviceItem), new PropertyMetadata(null));

        #endregion


        private void Add_Clicked(TH_WPF.Button bt)
        {

        }
    }

}
