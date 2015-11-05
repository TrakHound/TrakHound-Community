// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Linq;

using System.Xml;
using System.Data;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

using TH_Configuration;
using TH_Database;
using TH_Global;
using TH_MTC_Data;
using TH_PlugIns_Server;

namespace TH_InstanceTable
{
    public class InstanceTable : Table_PlugIn
    {

        bool firstPass = true;

        #region "PlugIn"

        public string Name { get { return "TH_InstanceTable"; } }

        public void Initialize(Configuration configuration)
        {
            InstanceConfiguration ic = null;

            if (firstPass)
            {
                ic = ReadXML(configuration.ConfigurationXML);              
            }
            else ic = GetConfiguration(configuration);
            
            if (ic != null)
            {
                if (ic.DataItems.Conditions || ic.DataItems.Events || ic.DataItems.Samples) AddMySQL = true;
                else AddMySQL = false;

                if (firstPass) configuration.CustomClasses.Add(ic);
            }

            firstPass = false;

            config = configuration;

            //SQL_Queue = new Queue();
            //SQL_Queue.SQL = configuration.SQL;

        }

        //public Type Config_Page { get { return typeof(Configuration_Page); } }


        public void Update_Probe(TH_MTC_Data.Components.ReturnData returnData)
        {

            ColumnNames = GetVariablesFromProbeData(returnData);

            if (AddMySQL) CreateInstanceTable(ColumnNames);
            
        }

        public void Update_Current(TH_MTC_Data.Streams.ReturnData returnData)
        {

            CurrentData = returnData;

            InstanceData instanceData = ProcessSingleInstance(returnData);

            CurrentInstanceData cid = new CurrentInstanceData();
            cid.currentData = returnData;
            cid.data = instanceData;

            // Send InstanceData object to other Plugins --
            DataEvent_Data de_d = new DataEvent_Data();
            de_d.id = "CurrentInstanceData";
            de_d.data = cid;

            if (DataEvent != null) DataEvent(de_d);
            // --------------------------------------------
          
        }

        public void Update_Sample(TH_MTC_Data.Streams.ReturnData returnData)
        {
            List<InstanceData> instanceDatas = ProcessInstances(CurrentData, returnData);

            if (AddMySQL) AddRowsToMySQL(ColumnNames, instanceDatas);

            PreviousInstanceData_old = PreviousInstanceData_new;

            // Send instanceDatas to other Plugins --------
            DataEvent_Data de_d = new DataEvent_Data();
            de_d.id = "InstanceData";
            de_d.data = instanceDatas;

            if (DataEvent != null) DataEvent(de_d);
            // --------------------------------------------

        }

        public void Update_DataEvent(DataEvent_Data de_data)
        {

        }

        public event DataEvent_Handler DataEvent;


        public void Closing()
        {


        }

        public Type Config_Page { get { return typeof(ConfigurationPage.Page); } }

        //public ConfigurationPage ConfigPage { get { return new Configuration_Page(); } }

        #endregion

        #region "Properties"

        Configuration config { get; set; }

        bool AddMySQL = false;

        List<string> ColumnNames { get; set; }

        TH_MTC_Data.Streams.ReturnData CurrentData { get; set; }

        #endregion

        #region "Methods"

        #region "Configuration"

        public class InstanceConfiguration
        {

            public InstanceConfiguration() { DataItems = new InstanceConfiguration_DataItems(); }

            public int number { get; set; }

            public InstanceConfiguration_DataItems DataItems;

        }

        public class InstanceConfiguration_DataItems
        {
            public InstanceConfiguration_DataItems() { Omit = new List<string>(); }

            public bool Conditions { get; set; }
            public bool Events { get; set; }
            public bool Samples { get; set; }

            public List<string> Omit;
        }

