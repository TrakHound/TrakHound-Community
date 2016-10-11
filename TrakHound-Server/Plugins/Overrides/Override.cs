// Copyright(c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;

namespace TrakHound_Server.Plugins.Overrides
{
    /// <summary>
    /// Type of Override
    /// </summary>
    public enum OverrideType
    {
        FEEDRATE_OVERRIDE,
        SPINDLE_OVERRIDE,
        RAPID_OVERRIDE
    }

    /// <summary>
    /// Override Configuration class
    /// </summary>
    public class Override
    {
        public string Name { get; set; }

        public string Link { get; set; }

        public OverrideType Type { get; set; }
    }

    /// <summary>
    /// Override item with values
    /// </summary>
    public class OverrideItem : Override
    {
        public OverrideItem(Override o)
        {
            Name = o.Name;
            Link = o.Link;
            Type = o.Type;
        }

        public double Value { get; set; }

        public DateTime Timestamp { get; set; }
        public long Sequence { get; set; }

        public OverrideItem Copy()
        {
            var ovr = new Override();
            ovr.Name = Name;
            ovr.Link = Link;
            ovr.Type = Type;

            var result = new OverrideItem(ovr);
            result.Value = Value;
            result.Timestamp = Timestamp;
            result.Sequence = Sequence;
            return result;
        }
    }

    /// <summary>
    /// Averages of combined OverrideItem objects
    /// </summary>
    public class OverrideInstance
    {
        public OverrideInstance(List<OverrideItem> items)
        {
            var i = items.FindAll(o => o.Type == OverrideType.FEEDRATE_OVERRIDE).Select(o => o.Value);
            if (i.Any()) FeedrateAverage = i.Average();

            i = items.FindAll(o => o.Type == OverrideType.SPINDLE_OVERRIDE).Select(o => o.Value);
            if (i.Any()) SpindleAverage = i.Average();

            i = items.FindAll(o => o.Type == OverrideType.RAPID_OVERRIDE).Select(o => o.Value);
            if (i.Any()) RapidAverage = i.Average();
        }

        public double FeedrateAverage { get; set; }
        public double SpindleAverage { get; set; }
        public double RapidAverage { get; set; }

        public DateTime Timestamp { get; set; }
        public TimeSpan Duration { get; set; }
        public long Sequence { get; set; }
    }

}
