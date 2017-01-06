// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MTConnect.Application.Components;
using System.ComponentModel;

namespace TrakHound_Dashboard.Pages.DeviceManager.AddDevice
{
    public class DeviceInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public DeviceInfo(string address, int port, Device device)
        {
            Id = TrakHound.Tools.String_Functions.RandomString(20);
            Address = address;
            Port = port;
            Device = device;
        }

        public string Id { get; set; }

        private bool _loading = false;
        public bool Loading
        {
            get { return _loading; }
            set
            {
                _loading = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Loading"));
            }
        }

        public string Address { get; set; }
        public int Port { get; set; }
        public Device Device { get; set; }

        public Description DeviceDescription
        {
            get
            {
                if (Device != null) return Device.Description;
                else return null;
            }
        }
    }
   
}
