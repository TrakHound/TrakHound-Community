// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

using TH_Configuration;
using TH_Plugins;
using TH_Plugins.Server;

namespace TH_Status
{
    public class Plugin : IServerPlugin
    {

        public string Name { get { return "TH_Status"; } }

        public void Initialize(DeviceConfiguration config)
        {
            configuration = config;

            Database.CreateTable(config);
        }

        private List<StatusInfo> statusInfos = new List<StatusInfo>();

        public void GetSentData(EventData data)
        {
            if (data != null)
            {
                if (data.Id.ToLower() == "mtconnect_probe" && data.Data02 != null)
                {
                    var infos = StatusInfo.GetList((MTConnect.Application.Components.ReturnData)data.Data02);
                    if (infos.Count > 0)
                    {
                        statusInfos = infos;
                        Database.AddRows(configuration, infos);
                    }
                }
                else if (data.Id.ToLower() == "mtconnect_current" && data.Data02 != null)
                {
                    StatusInfo.ProcessList((MTConnect.Application.Streams.ReturnData)data.Data02, statusInfos);

                    SendStatusData(statusInfos);

                    Database.UpdateRows(configuration, statusInfos);
                }
            }
        }

        public event SendData_Handler SendData;

        public event Status_Handler StatusChanged;

        public event Status_Handler ErrorOccurred;

        public void Closing() { }

        public Type[] ConfigurationPageTypes { get { return null; } }


        private DeviceConfiguration configuration;

        private void SendStatusData(List<StatusInfo> infos)
        {
            var data = new EventData();
            data.Id = "Status_Data";
            data.Data01 = configuration;
            data.Data02 = infos;

            if (SendData != null) SendData(data);
        }

    }
}
