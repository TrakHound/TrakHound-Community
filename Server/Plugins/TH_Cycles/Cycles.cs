// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;
using System.Reflection;
using System.Data;
using System.Threading;

using TH_Configuration;
using TH_Database;
using TH_Global;
using TH_Global.Functions;
using TH_MTConnect;
using TH_Plugins_Server;

using TH_GeneratedData;
using TH_InstanceTable;
using TH_ShiftTable;

namespace TH_Cycles
{
    public class Cycles : IServerPlugin
    {

        #region "PlugIn"

        public string Name { get { return "TH_Cycles"; } }

        public void Initialize(Configuration configuration)
        {
            var cc = CycleConfiguration.Read(configuration.ConfigurationXML);
            if (cc != null)
            {
                configuration.CustomClasses.Add(cc);

                config = configuration;

                if (UseDatabases)
                {
                    CreateCycleTable();
                    AddOverrideColumns();

                    CreateSetupTable();
                }

                // $$$ DEBUG $$$
                if (UseDatabases) DEBUG_AddSetupRows();
            }
        }


        public void Update_Probe(TH_MTConnect.Components.ReturnData returnData)
        {

        }

        public void Update_Current(TH_MTConnect.Streams.ReturnData returnData)
        {

        }

        public void Update_Sample(TH_MTConnect.Streams.ReturnData returnData)
        {

        }


        public void Update_DataEvent(DataEvent_Data de_data)
        {
            if (de_data != null)
            {
                List<CycleData> cycles = null;

                switch (de_data.id.ToLower())
                {                  
                    // InstanceTable data after Sample received
                    case "instancedata":

                        var instanceDatas = (List<InstanceTable.InstanceData>)de_data.data02;

                        cycles = ProcessCycles(instanceDatas);

                        SendCycleData(cycles);

                        break;


                    // InstanceData object after current received
                    case "currentinstancedata":

                        var currentInstanceData = (InstanceTable.CurrentInstanceData)de_data.data02;

                        var data = currentInstanceData.data;

                        var list = new List<InstanceTable.InstanceData>();
                        list.Add(data);

                        cycles = ProcessCycles(list);

                        SendCycleData(cycles);

                        break;
                }
            }
        }

        public event DataEvent_Handler DataEvent;

        public event Status_Handler StatusChanged;

        public event Status_Handler ErrorOccurred;

        public void Closing()
        {

        }

        public Type Config_Page { get { return typeof(ConfigurationPage.Page); } }

        public bool UseDatabases { get; set; }

        public string TablePrefix { get; set; }

        #endregion

        #region "Properties"

        Configuration config { get; set; }

        #endregion

        #region "Cycles"

        #region "Configuration"

        
        #endregion

        #region "Database"

        string cycleTableName = TableNames.Cycles;
        string[] cyclePrimaryKey = null; 

        void CreateCycleTable()
        {
            cycleTableName = TablePrefix + TableNames.Cycles;

            List<ColumnDefinition> columns = new List<ColumnDefinition>();

            columns.Add(new ColumnDefinition("CYCLE_ID", DataType.LargeText, false, true));
            columns.Add(new ColumnDefinition("INSTANCE_ID", DataType.LargeText, false, true));
            columns.Add(new ColumnDefinition("NAME", DataType.LargeText, false, true));
            columns.Add(new ColumnDefinition("Event", DataType.LargeText));

            columns.Add(new ColumnDefinition("START_TIME", DataType.DateTime));
            columns.Add(new ColumnDefinition("STOP_TIME", DataType.DateTime));

            columns.Add(new ColumnDefinition("SHIFT_ID", DataType.MediumText));

            columns.Add(new ColumnDefinition("DURATION", DataType.Double));

            ColumnDefinition[] ColArray = columns.ToArray();

            Table.Create(config.Databases_Server, cycleTableName, ColArray, cyclePrimaryKey);
        }

