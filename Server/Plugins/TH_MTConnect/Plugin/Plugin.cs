using System;

using TH_Configuration;
using TH_Plugins;
using TH_Plugins.Server;

namespace TH_MTConnect.Plugin
{
    public partial class MTConnect : IServerPlugin
    {
        public string Name { get { return "TH_MTConnect"; } }

        public void Initialize(Configuration config)
        {
            var ac = AgentConfiguration.Read(config.ConfigurationXML);
            if (ac != null)
            {
                config.CustomClasses.Add(ac);

                configuration = config;
            }
        }

        public void GetSentData(EventData data)
        {
            if (data != null && data.Id != null && data.Data02 != null)
            {
                UpdateDatabaseStatus(data);
            }
        }

        public event SendData_Handler SendData;


        public void Closing()
        {
            Stop();
        }

        public Type[] ConfigurationPageTypes { get { return new Type[] { typeof(ConfigurationPage.Page) }; } }


        Configuration configuration;

        private bool started = false;

        private void UpdateDatabaseStatus(EventData data)
        {
            if (data.Id.ToLower() == "databasestatus")
            {
                if (data.Data02.GetType() == typeof(bool))
                {
                    var connected = (bool)data.Data02;
                    if (connected) Start(configuration);
                    else if (!connected) Stop();
                }
            }
        }

    }
}
