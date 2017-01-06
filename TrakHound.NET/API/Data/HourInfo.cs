// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

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
            public const string DATE_FORMAT = "yyyy-MM-dd";
            public const int MAX_VALUE = 3600;

            [JsonProperty("date")]
            public string Date { get; set; }

            [JsonProperty("hour")]
            public int Hour { get; set; }


            private double plannedProductionTime;
            [JsonProperty("planned_production_time")]
            public double PlannedProductionTime
            {
                get { return plannedProductionTime; }
                set
                {
                    var val = Math.Min(value, MAX_VALUE);
                    plannedProductionTime = val;
                }
            }

            private double operatingTime;
            [JsonProperty("operating_time")]
            public double OperatingTime
            {
                get { return operatingTime; }
                set
                {
                    var val = Math.Min(value, MAX_VALUE);
                    operatingTime = val;
                }
            }

            private double idealOperatingTime;
            [JsonProperty("ideal_operating_time")]
            public double IdealOperatingTime
            {
                get { return idealOperatingTime; }
                set
                {
                    var val = Math.Min(value, MAX_VALUE);
                    idealOperatingTime = val;
                }
            }

            [JsonProperty("total_pieces")]
            public int TotalPieces { get; set; }

            [JsonProperty("good_pieces")]
            public int GoodPieces { get; set; }


            private double active;
            [JsonProperty("active")]
            public double Active
            {
                get { return active; }
                set
                {
                    var val = Math.Min(value, MAX_VALUE);
                    active = val;
                }
            }

            private double idle;
            [JsonProperty("idle")]
            public double Idle
            {
                get { return idle; }
                set
                {
                    var val = Math.Min(value, MAX_VALUE);
                    idle = val;
                }
            }

            private double alert;
            [JsonProperty("alert")]
            public double Alert
            {
                get { return alert; }
                set
                {
                    var val = Math.Min(value, MAX_VALUE);
                    alert = val;
                }
            }


            private double production;
            [JsonProperty("production")]
            public double Production
            {
                get { return production; }
                set
                {
                    var val = Math.Min(value, MAX_VALUE);
                    production = val;
                }
            }

            private double setup;
            [JsonProperty("setup")]
            public double Setup
            {
                get { return setup; }
                set
                {
                    var val = Math.Min(value, MAX_VALUE);
                    setup = val;
                }
            }

            private double teardown;
            [JsonProperty("teardown")]
            public double Teardown
            {
                get { return teardown; }
                set
                {
                    var val = Math.Min(value, MAX_VALUE);
                    teardown = val;
                }
            }

            private double maintenance;
            [JsonProperty("maintenance")]
            public double Maintenance
            {
                get { return maintenance; }
                set
                {
                    var val = Math.Min(value, MAX_VALUE);
                    maintenance = val;
                }
            }

            private double processDevelopment;
            [JsonProperty("process_development")]
            public double ProcessDevelopment
            {
                get { return processDevelopment; }
                set
                {
                    var val = Math.Min(value, MAX_VALUE);
                    processDevelopment = val;
                }
            }


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


            public DateTime GetDateTime()
            {
                if (!string.IsNullOrEmpty(Date))
                {
                    DateTime date = DateTime.MinValue;
                    if (DateTime.TryParse(Date, out date))
                    {
                        try
                        {
                            return new DateTime(date.Year, date.Month, date.Day, Hour, 0, 0, DateTimeKind.Utc);
                        }
                        catch (Exception ex)
                        {
                            Logging.Logger.Log("GetDateTime() :: Exception :: " + ex.Message);
                        }
                    }
                }

                return DateTime.MinValue;
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

            public static TimersInfo GetTimersInfo(List<HourInfo> hours)
            {
                var result = new TimersInfo();

                if (hours.Count > 0)
                {
                    double totalTime = 0;

                    double active = 0;
                    double idle = 0;
                    double alert = 0;

                    double production = 0;
                    double setup = 0;
                    double teardown = 0;
                    double maintenance = 0;
                    double processDevelopment = 0;

                    foreach (var hour in hours)
                    {
                        totalTime += hour.PlannedProductionTime;

                        // Device Status
                        active += hour.Active;
                        idle += hour.Idle;
                        alert += hour.Alert;

                        // Production Status
                        production += hour.Production;
                        setup += hour.Setup;
                        teardown += hour.Teardown;
                        maintenance += hour.Maintenance;
                        processDevelopment += hour.ProcessDevelopment;
                    }

                    result.Total = Math.Round(totalTime, 2);

                    result.Active = Math.Round(active, 2);
                    result.Idle = Math.Round(idle, 2);
                    result.Alert = Math.Round(alert, 2);

                    result.Production = Math.Round(production, 2);
                    result.Setup = Math.Round(setup, 2);
                    result.Teardown = Math.Round(teardown, 2);
                    result.Maintenance = Math.Round(maintenance, 2);
                    result.ProcessDevelopment = Math.Round(processDevelopment, 2);
                }

                return result;
            }

        }
    }
}
