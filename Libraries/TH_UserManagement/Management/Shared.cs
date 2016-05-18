using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;
using System.Data;
using System.Net;
using System.Collections.Specialized;

using TH_Configuration;
using TH_Global;
using TH_Global.Functions;
using TH_Global.Web;

namespace TH_UserManagement.Management
{

    public static class Shared
    {
        public class SharedListItem
        {
            public string id { get; set; }

            public string description { get; set; }
            public string manufacturer { get; set; }
            public string device_type { get; set; }
            public string model { get; set; }
            public string controller { get; set; }

            public string author { get; set; }
            public bool certified { get; set; }
            public Int64 stars { get; set; }
            public Int64 downloads { get; set; }

            public string tablename { get; set; }
            public DateTime upload_date { get; set; }
            public string version { get; set; }

            public string image_url { get; set; }
            public string tags { get; set; }
            public string dependencies { get; set; }
            public string link_tag { get; set; }

            public string list_id { get; set; }
        }

        public static string GetRowValue(string name, DataRow row)
        {
            if (row.Table.Columns.Contains(name))
            {
                if (row[name] != String.Empty) return row[name].ToString();
            }

            return null;
        }

        public static List<SharedListItem> GetSharedList()
        {
            List<SharedListItem> result = new List<SharedListItem>();

            try
            {
                using (WebClient client = new WebClient())
                {
                    NameValueCollection values = new NameValueCollection();

                    byte[] response = client.UploadValues("http://www.feenux.com/php/configurations/getsharedlist.php", values);

                    string responseString = Encoding.Default.GetString(response);

                    if (responseString.Trim() != "")
                    {
                        try
                        {
                            DataTable dt = JSON.ToTable(responseString);
                            if (dt != null)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    SharedListItem item = new SharedListItem();

                                    item.id = GetRowValue("id", row);

                                    item.description = GetRowValue("description", row);
                                    item.manufacturer = GetRowValue("manufacturer", row);
                                    item.device_type = GetRowValue("device_type", row);
                                    item.model = GetRowValue("model", row);
                                    item.controller = GetRowValue("controller", row);

                                    item.author = GetRowValue("author", row);
                                    string s;

                                    s = GetRowValue("certified", row);
                                    bool certified;
                                    bool.TryParse(s, out certified);
                                    item.certified = certified;

                                    s = GetRowValue("stars", row);
                                    Int64 stars;
                                    Int64.TryParse(s, out stars);
                                    item.stars = stars;

                                    s = GetRowValue("downloads", row);
                                    Int64 downloads;
                                    Int64.TryParse(s, out downloads);
                                    item.downloads = downloads;

                                    item.tablename = GetRowValue("tablename", row);

                                    s = GetRowValue("upload_date", row);
                                    DateTime upload_date = DateTime.MinValue;
                                    DateTime.TryParse(s, out upload_date);
                                    item.upload_date = upload_date;

                                    item.version = GetRowValue("version", row);
                                    item.image_url = GetRowValue("image_url", row);
                                    item.tags = GetRowValue("tags", row);
                                    item.dependencies = GetRowValue("dependencies", row);
                                    item.link_tag = GetRowValue("link_tag", row);

                                    item.list_id = String_Functions.RandomString(20);

                                    result.Add(item);
                                }
                            }
                        }
                        catch (Exception ex) { Logger.Log("GetSharedList() :: Exception :: " + ex.Message); }
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex.Message); }

            return result;
        }

