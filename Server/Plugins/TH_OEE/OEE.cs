// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using TH_Configuration;
using TH_Cycles;
using TH_Database;
using TH_Global;
using TH_Plugins;
using TH_Plugins.Server;
using TH_Plugins.Database;

namespace TH_OEE
{
    public class OEE : IServerPlugin
    {

        #region "PlugIn"

        public string Name { get { return "TH_OEE"; } }

        public void Initialize(Configuration configuration)
        {
            config = configuration;

            CreateTable();
        }


        public void GetSentData(EventData data)
        {
            if (data != null)
            {
                if (data.Id.ToLower() == "cycledata")
                {
                    if (data.Data02.GetType() == typeof(List<CycleData>))
                    {
                        List<CycleData> cycles = (List<CycleData>)data.Data02;

                        ProcessOEE(cycles);
                    }
                }
            }
        }

        public event SendData_Handler SendData;

        public event Status_Handler StatusChanged;

        public event Status_Handler ErrorOccurred;

        public void Closing()
        {

        }

        //public TH_Plugins_Server.ConfigurationPage ConfigurationPage { get { return null; } }

        ////public Type Config_Page { get { return typeof(ConfigurationPage.Page); } }
        //public Type Config_Page { get { return null; } }
        public Type[] ConfigurationPageTypes { get { return null; } }

        public bool UseDatabases { get; set; }

        public string TablePrefix { get; set; }

        #endregion

        #region "Properties"

        Configuration config { get; set; }

        #endregion

        #region "OEE"

        #region "Database"

        string TableName = TableNames.OEE;
        string[] primaryKey = { "SHIFT_ID", "CYCLE_ID", "CYCLE_INSTANCE_ID" };

        void CreateTable()
        {
            if (config.DatabaseId != null) TableName = config.DatabaseId + "_" + TableNames.OEE;
            else TableName = TableNames.OEE;

            List<ColumnDefinition> columns = new List<ColumnDefinition>();

            columns.Add(new ColumnDefinition("SHIFT_ID", DataType.LargeText, true));
            columns.Add(new ColumnDefinition("CYCLE_ID", DataType.LargeText, true));
            columns.Add(new ColumnDefinition("CYCLE_INSTANCE_ID", DataType.LargeText, true));

            columns.Add(new ColumnDefinition("OEE", DataType.Double));

            columns.Add(new ColumnDefinition("AVAILABILITY", DataType.Double));
            columns.Add(new ColumnDefinition("PERFORMANCE", DataType.Double));
            columns.Add(new ColumnDefinition("QUALITY", DataType.Double));

            columns.Add(new ColumnDefinition("OPERATING_TIME", DataType.Double));
            columns.Add(new ColumnDefinition("PLANNED_PRODUCTION_TIME", DataType.Double));
            columns.Add(new ColumnDefinition("IDEAL_OPERATING_TIME", DataType.Double));

            columns.Add(new ColumnDefinition("IDEAL_CYCLE_TIME", DataType.Long));
            columns.Add(new ColumnDefinition("TOTAL_PIECES", DataType.Long));

            columns.Add(new ColumnDefinition("GOOD_PIECES", DataType.Long));

            ColumnDefinition[] ColArray = columns.ToArray();

            Table.Create(config.Databases_Server, TableName, ColArray, primaryKey);  
        }

