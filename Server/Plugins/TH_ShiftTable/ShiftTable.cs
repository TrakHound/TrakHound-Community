// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

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
            GenEventShiftItem.previousItems = new List<GeneratedData.GeneratedEventItem>();

            ShiftConfiguration sc = ShiftConfiguration.ReadXML(configuration.ConfigurationXML);

            if (sc != null)
            {
                configuration.CustomClasses.Add(sc);
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
            CurrentShiftInfo shiftInfo = CurrentShiftInfo.Get(config, returnData);
            if (shiftInfo != null)
            {
                TH_MySQL.Tables.Variables.Update(config.SQL, "shift_name", shiftInfo.name, returnData.header.creationTime);

                TH_MySQL.Tables.Variables.Update(config.SQL, "shift_id", shiftInfo.id, returnData.header.creationTime);

                TH_MySQL.Tables.Variables.Update(config.SQL, "shift_date", shiftInfo.date, returnData.header.creationTime);

                TH_MySQL.Tables.Variables.Update(config.SQL, "shift_begintime", shiftInfo.shiftStart.ToString(), returnData.header.creationTime);
                TH_MySQL.Tables.Variables.Update(config.SQL, "shift_endtime", shiftInfo.shiftEnd.ToString(), returnData.header.creationTime);

                TH_MySQL.Tables.Variables.Update(config.SQL, "shift_currenttime", shiftInfo.currentTime.ToString(), returnData.header.creationTime);
            }
            else
            {
                TH_MySQL.Tables.Variables.Update(config.SQL, "shift_name", "", returnData.header.creationTime);

                TH_MySQL.Tables.Variables.Update(config.SQL, "shift_id", "", returnData.header.creationTime);

                TH_MySQL.Tables.Variables.Update(config.SQL, "shift_date", "", returnData.header.creationTime);

                TH_MySQL.Tables.Variables.Update(config.SQL, "shift_begintime", "", returnData.header.creationTime);
                TH_MySQL.Tables.Variables.Update(config.SQL, "shift_endtime", "", returnData.header.creationTime);

                TH_MySQL.Tables.Variables.Update(config.SQL, "shift_currenttime", "", returnData.header.creationTime);
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
            ShiftConfiguration sc = ShiftConfiguration.Get(config);
            if (sc != null)
            {
                GeneratedData.GenDataConfiguration gdc = GeneratedData.GetConfiguration(config);
                if (gdc != null)
                {
                    GenEventColumns = new List<string>();

                    foreach (GeneratedData.GeneratedEvents.Event genEvent in gdc.generatedEvents.events)
                    {
                        if ((sc.generatedEvents.Find(x => x.name.ToUpper() == genEvent.Name.ToUpper()) != null))
                        {
                            string columnName;

                            foreach (GeneratedData.GeneratedEvents.Value value in genEvent.Values)
                            {
                                columnName = Tools.FormatColumnName(genEvent, value);

                                if (!ShiftTableColumns.Contains(columnName))
                                {
                                    string columnDefinition = columnName + " " + MySQL_Tools.BigInt;
                                    Global.Table_AddColumn(config.SQL, TableName, columnDefinition);
                                }

                                GenEventColumns.Add(columnName);
                            }

                            // Add GenEvent.Default column
                            columnName = Tools.FormatColumnName(genEvent.Name, genEvent.Default.NumVal, genEvent.Default.Value);

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

        void ProcessShifts(List<GeneratedData.GeneratedEventItem> genEventItems)
        {
            List<GenEventShiftItem> genEventShiftItems = GenEventShiftItem.Get(config, genEventItems);

            // Send List of GenEventShiftItems to other Plugins
            SendGenEventShiftItems(genEventShiftItems);

            List<ShiftRowInfo> shiftRowInfos = ShiftRowInfo.Get(config, genEventShiftItems, currentData);

            List<ShiftRowInfo> changedInfos = UpdateRowInfos(shiftRowInfos);

            UpdateShiftRows(changedInfos);

            // Send List of Shift Info to other Plugins
            SendShiftRowInfos(changedInfos);
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

        // Used to hold previous rowinfos
        List<ShiftRowInfo> rowInfos;

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

        #endregion

        #endregion

    }

    class SegmentShiftTimes
    {
        public DateTime start { get; set; }
        public DateTime end { get; set; }
        public DateTime segmentStart { get; set; }
        public DateTime segmentEnd { get; set; }

        public static SegmentShiftTimes Get(DateTime d1, DateTime d2, ShiftDate date, Segment segment)
        {
            // Convert times to Local
            DateTime start = d1.ToLocalTime();
            DateTime end = d2.ToLocalTime();

            // Get DateTime objects from ShiftTime and ShiftDate along with segment.dayOffset
            DateTime segmentStart = Tools.GetDateTimeFromShiftTime(segment.beginTime, date, segment.beginDayOffset);
            DateTime segmentEnd = Tools.GetDateTimeFromShiftTime(segment.endTime, date, segment.endDayOffset);

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

            SegmentShiftTimes Result = new SegmentShiftTimes();
            Result.start = start;
            Result.end = end;
            Result.segmentStart = segmentStart;
            Result.segmentEnd = segmentEnd;

            return Result;
        }
    }
}
