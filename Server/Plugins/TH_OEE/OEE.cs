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

namespace TH_OEE
{
    public class OEE : Table_PlugIn
    {

        #region "PlugIn"

        public string Name { get { return "TH_OEE"; } }

        public int Priority { get { return 4; } }

        public void Initialize(Configuration configuration)
        {
            OEEConfiguration oc = ReadXML(configuration.ConfigurationXML);
            if (oc != null) configuration.CustomClasses.Add(oc);
            config = configuration;

            CreateTable();
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
                if (de_data.id.ToLower() == "shifttable_shiftrowinfos")
                {
                    if (de_data.data.GetType() == typeof(List<ShiftRowInfo>))
                    {
                        List<ShiftRowInfo> infos = (List<ShiftRowInfo>)de_data.data;

                        List<OEEInfo> oeeInfos = ProcessOEE(infos);

                        List<OeeRowInfo> rowInfos = GetRowInfos(oeeInfos);

                        UpdateRows(rowInfos);
                    }
                }
            }
        }

        public event DataEvent_Handler DataEvent;


        public void Closing()
        {

        }

        public Type Config_Page { get { return null; } }

        #endregion

        #region "Properties"

        Configuration config { get; set; }

        #endregion

        #region "OEE"

        #region "Configuration"

        public class OEEConfiguration
        {
            public OEEConfiguration()
            {
                availability = new AvailabilityConfiguration();
                performance = new PeformanceConfiguration();
                quality = new QualityConfiguration();
            }

            public AvailabilityConfiguration availability;
            public PeformanceConfiguration performance;
            public QualityConfiguration quality;
        }

        public class AvailabilityConfiguration
        {
            public string Event { get; set; }
            public string Value { get; set; }
        }

        public class PeformanceConfiguration
        {
            public bool Enabled { get; set; }
        }

        public class QualityConfiguration
        {
            public bool Enabled { get; set; }
        }

