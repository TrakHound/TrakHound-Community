// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using TH_Configuration;
using TH_Database;
using TH_GeneratedData.GeneratedEvents;
using TH_Global;
using TH_Global.Functions;
using TH_Global.Shifts;
using TH_InstanceData;
using TH_Plugins;
using TH_Plugins.Database;
using TH_Plugins.Server;

namespace TH_Cycles
{
    public class Cycles : IServerPlugin
    {

        #region "PlugIn"

        public string Name { get { return "TH_Cycles"; } }

        public void Initialize(Configuration config)
        {
            var cc = CycleConfiguration.Read(config.ConfigurationXML);
            if (cc != null)
            {
                config.CustomClasses.Add(cc);

                configuration = config;

                CreateCycleTable();
                CreateSetupTable();

                // $$$ DEBUG $$$
                if (UseDatabases) DEBUG_AddSetupRows();
            }
        }

        public void GetSentData(EventData data)
        {
            if (data != null)
            {
                switch (data.Id.ToLower())
                {                  
                    // InstanceTable data after Sample received
                    case "instancedata":

                        var instanceDatas = (List<InstanceData>)data.Data02;

                        List<CycleData> cycles = ProcessCycles(instanceDatas);
                        if (cycles != null && cycles.Count > 0)
                        {
                            AddCycleRows(cycles);

                            SendCycleData(cycles);
                        }

                        break;
                }
            }
        }

        public event SendData_Handler SendData;

        public event Status_Handler StatusChanged;

        public event Status_Handler ErrorOccurred;

        public void Closing() { }

        //public Type[] ConfigurationPageTypes { get { return new Type[] { typeof(ConfigurationPage.Page) }; } }

        public bool UseDatabases { get; set; }

        public string TablePrefix { get; set; }

        #endregion

        #region "Properties"

        private Configuration configuration;

        #endregion

        #region "Cycles"

        #region "Database"

        string[] cyclePrimaryKey = new string[] { "CYCLE_ID", "INSTANCE_ID", "SHIFT_ID" };

        void CreateCycleTable()
        {
            List<ColumnDefinition> columns = new List<ColumnDefinition>();

            columns.Add(new ColumnDefinition("CYCLE_ID", DataType.LargeText, false, true));
            columns.Add(new ColumnDefinition("INSTANCE_ID", DataType.LargeText, false, true));
            columns.Add(new ColumnDefinition("SHIFT_ID", DataType.MediumText));

            columns.Add(new ColumnDefinition("NAME", DataType.LargeText, false, true));
            columns.Add(new ColumnDefinition("EVENT", DataType.LargeText));

            columns.Add(new ColumnDefinition("START_TIME", DataType.DateTime));
            columns.Add(new ColumnDefinition("STOP_TIME", DataType.DateTime));

            columns.Add(new ColumnDefinition("START_TIME_UTC", DataType.DateTime));
            columns.Add(new ColumnDefinition("STOP_TIME_UTC", DataType.DateTime));

            columns.Add(new ColumnDefinition("DURATION", DataType.Double));

            columns.AddRange(GetOverrideColumns());

            ColumnDefinition[] ColArray = columns.ToArray();

            Table.Create(configuration.Databases_Server, Global.GetTableName(TableNames.Cycles, configuration.DatabaseId), ColArray, cyclePrimaryKey);
        }

        private List<ColumnDefinition> GetOverrideColumns()
        {
            var result = new List<ColumnDefinition>();

            var cc = CycleConfiguration.Get(configuration);
            if (cc != null)
            {
                foreach (var ovr in cc.OverrideLinks)
                {
                    var columnName = FormatColumnName(ovr).ToUpper();

                    var column = new ColumnDefinition(columnName, DataType.Double);

                    result.Add(column);
                }
            }

            return result;
        }

        static string FormatColumnName(string str)
        {
            return str.Replace(' ', '_');
        }

