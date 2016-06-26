// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using TH_Configuration;
using TH_Cycles;
using TH_Global.Shifts;
using TH_Plugins;
using TH_Plugins.Server;
using TH_Shifts;

namespace TH_OEE
{
    public class OEE : IServerPlugin
    {

        #region "PlugIn"

        public string Name { get { return "TH_OEE"; } }

        public void Initialize(DeviceConfiguration configuration)
        {
            config = configuration;

            Database.CycleBased.CreateTable(config);
            Database.ShiftBased.CreateTable(config);
            Database.SegmentBased.CreateTable(config);
        }


        public void GetSentData(EventData data)
        {
            if (data != null)
            {
                if (data.Id.ToLower() == "cycledata")
                {
                    if (data.Data02.GetType() == typeof(List<CycleData>))
                    {
                        var cycles = (List<CycleData>)data.Data02;

                        ProcessOEE(cycles);
                    }
                }
            }
        }

        public event SendData_Handler SendData;

        public event Status_Handler StatusChanged;

        public event Status_Handler ErrorOccurred;

        public void Closing() { }

        public Type[] ConfigurationPageTypes { get { return null; } }

        public bool UseDatabases { get; set; }

        public string TablePrefix { get; set; }

        #endregion

        #region "Properties"

        DeviceConfiguration config { get; set; }

        #endregion

        #region "Processing"

        void ProcessOEE(List<CycleData> cycles)
        {
            if (cycles.Count > 0)
            {
                ProcessCycles(cycles);
                ProcessShifts(cycles);
                ProcessSegments(cycles);
            }
        }

        OEEData cycleOeeData;
        double previousCycleCycleDuration; // rename this variable cause this name sucks

        private void ProcessCycles(List<CycleData> cycles)
        {
            var oeeDatas = new List<OEEData>();

            foreach (var cycle in cycles)
            {
                if (cycleOeeData == null)
                {
                    // Get previous row from database
                    cycleOeeData = Database.CycleBased.GetPrevious(config, cycle.ShiftId, cycle.CycleId, cycle.InstanceId);

                    // If not row found in database then just create a new one
                    if (cycleOeeData == null)
                    {
                        cycleOeeData = new OEEData();
                        cycleOeeData.ConstantQuality = 1; // Change when Quality (TH_Parts) is implemented
                        cycleOeeData.ShiftId = new ShiftId(cycle.ShiftId.Id);
                        cycleOeeData.CycleId = cycle.CycleId;
                        cycleOeeData.CycleInstanceId = cycle.InstanceId;
                    }

                    previousCycleCycleDuration = 0;
                }

                // Get (or create) the OEEInfo for each cycle
                OEEData oeeData;
                oeeData = oeeDatas.Find(x => x.CycleId == cycle.CycleId && x.CycleInstanceId == cycle.InstanceId);
                if (oeeData == null)
                {
                    if (cycleOeeData != null && cycleOeeData.CycleId == cycle.CycleId && cycleOeeData.CycleInstanceId == cycle.InstanceId)
                    {
                        oeeData = cycleOeeData;
                    }
                    else
                    {
                        oeeData = new OEEData();
                        oeeData.ConstantQuality = 1; // Change when Quality (TH_Parts) is implemented
                        oeeData.ShiftId = new ShiftId(cycle.ShiftId.Id);
                        oeeData.CycleId = cycle.CycleId;
                        oeeData.CycleInstanceId = cycle.InstanceId;
                    }

                    oeeDatas.Add(oeeData);
                }

                // Set increment
                double inc = cycle.Duration.TotalSeconds - previousCycleCycleDuration; ;
                if (inc < 0) inc = cycle.Duration.TotalSeconds;
                previousCycleCycleDuration = cycle.Duration.TotalSeconds;

                oeeData.StartTime = cycle.StartTime;
                oeeData.EndTime = cycle.StopTime;
                oeeData.StartTimeUTC = cycle.StartTimeUtc;
                oeeData.EndTimeUTC = cycle.StopTimeUtc;

                oeeData.PlannedProductionTime += inc;

                if (cycle.ProductionType == CycleData.CycleProductionType.IN_PRODUCTION)
                {
                    oeeData.OperatingTime += inc;

                    if (cycle.CycleOverrides.Count > 0)
                    {
                        oeeData.IdealOperatingTime += IdealTimeFromOverrides(cycle, inc).TotalSeconds;
                    }
                    else
                    {
                        oeeData.IdealOperatingTime += inc; ;
                    }
                }

                cycleOeeData = oeeData;
            }

            Database.CycleBased.UpdateRows(config, oeeDatas);

            SendCyclesTable(oeeDatas);
        }

