// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.ComponentModel;

namespace TrakHound_Dashboard.Pages.DeviceDetails
{
    public class HourData : INotifyPropertyChanged
    {

        public HourData(int startHour, int endHour)
        {
            StartHour = startHour;
            EndHour = endHour;
        }

        public void Reset()
        {
            Value = 0;
            Status = -1;
        }


        public int StartHour { get; set; }
        public int EndHour { get; set; }


        private double _value;
        public double Value
        {
            get { return _value; }
            set
            {
                var val = _value;
                _value = value;
                if (val != _value) NotifyChanged("Value");
            }
        }

        private int _status = -1;
        public int Status
        {
            get { return _status; }
            set
            {
                var val = _status;
                _status = value;
                if (val != _status) NotifyChanged("Status");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    
}
