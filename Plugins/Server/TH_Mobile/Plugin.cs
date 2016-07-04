// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

using TH_Configuration;
using TH_Global.Functions;
using TH_Plugins;
using TH_Plugins.Server;
using TH_Shifts;
using TH_Status;

using TH_Global.TrakHound;
using TH_Global.TrakHound.Users;

namespace TH_Mobile
{

    public class Plugin : IServerPlugin
    {
        public static UpdateQueue queue = new UpdateQueue();

        internal static UserConfiguration currentUser;


        private DeviceConfiguration configuration;

        private Data.DeviceInfo deviceInfo;

        //private string userId;

        //private UpdateData updateData;

        private bool databaseConnected;
        private bool deviceAvailable;


        public string Name { get { return "TH_Mobile"; } }

        public void Initialize(DeviceConfiguration config)
        {
            configuration = config;

            deviceInfo = new Data.DeviceInfo();
            deviceInfo.Description.Description = config.Description.Description;
            deviceInfo.Description.DeviceId = config.Description.Device_ID;
            deviceInfo.Description.Manufacturer = config.Description.Manufacturer;
            deviceInfo.Description.Model = config.Description.Model;
            deviceInfo.Description.Serial = config.Description.Serial;
            deviceInfo.Description.Controller = config.Description.Controller;
            deviceInfo.Description.ImageUrl = config.FileLocations.Image_Path;
            deviceInfo.Description.LogoUrl = config.FileLocations.Manufacturer_Logo_Path;

            queue.Add(deviceInfo);
        }

        public void GetSentData(EventData data)
        {
            if (data != null && deviceInfo != null)
            {
                switch (data.Id.ToLower())
                {
                    // Server User Changed
                    case "userlogin": UpdateUserLogin(data); break;

                    // Update Database Connection Status
                    case "databasestatus": UpdateDatabaseStatus(data); break;

                    // Update Device Availability (MTConnect) Status
                    case "deviceavailability": UpdateDeviceAvailability(data); break;

                    // Get Snapshot Data
                    case "snapshottable": UpdateSnapshots(data); break;

                    // Get Status Data
                    case "status_data": UpdateStatus(data); break;

                    // Get OEE Value
                    case "oee_shift_oee": UpdateOee(data); break;

                    // Get OEE Availability Value
                    case "oee_shift_availability": UpdateOeeAvailability(data); break;

                    // Get OEE Performance Value
                    case "oee_shift_performance": UpdateOeePeformance(data); break;

                    // Get Status Timers from Shifts Table
                    case "shifttable_shiftrowinfos": UpdateStatusTimers(data); break;
                }
            }
        }

        private void UpdateUserLogin(EventData data)
        {
            if (data.Data01 != null && configuration != null)
            {
                currentUser = (UserConfiguration)data.Data01;

                //userId = data.Data01.ToString();

                //updateData.UserId = userId;
            }
        }

        private void UpdateDatabaseStatus(EventData data)
        {
            if (data.Data01 != null && data.Data02 != null && currentUser != null)
            {
                databaseConnected = (bool)data.Data02;

                int prev = deviceInfo.Status.Connected;

                if (databaseConnected && deviceAvailable) deviceInfo.Status.Connected = 1;
                else deviceInfo.Status.Connected = 0;
            }
        }

        private void UpdateDeviceAvailability(EventData data)
        {
            if (data.Data01 != null && data.Data02 != null && currentUser != null)
            {
                deviceAvailable = (bool)data.Data02;

                int prev = deviceInfo.Status.Connected;

                if (databaseConnected && deviceAvailable) deviceInfo.Status.Connected = 1;
                else deviceInfo.Status.Connected = 0;
            }
        }

