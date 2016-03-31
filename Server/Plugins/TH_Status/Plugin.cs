// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

using TH_Configuration;
using TH_Database;
using TH_Plugins;
using TH_Plugins.Server;

namespace TH_Status
{
    public class Plugin : IServerPlugin
    {

        public string Name { get { return "TH_Status"; } }

        public void Initialize(Configuration config)
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
                    var infos = StatusInfo.GetList((TH_MTConnect.Components.ReturnData)data.Data02);
                    if (infos.Count > 0)
                    {
                        statusInfos = infos;
                        Database.AddRows(configuration, infos);
                    }
                }
                if (data.Id.ToLower() == "mtconnect_current" && data.Data02 != null)
                {
                    StatusInfo.ProcessList((TH_MTConnect.Streams.ReturnData)data.Data02, statusInfos);
                    Database.UpdateRows(configuration, statusInfos);
                }
            }
        }

        public event SendData_Handler SendData;

        public event Status_Handler StatusChanged;

        public event Status_Handler ErrorOccurred;

        public void Closing() { }

        public Type[] ConfigurationPageTypes { get { return null; } }


        private Configuration configuration;

    }
}