        void AddCycleRows(List<CycleData> cycles)
        {
            var cc = CycleConfiguration.Get(configuration);
            if (cc != null && cycles != null && cycles.Count > 0)
            {
                List<string> columns = new List<string>();
                columns.Add("cycle_id");
                columns.Add("instance_id");
                columns.Add("shift_id");
                columns.Add("name");
                columns.Add("event");
                columns.Add("start_time");
                columns.Add("stop_time");
                columns.Add("start_time_utc");
                columns.Add("stop_time_utc");
                columns.Add("duration");

                // Add Override Column Names
                foreach (var ovr in cc.OverrideLinks)
                {
                    var columnName = FormatColumnName(ovr).ToLower();
                    columns.Add(columnName);
                }

                var rowValues = new List<List<object>>();

                foreach (var cycle in cycles)
                {
                    var values = new List<object>();

                    values.Add(cycle.CycleId);
                    values.Add(cycle.InstanceId);
                    values.Add(cycle.ShiftId.Id);
                    values.Add(cycle.Name);
                    values.Add(cycle.Event);
                    values.Add(cycle.StartTime);
                    values.Add(cycle.StopTime);
                    values.Add(cycle.StartTimeUtc);
                    values.Add(cycle.StopTimeUtc);
                    values.Add(cycle.Duration.TotalSeconds);

                    // Add Override Values
                    foreach (var ovr in cc.OverrideLinks)
                    {
                        var o = cycle.CycleOverrides.Find(x => x.Id == ovr);
                        if (o != null) values.Add(o.Value);
                        else values.Add(null);
                    }

                    rowValues.Add(values);
                }

                Row.Insert(configuration.Databases_Server, Global.GetTableName(TableNames.Cycles, configuration.DatabaseId), columns.ToArray(), rowValues, cyclePrimaryKey, true);
            }
        }


        string[] setupPrimaryKey = { "Cycle_Id" };

        void CreateSetupTable()
        {
            List<ColumnDefinition> columns = new List<ColumnDefinition>();

            columns.Add(new ColumnDefinition("CYCLE_ID", DataType.LargeText, true));
            columns.Add(new ColumnDefinition("IDEAL_CYCLE_TIME", DataType.Long));
            columns.Add(new ColumnDefinition("PARTS_PER_CYCLE", DataType.Long));

            ColumnDefinition[] ColArray = columns.ToArray();

            Table.Create(configuration.Databases_Server, Global.GetTableName(TableNames.Cycles_Setup, configuration.DatabaseId), ColArray, setupPrimaryKey);  
        }

        void DEBUG_AddSetupRows()
        {
            List<List<object>> rowValues = new List<List<object>>();

            List<string> columns = new List<string>();
            columns.Add("cycle_id");
            columns.Add("ideal_cycle_time");
            columns.Add("parts_per_cycle");

            List<object> values;

            values = new List<object>();
            values.Add("CABINET001.NC");
            values.Add(180);
            values.Add(5);
            rowValues.Add(values);

            values = new List<object>();
            values.Add("CABINET035.NC");
            values.Add(162);
            values.Add(3);
            rowValues.Add(values);

            values = new List<object>();
            values.Add("CHAIRARM05.NC");
            values.Add(587);
            values.Add(1);
            rowValues.Add(values);

            Row.Insert(configuration.Databases_Server, Global.GetTableName(TableNames.Cycles_Setup, configuration.DatabaseId), columns.ToArray(), rowValues, setupPrimaryKey, true);
        }

        #endregion

        #region "Processing"

        // Local variables
        CycleData cycleData;
        DateTime lastTimestamp = DateTime.MinValue;
        
