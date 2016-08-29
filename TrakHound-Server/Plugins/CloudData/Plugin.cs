// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;

using TrakHound.API;
using TrakHound.API.Users;
using TrakHound.Configurations;
using TrakHound.Plugins;
using TrakHound.Plugins.Server;
using TrakHound_Server.Plugins.Status;

namespace TrakHound_Server.Plugins.CloudData
{

    public class Plugin : IServerPlugin
    {
        public string Name { get { return "Cloud Data"; } }

        public static UpdateQueue queue = new UpdateQueue();

        internal static UserConfiguration currentUser;

        private DeviceConfiguration configuration;

        private Data.DeviceInfo deviceInfo;


        public void Initialize(DeviceConfiguration config)
        {
            configuration = config;

            deviceInfo = new Data.DeviceInfo();
            deviceInfo.UniqueId = config.UniqueId;

            deviceInfo.Description.Description = config.Description.Description;
            deviceInfo.Description.DeviceId = config.Description.DeviceId;
            deviceInfo.Description.Manufacturer = config.Description.Manufacturer;
            deviceInfo.Description.Model = config.Description.Model;
            deviceInfo.Description.Serial = config.Description.Serial;
            deviceInfo.Description.Controller = config.Description.Controller;
            deviceInfo.Description.LogoUrl = config.Description.LogoUrl;
            deviceInfo.Description.ImageUrl = config.Description.ImageUrl;

            queue.Add(deviceInfo);
        }

        public void GetSentData(EventData data)
        {
            if (data != null && deviceInfo != null)
            {
                switch (data.Id)
                {
                    // Server User Changed
                    case "USER_LOGIN": UpdateUserLogin(data); break;

                    // Get Snapshot Data
                    case "SNAPSHOTS": UpdateSnapshots(data); break;

                    // Get Status Data
                    case "MTCONNECT_STATUS": UpdateMTConnectStatus(data); break;

                    // Get OEE Values
                    case "OEE": UpdateOee(data); break;

                    // Get Parts Values
                    case "PARTS": UpdatePartCount(data); break;

                    // Get Generated Event Item data
                    case "GENERATED_EVENTS": UpdateGeneratedEvents(data); break;

                    // Get Cycle Data
                    case "CYCLES": UpdateCycleData(data); break;
                }
            }
        }

        private void UpdateUserLogin(EventData data)
        {
            currentUser = (UserConfiguration)data.Data01;

            queue.Start();
        }

        private DateTime previousDeviceStatusTimestamp = DateTime.MinValue;
        private DateTime previousProductionStatusTimestamp = DateTime.MinValue;

