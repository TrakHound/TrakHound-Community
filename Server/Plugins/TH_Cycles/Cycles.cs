// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using TH_Configuration;
using TH_Database;
using TH_GeneratedData;
using TH_GeneratedData.GeneratedEvents;
using TH_Global;
using TH_Global.Functions;
using TH_InstanceTable;
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
                List<CycleData> cycles = null;

                switch (data.Id.ToLower())
                {                  
                    // InstanceTable data after Sample received
                    case "instancedata":

                        var instanceDatas = (List<InstanceData>)data.Data02;

                        cycles = ProcessCycles(instanceDatas);

                        SendCycleData(cycles);

                        break;


                    // InstanceData object after current received
                    case "currentinstancedata":

                        var currentInstanceData = (CurrentInstanceData)data.Data02;

                        var list = new List<InstanceData>();
                        list.Add(currentInstanceData.Data);

                        cycles = ProcessCycles(list);

                        SendCycleData(cycles);

                        break;
                }
            }
        }

        public event SendData_Handler SendData;

        public event Status_Handler StatusChanged;

        public event Status_Handler ErrorOccurred;

        public void Closing() { }

        public Type[] ConfigurationPageTypes { get { return new Type[] { typeof(ConfigurationPage.Page) }; } }

        public bool UseDatabases { get; set; }

        public string TablePrefix { get; set; }

        #endregion

        #region "Properties"

        private Configuration configuration;

        #endregion

        #region "Cycles"

        #region "Database"

        string cycleTableName = TableNames.Cycles;
        string[] cyclePrimaryKey = null; 

        void CreateCycleTable()
        {
            if (configuration.DatabaseId != null) cycleTableName = configuration.DatabaseId + "_" + TableNames.Cycles;
            else cycleTableName = TableNames.Cycles;

            List<ColumnDefinition> columns = new List<ColumnDefinition>();

            columns.Add(new ColumnDefinition("CYCLE_ID", DataType.LargeText, false, true));
            columns.Add(new ColumnDefinition("INSTANCE_ID", DataType.LargeText, false, true));
            columns.Add(new ColumnDefinition("NAME", DataType.LargeText, false, true));
            columns.Add(new ColumnDefinition("EVENT", DataType.LargeText));

            columns.Add(new ColumnDefinition("START_TIME", DataType.DateTime));
            columns.Add(new ColumnDefinition("STOP_TIME", DataType.DateTime));

            columns.Add(new ColumnDefinition("START_TIME_UTC", DataType.DateTime));
            columns.Add(new ColumnDefinition("STOP_TIME_UTC", DataType.DateTime));

            columns.Add(new ColumnDefinition("SHIFT_ID", DataType.MediumText));

            columns.Add(new ColumnDefinition("DURATION", DataType.Double));

            columns.AddRange(GetOverrideColumns());

            ColumnDefinition[] ColArray = columns.ToArray();

            Table.Create(configuration.Databases_Server, cycleTableName, ColArray, cyclePrimaryKey);
        }

        List<ColumnDefinition> GetOverrideColumns()
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

        void AddCycleRow(CycleData cycle)
        {
            var cc = CycleConfiguration.Get(configuration);
            if (cc != null)
            {
                List<string> columns = new List<string>();
                columns.Add("cycle_id");
                columns.Add("instance_id");
                columns.Add("name");
                columns.Add("event");
                columns.Add("start_time");
                columns.Add("stop_time");
                columns.Add("start_time_utc");
                columns.Add("stop_time_utc");
                columns.Add("shift_id");
                columns.Add("duration");

                // Add Override Column Names
                foreach (var ovr in cc.OverrideLinks)
                {
                    var columnName = FormatColumnName(ovr).ToLower();
                    columns.Add(columnName);
                }

                List<object> values = new List<object>();

                values.Add(cycle.CycleId);
                values.Add(cycle.InstanceId);
                values.Add(cycle.Name);
                values.Add(cycle.Event);
                values.Add(cycle.StartTime);
                values.Add(cycle.StopTime);
                values.Add(cycle.StartTimeUtc);
                values.Add(cycle.StopTimeUtc);
                values.Add(cycle.ShiftId);
                values.Add(cycle.Duration.TotalSeconds);

                // Add Override Values
                foreach (var ovr in cycle.CycleOverrides)
                {
                    values.Add(ovr.Value);
                }

                Row.Insert(configuration.Databases_Server, cycleTableName, columns.ToArray(), values.ToArray(), cyclePrimaryKey, true);

            }
        }

        string SetupTableName = TableNames.Cycles_Setup;
        string[] setupPrimaryKey = { "Cycle_Id" };

        void CreateSetupTable()
        {
            if (configuration.DatabaseId != null) SetupTableName = configuration.DatabaseId + "_" + TableNames.Cycles_Setup;
            else SetupTableName = TableNames.Cycles;

            List<ColumnDefinition> columns = new List<ColumnDefinition>();

            columns.Add(new ColumnDefinition("CYCLE_ID", DataType.LargeText, true));
            columns.Add(new ColumnDefinition("IDEAL_CYCLE_TIME", DataType.Long));
            columns.Add(new ColumnDefinition("PARTS_PER_CYCLE", DataType.Long));

            ColumnDefinition[] ColArray = columns.ToArray();

            Table.Create(configuration.Databases_Server, SetupTableName, ColArray, setupPrimaryKey);  
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

            Row.Insert(configuration.Databases_Server, SetupTableName, columns.ToArray(), rowValues, setupPrimaryKey, true);

        }

        #endregion

        #region "Processing"

        // Local variables
        CycleData cycleData;
        DateTime lastTimestamp = DateTime.MinValue;

        List<CycleData> ProcessCycles(List<InstanceData> data)
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
                                        var cycle = ProcessCycleEvent(cycleEvent, instanceData, cc);
                                        if (cycle != null) result.Add(cycle);

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

        CycleData ProcessCycleEvent(Event cycleEvent, InstanceData instanceData, CycleConfiguration cc)
        {
            CycleData result = null;

            // Search for cycle name link in InstanceData
            var instanceValue = instanceData.Values.Find(x => x.Id == cc.CycleNameLink);
            if (instanceValue != null)
            {
                List<CycleOverride> cycleOverrides = new List<CycleOverride>();

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

                    // Get ShiftId from Timestamp
                    var shiftId = TH_ShiftTable.CurrentShiftInfo.Get(configuration, instanceData.Timestamp);

                    CycleData cycle = cycleData;

                    // Check if new cycle is needed
                    if (cycle != null &&
                        (cycle.ShiftId == shiftId.id &&
                        cycle.Name == cycleName &&
                         cycle.Event == cycleEventValue &&
                         CompareOverrideLists(cycle.CycleOverrides, cycleOverrides)
                        ))
                    {
                        //Use current stored cycle
                        UpdateCycleData(cycle, instanceData);
                    }
                    else
                    {
                        if (cycle != null)
                        {
                            ProcessPreviousCycle(cycle, instanceData);

                            if (cycle.Name != cycleName || cycleEventValue == cc.StoppedEventValue)
                            {
                                cycle = CreateCycleData(cycleName);
                            }
                        }
                        else
                        {
                            cycle = CreateCycleData(cycleName);
                        }

                        cycle.CycleOverrides = cycleOverrides.ToList();

                        //Update Cycle
                        UpdateCycleDataEvent(cycle, eventReturn.Value, instanceData, cc);
                        UpdateCycleData(cycle, instanceData);
                    }

                    cycleData = cycle.Copy();
                    result = cycleData;
                }
            }

            return result;
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

        CycleData CreateCycleData(string name)
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
            var productionType = cc.ProductionTypes.Find(x => x.Item1 == eventValue);
            if (productionType != null) cycle.ProductionType = productionType.Item2;
            else cycle.ProductionType = CycleData.CycleProductionType.UNCATEGORIZED;

            // Set/Reset Times & Duration
            cycle.StartTime = data.Timestamp.ToLocalTime();
            cycle.StartTimeUtc = data.Timestamp;
            cycle.StopTime = cycle.StartTime;
            cycle.StopTimeUtc = cycle.StartTimeUtc;

            // Set to local variable
            cycleData = cycle;
        }

        void ProcessPreviousCycle(CycleData cycle, InstanceData data)
        {
            // Set Stop Time
            cycle.StopTime = data.Timestamp.ToLocalTime();
            cycle.StopTimeUtc = data.Timestamp;

            // Add to database
            AddCycleRow(cycle);

            // Set Shift Segment ID
            SetCycleShiftId(cycle, data);
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
            var shiftId = TH_ShiftTable.CurrentShiftInfo.Get(configuration, data.Timestamp);
            if (shiftId != null) cycle.ShiftId = shiftId.id;
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
