// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

using TH_Configuration;
using TH_Plugins;
using TH_Plugins.Server;

namespace TH_InstanceTable
{
    public partial class Plugin : IServerPlugin
    {

        public string Name { get { return "TH_InstanceData"; } }

        public void Initialize(Configuration config)
        {
            InstanceConfiguration ic = InstanceConfiguration.Read(config.ConfigurationXML);
            if (ic != null)
            {
                if (ic.Conditions || ic.Events || ic.Samples) AddDatabases = true;
                else AddDatabases = false;

                config.CustomClasses.Add(ic);
            }

            configuration = config;
        }

        public void GetSentData(EventData data)
        {
            if (data != null && data.Id != null & data.Data02 != null)
            {
                switch (data.Id.ToLower())
                {
                    case "mtconnect_probe": Update_Probe((TH_MTConnect.Components.ReturnData)data.Data02); break;
                    case "mtconnect_current": Update_Current((TH_MTConnect.Streams.ReturnData)data.Data02); break;
                    case "mtconnect_sample": Update_Sample((TH_MTConnect.Streams.ReturnData)data.Data02); break;
                }
            }
        }

        public event SendData_Handler SendData;

        public void Closing() { }

        public Type[] ConfigurationPageTypes { get { return new Type[] { typeof(ConfigurationPage.Page) }; } }

    }
}
