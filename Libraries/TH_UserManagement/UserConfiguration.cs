using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;

namespace TH_UserManagement
{
    public class UserConfiguration
    {
        public string username { get; set; }
        public string hash { get; set; }
        public int salt { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string company { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string country { get; set; }
        public string zipcode { get; set; }
        public string image_url { get; set; }
        public DateTime last_login { get; set; }

        public static UserConfiguration GetFromDataRow(DataRow row)
        {
            UserConfiguration result = new UserConfiguration();

            foreach (System.Reflection.PropertyInfo info in typeof(UserConfiguration).GetProperties())
            {
                if (info.Name == "last_login") result.last_login = DateTime.UtcNow;
                else
                {
                    if (row.Table.Columns.Contains(info.Name))
                    {
                        object value = row[info.Name];

                        if (value != DBNull.Value)
                        {
                            Type t = info.PropertyType;
                            info.SetValue(result, Convert.ChangeType(value, t), null);
                        }
                    }
                }
            }

            return result;
        }
    }
}
