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
                    //else
                    //{
                    //    obj = new StatusInfo();
                    //    AddClass("status", obj);
                    //    return (StatusInfo)obj;
                    //}
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
                    //else
                    //{
                    //    obj = new ControllerInfo();
                    //    AddClass("controller", obj);
                    //    return (ControllerInfo)obj;
                    //}
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
                    //else
                    //{
                    //    obj = new OeeInfo();
                    //    AddClass("oee", obj);
                    //    return (OeeInfo)obj;
                    //}
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
                    //else
                    //{
                    //    obj = new TimersInfo();
                    //    AddClass("timers", obj);
                    //    return (TimersInfo)obj;
                    //}
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
                    //else
                    //{
                    //    obj = new List<CycleInfo>();
                    //    AddClass("cycles", obj);
                    //    return (List<CycleInfo>)obj;
                    //}
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
                    //else
                    //{
                    //    obj = new List<HourInfo>();
                    //    AddClass("hours", obj);
                    //    return (List<HourInfo>)obj;
                    //}
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


            private Dictionary<string, object> classes = new Dictionary<string, object>();

            public Dictionary<string, object> Classes { get { return classes; } }


            public void AddClass(string id, object obj)
            {
                var o = GetClass(id);
                if (o == null) classes.Add(id, obj);
                else
                {
                    RemoveClass(id);
                    AddClass(id, obj);
                }
            }

            public object GetClass(string id)
            {
                object obj = null;
                classes.TryGetValue(id, out obj);

                return obj;
            }

            public void RemoveClass(string id)
            {
                if (id != null) classes.Remove(id);
            }

            public void ClearClasses()
            {
                classes.Clear();
            }

            //public void AddHourInfo(Data.HourInfo hour)
            //{
            //    var obj = GetClass("hours");
            //    if (obj == null)
            //    {
            //        obj = new List<Data.HourInfo>();
            //        AddClass("hours", obj);
            //    }

            //    var hours = (List<Data.HourInfo>)obj;
            //    hours.Add(hour);
            //}


            public string ToJson()
            {
                return JSON.FromObject(ToJsonObject());
            }

            private object ToJsonObject()
            {
                var data = new Dictionary<string, object>();

                data.Add("unique_id", UniqueId);

                foreach (var c in classes)
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