        void AddOverrideColumns()
        {
            var cc = CycleConfiguration.Get(config);
            if (cc != null)
            {
                var CycleColumns = Column.Get(config.Databases_Server, TableNames.Cycles);

                foreach (var ovr in cc.OverrideLinks)
                {
                    if (!CycleColumns.Contains(ovr))
                    {
                        var columnName = FormatColumnName(ovr).ToUpper();

                        Column.Add(config.Databases_Server, TableNames.Cycles, new ColumnDefinition(columnName, DataType.Double));
                    }
                }
            }
        }

        static string FormatColumnName(string str)
        {
            return str.Replace(' ', '_');
        }

        void AddCycleRow(CycleData cycle)
        {
            var cc = CycleConfiguration.Get(config);
            if (cc != null)
            {
                List<string> columns = new List<string>();
                columns.Add("cycle_id");
                columns.Add("instance_id");
                columns.Add("name");
                columns.Add("event");
                columns.Add("start_time");
                columns.Add("stop_time");
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
                values.Add(cycle.ShiftId);
                values.Add(cycle.Duration.TotalSeconds);

                // Add Override Values
                foreach (var ovr in cycle.CycleOverrides)
                {
                    values.Add(ovr.Value);
                }

                Row.Insert(config.Databases_Server, cycleTableName, columns.ToArray(), values.ToArray(), cyclePrimaryKey, true);

            }
        }

        string SetupTableName = TableNames.Cycles_Setup;
        string[] setupPrimaryKey = { "Cycle_Id" };

        void CreateSetupTable()
        {
            SetupTableName = TablePrefix + TableNames.Cycles_Setup;

            List<ColumnDefinition> columns = new List<ColumnDefinition>();

            columns.Add(new ColumnDefinition("CYCLE_ID", DataType.LargeText, true));
            columns.Add(new ColumnDefinition("IDEAL_CYCLE_TIME", DataType.Long));
            columns.Add(new ColumnDefinition("PARTS_PER_CYCLE", DataType.Long));

            ColumnDefinition[] ColArray = columns.ToArray();

            Table.Create(config.Databases_Server, SetupTableName, ColArray, setupPrimaryKey);  
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

            Row.Insert(config.Databases_Server, SetupTableName, columns.ToArray(), rowValues, setupPrimaryKey, true);

        }

        #endregion

        #region "Processing"

        // $$ DEBUG $$
        //const string eventName = "Program_Execution";

        //const string startValue = "Program Started";
        //const string stoppedValue = "Program Stopped";

        //const string programNameLink = "program";
        ////const string programNameLink = "cn5";

        //List<string> overrideLinks = new List<string>() { "feed_ovr", "rapid_ovr" };

        //// Comboboxes for each Event Value
        //List<Tuple<string, CycleData.CycleProductionType>> productionTypes = new List<Tuple<string, CycleData.CycleProductionType>>() 
        //{ 
        //    new Tuple<string, CycleData.CycleProductionType>("Program Started", CycleData.CycleProductionType.IN_PRODUCTION),
        //    new Tuple<string, CycleData.CycleProductionType>("Toolchange", CycleData.CycleProductionType.TOOLING_CHANGEOVER),
        //    new Tuple<string, CycleData.CycleProductionType>("Feedhold", CycleData.CycleProductionType.PAUSED),
        //    new Tuple<string, CycleData.CycleProductionType>("Program Stopped", CycleData.CycleProductionType.STOPPED)
        //};

        
        // Local variables
        CycleData cycleData;
        DateTime lastTimestamp = DateTime.MinValue;