        private List<CycleData> ProcessCycles(List<InstanceData> data)
        {
            var result = new List<CycleData>();

            if (data != null && data.Count > 0)
            {
                var orderedData = data.OrderBy(x => x.Timestamp).ToList();

                if (configuration != null)
                {
                    var cc = CycleConfiguration.Get(configuration);
                    if (cc != null)
                    {
                        if (cc.CycleEventName != null)
                        {
                            var gec = GeneratedEventsConfiguration.Get(configuration);
                            if (gec != null)
                            {
                                var cycleEvent = gec.Events.Find(x => x.Name.ToLower() == cc.CycleEventName.ToLower());
                                if (cycleEvent != null)
                                {
                                    var latestData = orderedData.FindAll(x => x.Timestamp > lastTimestamp);

                                    foreach (var instanceData in latestData)
                                    {
                                        result.AddRange(ProcessCycleEvent(cycleEvent, instanceData, cc));
                                        lastTimestamp = instanceData.Timestamp;
                                    }
                                }
                            }
                        } 
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Updates the list passed in args and return a list of CycleData(s) to update in the Database
        /// </summary>
        /// <param name="cycles"></param>
        /// <param name="cycleEvent"></param>
        /// <param name="instanceData"></param>
        /// <param name="cc"></param>
        /// <returns></returns>
        private List<CycleData> ProcessCycleEvent(Event cycleEvent, InstanceData instanceData, CycleConfiguration cc)
        {
            var result = new List<CycleData>();

            // Search for cycle name link in InstanceData
            var instanceValue = instanceData.Values.Find(x => x.Id == cc.CycleNameLink);
            if (instanceValue != null)
            {
                var cycleOverrides = new List<CycleOverride>();

                // Get CycleOverride values from InstanceData
                foreach (var ovr in cc.OverrideLinks)
                {
                    var cycleOverride = GetOverrideFromInstanceData(ovr, instanceData);
                    if (cycleOverride != null) cycleOverrides.Add(cycleOverride);
                }

                // Process Cycle Event using instanceData
                var eventReturn = cycleEvent.Process(instanceData);
                if (eventReturn != null)
                {
                    // Get the name of the cycle
                    string cycleName = instanceValue.Value;

                    // Get the name of the cycleEvent (cycleEvent.Value)
                    string cycleEventValue = eventReturn.Value;

                    TH_Shifts.CurrentShiftInfo startShiftInfo = null;
                    TH_Shifts.CurrentShiftInfo endShiftInfo = null;

                    // Get ShiftId from Timestamp
                    if (cycleData != null) startShiftInfo = TH_Shifts.CurrentShiftInfo.Get(configuration, cycleData.StartTime);
                    if (instanceData != null) endShiftInfo = TH_Shifts.CurrentShiftInfo.Get(configuration, instanceData.Timestamp);


                    CycleData cycle = cycleData;

                    // Check if new cycle is needed
                    if (cycle != null &&
                        (cycle.ShiftId.Id == GetShiftId(endShiftInfo) &&
                         cycle.Name == cycleName &&
                         cycle.Event == cycleEventValue &&
                         CompareOverrideLists(cycle.CycleOverrides, cycleOverrides)
                        ))
                    {
                        // Use current stored cycle
                        UpdateCycleData(cycle, instanceData);
                        result.Add(cycle.Copy());
                    }
                    else
                    {
                        if (cycle != null)
                        {
                            // If a different shift then split the times
                            if (GetShiftId(startShiftInfo) != GetShiftId(endShiftInfo))
                            {
                                cycle.StopTime = startShiftInfo.segmentEnd;
                                cycle.StopTimeUtc = startShiftInfo.segmentEndUTC;
                                result.Add(cycle.Copy());

                                // Set new Shift ID and StartTimes for next shift segment (keep same cycle instance ID)
                                SetCycleShiftId(cycle, instanceData);
                                cycle.StartTime = startShiftInfo.segmentEnd;
                                cycle.StartTimeUtc = startShiftInfo.segmentEndUTC;
                            }

                            // Set Stop Time
                            cycle.StopTime = instanceData.Timestamp.ToLocalTime();
                            cycle.StopTimeUtc = instanceData.Timestamp;
                            result.Add(cycle.Copy());

                            if (cycle.Name != cycleName || cycleEventValue == cc.StoppedEventValue)
                            {
                                cycle = CreateCycleData(cycleName);
                            }
                        }
                        else // No previous Cycle so create one (usually when server is first started
                        {
                            cycle = CreateCycleData(cycleName);
                        }

                        cycle.CycleOverrides = cycleOverrides.ToList();

                        // Update Cycle
                        UpdateCycleDataEvent(cycle, eventReturn.Value, instanceData, cc);
                        UpdateCycleData(cycle, instanceData);
                        result.Add(cycle.Copy());
                    }

                    cycleData = cycle.Copy();
                }
            }

            return result;
        }
        
        private static string GetShiftId(TH_Shifts.CurrentShiftInfo info)
        {
            if (info != null) return info.id;
            return null;
        }

        private static bool CompareOverrideLists(List<CycleOverride> l1, List<CycleOverride> l2)
        {
            // Check if one is null and the other isn't
            if (l1 == null && l2 != null) return false;
            if (l1 != null && l2 == null) return false;
            
            // Check count
            if (l1.Count != l2.Count) return false;

            //Check Values and Id's
            for (var x = 0; x <= l1.Count - 1; x++)
            {
                if (l1[x].Id != l2[x].Id ||
                    l1[x].Value != l2[x].Value)
                {
                    return false;
                }
            }

            return true;
        }

        private static CycleOverride GetOverrideFromInstanceData(string overrideId, InstanceData instanceData)
        {
            CycleOverride result = null;

            var val = instanceData.Values.Find(x => x.Id == overrideId);
            if (val != null)
            {
                double d = -1;
                if (double.TryParse(val.Value, out d))
                {
                    result = new CycleOverride();
                    result.Id = overrideId;
                    result.Value = d;
                }
            }

            return result;
        }

        private CycleData CreateCycleData(string name)
        {
            var result = new CycleData();
            result.CycleId = String_Functions.RandomString(80);
            result.Name = name;

            return result;
        }

        void UpdateCycleDataEvent(CycleData cycle, string eventValue, InstanceData data, CycleConfiguration cc)
        {
            // Set new Event Value and InstanceId for Cycle 'Segment'
            cycle.Event = eventValue;
            cycle.InstanceId = String_Functions.RandomString(20);

            // Set Production Type
            var productionType = cc.ProductionTypes.Find(x => x.EventValue == eventValue);
            if (productionType != null) cycle.ProductionType = productionType.ProductionType;
            else cycle.ProductionType = CycleData.CycleProductionType.UNCATEGORIZED;

            // Set/Reset Times & Duration
            cycle.StartTime = data.Timestamp.ToLocalTime();
            cycle.StartTimeUtc = data.Timestamp;
            cycle.StopTime = cycle.StartTime;
            cycle.StopTimeUtc = cycle.StartTimeUtc;

            // Set to local variable
            cycleData = cycle;
        }
        
        void UpdateCycleData(CycleData cycle, InstanceData data)
        {
            // Set Stop Time
            cycle.StopTime = data.Timestamp.ToLocalTime();
            cycle.StopTimeUtc = data.Timestamp;

            // Set Shift Segment ID
            SetCycleShiftId(cycle, data);
        }

        void SetCycleShiftId(CycleData cycle, InstanceData data)
        {         
            var shiftId = TH_Shifts.CurrentShiftInfo.Get(configuration, data.Timestamp);
            if (shiftId != null) cycle.ShiftId = new ShiftId(shiftId.id);
        }

        void SendCycleData(List<CycleData> cycleData)
        {
            var data = new EventData();
            data.Id = "CycleData";
            data.Data01 = configuration;
            data.Data02 = cycleData;
            if (SendData != null) SendData(data);
        }

        #endregion

        #endregion

    }
}
