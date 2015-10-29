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

using TH_Configuration;
using TH_Database;
using TH_Global;
using TH_MTC_Data;
using TH_PlugIns_Server;

using TH_ShiftTable;

namespace TH_Cycles
{
    public class Cycles : Table_PlugIn
    {

        #region "PlugIn"

        public string Name { get { return "TH_Cycles"; } }

        public int Priority { get { return 3; } }

        public void Initialize(Configuration configuration)
        {
            CycleConfiguration cc = ReadXML(configuration.ConfigurationXML);

            if (cc != null)
            {
                configuration.CustomClasses.Add(cc);

                config = configuration;

                cycleInfos = new List<CycleRowInfo>();

                CreateCycleTable();
                CreateSetupTable();


                // $$$ DEBUG $$$
                DEBUG_AddSetupRows();
            }
        }


        public void Update_Probe(TH_MTC_Data.Components.ReturnData returnData)
        {

        }

        public void Update_Current(TH_MTC_Data.Streams.ReturnData returnData)
        {
            currentData = returnData;
        }

        public void Update_Sample(TH_MTC_Data.Streams.ReturnData returnData)
        {

        }


        public void Update_DataEvent(DataEvent_Data de_data)
        {
            if (de_data != null)
            {
                if (de_data.id.ToLower() == "shifttable_geneventshiftitems")
                {
                    if (de_data.data.GetType() == typeof(List<GenEventShiftItem>))
                    {
                        List<GenEventShiftItem> items = (List<GenEventShiftItem>)de_data.data;

                        ProcessCycles(items);               
                    }
                }
            }
        }

        public event DataEvent_Handler DataEvent;


        public void Closing()
        {

        }

        public ConfigurationPage ConfigPage { get { return null; } }

        #endregion

        #region "Properties"

        Configuration config { get; set; }

        #endregion

        #region "Cycles"

        #region "Configuration"

        public class CycleConfiguration
        {
            public string Event { get; set; }
            public string CycleIdLink { get; set; }
        }

        CycleConfiguration ReadXML(XmlDocument configXML)
        {

            CycleConfiguration Result = new CycleConfiguration();

            XmlNodeList nodes = configXML.SelectNodes("/Settings/Cycles");

            if (nodes != null)
            {
                if (nodes.Count > 0)
                {

                    XmlNode node = nodes[0];

                    foreach (XmlNode Child in node.ChildNodes)
                    {
                        if (Child.NodeType == XmlNodeType.Element)
                        {

                            Type Setting = typeof(CycleConfiguration);
                            PropertyInfo info = Setting.GetProperty(Child.Name);

                            if (info != null)
                            {
                                Type t = info.PropertyType;
                                info.SetValue(Result, Convert.ChangeType(Child.InnerText, t), null);
                            }
                        }
                    }
                }
            }

            return Result;

        }

        public static CycleConfiguration GetConfiguration(Configuration configuration)
        {
            CycleConfiguration Result = null;

            var customClass = configuration.CustomClasses.Find(x => x.GetType() == typeof(CycleConfiguration));
            if (customClass != null) Result = (CycleConfiguration)customClass;

            return Result;
        }


        #endregion

        #region "MySQL"

        void CreateCycleTable()
        {
            List<ColumnDefinition> columns = new List<ColumnDefinition>();

            columns.Add(new ColumnDefinition("SHIFT_ID", DataType.LargeText, true, true));
            columns.Add(new ColumnDefinition("CYCLE_ID", DataType.LargeText, false, true));
            columns.Add(new ColumnDefinition("DATE", DataType.LargeText));

            columns.Add(new ColumnDefinition("CYCLES_TOTAL", DataType.Long));
            columns.Add(new ColumnDefinition("CYCLES_INTERRUPTED", DataType.Long));

            columns.Add(new ColumnDefinition("IDEAL_CYCLE_TIME", DataType.Long));
            columns.Add(new ColumnDefinition("AVG_CYCLE_TIME", DataType.Long));

            columns.Add(new ColumnDefinition("PARTS_PER_CYCLE", DataType.Long));

            columns.Add(new ColumnDefinition("PARTS_TOTAL", DataType.Long));
            columns.Add(new ColumnDefinition("PARTS_REJECTED", DataType.Long));

            ColumnDefinition[] ColArray = columns.ToArray();

            Table.Create(config.Databases, TableNames.Cycles, ColArray, "Shift_Id, Cycle_Id");  

        }

        void AddCycleRows(List<CycleRowInfo> infos)
        {
            List<List<object>> rowValues = new List<List<object>>();

            List<string> columns = new List<string>();
            columns.Add("shift_id");
            columns.Add("cycle_id");
            columns.Add("date");
            columns.Add("cycles_total");
            columns.Add("cycles_interrupted");
            columns.Add("ideal_cycle_time");
            columns.Add("avg_cycle_time");
            columns.Add("parts_per_cycle");
            columns.Add("parts_total");
            columns.Add("parts_rejected");

            foreach (CycleRowInfo info in infos)
            {
                List<object> values = new List<object>();

                values.Add(info.shift_id);
                values.Add(info.cycle_id);
                values.Add(info.date);
                values.Add(info.cycles_total);
                values.Add(info.cycles_interrupted);
                values.Add(info.ideal_cycle_time);
                values.Add(info.avg_cycle_time);
                values.Add(info.parts_per_cycle);
                values.Add(info.parts_total);
                values.Add(info.parts_rejected);

                rowValues.Add(values);
            }

            Row.Insert(config.Databases, TableNames.Cycles, columns.ToArray(), rowValues, true);

        }



