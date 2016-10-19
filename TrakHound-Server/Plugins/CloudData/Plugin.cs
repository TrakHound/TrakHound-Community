// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;

using TrakHound;
using TrakHound.API;
using TrakHound.API.Users;
using TrakHound.Configurations;
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

        private object _lock = new object();


        public void Initialize(DeviceConfiguration config)
        {
            configuration = config;

            deviceInfo = new Data.DeviceInfo();
            deviceInfo.UniqueId = config.UniqueId;
            deviceInfo.Enabled = config.Enabled;
            deviceInfo.Index = config.Index;
        }

        public void GetSentData(EventData data)
        {
            if (data != null && deviceInfo != null)
            {
                switch (data.Id)
                {
                    // Add Custom Device Data Class to DeviceInfo object
                    case "ADD_DEVICE_DATA": AddData(data); break;

                    // Get Cycle Data
                    //case "CYCLES": UpdateCycleData(data); break;

                    // Get Device Availability
                    case "DEVICE_AVAILABILITY": UpdateDeviceAvailability(data); break;

                    // Get Generated Event Item data
                    case "GENERATED_EVENTS": UpdateGeneratedEvents(data); break;

                    // Get Status Data
                    case "MTCONNECT_STATUS": UpdateMTConnectStatus(data); break;

                    // Get OEE Values
                    case "OEE": UpdateOee(data); break;

                    // Get Override Values
                    case "OVERRIDE_ITEMS": UpdateOverrideData(data); break;

                    // Get Parts Values
                    case "PARTS": UpdatePartCount(data); break;

                    // Get Server Stopped
                    case "SERVER_STOPPED": UpdateServerStopped(data); break;

                    // Get Snapshot Data
                    case "SNAPSHOTS": UpdateSnapshots(data); break;

                    // Server User Changed
                    case "USER_LOGIN": UpdateUserLogin(data); break;
                }
            }
        }

        private void UpdateUserLogin(EventData data)
        {
            currentUser = (UserConfiguration)data.Data01;

            queue.Start();
        }

        private void UpdateServerStopped(EventData data)
        {
            queue.Stop();
        }

        private void AddData(EventData data)
        {
            if (data.Data02 != null && data.Data03 != null)
            {
                deviceInfo.AddClass(data.Data02.ToString(), data.Data03);
            }
        }

        private void UpdateDeviceAvailability(EventData data)
        {
            if (data.Data02 != null)
            {
                bool availability = (bool)data.Data02;

                if (deviceInfo.Status == null) deviceInfo.Status = new Data.StatusInfo();
                deviceInfo.Status.Connected = availability ? 1 : 0;

                if (!availability || deviceInfo.Controller == null) deviceInfo.Controller = new Data.ControllerInfo();
                deviceInfo.Controller.Availability = availability ? "AVAILABLE" : "UNAVAILABLE";
            }
        }

        private DateTime previousDeviceStatusTimestamp = DateTime.MinValue;
        private DateTime previousProductionStatusTimestamp = DateTime.MinValue;

        private void UpdateSnapshots(EventData data)
        {
            if (data.Data02 != null)
            {
                lock(_lock)
                {
                    var snapshots = (List<SnapshotData.Snapshot>)data.Data02;

                    // Only update if deviceInfo.Status is not null since this doesn't always include the Connected property which causes 
                    // the device to appear to be turning on and off using a client
                    if (deviceInfo.Status != null)
                    {
                        // Day Run Time
                        var snapshot = snapshots.Find(o => o.Name == "Day Run Time");
                        if (snapshot != null)
                        {
                            double val = 0;
                            double.TryParse(snapshot.Value, out val);
                            deviceInfo.Status.DayRun = Math.Round(val, 2);
                        }

                        // Day Operating Time
                        snapshot = snapshots.Find(o => o.Name == "Day Operating Time");
                        if (snapshot != null)
                        {
                            double val = 0;
                            double.TryParse(snapshot.Value, out val);
                            deviceInfo.Status.DayOperating = Math.Round(val, 2);
                        }

                        // Day Cutting Time
                        snapshot = snapshots.Find(o => o.Name == "Day Cutting Time");
                        if (snapshot != null)
                        {
                            double val = 0;
                            double.TryParse(snapshot.Value, out val);
                            deviceInfo.Status.DayCutting = Math.Round(val, 2);
                        }

                        // Day Spindle Time
                        snapshot = snapshots.Find(o => o.Name == "Day Spindle Time");
                        if (snapshot != null)
                        {
                            double val = 0;
                            double.TryParse(snapshot.Value, out val);
                            deviceInfo.Status.DaySpindle = Math.Round(val, 2);
                        }


                        // Total Run Time
                        snapshot = snapshots.Find(o => o.Name == "Total Run Time");
                        if (snapshot != null)
                        {
                            double val = 0;
                            double.TryParse(snapshot.Value, out val);
                            deviceInfo.Status.TotalRun = Math.Round(val, 2);
                        }

                        // Total Operating Time
                        snapshot = snapshots.Find(o => o.Name == "Total Operating Time");
                        if (snapshot != null)
                        {
                            double val = 0;
                            double.TryParse(snapshot.Value, out val);
                            deviceInfo.Status.TotalOperating = Math.Round(val, 2);
                        }

                        // Total Cutting Time
                        snapshot = snapshots.Find(o => o.Name == "Total Cutting Time");
                        if (snapshot != null)
                        {
                            double val = 0;
                            double.TryParse(snapshot.Value, out val);
                            deviceInfo.Status.TotalCutting = Math.Round(val, 2);
                        }

                        // Total Spindle Time
                        snapshot = snapshots.Find(o => o.Name == "Total Spindle Time");
                        if (snapshot != null)
                        {
                            double val = 0;
                            double.TryParse(snapshot.Value, out val);
                            deviceInfo.Status.TotalSpindle = Math.Round(val, 2);
                        }

                        queue.Add(deviceInfo);
                    }
                }
            }
        }
        

        private void UpdateGeneratedEvents(EventData data)
        {
            if (data.Data01 != null && data.Data02 != null)
            {
                var gEvents = (List<GeneratedEvents.GeneratedEvent>)data.Data02;

                // Only update if deviceInfo.Status is not null since this doesn't always include the Connected property which causes 
                // the device to appear to be turning on and off using a client
                if (deviceInfo.Status != null)
                {
                    // Update Generated Event Status Info
                    var statusInfo = GeneratedEventStatusInfo.Get(gEvents);
                    deviceInfo.Status.DeviceStatus = statusInfo.DeviceStatus;
                    deviceInfo.Status.ProductionStatus = statusInfo.ProductionStatus;
                    deviceInfo.Status.DeviceStatusTimer = statusInfo.DeviceStatusTimer;
                    deviceInfo.Status.ProductionStatusTimer = statusInfo.ProductionStatusTimer;
                }

                // Update Generated Event Times
                deviceInfo.AddHourInfos(GeneratedEventTime.GetHourInfos(gEvents));

                queue.Add(deviceInfo);
            }
        }

        private class GeneratedEventStatusInfo
        {
            public string DeviceStatus { get; set; }
            public string ProductionStatus { get; set; }

            public double DeviceStatusTimer { get; set; }
            public double ProductionStatusTimer { get; set; }

            public static GeneratedEventStatusInfo Get(List<GeneratedEvents.GeneratedEvent> gEvents)
            {
                var statusInfo = new GeneratedEventStatusInfo();

                // Get Device Status
                var deviceStatusEvents = gEvents.FindAll(o => o.EventName == "device_status" && o.CurrentValue != null);
                if (deviceStatusEvents.Any())
                {
                    var deviceStatusEvent = deviceStatusEvents.OrderBy(o => o.CurrentValue.Timestamp).Last();
                    if (deviceStatusEvent != null)
                    {
                        statusInfo.DeviceStatus = deviceStatusEvent.CurrentValue.Value;
                        statusInfo.DeviceStatusTimer = (deviceStatusEvent.CurrentValue.Timestamp - deviceStatusEvent.CurrentValue.ChangedTimestamp).TotalSeconds;
                    }
                }

                // Get Production Status
                var productionStatusEvents = gEvents.FindAll(o => o.EventName == "production_status" && o.CurrentValue != null);
                if (productionStatusEvents.Any())
                {
                    var productionStatusEvent = productionStatusEvents.OrderBy(o => o.CurrentValue.Timestamp).Last();
                    if (productionStatusEvent != null)
                    {
                        statusInfo.ProductionStatus = productionStatusEvent.CurrentValue.Value;
                        statusInfo.ProductionStatusTimer = (productionStatusEvent.CurrentValue.Timestamp - productionStatusEvent.CurrentValue.ChangedTimestamp).TotalSeconds;
                    }
                }

                return statusInfo;
            }
        }

        private class GeneratedEventTime
        {
            public string Event { get; set; }
            public string Value { get; set; }
            public DateTime Start { get; set; }
            public DateTime End { get; set; }

            public TimeSpan Duration { get { return End - Start; } }

            public static List<Data.HourInfo> GetHourInfos(List<GeneratedEvents.GeneratedEvent> gEvents)
            {
                var hours = new List<Data.HourInfo>();

                foreach (var gEvent in gEvents)
                {
                    if (gEvent.CurrentValue != null)
                    {
                        var hourInfo = new Data.HourInfo();
                        hourInfo.Date = gEvent.CurrentValue.Timestamp.ToString(Data.HourInfo.DateFormat);
                        hourInfo.Hour = gEvent.CurrentValue.Timestamp.Hour;

                        double duration = Math.Round(gEvent.Duration.TotalSeconds, 2);

                        // Device Status
                        if (gEvent.EventName == "device_status" && !string.IsNullOrEmpty(gEvent.CurrentValue.Value))
                        {
                            switch (gEvent.CurrentValue.Value.ToLower())
                            {
                                case "active": hourInfo.Active = duration; break;
                                case "idle": hourInfo.Idle = duration; break;
                                case "alert": hourInfo.Alert = duration; break;
                            }
                        }

                        // Production Status
                        if (gEvent.EventName == "production_status" && !string.IsNullOrEmpty(gEvent.CurrentValue.Value))
                        {
                            switch (gEvent.CurrentValue.Value.ToLower())
                            {
                                case "production": hourInfo.Production = duration; break;
                                case "setup": hourInfo.Setup = duration; break;
                                case "teardown": hourInfo.Teardown = duration; break;
                                case "maintenance": hourInfo.Maintenance = duration; break;
                                case "process_development": hourInfo.ProcessDevelopment = duration; break;
                            }
                        }

                        hours.Add(hourInfo);
                    }
                }

                return hours;
            }
        }

        private void UpdateMTConnectStatus(EventData data)
        {
            if (data.Data02 != null)
            {
                lock (_lock)
                {
                    if (deviceInfo.Status == null) deviceInfo.Status = new Data.StatusInfo();
                    if (deviceInfo.Controller == null) deviceInfo.Controller = new Data.ControllerInfo();

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
                    var systemStatusInfos = infos.FindAll(x => x.Category == MTConnect.DataItemCategory.CONDITION &&
                    x.Type == "SYSTEM" && (x.Address.ToLower().Contains("controller") || x.Address.ToLower().Contains("path")));

                    var logicStatusInfos = infos.FindAll(x => x.Category == MTConnect.DataItemCategory.CONDITION &&
                    x.Type == "LOGIC_PROGRAM" && (x.Address.ToLower().Contains("controller") || x.Address.ToLower().Contains("path")));

                    var motionStatusInfos = infos.FindAll(x => x.Category == MTConnect.DataItemCategory.CONDITION &&
                    x.Type == "MOTION_PROGRAM" && (x.Address.ToLower().Contains("controller") || x.Address.ToLower().Contains("path")));

                    MTConnect.ConditionValue systemStatus = MTConnect.ConditionValue.UNAVAILABLE;
                    string systemMessage = string.Empty;

                    // Check System Type first
                    foreach (var sInfo in systemStatusInfos)
                    {
                        if (sInfo.Value2 != null)
                        {
                            MTConnect.ConditionValue s = MTConnect.ConditionValue.UNAVAILABLE;

                            switch (sInfo.Value2.ToLower())
                            {
                                case "normal": s = MTConnect.ConditionValue.NORMAL; break;
                                case "warning": s = MTConnect.ConditionValue.WARNING; break;
                                case "fault": s = MTConnect.ConditionValue.FAULT; break;
                            }

                            if (s > systemStatus)
                            {
                                systemStatus = s;
                                systemMessage = sInfo.Value1;
                            }
                        }
                    }

                    // Check Logic Program Type second
                    foreach (var sInfo in logicStatusInfos)
                    {
                        if (sInfo.Value2 != null)
                        {
                            MTConnect.ConditionValue s = MTConnect.ConditionValue.UNAVAILABLE;

                            switch (sInfo.Value2.ToLower())
                            {
                                case "normal": s = MTConnect.ConditionValue.NORMAL; break;
                                case "warning": s = MTConnect.ConditionValue.WARNING; break;
                                case "fault": s = MTConnect.ConditionValue.FAULT; break;
                            }

                            if (s > systemStatus)
                            {
                                systemStatus = s;
                                systemMessage = sInfo.Value1;
                            }
                        }
                    }

                    // Check Motion Program Type third
                    foreach (var sInfo in motionStatusInfos)
                    {
                        if (sInfo.Value2 != null)
                        {
                            MTConnect.ConditionValue s = MTConnect.ConditionValue.UNAVAILABLE;

                            switch (sInfo.Value2.ToLower())
                            {
                                case "normal": s = MTConnect.ConditionValue.NORMAL; break;
                                case "warning": s = MTConnect.ConditionValue.WARNING; break;
                                case "fault": s = MTConnect.ConditionValue.FAULT; break;
                            }

                            if (s > systemStatus)
                            {
                                systemStatus = s;
                                systemMessage = sInfo.Value1;
                            }
                        }
                    }

                    deviceInfo.Controller.SystemStatus = systemStatus.ToString();
                    deviceInfo.Controller.SystemMessage = systemMessage;

                    // Program Name
                    info = infos.Find(x => x.Type == "PROGRAM");
                    if (info != null) deviceInfo.Controller.ProgramName = info.Value1;

                    // Program Block
                    info = infos.Find(x => x.Type == "BLOCK");
                    if (info != null) deviceInfo.Controller.ProgramBlock = info.Value1;

                    // Program Line
                    info = infos.Find(x => x.Type == "LINE");
                    if (info != null) deviceInfo.Controller.ProgramLine = info.Value1;

                    queue.Add(deviceInfo);
                }
            }
        }

        private void UpdateOee(EventData data)
        {
            if (data.Data01 != null && data.Data02 != null)
            {
                var oeeDatas = (List<OEE.OEEData>)data.Data02;
                if (oeeDatas != null)
                {
                    foreach (var oeeData in oeeDatas)
                    {
                        var info = new Data.HourInfo();
                        info.Date = oeeData.Timestamp.ToString(Data.HourInfo.DateFormat);
                        info.Hour = oeeData.Timestamp.Hour;
                        info.PlannedProductionTime = Math.Round(Math.Max(0, oeeData.PlannedProductionTime), 2);
                        info.OperatingTime = Math.Round(Math.Max(0, oeeData.OperatingTime), 2);
                        info.IdealOperatingTime = Math.Round(Math.Max(0, oeeData.IdealOperatingTime), 2);

                        deviceInfo.AddHourInfo(info);
                    }

                    queue.Add(deviceInfo);
                }
            }
        }

        private void UpdateOverrideData(EventData data)
        {
            if (data.Data01 != null && data.Data02 != null)
            {
                var overrides = (List<Overrides.OverrideItem>)data.Data02;
                if (overrides != null)
                {
                    var infos = new List<Data.OverrideInfo>();

                    foreach (var ovr in overrides)
                    {
                        var info = new Data.OverrideInfo();
                        info.Name = ovr.Name;
                        info.Type = ovr.Type.ToString();
                        info.Value = Math.Round(ovr.Value, 2);
                        info.Timestamp = ovr.Timestamp;

                        infos.Add(info);
                    }

                    if (infos.Count > 0)
                    {
                        if (deviceInfo.Overrides == null) deviceInfo.Overrides = new List<Data.OverrideInfo>();
                        deviceInfo.Overrides.AddRange(infos);

                        queue.Add(deviceInfo);
                    }
                }
            }
        }

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

                            hourInfo.TotalPieces = info.Count;

                            lock (_lock) deviceInfo.AddHourInfo(hourInfo);
                        }

                        queue.Add(deviceInfo);
                    }
                }
            }
        }

        private string GetHourId(string date, int hour)
        {
            return date + "_" + hour;
        }

        private Data.CycleInfo previousCycleInfo = null;

        private void UpdateCycleData(EventData data)
        {
            if (data.Data01 != null && data.Data02 != null)
            {
                var cycleDatas = (List<Cycles.CycleData>)data.Data02;
                if (cycleDatas != null)
                {
                    lock(_lock)
                    {
                        var cycleInfos = new List<Data.CycleInfo>();

                        foreach (var cycleData in cycleDatas)
                        {
                            if (cycleData.Completed && cycleData.Duration > TimeSpan.Zero)
                            {
                                var cycleInfo = new Data.CycleInfo();

                                cycleInfo.CycleId = cycleData.CycleId;
                                cycleInfo.CycleInstanceId = cycleData.InstanceId;

                                cycleInfo.CycleName = cycleData.Name;
                                cycleInfo.CycleEvent = cycleData.Event;
                                cycleInfo.ProductionType = cycleData.ProductionType.ToString();

                                cycleInfo.Start = cycleData.Start;
                                cycleInfo.Stop = cycleData.Stop;
                                cycleInfo.Duration = cycleData.Duration.TotalSeconds;

                                if (previousCycleInfo == null || cycleInfo.CycleId != previousCycleInfo.CycleId || cycleInfo.CycleInstanceId != previousCycleInfo.CycleInstanceId)
                                {
                                    cycleInfos.Add(cycleInfo);
                                }

                                previousCycleInfo = cycleInfo;
                            }
                        }

                        if (cycleInfos.Count > 0)
                        {
                            deviceInfo.AddClass("cycles", cycleInfos);

                            queue.Add(deviceInfo);
                        }
                    }
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
