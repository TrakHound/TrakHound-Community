// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

using TH_Configuration;
using TH_GeneratedData;
using TH_Plugins;
using TH_Plugins.Server;

namespace TH_Parts
{
    public class Plugin : IServerPlugin
    {

        public string Name { get { return "TH_Parts"; } }

        public void Initialize(Configuration config)
        {
            PartsConfiguration pc = PartsConfiguration.Read(config.ConfigurationXML);
            if (pc != null)
            {
                config.CustomClasses.Add(pc);

                configuration = config;

                Database.CreatePartsTable(config);
            }
        }


        public void GetSentData(EventData data)
        {
            if (data != null)
            {
                if (data.id.ToLower() == "generatedeventitems")
                {
                    var genEventItems = (List<GeneratedEventItem>)data.data02;

                    var pc = PartsConfiguration.Get(configuration);
                    if (pc != null)
                    {
                        genEventItems = genEventItems.FindAll(x => x.EventName == pc.PartsEventName);

                        var infos = new List<PartInfo>();

                        foreach (var item in genEventItems)
                        {
                            PartInfo info = PartInfo.Get(configuration, item);
                            if (info != null) infos.Add(info);
                        }

                        if (infos.Count > 0)
                        {
                            Database.AddRows(configuration, infos);
                            SendPartInfos(infos);
                        }
                    }
                }
            }
        }

        public event SendData_Handler SendData;

        public event Status_Handler StatusChanged;

        public event Status_Handler ErrorOccurred;

        public void Closing() { }

        public Type[] ConfigurationPageTypes { get { return new Type[] { typeof(ConfigurationPage.Page) }; } }


        private Configuration configuration;

        private void SendPartInfos(List<PartInfo> infos)
        {
            var data = new EventData();
            data.id = "PartInfos";
            data.data01 = configuration;
            data.data02 = infos;
            if (SendData != null) SendData(data);
        }
    }
}