        void CreateSetupTable()
        {

            List<ColumnDefinition> columns = new List<ColumnDefinition>();

            columns.Add(new ColumnDefinition("CYCLE_ID", DataType.LargeText, true));
            columns.Add(new ColumnDefinition("IDEAL_CYCLE_TIME", DataType.Long));
            columns.Add(new ColumnDefinition("PARTS_PER_CYCLE", DataType.Long));

            ColumnDefinition[] ColArray = columns.ToArray();

            Table.Create(config.Databases, TableNames.Cycles_Setup, ColArray, "Cycle_Id");  
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

            Row.Insert(config.Databases, TableNames.Cycles_Setup, columns.ToArray(), rowValues, true);

        }

        #endregion

        #region "Processing"

        class CycleRowInfo
        {
            public string shift_id { get; set; }
            public string cycle_id { get; set; }

            public string date { get; set; }

            public int cycles_total { get; set; }
            public int cycles_interrupted { get; set; }

            public int ideal_cycle_time { get; set; }
            public int avg_cycle_time { get; set; }

            public int parts_per_cycle { get; set; }
            public int parts_total { get; set; }
            public int parts_rejected { get; set; }
        }

        class CycleSetupRowInfo
        {
            public string cycle_id { get; set; }
            public int ideal_cycle_time { get; set; }
            public int parts_per_cycle { get; set; }
        }

        List<CycleRowInfo> cycleInfos;

        GenEventShiftItem previousItem;

        void ProcessCycles(List<GenEventShiftItem> items)
        {
            // Get Setup Info
            List<CycleSetupRowInfo> SetupInfos = new List<CycleSetupRowInfo>();
            DataTable cycle_setup = Table.Get(config.Databases, TableNames.Cycles_Setup);
            if (cycle_setup != null)
            {
                foreach (DataRow row in cycle_setup.Rows)
                {
                    CycleSetupRowInfo csri = new CycleSetupRowInfo();
                    if (cycle_setup.Columns.Contains("Cycle_Id")) csri.cycle_id = row["Cycle_Id"].ToString();

                    if (cycle_setup.Columns.Contains("Ideal_Cycle_Time"))
                    {
                        int ideal_cycle_time = -1;
                        if (int.TryParse(row["Ideal_Cycle_Time"].ToString(), out ideal_cycle_time))
                        {
                            csri.ideal_cycle_time = ideal_cycle_time;
                        }
                    }

                    if (cycle_setup.Columns.Contains("Parts_Per_Cycle"))
                    {
                        int parts_per_cycle = -1;
                        if (int.TryParse(row["Parts_Per_Cycle"].ToString(), out parts_per_cycle))
                        {
                            csri.parts_per_cycle = parts_per_cycle;
                        }
                    }

                    SetupInfos.Add(csri);
                }
            }


            CycleConfiguration cc = GetConfiguration(config);

            if (cc != null)
            {
                List<CycleRowInfo> newInfos = new List<CycleRowInfo>();

                // Get only the Items with the Cycle Event name
                List<GenEventShiftItem> cycleEvents = items.FindAll(x => x.eventName.ToLower() == cc.Event.ToLower());

                foreach (GenEventShiftItem item in cycleEvents)
                {
                    TH_GeneratedData.GeneratedData.GeneratedEvents.CaptureItem ci = item.CaptureItems.Find(x => x.id.ToLower() == cc.CycleIdLink.ToLower());
                    if (ci != null)
                    {
                        //string shift_id = ShiftRowInfo.GetId(item.shiftDate, item.segment.shiftConfiguration.id, item.segment.id);
                        string shift_id = Tools.GetShiftId(item.shiftDate, item.segment);
                        string cycle_id = ci.value;

                        CycleRowInfo info = null;
                        info = cycleInfos.Find(x => (x.shift_id == shift_id && x.cycle_id == cycle_id));
                        if (info == null)
                        {
                            info = new CycleRowInfo();
                            info.shift_id = shift_id;
                            info.cycle_id = cycle_id;
                            info.date = item.shiftDate.ToString();
                            cycleInfos.Add(info);
                        }

                        // Increment values if value is different than previousItem's value
                        if (previousItem == null)
                        {
                            if (item.eventNumVal > 0)
                            {
                                info.cycles_total += 1;
                                info.parts_total += info.parts_per_cycle;
                            }
                        }
                        else if (item.eventNumVal != previousItem.eventNumVal && item.eventNumVal > 0)
                        {    
                            info.cycles_total += 1;
                            info.parts_total += info.parts_per_cycle;
                        }

                        CycleSetupRowInfo setupinfo = SetupInfos.Find(x => x.cycle_id.ToLower() == info.cycle_id.ToLower());
                        if (setupinfo != null)
                        {
                            info.ideal_cycle_time = setupinfo.ideal_cycle_time;
                            info.parts_per_cycle = setupinfo.parts_per_cycle;
                        }

                        newInfos.Add(info);

                        previousItem = item;
                    }
                }

                // Upload new data to MySQL
                AddCycleRows(newInfos);
            }
        }

        #endregion

        #endregion

        TH_MTC_Data.Streams.ReturnData currentData;

    }
}
