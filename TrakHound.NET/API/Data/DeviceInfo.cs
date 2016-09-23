// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Newtonsoft.Json;
using System.Collections.Generic;

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
                var o = GetClass(id);
                if (o == null) Classes.Add(id, obj);
                else
                {
                    RemoveClass(id);
                    AddClass(id, obj);
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

        }
    }
}
