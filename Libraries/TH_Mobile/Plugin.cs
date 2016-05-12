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
using TH_Status;

namespace TH_Mobile
{

    public class Plugin : IServerPlugin
    {

        public string Name { get { return "TH_Mobile"; } }


        private Configuration configuration;

        private string username;

        private UpdateData updateData;

        private bool databaseConnected;
        private bool deviceAvailable;


        public void Initialize(Configuration config)
        {
            configuration = config;

            StartQueue();
        }

        public void GetSentData(EventData data)
        {
            if (data != null)
            {
                switch (data.Id.ToLower())
                {
                    // Server User Changed
                    case "userlogin":

                        if (data.Data01 != null && configuration != null)
                        {
                            username = data.Data01.ToString();

                            Database.CreateTable(username, configuration);
                        }

                        break;

                    // Update Database Connection Status
                    case "databasestatus":

                        if (data.Data01 != null && data.Data02 != null && !string.IsNullOrEmpty(username))
                        {
                            if (updateData == null) updateData = new UpdateData();

                            databaseConnected = (bool)data.Data02;

                            bool prev = updateData.Connected;

                            if (databaseConnected && deviceAvailable) updateData.Connected = true;
                            else updateData.Connected = false;

                            if (updateData.Connected != prev) queueChanged = true;
                        }

                        break;

                    // Update Device Availability (MTConnect) Status
                    case "deviceavailability":

                        if (data.Data01 != null && data.Data02 != null && !string.IsNullOrEmpty(username))
                        {
                            if (updateData == null) updateData = new UpdateData();

                            deviceAvailable = (bool)data.Data02;

                            bool prev = updateData.Connected;

                            if (databaseConnected && deviceAvailable) updateData.Connected = true;
                            else updateData.Connected = false;

                            if (updateData.Connected != prev) queueChanged = true;
                        }

                        break;

                    // Get Snapshot Data
                    case "snapshottable":

                        if (data.Data01 != null && data.Data02 != null && !string.IsNullOrEmpty(username))
                        {
                            if (updateData == null) updateData = new UpdateData();

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

                            queueChanged = true;
                        }

                        break;

                    // Get Status Data
                    case "status_data":

                        if (data.Data01 != null && data.Data02 != null && !string.IsNullOrEmpty(username))
                        {
                            var infos = (List<StatusInfo>)data.Data02;
                            StatusInfo info = null;

                            // Controller Mode
                            info = infos.Find(x => x.Type == "CONTROLLER_MODE");
                            if (info != null && updateData.ControllerMode != info.Value1) updateData.ControllerMode = info.Value1; queueChanged = true;

                            // Emergency Stop
                            info = infos.Find(x => x.Type == "EMERGENCY_STOP");
                            if (info != null && updateData.EmergencyStop != info.Value1) updateData.EmergencyStop = info.Value1; queueChanged = true;

                            // Execution Mode
                            info = infos.Find(x => x.Type == "EXECUTION");
                            if (info != null && updateData.ExecutionMode != info.Value1) updateData.ExecutionMode = info.Value1; queueChanged = true;

                            // System status
                            info = infos.Find(x => x.Type == "SYSTEM");
                            if (info != null && updateData.SystemMessage != info.Value1 && updateData.SystemStatus != info.Value2)
                            {
                                updateData.SystemMessage = info.Value1;
                                updateData.SystemStatus = info.Value2;

                                queueChanged = true;
                            }
                        }

                        break;

                    // Get Snapshot Data
                    case "oee_shifts":

                        if (data.Data01 != null && data.Data02 != null && !string.IsNullOrEmpty(username))
                        {
                            if (updateData == null) updateData = new UpdateData();

                            var table = data.Data02 as DataTable;
                            if (table != null)
                            {
                                var oees = new List<double>();
                                var availabilities = new List<double>();
                                var performances = new List<double>();

                                foreach (DataRow row in table.Rows)
                                {
                                    oees.Add(DataTable_Functions.GetDoubleFromRow("OEE", row));
                                    availabilities.Add(DataTable_Functions.GetDoubleFromRow("AVAILABILITY", row));
                                    performances.Add(DataTable_Functions.GetDoubleFromRow("PERFORMANCE", row));
                                }

                                // OEE
                                double oee = oees.Average();
                                if (updateData.Oee != oee)
                                {
                                    updateData.Oee = oee;
                                    queueChanged = true;
                                }

                                // Availability
                                double availability = availabilities.Average();
                                if (updateData.Availability != availability)
                                {
                                    updateData.Availability = availability;
                                    queueChanged = true;
                                }

                                // Performance
                                double performance = performances.Average();
                                if (updateData.Performance != performance)
                                {
                                    updateData.Performance = performance;
                                    queueChanged = true;
                                }
                            }
                        }

                        break;
                }
            }
        }


        System.Timers.Timer queueTimer;

        private bool queueChanged = false;

        private void StartQueue()
        {
            if (queueTimer != null) queueTimer.Enabled = false;

            queueTimer = new System.Timers.Timer();
            queueTimer.Interval = 2000;
            queueTimer.Elapsed += QueueTimer_Elapsed;
            queueTimer.Enabled = true;
        }

        private void QueueTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            ProcessQueue();
        }

        private void ProcessQueue()
        {
            if (queueChanged) Database.Update(username, configuration, updateData);
            queueChanged = false;
        }

        private void StopQueue()
        {
            if (queueTimer != null) queueTimer.Enabled = false;
            queueTimer = null;
        }


        public event SendData_Handler SendData;

        public event Status_Handler StatusChanged;

        public event Status_Handler ErrorOccurred;

        public void Closing()
        {
            StopQueue();

            if (updateData != null)
            {
                updateData.Connected = false;
                Database.Update(username, configuration, updateData);
            }
        }

        public Type[] ConfigurationPageTypes { get { return null; } }

    }

}
