using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

using System.ComponentModel.Composition;

using System.Data;

namespace TH_Database
{
    public enum Application_Type
    {
        Client = 0,
        Server = 1
    }

    public delegate void SettingChanged_Handler(string name, string oldVal, string newVal);


    [InheritedExport(typeof(DatabaseConfigurationPage))]
    public interface DatabaseConfigurationPage
    {
        string PageName { get; }

        ImageSource Image { get; }

        event SettingChanged_Handler SettingChanged;

        string prefix { get; set; }
        //string ClientPrefix { get; set; }
        //string ServerPrefix { get; set; }

        void LoadConfiguration(DataTable dt);

        void SaveConfiguration(DataTable dt);

        Application_Type ApplicationType { get; set; }

        IDatabasePlugin Plugin { get; }
    }

}
