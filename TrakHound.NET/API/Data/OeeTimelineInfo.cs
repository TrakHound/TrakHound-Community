// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace TrakHound.API
{
    public static partial class Data
    {
        public class OeeTimelineInfo
        {
            public OeeTimelineInfo()
            {
                Hours = new List<HourInfo>();
            }

            public class HourInfo
            {
                [JsonProperty("hour")]
                /// <summary>
                /// Hour of the day (0 - 23) 
                /// </summary>
                public int Hour { get; set; }

                //[JsonProperty("oee")]
                //public double Oee { get; set; }

                //[JsonProperty("availability")]
                //public double Availability { get; set; }

                //[JsonProperty("performance")]
                //public double Performance { get; set; }

                //[JsonProperty("quality")]
                //public double Quality { get; set; }

                [JsonProperty("planned_production_time")]
                public double PlannedProductionTime { get; set; }

                [JsonProperty("operating_time")]
                public double OperatingTime { get; set; }

                [JsonProperty("ideal_operating_time")]
                public double IdealOperatingTime { get; set; }

                [JsonProperty("ideal_cycle_time")]
                public double IdealCycleTime { get; set; }

                [JsonProperty("ideal_run_rate")]
                public double IdealRunRate { get; set; }

                [JsonProperty("total_pieces")]
                public int TotalPieces { get; set; }

                [JsonProperty("good_pieces")]
                public int GoodPieces { get; set; }

            }

            [JsonProperty("date")]
            public string Date { get; set; }

            [JsonProperty("hours")]
            public List<HourInfo> Hours { get; set; }

            public void CombineHours()
            {
                var newHours = new List<HourInfo>();

                var distinctHours = Hours.Select(o => o.Hour).Distinct();
                foreach (var distinctHour in distinctHours)
                {
                    var hourInfo = new HourInfo();
                    hourInfo.Hour = distinctHour;

                    var sameHours = Hours.FindAll(o => o.Hour == distinctHour);
                    foreach (var sameHour in sameHours)
                    {
                        hourInfo.PlannedProductionTime += sameHour.PlannedProductionTime;
                        hourInfo.OperatingTime += sameHour.OperatingTime;
                        hourInfo.IdealOperatingTime += sameHour.IdealOperatingTime;
                        hourInfo.TotalPieces += sameHour.TotalPieces;
                        hourInfo.GoodPieces += sameHour.GoodPieces;
                    }

                    newHours.Add(hourInfo);
                }

                Hours = newHours;
            }

            public double Oee
            {
                get
                {
                    if (Hours.Count > 0)
                    {
                        var oee = new OEE();
                        oee.ConstantQuality = 1;

                        foreach (var hour in Hours)
                        {
                            oee.PlannedProductionTime += hour.PlannedProductionTime;
                            oee.OperatingTime += hour.OperatingTime;
                            oee.IdealOperatingTime += hour.IdealOperatingTime;
                            oee.TotalPieces += hour.TotalPieces;
                            oee.GoodPieces += hour.GoodPieces;
                        }

                        return oee.Oee;
                    }

                    return 0;
                }
            }


            //[JsonProperty("hour_1")]
            //public double Hour1 { get; set; }

            //[JsonProperty("hour_2")]
            //public double Hour2 { get; set; }

            //[JsonProperty("hour_3")]
            //public double Hour3 { get; set; }

            //[JsonProperty("hour_4")]
            //public double Hour4 { get; set; }

            //[JsonProperty("hour_5")]
            //public double Hour5 { get; set; }

            //[JsonProperty("hour_6")]
            //public double Hour6 { get; set; }

            //[JsonProperty("hour_7")]
            //public double Hour7 { get; set; }

            //[JsonProperty("hour_8")]
            //public double Hour8 { get; set; }

            //[JsonProperty("hour_9")]
            //public double Hour9 { get; set; }

            //[JsonProperty("hour_10")]
            //public double Hour10 { get; set; }

            //[JsonProperty("hour_11")]
            //public double Hour11 { get; set; }

            //[JsonProperty("hour_12")]
            //public double Hour12 { get; set; }

            //[JsonProperty("hour_13")]
            //public double Hour13 { get; set; }

            //[JsonProperty("hour_14")]
            //public double Hour14 { get; set; }

            //[JsonProperty("hour_15")]
            //public double Hour15 { get; set; }

            //[JsonProperty("hour_16")]
            //public double Hour16 { get; set; }

            //[JsonProperty("hour_17")]
            //public double Hour17 { get; set; }

            //[JsonProperty("hour_18")]
            //public double Hour18 { get; set; }

            //[JsonProperty("hour_19")]
            //public double Hour19 { get; set; }

            //[JsonProperty("hour_20")]
            //public double Hour20 { get; set; }

            //[JsonProperty("hour_21")]
            //public double Hour21 { get; set; }

            //[JsonProperty("hour_22")]
            //public double Hour22 { get; set; }

            //[JsonProperty("hour_23")]
            //public double Hour23 { get; set; }

            //[JsonProperty("hour_24")]
            //public double Hour24 { get; set; }

        }
    }
}
