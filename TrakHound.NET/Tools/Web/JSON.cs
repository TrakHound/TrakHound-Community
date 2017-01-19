// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace TrakHound.Tools.Web
{
    public static class JSON
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static DataTable ToTable(string data)
        {
            DataTable result = null;

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

                        DataTable DT = (DataTable)JsonConvert.DeserializeObject(data, (typeof(DataTable)), jss);

                        result = DT;
                    }
                    catch (JsonException ex) { logger.Error(ex); }
                    catch (Exception ex) { logger.Error(ex); }
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
                        jss.NullValueHandling = NullValueHandling.Ignore;

                        data = ConvertToSafe(data);

                        return (T)JsonConvert.DeserializeObject(data, (typeof(T)), jss);
                    }
                    catch (JsonException ex) { logger.Error(ex); }
                    catch (Exception ex) { logger.Error(ex); }
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
                catch (JsonException ex) { logger.Error(ex); }
                catch (Exception ex) { logger.Error(ex); }
            }

            return null;
        }

        private static string ConvertToSafe(string s)
        {
            return s;
        }

        public static string FromList<T>(List<T> data)
        {
            try
            {
                var jss = new JsonSerializerSettings();
                jss.NullValueHandling = NullValueHandling.Ignore;

                return JsonConvert.SerializeObject(data.ToList(), jss);
            }
            catch (JsonException ex) { logger.Error(ex); }
            catch (Exception ex) { logger.Error(ex); }

            return null;
        }

        public static string FromObject(object data)
        {
            try
            {
                var jss = new JsonSerializerSettings();
                jss.NullValueHandling = NullValueHandling.Ignore;

                return JsonConvert.SerializeObject(data, jss);
            }
            catch (JsonException ex) { logger.Error(ex); }
            catch (Exception ex) { logger.Error(ex); }

            return null;
        }

    }
}
