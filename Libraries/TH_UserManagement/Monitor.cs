using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Xml;
using System.Threading;

using TH_Configuration;
using TH_Global;

using TH_UserManagement.Management;

namespace TH_UserManagement
{

    /// <summary>
    /// Monitor for Configuration Table Changes
    /// </summary>
    public class ConfigurationMonitor
    {

        #region "Properties"

        public int Interval { get; set; }

        public string TableName { get; set; }

        public Configuration CurrentConfiguration { get; set; }

        #endregion

        #region "Events"

        public delegate void ConfigurationChanged_Handler(Configuration config);
        public event ConfigurationChanged_Handler ConfigurationChanged;

        #endregion

        System.Timers.Timer monitor_TIMER;

        Thread monitor_THREAD;

        public void Start()
        {
            monitor_TIMER = new System.Timers.Timer();

            Interval = Math.Max(1000, Interval);

            monitor_TIMER.Interval = Interval;
            monitor_TIMER.Elapsed += monitor_TIMER_Elapsed;
            monitor_TIMER.Enabled = true;
        }

        public void Stop()
        {
            if (monitor_TIMER != null) monitor_TIMER.Enabled = false;
            if (monitor_THREAD != null) monitor_THREAD.Abort();
        }

        void monitor_TIMER_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (TableName != null && CurrentConfiguration != null)
            {
                Run();
            }
        }

        void Run()
        {
            if (monitor_THREAD != null) monitor_THREAD.Abort();

            monitor_THREAD = new Thread(new ThreadStart(Run_Worker));
            monitor_THREAD.Start();
        }

        void Run_Worker()
        {
            try
            {
                DataTable dt = Remote.Configurations.GetConfigurationTable(TableName);
                if (dt != null)
                {
                    XmlDocument xml = Converter.TableToXML(dt);
                    if (xml != null)
                    {
                        Configuration config = Configuration.ReadConfigFile(xml);
                        if (config != null)
                        {
                            if (config.UpdateId != CurrentConfiguration.UpdateId)
                            {
                                config.Index = CurrentConfiguration.Index;

                                monitor_TIMER.Enabled = false;

                                if (ConfigurationChanged != null) ConfigurationChanged(config);
                            }
                        }
                    }
                }
            }
            catch (Exception ex) 
            {
                //Logger.Log("Run_Worker() :: Exception :: " + ex.Message);
            }
        }

    }

}
