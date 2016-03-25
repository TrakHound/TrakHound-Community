// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Data;

using TH_Configuration;
using TH_Database;
using TH_Global;
using TH_Plugins.Database;

namespace TH_Device_Server.TableManagement
{
    public static class AgentInfo
    {

        public const string TableName = TableNames.AgentInfo;
        static string[] primaryKey = { "Agent_Instance_ID" };

        public static void Initialize(Configuration lSettings)
        {

            ColumnDefinition[] Columns = new ColumnDefinition[]
            {
                new ColumnDefinition("Agent_Instance_ID", DataType.Long),
                new ColumnDefinition("First_TimeStamp", DataType.DateTime),
                new ColumnDefinition("First_TimeStamp", DataType.DateTime),
                new ColumnDefinition("Seconds", DataType.Long)
            };

            Table.Create(lSettings.Databases_Server, TableName, Columns, primaryKey);

            //Table.Create(lSettings.Databases, TableName,
            //    new object[] 
            //        {
            //        "Agent_Instance_ID " + DataType.Long,
            //        "First_TimeStamp " + DataType.DateTime,
            //        "Last_TimeStamp " + DataType.DateTime,
            //        "Seconds " + DataType.Long
            //        },
            //    "Agent_Instance_ID"
            //    );

        }

        public static void UpdateAgentInfoTable(Configuration lSettings, Int64 AgentInstanceID, DateTime FirstTimeStamp, DateTime LastTimeStamp)
        {

            List<Tuple<string, object>> Changed = new List<Tuple<string, object>>();

            DataRow PreviousRow = Row.Get(lSettings.Databases_Server, TableName, "Agent_Instance_ID", AgentInstanceID.ToString());

            DateTime PreviousFirst = DateTime.MinValue;
            DateTime PreviousLast = DateTime.MinValue;
            TimeSpan TS = TimeSpan.Zero;

            if (PreviousRow != null)
            {
                if (PreviousRow["First_TimeStamp"] != null)
                {
                    PreviousFirst = DateTime.Parse(PreviousRow["First_Timestamp"].ToString());
                    if (FirstTimeStamp < PreviousFirst) Changed.Add(new Tuple<string, object>("First_Timestamp", FirstTimeStamp.ToString()));
                }

                if (PreviousRow["Last_TimeStamp"] != null)
                {
                    DateTime.TryParse(PreviousRow["Last_Timestamp"].ToString(), out PreviousLast);
                    if (LastTimeStamp > PreviousLast) Changed.Add(new Tuple<string, object>("Last_Timestamp", LastTimeStamp.ToString()));
                }
                if (PreviousFirst > DateTime.MinValue && PreviousLast > DateTime.MinValue)
                {
                    TS = PreviousLast - PreviousFirst;
                }
            }
            else
            {
                Changed.Add(new Tuple<string, object>("First_Timestamp", FirstTimeStamp.ToString()));
                Changed.Add(new Tuple<string, object>("Last_Timestamp", LastTimeStamp.ToString()));
                TS = LastTimeStamp - FirstTimeStamp;
            }

            if (TS > TimeSpan.Zero)
                Changed.Add(new Tuple<string, object>("Seconds", TS.TotalSeconds));


            List<object> Cols = new List<object>();
            List<object> Vals = new List<object>();

            Cols.Add("Agent_Instance_ID");
            Vals.Add(AgentInstanceID);

            foreach (Tuple<string, object> ChangedItem in Changed)
            {
                Cols.Add(ChangedItem.Item1);
                Vals.Add(ChangedItem.Item2);
            }

            object[] Columns = Cols.ToArray();
            object[] Values = Vals.ToArray();

            if (Columns.Length > 1 && Values.Length > 1)
                Row.Insert(lSettings.Databases_Server, TableName, Columns, Values, primaryKey, true);

        }

    }
}
