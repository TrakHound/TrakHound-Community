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
        public class DayInfo
        {
            public DayInfo()
            {
                Hours = new List<HourInfo>();
            }

            [JsonProperty("date")]
            public string Date { get; set; }

            [JsonProperty("hours")]
            public List<HourInfo> Hours { get; set; }

            [JsonProperty("oee")]
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

            [JsonProperty("availability")]
            public double Availability
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
                        }

                        return oee.Availability;
                    }

                    return 0;
                }
            }

            [JsonProperty("performance")]
            public double Performance
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

                        return oee.Performance;
                    }

                    return 0;
                }
            }

            [JsonProperty("quality")]
            public double Quality
            {
                get
                {
                    if (Hours.Count > 0)
                    {
                        var oee = new OEE();
                        oee.ConstantQuality = 1; // Always returns 1

                        foreach (var hour in Hours)
                        {
                            oee.TotalPieces += hour.TotalPieces;
                            oee.GoodPieces += hour.GoodPieces;
                        }

                        return oee.Quality;
                    }

                    return 0;
                }
            }


            public void CombineHours()
            {
                var newHours = new List<HourInfo>();

                var _hours = Hours.ToList();

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

                Hours.Clear();
                Hours.AddRange(newHours);
            }
           
        }
    }
}
