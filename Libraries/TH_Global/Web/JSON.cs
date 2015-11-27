using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    }
}
