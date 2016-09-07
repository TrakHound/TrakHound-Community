// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;

namespace TrakHound_Server.Plugins.Cycles
{
    public class CycleData
    {
        public CycleData()
        {
            Overrides = new List<Override>();
            CycleOverrides = new List<CycleOverride>();
        }

        public string CycleId { get; set; }
        public string InstanceId { get; set; }

        public string Name { get; set; }
        public string Event { get; set; }

        public CycleProductionType ProductionType { get; set; }

        public DateTime Start { get; set; }
        public DateTime Stop { get; set; }

        public TimeSpan Duration
        {
            get
            {
                return Stop - Start;
            }
        }

        public bool Completed { get; set; }

        public List<CycleOverride> CycleOverrides { get; set; }

        public List<Override> Overrides { get; set; }

        public double OverrideAverage
        {
            get
            {
                // Create List of Averages for each Override
                var ovrAvgs = new List<double>();
                foreach (var ovr in Overrides)
                {
                    ovrAvgs.Add(ovr.Average);
                }

                // Get average of all overrides
                double avg = 1;
                if (ovrAvgs.Count > 0) avg = ovrAvgs.Average();

                return avg;
            }
        }

        public CycleData Copy()
        {
            var result = new CycleData();
            result.CycleId = CycleId;
            result.InstanceId = InstanceId;
            result.Name = Name;
            result.Event = Event;
            result.ProductionType = ProductionType;
            result.Start = Start;
            result.Stop = Stop;
            result.Completed = Completed;
            result.CycleOverrides = CycleOverrides.ToList();
            result.Overrides = Overrides.ToList();
            return result;
        }

    }

}
