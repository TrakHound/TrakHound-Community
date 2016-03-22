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

using TH_Configuration;
using TH_Plugins_Client;

namespace TH_DeviceTable
{
    public partial class DeviceTable : UserControl
    {
        public DeviceTable()
        {
            InitializeComponent();
            Devices_DG.DataContext = this;
        }

        private void DataGridMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var item = (DataGridMenuItem)sender;

            if (item.DataObject != null)
            {
                if (item.DataObject.GetType() == typeof(DeviceInfo))
                {
                    var info = (DeviceInfo)item.DataObject;

                    if (info.Configuration != null)
                    {
                        OpenDevicePage(info.Configuration);
                    }
                }
            }
        }

        private void OpenDevicePage(Configuration config)
        {
            var de_d = new DataEvent_Data();
            de_d.id = "DevicePage_Show";
            de_d.data01 = config;
            if (DataEvent != null) DataEvent(de_d);
        }
    }

    public class DataGridMenuItem : MenuItem
    {

        public object DataObject
        {
            get { return (object)GetValue(DataObjectProperty); }
            set { SetValue(DataObjectProperty, value); }
        }

        public static readonly DependencyProperty DataObjectProperty =
            DependencyProperty.Register("DataObject", typeof(object), typeof(DataGridMenuItem), new PropertyMetadata(null));

    }
}
