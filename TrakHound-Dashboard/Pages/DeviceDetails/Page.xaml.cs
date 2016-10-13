// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using TrakHound;
using TrakHound.API;
using TrakHound.API.Users;
using TrakHound.Configurations;
using TrakHound.Logging;
using TrakHound.Tools;

namespace TrakHound_Dashboard.Pages.DeviceDetails
{
    /// <summary>
    /// Interaction logic for DeviceDetails.xaml
    /// </summary>
    public partial class Page : UserControl, IPage
    {
        private const double OEE_HIGH = 0.70;
        private const double OEE_LOW = 0.50;


        public string Title { get { return "Device Details Page"; } }

        public string Description { get { return null; } }

        public Uri Image { get { return null; } }

        public bool ZoomEnabled { get { return false; } }

        public event SendData_Handler SendData;

        private UserConfiguration _userConfiguration;
        public UserConfiguration UserConfiguration
        {
            get { return _userConfiguration; }
            set { _userConfiguration = value; }
        }

        public Page(DeviceDescription device, Data.DeviceInfo deviceInfo, UserConfiguration userConfig)
        {
            InitializeComponent();
            root.DataContext = this;

            UserConfiguration = userConfig;

            Device = device;

            // Active Hour Segments
            ActiveHourDatas = new List<HourData>();
            for (var x = 0; x < 24; x++) ActiveHourDatas.Add(new HourData(x, x + 1));

            // Idle Hour Segments
            IdleHourDatas = new List<HourData>();
            for (var x = 0; x < 24; x++) IdleHourDatas.Add(new HourData(x, x + 1));

            // Alert Hour Segments
            AlertHourDatas = new List<HourData>();
            for (var x = 0; x < 24; x++) AlertHourDatas.Add(new HourData(x, x + 1));


            // Oee Hour Segments
            OeeHourDatas = new List<HourData>();
            for (var x = 0; x < 24; x++) OeeHourDatas.Add(new HourData(x, x + 1));

            // Availability Hour Segments
            AvailabilityHourDatas = new List<HourData>();
            for (var x = 0; x < 24; x++) AvailabilityHourDatas.Add(new HourData(x, x + 1));

            // Performance Hour Segments
            PerformanceHourDatas = new List<HourData>();
            for (var x = 0; x < 24; x++) PerformanceHourDatas.Add(new HourData(x, x + 1));

            // Quality Hour Segments
            QualityHourDatas = new List<HourData>();
            for (var x = 0; x < 24; x++) QualityHourDatas.Add(new HourData(x, x + 1));

            // Parts Count Hour Segments
            PartCountHourDatas = new List<HourData>();
            for (var x = 0; x < 24; x++) PartCountHourDatas.Add(new HourData(x, x + 1));

            // Initialize Device Status Pie Chart
            DeviceStatusPieChartData.Clear();
            DeviceStatusPieChartData.Add(new PieChartData("Active"));
            DeviceStatusPieChartData.Add(new PieChartData("Idle"));
            DeviceStatusPieChartData.Add(new PieChartData("Alert"));

            if (deviceInfo != null)
            {
                UpdateDeviceInfo(deviceInfo.Status);
                UpdateDeviceInfo(deviceInfo.Controller);
                UpdateDeviceInfo(deviceInfo.Oee);
                UpdateDeviceInfo(deviceInfo.Timers);
                UpdateDeviceInfo(deviceInfo.Hours);
            }

            Loading = false;
        }

        public void Initialize() { }

        public bool Opening() { return true; }

        public void Opened() { }

        public bool Closing() { return true; }

        public void Closed() { }

        public void SetZoom(double zoomPercentage) { }

