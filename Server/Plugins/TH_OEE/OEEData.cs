// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TH_OEE
{

    public class OEEData
    {
        public string ShiftId { get; set; }
        public string CycleId { get; set; }
        public string CycleInstanceId { get; set; }

        public double PlannedProductionTime { get; set; }
        public double OperatingTime { get; set; }

        // TrakHound added version of IdealCycleTime
        // Represents OperatingTime taking into account Overrides (if overrides = 100%)
        public double IdealOperatingTime { get; set; }

        public double IdealCycleTime { get; set; }
        public double IdealRunRate { get; set; }
        public int TotalPieces { get; set; }

        public int GoodPieces { get; set; }

        /// <summary>
        /// Used for when certain information is not available (ex. Quality is not available to be calculated)
        /// </summary>
        #region "User Configurable Constant Values"

        public double ConstantAvailability { get; set; }
        public double ConstantPerformance { get; set; }
        public double ConstantQuality { get; set; }

        #endregion

        public double Oee
        {
            get
            {
                return Availability * Performance * Quality;
            }
        }

        public double Availability
        {
            get
            {
                if (ConstantAvailability > 0) return ConstantAvailability;

                if (PlannedProductionTime > 0)
                {
                    return Math.Min(1, OperatingTime / PlannedProductionTime);
                }
                return 0;
            }
        }

        public double Performance
        {
            get
            {
                if (ConstantPerformance > 0) return ConstantPerformance;

                if (OperatingTime > 0)
                {
                    return Math.Min(1, IdealOperatingTime / OperatingTime);
                }
                return 0;
            }
        }

        public double Quality
        {
            get
            {
                if (ConstantQuality > 0) return ConstantQuality;

                if (TotalPieces > 0)
                {
                    return Math.Min(1, GoodPieces / TotalPieces);
                }
                return 0;
            }
        }

        public OEEData Copy()
        {
            var result = new OEEData();
            
            foreach (var info in typeof(OEEData).GetProperties())
            {
                if (info.CanWrite)
                {
                    object val = info.GetValue(this, null);
                    info.SetValue(result, val, null);
                }
            }

            return result;
        }

        public override string ToString()
        {
            string result = "";
            result += ShiftId + " :: ";
            result += CycleId + " : ";
            result += CycleInstanceId + " :: ";
            result += Oee.ToString() + " : ";
            result += Availability.ToString() + " : ";
            result += Performance.ToString() + " : ";
            result += Quality.ToString() + " :: ";
            result += PlannedProductionTime.ToString() + " : ";
            result += OperatingTime.ToString() + " : ";
            result += IdealOperatingTime.ToString() + " : ";
            result += IdealCycleTime.ToString() + " : ";
            result += IdealRunRate.ToString() + " : ";
            result += TotalPieces.ToString() + " : ";
            result += GoodPieces.ToString() + " : ";

            return result;
        }
    }

}
