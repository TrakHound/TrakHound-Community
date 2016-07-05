// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using TH_Global.TrakHound.Configurations;
using TH_Mobile.Data;

namespace TH_Mobile
{
    public class UpdateData
    {
        public UpdateData(DeviceConfiguration config)
        {
            UniqueId = config.UniqueId;

            Description = new DescriptionInfo(config);
            Status = new StatusInfo();
            Controller = new ControllerInfo();
            Oee = new OeeInfo();
            Timers = new TimersInfo();
        }

        public string UserId { get; set; }
        public string UniqueId { get; set; }

        public DescriptionInfo Description { get; set; }

        public StatusInfo Status { get; set; }

        public ControllerInfo Controller { get; set; }

        public OeeInfo Oee { get; set; }

        public TimersInfo Timers { get; set; }

        public void Reset()
        {
            Description.Changed = false;
            Status.Changed = false;
            Controller.Changed = false;
            Oee.Changed = false;
            Timers.Changed = false;
        }

    }
}
