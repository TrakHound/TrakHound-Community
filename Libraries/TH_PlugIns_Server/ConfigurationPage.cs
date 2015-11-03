using System;

using System.ComponentModel.Composition;
using System.Windows.Media;

using System.Data;

using TH_Configuration;

namespace TH_PlugIns_Server
{
    [InheritedExport(typeof(ConfigurationPage))]
    public interface ConfigurationPage
    {

        string PageName { get; }

        ImageSource Image { get; }

        event SettingChanged_Handler SettingChanged;

        void LoadConfiguration(DataTable dt);

        void SaveConfiguration(DataTable dt);

        UserConfiguration currentUser { get; set; }

    }

    public delegate void SaveRequest_Handler();
    public delegate void SettingChanged_Handler(string name, string oldVal, string newVal);

    public static class Tools
    {

        //public static string GetTableValue(string key, DataTable dt)
        //{
        //    string result = null;

        //    DataRow row = dt.Rows.Find(key);
        //    if (row != null)
        //    {
        //        result = row["value"].ToString();
        //    }

        //    return result;
        //}

        //public static void UpdateTableValue(string value, string key, DataTable dt)
        //{
        //    DataRow row = dt.Rows.Find(key);
        //    if (row != null)
        //    {
        //        row["value"] = value;
        //    }
        //    else
        //    {
        //        row = dt.NewRow();
        //        row["address"] = key;
        //        row["value"] = value;
        //        dt.Rows.Add(row);
        //    }
        //}

    }

}
