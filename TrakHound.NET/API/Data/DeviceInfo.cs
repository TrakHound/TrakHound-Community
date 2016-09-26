// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

using TrakHound.Tools.Web;

namespace TrakHound.API
{
    public static partial class Data
    {
        public class DeviceInfo
        {

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
                if (Hours != null && Hours.Count > 0)
                {
                    lock (Hours)
                    {
                        var newHours = new List<HourInfo>();

                        var _hours = Hours.ToList();
                        if (_hours != null)
                        {
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

                            Hours = newHours;
                        }
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
                lock (Classes)
                {
                    var o = GetClass(id);
                    if (o == null) Classes.Add(id, obj);
                    else
                    {
                        RemoveClass(id);
                        AddClass(id, obj);
                    }
                }
            }

            public object GetClass(string id)
            {
                object obj = null;
                Classes.TryGetValue(id, out obj);

                return obj;
            }

            public void RemoveClass(string id)
            {
                if (id != null) Classes.Remove(id);
            }

            public void ClearClasses()
            {
                Classes.Clear();
            }

            #endregion


            public string ToJson()
            {
                return JSON.FromObject(ToJsonObject());
            }

            private object ToJsonObject()
            {
                var data = new Dictionary<string, object>();

                data.Add("unique_id", UniqueId);
                data.Add("enabled", Enabled);

                foreach (var c in Classes)
                {
                    object match = false;
                    if (!data.TryGetValue(c.Key, out match))
                    {

                        data.Add(c.Key, c.Value);
                    }
                }

                if (data.Count > 1) return data;
                else return null;
            }

            public static string ListToJson(List<DeviceInfo> deviceInfos)
            {
                var datas = new List<object>();

                foreach (var deviceInfo in deviceInfos)
                {
                    var data = deviceInfo.ToJsonObject();
                    if (data != null) datas.Add(data);
                }

                if (datas.Count > 0) return JSON.FromList<object>(datas);
                else return null;
            }

            public DeviceInfo Copy()
            {
                var result = new DeviceInfo();
                foreach (var c in Classes)
                {
                    result.AddClass(c.Key, c.Value);
                }

                return result;
            }


            public void AddHourInfo(Data.HourInfo hourInfo)
            {
                if (hourInfo != null)
                {
                    lock (this)
                    {
                        var hours = Hours;
                        if (hours == null) hours = new List<Data.HourInfo>();

                        hours.Add(hourInfo);

                        Hours = hours;
                    }
                }
            }

            public void AddHourInfos(List<Data.HourInfo> hourInfos)
            {
                if (hourInfos != null && hourInfos.Count > 0)
                {
                    var _hourInfos = hourInfos.FindAll(o => o != null);

                    lock (this)
                    {
                        var hours = Hours;
                        if (hours == null) hours = new List<Data.HourInfo>();

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