        OEEData shiftOeeData;
        double previousShiftCycleDuration; // rename this variable cause this name sucks

        private void ProcessShifts(List<CycleData> cycles)
        {
            var oeeDatas = new List<OEEData>();

            foreach (var cycle in cycles)
            {
                if (shiftOeeData == null)
                {
                    // Get previous row from database
                    shiftOeeData = Database.ShiftBased.GetPrevious(config, cycle.ShiftId);
                    
                    // If not row found in database then just create a new one
                    if (shiftOeeData == null)
                    {
                        shiftOeeData = new OEEData();
                        shiftOeeData.ConstantQuality = 1; // Change when Quality (TH_Parts) is implemented
                        shiftOeeData.ShiftId = new ShiftId(cycle.ShiftId.Id);
                        shiftOeeData.CycleId = cycle.CycleId;
                        shiftOeeData.CycleInstanceId = cycle.InstanceId;
                    }

                    previousShiftCycleDuration = 0;
                }

                // Get (or create) the OEEInfo for each cycle
                OEEData oeeData;
                oeeData = oeeDatas.Find(x => x.ShiftId.Shift == cycle.ShiftId.Shift);
                if (oeeData == null)
                {
                    if (shiftOeeData != null && shiftOeeData.ShiftId.Shift == cycle.ShiftId.Shift)
                    {
                        oeeData = shiftOeeData;
                    }
                    else
                    {
                        oeeData = new OEEData();
                        oeeData.ConstantQuality = 1; // Change when Quality (TH_Parts) is implemented
                        oeeData.ShiftId = new ShiftId(cycle.ShiftId.Id);
                        oeeData.CycleId = cycle.CycleId;
                        oeeData.CycleInstanceId = cycle.InstanceId;
                    }

                    oeeDatas.Add(oeeData);
                }

                // Set increment
                double inc = cycle.Duration.TotalSeconds - previousShiftCycleDuration;
                if (inc < 0) inc = cycle.Duration.TotalSeconds;
                previousShiftCycleDuration = cycle.Duration.TotalSeconds;

                oeeData.StartTime = cycle.StartTime;
                oeeData.EndTime = cycle.StopTime;
                oeeData.StartTimeUTC = cycle.StartTimeUtc;
                oeeData.EndTimeUTC = cycle.StopTimeUtc;

                oeeData.PlannedProductionTime += inc;

                if (cycle.ProductionType == CycleData.CycleProductionType.IN_PRODUCTION)
                {
                    oeeData.OperatingTime += inc;

                    if (cycle.CycleOverrides.Count > 0)
                    {
                        oeeData.IdealOperatingTime += IdealTimeFromOverrides(cycle, inc).TotalSeconds;
                    }
                    else
                    {
                        oeeData.IdealOperatingTime += inc; ;
                    }
                }

                shiftOeeData = oeeData;
            }

            Database.ShiftBased.UpdateRows(config, oeeDatas);

            SendShiftsTable(oeeDatas);

            SendShiftOee(shiftOeeData.Oee);
            SendShiftAvailability(shiftOeeData.Availability);
            SendShiftPerformance(shiftOeeData.Performance);
        }


        OEEData segmentOeeData;
        double previousSegmentCycleDuration;

        private static string GetUnderscoreDelimitedValue(string val)
        {
            if (val == null) return null;

            string[] words = val.Split('_');
            if (words != null && words.Length > 0)
            {
                var builder = new StringBuilder();

                for (var i = 0; i < words.Length; i++)
                {
                    builder.Append(TH_Global.Functions.String_Functions.UppercaseFirst(words[i]));
                    if (i < words.Length - 1) builder.Append(" ");
                }

                return builder.ToString();
            }

            return TH_Global.Functions.String_Functions.UppercaseFirst(val);
        }


