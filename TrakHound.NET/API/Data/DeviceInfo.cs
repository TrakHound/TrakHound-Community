// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

using TrakHound.Tools.Web;

namespace TrakHound.API
{
    public static partial class Data
    {
        public class DeviceInfo
        {
            private object _lock = new object();


            [JsonProperty("unique_id")]
            public string UniqueId { get; set; }

            [JsonProperty("enabled")]
            public bool Enabled { get; set; }

            [JsonProperty("index")]
            public int Index { get; set; }


            [JsonProperty("description")]
            public DescriptionInfo Description
            {
                get
                {
                    var obj = GetClass("description");
                    if (obj != null) return (DescriptionInfo)obj;
                    return null;
                }
                set
                {
                    var o = value;
                    if (o != null)
                    {
                        AddClass("description", o);
                    }
                }
            }

            [JsonProperty("status")]
            public StatusInfo Status
            {
                get
                {
                    var obj = GetClass("status");
                    if (obj != null) return (StatusInfo)obj;
                    return null;
                }
                set
                {
                    var o = value;
                    if (o != null)
                    {
                        AddClass("status", o);
                    }
                }
            }

            [JsonProperty("controller")]
            public ControllerInfo Controller
            {
                get
                {
                    var obj = GetClass("controller");
                    if (obj != null) return (ControllerInfo)obj;
                    return null;
                }
                set
                {
                    var o = value;
                    if (o != null)
                    {
                        AddClass("controller", o);
                    }
                }
            }

            [JsonProperty("oee")]
            public OeeInfo Oee
            {
                get
                {
                    var obj = GetClass("oee");
                    if (obj != null) return (OeeInfo)obj;
                    return null;
                }
                set
                {
                    var o = value;
                    if (o != null)
                    {
                        AddClass("oee", o);
                    }
                }
            }

            [JsonProperty("timers")]
            public TimersInfo Timers
            {
                get
                {
                    var obj = GetClass("timers");
                    if (obj != null) return (TimersInfo)obj;
                    return null;
                }
                set
                {
                    var o = value;
                    if (o != null)
                    {
                        AddClass("timers", o);
                    }
                }
            }

            [JsonProperty("cycles")]
            public List<CycleInfo> Cycles
            {
                get
                {
                    var obj = GetClass("cycles");
                    if (obj != null) return (List<CycleInfo>)obj;
                    return null;
                }
                set
                {
                    var o = value;
                    if (o != null)
                    {
                        AddClass("cycles", o);
                    }
                }
            }

            [JsonProperty("hours")]
            public List<HourInfo> Hours
            {
                get
                {
                    var obj = GetClass("hours");
                    if (obj != null) return (List<HourInfo>)obj;
                    return null;
                }
                set
                {
                    var o = value;
                    if (o != null)
                    {
                        AddClass("hours", o);
                    }
                }
            }

            [JsonProperty("overrides")]
            public List<OverrideInfo> Overrides
            {
                get
                {
                    var obj = GetClass("overrides");
                    if (obj != null) return (List<OverrideInfo>)obj;
                    return null;
                }
                set
                {
                    var o = value;
                    if (o != null)
                    {
                        AddClass("overrides", o);
                    }
                }
            }

            [JsonProperty("agent")]
            public AgentInfo Agent
            {
                get
                {
                    var obj = GetClass("agent");
                    if (obj != null) return (AgentInfo)obj;
                    return null;
                }
                set
                {
                    var o = value;
                    if (o != null)
                    {
                        AddClass("agent", o);
                    }
                }
            }


            public void CombineHours()
            {
                lock (_lock)
                {
                    if (Hours != null && Hours.Count > 0)
                    {
                        var newHours = new List<HourInfo>();

                        var _hours = Hours.ToList();
                        if (_hours != null) _hours = _hours.FindAll(o => o != null); // Clean list of any null HourInfos

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

                                if (hourInfo.OperatingTime != hourInfo.Active)
                                {
                                    string format = "OperatingTime = {0}({1}) :: Active = {2}({3})";
                                    Console.WriteLine(format, hourInfo.OperatingTime, operatingtime, hourInfo.Active, active);
                                }

                                newHours.Add(hourInfo);
                            }

                        }

                        Hours = newHours;
                    }
                }
            }