        private void UpdateSnapshots(EventData data)
        {
            if (data.Data02 != null)
            {
                var snapshots = (List<SnapshotData.Snapshot>)data.Data02;

                // Device Status
                var snapshot = snapshots.Find(o => o.Name == "Device Status");
                if (snapshot != null)
                {
                    deviceInfo.Status.DeviceStatus = snapshot.Value;
                    deviceInfo.Status.DeviceStatusTimer = Math.Max(0, Convert.ToInt32((snapshot.Timestamp - snapshot.PreviousTimestamp).TotalSeconds));
                    previousDeviceStatusTimestamp = snapshot.Timestamp;
                }

                // Production Status
                snapshot = snapshots.Find(o => o.Name == "Production Status");
                if (snapshot != null)
                {
                    deviceInfo.Status.ProductionStatus = snapshot.Value;
                    deviceInfo.Status.ProductionStatusTimer = Math.Max(0, Convert.ToInt32((snapshot.Timestamp - snapshot.PreviousTimestamp).TotalSeconds));
                    previousProductionStatusTimestamp = snapshot.Timestamp;
                }


                // Day Run Time
                snapshot = snapshots.Find(o => o.Name == "Day Run Time");
                if (snapshot != null)
                {
                    double val = 0;
                    double.TryParse(snapshot.Value, out val);
                    deviceInfo.Timers.DayRun = val;
                }

                // Day Operating Time
                snapshot = snapshots.Find(o => o.Name == "Day Operating Time");
                if (snapshot != null)
                {
                    double val = 0;
                    double.TryParse(snapshot.Value, out val);
                    deviceInfo.Timers.DayOperating = val;
                }

                // Day Cutting Time
                snapshot = snapshots.Find(o => o.Name == "Day Cutting Time");
                if (snapshot != null)
                {
                    double val = 0;
                    double.TryParse(snapshot.Value, out val);
                    deviceInfo.Timers.DayCutting = val;
                }

                // Day Spindle Time
                snapshot = snapshots.Find(o => o.Name == "Day Spindle Time");
                if (snapshot != null)
                {
                    double val = 0;
                    double.TryParse(snapshot.Value, out val);
                    deviceInfo.Timers.DaySpindle = val;
                }


                // Total Run Time
                snapshot = snapshots.Find(o => o.Name == "Total Run Time");
                if (snapshot != null)
                {
                    double val = 0;
                    double.TryParse(snapshot.Value, out val);
                    deviceInfo.Timers.TotalRun = val;
                }

                // Total Operating Time
                snapshot = snapshots.Find(o => o.Name == "Total Operating Time");
                if (snapshot != null)
                {
                    double val = 0;
                    double.TryParse(snapshot.Value, out val);
                    deviceInfo.Timers.TotalOperating = val;
                }

                // Total Cutting Time
                snapshot = snapshots.Find(o => o.Name == "Total Cutting Time");
                if (snapshot != null)
                {
                    double val = 0;
                    double.TryParse(snapshot.Value, out val);
                    deviceInfo.Timers.TotalCutting = val;
                }

                // Total Spindle Time
                snapshot = snapshots.Find(o => o.Name == "Total Spindle Time");
                if (snapshot != null)
                {
                    double val = 0;
                    double.TryParse(snapshot.Value, out val);
                    deviceInfo.Timers.TotalSpindle = val;
                }

                queue.Add(deviceInfo);
            }
            else
            {
                deviceInfo.Status.DeviceStatus = "Alert";
                deviceInfo.Status.ProductionStatus = "Production";

                if (previousDeviceStatusTimestamp > DateTime.MinValue)
                {
                    deviceInfo.Status.DeviceStatusTimer += (DateTime.UtcNow - previousDeviceStatusTimestamp).TotalSeconds;
                }

                previousDeviceStatusTimestamp = DateTime.UtcNow;

                
            }
        }
        

        private class GeneratedEventTime
        {
            public string Event { get; set; }
            public string Value { get; set; }
            public DateTime Start { get; set; }
            public DateTime End { get; set; }

            public TimeSpan Duration { get { return End - Start; } }
        }