        private void ProcessSegments(List<CycleData> cycles)
        {
            var oeeDatas = new List<OEEData>();

            foreach (var cycle in cycles)
            {
                if (segmentOeeData == null)
                {
                    // Get previous row from database
                    segmentOeeData = Database.SegmentBased.GetPrevious(config, cycle.ShiftId);

                    // If no row found in database then just create a new one
                    if (segmentOeeData == null)
                    {
                        segmentOeeData = new OEEData();
                        segmentOeeData.ConstantQuality = 1; // Change when Quality (TH_Parts) is implemented
                        segmentOeeData.ShiftId = new ShiftId(cycle.ShiftId.Id);
                        segmentOeeData.CycleId = cycle.CycleId;
                        segmentOeeData.CycleInstanceId = cycle.InstanceId;
                    }

                    previousSegmentCycleDuration = 0;
                }

                // Get (or create) the OEEInfo for each cycle
                OEEData oeeData;
                oeeData = oeeDatas.Find(x => x.ShiftId.Id == cycle.ShiftId.Id);
                if (oeeData == null)
                {
                    if (segmentOeeData != null && segmentOeeData.ShiftId.Id == cycle.ShiftId.Id)
                    {
                        oeeData = segmentOeeData;
                    }
                    else
                    {
                        oeeData = new OEEData();
                        oeeData.ConstantQuality = 1; // Change when Quality (TH_Parts) is implemented
                        oeeData.ShiftId = new ShiftId(cycle.ShiftId.Id);
                        oeeData.CycleId = cycle.CycleId;
                        oeeData.CycleInstanceId = cycle.InstanceId;
                    }

                    oeeDatas.Add(oeeData);
                }

                // Set increment
                double inc = cycle.Duration.TotalSeconds - previousSegmentCycleDuration; ;
                if (inc < 0) inc = cycle.Duration.TotalSeconds;
                previousSegmentCycleDuration = cycle.Duration.TotalSeconds;

                oeeData.StartTime = cycle.StartTime;
                oeeData.EndTime = cycle.StopTime;
                oeeData.StartTimeUTC = cycle.StartTimeUtc;
                oeeData.EndTimeUTC = cycle.StopTimeUtc;

                oeeData.PlannedProductionTime += inc;

                if (cycle.ProductionType == CycleData.CycleProductionType.IN_PRODUCTION)
                {
                    oeeData.OperatingTime += inc;

                    if (cycle.CycleOverrides.Count > 0)
                    {
                        oeeData.IdealOperatingTime += IdealTimeFromOverrides(cycle, inc).TotalSeconds;
                    }
                    else
                    {
                        oeeData.IdealOperatingTime += inc; ;
                    }
                }

                segmentOeeData = oeeData;
            }

            Database.SegmentBased.UpdateRows(config, oeeDatas);

            SendShiftsTable(oeeDatas);
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

            return TimeSpan.FromSeconds(idealSeconds);
        }

       
        void SendCyclesTable(List<OEEData> datas)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("SHIFT_ID");
            dt.Columns.Add("CYCLE_ID");
            dt.Columns.Add("CYCLE_INSTANCE_ID");
            dt.Columns.Add("OEE");
            dt.Columns.Add("AVAILABILITY");
            dt.Columns.Add("PERFORMANCE");
            dt.Columns.Add("QUALITY");
            dt.Columns.Add("OPERATING_TIME");
            dt.Columns.Add("PLANNED_PRODUCTION_TIME");
            dt.Columns.Add("IDEAL_OPERATING_TIME");
            dt.Columns.Add("IDEAL_CYCLE_TIME");
            dt.Columns.Add("TOTAL_PIECES");
            dt.Columns.Add("GOOD_PIECES");

            foreach (var data in datas)
            {
                DataRow row = dt.NewRow();

                row["SHIFT_ID"] = data.ShiftId;
                row["CYCLE_ID"] = data.CycleId;
                row["CYCLE_INSTANCE_ID"] = data.CycleInstanceId;
                row["OEE"] = data.Oee;
                row["AVAILABILITY"] = data.Availability;
                row["PERFORMANCE"] = data.Performance;
                row["QUALITY"] = data.Quality;
                row["OPERATING_TIME"] = data.OperatingTime;
                row["PLANNED_PRODUCTION_TIME"] = data.PlannedProductionTime;
                row["IDEAL_OPERATING_TIME"] = data.IdealOperatingTime;
                row["IDEAL_CYCLE_TIME"] = data.IdealCycleTime;
                row["TOTAL_PIECES"] = data.TotalPieces;
                row["GOOD_PIECES"] = data.GoodPieces;

                dt.Rows.Add(row);
            }

