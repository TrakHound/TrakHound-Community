// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

namespace TrakHound.Server.Plugins.Cycles
{
    public class Override
    {
        public Override()
        {
            Values = new List<OverrideValue>();
        }

        public string Id { get; set; }

        /// <summary>
        /// Return a weighted average based on how long each override was set
        /// </summary>
        public double Average
        {
            get
            {
                double avg = -1;

                if (Values.Count > 0)
                {
                    // Get Total Duration of all Values to use for Weight calculation
                    DateTime firstTimestamp = Values[0].StartTime;
                    DateTime lastTimestamp = Values[Values.Count - 1].StopTime;

                    TimeSpan totalDuration = lastTimestamp - firstTimestamp;

                    var grps = new List<Tuple<double, double>>();
                    foreach (var val in Values)
                    {
                        TimeSpan duration = val.StopTime - val.StartTime;

                        double weight = duration.TotalSeconds / totalDuration.TotalSeconds;

                        var grp = new Tuple<double, double>(val.Value, weight);
                        grps.Add(grp);
                    }

                    double top = 0;
                    double bottom = 0;

                    foreach (var grp in grps)
                    {
                        top += grp.Item1 * grp.Item2;
                        bottom += grp.Item2;
                    }

                    if (bottom > 0) avg = top / bottom;
                }

                return avg;
            }
        }

        public List<OverrideValue> Values { get; set; }
    }

    public class OverrideValue
    {
        public double Value { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
    }

    public class CycleOverride
    {
        public string Id { get; set; }
        public double Value { get; set; }
    }
}
