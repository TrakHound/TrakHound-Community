// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;

using System.Data;
using System.Xml;
using System.Reflection;
using System.Threading;

using TH_Configuration;
using TH_Database;
using TH_Global;
using TH_InstanceTable;
using TH_MTC_Data;
using TH_PlugIns_Server;

namespace TH_GeneratedData
{
    public class GeneratedData : Table_PlugIn
    {

        #region "PlugIn"

        public string Name { get { return "TH_GeneratedData"; } }

        //public int Priority { get { return 1; } }

        public void Initialize(Configuration configuration)
        {

            GenDataConfiguration gdc = ReadXML(configuration.ConfigurationXML);

            if (gdc != null)
            {
                configuration.CustomClasses.Add(gdc);

                config = configuration;

                // Snapshot 
                if (gdc.snapshots.UploadToMySQL)
                {
                    CreateSnapShotTable();
                    IntializeRows();
                }


                // Generated Events
                if (gdc.generatedEvents.UploadToMySQL)
                {
                    foreach (GeneratedEvents.Event e in gdc.generatedEvents.events) CreateGeneratedEventTable(e);
                }  
            }

            //SQL_Queue = new Queue();
            //SQL_Queue.SQL = configuration.SQL;

        }


        public void Update_Probe(TH_MTC_Data.Components.ReturnData returnData)
        {


        }

        public void Update_Current(TH_MTC_Data.Streams.ReturnData returnData)
        {

        }

        public void Update_Sample(TH_MTC_Data.Streams.ReturnData returnData)
        {


        }

        public void Update_DataEvent(DataEvent_Data de_data)
        {

            if (de_data != null)
            {

                GenDataConfiguration gdc = GetConfiguration(config);
                if (gdc != null)
                {
                    switch (de_data.id.ToLower())
                    {

                        // InstanceTable data after Sample received
                        case "instancedata":

                            List<InstanceTable.InstanceData> instanceDatas = (List<InstanceTable.InstanceData>)de_data.data;

                            List<GeneratedEventItem> geis = ProcessGeneratedEvents(instanceDatas);

                            if (gdc.generatedEvents.UploadToMySQL) InsertGeneratedEventItems(geis);

                            // Send List of GeneratedEventItems to other Plugins--------
                            DataEvent_Data de_dge = new DataEvent_Data();
                            de_dge.id = "GeneratedEventItems";
                            de_dge.data = geis;
                            if (DataEvent != null) DataEvent(de_dge);
                            // ----------------------------------------------------

                            break;


                        // InstanceData object after current received
                        case "currentinstancedata":

                            InstanceTable.CurrentInstanceData currentInstanceData = (InstanceTable.CurrentInstanceData)de_data.data;

                            List<SnapShotItem> snapShots = ProcessSnapShots(previousSSI, currentInstanceData.currentData, currentInstanceData.data);

                            if (gdc.snapshots.UploadToMySQL) UpdateRows(snapShots);

                            previousSSI = snapShots;

                            // Send List of SnapShotItems to other Plugins--------
                            DataEvent_Data de_dss = new DataEvent_Data();
                            de_dss.id = "SnapShotItems";
                            de_dss.data = snapShots;
                            if (DataEvent != null) DataEvent(de_dss);
                            // ----------------------------------------------------

                            break;
                    }
                }
            }

        }

        public event DataEvent_Handler DataEvent;


        public void Closing()
        {

        }

        #endregion

        #region "Properties"

        public Configuration config { get; set; }

        #endregion

        #region "Methods"

        #region "Configuration"

        public class GenDataConfiguration
        {

            public GenDataConfiguration()
            {
                snapshots = new Snapshots();
                generatedEvents = new GeneratedEvents();
            }

            public Snapshots snapshots;

            public GeneratedEvents generatedEvents;

        }

        #region "Snapshot Data"

        public class Snapshots
        {

            public Snapshots() 
            { 
                Items = new List<Item>();
                UploadToMySQL = true;
            }

            public bool UploadToMySQL { get; set; }

            public List<Item> Items;

            public class Item
            {
                public string type { get; set; }

                public string name { get; set; }
                public string link { get; set; }
            }

        }

        #endregion

        #region "Generated Events"

        public class GeneratedEvents
        {

