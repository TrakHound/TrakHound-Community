// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.ServiceModel;

using TH_Global.Functions;
using TH_Global.Updates;

namespace TrakHound_Updater
{
    public partial class Service1
    {
        private ServiceHost host;

        public void StartMessageServer()
        {
            if (host == null)
            {
                host = WCF_Functions.Server.Create<MessageServer>(UpdateConfiguration.UPDATER_PIPE_NAME);
            }
        }

        public void StopMessageServer()
        {
            if (host != null) host.Close();
        }
    }
}