        List<CycleData> ProcessCycles(List<TH_InstanceTable.InstanceTable.InstanceData> data)
        {
            var result = new List<CycleData>();

            if (data != null && data.Count > 0)
            {
                var orderedData = data.OrderBy(x => x.timestamp).ToList();

                if (config != null)
                {
                    var cc = CycleConfiguration.Get(config);
                    if (cc != null)
                    {
                        if (cc.CycleEventName != null)
                        {
                            var gdc = TH_GeneratedData.GeneratedData.GetConfiguration(config);
                            if (gdc != null)
                            {
                                var cycleEvent = gdc.generatedEvents.events.Find(x => x.Name.ToLower() == cc.CycleEventName.ToLower());
                                if (cycleEvent != null)
                                {
                                    var latestData = orderedData.FindAll(x => x.timestamp > lastTimestamp);

                                    foreach (var instanceData in latestData)
                                    {
                                        var cycle = ProcessCycleEvent(cycleEvent, instanceData, cc);
                                        if (cycle != null) result.Add(cycle);

                                        lastTimestamp = instanceData.timestamp;
                                    }
                                }
                            }
                        } 
                    }
                }
            }

            return result;
        }

        CycleData ProcessCycleEvent(GeneratedData.GeneratedEvents.Event cycleEvent, InstanceTable.InstanceData instanceData, CycleConfiguration cc)
        {
            CycleData result = null;

            // Search for cycle name link in InstanceData
            var instanceValue = instanceData.values.Find(x => x.id == cc.CycleNameLink);
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
                var eventReturn = cycleEvent.ProcessEvent(instanceData);
                if (eventReturn != null)
                {
                    // Get the name of the cycle
                    string cycleName = instanceValue.value;

                    // Get the name of the cycleEvent (cycleEvent.Value)
                    string cycleEventValue = eventReturn.Value;

                    // Get ShiftId from Timestamp
                    var shiftId = TH_ShiftTable.CurrentShiftInfo.Get(config, instanceData.timestamp);

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

        private static CycleOverride GetOverrideFromInstanceData(string overrideId, InstanceTable.InstanceData instanceData)
        {
            CycleOverride result = null;

            var val = instanceData.values.Find(x => x.id == overrideId);
            if (val != null)
            {
                double d = -1;
                if (double.TryParse(val.value, out d))
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

        void UpdateCycleDataEvent(CycleData cycle, string eventValue, InstanceTable.InstanceData data, CycleConfiguration cc)
        {
            // Set new Event Value and InstanceId for Cycle 'Segment'
            cycle.Event = eventValue;
            cycle.InstanceId = String_Functions.RandomString(20);

            // Set Production Type
            var productionType = cc.ProductionTypes.Find(x => x.Item1 == eventValue);
            if (productionType != null) cycle.ProductionType = productionType.Item2;
            else cycle.ProductionType = CycleData.CycleProductionType.UNCATEGORIZED;

            // Set/Reset Times & Duration
            cycle.StartTime = data.timestamp;
            cycle.StopTime = cycle.StartTime;

            // Set to local variable
            cycleData = cycle;
        }

        void ProcessPreviousCycle(CycleData cycle, InstanceTable.InstanceData data)
        {
            // Set Stop Time
            cycle.StopTime = data.timestamp;

            // Add to database
            AddCycleRow(cycle);

            // Set Shift Segment ID
            SetCycleShiftId(cycle, data);
        }

        void UpdateCycleData(CycleData cycle, InstanceTable.InstanceData data)
        {
            // Set Stop Time
            cycle.StopTime = data.timestamp;

            // Set Shift Segment ID
            SetCycleShiftId(cycle, data);
        }

        void SetCycleShiftId(CycleData cycle, InstanceTable.InstanceData data)
        {         
            var shiftId = TH_ShiftTable.CurrentShiftInfo.Get(config, data.timestamp);
            if (shiftId != null) cycle.ShiftId = shiftId.id;
        }

        void SendCycleData(List<CycleData> data)
        {
            DataEvent_Data de_data = new DataEvent_Data();
            de_data.id = "CycleData";
            de_data.data01 = config;
            de_data.data02 = data;
            if (DataEvent != null) DataEvent(de_data);
        }

        #endregion

        #endregion

    }
}