            public GeneratedEvents() 
            { 
                events = new List<Event>();
                UploadToMySQL = true;
            }

            public bool UploadToMySQL { get; set; }

            public List<Event> events;

            #region "SubClasses"

            public class Event
            {

                public Event() 
                { 
                    Values = new List<Value>();
                    CaptureItems = new List<CaptureItem>();
                }

                public string Name { get; set; }

                public List<Value> Values { get; set; }

                public Return Default { get; set; }

                public List<CaptureItem> CaptureItems { get; set; }


                public Return CurrentValue { get; set; }

                public Return PreviousValue { get; set; }

                public Return ProcessEvent(InstanceTable.InstanceData instanceData)
                {

                    Return ReturnValue;

                    ReturnValue = Default;

                    bool ConditionsMet = false;

                    foreach (Value Value in Values)
                    {

                        ConditionsMet = false;

                        foreach (Trigger Trigger in Value.Triggers)
                        {

                            InstanceTable.InstanceData.Value instanceValue = instanceData.values.Find(x => x.id.ToLower() == Trigger.Link.ToLower());
                            if (instanceValue != null)
                            {
                                if (Trigger.Modifier == TriggerModifier.NOT)
                                {
                                    if (instanceValue.value.ToLower() != Trigger.Value.ToLower()) ConditionsMet = true;
                                    else ConditionsMet = false;
                                }
                                else
                                {
                                    if (instanceValue.value.ToLower() == Trigger.Value.ToLower()) ConditionsMet = true;
                                    else ConditionsMet = false;
                                }
                            }

                            // if any triggers are not met then break
                            if (!ConditionsMet) break;

                        }

                        if (ConditionsMet)
                        {

                            foreach (MultiTrigger MultiTrigger in Value.MultiTriggers)
                            {

                                foreach (Trigger MultiTrigger_Trigger in MultiTrigger.Triggers)
                                {

                                    InstanceTable.InstanceData.Value instanceValue = instanceData.values.Find(x => x.id.ToLower() == MultiTrigger_Trigger.Link.ToLower());
                                    if (instanceValue != null)
                                    {
                                        if (MultiTrigger_Trigger.Modifier == TriggerModifier.NOT)
                                        {
                                            if (instanceValue.value.ToLower() != MultiTrigger_Trigger.Value.ToLower()) ConditionsMet = true;
                                            else ConditionsMet = false;
                                        }
                                        else
                                        {
                                            if (instanceValue.value.ToLower() == MultiTrigger_Trigger.Value.ToLower()) ConditionsMet = true;
                                            else ConditionsMet = false;
                                        }
                                    }

                                    // if any trigger is met then break (only one has to be met)
                                    if (ConditionsMet) break;

                                }

                                // if none of the triggers were met then break (at least one has to be met)
                                if (!ConditionsMet) break;

                            }

                        }

                        if (ConditionsMet)
                        {
                            ReturnValue = Value.Result;

                            // Break from loop since it shouldn't be able to meet any more Value's conditions
                            break;
                        }
                    }

                    // Get CaptureItems
                    ReturnValue.CaptureItems.Clear();
                    foreach (CaptureItem item in CaptureItems)
                    {
                        CaptureItem i = new CaptureItem();
                        i.id = item.id;
                        i.link = item.link;

                        InstanceTable.InstanceData.Value instanceValue = instanceData.values.ToList().Find(x => x.id.ToLower() == item.link.ToLower());
                        if (instanceValue != null) i.value = instanceValue.value;
                        else i.value = "";

                        ReturnValue.CaptureItems.Add(i);
                    }

                    ReturnValue.TimeStamp = instanceData.timestamp;

                    return ReturnValue;

                }

            }

            public class Value
            {

                public Value() { Triggers = new List<Trigger>(); MultiTriggers = new List<MultiTrigger>(); }

                public List<Trigger> Triggers { get; set; }
                public List<MultiTrigger> MultiTriggers { get; set; }
                public Return Result { get; set; }

            }

            public enum TriggerModifier
            {
                None = 0,
                NOT = 1
            };

            public class Trigger
            {

                public string Link { get; set; }
                public string Value { get; set; }
                public TriggerModifier Modifier { get; set; }

            }

            public class MultiTrigger
            {

                public List<Trigger> Triggers { get; set; }

            }

            public class Return
            {

