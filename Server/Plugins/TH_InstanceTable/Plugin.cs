﻿// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

using TH_Configuration;
using TH_Plugins;
using TH_Plugins.Server;

namespace TH_InstanceTable
{
    public partial class InstanceTable : IServerPlugin
    {

        public string Name { get { return "TH_InstanceTable"; } }

        public void Initialize(Configuration configuration)
        {
            InstanceConfiguration ic = null;

            if (firstPass)
            {
                ic = InstanceConfiguration.Read(configuration.ConfigurationXML);
            }
            else ic = InstanceConfiguration.Get(configuration);

            if (ic != null)
            {
                if (ic.DataItems.Conditions || ic.DataItems.Events || ic.DataItems.Samples) AddDatabases = true;
                else AddDatabases = false;

                if (firstPass) configuration.CustomClasses.Add(ic);
            }

            firstPass = false;

            config = configuration;
        }

        //void SendData(EventData data)
        //{
        //    if (SendData != null) SendData(data);
        //}

        public void GetSentData(EventData data)
        {
            if (data != null && data.id != null & data.data02 != null)
            {
                switch (data.id.ToLower())
                {
                    case "mtconnect_probe": Update_Probe((TH_MTConnect.Components.ReturnData)data.data02); break;
                    case "mtconnect_current": Update_Current((TH_MTConnect.Streams.ReturnData)data.data02); break;
                    case "mtconnect_sample": Update_Sample((TH_MTConnect.Streams.ReturnData)data.data02); break;
                }
            }
        }

        public event SendData_Handler SendData;

        //public event Status_Handler StatusChanged;

        //public event Status_Handler ErrorOccurred;

        public void Closing()
        {


        }

        public Type[] ConfigurationPageTypes { get { return new Type[] { typeof(ConfigurationPage.Page) }; } }

        //public bool UseDatabases { get; set; }

        //public string TablePrefix { get; set; }


    }
}
