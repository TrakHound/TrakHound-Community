// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Data;
using System.Windows.Media;

using TH_Global.Functions;
using TH_WPF;

namespace TH_StatusTimeline
{
    public class SegmentData : INotifyPropertyChanged
    {
        public string Title { get; set; }


        private double _totalSeconds;
        public double TotalSeconds
        {
            get { return _totalSeconds; }
            set
            {
                var val = _totalSeconds;
                _totalSeconds = value;
                if (val != _totalSeconds) NotifyChanged("TotalSeconds");
            }
        }


        private double _productionSeconds;
        public double ProductionSeconds
        {
            get { return _productionSeconds; }
            set
            {
                var val = _productionSeconds;
                _productionSeconds = value;
                if (val != _productionSeconds) NotifyChanged("ProductionSeconds");
            }
        }

        private double _idleSeconds;
        public double IdleSeconds
        {
            get { return _idleSeconds; }
            set
            {
                var val = _idleSeconds;
                _idleSeconds = value;
                if (val != _idleSeconds) NotifyChanged("IdleSeconds");
            }
        }

        private double _alertSeconds;
        public double AlertSeconds
        {
            get { return _alertSeconds; }
            set
            {
                var val = _alertSeconds;
                _alertSeconds = value;
                if (val != _alertSeconds) NotifyChanged("AlertSeconds");
            }
        }


        private int _status;
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

        //private Brush _fillBrush;
        //public Brush FillBrush
        //{
        //    get { return _fillBrush; }
        //    set
        //    {
        //        var val = _fillBrush;
        //        _fillBrush = value;
        //        if (val != _fillBrush) NotifyChanged("FillBrush");
        //    }
        //}


        public HourInfo HourInfo { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class HourInfo
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public int StartHour { get; set; }
        public int EndHour { get; set; }

        public string StartText { get; set; }
        public string EndText { get; set; }

        public static HourInfo Get(DataRow row)
        {
            string start = DataTable_Functions.GetRowValue("START", row);
            string end = DataTable_Functions.GetRowValue("END", row);

            DateTime s = DateTime.MinValue;
            DateTime e = DateTime.MinValue;

            DateTime.TryParse(start, out s);
            DateTime.TryParse(end, out e);

            var info = new HourInfo();
            info.Start = s;
            info.End = e;

            info.StartHour = s.Hour;
            info.EndHour = e.Hour;

            info.StartText = start;
            info.EndText = end;

            return info;
        }
    }
}