                public Return() { CaptureItems = new List<CaptureItem>(); }

                public Int16 NumVal { get; set; }
                public string Value { get; set; }
                public DateTime TimeStamp { get; set; }
                public double Duration { get; set; }

                public List<CaptureItem> CaptureItems { get; set; }

            }

            public class CaptureItem
            {
                public string id {get; set;}
                public string link { get; set; }
                public string value { get; set; }
            }

            #endregion

        }

        #endregion

        GenDataConfiguration ReadXML(XmlDocument configXML)
        {

            GenDataConfiguration Result = new GenDataConfiguration();

            XmlNodeList nodes = configXML.SelectNodes("/Settings/GeneratedData");

            if (nodes != null)
            {
                if (nodes.Count > 0)
                {

                    XmlNode node = nodes[0];

                    foreach (XmlNode Child in node.ChildNodes)
                    {
                        if (Child.NodeType == XmlNodeType.Element)
                        {

                            Type Setting = typeof(GenDataConfiguration);
                            PropertyInfo info = Setting.GetProperty(Child.Name);

                            if (info != null)
                            {
                                Type t = info.PropertyType;
                                info.SetValue(Result, Convert.ChangeType(Child.InnerText, t), null);
                            }
                            else
                            {
                                switch (Child.Name.ToLower())
                                {
                                    case "snapshotdata":

                                        foreach (XmlNode snapshotNode in Child.ChildNodes)
                                        {
                                            if (snapshotNode.NodeType == XmlNodeType.Element)
                                            {
                                                Type snapshotSetting = typeof(Snapshots);
                                                PropertyInfo snapshotinfo = snapshotSetting.GetProperty(snapshotNode.Name);

                                                if (snapshotinfo != null)
                                                {
                                                    Type t = snapshotinfo.PropertyType;
                                                    snapshotinfo.SetValue(Result.snapshots, Convert.ChangeType(snapshotNode.InnerText, t), null);
                                                }
                                                else if (snapshotNode.Attributes != null)
                                                {
                                                    if (snapshotNode.Attributes["name"] != null && snapshotNode.Attributes["link"] != null)
                                                    {
                                                        Snapshots.Item item = new Snapshots.Item();
                                                        item.type = snapshotNode.Name.ToUpper();
                                                        item.name = snapshotNode.Attributes["name"].Value;
                                                        item.link = snapshotNode.Attributes["link"].Value;

                                                        Result.snapshots.Items.Add(item);
                                                    }
                                                }
                                            }
                                        }

                                        break;

                                    case "generatedevents":

                                        Result.generatedEvents = ReadXML_GeneratedEvents(Child);

                                        break;

                                }

                            }
                        }
                    }
                }
            }

            return Result;

        }