            #region "SubClass Management"

            private Dictionary<string, object> _classes;
            public Dictionary<string, object> Classes
            {
                get
                {
                    if (_classes == null) _classes = new Dictionary<string, object>();
                    return _classes;
                }
                set { _classes = value; }
            }


            public void AddClass(string id, object obj)
            {
                if (!string.IsNullOrEmpty(id))
                {
                    lock (_lock)
                    {
                        object o = null;
                        Classes.TryGetValue(id, out o);
                        if (o == null) Classes.Add(id, obj);
                        else
                        {
                            Classes.Remove(id);
                            Classes.Add(id, obj);
                        }
                    }
                }
            }

            public object GetClass(string id)
            {
                if (!string.IsNullOrEmpty(id))
                {
                    lock (_lock)
                    {
                        object obj = null;
                        Classes.TryGetValue(id, out obj);

                        return obj;
                    }
                }

                return null;
            }

            public void RemoveClass(string id)
            {
                lock (_lock)
                {
                    if (id != null) Classes.Remove(id);
                }
            }

            public void ClearClasses()
            {
                lock (_lock)
                {
                    Classes.Clear();
                }
            }

            #endregion


            public string ToJson()
            {
                return JSON.FromObject(ToJsonObject());
            }

            private object ToJsonObject()
            {
                lock (_lock)
                {
                    var data = new Dictionary<string, object>();

                    data.Add("unique_id", UniqueId);
                    data.Add("enabled", Enabled);
                    data.Add("index", Index);

                    foreach (var c in Classes)
                    {
                        object match = false;
                        if (!data.TryGetValue(c.Key, out match))
                        {
                            data.Add(c.Key, c.Value);
                        }
                    }

                    return data;
                }
            }

            public static string ListToJson(List<DeviceInfo> deviceInfos)
            {
                var datas = new List<object>();

                foreach (var deviceInfo in deviceInfos.ToList())
                {
                    var data = deviceInfo.ToJsonObject();
                    if (data != null) datas.Add(data);
                }

                if (datas.Count > 0) return JSON.FromList<object>(datas);
                else return null;
            }

            public DeviceInfo Copy()
            {
                lock (_lock)
                {
                    var result = new DeviceInfo();
                    result.UniqueId = UniqueId;
                    result.Enabled = Enabled;
                    result.Index = Index;

                    foreach (var c in Classes.ToList())
                    {
                        object o = null;
                        result.Classes.TryGetValue(c.Key, out o);
                        if (o == null) result.Classes.Add(c.Key, c.Value);
                        else
                        {
                            result.Classes.Remove(c.Key);
                            result.Classes.Add(c.Key, c.Value);
                        }
                    }

                    return result;
                }
            }


            public void AddHourInfo(HourInfo hourInfo)
            {
                lock (_lock)
                {
                    if (hourInfo != null)
                    {

                        var hours = Hours;
                        if (hours == null) hours = new List<HourInfo>();

                        hours.Add(hourInfo);

                        Hours = hours;
                    }
                }
            }

            public void AddHourInfos(List<HourInfo> hourInfos)
            {
                lock (_lock)
                {
                    if (hourInfos != null && hourInfos.Count > 0)
                    {
                        var _hourInfos = hourInfos.FindAll(o => o != null);

                        var hours = Hours;
                        if (hours == null) hours = new List<HourInfo>();

                        foreach (var hourInfo in _hourInfos)
                        {
                            if (hourInfo != null) hours.Add(hourInfo);
                        }

                        Hours = hours;
                    }
                }
            }

        }
    }
}