        private void UpdateSnapshots(EventData data)
        {
            if (data.Data01 != null && data.Data02 != null && currentUser != null)
            {
                //bool alert = DataTable_Functions.GetBooleanTableValue(data.Data02, "NAME", "Alert", "VALUE");
                //bool idle = DataTable_Functions.GetBooleanTableValue(data.Data02, "NAME", "Idle", "VALUE");
                //bool production = DataTable_Functions.GetBooleanTableValue(data.Data02, "NAME", "Production", "VALUE");

                //if (alert) deviceInfo.Status.Status = 0;
                //else if (idle) deviceInfo.Status.Status = 1;
                //else if (production) deviceInfo.Status.Status = 2;

                deviceInfo.Status.DeviceStatus = DataTable_Functions.GetTableValue(data.Data02, "NAME", "Device Status", "VALUE");

                deviceInfo.Status.ProductionStatus = DataTable_Functions.GetTableValue(data.Data02, "NAME", "Production Status", "VALUE");

                DateTime start = DataTable_Functions.GetDateTimeTableValue(data.Data02, "NAME", "Production Status", "PREVIOUS_TIMESTAMP");
                DateTime end = DataTable_Functions.GetDateTimeTableValue(data.Data02, "NAME", "Production Status", "TIMESTAMP");

                if (start > DateTime.MinValue && end > DateTime.MinValue)
                {
                    deviceInfo.Status.ProductionStatusTimer = Convert.ToInt32((end - start).TotalSeconds);
                }
            }
        }

        private void UpdateStatus(EventData data)
        {
            if (data.Data01 != null && data.Data02 != null && currentUser != null)
            {
                var infos = (List<StatusInfo>)data.Data02;
                StatusInfo info = null;

                // Availability
                info = infos.Find(x => x.Type == "AVAILABILITY");
                if (info != null) deviceInfo.Controller.Availability = info.Value1;

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
        }

        private void UpdateOee(EventData data)
        {
            if (data.Data01 != null && data.Data02 != null && currentUser != null)
            {
                double val = (double)data.Data02;
                if (deviceInfo.Oee.Oee != val)
                {
                    deviceInfo.Oee.Oee = Math.Round(val, 2);
                }
            }
        }

        private void UpdateOeeAvailability(EventData data)
        {
            if (data.Data01 != null && data.Data02 != null && currentUser != null)
            {
                double val = (double)data.Data02;
                if (deviceInfo.Oee.Availability != val)
                {
                    deviceInfo.Oee.Availability = Math.Round(val, 2);
                }
            }
        }

        private void UpdateOeePeformance(EventData data)
        {
            if (data.Data01 != null && data.Data02 != null && currentUser != null)
            {
                double val = (double)data.Data02;
                if (deviceInfo.Oee.Performance != val)
                {
                    deviceInfo.Oee.Performance = Math.Round(val, 2);
                }
            }
        }

        private void UpdateStatusTimers(EventData data)
        {
            if (data.Data01 != null && data.Data02 != null && currentUser != null)
            {
                var infos = (List<ShiftRowInfo>)data.Data02;

                double total = 0;
                double production = 0;
                double idle = 0;
                double alert = 0;

                foreach (var info in infos)
                {
                    total += info.TotalTime;

                    // Production
                    var item = info.GenEventRowInfos.Find(x => x.ColumnName.ToLower() == "active__true");
                    if (item != null) production += item.Seconds;

                    // Idle
                    item = info.GenEventRowInfos.Find(x => x.ColumnName.ToLower() == "idle__true");
                    if (item != null) idle += item.Seconds;

                    // Alert
                    item = info.GenEventRowInfos.Find(x => x.ColumnName.ToLower() == "alert__true");
                    if (item != null) alert += item.Seconds;
                }

                if (deviceInfo.Timers.Total != total ||
                    deviceInfo.Timers.Active != production ||
                    deviceInfo.Timers.Idle != idle ||
                    deviceInfo.Timers.Alert != alert)
                {
                    deviceInfo.Timers.Total = total;
                    deviceInfo.Timers.Active = production;
                    deviceInfo.Timers.Idle = idle;
                    deviceInfo.Timers.Alert = alert;

                    queue.Add(deviceInfo);
                }
            }
        }
        

        public event SendData_Handler SendData;

        public event Status_Handler StatusChanged;

        public event Status_Handler ErrorOccurred;

        public void Closing() { }

        public Type[] ConfigurationPageTypes { get { return null; } }
    }

}
