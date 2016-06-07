// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

using TH_Configuration;
using TH_Global.Functions;
using TH_Plugins;
using TH_Plugins.Server;
using TH_ShiftTable;
using TH_Status;

namespace TH_Mobile
{

    public class Plugin : IServerPlugin
    {
        public static UpdateQueue queue = new UpdateQueue();


        private Configuration configuration;

        private string userId;

        private UpdateData updateData;

        private bool databaseConnected;
        private bool deviceAvailable;


        public string Name { get { return "TH_Mobile"; } }

        public void Initialize(Configuration config)
        {
            configuration = config;

            updateData = new UpdateData(config);
            queue.Add(updateData);
        }

        public void GetSentData(EventData data)
        {
            if (data != null && updateData != null)
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
                userId = data.Data01.ToString();

                updateData.UserId = userId;
            }
        }

        private void UpdateDatabaseStatus(EventData data)
        {
            if (data.Data01 != null && data.Data02 != null && !string.IsNullOrEmpty(userId))
            {
                databaseConnected = (bool)data.Data02;

                int prev = updateData.Status.Connected;

                if (databaseConnected && deviceAvailable) updateData.Status.Connected = 1;
                else updateData.Status.Connected = 0;
            }
        }

        private void UpdateDeviceAvailability(EventData data)
        {
            if (data.Data01 != null && data.Data02 != null && !string.IsNullOrEmpty(userId))
            {
                deviceAvailable = (bool)data.Data02;

                int prev = updateData.Status.Connected;

                if (databaseConnected && deviceAvailable) updateData.Status.Connected = 1;
                else updateData.Status.Connected = 0;
            }
        }

        private void UpdateSnapshots(EventData data)
        {
            if (data.Data01 != null && data.Data02 != null && !string.IsNullOrEmpty(userId))
            {
                bool alert = DataTable_Functions.GetBooleanTableValue(data.Data02, "NAME", "Alert", "VALUE");
                bool idle = DataTable_Functions.GetBooleanTableValue(data.Data02, "NAME", "Idle", "VALUE");
                bool production = DataTable_Functions.GetBooleanTableValue(data.Data02, "NAME", "Production", "VALUE");

                if (alert) updateData.Status.Status = 0;
                else if (idle) updateData.Status.Status = 1;
                else if (production) updateData.Status.Status = 2;

                updateData.Status.ProductionStatus = DataTable_Functions.GetTableValue(data.Data02, "NAME", "Production Status", "VALUE");

                DateTime start = DataTable_Functions.GetDateTimeTableValue(data.Data02, "NAME", "Production Status", "PREVIOUS_TIMESTAMP");
                DateTime end = DataTable_Functions.GetDateTimeTableValue(data.Data02, "NAME", "Production Status", "TIMESTAMP");

                if (start > DateTime.MinValue && end > DateTime.MinValue)
                {
                    updateData.Status.ProductionStatusTimer = Convert.ToInt32((end - start).TotalSeconds);
                }
            }
        }

        private void UpdateStatus(EventData data)
        {
            if (data.Data01 != null && data.Data02 != null && !string.IsNullOrEmpty(userId))
            {
                var infos = (List<StatusInfo>)data.Data02;
                StatusInfo info = null;

                // Availability
                info = infos.Find(x => x.Type == "AVAILABILITY");
                updateData.Controller.Availability = info.Value1;

                // Controller Mode
                info = infos.Find(x => x.Type == "CONTROLLER_MODE");
                updateData.Controller.ControllerMode = info.Value1;

                // Emergency Stop
                info = infos.Find(x => x.Type == "EMERGENCY_STOP");
                updateData.Controller.EmergencyStop = info.Value1;

                // Execution Mode
                info = infos.Find(x => x.Type == "EXECUTION");
                updateData.Controller.ExecutionMode = info.Value1;

                // System status
                info = infos.Find(x => x.Type == "SYSTEM");
                updateData.Controller.SystemMessage = info.Value1;
                updateData.Controller.SystemStatus = info.Value2;

                // Program Name
                info = infos.Find(x => x.Type == "PROGRAM");
                updateData.Controller.ProgramName = info.Value1;
            }
        }

        private void UpdateOee(EventData data)
        {
            if (data.Data01 != null && data.Data02 != null && !string.IsNullOrEmpty(userId))
            {
                double val = (double)data.Data02;
                if (updateData.Oee.Oee != val)
                {
                    updateData.Oee.Oee = Math.Round(val, 2);
                }
            }
        }

        private void UpdateOeeAvailability(EventData data)
        {
            if (data.Data01 != null && data.Data02 != null && !string.IsNullOrEmpty(userId))
            {
                double val = (double)data.Data02;
                if (updateData.Oee.Availability != val)
                {
                    updateData.Oee.Availability = Math.Round(val, 2);
                }
            }
        }

        private void UpdateOeePeformance(EventData data)
        {
            if (data.Data01 != null && data.Data02 != null && !string.IsNullOrEmpty(userId))
            {
                double val = (double)data.Data02;
                if (updateData.Oee.Performance != val)
                {
                    updateData.Oee.Performance = Math.Round(val, 2);
                }
            }
        }

        private void UpdateStatusTimers(EventData data)
        {
            if (data.Data01 != null && data.Data02 != null && !string.IsNullOrEmpty(userId))
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
                    var item = info.GenEventRowInfos.Find(x => x.ColumnName.ToLower() == "production__true");
                    if (item != null) production += item.Seconds;

                    // Idle
                    item = info.GenEventRowInfos.Find(x => x.ColumnName.ToLower() == "idle__true");
                    if (item != null) idle += item.Seconds;

                    // Alert
                    item = info.GenEventRowInfos.Find(x => x.ColumnName.ToLower() == "alert__true");
                    if (item != null) alert += item.Seconds;
                }

                if (updateData.Timers.Total != total ||
                    updateData.Timers.Production != production ||
                    updateData.Timers.Idle != idle ||
                    updateData.Timers.Alert != alert)
                {
                    updateData.Timers.Total = total;
                    updateData.Timers.Production = production;
                    updateData.Timers.Idle = idle;
                    updateData.Timers.Alert = alert;

                    queue.Add(updateData);
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