        static GeneratedEvents ReadXML_GeneratedEvents(XmlNode Node)
        {

            GeneratedEvents Result = new GeneratedEvents();

            if (Node.HasChildNodes)
            {
                foreach (XmlNode EventNode in Node.ChildNodes)
                {
                    if (EventNode.InnerText != null)
                    {
                        Type Setting = typeof(GeneratedEvents);
                        PropertyInfo info = Setting.GetProperty(EventNode.Name);

                        if (info != null)
                        {
                            Type t = info.PropertyType;
                            info.SetValue(Result, Convert.ChangeType(EventNode.InnerText, t), null);
                        }
                        else if (EventNode.Name.ToLower() == "event")
                        {
                            if (EventNode.NodeType == XmlNodeType.Element && EventNode.Attributes != null)
                            {
                                if (EventNode.Attributes["name"] != null)
                                {

                                    GeneratedEvents.Event Event = new GeneratedEvents.Event();
                                    Event.Values = new List<GeneratedEvents.Value>();

                                    Event.Name = EventNode.Attributes["name"].Value.ToString();

                                    foreach (XmlNode Childnode in EventNode.ChildNodes)
                                    {
                                        if (Childnode.NodeType == XmlNodeType.Element)
                                        {
                                            switch (Childnode.Name.ToLower())
                                            {

                                                case "value":

                                                    GeneratedEvents.Value Value = new GeneratedEvents.Value();
                                                    Value.Triggers = new List<GeneratedEvents.Trigger>();
                                                    Value.MultiTriggers = new List<GeneratedEvents.MultiTrigger>();

                                                    foreach (XmlNode ValueNode in Childnode.ChildNodes)
                                                    {
                                                        if (ValueNode.NodeType == XmlNodeType.Element)
                                                        {
                                                            switch (ValueNode.Name.ToLower())
                                                            {
                                                                case "triggers":

                                                                    foreach (XmlNode TriggerNode in ValueNode)
                                                                    {
                                                                        if (TriggerNode.NodeType == XmlNodeType.Element)
                                                                        {

                                                                            switch (TriggerNode.Name.ToLower())
                                                                            {

                                                                                case "trigger":

                                                                                    GeneratedEvents.Trigger Trigger = ReadXML_Trigger(TriggerNode);
                                                                                    if (Trigger != null) Value.Triggers.Add(Trigger);

                                                                                    break;


                                                                                case "multitrigger":

                                                                                    GeneratedEvents.MultiTrigger MultiTrigger = ReadXML_MultiTrigger(TriggerNode);
                                                                                    if (MultiTrigger != null) Value.MultiTriggers.Add(MultiTrigger);

                                                                                    break;
                                                                            }
                                                                        }
                                                                    }

                                                                    break;

                                                                case "result":

                                                                    GeneratedEvents.Return result = new GeneratedEvents.Return();

                                                                    if (ValueNode.Attributes != null)
                                                                        if (ValueNode.Attributes["numval"] != null)
                                                                            result.NumVal = Convert.ToInt16(ValueNode.Attributes["numval"].Value);

                                                                    result.Value = ValueNode.InnerText;

                                                                    Value.Result = result;

                                                                    break;
                                                            }
                                                        }
                                                    }

                                                    Event.Values.Add(Value);

                                                    break;

                                                case "default":

                                                    GeneratedEvents.Return Default = new GeneratedEvents.Return();

                                                    if (Childnode.Attributes != null)
                                                        if (Childnode.Attributes["numval"] != null)
                                                            Default.NumVal = Convert.ToInt16(Childnode.Attributes["numval"].Value);

                                                    Default.Value = Childnode.InnerText;

                                                    Event.Default = Default;

                                                    break;

                                                case "capture":

                                                    foreach (XmlNode itemNode in Childnode.ChildNodes)
                                                    {
                                                        if (itemNode.NodeType == XmlNodeType.Element)
                                                        {
                                                            if (itemNode.Attributes != null)
                                                            {
                                                                if (itemNode.Attributes["id"] != null && itemNode.Attributes["link"] != null)
                                                                {
                                                                    GeneratedEvents.CaptureItem item = new GeneratedEvents.CaptureItem();
                                                                    item.id = itemNode.Attributes["id"].Value;
                                                                    item.link = itemNode.Attributes["link"].Value;
                                                                    Event.CaptureItems.Add(item);
                                                                }
                                                            }
                                                        }
                                                    }

                                                    break;
                                            }
                                        }
                                    }

                                    Event.CurrentValue = Event.Default;
                                    Event.CurrentValue.TimeStamp = DateTime.MinValue;
                                    Result.events.Add(Event);

                                }
                            }
                        }     
                    }
                }
            }

            return Result;

        }

        static GeneratedEvents.MultiTrigger ReadXML_MultiTrigger(XmlNode Node)
        {

            GeneratedEvents.MultiTrigger Result = new GeneratedEvents.MultiTrigger();
            Result.Triggers = new List<GeneratedEvents.Trigger>();

            foreach (XmlNode TriggerNode in Node)
            {
                if (TriggerNode.NodeType == XmlNodeType.Element)
                {

                    switch (TriggerNode.Name.ToLower())
                    {

                        case "trigger":

                            GeneratedEvents.Trigger Trigger = ReadXML_Trigger(TriggerNode);
                            if (Trigger != null) Result.Triggers.Add(Trigger);

                            break;

                    }
                }
            }

            return Result;

        }

        static GeneratedEvents.Trigger ReadXML_Trigger(XmlNode Node)
        {

            GeneratedEvents.Trigger Result = null;

            if (Node.Attributes != null)
            {
                Result = new GeneratedEvents.Trigger();

                if (Node.Attributes["link"] != null)
                    Result.Link = Node.Attributes["link"].Value;

                if (Node.Attributes["value"] != null)
                    Result.Value = Node.Attributes["value"].Value;

                // Added modifier to enable being able to say != to 'value' (ex. Message != "")
                if (Node.Attributes["modifier"] != null)

                    switch (Node.Attributes["modifier"].Value.ToString().ToLower())
                    {
                        case "not": Result.Modifier = GeneratedEvents.TriggerModifier.NOT; break;
                    }
            }

            return Result;

        }


