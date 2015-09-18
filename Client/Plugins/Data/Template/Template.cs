using System;
using System.Collections.Generic;
using System.Data;

using TH_Configuration;
using TH_Global;
using TH_MySQL;
using TH_PlugIns_Client_Data;

namespace TH_DeviceStatus
{
    public class Template : Data_PlugIn
    {

        #region "Plugin"

        /// <summary>
        /// Plugin's Name
        /// </summary>
        public string Name { get { return "Plugin Name"; } }

        /// <summary>
        /// Priority that this Plugin should run at 
        /// (ex. if another plugin needs information from this plugin, it should have a higher number than this plugin)
        /// </summary>
        public int Priority { get { return 1; } }

        /// <summary>
        /// Method called when this plugin is intially loaded
        /// </summary>
        /// <param name="config"></param>
        public void Initialize(Configuration config) { }

        /// <summary>
        /// Method called when the TrakHound Client is closing
        /// </summary>
        public void Closing() { }

        /// <summary>
        /// Called by the TrakHound Client when it is ready to update the Device's data
        /// This is where the action for this Plugin should be run
        /// </summary>
        public void Run() { }

        /// <summary>
        /// Method called when another plugin has raised it's "DataEvent" event and can be used to communicate between plugins
        /// </summary>
        /// <param name="de_d"></param>
        public void Update_DataEvent(DataEvent_Data de_d) { }

        /// <summary>
        /// Event used to send data back to the TrakHound Client to use to send to Control Plugins
        /// </summary>
        public event DataEvent_Handler DataEvent;

        #endregion

    }

}
