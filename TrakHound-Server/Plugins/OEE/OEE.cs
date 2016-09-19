// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

using TrakHound;
using TrakHound.Configurations;
using TrakHound.Plugins;
using TrakHound.Plugins.Server;
using TrakHound_Server.Plugins.Cycles;

namespace TrakHound_Server.Plugins.OEE
{
    public class Plugin : IServerPlugin
    {

        public string Name { get { return "OEE"; } }

        public void Initialize(DeviceConfiguration config)
        {
            configuration = config;
        }

        public void GetSentData(EventData data)
        {
            if (data != null)
            {
                if (data.Id == "CYCLES")
                {
                    if (data.Data02.GetType() == typeof(List<CycleData>))
                    {
                        var cycles = (List<CycleData>)data.Data02;

                        var oeeDatas = Process(cycles);
                        if (oeeDatas.Count > 0)
                        {
                            SendOeeData(oeeDatas);
                        }
                    }
                }
            }
        }

        public event SendData_Handler SendData;

        public void Starting() { }

        public void Closing() { }

        DeviceConfiguration configuration { get; set; }

        private List<OEEData> Process(List<CycleData> cycles)
        {
            var result = new List<OEEData>();

            foreach (var cycle in cycles)
            {
                // Check if cycle spans two or more hours
                if (cycle.Start.Hour != cycle.Stop.Hour)
                {
                    var hourStart = cycle.Start;

                    while (hourStart < cycle.Stop)
                    {
                        var hourPlus = hourStart.AddHours(1);
                        var hourEnd = new DateTime(hourPlus.Year, hourPlus.Month, hourPlus.Day, hourPlus.Hour, 0, 0);
                        if (hourEnd > cycle.Stop) hourEnd = cycle.Stop;

                        var oeeData = new OEEData(cycle, hourStart, hourEnd);
                        result.Add(oeeData);

                        hourStart = hourEnd;
                    }
                }
                else
                {
                    var oeeData = new OEEData(cycle);
                    result.Add(oeeData);
                }
            }

            return result;
        }

        private void SendOeeData(List<OEEData> oeeDatas)
        {
            var data = new EventData();
            data.Id = "OEE";
            data.Data01 = configuration;
            data.Data02 = oeeDatas;
            if (SendData != null) SendData(data);
        }
    }
    
}
