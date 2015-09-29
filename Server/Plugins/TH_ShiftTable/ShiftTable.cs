using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Xml;
using System.Reflection;

using TH_Configuration;
using TH_GeneratedData;
using TH_MTC_Data;
using TH_MySQL;
using TH_PlugIns_Server;

namespace TH_ShiftTable
{
    public class ShiftTable : Table_PlugIn
    {

        #region "PlugIn"

        public string Name { get { return "TH_ShiftTable"; } }

        public int Priority { get { return 2; } }

        public void Initialize(Configuration configuration)
        {

            previousItems = new List<GeneratedData.GeneratedEventItem>();

            ShiftTableConfiguration stc = ReadXML(configuration.ConfigurationXML);

            if (stc != null)
            {
                configuration.CustomClasses.Add(stc);
            }

            config = configuration;

            CreateTable();
            GetTableColumns();
            AddGeneratedEventColumns();

            rowInfos = GetExistingValues();
        }


        public void Update_Probe(TH_MTC_Data.Components.ReturnData returnData)
        {

        }

        public void Update_Current(TH_MTC_Data.Streams.ReturnData returnData)
        {
            currentData = returnData;

            // Update shift_current in "Variables" table
            CurrentShiftInfo currentShift = GetCurrentShift(returnData);
            if (currentShift != null)
            {
                TH_MySQL.Tables.Variables.Update(config.SQL, "shift_name", currentShift.name, returnData.header.creationTime);

                TH_MySQL.Tables.Variables.Update(config.SQL, "shift_id", currentShift.id, returnData.header.creationTime);

                TH_MySQL.Tables.Variables.Update(config.SQL, "shift_date", currentShift.date, returnData.header.creationTime);

                TH_MySQL.Tables.Variables.Update(config.SQL, "shift_begintime", currentShift.shift.beginTime.ToString(), returnData.header.creationTime);
                TH_MySQL.Tables.Variables.Update(config.SQL, "shift_endtime", currentShift.shift.endTime.ToString(), returnData.header.creationTime);

                TH_MySQL.Tables.Variables.Update(config.SQL, "shift_currenttime", currentShift.currentTime.ToString(), returnData.header.creationTime);
            }
        }

        public void Update_Sample(TH_MTC_Data.Streams.ReturnData returnData)
        {

        }


        public void Update_DataEvent(DataEvent_Data de_data)
        {
            if (de_data != null)
            {
                if (de_data.id.ToLower() == "generatedeventitems")
                {

                    List<GeneratedData.GeneratedEventItem> genEventItems = (List<GeneratedData.GeneratedEventItem>)de_data.data;

                    ProcessShifts(genEventItems);

                }
            }
        }

        public event DataEvent_Handler DataEvent;

        public void Closing()
        {

        }

        #endregion

        #region "Properties"

        Configuration config { get; set; }

        #endregion

        #region "Methods"

        TH_MTC_Data.Streams.ReturnData currentData;

        #region "Configuration"

        public class ShiftTableConfiguration
        {
            public ShiftTableConfiguration()
            {
                shifts = new List<ShiftConfiguration>();
                generatedEvents = new List<GeneratedEventConfiguration>();
            }

            public List<ShiftConfiguration> shifts;

            public List<GeneratedEventConfiguration> generatedEvents;
        }

