// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.


namespace TH_Mobile
{
    public class UpdateData
    {

        public bool Connected { get; set; }
        public string Status { get; set; }

        public string ProductionStatus { get; set; }
        public int ProductionStatusTimer { get; set; }

        public string ControllerMode { get; set; }
        public string EmergencyStop { get; set; }
        public string ExecutionMode { get; set; }
        public string SystemStatus { get; set; }
        public string SystemMessage { get; set; }

        public double Oee { get; set; }
        public double Availability { get; set; }
        public double Performance { get; set; }

    }
}
