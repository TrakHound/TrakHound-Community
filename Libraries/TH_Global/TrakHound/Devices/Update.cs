using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime;

using System.Data;

using TH_Global.TrakHound.Users;
using TH_Global.Functions;
using TH_Global.Web;
using TH_Global.TrakHound.Configurations;


namespace TH_Global.TrakHound
{
    public static partial class Devices
    {
        [DataContract()]
        private class DeviceInfo
        {
            public DeviceInfo(string uniqueId, DataTable table)
            {
                UniqueId = uniqueId;

                if (table != null)
                {
                    Data = new List<Row>();

                    foreach (DataRow row in table.Rows)
                    {
                        Data.Add(Row.FromDataRow(row));
                    }
                }
            }

            [DataMember(Name = "unique_id")]
            public string UniqueId { get; set; }

            [DataMember(Name = "data")]
            public List<Row> Data { get; set; }

            [DataContract()]
            public class Row
            {
                [DataMember(Name = "address")]
                public string Address { get; set; }

                [DataMember(Name = "value")]
                public string Value { get; set; }

                [DataMember(Name = "attributes")]
                public string Attributes { get; set; }

                public static Row FromDataRow(DataRow row)
                {
                    var result = new Row();

                    result.Address = DataTable_Functions.GetRowValue("address", row);
                    result.Value = DataTable_Functions.GetRowValue("value", row);
                    result.Attributes = DataTable_Functions.GetRowValue("attributes", row);

                    return result;
                }
            } 
        }


        public static bool Update(UserConfiguration userConfig, DeviceConfiguration deviceConfig)
        {
            bool result = false;

            if (userConfig != null)
            {
                var table = Converter.XMLToTable(deviceConfig.ConfigurationXML);
                if (table != null)
                {
                    var infos = new List<DeviceInfo>();
                    infos.Add(new DeviceInfo(deviceConfig.UniqueId, table));

                    string json = JSON.FromObject(infos);
                    if (json != null)
                    {
                        Uri apiHost = UserManagement.ApiHost;

                        string url = new Uri(apiHost, "devices/update/index.php").ToString();

                        var postDatas = new NameValueCollection();
                        postDatas["token"] = userConfig.SessionToken;
                        postDatas["sender_id"] = UserManagement.SenderId.Get();
                        postDatas["devices"] = json;

                        string response = HTTP.POST(url, postDatas);
                        if (response != null)
                        {
                            
                        }
                    }
                }
            }

            return result;
        }

    }
}
