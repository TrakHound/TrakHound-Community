using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TH_Plugins_Server;

namespace TH_DeviceManager
{
    public partial class DeviceManagerPage
    {
        void LoadConfiguration()
        {
            if (ConfigurationTable != null)
            {
                if (ConfigurationPages != null)
                {
                    foreach (ConfigurationPage page in ConfigurationPages)
                    {
                        this.Dispatcher.BeginInvoke(new Action(() => { page.LoadConfiguration(ConfigurationTable); }));
                    }
                }
            }
        }

    }
}
