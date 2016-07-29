// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Windows.Media;
using System.ComponentModel;

using MTConnect.Application.Components;

namespace TrakHound_Device_Manager.AddDevice
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
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Loading"));
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


    /// <summary>
    /// Class containing the data for each device found using Pages.AutoDetect
    /// </summary>
    //public class DeviceInfo
    //{
    //    public string Id { get; set; }

    //    public bool Loading { get; set; }

    //    public string IPAddress { get; set; }
    //    public int Port { get; set; }
    //    public Device Device { get; set; }

    //    public Description DeviceDescription
    //    {
    //        get
    //        {
    //            if (Device != null) return Device.Description;
    //            else return null;
    //        }
    //    }

    //    public string DeviceType
    //    {
    //        get
    //        {
    //            if (SharedListItem != null) return SharedListItem.device_type;
    //            return null;
    //        }
    //    }

    //    public string Controller
    //    {
    //        get
    //        {
    //            if (SharedListItem != null) return SharedListItem.controller;
    //            return null;
    //        }
    //    }

    //    public TH_UserManagement.Management.Shared.SharedListItem SharedListItem { get; set; }

    //    public ImageSource Image { get; set; }
    //}
}