        public void GetSentData(EventData data)
        {
            if (data != null && data.Id == "USER_LOGIN")
            {
                if (data.Data01.GetType() == typeof(UserConfiguration))
                {
                    _userConfiguration = (UserConfiguration)data.Data01;
                }
            }

            if (data != null && data.Id == "USER_LOGOUT")
            {
                _userConfiguration = null;
            }

            if (data != null && data.Id == "STATUS_STATUS" && data.Data02 != null && data.Data02.GetType() == typeof(Data.StatusInfo))
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    string uniqueId = data.Data01.ToString();
                    if (Device != null && Device.UniqueId == uniqueId)
                    {
                        var info = (Data.StatusInfo)data.Data02;

                        UpdateDeviceInfo(info);
                    }
                }), System.Windows.Threading.DispatcherPriority.Background, new object[] { });
            }

            if (data != null && data.Id == "STATUS_CONTROLLER" && data.Data02 != null && data.Data02.GetType() == typeof(Data.ControllerInfo))
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    string uniqueId = data.Data01.ToString();
                    if (Device != null && Device.UniqueId == uniqueId)
                    {
                        var info = (Data.ControllerInfo)data.Data02;

                        UpdateDeviceInfo(info);
                    }
                }), System.Windows.Threading.DispatcherPriority.Background, new object[] { });
            }

            if (data != null && data.Id == "STATUS_HOURS" && data.Data02 != null && data.Data02.GetType() == typeof(List<Data.HourInfo>))
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    string uniqueId = data.Data01.ToString();
                    if (Device != null && Device.UniqueId == uniqueId)
                    {
                        var info = (List<Data.HourInfo>)data.Data02;

                        UpdateDeviceInfo(info);
                    }
                }), System.Windows.Threading.DispatcherPriority.Background, new object[] { });
            }

            if (data != null && data.Id == "STATUS_TIMERS" && data.Data02 != null && data.Data02.GetType() == typeof(Data.TimersInfo))
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    string uniqueId = data.Data01.ToString();
                    if (Device != null && Device.UniqueId == uniqueId)
                    {
                        var info = (Data.TimersInfo)data.Data02;

                        UpdateDeviceInfo(info);
                    }
                }), System.Windows.Threading.DispatcherPriority.Background, new object[] { });
            }

            if (data != null && data.Id == "STATUS_OEE" && data.Data02 != null && data.Data02.GetType() == typeof(TrakHound.API.Data.OeeInfo))
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    string uniqueId = data.Data01.ToString();
                    if (Device != null && Device.UniqueId == uniqueId)
                    {
                        var info = (Data.OeeInfo)data.Data02;

                        UpdateDeviceInfo(info);
                    }
                }), System.Windows.Threading.DispatcherPriority.DataBind, new object[] { });
            }
        }


        private void UpdateDeviceInfo(Data.StatusInfo info)
        {
            if (info != null)
            {
                Connected = info.Connected == 1;
                if (!string.IsNullOrEmpty(info.DeviceStatus)) DeviceStatus = info.DeviceStatus;
                DeviceStatusTime = TimeSpan.FromSeconds(info.DeviceStatusTimer);
            }
        }

        private void UpdateDeviceInfo(Data.ControllerInfo info)
        {
            if (info != null)
            {
                EmergencyStop = info.EmergencyStop;
                ControllerMode = info.ControllerMode;
                ExecutionMode = info.ExecutionMode;
                Program = info.ProgramName;
                ProgramLine = info.ProgramLine;
                ProgramBlock = info.ProgramBlock;
                SystemStatus = info.SystemStatus;
                SystemMessage = info.SystemMessage;
            }
        }

        private void UpdateDeviceInfo(Data.OeeInfo info)
        {
            if (info != null)
            {
                Oee = info.Oee;
                Availability = info.Availability;
                Performance = info.Performance;
                Quality = info.Quality;
            }
        }

        private void UpdateDeviceInfo(Data.TimersInfo info)
        {
            if (info != null)
            {
                ActiveTime = TimeSpan.FromSeconds(info.Active);
                IdleTime = TimeSpan.FromSeconds(info.Idle);
                AlertTime = TimeSpan.FromSeconds(info.Alert);

                if (info.Total > 0)
                {
                    ActivePercentage = info.Active / info.Total;
                    IdlePercentage = info.Idle / info.Total;
                    AlertPercentage = info.Alert / info.Total;
                }

                int i = -1;

                // Update Active Pie Chart Value
                i = DeviceStatusPieChartData.ToList().FindIndex(o => o.ID == "Active");
                if (i >= 0) DeviceStatusPieChartData[i].Value = ActivePercentage;

                // Update Idle Pie Chart Value
                i = DeviceStatusPieChartData.ToList().FindIndex(o => o.ID == "Idle");
                if (i >= 0) DeviceStatusPieChartData[i].Value = IdlePercentage;

                // Update Alert Pie Chart Value
                i = DeviceStatusPieChartData.ToList().FindIndex(o => o.ID == "Alert");
                if (i >= 0) DeviceStatusPieChartData[i].Value = AlertPercentage;
            }
        }

        private void UpdateDeviceInfo(List<Data.HourInfo> info)
        {
            if (info != null)
            {
                UpdateActiveData(info);
                UpdateIdleData(info);
                UpdateAlertData(info);

                UpdateOeeStatistics(info);
                UpdateAvailabilityStatistics(info);
                UpdatePerformanceStatistics(info);
                UpdateQualityStatistics(info);
                UpdatePartCountStatistics(info);

                UpdateOeeData(info);
                UpdateAvailabilityData(info);
                UpdatePerformanceData(info);
                UpdateQualityData(info);
                UpdatePartCountData(info);
            }
        }

        private void UpdateOeeStatistics(List<Data.HourInfo> infos)
        {
            OeeDeviation = Math_Functions.StdDev(infos.Select(o => o.Oee).ToArray());

            var high = infos.OrderBy(o => o.Oee).Last();
            if (high != null)
            {
                OeeHigh = high.Oee;
                OeeHighTime = DateTime.ParseExact(GetLocalHour(high.Hour).ToString("00"), "HH", CultureInfo.CurrentCulture).ToString("h tt");
            }

            var low = infos.OrderBy(o => o.Oee).First();
            if (low != null)
            {
                OeeLow = low.Oee;
                OeeLowTime = DateTime.ParseExact(GetLocalHour(low.Hour).ToString("00"), "HH", CultureInfo.CurrentCulture).ToString("h tt");
            }
        }

        private void UpdateAvailabilityStatistics(List<Data.HourInfo> infos)
        {
            AvailabilityDeviation = Math_Functions.StdDev(infos.Select(o => o.Availability).ToArray());

            var high = infos.OrderBy(o => o.Availability).Last();
            if (high != null)
            {
                AvailabilityHigh = high.Availability;
                AvailabilityHighTime = DateTime.ParseExact(GetLocalHour(high.Hour).ToString("00"), "HH", CultureInfo.CurrentCulture).ToString("h tt");
            }

            var low = infos.OrderBy(o => o.Availability).First();
            if (low != null)
            {
                AvailabilityLow = low.Availability;
                AvailabilityLowTime = DateTime.ParseExact(GetLocalHour(low.Hour).ToString("00"), "HH", CultureInfo.CurrentCulture).ToString("h tt");
            }
        }

        private void UpdatePerformanceStatistics(List<Data.HourInfo> infos)
        {
            PerformanceDeviation = Math_Functions.StdDev(infos.Select(o => o.Performance).ToArray());

            var high = infos.OrderBy(o => o.Performance).Last();
            if (high != null)
            {
                PerformanceHigh = high.Performance;
                PerformanceHighTime = DateTime.ParseExact(GetLocalHour(high.Hour).ToString("00"), "HH", CultureInfo.CurrentCulture).ToString("h tt");
            }

            var low = infos.OrderBy(o => o.Performance).First();
            if (low != null)
            {
                PerformanceLow = low.Performance;
                PerformanceLowTime = DateTime.ParseExact(GetLocalHour(low.Hour).ToString("00"), "HH", CultureInfo.CurrentCulture).ToString("h tt");
            }
        }

        private void UpdateQualityStatistics(List<Data.HourInfo> infos)
        {
            QualityDeviation = Math_Functions.StdDev(infos.Select(o => o.Quality).ToArray());

            var high = infos.OrderBy(o => o.Quality).Last();
            if (high != null)
            {
                QualityHigh = high.Quality;
                QualityHighTime = DateTime.ParseExact(GetLocalHour(high.Hour).ToString("00"), "HH", CultureInfo.CurrentCulture).ToString("h tt");
            }

            var low = infos.OrderBy(o => o.Quality).First();
            if (low != null)
            {
                QualityLow = low.Quality;
                QualityLowTime = DateTime.ParseExact(GetLocalHour(low.Hour).ToString("00"), "HH", CultureInfo.CurrentCulture).ToString("h tt");
            }
        }

        private void UpdatePartCountStatistics(List<Data.HourInfo> infos)
        {
            PartCountTotal = infos.Select(o => o.TotalPieces).Sum();
            PartCountAverage = infos.Select(o => o.TotalPieces).Average();

            var high = infos.OrderBy(o => o.TotalPieces).Last();
            if (high != null)
            {
                PartCountHigh = high.TotalPieces;
                PartCountHighTime = DateTime.ParseExact(GetLocalHour(high.Hour).ToString("00"), "HH", CultureInfo.CurrentCulture).ToString("h tt");
            }

            var low = infos.OrderBy(o => o.TotalPieces).First();
            if (low != null)
            {
                PartCountLow = low.TotalPieces;
                PartCountLowTime = DateTime.ParseExact(GetLocalHour(low.Hour).ToString("00"), "HH", CultureInfo.CurrentCulture).ToString("h tt");
            }
        }

        private static int GetLocalHour(int utcHour)
        {
            int timeZoneOffset = (DateTime.UtcNow - DateTime.Now).Hours;
            int h = utcHour - timeZoneOffset;
            if (h < 0) h = 24 - Math.Abs(h);
            return h;
        }

        #region "Dependency Properties"

        public bool Loading
        {
            get { return (bool)GetValue(LoadingProperty); }
            set { SetValue(LoadingProperty, value); }
        }

        public static readonly DependencyProperty LoadingProperty =
            DependencyProperty.Register("Loading", typeof(bool), typeof(Page), new PropertyMetadata(true));


        public bool Connected
        {
            get { return (bool)GetValue(ConnectedProperty); }
            set { SetValue(ConnectedProperty, value); }
        }

        public static readonly DependencyProperty ConnectedProperty =
            DependencyProperty.Register("Connected", typeof(bool), typeof(Page), new PropertyMetadata(false));


        public DeviceDescription Device
        {
            get { return (DeviceDescription)GetValue(DeviceProperty); }
            set
            {
                SetValue(DeviceProperty, value);

                if (value != null)
                {
                    var device = value;

                    // Load Device Logo
                    if (!string.IsNullOrEmpty(device.Description.LogoUrl)) LoadDeviceLogo(device.Description.LogoUrl);

                    // Load Device Image
                    if (!string.IsNullOrEmpty(device.Description.ImageUrl)) LoadDeviceImage(device.Description.ImageUrl);
                }
            }
        }

        public static readonly DependencyProperty DeviceProperty =
            DependencyProperty.Register("Device", typeof(DeviceDescription), typeof(Page), new PropertyMetadata(null));


        public string DeviceStatus
        {
            get { return (string)GetValue(DeviceStatusProperty); }
            set { SetValue(DeviceStatusProperty, value); }
        }

        public static readonly DependencyProperty DeviceStatusProperty =
            DependencyProperty.Register("DeviceStatus", typeof(string), typeof(Page), new PropertyMetadata(null));


        public TimeSpan DeviceStatusTime
        {
            get { return (TimeSpan)GetValue(DeviceStatusTimeProperty); }
            set { SetValue(DeviceStatusTimeProperty, value); }
        }

        public static readonly DependencyProperty DeviceStatusTimeProperty =
            DependencyProperty.Register("DeviceStatusTime", typeof(TimeSpan), typeof(Page), new PropertyMetadata(TimeSpan.Zero));


        #region "Device Status"

        public double ActivePercentage
        {
            get { return (double)GetValue(ActivePercentageProperty); }
            set { SetValue(ActivePercentageProperty, value); }
        }

        public static readonly DependencyProperty ActivePercentageProperty =
            DependencyProperty.Register("ActivePercentage", typeof(double), typeof(Page), new PropertyMetadata(0d));

        public TimeSpan ActiveTime
        {
            get { return (TimeSpan)GetValue(ActiveTimeProperty); }
            set { SetValue(ActiveTimeProperty, value); }
        }

        public static readonly DependencyProperty ActiveTimeProperty =
            DependencyProperty.Register("ActiveTime", typeof(TimeSpan), typeof(Page), new PropertyMetadata(TimeSpan.Zero));

        public List<HourData> ActiveHourDatas
        {
            get { return (List<HourData>)GetValue(ActiveHourDatasProperty); }
            set { SetValue(ActiveHourDatasProperty, value); }
        }

        public static readonly DependencyProperty ActiveHourDatasProperty =
            DependencyProperty.Register("ActiveHourDatas", typeof(List<HourData>), typeof(Page), new PropertyMetadata(null));



        public double IdlePercentage
        {
            get { return (double)GetValue(IdlePercentageProperty); }
            set { SetValue(IdlePercentageProperty, value); }
        }

        public static readonly DependencyProperty IdlePercentageProperty =
            DependencyProperty.Register("IdlePercentage", typeof(double), typeof(Page), new PropertyMetadata(0d));

        public TimeSpan IdleTime
        {
            get { return (TimeSpan)GetValue(IdleTimeProperty); }
            set { SetValue(IdleTimeProperty, value); }
        }

        public static readonly DependencyProperty IdleTimeProperty =
            DependencyProperty.Register("IdleTime", typeof(TimeSpan), typeof(Page), new PropertyMetadata(TimeSpan.Zero));

        public List<HourData> IdleHourDatas
        {
            get { return (List<HourData>)GetValue(IdleHourDatasProperty); }
            set { SetValue(IdleHourDatasProperty, value); }
        }

        public static readonly DependencyProperty IdleHourDatasProperty =
            DependencyProperty.Register("IdleHourDatas", typeof(List<HourData>), typeof(Page), new PropertyMetadata(null));



        public double AlertPercentage
        {
            get { return (double)GetValue(AlertPercentageProperty); }
            set { SetValue(AlertPercentageProperty, value); }
        }

        public static readonly DependencyProperty AlertPercentageProperty =
            DependencyProperty.Register("AlertPercentage", typeof(double), typeof(Page), new PropertyMetadata(0d));


        public TimeSpan AlertTime
        {
            get { return (TimeSpan)GetValue(AlertTimeProperty); }
            set { SetValue(AlertTimeProperty, value); }
        }

        public static readonly DependencyProperty AlertTimeProperty =
            DependencyProperty.Register("AlertTime", typeof(TimeSpan), typeof(Page), new PropertyMetadata(TimeSpan.Zero));

        public List<HourData> AlertHourDatas
        {
            get { return (List<HourData>)GetValue(AlertHourDatasProperty); }
            set { SetValue(AlertHourDatasProperty, value); }
        }

        public static readonly DependencyProperty AlertHourDatasProperty =
            DependencyProperty.Register("AlertHourDatas", typeof(List<HourData>), typeof(Page), new PropertyMetadata(null));

        #endregion

        #region "Oee Analysis"

        #region "Oee"

        public double Oee
        {
            get { return (double)GetValue(OeeProperty); }
            set
            {
                SetValue(OeeProperty, value);

                if (value > OEE_HIGH) OeeStatus = 2;
                else if (value > OEE_LOW) OeeStatus = 1;
                else OeeStatus = 0;
            }
        }

        public static readonly DependencyProperty OeeProperty =
            DependencyProperty.Register("Oee", typeof(double), typeof(Page), new PropertyMetadata(0d));

        public int OeeStatus
        {
            get { return (int)GetValue(OeeStatusProperty); }
            set { SetValue(OeeStatusProperty, value); }
        }

        public static readonly DependencyProperty OeeStatusProperty =
            DependencyProperty.Register("OeeStatus", typeof(int), typeof(Page), new PropertyMetadata(0));

        public double OeeDeviation
        {
            get { return (double)GetValue(OeeDeviationProperty); }
            set { SetValue(OeeDeviationProperty, value); }
        }

        public static readonly DependencyProperty OeeDeviationProperty =
            DependencyProperty.Register("OeeDeviation", typeof(double), typeof(Page), new PropertyMetadata(0d));

        public double OeeHigh
        {
            get { return (double)GetValue(OeeHighProperty); }
            set { SetValue(OeeHighProperty, value); }
        }

        public static readonly DependencyProperty OeeHighProperty =
            DependencyProperty.Register("OeeHigh", typeof(double), typeof(Page), new PropertyMetadata(0d));

        public string OeeHighTime
        {
            get { return (string)GetValue(OeeHighTimeProperty); }
            set { SetValue(OeeHighTimeProperty, value); }
        }

        public static readonly DependencyProperty OeeHighTimeProperty =
            DependencyProperty.Register("OeeHighTime", typeof(string), typeof(Page), new PropertyMetadata(null));

        public double OeeLow
        {
            get { return (double)GetValue(OeeLowProperty); }
            set { SetValue(OeeLowProperty, value); }
        }

        public static readonly DependencyProperty OeeLowProperty =
            DependencyProperty.Register("OeeLow", typeof(double), typeof(Page), new PropertyMetadata(0d));

        public string OeeLowTime
        {
            get { return (string)GetValue(OeeLowTimeProperty); }
            set { SetValue(OeeLowTimeProperty, value); }
        }

        public static readonly DependencyProperty OeeLowTimeProperty =
            DependencyProperty.Register("OeeLowTime", typeof(string), typeof(Page), new PropertyMetadata(null));

        public List<HourData> OeeHourDatas
        {
            get { return (List<HourData>)GetValue(OeeHourDatasProperty); }
            set { SetValue(OeeHourDatasProperty, value); }
        }

        public static readonly DependencyProperty OeeHourDatasProperty =
            DependencyProperty.Register("OeeHourDatas", typeof(List<HourData>), typeof(Page), new PropertyMetadata(null));

        #endregion

        #region "Availability"

        public double Availability
        {
            get { return (double)GetValue(AvailabilityProperty); }
            set
            {
                SetValue(AvailabilityProperty, value);

                if (value > OEE_HIGH) AvailabilityStatus = 2;
                else if (value > OEE_LOW) AvailabilityStatus = 1;
                else AvailabilityStatus = 0;
            }
        }

        public static readonly DependencyProperty AvailabilityProperty =
            DependencyProperty.Register("Availability", typeof(double), typeof(Page), new PropertyMetadata(0d));

        public int AvailabilityStatus
        {
            get { return (int)GetValue(AvailabilityStatusProperty); }
            set { SetValue(AvailabilityStatusProperty, value); }
        }

        public static readonly DependencyProperty AvailabilityStatusProperty =
            DependencyProperty.Register("AvailabilityStatus", typeof(int), typeof(Page), new PropertyMetadata(0));

        public double AvailabilityDeviation
        {
            get { return (double)GetValue(AvailabilityDeviationProperty); }
            set { SetValue(AvailabilityDeviationProperty, value); }
        }

        public static readonly DependencyProperty AvailabilityDeviationProperty =
            DependencyProperty.Register("AvailabilityDeviation", typeof(double), typeof(Page), new PropertyMetadata(0d));

        public double AvailabilityHigh
        {
            get { return (double)GetValue(AvailabilityHighProperty); }
            set { SetValue(AvailabilityHighProperty, value); }
        }

        public static readonly DependencyProperty AvailabilityHighProperty =
            DependencyProperty.Register("AvailabilityHigh", typeof(double), typeof(Page), new PropertyMetadata(0d));

        public string AvailabilityHighTime
        {
            get { return (string)GetValue(AvailabilityHighTimeProperty); }
            set { SetValue(AvailabilityHighTimeProperty, value); }
        }

        public static readonly DependencyProperty AvailabilityHighTimeProperty =
            DependencyProperty.Register("AvailabilityHighTime", typeof(string), typeof(Page), new PropertyMetadata(null));

        public double AvailabilityLow
        {
            get { return (double)GetValue(AvailabilityLowProperty); }
            set { SetValue(AvailabilityLowProperty, value); }
        }

        public static readonly DependencyProperty AvailabilityLowProperty =
            DependencyProperty.Register("AvailabilityLow", typeof(double), typeof(Page), new PropertyMetadata(0d));

        public string AvailabilityLowTime
        {
            get { return (string)GetValue(AvailabilityLowTimeProperty); }
            set { SetValue(AvailabilityLowTimeProperty, value); }
        }

        public static readonly DependencyProperty AvailabilityLowTimeProperty =
            DependencyProperty.Register("AvailabilityLowTime", typeof(string), typeof(Page), new PropertyMetadata(null));

        public List<HourData> AvailabilityHourDatas
        {
            get { return (List<HourData>)GetValue(AvailabilityHourDatasProperty); }
            set { SetValue(AvailabilityHourDatasProperty, value); }
        }

        public static readonly DependencyProperty AvailabilityHourDatasProperty =
            DependencyProperty.Register("AvailabilityHourDatas", typeof(List<HourData>), typeof(Page), new PropertyMetadata(null));

        #endregion

        #region "Performance"

        public double Performance
        {
            get { return (double)GetValue(PerformanceProperty); }
            set
            {
                SetValue(PerformanceProperty, value);

                if (value > OEE_HIGH) PerformanceStatus = 2;
                else if (value > OEE_LOW) PerformanceStatus = 1;
                else PerformanceStatus = 0;
            }
        }

        public static readonly DependencyProperty PerformanceProperty =
            DependencyProperty.Register("Performance", typeof(double), typeof(Page), new PropertyMetadata(0d));

        public int PerformanceStatus
        {
            get { return (int)GetValue(PerformanceStatusProperty); }
            set { SetValue(PerformanceStatusProperty, value); }
        }

        public static readonly DependencyProperty PerformanceStatusProperty =
            DependencyProperty.Register("PerformanceStatus", typeof(int), typeof(Page), new PropertyMetadata(0));

        public double PerformanceDeviation
        {
            get { return (double)GetValue(PerformanceDeviationProperty); }
            set { SetValue(PerformanceDeviationProperty, value); }
        }

        public static readonly DependencyProperty PerformanceDeviationProperty =
            DependencyProperty.Register("PerformanceDeviation", typeof(double), typeof(Page), new PropertyMetadata(0d));

        public double PerformanceHigh
        {
            get { return (double)GetValue(PerformanceHighProperty); }
            set { SetValue(PerformanceHighProperty, value); }
        }

        public static readonly DependencyProperty PerformanceHighProperty =
            DependencyProperty.Register("PerformanceHigh", typeof(double), typeof(Page), new PropertyMetadata(0d));

        public string PerformanceHighTime
        {
            get { return (string)GetValue(PerformanceHighTimeProperty); }
            set { SetValue(PerformanceHighTimeProperty, value); }
        }

        public static readonly DependencyProperty PerformanceHighTimeProperty =
            DependencyProperty.Register("PerformanceHighTime", typeof(string), typeof(Page), new PropertyMetadata(null));

        public double PerformanceLow
        {
            get { return (double)GetValue(PerformanceLowProperty); }
            set { SetValue(PerformanceLowProperty, value); }
        }

        public static readonly DependencyProperty PerformanceLowProperty =
            DependencyProperty.Register("PerformanceLow", typeof(double), typeof(Page), new PropertyMetadata(0d));

        public string PerformanceLowTime
        {
            get { return (string)GetValue(PerformanceLowTimeProperty); }
            set { SetValue(PerformanceLowTimeProperty, value); }
        }

        public static readonly DependencyProperty PerformanceLowTimeProperty =
            DependencyProperty.Register("PerformanceLowTime", typeof(string), typeof(Page), new PropertyMetadata(null));

        public List<HourData> PerformanceHourDatas
        {
            get { return (List<HourData>)GetValue(PerformanceHourDatasProperty); }
            set { SetValue(PerformanceHourDatasProperty, value); }
        }

        public static readonly DependencyProperty PerformanceHourDatasProperty =
            DependencyProperty.Register("PerformanceHourDatas", typeof(List<HourData>), typeof(Page), new PropertyMetadata(null));

        #endregion

        #region "Quality"

        public double Quality
        {
            get { return (double)GetValue(QualityProperty); }
            set
            {
                SetValue(QualityProperty, value);

                if (value > OEE_HIGH) QualityStatus = 2;
                else if (value > OEE_LOW) QualityStatus = 1;
                else QualityStatus = 0;
            }
        }

        public static readonly DependencyProperty QualityProperty =
            DependencyProperty.Register("Quality", typeof(double), typeof(Page), new PropertyMetadata(0d));

        public int QualityStatus
        {
            get { return (int)GetValue(QualityStatusProperty); }
            set { SetValue(QualityStatusProperty, value); }
        }

        public static readonly DependencyProperty QualityStatusProperty =
            DependencyProperty.Register("QualityStatus", typeof(int), typeof(Page), new PropertyMetadata(0));

        public double QualityDeviation
        {
            get { return (double)GetValue(QualityDeviationProperty); }
            set { SetValue(QualityDeviationProperty, value); }
        }

        public static readonly DependencyProperty QualityDeviationProperty =
            DependencyProperty.Register("QualityDeviation", typeof(double), typeof(Page), new PropertyMetadata(0d));

        public double QualityHigh
        {
            get { return (double)GetValue(QualityHighProperty); }
            set { SetValue(QualityHighProperty, value); }
        }

        public static readonly DependencyProperty QualityHighProperty =
            DependencyProperty.Register("QualityHigh", typeof(double), typeof(Page), new PropertyMetadata(0d));

        public string QualityHighTime
        {
            get { return (string)GetValue(QualityHighTimeProperty); }
            set { SetValue(QualityHighTimeProperty, value); }
        }

        public static readonly DependencyProperty QualityHighTimeProperty =
            DependencyProperty.Register("QualityHighTime", typeof(string), typeof(Page), new PropertyMetadata(null));

        public double QualityLow
        {
            get { return (double)GetValue(QualityLowProperty); }
            set { SetValue(QualityLowProperty, value); }
        }

        public static readonly DependencyProperty QualityLowProperty =
            DependencyProperty.Register("QualityLow", typeof(double), typeof(Page), new PropertyMetadata(0d));

        public string QualityLowTime
        {
            get { return (string)GetValue(QualityLowTimeProperty); }
            set { SetValue(QualityLowTimeProperty, value); }
        }

        public static readonly DependencyProperty QualityLowTimeProperty =
            DependencyProperty.Register("QualityLowTime", typeof(string), typeof(Page), new PropertyMetadata(null));

        public List<HourData> QualityHourDatas
        {
            get { return (List<HourData>)GetValue(QualityHourDatasProperty); }
            set { SetValue(QualityHourDatasProperty, value); }
        }

        public static readonly DependencyProperty QualityHourDatasProperty =
            DependencyProperty.Register("QualityHourDatas", typeof(List<HourData>), typeof(Page), new PropertyMetadata(null));

        #endregion

        #endregion

        #region "Controller Status"

        public string EmergencyStop
        {
            get { return (string)GetValue(EmergencyStopProperty); }
            set { SetValue(EmergencyStopProperty, value); }
        }

        public static readonly DependencyProperty EmergencyStopProperty =
            DependencyProperty.Register("EmergencyStop", typeof(string), typeof(Page), new PropertyMetadata(null));

        public string ControllerMode
        {
            get { return (string)GetValue(ControllerModeProperty); }
            set { SetValue(ControllerModeProperty, value); }
        }

        public static readonly DependencyProperty ControllerModeProperty =
            DependencyProperty.Register("ControllerMode", typeof(string), typeof(Page), new PropertyMetadata(null));

        public string ExecutionMode
        {
            get { return (string)GetValue(ExecutionModeProperty); }
            set { SetValue(ExecutionModeProperty, value); }
        }

        public static readonly DependencyProperty ExecutionModeProperty =
            DependencyProperty.Register("ExecutionMode", typeof(string), typeof(Page), new PropertyMetadata(null));

        public string Program
        {
            get { return (string)GetValue(ProgramProperty); }
            set { SetValue(ProgramProperty, value); }
        }

        public static readonly DependencyProperty ProgramProperty =
            DependencyProperty.Register("Program", typeof(string), typeof(Page), new PropertyMetadata(null));

        public string ProgramLine
        {
            get { return (string)GetValue(ProgramLineProperty); }
            set { SetValue(ProgramLineProperty, value); }
        }

        public static readonly DependencyProperty ProgramLineProperty =
            DependencyProperty.Register("ProgramLine", typeof(string), typeof(Page), new PropertyMetadata(null));

        public string ProgramBlock
        {
            get { return (string)GetValue(ProgramBlockProperty); }
            set { SetValue(ProgramBlockProperty, value); }
        }

        public static readonly DependencyProperty ProgramBlockProperty =
            DependencyProperty.Register("ProgramBlock", typeof(string), typeof(Page), new PropertyMetadata(null));

        public string SystemStatus
        {
            get { return (string)GetValue(SystemStatusProperty); }
            set { SetValue(SystemStatusProperty, value); }
        }

        public static readonly DependencyProperty SystemStatusProperty =
            DependencyProperty.Register("SystemStatus", typeof(string), typeof(Page), new PropertyMetadata(null));

        public string SystemMessage
        {
            get { return (string)GetValue(SystemMessageProperty); }
            set { SetValue(SystemMessageProperty, value); }
        }

        public static readonly DependencyProperty SystemMessageProperty =
            DependencyProperty.Register("SystemMessage", typeof(string), typeof(Page), new PropertyMetadata(null));

        #endregion

        #region "Parts Count"

        public int PartCountTotal
        {
            get { return (int)GetValue(PartCountTotalProperty); }
            set { SetValue(PartCountTotalProperty, value); }
        }

        public static readonly DependencyProperty PartCountTotalProperty =
            DependencyProperty.Register("PartCountTotal", typeof(int), typeof(Page), new PropertyMetadata(0));

        public double PartCountAverage
        {
            get { return (double)GetValue(PartCountAverageProperty); }
            set { SetValue(PartCountAverageProperty, value); }
        }

        public static readonly DependencyProperty PartCountAverageProperty =
            DependencyProperty.Register("PartCountAverage", typeof(double), typeof(Page), new PropertyMetadata(0d));

        public int PartCountHigh
        {
            get { return (int)GetValue(PartCountHighProperty); }
            set { SetValue(PartCountHighProperty, value); }
        }

        public static readonly DependencyProperty PartCountHighProperty =
            DependencyProperty.Register("PartCountHigh", typeof(int), typeof(Page), new PropertyMetadata(0));

        public string PartCountHighTime
        {
            get { return (string)GetValue(PartCountHighTimeProperty); }
            set { SetValue(PartCountHighTimeProperty, value); }
        }

        public static readonly DependencyProperty PartCountHighTimeProperty =
            DependencyProperty.Register("PartCountHighTime", typeof(string), typeof(Page), new PropertyMetadata(null));

        public int PartCountLow
        {
            get { return (int)GetValue(PartCountLowProperty); }
            set { SetValue(PartCountLowProperty, value); }
        }

        public static readonly DependencyProperty PartCountLowProperty =
            DependencyProperty.Register("PartCountLow", typeof(int), typeof(Page), new PropertyMetadata(0));

        public string PartCountLowTime
        {
            get { return (string)GetValue(PartCountLowTimeProperty); }
            set { SetValue(PartCountLowTimeProperty, value); }
        }

        public static readonly DependencyProperty PartCountLowTimeProperty =
            DependencyProperty.Register("PartCountLowTime", typeof(string), typeof(Page), new PropertyMetadata(null));

        public List<HourData> PartCountHourDatas
        {
            get { return (List<HourData>)GetValue(PartCountHourDatasProperty); }
            set { SetValue(PartCountHourDatasProperty, value); }
        }

        public static readonly DependencyProperty PartCountHourDatasProperty =
            DependencyProperty.Register("PartCountHourDatas", typeof(List<HourData>), typeof(Page), new PropertyMetadata(null));

        #endregion

        #endregion

        ObservableCollection<PieChartData> _deviceStatusPieChartData;
        public ObservableCollection<PieChartData> DeviceStatusPieChartData
        {
            get
            {
                if (_deviceStatusPieChartData == null) _deviceStatusPieChartData = new ObservableCollection<PieChartData>();
                return _deviceStatusPieChartData;
            }
            set
            {
                _deviceStatusPieChartData = value;
            }
        }


        public void UpdateActiveData(List<Data.HourInfo> hours)
        {
            if (hours != null)
            {
                foreach (var hourData in ActiveHourDatas) hourData.Reset();

                foreach (var hour in hours)
                {
                    // Probably a more elegant way of getting the Time Zone Offset could be done here
                    int timeZoneOffset = (DateTime.UtcNow - DateTime.Now).Hours;
                    int h = hour.Hour - timeZoneOffset;
                    if (h < 0) h = 24 - Math.Abs(h);

                    var match = ActiveHourDatas.Find(o => o.StartHour == h);
                    if (match != null)
                    {
                        if (hour.PlannedProductionTime > 0)
                        {
                            match.Value = hour.Active / hour.PlannedProductionTime;
                            match.Status = 2;
                        }
                        else match.Status = -1;
                    }
                }
            }
        }

        public void UpdateIdleData(List<Data.HourInfo> hours)
        {
            if (hours != null)
            {
                foreach (var hourData in IdleHourDatas) hourData.Reset();

                foreach (var hour in hours)
                {
                    // Probably a more elegant way of getting the Time Zone Offset could be done here
                    int timeZoneOffset = (DateTime.UtcNow - DateTime.Now).Hours;
                    int h = hour.Hour - timeZoneOffset;
                    if (h < 0) h = 24 - Math.Abs(h);

                    var match = IdleHourDatas.Find(o => o.StartHour == h);
                    if (match != null)
                    {
                        if (hour.PlannedProductionTime > 0)
                        {
                            match.Value = hour.Idle / hour.PlannedProductionTime;
                            match.Status = 1;
                        }
                        else match.Status = -1;
                    }
                }
            }
        }

        public void UpdateAlertData(List<Data.HourInfo> hours)
        {
            if (hours != null)
            {
                foreach (var hourData in AlertHourDatas) hourData.Reset();

                foreach (var hour in hours)
                {
                    // Probably a more elegant way of getting the Time Zone Offset could be done here
                    int timeZoneOffset = (DateTime.UtcNow - DateTime.Now).Hours;
                    int h = hour.Hour - timeZoneOffset;
                    if (h < 0) h = 24 - Math.Abs(h);

                    var match = AlertHourDatas.Find(o => o.StartHour == h);
                    if (match != null)
                    {
                        if (hour.PlannedProductionTime > 0)
                        {
                            match.Value = hour.Alert / hour.PlannedProductionTime;
                            match.Status = 0;
                        }
                        else match.Status = -1;
                    }
                }
            }
        }


        public void UpdateOeeData(List<Data.HourInfo> hours)
        {
            if (hours != null)
            {
                foreach (var hourData in OeeHourDatas) hourData.Reset();

                foreach (var hour in hours)
                {
                    // Probably a more elegant way of getting the Time Zone Offset could be done here
                    int timeZoneOffset = (DateTime.UtcNow - DateTime.Now).Hours;
                    int h = hour.Hour - timeZoneOffset;
                    if (h < 0) h = 24 - Math.Abs(h);

                    var match = OeeHourDatas.Find(o => o.StartHour == h);
                    if (match != null)
                    {
                        if (hour.PlannedProductionTime > 0)
                        {
                            match.Value = hour.Oee;

                            if (match.Value > OEE_HIGH) match.Status = 2;
                            else if (match.Value > OEE_LOW) match.Status = 1;
                            else match.Status = 0;
                        }
                        else match.Status = -1;
                    }
                }
            }
        }

        public void UpdateAvailabilityData(List<Data.HourInfo> hours)
        {
            if (hours != null)
            {
                foreach (var hourData in AvailabilityHourDatas) hourData.Reset();

                foreach (var hour in hours)
                {
                    // Probably a more elegant way of getting the Time Zone Offset could be done here
                    int timeZoneOffset = (DateTime.UtcNow - DateTime.Now).Hours;
                    int h = hour.Hour - timeZoneOffset;
                    if (h < 0) h = 24 - Math.Abs(h);

                    var match = AvailabilityHourDatas.Find(o => o.StartHour == h);
                    if (match != null)
                    {
                        if (hour.PlannedProductionTime > 0)
                        {
                            match.Value = hour.Availability;

                            if (match.Value > OEE_HIGH) match.Status = 2;
                            else if (match.Value > OEE_LOW) match.Status = 1;
                            else match.Status = 0;
                        }
                        else match.Status = -1;
                    }
                }
            }
        }

        public void UpdatePerformanceData(List<Data.HourInfo> hours)
        {
            if (hours != null)
            {
                foreach (var hourData in PerformanceHourDatas) hourData.Reset();

                foreach (var hour in hours)
                {
                    // Probably a more elegant way of getting the Time Zone Offset could be done here
                    int timeZoneOffset = (DateTime.UtcNow - DateTime.Now).Hours;
                    int h = hour.Hour - timeZoneOffset;
                    if (h < 0) h = 24 - Math.Abs(h);

                    var match = PerformanceHourDatas.Find(o => o.StartHour == h);
                    if (match != null)
                    {
                        if (hour.PlannedProductionTime > 0)
                        {
                            match.Value = hour.Performance;

                            if (match.Value > OEE_HIGH) match.Status = 2;
                            else if (match.Value > OEE_LOW) match.Status = 1;
                            else match.Status = 0;
                        }
                        else match.Status = -1;
                    }
                }
            }
        }

        public void UpdateQualityData(List<Data.HourInfo> hours)
        {
            if (hours != null)
            {
                foreach (var hourData in QualityHourDatas) hourData.Reset();

                foreach (var hour in hours)
                {
                    // Probably a more elegant way of getting the Time Zone Offset could be done here
                    int timeZoneOffset = (DateTime.UtcNow - DateTime.Now).Hours;
                    int h = hour.Hour - timeZoneOffset;
                    if (h < 0) h = 24 - Math.Abs(h);

                    var match = QualityHourDatas.Find(o => o.StartHour == h);
                    if (match != null)
                    {
                        if (hour.PlannedProductionTime > 0)
                        {
                            match.Value = hour.Quality;

                            if (match.Value > OEE_HIGH) match.Status = 2;
                            else if (match.Value > OEE_LOW) match.Status = 1;
                            else match.Status = 0;
                        }
                        else match.Status = -1;
                    }
                }
            }
        }

        public void UpdatePartCountData(List<Data.HourInfo> hours)
        {
            if (hours != null)
            {
                foreach (var hourData in PartCountHourDatas) hourData.Reset();

                foreach (var hour in hours)
                {
                    // Probably a more elegant way of getting the Time Zone Offset could be done here
                    int timeZoneOffset = (DateTime.UtcNow - DateTime.Now).Hours;
                    int h = hour.Hour - timeZoneOffset;
                    if (h < 0) h = 24 - Math.Abs(h);

                    var match = PartCountHourDatas.Find(o => o.StartHour == h);
                    if (match != null)
                    {
                        if (hour.PlannedProductionTime > 0)
                        {
                            match.Value = hour.TotalPieces;
                            match.Status = 3;
                        }
                        else match.Status = -1;
                    }
                }
            }
        }

        #region "Images"

        public ImageSource DeviceImage
        {
            get { return (ImageSource)GetValue(DeviceImageProperty); }
            set { SetValue(DeviceImageProperty, value); }
        }

        public static readonly DependencyProperty DeviceImageProperty =
            DependencyProperty.Register("DeviceImage", typeof(ImageSource), typeof(Page), new PropertyMetadata(null));

        public ImageSource DeviceLogo
        {
            get { return (ImageSource)GetValue(DeviceLogoProperty); }
            set { SetValue(DeviceLogoProperty, value); }
        }

        public static readonly DependencyProperty DeviceLogoProperty =
            DependencyProperty.Register("DeviceLogo", typeof(ImageSource), typeof(Page), new PropertyMetadata(null));


        #region "Device Logo"

        public void LoadDeviceLogo(string fileId)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(LoadDeviceLogo_Worker), fileId);
        }

        void LoadDeviceLogo_Worker(object o)
        {
            BitmapSource result = null;

            if (o != null)
            {
                string fileId = o.ToString();

                System.Drawing.Image img = null;

                if (_userConfiguration != null) img = Files.DownloadImage(_userConfiguration, fileId);
                else
                {
                    string path = Path.Combine(FileLocations.Storage, fileId);
                    if (File.Exists(path)) img = System.Drawing.Image.FromFile(path);
                }

                if (img != null)
                {
                    try
                    {
                        var bmp = new System.Drawing.Bitmap(img);
                        if (bmp != null)
                        {
                            IntPtr bmpPt = bmp.GetHbitmap();
                            result = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmpPt, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                            if (result != null)
                            {
                                if (result.PixelWidth > result.PixelHeight)
                                {
                                    result = TrakHound_UI.Functions.Images.SetImageSize(result, 300);
                                }
                                else
                                {
                                    result = TrakHound_UI.Functions.Images.SetImageSize(result, 0, 80);
                                }

                                result.Freeze();
                            }
                        }
                    }
                    catch (Exception ex) { Logger.Log("Error Loading Device Image :: " + ex.Message, LogLineType.Error); }
                }
            }

            Dispatcher.BeginInvoke(new Action<BitmapSource>(LoadDeviceLogo_GUI), System.Windows.Threading.DispatcherPriority.DataBind, new object[] { result });
        }

        void LoadDeviceLogo_GUI(BitmapSource img)
        {
            DeviceLogo = img;
        }

        #endregion

        #region "Device Image"

        public void LoadDeviceImage(string fileId)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(LoadDeviceImage_Worker), fileId);
        }

        void LoadDeviceImage_Worker(object o)
        {
            BitmapSource result = null;

            if (o != null)
            {
                string fileId = o.ToString();

                System.Drawing.Image img = null;

                if (_userConfiguration != null) img = Files.DownloadImage(_userConfiguration, fileId);
                else
                {
                    string path = Path.Combine(FileLocations.Storage, fileId);
                    if (File.Exists(path)) img = System.Drawing.Image.FromFile(path);
                }

                if (img != null)
                {
                    try
                    {
                        var bmp = new System.Drawing.Bitmap(img);
                        if (bmp != null)
                        {
                            IntPtr bmpPt = bmp.GetHbitmap();
                            result = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmpPt, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                            if (result != null)
                            {
                                if (result.PixelWidth > result.PixelHeight)
                                {
                                    result = TrakHound_UI.Functions.Images.SetImageSize(result, 300);
                                }
                                else
                                {
                                    result = TrakHound_UI.Functions.Images.SetImageSize(result, 0, 150);
                                }

                                result.Freeze();
                            }
                        }
                    }
                    catch (Exception ex) { Logger.Log("Error Loading Device Image :: " + ex.Message, LogLineType.Error); }
                }
            }

            Dispatcher.BeginInvoke(new Action<BitmapSource>(LoadDeviceImage_GUI), System.Windows.Threading.DispatcherPriority.DataBind, new object[] { result });
        }

        void LoadDeviceImage_GUI(BitmapSource img)
        {
            DeviceImage = img;
        }

        #endregion

        #endregion

    }

    public class PieChartData : INotifyPropertyChanged
    {
        public PieChartData(string id)
        {
            ID = id;
        }

        public string ID { get; set; }

        private double _value;
        public double Value
        {
            get { return _value; }
            set
            {
                _value = value;
                RaisePropertyChangeEvent("Value");
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangeEvent(String propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
