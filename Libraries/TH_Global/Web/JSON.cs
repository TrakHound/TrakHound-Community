// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Data;

using Newtonsoft.Json;

namespace TH_Global.Web
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
                        JsonSerializerSettings JSS = new JsonSerializerSettings();
                        JSS.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                        JSS.DateParseHandling = DateParseHandling.DateTime;
                        JSS.DateTimeZoneHandling = DateTimeZoneHandling.Utc;

                        return (T)JsonConvert.DeserializeObject(data, (typeof(T)), JSS);
                    }
                    catch (JsonException jex) { Logger.Log(jex.Message); }
                    catch (Exception ex) { Logger.Log(ex.Message); }
                }

            return default(T);
        }

    }
}
