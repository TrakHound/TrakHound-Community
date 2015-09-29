// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

namespace TH_Configuration
{

    public class Agent_Settings
    {

        public string ServiceName { get; set; }
        public string IP_Address { get; set; }
        public int Port { get; set; }

        public string Device_Name { get; set; }

        public int Current_Heartbeat { get; set; }
        public int Sample_Heartbeat { get; set; }

        public int Max_Sample_Interval { get; set; }

        public List<string> Simulation_Sample_Files = new List<string>();

        //public string Simulation_Sample_Path { get; set; }

        // Row Limit : <MTC Data Item Link>Limit of Rows</MTC Data Item Link>
        public List<Tuple<string, int>> RowLimits = new List<Tuple<string, int>>();

        // List of values that are to be Ommitted from the Instance table 
        // (to prevent some values that change often from creating a ton of instance rows)
        public List<string> OmitInstance = new List<string>();

    }

}