        public static GenDataConfiguration GetConfiguration(Configuration configuration)
        {
            GenDataConfiguration Result = null;

            var customClass = configuration.CustomClasses.Find(x => x.GetType() == typeof(GenDataConfiguration));
            if (customClass != null) Result = (GenDataConfiguration)customClass;

            return Result;
        }

        #endregion

        #region "MySQL"

        //Queue SQL_Queue;

        #region "Snapshot"

        public const string SnapshotsTableName = TableNames.SnapShots;

        void CreateSnapShotTable()
        {
            List<ColumnDefinition> columns = new List<ColumnDefinition>()
            {
                new ColumnDefinition("TIMESTAMP", DataType.DateTime),
                new ColumnDefinition("NAME", DataType.LargeText),
                new ColumnDefinition("VALUE", DataType.LargeText),
                new ColumnDefinition("PREVIOUS_VALUE", DataType.LargeText)
            };

            ColumnDefinition[] ColArray = columns.ToArray();

            Table.Create(config.Databases, SnapshotsTableName, ColArray, "NAME");
        }

        void IntializeRows()
        {

            GenDataConfiguration gdc = GetConfiguration(config);

            if (gdc != null)
            {

                List<string> Columns = new List<string>();
                Columns.Add("Name");

                List<List<object>> rowValues = new List<List<object>>();

                foreach (Snapshots.Item item in gdc.snapshots.Items)
                {

                    List<object> values = new List<object>();

                    values.Add(item.name);

                    rowValues.Add(values);

                }

                Row.Insert(config.Databases, SnapshotsTableName, Columns.ToArray(), rowValues, true);

            }

        }

        void UpdateRows(List<SnapShotItem> snapShotItems)
        {

            // Set Columns to Update (include Name so that it can Update the row instead of creating a new one)
            List<string> columns = new List<string>();
            columns.Add("Name");
            columns.Add("Timestamp");
            columns.Add("Value");
            columns.Add("Previous_Value");

            List<List<object>> rowValues = new List<List<object>>();

            foreach (SnapShotItem ssi in snapShotItems)
            {
                List<object> values = new List<object>();

                values.Add(ssi.name);
                values.Add(ssi.timestamp);
                values.Add(ssi.value);
                values.Add(ssi.previous_value);

                // only add to query if different (no need sending more info than needed)
                if (ssi.value != ssi.previous_value) rowValues.Add(values);
            }

            Row.Insert(config.Databases, SnapshotsTableName, columns.ToArray(), rowValues, true);

        }

        #endregion

        #region "Generated Events"

        public const string TablePrefix = TableNames.Gen_Events_TablePrefix;

        void CreateGeneratedEventTable(GeneratedEvents.Event e)
        {
            List<ColumnDefinition> columns = new List<ColumnDefinition>()
            {
                new ColumnDefinition("TIMESTAMP", DataType.DateTime),
                new ColumnDefinition("VALUE", DataType.LargeText),
                new ColumnDefinition("NUMVAL", DataType.Long)
            };

            foreach (GeneratedEvents.CaptureItem item in e.CaptureItems) columns.Add(new ColumnDefinition(item.id, DataType.LargeText));

            ColumnDefinition[] ColArray = columns.ToArray();

            Table.Create(config.Databases, TablePrefix + e.Name, ColArray, "TIMESTAMP");
        }

