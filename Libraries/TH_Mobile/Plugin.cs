// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

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
        }

        public void GetSentData(EventData data)
        {
            if (data != null && updateData != null)
            {
                switch (data.Id.ToLower())
                {
                    // Server User Changed
                    case "userlogin":

                        if (data.Data01 != null && configuration != null)
                        {
                            userId = data.Data01.ToString();

                            updateData.UserId = userId;

                            Database.CreateTable(userId, configuration);
                        }

                        break;

                    // Update Database Connection Status
                    case "databasestatus":

                        if (data.Data01 != null && data.Data02 != null && !string.IsNullOrEmpty(userId))
                        {
                            databaseConnected = (bool)data.Data02;

                            bool prev = updateData.Connected;

                            if (databaseConnected && deviceAvailable) updateData.Connected = true;
                            else updateData.Connected = false;

                            if (updateData.Connected != prev) queue.Add(updateData);
                        }

                        break;

                    // Update Device Availability (MTConnect) Status
                    case "deviceavailability":

                        if (data.Data01 != null && data.Data02 != null && !string.IsNullOrEmpty(userId))
                        {
                            deviceAvailable = (bool)data.Data02;

                            bool prev = updateData.Connected;

                            if (databaseConnected && deviceAvailable) updateData.Connected = true;
                            else updateData.Connected = false;

                            if (updateData.Connected != prev) queue.Add(updateData);
                        }

                        break;

                    // Get Snapshot Data
                    case "snapshottable":

                        if (data.Data01 != null && data.Data02 != null && !string.IsNullOrEmpty(userId))
                        {
                            bool alert = DataTable_Functions.GetBooleanTableValue(data.Data02, "NAME", "Alert", "VALUE");
                            bool idle = DataTable_Functions.GetBooleanTableValue(data.Data02, "NAME", "Idle", "VALUE");
                            bool production = DataTable_Functions.GetBooleanTableValue(data.Data02, "NAME", "Production", "VALUE");

                            if (alert) updateData.Status = "Alert";
                            else if (idle) updateData.Status = "Idle";
                            else if (production) updateData.Status = "Production";

                            updateData.ProductionStatus = DataTable_Functions.GetTableValue(data.Data02, "NAME", "Production Status", "VALUE");

                            DateTime start = DataTable_Functions.GetDateTimeTableValue(data.Data02, "NAME", "Production Status", "PREVIOUS_TIMESTAMP");
                            DateTime end = DataTable_Functions.GetDateTimeTableValue(data.Data02, "NAME", "Production Status", "TIMESTAMP");

                            if (start > DateTime.MinValue && end > DateTime.MinValue)
                            {
                                updateData.ProductionStatusTimer = Convert.ToInt32((end - start).TotalSeconds);
                            }

                            queue.Add(updateData);
                        }

                        break;

                    // Get Status Data
                    case "status_data":

                        if (data.Data01 != null && data.Data02 != null && !string.IsNullOrEmpty(userId))
                        {
                            var infos = (List<StatusInfo>)data.Data02;
                            StatusInfo info = null;

                            // Controller Mode
                            info = infos.Find(x => x.Type == "CONTROLLER_MODE");
                            if (info != null && updateData.ControllerMode != info.Value1) updateData.ControllerMode = info.Value1; queue.Add(updateData);

                            // Emergency Stop
                            info = infos.Find(x => x.Type == "EMERGENCY_STOP");
                            if (info != null && updateData.EmergencyStop != info.Value1) updateData.EmergencyStop = info.Value1; queue.Add(updateData);

                            // Execution Mode
                            info = infos.Find(x => x.Type == "EXECUTION");
                            if (info != null && updateData.ExecutionMode != info.Value1) updateData.ExecutionMode = info.Value1; queue.Add(updateData);

                            // System status
                            info = infos.Find(x => x.Type == "SYSTEM");
                            if (info != null && updateData.SystemMessage != info.Value1 && updateData.SystemStatus != info.Value2)
                            {
                                updateData.SystemMessage = info.Value1;
                                updateData.SystemStatus = info.Value2;

                                queue.Add(updateData);
                            }
                        }

                        break;

                    // Get OEE Value
                    case "oee_shift_oee":

                        if (data.Data01 != null && data.Data02 != null && !string.IsNullOrEmpty(userId))
                        {
                            double val = (double)data.Data02;
                            if (updateData.Oee != val)
                            {
                                updateData.Oee = Math.Round(val, 2);
                                queue.Add(updateData);
                            }
                        }

                        break;

                    case "oee_shift_availability":

                        if (data.Data01 != null && data.Data02 != null && !string.IsNullOrEmpty(userId))
                        {
                            double val = (double)data.Data02;
                            if (updateData.Availability != val)
                            {
                                updateData.Availability = Math.Round(val, 2);
                                queue.Add(updateData);
                            }
                        }

                        break;

                    case "oee_shift_performance":

                        if (data.Data01 != null && data.Data02 != null && !string.IsNullOrEmpty(userId))
                        {
                            double val = (double)data.Data02;
                            if (updateData.Performance != val)
                            {
                                updateData.Performance = Math.Round(val, 2);
                                queue.Add(updateData);
                            }
                        }

                        break;

                    case "shifttable_shiftrowinfos":

                        if (data.Data01 != null && data.Data02 != null && !string.IsNullOrEmpty(userId))
                        {
                            var infos = (List<ShiftRowInfo>)data.Data02;

                            int total = 0;
                            int production = 0;
                            int idle = 0;
                            int alert = 0;

                            foreach (var info in infos)
                            {
                                total += info.totalTime;

                                // Production
                                var item = info.genEventRowInfos.Find(x => x.columnName.ToLower() == "production__true");
                                if (item != null) production += item.seconds;

                                // Idle
                                item = info.genEventRowInfos.Find(x => x.columnName.ToLower() == "idle__true");
                                if (item != null) idle += item.seconds;

                                // Alert
                                item = info.genEventRowInfos.Find(x => x.columnName.ToLower() == "alert__true");
                                if (item != null) alert += item.seconds;
                            }

                            if (updateData.TotalSeconds != total ||
                                updateData.ProductionSeconds != production ||
                                updateData.IdleSeconds != idle ||
                                updateData.AlertSeconds != alert)
                            {
                                updateData.TotalSeconds = total;
                                updateData.ProductionSeconds = production;
                                updateData.IdleSeconds = idle;
                                updateData.AlertSeconds = alert;

                                queue.Add(updateData);
                            }
                        }

                        break;
                }
            }
        }
        

        public event SendData_Handler SendData;

        public event Status_Handler StatusChanged;

        public event Status_Handler ErrorOccurred;

        public void Closing()
        {
            if (updateData != null)
            {
                updateData.Connected = false;
                queue.Add(updateData);
            }
        }

        public Type[] ConfigurationPageTypes { get { return null; } }
    }

}