            var edata = new EventData();
            edata.Id = "oee_cycles";
            edata.Data01 = config;
            edata.Data02 = dt;
            if (SendData != null) SendData(edata);
        }

        void SendShiftOee(double val)
        {
            var edata = new EventData();
            edata.Id = "oee_shift_oee";
            edata.Data01 = config;
            edata.Data02 = val;
            if (SendData != null) SendData(edata);
        }

        void SendShiftAvailability(double val)
        {
            var edata = new EventData();
            edata.Id = "oee_shift_availability";
            edata.Data01 = config;
            edata.Data02 = val;
            if (SendData != null) SendData(edata);
        }

        void SendShiftPerformance(double val)
        {
            var edata = new EventData();
            edata.Id = "oee_shift_performance";
            edata.Data01 = config;
            edata.Data02 = val;
            if (SendData != null) SendData(edata);
        }

        void SendShiftsTable(List<OEEData> datas)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("DATE");
            dt.Columns.Add("SHIFT");
            dt.Columns.Add("OEE");
            dt.Columns.Add("AVAILABILITY");
            dt.Columns.Add("PERFORMANCE");
            dt.Columns.Add("QUALITY");
            dt.Columns.Add("OPERATING_TIME");
            dt.Columns.Add("PLANNED_PRODUCTION_TIME");
            dt.Columns.Add("IDEAL_OPERATING_TIME");
            dt.Columns.Add("IDEAL_CYCLE_TIME");
            dt.Columns.Add("TOTAL_PIECES");
            dt.Columns.Add("GOOD_PIECES");

            foreach (var data in datas)
            {
                DataRow row = dt.NewRow();

                row["DATE"] = data.ShiftId.FormattedDate;
                row["SHIFT"] = data.ShiftId.Shift;
                row["OEE"] = data.Oee;
                row["AVAILABILITY"] = data.Availability;
                row["PERFORMANCE"] = data.Performance;
                row["QUALITY"] = data.Quality;
                row["OPERATING_TIME"] = data.OperatingTime;
                row["PLANNED_PRODUCTION_TIME"] = data.PlannedProductionTime;
                row["IDEAL_OPERATING_TIME"] = data.IdealOperatingTime;
                row["IDEAL_CYCLE_TIME"] = data.IdealCycleTime;
                row["TOTAL_PIECES"] = data.TotalPieces;
                row["GOOD_PIECES"] = data.GoodPieces;

                dt.Rows.Add(row);
            }

            var edata = new EventData();
            edata.Id = "oee_shifts";
            edata.Data01 = config;
            edata.Data02 = dt;
            if (SendData != null) SendData(edata);
        }

        void SendSegmentsTable(List<OEEData> datas)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("SHIFT_ID");
            dt.Columns.Add("OEE");
            dt.Columns.Add("AVAILABILITY");
            dt.Columns.Add("PERFORMANCE");
            dt.Columns.Add("QUALITY");
            dt.Columns.Add("OPERATING_TIME");
            dt.Columns.Add("PLANNED_PRODUCTION_TIME");
            dt.Columns.Add("IDEAL_OPERATING_TIME");
            dt.Columns.Add("IDEAL_CYCLE_TIME");
            dt.Columns.Add("TOTAL_PIECES");
            dt.Columns.Add("GOOD_PIECES");

            foreach (var data in datas)
            {
                DataRow row = dt.NewRow();

                row["SHIFT_ID"] = data.ShiftId.ToString();
                row["OEE"] = data.Oee;
                row["AVAILABILITY"] = data.Availability;
                row["PERFORMANCE"] = data.Performance;
                row["QUALITY"] = data.Quality;
                row["OPERATING_TIME"] = data.OperatingTime;
                row["PLANNED_PRODUCTION_TIME"] = data.PlannedProductionTime;
                row["IDEAL_OPERATING_TIME"] = data.IdealOperatingTime;
                row["IDEAL_CYCLE_TIME"] = data.IdealCycleTime;
                row["TOTAL_PIECES"] = data.TotalPieces;
                row["GOOD_PIECES"] = data.GoodPieces;

                dt.Rows.Add(row);
            }

            var edata = new EventData();
            edata.Id = "oee_segments";
            edata.Data01 = config;
            edata.Data02 = dt;
            if (SendData != null) SendData(edata);
        }

        #endregion

    }
}
