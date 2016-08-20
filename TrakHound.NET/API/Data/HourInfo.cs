// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace TrakHound.API
{
    public partial class Data
    {
        public class HourInfo
        {
            public const string DateFormat = "yyyy-MM-dd";

            [JsonProperty("date")]
            public string Date { get; set; }

            [JsonProperty("hour")]
            public int Hour { get; set; }


            [JsonProperty("planned_production_time")]
            public double PlannedProductionTime { get; set; }

            [JsonProperty("operating_time")]
            public double OperatingTime { get; set; }

            [JsonProperty("ideal_operating_time")]
            public double IdealOperatingTime { get; set; }

            [JsonProperty("total_pieces")]
            public int TotalPieces { get; set; }

            [JsonProperty("good_pieces")]
            public int GoodPieces { get; set; }


            [JsonProperty("total_time")]
            public double TotalTime { get; set; }


            [JsonProperty("active")]
            public double Active { get; set; }

            [JsonProperty("idle")]
            public double Idle { get; set; }

            [JsonProperty("alert")]
            public double Alert { get; set; }


            [JsonProperty("production")]
            public double Production { get; set; }

            [JsonProperty("setup")]
            public double Setup { get; set; }

            [JsonProperty("teardown")]
            public double Teardown { get; set; }

            [JsonProperty("maintenance")]
            public double Maintenance { get; set; }

            [JsonProperty("process_development")]
            public double ProcessDevelopment { get; set; }


            [JsonProperty("oee")]
            public double Oee
            {
                get
                {
                    return Availability * Performance * Quality;
                }
            }

            [JsonProperty("availability")]
            public double Availability
            {
                get
                {
                    var oee = new OEE();
                    oee.ConstantQuality = 1;
                    oee.PlannedProductionTime = PlannedProductionTime;
                    oee.OperatingTime = OperatingTime;

                    return oee.Availability;
                }
            }

            [JsonProperty("performance")]
            public double Performance
            {
                get
                {
                    var oee = new OEE();
                    oee.ConstantQuality = 1;
                    oee.PlannedProductionTime = PlannedProductionTime;
                    oee.OperatingTime = OperatingTime;
                    oee.IdealOperatingTime = IdealOperatingTime;

                    return oee.Performance;
                }
            }

            [JsonProperty("quality")]
            public double Quality
            {
                get
                {
                    var oee = new OEE();
                    oee.ConstantQuality = 1;
                    oee.TotalPieces = TotalPieces;
                    oee.GoodPieces = GoodPieces;

                    return oee.Quality;
                }
            }

            public static List<HourInfo> CombineHours(List<HourInfo> hours)
            {
                var newHours = new List<HourInfo>();

                var _hours = hours.ToList();

                var distinctDates = _hours.Select(o => o.Date).Distinct();
                foreach (string distinctDate in distinctDates.ToList())
                {
                    var sameDate = _hours.FindAll(o => o.Date == distinctDate);

                    var distinctHours = sameDate.Select(o => o.Hour).Distinct();
                    foreach (int distinctHour in distinctHours.ToList())
                    {
                        var hourInfo = new HourInfo();
                        hourInfo.Date = distinctDate;
                        hourInfo.Hour = distinctHour;

                        var sameHours = _hours.FindAll(o => o.Hour == distinctHour);
                        foreach (var sameHour in sameHours.ToList())
                        {
                            // OEE
                            hourInfo.PlannedProductionTime += sameHour.PlannedProductionTime;
                            hourInfo.OperatingTime += sameHour.OperatingTime;
                            hourInfo.IdealOperatingTime += sameHour.IdealOperatingTime;
                            hourInfo.TotalPieces += sameHour.TotalPieces;
                            hourInfo.GoodPieces += sameHour.GoodPieces;

                            hourInfo.TotalTime += sameHour.TotalTime;

                            // Device Status
                            hourInfo.Active += sameHour.Active;
                            hourInfo.Idle += sameHour.Idle;
                            hourInfo.Alert += sameHour.Alert;

                            // Production Status
                            hourInfo.Production += sameHour.Production;
                            hourInfo.Setup += sameHour.Setup;
                            hourInfo.Teardown += sameHour.Teardown;
                            hourInfo.Maintenance += sameHour.Maintenance;
                            hourInfo.ProcessDevelopment += sameHour.ProcessDevelopment;
                        }

                        newHours.Add(hourInfo);
                    }
                }

                return newHours;
            }

            public static OeeInfo GetOeeInfo(List<HourInfo> hours)
            {
                var result = new OeeInfo();

                if (hours.Count > 0)
                {
                    var oee = new OEE();
                    oee.ConstantQuality = 1;

                    foreach (var hour in hours)
                    {
                        oee.PlannedProductionTime += hour.PlannedProductionTime;
                        oee.OperatingTime += hour.OperatingTime;
                        oee.IdealOperatingTime += hour.IdealOperatingTime;
                        oee.TotalPieces += hour.TotalPieces;
                        oee.GoodPieces += hour.GoodPieces;
                    }

                    result.Oee = oee.Oee;
                    result.Availability = oee.Availability;
                    result.Performance = oee.Performance;
                    result.Quality = oee.Quality;
                }

                return result;
            }

            //public static TimersInfo GetTimersInfo(List<HourInfo> hours)
            //{
            //    var result = new TimersInfo();

            //    if (hours.Count > 0)
            //    {
            //        result.Total = hours.Select(o => o.TotalTime).Sum();

            //        result.Active = hours.Select(o => o.Active).Sum();
            //        result.Idle = hours.Select(o => o.Idle).Sum();
            //        result.Alert = hours.Select(o => o.Alert).Sum();

            //        result.Production = hours.Select(o => o.Production).Sum();
            //        result.Setup = hours.Select(o => o.Setup).Sum();
            //        result.Teardown = hours.Select(o => o.Teardown).Sum();
            //        result.Maintenance = hours.Select(o => o.Maintenance).Sum();
            //        result.ProcessDevelopment = hours.Select(o => o.ProcessDevelopment).Sum();

            //        result.

            //        result.Active = hours.Select(o => o.Active).Sum();
            //        result.Active = hours.Select(o => o.Active).Sum();
            //        result.Active = hours.Select(o => o.Active).Sum();
            //        result.Active = hours.Select(o => o.Active).Sum();
            //        result.Active = hours.Select(o => o.Active).Sum();
            //        result.Active = hours.Select(o => o.Active).Sum();
            //        result.Active = hours.Select(o => o.Active).Sum();
            //        result.Active = hours.Select(o => o.Active).Sum();
            //        result.Active = hours.Select(o => o.Active).Sum();
            //        result.Active = hours.Select(o => o.Active).Sum();



            //    }

            //    return result;
            //}
        }
    }
}
