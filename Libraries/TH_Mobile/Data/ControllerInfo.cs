// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace TH_Mobile.Data
{
    public class ControllerInfo
    {
        [NonSerialized]
        public bool Changed;

        public string ControllerMode { get; set; }
        public string EmergencyStop { get; set; }
        public string ExecutionMode { get; set; }
        public string SystemStatus { get; set; }
        public string SystemMessage { get; set; }

        public bool EqualTo(ControllerInfo info)
        {
            return
                info != null &&
                ControllerMode == info.ControllerMode &&
                EmergencyStop == info.EmergencyStop &&
                ExecutionMode == info.ExecutionMode &&
                SystemStatus == info.SystemStatus &&
                SystemMessage == info.SystemMessage;
        }
    }
}