        private void UpdateGeneratedEvents(EventData data)
        {
            if (data.Data01 != null && data.Data02 != null)
            {
                var events = (List<GeneratedEvents.GeneratedEvent>)data.Data02;

                var eventTimes = new List<GeneratedEventTime>();

                // Convert into GeneratedEventTime objects separated by hour
                foreach (var e in events)
                {
                    if (e.CurrentValue != null && e.PreviousValue != null && e.PreviousValue.Timestamp > DateTime.MinValue)
                    {
                        // Check if event spans into next hour(s)
                        if (e.PreviousValue.Timestamp.Hour != e.CurrentValue.Timestamp.Hour)
                        {
                            var hourStart = e.PreviousValue.Timestamp;

                            while (hourStart < e.CurrentValue.Timestamp)
                            {
                                var hourPlus = hourStart.AddHours(1);
                                var hourEnd = new DateTime(hourPlus.Year, hourPlus.Month, hourPlus.Day, hourPlus.Hour, 0, 0);
                                if (hourEnd > e.CurrentValue.Timestamp) hourEnd = e.CurrentValue.Timestamp;

                                var eventTime = new GeneratedEventTime();
                                eventTime.Event = e.EventName;
                                eventTime.Value = e.PreviousValue.Value;
                                eventTime.Start = hourStart;
                                eventTime.End = hourEnd;
                                eventTimes.Add(eventTime);

                                hourStart = hourEnd;
                            }
                        }
                        else
                        {
                            var eventTime = new GeneratedEventTime();
                            eventTime.Event = e.EventName;
                            eventTime.Value = e.PreviousValue.Value;
                            eventTime.Start = e.PreviousValue.Timestamp;
                            eventTime.End = e.CurrentValue.Timestamp;
                            eventTimes.Add(eventTime);
                        }
                    }
                }

                // Add HourInfo objects for each GeneratedEventTime in list
                foreach (var eventTime in eventTimes)
                {
                    var hourInfo = new Data.HourInfo();
                    hourInfo.Date = eventTime.Start.ToString(Data.HourInfo.DateFormat);
                    hourInfo.Hour = eventTime.Start.Hour;
                    
                    // Device Status
                    if (eventTime.Event == "device_status" && !string.IsNullOrEmpty(eventTime.Value))
                    {
                        hourInfo.TotalTime = eventTime.Duration.TotalSeconds;

                        switch (eventTime.Value.ToLower())
                        {
                            case "active": hourInfo.Active = eventTime.Duration.TotalSeconds; break;
                            case "idle": hourInfo.Idle = eventTime.Duration.TotalSeconds; break;
                            case "alert": hourInfo.Alert = eventTime.Duration.TotalSeconds; break;
                        }
                    }

                    // Production Status
                    if (eventTime.Event == "production_status" && !string.IsNullOrEmpty(eventTime.Value))
                    {
                        //hourInfo.TotalTime = eventTime.Duration.TotalSeconds;

                        switch (eventTime.Value.ToLower())
                        {
                            case "production": hourInfo.Production = eventTime.Duration.TotalSeconds; break;
                            case "setup": hourInfo.Setup = eventTime.Duration.TotalSeconds; break;
                            case "teardown": hourInfo.Teardown = eventTime.Duration.TotalSeconds; break;
                            case "maintenance": hourInfo.Maintenance = eventTime.Duration.TotalSeconds; break;
                            case "process_development": hourInfo.ProcessDevelopment = eventTime.Duration.TotalSeconds; break;
                        }
                    }

                    deviceInfo.Hours.Add(hourInfo);
                }
            }
        }


        private void UpdateMTConnectStatus(EventData data)
        {
            if (data.Data02 != null)
            {
                var infos = (List<StatusInfo>)data.Data02;
                StatusInfo info = null;

                // Availability
                info = infos.Find(x => x.Type == "AVAILABILITY");
                if (info != null)
                {
                    deviceInfo.Status.Connected = info.Value1 == "AVAILABLE" ? 1 : 0;
                    deviceInfo.Controller.Availability = info.Value1;
                }

                // Controller Mode
                info = infos.Find(x => x.Type == "CONTROLLER_MODE");
                if (info != null) deviceInfo.Controller.ControllerMode = info.Value1;

                // Emergency Stop
                info = infos.Find(x => x.Type == "EMERGENCY_STOP");
                if (info != null) deviceInfo.Controller.EmergencyStop = info.Value1;

                // Execution Mode
                info = infos.Find(x => x.Type == "EXECUTION");
                if (info != null) deviceInfo.Controller.ExecutionMode = info.Value1;

                // System status
                info = infos.Find(x => x.Type == "SYSTEM");
                if (info != null) deviceInfo.Controller.SystemMessage = info.Value1;
                if (info != null) deviceInfo.Controller.SystemStatus = info.Value2;

                // Program Name
                info = infos.Find(x => x.Type == "PROGRAM");
                if (info != null) deviceInfo.Controller.ProgramName = info.Value1;
            }
            else
            {
                deviceInfo.Status.Connected = 0;
                deviceInfo.Controller.Availability = "UNAVAILABLE";
                deviceInfo.Controller.ControllerMode = "UNAVAILABLE";
                deviceInfo.Controller.EmergencyStop = "UNAVAILABLE";
                deviceInfo.Controller.ExecutionMode = "UNAVAILABLE";
                deviceInfo.Controller.SystemMessage = "UNAVAILABLE";
                deviceInfo.Controller.SystemStatus = "UNAVAILABLE";
                deviceInfo.Controller.ProgramName = "UNAVAILABLE";
            }

            queue.Add(deviceInfo);
        }


        private List<OEE.OEEData> storedOeeDatas = new List<OEE.OEEData>();