        OEEConfiguration ReadXML(XmlDocument configXML)
        {
            OEEConfiguration Result = new OEEConfiguration();

            XmlNodeList nodes = configXML.SelectNodes("/Settings/OEE");

            if (nodes != null)
            {
                if (nodes.Count > 0)
                {
                    XmlNode node = nodes[0];

                    foreach (XmlNode Child in node.ChildNodes)
                    {
                        if (Child.NodeType == XmlNodeType.Element)
                        {
                            Type Setting = typeof(OEEConfiguration);
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
                                    case "availability": Result.availability = ProcessAvailability(Child); break;
                                    case "performance": Result.performance = ProcessPerformance(Child); break;
                                    case "quality": Result.quality = ProcessQuality(Child); break;
                                }
                            }
                        }
                    }
                }
            }

            return Result;
        }

        AvailabilityConfiguration ProcessAvailability(XmlNode node)
        {
            AvailabilityConfiguration Result = new AvailabilityConfiguration();

            foreach (XmlNode Child in node.ChildNodes)
            {
                if (Child.NodeType == XmlNodeType.Element)
                {
                    Type Setting = typeof(AvailabilityConfiguration);
                    PropertyInfo info = Setting.GetProperty(Child.Name);

                    if (info != null)
                    {
                        Type t = info.PropertyType;
                        info.SetValue(Result, Convert.ChangeType(Child.InnerText, t), null);
                    }
                }
            }

            return Result;
        }

        PeformanceConfiguration ProcessPerformance(XmlNode node)
        {
            PeformanceConfiguration Result = new PeformanceConfiguration();

            foreach (XmlNode Child in node.ChildNodes)
            {
                if (Child.NodeType == XmlNodeType.Element)
                {
                    Type Setting = typeof(PeformanceConfiguration);
                    PropertyInfo info = Setting.GetProperty(Child.Name);

                    if (info != null)
                    {
                        Type t = info.PropertyType;
                        info.SetValue(Result, Convert.ChangeType(Child.InnerText, t), null);
                    }
                }
            }

            return Result;
        }

        QualityConfiguration ProcessQuality(XmlNode node)
        {
            QualityConfiguration Result = new QualityConfiguration();

            foreach (XmlNode Child in node.ChildNodes)
            {
                if (Child.NodeType == XmlNodeType.Element)
                {
                    Type Setting = typeof(QualityConfiguration);
                    PropertyInfo info = Setting.GetProperty(Child.Name);

                    if (info != null)
                    {
                        Type t = info.PropertyType;
                        info.SetValue(Result, Convert.ChangeType(Child.InnerText, t), null);
                    }
                }
            }

            return Result;
        }

        public static OEEConfiguration GetConfiguration(Configuration configuration)
        {
            OEEConfiguration Result = null;

            var customClass = configuration.CustomClasses.Find(x => x.GetType() == typeof(OEEConfiguration));
            if (customClass != null) Result = (OEEConfiguration)customClass;

            return Result;
        }

        #endregion

        #region "MySQL"

        void CreateTable()
        {

            List<ColumnDefinition> columns = new List<ColumnDefinition>();

            columns.Add(new ColumnDefinition("SHIFT_ID", DataType.LargeText, true));

            columns.Add(new ColumnDefinition("OEE", DataType.Double));

            columns.Add(new ColumnDefinition("AVAILABILITY", DataType.Double));
            columns.Add(new ColumnDefinition("PERFORMANCE", DataType.Double));
            columns.Add(new ColumnDefinition("QUALITY", DataType.Double));

            columns.Add(new ColumnDefinition("OPERATING_TIME", DataType.Long));
            columns.Add(new ColumnDefinition("PLANNED_PRODUCTION_TIME", DataType.Long));


            columns.Add(new ColumnDefinition("IDEAL_CYCLE_TIME", DataType.Long));
            columns.Add(new ColumnDefinition("TOTAL_PIECES", DataType.Long));

            columns.Add(new ColumnDefinition("GOOD_PIECES", DataType.Long));


            ColumnDefinition[] ColArray = columns.ToArray();

            Table.Create(config.Databases, TableNames.OEE, ColArray, "SHIFT_ID");  


            //List<string> columns = new List<string>();
            //columns.Add("Shift_Id " + MySQL_Tools.VarChar + " UNIQUE NOT NULL");

            //columns.Add("OEE " + MySQL_Tools.Double);

            //columns.Add("Availability " + MySQL_Tools.Double);
            //columns.Add("Performance " + MySQL_Tools.Double);
            //columns.Add("Quality " + MySQL_Tools.Double);
            
            //columns.Add("Operating_Time " + MySQL_Tools.BigInt);
            //columns.Add("Planned_Production_Time " + MySQL_Tools.BigInt);

            //columns.Add("Ideal_Cycle_Time " + MySQL_Tools.BigInt);
            //columns.Add("Total_Pieces " + MySQL_Tools.BigInt);

            //columns.Add("Good_Pieces " + MySQL_Tools.BigInt);

            //Global.Table_Create(config.SQL, TableNames.OEE, columns.ToArray(), "Shift_Id");
        }

        void UpdateRows(List<OeeRowInfo> infos)
        {
            List<List<object>> rowValues = new List<List<object>>();

            List<string> columns = new List<string>();
            columns.Add("shift_id");
            columns.Add("oee");

            columns.Add("availability");
            columns.Add("performance");
            columns.Add("quality");

            columns.Add("operating_time");
            columns.Add("planned_production_time");

            columns.Add("ideal_cycle_time");
            columns.Add("total_pieces");

            columns.Add("good_pieces");          

            foreach (OeeRowInfo info in infos)
            {
                List<object> values = new List<object>();

                values.Add(info.shift_id);

                values.Add(info.oee);

                values.Add(info.availability);
                values.Add(info.performance);
                values.Add(info.quality);

                values.Add(info.operating_time);
                values.Add(info.planned_production_time);

                values.Add(info.ideal_cycle_time);
                values.Add(info.total_pieces);

                values.Add(info.good_pieces);

                rowValues.Add(values);
            }


            Row.Insert(config.Databases, TableNames.OEE, columns.ToArray(), rowValues, true);


            //Global.Row_Insert(config.SQL, TableNames.OEE, columns.ToArray(), rowValues);
        }

        #endregion

        #region "Processing"

        List<OEEInfo> ProcessOEE(List<ShiftRowInfo> infos)
        {
            List<OEEInfo> Result = new List<OEEInfo>();

            OEEConfiguration oc = GetConfiguration(config);
            if (oc != null)
            {
                IEnumerable<ShiftDate> dates = infos.Select(x => x.date).Distinct();

                foreach (ShiftDate date in dates.ToList())
                {
                    List<CycleInfo> cyclesInfos = GetCyclesInfos(date);

                    List<ShiftRowInfo> sameDates = infos.FindAll(x => x.date == date);

                    foreach (ShiftRowInfo info in sameDates)
                    {
                        if (info.type.ToLower() == "work")
                        {
                            OEEInfo oee = new OEEInfo();

                            oee.shift_id = info.id;

                            // Get Availability Info
                            string columnName = oc.availability.Event.ToLower() + "__" + oc.availability.Value.Replace(' ', '_').ToLower();
                            GenEventRowInfo geri = info.genEventRowInfos.Find(x => x.columnName.ToLower() == columnName);
                            if (geri != null)
                            {
                                AvailabilityInfo availability = new AvailabilityInfo();
                                availability.operating_time = geri.seconds;
                                availability.planned_production_time = info.totalTime;
                                oee.availability = availability;
                            }

                            // Get Performance Info
                            if (oc.performance.Enabled)
                            {

                            }

                            // Get Quality Info
                            if (oc.quality.Enabled)
                            {

                            }

                            Result.Add(oee);
                        }
                    }
                }
            }

            return Result;
        }

        List<OeeRowInfo> GetRowInfos(List<OEEInfo> infos)
        {
            List<OeeRowInfo> Result = new List<OeeRowInfo>();

            OEEConfiguration oc = GetConfiguration(config);
            if (oc != null)
            {
                foreach (OEEInfo info in infos)
                {
                    OeeRowInfo rowInfo = new OeeRowInfo();

                    rowInfo.shift_id = info.shift_id;

                    rowInfo.oee = CalculateOEE(info);

                    rowInfo.availability = CalculateAvailability(info.availability);

                    if (oc.performance.Enabled) rowInfo.performance = CalculatePerformance(info.performance);
                    else rowInfo.performance = 1;

                    if (oc.quality.Enabled) rowInfo.quality = CalculateQuality(info.quality);
                    else rowInfo.quality = 1;

                    rowInfo.operating_time = info.availability.operating_time;
                    rowInfo.planned_production_time = info.availability.planned_production_time;

                    rowInfo.ideal_cycle_time = info.performance.ideal_cycle_time;
                    rowInfo.total_pieces = info.performance.total_pieces;

                    rowInfo.good_pieces = info.quality.good_pieces;

                    Result.Add(rowInfo);
                }
            }

            return Result;
        }

        #region "Row Info"

        public class OeeRowInfo
        {
            public string shift_id { get; set; }

            public double oee { get; set; }

            public double availability { get; set; }
            public double performance { get; set; }
            public double quality { get; set; }

            public int operating_time { get; set; }
            public int planned_production_time { get; set; }

            public int ideal_cycle_time { get; set; }
            public int total_pieces { get; set; }

            public int good_pieces { get; set; }
        }

        #endregion

        #region "Infos"

        class CycleInfo
        {
            public string shift_id { get; set; }
            public int ideal_cycle_time { get; set; }
        }

        List<CycleInfo> GetCyclesInfos(ShiftDate date)
        {
            List<CycleInfo> Result = new List<CycleInfo>();

            DataTable dt = Table.Get(config.Databases, TableNames.Cycles, "WHERE Date='" + date.ToString() + "'");

            if (dt != null)
            {
                foreach (DataRow row in dt.Rows)
                {
                    if (dt.Columns.Contains("Shift_Id"))
                    {
                        CycleInfo info = new CycleInfo();
                        info.shift_id = row["Shift_Id"].ToString();

                        if (dt.Columns.Contains("Ideal_Cycle_Time"))
                        {
                            int i = -1;
                            int.TryParse(row["Ideal_Cycle_Time"].ToString(), out i);
                            if (i >= 0) info.ideal_cycle_time = i;
                        }

                        Result.Add(info);
                    }
                }
            }

            return Result;
        }

        #region "Availability"

        class AvailabilityInfo
        {
            public int operating_time { get; set; }
            public int planned_production_time { get; set; }
        }

        double CalculateAvailability(AvailabilityInfo info)
        {
            return (double)info.operating_time / (double)info.planned_production_time;
        }

        #endregion

        #region "Performance"

        class PerformanceInfo
        {
            public int ideal_cycle_time { get; set; }
            public int operating_time { get; set; }
            public int total_pieces { get; set; }
        }

        double CalculatePerformance(PerformanceInfo info)
        {
            return (double)info.ideal_cycle_time / ((double)info.operating_time / (double)info.total_pieces);
        }

        #endregion

        #region "Quality"

        class QualityInfo
        {
            public int good_pieces { get; set; }
            public int total_pieces { get; set; }
        }

        double CalculateQuality(QualityInfo info)
        {
            return (double)info.good_pieces / (double)info.total_pieces;
        }

        #endregion

        #region "OEE"

        class OEEInfo
        {
            public OEEInfo()
            {
                availability = new AvailabilityInfo();
                performance = new PerformanceInfo();
                quality = new QualityInfo();
            }

            public string shift_id { get; set; }

            public AvailabilityInfo availability { get; set; }
            public PerformanceInfo performance { get; set; }
            public QualityInfo quality { get; set; }
        }

        double CalculateOEE(OEEInfo info)
        {
            double Result = 0;

            if (info.availability != null)
            {
                double availability = CalculateAvailability(info.availability);
                double performance = 1;
                double quality = 1;

                OEEConfiguration oc = GetConfiguration(config);
                if (oc != null)
                {
                    if (oc.performance.Enabled) performance = CalculatePerformance(info.performance);
                    if (oc.quality.Enabled) quality = CalculateQuality(info.quality);
                }

                Result = availability * performance * quality;
            }

            return Result;
        }

        #endregion

        #endregion

        #endregion

        #endregion

        TH_MTC_Data.Streams.ReturnData currentData;

    }
}