        void UpdateRows(List<OEEData> datas)
        {
            List<List<object>> rowValues = new List<List<object>>();

            List<string> columns = new List<string>();
            columns.Add("shift_id");
            columns.Add("cycle_id");
            columns.Add("cycle_instance_id");

            columns.Add("oee");
            columns.Add("availability");
            columns.Add("performance");
            columns.Add("quality");

            columns.Add("operating_time");
            columns.Add("planned_production_time");
            columns.Add("ideal_operating_time");

            columns.Add("ideal_cycle_time");
            columns.Add("total_pieces");

            columns.Add("good_pieces");

            foreach (var data in datas)
            {
                List<object> values = new List<object>();

                values.Add(data.ShiftId);
                values.Add(data.CycleId);
                values.Add(data.CycleInstanceId);

                values.Add(data.Oee);
                values.Add(data.Availability);
                values.Add(data.Performance);
                values.Add(data.Quality);

                values.Add(data.OperatingTime);
                values.Add(data.PlannedProductionTime);
                values.Add(data.IdealOperatingTime);

                values.Add(data.IdealCycleTime);

                values.Add(data.TotalPieces);
                values.Add(data.GoodPieces);

                rowValues.Add(values);
            }

            Row.Insert(config.Databases_Server, TableName, columns.ToArray(), rowValues, primaryKey, true);
        }

        #endregion

        #region "Processing"

        void ProcessOEE(List<CycleData> cycles)
        {
            if (cycles.Count > 0)
            {
                var oeeDatas = new List<OEEData>();

                foreach (var cycle in cycles)
                {
                    // Get (or create) the OEEInfo for each cycle
                    OEEData oeeData;
                    oeeData = oeeDatas.Find(x => x.CycleId == cycle.CycleId && x.CycleInstanceId == cycle.InstanceId);
                    if (oeeData == null)
                    {
                        oeeData = new OEEData();
                        oeeData.ConstantQuality = 1; // Change when Quality (TH_Parts) is implemented
                        oeeData.ShiftId = cycle.ShiftId;
                        oeeData.CycleId = cycle.CycleId;
                        oeeData.CycleInstanceId = cycle.InstanceId;
                        oeeDatas.Add(oeeData);
                    }

                    oeeData.PlannedProductionTime = cycle.Duration.TotalSeconds;

                    if (cycle.ProductionType == CycleData.CycleProductionType.IN_PRODUCTION)
                    {
                        oeeData.OperatingTime = cycle.Duration.TotalSeconds;

                        if (cycle.CycleOverrides.Count > 0)
                        {
                            oeeData.IdealOperatingTime = IdealTimeFromOverrides(cycle).TotalSeconds;
                        }
                        else
                        {
                            oeeData.IdealOperatingTime = oeeData.OperatingTime;
                        }                  
                    }
                }

                UpdateRows(oeeDatas);
            }
        }

        private static TimeSpan IdealTimeFromOverrides(CycleData cycle)
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
            idealSeconds = cycle.Duration.TotalSeconds * (avg / 100);

            return TimeSpan.FromSeconds(idealSeconds);
        }

        #region "Cycle Infos (Possibly Obsolete - Need to see how to incorporate Cycle_Setup table values for Ideal_Cycle_Time)"

        class CycleInfo
        {
            public string ShiftId { get; set; }
            public double Duration { get; set; }
            public double Override { get; set; }
            //public int ideal_cycle_time { get; set; }
        }

        List<CycleInfo> GetCyclesInfos(string shiftId)
        {
            List<CycleInfo> Result = new List<CycleInfo>();

            DataTable dt = Table.Get(config.Databases_Server, TableNames.Cycles, "WHERE START_SHIFT='" + shiftId + "'");

            if (dt != null)
            {
                foreach (DataRow row in dt.Rows)
                {
                    if (dt.Columns.Contains("START_SHIFT"))
                    {
                        CycleInfo info = new CycleInfo();
                        info.ShiftId = row["START_SHIFT"].ToString();

                        if (dt.Columns.Contains("DURATION"))
                        {
                            double d = -1;
                            double.TryParse(row["DURATION"].ToString(), out d);
                            if (d >= 0) info.Duration = d;
                        }

                        Result.Add(info);
                    }
                }
            }

            return Result;
        }

        #endregion

        void SendOEETable(List<OEEData> datas)
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
            edata.Id = "OeeTable";
            edata.Data01 = config;
            edata.Data02 = dt;
            if (SendData != null) SendData(edata);
        }

        #endregion

        #endregion

    }
}