        public static ShiftTableConfiguration ReadXML(XmlDocument configXML)
        {

            ShiftTableConfiguration Result = new ShiftTableConfiguration();

            XmlNodeList nodes = configXML.SelectNodes("/Settings/ShiftData");

            if (nodes != null)
            {
                if (nodes.Count > 0)
                {

                    XmlNode node = nodes[0];

                    foreach (XmlNode Child in node.ChildNodes)
                    {
                        if (Child.NodeType == XmlNodeType.Element)
                        {

                            Type Setting = typeof(ShiftTableConfiguration);
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
                                    case "shifts":

                                        foreach (XmlNode shiftNode in Child.ChildNodes)
                                        {
                                            if (shiftNode.Name.ToLower() == "shift")
                                            {
                                                if (shiftNode.NodeType == XmlNodeType.Element)
                                                {
                                                    Result.shifts.Add(ProcessShiftConfiguration(shiftNode));
                                                }
                                            }
                                        }

                                        break;

                                    case "generatedevents":

                                        foreach (XmlNode eventNode in Child.ChildNodes)
                                        {

                                            if (eventNode.Name.ToLower() == "event")
                                            {
                                                if (eventNode.NodeType == XmlNodeType.Element)
                                                {
                                                    Result.generatedEvents.Add(ProcessGeneratedEvent(eventNode));
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

            return Result;

        }
        
        #region "Shifts"

        static ShiftConfiguration ProcessShiftConfiguration(XmlNode node)
        {
            ShiftConfiguration Result = null;

            if (node.Attributes != null)
            {
                if (node.Attributes["name"] != null &&
                    node.Attributes["id"] != null &&
                    node.Attributes["begintime"] != null &&
                    node.Attributes["endtime"] != null
                    )
                {

                    Result = new ShiftConfiguration();

                    int id;
                    int.TryParse(node.Attributes["id"].Value, out id);
                    Result.id = id;

                    Result.name = node.Attributes["name"].Value;

                    DateTime beginTime = DateTime.MinValue;
                    if (DateTime.TryParse(node.Attributes["begintime"].Value, out beginTime))
                    {
                        Result.beginTime = new ShiftTime(beginTime, false);
                    }

                    DateTime endTime = DateTime.MinValue;
                    if (DateTime.TryParse(node.Attributes["endtime"].Value, out endTime))
                    {
                        Result.endTime = new ShiftTime(endTime, false);
                    }

                    foreach (XmlNode child in node.ChildNodes)
                    {
                        if (child.NodeType == XmlNodeType.Element)
                        {
                            switch (child.Name.ToLower())
                            {
                                case "segments":

                                    Result.segments = ProcessSegments(child, Result);

                                    break;

                                case "days":

                                    Result.days = ProcessDays(child);

                                    break;
                            }
                        }
                    }

                    // Get last dayValue from Segments to use for ShiftConfiguration.endTime.dayValue
                    //if (Result.segments.Count > 0)
                    //{
                    //    SegmentConfiguration lastSegment = Result.segments[Result.segments.Count - 1];
                    //    if (lastSegment != null)
                    //    {
                    //        Result.endTime.dayValue = lastSegment.dayvalue;
                    //    }
                    //}
                    

                }
            }

            return Result;
        }

        static List<SegmentConfiguration> ProcessSegments(XmlNode node, ShiftConfiguration shiftConfiguration)
        {
            List<SegmentConfiguration> Result = new List<SegmentConfiguration>();

            //int dayValue = 0;

            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element && child.Attributes != null)
                {
                    if (child.Attributes["id"] != null && child.Attributes["begintime"] != null && child.Attributes["endtime"] != null)
                    {
                        SegmentConfiguration sc = new SegmentConfiguration();
                        int id;
                        int.TryParse(child.Attributes["id"].Value, out id);
                        sc.id = id;
                        sc.type = child.Name;
                        sc.shiftConfiguration = shiftConfiguration;

                        if (child.Attributes["dayoffset"] != null)
                        {
                            int dayOffset = 0;
                            int.TryParse(child.Attributes["dayoffset"].Value, out dayOffset);
                            sc.dayOffset = dayOffset;
                        }

                        DateTime beginTime = DateTime.MinValue;
                        if (DateTime.TryParse(child.Attributes["begintime"].Value, out beginTime))
                        {
                            sc.beginTime = new ShiftTime(beginTime, false);
                        }

                        DateTime endTime = DateTime.MinValue;
                        if (DateTime.TryParse(child.Attributes["endtime"].Value, out endTime))
                        {
                            sc.endTime = new ShiftTime(endTime, false);
                        }

                        if (beginTime > DateTime.MinValue && endTime > DateTime.MinValue)
                        {
                            //if (sc.endTime.hour < sc.beginTime.hour) dayValue = 1; ;
                            //sc.endTime.dayValue = dayValue;

                            if (sc.dayOffset > 0)
                            sc.endTime.dayValue = sc.dayOffset;
                            sc.shiftConfiguration.endTime.dayValue = sc.dayOffset;

                            Result.Add(sc);
                        } 
                    }
                }
            }

            return Result;
        }

        static List<DayOfWeek> ProcessDays(XmlNode node)
        {
            List<DayOfWeek> Result = new List<DayOfWeek>();

            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element && child.InnerText != null)
                {
                    if (child.Name.ToLower() == "day")
                    {
                        switch (child.InnerText.ToLower())
                        {
                            case "sunday":
                            case "sun": Result.Add(DayOfWeek.Sunday); break;
                            case "monday":
                            case "mon": Result.Add(DayOfWeek.Monday); break;
                            case "tuesday":
                            case "tue": Result.Add(DayOfWeek.Tuesday); break;
                            case "wednesday":
                            case "wed": Result.Add(DayOfWeek.Wednesday); break;
                            case "thursday":
                            case "thur": Result.Add(DayOfWeek.Thursday); break;
                            case "friday":
                            case "fri": Result.Add(DayOfWeek.Friday); break;
                            case "saturday":
                            case "sat": Result.Add(DayOfWeek.Saturday); break;
                        }
                    }
                }
            }

            return Result;
        }

        #endregion

        #region "GeneratedEvents"

        static GeneratedEventConfiguration ProcessGeneratedEvent(XmlNode node)
        {
            GeneratedEventConfiguration Result = null;

            if (node.InnerText != null)
            {
                Result = new GeneratedEventConfiguration();
                Result.name = node.InnerText.ToUpper();
            }
            
            return Result;
        }

        #endregion

        public static ShiftTableConfiguration GetConfiguration(Configuration configuration)
        {
            ShiftTableConfiguration Result = null;

            var customClass = configuration.CustomClasses.Find(x => x.GetType() == typeof(ShiftTableConfiguration));
            if (customClass != null) Result = (ShiftTableConfiguration)customClass;

            return Result;
        }

        #endregion

        #region "MySQL"

        public const string TableName = "shifts";

        List<string> ShiftTableColumns;

        List<string> GenEventColumns;

        void CreateTable()
        {
            List<string> columns = new List<string>();
            //columns.Add("Row " + MySQL_Tools.BigInt + " NOT NULL AUTO_INCREMENT");
            columns.Add("Id " + MySQL_Tools.VarChar + " UNIQUE NOT NULL");
            columns.Add("Date " + MySQL_Tools.VarChar + " NOT NULL");
            columns.Add("Shift " + MySQL_Tools.VarChar + " NOT NULL");
            columns.Add("SegmentId " + MySQL_Tools.BigInt + " NOT NULL");
            columns.Add("Start " + MySQL_Tools.VarChar + " NOT NULL");
            columns.Add("End " + MySQL_Tools.VarChar + " NOT NULL");
            columns.Add("Type " + MySQL_Tools.VarChar + " NOT NULL");
            columns.Add("TotalTime " + MySQL_Tools.BigInt + " NOT NULL");

            //Global.Table_Create(config.SQL, TableName, columns.ToArray(), "Row, Id");
            Global.Table_Create(config.SQL, TableName, columns.ToArray(), "Id");
        }

        void GetTableColumns()
        {
            ShiftTableColumns = Global.Table_GetColumns(config.SQL, TableName);
        }

        void AddGeneratedEventColumns()
        {
            ShiftTableConfiguration stc = GetConfiguration(config);
            if (stc != null)
            {
                GeneratedData.GenDataConfiguration gdc = GeneratedData.GetConfiguration(config);
                if (gdc != null)
                {
                    GenEventColumns = new List<string>();

                    foreach (GeneratedData.GeneratedEvents.Event genEvent in gdc.generatedEvents.events)
                    {
                        if ((stc.generatedEvents.Find(x => x.name.ToUpper() == genEvent.Name.ToUpper()) != null))
                        {
                            string columnName;

                            foreach (GeneratedData.GeneratedEvents.Value value in genEvent.Values)
                            {
                                columnName = FormatColumnName(genEvent, value);

                                if (!ShiftTableColumns.Contains(columnName))
                                {
                                    string columnDefinition = columnName + " " + MySQL_Tools.BigInt;
                                    Global.Table_AddColumn(config.SQL, TableName, columnDefinition);
                                }

                                GenEventColumns.Add(columnName);
                            }

                            // Add GenEvent.Default column
                            columnName = FormatColumnName(genEvent.Name, genEvent.Default.NumVal, genEvent.Default.Value);

                            if (!ShiftTableColumns.Contains(columnName))
                            {
                                string columnDefinition = columnName + " " + MySQL_Tools.BigInt;
                                Global.Table_AddColumn(config.SQL, TableName, columnDefinition);
                            }

                            GenEventColumns.Add(columnName);
                        }
                    }
                }
            }
        }

        List<ShiftRowInfo> GetExistingValues()
        {
            List<ShiftRowInfo> Result = new List<ShiftRowInfo>();

            DataTable dt = Global.Table_Get(config.SQL, TableName);
            if (dt != null)
            {
                foreach (DataRow row in dt.Rows)
                {
                    ShiftRowInfo sri = new ShiftRowInfo();

                    sri.id = row["id"].ToString();

                    DateTime date = DateTime.MinValue;
                    DateTime.TryParse(row["date"].ToString(), out date);
                    sri.date = new ShiftDate(date);

                    sri.shift = row["shift"].ToString();

                    int segmentid = -1;
                    int.TryParse(row["segmentid"].ToString(), out segmentid);
                    sri.segmentId = segmentid;

                    // get start time -----------------------------------------
                    date = DateTime.MinValue;
                    DateTime.TryParse(row["start"].ToString(), out date);
                    sri.start = new ShiftTime(date);
                    // --------------------------------------------------------

                    // get end time -------------------------------------------
                    date = DateTime.MinValue;
                    DateTime.TryParse(row["end"].ToString(), out date);
                    sri.end = new ShiftTime(date);
                    // --------------------------------------------------------

                    sri.type = row["type"].ToString();

                    int totalTime = -1;
                    int.TryParse(row["totaltime"].ToString(), out totalTime);
                    sri.totalTime = totalTime;

                    // Get all of the rest of the columns as GenEventRowInfos
                    List<object> values = row.ItemArray.ToList();
                    if (values.Count > 8)
                    {
                        for (int x = 9; x <= values.Count - 1; x++)
                        {
                            GenEventRowInfo geri = new GenEventRowInfo();
                            geri.columnName = row.Table.Columns[x].ColumnName;

                            int seconds = -1;
                            int.TryParse(values[x].ToString(), out seconds);
                            geri.seconds = seconds;

                            sri.genEventRowInfos.Add(geri);
                        }
                    }
                   
                    Result.Add(sri);

                }
            }

            return Result;

        }

        void UpdateShiftRows(List<ShiftRowInfo> infos)
        {
            List<List<object>> rowValues = new List<List<object>>();

            List<string> columns = new List<string>();
            columns.Add("id");
            columns.Add("date");
            columns.Add("shift");
            columns.Add("segmentid");
            columns.Add("start");
            columns.Add("end");
            columns.Add("type");

            foreach (string column in GenEventColumns) columns.Add(column);

            columns.Add("totaltime");

            foreach (ShiftRowInfo info in infos)
            {
                List<object> values = new List<object>();

                values.Add(info.id);
                values.Add(info.date.ToString());
                values.Add(info.shift);
                values.Add(info.segmentId);
                values.Add(info.start.ToString());
                values.Add(info.end.ToString());
                values.Add(info.type);

                foreach (string column in GenEventColumns)
                {
                    GenEventRowInfo geri = info.genEventRowInfos.Find(x => x.columnName.ToUpper() == column.ToUpper());
                    if (geri != null) values.Add(geri.seconds);
                    else values.Add(0);
                }

                values.Add(info.totalTime);

                rowValues.Add(values);

            }

            Global.Row_Insert(config.SQL, TableName, columns.ToArray(), rowValues);
        }

        #endregion

        #region "Processing"

        List<ShiftRowInfo> rowInfos;

        void ProcessShifts(List<GeneratedData.GeneratedEventItem> genEventItems)
        {
            //List<GenEventShiftItem> genEventShiftItems = GetGenEventShiftItems(genEventItems);
            List<GenEventShiftItem> genEventShiftItems = NewGetGenEventShiftItems(genEventItems);

            // Send List of GenEventShiftItems to other Plugins
            SendGenEventShiftItems(genEventShiftItems);

            List<ShiftRowInfo> shiftRowInfos = GetShiftRowInfos(genEventShiftItems);

            List<ShiftRowInfo> changedInfos = UpdateRowInfos(shiftRowInfos);

            UpdateShiftRows(changedInfos);

            // Send List of Shift Info to other Plugins
            SendShiftRowInfos(changedInfos);
        }

        void SendShiftRowInfos(List<ShiftRowInfo> infos)
        {
            DataEvent_Data de_dge = new DataEvent_Data();
            de_dge.id = "ShiftTable_ShiftRowInfos";
            de_dge.data = infos;
            if (DataEvent != null) DataEvent(de_dge);
        }

        void SendGenEventShiftItems(List<GenEventShiftItem> items)
        {
            DataEvent_Data de_dge = new DataEvent_Data();
            de_dge.id = "ShiftTable_GenEventShiftItems";
            de_dge.data = items;
            if (DataEvent != null) DataEvent(de_dge);
        }

        // Used to hold information between Samples
        List<GeneratedData.GeneratedEventItem> previousItems;


        // NEW Algorithm 9-24-15 ------------------------------------------------------------------

        #region "New Algorithm"

        class ListInfo
        {
            public ListInfo() { genEventItems = new List<GeneratedData.GeneratedEventItem>(); }

            public string title { get; set; }
            public object data { get; set; }
            public List<GeneratedData.GeneratedEventItem> genEventItems { get; set; }
        }

        List<GenEventShiftItem> NewGetGenEventShiftItems(List<GeneratedData.GeneratedEventItem> genEventItems)
        {
            List<GenEventShiftItem> Result = new List<GenEventShiftItem>();

            ShiftTableConfiguration stc = GetConfiguration(config);
            if (stc != null)
            {
                List<ListInfo> nameInfos = GetListByName(genEventItems);

                foreach (ListInfo nameInfo in nameInfos)
                {
                    GeneratedData.GeneratedEventItem previousItem = previousItems.Find(x => x.eventName.ToLower() == nameInfo.title.ToLower());

                    for (int i = 0; i <= nameInfo.genEventItems.Count - 1; i++)
                    {
                        GeneratedData.GeneratedEventItem item = nameInfo.genEventItems[i];

                        if (previousItem != null)
                        {
                            // Skip items that are not newer (test to see if any real data is lost due to this)
                            if (item.timestamp > previousItem.timestamp)
                            {
                                List<GenEventShiftItem> items = GetItems(stc, previousItem, item);
                                Result.AddRange(items);
                                previousItem = item;
                            }
                        }
                        else
                        {
                            previousItems.Add(item);
                            previousItem = item;
                        }
                        
                        int prevIndex = previousItems.FindIndex(x => x.eventName.ToLower() == nameInfo.title.ToLower());
                        if (prevIndex >= 0) previousItems[prevIndex] = previousItem;
                    }
                }
            }

            return Result;

        }

        static List<GenEventShiftItem> GetItems(ShiftTableConfiguration stc, GeneratedData.GeneratedEventItem item1, GeneratedData.GeneratedEventItem item2)
        {
            List<GenEventShiftItem> Result = new List<GenEventShiftItem>();

            ShiftDate date1 = new ShiftDate(item1.timestamp);
            ShiftDate date2 = new ShiftDate(item2.timestamp);

            int daySpan = date2 - date1;

            for (int x = 0; x <= daySpan; x++)
            {
                DateTime dt = new DateTime(date1.year, date1.month, date1.day);
                ShiftDate date = new ShiftDate(dt.AddDays(x), false);

                foreach (ShiftConfiguration shift in stc.shifts)
                {
                    Result.AddRange(GetItemsDuringShift(item1, item2, date, shift));
                }
            }

            return Result;
        }

        static List<ListInfo> GetListByName(List<GeneratedData.GeneratedEventItem> genEventItems)
        {
            List<ListInfo> Result = new List<ListInfo>();

            // Get a list of all of the distinct (by Event Name) genEventItems
            IEnumerable<string> distinctNames = genEventItems.Select(x => x.eventName).Distinct();

            // Loop through the distinct event names  
            foreach (string distinctName in distinctNames.ToList())
            {
                ListInfo info = new ListInfo();
                info.title = distinctName;
                info.genEventItems = genEventItems.FindAll(x => x.eventName.ToLower() == distinctName.ToLower());
                Result.Add(info);
            }

            return Result;
        }

        static List<GenEventShiftItem> GetItemsDuringShift(GeneratedData.GeneratedEventItem item1, GeneratedData.GeneratedEventItem item2, ShiftDate date, ShiftConfiguration shift)
        {
            List<GenEventShiftItem> Result = new List<GenEventShiftItem>();

            foreach (SegmentConfiguration segment in shift.segments)
            {
                GenEventShiftItem gesi = GetItemDuringSegment(item1, item2, date, segment);
                if (gesi != null) Result.Add(gesi);
            }

            return Result;
        }

        static GenEventShiftItem GetItemDuringSegment(GeneratedData.GeneratedEventItem item1, GeneratedData.GeneratedEventItem item2, ShiftDate date, SegmentConfiguration segment)
        {
            GenEventShiftItem Result = null;

            // -1 = Timestamp does not fall within segment          
            //  0 = Timestamps span entire segment
            //  1 = Start timestamp is more than segment.beginTime
            //  2 = End timestamp is less than segment.endTime
            //  3 = Both timestamps are within segment
            int type = GetItemDuringSegmentType(item1, item2, segment, date);

            if (type >= 0)
            {
                GenEventShiftItem gesi = new GenEventShiftItem();
                gesi.eventName = item1.eventName;
                gesi.eventValue = item1.value;
                gesi.eventNumVal = item1.numval;

                gesi.shiftName = segment.shiftConfiguration.name;
                gesi.segment = segment;

                gesi.CaptureItems = item1.CaptureItems;

                gesi.shiftDate = date;

                // Calculate Start and End timestamps based on the DuringSegmentType
                switch (type)
                {
                    case 0:
                        gesi.start_timestamp = GetDateTimeFromShiftTime(segment.beginTime, date, segment.dayOffset);
                        gesi.end_timestamp = GetDateTimeFromShiftTime(segment.endTime, date, segment.dayOffset);

                        break;

                    case 1:
                        gesi.start_timestamp = item1.timestamp.ToLocalTime();
                        gesi.end_timestamp = GetDateTimeFromShiftTime(segment.endTime, date, segment.dayOffset);
                        break;

                    case 2:
                        gesi.start_timestamp = GetDateTimeFromShiftTime(segment.beginTime, date, segment.dayOffset);
                        gesi.end_timestamp = item2.timestamp.ToLocalTime();
                        break;

                    case 3:
                        gesi.start_timestamp = item1.timestamp.ToLocalTime();
                        gesi.end_timestamp = item2.timestamp.ToLocalTime();
                        break;
                }

                // Insure that end is not less than start
                // Example ------------
                // start_timestamp = 11:50 PM 9/28/2015
                // end_timestamp   = 12:10 AM 9/28/2015
                // --------------------
                if (gesi.end_timestamp < gesi.start_timestamp) gesi.end_timestamp = gesi.end_timestamp.AddDays(1);

                // Calculate duration of GeneratedEventShiftItem
                TimeSpan duration = gesi.end_timestamp - gesi.start_timestamp;
                gesi.duration = duration;

                // Add to Log
                Log(gesi.eventValue + " :: " +
                    item1.timestamp.ToString() + " :: " +
                    item2.timestamp.ToString() + " :: " +
                    gesi.start_timestamp.ToString() + " :: " +
                    gesi.end_timestamp.ToString() + " :: " +
                    gesi.segment.beginTime.ToString() + " :: " +
                    gesi.segment.endTime.ToString() + " :: " +
                    GetShiftId(gesi.shiftDate, gesi.segment.shiftConfiguration.id, gesi.segment.id) + " :: " +
                    type.ToString() + " :: " +
                    gesi.duration.ToString());

                Result = gesi;
            }

            return Result;
        }

        //static GenEventShiftItem GetItemDuringSegment(GeneratedData.GeneratedEventItem item1, GeneratedData.GeneratedEventItem item2, ShiftDate date, SegmentConfiguration segment)
        //{
        //    GenEventShiftItem Result = null;

        //    // -1 = Timestamp does not fall within segment          
        //    //  0 = Timestamps span entire segment
        //    //  1 = Start timestamp is more than segment.beginTime
        //    //  2 = End timestamp is less than segment.endTime
        //    //  3 = Both timestamps are within segment
        //    int type = GetItemDuringSegmentType(item1, item2, segment, date);

        //    if (type >= 0)
        //    {
        //        GenEventShiftItem gesi = new GenEventShiftItem();
        //        gesi.eventName = item1.eventName;
        //        gesi.eventValue = item1.value;
        //        gesi.eventNumVal = item1.numval;

        //        gesi.shiftName = segment.shiftConfiguration.name;
        //        gesi.segment = segment;

        //        gesi.CaptureItems = item1.CaptureItems;

        //        gesi.shiftDate = date;

        //        switch (type)
        //        {
        //            case 0:
        //                //gesi.start_timestamp = GetDateTimeFromShiftTime(segment.beginTime, date);
        //                //gesi.end_timestamp = GetDateTimeFromShiftTime(segment.endTime, date);

        //                DateTime start_0;
        //                //if (new ShiftDate(item2.timestamp) != date) start_0 = GetDateTimeFromShiftTime(segment.beginTime, date);
        //                //else start_0 = GetDateTimeFromShiftTime(segment.beginTime, date, false);
        //                //if (new ShiftDate(item2.timestamp) != date) start_0 = GetDateTimeFromShiftTime(segment.beginTime, date);
        //                start_0 = GetDateTimeFromShiftTime(segment.beginTime, date);

        //                DateTime end_0;
        //                //if (new ShiftDate(item2.timestamp) != date) end_0 = GetDateTimeFromShiftTime(segment.endTime, date);
        //                //else end_0 = GetDateTimeFromShiftTime(segment.endTime, date, false);
        //                end_0 = GetDateTimeFromShiftTime(segment.endTime, date);

        //                gesi.start_timestamp = start_0;
        //                gesi.end_timestamp = end_0;

        //                break;

        //            case 1:

        //                DateTime start_1 = item1.timestamp.ToLocalTime();

        //                DateTime end_1;
        //                if (new ShiftDate(item2.timestamp) != date) end_1 = GetDateTimeFromShiftTime(segment.endTime, date);
        //                else end_1 = GetDateTimeFromShiftTime(segment.endTime, date, false);

        //                gesi.start_timestamp = start_1;
        //                gesi.end_timestamp = end_1;

        //                break;

        //            case 2:
        //                gesi.start_timestamp = GetDateTimeFromShiftTime(segment.beginTime, date);
        //                gesi.end_timestamp = item2.timestamp.ToLocalTime();
        //                break;

        //            case 3:
        //                gesi.start_timestamp = item1.timestamp.ToLocalTime();
        //                gesi.end_timestamp = item2.timestamp.ToLocalTime();
        //                break;
        //        }

        //        if (gesi.end_timestamp < gesi.start_timestamp) gesi.end_timestamp = gesi.end_timestamp.AddDays(1);

        //        TimeSpan duration = gesi.end_timestamp - gesi.start_timestamp;
        //        //TimeSpan duration = segment.endTime - segment.beginTime;

        //        gesi.duration = duration;

        //        // Add to Log
        //        Log(gesi.eventValue + " :: " +
        //            item1.timestamp.ToString() + " :: " + 
        //            item2.timestamp.ToString() + " :: " + 
        //            gesi.start_timestamp.ToString() + " :: " +
        //            gesi.end_timestamp.ToString() + " :: " +
        //            gesi.segment.beginTime.ToString() + " :: " +
        //            gesi.segment.endTime.ToString() + " :: " +
        //            GetShiftId(gesi.shiftDate, gesi.segment.shiftConfiguration.id, gesi.segment.id) + " :: " +
        //            type.ToString() + " :: " + 
        //            gesi.duration.ToString());

        //        Result = gesi;
        //    }

        //    return Result;
        //}

        static int GetItemDuringSegmentType(GeneratedData.GeneratedEventItem item1, GeneratedData.GeneratedEventItem item2, SegmentConfiguration segment, ShiftDate date)
        {
            // Convert times to Local
            DateTime start = item1.timestamp.ToLocalTime();
            DateTime end = item2.timestamp.ToLocalTime();

            // Get DateTime objects from ShiftTime and ShiftDate along with segment.dayOffset
            DateTime segmentStart = GetDateTimeFromShiftTime(segment.beginTime, date, segment.dayOffset);
            DateTime segmentEnd = GetDateTimeFromShiftTime(segment.endTime, date, segment.dayOffset);

            // Account for cases such where start & end are not during the same day as segmentStart and segmentEnd
            // Usually where dayOffset for segment is > 0 and segmentduringtype = 3;
            // Example ----------------
            // start        = 12:05 AM 9/28/2015
            // end          = 12:10 AM 9/28/2015
            // segmentStart = 12:00 AM 9/29/2015
            // segmentEnd   = 12:30 AM 9/29/2015
            // ------------------------
            if ((new ShiftDate(start, false) == new ShiftDate(end, false) && (new ShiftDate(start, false)) != new ShiftDate(segmentStart, false) && new ShiftDate(end, false) != new ShiftDate(segmentEnd, false)))
            {
                segmentStart = segmentStart.Subtract(new TimeSpan(24, 0, 0));
                segmentEnd = segmentEnd.Subtract(new TimeSpan(24, 0, 0));
            }

            // Account for cases such where End is the next day compared to Start
            // Example ----------------
            // segmentStart = 11:00 AM 9/28/2015
            // segmentEnd   = 12:00 AM 9/28/2015
            // ------------------------
            if (segmentEnd < segmentStart) segmentEnd = segmentEnd.AddDays(1);

            int type = -1;

            // Timestamp does not fall within segment
            if ((start < segmentStart && end < segmentStart) || (start > segmentEnd && end > segmentEnd))
                type = -1;

            // Timestamps span entire segment
            else if (start <= segmentStart && end >= segmentEnd)
                type = 0;

            // Start timestamp is more than segment.beginTime
            else if ((start >= segmentStart && start < segmentEnd) && end > segmentEnd)
                type = 1;

            // End timestamp is less than segment.endTime
            else if (start <= segmentStart && (end < segmentEnd && end > segmentStart))
                type = 2;

            // Both timestamps are within segment
            else if ((start >= segmentStart && start < segmentEnd) && end < segmentEnd)
                type = 3;

            return type;
        }

        //static int GetItemDuringSegmentType(GeneratedData.GeneratedEventItem item1, GeneratedData.GeneratedEventItem item2, SegmentConfiguration segment, ShiftDate date)
        //{
        //    ShiftTime start = new ShiftTime(item1.timestamp);
        //    ShiftTime end = new ShiftTime(item2.timestamp);

        //    if (new ShiftDate(item2.timestamp) != date) end.dayValue = 1;
        //    //if (new ShiftDate(item2.timestamp) != date) end.dayValue = dayValue;
        //    //if (new ShiftDate(item1.timestamp) != date) start.dayValue = dayValue - 1;
                

        //    int type = -1;

        //    // Timestamp does not fall within segment
        //    if (start < segment.beginTime && end < segment.beginTime)
        //        type = -1;

        //    // Timestamps span entire segment
        //    else if (start <= segment.beginTime && end >= segment.endTime)
        //        type = 0;

        //    // Start timestamp is more than segment.beginTime
        //    else if ((start >= segment.beginTime && start < segment.endTime) && end > segment.endTime)
        //        type = 1;

        //    // End timestamp is less than segment.endTime
        //    else if (start <= segment.beginTime && (end < segment.endTime && end > segment.beginTime))
        //        type = 2;

        //    // Both timestamps are within segment
        //    else if ((start > segment.beginTime && start < segment.endTime) && end < segment.endTime)
        //        type = 3;

        //    return type;
        //}

        #endregion

        #region "Not Used"

        List<GenEventShiftItem> OldGetGenEventShiftItems(List<GeneratedData.GeneratedEventItem> genEventItems)
        {
            List<GenEventShiftItem> Result = new List<GenEventShiftItem>();

            ShiftTableConfiguration stc = GetConfiguration(config);
            if (stc != null)
            {
                List<ListInfo> nameInfos = GetListByName(genEventItems);

                foreach (ListInfo nameInfo in nameInfos)
                {
                    Console.WriteLine(nameInfo.title + " ---------------------------");

                    GenEventShiftItem previousItem = null;

                    List<ListInfo> dateInfos = GetListByDate(nameInfo.genEventItems);

                    for (int x = 0; x <= dateInfos.Count - 1; x++)
                    {
                        Console.WriteLine(dateInfos[x].title + " --------------------");

                        List<ListInfo> shiftInfos = GetListByShift(stc, dateInfos[x].genEventItems);

                        for (int i = 0; i <= shiftInfos.Count - 1; i++)
                        {
                            Console.WriteLine(shiftInfos[i].title + " --------------");

                            ShiftConfiguration shift = (ShiftConfiguration)shiftInfos[i].data;

                            List<ListInfo> segmentInfos = GetListByShiftSegment(shift, shiftInfos[i].genEventItems);

                            for (int u = 0; u <= segmentInfos.Count - 1; u++)
                            {
                                Console.WriteLine(segmentInfos[u].title + "------");

                                foreach (GeneratedData.GeneratedEventItem item in segmentInfos[u].genEventItems)
                                {
                                    GenEventShiftItem gesi = new GenEventShiftItem();
                                    gesi.eventName = item.eventName;
                                    gesi.eventValue = item.value;
                                    gesi.eventNumVal = item.numval;

                                    gesi.shiftName = shift.name;
                                    gesi.segment = (SegmentConfiguration)segmentInfos[u].data;

                                    gesi.CaptureItems = item.CaptureItems;

                                    gesi.shiftDate = new ShiftDate(item.timestamp);


                                    if (previousItem != null) gesi.start_timestamp = previousItem.end_timestamp;
                                    else gesi.start_timestamp = item.timestamp.ToLocalTime();
                                    gesi.end_timestamp = item.timestamp.ToLocalTime();

                                    gesi.start = new ShiftTime(gesi.start_timestamp);
                                    gesi.end = new ShiftTime(gesi.end_timestamp);

                                    if (previousItem != null)
                                    {
                                        List<GenEventShiftItem> betweenItems = GetItemsBetween(stc, gesi, previousItem);
                                        Result.AddRange(betweenItems);

                                        // Calculate Duration for previous GenEventShiftItem
                                        GenEventShiftItem last = gesi;

                                        if (betweenItems.Count > 0) last = betweenItems[betweenItems.Count - 1];

                                        TimeSpan duration = last.end_timestamp - last.start_timestamp;
                                        previousItem.duration = duration;
                                    }

                                    previousItem = gesi;

                                    Console.WriteLine(gesi.eventValue + " :: " + gesi.end_timestamp.ToString());

                                    Result.Add(gesi);
                                }
                            }
                        }
                    }
                }
            }

            // $$$ DEBUG $$$ Write Values to Console
            foreach (GenEventShiftItem gesi in Result)
                Console.WriteLine(gesi.eventValue + " :: " + gesi.end_timestamp.ToString() + " :: " + gesi.duration.ToString());

            return Result;

        }

        static SegmentConfiguration GetSegment(ShiftTableConfiguration stc, GeneratedData.GeneratedEventItem item)
        {
            SegmentConfiguration Result = null;

            foreach (ShiftConfiguration shift in stc.shifts)
            {
                foreach (SegmentConfiguration segment in shift.segments)
                {
                    ShiftTime time = new ShiftTime(item.timestamp);

                    if (time >= segment.beginTime && time < segment.endTime)
                    {
                        Result = segment;
                        break;
                    }
                }
                if (Result != null) break;
            }

            return Result;
        }

        static List<GenEventShiftItem> GetItemsBetween(ShiftTableConfiguration stc, GenEventShiftItem gesi_1, GenEventShiftItem gesi_2)
        {
            List<GenEventShiftItem> Result = new List<GenEventShiftItem>();

            // Get ShiftId's for both GenEventShiftItems
            string id1 = GetShiftId(gesi_1.shiftDate, gesi_1.segment.shiftConfiguration.id, gesi_1.segment.id);
            string id2 = GetShiftId(gesi_2.shiftDate, gesi_2.segment.shiftConfiguration.id, gesi_2.segment.id);

            // if not in the same segment during the same shift on the same day
            if (id1 != id2)
            {
                if (gesi_1.shiftDate != gesi_2.shiftDate)
                {
                    int daySpan = gesi_2.shiftDate - gesi_1.shiftDate;

                    for (int x = 0; x <= daySpan; x++)
                    {
                        DateTime dt = new DateTime(gesi_1.shiftDate.year, gesi_1.shiftDate.month, gesi_1.shiftDate.day);
                        ShiftDate date = new ShiftDate(dt.AddDays(x));

                        foreach (ShiftConfiguration shift in stc.shifts)
                        {
                            Result.AddRange(GetItemsDuringShift(gesi_1, date, shift));
                        }
                    }
                }
                else if (gesi_1.segment.shiftConfiguration != gesi_2.segment.shiftConfiguration)
                {
                    Result.AddRange(GetItemsDuringShift(gesi_1, gesi_1.shiftDate, gesi_1.segment.shiftConfiguration));
                    Result.AddRange(GetItemsDuringShift(gesi_1, gesi_1.shiftDate, gesi_2.segment.shiftConfiguration));
                }
                else if (gesi_1.segment != gesi_2.segment)
                {
                    Result.AddRange(GetItemsDuringShift(gesi_1, gesi_1.shiftDate, gesi_1.segment.shiftConfiguration));
                }
            }

            return Result;
        }

        static int GetItemDuringSegmentType(GenEventShiftItem item, SegmentConfiguration segment)
        {
            int type = -1;

            // Timestamp does not fall within segment
            if ((item.start < segment.beginTime && item.end < segment.beginTime) ||
                (item.start > segment.endTime && item.end > segment.endTime))
                type = -1;

            // Timestamps span entire segment
            else if (item.start <= segment.beginTime && item.end >= segment.endTime)
                type = 0;

            // Start timestamp is more than segment.beginTime
            else if (item.start > segment.beginTime && item.end >= segment.endTime)
                type = 1;

            // End timestamp is less than segment.endTime
            else if (item.start <= segment.beginTime && (item.end < segment.endTime && item.end > segment.beginTime))
                type = 2;

            // Both timestamps are within segment
            else if (item.start > segment.beginTime && item.end < segment.endTime)
                type = 3;

            return type;
        }

        static GenEventShiftItem GetItemDuringSegment(GenEventShiftItem item, ShiftDate date, SegmentConfiguration segment)
        {
            GenEventShiftItem Result = null;

            // -1 = Timestamp does not fall within segment          
            // 0 = Timestamps span entire segment
            // 1 = Start timestamp is more than segment.beginTime
            // 2 = End timestamp is less than segment.endTime
            // 3 = Both timestamps are within segment
            int type = GetItemDuringSegmentType(item, segment);

            if (type >= 0)
            {
                GenEventShiftItem gesi = new GenEventShiftItem();
                gesi.eventName = item.eventName;
                gesi.eventValue = item.eventValue;
                gesi.eventNumVal = item.eventNumVal;

                gesi.shiftName = segment.shiftConfiguration.name;
                gesi.segment = segment;

                gesi.CaptureItems = item.CaptureItems;

                gesi.shiftDate = date;

                switch (type)
                {
                    case 0:
                        gesi.start_timestamp = GetDateTimeFromShiftTime(segment.beginTime, date);
                        gesi.end_timestamp = GetDateTimeFromShiftTime(segment.endTime, date);
                        break;

                    case 1:
                        gesi.start_timestamp = item.start_timestamp;
                        gesi.end_timestamp = GetDateTimeFromShiftTime(segment.endTime, date);
                        break;

                    case 2:
                        gesi.start_timestamp = GetDateTimeFromShiftTime(segment.beginTime, date);
                        gesi.end_timestamp = item.end_timestamp;
                        break;

                    case 3:
                        gesi.start_timestamp = item.start_timestamp;
                        gesi.end_timestamp = item.end_timestamp;
                        break;
                }

                TimeSpan duration = gesi.end_timestamp - gesi.start_timestamp;

                Result = gesi;
            }

            return Result;
        }

        static List<GenEventShiftItem> GetItemsDuringShift(GenEventShiftItem item, ShiftDate date, ShiftConfiguration shift)
        {
            List<GenEventShiftItem> Result = new List<GenEventShiftItem>();

            GenEventShiftItem previousItem = item;

            foreach (SegmentConfiguration segment in shift.segments)
            {
                GenEventShiftItem gesi = GetItemDuringSegment(item, date, segment);

                if (gesi != null)
                {
                    TimeSpan duration = gesi.end_timestamp - gesi.start_timestamp;

                    previousItem.duration = duration;

                    previousItem = gesi;

                    Result.Add(gesi);
                }
            }

            return Result;
        }

        static List<ListInfo> GetListByDate(List<GeneratedData.GeneratedEventItem> genEventItems)
        {
            List<ListInfo> Result = new List<ListInfo>();

            IEnumerable<ShiftDate> distinctDates = genEventItems.Select(x => new ShiftDate(x.timestamp)).Distinct();

            // Loop through the distinct ShiftDates  
            foreach (ShiftDate shiftDate in distinctDates.ToList())
            {
                // Get a list of all of the eventitems during this shiftDate
                List<GeneratedData.GeneratedEventItem> sameDates = genEventItems.FindAll(x => new ShiftDate(x.timestamp) == shiftDate);

                // Make sure they are Sorted First to Last
                sameDates.Sort((a, b) => a.timestamp.CompareTo(b.timestamp));

                ListInfo info = new ListInfo();
                info.title = shiftDate.ToString();
                info.genEventItems = sameDates;
                Result.Add(info);
            }

            return Result;
        }

        static List<ListInfo> GetListByShift(ShiftTableConfiguration stc, List<GeneratedData.GeneratedEventItem> genEventItems)
        {
            List<ListInfo> Result = new List<ListInfo>();

            foreach (ShiftConfiguration shift in stc.shifts)
            {
                ListInfo info = new ListInfo();
                info.title = shift.name;
                info.data = shift;
                info.genEventItems = genEventItems.FindAll(x => (new ShiftTime(x.timestamp) >= shift.beginTime && new ShiftTime(x.timestamp) < shift.endTime));
                Result.Add(info);
            }

            return Result;
        }

        static List<ListInfo> GetListByShiftSegment(ShiftConfiguration shift, List<GeneratedData.GeneratedEventItem> genEventItems)
        {
            List<ListInfo> Result = new List<ListInfo>();

            foreach (SegmentConfiguration segment in shift.segments)
            {
                ListInfo info = new ListInfo();
                info.title = segment.id.ToString();
                info.data = segment;
                info.genEventItems = genEventItems.FindAll(x => (new ShiftTime(x.timestamp) >= segment.beginTime && new ShiftTime(x.timestamp) < segment.endTime));
                Result.Add(info);
            }

            return Result;
        }


        #endregion

        // ----------------------------------------------------------------------------------------

        #region "Old"

        List<GenEventShiftItem> GetGenEventShiftItems(List<GeneratedData.GeneratedEventItem> genEventItems)
        {
            List<GenEventShiftItem> Result = new List<GenEventShiftItem>();

            ShiftTableConfiguration stc = GetConfiguration(config);
            if (stc != null)
            {
                // Get a list of all of the distinct (by Event Name) genEventItems
                IEnumerable<string> distinctNames = genEventItems.Select(x => x.eventName).Distinct();

                // Loop through the distinct event names  
                foreach (string distinctName in distinctNames.ToList())
                {
                    // Get a list of all of the eventitems with this eventname
                    List<GeneratedData.GeneratedEventItem> sameNames = genEventItems.FindAll(x => x.eventName.ToLower() == distinctName.ToLower());

                    // Get a list of all of the distinct (by ShiftDate) genEventItems with this EventName
                    IEnumerable<ShiftDate> distinctDates = sameNames.Select(x => new ShiftDate(x.timestamp)).Distinct();

                    GeneratedData.GeneratedEventItem previousItem = previousItems.Find(x => x.eventName.ToLower() == distinctName.ToLower());

                    int daycount = 0;

                    // Loop through the distinct ShiftDates  
                    foreach (ShiftDate shiftDate in distinctDates.ToList())
                    {
                        // Get a list of all of the eventitems during this shiftDate
                        List<GeneratedData.GeneratedEventItem> sameDates = genEventItems.FindAll(x => new ShiftDate(x.timestamp) == shiftDate && x.eventName.ToLower() == distinctName.ToLower());
                        sameDates.Sort((a, b) => a.timestamp.CompareTo(b.timestamp));

                        // Dont allow item from previous day to be the previous item (will count all of the time between ~12+ hours)
                        if (daycount > 0) previousItem = null;

                        // Loop through the ShiftInfo objects
                        foreach (ShiftConfiguration shift in stc.shifts)
                        {
                            // Loop through the Shift Segment objects in the ShiftInfo
                            foreach (SegmentConfiguration segment in shift.segments)
                            {
                                // Get a list of all of the genEventItems with Timestamps within this Segment
                                List<GeneratedData.GeneratedEventItem> sameSegments = sameDates.FindAll(x => (new ShiftTime(x.timestamp) >= segment.beginTime && new ShiftTime(x.timestamp) <= segment.endTime));

                                sameSegments.Sort((a, b) => a.timestamp.CompareTo(b.timestamp));

                                int z = 0;

                                foreach (GeneratedData.GeneratedEventItem sameSegment in sameSegments)
                                {
                                    if (previousItem != null)
                                    {
                                        // If previous timestamp was during previous shift or shift segment split the duration between two GenEventShiftItem objects
                                        ShiftTime prev_ShiftTime = new ShiftTime(previousItem.timestamp);
                                        if (prev_ShiftTime < segment.beginTime)
                                        {
                                            TimeSpan totalduration = sameSegment.timestamp - previousItem.timestamp;

                                            TimeSpan prev_duration = segment.beginTime - prev_ShiftTime;
                                            TimeSpan duration = new ShiftTime(sameSegment.timestamp) - segment.beginTime;

                                            ShiftConfiguration prevShiftConfiguration = stc.shifts.Find(x => x.beginTime <= prev_ShiftTime && x.endTime.AdjustForDayValue() >= prev_ShiftTime);
                                            if (prevShiftConfiguration != null)
                                            {
                                                SegmentConfiguration prevSegment = prevShiftConfiguration.segments.Find(x => x.beginTime <= prev_ShiftTime && x.endTime >= prev_ShiftTime);
                                                if (prevSegment != null)
                                                {
                                                    if (prev_duration > TimeSpan.Zero)
                                                    {
                                                        GenEventShiftItem prev_gesi = new GenEventShiftItem();
                                                        prev_gesi.eventName = previousItem.eventName;
                                                        prev_gesi.eventValue = previousItem.value;
                                                        prev_gesi.eventNumVal = previousItem.numval;

                                                        prev_gesi.shiftName = prevShiftConfiguration.name;
                                                        prev_gesi.segment = prevSegment;

                                                        prev_gesi.CaptureItems = previousItem.CaptureItems;

                                                        DateTime prev_ts = previousItem.timestamp;
                                                        prev_gesi.start_timestamp = prev_ts;
                                                        prev_gesi.end_timestamp = new DateTime(prev_ts.Year, prev_ts.Month, prev_ts.Day, segment.beginTime.hour, segment.beginTime.minute, segment.beginTime.second);

                                                        prev_gesi.shiftDate = new ShiftDate(prev_ts);

                                                        prev_gesi.duration = prev_duration;

                                                        Result.Add(prev_gesi);
                                                    }
                                                }
                                            }

                                            if (duration > TimeSpan.Zero)
                                            {
                                                GenEventShiftItem gesi = new GenEventShiftItem();
                                                gesi.eventName = sameSegment.eventName;
                                                gesi.eventValue = sameSegment.value;
                                                gesi.eventNumVal = sameSegment.numval;

                                                gesi.shiftName = shift.name;
                                                gesi.segment = segment;

                                                gesi.CaptureItems = sameSegment.CaptureItems;

                                                DateTime ts = sameSegment.timestamp;
                                                gesi.start_timestamp = new DateTime(ts.Year, ts.Month, ts.Day, segment.beginTime.hour, segment.beginTime.minute, segment.beginTime.second);
                                                gesi.end_timestamp = sameSegment.timestamp;

                                                gesi.shiftDate = new ShiftDate(sameSegment.timestamp);

                                                gesi.duration = duration;

                                                Result.Add(gesi);
                                            }
                                        }
                                        else
                                        {
                                            TimeSpan duration = sameSegment.timestamp - previousItem.timestamp;

                                            if (duration > TimeSpan.Zero)
                                            {
                                                GenEventShiftItem gesi = new GenEventShiftItem();
                                                gesi.eventName = sameSegment.eventName;
                                                gesi.eventValue = sameSegment.value;
                                                gesi.eventNumVal = sameSegment.numval;

                                                gesi.shiftName = shift.name;
                                                gesi.segment = segment;

                                                gesi.CaptureItems = sameSegment.CaptureItems;

                                                gesi.start_timestamp = previousItem.timestamp;
                                                gesi.end_timestamp = sameSegment.timestamp;

                                                gesi.shiftDate = new ShiftDate(sameSegment.timestamp);

                                                gesi.duration = duration;

                                                Result.Add(gesi);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        previousItems.Add(sameSegment);
                                    }

                                    previousItem = sameSegment;

                                    int prevIndex = -1;

                                    prevIndex = previousItems.FindIndex(x => x.eventName.ToLower() == distinctName.ToLower());
                                    if (prevIndex >= 0) previousItems[prevIndex] = previousItem;

                                    z += 1;

                                }
                            }
                        }

                        daycount += 1;
                    }
                }
            }

            return Result;

        }

        #endregion



        List<ShiftRowInfo> GetShiftRowInfos(List<GenEventShiftItem> genEventShiftItems)
        {
            List<ShiftRowInfo> Result = new List<ShiftRowInfo>();

            ShiftTableConfiguration stc = GetConfiguration(config);

            IEnumerable<ShiftDate> shiftDates = genEventShiftItems.Select(x => x.shiftDate).Distinct();
            foreach (ShiftDate shiftDate in shiftDates)
            {
                List<GenEventShiftItem> sameDates = genEventShiftItems.FindAll(x => x.shiftDate == shiftDate);

                IEnumerable<string> shiftNames = sameDates.Select(x => x.shiftName).Distinct();
                foreach (string shiftName in shiftNames)
                {
                    List<GenEventShiftItem> sameShifts = sameDates.FindAll(x => x.shiftName == shiftName);

                    IEnumerable<SegmentConfiguration> segments = sameShifts.Select(x => x.segment).Distinct();
                    foreach (SegmentConfiguration segment in segments)
                    {
                        ShiftRowInfo sri = new ShiftRowInfo();
                        sri.id = GetShiftId(shiftDate, segment.shiftConfiguration.id, segment.id);
                        sri.date = shiftDate;
                        sri.shift = shiftName;
                        sri.segmentId = segment.id;
                        sri.start = segment.beginTime;
                        sri.end = segment.endTime;
                        sri.type = segment.type;

                        sri.totalTime = GetTotalShiftSeconds(sri, currentData);

                        if (stc != null)
                        {
                            IEnumerable<string> eventNames = genEventShiftItems.Select(x => x.eventName).Distinct();
                            foreach (string eventName in eventNames.ToList())
                            {
                                if (stc.generatedEvents.Find(x => x.name.ToLower() == eventName.ToLower()) != null)
                                {
                                    List<GenEventShiftItem> sameNames = genEventShiftItems.FindAll(x => (x.shiftDate == shiftDate &&
                                            x.shiftName == shiftName &&
                                            x.segment == segment &&
                                            x.eventName == eventName));

                                    IEnumerable<string> eventValues = sameNames.Select(x => x.eventValue).Distinct();
                                    foreach (string eventValue in eventValues.ToList())
                                    {
                                        TimeSpan duration = TimeSpan.Zero;

                                        // Get list of GenEventShiftItem objects that satisfy all of the filters
                                        List<GenEventShiftItem> items = sameNames.FindAll(x => x.eventValue == eventValue);
                                        if (items != null)
                                        {
                                            int eventNumVal = 0;

                                            foreach (GenEventShiftItem item in items)
                                            {
                                                duration += item.duration;
                                                eventNumVal = item.eventNumVal;
                                            }

                                            GenEventRowInfo geri = new GenEventRowInfo();
                                            geri.columnName = FormatColumnName(eventName, eventNumVal, eventValue);
                                            geri.seconds = Convert.ToInt32(duration.TotalSeconds);

                                            sri.genEventRowInfos.Add(geri);
                                        }
                                    }
                                }
                            }
                        }

                        Result.Add(sri);

                    }
                }
            }

            return Result;

        }

        // Returns list of CHANGED ShiftRowInfo objects and updates rowInfos
        List<ShiftRowInfo> UpdateRowInfos(List<ShiftRowInfo> newInfos)
        {
            List<ShiftRowInfo> Result = new List<ShiftRowInfo>();

            foreach (ShiftRowInfo newInfo in newInfos)
            {
                ShiftRowInfo info = rowInfos.Find(x => x.date == newInfo.date && x.shift == newInfo.shift && x.segmentId == newInfo.segmentId);
                if (info == null)
                {
                    info = new ShiftRowInfo();
                    info.id = newInfo.id;
                    info.date = newInfo.date;
                    info.shift = newInfo.shift;
                    info.segmentId = newInfo.segmentId;
                    info.start = newInfo.start;
                    info.end = newInfo.end;
                    info.type = newInfo.type;

                    foreach (string column in GenEventColumns)
                    {
                        GenEventRowInfo emptyGeri = new GenEventRowInfo();
                        emptyGeri.columnName = column;
                        info.genEventRowInfos.Add(emptyGeri);
                    }
                }

                info.totalTime = newInfo.totalTime;

                // Update GenEventRowInfos
                foreach (GenEventRowInfo geri in newInfo.genEventRowInfos)
                {
                    GenEventRowInfo originalGeri = info.genEventRowInfos.Find(x => x.columnName == geri.columnName);
                    if (originalGeri != null)
                    {
                        originalGeri.seconds += geri.seconds;
                    }
                    else
                    {
                        GenEventRowInfo newGeri = new GenEventRowInfo();
                        newGeri.columnName = geri.columnName;
                        newGeri.seconds += geri.seconds;
                        info.genEventRowInfos.Add(newGeri);
                    }
                }

                Result.Add(info);
                rowInfos.Add(info);

            }

            return Result;

        }


        class CurrentShiftInfo
        {
            public string id { get; set; }
            public string name { get; set; }
            public string date { get; set; }

            public ShiftTime currentTime { get; set; }

            public ShiftConfiguration shift { get; set; }

            public SegmentConfiguration segment { get; set; }
        }

        CurrentShiftInfo GetCurrentShift(TH_MTC_Data.Streams.ReturnData rd)
        {
            CurrentShiftInfo Result = null;

            ShiftTableConfiguration stc = GetConfiguration(config);
            if (stc != null)
            {
                ShiftTime currentTime = new ShiftTime(rd.header.creationTime);

                foreach (ShiftConfiguration shift in stc.shifts)
                {
                    if (currentTime >= shift.beginTime && currentTime <= shift.endTime)
                    {
                        Result = new CurrentShiftInfo();

                        Result.currentTime = currentTime;

                        Result.name = shift.name;
                        Result.shift = shift;
                        Result.date = new ShiftDate(rd.header.creationTime).ToString();

                        foreach (SegmentConfiguration segment in shift.segments)
                        {
                            if (Result.currentTime >= segment.beginTime && Result.currentTime <= segment.endTime)
                            {
                                Result.segment = segment;
                                Result.id = GetShiftId(new ShiftDate(rd.header.creationTime), shift.id, segment.id);
                                break;
                            }
                        }

                        break;
                    }
                }
            }
            return Result;
        }

        //CurrentShiftInfo GetCurrentShift(TH_MTC_Data.Streams.ReturnData rd)
        //{
        //    CurrentShiftInfo Result = null;

        //    ShiftTableConfiguration stc = GetConfiguration(config);
        //    if (stc != null)
        //    {
        //        ShiftTime currentTime = new ShiftTime(rd.header.creationTime);

        //        foreach (ShiftConfiguration shift in stc.shifts)
        //        {
        //            if (currentTime >= shift.beginTime && currentTime <= shift.endTime)
        //            {
        //                Result = new CurrentShiftInfo();

        //                Result.currentTime = currentTime;

        //                Result.name = shift.name;
        //                Result.shift = shift;
        //                Result.date = new ShiftDate(rd.header.creationTime).ToString();
                        
        //                foreach (SegmentConfiguration segment in shift.segments)
        //                {
        //                    if (Result.currentTime >= segment.beginTime && Result.currentTime <= segment.endTime)
        //                    {
        //                        Result.segment = segment;
        //                        Result.id = GetShiftId(new ShiftDate(rd.header.creationTime), shift.id, segment.id);
        //                        break;
        //                    }
        //                }

        //                break;
        //            }
        //        }
        //    }
        //    return Result;
        //}

        #endregion

        #region "Tools"

        static bool Debug = true;

        static void Log(string line)
        {
            if (Debug) TH_Global.Logger.Log(line);
        }

        /// <summary>
        /// Get Total Time using ShiftRowInfo.End and currentData from last MTC Current Request
        /// </summary>
        /// <param name="info"></param>
        /// <param name="currentData"></param>
        /// <returns></returns>
        static int GetTotalShiftSeconds(ShiftRowInfo info, TH_MTC_Data.Streams.ReturnData currentData)
        {
            int Result = 0;

            ShiftTime currentTime = new ShiftTime(currentData.header.creationTime);
            ShiftDate currentDate = new ShiftDate(currentData.header.creationTime);
            if (currentDate > info.date)
            {
                if (info.end < info.start) info.end.hour += 24;
                Result = Convert.ToInt32((info.end - info.start).TotalSeconds);
            }
            else if (currentTime > info.end)
            {
                if (info.end < info.start) info.end.hour += 24;
                Result = Convert.ToInt32((info.end - info.start).TotalSeconds);
            }
            else
            {
                ShiftTime headerTime = new ShiftTime(currentData.header.creationTime);

                Result = Convert.ToInt32((headerTime - info.start).TotalSeconds);
            }

            return Result;
        }

        public static string GetShiftId(ShiftDate date, int id, int segmentId)
        {
            return date.year.ToString("0000") + date.month.ToString("00") + date.day.ToString("00") + "_" + id.ToString("00") + "_" + segmentId.ToString("00");
        }

        static DateTime GetDateTimeFromShiftTime(ShiftTime time, ShiftDate date)
        {
            return getDateTimeFromShift(time, date, true);
        }

        static DateTime GetDateTimeFromShiftTime(ShiftTime time, ShiftDate date, bool addDayValue)
        {
            return getDateTimeFromShift(time, date, addDayValue);
        }

        static DateTime GetDateTimeFromShiftTime(ShiftTime time, ShiftDate date, int dayOffset)
        {
            //if (dayOffset > 0)
            //{
            //    DateTime dt = new DateTime(date.year, date.month, date.day).AddDays(dayOffset);
            //    date = new ShiftDate(dt, false);
            //}

            return new DateTime(date.year, date.month, date.day, time.hour, time.minute, time.second).AddDays(dayOffset);
        }

        static DateTime getDateTimeFromShift(ShiftTime time, ShiftDate date, bool addDayValue)
        {
            if (time.dayValue > 0 && addDayValue)
            {
                DateTime dt = new DateTime(date.year, date.month, date.day).AddDays(time.dayValue);
                date = new ShiftDate(dt, false);
            }

            return new DateTime(date.year, date.month, date.day, time.hour, time.minute, time.second);
        }





        static string FormatColumnName(string eventName, int resultNumValue, string resultValue)
        {
            return eventName + "__" + resultValue.Replace(' ', '_');
        }

        static string FormatColumnName(GeneratedData.GeneratedEvents.Event genEvent, GeneratedData.GeneratedEvents.Value value)
        {
            return genEvent.Name + "__" + value.Result.Value.Replace(' ', '_');
        }

        #endregion

        #endregion

    }
}
