using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TH_Plugins_Server;

namespace TH_DeviceManager
{
    public partial class DeviceManager
    {

        void LoadConfiguration()
        {
            if (ConfigurationTable != null)
            {
                if (ConfigurationPages != null)
                {
                    foreach (ConfigurationPage page in ConfigurationPages)
                    {
                        page.LoadConfiguration(ConfigurationTable);
                    }
                }
            }
        }

    }
}
