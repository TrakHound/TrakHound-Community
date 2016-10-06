// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Newtonsoft.Json;
using System;
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
                    return Math.Round(Math.Min(1, Math.Max(0, Availability * Performance * Quality)), 4);
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

                    return Math.Round(Math.Min(1, Math.Max(0, oee.Availability)), 4);
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

                    return Math.Round(Math.Min(1, Math.Max(0, oee.Performance)), 4);
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

                    return Math.Round(Math.Min(1, Math.Max(0, oee.Quality)), 4);
                }
            }

            public static List<HourInfo> CombineHours(List<HourInfo> hours)
            {
                var newHours = new List<HourInfo>();

                // Used 'lock' since it sometimes threw a 'source array was not long enough' exception
                var _hours = hours.ToList();

                // Clean list of any null HourInfos
                _hours = _hours.FindAll(o => o != null);

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

                        double plannedProductionTime = 0;
                        double operatingtime = 0;
                        double idealOperatingTime = 0;
                        int totalPieces = 0;
                        int goodPieces = 0;

                        double totalTime = 0;

                        double active = 0;
                        double idle = 0;
                        double alert = 0;

                        double production = 0;
                        double setup = 0;
                        double teardown = 0;
                        double maintenance = 0;
                        double processDevelopment = 0;

                        var sameHours = _hours.FindAll(o => o.Hour == distinctHour);
                        foreach (var sameHour in sameHours.ToList())
                        {
                            // OEE
                            plannedProductionTime += sameHour.PlannedProductionTime;
                            operatingtime += sameHour.OperatingTime;
                            idealOperatingTime += sameHour.IdealOperatingTime;
                            totalPieces += sameHour.TotalPieces;
                            goodPieces += sameHour.GoodPieces;

                            totalTime += sameHour.TotalTime;

                            // Device Status
                            active += sameHour.Active;
                            idle += sameHour.Idle;
                            alert += sameHour.Alert;

                            // Production Status
                            production += sameHour.Production;
                            setup += sameHour.Setup;
                            teardown += sameHour.Teardown;
                            maintenance += sameHour.Maintenance;
                            processDevelopment += sameHour.ProcessDevelopment;
                        }

                        hourInfo.PlannedProductionTime = Math.Round(plannedProductionTime, 2);
                        hourInfo.OperatingTime = Math.Round(operatingtime, 2);
                        hourInfo.IdealOperatingTime = Math.Round(idealOperatingTime, 2);
                        hourInfo.TotalPieces = totalPieces;
                        hourInfo.GoodPieces = goodPieces;

                        hourInfo.TotalTime = Math.Round(totalTime, 2);

                        hourInfo.Active = Math.Round(active, 2);
                        hourInfo.Idle = Math.Round(idle, 2);
                        hourInfo.Alert = Math.Round(alert, 2);

                        hourInfo.Production = Math.Round(production, 2);
                        hourInfo.Setup = Math.Round(setup, 2);
                        hourInfo.Teardown = Math.Round(teardown, 2);
                        hourInfo.Maintenance = Math.Round(maintenance, 2);
                        hourInfo.ProcessDevelopment = Math.Round(processDevelopment, 2);

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

                    result.Oee = Math.Round(Math.Min(1, Math.Max(0, oee.Oee)), 4);
                    result.Availability = Math.Round(Math.Min(1, Math.Max(0, oee.Availability)), 4);
                    result.Performance = Math.Round(Math.Min(1, Math.Max(0, oee.Performance)), 4);
                    result.Quality = Math.Round(Math.Min(1, Math.Max(0, oee.Quality)), 4);
                    result.TotalPieces = oee.TotalPieces;
                    result.GoodPieces = oee.GoodPieces;
                }

                return result;
            }

        }
    }
}
