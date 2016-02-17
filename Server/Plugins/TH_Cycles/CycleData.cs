using System;
using System.Collections.Generic;
using System.Linq;

namespace TH_Cycles
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


        public enum CycleProductionType
        {
            UNCATEGORIZED,
            STOPPED,
            ERROR,
            PAUSED,
            MATERIAL_CHANGEOVER,
            TOOLING_CHANGEOVER,
            IN_PRODUCTION
        }

        public CycleProductionType ProductionType { get; set; }


        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }

        public DateTime StartTimeUtc { get; set; }
        public DateTime StopTimeUtc { get; set; }

        public string ShiftId { get; set; }

        public TimeSpan Duration
        {
            get
            {
                return StopTime - StartTime;
            }
        }

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
            result.StartTime = StartTime;
            result.StopTime = StopTime;
            result.StartTimeUtc = StartTimeUtc;
            result.StopTimeUtc = StopTimeUtc;
            result.ShiftId = ShiftId;
            result.CycleOverrides = CycleOverrides.ToList();
            result.Overrides = Overrides.ToList();
            return result;
        }

        public override string ToString()
        {
            string result = "";
            result += CycleId + " : ";
            result += InstanceId + " : ";
            result += Name + " : ";
            result += Event + " : ";
            result += ProductionType.ToString() + " : ";
            result += StartTime.ToString() + " : ";
            result += StopTime.ToString() + " : ";
            result += ShiftId + " : ";
            result += Duration.ToString() + " :: ";

            foreach (var ovr in CycleOverrides)
            {
                result += ovr.Value.ToString() + " :: ";
            }

            return result;
        }
    }
}