        public static List<Configuration> GetOwnedSharedConfigurations(UserConfiguration userConfig)
        {
            List<Configuration> result = null;

            NameValueCollection values = new NameValueCollection();

            if (userConfig.username != null) values["username"] = userConfig.username.ToLower();

            string url = "https://www.feenux.com/php/configurations/getownedsharedconfigurations.php";
            string responseString = HTTP.POST(url, values);

            if (responseString != null)
            {
                result = new List<Configuration>();

                string[] tables = responseString.Split('%');

                foreach (string table in tables)
                {
                    if (!String.IsNullOrEmpty(table))
                    {
                        var delimiter = table.IndexOf('~');
                        if (delimiter > 0)
                        {
                            string tablename = table.Substring(0, delimiter);
                            string tabledata = table.Substring(delimiter + 1);

                            DataTable dt = JSON.ToTable(tabledata);
                            if (dt != null)
                            {
                                XmlDocument xml = TH_Configuration.Converter.TableToXML(dt);
                                if (xml != null)
                                {
                                    Configuration config = TH_Configuration.Configuration.Read(xml);
                                    if (config != null)
                                    {
                                        config.Remote = true;
                                        config.TableName = tablename;
                                        result.Add(config);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        public static bool CreateSharedConfiguration(string tablename, DataTable dt, SharedListItem item)
        {
            bool result = false;

            item.tablename = tablename;

            if (UpdateSharedConfiguration_ToList(item))
            {
                if (Remote.Configurations.Create(tablename))
                {
                    if (dt != null)
                    {
                        if (Remote.Configurations.Update(tablename, dt)) return true;
                    }
                }
            }

            return result;
        }

        public static bool UpdateDownloads(SharedListItem item)
        {
            bool result = false;

            if (item != null)
            {
                NameValueCollection values = new NameValueCollection();

                values["id"] = item.id;

                string url = "https://www.feenux.com/php/configurations/updateshareddownloads.php";

                string responseString = HTTP.POST(url, values);

                if (responseString != null) if (responseString.ToLower().Trim() == "true") result = true;

            }

            return result;
        }

        public static bool UpdateSharedConfiguration_ToList(SharedListItem item)
        {
            bool result = false;

            try
            {
                using (WebClient client = new WebClient())
                {
                    NameValueCollection values = new NameValueCollection();

                    // Add Columns
                    List<string> columnsList = new List<string>();
                    columnsList.Add("id");
                    columnsList.Add("description");
                    columnsList.Add("manufacturer");
                    columnsList.Add("device_type");
                    columnsList.Add("model");
                    columnsList.Add("controller");
                    columnsList.Add("author");
                    columnsList.Add("certified");
                    columnsList.Add("stars");
                    columnsList.Add("downloads");
                    columnsList.Add("tablename");
                    columnsList.Add("upload_date");
                    columnsList.Add("version");
                    columnsList.Add("image_url");
                    columnsList.Add("tags");
                    columnsList.Add("link_tag");
                    columnsList.Add("dependencies");

                    object[] columns = columnsList.ToArray();

                    List<object> rowValues = new List<object>();
                    rowValues.Add(item.id);
                    rowValues.Add(item.description);
                    rowValues.Add(item.manufacturer);
                    rowValues.Add(item.device_type);
                    rowValues.Add(item.model);
                    rowValues.Add(item.controller);
                    rowValues.Add(item.author);
                    rowValues.Add(item.certified.ToString());
                    rowValues.Add(item.stars.ToString());
                    rowValues.Add(item.downloads.ToString());
                    rowValues.Add(item.tablename);
                    rowValues.Add(item.upload_date.ToString("yyyy-MM-dd hh:mm:ss"));
                    rowValues.Add(item.version);
                    rowValues.Add(item.image_url);
                    rowValues.Add(item.tags);
                    rowValues.Add(item.link_tag);
                    rowValues.Add(item.dependencies);


                    //Create Values string
                    string vals = "VALUES (";
                    for (int x = 0; x <= rowValues.Count - 1; x++)
                    {
                        // Dont put the ' characters if the value is null
                        if (rowValues[x] == null) vals += "null";
                        else
                        {
                            object val = rowValues[x];

                            if (rowValues[x].ToString().ToLower() != "null") vals += "'" + val.ToString() + "'";
                            else vals += val.ToString();
                        }

                        if (x < rowValues.Count - 1) vals += ", ";
                    }
                    vals += ")";


                    //Create Columns string
                    string cols = "";
                    for (int x = 0; x <= columns.Length - 1; x++)
                    {
                        cols += columns[x].ToString().ToUpper();
                        if (x < columns.Length - 1) cols += ", ";
                    }


                    string update = "";
                    update = " ON DUPLICATE KEY UPDATE ";
                    for (int x = 0; x <= columns.Length - 1; x++)
                    {
                        update += columns[x].ToString().ToUpper();
                        update += "=";

                        object val = rowValues[x];

                        if (val != null) update += "'" + val.ToString() + "'";
                        else update += "null";

                        if (x < columns.Length - 1) update += ", ";
                    }

                    string query = "INSERT IGNORE INTO shared (" + cols + ") " + vals + update;

                    values["query"] = query;

                    byte[] response = client.UploadValues("http://www.feenux.com/php/configurations/updatesharedlistitem.php", values);

                    string responseString = Encoding.Default.GetString(response);

                    if (responseString.ToLower().Trim() == "true") result = true;
                }
            }
            catch (Exception ex) { Logger.Log(ex.Message); }

            return result;
        }

        public static bool UpdateSharedConfigurationDate(SharedListItem item)
        {
            bool result = false;

            try
            {
                using (WebClient client = new WebClient())
                {
                    NameValueCollection values = new NameValueCollection();

                    string query = "UPDATE shared SET upload_date='" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "' WHERE id='" + item.id + "'";

                    values["query"] = query;

                    byte[] response = client.UploadValues("http://www.feenux.com/php/configurations/updatesharedlistitem.php", values);

                    string responseString = Encoding.Default.GetString(response);

                    if (responseString.ToLower().Trim() == "true") result = true;
                }
            }
            catch (Exception ex) { Logger.Log(ex.Message); }

            return result;
        }

        public static bool RemoveSharedConfiguration_FromList(SharedListItem item)
        {
            bool result = false;

            try
            {
                using (WebClient client = new WebClient())
                {
                    NameValueCollection values = new NameValueCollection();

                    string query = "DELETE FROM shared WHERE id='" + item.id + "'";

                    values["query"] = query;

                    byte[] response = client.UploadValues("http://www.feenux.com/php/configurations/updatesharedlistitem.php", values);

                    string responseString = Encoding.Default.GetString(response);

                    if (responseString.ToLower().Trim() == "true") result = true;
                }
            }
            catch (Exception ex) { Logger.Log(ex.Message); }

            return result;
        }
    }

}