        private void UpdateOee(EventData data)
        {
            if (data.Data01 != null && data.Data02 != null)
            {
                var oeeDatas = (List<OEE.OEEData>)data.Data02;
                if (oeeDatas != null)
                {
                    //var oee = new TrakHound.OEE();
                    //oee.ConstantQuality = 1;

                    foreach (var oeeData in oeeDatas.ToList())
                    {
                        var storedOeeData = storedOeeDatas.Find(o => o.Start.Hour == oeeData.Start.Hour && o.CycleId == oeeData.CycleId && o.CycleInstanceId == oeeData.CycleInstanceId);
                        if (storedOeeData == null)
                        {
                            storedOeeData = new OEE.OEEData();
                            storedOeeDatas.Add(oeeData);
                        }

                        var hourInfo = new Data.HourInfo();
                        hourInfo.Date = oeeData.Start.ToString(Data.HourInfo.DateFormat);
                        hourInfo.Hour = oeeData.Start.Hour;
                        hourInfo.PlannedProductionTime = Math.Max(0, oeeData.PlannedProductionTime - storedOeeData.PlannedProductionTime);
                        hourInfo.OperatingTime = Math.Max(0, oeeData.OperatingTime - storedOeeData.OperatingTime);
                        hourInfo.IdealOperatingTime = Math.Max(0, oeeData.IdealOperatingTime - storedOeeData.IdealOperatingTime);
                        //hourInfo.TotalPieces = Math.Max(0, oeeData.TotalPieces - storedOeeData.TotalPieces);
                        //hourInfo.GoodPieces = Math.Max(0, oeeData.GoodPieces - storedOeeData.GoodPieces);

                        // Update in stored list
                        int i = storedOeeDatas.FindIndex(o => o.Start.Hour == oeeData.Start.Hour && o.CycleId == oeeData.CycleId && o.CycleInstanceId == oeeData.CycleInstanceId);
                        if (i >= 0) storedOeeDatas[i] = oeeData;

                        deviceInfo.Hours.Add(hourInfo);
                    }

                    queue.Add(deviceInfo);
                }
            }
        }


        private int storedPartCount = 0;

        private void UpdatePartCount(EventData data)
        {
            if (data.Data01 != null && data.Data02 != null)
            {
                var infos = (List<Parts.PartInfo>)data.Data02;
                if (infos != null)
                {
                    var pc = Parts.Configuration.Get(configuration);
                    if (pc != null)
                    {
                        foreach (var info in infos)
                        {
                            var hourInfo = new Data.HourInfo();
                            hourInfo.Date = info.Timestamp.ToString(Data.HourInfo.DateFormat);
                            hourInfo.Hour = info.Timestamp.Hour;

                            if (pc.CalculationType == Parts.CalculationType.Incremental)
                            {
                                hourInfo.TotalPieces = info.Count;

                                deviceInfo.Hours.Add(hourInfo);
                            }
                            else
                            {
                                if (info.Count != storedPartCount)
                                {
                                    hourInfo.TotalPieces = info.Count - storedPartCount;

                                    deviceInfo.Hours.Add(hourInfo);
                                    storedPartCount = info.Count;
                                }
                            }                         
                        }

                        queue.Add(deviceInfo);
                    } 
                }
            }
        }

        private void UpdateCycleData(EventData data)
        {
            if (data.Data01 != null && data.Data02 != null)
            {
                var cycleDatas = (List<Cycles.CycleData>)data.Data02;
                if (cycleDatas != null)
                {
                    var cyclesInfo = new Data.CyclesInfo();

                    foreach (var cycleData in cycleDatas)
                    {
                        var cycleInfo = new Data.CyclesInfo.CycleInfo();
                        cycleInfo.CycleId = cycleData.CycleId;
                        cycleInfo.InstanceId = cycleData.InstanceId;
                        cycleInfo.CycleName = cycleData.Name;
                        cycleInfo.CycleEvent = cycleData.Event;
                        cycleInfo.Start = cycleData.Start;
                        cycleInfo.End = cycleData.Stop;
                        cycleInfo.Duration = cycleData.Duration.TotalSeconds;

                        cyclesInfo.Cycles.Add(cycleInfo);
                    }

                    deviceInfo.Cycles = cyclesInfo;

                    queue.Add(deviceInfo);
                }
            }
        }
        
        public event SendData_Handler SendData;

        public void Starting() { }

        public void Closing()
        {
            queue.Remove(deviceInfo);
        }
    }

}