        void InsertGeneratedEventItems(List<GeneratedEventItem> generatedEventItems)
        {
            IEnumerable<string> distinctEventNames = generatedEventItems.Select(x => x.eventName).Distinct();

            foreach (string eventName in distinctEventNames)
            {

                // Set Columns to Update (include Name so that it can Update the row instead of creating a new one)
                List<string> columns = new List<string>();
                columns.Add("Timestamp");
                columns.Add("Value");
                columns.Add("Numval");

                // Add columns to update for CaptureItems
                GenDataConfiguration gdc = GetConfiguration(config);
                if (gdc != null)
                {
                    GeneratedEvents.Event e = gdc.generatedEvents.events.Find(x => x.Name.ToLower() == eventName.ToLower());
                    if (e != null)
                    {
                        foreach (GeneratedEvents.CaptureItem item in e.CaptureItems) columns.Add(item.id);
                    }
                }

                List<List<object>> rowValues = new List<List<object>>();

                List<GeneratedEventItem> eventItems = generatedEventItems.FindAll(x => x.eventName == eventName);
                if (eventItems != null)
                {
                    foreach (GeneratedEventItem eventItem in eventItems)
                    {
                        if (eventItem.value != eventItem.previous_value)
                        {
                            List<object> values = new List<object>();

                            values.Add(eventItem.timestamp);
                            values.Add(eventItem.value);
                            values.Add(eventItem.numval);

                            foreach (GeneratedEvents.CaptureItem item in eventItem.CaptureItems) values.Add(FormatCaptureItemValue(item.value));

                            rowValues.Add(values);
                        }

                    }

                    Row.Insert(config.Databases, TablePrefix + eventName, columns.ToArray(), rowValues, true);

                }

            }

        }

        string FormatCaptureItemValue(string val)
        {
            string Result = val;

            return Result;
        }

        #endregion

        #endregion

        #region "Processing"

        public class GeneratedEventItem
        {
            public GeneratedEventItem() { CaptureItems = new List<GeneratedEvents.CaptureItem>(); }

            public string eventName { get; set; }

            public DateTime timestamp { get; set; }
            public string value { get; set; }
            public int numval { get; set; }
            public string previous_value { get; set; }
            public int previous_numval { get; set; }

            public List<GeneratedEvents.CaptureItem> CaptureItems { get; set; }
        }

        List<GeneratedEventItem> ProcessGeneratedEvents(List<InstanceTable.InstanceData> instanceDatas)
        {

            List<GeneratedEventItem> Result = new List<GeneratedEventItem>();

            // Get GenDataConfiguration object from Configuration.CustomClasses List (if exists)
            GenDataConfiguration gdc = GetConfiguration(config);

            if (gdc != null)
            {
                // Loop through each InstanceData object in instanceDatas
                foreach (InstanceTable.InstanceData instanceData in instanceDatas)
                {
                    // Loop through all of the Events and process Event using instanceData object
                    foreach (GeneratedEvents.Event e in gdc.generatedEvents.events)
                    {
                        GeneratedEvents.Return eventReturn = e.ProcessEvent(instanceData);

                        e.PreviousValue = e.CurrentValue;

                        e.CurrentValue = eventReturn;

                        if (e.PreviousValue != null)
                        {
                            if (e.CurrentValue.Value != e.PreviousValue.Value)
                            {
                                TimeSpan ts = e.CurrentValue.TimeStamp - e.PreviousValue.TimeStamp;

                                eventReturn.Duration = ts.TotalSeconds;
                            }
                        }


                        GeneratedEventItem gei = new GeneratedEventItem();
                        gei.eventName = e.Name;
                        gei.timestamp = e.CurrentValue.TimeStamp;
                        gei.value = e.CurrentValue.Value;
                        gei.numval = e.CurrentValue.NumVal;

                        gei.CaptureItems.AddRange(e.CurrentValue.CaptureItems);

                        if (e.PreviousValue != null)
                        {
                            gei.previous_value = e.PreviousValue.Value;
                            gei.previous_numval = e.PreviousValue.NumVal;

                            // if value different than previous then add it to the methods return List
                            //if (e.CurrentValue.Value != e.PreviousValue.Value)
                            //{
                            //    Result.Add(gei);
                            //}
                            
                            // Just add all of the events even if they are the same (that way variables for a User Interface
                            // are able to be seen (and graphed) in real time even if they dont change for a while)
                            Result.Add(gei);
                        }                        
                    }
                }
            }

            return Result;

        }

        class SnapShotItem
        {
            public DateTime timestamp { get; set; }
            public string name { get; set; }
            public string value { get; set; }
            public string previous_value { get; set; }

            public SnapShotItem Copy()
            {
                SnapShotItem Result = new SnapShotItem();
                Result.timestamp = timestamp;
                Result.name = name;
                Result.value = value;
                Result.previous_value = previous_value;
                return Result;
            }
        }

