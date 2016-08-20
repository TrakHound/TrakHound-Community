// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.ComponentModel;

namespace TrakHound_Dashboard.Pages.Dashboard.OeeHourTimeline
{
    public class HourData : INotifyPropertyChanged
    {

        public HourData(int startHour, int endHour)
        {
            StartHour = startHour;
            EndHour = endHour;

            StartTime = GetHourString(startHour);
            EndTime = GetHourString(endHour);
        }

        public void SetDate(DateTime date)
        {


        }

        private string GetHourString(int hour)
        {
            string h = hour.ToString();
            string t = "AM";

            if (hour > 12)
            {
                h = (hour - 12).ToString();
                t = "PM";
            }

            string format = "{0}:00 {1}";
            return string.Format(format, h, t);
        }

        public int StartHour { get; set; }
        public int EndHour { get; set; }



        private string _startTime;
        public string StartTime
        {
            get { return _startTime; }
            set
            {
                var val = _startTime;
                _startTime = value;
                if (val != _startTime) NotifyChanged("StartTime");
            }
        }

        private string _endTime;
        public string EndTime
        {
            get { return _endTime; }
            set
            {
                var val = _endTime;
                _endTime = value;
                if (val != _endTime) NotifyChanged("EndTime");
            }
        }

        private string _date;
        public string Date
        {
            get { return _date; }
            set
            {
                var val = _date;
                _date = value;
                if (val != _date) NotifyChanged("Date");
            }
        }


        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                var val = _title;
                _title = value;
                if (val != _title) NotifyChanged("Title");
            }
        }




        private double _oee;
        public double Oee
        {
            get { return _oee; }
            set
            {
                var val = _oee;
                _oee = value;

                //if (value > 0.75) Status = 2;
                //else if (value > 0.5) Status = 1;
                //else if (value >= 0) Status = 0;
                //else Status = -1;

                if (val != _oee) NotifyChanged("Oee");
            }
        }

        private double _availability;
        public double Availability
        {
            get { return _availability; }
            set
            {
                var val = _availability;
                _availability = value;
                if (val != _availability) NotifyChanged("Availability");
            }
        }

        private double _performance;
        public double Performance
        {
            get { return _performance; }
            set
            {
                var val = _performance;
                _performance = value;
                if (val != _performance) NotifyChanged("Performance");
            }
        }

        private double _quality;
        public double Quality
        {
            get { return _quality; }
            set
            {
                var val = _quality;
                _quality = value;
                if (val != _quality) NotifyChanged("Quality");
            }
        }


        //private double _totalSeconds;
        //public double TotalSeconds
        //{
        //    get { return _totalSeconds; }
        //    set
        //    {
        //        var val = _totalSeconds;
        //        _totalSeconds = value;
        //        ProcessStatus();
        //        ProcessPercentages();
        //        if (val != _totalSeconds) NotifyChanged("TotalSeconds");
        //    }
        //}


        //private double _activeSeconds;
        //public double ActiveSeconds
        //{
        //    get { return _activeSeconds; }
        //    set
        //    {
        //        var val = _activeSeconds;
        //        _activeSeconds = value;
        //        ProcessStatus();
        //        ProcessPercentages();
        //        if (val != _activeSeconds) NotifyChanged("ActiveSeconds");
        //    }
        //}

        //private double _idleSeconds;
        //public double IdleSeconds
        //{
        //    get { return _idleSeconds; }
        //    set
        //    {
        //        var val = _idleSeconds;
        //        _idleSeconds = value;
        //        ProcessStatus();
        //        ProcessPercentages();
        //        if (val != _idleSeconds) NotifyChanged("IdleSeconds");
        //    }
        //}

        //private double _alertSeconds;
        //public double AlertSeconds
        //{
        //    get { return _alertSeconds; }
        //    set
        //    {
        //        var val = _alertSeconds;
        //        _alertSeconds = value;
        //        ProcessStatus();
        //        ProcessPercentages();
        //        if (val != _alertSeconds) NotifyChanged("AlertSeconds");
        //    }
        //}



        //private void ProcessPercentages()
        //{
        //    if (TotalSeconds > 0)
        //    {
        //        ActivePercentage = ActiveSeconds / TotalSeconds;
        //        IdlePercentage = IdleSeconds / TotalSeconds;
        //        AlertPercentage = AlertSeconds / TotalSeconds;
        //    }
        //}

        //private double _activePercentage;
        //public double ActivePercentage
        //{
        //    get { return _activePercentage; }
        //    set
        //    {
        //        var val = _activePercentage;
        //        _activePercentage = value;
        //        if (val != _activePercentage) NotifyChanged("ActivePercentage");
        //    }
        //}

        //private double _idlePercentage;
        //public double IdlePercentage
        //{
        //    get { return _idlePercentage; }
        //    set
        //    {
        //        var val = _idlePercentage;
        //        _idlePercentage = value;
        //        if (val != _idlePercentage) NotifyChanged("IdlePercentage");
        //    }
        //}

        //private double _alertPercentage;
        //public double AlertPercentage
        //{
        //    get { return _alertPercentage; }
        //    set
        //    {
        //        var val = _alertPercentage;
        //        _alertPercentage = value;
        //        if (val != _alertPercentage) NotifyChanged("AlertPercentage");
        //    }
        //}


        private void ProcessStatus()
        {



            //if (ActiveSeconds > IdleSeconds && ActiveSeconds > AlertSeconds) Status = 2;
            //else if (IdleSeconds > AlertSeconds && IdleSeconds > ActiveSeconds) Status = 1;
            //else Status = 0;
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
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

    }



    //public class HourData : INotifyPropertyChanged
    //{

    //    public HourData(int startHour, int endHour)
    //    {
    //        StartHour = startHour;
    //        EndHour = endHour;

    //        StartTime = GetHourString(startHour);
    //        EndTime = GetHourString(endHour);
    //    }

    //    public void SetDate(DateTime date)
    //    {


    //    }

    //    private string GetHourString(int hour)
    //    {
    //        string h = hour.ToString();
    //        string t = "AM";

    //        if (hour > 12)
    //        {
    //            h = (hour - 12).ToString();
    //            t = "PM";
    //        }

    //        string format = "{0}:00 {1}";
    //        return string.Format(format, h, t);
    //    }

    //    public int StartHour { get; set; }
    //    public int EndHour { get; set; }



    //    private string _startTime;
    //    public string StartTime
    //    {
    //        get { return _startTime; }
    //        set
    //        {
    //            var val = _startTime;
    //            _startTime = value;
    //            if (val != _startTime) NotifyChanged("StartTime");
    //        }
    //    }

    //    private string _endTime;
    //    public string EndTime
    //    {
    //        get { return _endTime; }
    //        set
    //        {
    //            var val = _endTime;
    //            _endTime = value;
    //            if (val != _endTime) NotifyChanged("EndTime");
    //        }
    //    }

    //    private string _date;
    //    public string Date
    //    {
    //        get { return _date; }
    //        set
    //        {
    //            var val = _date;
    //            _date = value;
    //            if (val != _date) NotifyChanged("Date");
    //        }
    //    }


    //    private string _title;
    //    public string Title
    //    {
    //        get { return _title; }
    //        set
    //        {
    //            var val = _title;
    //            _title = value;
    //            if (val != _title) NotifyChanged("Title");
    //        }
    //    }

    //    private double _totalSeconds;
    //    public double TotalSeconds
    //    {
    //        get { return _totalSeconds; }
    //        set
    //        {
    //            var val = _totalSeconds;
    //            _totalSeconds = value;
    //            ProcessStatus();
    //            ProcessPercentages();
    //            if (val != _totalSeconds) NotifyChanged("TotalSeconds");
    //        }
    //    }


    //    private double _activeSeconds;
    //    public double ActiveSeconds
    //    {
    //        get { return _activeSeconds; }
    //        set
    //        {
    //            var val = _activeSeconds;
    //            _activeSeconds = value;
    //            ProcessStatus();
    //            ProcessPercentages();
    //            if (val != _activeSeconds) NotifyChanged("ActiveSeconds");
    //        }
    //    }

    //    private double _idleSeconds;
    //    public double IdleSeconds
    //    {
    //        get { return _idleSeconds; }
    //        set
    //        {
    //            var val = _idleSeconds;
    //            _idleSeconds = value;
    //            ProcessStatus();
    //            ProcessPercentages();
    //            if (val != _idleSeconds) NotifyChanged("IdleSeconds");
    //        }
    //    }

    //    private double _alertSeconds;
    //    public double AlertSeconds
    //    {
    //        get { return _alertSeconds; }
    //        set
    //        {
    //            var val = _alertSeconds;
    //            _alertSeconds = value;
    //            ProcessStatus();
    //            ProcessPercentages();
    //            if (val != _alertSeconds) NotifyChanged("AlertSeconds");
    //        }
    //    }



    //    private void ProcessPercentages()
    //    {
    //        if (TotalSeconds > 0)
    //        {
    //            ActivePercentage = ActiveSeconds / TotalSeconds;
    //            IdlePercentage = IdleSeconds / TotalSeconds;
    //            AlertPercentage = AlertSeconds / TotalSeconds;
    //        }
    //    }

    //    private double _activePercentage;
    //    public double ActivePercentage
    //    {
    //        get { return _activePercentage; }
    //        set
    //        {
    //            var val = _activePercentage;
    //            _activePercentage = value;
    //            if (val != _activePercentage) NotifyChanged("ActivePercentage");
    //        }
    //    }

    //    private double _idlePercentage;
    //    public double IdlePercentage
    //    {
    //        get { return _idlePercentage; }
    //        set
    //        {
    //            var val = _idlePercentage;
    //            _idlePercentage = value;
    //            if (val != _idlePercentage) NotifyChanged("IdlePercentage");
    //        }
    //    }

    //    private double _alertPercentage;
    //    public double AlertPercentage
    //    {
    //        get { return _alertPercentage; }
    //        set
    //        {
    //            var val = _alertPercentage;
    //            _alertPercentage = value;
    //            if (val != _alertPercentage) NotifyChanged("AlertPercentage");
    //        }
    //    }


    //    private void ProcessStatus()
    //    {
    //        if (ActiveSeconds > IdleSeconds && ActiveSeconds > AlertSeconds) Status = 2;
    //        else if (IdleSeconds > AlertSeconds && IdleSeconds > ActiveSeconds) Status = 1;
    //        else Status = 0;
    //    }

    //    private int _status;
    //    public int Status
    //    {
    //        get { return _status; }
    //        set
    //        {
    //            var val = _status;
    //            _status = value;
    //            if (val != _status) NotifyChanged("Status");
    //        }
    //    }

    //    public event PropertyChangedEventHandler PropertyChanged;

    //    private void NotifyChanged(string propertyName)
    //    {
    //        if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    //    }

    //}
}
