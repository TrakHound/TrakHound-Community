// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;

using TrakHound_Server.Plugins.Cycles;

namespace TrakHound_Server.Plugins.OEE
{
    public class OEEData : TrakHound.OEE
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public string CycleId { get; set; }
        public string CycleInstanceId { get; set; }

        public OEEData() { }

        public OEEData(CycleData cycle)
        {
            Init(cycle, cycle.Start, cycle.Stop);
        }

        public OEEData(CycleData cycle, DateTime start, DateTime stop)
        {
            Init(cycle, start, stop);
        }

        public void Init(CycleData cycle, DateTime start, DateTime stop)
        {
            CycleId = cycle.CycleId;
            CycleInstanceId = cycle.InstanceId;

            Start = start;
            End = stop;

            double duration = (stop - start).TotalSeconds;

            PlannedProductionTime = duration;

            // Availability is caluculated when Cycle Type is 'IN_PRODUCTION'
            if (cycle.ProductionType == CycleProductionType.IN_PRODUCTION)
            {
                OperatingTime = duration;

                if (cycle.CycleOverrides.Count > 0)
                {
                    IdealOperatingTime = IdealTimeFromOverrides(cycle, duration).TotalSeconds;
                }
                else
                {
                    IdealOperatingTime = duration;
                }
            }
        }

        private static TimeSpan IdealTimeFromOverrides(CycleData cycle, double duration)
        {
            // Create List of Averages for each Override
            var ovrAvgs = new List<double>();
            foreach (var ovr in cycle.CycleOverrides)
            {
                ovrAvgs.Add(ovr.Value);
            }

            // Get average of all overrides
            double idealSeconds;
            double avg = 1;
            if (ovrAvgs.Count > 0) avg = ovrAvgs.Average();

            // Calculate IdealDuration based on percentage of the average override value and the Actual Duration of the Cycle
            idealSeconds = duration * (avg / 100);

            return TimeSpan.FromSeconds(Math.Min(duration, idealSeconds));
        }
    }
    
}
