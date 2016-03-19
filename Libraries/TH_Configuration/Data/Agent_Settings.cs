// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

namespace TH_Configuration
{

    public class Agent_Settings : IConfiguration
    {
        public event ConfigurationPropertyChanged_Handler ConfigurationPropertyChanged;

        private string _serviceName;
        public string ServiceName
        {
            get { return _serviceName; }
            set
            {
                _serviceName = value;
                if (ConfigurationPropertyChanged != null) ConfigurationPropertyChanged("ServiceName", _serviceName);
            }
        }

        private string _address;
        public string Address
        {
            get { return _address; }
            set
            {
                _address = value;
                if (ConfigurationPropertyChanged != null) ConfigurationPropertyChanged("Address", _address);
            }
        }

        /// <summary>
        /// Deprecated Property (Use 'Address')
        /// </summary>
        public string IP_Address
        {
            get { return _address; }
            set
            {
                _address = value;
                if (ConfigurationPropertyChanged != null) ConfigurationPropertyChanged("Address", _address);
            }
        }

        private int _port;
        public int Port
        {
            get { return _port; }
            set
            {
                _port = value;
                if (ConfigurationPropertyChanged != null) ConfigurationPropertyChanged("Port", _port.ToString());
            }
        }

        private string _deviceName;
        public string DeviceName
        {
            get { return _deviceName; }
            set
            {
                _deviceName = value;
                if (ConfigurationPropertyChanged != null) ConfigurationPropertyChanged("DeviceName", _deviceName);
            }
        }

        /// <summary>
        /// Deprecated Property (Use 'DeviceName')
        /// </summary>
        public string Device_Name
        {
            get { return _deviceName; }
            set
            {
                _deviceName = value;
                if (ConfigurationPropertyChanged != null) ConfigurationPropertyChanged("DeviceName", _deviceName);
            }
        }

        private int _heartbeat;
        public int Heartbeat
        {
            get { return _heartbeat; }
            set
            {
                _heartbeat = value;
                if (ConfigurationPropertyChanged != null) ConfigurationPropertyChanged("Heartbeat", _heartbeat.ToString());
            }
        }

        private string _proxyAddress;
        public string ProxyAddress
        {
            get { return _proxyAddress; }
            set
            {
                _proxyAddress = value;
                if (ConfigurationPropertyChanged != null) ConfigurationPropertyChanged("ProxyAddress", _proxyAddress);
            }
        }

        private int _proxyPort;
        public int ProxyPort
        {
            get { return _proxyPort; }
            set
            {
                _proxyPort = value;
                if (ConfigurationPropertyChanged != null) ConfigurationPropertyChanged("ProxyPort", _proxyPort.ToString());
            }
        }


        #region "OBSOLETE 1-23-16"

        public int Current_Heartbeat { get; set; }
        public int Sample_Heartbeat { get; set; }

        public int Max_Sample_Interval { get; set; }

        public List<string> Simulation_Sample_Files = new List<string>();


        // Row Limit : <MTC Data Item Link>Limit of Rows</MTC Data Item Link>
        public List<Tuple<string, int>> RowLimits = new List<Tuple<string, int>>();

        // List of values that are to be Ommitted from the Instance table 
        // (to prevent some values that change often from creating a ton of instance rows)
        public List<string> OmitInstance = new List<string>();

        #endregion

    }

}