        InstanceConfiguration ReadXML(XmlDocument configXML)
        {

            InstanceConfiguration result = new InstanceConfiguration();

            XmlNodeList nodes = configXML.SelectNodes("/Settings/InstanceTable");

            if (nodes != null)
            {
                if (nodes.Count > 0)
                {

                    XmlNode node = nodes[0];

                    foreach (XmlNode Child in node.ChildNodes)
                    {
                        if (Child.NodeType == XmlNodeType.Element)
                        {
                            // Read Properties
                            if (Child.InnerText != "")
                            {
                                Type Setting = typeof(InstanceConfiguration);
                                PropertyInfo info = Setting.GetProperty(Child.Name);

                                if (info != null)
                                {
                                    Type t = info.PropertyType;
                                    info.SetValue(result, Convert.ChangeType(Child.InnerText, t), null);
                                }
                                else
                                {
                                    switch (Child.Name.ToLower())
                                    {
                                        case "dataitems":

                                            foreach (XmlNode DataItemNode in Child.ChildNodes)
                                            {
                                                if (DataItemNode.NodeType == XmlNodeType.Element)
                                                {
                                                    switch (DataItemNode.Name.ToLower())
                                                    {
                                                        case "conditions":
                                                            result.DataItems.Conditions = Convert.ToBoolean(DataItemNode.InnerText);
                                                            break;

                                                        case "events":
                                                            result.DataItems.Events = Convert.ToBoolean(DataItemNode.InnerText);
                                                            break;

                                                        case "samples":
                                                            result.DataItems.Samples = Convert.ToBoolean(DataItemNode.InnerText);
                                                            break;

                                                        case "omit":

                                                            foreach (XmlNode OmitNode in DataItemNode.ChildNodes)
                                                            {
                                                                if (OmitNode.NodeType == XmlNodeType.Element)
                                                                {
                                                                    result.DataItems.Omit.Add(OmitNode.Name.ToUpper());
                                                                }
                                                            }

                                                            break;
                                                    }
                                                }
                                            }

                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return result;

        }

        InstanceConfiguration GetConfiguration(Configuration configuration)
        {
            InstanceConfiguration Result = null;

            var customClass = config.CustomClasses.Find(x => x.GetType() == typeof(InstanceConfiguration));
            if (customClass != null) Result = (InstanceConfiguration)customClass;

            return Result;

        }

        #endregion

        #region "MySQL"

        //Queue SQL_Queue;

        public const string TableName = TableNames.Instance;

        void CreateInstanceTable(List<string> variablesToRecord)
        {

            List<ColumnDefinition> columns = new List<ColumnDefinition>();

            columns.Add(new ColumnDefinition("TIMESTAMP", DataType.DateTime));
            columns.Add(new ColumnDefinition("SEQUENCE", DataType.Long));
            columns.Add(new ColumnDefinition("AGENTINSTANCEID", DataType.Long));

            foreach (string variable in variablesToRecord) columns.Add(new ColumnDefinition(variable.ToUpper(), DataType.LargeText));

            ColumnDefinition[] ColArray = columns.ToArray();

            Table.Create(config.Databases, TableName, ColArray, null);  

        }

        void AddRowsToMySQL(List<string> columns, List<InstanceData> instanceDatas)
        {
            System.Diagnostics.Stopwatch stpw = new System.Diagnostics.Stopwatch();
            stpw.Start();

            List<string> reqColumns = new List<string>();

            if (columns != null)
            {
                if (!columns.Contains("TIMESTAMP")) reqColumns.Add("TIMESTAMP");
                if (!columns.Contains("SEQUENCE")) reqColumns.Add("SEQUENCE");
                if (!columns.Contains("AGENTINSTANCEID")) reqColumns.Add("AGENTINSTANCEID");

                List<List<object>> rowValues = new List<List<object>>();

                InstanceData previousData = PreviousInstanceData_old;

                foreach (InstanceData instanceData in instanceDatas)
                {
                    // Get list of just Value Ids to use for Intersect with columns list
                    List<string> valueIdsInInstanceData = new List<string>();
                    foreach (InstanceData.Value value in instanceData.values) valueIdsInInstanceData.Add(value.id.ToUpper());

                    // See if instanceData.values contains any of the Columns (set in InstanceConfiguration.DataItems)
                    if (valueIdsInInstanceData.Intersect(columns).Any())
                    {
                        List<object> values = new List<object>();

                        values.Add(instanceData.timestamp);
                        values.Add(instanceData.sequence);
                        values.Add(instanceData.agentInstanceId);

                        bool anyDifferent = false;

                        foreach (string column in columns)
                        {
                            bool columnInValues = false;
                            bool previousDataFound = false;
                            bool columnInPreviousData = false;
                            bool valueDifferentFromPrev = false;
                            bool omitValue = false;

                            InstanceConfiguration ic = GetConfiguration(config);
                            if (ic != null)
                            {
                                if (ic.DataItems.Omit.Contains(column.ToUpper())) omitValue = true;
                            }

                            InstanceData.Value value = instanceData.values.Find(x => x.id.ToLower() == column.ToLower());

                            // value for column is in instanceData.values
                            columnInValues = value != null;

                            //if a previous row has been found (either from this loop or from a different sample)
                            previousDataFound = previousData != null;

                            InstanceData.Value prevValue = null;

                            if (previousDataFound)
                            {
                                prevValue = previousData.values.Find(x => x.id.ToLower() == column.ToLower());
                                columnInPreviousData = prevValue != null;

                                if (columnInValues && columnInPreviousData)
                                {
                                    valueDifferentFromPrev = value.value != prevValue.value;
                                }
                            }

                            // Decide what to do based on previous bools
                            if (columnInValues && previousDataFound && columnInPreviousData && valueDifferentFromPrev && omitValue)
                            {
                                values.Add(value.value);
                            }
                            else if (columnInValues && previousDataFound && columnInPreviousData && valueDifferentFromPrev)
                            {
                                values.Add(value.value);
                                anyDifferent = true;
                            }
                            else if (columnInValues && previousDataFound)
                            {
                                values.Add(value.value);
                            }
                            else if (columnInValues)
                            {
                                values.Add(value.value);
                                anyDifferent = true;
                            }
                            else if (previousDataFound && columnInPreviousData)
                            {
                                values.Add(prevValue.value);
                            }
                            else
                            {
                                values.Add("");
                            }
                        }

                        if (anyDifferent) rowValues.Add(values);

                    }

                    if (previousData != null)
                    {
                        foreach (InstanceData.Value newval in instanceData.values)
                        {
                            InstanceData.Value val = previousData.values.Find(x => x.id == newval.id);
                            if (val != null)
                            {
                                val.value = newval.value;
                            }
                            else
                            {
                                InstanceData.Value addVal = new InstanceData.Value();
                                addVal.id = newval.id;
                                addVal.value = newval.value;
                                previousData.values.Add(addVal);
                            }
                        }
                    }
                    else previousData = instanceData;

                }

                List<string> columnsMySQL = new List<string>();

                columnsMySQL.AddRange(reqColumns);
                columnsMySQL.AddRange(columns);

                stpw.Stop();
                Logger.Log("InstanceTable AddRowsToMySQL() : Processing : " + stpw.ElapsedMilliseconds + "ms");

                stpw = new System.Diagnostics.Stopwatch();
                stpw.Start();

                Logger.Log("Adding " + rowValues.Count.ToString() + " Rows to MySQL Instance Table...");

                int interval = 100;
                int countLeft = rowValues.Count;


                while (countLeft > 0)
                {
                    IEnumerable<List<object>> ValuesToAdd = rowValues.Take(interval);

                    countLeft -= interval;

                    Row.Insert(config.Databases, TableName, columnsMySQL.ToArray(), ValuesToAdd.ToList(), false);

                    //ThreadInfo threadInfo = new ThreadInfo();
                    //threadInfo.tableName = TableName;
                    //threadInfo.columns = columnsMySQL.ToArray();
                    //threadInfo.values = ValuesToAdd.ToList();
                    //threadInfo.update = false;

                    //ThreadPool.QueueUserWorkItem(new WaitCallback(AddItemToSQLQueue), threadInfo);
                }

                stpw.Stop();
                Logger.Log("InstanceTable AddRowsToMySQL() : Transferring : " + stpw.ElapsedMilliseconds + "ms");
            }

        }

        //class ThreadInfo
        //{
        //    public string tableName { get; set; }
        //    public object[] columns { get; set; }
        //    public List<List<object>> values { get; set; }
        //    public bool update { get; set; }
        //}

        //void AddItemToSQLQueue(object o)
        //{
        //    ThreadInfo threadInfo = (ThreadInfo)o;

        //    string query = Global.Row_Insert_CreateQuery(threadInfo.tableName, threadInfo.columns, threadInfo.values, threadInfo.update);

        //    SQL_Queue.Add(query);
        //}

        #endregion

        #region "Processing"

        List<string> GetVariablesFromProbeData(TH_MTC_Data.Components.ReturnData returnData)
        {

            List<string> Result = new List<string>();

            TH_MTC_Data.Components.DataItemCollection dataItems = TH_MTC_Data.Components.Tools.GetDataItemsFromDevice(returnData.devices[0]);

            InstanceConfiguration ic = GetConfiguration(config);
            if (ic != null)
            {
                if (ic.DataItems.Conditions)
                {
                    // Conditions -------------------------------------------------------------------------
                    foreach (TH_MTC_Data.Components.DataItem dataItem in dataItems.Conditions)
                    {
                        Result.Add(dataItem.id.ToUpper());
                    }
                    // ------------------------------------------------------------------------------------
                }

                if (ic.DataItems.Events)
                {
                    // Events -----------------------------------------------------------------------------
                    foreach (TH_MTC_Data.Components.DataItem dataItem in dataItems.Events)
                    {
                        Result.Add(dataItem.id.ToUpper());
                    }
                    // ------------------------------------------------------------------------------------
                }

                if (ic.DataItems.Samples)
                {
                    // Samples ----------------------------------------------------------------------------
                    foreach (TH_MTC_Data.Components.DataItem dataItem in dataItems.Samples)
                    {
                        Result.Add(dataItem.id.ToUpper());
                    }
                    // ------------------------------------------------------------------------------------
                }
            }

            return Result;

        }

        // Before ProcessInstances()
        InstanceData PreviousInstanceData_old;
        // After ProcessInstances()
        InstanceData PreviousInstanceData_new;

        public class InstanceData
        {
            public InstanceData() { values = new List<Value>(); }

            public DateTime timestamp { get; set; }
            public Int64 sequence { get; set; }
            public Int64 agentInstanceId { get; set; }

            public class Value
            {
                public string id { get; set; }
                public string value { get; set; }
            }

            public List<Value> values;
        }

        class InstanceVariableData
        {
            public string id { get; set; }
            public object value { get; set; }
            public DateTime timestamp { get; set; }
            public Int64 sequence { get; set; }
        }

        public class CurrentInstanceData
        {

            public TH_MTC_Data.Streams.ReturnData currentData { get; set; }
            public InstanceData data { get; set; }

        }

        // Process instance table after receiving Sample Data
        List<InstanceData> ProcessInstances(TH_MTC_Data.Streams.ReturnData currentData, TH_MTC_Data.Streams.ReturnData sampleData)
        {

            List<InstanceData> Result = new List<InstanceData>();

            if (currentData != null && sampleData != null)
            {
                InstanceData previousData = PreviousInstanceData_old;

                if (sampleData.deviceStream != null && currentData.deviceStream != null)
                {
                    // Get all of the DataItems from the DeviceStream object
                    TH_MTC_Data.Streams.DataItemCollection dataItems = TH_MTC_Data.Streams.Tools.GetDataItemsFromDeviceStream(sampleData.deviceStream);

                    // Convert the DataItems to a List of InstanceVariableData objects
                    List<InstanceVariableData> values = GetVariableDataFromDataItemCollection(dataItems);

                    // Get List of Distinct Timestamps
                    IEnumerable<DateTime> timestamps = values.Select(x => x.timestamp).Distinct();

                    // Sort timestamps by DateTime ASC
                    List<DateTime> sortedTimestamps = timestamps.ToList();
                    sortedTimestamps.Sort();

                    // Get List of Variables used
                    IEnumerable<string> usedVariables = values.Select(x => x.id).Distinct();

                    foreach (DateTime timestamp in sortedTimestamps.ToList())
                    {
                        InstanceData data = new InstanceData();

                        // Preset previous values into new InstanceData object
                        if (previousData != null) data.values = previousData.values;

                        // Fill unused variables with the values from the CurrentData object
                        else FillInstanceDataWithCurrentData(usedVariables.ToList(), data, currentData);

                        // Set timestamp for InstanceData object
                        data.timestamp = timestamp;

                        data.agentInstanceId = currentData.header.instanceId;

                        // Get List of Values at this timestamp
                        List<InstanceVariableData> valuesAtTimestamp = values.FindAll(x => x.timestamp == timestamp);

                        foreach (InstanceVariableData ivd in valuesAtTimestamp)
                        {
                            InstanceData.Value oldval = data.values.Find(x => x.id == ivd.id);
                            // if value with id is already in data.values then overwrite the value
                            if (oldval != null)
                            {
                                oldval.value = ivd.value.ToString();
                            }
                            // if not already in data.values then create new InstanceData.Value object and add it
                            else
                            {
                                InstanceData.Value newval = new InstanceData.Value();
                                newval.id = ivd.id;
                                newval.value = ivd.value.ToString();
                                data.values.Add(newval);
                            }

                            data.sequence = ivd.sequence;
                        }

                        Result.Add(data);

                        previousData = new InstanceData();
                        previousData.timestamp = data.timestamp;
                        previousData.sequence = data.sequence;
                        previousData.agentInstanceId = data.agentInstanceId;                     

                        foreach (InstanceData.Value val in data.values)
                        {
                            InstanceData.Value newval = new InstanceData.Value();
                            newval.id = val.id;
                            newval.value = val.value;
                            previousData.values.Add(newval);
                        }
                    }

                    PreviousInstanceData_new = previousData;

                }
            }
            else if (currentData != null)
            {
                InstanceData instanceData = ProcessSingleInstance(currentData);

                Result.Add(instanceData);
            }

            return Result;

        }

        // Process InstanceData after receiving Current Data
        InstanceData ProcessSingleInstance(TH_MTC_Data.Streams.ReturnData currentData)
        {
            InstanceData Result = new InstanceData(); ;

            Result.timestamp = currentData.header.creationTime;

            FillInstanceDataWithCurrentData(new List<string>(), Result, currentData);

            return Result;
        }

        void FillInstanceDataWithCurrentData(List<string> usedVariables, InstanceData data, TH_MTC_Data.Streams.ReturnData currentData)
        {

            // Get all of the DataItems from the DeviceStream object
            TH_MTC_Data.Streams.DataItemCollection dataItems = TH_MTC_Data.Streams.Tools.GetDataItemsFromDeviceStream(currentData.deviceStream);

            // Set Conditions
            foreach (TH_MTC_Data.Streams.Condition condition_DI in dataItems.Conditions)
            {
                if (!usedVariables.Contains(condition_DI.dataItemId))
                {
                    InstanceData.Value value = new InstanceData.Value();
                    value.id = condition_DI.dataItemId;
                    value.value = condition_DI.value;
                    data.values.Add(value);
                }
            }

            // Set Events
            foreach (TH_MTC_Data.Streams.Event event_DI in dataItems.Events)
            {
                if (!usedVariables.Contains(event_DI.dataItemId))
                {
                    InstanceData.Value value = new InstanceData.Value();
                    value.id = event_DI.dataItemId;
                    value.value = event_DI.CDATA;
                    data.values.Add(value);
                }
            }

            // Set Samples
            foreach (TH_MTC_Data.Streams.Sample sample_DI in dataItems.Samples)
            {
                if (!usedVariables.Contains(sample_DI.dataItemId))
                {
                    InstanceData.Value value = new InstanceData.Value();
                    value.id = sample_DI.dataItemId;
                    value.value = sample_DI.CDATA;
                    data.values.Add(value);
                }
            }

        }

        List<InstanceVariableData> GetVariableDataFromDataItemCollection(TH_MTC_Data.Streams.DataItemCollection dataItems)
        {

            List<InstanceVariableData> Result = new List<InstanceVariableData>();

            // Get Conditions
            foreach (TH_MTC_Data.Streams.Condition condition_DI in dataItems.Conditions)
            {
                InstanceVariableData instanceData = new InstanceVariableData();

                instanceData.id = condition_DI.dataItemId;
                instanceData.value = condition_DI.value;
                instanceData.timestamp = condition_DI.timestamp;
                instanceData.sequence = condition_DI.sequence;

                Result.Add(instanceData);
            }

            // Get Events
            foreach (TH_MTC_Data.Streams.Event event_DI in dataItems.Events)
            {
                InstanceVariableData instanceData = new InstanceVariableData();

                instanceData.id = event_DI.dataItemId;
                instanceData.value = event_DI.CDATA;
                instanceData.timestamp = event_DI.timestamp;
                instanceData.sequence = event_DI.sequence;

                Result.Add(instanceData);
            }

            // Get Samples
            foreach (TH_MTC_Data.Streams.Sample sample_DI in dataItems.Samples)
            {
                InstanceVariableData instanceData = new InstanceVariableData();

                instanceData.id = sample_DI.dataItemId;
                instanceData.value = sample_DI.CDATA;
                instanceData.timestamp = sample_DI.timestamp;
                instanceData.sequence = sample_DI.sequence;

                Result.Add(instanceData);
            }

            // Sort List by timestamp ASC
            Result.Sort((x, y) => x.timestamp.Second.CompareTo(y.timestamp.Second));

            return Result;

        }

        #endregion

        #endregion

    }
}
