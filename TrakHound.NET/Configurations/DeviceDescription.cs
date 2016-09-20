// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using TrakHound.API;

namespace TrakHound.Configurations
{
    public class DeviceDescription
    {
        public DeviceDescription() { }

        public DeviceDescription(Data.DeviceInfo deviceInfo)
        {
            UniqueId = deviceInfo.UniqueId;
            Description = deviceInfo.Description;
            Agent = deviceInfo.Agent;
        }

        public DeviceDescription(DeviceConfiguration deviceConfig)
        {
            UniqueId = deviceConfig.UniqueId;
            Description = deviceConfig.Description;
            Agent = deviceConfig.Agent;
        }


        public string UniqueId { get; set; }

        public Data.DescriptionInfo Description { get; set; }

        public Data.AgentInfo Agent { get; set; }
    }
}