        List<SnapShotItem> previousSSI;

        List<SnapShotItem> ProcessSnapShots(List<SnapShotItem> lpreviousSSI, TH_MTC_Data.Streams.ReturnData currentData, InstanceTable.InstanceData currentInstanceData)
        {
            List<SnapShotItem> Result = new List<SnapShotItem>();

            if (currentInstanceData != null)
            {
                GenDataConfiguration gdc = GetConfiguration(config);
                if (gdc != null)
                {

                    DataTable variables_DT = null;

                    // Get Variables Table from MySQL (if any snapshots are set to "Variable")
                    if (gdc.snapshots.Items.FindAll(x => x.type.ToLower() == "variable").Count > 0)
                    {
                        variables_DT = Table.Get(config.Databases, TableNames.Variables);
                    }


                   foreach (Snapshots.Item item in gdc.snapshots.Items)
                   {
                       // Get ssi in previousSSI list
                       SnapShotItem ssi = null;
                       if (lpreviousSSI != null) ssi = lpreviousSSI.Find(x => x.name == item.name);
                       if (ssi != null)
                       {
                           ssi.previous_value = ssi.value;
                       }
                       else
                       {
                           ssi = new SnapShotItem();
                           ssi.name = item.name;
                       }

                       switch (item.type.ToLower())
                       {
                           case "collected":

                               if (currentData.deviceStream != null)
                               {
                                   TH_MTC_Data.Streams.DataItemCollection dataItems = TH_MTC_Data.Streams.Tools.GetDataItemsFromDeviceStream(currentData.deviceStream);

                                   bool found = false;

                                   // Seach Conditions
                                   TH_MTC_Data.Streams.Condition condition_DI = dataItems.Conditions.Find(x => x.dataItemId.ToLower() == item.link.ToLower());
                                   if (condition_DI != null)
                                   {
                                       ssi.value = condition_DI.value;
                                       ssi.timestamp = condition_DI.timestamp;
                                       found = true;
                                   }

                                   // Search Events
                                   if (!found)
                                   {
                                       TH_MTC_Data.Streams.Event event_DI = dataItems.Events.Find(x => x.dataItemId.ToLower() == item.link.ToLower());
                                       if (event_DI != null)
                                       {
                                           ssi.value = event_DI.CDATA;
                                           ssi.timestamp = event_DI.timestamp;
                                           found = true;
                                       }
                                   }

                                   // Search Samples
                                   if (!found)
                                   {
                                       TH_MTC_Data.Streams.Sample sample_DI = dataItems.Samples.Find(x => x.dataItemId.ToLower() == item.link.ToLower());
                                       if (sample_DI != null)
                                       {
                                           ssi.value = sample_DI.CDATA;
                                           ssi.timestamp = sample_DI.timestamp;
                                           found = true;
                                       }
                                   }

                                   if (found) Result.Add(ssi.Copy());
                               }

                               break;

                           case "generated":

                               GeneratedEvents.Event e = gdc.generatedEvents.events.Find(x => x.Name.ToLower() == item.link.ToLower());
                               if (e != null)
                               {
                                   GeneratedEvents.Return returnValue = e.ProcessEvent(currentInstanceData);

                                   ssi.value = returnValue.Value;
                                   ssi.timestamp = returnValue.TimeStamp;

                                   //Result.Add(ssi.Copy());

                                   if (ssi.value != ssi.previous_value) Result.Add(ssi.Copy());
                               }

                               break;

                           case "variable":

                               if (variables_DT != null)
                               {
                                   DataRow[] rows = variables_DT.Select("variable = '" + item.link + "'");
                                   if (rows != null)
                                   {
                                       if (rows.Length > 0)
                                       {
                                           ssi.value = rows[0]["value"].ToString();

                                           DateTime timestamp = DateTime.MinValue;
                                           DateTime.TryParse(rows[0]["timestamp"].ToString(), out timestamp);
                                           if (timestamp > DateTime.MinValue) ssi.timestamp = timestamp;

                                           if (ssi.value != ssi.previous_value) Result.Add(ssi.Copy());
                                       }
                                   }
                               }

                               break;

                       }
                   }
                }
            }

            return Result;

        }

        #endregion

        #endregion

    }
}
