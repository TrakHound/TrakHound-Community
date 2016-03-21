using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TH_Configuration;

namespace TH_DeviceTable
{
    public partial class DeviceTable
    {

        ObservableCollection<DeviceInfo> _deviceInfos;
        /// <summary>
        /// Collection of TH_Configuration.Configuration objects that represent the devices
        /// </summary>
        public ObservableCollection<DeviceInfo> DeviceInfos
        {
            get
            {
                if (_deviceInfos == null)
                    _deviceInfos = new ObservableCollection<DeviceInfo>();
                return _deviceInfos;
            }

            set
            {
                _deviceInfos = value;
            }
        }

        public void AddDevice(Configuration config)
        {
            var info = new DeviceInfo();
            info.Configuration = config;
            DeviceInfos.Add(info);
        }

    }
}
