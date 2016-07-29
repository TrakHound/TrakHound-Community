// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Data;

using TrakHound.Tools;
using TrakHound.Shifts;

namespace TrakHound.Server.Plugins.OEE
{

    public class OEEData
    {
        public OEEData()
        {
            PlannedProductionTime = 0;
            OperatingTime = 0;
            IdealOperatingTime = 0;
            IdealCycleTime = 0;
            IdealRunRate = 0;
            TotalPieces = 0;
            GoodPieces = 0;
        }

        public ShiftId ShiftId { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime StartTimeUTC { get; set; }
        public DateTime EndTimeUTC { get; set; }

        //public string ShiftId { get; set; }
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

        public static OEEData FromCycleDataTable(DataTable dt, string shiftId, string cycleId, string cycleInstanceId)
        {
            OEEData result = null;

            var match = dt.Select("shift_id='" + shiftId + "' AND cycle_id='" + cycleId + "' AND cycle_instance_id='" + cycleInstanceId + "'");
            if (match != null && match.Length > 0)
            {
                DataRow row = match[0];

                result = new OEEData();
                result.ShiftId = new ShiftId(shiftId);
                result.ConstantQuality = 1;
                result.CycleId = cycleId;
                result.CycleInstanceId = cycleInstanceId;
                result.PlannedProductionTime = DataTable_Functions.GetDoubleFromRow("planned_production_time", row);
                result.OperatingTime = DataTable_Functions.GetDoubleFromRow("operating_time", row);
                result.IdealOperatingTime = DataTable_Functions.GetDoubleFromRow("ideal_operating_time", row);
            }

            return result;
        }

        public static OEEData FromCycleDataRow(DataRow row)
        {
            string shiftId = DataTable_Functions.GetRowValue("shift_id", row);
            string cycleId = DataTable_Functions.GetRowValue("cycle_id", row);
            string cycleInstanceId = DataTable_Functions.GetRowValue("cycle_instance_id", row);

            if (shiftId != null && cycleId != null && cycleInstanceId != null)
            {
                var result = new OEEData();
                result.ShiftId = new ShiftId(shiftId);
                result.ConstantQuality = 1;
                result.CycleId = cycleId;
                result.CycleInstanceId = cycleInstanceId;
                result.PlannedProductionTime = DataTable_Functions.GetDoubleFromRow("planned_production_time", row);
                result.OperatingTime = DataTable_Functions.GetDoubleFromRow("operating_time", row);
                result.IdealOperatingTime = DataTable_Functions.GetDoubleFromRow("ideal_operating_time", row);

                return result;
            }

            return null;
        }

        public static OEEData FromSegmentDataRow(DataRow row)
        {
            string shiftId = DataTable_Functions.GetRowValue("shift_id", row);

            if (shiftId != null)
            {
                var result = new OEEData();
                result.ShiftId = new ShiftId(shiftId);
                result.ConstantQuality = 1;
                result.PlannedProductionTime = DataTable_Functions.GetDoubleFromRow("planned_production_time", row);
                result.OperatingTime = DataTable_Functions.GetDoubleFromRow("operating_time", row);
                result.IdealOperatingTime = DataTable_Functions.GetDoubleFromRow("ideal_operating_time", row);

                return result;
            }

            return null;
        }

        public static OEEData FromShiftDataRow(DataRow row)
        {
            string date = DataTable_Functions.GetRowValue("date", row);
            string shift = DataTable_Functions.GetRowValue("shift", row);

            if (date != null && shift != null)
            {
                var result = new OEEData();
                result.ShiftId = ShiftId.Get(date, shift);
                result.ConstantQuality = 1;
                result.PlannedProductionTime = DataTable_Functions.GetDoubleFromRow("planned_production_time", row);
                result.OperatingTime = DataTable_Functions.GetDoubleFromRow("operating_time", row);
                result.IdealOperatingTime = DataTable_Functions.GetDoubleFromRow("ideal_operating_time", row);

                return result;
            }

            return null;
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
