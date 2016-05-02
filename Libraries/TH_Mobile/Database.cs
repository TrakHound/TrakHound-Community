// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Specialized;
using System.Data;

using TH_Global.Web;
using TH_Configuration;

namespace TH_Mobile
{
    public static class Database
    {
        private const string PHP_URL = "http://www.feenux.com/php/mobile/";

        public static bool CreateTable(string userId, Configuration config)
        {
            bool result = false;

            if (!string.IsNullOrEmpty(userId) && config != null)
            {
                var values = new NameValueCollection();
                values["user_id"] = userId;
                values["database_id"] = config.DatabaseId;

                values["unique_id"] = config.UniqueId;
                values["description"] = config.Description.Description;
                values["device_id"] = config.Description.Device_ID;
                values["manufacturer"] = config.Description.Manufacturer;
                values["model"] = config.Description.Model;
                values["serial"] = config.Description.Serial;
                values["controller"] = config.Description.Controller;
                values["logo_url"] = config.FileLocations.Manufacturer_Logo_Path;

                string url = PHP_URL + "create_table.php";
                string responseString = HTTP.SendData(url, values);

                if (responseString != null && responseString == "true")
                {
                    result = true;
                }
            }

            return result;
        }

        public static bool Update(string userId, Configuration config, UpdateData updateData)
        {
            bool result = false;

            if (!string.IsNullOrEmpty(userId) && config != null)
            {
                var values = new NameValueCollection();
                values["user_id"] = userId;
                values["database_id"] = config.DatabaseId;

                values["unique_id"] = config.UniqueId;
                values["connected"] = updateData.Connected.ToString();
                values["status"] = updateData.Status;
                values["production_status"] = updateData.ProductionStatus;
                values["production_status_timer"] = updateData.ProductionStatusTimer.ToString();

                string url = PHP_URL + "update.php";
                string responseString = HTTP.SendData(url, values);

                if (responseString != null && responseString == "true")
                {
                    result = true;
                }
            }

            return result;
        }

        //public static bool Get(string userId)
        //{
        //    bool result = false;

        //    if (!string.IsNullOrEmpty(userId))
        //    {
        //        var values = new NameValueCollection();
        //        values["user_id"] = userId;


        //        string url = PHP_URL + "get.php";
        //        string responseString = HTTP.SendData(url, values);

        //        if (responseString != null && responseString == "true")
        //        {
        //            result = true;
        //        }
        //    }

        //    return result;
        //}



        //public static List<DataTable> GetTablesFromString(string s)
        //{
        //    List<Configuration> result = null;

        //    NameValueCollection values = new NameValueCollection();

        //    if (userConfig.username != null) values["username"] = userConfig.username.ToLower();

        //    string url = PHPFilePath + "getconfigurationslist.php";
        //    string responseString = HTTP.SendData(url, values);

        //    if (responseString != null)
        //    {
        //        result = new List<Configuration>();

        //        string[] tables = responseString.Split(TABLE_DELIMITER_START.ToCharArray());

        //        foreach (string table in tables)
        //        {
        //            if (!String.IsNullOrEmpty(table))
        //            {
        //                var delimiter = table.IndexOf(TABLE_DELIMITER_END);
        //                if (delimiter > 0)
        //                {
        //                    string tablename = table.Substring(0, delimiter);
        //                    string tabledata = table.Substring(delimiter + TABLE_DELIMITER_END.Length);

        //                    DataTable dt = JSON.ToTable(tabledata);
        //                    if (dt != null)
        //                    {
        //                        XmlDocument xml = TH_Configuration.Converter.TableToXML(dt);
        //                        if (xml != null)
        //                        {
        //                            Configuration config = TH_Configuration.Configuration.Read(xml);
        //                            if (config != null)
        //                            {
        //                                config.Remote = true;
        //                                config.TableName = tablename;
        //                                result.Add(config);
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    return result;
        //}

    }
}
