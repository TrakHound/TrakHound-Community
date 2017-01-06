// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;

using TrakHound;
using TrakHound.Configurations;
using TrakHound.Plugins.Server;
using TrakHound_Server.Plugins.Instances;

namespace TrakHound_Server.Plugins.SnapshotData
{
    public partial class Plugin : IServerPlugin
    {

        public string Name { get { return "Snapshot Data"; } }

        private DeviceConfiguration configuration;

        public void Initialize(DeviceConfiguration config)
        {
            var sdc = Configuration.Read(config.Xml);
            if (sdc != null)
            {
                config.CustomClasses.Add(sdc);

                configuration = config;
            }
        }

        public void GetSentData(EventData data)
        {
            if (data != null && data.Id != null && configuration != null)
            {
                if (data.Id == "CURRENT_INSTANCE")
                {
                    if (data.Data02 != null)
                    {
                        var sdc = Configuration.Get(configuration);
                        if (sdc != null)
                        {
                            var currentInstance = (CurrentInstance)data.Data02;

                            var info = new Snapshot.ProcessInfo();
                            info.CurrentData = currentInstance.CurrentData;
                            info.CurrentInstance = currentInstance.Instance;

                            Snapshot.Process(configuration, info);

                            // Send List of SnapShotItems to other Plugins
                            SendSnapShotItems(sdc.Snapshots);
                        }
                    }
                    else
                    {
                        SendSnapShotItems(null);
                    }
                }
            }
        }

        public event SendData_Handler SendData;

        public void Starting() { }

        public void Closing() { }

        void SendSnapShotItems(List<Snapshot> snapshots)
        {
            var data = new EventData(this);
            data.Id = "SNAPSHOTS";
            data.Data01 = configuration;
            data.Data02 = snapshots;
            SendData?.Invoke(data);
        }

    }
}
