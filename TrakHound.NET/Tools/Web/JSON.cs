// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using Newtonsoft.Json;

using TrakHound.Logging;

namespace TrakHound.Tools.Web
{
    public static class JSON
    {

        public static DataTable ToTable(string data)
        {
            DataTable result = null;

            if (data != null)
            if (data.Trim() != "")
            {
                try
                {
                    JsonSerializerSettings JSS = new JsonSerializerSettings();
                    JSS.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                    JSS.DateParseHandling = DateParseHandling.DateTime;
                    JSS.DateTimeZoneHandling = DateTimeZoneHandling.Utc;

                    data = ConvertToSafe(data);

                    DataTable DT = (DataTable)JsonConvert.DeserializeObject(data, (typeof(DataTable)), JSS);

                    result = DT;
                }
                catch (JsonException jex) { Logger.Log(jex.Message); }
                catch (Exception ex) { Logger.Log(ex.Message); }
            }

            return result;
        }

        public static T ToType<T>(string data)
        {
            if (data != null)
                if (data.Trim() != "")
                {
                    try
                    {
                        var jss = new JsonSerializerSettings();
                        jss.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                        jss.DateParseHandling = DateParseHandling.DateTime;
                        jss.DateTimeZoneHandling = DateTimeZoneHandling.Utc;

                        data = ConvertToSafe(data);

                        return (T)JsonConvert.DeserializeObject(data, (typeof(T)), jss);
                    }
                    catch (JsonException jex) { Logger.Log(jex.Message); }
                    catch (Exception ex) { Logger.Log(ex.Message); }
                }

            return default(T);
        }

        public static Dictionary<string, dynamic> ToDictionary(string json)
        {
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var jss = new JsonSerializerSettings();
                    jss.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                    jss.DateParseHandling = DateParseHandling.DateTime;
                    jss.DateTimeZoneHandling = DateTimeZoneHandling.Utc;

                    json = ConvertToSafe(json);

                    return JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(json, jss);
                }
                catch (JsonException jex) { Logger.Log(jex.Message); }
                catch (Exception ex) { Logger.Log(ex.Message); }
            }

            return null;

            //if (data != null)
            //    if (data.Trim() != "")
            //    {
            //        try
            //        {
            //            var jss = new JsonSerializerSettings();
            //            jss.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            //            jss.DateParseHandling = DateParseHandling.DateTime;
            //            jss.DateTimeZoneHandling = DateTimeZoneHandling.Utc;

            //            data = ConvertToSafe(data);

            //            return (T)JsonConvert.DeserializeObject(data, (typeof(T)), jss);
            //        }
            //        catch (JsonException jex) { Logger.Log(jex.Message); }
            //        catch (Exception ex) { Logger.Log(ex.Message); }
            //    }

            //return default(T);
        }

        private static string ConvertToSafe(string s)
        {
            //if (s.Contains("%")) s = s.Replace("%", "%25");

            return s;
        }

        public static string FromList<T>(List<T> data)
        {
            try
            {
                return JsonConvert.SerializeObject(data.ToList());
            }
            catch (Exception ex)
            {
                Logger.Log("Error During Serialization :: " + ex.Message, LogLineType.Error);
            }

            return null;
        }

        public static string FromObject(object data)
        {
            return JsonConvert.SerializeObject(data);
        }

    }
}
