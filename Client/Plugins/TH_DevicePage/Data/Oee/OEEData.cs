using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;

using TH_Global.Functions;

namespace TH_DevicePage
{

    public class OEEData
    {
        public OEEData()
        {
            ShiftSegments = new List<SegmentData>();
        }

        public List<SegmentData> ShiftSegments { get; set; }

        public class SegmentData
        {
            public string ShiftId { get; set; }

            public double PlannedProductionTime { get; set; }
            public double OperatingTime { get; set; }

            // TrakHound added version of IdealCycleTime
            // Represents OperatingTime taking into account Overrides (if overrides = 100%)
            public double IdealOperatingTime { get; set; }

            public double IdealCycleTime { get; set; }
            public double IdealRunRate { get; set; }
            public int TotalPieces { get; set; }

            public int GoodPieces { get; set; }


            public double Oee
            {
                get
                {
                    return Availability * Performance;
                    //return Availability * Performance * Quality;
                }
            }

            public double Availability
            {
                get
                {
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
                    if (TotalPieces > 0)
                    {
                        return Math.Min(1, GoodPieces / TotalPieces);
                    }
                    return 0;
                }
            }
        }

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

                double ppt = 0;
                double ot = 0;

                foreach (var segment in ShiftSegments)
                {
                    ppt += segment.PlannedProductionTime;
                    ot += segment.OperatingTime;
                }

                if (ppt > 0)
                {
                    return Math.Min(1, ot / ppt);
                }
                return 0;
            }
        }

        public double Performance
        {
            get
            {
                if (ConstantPerformance > 0) return ConstantPerformance;

                double ot = 0;
                double iot = 0;

                foreach (var segment in ShiftSegments)
                {
                    ot += segment.OperatingTime;
                    iot += segment.IdealOperatingTime;
                }

                if (ot > 0)
                {
                    return Math.Min(1, iot / ot);
                }
                return 0;
            }
        }

        public double Quality
        {
            get
            {
                if (ConstantQuality > 0) return ConstantQuality;

                double tp = 0;
                double gp = 0;

                foreach (var segment in ShiftSegments)
                {
                    tp += segment.TotalPieces;
                    gp += segment.GoodPieces;
                }

                if (tp > 0)
                {
                    return Math.Min(1, gp / tp);
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


        public static OEEData FromDataTable(DataTable dt)
        {
            var result = new OEEData();

            var dv = dt.AsDataView();
            var temp_dt = dv.ToTable(true, "shift_id");

            foreach (DataRow row in temp_dt.Rows)
            {
                string shiftId = row[0].ToString();

                // Sort Table for just rows matching the ShiftId
                dv = dt.AsDataView();
                dv.RowFilter = "shift_id='" + shiftId + "'";
                var id_dt = dv.ToTable();

                double plannedProductionTime = 0;
                double operatingTime = 0;
                double idealOperatingTime = 0;

                foreach (DataRow idRow in id_dt.Rows)
                {
                    plannedProductionTime += DataTable_Functions.GetDoubleFromRow("planned_production_time", idRow);
                    operatingTime += DataTable_Functions.GetDoubleFromRow("operating_time", idRow);
                    idealOperatingTime += DataTable_Functions.GetDoubleFromRow("ideal_operating_time", idRow);
                }

                var data = new OEEData.SegmentData();
                data.ShiftId = shiftId;
                data.PlannedProductionTime = plannedProductionTime;
                data.OperatingTime = operatingTime;
                data.IdealOperatingTime = idealOperatingTime;
                result.ShiftSegments.Add(data);
            }

            result.ConstantQuality = 1;

            return result;
        }
    }
}
