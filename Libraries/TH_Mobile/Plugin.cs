// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

using TH_Configuration;
using TH_Global.Functions;
using TH_Plugins;
using TH_Plugins.Server;

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

                            if (databaseConnected && deviceAvailable) updateData.Connected = true;
                            else updateData.Connected = false;

                            Database.Update(username, configuration, updateData);
                        }

                        break;

                    // Update Device Availability (MTConnect) Status
                    case "deviceavailability":

                        if (data.Data01 != null && data.Data02 != null && !string.IsNullOrEmpty(username))
                        {
                            if (updateData == null) updateData = new UpdateData();

                            deviceAvailable = (bool)data.Data02;

                            if (databaseConnected && deviceAvailable) updateData.Connected = true;
                            else updateData.Connected = false;

                            Database.Update(username, configuration, updateData);
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

                            Database.Update(username, configuration, updateData);
                        }

                        break;

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
